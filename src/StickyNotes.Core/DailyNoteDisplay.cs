namespace StickyNotes.Core;

public enum DailyNoteRetention { ShowWaitingMessage = 0, UntilCreated = 1, UntilRefresh = 2 }

// State belongs to one sticky note; a manual refresh releases its fallback for this day.
public sealed class DailyNoteDisplay
{
    private (string Source, DailyNoteRetention Mode)? configuration;
    private DateTime? heldOn, refreshedOn;
    public DateTime TargetDate { get; private set; }

    public void Refresh(DateTime today) { refreshedOn = today.Date; heldOn = null; }

    public string Resolve(string folder, string pattern, DateTime today, DailyNoteRetention mode) =>
        Resolve(folder + "\0" + pattern, today, mode, date => DailyNoteResolver.Resolve(folder, pattern, date));

    public string Resolve(string source, DateTime today, DailyNoteRetention mode, Func<DateTime, string> resolve)
    {
        today = today.Date;
        var next = (source, mode);
        if (configuration.HasValue && configuration != next) { heldOn = null; refreshedOn = null; }
        configuration = next;
        TargetDate = today;
        var retain = (mode is DailyNoteRetention.UntilCreated or DailyNoteRetention.UntilRefresh) && refreshedOn != today;
        if (retain && mode == DailyNoteRetention.UntilRefresh && heldOn == today)
        {
            try
            {
                var path = resolve(today.AddDays(-1));
                TargetDate = today.AddDays(-1);
                return path;
            }
            catch (DailyNoteMissingException) { heldOn = null; }
        }
        try { return resolve(today); }
        catch (DailyNoteMissingException) when (retain)
        {
            var path = resolve(today.AddDays(-1));
            TargetDate = today.AddDays(-1); heldOn = today;
            return path;
        }
    }
}
