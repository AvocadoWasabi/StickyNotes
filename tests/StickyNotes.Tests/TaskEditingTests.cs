using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using StickyNotes;
using StickyNotes.Core;

internal static partial class Program
{
    private static void TaskEditingTests(string root)
    {
        var today = new DateTime(2026, 12, 31);
        TaskSuggestions Suggest(string text, int? caret = null, int selection = 0) => TaskInput.Suggest(text, caret ?? text.Length, selection, today);
        string Apply(string text, TaskTextEdit edit) => text[..edit.Start] + edit.Text + text[(edit.Start + edit.Length)..];
        const string due = "- [ ] 日本語 😀 du";
        Check(Apply(due, Suggest(due).Items.Single().Edit) == "- [ ] 日本語 😀 📅 ", "Task completion: replaces keyword and preserves Unicode description");
        const string dated = "- [ ] Work 📅 tomorrow";
        Check(Apply(dated, Suggest(dated).Items.Single().Edit).EndsWith("📅 2027-01-01 "), "Task completion: relative date handles year rollover");
        Check(Suggest("- [ ] Work 📅 ").Items.Length == 5 && !Suggest("- [ ] Work 📅 ").SelectFirst,
            "Task completion: date choices have no implicit selection on empty input");
        Check(Suggest("- [ ] Work 🔁 every w").Items.Length == 2 && Suggest("- [ ] Work every w").Items.Single().Edit.Text == "🔁 every week ",
            "Task completion: recurrence with and without existing emoji");
        Check(Suggest("- [ ] Work high").Items[0].Edit.Text == "⏫ " && Suggest("- [ ] Work highest").Items.Single().Edit.Text == "🔺 ",
            "Task completion: exact priority keyword is first");
        Check(Suggest("- [ ] Work 🔁 every week due").Items.Single().Edit.Text == "📅 " && !Suggest("- [ ] Work 🔁 every week ").SelectFirst,
            "Task completion: following fields still complete; finished recurrence does not select when-done automatically");
        Check(Suggest("- [ ] Work 📅 2026-12-31 due").Items.Length == 0 && Suggest("- [ ] Work 🔺 low").Items.Length == 0,
            "Task completion: existing date and priority are not duplicated");
        foreach (var text in new[] { "ordinary due", "- [x] Done due", "```md\n- [ ] due\n```", "    - [ ] due", "<!--\n- [ ] due\n-->", "- [ ] `due`" })
        {
            var caret = text.IndexOf("due", StringComparison.Ordinal) + 3;
            Check(Suggest(text, caret).Items.Length == 0, "Task completion: avoids non-task/completed/code context " + text.Replace('\n', ' '));
        }
        Check(Suggest(due, due.Length, 1).Items.Length == 0 && Suggest("- [ ] dueXYZ", 9).Items.Length == 0,
            "Task completion: selection and middle-of-word positions are untouched");
        const string list = "- [ ] first\r\n  * [x] finished";
        Check(Apply(list, TaskInput.ContinueList(list, list.Length, 0)!) == list + "\r\n  * [ ] ",
            "Task Enter: keeps indentation, bullet and CRLF; resets checked state");
        Check(Apply("12. [ ] abc", TaskInput.ContinueList("12. [ ] abc", 9, 0)!) == "12. [ ] a\n13. [ ] bc",
            "Task Enter: splits text at caret and increments ordered list");
        Check(Apply("- [ ] first\n- [ ] \nnext", TaskInput.ContinueList("- [ ] first\n- [ ] \nnext", 18, 0)!) == "- [ ] first\n\nnext",
            "Task Enter: empty item ends list without consuming next line");
        Check(TaskInput.ContinueList("```\n- [ ] text\n```", 14, 0) is null && TaskInput.ContinueList(due, due.Length, 1) is null,
            "Task Enter: code and selected text retain default behavior");
        TaskEditorWindowTests(today);
        TaskCheckboxTests(root);
        TaskCheckboxWindowTests(root);
        TaskCompletionPolicyTests(root);
    }

