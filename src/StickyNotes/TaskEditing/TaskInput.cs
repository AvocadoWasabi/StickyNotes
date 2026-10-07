using System.Globalization;
using System.Text.RegularExpressions;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace StickyNotes;

// Pure editing rules. No WPF state, CLI calls or writes; each edit is one undo unit.
internal sealed record TaskTextEdit(int Start, int Length, string Text);
internal sealed record TaskSuggestion(string Label, TaskTextEdit Edit);
internal sealed record TaskSuggestions(TaskSuggestion[] Items, bool SelectFirst);

internal static class TaskInput
{
    private static readonly Regex TaskLine = new(@"^(?<indent>[ \t]*)(?<bullet>[-+*]|[0-9]{1,9}[.)]) (?<box>\[[ xX]\]) (?<body>.*)$",
        RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100));

    private static (int Start, int End, Match Match)? Context(string text, int caret, int selection)
    {
        if (selection != 0 || caret < 0 || caret > text.Length) return null;
        var start = caret == 0 ? 0 : text.LastIndexOf('\n', caret - 1) + 1;
        var end = text.IndexOf('\n', caret); if (end < 0) end = text.Length;
        if (end > start && text[end - 1] == '\r') end--;
        var match = TaskLine.Match(text[start..end]);
        if (!match.Success || caret < start + match.Groups["body"].Index || caret > end) return null;
        // Parse only after finding a task-shaped line; ignore fenced/indented code and HTML.
        var document = Markdown.Parse(text, MarkdownView.Pipeline);
        if (document.Descendants().Any(b => b is CodeBlock or HtmlBlock && b.Span.Start <= caret && b.Span.End >= caret - 1)) return null;
        if (document.Descendants<ParagraphBlock>().Any(p => p.Inline?.Descendants<CodeInline>()
            .Any(i => i.Span.Start < caret && i.Span.End >= caret) == true)) return null;
        return (start, end, match);
    }

    internal static TaskSuggestions Suggest(string text, int caret, int selection, DateTime today)
    {
        var context = Context(text, caret, selection);
        if (context is not { } c || c.Match.Groups["box"].Value != "[ ]" ||
            (caret < c.End && !char.IsWhiteSpace(text[caret]))) return new([], false);
        var bodyStart = c.Start + c.Match.Groups["body"].Index;
        var prefix = text[bodyStart..caret];
        var body = c.Match.Groups["body"].Value;
        var items = new List<TaskSuggestion>();
        var dates = new (string Key, DateTime Date)[] { ("today", today), ("tomorrow", today.AddDays(1)),
            ("next week", today.AddDays(7)), ("next month", today.AddMonths(1)), ("next year", today.AddYears(1)) };
        // Emoji may contain surrogate pairs; use literal alternatives instead of a character class.
        var date = Regex.Match(prefix, @"(?:📅|⏳|🛫|➕) (?<query>[a-zA-Z ]*)$", RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100));
        if (date.Success)
        {
            var query = date.Groups["query"];
            foreach (var candidate in dates.Where(d => d.Key.StartsWith(query.Value, StringComparison.OrdinalIgnoreCase)))
            {
                var formatted = candidate.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                items.Add(new(candidate.Key + " → " + formatted, new(bodyStart + query.Index, query.Length, formatted + " ")));
            }
            if (items.Count > 0) return new(items.ToArray(), query.Length > 0 && !query.Value.EndsWith(' '));
        }
        var recurring = Regex.Match(prefix, @"🔁 (?<query>[a-zA-Z ]*)$", RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100));
        if (recurring.Success)
        {
            var query = recurring.Groups["query"];
            foreach (var key in new[] { "every day", "every week", "every month", "every year", "every week when done" })
                if (key.StartsWith(query.Value, StringComparison.OrdinalIgnoreCase))
                    items.Add(new(key, new(bodyStart + query.Index, query.Length, key + " ")));
            if (items.Count > 0) return new(items.ToArray(), query.Length > 0 && !query.Value.EndsWith(' '));
        }
        var word = Regex.Match(prefix, @"(?:^|\s)(?<query>[a-zA-Z]+(?: [a-zA-Z]+)?)$", RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100));
        if (!word.Success) return new([], false);
        var token = word.Groups["query"];
        // Prefer a phrase ("every w"); otherwise match the final word in a description.
        void Add(string key, string value, string? absent = null)
        {
            if (absent is not null && absent.Split('|').Any(body.Contains)) return;
            var query = token.Value; var start = bodyStart + token.Index;
            if (!key.StartsWith(query, StringComparison.OrdinalIgnoreCase))
            {
                var space = query.LastIndexOf(' '); if (space < 0) return;
                start += space + 1; query = query[(space + 1)..];
                if (!key.StartsWith(query, StringComparison.OrdinalIgnoreCase)) return;
            }
            items.Add(new(key + " → " + value, new(start, query.Length, value + " ")));
        }
        Add("due", "📅", "📅"); Add("scheduled", "⏳", "⏳"); Add("start", "🛫", "🛫");
        Add("created", "➕ " + today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), "➕");
        const string priorities = "🔺|⏫|🔼|🔽|⏬";
        Add("high", "⏫", priorities); Add("highest", "🔺", priorities); Add("medium", "🔼", priorities);
        Add("low", "🔽", priorities); Add("lowest", "⏬", priorities);
        Add("repeat", "🔁", "🔁"); Add("recurring", "🔁", "🔁");
        foreach (var key in new[] { "every day", "every week", "every month", "every year" }) Add(key, "🔁 " + key, "🔁");
        foreach (var candidate in dates) Add(candidate.Key, "📅 " + candidate.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), "📅");
        return new(items.ToArray(), true);
    }

    internal static TaskTextEdit? ContinueList(string text, int caret, int selection)
    {
        if (Context(text, caret, selection) is not { } c) return null;
        var body = c.Match.Groups["body"].Value;
        if (string.IsNullOrWhiteSpace(body)) return new(c.Start, c.End - c.Start, "");
        var bullet = c.Match.Groups["bullet"].Value;
        if (bullet.Length > 1) bullet = (int.Parse(bullet[..^1], CultureInfo.InvariantCulture) + 1).ToString(CultureInfo.InvariantCulture) + bullet[^1];
        var newline = text.Contains("\r\n") ? "\r\n" : "\n";
        return new(caret, 0, newline + c.Match.Groups["indent"].Value + bullet + " [ ] ");
    }
}
