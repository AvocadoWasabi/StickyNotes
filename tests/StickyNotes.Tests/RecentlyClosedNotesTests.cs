using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Windows;
using StickyNotes;
using StickyNotes.Core;

internal static partial class Program
{
    private static void RecentlyClosedNotesTests(string root)
    {
        Check(JsonSerializer.Deserialize<Settings>("{}")!.RecentlyClosedWindows.Count == 0 &&
            JsonSerializer.Deserialize<Settings>("{\"RecentlyClosedWindows\":null}")!.RecentlyClosedWindows.Count == 0,
            "Reopen: older settings and null history start empty");
        var app = App.Current;
        var previous = app.Config;
        var previousNotes = app.Notes.ToArray();
        var previousHistory = previous.RecentlyClosedWindows;
        app.Notes.Clear();
        var previousClock = app.CloseClock;
        long clock = 10000;
        app.CloseClock = () => clock;
        app.ApplySettings(new Settings { ObsidianTasksEnabled = false, NotesFolder = root }, false);
        app.Config.RecentlyClosedWindows.Clear();
        var path = Path.Combine(root, "reopen.md");
        const string text = "# Reopen\n## One\nbody\n## Two\nother\n";
        File.WriteAllText(path, text);
        try
        {
            app.ShowNote(new NotePlacement { Path = path, Pinned = true, Width = 420, Height = 330,
                ContentScale = 130, HeadingFilters = ["One"] });
            app.ShowNote(new NotePlacement { Path = path, Heading = "Two", Color = "blue" });
            var first = app.Notes.Single(n => n.Placement.Heading == "");
            var second = app.Notes.Single(n => n.Placement.Heading == "Two");
            CancelEventHandler cancel = (_, args) => args.Cancel = true;
            first.Closing += cancel;
            first.Close();
            Check(app.Notes.Count == 2 && first.IsVisible && app.Config.RecentlyClosedWindows.Count == 0,
                "Reopen: canceled close leaves window active and history unchanged");
            first.Closing -= cancel;
            first.Closing += (_, _) => clock += 10000; // Simulate time spent confirming an unsaved edit.
            SendMessage(new System.Windows.Interop.WindowInteropHelper(first).Handle, 0x0010, IntPtr.Zero, IntPtr.Zero);
            var expected = JsonSerializer.Serialize(app.Config.RecentlyClosedWindows.Single());
            first.Placement.HeadingFilters.Clear();
            Check(app.Config.RecentlyClosedWindows.Single().HeadingFilters.SequenceEqual(new[] { "One" }),
                "Reopen: closed placement is a deep snapshot");
            clock += 100;
            SendMessage(new System.Windows.Interop.WindowInteropHelper(second).Handle, 0x0010, IntPtr.Zero, IntPtr.Zero);
            Check(app.Notes.Count == 0 && app.Config.RecentlyClosedWindows.Count == 2,
                "Reopen: close-all batch survives a slow close confirmation");
            var persisted = JsonSerializer.Deserialize<Settings>(File.ReadAllText(Path.Combine(App.DataDirectory, "settings.json")))!;
            Check(persisted.Windows.Count == 0 && persisted.RecentlyClosedWindows.Count == 2 &&
                JsonSerializer.Serialize(persisted.RecentlyClosedWindows[0]) == expected,
                "Reopen: batch and full placement survive settings serialization");
            app.Config.RecentlyClosedWindows = persisted.RecentlyClosedWindows;
            app.ExecuteCommand(AppCommands.Parse(["--command", "restore-closed"])!);
            Check(app.Notes.Count == 2 && app.Config.RecentlyClosedWindows.Count == 0,
                "Reopen: command restores and consumes the full batch");
            Check(app.Notes.Any(n => n.Placement.Heading == "Two" && n.Placement.Color == "blue") &&
                app.Notes.Any(n => n.Placement.Pinned && n.Placement.ContentScale == 130 &&
                    n.Placement.HeadingFilters.SequenceEqual(new[] { "One" })),
                "Reopen: linked section, color, filters, pin and scale are restored");
            var restored = app.Notes.Single(n => n.Placement.Pinned).Placement;
            var originalPlacement = JsonSerializer.Deserialize<NotePlacement>(expected)!;
            Check(Math.Abs(restored.Width - originalPlacement.Width) < 1 && Math.Abs(restored.Height - originalPlacement.Height) < 1 &&
                restored.PixelLeft == originalPlacement.PixelLeft && restored.PixelTop == originalPlacement.PixelTop,
                "Reopen: window size and pixel position are restored within DPI rounding");
            app.RestoreRecentlyClosedNotes();
            Check(app.Notes.Count == 2, "Reopen: repeated command does not duplicate windows");
            first = app.Notes[0]; second = app.Notes[1];
            first.Close(); clock += 2001; second.Close();
            Check(app.Config.RecentlyClosedWindows.Count == 1 && app.Config.RecentlyClosedWindows[0].Heading == "Two",
                "Reopen: a later separate close replaces the previous batch");
            app.ShowNote(new NotePlacement { Path = path, Heading = "Two" });
            app.RestoreRecentlyClosedNotes();
            Check(app.Notes.Count == 1, "Reopen: manually reopened notes are activated without duplicates");
            app.Notes.Single().Close();
            app.ApplySettings(new Settings { ObsidianTasksEnabled = false, NotesFolder = root, ShowInTaskbar = true }, false);
            Check(app.Config.RecentlyClosedWindows.Count == 1, "Reopen: ordinary settings changes preserve history");

            app.Config.RecentlyClosedWindows.Insert(0, null!); // Simulate a malformed entry in saved history.
            Throws<AggregateException>(app.RestoreRecentlyClosedNotes, "Reopen: malformed history entry is reported");
            Check(app.Notes.Count == 1 && app.Config.RecentlyClosedWindows.Count == 1 && app.Config.RecentlyClosedWindows[0] is null,
                "Reopen: one failure does not block other windows and remains retryable");
            app.Config.RecentlyClosedWindows.Clear();
            app.Notes.Single().Close();
            var beforeExit = JsonSerializer.Serialize(app.Config.RecentlyClosedWindows);
            app.ShowNote(new NotePlacement { Path = path });
            var exiting = app.Notes.Single();
            app.SaveConfig();
            typeof(App).GetProperty(nameof(App.Exiting))!.SetValue(app, true);
            try { exiting.Close(); }
            finally { typeof(App).GetProperty(nameof(App.Exiting))!.SetValue(app, false); app.Notes.Remove(exiting); }
            var afterExit = JsonSerializer.Deserialize<Settings>(File.ReadAllText(Path.Combine(App.DataDirectory, "settings.json")))!;
            Check(JsonSerializer.Serialize(afterExit.RecentlyClosedWindows) == beforeExit && afterExit.Windows.Count == 1,
                "Reopen: application exit preserves history and the open layout separately");
            var migrated = Path.Combine(root, "reopen-migrated");
            Directory.CreateDirectory(migrated);
            // Only migrate a synthetic sticky note in its own folder.
            var sourceFolder = Path.Combine(root, "reopen-source");
            Directory.CreateDirectory(sourceFolder);
            var sticky = NoteStore.Create(sourceFolder);
            app.ApplySettings(new Settings { ObsidianTasksEnabled = false, NotesFolder = sourceFolder }, false);
            app.Config.RecentlyClosedWindows = [new NotePlacement { Path = sticky }];
            app.ApplySettings(new Settings { ObsidianTasksEnabled = false, NotesFolder = migrated }, true);
            Check(app.Config.RecentlyClosedWindows.Single().Path == Path.Combine(migrated, Path.GetFileName(sticky)),
                "Reopen: folder migration updates closed-note paths");
            app.ApplySettings(new Settings { ObsidianTasksEnabled = true, ObsidianVaultFolder = root }, false);
            Check(app.Config.RecentlyClosedWindows.Count == 0, "Reopen: switching note source clears incompatible history");
            Check(File.ReadAllText(path) == text, "Reopen: close and restore never rewrite note content");
        }
        finally
        {
            foreach (var note in app.Notes.ToArray()) note.Close();
            app.Config.RecentlyClosedWindows.Clear();
            app.CloseClock = previousClock;
            app.Notes.AddRange(previousNotes);
            app.ApplySettings(previous, false);
            app.Config.RecentlyClosedWindows = previousHistory;
            app.SaveConfig();
        }
    }
}
