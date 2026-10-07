using System.Text.Json;

namespace StickyNotes;

internal static class StickyFolderChange
{
    internal static async Task ApplyAsync(Settings previous, Settings next, bool migrate, Func<Settings, NoteSource> sources,
        Action<IReadOnlyDictionary<string, string>> commit, string journalDirectory)
    {
        var target = sources(next);
        var destination = await target.EffectiveNotesFolderAsync();
        _ = StickyFolderPath.Relative(target.FolderRoot, destination);
        var paths = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var files = Array.Empty<StickyFile>();
        if (migrate)
        {
            if (NoteSource.UsesCli(previous) != NoteSource.UsesCli(next) ||
                target.IsCli && !string.IsNullOrWhiteSpace(previous.ObsidianVaultFolder) && !NoteFolderMigration.SameFolder(previous.ObsidianVaultFolder, next.ObsidianVaultFolder))
                throw new InvalidOperationException(L10n.Text("StickyFolder.CrossSource"));
            var old = target.IsCli && string.IsNullOrWhiteSpace(previous.ObsidianVaultFolder) ? target : sources(previous);
            var source = target.IsCli && string.IsNullOrWhiteSpace(previous.ObsidianVaultFolder)
                ? StickyFolderPath.Absolute(target.FolderRoot, (await target.GetFoldersAsync()).DefaultFolder)
                : await old.EffectiveNotesFolderAsync();
            if (!NoteFolderMigration.SameFolder(source, destination))
            {
                files = await old.StickyFilesAsync(source, destination);
                foreach (var file in files)
                {
                    var dest = StickyFolderPath.Absolute(destination, StickyFolderPath.Relative(source, file.Path));
                    if (paths.ContainsKey(file.Path) || paths.Values.Contains(dest, StringComparer.OrdinalIgnoreCase) || await target.PathExistsAsync(dest))
                        throw new IOException(L10n.Text("StickyFolder.Conflict") + "\n" + dest);
                    paths.Add(file.Path, dest);
                }
            }
        }
        // Keep exact recovery paths outside the vault if the app/CLI is interrupted.
        string? journal = null;
        if (files.Length > 0)
        {
            Directory.CreateDirectory(journalDirectory);
            journal = Path.Combine(journalDirectory, "folder-move-" + Guid.NewGuid().ToString("N") + ".json");
            File.WriteAllText(journal, JsonSerializer.Serialize(files.Select(f => new { From = f.Path, To = paths[f.Path], f.Hash }), new JsonSerializerOptions { WriteIndented = true }));
        }
        var attempted = new List<StickyFile>();
        try
        {
            await target.EnsureFolderAsync(destination);
            foreach (var file in files)
            {
                attempted.Add(file);
                await target.MoveStickyAsync(file.Path, paths[file.Path], file.Hash);
            }
            commit(paths);
        }
        catch (Exception error)
        {
            var failures = new List<string>();
            foreach (var file in attempted.AsEnumerable().Reverse())
            {
                try { await target.MoveStickyAsync(paths[file.Path], file.Path, file.Hash); }
                catch (Exception rollback) { failures.Add(paths[file.Path] + " → " + file.Path + ": " + rollback.Message); }
            }
            if (failures.Count > 0) throw new IOException(L10n.Text("StickyFolder.Recovery") + "\n" + journal + "\n" + string.Join("\n", failures), error);
            if (journal is not null) File.Delete(journal);
            throw;
        }
        // A cleanup failure must not roll back a successfully persisted configuration.
        if (journal is not null) { try { File.Delete(journal); } catch (IOException) { } catch (UnauthorizedAccessException) { } }
    }
}
