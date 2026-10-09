using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using StickyNotes;
using StickyNotes.Core;

internal static partial class Program
{
    private static void TaskToggleShortcutTests(string root)
    {
        Check(new Settings().TaskToggleShortcut == "" && JsonSerializer.Deserialize<Settings>("{}")!.TaskToggleShortcut == "",
            "Toggle shortcut: new and legacy settings default to unassigned");
        foreach (var invalid in new string?[] { null, "", "garbage", "Ctrl+Bogus", "Ctrl+S", "Ctrl+Z", "Ctrl+Shift+Z", "Alt+F4", "A", "Enter", "Windows+T" })
            Check(TaskToggleShortcut.Parse(invalid) is null, "Toggle shortcut: rejects invalid/reserved binding " + invalid);
        Check(TaskToggleShortcut.Matches("Ctrl+Enter", Key.Enter, ModifierKeys.Control) &&
            !TaskToggleShortcut.Matches("Ctrl+Enter", Key.Enter, ModifierKeys.Control | ModifierKeys.Shift) &&
            TaskToggleShortcut.Matches(TaskToggleShortcut.Format(Key.T, ModifierKeys.Alt | ModifierKeys.Shift), Key.T, ModifierKeys.Alt | ModifierKeys.Shift),
            "Toggle shortcut: custom modifiers match exactly and serialize consistently");
        foreach (var source in new[] { "- [ ] 日本語 😀", "- [X] done", "+ [x] done", "1. [ ] numbered", "> - [ ] quoted", "- [ ] parent\r\n  - [ ] child `code`\r\n" })
        {
            var marker = source.LastIndexOf('[');
            foreach (var caret in new[] { marker - 2, marker + 1, source.TrimEnd('\r', '\n').Length })
            {
                var edit = TaskInput.ToggleCheckbox(source, caret, 0);
                Check(edit is { Length: 1 } && edit.Start == marker + 1 && edit.Text == (source[marker + 1] == ' ' ? "x" : " "),
                    "Toggle shortcut: targets current Markdown task at caret " + caret + " in " + source);
            }
        }
        foreach (var source in new[] { "plain [ ] text", "```tasks\n- [ ] query\n```", "    - [ ] code", "<div>\n- [ ] html\n</div>", "- [-] custom", "- [ ] task\ncontinuation" })
            Check(TaskInput.ToggleCheckbox(source, source.IndexOf('\n') >= 0 && !source.EndsWith("continuation") ? source.IndexOf("[ ]", StringComparison.Ordinal) + 3 : source.Length, 0) is null,
                "Toggle shortcut: leaves non-task source unchanged: " + source);
        Check(TaskInput.ToggleCheckbox("- [ ] task", 7, 1) is null, "Toggle shortcut: preserves text selections");

        var app = App.Current; var previous = app.Config;
        var path = Path.Combine(root, "shortcut-edit.md");
        const string original = "---\r\ntitle: Preserve\r\n---\r\n- [ ] first\r\n- [ ] second 📅 2026-10-10\r\n";
        File.WriteAllText(path, original);
        app.ApplySettings(new Settings { ObsidianTasksEnabled = false, NotesFolder = previous.NotesFolder,
            DailyFolder = root, TaskCompletion = TaskCompletionMode.Off }, false);
        var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        var window = new NoteWindow(new NotePlacement { Path = path });
        var editor = (TextBox)typeof(NoteWindow).GetField("editor", flags)!.GetValue(window)!;
        var behavior = (TaskEditorBehavior)typeof(NoteWindow).GetField("taskEditor", flags)!.GetValue(window)!;
        try
        {
            window.Show(); window.Activate();
            typeof(NoteWindow).GetMethod("BeginEdit", flags)!.Invoke(window, null);
            WaitFor(() => editor.IsKeyboardFocusWithin, "Toggle shortcut: note editor focused");
            editor.CaretIndex = editor.Text.IndexOf("second", StringComparison.Ordinal) + 3;
            var before = editor.Text; var caret = editor.CaretIndex;
            Check(!window.HandleEditingKey(Key.Enter, ModifierKeys.Control) && editor.Text == before,
                "Toggle shortcut: unassigned binding does nothing");
            app.Config.TaskToggleShortcut = "Ctrl+Enter";
            behavior.SetComposing(true);
            Check(!window.HandleEditingKey(Key.Enter, ModifierKeys.Control) && editor.Text == before,
                "Toggle shortcut: IME composition prevents changes");
            behavior.SetComposing(false);
            Check(window.HandleEditingKey(Key.Enter, ModifierKeys.Control) && editor.Text.Contains("- [x] second") &&
                editor.Text.StartsWith("- [ ] first") && editor.CaretIndex == caret && File.ReadAllText(path) == original,
                "Toggle shortcut: completion OFF still toggles only current draft line, preserving caret and file");
            editor.Undo(); Check(editor.Text == before, "Toggle shortcut: one undo restores the draft");
            editor.Redo(); Check(editor.Text.Contains("- [x] second"), "Toggle shortcut: redo restores checked state");
            Check(window.HandleEditingKey(Key.Enter, ModifierKeys.Control) && editor.Text == before,
                "Toggle shortcut: second press unchecks");
            editor.Select(caret, 2);
            Check(!window.HandleEditingKey(Key.Enter, ModifierKeys.Control) && editor.SelectionLength == 2 && editor.Text == before,
                "Toggle shortcut: selection is not overwritten");
            editor.Select(caret, 0);
            app.Config.TaskToggleShortcut = "Alt+T";
            Check(!window.HandleEditingKey(Key.Enter, ModifierKeys.Control), "Toggle shortcut: settings change takes effect immediately");
            // Direct handler checks modifiers deterministically without synthesizing physical keyboard state.
            Check(window.HandleEditingKey(Key.T, ModifierKeys.Alt), "Toggle shortcut: custom Alt binding toggles");
            window.HandleEditingKey(Key.Escape, ModifierKeys.None);
            Check(File.ReadAllText(path) == original.Replace("- [ ] second", "- [x] second"),
                "Toggle shortcut: normal save retains frontmatter, Unicode, dates and CRLF");
            Check(!window.HandleEditingKey(Key.T, ModifierKeys.Alt), "Toggle shortcut: reading mode cannot toggle");

            var settings = new SettingsWindow(); var panel = SettingsPanel(settings);
            var input = panel.Children.OfType<StackPanel>().SelectMany(p => p.Children.OfType<TextBox>()).Single(t => t.Name == "TaskToggleShortcut");
            Check(input.IsReadOnly && input.Text == "Alt+T", "Toggle shortcut: settings show current binding");
            input.Text = "Ctrl+Shift+K";
            Check(app.Config.TaskToggleShortcut == "Alt+T", "Toggle shortcut: settings changes wait for save");
            panel.Children.OfType<Button>().Single(b => (string)b.Content == L10n.Text("SettingsWindow.Text68"))
                .RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
            Check(app.Config.TaskToggleShortcut == "Ctrl+Shift+K" && JsonSerializer.Deserialize<Settings>(
                File.ReadAllText(Path.Combine(App.DataDirectory, "settings.json")))!.TaskToggleShortcut == "Ctrl+Shift+K",
                "Toggle shortcut: chosen binding persists");
            settings = new SettingsWindow(); panel = SettingsPanel(settings);
            Check(panel.Children.OfType<StackPanel>().SelectMany(p => p.Children.OfType<TextBox>()).Single(t => t.Name == "TaskToggleShortcut").Text == "Ctrl+Shift+K",
                "Toggle shortcut: reopened settings retain binding");
            panel.Children.OfType<Button>().Single(b => (string)b.Content == L10n.Text("TaskEditing.ShortcutClear"))
                .RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
            panel.Children.OfType<Button>().Single(b => (string)b.Content == L10n.Text("SettingsWindow.Text68"))
                .RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
            Check(app.Config.TaskToggleShortcut == "", "Toggle shortcut: clear and save disables binding");
        }
        finally { window.Close(); app.ApplySettings(previous, false); }
    }
}
