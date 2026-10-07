using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using StickyNotes;
using StickyNotes.Core;

internal static partial class Program
{
    private static async Task NativeDailyCliSmoke(Settings settings)
    {
        var date = DateTime.Today;
        using var timeout = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(30));
        var daily = await new NoteSource(settings, true).GetDailyNotesAsync(date, timeout.Token);
        Check(daily.Targets.Length == 2 && !string.IsNullOrWhiteSpace(daily.Format), "Native daily desktop: reads native settings and two dated paths");
        var start = new System.Diagnostics.ProcessStartInfo(settings.ObsidianCli) { UseShellExecute = false, CreateNoWindow = true,
            RedirectStandardOutput = true, RedirectStandardError = true, StandardOutputEncoding = Encoding.UTF8, StandardErrorEncoding = Encoding.UTF8 };
        start.ArgumentList.Add("vault=" + settings.ObsidianVaultId);
        start.ArgumentList.Add("daily:path");
        using var process = System.Diagnostics.Process.Start(start)!;
        var stdout = process.StandardOutput.ReadToEndAsync(timeout.Token);
        var stderr = process.StandardError.ReadToEndAsync(timeout.Token);
        try { await process.WaitForExitAsync(timeout.Token); }
        finally { if (!process.HasExited) process.Kill(); }
        var output = await stdout;
        await stderr;
        var expected = ObsidianTasksClient.RelativeNotePath(settings.ObsidianVaultFolder, daily.Targets.Single(t => t.Date == ObsidianDailyNotes.DateKey(date)).Path);
        Check(process.ExitCode == 0 && date == DateTime.Today && output.Split('\n').Any(line => line.Trim().Replace('\\', '/') == expected), "Native daily desktop: today's path matches official daily:path without creating a note");
    }

    private static string DailyEnvelope(DateTime today, bool current = true, string path = "Diary/today(水).md", string configuration = "native-config") =>
        "=> STICKY_TASKS_PREVIEW:" + JsonSerializer.Serialize(new { folder = "Diary", format = "YYYY-MM-DD(ddd)", template = "Templates/Daily",
            configuration, targets = new[] {
                new { date = ObsidianDailyNotes.DateKey(today), path, exists = current },
                new { date = ObsidianDailyNotes.DateKey(today.AddDays(-1)), path = "Diary/yesterday(火).md", exists = true } } });

    private static JsonDocument DecodeCliRequest(string code)
    {
        var payload = code.Split("Buffer.from('")[1].Split("'")[0];
        using var compressed = new MemoryStream(Convert.FromBase64String(payload));
        using var inflated = new System.IO.Compression.ZLibStream(compressed, System.IO.Compression.CompressionMode.Decompress);
        return JsonDocument.Parse(inflated);
    }

    private static void ObsidianDailyTests(string root)
    {
        var date = new DateTime(2026, 10, 7);
        var calls = 0;
        var config = new Settings { ObsidianVaultFolder = root, DailyFolder = "invalid legacy folder", DailyPattern = "[" };
        var source = new NoteSource(config, true, (_, code, _) =>
        {
            calls++;
            using var request = DecodeCliRequest(code);
            Check(request.RootElement.GetProperty("dates").GetArrayLength() == 2 && !request.RootElement.TryGetProperty("mode", out _), "Native daily: exactly two dates, no list/search request");
            return Task.FromResult(DailyEnvelope(date));
        });
        var daily = source.GetDailyNotesAsync(date).GetAwaiter().GetResult();
        Check(calls == 1 && daily.Folder == "Diary" && daily.Format == "YYYY-MM-DD(ddd)" && daily.Template == "Templates/Daily", "Native daily: settings come from Obsidian without local regex validation");
        Check(daily.Resolve(date) == Path.Combine(root, "Diary", "today(水).md") && !File.Exists(daily.Resolve(date)), "Native daily: uses native path without requiring a local file");
        using var missingJson = ObsidianTasksClient.ParseEnvelope(DailyEnvelope(date, false));
        var missing = ObsidianDailyNotes.Decode(missingJson.RootElement, root, date);
        Throws<DailyNoteMissingException>(() => missing.Resolve(date), "Native daily: missing note is not created");
        var display = new DailyNoteDisplay();
        Check(display.Resolve(missing.Configuration, date, DailyNoteRetention.UntilCreated, missing.Resolve) == missing.Resolve(date.AddDays(-1)), "Native daily: previous-day retention uses Obsidian path");
        Check(display.Resolve(daily.Configuration, date, DailyNoteRetention.UntilCreated, daily.Resolve) == daily.Resolve(date), "Native daily: newly created today replaces previous day");
        display = new DailyNoteDisplay();
        display.Resolve(missing.Configuration, date, DailyNoteRetention.UntilRefresh, missing.Resolve);
        Check(display.Resolve(daily.Configuration, date, DailyNoteRetention.UntilRefresh, daily.Resolve) == daily.Resolve(date.AddDays(-1)), "Native daily: until-refresh keeps previous day");
        Check(display.Resolve("changed-native-settings", date, DailyNoteRetention.UntilRefresh, daily.Resolve) == daily.Resolve(date), "Native daily: changing Obsidian settings invalidates retention state");
        foreach (var unsafePath in new[] { "../outside.md", Path.Combine(root, "absolute.md"), "note.txt" })
        {
            using var unsafeJson = ObsidianTasksClient.ParseEnvelope(DailyEnvelope(date, path: unsafePath));
            Throws<InvalidOperationException>(() => ObsidianDailyNotes.Decode(unsafeJson.RootElement, root, date), "Native daily: rejects path outside the vault or Markdown");
        }
        var disconnected = new NoteSource(config, true, (_, _, _) => Task.FromException<string>(new IOException("disconnected")));
        Throws<IOException>(() => disconnected.ResolveDailyAsync(date).GetAwaiter().GetResult(), "Native daily: CLI failure cannot fall back to legacy folder/regex");
        Throws<InvalidOperationException>(() => ObsidianTasksClient.ParseEnvelope("=> STICKY_TASKS_PREVIEW:{\"error\":\"STICKY_DAILY_DISABLED\"}"), "Native daily: disabled core plugin is an explicit error");
        NativeDailySettingsUiTests(root);
        NativeDailyWindowTests(root);
    }

    private static void NativeDailyWindowTests(string root)
    {
        var app = App.Current;
        var previous = app.Config; var factory = app.NoteSources;
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        NoteWindow? window = null;
        var date = new DateTime(2026, 10, 7);
        var todayExists = false;
        var paths = new List<string>();
        try
        {
            app.ApplySettings(new Settings { ObsidianVaultFolder = root, DailyRetention = DailyNoteRetention.UntilCreated,
                DailyFolder = "unused", DailyPattern = "[" }, false);
            app.NoteSources = settings => new NoteSource(settings, true, (_, code, _) =>
            {
                using var request = DecodeCliRequest(code);
                if (request.RootElement.TryGetProperty("dates", out _)) return Task.FromResult(DailyEnvelope(date, todayExists));
                Check(request.RootElement.GetProperty("mode").GetString() == "read", "Native daily window: content is read through CLI after native path resolution");
                var path = request.RootElement.GetProperty("path").GetString()!;
                paths.Add(path);
                return Task.FromResult(CliSnapshotEnvelope("## Tasks\n" + path));
            });
            window = new NoteWindow(new NotePlacement { Daily = true, Heading = "Tasks" }, null, null, () => date);
            typeof(NoteWindow).GetMethod("ReloadCliNoteAsync", flags)!.Invoke(window, [true]);
            Check(paths.Single() == "Diary/yesterday(火).md" && (string)typeof(NoteWindow).GetField("content", flags)!.GetValue(window)! == paths[0],
                "Native daily window: retains yesterday's native heading with no local file or regex");
            todayExists = true;
            typeof(NoteWindow).GetMethod("ReloadCliNoteAsync", flags)!.Invoke(window, [true]);
            Check(paths.Last() == "Diary/today(水).md" && (string)typeof(NoteWindow).GetField("content", flags)!.GetValue(window)! == paths[^1],
                "Native daily window: switches content to today's native path when created");
        }
        finally { window?.Close(); app.NoteSources = factory; app.ApplySettings(previous, false); }
    }

    private static void NativeDailySettingsUiTests(string root)
    {
        var app = App.Current;
        var previous = app.Config; var factory = app.NoteSources;
        SettingsWindow? window = null;
        try
        {
            app.ApplySettings(new Settings { ObsidianVaultFolder = root, NotesFolder = "preserved invalid legacy folder", DailyFolder = "old-folder", DailyPattern = "[" }, false);
            app.NoteSources = settings => new NoteSource(settings, NoteSource.UsesCli(settings), (_, code, _) =>
            {
                if (code.Contains("root:app.vault")) return Task.FromResult("=> STICKY_TASKS_PREVIEW:" + JsonSerializer.Serialize(new { root, name = "DetectedVault" }));
                return Task.FromResult(DailyEnvelope(DateTime.Today));
            });
            window = new SettingsWindow();
            var panel = SettingsPanel(window);
            var manual = panel.Children.OfType<TextBox>().ToArray();
            Check(manual.Length == 3 && manual.All(t => t.Visibility == Visibility.Collapsed), "CLI settings: hides duplicated note folder, daily folder and regex");
            var cli = (StackPanel)panel.Children.OfType<Expander>().Single(e => (string)e.Header == "Obsidian CLI").Content;
            var enabled = cli.Children.OfType<CheckBox>().Single();
            var status = panel.Children.OfType<StackPanel>().SelectMany(p => p.Children.OfType<TextBlock>()).Single(t => t.Name == "DailyPreviewStatus");
            WaitFor(() => status.Text.Contains("Templates/Daily"), "CLI settings: previews native folder, format, template and resolved path");
            var layout = (Grid)window.Content;
            layout.Measure(new Size(1100, 760)); layout.Arrange(new Rect(0, 0, 1100, 760)); layout.UpdateLayout();
            RenderLocalizationPreview(layout, "cli-first-settings");
            enabled.IsChecked = false;
            Check(manual.All(t => t.Visibility == Visibility.Visible) && manual[2].Text == "[", "Local settings: explicit opt-out restores preserved manual fields");
            enabled.IsChecked = true;
            cli.Children.OfType<Button>().Single(b => b.Name == "DetectObsidianVault").RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
            WaitFor(() => cli.Children.OfType<TextBox>().Last().Text == "DetectedVault", "CLI settings: active vault discovery fills connection fields");
            panel.Children.OfType<Button>().Last().RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
            Check(app.Config.ObsidianTasksEnabled && app.Config.DailyFolder == "old-folder" && app.Config.DailyPattern == "[" && app.Config.NotesFolder == "preserved invalid legacy folder", "CLI settings: save ignores hidden validation and preserves local settings without migration");
        }
        finally { window?.Close(); app.NoteSources = factory; app.ApplySettings(previous, false); }
    }
}
