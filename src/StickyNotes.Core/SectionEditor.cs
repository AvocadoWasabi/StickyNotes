using System.Text.RegularExpressions;

namespace StickyNotes.Core;

public sealed record SectionSpan(int Start, int Length, string Content);

public static class SectionEditor
{
    // Share the same heading boundaries between the picker and section editing.
    private static List<(int Start, int End, int Level, string Title)> Scan(string text)
    {
        var bodyOffset = text.Length - NoteStore.Split(text).Body.Length;
        var matches = new List<(int Start, int End, int Level, string Title)>();
        char fence = '\0';
        int fenceLength = 0;
        foreach (Match line in Regex.Matches(text[bodyOffset..], @"[^\r\n]*(?:\r\n|\n|$)"))
        {
            var value = line.Value.TrimEnd('\r', '\n');
            var f = Regex.Match(value, @"^ {0,3}(`{3,}|~{3,})(.*)$");
            if (f.Success)
            {
                if (fence == '\0') { fence = f.Groups[1].Value[0]; fenceLength = f.Groups[1].Length; }
                else if (f.Groups[1].Value[0] == fence && f.Groups[1].Length >= fenceLength && string.IsNullOrWhiteSpace(f.Groups[2].Value)) fence = '\0';
                continue;
            }
            if (fence != '\0') continue;
            var h = Regex.Match(value, @"^ {0,3}(#{1,6})\s+(.+?)\s*$");
            if (h.Success) matches.Add((bodyOffset + line.Index, bodyOffset + line.Index + line.Length,
                h.Groups[1].Length, Regex.Replace(h.Groups[2].Value, @"\s+#+$", "")));
        }
        return matches;
    }

    public static IReadOnlyList<string> Headings(string text) => Scan(text).Select(x => x.Title).Distinct().ToArray();

    public static string EnsureHeading(string text, string heading)
    {
        if (string.IsNullOrWhiteSpace(heading)) return text;
        if (heading.Any(char.IsControl)) throw new InvalidOperationException("見出し名は改行を含まない1行で入力してください。");
        if (Headings(text).Contains(heading)) { _ = Find(text, heading); return text; }
        var newline = text.Contains("\r\n") ? "\r\n" : "\n";
        var candidate = text + (text.Length == 0 ? "" : text.EndsWith(newline) ? newline : newline + newline) + "## " + heading + newline;
        // Do not write into an unclosed code fence, or accept a title altered by Markdown syntax.
        try { _ = Find(candidate, heading); }
        catch (InvalidOperationException ex) { throw new InvalidOperationException("見出しを末尾に追加できません。見出し名や元ノートの閉じていないコードフェンスを確認してください。", ex); }
        return candidate;
    }

    // ATX headings outside YAML and fenced code. Duplicate headings are deliberately rejected.
    public static SectionSpan Find(string text, string heading)
    {
        var matches = Scan(text);
        var selected = matches.Where(x => x.Title == heading).ToArray();
        if (selected.Length != 1) throw new InvalidOperationException(selected.Length == 0
            ? $"見出し「{heading}」が見つかりません。元ノートに作成してください。"
            : $"見出し「{heading}」が複数あります。一意の名前にしてください。");
        var target = selected[0];
        var end = matches.Where(x => x.Start >= target.End && x.Level <= target.Level).Select(x => x.Start).DefaultIfEmpty(text.Length).First();
        return new(target.End, end - target.End, text[target.End..end]);
    }

    public static string Replace(string text, string heading, string content)
    {
        var span = Find(text, heading);
        var newline = text.Contains("\r\n") ? "\r\n" : "\n";
        content = content.Replace("\r\n", "\n").Replace("\n", newline);
        if (content.Length > 0 && !content.EndsWith(newline)) content += newline;
        // A new peer heading would escape the linked region and make subsequent edits ambiguous.
        var candidate = text[..span.Start] + content + text[(span.Start + span.Length)..];
        var check = Find(candidate, heading);
        if (check.Length != content.Length) throw new InvalidOperationException("この範囲には、対象見出しと同じか上位の見出しを追加できません。");
        return candidate;
    }

    public static string ToggleTaskAtLine(string content, int line, bool value)
    {
        var lines = content.Split('\n');
        if (line < 0 || line >= lines.Length) throw new ArgumentOutOfRangeException(nameof(line));
        var match = Regex.Match(lines[line], @"^(\s*(?:[-+*]|\d+[.)])\s+\[)[ xX](\])");
        if (!match.Success) throw new InvalidOperationException("指定行はタスクではありません。");
        var position = match.Groups[1].Length;
        lines[line] = lines[line][..position] + (value ? "x" : " ") + lines[line][(position + 1)..];
        return string.Join("\n", lines);
    }
}
