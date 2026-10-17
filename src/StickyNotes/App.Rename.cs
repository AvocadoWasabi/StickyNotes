namespace StickyNotes;

public partial class App
{
    internal async Task RenameNoteAsync(NoteWindow owner, FileSnapshot expected, string target)
    {
        if (IsChangingFolder || Notes.Any(n => n.IsSaving)) throw new InvalidOperationException(L10n.Text("StickyFolder.Working"));
        if (owner.Placement.Daily || owner.Placement.Path != expected.Path) throw new InvalidOperationException(L10n.Text("Rename.Daily"));
        if (Notes.Any(n => n.HasUnsavedChanges && string.Equals(n.Placement.Path, expected.Path, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException(L10n.Text("Rename.Draft"));
        var windows = Notes.ToArray();
        var enabled = windows.Select(n => n.IsEnabled).ToArray();
        IsChangingFolder = true;
        foreach (var note in windows) { note.PauseFolderChange(); note.IsEnabled = false; }
        try
        {
            await NoteRename.ApplyAsync(NoteSources(Config), expected, target, paths =>
            {
                Config.RecentlyClosedWindows = CopyClosedNotesForSettings(Config, paths);
                foreach (var note in Notes) note.Relocate(paths);
                // Keep the new in-memory paths if persistence fails: the file has already moved.
                SaveConfig();
            }, Path.Combine(DataDirectory, "note-renames"));
        }
        finally
        {
            IsChangingFolder = false;
            for (var i = 0; i < windows.Length; i++) windows[i].IsEnabled = enabled[i];
            foreach (var note in windows) note.RefreshAfterRename();
        }
    }
}
