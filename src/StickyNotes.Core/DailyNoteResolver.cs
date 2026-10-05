using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;

namespace StickyNotes.Core;

public static class DailyNoteResolver
{
    public const string RegexExample = @"(?<year>\d{4})-(?<month>\d{2})-(?<day>\d{2})(?:\([^)]+\))?\.md";

    public static void Validate(string pattern) => _ = CreateRegex(pattern);

    private static Regex CreateRegex(string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern)) throw new InvalidOperationException("デイリーノートの正規表現を指定してください。");
        if (pattern.Length > 4096) throw new InvalidOperationException("正規表現は4096文字以内で指定してください。");
        Regex regex;
        try { regex = new Regex(@"\A(?:" + pattern + @")\z", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100)); }
        catch (ArgumentException ex) { throw new InvalidOperationException("デイリーノートの正規表現が不正です: " + ex.Message, ex); }
        if (!new[] { "year", "month", "day" }.All(regex.GetGroupNames().Contains))
            throw new InvalidOperationException("今日の日付を判別するため、名前付きグループ year・month・day を指定してください。例: " + RegexExample);
        return regex;
    }

    public static string Resolve(string folder, string pattern, DateTime today, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrWhiteSpace(folder)) throw new InvalidOperationException("設定でデイリーノートフォルダを選んでください。");
        var root = NoteFolderMigration.Normalize(folder);
        var regex = CreateRegex(pattern);
        // Check the selected root and its ancestors too; traversal below skips reparse points.
        for (string? parent = root; parent is not null; parent = Path.GetDirectoryName(parent))
            if ((File.GetAttributes(parent) & FileAttributes.ReparsePoint) != 0)
                throw new IOException("正規表現検索ではリンク・ジャンクションを含むフォルダは使用できません。");

        var timer = Stopwatch.StartNew();
        var pending = new Stack<string>();
        pending.Push(root);
        string? found = null;
        var count = 0;
        while (pending.TryPop(out var directory))
        {
            cancellationToken.ThrowIfCancellationRequested();
            foreach (var entry in Directory.EnumerateFileSystemEntries(directory))
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (++count > 10000 || timer.Elapsed > TimeSpan.FromSeconds(1))
                    throw new IOException("デイリーノートの検索範囲が大きすぎます。対象フォルダを絞ってください（上限10000項目・1秒）。");
                var attributes = File.GetAttributes(entry);
                if ((attributes & FileAttributes.ReparsePoint) != 0) continue;
                if ((attributes & FileAttributes.Directory) != 0) { pending.Push(entry); continue; }
                if (!Path.GetExtension(entry).Equals(".md", StringComparison.OrdinalIgnoreCase)) continue;
                var relative = Path.GetRelativePath(root, entry).Replace(Path.DirectorySeparatorChar, '/');
                Match match;
                try { match = regex.Match(relative); }
                catch (RegexMatchTimeoutException ex) { throw new IOException("正規表現の照合がタイムアウトしました。式を簡単にしてください。", ex); }
                if (!match.Success || !DatePart("year", today.Year) || !DatePart("month", today.Month) || !DatePart("day", today.Day)) continue;
                bool DatePart(string name, int value) => match.Groups[name].Captures.Count == 1 &&
                    int.TryParse(match.Groups[name].Value, NumberStyles.None, CultureInfo.InvariantCulture, out var actual) && actual == value;
                if (found is not null) throw new IOException("今日のデイリーノートが複数一致しました。正規表現を絞ってください。\n" + found + "\n" + entry);
                found = entry;
            }
        }
        return found ?? throw new DailyNoteMissingException();
    }
}

public sealed class DailyNoteMissingException() : FileNotFoundException("正規表現に一致する今日のデイリーノートがありません。");
