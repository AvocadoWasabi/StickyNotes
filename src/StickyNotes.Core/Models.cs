using System.Text.Json.Serialization;

namespace StickyNotes.Core;

public sealed class Settings : IJsonOnDeserialized
{
    public string Language { get; set; } = "";
    public string NotesFolder { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "StickyNotesData");
    public string DailyFolder { get; set; } = "";
    public string DailyPattern { get; set; } = DailyNoteResolver.RegexExample;
    public DailyNoteRetention DailyRetention { get; set; }
    public bool AutoSaveOnFocusLoss { get; set; }
    public bool TitleButtonOverlay { get; set; }
    public bool ShowInTaskbar { get; set; }
    public string GoogleCredentialsFile { get; set; } = "";
    public string CalendarId { get; set; } = "primary";
    public bool ObsidianTasksEnabled { get; set; }
    public string ObsidianCli { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Obsidian", "Obsidian.com");
    public string ObsidianVaultFolder { get; set; } = "";
    public string ObsidianVaultId { get; set; } = "";
    public List<NotePlacement> Windows { get; set; } = [];

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
}

public sealed record NoteMetadata(string Title, string[] Tags, string Status, string Color);
public sealed record FileSnapshot(string Path, string Text, string Hash);
public sealed class ConflictException(string message) : IOException(message);
