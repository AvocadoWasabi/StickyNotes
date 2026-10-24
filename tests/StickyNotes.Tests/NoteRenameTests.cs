using System.IO;
using System.Text.Json;
using System.Windows.Controls;
using StickyNotes;
using StickyNotes.Core;

internal static partial class Program
{
    private static void NoteRenameTests(string root)
    {
        var folder = Path.Combine(root, "rename-tests"); Directory.CreateDirectory(folder);
        var path = Path.Combine(folder, "original.md"); File.WriteAllText(path, "---\ntitle: Display title\ncolor: pink\n---\n## Section\nBody\n");
        foreach (var name in new[] { "", " ", "../escape", "a/b", "a\\b", "a:b", "NUL", "COM1", "LPT¹", "CON.txt", "name.", "name ", ".hidden" })
            Throws<InvalidOperationException>(() => NoteRename.Target(path, name), "Rename rejects unsafe name: " + name);
        var target = NoteRename.Target(path, "新しい名前");
        Check(target == Path.Combine(folder, "新しい名前.md"), "Rename preserves directory and Markdown extension");
        var settings = new Settings { ObsidianTasksEnabled = false, NotesRoot = folder, NotesFolder = folder };
        var source = new NoteSource(settings, false);
        var journal = Path.Combine(folder, "journal");
        var expected = NoteStore.Read(path);
        File.WriteAllText(target, "collision");
        Throws<IOException>(() => NoteRename.ApplyAsync(source, expected, target, _ => throw new Exception("must not commit"), journal).GetAwaiter().GetResult(), "Rename rejects duplicate without commit");
        Check(File.ReadAllText(target) == "collision" && File.Exists(path), "Rename leaves both files intact on collision");
        File.Delete(target);
        File.AppendAllText(path, "external edit");
        Throws<IOException>(() => NoteRename.ApplyAsync(source, expected, target, _ => throw new Exception("must not commit"), journal).GetAwaiter().GetResult(), "Rename rejects stale source hash");
        Check(File.Exists(path) && !File.Exists(target), "Rename conflict preserves external edit");

        var app = App.Current; var previous = app.Config;
        app.ApplySettings(settings, false);
        var note = new NoteWindow(new NotePlacement { Path = path, Color = "pink", ContentScale = 125 });
        var section = new NoteWindow(new NotePlacement { Path = path, Heading = "Section" });
        var daily = new NoteWindow(new NotePlacement { Daily = true });
        app.Notes.Add(note); app.Notes.Add(section);
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        try
        {
            typeof(NoteWindow).GetMethod("Reload", flags)!.Invoke(note, null);
            typeof(NoteWindow).GetMethod("Reload", flags)!.Invoke(section, null);
            Check(note.Title == "original" && section.Title == "original", "Note and linked-section titles use filename instead of YAML or heading");
            Check(note.RenameMenuItem().IsEnabled && section.RenameMenuItem().IsEnabled && !daily.RenameMenuItem().IsEnabled,
                "Rename menu enables regular and heading notes, disables daily notes");
            typeof(NoteWindow).GetMethod("BeginEdit", flags)!.Invoke(section, null);
            var editor = (TextBox)typeof(NoteWindow).GetField("editor", flags)!.GetValue(section)!;
            editor.Text = "unsaved section draft";
            Throws<InvalidOperationException>(() => app.RenameNoteAsync(note, NoteStore.Read(path), target).GetAwaiter().GetResult(), "Rename protects other windows with unsaved drafts");
            Check(File.Exists(path) && editor.Text == "unsaved section draft", "Rename rejected without discarding draft");
            Check(section.SaveEditorAsync().GetAwaiter().GetResult(), "Rename fixture saves section draft");
            app.Config.RecentlyClosedWindows.Add(new NotePlacement { Path = path, Heading = "Section", Width = 777 });
            var before = File.ReadAllBytes(path);
            typeof(NoteWindow).GetMethod("BeginEdit", flags)!.Invoke(note, null);
            var rename = app.RenameNoteAsync(note, NoteStore.Read(path), target);
            WaitFor(() => rename.IsCompleted, "Rename app operation completes"); rename.GetAwaiter().GetResult();
            Check(!File.Exists(path) && File.ReadAllBytes(target).SequenceEqual(before), "Rename preserves exact bytes including display title");
            Check(note.Placement.Path == target && section.Placement.Path == target && note.Placement.Color == "pink" && note.Placement.ContentScale == 125,
                "Rename relocates open windows preserving appearance");
            Check(note.Title == "新しい名前" && section.Title == "新しい名前" &&
                ((TextBlock)typeof(NoteWindow).GetField("title", flags)!.GetValue(note)!).Text == "新しい名前" &&
                (bool)typeof(NoteWindow).GetField("editing", flags)!.GetValue(note)!,
                "Rename updates header and taskbar titles immediately even in a clean editor");
            var undo = note.Relocate(new Dictionary<string, string> { [target] = path });
            undo();
            Check(note.Title == "新しい名前", "Relocation rollback restores filename title");
            var persisted = JsonSerializer.Deserialize<Settings>(File.ReadAllText(Path.Combine(App.DataDirectory, "settings.json")))!;
            Check(persisted.RecentlyClosedWindows.Single(n => n.Path == target).Width == 777,
                "Rename persists closed history with geometry");
            Check(persisted.Windows.Count(n => n.Path == target) == 2, "Rename persists both open window paths");
            var second = NoteRename.Target(target, "after-persistence-failure");
            Throws<IOException>(() => NoteRename.ApplyAsync(source, NoteStore.Read(target), second, _ => throw new IOException("settings failure"), journal).GetAwaiter().GetResult(),
                "Rename reports settings persistence failure");
            Check(File.Exists(second) && Directory.GetFiles(journal).Length > 0, "Rename leaves moved file and recovery record after persistence failure");
        }
        finally
        {
            note.Close(); section.Close(); daily.Close(); app.Notes.Remove(note); app.Notes.Remove(section); app.ApplySettings(previous, false);
        }

        var calls = 0; string? firstRequest = null;
        var cli = new NoteSource(new Settings { ObsidianVaultFolder = folder }, true, (_, code, _) =>
        {
            using var json = DecodeCliRequest(code);
            var request = json.RootElement;
            Check(request.GetProperty("mode").GetString() == "rename" && request.GetProperty("target").GetString() == "新しい名前.md", "Rename CLI request uses relative paths");
            var operation = request.GetProperty("operation").GetString();
            if (calls++ == 0) { firstRequest = operation; throw new TimeoutException("lost response"); }
            Check(operation == firstRequest, "Rename CLI retry retains operation identity");
            return Task.FromResult("STICKY_TASKS_PREVIEW:{}");
        });
        cli.RenameAsync(expected with { Hash = "cli:" + new string('a', 64) }, target, Guid.NewGuid().ToString("N")).GetAwaiter().GetResult();
        Check(calls == 2, "Rename CLI retries lost response without local fallback");
        System.Windows.Threading.Dispatcher.CurrentDispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.ApplicationIdle, new Action(() =>
        {
            var dialog = System.Windows.Application.Current.Windows.OfType<System.Windows.Window>().Single(w => w.Title == "Rename test");
            var panel = (StackPanel)((ScrollViewer)dialog.Content).Content;
            var input = panel.Children.OfType<TextBox>().Single();
            var buttons = panel.Children.OfType<StackPanel>().Single().Children.OfType<Button>().ToArray();
            Check(input.Text == "original" && input.SelectionLength == input.Text.Length && input.IsKeyboardFocused,
                "Rename dialog selects current filename and focuses input");
            Check(buttons[0].IsCancel && buttons[1].IsDefault, "Rename dialog supports Escape and Enter");
            dialog.DialogResult = false;
        }));
        Check(Ui.Prompt("Rename test", [new("name", "File name", "original")], singleLineSubmit: true) is null,
            "Rename dialog cancellation returns no change");
    }
}
