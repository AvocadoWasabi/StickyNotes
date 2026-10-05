using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace StickyNotes.Core;

public sealed record CalendarQuery(DateTimeOffset From, string Search)
{
    public static CalendarQuery Parse(string command)
    {
        var m = Regex.Match(command.Trim(), @"^@calendar\s+(\S+)(?:\s+(.*))?$", RegexOptions.IgnoreCase);
        if (!m.Success || !DateTimeOffset.TryParse(m.Groups[1].Value, out var date))
            throw new FormatException("例: @calendar 2026-10-05T09:00 会議（検索語は省略可、時差省略時はPCのローカル時間）");
        return new(date, m.Groups[2].Value.Trim());
    }
}

public sealed record CalendarEvent(string Id, string Summary, string Description, string Start, string ETag);
public sealed class OAuthTokens
{
    [JsonPropertyName("access_token")] public string AccessToken { get; set; } = "";
    [JsonPropertyName("refresh_token")] public string RefreshToken { get; set; } = "";
    [JsonPropertyName("expires_in")] public int ExpiresIn { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
}

public sealed class CalendarService(Func<string?> loadToken, Action<string> saveToken, HttpMessageHandler? transport = null) : IDisposable
{
    private readonly HttpClient http = new(transport ?? new HttpClientHandler()) { Timeout = TimeSpan.FromSeconds(40) };
    private readonly SemaphoreSlim gate = new(1, 1);
    private string clientId = "", clientSecret = "";
    private OAuthTokens? tokens;

    public void Configure(string credentialsPath)
    {
        using var json = JsonDocument.Parse(File.ReadAllText(credentialsPath));
        if (!json.RootElement.TryGetProperty("installed", out var installed))
            throw new FormatException("Google Cloudで種類「デスクトップアプリ」のOAuth JSONを作成してください。");
        var nextId = installed.GetProperty("client_id").GetString()!;
        if (clientId != nextId)
        {
            tokens = null;
            var stored = loadToken();
            if (!string.IsNullOrEmpty(stored))
            {
                using var saved = JsonDocument.Parse(stored);
                if (saved.RootElement.GetProperty("clientId").GetString() == nextId)
                    tokens = saved.RootElement.GetProperty("tokens").Deserialize<OAuthTokens>();
            }
        }
        clientId = nextId;
        clientSecret = installed.TryGetProperty("client_secret", out var secret) ? secret.GetString()! : "";
    }

    private void Persist() => saveToken(JsonSerializer.Serialize(new { clientId, tokens }));
    private static string Base64(byte[] bytes) => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    public async Task SignInAsync()
    {
        if (string.IsNullOrWhiteSpace(clientId)) throw new InvalidOperationException("先にGoogle OAuth JSONを設定してください。");
        await gate.WaitAsync();
        try
        {
            var probe = new TcpListener(IPAddress.Loopback, 0);
            probe.Start();
            var port = ((IPEndPoint)probe.LocalEndpoint).Port;
            probe.Stop();
            var redirect = $"http://127.0.0.1:{port}/";
            using var listener = new HttpListener();
            listener.Prefixes.Add(redirect);
            listener.Start();
            var verifier = Base64(RandomNumberGenerator.GetBytes(48));
            var state = Base64(RandomNumberGenerator.GetBytes(32));
            var parameters = new Dictionary<string, string>
            {
                ["client_id"] = clientId, ["redirect_uri"] = redirect, ["response_type"] = "code",
                ["scope"] = "https://www.googleapis.com/auth/calendar.events", ["access_type"] = "offline",
                ["prompt"] = "consent", ["state"] = state, ["code_challenge_method"] = "S256",
                ["code_challenge"] = Base64(SHA256.HashData(Encoding.ASCII.GetBytes(verifier)))
            };
            var query = await new FormUrlEncodedContent(parameters).ReadAsStringAsync();
            Process.Start(new ProcessStartInfo("https://accounts.google.com/o/oauth2/v2/auth?" + query) { UseShellExecute = true });
            using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(3));
            string? code = null;
            while (code is null)
            {
                var context = await listener.GetContextAsync().WaitAsync(timeout.Token);
                var validState = context.Request.QueryString["state"] == state;
                var error = context.Request.QueryString["error"];
                code = validState ? context.Request.QueryString["code"] : null;
                context.Response.StatusCode = validState ? 200 : 400;
                context.Response.ContentType = "text/plain; charset=utf-8";
                var bytes = Encoding.UTF8.GetBytes(code is null ? "認証できませんでした。付箋アプリに戻ってください。" : "認証を受け取りました。このタブを閉じて付箋に戻ってください。");
                await context.Response.OutputStream.WriteAsync(bytes);
                context.Response.Close();
                if (validState && error is not null) throw new InvalidOperationException("Googleの認証がキャンセルされました: " + error);
            }
            tokens = await Exchange(new() { ["code"] = code, ["redirect_uri"] = redirect, ["grant_type"] = "authorization_code", ["code_verifier"] = verifier });
            Persist();
        }
        finally { gate.Release(); }
    }

