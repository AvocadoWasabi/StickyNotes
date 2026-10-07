using System.Text.Json;
using System.Threading;

namespace StickyNotes;

// All note-content I/O is selected once per operation; CLI failures never fall back to disk.
internal sealed partial class NoteSource
{
    private readonly Settings settings;
    private readonly bool cli;
    private readonly Func<Settings, string, CancellationToken, Task<string>> execute;

    internal NoteSource(Settings settings, bool cli,
        Func<Settings, string, CancellationToken, Task<string>>? execute = null)
    {
        // Do not let an in-flight request adopt a newly selected vault/profile.
        this.settings = new Settings { ObsidianCli = settings.ObsidianCli,
            ObsidianVaultFolder = settings.ObsidianVaultFolder, ObsidianVaultId = settings.ObsidianVaultId,
            NotesFolder = settings.NotesFolder, NotesRoot = settings.NotesRoot, ObsidianNotesFolder = settings.ObsidianNotesFolder,
            DailyFolder = settings.DailyFolder, DailyPattern = settings.DailyPattern };
        this.cli = cli; this.execute = execute ?? ObsidianTasksClient.ExecuteAsync;
    }

    internal bool IsCli => cli;
    internal static bool UsesCli(Settings settings) => settings.ObsidianTasksEnabled;
    internal static NoteSource Create(Settings settings) => new(settings, UsesCli(settings));

    private string Root => Path.GetFullPath(settings.ObsidianVaultFolder).TrimEnd('\\', '/');
    internal async Task<(string Root, string Name)> DiscoverVaultAsync(CancellationToken token = default)
    {
        const string code = "'STICKY_TASKS_PREVIEW:'+JSON.stringify({root:app.vault.adapter.getBasePath(),name:app.vault.getName()})";
        using var result = ObsidianTasksClient.ParseEnvelope(await execute(settings, code, token).ConfigureAwait(false));
        var root = result.RootElement.GetProperty("root").GetString();
        var name = result.RootElement.GetProperty("name").GetString();
        if (root is null || !Path.IsPathFullyQualified(root) || string.IsNullOrWhiteSpace(name))
            throw new InvalidOperationException(L10n.Text("TasksPreview.NoResponse"));
        return (Path.GetFullPath(root), name);
    }
    private async Task<JsonDocument> Request(object request, CancellationToken token)
    {
        using var command = ObsidianTasksClient.PrepareNoteCommand(settings, request);
        var output = await execute(settings, command.Code, token).ConfigureAwait(false);
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

    internal async Task<FileSnapshot> ToggleQueryTaskAsync(QueryTaskTarget task, string provider, bool value, string backup)
    {
        if (!cli || System.Windows.Application.Current is App { IsChangingFolder: true })
            throw new InvalidOperationException(L10n.Text("CliNote.ReloadBeforeSave"));
        var path = StickyFolderPath.Absolute(Root, task.Path);
        var relative = ObsidianTasksClient.RelativeNotePath(Root, path);
        if (task.Line < 0 || task.Line > 2000000 || provider is not ("tasks" or "dataview") ||
            !System.Text.RegularExpressions.Regex.IsMatch(task.Hash, "^cli:[0-9a-f]{64}$"))
            throw new InvalidOperationException(L10n.Text("TasksPreview.NoResponse"));
        using var result = await Request(new { mode = "toggle-query-task", root = Root, path = relative,
            hash = task.Hash, line = task.Line, provider, @checked = value, backup = Path.GetFullPath(backup) }, CancellationToken.None).ConfigureAwait(false);
        return DecodeSnapshot(path, result.RootElement);
    }

    public FileSnapshot Save(FileSnapshot expected, string text, string backup)
    {
        if (System.Windows.Application.Current is App { IsChangingFolder: true }) throw new InvalidOperationException(L10n.Text("StickyFolder.Working"));
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
        var name = L10n.Format("NoteStore.Text04", DateTime.Now, Guid.NewGuid().ToString("N")[..6]);
        var text = NoteStore.InitialText();
        return Task.Run(async () =>
        {
            var folder = settings.ObsidianNotesFolder is null ? null : StickyFolderPath.Normalize(settings.ObsidianNotesFolder);
            using var result = await Request(new { mode = "create", root = Root, name, text, folder }, CancellationToken.None).ConfigureAwait(false);
            var path = Path.GetFullPath(Path.Combine(Root, result.RootElement.GetProperty("path").GetString()!));
            _ = ObsidianTasksClient.RelativeNotePath(Root, path);
            return DecodeSnapshot(path, result.RootElement).Path;
        }).GetAwaiter().GetResult();
    }

    public async Task<ObsidianDailyNotes> GetDailyNotesAsync(DateTime today, CancellationToken token = default)
    {
        if (!cli) throw new InvalidOperationException("Obsidian daily settings require CLI mode.");
        var code = ObsidianTasksClient.BuildDailyCode(new { root = Root,
            dates = new[] { ObsidianDailyNotes.DateKey(today), ObsidianDailyNotes.DateKey(today.AddDays(-1)) } });
        using var result = ObsidianTasksClient.ParseEnvelope(await execute(settings, code, token).ConfigureAwait(false));
        return ObsidianDailyNotes.Decode(result.RootElement, Root, today);
    }

    public async Task<string> ResolveDailyAsync(DateTime date, CancellationToken token = default)
    {
        if (!cli) return DailyNoteResolver.Resolve(settings.DailyFolder, settings.DailyPattern, date, token);
        return (await GetDailyNotesAsync(date, token).ConfigureAwait(false)).Resolve(date);
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
