using System.Text.Json;
using System.Threading;

namespace StickyNotes;

// All note-content I/O is selected once per operation; CLI failures never fall back to disk.
internal sealed class NoteSource
{
    private readonly Settings settings;
    private readonly bool cli;
    private readonly Func<Settings, string, CancellationToken, Task<string>> execute;

    internal NoteSource(Settings settings, bool cli,
        Func<Settings, string, CancellationToken, Task<string>>? execute = null)
    {
        // Do not let an in-flight request adopt a newly selected vault/profile.
        this.settings = new Settings { ObsidianCli = settings.ObsidianCli,
            ObsidianVaultFolder = settings.ObsidianVaultFolder, ObsidianVaultId = settings.ObsidianVaultId };
        this.cli = cli; this.execute = execute ?? ObsidianTasksClient.ExecuteAsync;
    }

    internal bool IsCli => cli;
    internal static bool UsesCli(Settings settings) => BuildFlavor.TasksPreview && settings.ObsidianTasksEnabled;
    internal static NoteSource Create(Settings settings) => new(settings, UsesCli(settings));

    private string Root => Path.GetFullPath(settings.ObsidianVaultFolder).TrimEnd('\\', '/');
    private async Task<JsonDocument> Request(object request, CancellationToken token)
    {
        var output = await execute(settings, ObsidianTasksClient.BuildRequestCode(request, notes: true), token).ConfigureAwait(false);
        return ObsidianTasksClient.ParseEnvelope(output);
    }

    internal static FileSnapshot DecodeSnapshot(string path, JsonElement result)
    {
        var text = result.GetProperty("text").GetString() ?? throw new InvalidOperationException(L10n.Text("TasksPreview.NoResponse"));
        var hash = result.GetProperty("hash").GetString();
        var expected = "cli:" + Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(text))).ToLowerInvariant();
        if (hash != expected) throw new InvalidOperationException(L10n.Text("TasksPreview.NoResponse"));
        // Exactly the same BOM policy as NoteStore.Read. Do not trim whitespace or newlines.
        return new(Path.GetFullPath(path), text.TrimStart('\uFEFF'), hash);
    }

    public async Task<FileSnapshot> ReadAsync(string path, CancellationToken token = default)
    {
        if (!cli) return NoteStore.Read(path);
        var relative = ObsidianTasksClient.RelativeNotePath(Root, path);
        using var result = await Request(new { mode = "read", root = Root, path = relative }, token).ConfigureAwait(false);
        return DecodeSnapshot(path, result.RootElement);
    }

    // Save/close confirmation remains synchronous. Run CLI continuations off the dispatcher.
    public FileSnapshot Read(string path) => Task.Run(() => ReadAsync(path)).GetAwaiter().GetResult();

    public FileSnapshot Save(FileSnapshot expected, string text, string backup)
    {
        if (cli != expected.Hash.StartsWith("cli:", StringComparison.Ordinal))
            throw new ConflictException(L10n.Text("CliNote.ReloadBeforeSave"));
        if (!cli) return NoteStore.Save(expected, text, backup);
        return Task.Run(async () =>
        {
            var relative = ObsidianTasksClient.RelativeNotePath(Root, expected.Path);
            using var result = await Request(new { mode = "save", root = Root, path = relative, text, hash = expected.Hash,
                backup = Path.GetFullPath(backup) }, CancellationToken.None).ConfigureAwait(false);
            return DecodeSnapshot(expected.Path, result.RootElement);
        }).GetAwaiter().GetResult();
    }

    public string CreateNote(string directory)
    {
        if (!cli) return NoteStore.Create(directory);
        var path = Path.Combine(directory, L10n.Format("NoteStore.Text04", DateTime.Now, Guid.NewGuid().ToString("N")[..6]));
        var relative = ObsidianTasksClient.RelativeNotePath(Root, path);
        var text = NoteStore.InitialText();
        return Task.Run(async () =>
        {
            using var result = await Request(new { mode = "create", root = Root, path = relative, text }, CancellationToken.None).ConfigureAwait(false);
            return DecodeSnapshot(path, result.RootElement).Path;
        }).GetAwaiter().GetResult();
    }

    public async Task<string[]> MarkdownPathsAsync(CancellationToken token = default)
    {
        if (!cli) throw new InvalidOperationException("CLI file listing only");
        using var result = await Request(new { mode = "list", root = Root }, token).ConfigureAwait(false);
        var paths = result.RootElement.GetProperty("paths").Deserialize<string[]>() ?? throw new InvalidOperationException(L10n.Text("TasksPreview.NoResponse"));
        return paths.Select(relative =>
        {
            var path = Path.GetFullPath(Path.Combine(Root, relative));
            _ = ObsidianTasksClient.RelativeNotePath(Root, path);
            return path;
        }).ToArray();
    }

    public async Task<string> ResolveDailyAsync(string folder, string pattern, DateTime date, CancellationToken token = default)
    {
        if (!cli) return DailyNoteResolver.Resolve(folder, pattern, date, token);
        _ = ObsidianTasksClient.RelativeNotePath(Root, Path.Combine(folder, "validation.md"));
        return DailyNoteResolver.ResolveFromPaths(folder, pattern, date, await MarkdownPathsAsync(token).ConfigureAwait(false), token);
    }

    public NotePlacement PrepareLink(FileSnapshot source, string heading, bool daily, string backup)
    {
        heading = heading.Trim();
        var updated = SectionEditor.EnsureHeading(source.Text, heading);
        if (updated != source.Text) Save(source, updated, backup);
        else if (Read(source.Path).Hash != source.Hash) throw new ConflictException(L10n.Text("NoteLink.Text01"));
        return new NotePlacement { Path = daily ? "" : source.Path, Heading = heading, Daily = daily, Color = "green" };
    }
}