    private async Task<OAuthTokens> Exchange(Dictionary<string, string> form)
    {
        form["client_id"] = clientId;
        if (clientSecret.Length > 0) form["client_secret"] = clientSecret;
        using var response = await http.PostAsync("https://oauth2.googleapis.com/token", new FormUrlEncodedContent(form));
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException($"Google認証に失敗しました ({(int)response.StatusCode})。設定から再認証してください。");
        var result = JsonSerializer.Deserialize<OAuthTokens>(await response.Content.ReadAsStringAsync())!;
        result.ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(result.ExpiresIn - 60);
        return result;
    }

    private async Task<string> AccessToken()
    {
        await gate.WaitAsync();
        try
        {
            if (tokens is null) throw new InvalidOperationException("設定からGoogleにログインしてください。");
            if (tokens.ExpiresAt <= DateTimeOffset.UtcNow)
            {
                if (tokens.RefreshToken.Length == 0) throw new InvalidOperationException("Googleへの再ログインが必要です。");
                var refreshed = await Exchange(new() { ["grant_type"] = "refresh_token", ["refresh_token"] = tokens.RefreshToken });
                refreshed.RefreshToken = tokens.RefreshToken;
                tokens = refreshed;
                Persist();
            }
            return tokens.AccessToken;
        }
        finally { gate.Release(); }
    }

    private async Task<JsonDocument> Send(HttpMethod method, string path, object? payload = null, string? etag = null)
    {
        using var request = new HttpRequestMessage(method, "https://www.googleapis.com/calendar/v3/" + path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await AccessToken());
        if (etag is not null) request.Headers.TryAddWithoutValidation("If-Match", etag);
        if (payload is not null) request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        using var response = await http.SendAsync(request);
        if (response.StatusCode == HttpStatusCode.PreconditionFailed)
            throw new ConflictException("Google側で予定が変更されています。予定を再取得してから編集してください。");
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException($"Calendar APIエラー ({(int)response.StatusCode})。接続、認証、カレンダーの編集権限を確認してください。");
        return JsonDocument.Parse(await response.Content.ReadAsStringAsync());
    }

    private static string Escape(string s) => Uri.EscapeDataString(s);
    private static CalendarEvent ReadEvent(JsonElement e)
    {
        string Value(string name) => e.TryGetProperty(name, out var p) ? p.GetString() ?? "" : "";
        var start = e.GetProperty("start");
        return new(Value("id"), Value("summary"), Value("description"),
            start.TryGetProperty("dateTime", out var date) ? date.GetString()! : start.GetProperty("date").GetString()!, Value("etag"));
    }

    // First 100 matching upcoming instances, including a currently ongoing event per Google's timeMin semantics.
    public async Task<List<CalendarEvent>> SearchAsync(string calendarId, CalendarQuery query)
    {
        var path = $"calendars/{Escape(calendarId)}/events?singleEvents=true&orderBy=startTime&maxResults=100&timeMin={Escape(query.From.ToString("o"))}&q={Escape(query.Search)}";
        using var json = await Send(HttpMethod.Get, path);
        return json.RootElement.GetProperty("items").EnumerateArray().Select(ReadEvent).ToList();
    }

    public async Task<CalendarEvent> UpdateAsync(string calendarId, CalendarEvent original, string summary, string description)
    {
        if (string.IsNullOrEmpty(original.ETag)) throw new InvalidOperationException("予定のバージョンを取得できません。再検索してください。");
        using var json = await Send(HttpMethod.Patch, $"calendars/{Escape(calendarId)}/events/{Escape(original.Id)}?sendUpdates=all",
            new { summary, description }, original.ETag);
        return ReadEvent(json.RootElement);
    }

    public void Dispose() { http.Dispose(); gate.Dispose(); }
}
