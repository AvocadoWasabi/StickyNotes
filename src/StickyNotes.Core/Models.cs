using System.Text.Json.Serialization;

namespace StickyNotes.Core;

public sealed class Settings : IJsonOnDeserialized
{
    public string NotesFolder { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "StickyNotesData");
    public string DailyFolder { get; set; } = "";
    public string DailyPattern { get; set; } = DailyNoteResolver.RegexExample;
    public bool AutoSaveOnFocusLoss { get; set; }
    public string GoogleCredentialsFile { get; set; } = "";
    public string CalendarId { get; set; } = "primary";
    public List<NotePlacement> Windows { get; set; } = [];

    void IJsonOnDeserialized.OnDeserialized() => DailyPattern = DailyNotePatternMigration.Upgrade(DailyPattern);
}

public sealed class NotePlacement
{
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
