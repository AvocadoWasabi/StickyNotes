using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows.Controls;
using StickyNotes;
using StickyNotes.Core;

internal static partial class Program
{
    private static void NoteDeleteTests(string root)
    {
        var folder = Path.Combine(root, "delete-tests"); Directory.CreateDirectory(folder);
        var recovery = Path.Combine(root, "deleted-recovery");
        var path = Path.Combine(folder, "日本語.md");
        File.WriteAllText(path, "---\r\ntitle: Keep\r\n---\r\n## Section\r\nBody\r\n", new UTF8Encoding(true));
        var settings = new Settings { ObsidianTasksEnabled = false, NotesRoot = folder, NotesFolder = folder };
        var source = new NoteSource(settings, false);
        var expected = NoteStore.Read(path);
        var bytes = File.ReadAllBytes(path);
        var outside = Path.Combine(root, "outside.md"); File.WriteAllText(outside, "outside");
        Throws<InvalidOperationException>(() => source.DeleteAsync(NoteStore.Read(outside), recovery).GetAwaiter().GetResult(), "Delete rejects files outside configured root");
        var otherType = Path.Combine(folder, "file.txt"); File.WriteAllText(otherType, "keep");
        Throws<InvalidOperationException>(() => source.DeleteAsync(NoteStore.Read(otherType), recovery).GetAwaiter().GetResult(), "Delete rejects non-Markdown files");
        File.AppendAllText(path, "changed");
        Throws<ConflictException>(() => source.DeleteAsync(expected, recovery).GetAwaiter().GetResult(), "Delete rejects changed snapshot");
        Check(File.ReadAllText(path).EndsWith("changed"), "Delete conflict preserves external edits");
        File.WriteAllBytes(path, bytes);
        var blocked = Path.Combine(root, "blocked-recovery"); File.WriteAllText(blocked, "not a directory");
        Throws<IOException>(() => source.DeleteAsync(expected, blocked).GetAwaiter().GetResult(), "Delete stops when recovery cannot be created");
        Check(File.ReadAllBytes(path).SequenceEqual(bytes), "Recovery failure preserves original bytes");
        source.DeleteAsync(expected, recovery).GetAwaiter().GetResult();
        var archived = Directory.GetFiles(recovery, "*.md", SearchOption.AllDirectories).Single();
        Check(!File.Exists(path) && File.ReadAllBytes(archived).SequenceEqual(bytes), "Delete retains exact bytes including BOM in recovery");
        using (var record = JsonDocument.Parse(File.ReadAllText(Path.Combine(Path.GetDirectoryName(archived)!, "source.json"))))
            Check(record.RootElement.GetProperty("Path").GetString() == path, "Delete recovery records original path");

        File.WriteAllBytes(path, bytes);
        var confirmed = false; var calls = 0;
        var pending = new TaskCompletionSource();
        var browser = new NoteBrowserWindow(source, _ => throw new Exception("must not open"), async snapshot =>
        {
            calls++; await pending.Task; await source.DeleteAsync(snapshot, recovery);
        }, selected => { Check(selected == path, "Delete confirmation identifies full path"); return confirmed; });
        try
        {
            var load = browser.InitializeAsync(); WaitFor(() => load.IsCompleted && browser.Delete.IsEnabled, "Delete action enabled after preview");
            browser.DeleteSelectedAsync().GetAwaiter().GetResult();
            Check(calls == 0 && File.Exists(path) && browser.Delete.IsEnabled, "Cancelled deletion leaves selection and file intact");
            confirmed = true;
            var deleting = browser.DeleteSelectedAsync();
            browser.DeleteSelectedAsync().GetAwaiter().GetResult(); browser.OpenSelected(); browser.Close();
            Check(calls == 1 && !browser.Folder.IsEnabled && !browser.Open.IsEnabled, "Delete blocks duplicate clicks, open, folder changes and close");
            pending.SetResult(); WaitFor(() => deleting.IsCompleted, "Delete browser operation finishes"); deleting.GetAwaiter().GetResult();
            Check(browser.Notes.Items.Count == 0 && !browser.Delete.IsEnabled && browser.Folder.IsEnabled, "Delete refreshes list and disables actions on empty selection");
        }
        finally { pending.TrySetResult(); browser.Close(); }

        File.WriteAllBytes(path, bytes);
        browser = new NoteBrowserWindow(source, _ => { }, _ => throw new IOException("delete unavailable"), _ => true);
        try
        {
            var load = browser.InitializeAsync(); WaitFor(() => load.IsCompleted && browser.Delete.IsEnabled, "Delete error fixture loads");
            browser.DeleteSelectedAsync().GetAwaiter().GetResult();
            Check(browser.Status.Text == "delete unavailable" && !browser.Delete.IsEnabled && !browser.Open.IsEnabled && File.Exists(path),
                "Delete error remains visible and invalidates actions until refresh");
            load = browser.LoadFolderAsync(); WaitFor(() => load.IsCompleted && browser.Delete.IsEnabled, "Delete error can be recovered by refresh");
        }
        finally { browser.Close(); }

        var app = App.Current; var previous = app.Config;
        app.ApplySettings(settings, false);
        var note = new NoteWindow(new() { Path = path }); var section = new NoteWindow(new() { Path = path, Heading = "Section" });
        app.Notes.Add(note); app.Notes.Add(section);
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        try
        {
            typeof(NoteWindow).GetField("queryTaskSaving", flags)!.SetValue(note, true);
            Throws<InvalidOperationException>(() => app.DeleteNoteAsync(source, expected).GetAwaiter().GetResult(), "Delete waits for pending query checkbox writes");
            typeof(NoteWindow).GetField("queryTaskSaving", flags)!.SetValue(note, false);
            typeof(NoteWindow).GetMethod("Reload", flags)!.Invoke(section, null);
            typeof(NoteWindow).GetMethod("BeginEdit", flags)!.Invoke(section, null);
            var editor = (TextBox)typeof(NoteWindow).GetField("editor", flags)!.GetValue(section)!;
            editor.Text = "unsaved draft";
            Throws<InvalidOperationException>(() => app.DeleteNoteAsync(source, expected).GetAwaiter().GetResult(), "Delete protects unsaved edits in another view");
            Check(File.Exists(path) && editor.Text == "unsaved draft", "Delete rejection keeps file and draft");
            Check(section.SaveEditorAsync().GetAwaiter().GetResult(), "Delete fixture saves draft");
            app.Config.RecentlyClosedWindows.Add(new() { Path = path });
            app.Config.RecentlyClosedWindows.Add(new() { Path = outside });
            var deletion = app.DeleteNoteAsync(source, NoteStore.Read(path));
            WaitFor(() => deletion.IsCompleted, "Delete app operation completes"); deletion.GetAwaiter().GetResult();
            Check(!File.Exists(path) && !app.Notes.Contains(note) && !app.Notes.Contains(section), "Delete closes every view of the file");
            var persisted = JsonSerializer.Deserialize<Settings>(File.ReadAllText(Path.Combine(App.DataDirectory, "settings.json")))!;
            Check(persisted.Windows.All(p => p.Path != path) && persisted.RecentlyClosedWindows.All(p => p.Path != path) &&
                persisted.RecentlyClosedWindows.Any(p => p.Path == outside), "Delete persists cleanup while preserving unrelated closed history");
        }
        finally { note.Close(); section.Close(); app.Notes.Remove(note); app.Notes.Remove(section); app.ApplySettings(previous, false); }

        File.WriteAllBytes(path, bytes);
        var cliCalls = 0; string? identity = null;
        var cli = new NoteSource(new Settings { ObsidianVaultFolder = folder }, true, (_, code, _) =>
        {
            var request = CliRequest(code);
            Check(request.GetProperty("mode").GetString() == "delete" && request.GetProperty("path").GetString() == "日本語.md", "Delete CLI uses relative path");
            if (cliCalls++ == 0) { identity = request.GetProperty("operation").GetString(); throw new TimeoutException("lost response"); }
            Check(identity == request.GetProperty("operation").GetString(), "Delete CLI retry retains operation identity");
            return Task.FromResult("STICKY_TASKS_PREVIEW:{}");
        });
        var cliSnapshot = expected with { Hash = "cli:" + new string('a', 64) };
        cli.DeleteAsync(cliSnapshot, recovery).GetAwaiter().GetResult();
        Check(cliCalls == 2 && File.Exists(path), "Delete CLI never falls back to local removal");
        cli = new NoteSource(new Settings { ObsidianVaultFolder = folder }, true, (_, _, _) => throw new IOException("offline"));
        Throws<IOException>(() => cli.DeleteAsync(cliSnapshot, recovery).GetAwaiter().GetResult(), "Delete CLI failure reported");
        Check(File.ReadAllBytes(path).SequenceEqual(bytes), "Delete CLI failure preserves local file");
    }
}
