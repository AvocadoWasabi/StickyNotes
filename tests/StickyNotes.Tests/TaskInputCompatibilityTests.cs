using System.IO;
using System.Text.Json;
using StickyNotes;

internal static partial class Program
{
    private static void TaskInputCompatibilityTests()
    {
        var today = new DateTime(2022, 7, 11); // Same fixed Monday as the upstream approved fixture.
        TaskSuggestions Suggest(string text, int? caret = null, int selection = 0) => TaskInput.Suggest(text, caret ?? text.Length, selection, today);
        string Apply(string text, TaskSuggestion suggestion) => text[..suggestion.Edit.Start] + suggestion.Edit.Text + text[(suggestion.Edit.Start + suggestion.Edit.Length)..];
        using var fixture = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures/TasksDefaultSuggestions.json")));
        var expected = fixture.RootElement.EnumerateArray().Where(s => !s.GetProperty("displayText").GetString()!.StartsWith("🆔") &&
            !s.GetProperty("displayText").GetString()!.StartsWith("⛔")).ToArray();
        foreach (var line in new[] { "- [ ] ", "- [ ] 買い物 ", "- [x] Completed ", "- [ ] unmatched" })
        {
            var actual = Suggest(line);
            Check(actual.Items.Select(i => i.Label).SequenceEqual(expected.Select(i => i.GetProperty("displayText").GetString())),
                "Upstream fixture: default menu order/labels match, excluding external-editor dependencies: " + line);
            Check(actual.SelectFirst && actual.Items[0].ContinueList && actual.Items.Skip(1).Select(i => i.Edit.Text)
                .SequenceEqual(expected.Skip(1).Select(i => i.GetProperty("appendText").GetString())),
                "Upstream fixture: newline is selected; inserts and trailing spaces match: " + line);
        }
        var due = Suggest("- [ ] 買い物 du");
        Check(due.Items.Select(i => i.Label).SequenceEqual(new[] { "📅 due date", "⏳ scheduled date" }) && !due.Items[0].ContinueList,
            "Upstream matching: du finds due and scheduled by substring, without newline option");
        Check(Suggest("- [ ] pRi").Items.Select(i => i.Label).SequenceEqual(new[] { "⏫ high priority", "🔼 medium priority", "🔽 low priority", "🔺 highest priority", "⏬ lowest priority" }),
            "Upstream matching: case-insensitive priority order");
        Check(Suggest("- [ ] high").Items[0].Label == "⏫ high priority", "Upstream matching: high comes before highest");
        const string middle = "- [ ] 日本語 😀 scheduled later";
        Check(Apply(middle, Suggest(middle, middle.IndexOf("scheduled", StringComparison.Ordinal) + 3).Items[0]) == "- [ ] 日本語 😀 ⏳  later",
            "Upstream matching: replace entire word under caret and preserve text after it");
        Check(Suggest("- [ ] 📅 2022-08-01 ⏫ ").Items.All(i => !i.Label.Contains("due date") && !i.Label.Contains("priority")),
            "Upstream menu: no duplicate date or priority fields");
        Check(Suggest("- [ ] 📅").Items[0].Label == "today (2022-07-11)" && Suggest("- [ ] 📅 ").Items[0].Label == "today (2022-07-11)",
            "Upstream date field: symbol alone or space selects today directly");
        Check(Suggest("- [ ] 📅 to").Items.Take(2).Select(i => i.Label).SequenceEqual(new[] { "today (2022-07-11)", "tomorrow (2022-07-12)" }),
            "Upstream date field: partial date filters contextual candidates");
        Check(Suggest("- [ ] 📅 27 oct").Items[0].Label == "2022-10-27" && Suggest("- [ ] 📅 1 year").Items[0].Label == "2023-07-11",
            "Upstream approved date examples: absolute month name and relative year");
        Check(Suggest("- [ ] 📅 tm ").Items[0].Label == "2022-07-12" && TaskSuggestionDates.Parse("tm", today) is null,
            "Upstream date abbreviations: expansion requires trailing space");
        Check(TaskSuggestionDates.Parse("tomorrow", new DateTime(2026, 12, 31)) == new DateTime(2027, 1, 1) &&
            TaskSuggestionDates.Parse("Sunday", today) == new DateTime(2022, 7, 17), "Date suggestions: year rollover and forward weekday");
        Check(Suggest("- [ ] 🔁").Items.Take(3).Select(i => i.Label).SequenceEqual(new[] { "every", "every day", "every week" }),
            "Upstream recurrence: generic rules appear before other fields");
        Check(Suggest("- [ ] 🔁").Items.Length == 20, "Upstream aggregation: combined menu is capped at 20 entries");
        Check(Suggest("- [ ] 🔁 every w").Items.Take(3).Select(i => i.Label).SequenceEqual(new[] { "every week", "every week on Sunday", "every week on Monday" }),
            "Upstream recurrence: partial rules include weekday variants");
        Check(Suggest("- [ ] 🔁 every day").Items[0].Label == "✅ every day" && Suggest("- [ ] 🔁 every day when done").Items[0].Label == "✅ every day when done",
            "Upstream recurrence: complete supported rules show validation marker");
        Check(Suggest("- [ ] 🔁 every day ").Items[0].ContinueList && Suggest("- [ ] 🔁 every week due").Items[0].Label == "📅 due date",
            "Upstream recurrence: trailing space exits recurrence, then another field can be completed");
        Check(Suggest("- [ ] 🏁").Items.Take(2).Select(i => i.Label).SequenceEqual(new[] { "delete", "keep" }),
            "Upstream on-completion: delete and keep are offered");
        Check(Suggest("- [ ] today").Items[0].ContinueList, "Upstream lifecycle: today does not incorrectly match created date");
        foreach (var text in new[] { "ordinary due", "```md\n- [ ] due\n```", "    - [ ] due", "<!--\n- [ ] due\n-->", "- [ ] `due`" })
            Check(Suggest(text, text.IndexOf("due", StringComparison.Ordinal) + 3).Items.Length == 0,
                "Completion safety: non-task and code stay untouched: " + text.Replace('\n', ' '));
        Check(Suggest("- [ ] due", selection: 1).Items.Length == 0, "Completion safety: selected text stays untouched");
        const string list = "- [ ] first\r\n  * [x] finished";
        var continuation = TaskInput.ContinueList(list, list.Length, 0)!;
        Check(continuation.Text == "\r\n  * [ ] ", "Task Enter: preserves indentation, bullet and CRLF, resets status");
        Check(TaskInput.ContinueList("- [ ] ", 6, 0) == new TaskTextEdit(0, 6, ""), "Task Enter: empty task ends list");
        const string empty = "- [ ] first\n- [ ] \nnext";
        var ending = TaskInput.ContinueList(empty, 18, 0)!;
        Check(empty[..ending.Start] + ending.Text + empty[(ending.Start + ending.Length)..] == "- [ ] first\n\nnext",
            "Task Enter: empty item does not consume the following line");
        Check(TaskInput.ContinueList("```\n- [ ] text\n```", 14, 0) is null && TaskInput.ContinueList("- [ ] text", 10, 1) is null,
            "Task Enter: code and selected text retain default behavior");
        Check(TaskInput.ContinueList("12. [ ] abc", 9, 0)!.Text == "\n13. [ ] ", "Task Enter: ordered list continues at caret");
        Check(TaskInput.Indent("- [ ] due", 9, 0, false) == new TaskTextEdit(0, 0, "\t") &&
            TaskInput.Indent("\t- [ ] due", 10, 0, true) == new TaskTextEdit(0, 1, ""), "Task Tab: indent and unindent operate on line, not candidate");
    }
}
