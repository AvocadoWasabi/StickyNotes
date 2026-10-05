namespace StickyNotes.Core;

// Import only the old documented formats. Runtime resolution always uses regex.
public static class DailyNotePatternMigration
{
    public static string Upgrade(string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern)) return DailyNoteResolver.RegexExample;
        const string date = @"(?<year>\d{4})-(?<month>\d{2})-(?<day>\d{2})";
        const string nested = @"(?<year>\d{4})/(?<month>\d{2})/\k<year>-\k<month>-(?<day>\d{2})";
        return pattern switch
        {
            "yyyy-MM-dd" => date + @"\.md",
            "yyyy-MM-dd(ddd)" or "yyyy-MM-dd(dddd)" => date + @"\([^()/]+\)\.md",
            "yyyy/MM/yyyy-MM-dd" => nested + @"\.md",
            "yyyy/MM/yyyy-MM-dd(ddd)" or "yyyy/MM/yyyy-MM-dd(dddd)" => nested + @"\([^()/]+\)\.md",
            // Preserve custom input so unsupported formats can be corrected in Settings.
            _ => pattern
        };
    }
}
