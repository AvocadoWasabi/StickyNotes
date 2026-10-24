namespace StickyNotes;

public partial class App
{
    // Windows sends separate close requests for "Close all windows". Group a
    // sequence with no more than two seconds between requests. Measure from the
    // last completed close so time spent answering a save dialog does not split it.
    internal Func<long> CloseClock { get; set; } = () => Environment.TickCount64;
    private long? lastCloseCompleted;

    internal void NoteClosed(NoteWindow note, long closeRequested)
    {
        if (Exiting || !Notes.Remove(note)) return;
        if (lastCloseCompleted is not { } completed || closeRequested - completed > 2000)
            Config.RecentlyClosedWindows.Clear();
        Config.RecentlyClosedWindows.Add(note.Placement.Copy());
        Safe(SaveConfig);
        lastCloseCompleted = CloseClock();
    }

    public void RestoreRecentlyClosedNotes()
    {
        if (IsChangingFolder) throw new InvalidOperationException(L10n.Text("StickyFolder.Working"));
        lastCloseCompleted = null;
        var errors = new List<Exception>();
        foreach (var placement in Config.RecentlyClosedWindows.ToArray())
        {
            try
            {
                ShowNote(placement.Copy());
                Config.RecentlyClosedWindows.Remove(placement);
            }
            catch (Exception ex) { errors.Add(ex); }
        }
        SaveConfig();
        if (errors.Count > 0) throw new AggregateException(errors);
    }

    private List<NotePlacement> CopyClosedNotesForSettings(Settings next, IReadOnlyDictionary<string, string> paths)
    {
        // Relative CLI paths and daily links belong to the selected source/vault.
        // Do not reinterpret old history after switching to a different source.
        return Config.RecentlyClosedWindows.Where(p => Settings.SameSource(Config.ForVault(p.VaultId), next.ForVault(p.VaultId))).Select(placement =>
        {
            var copy = placement.Copy();
            if (copy.Path.Length > 0 && paths.Count > 0 && paths.TryGetValue(Path.GetFullPath(copy.Path), out var path))
                copy.Path = path;
            return copy;
        }).ToList();
    }
}
