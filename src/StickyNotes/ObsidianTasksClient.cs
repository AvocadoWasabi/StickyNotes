using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace StickyNotes;

internal sealed record TasksOutput(string? Markdown, string? Error);
internal sealed record TasksResponse(string Version, TasksOutput[] Results);

internal static class ObsidianTasksClient
{
    private static readonly SemaphoreSlim Gate = new(1, 1);
    private static readonly JsonSerializerOptions Json = new() { PropertyNameCaseInsensitive = true };
    private const string Marker = "STICKY_TASKS_PREVIEW:";
    private static readonly string Script = ReadScript();

    private static string ReadScript()
    {
        using var stream = typeof(ObsidianTasksClient).Assembly.GetManifestResourceStream("StickyNotes.TasksBridge.js")!;
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
        var payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new { root, path, queries })));
        var code = "(async()=>{" + Script + ";try{const request=JSON.parse(new TextDecoder().decode(Uint8Array.from(atob('" + payload +
            "'),c=>c.charCodeAt(0))));return '" + Marker + "'+JSON.stringify(await stickyTasksPreview(request));}" +
            "catch(e){return '" + Marker + "'+JSON.stringify({error:String(e.message||e)});}})()";
        if (code.Length > 26000) throw new InvalidOperationException(L10n.Text("TasksPreview.QueryLimit"));
        return code;
    }

    internal static TasksResponse ParseResponse(string output, int count)
    {
        // Obsidian may write startup diagnostics before its single-line eval result.
        var line = output.Split('\n').LastOrDefault(l => l.StartsWith("=> " + Marker, StringComparison.Ordinal) || l.StartsWith(Marker, StringComparison.Ordinal));
        if (line is null) throw new InvalidOperationException(L10n.Text("TasksPreview.NoResponse"));
        var json = line[(line.IndexOf(Marker, StringComparison.Ordinal) + Marker.Length)..].Trim();
        using var doc = JsonDocument.Parse(json);
        if (doc.RootElement.TryGetProperty("error", out var error)) throw new InvalidOperationException(error.GetString());
        var response = JsonSerializer.Deserialize<TasksResponse>(json, Json);
        if (response is null || string.IsNullOrWhiteSpace(response.Version) || response.Results is null || response.Results.Length != count ||
            response.Results.Any(r => r is null || (r.Markdown is null && r.Error is null)))
            throw new InvalidOperationException(L10n.Text("TasksPreview.NoResponse"));
        return response;
    }

    public static async Task<TasksResponse> QueryAsync(Settings settings, string note, string[] queries, CancellationToken token)
    {
        var relative = RelativeNotePath(settings.ObsidianVaultFolder, note);
        var root = Path.GetFullPath(settings.ObsidianVaultFolder).TrimEnd('\\', '/');
        var code = BuildCode(root, relative, queries);
        var cli = settings.ObsidianCli.Trim();
        if (!Path.IsPathFullyQualified(cli) || !File.Exists(cli) ||
            !Path.GetFileName(cli).Equals("Obsidian.com", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(L10n.Text("TasksPreview.CliRequired"));
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(token);
        timeout.CancelAfter(TimeSpan.FromSeconds(25));
        await Gate.WaitAsync(timeout.Token);
        try
        {
            // CLI can launch Obsidian itself. This preview requires an already running app.
            var running = Process.GetProcessesByName("Obsidian");
            try { if (running.Length == 0) throw new InvalidOperationException(L10n.Text("TasksPreview.NotRunning")); }
            finally { foreach (var process in running) process.Dispose(); }
            var start = new ProcessStartInfo(cli) { UseShellExecute = false, CreateNoWindow = true,
                RedirectStandardOutput = true, RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8, StandardErrorEncoding = Encoding.UTF8 };
            start.ArgumentList.Add("vault=" + (string.IsNullOrWhiteSpace(settings.ObsidianVaultId) ? new DirectoryInfo(root).Name : settings.ObsidianVaultId.Trim()));
            start.ArgumentList.Add("eval");
            start.ArgumentList.Add("code=" + code);
            using var child = Process.Start(start) ?? throw new InvalidOperationException(L10n.Text("TasksPreview.NoResponse"));
            using var stop = timeout.Token.Register(() => { try { if (!child.HasExited) child.Kill(); } catch (InvalidOperationException) { } catch (System.ComponentModel.Win32Exception) { } });
            var stdout = ReadBounded(child.StandardOutput, timeout.Token);
            var stderr = ReadBounded(child.StandardError, timeout.Token);
            try
            {
                await Task.WhenAll(stdout, stderr).WaitAsync(timeout.Token);
                await child.WaitForExitAsync(timeout.Token);
                if (child.ExitCode != 0) throw new InvalidOperationException(L10n.Text("TasksPreview.NoResponse") + "\n" + (await stderr)[..Math.Min((await stderr).Length, 2000)]);
                return ParseResponse(await stdout, queries.Length);
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
