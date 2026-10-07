using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using StickyNotes;
using StickyNotes.Core;

internal static partial class Program
{
    private static async Task CliNoteFixtureSmoke(Settings settings)
    {
        var relative = "StickyNotes-CLI-test-" + Guid.NewGuid().ToString("N") + ".md";
        var path = Path.Combine(settings.ObsidianVaultFolder, relative);
        var backup = Path.Combine(Path.GetTempPath(), "StickyNotes-CLI-test-" + Guid.NewGuid().ToString("N"));
        var created = false;
        using var timeout = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(60));
        try
        {
            var original = "\uFEFF" + RenderingCorpus.Replace("\n", "\r\n");
            var code = ObsidianTasksClient.BuildRequestCode(new { mode = "create", root = settings.ObsidianVaultFolder, name = relative, text = original }, notes: true);
            var creationOutput = await ObsidianTasksClient.ExecuteAsync(settings, code, timeout.Token);
            using var creation = ObsidianTasksClient.ParseEnvelope(creationOutput);
            relative = creation.RootElement.GetProperty("path").GetString()!;
            path = Path.GetFullPath(Path.Combine(settings.ObsidianVaultFolder, relative));
            _ = ObsidianTasksClient.RelativeNotePath(settings.ObsidianVaultFolder, path);
            created = true;
            using var parent = ObsidianTasksClient.ParseEnvelope(await ObsidianTasksClient.ExecuteAsync(settings,
                "'STICKY_TASKS_PREVIEW:'+JSON.stringify({path:app.fileManager.getNewFileParent('').path})", timeout.Token));
            var parentPath = parent.RootElement.GetProperty("path").GetString()!.Trim('/');
            Check(relative == (parentPath.Length == 0 ? "" : parentPath + "/") + Path.GetFileName(path), "CLI desktop fixture: new note uses Obsidian's configured new-note folder");
            var access = new NoteSource(settings, true);
            var remote = await access.ReadAsync(path, timeout.Token);
            var local = NoteStore.Read(path); // Test reference only; production CLI mode never does this.
            Check(remote.Text == local.Text && remote.Text == original.TrimStart('\uFEFF'), "CLI desktop fixture: actual CLI preserves BOM policy, CRLF, Unicode and trailing blanks");
            Exception? renderingError = null;
            var renderThread = new System.Threading.Thread(() =>
            {
                try
                {
                    Check(RenderSignature(remote.Text) == RenderSignature(local.Text), "CLI desktop fixture: actual acquired strings produce identical WPF rendering");
                    Check(RenderSignature(remote.Text, true) == RenderSignature(local.Text, true), "CLI desktop fixture: query replacement preserves surrounding formatting");
                }
                catch (Exception ex) { renderingError = ex; }
            });
            renderThread.SetApartmentState(System.Threading.ApartmentState.STA); renderThread.Start(); renderThread.Join();
            if (renderingError is not null) throw renderingError;
            var before = File.ReadAllBytes(path);
            Check(remote.Hash == "cli:" + Convert.ToHexString(SHA256.HashData(before)).ToLowerInvariant(), "CLI desktop fixture: snapshot hash matches exact on-disk UTF-8 bytes before save");
            var updated = remote.Text.Replace("- [ ] normal task", "- [x] normal task");
            var saved = access.Save(remote, updated, backup);
            Check((await access.ReadAsync(path, timeout.Token)).Text == updated && saved.Text == updated, "CLI desktop fixture: write and subsequent read use Obsidian Vault APIs");
            Check(File.ReadAllBytes(Directory.GetFiles(backup, "*.md").Single()).SequenceEqual(before), "CLI desktop fixture: backup preserves original BOM and CRLF");
            Throws<ConflictException>(() => access.Save(remote, "stale overwrite", backup), "CLI desktop fixture: stale save rejected atomically by Obsidian");
            Check((await access.ReadAsync(path, timeout.Token)).Text == updated, "CLI desktop fixture: conflict preserves updated note");
            var malformedPayload = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new { root = settings.ObsidianVaultFolder, path = relative })));
            var malformedCode = "(async()=>{const q=JSON.parse(Buffer.from('" + malformedPayload + "','base64').toString('utf8'));if(app.vault.adapter.getBasePath().replace(/\\\\/g,'/').toLowerCase()!==q.root.replace(/\\\\/g,'/').toLowerCase())throw Error('Wrong fixture vault');await app.vault.modifyBinary(app.vault.getAbstractFileByPath(q.path),new Uint8Array([255,254]).buffer);return 'fixture encoding changed';})()";
            await ObsidianTasksClient.ExecuteAsync(settings, malformedCode, timeout.Token);
            Throws<InvalidOperationException>(() => access.Read(path), "CLI desktop fixture: invalid UTF-8 is rejected instead of silently replacing text");
            Throws<DecoderFallbackException>(() => NoteStore.Read(path), "CLI desktop fixture: original reader rejects the same invalid UTF-8");
        }
        finally
        {
            if (created)
            {
                // Delete only the exact random fixture created by this test, in the same vault.
                var payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new { root = settings.ObsidianVaultFolder, path = relative })));
                var cleanup = "(async()=>{const q=JSON.parse(new TextDecoder().decode(Uint8Array.from(atob('" + payload + "'),c=>c.charCodeAt(0))));if(app.vault.adapter.getBasePath().replace(/\\\\/g,'/').toLowerCase()!==q.root.replace(/\\\\/g,'/').toLowerCase())throw Error('Wrong cleanup vault');const f=app.vault.getAbstractFileByPath(q.path);if(f)await app.vault.delete(f);return 'fixture removed';})()";
                var result = await ObsidianTasksClient.ExecuteAsync(settings, cleanup, System.Threading.CancellationToken.None);
                Check(result.Contains("fixture removed"), "CLI desktop fixture: temporary note cleaned up");
            }
            if (Directory.Exists(backup)) Directory.Delete(backup, true);
        }
    }

    private static string CliSnapshotEnvelope(string text) => "CLI diagnostic\n=> STICKY_TASKS_PREVIEW:" + JsonSerializer.Serialize(new
    { text, hash = "cli:" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant() }) + "\n";

    private const string RenderingCorpus = "---\ntitle: 日本語 / 中文\ncustom: keep\n---\n# 見出し 😀\n\nParagraph **bold** *italic* ~~strike~~ `code`  \nnext line\\n literal\n\n## Section\n- [ ] normal task\n  - [x] nested task\n\n> quote **bold**\n\n1. ordered\n2. second\n\n| A | B |\n|---|---|\n| 日本語 | 中文 |\n\n[link](https://example.com/a?q=b) ![画像](image.png) [[Wiki]]\n\n```text\n- [ ] code sample\n```\n\n```tasks\nnot done\nlimit 3\n```\n\n<div>literal HTML</div>\n\n---\n\nTail\n\n";

    private static string RenderSignature(string text, bool queries = false)
    {
        var body = NoteStore.Split(text).Body;
        var document = MarkdownView.Render(body, (_, _) => { }, queries ? _ => new Section(new Paragraph(new Run("Native Tasks placeholder"))) : null);
        return XamlWriter.Save(document);
    }

    private static void NoteSourceTests(string root)
    {
        var folder = Path.Combine(root, "source-parity"); Directory.CreateDirectory(folder);
        var settings = new Settings { ObsidianVaultFolder = folder };
        var local = new NoteSource(settings, false, (_, _, _) => throw new Exception("CLI called in local mode"));
        var variants = new[] { "", "one line without trailing newline", RenderingCorpus, RenderingCorpus.Replace("\n", "\r\n"),
            "\uFEFF" + RenderingCorpus, "\uFEFF" + RenderingCorpus.Replace("\n", "\r\n"), "  leading\n\ntrailing  \n\n" };
        foreach (var (text, index) in variants.Select((text, index) => (text, index)))
        {
            var path = Path.Combine(folder, index + ".md");
            File.WriteAllText(path, text, new UTF8Encoding(false));
            var disk = local.Read(path);
            var calls = 0;
            var cli = new NoteSource(settings, true, (_, _, _) => { calls++; return Task.FromResult(CliSnapshotEnvelope(text)); });
            var remote = cli.Read(path);
            Check(calls == 1 && disk.Text == remote.Text, "Note source " + index + ": BOM/newlines/Unicode/whitespace preserved across transports");
            Check(RenderSignature(disk.Text) == RenderSignature(remote.Text), "Note source " + index + ": non-query WPF structure and formatting identical");
            Check(RenderSignature(disk.Text, true) == RenderSignature(remote.Text, true), "Note source " + index + ": surrounding rendering identical with query replacement");
            if (text.Contains("## Section"))
                Check(SectionEditor.Find(disk.Text, "Section").Content == SectionEditor.Find(remote.Text, "Section").Content &&
                    SectionEditor.Headings(disk.Text).SequenceEqual(SectionEditor.Headings(remote.Text)), "Note source " + index + ": heading extraction identical");
            var before = File.ReadAllBytes(path);
            Throws<ConflictException>(() => local.Save(remote, "bad", Path.Combine(folder, "backups")), "Note source: CLI snapshot cannot be saved through disk source");
            Throws<ConflictException>(() => cli.Save(disk, "bad", Path.Combine(folder, "backups")), "Note source: disk snapshot cannot be saved through CLI source");
            Check(File.ReadAllBytes(path).SequenceEqual(before), "Note source: mode mismatch preserves file");
            File.Delete(path);
            Check(cli.Read(path).Text == remote.Text, "Note source: CLI read does not require a locally readable file");
            var failing = new NoteSource(settings, true, (_, _, _) => throw new IOException("CLI disconnected"));
            Throws<IOException>(() => failing.Read(path), "Note source: CLI failure never falls back to local reading");
        }
        ObsidianDailyTests(root);
        CliNoteWindowTests(root);
    }

    private static void CliNoteWindowTests(string root)
    {
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        var app = App.Current; var settings = app.Config;
        var previousFactory = app.NoteSources; var previousQueries = app.TasksQueries;
        var enabled = settings.ObsidianTasksEnabled; var previousVault = settings.ObsidianVaultFolder;
        var path = Path.Combine(root, "cli-only-note.md"); // Deliberately absent from disk.
        NoteWindow? window = null;
        var reads = 0; var fail = false;
        try
        {
            settings.ObsidianTasksEnabled = true; settings.ObsidianVaultFolder = root;
            app.NoteSources = config => new NoteSource(config, true, (_, _, _) =>
            {
                reads++;
                return fail ? Task.FromException<string>(new IOException("CLI failed")) : Task.FromResult(CliSnapshotEnvelope(RenderingCorpus));
            });
            app.TasksQueries = (_, _, queries, _) => Task.FromResult(new TasksResponse("test", queries.Select(_ => new TasksOutput("- [ ] remote result", null)).ToArray()));
            window = new NoteWindow(new NotePlacement { Path = path });
            typeof(NoteWindow).GetMethod("Reload", flags)!.Invoke(window, null);
            var viewer = (FlowDocumentScrollViewer)typeof(NoteWindow).GetField("preview", flags)!.GetValue(window)!;
            Check(reads == 1 && new TextRange(viewer.Document.ContentStart, viewer.Document.ContentEnd).Text.Contains("remote result"), "CLI window: first load obtains whole Markdown through CLI before running queries");
            var baseline = NoteStore.Split(RenderingCorpus).Body;
            Check((string)typeof(NoteWindow).GetField("content", flags)!.GetValue(window)! == baseline, "CLI window: original Markdown is retained separately from generated query output");
            fail = true;
            typeof(NoteWindow).GetMethod("ReloadCliNoteAsync", flags)!.Invoke(window, [true]);
            Check(new TextRange(viewer.Document.ContentStart, viewer.Document.ContentEnd).Text.Contains("remote result") &&
                ((TextBlock)typeof(NoteWindow).GetField("status", flags)!.GetValue(window)!).Text.Contains("CLI failed"), "CLI window: failed refresh retains prior CLI content and reports error without disk fallback");
            fail = false;
            var dialog = new NoteLinkWindow(false);
            dialog.LoadSource(path);
            Check(dialog.Preview.Body.Text.Contains("Paragraph") && dialog.Headings.Input.Items.Contains("Section"), "CLI linking: heading selection and preview use CLI Markdown even without local file");
            dialog.Close();
            typeof(NoteWindow).GetMethod("BeginEdit", flags)!.Invoke(window, null);
            var editor = (TextBox)typeof(NoteWindow).GetField("editor", flags)!.GetValue(window)!;
            editor.Text = "unsaved draft";
            typeof(NoteWindow).GetMethod("ReloadCliNoteAsync", flags)!.Invoke(window, [true]);
            Check(editor.Text == "unsaved draft", "CLI background read preserves an active draft");
            typeof(NoteWindow).GetMethod("Reload", flags)!.Invoke(window, null);
            Check(!(bool)typeof(NoteWindow).GetField("dirty", flags)!.GetValue(window)!, "CLI explicit discard reload clears draft only after user action");
            var pending = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
            app.NoteSources = config => new NoteSource(config, true, (_, _, _) => pending.Task);
            var oldRead = (Task)typeof(NoteWindow).GetMethod("ReloadCliNoteAsync", flags)!.Invoke(window, [true])!;
            app.NoteSources = config => new NoteSource(config, true, (_, _, _) => Task.FromResult(CliSnapshotEnvelope("# Latest response")));
            typeof(NoteWindow).GetMethod("ReloadCliNoteAsync", flags)!.Invoke(window, [true]);
            pending.SetResult(CliSnapshotEnvelope("# Obsolete response"));
            WaitFor(() => oldRead.IsCompleted, "CLI window: cancelled response settles");
            Check((string)typeof(NoteWindow).GetField("content", flags)!.GetValue(window)! == "# Latest response", "CLI window: late obsolete response cannot overwrite newer snapshot");
            var preSave = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
            app.NoteSources = config => new NoteSource(config, true, (_, _, _) => preSave.Task);
            var preSaveRead = (Task)typeof(NoteWindow).GetMethod("ReloadCliNoteAsync", flags)!.Invoke(window, [true])!;
            app.NoteSources = config => new NoteSource(config, true, (_, _, _) => Task.FromResult(CliSnapshotEnvelope("# Saved response")));
            typeof(NoteWindow).GetMethod("SaveContent", flags)!.Invoke(window, ["# Saved response"]);
            preSave.SetResult(CliSnapshotEnvelope("# Before save"));
            WaitFor(() => preSaveRead.IsCompleted, "CLI window: read started before save settles");
            Check((string)typeof(NoteWindow).GetField("content", flags)!.GetValue(window)! == "# Saved response", "CLI window: old read cannot replace a successfully saved snapshot");
            typeof(NoteWindow).GetMethod("ReloadCliNoteAsync", flags)!.Invoke(window, [true]);
            var closing = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
            app.NoteSources = config => new NoteSource(config, true, (_, _, _) => closing.Task);
            var closingRead = (Task)typeof(NoteWindow).GetMethod("ReloadCliNoteAsync", flags)!.Invoke(window, [true])!;
            window.Close(); closing.SetResult(CliSnapshotEnvelope("after close"));
            WaitFor(() => closingRead.IsCompleted, "CLI window: late response settles after close");
            Check((string)typeof(NoteWindow).GetField("content", flags)!.GetValue(window)! == "# Saved response", "CLI window: closed window ignores late response");
        }
        finally
        {
            window?.Close(); settings.ObsidianTasksEnabled = enabled; settings.ObsidianVaultFolder = previousVault;
            app.NoteSources = previousFactory; app.TasksQueries = previousQueries;
        }
    }
}
