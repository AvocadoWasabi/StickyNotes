using System.Globalization;
using System.Text.RegularExpressions;

namespace StickyNotes;

// Bounded local date vocabulary, not a replacement for Tasks' complete chrono-node parser.
internal static class TaskSuggestionDates
{
    internal static DateTime? Parse(string input, DateTime today)
    {
        var text = input.Trim().ToLowerInvariant();
        if (input.EndsWith(' ')) text = text switch { "td" => "today", "tm" => "tomorrow", "yd" => "yesterday", "nw" => "next week", "we" or "weekend" => "saturday", _ => text };
        if (text == "today") return today.Date;
        if (text == "tomorrow") return today.Date.AddDays(1);
        if (text == "yesterday") return today.Date.AddDays(-1);
        if (text == "next week") return today.Date.AddDays(7);
        if (text == "next month") return today.Date.AddMonths(1);
        if (text == "next year") return today.Date.AddYears(1);
        if (text.All(char.IsLetter) && Enum.TryParse<DayOfWeek>(text, true, out var day) && Enum.IsDefined(day))
            return today.Date.AddDays(((int)day - (int)today.DayOfWeek + 7) % 7);
        var relative = Regex.Match(text, @"^(?:in )?([0-9]{1,4}) (day|week|month|year)s?$", RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100));
        try
        {
            if (relative.Success)
            {
                var n = int.Parse(relative.Groups[1].Value, CultureInfo.InvariantCulture);
                return relative.Groups[2].Value switch { "day" => today.Date.AddDays(n), "week" => today.Date.AddDays(7 * n),
                    "month" => today.Date.AddMonths(n), _ => today.Date.AddYears(n) };
            }
            // Use the supplied year, never the host parser's current year, for deterministic tests.
            if (DateTime.TryParseExact(text + " " + today.Year.ToString(CultureInfo.InvariantCulture), ["d MMM yyyy", "d MMMM yyyy", "MMM d yyyy", "MMMM d yyyy"],
                CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out var date)) return date < today.Date ? date.AddYears(1) : date;
        }
        catch (ArgumentOutOfRangeException) { }
        return null;
    }
}
