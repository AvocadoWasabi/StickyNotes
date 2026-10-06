using System.Globalization;
using System.Resources;

namespace StickyNotes.Core;

public static class L10n
{
    private static readonly ResourceManager Resources = new("StickyNotes.Core.Strings", typeof(L10n).Assembly);
    private static readonly CultureInfo WindowsLanguage = CultureInfo.CurrentUICulture;
    private static CultureInfo culture = CultureInfo.GetCultureInfo("ja");
    public static string Language => culture.Name;

    public static string ResolveLanguage(string? preference, CultureInfo windowsLanguage)
    {
        if (preference is "ja" or "en" or "zh-CN") return preference;
        return windowsLanguage.TwoLetterISOLanguageName switch { "ja" => "ja", "zh" => "zh-CN", _ => "en" };
    }

    // Set once at startup. Saving a new preference must not change an active editor or dialog.
    public static void Initialize(string? preference, CultureInfo? windowsLanguage = null)
    {
        culture = CultureInfo.GetCultureInfo(ResolveLanguage(preference, windowsLanguage ?? WindowsLanguage));
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        CultureInfo.CurrentUICulture = culture;
    }

    public static string Text(string key) => Resources.GetString(key, culture)
        ?? throw new MissingManifestResourceException("Missing UI resource: " + key);

    // Dates in filenames and Markdown remain stable across language and regional settings.
    public static string Format(string key, params object?[] arguments) =>
        string.Format(CultureInfo.InvariantCulture, Text(key), arguments);
}
