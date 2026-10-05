using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using StickyNotes;
using StickyNotes.Core;

internal static class Program
{
    private sealed class TestApp : App
    {
        // Dispatcher-driven UI tests must not start the real tray app or acquire its mutex.
        protected override void OnStartup(StartupEventArgs e) { }
    }

    private static int count;
    private static void Check(bool condition, string name)
    {
        if (!condition) throw new Exception("FAIL: " + name);
        Console.WriteLine("PASS: " + name); count++;
    }
    private static void Throws<T>(Action action, string name) where T : Exception
    {
        try { action(); } catch (T) { Check(true, name); return; }
        throw new Exception("FAIL: " + name);
    }

    [STAThread]
    public static void Main()
    {
        var root = Path.Combine(Path.GetTempPath(), "StickyNotes.Tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var backups = Path.Combine(root, "backups");
        try
        {
            const string original = "---\r\ntags: [daily]\r\ncustom: keep\r\n---\r\n# Journal\r\nintro\r\n## Tasks\r\n- [ ] One\r\n### Sub\r\n- [x] Two\r\n```md\r\n## fake\r\n```\r\n## Log\r\nuntouched\r\n";
            var span = SectionEditor.Find(original, "Tasks");
            Check(span.Content.Contains("## fake") && !span.Content.Contains("untouched"), "section includes children and ignores fenced headings");
            var changed = SectionEditor.Replace(original, "Tasks", "- [x] One\n");
            Check(changed.StartsWith(original[..span.Start]) && changed.EndsWith("## Log\r\nuntouched\r\n"), "section edit preserves prefix, following section and CRLF");
            Check(changed.Contains("- [x] One\r\n## Log"), "section edit uses existing newline convention");
            Throws<InvalidOperationException>(() => SectionEditor.Find("## Tasks\na\n## Tasks\nb", "Tasks"), "duplicate headings fail safely");
            Throws<InvalidOperationException>(() => SectionEditor.Find(original, "Missing"), "missing heading fails safely");
            Throws<InvalidOperationException>(() => SectionEditor.Replace(original, "Tasks", "## Unexpected\n"), "peer heading cannot escape linked region");
            Check(SectionEditor.Find("~~~\n## Tasks\n~~~\n## Tasks\nyes\n", "Tasks").Content == "yes\n", "tilde fenced headings ignored");
            var task = SectionEditor.ToggleTaskAtLine("- [ ] first\r\n  - [ ] second\r\n", 1, true);
            Check(task == "- [ ] first\r\n  - [x] second\r\n", "nested task update affects only selected line");
            Throws<InvalidOperationException>(() => SectionEditor.ToggleTaskAtLine("# heading", 0, true), "non-task cannot be toggled");

            var note = NoteStore.Create(root);
            var snap = NoteStore.Read(note);
            var meta = NoteStore.Metadata(snap.Text);
            Check(meta.Tags.SequenceEqual(new[] { "sticky" }) && meta.Color == "yellow", "new note has Obsidian properties");
            var metadataText = NoteStore.WithMetadata(original, new("a: title", ["project/日本語", "task"], "done", "blue"));
            Check(NoteStore.Split(metadataText).Body == NoteStore.Split(original).Body && metadataText.Contains("custom: keep"), "metadata edit retains body and unknown property");
            Check(NoteStore.Metadata(metadataText).Title == "a: title", "YAML escaping round trip");
            var saved = NoteStore.Save(snap, snap.Text + "\nnew", backups);
            Check(File.ReadAllText(note) == saved.Text && Directory.GetFiles(backups).Length == 1, "save writes note and recoverable backup");
            File.AppendAllText(note, " external");
            Throws<ConflictException>(() => NoteStore.Save(saved, "bad", backups), "external edits block stale write");
            Check(File.ReadAllText(note).EndsWith(" external"), "conflict preserves external content");
            var bomPath = Path.Combine(root, "bom.md");
            File.WriteAllText(bomPath, "# UTF8\r\n日本語", new UTF8Encoding(true));
            var bom = NoteStore.Read(bomPath);
            NoteStore.Save(bom, bom.Text + "!", backups);
            Check(File.ReadAllBytes(bomPath).Take(3).SequenceEqual(new byte[] { 239, 187, 191 }), "UTF8 BOM preserved");
            MigrationTests(root);
            IndependentFolderSettingsTests(root);
            DailyRegexTests(root);
            DailyPreviewTests(root);

            var query = CalendarQuery.Parse("@calendar 2026-10-05T09:00+09:00 設計 会議");
            Check(query.From.Offset == TimeSpan.FromHours(9) && query.Search == "設計 会議", "calendar query parses offset and multiword search");
            Throws<FormatException>(() => CalendarQuery.Parse("@calendar invalid"), "invalid date rejected");
            Check(CalendarQuery.Parse("@calendar 2026-10-05").Search == "", "calendar search is optional");
            CalendarTests(root).GetAwaiter().GetResult();

            var toggledLine = -1;
            var doc = MarkdownView.Render("# Title\n\n- [ ] task\n\n```md\n- [ ] example\n```\n\n| A | B |\n|---|---|\n| x | y |", (line, _) => toggledLine = line);
            Check(doc.Blocks.OfType<Paragraph>().Any(p => p.FontSize == 25), "Markdown heading rendered");
            Check(doc.Blocks.OfType<Table>().Count() == 1, "Markdown table rendered");
            var list = doc.Blocks.OfType<System.Windows.Documents.List>().Single();
            var paragraph = list.ListItems.FirstListItem.Blocks.OfType<Paragraph>().Single();
            var checkbox = (CheckBox)paragraph.Inlines.OfType<InlineUIContainer>().Single().Child;
            checkbox.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
            Check(toggledLine == 2, "rendered task callback maps to source line, excluding fenced example");
            Check(MarkdownView.FindCalendarCommand("```text\n@calendar 2026-10-05 example\n```\n\n@calendar 2026-10-06 real") == "@calendar 2026-10-06 real", "calendar examples in fenced code are not executed");
            Console.WriteLine($"\n{count} tests passed.");
        }
        finally { Directory.Delete(root, true); }
    }

    private static void DailyPreviewTests(string root)
    {
        var folder = Path.Combine(root, "daily-preview");
        Directory.CreateDirectory(folder);
        var today = DateTime.Today;
        var file = Path.Combine(folder, today.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture) + ".md");
        const string content = "## Tasks\n- [ ] read-only preview\n<script>literal text</script>";
        File.WriteAllText(file, content, new UTF8Encoding(true));
        var bytes = File.ReadAllBytes(file);
        var result = DailyNotePreview.Read(folder, DailyNoteResolver.RegexExample, true, today);
        Check(result.Path == file && result.Text == content && !result.Truncated, "daily preview finds today's note and reads UTF8 without BOM");
        Check(File.ReadAllBytes(file).SequenceEqual(bytes), "daily preview never modifies note contents");
        File.WriteAllText(file, new string('x', DailyNotePreview.CharacterLimit) + "tail");
        result = DailyNotePreview.Read(folder, DailyNoteResolver.RegexExample, true, today);
        Check(result.Text.Length == DailyNotePreview.CharacterLimit && result.Truncated, "preview bounds large note contents");
        File.WriteAllText(file, new string('x', DailyNotePreview.CharacterLimit - 1) + "😀tail");
        result = DailyNotePreview.Read(folder, DailyNoteResolver.RegexExample, true, today);
        Check(!char.IsSurrogate(result.Text[^1]) && result.Truncated, "preview truncation does not split surrogate pairs");
        using (var cancelled = new CancellationTokenSource())
        {
            cancelled.Cancel();
            Throws<OperationCanceledException>(() => DailyNotePreview.Read(folder, DailyNoteResolver.RegexExample, true, today, cancelled.Token), "obsolete preview requests can be cancelled");
        }
        File.WriteAllText(file, content);
        var app = App.Current;
        app.ShutdownMode = ShutdownMode.OnExplicitShutdown;
        var config = File.ReadAllBytes(Path.Combine(App.DataDirectory, "settings.json"));
        var originalDaily = app.Config.DailyFolder;
        var window = new SettingsWindow();
        var panel = (StackPanel)((ScrollViewer)window.Content).Content;
        var inputs = panel.Children.OfType<TextBox>().ToArray();
        var regexMode = panel.Children.OfType<CheckBox>().Single();
        var previewPanel = panel.Children.OfType<StackPanel>().Single();
        var status = previewPanel.Children.OfType<TextBlock>().Single(x => x.Name == "DailyPreviewStatus");
        var preview = previewPanel.Children.OfType<TextBox>().Single();
        inputs[1].Text = folder; regexMode.IsChecked = true; inputs[2].Text = DailyNoteResolver.RegexExample;
        WaitFor(() => preview.Visibility == Visibility.Visible, "typing regex updates the live preview without saving");
        Check(status.Text.Contains(Path.GetFileName(file)) && preview.Text == content && preview.IsReadOnly, "live preview shows matching filename and read-only content");
        inputs[2].Text = "[";
        Check(preview.Visibility == Visibility.Collapsed && preview.Text == "", "new input immediately removes stale content");
        WaitFor(() => status.Text.StartsWith("確認できません:"), "invalid regex displays an inline error");
        inputs[2].Text = "missing/" + DailyNoteResolver.RegexExample;
        WaitFor(() => status.Text.Contains("今日のデイリーノートがありません"), "no matching file displays an inline error");
        var duplicate = Path.Combine(folder, today.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture) + "(weekday).md");
        File.WriteAllText(duplicate, "duplicate");
        inputs[2].Text = DailyNoteResolver.RegexExample;
        WaitFor(() => status.Text.Contains("複数一致"), "ambiguous preview does not choose a file");
        File.Delete(duplicate);
        regexMode.IsChecked = false; inputs[2].Text = "yyyy-MM-dd";
        WaitFor(() => preview.Visibility == Visibility.Visible, "switching to date-format mode refreshes preview");
        inputs[1].Text = Path.Combine(folder, "missing");
        WaitFor(() => status.Text.StartsWith("確認できません:"), "changing daily folder refreshes preview");
        inputs[1].Text = folder; inputs[2].Text = "'missing'"; inputs[2].Text = "yyyy-MM-dd";
        WaitFor(() => preview.Visibility == Visibility.Visible && status.Text.Contains(Path.GetFileName(file)), "rapid edits display only the latest input result");
        Check(File.ReadAllBytes(Path.Combine(App.DataDirectory, "settings.json")).SequenceEqual(config) && app.Config.DailyFolder == originalDaily && File.ReadAllText(file) == content, "live preview does not save settings or change note files");
        inputs[2].Text = "'missing'";
        window.Close();
        var closeStatus = status.Text;
        var stop = System.Diagnostics.Stopwatch.StartNew();
        WaitFor(() => stop.ElapsedMilliseconds >= 450, "closed settings cancel pending preview updates");
        Check(status.Text == closeStatus, "closed preview cannot receive a delayed result");
    }

    private static void WaitFor(Func<bool> condition, string name)
    {
        var frame = new System.Windows.Threading.DispatcherFrame();
        var timer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromMilliseconds(20) };
        var elapsed = System.Diagnostics.Stopwatch.StartNew();
        timer.Tick += (_, _) => { if (condition() || elapsed.Elapsed > TimeSpan.FromSeconds(5)) frame.Continue = false; };
        timer.Start();
        try { System.Windows.Threading.Dispatcher.PushFrame(frame); }
        finally { timer.Stop(); }
        Check(condition(), name);
    }

    private static void IndependentFolderSettingsTests(string root)
    {
        var app = App.Current;
        var source = Path.Combine(root, "independent-source");
        var target = Path.Combine(root, "independent-target");
        Directory.CreateDirectory(source);
        var note = NoteStore.Create(source);
        app.ApplySettings(new Settings { NotesFolder = source, DailyFolder = source }, false);
        app.ApplySettings(new Settings { NotesFolder = target, DailyFolder = source }, true);
        Check(app.Config.NotesFolder == target && app.Config.DailyFolder == source, "migrating notes does not synchronize equal daily-folder setting");
        Check(File.Exists(Path.Combine(target, Path.GetFileName(note))), "independent settings retain requested Markdown migration");
        var saved = JsonSerializer.Deserialize<Settings>(File.ReadAllText(Path.Combine(App.DataDirectory, "settings.json")))!;
        Check(saved.NotesFolder == target && saved.DailyFolder == source, "separate folder values persist for restart");

        var chosenDaily = Path.Combine(target, "chosen-daily");
        app.ApplySettings(new Settings { NotesFolder = source, DailyFolder = chosenDaily }, true);
        Check(app.Config.DailyFolder == chosenDaily, "migration preserves explicitly selected daily folder inside old notes folder");
        app.ApplySettings(new Settings { NotesFolder = source, DailyFolder = target }, false);
        Check(app.Config.NotesFolder == source && app.Config.DailyFolder == target, "changing daily folder leaves notes folder unchanged");
        app.ApplySettings(new Settings { NotesFolder = target, DailyFolder = target }, false);
        Check(app.Config.NotesFolder == target && app.Config.DailyFolder == target, "changing notes folder without migration preserves chosen daily folder");

        var settingsWindow = new SettingsWindow();
        var panel = (StackPanel)((ScrollViewer)settingsWindow.Content).Content;
        var inputs = panel.Children.OfType<TextBox>().ToArray();
        inputs[0].Text = source;
        Check(inputs[1].Text == target, "editing notes input does not change daily input");
        inputs[1].Text = chosenDaily;
        Check(inputs[0].Text == source, "editing daily input does not change notes input");
        settingsWindow.Close();
        app.ApplySettings(new Settings { NotesFolder = source, DailyFolder = target }, false);
        var reopened = new SettingsWindow();
        var reopenedInputs = ((StackPanel)((ScrollViewer)reopened.Content).Content).Children.OfType<TextBox>().ToArray();
        Check(reopenedInputs[0].Text == source && reopenedInputs[1].Text == target, "reopened settings display independently saved folders");
        reopened.Close();
    }

    private static void DailyRegexTests(string root)
    {
        var folder = Path.Combine(root, "daily-regex");
        Directory.CreateDirectory(folder);
        var today = new DateTime(2026, 10, 5);
        var current = Path.Combine(folder, "2026-10-05(月).md");
        File.WriteAllText(current, "## Tasks\n- [ ] today");
        File.WriteAllText(Path.Combine(folder, "2026-10-04(日).md"), "## Tasks\nyesterday");
        File.WriteAllText(Path.Combine(folder, "WeeklyTasksLog-2026-10-05(月).md"), "unrelated");
        Check(DailyNoteResolver.Resolve(folder, DailyNoteResolver.RegexExample, true, today) == current, "regex selects today's Japanese weekday filename and ignores prefixes");
        Check(DailyNoteResolver.Resolve(folder, DailyNoteResolver.RegexExample, true, today.AddDays(-1)).EndsWith("2026-10-04(日).md"), "regex rolls over using requested date");
        Check(DailyNoteResolver.Resolve(folder, "yyyy/MM/yyyy-MM-dd", false, today) == Path.Combine(folder, "2026", "10", "2026-10-05.md"), "legacy date formats and automatic extension remain compatible");
        Check(!JsonSerializer.Deserialize<Settings>("{\"DailyPattern\":\"yyyy-MM-dd\"}")!.DailyPatternIsRegex, "existing settings default to date-format mode");
        var settings = new Settings { DailyPattern = DailyNoteResolver.RegexExample, DailyPatternIsRegex = true };
        Check(JsonSerializer.Deserialize<Settings>(JsonSerializer.Serialize(settings))!.DailyPatternIsRegex, "regex setting round trips");
        Throws<InvalidOperationException>(() => DailyNoteResolver.Validate("[", true), "invalid regex rejected at settings validation");
        Throws<InvalidOperationException>(() => DailyNoteResolver.Validate(@"\d{4}-\d{2}-\d{2}\.md", true), "regex without date groups rejected");
        Throws<InvalidOperationException>(() => DailyNoteResolver.Validate("", true), "blank regex rejected");
        Throws<InvalidOperationException>(() => DailyNoteResolver.Validate(new string('a', 4097), true), "excessive regex length rejected");
        Throws<InvalidOperationException>(() => DailyNoteResolver.Resolve(folder, "'../outside'", false, today), "legacy date path cannot escape daily folder");
        Throws<FileNotFoundException>(() => DailyNoteResolver.Resolve(folder, DailyNoteResolver.RegexExample, true, today.AddDays(1)), "missing date does not fall back to another note");
        File.WriteAllText(Path.Combine(folder, "2026-10-05.md"), "duplicate");
        Throws<IOException>(() => DailyNoteResolver.Resolve(folder, DailyNoteResolver.RegexExample, true, today), "ambiguous same-date matches fail safely");
        File.Delete(Path.Combine(folder, "2026-10-05.md"));
        Directory.CreateDirectory(Path.Combine(folder, "Diary"));
        var nested = Path.Combine(folder, "Diary", "2026-10-05(月).md");
        File.WriteAllText(nested, "nested");
        Check(DailyNoteResolver.Resolve(folder, "Diary/" + DailyNoteResolver.RegexExample, true, today) == nested, "regex uses slash-separated full relative path including extension");
        File.WriteAllText(Path.Combine(folder, "2026-10-05" + new string('a', 80) + "!.md"), "timeout fixture");
        Throws<IOException>(() => DailyNoteResolver.Resolve(folder, @"(?<year>\d{4})-(?<month>\d{2})-(?<day>\d{2})(a+)+\.md", true, today), "pathological regex times out instead of blocking indefinitely");

        var app = App.Current;
        app.Config.DailyFolder = folder; app.Config.DailyPattern = DailyNoteResolver.RegexExample; app.Config.DailyPatternIsRegex = true;
        var window = new NoteWindow(new NotePlacement { Daily = true, Heading = "Tasks" });
        var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        var snapshotField = typeof(NoteWindow).GetField("snapshot", flags)!;
        var tick = typeof(NoteWindow).GetMethod("Tick", flags)!;
        // An invalid regex simulates a resolution failure independently of the machine's date.
        app.Config.DailyPattern = "[";
        snapshotField.SetValue(window, NoteStore.Read(current));
        ((Task)tick.Invoke(window, null)!).GetAwaiter().GetResult();
        Check(snapshotField.GetValue(window) is null, "failed daily resolution clears stale editable preview");
        snapshotField.SetValue(window, NoteStore.Read(current));
        var editor = (TextBox)typeof(NoteWindow).GetField("editor", flags)!.GetValue(window)!;
        editor.Text = "keep unsaved edit";
        typeof(NoteWindow).GetField("editing", flags)!.SetValue(window, true);
        ((Task)tick.Invoke(window, null)!).GetAwaiter().GetResult();
        Check(snapshotField.GetValue(window) is not null && editor.Text == "keep unsaved edit", "failed resolution retains unsaved editing");
    }

    private static void MigrationTests(string root)
    {
        var source = Path.Combine(root, "migration-source");
        var destination = Path.Combine(root, "migration-destination");
        Directory.CreateDirectory(Path.Combine(source, "nested"));
        var first = Path.Combine(source, "a.md");
        var second = Path.Combine(source, "nested", "b.MD");
        File.WriteAllText(first, "---\r\ntype: sticky\r\n---\r\n日本語", new UTF8Encoding(true));
        File.WriteAllText(second, "closed note");
        File.WriteAllText(Path.Combine(source, "keep.txt"), "unrelated");
        var bytes = File.ReadAllBytes(first);
        Check(NoteFolderMigration.SameFolder(source, source.ToUpperInvariant() + Path.DirectorySeparatorChar), "folder comparison normalizes case and trailing separator");
        Throws<InvalidOperationException>(() => NoteFolderMigration.Normalize("relative"), "migration rejects relative paths");
        Throws<IOException>(() => NoteFolderMigration.Move(source, Path.Combine(source, "child"), _ => { }), "migration rejects descendant destination");
        Throws<IOException>(() => NoteFolderMigration.Move(source, root, _ => { }), "migration rejects ancestor destination");
        Directory.CreateDirectory(Path.Combine(destination, "nested"));
        File.WriteAllText(Path.Combine(destination, "nested", "b.MD"), "existing");
        Throws<IOException>(() => NoteFolderMigration.Move(source, destination, _ => throw new Exception("must not commit")), "collision preflight stops all moves");
        Check(File.Exists(first) && File.ReadAllText(Path.Combine(destination, "nested", "b.MD")) == "existing", "collision preserves both folders");
        File.Delete(Path.Combine(destination, "nested", "b.MD"));
        using (var locked = new FileStream(second, FileMode.Open, FileAccess.Read, FileShare.Read))
            Throws<IOException>(() => NoteFolderMigration.Move(source, destination, _ => throw new Exception("must not commit")), "locked file rolls back earlier moves");
        Check(File.Exists(first) && File.Exists(second) && !File.Exists(Path.Combine(destination, "a.md")), "move failure restores source files");
        Throws<InvalidOperationException>(() => NoteFolderMigration.Move(source, destination, _ => throw new InvalidOperationException("settings write failed")), "settings failure rolls back migration");
        Check(File.ReadAllBytes(first).SequenceEqual(bytes) && File.Exists(second), "rollback preserves exact note bytes");
        NoteFolderMigration.Move(source, destination, paths =>
        {
            Check(paths.Count == 2 && paths[second] == Path.Combine(destination, "nested", "b.MD"), "migration maps all Markdown including closed and nested notes");
            Check(paths.Values.All(File.Exists), "settings committed only after all files move");
        });
        Check(!File.Exists(first) && !File.Exists(second) && File.ReadAllBytes(Path.Combine(destination, "a.md")).SequenceEqual(bytes), "migration preserves BOM and bytes and removes originals");
        Check(File.Exists(Path.Combine(source, "keep.txt")), "migration leaves non-Markdown files in place");
        NoteFolderMigration.Move(Path.Combine(root, "missing-source"), Path.Combine(root, "empty-destination"), paths => Check(paths.Count == 0, "missing old folder allows first-time setup"));
        NoteFolderMigration.Move(destination, destination + Path.DirectorySeparatorChar, paths => Check(paths.Count == 0, "same folder does not move files"));
        var recoverySource = Path.Combine(root, "recovery-source");
        var recoveryDestination = Path.Combine(root, "recovery-destination");
        Directory.CreateDirectory(recoverySource);
        File.WriteAllText(Path.Combine(recoverySource, "note.md"), "original");
        try
        {
            NoteFolderMigration.Move(recoverySource, recoveryDestination, _ =>
            {
                File.WriteAllText(Path.Combine(recoverySource, "note.md"), "concurrent creation");
                throw new IOException("settings failure");
            });
            throw new Exception("Expected rollback failure");
        }
        catch (IOException error)
        {
            Check(error.Message.Contains(Path.Combine(recoveryDestination, "note.md")), "rollback failure reports recoverable file location");
        }
        Check(File.ReadAllText(Path.Combine(recoverySource, "note.md")) == "concurrent creation" && File.ReadAllText(Path.Combine(recoveryDestination, "note.md")) == "original", "rollback never overwrites concurrent source files");

        var app = new TestApp { ShutdownMode = ShutdownMode.OnExplicitShutdown };
        typeof(App).GetProperty(nameof(App.DataDirectory))!.SetValue(null, Path.Combine(root, "app-settings"));
        Directory.CreateDirectory(App.DataDirectory);
        app.Config.NotesFolder = destination;
        app.Config.DailyFolder = Path.Combine(destination, "nested");
        var placement = new NotePlacement { Path = Path.Combine(destination, "a.md"), Left = 234, Pinned = true };
        var window = new NoteWindow(placement);
        app.Notes.Add(window);
        var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        var snapshotField = typeof(NoteWindow).GetField("snapshot", flags)!;
        var dirtyField = typeof(NoteWindow).GetField("dirty", flags)!;
        var editor = (TextBox)typeof(NoteWindow).GetField("editor", flags)!.GetValue(window)!;
        var snapshot = NoteStore.Read(placement.Path);
        snapshotField.SetValue(window, snapshot);
        editor.Text = "unsaved edit";
        typeof(NoteWindow).GetField("editing", flags)!.SetValue(window, true);
        var dailyWindow = new NoteWindow(new NotePlacement { Daily = true, Heading = "Tasks" });
        snapshotField.SetValue(dailyWindow, NoteStore.Read(Path.Combine(destination, "nested", "b.MD")));
        app.Notes.Add(dailyWindow);
        var outside = new NoteWindow(new NotePlacement { Path = Path.Combine(root, "bom.md"), Heading = "Tasks" });
        app.Notes.Add(outside);
        var next = Path.Combine(root, "app-migrated");
        app.ApplySettings(new Settings { NotesFolder = next, DailyFolder = app.Config.DailyFolder }, true);
        Check(placement.Path == Path.Combine(next, "a.md") && placement.Left == 234 && placement.Pinned, "open window follows migration and retains placement");
        Check(((FileSnapshot)snapshotField.GetValue(window)!).Path == placement.Path && editor.Text == "unsaved edit" && (bool)dirtyField.GetValue(window)!, "migration retains unsaved editor and updates snapshot path");
        Check(app.Config.DailyFolder == Path.Combine(destination, "nested"), "daily folder inside source remains explicitly configured after migration");
        Check(((FileSnapshot)snapshotField.GetValue(dailyWindow)!).Path == Path.Combine(next, "nested", "b.MD") && outside.Placement.Path == Path.Combine(root, "bom.md"), "daily snapshot follows move while outside linked note stays in place");
        Check(JsonSerializer.Deserialize<Settings>(File.ReadAllText(Path.Combine(App.DataDirectory, "settings.json")))!.Windows[0].Path == placement.Path, "restart settings contain migrated note paths");
        var oldConfig = app.Config;
        // Block the temporary settings file to exercise application + filesystem rollback together.
        Directory.CreateDirectory(Path.Combine(App.DataDirectory, "settings.json.tmp"));
        Throws<UnauthorizedAccessException>(() => app.ApplySettings(new Settings { NotesFolder = destination }, true), "settings persistence error aborts application migration");
        Check(ReferenceEquals(app.Config, oldConfig) && File.Exists(placement.Path) && ((FileSnapshot)snapshotField.GetValue(window)!).Path == placement.Path, "settings failure restores config, placement, snapshot, and files");
        Directory.Delete(Path.Combine(App.DataDirectory, "settings.json.tmp"));
        Check((bool)typeof(NoteWindow).GetMethod("Save", flags)!.Invoke(window, null)! && File.ReadAllText(placement.Path).Contains("unsaved edit") && !File.Exists(Path.Combine(destination, "a.md")), "unsaved editing saves to migrated file without recreating original");
        app.ApplySettings(new Settings { NotesFolder = destination }, false);
        Check(app.Config.NotesFolder == destination && placement.Path == Path.Combine(next, "a.md") && File.Exists(placement.Path), "declining migration changes only new-note folder");
    }

    private static async Task CalendarTests(string root)
    {
        var credentials = Path.Combine(root, "test-oauth.json");
        File.WriteAllText(credentials, "{\"installed\":{\"client_id\":\"test-client\"}}");
        var stored = JsonSerializer.Serialize(new { clientId = "test-client", tokens = new OAuthTokens { AccessToken = "test-token", RefreshToken = "test-refresh", ExpiresAt = DateTimeOffset.UtcNow.AddHours(1) } });
        var handler = new FakeHandler();
        using var service = new CalendarService(() => stored, _ => { }, handler);
        service.Configure(credentials);
        var events = await service.SearchAsync("primary", CalendarQuery.Parse("@calendar 2026-10-05T09:00+09:00 会議"));
        Check(events.Count == 1 && events[0].Summary == "Meeting", "calendar response decoded");
        Check(handler.Uri!.Contains("singleEvents=true") && handler.Uri.Contains("timeMin=") && handler.Uri.Contains("q="), "calendar search bounded, ordered and filtered");
        await service.UpdateAsync("primary", events[0], "Changed", "Description");
        Check(handler.Method == HttpMethod.Patch && handler.ETag == "\"v1\"", "calendar update uses PATCH and If-Match");
        using var body = JsonDocument.Parse(handler.Body!);
        Check(body.RootElement.GetProperty("summary").GetString() == "Changed" && !body.RootElement.TryGetProperty("start", out _), "calendar edit changes only title and description");
        handler.Conflict = true;
        try { await service.UpdateAsync("primary", events[0], "Changed", ""); throw new Exception("Expected conflict"); }
        catch (ConflictException) { Check(true, "Google etag conflict becomes safe user-facing error"); }
    }

    private sealed class FakeHandler : HttpMessageHandler
    {
        public string? Uri, ETag, Body;
        public HttpMethod? Method;
        public bool Conflict;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Uri = request.RequestUri!.OriginalString; Method = request.Method;
            ETag = request.Headers.TryGetValues("If-Match", out var values) ? values.Single() : null;
            Body = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
            const string item = "{\"id\":\"event-1\",\"summary\":\"Meeting\",\"description\":\"old\",\"start\":{\"dateTime\":\"2026-10-05T10:00:00+09:00\"},\"etag\":\"\\\"v1\\\"\"}";
            return new HttpResponseMessage(Conflict ? HttpStatusCode.PreconditionFailed : HttpStatusCode.OK) { Content = new StringContent(request.Method == HttpMethod.Get ? "{\"items\":[" + item + "]}" : item) };
        }
    }
}
