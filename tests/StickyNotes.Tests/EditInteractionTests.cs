using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using StickyNotes;
using StickyNotes.Core;

internal static partial class Program
{
    private static IEnumerable<Run> DocumentRuns(FlowDocument document)
    {
        for (var pointer = document.ContentStart; pointer is not null; pointer = pointer.GetNextContextPosition(LogicalDirection.Forward))
            if (pointer.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.ElementStart &&
                pointer.GetAdjacentElement(LogicalDirection.Forward) is Run run) yield return run;
    }

    private static void SourcePositionTests()
    {
        const string source = "# Heading 日本語\r\n\r\nPlain **bold text** and `inline code`.\r\n\r\n- [ ] task text\r\n\r\n> quote text\r\n\r\n| Column |\r\n| --- |\r\n| Cell text |\r\n\r\n```text\r\nalpha\r\n  beta\r\n```\r\n\r\nrepeat\r\n\r\nrepeat\r\n";
        var document = MarkdownView.Render(source, (_, _) => { });
        var runs = DocumentRuns(document).ToArray();
        foreach (var value in new[] { "Heading 日本語", "bold text", "inline code", "task text", "quote text", "Cell text" })
        {
            var run = runs.Single(r => r.Text.Contains(value));
            var offset = run.Text.IndexOf(value, StringComparison.Ordinal) + 3;
            Check(MarkdownSourceMap.Position(run.ContentStart.GetPositionAtOffset(offset)) == source.IndexOf(value, StringComparison.Ordinal) + 3,
                "Click position: Markdown source offset for " + value);
        }
        var code = runs.Single(r => r.Text.Contains("alpha"));
        Check(MarkdownSourceMap.Position(code.ContentStart.GetPositionAtOffset(code.Text.IndexOf("beta", StringComparison.Ordinal) + 2)) == source.IndexOf("beta", StringComparison.Ordinal) + 2,
            "Click position: code block maps normalized newlines to original CRLF");
        var repeated = runs.Where(r => r.Text == "repeat").ToArray();
        Check(repeated.Length == 2 && MarkdownSourceMap.Position(repeated[1].ContentStart.GetPositionAtOffset(4)) == source.LastIndexOf("repeat", StringComparison.Ordinal) + 4,
            "Click position: repeated text maps to the clicked occurrence");
        const string backticks = "Code: `` `inside` ``.";
        var backtickRun = DocumentRuns(MarkdownView.Render(backticks, (_, _) => { })).Single(r => r.Text == "`inside`");
        Check(MarkdownSourceMap.Position(backtickRun.ContentStart) == backticks.IndexOf("`inside`", StringComparison.Ordinal),
            "Click position: inline code content does not map to its opening delimiters");
        var generated = MarkdownView.Render("before\n\n```tasks\nnot done\n```\n", (_, _) => { }, _ =>
        {
            var section = new Section();
            var result = MarkdownView.Render("generated result", (_, _) => { }, readOnly: true);
            var child = result.Blocks.FirstBlock; result.Blocks.Remove(child); section.Blocks.Add(child);
            return section;
        });
        var generatedRun = DocumentRuns(generated).Single(r => r.Text == "generated result");
        Check(MarkdownSourceMap.Position(generatedRun.ContentStart.GetPositionAtOffset(8)) == 8,
            "Click position: generated Tasks output maps to the source query fence");
    }

    private static void EditInteractionTests(string root)
    {
        MenuSaveTests(root);
        SourcePositionTests();
        Check(!new Settings().DoubleClickToEdit && !JsonSerializer.Deserialize<Settings>("{}")!.DoubleClickToEdit,
            "Edit gesture: existing and new settings default to single click");
        var app = App.Current; var previous = app.Config;
        app.ApplySettings(new Settings { ObsidianTasksEnabled = false, NotesFolder = previous.NotesFolder, DailyFolder = root }, false);
        var path = Path.Combine(root, "edit-interactions.md");
        const string original = "---\r\ntitle: Keep metadata\r\n---\r\n# Title\r\n\r\nFirst **click here** then edit.\r\n";
        File.WriteAllText(path, original);
        var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        var errors = 0; var confirmations = 0;
        var window = new NoteWindow(new NotePlacement { Path = path }, () => { confirmations++; return MessageBoxResult.Cancel; }, _ => errors++);
        var editor = (TextBox)typeof(NoteWindow).GetField("editor", flags)!.GetValue(window)!;
        var preview = (FlowDocumentScrollViewer)typeof(NoteWindow).GetField("preview", flags)!.GetValue(window)!;
        void Reload() => typeof(NoteWindow).GetMethod("Reload", flags)!.Invoke(window, null);
        TextPointer ClickPosition() => DocumentRuns(preview.Document).Single(r => r.Text == "click here").ContentStart.GetPositionAtOffset(6)!;
        try
        {
            window.Show(); window.Activate(); window.UpdateLayout();
            var expected = NoteStore.Split(original).Body.IndexOf("click here", StringComparison.Ordinal) + 6;
            var position = ClickPosition();
            preview.Selection.Select(position, position);
            preview.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left)
                { RoutedEvent = Mouse.MouseDownEvent, Handled = true });
            Check(editor.IsVisible && editor.CaretIndex == expected && editor.SelectionLength == 0,
                "Edit gesture: handled native text click opens editor at the source character");
            Check(window.HandleEditingKey(Key.Escape, ModifierKeys.None) && !editor.IsVisible && File.ReadAllText(path) == original && confirmations == 0,
                "Escape: unchanged editing ends without a write or confirmation");

