using System.Text.Json.Serialization;

namespace StickyNotes.Core;

public enum TaskCompletionMode { Automatic = 0, On = 1, Off = 2 }

public sealed class Settings : IJsonOnDeserialized
{
    public string Language { get; set; } = "";
    public string NotesFolder { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "StickyNotesData");
    public string NotesRoot { get; set; } = "";
    // null inherits Obsidian's location; "." explicitly selects the vault root.
    public string? ObsidianNotesFolder { get; set; }
    public string DailyFolder { get; set; } = "";
    public string DailyPattern { get; set; } = DailyNoteResolver.RegexExample;
    public DailyNoteRetention DailyRetention { get; set; }
    public bool AutoSaveOnFocusLoss { get; set; }
    public bool DoubleClickToEdit { get; set; }
    public TaskCompletionMode TaskCompletion { get; set; } = TaskCompletionMode.Automatic;
    public string TaskToggleShortcut { get; set; } = "";
    public bool TitleButtonOverlay { get; set; }
    public bool ShowInTaskbar { get; set; }
    public string GoogleCredentialsFile { get; set; } = "";
    public string CalendarId { get; set; } = "primary";
    public bool ObsidianTasksEnabled { get; set; } = true;
    public string ObsidianCli { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Obsidian", "Obsidian.com");
    public string ObsidianVaultFolder { get; set; } = "";
    public string ObsidianVaultId { get; set; } = "";
    public List<NotePlacement> Windows { get; set; } = [];
    private List<NotePlacement> recentlyClosedWindows = [];
    public List<NotePlacement> RecentlyClosedWindows { get => recentlyClosedWindows; set => recentlyClosedWindows = value ?? []; }

    void IJsonOnDeserialized.OnDeserialized() => DailyPattern = DailyNotePatternMigration.Upgrade(DailyPattern);
}

public sealed class NotePlacement
{
    public const int MinContentScale = 50;
    public const int MaxContentScale = 200;
    private int contentScale = 100;
    public int ContentScale { get => contentScale; set => contentScale = Math.Clamp(value, MinContentScale, MaxContentScale); }
    public string Path { get; set; } = "";
    public string Heading { get; set; } = "";
    private List<string> headingFilters = [];
    public List<string> HeadingFilters { get => headingFilters; set => headingFilters = value ?? []; }
    public bool Daily { get; set; }
    public double Left { get; set; } = 100;
    public double Top { get; set; } = 100;
    public double Width { get; set; } = 360;
    public double Height { get; set; } = 400;
    public bool Pinned { get; set; }
    public string Color { get; set; } = "yellow";
    public string Monitor { get; set; } = "";
    public int PixelLeft { get; set; }
    public int PixelTop { get; set; }
    public bool HasPixelPosition { get; set; }

    public NotePlacement Copy()
    {
        var copy = (NotePlacement)MemberwiseClone();
        copy.HeadingFilters = [.. HeadingFilters];
        return copy;
    }
}

public sealed record NoteMetadata(string Title, string[] Tags, string Status, string Color);
public sealed record FileSnapshot(string Path, string Text, string Hash);
public sealed class ConflictException(string message) : IOException(message);
