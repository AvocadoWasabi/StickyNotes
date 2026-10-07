using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;

namespace StickyNotes.Core;

public static class DailyNoteResolver
{
    public const string RegexExample = @"(?<year>\d{4})-(?<month>\d{2})-(?<day>\d{2})(?:\([^)]+\))?\.md";

    public static void Validate(string pattern) => _ = CreateRegex(pattern);

    public static string ResolveFromPaths(string folder, string pattern, DateTime today, IEnumerable<string> paths, CancellationToken token = default)
    {
        if (!Path.IsPathFullyQualified(folder)) throw new InvalidOperationException(L10n.Text("DailyNoteResolver.Text05"));
        var root = Path.GetFullPath(folder);
        var regex = CreateRegex(pattern);
        string? found = null;
        var timer = Stopwatch.StartNew();
        var count = 0;
        foreach (var path in paths)
        {
            token.ThrowIfCancellationRequested();
            if (++count > 10000 || timer.Elapsed > TimeSpan.FromSeconds(1)) throw new IOException(L10n.Text("DailyNoteResolver.Text07"));
            var relative = Path.GetRelativePath(root, path).Replace('\\', '/');
            if (Path.IsPathRooted(relative) || relative == ".." || relative.StartsWith("../")) continue;
            Match match;
            try { match = regex.Match(relative); }
            catch (RegexMatchTimeoutException ex) { throw new IOException(L10n.Text("DailyNoteResolver.Text08"), ex); }
            bool DatePart(string name, int value) => match.Groups[name].Captures.Count == 1 &&
                int.TryParse(match.Groups[name].Value, NumberStyles.None, CultureInfo.InvariantCulture, out var actual) && actual == value;
            if (!match.Success || !DatePart("year", today.Year) || !DatePart("month", today.Month) || !DatePart("day", today.Day)) continue;
            if (found is not null) throw new IOException(L10n.Text("DailyNoteResolver.Text09") + found + "\n" + path);
            found = path;
        }
        return found ?? throw new DailyNoteMissingException();
    }

    private static Regex CreateRegex(string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern)) throw new InvalidOperationException(L10n.Text("DailyNoteResolver.Text01"));
        if (pattern.Length > 4096) throw new InvalidOperationException(L10n.Text("DailyNoteResolver.Text02"));
        Regex regex;
        try { regex = new Regex(@"\A(?:" + pattern + @")\z", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100)); }
        catch (ArgumentException ex) { throw new InvalidOperationException(L10n.Text("DailyNoteResolver.Text03") + ex.Message, ex); }
        if (!new[] { "year", "month", "day" }.All(regex.GetGroupNames().Contains))
            throw new InvalidOperationException(L10n.Text("DailyNoteResolver.Text04") + RegexExample);
        return regex;
    }

    public static string Resolve(string folder, string pattern, DateTime today, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrWhiteSpace(folder)) throw new InvalidOperationException(L10n.Text("DailyNoteResolver.Text05"));
        var root = NoteFolderMigration.Normalize(folder);
        var regex = CreateRegex(pattern);
        // Check the selected root and its ancestors too; traversal below skips reparse points.
        for (string? parent = root; parent is not null; parent = Path.GetDirectoryName(parent))
            if ((File.GetAttributes(parent) & FileAttributes.ReparsePoint) != 0)
                throw new IOException(L10n.Text("DailyNoteResolver.Text06"));

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
                    throw new IOException(L10n.Text("DailyNoteResolver.Text07"));
                var attributes = File.GetAttributes(entry);
                if ((attributes & FileAttributes.ReparsePoint) != 0) continue;
                if ((attributes & FileAttributes.Directory) != 0) { pending.Push(entry); continue; }
                if (!Path.GetExtension(entry).Equals(".md", StringComparison.OrdinalIgnoreCase)) continue;
                var relative = Path.GetRelativePath(root, entry).Replace(Path.DirectorySeparatorChar, '/');
                Match match;
                try { match = regex.Match(relative); }
                catch (RegexMatchTimeoutException ex) { throw new IOException(L10n.Text("DailyNoteResolver.Text08"), ex); }
                if (!match.Success || !DatePart("year", today.Year) || !DatePart("month", today.Month) || !DatePart("day", today.Day)) continue;
                bool DatePart(string name, int value) => match.Groups[name].Captures.Count == 1 &&
                    int.TryParse(match.Groups[name].Value, NumberStyles.None, CultureInfo.InvariantCulture, out var actual) && actual == value;
                if (found is not null) throw new IOException(L10n.Text("DailyNoteResolver.Text09") + found + "\n" + entry);
                found = entry;
            }
        }
        return found ?? throw new DailyNoteMissingException();
    }
}

public sealed class DailyNoteMissingException() : FileNotFoundException(L10n.Text("DailyNoteResolver.Text10"));
