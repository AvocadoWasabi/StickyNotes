using System.Runtime.CompilerServices;
using System.Windows.Documents;

namespace StickyNotes;

// Kept outside the document's properties so mappings do not affect rendering or XAML export.
internal static class MarkdownSourceMap
{
    private sealed record Mapping(int Start, int End, int[]? Offsets = null, bool Anchor = false);
    private static readonly ConditionalWeakTable<TextElement, Mapping> Mappings = new();

    internal static void Track(TextElement element, int start, int end, bool anchor = false)
    {
        if (!Mappings.TryGetValue(element, out _)) Mappings.Add(element, new(Math.Max(0, start), Math.Max(start, end + 1), Anchor: anchor));
    }

    internal static void Track(Run run, string source, int start, int end)
    {
        if (Mappings.TryGetValue(run, out _)) return;
        start = Math.Clamp(start, 0, source.Length);
        var limit = Math.Clamp(end + 1, start, source.Length);
        var exact = source.IndexOf(run.Text, start, limit - start, StringComparison.Ordinal);
        if (exact >= 0) { Track(run, exact, exact + run.Text.Length - 1); return; }
        // Code blocks can omit indentation and normalize line endings. Map each visible
        // character to its original offset without searching outside this Markdown node.
        var offsets = new int[run.Text.Length + 1];
        var next = start;
        for (var i = 0; i < run.Text.Length; i++)
        {
            var found = source.IndexOf(run.Text[i], next, limit - next);
            offsets[i] = found < 0 ? next : found;
            next = found < 0 ? next : found + 1;
        }
        offsets[^1] = next;
        Mappings.Add(run, new(start, limit, offsets));
    }

    internal static int? Position(TextPointer? position)
    {
        if (position is null) return null;
        // Generated Tasks/Dataview results have their own render mappings; the enclosing query
        // anchor takes precedence so clicking a result edits the source query, not its output.
        for (var parent = position.Parent as FrameworkContentElement; parent is not null; parent = parent.Parent as FrameworkContentElement)
            if (parent is TextElement element && Mappings.TryGetValue(element, out var map) && map.Anchor) return map.Start;
        if (position.Parent is Run run && Mappings.TryGetValue(run, out var text))
        {
            var offset = Math.Clamp(run.ContentStart.GetOffsetToPosition(position), 0, run.Text.Length);
            return text.Offsets is { } offsets ? offsets[offset] : Math.Min(text.Start + offset, text.End);
        }
        foreach (var direction in new[] { LogicalDirection.Forward, LogicalDirection.Backward })
            if (position.GetAdjacentElement(direction) is Run adjacent && Mappings.TryGetValue(adjacent, out var map))
                return direction == LogicalDirection.Forward ? map.Start : map.End;
        for (var parent = position.Parent as FrameworkContentElement; parent is not null; parent = parent.Parent as FrameworkContentElement)
            if (parent is TextElement element && Mappings.TryGetValue(element, out var map)) return map.Start;
        return null;
    }
}
