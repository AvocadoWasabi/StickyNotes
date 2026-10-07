using System.IO;
using System.Text;
using System.Text.Json;
using StickyNotes;
using StickyNotes.Core;

internal static partial class Program
{
    private static async Task StickyFolderSmoke(Settings connection)
    {
        var fixture = "StickyNotes-folders-test-" + Guid.NewGuid().ToString("N");
        var journal = Path.Combine(Path.GetTempPath(), fixture);
        Settings At(string relative) => new() { ObsidianCli = connection.ObsidianCli, ObsidianVaultFolder = connection.ObsidianVaultFolder,
            ObsidianVaultId = connection.ObsidianVaultId, ObsidianNotesFolder = relative };
        var old = At(fixture + "/old"); var next = At(fixture + "/new");
        var access = new NoteSource(old, true);
        try
        {
            await access.EnsureFolderAsync(StickyFolderPath.Absolute(connection.ObsidianVaultFolder, old.ObsidianNotesFolder!));
            var note = new NoteSource(At(old.ObsidianNotesFolder!.ToUpperInvariant()), true).CreateNote("unused");
            Check(note.Contains(fixture, StringComparison.Ordinal), "Sticky folder desktop: case-varied selection resolves the existing folder");
            var before = await access.ReadAsync(note);
            using (var result = ObsidianTasksClient.ParseEnvelope(await ObsidianTasksClient.ExecuteAsync(old,
                ObsidianTasksClient.BuildRequestCode(new { mode = "create", root = old.ObsidianVaultFolder,
                    folder = old.ObsidianNotesFolder, name = "ordinary.md", text = "# Ordinary fixture" }, notes: true), default))) { }
            var catalog = await access.GetFoldersAsync();
            Check(catalog.Folders.Contains(old.ObsidianNotesFolder!), "Sticky folder desktop: native vault folder listing includes synthetic source");
            // Metadata indexing can lag a just-created file; wait for this fixture only.
            for (var i = 0; i < 20 && (await access.StickyFilesAsync(Path.GetDirectoryName(note)!, StickyFolderPath.Absolute(old.ObsidianVaultFolder, next.ObsidianNotesFolder!))).Length == 0; i++)
                await Task.Delay(100);
            IReadOnlyDictionary<string, string>? mapping = null;
            await StickyFolderChange.ApplyAsync(old, next, true, NoteSource.Create, paths => mapping = paths, journal);
            Check(mapping?.Count == 1 && mapping.ContainsKey(note), "Sticky folder desktop: only app-created fixture is migrated");
            var moved = mapping![note];
            Check((await access.ReadAsync(moved)).Text == before.Text && !await access.PathExistsAsync(note), "Sticky folder desktop: CLI move preserves text and removes old path");
            Check(await access.PathExistsAsync(Path.Combine(Path.GetDirectoryName(note)!, "ordinary.md")), "Sticky folder desktop: ordinary Markdown remains at source");
            var rollback = At(fixture + "/rollback");
            try { await StickyFolderChange.ApplyAsync(next, rollback, true, NoteSource.Create, paths =>
                { Check(paths.Count == 1, "Sticky folder desktop: rollback test actually moved the fixture"); throw new IOException("fixture settings failure"); }, journal); throw new Exception("Expected rollback"); }
            catch (IOException) { }
            Check(await access.PathExistsAsync(moved) && !await access.PathExistsAsync(Path.Combine(old.ObsidianVaultFolder, rollback.ObsidianNotesFolder!, Path.GetFileName(note))), "Sticky folder desktop: failed settings commit rolls native rename back");
        }
        finally
        {
            var payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new { root = connection.ObsidianVaultFolder, path = fixture })));
            var code = "(async()=>{const q=JSON.parse(Buffer.from('" + payload + "','base64').toString('utf8'));if(app.vault.adapter.getBasePath().replace(/\\\\/g,'/').toLowerCase()!==q.root.replace(/\\\\/g,'/').toLowerCase())throw Error('Wrong fixture vault');const f=app.vault.getAbstractFileByPath(q.path);if(f&&f.path===q.path&&f.parent===app.vault.getRoot())await app.vault.delete(f,true);if(await app.vault.adapter.exists(q.path))throw Error('Fixture still exists');return 'folder fixture removed';})()";
            var result = await ObsidianTasksClient.ExecuteAsync(connection, code, default);
            Check(result.Contains("folder fixture removed"), "Sticky folder desktop: exact synthetic folder tree removed");
            if (Directory.Exists(journal)) Directory.Delete(journal, true);
        }
    }
}
