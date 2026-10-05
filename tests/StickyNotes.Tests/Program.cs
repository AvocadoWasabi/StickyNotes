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
            DailyWaitingTests(root);
            DailyPreviewTests(root);
            RegexDefaultsTests(root);
            NoteLinkTests(root);
            MarkdownEditingTests(root);
            FocusEditingTests(root);
            LinkPreviewTests(root);
            ContentScaleTests(root);
            TitleButtonOverlayTests(root);
            TaskbarAndTemporaryFrontTests(root);

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

    private static void TaskbarAndTemporaryFrontTests(string root)
    {
        Check(!JsonSerializer.Deserialize<Settings>("{}")!.ShowInTaskbar, "older settings retain tray-only notes");
        var app = App.Current;
        var previous = app.Config;
        app.ApplySettings(new Settings { NotesFolder = previous.NotesFolder, DailyFolder = root }, false);
        var path = Path.Combine(root, "temporary-front.md");
        File.WriteAllText(path, "# Temporary front\nbody\n");
        var note = new NoteWindow(new NotePlacement { Path = path }, () => MessageBoxResult.Cancel, _ => { });
        var pinned = new NoteWindow(new NotePlacement { Path = path, Pinned = true });
        app.Notes.Add(note); app.Notes.Add(pinned);
        var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        T Field<T>(NoteWindow window, string name) => (T)typeof(NoteWindow).GetField(name, flags)!.GetValue(window)!;
        var timer = Field<System.Windows.Threading.DispatcherTimer>(note, "temporaryFrontTimer");
        var pinnedTimer = Field<System.Windows.Threading.DispatcherTimer>(pinned, "temporaryFrontTimer");
        void ClickPin(NoteWindow window) => window.NoteControls.Children.OfType<Button>().Single(x => (string)x.Content is "○" or "●")
            .RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
        try
        {
            note.Show(); pinned.Show();
            WaitFor(() => note.IsLoaded && pinned.IsLoaded, "temporary-front test windows load");
            Check(!note.ShowInTaskbar && !pinned.ShowInTaskbar, "notes start outside the taskbar");
            var settings = new SettingsWindow();
            var panel = (StackPanel)((ScrollViewer)settings.Content).Content;
            panel.Children.OfType<CheckBox>().Single(x => x.Name == "ShowInTaskbar").IsChecked = true;
            Check(!note.ShowInTaskbar && !app.Config.ShowInTaskbar, "taskbar option waits for Save");
            panel.Children.OfType<Button>().Single(x => (string)x.Content == "保存して閉じる")
                .RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
            Check(note.ShowInTaskbar && pinned.ShowInTaskbar, "taskbar setting applies to all existing windows");
            Check(JsonSerializer.Deserialize<Settings>(File.ReadAllText(Path.Combine(App.DataDirectory, "settings.json")))!.ShowInTaskbar, "taskbar setting persists for restart");
            var next = new NoteWindow(new NotePlacement { Path = path });
            Check(next.ShowInTaskbar, "new windows inherit taskbar setting"); next.Close();
            settings = new SettingsWindow(); panel = (StackPanel)((ScrollViewer)settings.Content).Content;
            var taskbar = panel.Children.OfType<CheckBox>().Single(x => x.Name == "ShowInTaskbar");
            Check(taskbar.IsChecked == true, "reopened settings retain taskbar preference");
            taskbar.IsChecked = false;
            panel.Children.OfType<Button>().Single(x => (string)x.Content == "保存して閉じる")
                .RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
            Check(!note.ShowInTaskbar && !pinned.ShowInTaskbar, "taskbar display can be disabled immediately");
            Check(timer.Interval == TimeSpan.FromSeconds(10), "temporary front lasts ten seconds by default");
            MonitorLayout.Capture(note, note.Placement);
            var normalPlacement = JsonSerializer.Serialize(note.Placement);
            note.WindowState = WindowState.Minimized;
            MonitorLayout.Capture(note, note.Placement);
            var minimizedElapsed = System.Diagnostics.Stopwatch.StartNew();
            WaitFor(() => minimizedElapsed.ElapsedMilliseconds >= 700, "geometry save settles while minimized");
            Check(JsonSerializer.Serialize(note.Placement) == normalPlacement, "minimizing preserves restored note position and size");
            pinned.Hide();
            app.BringNotesToFrontTemporarily();
            Check(note.WindowState == WindowState.Normal && pinned.IsVisible && note.Topmost && pinned.Topmost, "temporary front restores minimized and hidden notes");
            app.SaveConfig();
            var saved = JsonSerializer.Deserialize<Settings>(File.ReadAllText(Path.Combine(App.DataDirectory, "settings.json")))!.Windows.Where(x => x.Path == path).ToArray();
            Check(!saved[0].Pinned && saved[1].Pinned, "temporary front never persists as permanent pinning");
            timer.Interval = pinnedTimer.Interval = TimeSpan.FromMilliseconds(120);
            WaitFor(() => !timer.IsEnabled && !pinnedTimer.IsEnabled, "temporary timers expire automatically");
            Check(!note.Topmost && pinned.Topmost, "expiration restores each note's permanent pin state");
            timer.Interval = TimeSpan.FromMilliseconds(400);
            note.BringToFrontTemporarily();
            var elapsed = System.Diagnostics.Stopwatch.StartNew();
            WaitFor(() => elapsed.ElapsedMilliseconds >= 250, "part of the temporary interval elapses");
            note.BringToFrontTemporarily();
            elapsed.Restart();
            WaitFor(() => elapsed.ElapsedMilliseconds >= 250, "repeated invocation passes the old expiry");
            Check(note.Topmost && timer.IsEnabled, "repeated invocation restarts the temporary interval");
            WaitFor(() => !timer.IsEnabled, "restarted interval expires");
            note.BringToFrontTemporarily(); ClickPin(note);
            Check(note.Placement.Pinned && note.Topmost && !timer.IsEnabled, "pinning during temporary front becomes permanent");
            pinned.BringToFrontTemporarily(); ClickPin(pinned);
            Check(!pinned.Placement.Pinned && !pinned.Topmost && !pinnedTimer.IsEnabled, "unpinning during temporary front takes effect immediately");
            note.NoteControls.Children.OfType<Button>().Single(x => (string)x.Content == "編集")
                .RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
            var editor = Field<TextBox>(note, "editor"); editor.Text = "unsaved temporary-front draft";
            foreach (var show in new[] { true, false })
            {
                var nextSettings = JsonSerializer.Deserialize<Settings>(JsonSerializer.Serialize(app.Config))!;
                nextSettings.ShowInTaskbar = show;
                app.ApplySettings(nextSettings, false);
                Check(note.ShowInTaskbar == show && editor.Text == "unsaved temporary-front draft" && editor.Visibility == Visibility.Visible,
                    "taskbar changes preserve active draft: " + show);
            }
            app.BringNotesToFrontTemporarily();
            WaitFor(() => !timer.IsEnabled && !pinnedTimer.IsEnabled, "temporary front expires while editing");
            Check(editor.Text == "unsaved temporary-front draft" && File.ReadAllText(path).Contains("body"), "temporary front preserves unsaved input and source file");
            typeof(NoteWindow).GetMethod("Reload", flags)!.Invoke(note, null);
            note.BringToFrontTemporarily(); pinned.BringToFrontTemporarily();
        }
        finally
        {
            typeof(NoteWindow).GetMethod("Reload", flags)!.Invoke(note, null);
            note.Close(); pinned.Close(); app.ApplySettings(previous, false);
        }
        Check(!timer.IsEnabled && !pinnedTimer.IsEnabled, "closing notes stops temporary timers");
    }

    private static void TitleButtonOverlayTests(string root)
    {
        Check(!JsonSerializer.Deserialize<Settings>("{}")!.TitleButtonOverlay, "older settings keep the always-visible button row");
        var app = App.Current;
        var previous = app.Config;
        app.ApplySettings(new Settings { NotesFolder = previous.NotesFolder, DailyFolder = root }, false);
        var path = Path.Combine(root, "title-overlay.md");
        File.WriteAllText(path, "# Overlay\nbody\n");
        var note = new NoteWindow(new NotePlacement { Path = path }, () => MessageBoxResult.Cancel, _ => { });
        var other = new NoteWindow(new NotePlacement { Path = path });
        app.Notes.Add(note); app.Notes.Add(other);
        var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        T Field<T>(NoteWindow window, string name) => (T)typeof(NoteWindow).GetField(name, flags)!.GetValue(window)!;
        void Reload() => typeof(NoteWindow).GetMethod("Reload", flags)!.Invoke(note, null);
        void Hover(bool value) => note.NoteHeader.RaiseEvent(new System.Windows.Input.MouseEventArgs(System.Windows.Input.Mouse.PrimaryDevice, Environment.TickCount)
            { RoutedEvent = value ? System.Windows.Input.Mouse.MouseEnterEvent : System.Windows.Input.Mouse.MouseLeaveEvent });
        void Click(string label) => note.NoteControls.Children.OfType<Button>().Single(x => (string)x.Content == label).RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
        void Measure() { note.NoteHeader.Measure(new Size(278, 200)); note.NoteHeader.Arrange(new Rect(0, 0, 278, note.NoteHeader.DesiredSize.Height)); }
        try
        {
            Reload();
            var host = Field<Border>(note, "controlsHost");
            Check(host.Visibility == Visibility.Visible && Grid.GetRow(host) == 1, "default controls remain visible below the title");
            Measure(); var normalHeight = note.NoteHeader.DesiredSize.Height;
            var before = File.ReadAllBytes(Path.Combine(App.DataDirectory, "settings.json"));
            var settings = new SettingsWindow();
            var panel = (StackPanel)((ScrollViewer)settings.Content).Content;
            var checkbox = panel.Children.OfType<CheckBox>().Single(x => x.Name == "TitleButtonOverlay");
            Check(checkbox.IsChecked == false, "overlay option displays the existing default");
            checkbox.IsChecked = true;
            Check(!app.Config.TitleButtonOverlay && host.Visibility == Visibility.Visible && File.ReadAllBytes(Path.Combine(App.DataDirectory, "settings.json")).SequenceEqual(before), "changing the option does not affect notes or saved settings before Save");
            panel.Children.OfType<Button>().Single(x => (string)x.Content == "保存して閉じる").RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
            Check(app.Config.TitleButtonOverlay && JsonSerializer.Deserialize<Settings>(File.ReadAllText(Path.Combine(App.DataDirectory, "settings.json")))!.TitleButtonOverlay, "saving settings persists overlay mode");
            Check(Grid.GetRow(host) == 0 && host.Visibility == Visibility.Collapsed && Grid.GetRow(Field<Border>(other, "controlsHost")) == 0, "saving applies hidden title overlays immediately to all open notes");
            Measure(); var hiddenHeight = note.NoteHeader.DesiredSize.Height;
            Hover(true); Measure();
            Check(host.Visibility == Visibility.Visible && hiddenHeight == note.NoteHeader.DesiredSize.Height && hiddenHeight < normalHeight, "title hover reveals an overlay without shifting the body or reserving a second row");
            Check(host.Child.DesiredSize.Width <= 278 && Field<TextBlock>(note, "dragHandle").Visibility == Visibility.Visible && Field<TextBlock>(note, "dragHandle").Width == 24, "overlay buttons and a dedicated drag handle fit the minimum note width");
            Check(host.Background.ToString() == note.Background.ToString(), "overlay background covers the title using the note color");
            Hover(false);
            Check(host.Visibility == Visibility.Collapsed, "leaving the title hides the overlay");
            Hover(true); Click("編集");
            var editor = Field<TextBox>(note, "editor"); editor.Text = "edited with overlay";
            Hover(false); Hover(true);
            Check(editor.Text == "edited with overlay" && editor.Visibility == Visibility.Visible && File.ReadAllText(path).Contains("body"), "overlay visibility changes preserve unsaved editing");
            Click("保存");
            Check(File.ReadAllText(path) == "edited with overlay", "overlay Save still writes the edited note");
            var restored = new NoteWindow(new NotePlacement { Path = path });
            Check(Grid.GetRow(Field<Border>(restored, "controlsHost")) == 0 && Field<Border>(restored, "controlsHost").Visibility == Visibility.Collapsed, "new notes use the saved overlay preference");
            restored.Close();
            settings = new SettingsWindow(); panel = (StackPanel)((ScrollViewer)settings.Content).Content;
            checkbox = panel.Children.OfType<CheckBox>().Single(x => x.Name == "TitleButtonOverlay");
            Check(checkbox.IsChecked == true, "reopened settings retain overlay mode");
            checkbox.IsChecked = false;
            panel.Children.OfType<Button>().Single(x => (string)x.Content == "保存して閉じる").RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
            Hover(false); Measure();
            Check(host.Visibility == Visibility.Visible && Grid.GetRow(host) == 1 && note.NoteHeader.DesiredSize.Height == normalHeight &&
                note.NoteControls.Children.OfType<Button>().First().Width == 40, "disabling overlay restores the original always-visible row and button sizes");
            Check(Field<TextBlock>(note, "dragHandle").Visibility == Visibility.Collapsed && !app.Config.TitleButtonOverlay, "normal mode removes the overlay-only drag handle");
        }
        finally { Reload(); note.Close(); other.Close(); app.ApplySettings(previous, false); }
    }

    private static void ContentScaleTests(string root)
    {
        Check(JsonSerializer.Deserialize<NotePlacement>("{}")!.ContentScale == 100, "existing note settings default to 100% scale");
        Check(JsonSerializer.Deserialize<NotePlacement>("{\"ContentScale\":-100}")!.ContentScale == 50 &&
            JsonSerializer.Deserialize<NotePlacement>("{\"ContentScale\":1000000}")!.ContentScale == 200, "loaded content scale is bounded to 50–200%");
        var path = Path.Combine(root, "content-scale.md");
        const string markdown = "# Heading\n\nBody **bold** [link](https://example.com)\n\n- [ ] task\n\n```text\ncode\n```\n\n| A | B |\n|---|---|\n| one | two |\n";
        File.WriteAllText(path, markdown);
        var app = App.Current;
        var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        T Field<T>(NoteWindow note, string name) => (T)typeof(NoteWindow).GetField(name, flags)!.GetValue(note)!;
        void Call(NoteWindow note, string method) => typeof(NoteWindow).GetMethod(method, flags)!.Invoke(note, null);
        var note = new NoteWindow(new NotePlacement { Path = path }, () => MessageBoxResult.Cancel, _ => { });
        var other = new NoteWindow(new NotePlacement { Path = path });
        app.Notes.Add(note); app.Notes.Add(other);
        try
        {
            Call(note, "Reload"); Call(other, "Reload");
            var preview = Field<FlowDocumentScrollViewer>(note, "preview");
            var document = preview.Document;
            var toolbarSize = note.NoteControls.Children.OfType<Button>().First().FontSize;
            var menu = note.ContentScaleMenu();
            menu.Items.OfType<MenuItem>().Single(x => (string)x.Header == "150%").RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            Check(note.Placement.ContentScale == 150 && preview.Zoom == 150 && ReferenceEquals(document, preview.Document), "scale menu zooms the complete document without replacing content or task callbacks");
            Check(document.Blocks.OfType<Paragraph>().Any(p => p.FontSize == 25) && document.Blocks.OfType<Table>().Single().FontSize == 12, "zoom preserves relative heading and table font sizes");
            Check(((System.Windows.Media.ScaleTransform)Field<StackPanel>(note, "eventsPanel").LayoutTransform).ScaleX == 1.5 &&
                ((System.Windows.Media.ScaleTransform)Field<TextBlock>(note, "dailyNotice").LayoutTransform).ScaleY == 1.5, "calendar results and daily guidance share the content scale");
            Check(note.NoteControls.Children.OfType<Button>().First().FontSize == toolbarSize && Field<TextBox>(note, "editor").FontSize == 14 &&
                other.Placement.ContentScale == 100 && Field<FlowDocumentScrollViewer>(other, "preview").Zoom == 100, "content scaling leaves toolbar, source editor, and other notes unchanged");
            var saved = JsonSerializer.Deserialize<Settings>(File.ReadAllText(Path.Combine(App.DataDirectory, "settings.json")))!;
            var savedNotes = saved.Windows.Where(x => x.Path == path).ToArray();
            Check(savedNotes.Select(x => x.ContentScale).SequenceEqual(new[] { 150, 100 }), "each note's scale is saved separately to settings");
            var restored = new NoteWindow(savedNotes[0]);
            Check(Field<FlowDocumentScrollViewer>(restored, "preview").Zoom == 150, "reopened note restores saved scale");
            restored.Close();
            Check(File.ReadAllText(path) == markdown, "display scaling never modifies source Markdown");
            Call(note, "Reload");
            Check(preview.Zoom == 150, "reloading the Markdown retains content scale");
            preview.Zoom = 175;
            Check(note.Placement.ContentScale == 175 && JsonSerializer.Deserialize<Settings>(File.ReadAllText(Path.Combine(App.DataDirectory, "settings.json")))!.Windows.First(x => x.Path == path).ContentScale == 175, "native document zoom also updates saved scale");
            Check(note.HandleScaleKey(System.Windows.Input.Key.OemPlus, System.Windows.Input.ModifierKeys.Control | System.Windows.Input.ModifierKeys.Shift) && preview.Zoom == 185, "Ctrl plus increases scale by ten percentage points");
            note.HandleScaleKey(System.Windows.Input.Key.Subtract, System.Windows.Input.ModifierKeys.Control);
            Check(preview.Zoom == 175, "Ctrl numpad minus decreases scale");
            note.HandleScaleKey(System.Windows.Input.Key.D0, System.Windows.Input.ModifierKeys.Control);
            Check(preview.Zoom == 100 && note.ContentScaleMenu().Items.OfType<MenuItem>().Single(x => (string)x.Header == "100%").IsChecked, "Ctrl zero restores default and menu checks current scale");
            Check(!note.HandleScaleKey(System.Windows.Input.Key.OemPlus, System.Windows.Input.ModifierKeys.None) &&
                !note.HandleScaleKey(System.Windows.Input.Key.OemPlus, System.Windows.Input.ModifierKeys.Control | System.Windows.Input.ModifierKeys.Alt), "unmodified keys and AltGr do not change scale");
            Check(!note.HandleScaleWheel(120, System.Windows.Input.ModifierKeys.None) && preview.Zoom == 100, "ordinary mouse wheel keeps normal scrolling behavior");
            Check(note.HandleScaleWheel(120, System.Windows.Input.ModifierKeys.Control) && preview.Zoom == 110 &&
                note.HandleScaleWheel(-120, System.Windows.Input.ModifierKeys.Control) && preview.Zoom == 100, "Ctrl wheel zooms in both directions without native double zoom");
            note.SetContentScale(int.MaxValue);
            Check(preview.Zoom == 200 && !note.ContentScaleMenu().Items.OfType<MenuItem>().First().IsEnabled, "maximum scale disables further enlargement");
            var notice = Field<TextBlock>(note, "dailyNotice");
            notice.Text = NoteWindow.DailyWaitingMessage; notice.Visibility = Visibility.Visible;
            var noticeScroll = Field<DockPanel>(note, "reading").Children.OfType<ScrollViewer>().Single(x => ReferenceEquals(x.Content, notice));
            noticeScroll.Measure(new Size(260, 120)); noticeScroll.Arrange(new Rect(0, 0, 260, 120)); noticeScroll.UpdateLayout();
            Check(noticeScroll.ScrollableHeight > 0, "enlarged daily guidance remains scrollable in a small note");
            notice.Visibility = Visibility.Collapsed;
            note.SetContentScale(int.MinValue);
            Check(preview.Zoom == 50 && !note.ContentScaleMenu().Items.OfType<MenuItem>().ElementAt(1).IsEnabled, "minimum scale disables further reduction");
            note.SetContentScale(150);
            var paragraph = preview.Document.Blocks.OfType<System.Windows.Documents.List>().Single().ListItems.FirstListItem.Blocks.OfType<Paragraph>().Single();
            var checkbox = (CheckBox)paragraph.Inlines.OfType<InlineUIContainer>().Single().Child;
            checkbox.IsChecked = true;
            checkbox.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
            Check(File.ReadAllText(path).Contains("- [x] task") && preview.Zoom == 150, "task interaction still writes the correct source line after scaling");
            Call(note, "BeginEdit");
            var editor = Field<TextBox>(note, "editor"); editor.Text = "unsaved draft";
            Check(!note.HandleScaleKey(System.Windows.Input.Key.D0, System.Windows.Input.ModifierKeys.Control) &&
                !note.HandleScaleWheel(120, System.Windows.Input.ModifierKeys.Control) && preview.Zoom == 150, "reading zoom shortcuts do not intercept source editing");
            var before = File.ReadAllBytes(path);
            note.SetContentScale(125);
            Check(editor.Text == "unsaved draft" && Field<bool>(note, "dirty") && editor.Visibility == Visibility.Visible && File.ReadAllBytes(path).SequenceEqual(before), "scale change preserves active draft and does not save source text");
            Call(note, "Reload");
        }
        finally { note.Close(); other.Close(); }
    }

    private static void LinkPreviewTests(string root)
    {
        var path = Path.Combine(root, "preview-link.md");
        const string markdown = "---\ntitle: hidden metadata\n---\n# Journal\nintro\n## Tasks\n- [ ] preview only\n## Log\nother\n";
        File.WriteAllText(path, markdown);
        var before = File.ReadAllBytes(path);
        foreach (var daily in new[] { false, true })
        {
            var dialog = new NoteLinkWindow(daily, () => path);
            var panel = (StackPanel)((ScrollViewer)dialog.Content).Content;
            Check(ReferenceEquals(panel.Children[panel.Children.Count - 1], dialog.Preview), (daily ? "daily" : "fixed") + ": content preview is the final dialog item");
            dialog.LoadSource(path);
            Check(dialog.Preview.Body.IsReadOnly && dialog.Preview.Body.Text == NoteStore.Split(markdown).Body, "blank heading previews whole body without YAML");
            dialog.Headings.Input.Text = "Tasks";
            Check(dialog.Preview.Body.Text == SectionEditor.Find(markdown, "Tasks").Content, "typing an existing heading refreshes the selected section preview");
            dialog.Headings.Input.SelectedItem = "Log";
            Check(dialog.Preview.Body.Text == "other\n", "dropdown selection refreshes the preview");
            dialog.Headings.Input.Text = "New heading";
            Check(dialog.Preview.Body.Text == "" && dialog.Preview.Status.Text.Contains("追加予定") && File.ReadAllBytes(path).SequenceEqual(before), "new heading previews the pending addition without modifying the file");
            dialog.Headings.Input.Text = "Bad\nheading";
            Check(dialog.Preview.Body.Visibility == Visibility.Collapsed && dialog.Preview.Body.Text == "" && dialog.Preview.Status.Text.StartsWith("確認できません:"), "invalid heading clears stale preview and shows an error");
            dialog.Close();
        }
        var preview = new NoteLinkPreview();
        preview.Update(new FileSnapshot(path, new string('a', 3999) + "😀tail", "unused"), "");
        Check(preview.Body.Text.Length == 3999 && preview.Status.Text.Contains("4000"), "link preview limits content without splitting surrogate pairs");
        preview.Update(new FileSnapshot(path, "## Same\na\n## Same\nb\n", "unused"), "Same");
        Check(preview.Body.Visibility == Visibility.Collapsed && preview.Status.Text.Contains("複数"), "ambiguous headings display an error rather than a misleading preview");
        preview.Update(null, "");
        Check(preview.Body.Text == "" && preview.Body.Visibility == Visibility.Collapsed, "failed or cleared source removes previous preview content");

        var note = new NoteWindow(new NotePlacement { Path = path });
        var buttons = note.NoteControls.Children.OfType<Button>().ToArray();
        Check(buttons.Select(b => (string)b.Content).SequenceEqual(new[] { "編集", "保存", "再読込", "…", "＋", "○", "×" }), "all note actions share one horizontal row");
        note.NoteControls.Measure(new Size(278, 100));
        Check(note.NoteControls.DesiredSize.Width <= 278, "action row fits the minimum 280-pixel note width");
        Check(buttons.Take(4).Select(b => b.Background.ToString()).Distinct().Count() == 4 && buttons.Take(4).All(b => b.Background.ToString() != buttons[4].Background.ToString()), "edit/save/reload/menu use distinct backgrounds from other controls");
        note.Close();
    }

    private static void FocusEditingTests(string root)
    {
        var app = App.Current;
        Check(!new Settings().AutoSaveOnFocusLoss && !JsonSerializer.Deserialize<Settings>("{}")!.AutoSaveOnFocusLoss, "focus-loss saving defaults to confirmation for new and old settings");
        Check(JsonSerializer.Deserialize<Settings>(JsonSerializer.Serialize(new Settings { AutoSaveOnFocusLoss = true }))!.AutoSaveOnFocusLoss, "focus-loss autosave setting survives serialization");
        var previousAutoSave = app.Config.AutoSaveOnFocusLoss;
        app.Config.AutoSaveOnFocusLoss = false;
        var folder = Path.Combine(root, "focus-editing"); Directory.CreateDirectory(folder);
        var path = Path.Combine(folder, "note.md");
        const string original = "## Tasks\n- [ ] initial\n\n[link](https://example.com)\n";
        File.WriteAllText(path, original);
        var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        var answer = MessageBoxResult.Cancel;
        var confirmations = 0; var errors = 0;
        NoteWindow? window = null;
        window = new NoteWindow(new NotePlacement { Path = path }, () =>
        {
            confirmations++;
            window!.FinishFocusEditing();
            Check(!window.CanClose(), "focus confirmation blocks a nested close confirmation");
            return answer;
        }, _ => errors++);
        typeof(NoteWindow).GetMethod("Reload", flags)!.Invoke(window, null);
        var dock = (DockPanel)((Border)window.Content).Child;
        var body = dock.Children.OfType<Grid>().Single(x => x.Children.OfType<TextBox>().Any());
        var editor = body.Children.OfType<TextBox>().Single();
        var reading = body.Children.OfType<DockPanel>().Single();
        var preview = reading.Children.OfType<FlowDocumentScrollViewer>().Single();
        void ClickBody() => preview.RaiseEvent(new System.Windows.Input.MouseButtonEventArgs(System.Windows.Input.Mouse.PrimaryDevice, Environment.TickCount, System.Windows.Input.MouseButton.Left) { RoutedEvent = UIElement.PreviewMouseLeftButtonDownEvent });
        var checkbox = (CheckBox)preview.Document.Blocks.OfType<System.Windows.Documents.List>().Single().ListItems.FirstListItem.Blocks.OfType<Paragraph>().Single().Inlines.OfType<InlineUIContainer>().Single().Child;
        Check(!NoteWindow.IsBodyEditTarget(checkbox), "click editing excludes task checkboxes");
        var link = preview.Document.Blocks.OfType<Paragraph>().SelectMany(p => p.Inlines.OfType<Hyperlink>()).Single();
        Check(!NoteWindow.IsBodyEditTarget(link.Inlines.FirstInline) && !NoteWindow.IsBodyEditTarget(new System.Windows.Controls.Primitives.Thumb()), "click editing excludes hyperlink children and scroll controls");
        Check(NoteWindow.IsBodyEditTarget(new Run("text")) && NoteWindow.IsBodyEditTarget(preview), "click editing accepts body text and blank preview space");
        ClickBody();
        Check(editor.Visibility == Visibility.Visible && reading.Visibility == Visibility.Collapsed, "body click enters editing without the Edit button");
        window.FinishFocusEditing();
        Check(confirmations == 0 && reading.Visibility == Visibility.Visible, "unchanged blur returns to reading without a dialog");
        ClickBody(); editor.Text = "draft";
        window.FinishFocusEditing();
        Check(confirmations == 1 && editor.Text == "draft" && editor.Visibility == Visibility.Visible && File.ReadAllText(path) == original, "Cancel keeps unsaved input and never writes");
        answer = MessageBoxResult.No; window.FinishFocusEditing();
        Check(confirmations == 2 && reading.Visibility == Visibility.Visible && File.ReadAllText(path) == original, "No discards draft and reloads the original file");
        ClickBody(); editor.Text = "saved after blur";
        answer = MessageBoxResult.Yes; window.FinishFocusEditing();
        Check(confirmations == 3 && File.ReadAllText(path) == "saved after blur" && reading.Visibility == Visibility.Visible, "Yes saves changed Markdown and returns to reading");
        var beforeAuto = confirmations;
        app.Config.AutoSaveOnFocusLoss = true;
        ClickBody(); editor.Text = "automatic";
        typeof(NoteWindow).GetMethod("ScheduleFocusLoss", flags)!.Invoke(window, null);
        WaitFor(() => File.ReadAllText(path) == "automatic", "queued focus loss automatically saves");
        Check(confirmations == beforeAuto && reading.Visibility == Visibility.Visible, "autosave bypasses confirmation and ends editing");
        ClickBody(); editor.Text = "locked draft";
        using (var locked = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None)) window.FinishFocusEditing();
        Check(errors == 1 && editor.Text == "locked draft" && editor.Visibility == Visibility.Visible && File.ReadAllText(path) == "automatic", "failed autosave to a locked file retains input and original bytes");
        window.FinishFocusEditing();
        Check(File.ReadAllText(path) == "locked draft", "retained draft can save after the file becomes writable");
        ClickBody(); editor.Text = "retained conflict draft";
        File.WriteAllText(path, "external change");
        window.FinishFocusEditing();
        Check(errors == 2 && editor.Text == "retained conflict draft" && editor.Visibility == Visibility.Visible && File.ReadAllText(path) == "external change", "autosave conflict reports error and preserves both external data and draft");
        typeof(NoteWindow).GetMethod("Reload", flags)!.Invoke(window, null);
        ClickBody(); editor.Text = "temporary"; editor.Text = "external change";
        app.Config.AutoSaveOnFocusLoss = false; window.FinishFocusEditing();
        Check(confirmations == beforeAuto, "reverting to original text does not request a save");
        ClickBody(); editor.Text = "explicit save";
        typeof(NoteWindow).GetMethod("ScheduleFocusLoss", flags)!.Invoke(window, null);
        var toolbar = window.NoteControls;
        toolbar.Children.OfType<Button>().Single(x => (string)x.Content == "保存").RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
        var settle = System.Diagnostics.Stopwatch.StartNew();
        WaitFor(() => settle.ElapsedMilliseconds >= 250, "queued focus handling settles after explicit Save");
        Check(confirmations == beforeAuto && File.ReadAllText(path) == "explicit save", "explicit Save cancels the pending blur confirmation");
        window.Close();
        window.FinishFocusEditing();
        Check(confirmations == beforeAuto && errors == 2, "closed note ignores delayed focus handling");

        var savedConfig = File.ReadAllBytes(Path.Combine(App.DataDirectory, "settings.json"));
        var previousPattern = app.Config.DailyPattern;
        app.Config.DailyPattern = DailyNoteResolver.RegexExample;
        var settingsWindow = new SettingsWindow();
        var panel = (StackPanel)((ScrollViewer)settingsWindow.Content).Content;
        var autoSave = panel.Children.OfType<CheckBox>().Single(x => x.Name == "AutoSaveOnFocusLoss");
        Check(autoSave.IsChecked == false, "settings display the confirmation default");
        autoSave.IsChecked = true;
        Check(!app.Config.AutoSaveOnFocusLoss && File.ReadAllBytes(Path.Combine(App.DataDirectory, "settings.json")).SequenceEqual(savedConfig), "autosave preference is not applied before settings save");
        panel.Children.OfType<Button>().Single(x => (string)x.Content == "保存して閉じる").RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
        Check(app.Config.AutoSaveOnFocusLoss && JsonSerializer.Deserialize<Settings>(File.ReadAllText(Path.Combine(App.DataDirectory, "settings.json")))!.AutoSaveOnFocusLoss, "settings Save applies and persists focus-loss autosave");
        var reopened = new SettingsWindow();
        Check(((StackPanel)((ScrollViewer)reopened.Content).Content).Children.OfType<CheckBox>().Single(x => x.Name == "AutoSaveOnFocusLoss").IsChecked == true, "reopened settings retain autosave preference");
        reopened.Close();
        app.Config.AutoSaveOnFocusLoss = previousAutoSave;
        app.Config.DailyPattern = previousPattern;
    }

    private static void MarkdownEditingTests(string root)
    {
        var folder = Path.Combine(root, "markdown-editing");
        Directory.CreateDirectory(folder);
        const string original = "---\r\ntitle: Edit test\r\ncustom: keep\r\n---\r\n# Journal\r\nintro\r\n## Tasks\r\n- [ ] old\r\n## Log\r\nkeep\r\n";
        var previousFolder = App.Current.Config.DailyFolder;
        var previousPattern = App.Current.Config.DailyPattern;
        App.Current.Config.DailyFolder = folder;
        App.Current.Config.DailyPattern = DailyNoteResolver.RegexExample;
        foreach (var mode in new[] { "whole", "section", "daily" })
        {
            var path = Path.Combine(folder, mode == "daily" ? DateTime.Today.ToString("yyyy-MM-dd") + ".md" : mode + ".md");
            File.WriteAllText(path, original, new UTF8Encoding(true));
            var window = new NoteWindow(new NotePlacement { Path = mode == "daily" ? "" : path, Heading = mode == "whole" ? "" : "Tasks", Daily = mode == "daily" });
            var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
            typeof(NoteWindow).GetMethod("Reload", flags)!.Invoke(window, null);
            var dock = (DockPanel)((Border)window.Content).Child;
            var toolbar = window.NoteControls;
            var body = dock.Children.OfType<Grid>().Single(x => x.Children.OfType<TextBox>().Any());
            var reading = body.Children.OfType<DockPanel>().Single();
            var editor = body.Children.OfType<TextBox>().Single();
            void Click(string label) => toolbar.Children.OfType<Button>().Single(x => (string)x.Content == label).RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
            Check(reading.Visibility == Visibility.Visible && editor.Visibility == Visibility.Collapsed, mode + ": opens in reading mode");
            Click("編集");
            Check(editor.Visibility == Visibility.Visible && reading.Visibility == Visibility.Collapsed && !editor.IsReadOnly, mode + ": Edit replaces preview with writable Markdown input");
            var expectedInput = mode == "whole" ? NoteStore.Split(original).Body : SectionEditor.Find(original, "Tasks").Content;
            Check(editor.Text == expectedInput, mode + ": editor loads the displayed Markdown range");
            const string newContent = "- [ ] 日本語の本文を編集\r\n\r\n**Markdown** を追記\r\n";
            editor.SelectAll(); editor.SelectedText = newContent;
            Check(File.ReadAllText(path) == original, mode + ": typing does not save before explicit Save");
            ((Task)typeof(NoteWindow).GetMethod("Tick", flags)!.Invoke(window, null)!).GetAwaiter().GetResult();
            Check(editor.Text == newContent && editor.Visibility == Visibility.Visible && reading.Visibility == Visibility.Collapsed, mode + ": background refresh preserves the active editor and draft");
            Click("編集");
            Check(editor.Text == newContent, mode + ": clicking Edit again retains the unsaved draft");
            Click("保存");
            var expected = mode == "whole" ? original[..(original.Length - NoteStore.Split(original).Body.Length)] + newContent : SectionEditor.Replace(original, "Tasks", newContent);
            Check(NoteStore.Read(path).Text == expected && File.ReadAllBytes(path).Take(3).SequenceEqual(new byte[] { 239, 187, 191 }), mode + ": Save persists text and preserves unrelated content and BOM");
            Check(editor.Visibility == Visibility.Collapsed && reading.Visibility == Visibility.Visible, mode + ": Save restores only the rendered view");
            Click("編集"); Click("保存");
            Check(reading.Visibility == Visibility.Visible && editor.Visibility == Visibility.Collapsed, mode + ": unchanged Save also exits editing");
            var preview = reading.Children.OfType<FlowDocumentScrollViewer>().Single();
            var taskList = preview.Document.Blocks.OfType<System.Windows.Documents.List>().Single();
            var paragraph = taskList.ListItems.FirstListItem.Blocks.OfType<Paragraph>().Single();
            var checkbox = (CheckBox)paragraph.Inlines.OfType<InlineUIContainer>().Single().Child;
            checkbox.IsChecked = true;
            checkbox.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
            Check(NoteStore.Read(path).Text.Contains("- [x] 日本語の本文を編集"), mode + ": checkbox still writes after returning from editing");
            window.Close();
        }
        App.Current.Config.DailyFolder = previousFolder;
        App.Current.Config.DailyPattern = previousPattern;
    }

    private static void NoteLinkTests(string root)
    {
        const string markdown = "---\r\ntitle: Keep\r\n---\r\n# Journal\r\nintro\r\n## Tasks ##\r\n- [ ] one\r\n### Child\r\nsub\r\n```md\r\n## Fake\r\n```\r\n## Log\r\nlast";
        Check(SectionEditor.Headings(markdown).SequenceEqual(new[] { "Journal", "Tasks", "Child", "Log" }), "heading choices share section boundaries and exclude YAML and fenced code");
        Check(SectionEditor.EnsureHeading(markdown, "Tasks") == markdown, "existing heading never changes the source");
        Check(SectionEditor.EnsureHeading(markdown, " ") == markdown, "blank heading displays whole note without adding a section");
        var appended = SectionEditor.EnsureHeading(markdown, "New section");
        Check(appended == markdown + "\r\n\r\n## New section\r\n" && SectionEditor.Find(appended, "New section").Content == "", "missing heading appended at EOF with original CRLF bytes intact");
        Check(SectionEditor.EnsureHeading("", "First") == "## First\n", "empty Markdown accepts its first heading");
        Throws<InvalidOperationException>(() => SectionEditor.EnsureHeading(markdown, "Bad\n## Injected"), "heading rejects newline injection");
        Throws<InvalidOperationException>(() => SectionEditor.EnsureHeading("```\ncode", "Hidden"), "unclosed fence prevents invisible heading append");
        Throws<InvalidOperationException>(() => SectionEditor.EnsureHeading(markdown, "Changed ##"), "heading syntax that alters its name is rejected");
        Throws<InvalidOperationException>(() => SectionEditor.EnsureHeading("## Twice\na\n## Twice\nb", "Twice"), "duplicate headings cannot be linked or appended again");
        var folder = Path.Combine(root, "link-picker");
        var backups = Path.Combine(folder, "backups");
        Directory.CreateDirectory(folder);
        var path = Path.Combine(folder, "fixed.md");
        File.WriteAllText(path, markdown, new UTF8Encoding(true));
        var bytes = File.ReadAllBytes(path);
        var source = NoteStore.Read(path);
        var fixedPlacement = NoteLink.Prepare(source, " Tasks ", false, backups);
        Check(fixedPlacement.Path == path && fixedPlacement.Heading == "Tasks" && !fixedPlacement.Daily, "fixed selection stores an absolute file and trimmed heading");
        var dailyPlacement = NoteLink.Prepare(source, "", true, backups);
        Check(dailyPlacement.Path == "" && dailyPlacement.Heading == "" && dailyPlacement.Daily, "daily whole-note selection has no fixed path");
        Check(File.ReadAllBytes(path).SequenceEqual(bytes) && !Directory.Exists(backups), "selecting existing or blank headings performs no writes or backups");
        NoteLink.Prepare(source, "Added", false, backups);
        Check(NoteStore.Read(path).Text == markdown + "\r\n\r\n## Added\r\n" && File.ReadAllBytes(path).Take(3).SequenceEqual(new byte[] { 239, 187, 191 }), "confirmed new heading preserves source text and BOM");
        Check(Directory.GetFiles(backups).Length == 1 && File.ReadAllBytes(Directory.GetFiles(backups)[0]).SequenceEqual(bytes), "heading append creates an exact recovery backup");
        Throws<ConflictException>(() => NoteLink.Prepare(source, "Stale", false, backups), "stale append cannot overwrite external changes");
        Throws<ConflictException>(() => NoteLink.Prepare(source, "Tasks", false, backups), "stale existing selection requires reloading");
        Check(!File.ReadAllText(path).Contains("Stale"), "failed stale append leaves source intact");

        var fixedDialog = new NoteLinkWindow(false, () => throw new Exception("Fixed dialog must not resolve daily files"));
        var fixedPanel = (StackPanel)((ScrollViewer)fixedDialog.Content).Content;
        Check(fixedPanel.Children.OfType<Button>().Any(x => (string)x.Content == "フォルダを選択…") && fixedPanel.Children.OfType<Button>().Any(x => (string)x.Content == "Markdownファイルを選択…"), "fixed dialog provides folder and Markdown file pickers");
        Check(!fixedPanel.Children.OfType<TextBox>().Any() && !fixedPanel.Children.OfType<CheckBox>().Any(), "fixed dialog removes absolute path input and daily mode controls");
        fixedDialog.LoadSource(path);
        Check(fixedDialog.Headings.Input.Items.Cast<string>().Contains("Tasks"), "opening Markdown populates the shared editable heading choices");
        fixedDialog.Headings.Input.SelectedItem = "Tasks";
        Check(fixedDialog.Prepare().Heading == "Tasks", "choosing a dropdown item links the selected existing heading");
        fixedDialog.Headings.Input.Text = "";
        Check(fixedDialog.Prepare().Heading == "", "fixed dialog blank heading selects the whole body");
        var noteWindow = new NoteWindow(fixedDialog.Prepare());
        var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        typeof(NoteWindow).GetMethod("Reload", flags)!.Invoke(noteWindow, null);
        Check((string)typeof(NoteWindow).GetField("content", flags)!.GetValue(noteWindow)! == NoteStore.Split(NoteStore.Read(path).Text).Body, "whole-note selection renders every body section");
        noteWindow.Close();
        fixedDialog.Headings.Input.Text = "Cancel draft";
        var beforeCancel = File.ReadAllBytes(path);
        fixedDialog.Close();
        Check(File.ReadAllBytes(path).SequenceEqual(beforeCancel), "closing the heading dialog never appends typed names");

        var invalidDialog = new NoteLinkWindow(false, () => "");
        Throws<InvalidOperationException>(() => invalidDialog.LoadSource("relative.md"), "fixed picker rejects relative paths");
        Throws<InvalidOperationException>(() => invalidDialog.LoadSource(Path.Combine(folder, "other.txt")), "fixed picker rejects non-Markdown files");
        invalidDialog.Close();

        var dailyPath = Path.Combine(folder, "today.md");
        File.WriteAllText(dailyPath, "## Daily\ntoday\n");
        var currentPath = dailyPath;
        var dailyDialog = new NoteLinkWindow(true, () => currentPath);
        var dailyPanel = (StackPanel)((ScrollViewer)dailyDialog.Content).Content;
        Check(!dailyPanel.Children.OfType<Button>().Any(x => ((string)x.Content).Contains("選択")) && !dailyPanel.Children.OfType<TextBox>().Any(), "daily dialog has no folder picker or absolute path input");
        dailyDialog.LoadSource();
        Check(dailyDialog.Headings.GetType() == fixedDialog.Headings.GetType() && dailyDialog.Headings.Input.Items.Cast<string>().SequenceEqual(new[] { "Daily" }), "both dialogs use the same heading picker with the loaded note's choices");
        dailyDialog.Headings.Input.Text = "Daily added";
        var result = dailyDialog.Prepare();
        Check(result.Daily && result.Path == "" && File.ReadAllText(dailyPath).EndsWith("## Daily added\n"), "daily dialog appends requested heading to today's file only");
        dailyDialog.LoadSource();
        currentPath = path;
        dailyDialog.Headings.Input.Text = "Wrong day";
        Throws<InvalidOperationException>(() => dailyDialog.Prepare(), "daily date change requires reloading before an append");
        Check(!File.ReadAllText(dailyPath).Contains("Wrong day") && !File.ReadAllText(path).Contains("Wrong day"), "daily rollover cannot append to either stale or unreviewed file");
        currentPath = Path.Combine(folder, "missing.md");
        Throws<FileNotFoundException>(() => dailyDialog.LoadSource(), "missing daily file reports load error");
        Check(!dailyDialog.Headings.IsEnabled && dailyDialog.Headings.Input.Items.Count == 0, "failed read removes stale heading choices");
        Check(dailyDialog.Preview.Body.Text == "" && dailyDialog.Preview.Body.Visibility == Visibility.Collapsed, "failed daily read clears content preview");
        Throws<InvalidOperationException>(() => dailyDialog.Prepare(), "failed read blocks linking stale source");
        dailyDialog.Close();
    }

    private static void RegexDefaultsTests(string root)
    {
        var app = App.Current;
        var previousPattern = app.Config.DailyPattern;
        var previousFolder = app.Config.DailyFolder;
        var savedConfig = File.ReadAllBytes(Path.Combine(App.DataDirectory, "settings.json"));
        var custom = "Diary/" + DailyNoteResolver.RegexExample;
        app.Config.DailyPattern = custom;
        app.Config.DailyFolder = Path.Combine(root, "daily-preview");
        var answer = false;
        var confirmations = 0;
        var window = new SettingsWindow(() => { confirmations++; return answer; });
        var panel = (StackPanel)((ScrollViewer)window.Content).Content;
        var pattern = panel.Children.OfType<TextBox>().ElementAt(2);
        var insert = panel.Children.OfType<Button>().Single(x => (string)x.Content == "日時タグ付きの既定例を挿入");
        void ClickInsert() => insert.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
        var matchStatus = panel.Children.OfType<StackPanel>().Single().Children.OfType<TextBlock>().Single(x => x.Name == "DailyPreviewStatus");
        Check(panel.Children.OfType<CheckBox>().All(x => x.Name is "AutoSaveOnFocusLoss" or "TitleButtonOverlay" or "ShowInTaskbar"), "settings offer only tagged regex without a mode checkbox");
        Check(confirmations == 0 && pattern.Text == custom, "opening settings preserves custom regex without prompting");
        ClickInsert();
        Check(confirmations == 1 && pattern.Text == custom, "declining template button preserves existing expression");
        answer = true; ClickInsert();
        Check(confirmations == 2 && pattern.Text == DailyNoteResolver.RegexExample, "accepting template button inserts named-date regex example");
        WaitFor(() => matchStatus.Text.StartsWith("今日のノートが見つかりました:"), "inserted date tags update the matching filename");
        pattern.Text = "";
        Check(pattern.Text == DailyNoteResolver.RegexExample && confirmations == 2, "clearing regex automatically fills default without another prompt");
        pattern.Text = " \t ";
        Check(pattern.Text == DailyNoteResolver.RegexExample, "whitespace-only regex receives the default");
        pattern.Text = custom;
        Check(pattern.Text == custom, "nonempty custom regex remains unchanged");
        window.Close();
        app.Config.DailyPattern = "";
        var reopened = new SettingsWindow(() => throw new Exception("Opening must not prompt"));
        var reopenedPattern = ((StackPanel)((ScrollViewer)reopened.Content).Content).Children.OfType<TextBox>().ElementAt(2);
        Check(reopenedPattern.Text == DailyNoteResolver.RegexExample && app.Config.DailyPattern == "", "blank setting gets a default in the editor without saving");
        reopened.Close();
        Check(File.ReadAllBytes(Path.Combine(App.DataDirectory, "settings.json")).SequenceEqual(savedConfig), "regex assistance does not persist settings without Save");
        app.Config.DailyPattern = previousPattern; app.Config.DailyFolder = previousFolder;
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
        var result = DailyNotePreview.Read(folder, DailyNoteResolver.RegexExample, today);
        Check(result.Path == file && result.Text == content && !result.Truncated, "daily preview finds today's note and reads UTF8 without BOM");
        Check(File.ReadAllBytes(file).SequenceEqual(bytes), "daily preview never modifies note contents");
        File.WriteAllText(file, new string('x', DailyNotePreview.CharacterLimit) + "tail");
        result = DailyNotePreview.Read(folder, DailyNoteResolver.RegexExample, today);
        Check(result.Text.Length == DailyNotePreview.CharacterLimit && result.Truncated, "preview bounds large note contents");
        File.WriteAllText(file, new string('x', DailyNotePreview.CharacterLimit - 1) + "😀tail");
        result = DailyNotePreview.Read(folder, DailyNoteResolver.RegexExample, today);
        Check(!char.IsSurrogate(result.Text[^1]) && result.Truncated, "preview truncation does not split surrogate pairs");
        using (var cancelled = new CancellationTokenSource())
        {
            cancelled.Cancel();
            Throws<OperationCanceledException>(() => DailyNotePreview.Read(folder, DailyNoteResolver.RegexExample, today, cancelled.Token), "obsolete preview requests can be cancelled");
        }
        File.WriteAllText(file, content);
        var app = App.Current;
        app.ShutdownMode = ShutdownMode.OnExplicitShutdown;
        var config = File.ReadAllBytes(Path.Combine(App.DataDirectory, "settings.json"));
        var originalDaily = app.Config.DailyFolder;
        var window = new SettingsWindow(() => false);
        var panel = (StackPanel)((ScrollViewer)window.Content).Content;
        var inputs = panel.Children.OfType<TextBox>().ToArray();
        var previewPanel = panel.Children.OfType<StackPanel>().Single();
        var status = previewPanel.Children.OfType<TextBlock>().Single(x => x.Name == "DailyPreviewStatus");
        Check(!previewPanel.Children.OfType<TextBox>().Any(), "settings no longer contain the note content preview");
        inputs[1].Text = folder; inputs[2].Text = DailyNoteResolver.RegexExample;
        WaitFor(() => status.Text.StartsWith("今日のノートが見つかりました:"), "typing regex updates the matching filename without saving");
        Check(status.Text.Contains(Path.GetFileName(file)), "settings show the matching filename");
        inputs[2].Text = "[";
        Check(!status.Text.Contains(Path.GetFileName(file)), "new input immediately removes the stale matching filename");
        WaitFor(() => status.Text.StartsWith("確認できません:"), "invalid regex displays an inline error");
        inputs[2].Text = "missing/" + DailyNoteResolver.RegexExample;
        WaitFor(() => status.Text.Contains("今日のデイリーノートがありません"), "no matching file displays an inline error");
        var duplicate = Path.Combine(folder, today.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture) + "(weekday).md");
        File.WriteAllText(duplicate, "duplicate");
        inputs[2].Text = DailyNoteResolver.RegexExample;
        WaitFor(() => status.Text.Contains("複数一致"), "ambiguous preview does not choose a file");
        File.Delete(duplicate);
        inputs[2].Text = DailyNoteResolver.RegexExample + "$";
        WaitFor(() => status.Text.StartsWith("今日のノートが見つかりました:"), "correcting regex refreshes the matching filename");
        inputs[1].Text = Path.Combine(folder, "missing");
        WaitFor(() => status.Text.StartsWith("確認できません:"), "changing daily folder refreshes preview");
        inputs[1].Text = folder; inputs[2].Text = "'missing'"; inputs[2].Text = DailyNoteResolver.RegexExample;
        WaitFor(() => status.Text.StartsWith("今日のノートが見つかりました:") && status.Text.Contains(Path.GetFileName(file)), "rapid edits display only the latest input result");
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

        var settingsWindow = new SettingsWindow(() => false);
        var panel = (StackPanel)((ScrollViewer)settingsWindow.Content).Content;
        var inputs = panel.Children.OfType<TextBox>().ToArray();
        inputs[0].Text = source;
        Check(inputs[1].Text == target, "editing notes input does not change daily input");
        inputs[1].Text = chosenDaily;
        Check(inputs[0].Text == source, "editing daily input does not change notes input");
        settingsWindow.Close();
        app.ApplySettings(new Settings { NotesFolder = source, DailyFolder = target }, false);
        var reopened = new SettingsWindow(() => false);
        var reopenedInputs = ((StackPanel)((ScrollViewer)reopened.Content).Content).Children.OfType<TextBox>().ToArray();
        Check(reopenedInputs[0].Text == source && reopenedInputs[1].Text == target, "reopened settings display independently saved folders");
        reopened.Close();
    }

    private static void DailyWaitingTests(string root)
    {
        var folder = Path.Combine(root, "daily-waiting");
        Directory.CreateDirectory(folder);
        var date = new DateTime(2026, 12, 31);
        string FileFor(DateTime day) => Path.Combine(folder, day.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture) + ".md");
        var yesterday = FileFor(date.AddDays(-1));
        var current = FileFor(date);
        File.WriteAllText(yesterday, "## Tasks\n- [ ] yesterday\n");
        var app = App.Current;
        var previous = app.Config;
        app.ApplySettings(new Settings { NotesFolder = previous.NotesFolder, DailyFolder = folder }, false);
        var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        void Call(NoteWindow window, string method) => typeof(NoteWindow).GetMethod(method, flags)!.Invoke(window, null);
        void Tick(NoteWindow window) => ((Task)typeof(NoteWindow).GetMethod("Tick", flags)!.Invoke(window, null)!).GetAwaiter().GetResult();
        T Field<T>(NoteWindow window, string name) => (T)typeof(NoteWindow).GetField(name, flags)!.GetValue(window)!;
        NoteWindow Open(DailyNoteRetention mode)
        {
            app.Config.DailyRetention = mode;
            var window = new NoteWindow(new NotePlacement { Daily = true, Heading = "Tasks" }, () => MessageBoxResult.Cancel, _ => { }, () => date);
            Call(window, "Reload");
            return window;
        }
        try
        {
            var window = Open(DailyNoteRetention.ShowWaitingMessage);
            Check(Field<FileSnapshot?>(window, "snapshot") is null && Field<TextBlock>(window, "dailyNotice").Text.Contains("Obsidian") && Field<TextBlock>(window, "dailyNotice").Visibility == Visibility.Visible, "missing daily note displays creation and settings guidance in the sticky body");
            Call(window, "BeginEdit");
            Check(!Field<bool>(window, "editing") && !File.Exists(current), "waiting message cannot be edited or saved as a note");
            File.WriteAllText(current, "## Tasks\ntoday\n"); Tick(window);
            Check(Field<FileSnapshot>(window, "snapshot").Path == current && Field<TextBlock>(window, "dailyNotice").Visibility == Visibility.Collapsed, "poll automatically displays newly created daily note");
            window.Close(); File.Delete(current);

            window = Open(DailyNoteRetention.UntilCreated);
            Check(Field<FileSnapshot>(window, "snapshot").Path == yesterday && Field<TextBlock>(window, "dailyNotice").Text.Contains("昨日"), "until-created mode loads yesterday on startup with a visible date notice");
            File.WriteAllText(current, "## Tasks\ntoday\n"); Tick(window);
            Check(Field<FileSnapshot>(window, "snapshot").Path == current, "until-created mode switches automatically when today appears");
            date = date.AddDays(1); Tick(window);
            Check(Field<FileSnapshot>(window, "snapshot").Path == current && Field<TextBlock>(window, "title").Text.StartsWith("昨日"), "year rollover retains exactly yesterday and relabels it");
            Call(window, "BeginEdit"); Field<TextBox>(window, "editor").Text = "unsaved previous-day draft";
            File.WriteAllText(FileFor(date), "## Tasks\nnew year\n"); Tick(window);
            Check(Field<TextBox>(window, "editor").Text == "unsaved previous-day draft" && Field<FileSnapshot>(window, "snapshot").Path == current, "creation during editing preserves previous-day snapshot and draft");
            Call(window, "Save");
            Check(File.ReadAllText(current).Contains("unsaved previous-day draft") && File.ReadAllText(FileFor(date)).Contains("new year"), "retained edit saves only to its original dated file");
            Tick(window);
            Check(Field<FileSnapshot>(window, "snapshot").Path == FileFor(date), "automatic switching resumes after saving draft");
            window.Close(); File.Delete(FileFor(date)); date = date.AddDays(-1); File.Delete(current);

            window = Open(DailyNoteRetention.UntilRefresh);
            File.WriteAllText(current, "## Tasks\ntoday\n"); Tick(window);
            Check(Field<FileSnapshot>(window, "snapshot").Path == yesterday, "until-refresh mode keeps yesterday after today's creation");
            Call(window, "ReloadAsked");
            Check(Field<FileSnapshot>(window, "snapshot").Path == current, "manual refresh switches to today's existing file");
            window.Close(); File.Delete(current);
            window = Open(DailyNoteRetention.UntilRefresh);
            Call(window, "ReloadAsked"); Tick(window);
            Check(Field<FileSnapshot?>(window, "snapshot") is null && Field<TextBlock>(window, "dailyNotice").Text.Contains("Obsidian"), "refresh without today ends retention and remains waiting across polls");
            File.WriteAllText(current, "## Tasks\ntoday\n"); Tick(window);
            Check(Field<FileSnapshot>(window, "snapshot").Path == current, "creation after manual refresh automatically leaves waiting state");
            window.Close(); File.Delete(current);
            window = Open(DailyNoteRetention.UntilRefresh);
            date = date.AddDays(1); Tick(window);
            Check(Field<FileSnapshot?>(window, "snapshot") is null, "retention never keeps a file older than yesterday");
            window.Close(); date = date.AddDays(-1);

            window = Open(DailyNoteRetention.UntilCreated);
            var duplicate = Path.Combine(folder, "2026-12-31(duplicate).md");
            File.WriteAllText(current, "## Tasks\ntoday\n"); File.WriteAllText(duplicate, "## Tasks\nduplicate\n"); Tick(window);
            Check(Field<FileSnapshot?>(window, "snapshot") is null && Field<TextBlock>(window, "status").Text.Contains("複数一致") && Field<TextBlock>(window, "dailyNotice").Visibility == Visibility.Collapsed, "ambiguous matches report an error instead of retaining yesterday or showing creation advice");
            File.Delete(duplicate); Tick(window);
            Check(Field<FileSnapshot>(window, "snapshot").Path == current, "poll recovers after ambiguous match is removed");
            File.WriteAllText(current, "## Other\nmissing requested heading\n"); Tick(window);
            Check(Field<FileSnapshot?>(window, "snapshot") is null && Field<TextBlock>(window, "dailyNotice").Visibility == Visibility.Collapsed, "missing heading is an error rather than a fallback or creation message");
            window.Close();

            File.Delete(current);
            window = Open(DailyNoteRetention.UntilRefresh);
            File.Delete(yesterday); File.WriteAllText(current, "## Tasks\ntoday\n"); Tick(window);
            Check(Field<FileSnapshot>(window, "snapshot").Path == current, "deleted retained note allows recovery to today's existing note");
            window.Close();
            File.Delete(current);
            window = Open(DailyNoteRetention.UntilCreated);
            Check(Field<FileSnapshot?>(window, "snapshot") is null && Field<TextBlock>(window, "dailyNotice").Visibility == Visibility.Visible, "retention mode waits when both days are missing");
            app.Config.DailyPattern = "["; Tick(window);
            Check(Field<TextBlock>(window, "dailyNotice").Visibility == Visibility.Collapsed && Field<TextBlock>(window, "status").Text.Contains("正規表現"), "invalid regex is not disguised as a missing daily note");
            app.Config.DailyPattern = DailyNoteResolver.RegexExample;
            app.Config.DailyFolder = Path.Combine(folder, "missing-folder"); Tick(window);
            Check(Field<TextBlock>(window, "dailyNotice").Visibility == Visibility.Collapsed, "missing folder is not disguised as a note awaiting creation");
            app.Config.DailyFolder = folder;
            window.Close();

            var settings = new SettingsWindow();
            var panel = (StackPanel)((ScrollViewer)settings.Content).Content;
            var choice = panel.Children.OfType<ComboBox>().Single(x => x.Name == "DailyRetention");
            choice.SelectedIndex = (int)DailyNoteRetention.UntilRefresh;
            Check(app.Config.DailyRetention == DailyNoteRetention.UntilCreated, "retention selection is not applied before saving");
            panel.Children.OfType<Button>().Single(x => (string)x.Content == "保存して閉じる").RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
            Check(JsonSerializer.Deserialize<Settings>(File.ReadAllText(Path.Combine(App.DataDirectory, "settings.json")))!.DailyRetention == DailyNoteRetention.UntilRefresh, "settings persist retention mode for restart");
            settings = new SettingsWindow();
            Check(((StackPanel)((ScrollViewer)settings.Content).Content).Children.OfType<ComboBox>().Single().SelectedIndex == 2, "reopened settings show saved retention mode");
            settings.Close();
            Check(JsonSerializer.Deserialize<Settings>("{}")!.DailyRetention == DailyNoteRetention.ShowWaitingMessage, "older settings default to waiting message");
        }
        finally { app.ApplySettings(previous, false); }
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
        Check(DailyNoteResolver.Resolve(folder, DailyNoteResolver.RegexExample, today) == current, "regex selects today's Japanese weekday filename and ignores prefixes");
        Check(DailyNoteResolver.Resolve(folder, DailyNoteResolver.RegexExample, today.AddDays(-1)).EndsWith("2026-10-04(日).md"), "regex rolls over using requested date");
        Check(new Settings().DailyPattern == DailyNoteResolver.RegexExample, "new settings default to tagged regex");
        var settings = new Settings { DailyPattern = "Diary/" + DailyNoteResolver.RegexExample };
        Check(JsonSerializer.Deserialize<Settings>(JsonSerializer.Serialize(settings))!.DailyPattern == settings.DailyPattern, "custom tagged regex round trips unchanged");
        Check(!JsonSerializer.Serialize(settings).Contains("DailyPatternIsRegex"), "saved settings no longer contain a format mode flag");
        var legacy = JsonSerializer.Deserialize<Settings>("{\"DailyPattern\":\"yyyy-MM-dd(ddd)\",\"DailyPatternIsRegex\":false}")!;
        Check(DailyNoteResolver.Resolve(folder, legacy.DailyPattern, today) == current, "legacy weekday format converts to tagged regex on load");
        var customOldJson = JsonSerializer.Serialize(new { DailyPattern = settings.DailyPattern, DailyPatternIsRegex = true });
        Check(JsonSerializer.Deserialize<Settings>(customOldJson)!.DailyPattern == settings.DailyPattern, "previous regex mode preserves custom expressions");
        var unknown = JsonSerializer.Deserialize<Settings>("{\"DailyPattern\":\"yyyy_MM_dd\"}")!;
        Check(unknown.DailyPattern == "yyyy_MM_dd", "unsupported legacy formats are preserved for manual correction");
        Throws<InvalidOperationException>(() => DailyNoteResolver.Validate(unknown.DailyPattern), "unsupported legacy format is not executed as a date format");
        Check(JsonSerializer.Deserialize<Settings>("{\"DailyPattern\":\"\"}")!.DailyPattern == DailyNoteResolver.RegexExample, "empty saved pattern receives tagged default");
        var plainLegacy = JsonSerializer.Deserialize<Settings>("{\"DailyPattern\":\"yyyy-MM-dd\"}")!;
        Throws<FileNotFoundException>(() => DailyNoteResolver.Resolve(folder, plainLegacy.DailyPattern, today), "legacy plain format conversion does not broaden to weekday suffixes");
        Directory.CreateDirectory(Path.Combine(folder, "2026", "10"));
        var nestedLegacyFile = Path.Combine(folder, "2026", "10", "2026-10-05.md");
        File.WriteAllText(nestedLegacyFile, "nested legacy note");
        var nestedLegacy = JsonSerializer.Deserialize<Settings>("{\"DailyPattern\":\"yyyy/MM/yyyy-MM-dd\"}")!;
        Check(DailyNoteResolver.Resolve(folder, nestedLegacy.DailyPattern, today) == nestedLegacyFile, "legacy nested format converts with matching directory date tags");
        Directory.CreateDirectory(Path.Combine(folder, "2025", "10"));
        File.WriteAllText(Path.Combine(folder, "2025", "10", "2026-10-05.md"), "wrong directory year");
        Check(DailyNoteResolver.Resolve(folder, nestedLegacy.DailyPattern, today) == nestedLegacyFile, "converted nested regex rejects mismatched directory dates");
        var legacyCases = new[]
        {
            ("yyyy-MM-dd(dddd)", "2026-10-05(月).md"),
            ("yyyy/MM/yyyy-MM-dd(ddd)", "2026/10/2026-10-05(月).md"),
            ("yyyy/MM/yyyy-MM-dd(dddd)", "2026/10/2026-10-05(月).md")
        };
        foreach (var (oldPattern, relativePath) in legacyCases)
        {
            var path = Path.Combine(folder, relativePath.Replace('/', Path.DirectorySeparatorChar));
            File.WriteAllText(path, "weekday import");
            var imported = JsonSerializer.Deserialize<Settings>(JsonSerializer.Serialize(new { DailyPattern = oldPattern }))!;
            Check(DailyNoteResolver.Resolve(folder, imported.DailyPattern, today) == path, "legacy weekday import: " + oldPattern);
        }
        Throws<InvalidOperationException>(() => DailyNoteResolver.Validate("["), "invalid regex rejected at settings validation");
        Throws<InvalidOperationException>(() => DailyNoteResolver.Validate(@"\d{4}-\d{2}-\d{2}\.md"), "regex without date groups rejected");
        Throws<InvalidOperationException>(() => DailyNoteResolver.Validate(""), "blank regex rejected");
        Throws<InvalidOperationException>(() => DailyNoteResolver.Validate(new string('a', 4097)), "excessive regex length rejected");
        Throws<InvalidOperationException>(() => DailyNoteResolver.Resolve(folder, "yyyy-MM-dd", today), "runtime rejects date-format syntax without date tags");
        Throws<FileNotFoundException>(() => DailyNoteResolver.Resolve(folder, DailyNoteResolver.RegexExample, today.AddDays(1)), "missing date does not fall back to another note");
        File.WriteAllText(Path.Combine(folder, "2026-10-05.md"), "duplicate");
        Throws<IOException>(() => DailyNoteResolver.Resolve(folder, DailyNoteResolver.RegexExample, today), "ambiguous same-date matches fail safely");
        File.Delete(Path.Combine(folder, "2026-10-05.md"));
        Directory.CreateDirectory(Path.Combine(folder, "Diary"));
        var nested = Path.Combine(folder, "Diary", "2026-10-05(月).md");
        File.WriteAllText(nested, "nested");
        Check(DailyNoteResolver.Resolve(folder, "Diary/" + DailyNoteResolver.RegexExample, today) == nested, "regex uses slash-separated full relative path including extension");
        File.WriteAllText(Path.Combine(folder, "2026-10-05" + new string('a', 80) + "!.md"), "timeout fixture");
        Throws<IOException>(() => DailyNoteResolver.Resolve(folder, @"(?<year>\d{4})-(?<month>\d{2})-(?<day>\d{2})(a+)+\.md", today), "pathological regex times out instead of blocking indefinitely");

        var app = App.Current;
        app.Config.DailyFolder = folder; app.Config.DailyPattern = DailyNoteResolver.RegexExample;
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
