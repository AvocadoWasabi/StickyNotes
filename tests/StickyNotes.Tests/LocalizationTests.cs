using System.Collections;
using System.Globalization;
using System.IO;
using System.Resources;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using StickyNotes;
using StickyNotes.Core;

internal static partial class Program
{
    private static void LocalizationTests(string root)
    {
        Check(L10n.ResolveLanguage("", CultureInfo.GetCultureInfo("ja-JP")) == "ja" &&
            L10n.ResolveLanguage(null, CultureInfo.GetCultureInfo("zh-TW")) == "zh-TW" &&
            L10n.ResolveLanguage("invalid", CultureInfo.GetCultureInfo("fr-FR")) == "en" &&
            L10n.ResolveLanguage("en", CultureInfo.GetCultureInfo("ja-JP")) == "en",
            "language resolution supports Windows defaults, unsupported languages and explicit overrides");
        foreach (var (windows, expected) in new[] {
            ("zh-Hant", "zh-TW"), ("zh-HK", "zh-TW"), ("zh-MO", "zh-TW"),
            ("zh-Hans", "zh-CN"), ("zh-SG", "zh-CN"), ("zh-CN", "zh-CN"),
            ("ko-KR", "ko"), ("de-AT", "de"), ("ru-RU", "ru"), ("es-MX", "es"), ("sv-FI", "sv") })
            Check(L10n.ResolveLanguage(null, CultureInfo.GetCultureInfo(windows)) == expected,
                windows + ": Windows region/script resolves to the supported UI language");
        Check(L10n.ResolveLanguage("zh-CN", CultureInfo.GetCultureInfo("zh-TW")) == "zh-CN",
            "existing explicit Simplified Chinese preference remains unchanged on Taiwan Windows");
        var expectedLabels = new Dictionary<string, (string NewNote, string Keyword)> {
            ["ja"] = ("新しい付箋", "キーワード"), ["en"] = ("New note", "keyword"),
            ["zh-CN"] = ("新便签", "关键词"), ["zh-TW"] = ("新便利貼", "關鍵字"),
            ["ko"] = ("새 메모", "검색어"), ["de"] = ("Neue Notiz", "Suchwort"),
            ["ru"] = ("Новая заметка", "поисковое"), ["es"] = ("Nueva nota", "palabra"),
            ["sv"] = ("Ny anteckning", "sökord")
        };
        Check(L10n.SupportedLanguages.Select(l => l.Code).Order().SequenceEqual(expectedLabels.Keys.Order()),
            "language catalog contains exactly the nine supported languages");
        Check(JsonSerializer.Deserialize<Settings>("{}")!.Language == "", "old settings default to Windows language");
        var manager = new ResourceManager("StickyNotes.Core.Strings", typeof(L10n).Assembly);
        var neutral = manager.GetResourceSet(CultureInfo.InvariantCulture, true, false)!
            .Cast<DictionaryEntry>().ToDictionary(e => (string)e.Key, e => (string)e.Value!);
        var previous = App.Current.Config;
        var previousCulture = CultureInfo.CurrentCulture;
        var preserved = Path.Combine(root, "language-preserved.md");
        const string original = "---\ntitle: My 日本語标题\ntags: [仕事, 项目]\n---\n## 元の見出し\n- [ ] 原文 {0}\n";
        File.WriteAllText(preserved, original);
        var before = File.ReadAllBytes(preserved);
        try
        {
            App.Current.ApplySettings(new Settings { ObsidianTasksEnabled = false, NotesFolder = Path.Combine(root, "localized-notes") }, false);
            App.Current.Resources.MergedDictionaries.Add(new ResourceDictionary {
                Source = new Uri("/StickyNotes;component/AppStyles.xaml", UriKind.Relative) });
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ar-SA");
            foreach (var code in expectedLabels.Keys)
            {
                L10n.Initialize(code);
                var updateMessage = L10n.Format("TasksPreview.UpdateRequired", "7.23.1");
                string? actualUpdateMessage = null;
                try { ObsidianTasksClient.ParseEnvelope("STICKY_TASKS_PREVIEW:{\"error\":\"STICKY_TASKS_UPDATE_REQUIRED:7.23.1\"}"); }
                catch (InvalidOperationException error) { actualUpdateMessage = error.Message; }
                Check(actualUpdateMessage == updateMessage && updateMessage.Contains("8.2.2") &&
                    updateMessage.Contains(L10n.Text("QueryPreview.Refresh")), code + ": Tasks update error includes installed/required versions and actual refresh menu label");
                var unsupported = ObsidianTasksClient.ParseResponse("STICKY_TASKS_PREVIEW:{\"version\":\"8.2.2\",\"results\":[{\"error\":\"STICKY_TASKS_UNSUPPORTED_API\"}]}", 1);
                Check(unsupported.Results[0].Error == L10n.Text("TasksPreview.UnsupportedApi"), code + ": Tasks per-block API error is localized");
                var set = manager.GetResourceSet(code == "ja" ? CultureInfo.InvariantCulture : CultureInfo.GetCultureInfo(code), true, false)!;
                var values = set.Cast<DictionaryEntry>().ToDictionary(e => (string)e.Key, e => (string)e.Value!);
                Check(values.Keys.Order().SequenceEqual(neutral.Keys.Order()) && values.Values.All(v => !string.IsNullOrWhiteSpace(v)),
                    code + ": complete compiled translation catalog without fallback");
                Check(values.All(p => CompositeFormat.Parse(p.Value).MinimumArgumentCount == CompositeFormat.Parse(neutral[p.Key]).MinimumArgumentCount),
                    code + ": formatting placeholders match the source catalog");
                Check(values.All(p => System.Text.RegularExpressions.Regex.Matches(p.Value, @"\{\d+(?:[^{}]*)\}")
                    .Select(m => m.Value).Order().SequenceEqual(System.Text.RegularExpressions.Regex.Matches(neutral[p.Key], @"\{\d+(?:[^{}]*)\}")
                    .Select(m => m.Value).Order())), code + ": placeholder indices and format specifiers are preserved");
                Check(L10n.Format("NoteWindow.Text21", new DateTime(2026, 10, 6)).Contains("2026-10-06"),
                    code + ": date formats remain stable under non-Gregorian regional settings");
                var created = NoteStore.Read(NoteStore.Create(Path.Combine(root, "localized-notes")));
                Check(created.Text.Contains("# " + expectedLabels[code].NewNote) &&
                    NoteStore.Metadata(created.Text).Tags.SequenceEqual(new[] { "sticky" }), code + ": new note uses localized template and stable metadata");
                var metadata = NoteStore.Metadata(NoteStore.Read(preserved).Text);
                Check(metadata.Title == "My 日本語标题" && metadata.Tags.SequenceEqual(new[] { "仕事", "项目" }) && File.ReadAllBytes(preserved).SequenceEqual(before),
                    code + ": existing title, tags, headings and body are preserved byte for byte");
                using var menu = AppCommands.CreateTrayMenu(_ => { });
                Check(menu.Items[0].Text == expectedLabels[code].NewNote,
                    code + ": tray commands reflect the initialized language, not a cached startup language");
                Check(CalendarCompletion.Usage.Contains(expectedLabels[code].Keyword),
                    code + ": calendar completion explains keyword-free search");
                Check(L10n.Text("HeadingFilter.Missing").Contains(L10n.Text("HeadingFilter.Short")) &&
                    L10n.Text("HeadingFilter.Missing").Contains(L10n.Text("HeadingFilter.Clear")),
                    code + ": missing-heading guidance names the actual toolbar and reset controls");
                var settings = new SettingsWindow();
                var general = SettingsPanel(settings);
                var cliPanel = general.Children.OfType<Expander>().Single(p => (string)p.Header == "Obsidian CLI");
                Check(((StackPanel)cliPanel.Content).Children.OfType<CheckBox>().Single().IsChecked == false,
                    code + ": CLI integration settings preserve the explicitly selected local mode");
                var selector = general.Children.OfType<StackPanel>().SelectMany(p => p.Children.OfType<ComboBox>()).Single(c => c.Name == "DisplayLanguage");
                Check(selector.Items.Count == 10 && L10n.SupportedLanguages.All(l => selector.Items.Contains(l.Name)),
                    code + ": settings lists all native language names and automatic selection");
                var layout = (Grid)settings.Content;
                layout.Measure(new Size(1100, 700)); layout.Arrange(new Rect(0, 0, 1100, 700)); layout.UpdateLayout();
                var panes = LogicalDescendants(settings.Categories).OfType<ScrollViewer>().ToArray();
                Check(panes.Length >= 3 && panes.All(p => p.ScrollableWidth == 0), code + ": settings categories fit normal width");
                RenderLocalizationPreview(layout, code + "-settings");
                settings.Categories.SelectedIndex = 2;
                SettingsPanel(settings, true).Children.OfType<Expander>().Single().IsExpanded = true;
                layout.UpdateLayout();
                RenderLocalizationPreview(layout, code + "-guide");
                var nextIndex = (Array.IndexOf(L10n.SupportedLanguages.Select(l => l.Code).ToArray(), code) + 1) % L10n.SupportedLanguages.Count;
                var expected = L10n.SupportedLanguages[nextIndex].Code;
                selector.SelectedIndex = nextIndex + 1;
                general.Children.OfType<Button>().Single(b => (string)b.Content == L10n.Text("SettingsWindow.Text68"))
                    .RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
                Check(App.Current.Config.Language == expected && L10n.Language == code &&
                    JsonSerializer.Deserialize<Settings>(File.ReadAllText(Path.Combine(App.DataDirectory, "settings.json")))!.Language == expected,
                    code + ": UI saves language preference while deferring its application until restart");
                var reopened = new SettingsWindow();
                Check(SettingsPanel(reopened).Children.OfType<StackPanel>().SelectMany(p => p.Children.OfType<ComboBox>()).Single(c => c.Name == "DisplayLanguage").SelectedIndex == selector.SelectedIndex,
                    code + ": reopening settings retains saved language preference");
                reopened.Close();
                var note = new NoteWindow(new NotePlacement { Path = created.Path });
                typeof(NoteWindow).GetMethod("Reload", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.Invoke(note, null);
                var content = (FrameworkElement)note.Content;
                content.Measure(new Size(360, 400)); content.Arrange(new Rect(0, 0, 360, 400)); content.UpdateLayout();
                RenderLocalizationPreview(content, code + "-note");
                typeof(NoteWindow).GetMethod("Menu", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.Invoke(note, null);
                var noteMenu = note.ToolbarMenu!;
                noteMenu.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                noteMenu.Arrange(new Rect(noteMenu.DesiredSize)); noteMenu.UpdateLayout();
                RenderLocalizationPreview(noteMenu, code + "-note-menu");
                var display = noteMenu.Items.OfType<MenuItem>().Single(m => Equals(m.Header, L10n.Text("NoteMenu.Display")));
                var details = display.Items.OfType<MenuItem>().Single(m => Equals(m.Header, L10n.Text("NoteWindow.Details")));
                var tags = (FrameworkElement)typeof(NoteWindow).GetField("tags", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(note)!;
                var wasVisible = tags.Visibility == Visibility.Visible;
                Check(details.IsChecked == wasVisible, code + ": display submenu reflects metadata visibility");
                details.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
                Check((tags.Visibility == Visibility.Visible) != wasVisible, code + ": nested display action changes visibility exactly once");
                noteMenu.IsOpen = false;
                note.Close();
                var browser = new NoteBrowserWindow(new NoteSource(App.Current.Config, false), _ => throw new Exception("Close must not open a note"));
                var browserContent = (FrameworkElement)browser.Content;
                browserContent.Measure(new Size(568, 420)); browserContent.Arrange(new Rect(0, 0, 568, 420)); browserContent.UpdateLayout();
                RenderLocalizationPreview(browserContent, code + "-browser-compact");
                var closeBounds = browser.CloseButton.TransformToAncestor(browserContent).TransformBounds(new Rect(browser.CloseButton.RenderSize));
                Check(closeBounds.Right <= 568 && closeBounds.Bottom <= 420 && browser.CloseButton.ActualWidth >= 100,
                    code + ": manager Close stays inside the compact window");
                var browserClosed = false; browser.Closed += (_, _) => browserClosed = true;
                browser.CloseButton.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
                Check(browserClosed && browser.CloseButton.IsCancel && File.ReadAllBytes(preserved).SequenceEqual(before),
                    code + ": Close dismisses manager without opening or modifying notes");
            }
        }
        finally
        {
            L10n.Initialize("ja");
            CultureInfo.CurrentCulture = previousCulture;
            App.Current.ApplySettings(previous, false);
        }
    }

    private static void RenderLocalizationPreview(FrameworkElement content, string name)
    {
        var directory = Environment.GetEnvironmentVariable("STICKYNOTES_RENDER_DIRECTORY");
        if (string.IsNullOrEmpty(directory)) return;
        Directory.CreateDirectory(directory);
        var bitmap = new RenderTargetBitmap((int)content.ActualWidth, (int)content.ActualHeight, 96, 96, PixelFormats.Pbgra32);
        var drawing = new DrawingVisual();
        using (var context = drawing.RenderOpen())
        {
            var bounds = new Rect(0, 0, content.ActualWidth, content.ActualHeight);
            context.DrawRectangle(Brushes.White, null, bounds);
            context.DrawRectangle(new VisualBrush(content), null, bounds);
        }
        bitmap.Render(drawing);
        var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var output = File.Create(Path.Combine(directory, name + ".png"));
        encoder.Save(output);
    }
}
