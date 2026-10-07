using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using StickyNotes;
using StickyNotes.Core;

internal static partial class Program
{
    private static void StickyFolderTests(string root)
    {
        foreach (var invalid in new[] { "../outside", "C:/outside", "/outside", ".obsidian", "a//b", "CON", "a/b.", "a:b" })
            Throws<InvalidOperationException>(() => StickyFolderPath.Normalize(invalid), "Sticky folder: rejects unsafe relative path " + invalid);
        Check(StickyFolderPath.Normalize("日本語\\付箋") == "日本語/付箋", "Sticky folder: Unicode nested folder names normalized");
        Task.Run(async () =>
        {
            var basis = Path.Combine(root, "sticky-folders"); Directory.CreateDirectory(basis);
            var original = new Settings { ObsidianTasksEnabled = false, NotesRoot = basis, NotesFolder = basis };
            var first = NoteStore.Create(basis);
            var nested = Path.Combine(basis, "nested"); Directory.CreateDirectory(nested);
            var second = NoteStore.Create(nested);
            var ordinary = Path.Combine(basis, "ordinary.md"); File.WriteAllText(ordinary, "# Ordinary");
            var daily = Path.Combine(basis, "2026-10-07.md"); File.WriteAllText(daily, "## Daily");
            var bytes = File.ReadAllBytes(first);
            var target = Path.Combine(basis, "付箋");
            var next = new Settings { ObsidianTasksEnabled = false, NotesRoot = basis, NotesFolder = target };
            var journals = Path.Combine(root, "folder-journal");
            var access = new NoteSource(original, false);
            var outside = new Settings { ObsidianTasksEnabled = false, NotesRoot = basis, NotesFolder = Path.Combine(root, "outside-base") };
            try { await StickyFolderChange.ApplyAsync(original, outside, false, NoteSource.Create, _ => throw new Exception("must not commit"), journals); throw new Exception("Expected boundary rejection"); }
            catch (InvalidOperationException) { }
            Check(!Directory.Exists(outside.NotesFolder), "Sticky folder: destination cannot escape selected base even without migration");
            Check((await access.GetFoldersAsync()).Folders.Contains("nested"), "Sticky folder: local folder list retrieved under selected base");
            var committed = false;
            await StickyFolderChange.ApplyAsync(original, next, true, NoteSource.Create, paths =>
            {
                Check(paths.Count == 2 && paths.ContainsKey(first) && paths.ContainsKey(second), "Sticky folder: migration includes closed and nested app-created notes only");
                committed = true;
            }, journals);
            var moved = Path.Combine(target, Path.GetFileName(first));
            Check(committed && File.Exists(moved) && !File.Exists(first) && File.ReadAllBytes(moved).SequenceEqual(bytes), "Sticky folder: root-to-child migration preserves exact contents");
            Check(File.Exists(ordinary) && File.Exists(daily), "Sticky folder: ordinary and daily notes remain untouched");
            Check(!Directory.GetFiles(journals).Any(), "Sticky folder: committed migration removes recovery journal");
            var other = new Settings { ObsidianTasksEnabled = false, NotesRoot = basis, NotesFolder = Path.Combine(basis, "new-only") };
            await StickyFolderChange.ApplyAsync(next, other, false, NoteSource.Create, _ => { }, journals);
            Check(Directory.Exists(other.NotesFolder) && File.Exists(moved), "Sticky folder: No creates destination without moving existing notes");
            try { await StickyFolderChange.ApplyAsync(next, other, true, NoteSource.Create, _ => throw new IOException("settings failure"), journals); throw new Exception("Expected rollback"); }
            catch (IOException) { }
            Check(File.Exists(moved) && !File.Exists(Path.Combine(other.NotesFolder, Path.GetFileName(first))), "Sticky folder: failed settings commit rolls moves back");
            File.WriteAllText(Path.Combine(other.NotesFolder, Path.GetFileName(first)), "collision");
            try { await StickyFolderChange.ApplyAsync(next, other, true, NoteSource.Create, _ => throw new Exception("must not commit"), journals); throw new Exception("Expected conflict"); }
            catch (IOException) { }
            Check(File.ReadAllText(Path.Combine(other.NotesFolder, Path.GetFileName(first))) == "collision" && File.Exists(moved), "Sticky folder: collision is detected before moving and never overwritten");
            await StickyFolderChange.ApplyAsync(next, original, true, NoteSource.Create, _ => { }, journals);
            Check(File.Exists(first) && File.Exists(second), "Sticky folder: child-to-root migration also works");
        }).GetAwaiter().GetResult();
        StickyFolderWindowTests(root);
        StickyFolderCliTests(root);
        StickyFolderAppTests(root);
    }

