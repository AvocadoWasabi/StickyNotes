using System.Text.Json;
using System.Threading;

namespace StickyNotes;

internal sealed record StickyFolderCatalog(string[] Folders, string DefaultFolder);
internal sealed record StickyFile(string Path, string Hash);

internal sealed partial class NoteSource
{
    internal string FolderRoot => cli ? Root : NoteFolderMigration.Normalize(string.IsNullOrWhiteSpace(settings.NotesRoot) ? settings.NotesFolder : settings.NotesRoot);

    private async Task<JsonDocument> FolderRequest(object data, CancellationToken token = default) =>
        ObsidianTasksClient.ParseEnvelope(await execute(settings, ObsidianTasksClient.BuildFoldersCode(data), token).ConfigureAwait(false));

    internal async Task<StickyFolderCatalog> GetFoldersAsync(CancellationToken token = default)
    {
        if (cli)
        {
            using var json = await FolderRequest(new { mode = "folders", root = Root }, token).ConfigureAwait(false);
            var catalog = json.RootElement.Deserialize<StickyFolderCatalog>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
            if (catalog.Folders.Length > 10001) throw new IOException(L10n.Text("StickyFolder.Limit"));
            return new(catalog.Folders.Select(StickyFolderPath.Normalize).Distinct(StringComparer.OrdinalIgnoreCase).Order().ToArray(), StickyFolderPath.Normalize(catalog.DefaultFolder));
        }
        return await Task.Run(() =>
        {
            var folders = new List<string> { "." };
            Walk(FolderRoot, path => { folders.Add(StickyFolderPath.Relative(FolderRoot, path)); if (folders.Count > 10001) throw new IOException(L10n.Text("StickyFolder.Limit")); }, null, token);
            return new StickyFolderCatalog(folders.Order().ToArray(), ".");
        }, token).ConfigureAwait(false);
    }

    internal async Task<string> EffectiveNotesFolderAsync() => !cli ? NoteFolderMigration.Normalize(settings.NotesFolder) :
        StickyFolderPath.Absolute(Root, settings.ObsidianNotesFolder ?? (await GetFoldersAsync().ConfigureAwait(false)).DefaultFolder);

    internal async Task EnsureFolderAsync(string absolute)
    {
        if (!cli) { NoteFolderMigration.CheckPath(absolute); Directory.CreateDirectory(absolute); return; }
        using var result = await FolderRequest(new { mode = "ensure", root = Root, path = StickyFolderPath.Relative(Root, absolute) }).ConfigureAwait(false);
    }

    internal async Task<string[]> BrowseNotesAsync(string folder, CancellationToken token = default)
    {
        var relative = StickyFolderPath.Relative(FolderRoot, folder);
        if (cli)
        {
            using var response = await FolderRequest(new { mode = "browse", root = Root, path = relative }, token).ConfigureAwait(false);
            var notes = response.RootElement.GetProperty("notes").Deserialize<string[]>()!;
            if (notes.Length > 1000) throw new IOException(L10n.Text("StickyFolder.Limit"));
            return notes.Select(p =>
            {
                var full = StickyFolderPath.Absolute(Root, p);
                _ = ObsidianTasksClient.RelativeNotePath(folder, full);
                return full;
            }).Distinct(StringComparer.OrdinalIgnoreCase).Order(StringComparer.OrdinalIgnoreCase).ToArray();
        }
        return await Task.Run(() =>
        {
            if (!Directory.Exists(folder)) throw new DirectoryNotFoundException(folder);
            var notes = new List<string>();
            Walk(folder, _ => { }, path =>
            {
                if (Path.GetExtension(path).Equals(".md", StringComparison.OrdinalIgnoreCase)) notes.Add(path);
                if (notes.Count > 1000) throw new IOException(L10n.Text("StickyFolder.Limit"));
            }, token);
            return notes.Order(StringComparer.OrdinalIgnoreCase).ToArray();
        }, token).ConfigureAwait(false);
    }

