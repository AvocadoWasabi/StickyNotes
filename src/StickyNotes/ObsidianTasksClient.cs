using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.IO.Compression;
using System.Threading;

namespace StickyNotes;

internal sealed record TasksOutput(string? Markdown, string? Error);
internal sealed record TasksResponse(string Version, TasksOutput[] Results);

internal static class ObsidianTasksClient
{
    private static readonly SemaphoreSlim Gate = new(1, 1);
    private static readonly JsonSerializerOptions Json = new() { PropertyNameCaseInsensitive = true };
    private const string Marker = "STICKY_TASKS_PREVIEW:";
    private static readonly string Script = Compress(ReadScript("TasksBridge"));
    private static readonly string NotesScript = Compress(ReadScript("NotesBridge"));
    private static readonly string DailyScript = Compress(ReadScript("DailyBridge"));
    private static readonly string FoldersScript = Compress(ReadScript("FoldersBridge"));
    private static string Compress(string value)
    {
        using var output = new MemoryStream();
        using (var zip = new ZLibStream(output, CompressionLevel.SmallestSize, leaveOpen: true))
            zip.Write(Encoding.UTF8.GetBytes(value));
        return Convert.ToBase64String(output.ToArray());
    }

    private static string ReadScript(string name)
    {
        using var stream = typeof(ObsidianTasksClient).Assembly.GetManifestResourceStream("StickyNotes." + name + ".js")!;
        using var reader = new StreamReader(stream);
        // The bridge deliberately uses no inline comments, so whole-line comments can be removed.
        return string.Join(" ", reader.ReadToEnd().Split('\n').Select(s => s.Trim()).Where(s => !s.StartsWith("//")));
    }

