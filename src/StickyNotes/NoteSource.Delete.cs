using System.Text.Json;

namespace StickyNotes;

internal sealed partial class NoteSource
{
    internal async Task DeleteAsync(FileSnapshot expected, string recoveryDirectory)
    {
        var relative = StickyFolderPath.Relative(FolderRoot, expected.Path);
        if (!relative.EndsWith(".md", StringComparison.OrdinalIgnoreCase) || cli != expected.Hash.StartsWith("cli:", StringComparison.Ordinal))
            throw new InvalidOperationException(L10n.Text("StickyFolder.Invalid"));
        NoteFolderMigration.CheckPath(expected.Path);
        recoveryDirectory = NoteFolderMigration.Normalize(recoveryDirectory);
        NoteFolderMigration.CheckPath(recoveryDirectory);
        var operation = Guid.NewGuid().ToString("N");
        var recovery = Path.Combine(recoveryDirectory, operation);
        Directory.CreateDirectory(recovery);
        // Save the original location before any mutation, including a lost CLI response.
        File.WriteAllText(Path.Combine(recovery, "source.json"), JsonSerializer.Serialize(new { expected.Path, expected.Hash }));
        if (cli)
        {
            var request = new { mode = "delete", root = Root, path = relative, hash = expected.Hash, backup = recovery, operation };
            try { using var result = await FolderRequest(request).ConfigureAwait(false); }
            catch
            {
                // The bridge serializes operations and caches the outcome by identity.
                using var result = await FolderRequest(request).ConfigureAwait(false);
            }
            return;
        }
        if (NoteStore.Read(expected.Path).Hash != expected.Hash) throw new ConflictException(L10n.Text("StickyFolder.Conflict"));
        var target = Path.Combine(recovery, Path.GetFileName(expected.Path));
        File.Move(expected.Path, target); // Recoverable removal; never overwrite a backup.
        if (NoteStore.Read(target).Hash != expected.Hash)
        {
            // A concurrent replacement is also preserved. Restore only if the old name is free.
            if (!File.Exists(expected.Path)) File.Move(target, expected.Path);
            throw new ConflictException(L10n.Text("StickyFolder.Conflict") + "\n" + recovery);
        }
    }
}
