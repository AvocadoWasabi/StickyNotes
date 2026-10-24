namespace StickyNotes;

public partial class App
{
    internal async Task DeleteNoteAsync(NoteSource source, FileSnapshot expected)
    {
        if (IsChangingFolder || Notes.Any(n => n.HasPendingWrite)) throw new InvalidOperationException(L10n.Text("StickyFolder.Working"));
        var affected = Notes.Where(n => n.ReferencesFile(expected.Path)).ToArray();
        if (affected.Any(n => n.HasUnsavedChanges)) throw new InvalidOperationException(L10n.Text("NoteBrowser.DeleteDraft"));
        var windows = Notes.ToArray();
        var enabled = windows.Select(n => n.IsEnabled).ToArray();
        IsChangingFolder = true;
        foreach (var note in windows) { note.PauseFolderChange(); note.IsEnabled = false; }
        try
        {
            await source.DeleteAsync(expected, Path.Combine(DataDirectory, "deleted-notes"));
            foreach (var note in affected) { Notes.Remove(note); note.CloseAfterDelete(); }
            Config.RecentlyClosedWindows.RemoveAll(p => !p.Daily &&
                string.Equals(p.Path, expected.Path, StringComparison.OrdinalIgnoreCase));
            lastCloseCompleted = null;
            SaveConfig();
        }
        finally
        {
            IsChangingFolder = false;
            for (var i = 0; i < windows.Length; i++)
                if (Notes.Contains(windows[i])) windows[i].IsEnabled = enabled[i];
        }
    }
}

public sealed partial class NoteWindow
{
    private bool deleted;
    internal bool HasPendingWrite => IsSaving || queryTaskSaving;
    internal bool ReferencesFile(string path) => string.Equals(snapshot?.Path, path, StringComparison.OrdinalIgnoreCase) ||
        !Placement.Daily && string.Equals(Placement.Path, path, StringComparison.OrdinalIgnoreCase);
    internal void CloseAfterDelete() { deleted = true; Close(); }
}