    internal static string RelativeNotePath(string root, string note)
    {
        if (!Path.IsPathFullyQualified(root) || !Path.IsPathFullyQualified(note))
            throw new InvalidOperationException(L10n.Text("TasksPreview.VaultRequired"));
        var relative = Path.GetRelativePath(Path.GetFullPath(root), Path.GetFullPath(note));
        if (Path.IsPathRooted(relative) || relative == ".." || relative.StartsWith(".." + Path.DirectorySeparatorChar) ||
            !relative.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(L10n.Text("TasksPreview.OutsideVault"));
        return relative.Replace('\\', '/');
    }

    internal static string BuildCode(string root, string path, string[] queries)
    {
        if (queries.Length > 20 || queries.Sum(q => q.Length) > 8000)
            throw new InvalidOperationException(L10n.Text("TasksPreview.QueryLimit"));
        return BuildRequestCode(new { root, path, queries });
    }

    internal static string BuildRequestCode(object request, bool notes = false)
        => BuildRequestCode(request, notes ? NotesScript : Script);

    internal static string BuildDailyCode(object request) => BuildRequestCode(request, DailyScript);
    internal static string BuildFoldersCode(object request) => BuildRequestCode(request, FoldersScript);

    private static string BuildRequestCode(object request, string script)
    {
        var payload = Compress(JsonSerializer.Serialize(request));
        // Only the shipped adapter is evaluated as code. Note text stays compressed JSON data.
        var code = "(async()=>{try{const request=JSON.parse(require('zlib').inflateSync(Buffer.from('" + payload +
            "','base64')).toString('utf8'));const run=eval(require('zlib').inflateSync(Buffer.from('" + script +
            "','base64')).toString('utf8')+';stickyTasksPreview');return '" + Marker + "'+JSON.stringify(await run(request));}" +
            "catch(e){return '" + Marker + "'+JSON.stringify({error:String(e.message||e)});}})()";
        if (code.Length > 3500) throw new InvalidOperationException(L10n.Text("CliNote.CommandLimit"));
        return code;
    }

    internal static void ValidateCommand(string vault, string code)
    {
        // Obsidian 1.14.4 can parse partial pipe data as a complete JSON message.
        // Budget the serialized arguments plus framing below 4 KiB; reject before launching CLI.
        var args = JsonSerializer.Serialize(new[] { "vault=" + vault, "eval", "code=" + code });
        if (Encoding.UTF8.GetByteCount(args) + 512 > 4000)
            throw new InvalidOperationException(L10n.Text("CliNote.CommandLimit"));
    }

    internal static TasksResponse ParseResponse(string output, int count)
    {
        using var doc = ParseEnvelope(output);
        var response = doc.RootElement.Deserialize<TasksResponse>(Json);
        if (response is null || string.IsNullOrWhiteSpace(response.Version) || response.Results is null || response.Results.Length != count ||
            response.Results.Any(r => r is null || (r.Markdown is null && r.Error is null)))
            throw new InvalidOperationException(L10n.Text("TasksPreview.NoResponse"));
        return response;
    }

    internal static JsonDocument ParseEnvelope(string output)
    {
        // Obsidian may write startup diagnostics before its single-line eval result.
        var line = output.Split('\n').LastOrDefault(l => l.StartsWith("=> " + Marker, StringComparison.Ordinal) || l.StartsWith(Marker, StringComparison.Ordinal));
        if (line is null) throw new InvalidOperationException(L10n.Text("TasksPreview.NoResponse"));
        var json = line[(line.IndexOf(Marker, StringComparison.Ordinal) + Marker.Length)..].Trim();
        var doc = JsonDocument.Parse(json);
        if (doc.RootElement.TryGetProperty("error", out var error))
        {
            var message = error.GetString(); doc.Dispose();
            if (message == "STICKY_CONFLICT") throw new ConflictException(L10n.Text("NoteStore.Text01"));
            if (message is "STICKY_DAILY_DISABLED" or "STICKY_DAILY_UNSUPPORTED" or "STICKY_DAILY_PATH")
                throw new InvalidOperationException(L10n.Text("CliDaily." + message[13..]));
            throw new InvalidOperationException(message);
        }
        return doc;
    }

    public static async Task<TasksResponse> QueryAsync(Settings settings, string note, string[] queries, CancellationToken token)
    {
        var relative = RelativeNotePath(settings.ObsidianVaultFolder, note);
        var root = Path.GetFullPath(settings.ObsidianVaultFolder).TrimEnd('\\', '/');
        var code = BuildCode(root, relative, queries);
        return ParseResponse(await ExecuteAsync(settings, code, token).ConfigureAwait(false), queries.Length);
    }

    internal static async Task<string> ExecuteAsync(Settings settings, string code, CancellationToken token)
    {
        var root = string.IsNullOrWhiteSpace(settings.ObsidianVaultFolder) ? "" : Path.GetFullPath(settings.ObsidianVaultFolder).TrimEnd('\\', '/');
        var cli = settings.ObsidianCli.Trim();
        var vault = string.IsNullOrWhiteSpace(settings.ObsidianVaultId) ? (root.Length == 0 ? "" : new DirectoryInfo(root).Name) : settings.ObsidianVaultId.Trim();
        ValidateCommand(vault, code);
        if (!Path.IsPathFullyQualified(cli) || !File.Exists(cli) ||
            !Path.GetFileName(cli).Equals("Obsidian.com", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(L10n.Text("TasksPreview.CliRequired"));
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(token);
        timeout.CancelAfter(TimeSpan.FromSeconds(25));
        await Gate.WaitAsync(timeout.Token).ConfigureAwait(false);
        try
        {
            // CLI can launch Obsidian itself. This preview requires an already running app.
            var running = Process.GetProcessesByName("Obsidian");
            try { if (running.Length == 0) throw new InvalidOperationException(L10n.Text("TasksPreview.NotRunning")); }
            finally { foreach (var process in running) process.Dispose(); }
            var start = new ProcessStartInfo(cli) { UseShellExecute = false, CreateNoWindow = true,
                RedirectStandardOutput = true, RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8, StandardErrorEncoding = Encoding.UTF8 };
            if (vault.Length > 0) start.ArgumentList.Add("vault=" + vault);
            start.ArgumentList.Add("eval");
            start.ArgumentList.Add("code=" + code);
            using var child = Process.Start(start) ?? throw new InvalidOperationException(L10n.Text("TasksPreview.NoResponse"));
            using var stop = timeout.Token.Register(() => { try { if (!child.HasExited) child.Kill(); } catch (InvalidOperationException) { } catch (System.ComponentModel.Win32Exception) { } });
            var stdout = ReadBounded(child.StandardOutput, timeout.Token);
            var stderr = ReadBounded(child.StandardError, timeout.Token);
            try
            {
                await Task.WhenAll(stdout, stderr).WaitAsync(timeout.Token).ConfigureAwait(false);
                await child.WaitForExitAsync(timeout.Token).ConfigureAwait(false);
                if (child.ExitCode != 0) throw new InvalidOperationException(L10n.Text("TasksPreview.NoResponse") + "\n" + (await stderr)[..Math.Min((await stderr).Length, 2000)]);
                return await stdout.ConfigureAwait(false);
            }
            finally
            {
                try { if (!child.HasExited) child.Kill(); } catch (InvalidOperationException) { } catch (System.ComponentModel.Win32Exception) { }
            }
        }
        finally { Gate.Release(); }
    }

    private static async Task<string> ReadBounded(StreamReader reader, CancellationToken token)
    {
        var result = new StringBuilder();
        var buffer = new char[4096];
        int count;
        while ((count = await reader.ReadAsync(buffer.AsMemory(), token)) > 0)
        {
            if (result.Length + count > 4_500_000) throw new InvalidOperationException(L10n.Text("TasksPreview.OutputLimit"));
            result.Append(buffer, 0, count);
        }
        return result.ToString();
    }
}