    private static void StickyFolderWindowTests(string root)
    {
        var calls = 0; var answer = MessageBoxResult.Cancel;
        var source = new NoteSource(new Settings { ObsidianVaultFolder = root }, true, (_, code, _) =>
        {
            using var request = DecodeCliRequest(code);
            Check(request.RootElement.GetProperty("mode").GetString() == "folders", "Sticky folder UI: only lists folders before confirmation");
            return Task.FromResult("STICKY_TASKS_PREVIEW:{\"folders\":[\".\",\"Notes\"],\"defaultFolder\":\".\"}");
        });
        var window = new StickyFolderWindow(source, null, (path, migrate) =>
        {
            calls++; Check(path == "新規/付箋" && migrate, "Sticky folder UI: typed new path and Yes reach the commit together"); return Task.CompletedTask;
        }, () => answer);
        window.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));
        WaitFor(() => window.Confirm.IsEnabled, "Sticky folder UI: loaded folders enable selection");
        Check(window.Folder.Items.Contains("Notes") && window.Folder.IsEditable, "Sticky folder UI: dropdown supports existing and new folder names");
        var layout = (FrameworkElement)window.Content;
        ((ScrollViewer)layout).Background = System.Windows.Media.Brushes.White;
        layout.Measure(new Size(560, 340)); layout.Arrange(new Rect(0, 0, 560, 340)); layout.UpdateLayout();
        RenderLocalizationPreview(layout, "sticky-folder-dialog");
        window.Folder.Text = "新規/付箋";
        window.Confirm.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
        Check(calls == 0 && !window.Committed, "Sticky folder UI: Cancel performs no create, move or settings write");
        answer = MessageBoxResult.Yes;
        window.Confirm.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
        Check(calls == 1 && window.Committed, "Sticky folder UI: Confirm commits and closes after migration choice");
        var no = new StickyFolderWindow(source, "Notes", (path, migrate) =>
        {
            Check(path == "Notes" && !migrate, "Sticky folder UI: No confirms existing folder without migration");
            return Task.CompletedTask;
        }, () => MessageBoxResult.No);
        no.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));
        WaitFor(() => no.Confirm.IsEnabled, "Sticky folder UI: existing selection restored");
        no.Confirm.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
        Check(no.Committed, "Sticky folder UI: No saves the selected destination");
    }

    private static void StickyFolderCliTests(string root)
    {
        var settings = new Settings { ObsidianVaultFolder = root, ObsidianNotesFolder = "付箋" };
        var calls = new List<string>();
        var source = new NoteSource(settings, true, (_, code, _) =>
        {
            using var request = DecodeCliRequest(code);
            var q = request.RootElement;
            calls.Add(q.GetProperty("mode").GetString()!);
            if (calls[^1] == "create")
            {
                Check(q.GetProperty("folder").GetString() == "付箋", "Sticky folder CLI: explicit selection is sent for note creation");
                var text = q.GetProperty("text").GetString()!;
                var path = "付箋/" + q.GetProperty("name").GetString();
                return Task.FromResult(CliSnapshotEnvelope(text).Replace("{\"text\"", "{\"path\":" + JsonSerializer.Serialize(path) + ",\"text\""));
            }
            return Task.FromResult("STICKY_TASKS_PREVIEW:{}");
        });
        Check(source.CreateNote("unused invalid local folder").StartsWith(Path.Combine(root, "付箋")), "Sticky folder CLI: new-note path comes from selected vault folder without local access");
        Task.Run(async () =>
        {
            await source.EnsureFolderAsync(Path.Combine(root, "付箋"));
            await source.MoveStickyAsync(Path.Combine(root, "old.md"), Path.Combine(root, "付箋", "old.md"), "cli:" + new string('a', 64));
        }).GetAwaiter().GetResult();
        Check(calls.SequenceEqual(new[] { "create", "ensure", "move" }), "Sticky folder CLI: folder creation and migration both use CLI");
        var disconnected = new NoteSource(settings, true, (_, _, _) => throw new IOException("disconnected"));
        Throws<IOException>(() => disconnected.GetFoldersAsync().GetAwaiter().GetResult(), "Sticky folder CLI: listing failure never falls back to local traversal");
    }

    private static void StickyFolderAppTests(string root)
    {
        var app = App.Current; var previous = app.Config;
        var basis = Path.Combine(root, "folder-app"); Directory.CreateDirectory(basis);
        var from = NoteStore.Create(basis);
        var settings = new Settings { ObsidianTasksEnabled = false, NotesRoot = basis, NotesFolder = basis };
        app.ApplySettings(settings, false);
        var window = new NoteWindow(new NotePlacement { Path = from });
        app.Notes.Add(window);
        var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        try
        {
            typeof(NoteWindow).GetMethod("Reload", flags)!.Invoke(window, null);
            typeof(NoteWindow).GetMethod("BeginEdit", flags)!.Invoke(window, null);
            var editor = (TextBox)typeof(NoteWindow).GetField("editor", flags)!.GetValue(window)!;
            editor.Text = "preserved migration draft";
            var next = new Settings { ObsidianTasksEnabled = false, NotesRoot = basis, NotesFolder = Path.Combine(basis, "Chosen") };
            var change = app.ApplyFolderSettingsAsync(next, true);
            WaitFor(() => change.IsCompleted, "Sticky folder app: migration completes through app commit");
            change.GetAwaiter().GetResult();
            var snapshot = (FileSnapshot)typeof(NoteWindow).GetField("snapshot", flags)!.GetValue(window)!;
            Check(window.Placement.Path == Path.Combine(next.NotesFolder, Path.GetFileName(from)) && snapshot.Path == window.Placement.Path && editor.Text == "preserved migration draft",
                "Sticky folder app: source paths relocate while an unsaved draft is preserved");
            Check((bool)typeof(NoteWindow).GetMethod("Save", flags)!.Invoke(window, null)! && File.ReadAllText(window.Placement.Path).Contains("preserved migration draft"),
                "Sticky folder app: unsaved edit saves to the relocated file");
        }
        finally { window.Close(); app.Notes.Remove(window); app.ApplySettings(previous, false); }
    }
}
