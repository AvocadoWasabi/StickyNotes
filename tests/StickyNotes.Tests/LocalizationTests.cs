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
            L10n.ResolveLanguage(null, CultureInfo.GetCultureInfo("zh-TW")) == "zh-CN" &&
            L10n.ResolveLanguage("invalid", CultureInfo.GetCultureInfo("fr-FR")) == "en" &&
            L10n.ResolveLanguage("en", CultureInfo.GetCultureInfo("ja-JP")) == "en",
            "language resolution supports Windows defaults, unsupported languages and explicit overrides");
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
            App.Current.ApplySettings(new Settings { NotesFolder = Path.Combine(root, "localized-notes") }, false);
            App.Current.Resources.MergedDictionaries.Add(new ResourceDictionary {
                Source = new Uri("/StickyNotes;component/AppStyles.xaml", UriKind.Relative) });
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ar-SA");
            foreach (var code in new[] { "ja", "en", "zh-CN" })
            {
                L10n.Initialize(code);
                var set = manager.GetResourceSet(code == "ja" ? CultureInfo.InvariantCulture : CultureInfo.GetCultureInfo(code), true, false)!;
                var values = set.Cast<DictionaryEntry>().ToDictionary(e => (string)e.Key, e => (string)e.Value!);
                Check(values.Keys.Order().SequenceEqual(neutral.Keys.Order()) && values.Values.All(v => !string.IsNullOrWhiteSpace(v)),
                    code + ": complete compiled translation catalog without fallback");
                Check(values.All(p => CompositeFormat.Parse(p.Value).MinimumArgumentCount == CompositeFormat.Parse(neutral[p.Key]).MinimumArgumentCount),
                    code + ": formatting placeholders match the source catalog");
                Check(L10n.Format("NoteWindow.Text21", new DateTime(2026, 10, 6)).Contains("2026-10-06"),
                    code + ": date formats remain stable under non-Gregorian regional settings");
                var created = NoteStore.Read(NoteStore.Create(Path.Combine(root, "localized-notes")));
                Check(created.Text.Contains(code switch { "en" => "# New note", "zh-CN" => "# 新便签", _ => "# 新しい付箋" }) &&
                    NoteStore.Metadata(created.Text).Tags.SequenceEqual(new[] { "sticky" }), code + ": new note uses localized template and stable metadata");
                var metadata = NoteStore.Metadata(NoteStore.Read(preserved).Text);
                Check(metadata.Title == "My 日本語标题" && metadata.Tags.SequenceEqual(new[] { "仕事", "项目" }) && File.ReadAllBytes(preserved).SequenceEqual(before),
                    code + ": existing title, tags, headings and body are preserved byte for byte");
                using var menu = AppCommands.CreateTrayMenu(_ => { });
                Check(menu.Items[0].Text == (code switch { "en" => "New note", "zh-CN" => "新便签", _ => "新しい付箋" }),
                    code + ": tray commands reflect the initialized language, not a cached startup language");
                Check(CalendarCompletion.Usage.Contains(code switch { "en" => "keyword", "zh-CN" => "关键词", _ => "キーワード" }),
                    code + ": calendar completion explains keyword-free search");
                var settings = new SettingsWindow();
                var general = SettingsPanel(settings);
                var cliPanel = general.Children.OfType<Expander>().Single(p => (string)p.Header == "Obsidian Tasks — CLI Preview");
                Check(((StackPanel)cliPanel.Content).Children.OfType<CheckBox>().Single().IsChecked == false,
                    code + ": CLI integration settings are visible and opt-in in the standard build");
                var selector = general.Children.OfType<StackPanel>().SelectMany(p => p.Children.OfType<ComboBox>()).Single(c => c.Name == "DisplayLanguage");
                var layout = (Grid)settings.Content;
                layout.Measure(new Size(1100, 700)); layout.Arrange(new Rect(0, 0, 1100, 700)); layout.UpdateLayout();
                Check(layout.Children.OfType<ScrollViewer>().All(p => p.ScrollableWidth == 0), code + ": settings panes fit normal width");
                RenderLocalizationPreview(layout, code + "-settings");
                SettingsPanel(settings, true).Children.OfType<Expander>().Single().IsExpanded = true;
                layout.UpdateLayout();
                RenderLocalizationPreview(layout, code + "-guide");
                var expected = code == "en" ? "zh-CN" : "en";
                selector.SelectedIndex = expected == "en" ? 2 : 3;
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
                note.Close();
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