    internal async Task<bool> PathExistsAsync(string absolute)
    {
        if (!cli) { NoteFolderMigration.CheckPath(absolute); return File.Exists(absolute) || Directory.Exists(absolute); }
        using var result = await FolderRequest(new { mode = "exists", root = Root, path = StickyFolderPath.Relative(Root, absolute) }).ConfigureAwait(false);
        return result.RootElement.GetProperty("exists").GetBoolean();
    }

    internal async Task<StickyFile[]> StickyFilesAsync(string source, string destination)
    {
        if (cli)
        {
            using var result = await FolderRequest(new { mode = "sticky", root = Root, path = StickyFolderPath.Relative(Root, source), exclude = StickyFolderPath.Relative(Root, destination) }).ConfigureAwait(false);
            var notes = result.RootElement.GetProperty("notes").Deserialize<StickyFile[]>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
            if (notes.Length > 1000) throw new IOException(L10n.Text("StickyFolder.Limit"));
            return notes.Select(note =>
            {
                var path = StickyFolderPath.Absolute(Root, note.Path);
                _ = ObsidianTasksClient.RelativeNotePath(Root, path);
                if (!NoteFolderMigration.Contains(source, path) || NoteFolderMigration.Contains(destination, path) && NoteFolderMigration.Contains(source, destination) ||
                    !System.Text.RegularExpressions.Regex.IsMatch(note.Hash, "^cli:[0-9a-f]{64}$")) throw new IOException(L10n.Text("StickyFolder.Invalid"));
                return note with { Path = path };
            }).ToArray();
        }
        return await Task.Run(() =>
        {
            var files = new List<StickyFile>();
            Walk(source, _ => { }, path =>
            {
                if (!path.EndsWith(".md", StringComparison.OrdinalIgnoreCase)) return;
                var snapshot = NoteStore.Read(path);
                if (NoteStore.IsSticky(snapshot.Text)) files.Add(new(path, snapshot.Hash));
                if (files.Count > 1000) throw new IOException(L10n.Text("StickyFolder.Limit"));
            }, default, destination);
            return files.ToArray();
        }).ConfigureAwait(false);
    }

    // Also used in reverse for rollback. A timed-out move can have completed remotely.
    internal async Task MoveStickyAsync(string from, string to, string hash)
    {
        if (cli)
        {
            using var result = await FolderRequest(new { mode = "move", root = Root,
                path = ObsidianTasksClient.RelativeNotePath(Root, from), target = ObsidianTasksClient.RelativeNotePath(Root, to), hash }).ConfigureAwait(false);
            return;
        }
        NoteFolderMigration.CheckPath(from); NoteFolderMigration.CheckPath(to);
        if (!File.Exists(from) && File.Exists(to) && NoteStore.Read(to).Hash == hash) return;
        if (File.Exists(to) || Directory.Exists(to) || NoteStore.Read(from).Hash != hash) throw new ConflictException(L10n.Text("StickyFolder.Conflict"));
        Directory.CreateDirectory(Path.GetDirectoryName(to)!);
        File.Move(from, to);
    }

    private static void Walk(string root, Action<string> folder, Action<string>? file, CancellationToken token, string? exclude = null)
    {
        NoteFolderMigration.CheckPath(root);
        if (!Directory.Exists(root)) return;
        var pending = new Stack<string>(); pending.Push(root);
        var count = 0;
        while (pending.TryPop(out var current))
        {
            token.ThrowIfCancellationRequested();
            foreach (var entry in Directory.EnumerateFileSystemEntries(current))
            {
                token.ThrowIfCancellationRequested();
                if (++count > 10000) throw new IOException(L10n.Text("StickyFolder.Limit"));
                var attributes = File.GetAttributes(entry);
                if ((attributes & (FileAttributes.ReparsePoint | FileAttributes.Hidden | FileAttributes.System)) != 0 || Path.GetFileName(entry).StartsWith('.')) continue;
                if ((attributes & FileAttributes.Directory) != 0)
                {
                    if (exclude is not null && NoteFolderMigration.SameFolder(exclude, entry)) continue;
                    folder(entry); pending.Push(entry);
                }
                else file?.Invoke(entry);
            }
        }
    }
}