            var settings = new SettingsWindow();
            var panel = SettingsPanel(settings);
            var choice = panel.Children.OfType<ComboBox>().Single(c => c.Name == "EditGesture");
            choice.SelectedIndex = 1;
            var taskCompletionChoice = panel.Children.OfType<ComboBox>().Single(c => c.Name == "TaskCompletion");
            Check(taskCompletionChoice.SelectedIndex == 0, "Task completion settings: default selection is automatic");
            taskCompletionChoice.SelectedIndex = 1;
            Check(!app.Config.DoubleClickToEdit, "Edit gesture: setting is applied only after saving");
            panel.Children.OfType<Button>().Single(b => (string)b.Content == "保存して閉じる").RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
            Check(app.Config.DoubleClickToEdit && JsonSerializer.Deserialize<Settings>(File.ReadAllText(Path.Combine(App.DataDirectory, "settings.json")))!.DoubleClickToEdit,
                "Edit gesture: double click preference persists");
            Check(app.Config.TaskCompletion == TaskCompletionMode.On && JsonSerializer.Deserialize<Settings>(File.ReadAllText(Path.Combine(App.DataDirectory, "settings.json")))!.TaskCompletion == TaskCompletionMode.On,
                "Task completion settings: manual ON is saved");
            settings = new SettingsWindow();
            Check(SettingsPanel(settings).Children.OfType<ComboBox>().Single(c => c.Name == "EditGesture").SelectedIndex == 1,
                "Edit gesture: reopened settings retain double click preference");
            Check(SettingsPanel(settings).Children.OfType<ComboBox>().Single(c => c.Name == "TaskCompletion").SelectedIndex == 1,
                "Task completion settings: reopened settings retain manual ON");
            settings.Close();
            position = ClickPosition();
            Check(!window.HandleBodyClick(1, preview, position) && !editor.IsVisible,
                "Edit gesture: single click stays in reading mode when double click is selected");
            Check(window.HandleBodyClick(2, preview, position.Paragraph!.ContentStart) && editor.CaretIndex == expected,
                "Edit gesture: second click preserves the first click's exact character instead of word selection");
            editor.SelectedText = "追加";
            Check(!window.HandleEditingKey(Key.Escape, ModifierKeys.Control) && editor.IsVisible, "Escape: modified shortcut does not save");
            Check(!window.HandleEditingKey(Key.ImeProcessed, ModifierKeys.None) && editor.IsVisible,
                "Escape: IME-owned key is left to the input method");
            Check(window.HandleEditingKey(Key.Escape, ModifierKeys.None) && !editor.IsVisible && File.ReadAllText(path).Contains("click 追加here") &&
                File.ReadAllText(path).StartsWith("---\r\ntitle: Keep metadata\r\n---\r\n") && confirmations == 0,
                "Escape: saves the draft, preserves metadata and exits without confirmation");

