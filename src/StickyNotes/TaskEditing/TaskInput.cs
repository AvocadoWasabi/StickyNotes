using System.Globalization;
using System.Text.RegularExpressions;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace StickyNotes;

// Pure editing rules. No WPF state, CLI calls or writes; each edit is one undo unit.
internal sealed record TaskTextEdit(int Start, int Length, string Text);
internal sealed record TaskSuggestion(string Label, TaskTextEdit Edit, bool ContinueList = false);
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
        if (context is not { } c) return new([], false);
        var line = text[c.Start..c.End]; var column = caret - c.Start;
        // Adapted from Tasks Suggestor.ts, a01526153c71ce0faf72ad5dd42675f722502c50 (MIT).
        // Follow the default emoji-format menu with canSaveEdits=false (no dependency writes).
        var items = new List<TaskSuggestion>();
        var hasMatch = false;
        Match? AtCursor(string pattern) => Regex.Matches(line, pattern, RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100))
            .Cast<Match>().FirstOrDefault(m => m.Index > 0 && m.Index < column && column <= m.Index + m.Length);
        void AddFieldValue(Match match, string label, string value)
        {
            items.Add(new(label, new(c.Start + match.Index, match.Length, match.Groups[1].Value + " " + value + " ")));
            hasMatch = true;
        }
        string[] Filter(string[] candidates, string query, int max, bool fallback)
        {
            var matches = candidates.Where(s => query.Length > 0 && s.Contains(query, StringComparison.OrdinalIgnoreCase)).Take(max).ToArray();
            return matches.Length == 0 && fallback ? candidates.Take(max).ToArray() : matches;
        }
        var dates = new[] { "today", "tomorrow", "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "next week", "next month", "next year" };
        if (AtCursor(@"(📅|⏳|🛫)\s*([0-9a-zA-Z ]*)") is { } date)
        {
            var query = date.Groups[2].Value;
            if (query.Length > 1 && TaskSuggestionDates.Parse(query, today) is { } parsed)
            {
                var formatted = parsed.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                AddFieldValue(date, formatted, formatted);
            }
            foreach (var key in Filter(dates, query, 5, true))
            {
                var formatted = TaskSuggestionDates.Parse(key, today)!.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                AddFieldValue(date, key + " (" + formatted + ")", formatted);
            }
        }
        if (AtCursor(@"(🔁)\s*([0-9a-zA-Z ]*)") is { } recurrence)
        {
            var query = recurrence.Groups[2].Value;
            var valid = Regex.IsMatch(query.Trim(), @"^every (?:[1-9][0-9]* )?(?:day|week|month|year)s?(?: when done)?$",
                RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100));
            // A complete recurrence followed by its trailing space leaves the field menu.
            if (!(valid && recurrence.Value == "🔁 " + query.Trim() + " "))
            {
                if (valid) AddFieldValue(recurrence, "✅ " + query.Trim(), query.Trim());
                var rules = new[] { "every", "every day", "every week", "every month", "every month on the", "every year",
                    "every week on Sunday", "every week on Monday", "every week on Tuesday", "every week on Wednesday",
                    "every week on Thursday", "every week on Friday", "every week on Saturday" };
                foreach (var rule in Filter(rules, query, 10, string.IsNullOrWhiteSpace(query))) AddFieldValue(recurrence, rule, rule);
            }
        }
        if (AtCursor(@"(🏁)\s*([0-9a-zA-Z ]*)") is { } completion)
            foreach (var action in Filter(["delete", "keep"], completion.Groups[2].Value, 5, true)) AddFieldValue(completion, action, action);

        var generic = new List<(string Label, string Text, string Search)>();
        void Add(string symbol, string label)
        { if (!line.Contains(symbol)) generic.Add((symbol + " " + label, symbol + " ", symbol + " " + label)); }
        Add("📅", "due date"); Add("🛫", "start date"); Add("⏳", "scheduled date");
        if (!new[] { "⏫", "🔼", "🔽", "🔺", "⏬" }.Any(line.Contains))
        {
            Add("⏫", "high priority"); Add("🔼", "medium priority"); Add("🔽", "low priority");
            Add("🔺", "highest priority"); Add("⏬", "lowest priority");
        }
        Add("🔁", "recurring (repeat)");
        if (!line.Contains("➕"))
        {
            var formatted = today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            generic.Add(("➕ created today (" + formatted + ")", "➕ " + formatted + " ", "➕ created"));
        }
        Add("🏁", "on completion");
        var word = AtCursor(@"[a-zA-Z'_-]+");
        var matching = word is null ? [] : generic.Where(s => s.Search.Contains(word.Value, StringComparison.OrdinalIgnoreCase)).ToArray();
        if (matching.Length > 0)
        {
            hasMatch = true;
            items.AddRange(matching.Select(s => new TaskSuggestion(s.Label, new(c.Start + word!.Index, word.Length, s.Text))));
        }
        else items.AddRange(generic.Select(s => new TaskSuggestion(s.Label, new(caret, 0, s.Text))));
        if (items.Count > 0 && !hasMatch) items.Insert(0, new("⏎", new(caret, 0, ""), ContinueList: true));
        return new(items.Take(20).ToArray(), true);
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

    internal static TaskTextEdit? Indent(string text, int caret, int selection, bool unindent)
    {
        if (selection != 0 || caret < 0 || caret > text.Length) return null;
        var start = caret == 0 ? 0 : text.LastIndexOf('\n', caret - 1) + 1;
        var end = text.IndexOf('\n', caret); if (end < 0) end = text.Length;
        var match = TaskLine.Match(text[start..end].TrimEnd('\r'));
        if (!match.Success) return null;
        var indent = match.Groups["indent"].Value;
        var count = indent.StartsWith('\t') ? 1 : indent.TakeWhile(ch => ch == ' ').Take(4).Count();
        return unindent ? new(start, count, "") : new(start, 0, "\t");
    }
}
