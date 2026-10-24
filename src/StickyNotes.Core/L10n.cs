using System.Globalization;
using System.Resources;

namespace StickyNotes.Core;

public static class L10n
{
    public static IReadOnlyList<(string Code, string Name)> SupportedLanguages { get; } = Array.AsReadOnly(new[] {
        ("ja", "日本語"), ("en", "English"), ("zh-CN", "简体中文"),
        ("zh-TW", "繁體中文（台灣）"), ("ko", "한국어"), ("de", "Deutsch"),
        ("ru", "Русский"), ("es", "Español"), ("sv", "Svenska")
    });
    private static readonly ResourceManager Resources = new("StickyNotes.Core.Strings", typeof(L10n).Assembly);
    private static readonly CultureInfo WindowsLanguage = CultureInfo.CurrentUICulture;
    private static CultureInfo culture = CultureInfo.GetCultureInfo("ja");
    public static string Language => culture.Name;

    public static string ResolveLanguage(string? preference, CultureInfo windowsLanguage)
    {
        if (SupportedLanguages.Any(item => item.Code == preference)) return preference!;
        // Follow the script as well as the region (e.g. zh-Hant, zh-HK, zh-MO).
        for (var candidate = windowsLanguage; !string.IsNullOrEmpty(candidate.Name); candidate = candidate.Parent)
            if (candidate.Name is "zh-TW" or "zh-HK" or "zh-MO" or "zh-Hant") return "zh-TW";
        var language = windowsLanguage.TwoLetterISOLanguageName;
        if (language == "zh") return "zh-CN";
        return SupportedLanguages.Any(item => item.Code == language) ? language : "en";
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
