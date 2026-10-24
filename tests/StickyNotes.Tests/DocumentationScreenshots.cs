using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using StickyNotes;
using StickyNotes.Core;

internal static partial class Program
{
    // Opt-in documentation renderer: only fictional fixtures, no real app startup or CLI.
    private static void RenderDocumentationScreenshots(string samples, string output)
    {
        samples = Path.GetFullPath(samples);
        output = Path.GetFullPath(output);
        Directory.CreateDirectory(output);
        var root = Path.Combine(Path.GetTempPath(), "StickyNotes.Screenshots-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var app = new TestApp { ShutdownMode = ShutdownMode.OnExplicitShutdown };
        typeof(App).GetProperty(nameof(App.DataDirectory))!.SetValue(null, root);
        app.Config.ObsidianTasksEnabled = false;
        app.Resources.MergedDictionaries.Add(new ResourceDictionary {
            Source = new Uri("/StickyNotes;component/AppStyles.xaml", UriKind.Relative) });
        try
        {
            foreach (var language in new[] { "ja", "en", "zh-CN", "zh-TW", "ko" })
            {
                // Traditional Chinese and Korean are sample-text translations, not new UI locales.
                L10n.Initialize(language is "zh-TW" or "ko" ? "en" : language);
                var source = language == "ja" ? samples : Path.Combine(samples, language);
                var notes = Path.Combine(root, language);
                Directory.CreateDirectory(notes);
                foreach (var file in new[] { "Tasks.md", "Project.md", "Daily.md" })
                    File.Copy(Path.Combine(source, file), Path.Combine(notes, file));
                File.Copy(Path.Combine(notes, "Daily.md"), Path.Combine(notes, DateTime.Today.ToString("yyyy-MM-dd") + ".md"));
                app.Config.NotesFolder = notes;
                app.Config.DailyFolder = notes;
                app.Config.DailyPattern = @"(?<year>\d{4})-(?<month>\d{2})-(?<day>\d{2})\.md";
                foreach (var (file, name, color) in new[] {
                    ("Tasks.md", "tasks", "yellow"), ("Project.md", "markdown", "blue"), ("Daily.md", "daily", "green") })
                {
                    var window = new NoteWindow(new NotePlacement {
                        Path = name == "daily" ? "" : Path.Combine(notes, file),
                        Daily = name == "daily", Heading = name == "daily" ? "Tasks" : "",
                        Color = color, Width = 460, Height = 580
                    });
                    try
                    {
                        typeof(NoteWindow).GetMethod("Reload", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(window, null);
                        var content = (FrameworkElement)window.Content;
                        content.Measure(new Size(460, 580));
                        content.Arrange(new Rect(0, 0, 460, 580));
                        content.UpdateLayout();
                        // Finish deferred WPF templates (including inline checkboxes) before capture.
                        content.Dispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                        content.UpdateLayout();
                        var drawing = new DrawingVisual();
                        using (var context = drawing.RenderOpen())
                        {
                            context.DrawRectangle(window.Background, null, new Rect(0, 0, 460, 580));
                            context.DrawRectangle(new VisualBrush(content), null, new Rect(0, 0, 460, 580));
                        }
                        var bitmap = new RenderTargetBitmap(460, 580, 96, 96, PixelFormats.Pbgra32);
                        bitmap.Render(drawing);
                        var encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(bitmap));
                        using var stream = File.Create(Path.Combine(output, $"sticky-{name}-{language}.png"));
                        encoder.Save(stream);
                    }
                    finally { window.Close(); }
                }
            }
            Console.WriteLine("Rendered 15 documentation images from isolated sample notes.");
        }
        finally
        {
            app.Shutdown();
            Directory.Delete(root, true);
        }
    }
}
