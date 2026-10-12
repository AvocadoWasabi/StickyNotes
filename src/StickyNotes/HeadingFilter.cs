using System.Text.Json;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace StickyNotes;

internal sealed record FilterHeading(string Key, string Label, int Level, int Start, int End);

internal sealed class HeadingFilter
{
    internal FilterHeading[] Headings { get; }
    internal FilterHeading[] Selected { get; }
    internal string[] Missing { get; }
    internal bool Active { get; }

    internal HeadingFilter(string markdown, IEnumerable<string>? selection)
    {
        var blocks = Markdown.Parse(markdown, MarkdownView.Pipeline).OfType<HeadingBlock>().ToArray();
        var ancestors = new List<(int Level, string Title)>();
        var headings = new List<FilterHeading>();
        for (var i = 0; i < blocks.Length; i++)
        {
            var block = blocks[i];
            var text = PlainText(block.Inline);
            ancestors.RemoveAll(a => a.Level >= block.Level);
            ancestors.Add((block.Level, text));
            var key = JsonSerializer.Serialize(ancestors.Select(a => new { a.Level, a.Title }));
            var end = blocks.Skip(i + 1).FirstOrDefault(b => b.Level <= block.Level)?.Span.Start ?? markdown.Length;
            headings.Add(new(key, string.Join(" › ", ancestors.Select(a => a.Title)), block.Level, block.Span.Start, end));
        }
        Headings = headings.ToArray();
        var keys = (selection ?? []).Distinct(StringComparer.Ordinal).ToArray();
        Active = keys.Length > 0;
        Selected = Headings.Where(h => keys.Contains(h.Key) && Headings.Count(x => x.Key == h.Key) == 1).ToArray();
        Missing = keys.Where(k => !Selected.Any(h => h.Key == k)).ToArray();
    }

    internal bool Includes(int offset) => !Active || Selected.Any(h => offset >= h.Start && offset < h.End);

    private static string PlainText(Inline? inline) => inline switch
    {
        LiteralInline literal => literal.Content.ToString(),
        CodeInline code => code.Content,
        LineBreakInline => " ",
        ContainerInline container => string.Concat(container.Select(PlainText)),
        _ => ""
    };
}