    private static void TaskEditorWindowTests(DateTime today)
    {
        var editor = new TextBox { AcceptsReturn = true, AcceptsTab = true };
        var window = new Window { Content = editor, Width = 450, Height = 260 };
        var probes = 0;
        Func<System.Threading.CancellationToken, Task<bool>> probe = _ => Task.FromResult(true);
        var behavior = new TaskEditorBehavior(window, editor, () => today, token => { probes++; return probe(token); });
        void Set(string text) { editor.Text = text; editor.CaretIndex = text.Length; editor.UpdateLayout(); behavior.Update(); }
        try
        {
            window.Show(); window.Activate(); editor.Focus(); window.UpdateLayout();
            WaitFor(() => editor.IsKeyboardFocusWithin, "Task editor: keyboard focus acquired");
            var initialProbes = probes;
            Set("- [ ] Work du");
            Check(behavior.IsOpen && behavior.HandleKey(Key.Enter, ModifierKeys.None) && editor.Text == "- [ ] Work 📅 ",
                "Task editor: Enter accepts concrete suggestion without newline");
            editor.Undo(); Check(editor.Text == "- [ ] Work du", "Task editor: suggestion is one undo step");
            Set("- [ ] Work 📅 ");
            Check(behavior.IsOpen && behavior.HandleKey(Key.Enter, ModifierKeys.None) && editor.Text == "- [ ] Work 📅 \n- [ ] ",
                "Task editor: unselected contextual menu allows Enter to continue list");
            Set("- [ ] Work 📅 ");
            behavior.HandleKey(Key.Down, ModifierKeys.None); behavior.HandleKey(Key.Enter, ModifierKeys.None);
            Check(editor.Text == "- [ ] Work 📅 2026-12-31 ", "Task editor: arrow keys select date candidate");
            Set("- [ ] Work 📅 ");
            var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
            var popup = (System.Windows.Controls.Primitives.Popup)typeof(TaskEditorBehavior).GetField("popup", flags)!.GetValue(behavior)!;
            var choices = (ListBox)typeof(TaskEditorBehavior).GetField("choices", flags)!.GetValue(behavior)!;
            choices.UpdateLayout();
            RenderLocalizationPreview((FrameworkElement)popup.Child, "task-editor-completion");
            var item = (ListBoxItem)choices.ItemContainerGenerator.ContainerFromIndex(1);
            var click = new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left) { RoutedEvent = Mouse.PreviewMouseDownEvent };
            item.RaiseEvent(click);
            Check(click.Handled && editor.IsKeyboardFocusWithin && editor.Text == "- [ ] Work 📅 2027-01-01 ",
                "Task editor: mouse candidate insertion keeps editing focus");
            Set("- [ ] Work du");
            Check(behavior.HandleKey(Key.Escape, ModifierKeys.None) && !behavior.IsOpen && editor.Text == "- [ ] Work du",
                "Task editor: first Escape dismisses without changing draft");
            behavior.Update(); Check(!behavior.IsOpen && !behavior.HandleKey(Key.Escape, ModifierKeys.None), "Task editor: second Escape is available to save/exit");
            Set("- [ ] Work due"); behavior.SetComposing(true);
            Check(!behavior.IsOpen && !behavior.HandleKey(Key.Enter, ModifierKeys.None) && !behavior.HandleKey(Key.Escape, ModifierKeys.None),
                "Task editor: IME owns Enter/Escape during composition");
            behavior.SetComposing(false); behavior.Update();
            Check(behavior.IsOpen && !behavior.HandleKey(Key.Enter, ModifierKeys.Shift) && !behavior.HandleKey(Key.ImeProcessed, ModifierKeys.None),
                "Task editor: modified and IME keys remain native");
            behavior.HandleKey(Key.Tab, ModifierKeys.None); Check(editor.Text == "- [ ] Work 📅 ", "Task editor: Tab accepts candidate");
            Set("- [ ] abc");
            Check(behavior.HandleKey(Key.Enter, ModifierKeys.None) && editor.Text == "- [ ] abc\n- [ ] ", "Task editor: ordinary task Enter continues list");
            Check(behavior.HandleKey(Key.Enter, ModifierKeys.None) && editor.Text == "- [ ] abc\n", "Task editor: next Enter ends empty list");
            Check(probes == initialProbes && initialProbes > 0, "Task completion detection: typing does not make additional plugin requests");
            probe = _ => Task.FromResult(false); behavior.RefreshAvailability().GetAwaiter().GetResult(); Set("- [ ] Work du");
            Check(!behavior.IsOpen && behavior.HandleKey(Key.Enter, ModifierKeys.None) && editor.Text == "- [ ] Work du\n- [ ] ",
                "Task completion detection: absent plugin disables suggestions but keeps list continuation");
            var pending = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            probe = _ => pending.Task; var oldProbe = behavior.RefreshAvailability();
            probe = _ => Task.FromResult(false); behavior.RefreshAvailability().GetAwaiter().GetResult();
            pending.SetResult(true); WaitFor(() => oldProbe.IsCompleted, "Task completion detection: obsolete response settles");
            Set("- [ ] Work du"); Check(!behavior.IsOpen, "Task completion detection: obsolete enabled response cannot bypass newer OFF result");
            probe = _ => Task.FromResult(true); behavior.RefreshAvailability().GetAwaiter().GetResult();
            Check(behavior.IsOpen, "Task completion detection: explicit enable restores candidates");
            Set("- [ ] Work du"); window.Close();
            Check(!behavior.IsOpen, "Task editor: closing owner disposes popup and pending timer");
        }
        finally { window.Close(); }
    }

    private static void TaskCompletionPolicyTests(string root)
    {
        Check(new Settings().TaskCompletion == TaskCompletionMode.Automatic && JsonSerializer.Deserialize<Settings>("{}")!.TaskCompletion == TaskCompletionMode.Automatic,
            "Task completion policy: new and legacy settings default to plugin detection");
        var calls = 0; var available = false; var fail = false;
        var settings = new Settings { ObsidianVaultFolder = root };
        NoteSource Source(Settings value) => new(value, true, (_, code, token) =>
        {
            calls++; var request = CliRequest(code);
            Check(request.GetProperty("mode").GetString() == "availability" && !request.TryGetProperty("path", out var ignoredPath),
                "Task completion policy: detection asks only for loaded plugin presence");
            if (fail) throw new IOException("Synthetic unavailable Obsidian");
            return Task.FromResult("STICKY_TASKS_PREVIEW:" + JsonSerializer.Serialize(new { available }));
        });
        bool Enabled() => TaskCompletionPolicy.IsEnabledAsync(settings, Source, default).GetAwaiter().GetResult();
        Check(!Enabled(), "Task completion policy: missing or disabled Tasks stays off by default");
        available = true; Check(Enabled(), "Task completion policy: enabled Tasks allows automatic completion");
        fail = true; Check(!Enabled(), "Task completion policy: failed detection never enables completion");
        var before = calls;
        settings.TaskCompletion = TaskCompletionMode.On; Check(Enabled() && calls == before, "Task completion policy: manual ON overrides missing plugin without probing");
        settings.TaskCompletion = TaskCompletionMode.Off; Check(!Enabled() && calls == before, "Task completion policy: manual OFF never probes");
        settings.TaskCompletion = TaskCompletionMode.Automatic; settings.ObsidianTasksEnabled = false;
        Check(!Enabled() && calls == before, "Task completion policy: automatic local mode requires manual ON");
        settings.TaskCompletion = TaskCompletionMode.On; Check(Enabled(), "Task completion policy: manual ON works in local mode");
        Check(JsonSerializer.Deserialize<Settings>(JsonSerializer.Serialize(settings))!.TaskCompletion == TaskCompletionMode.On,
            "Task completion policy: explicit choice survives restart serialization");
    }

    private static void TaskCheckboxTests(string root)
    {
        var calls = 0; string result = ""; JsonElement request = default;
        var settings = new Settings { ObsidianVaultFolder = root };
        var source = new NoteSource(settings, true, (_, code, _) =>
        { calls++; request = CliRequest(code); return Task.FromResult("STICKY_TASKS_PREVIEW:" + JsonSerializer.Serialize(new { text = result })); });
        var snapshot = new FileSnapshot(Path.Combine(root, "tasks.md"), "", "cli:" + new string('a', 64));
        const string content = "heading\r\n- [ ] Recurring\r\n  - [ ] Child\r\nSuffix";
        result = "- [x] Recurring ✅ 2026-12-31\n- [ ] Recurring 📅 2027-01-01";
        var updated = source.ToggleTaskContent(snapshot, content, 1, true);
        Check(updated == "heading\r\n- [x] Recurring ✅ 2026-12-31\r\n- [ ] Recurring 📅 2027-01-01\r\n  - [ ] Child\r\nSuffix",
            "Native task transform: recurrence preserves surrounding lines and CRLF");
        Check(calls == 1 && request.GetProperty("line").GetString() == "- [ ] Recurring" && request.GetProperty("path").GetString() == "tasks.md",
            "Native task transform: only selected line and source path are transmitted");
        result = ""; Check(source.ToggleTaskContent(snapshot, content, 1, true) == "heading\r\n  - [ ] Child\r\nSuffix", "Native task transform: delete removes selected line only");
        Check(source.ToggleTaskContent(snapshot, "before\n- [ ] Last", 1, true) == "before\n", "Native task transform: delete handles final line without newline");
        var before = calls;
        Check(source.ToggleTaskContent(snapshot, "- [x] Done", 0, true) == "- [x] Done" && calls == before, "Native task transform: redundant state change is a no-op");
        Throws<ArgumentOutOfRangeException>(() => source.ToggleTaskContent(snapshot, content, 100, true), "Native task transform: invalid line rejected");
        Throws<ConflictException>(() => source.ToggleTaskContent(snapshot with { Hash = "local" }, content, 1, true), "Native task transform: stale storage mode rejected");
        var local = new NoteSource(settings, false, (_, _, _) => throw new Exception("No CLI in local mode"));
        Check(local.ToggleTaskContent(snapshot, "- [ ] Local", 0, true) == "- [x] Local", "Native task transform: local mode retains plain checkbox behavior");
    }

    private static void TaskCheckboxWindowTests(string root)
    {
        var app = App.Current; var enabled = app.Config.ObsidianTasksEnabled; var vault = app.Config.ObsidianVaultFolder; var factory = app.NoteSources;
        const string original = "---\r\ntitle: Keep\r\n---\r\n## Tasks\r\n- [ ] Repeat\r\n## Other\r\nKeep\r\n";
        var stored = original; string? backedUp = null; var conflict = false; var transforms = 0; var saves = 0;
        var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        try
        {
            app.Config.ObsidianTasksEnabled = true; app.Config.ObsidianVaultFolder = root;
            app.NoteSources = settings => new NoteSource(settings, true, (_, code, _) =>
            {
                var request = CliRequest(code);
                if (!request.TryGetProperty("mode", out var mode))
                {
                    transforms++; if (conflict) stored += "External change";
                    return Task.FromResult("STICKY_TASKS_PREVIEW:" + JsonSerializer.Serialize(new { text = "- [x] Repeat ✅ 2026-12-31\n- [ ] Next 📅 2027-01-01" }));
                }
                if (mode.GetString() == "save")
                {
                    using var snapshot = ObsidianTasksClient.ParseEnvelope(CliSnapshotEnvelope(stored));
                    if (request.GetProperty("hash").GetString() != snapshot.RootElement.GetProperty("hash").GetString()) throw new ConflictException("Synthetic checkbox conflict");
                    backedUp = stored; stored = request.GetProperty("text").GetString()!; saves++;
                    Check(request.GetProperty("backup").GetString()!.EndsWith("backups"), "Native checkbox save: retains backup destination");
                }
                return Task.FromResult(CliSnapshotEnvelope(stored));
            });
            var window = new NoteWindow(new NotePlacement { Path = Path.Combine(root, "linked-task.md"), Heading = "Tasks" });
            try
            {
                typeof(NoteWindow).GetMethod("Reload", flags)!.Invoke(window, null);
                typeof(NoteWindow).GetMethod("ToggleTask", flags)!.Invoke(window, [0, true]);
                Check(transforms == 1 && saves == 1 && backedUp == original && stored.StartsWith("---\r\ntitle: Keep\r\n---\r\n## Tasks\r\n") &&
                    stored.Contains("- [x] Repeat ✅ 2026-12-31\r\n- [ ] Next 📅 2027-01-01\r\n## Other\r\nKeep\r\n"),
                    "Native checkbox save: linked section retains YAML, surrounding headings and recurrence output; " + transforms + "/" + saves + " " +
                    ((TextBlock)typeof(NoteWindow).GetField("status", flags)!.GetValue(window)!).Text);
                var before = stored; conflict = true;
                typeof(NoteWindow).GetMethod("ToggleTask", flags)!.Invoke(window, [0, false]);
                var status = (TextBlock)typeof(NoteWindow).GetField("status", flags)!.GetValue(window)!;
                Check(saves == 1 && stored == before + "External change" && status.Text.Contains("Synthetic checkbox conflict"),
                    "Native checkbox save: external edit after transformation blocks overwrite and reports conflict");
            }
            finally { window.Close(); }
        }
        finally { app.Config.ObsidianTasksEnabled = enabled; app.Config.ObsidianVaultFolder = vault; app.NoteSources = factory; }
    }
}