            typeof(NoteWindow).GetMethod("BeginEdit", flags)!.Invoke(window, null);
            editor.Text = "conflict draft"; File.WriteAllText(path, "external update");
            window.HandleEditingKey(Key.Escape, ModifierKeys.None);
            Check(errors == 1 && editor.IsVisible && editor.Text == "conflict draft" && File.ReadAllText(path) == "external update",
                "Escape: conflict preserves both draft and external update");
            Reload();
            typeof(NoteWindow).GetMethod("BeginEdit", flags)!.Invoke(window, null);
            WaitFor(() => editor.IsKeyboardFocusWithin, "Escape: editor receives focus for completion checks");
            editor.Text = "@"; editor.CaretIndex = 1;
            var completion = (CalendarCompletion)typeof(NoteWindow).GetField("calendarCompletion", flags)!.GetValue(window)!;
            WaitFor(() => completion.IsOpen, "Escape: calendar suggestion opens");
            window.HandleEditingKey(Key.Escape, ModifierKeys.None);
            Check(!completion.IsOpen && editor.IsVisible && File.ReadAllText(path) == "external update",
                "Escape: dismisses completion before finishing editing");
            var escape = new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(window), Environment.TickCount, Key.Escape)
                { RoutedEvent = Keyboard.PreviewKeyDownEvent };
            editor.RaiseEvent(escape);
            Check(!editor.IsVisible && File.ReadAllText(path) == "@", "Escape: next press saves after dismissing completion");
            Check(escape.Handled, "Escape: routed keyboard event is handled by the note");

            var longBody = string.Join("\r\n\r\n", Enumerable.Range(0, 90).Select(i => "Paragraph " + i + " 日本語 😀"));
            File.WriteAllText(path, longBody); Reload(); preview.Zoom = 175;
            window.UpdateLayout();
            var last = DocumentRuns(preview.Document).Last();
            app.Config.DoubleClickToEdit = false;
            window.HandleBodyClick(1, preview, last.ContentStart.GetPositionAtOffset(12));
            Check(editor.CaretIndex == longBody.LastIndexOf("Paragraph", StringComparison.Ordinal) + 12,
                "Click position: enlarged long preview opens at source character (caret " + editor.CaretIndex + ")");
            WaitFor(() => editor.VerticalOffset > 0, "Click position: editor scrolls to clicked source line");
            window.HandleEditingKey(Key.Escape, ModifierKeys.None);
        }
        finally { Reload(); window.Close(); app.ApplySettings(previous, false); }
    }

    private static void MenuSaveTests(string root)
    {
        var previousContext = System.Threading.SynchronizationContext.Current;
        System.Threading.SynchronizationContext.SetSynchronizationContext(new System.Windows.Threading.DispatcherSynchronizationContext());
        var app = App.Current;
        var previousFactory = app.NoteSources;
        var cli = app.Config.ObsidianTasksEnabled;
        var autoSave = app.Config.AutoSaveOnFocusLoss;
        var vault = app.Config.ObsidianVaultFolder;
        var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        var calls = 0; var errors = 0; var confirmations = 0;
        TaskCompletionSource<string>? response = null;
        NoteWindow? window = null;
        try
        {
            app.Config.ObsidianTasksEnabled = true;
            app.Config.AutoSaveOnFocusLoss = true;
            app.Config.ObsidianVaultFolder = root;
            app.NoteSources = config => new NoteSource(config, true, (_, _, _) =>
            {
                if (response is null) return Task.FromResult(CliSnapshotEnvelope("original"));
                System.Threading.Interlocked.Increment(ref calls);
                return response.Task;
            });
            window = new NoteWindow(new NotePlacement { Path = Path.Combine(root, "menu-save.md") },
                () => { confirmations++; return MessageBoxResult.Cancel; }, _ => errors++);
            app.Notes.Add(window);
            window.Show(); window.Activate();
            var editor = (TextBox)typeof(NoteWindow).GetField("editor", flags)!.GetValue(window)!;
            var menuButton = window.NoteControls.Children.OfType<Button>().Single(b => Equals(b.Content, "…"));
            void Edit(string text)
            {
                typeof(NoteWindow).GetMethod("BeginEdit", flags)!.Invoke(window, null);
                editor.Text = text;
                WaitFor(() => editor.IsKeyboardFocusWithin, "menu save: editor receives focus");
            }
            void OpenMenu()
            {
                menuButton.Focus();
                menuButton.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
                Check(window.ToolbarMenu?.IsOpen == true, "menu save: toolbar menu opens");
            }
            Edit("draft");
            response = new(TaskCreationOptions.RunContinuationsAsynchronously);
            OpenMenu();
            var elapsed = System.Diagnostics.Stopwatch.StartNew();
            WaitFor(() => elapsed.ElapsedMilliseconds >= 300, "menu save: dispatcher runs past the blur delay");
            window.FinishFocusEditing();
            Check(calls == 0 && !window.IsSaving && editor.Text == "draft", "menu save: open toolbar menu defers autosave and retains draft");
            app.Config.AutoSaveOnFocusLoss = false;
            window.FinishFocusEditing();
            Check(confirmations == 0, "menu save: open toolbar menu also defers confirmation");
            app.Config.AutoSaveOnFocusLoss = true;
            window.ToolbarMenu!.IsOpen = false;
            window.FinishFocusEditing();
            WaitFor(() => calls == 1, "menu save: deferred save starts after menu closes");
            Check(window.IsSaving && editor.IsReadOnly && !window.CanClose(), "menu save: pending write guards editing and close");
            typeof(NoteWindow).GetMethod("ReloadAsked", flags)!.Invoke(window, null);
            Check(editor.Text == "draft" && window.IsSaving, "menu save: reload cannot discard an in-flight draft");
            Check(window.SaveEditorAsync().IsCompletedSuccessfully && calls == 1, "menu save: repeated save does not submit another write");
            Throws<InvalidOperationException>(() => app.ApplySettings(app.Config, false), "menu save: pending write blocks source changes");
            var folderChange = app.ApplyFolderSettingsAsync(app.Config, false);
            Check(folderChange.IsFaulted && folderChange.Exception!.GetBaseException() is InvalidOperationException,
                "menu save: pending write blocks folder migration");
            OpenMenu();
            elapsed.Restart();
            WaitFor(() => elapsed.ElapsedMilliseconds >= 300, "menu save: dispatcher remains responsive during delayed CLI save");
            Check(window.ToolbarMenu?.IsOpen == true && window.IsSaving, "menu save: menu remains usable while CLI save waits");
            response.SetResult(CliSnapshotEnvelope("draft"));
            WaitFor(() => !window.IsSaving, "menu save: successful write completes");
            Check(!editor.IsReadOnly && editor.Visibility == Visibility.Collapsed && errors == 0,
                "menu save: success ends editing and releases save guard");
            window.ToolbarMenu!.IsOpen = false;
            Edit("timeout draft");
            response = new(TaskCreationOptions.RunContinuationsAsynchronously);
            var failedSave = window.SaveEditorAsync();
            response.SetException(new TaskCanceledException("simulated CLI timeout"));
            WaitFor(() => failedSave.IsCompleted, "menu save: timeout completes without blocking dispatcher");
            Check(!failedSave.Result && errors == 1 && editor.Text == "timeout draft" && !editor.IsReadOnly && editor.IsVisible,
                "menu save: timeout preserves editable draft and reports once");
            response = new(TaskCreationOptions.RunContinuationsAsynchronously);
            var retry = window.SaveEditorAsync();
            response.SetResult(CliSnapshotEnvelope("timeout draft"));
            WaitFor(() => retry.IsCompleted, "menu save: retained draft can be retried");
            Check(retry.Result, "menu save: retry saves successfully");
            Edit("conflicting draft");
            response = new(TaskCreationOptions.RunContinuationsAsynchronously);
            var conflict = window.SaveEditorAsync();
            response.SetException(new ConflictException("simulated external edit"));
            WaitFor(() => conflict.IsCompleted, "menu save: conflict returns control to editor");
            var retained = (FileSnapshot)typeof(NoteWindow).GetField("snapshot", flags)!.GetValue(window)!;
            Check(!conflict.Result && errors == 2 && editor.Text == "conflicting draft" && retained.Text == "timeout draft" &&
                !File.Exists(window.Placement.Path), "menu save: conflict preserves draft and prior snapshot without disk fallback");
            editor.Text = "@"; editor.CaretIndex = 1;
            var calendar = (CalendarCompletion)typeof(NoteWindow).GetField("calendarCompletion", flags)!.GetValue(window)!;
            calendar.Update();
            Check(calendar.IsOpen, "menu save: calendar completion opens before saving");
            response = new(TaskCreationOptions.RunContinuationsAsynchronously);
            var calendarSave = window.SaveEditorAsync();
            calendar.Update(); calendar.Complete();
            Check(!calendar.IsOpen && editor.Text == "@", "menu save: calendar completion cannot change a pending draft");
            response.SetResult(CliSnapshotEnvelope("@"));
            WaitFor(() => calendarSave.IsCompleted, "menu save: calendar draft save finishes");
            Edit("- [ ] locked");
            var tasks = (TaskEditorBehavior)typeof(NoteWindow).GetField("taskEditor", flags)!.GetValue(window)!;
            response = new(TaskCreationOptions.RunContinuationsAsynchronously);
            var taskSave = window.SaveEditorAsync();
            Check(!tasks.ToggleCheckbox() && !tasks.HandleKey(Key.Enter, ModifierKeys.None) && editor.Text == "- [ ] locked",
                "menu save: task completion and toggle cannot change a pending draft");
            response.SetResult(CliSnapshotEnvelope("- [ ] locked"));
            WaitFor(() => taskSave.IsCompleted, "menu save: task draft save finishes");
        }
        finally
        {
            response?.TrySetException(new OperationCanceledException("test cleanup"));
            if (window is not null)
            {
                if (window.ToolbarMenu is { } menu) menu.IsOpen = false;
                typeof(NoteWindow).GetField("dirty", flags)!.SetValue(window, false);
                app.Notes.Remove(window); window.Close();
            }
            app.NoteSources = previousFactory;
            app.Config.ObsidianTasksEnabled = cli; app.Config.AutoSaveOnFocusLoss = autoSave; app.Config.ObsidianVaultFolder = vault;
            System.Threading.SynchronizationContext.SetSynchronizationContext(previousContext);
        }
    }
}
