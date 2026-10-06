using System.Diagnostics;
using System.Windows.Documents;
using Markdig;
using Markdig.Extensions.Tables;
using Markdig.Extensions.TaskLists;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace StickyNotes;

public static class MarkdownView
{
    internal static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();

    public static string FindCalendarCommand(string markdown)
    {
        foreach (var paragraph in Markdown.Parse(markdown, Pipeline).OfType<ParagraphBlock>())
        {
            var line = markdown[paragraph.Span.Start..(paragraph.Span.End + 1)].Split('\n')[0].TrimEnd('\r');
            if (line.StartsWith("@calendar ", StringComparison.OrdinalIgnoreCase)) return line;
        }
        return "";
    }

    public static FlowDocument Render(string markdown, Action<int, bool> toggle)
    {
        var result = new FlowDocument { FontFamily = new FontFamily("Yu Gothic UI"), FontSize = 14,
            PagePadding = new Thickness(5), Background = Brushes.Transparent, Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(52, 48, 35)) };
        foreach (var block in Markdown.Parse(markdown, Pipeline)) AddBlock(result.Blocks, block, toggle);
        return result;
    }

    private static void AddBlock(BlockCollection output, Markdig.Syntax.Block block, Action<int, bool> toggle)
    {
        switch (block)
        {
            case HeadingBlock heading:
                var title = new Paragraph { FontSize = heading.Level switch { 1 => 25, 2 => 21, _ => 17 }, FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 12, 0, 7) };
                AddInlines(title.Inlines, heading.Inline, toggle, heading.Line); output.Add(title); break;
            case ParagraphBlock paragraph:
                var p = new Paragraph { Margin = new Thickness(0, 3, 0, 9), LineHeight = 23 };
                AddInlines(p.Inlines, paragraph.Inline, toggle, paragraph.Line); output.Add(p); break;
            case ListBlock list:
                var rendered = new System.Windows.Documents.List { MarkerStyle = list.IsOrdered ? TextMarkerStyle.Decimal : TextMarkerStyle.Disc, Padding = new Thickness(22, 0, 0, 0), Margin = new Thickness(0, 2, 0, 8) };
                foreach (ListItemBlock item in list)
                {
                    var li = new ListItem();
                    foreach (var child in item) AddBlock(li.Blocks, child, toggle);
                    rendered.ListItems.Add(li);
                }
                output.Add(rendered); break;
            case QuoteBlock quote:
                var section = new Section { BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(177, 161, 107)), BorderThickness = new Thickness(3, 0, 0, 0), Padding = new Thickness(12, 0, 0, 0), Margin = new Thickness(0, 6, 0, 10) };
                foreach (var child in quote) AddBlock(section.Blocks, child, toggle);
                output.Add(section); break;
            case CodeBlock code:
                output.Add(new Paragraph(new Run(code.Lines.ToString())) { FontFamily = new FontFamily("Cascadia Mono,Consolas"), FontSize = 12, Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(20, 0, 0, 0)), Padding = new Thickness(10), Margin = new Thickness(0, 5, 0, 10) }); break;
            case ThematicBreakBlock:
                output.Add(new Paragraph { BorderBrush = Brushes.DarkGray, BorderThickness = new Thickness(0, 0, 0, 1), Margin = new Thickness(0, 10, 0, 10) }); break;
            case Markdig.Extensions.Tables.Table table:
                var grid = new System.Windows.Documents.Table { CellSpacing = 0, FontSize = 12 };
                var group = new TableRowGroup(); grid.RowGroups.Add(group);
                foreach (Markdig.Extensions.Tables.TableRow row in table)
                {
                    var tr = new System.Windows.Documents.TableRow();
                    foreach (Markdig.Extensions.Tables.TableCell cell in row)
                    {
                        var tc = new System.Windows.Documents.TableCell { Padding = new Thickness(5), BorderThickness = new Thickness(0.5), BorderBrush = Brushes.DarkGray, FontWeight = row.IsHeader ? FontWeights.Bold : FontWeights.Normal };
                        foreach (var child in cell) AddBlock(tc.Blocks, child, toggle);
                        tr.Cells.Add(tc);
                    }
                    group.Rows.Add(tr);
                }
                output.Add(grid); break;
            case HtmlBlock html:
                output.Add(new Paragraph(new Run(html.Lines.ToString())) { FontFamily = new FontFamily("Consolas"), FontSize = 12 }); break;
            case ContainerBlock container:
                foreach (var child in container) AddBlock(output, child, toggle); break;
            case LeafBlock leaf:
                var fallback = new Paragraph(); AddInlines(fallback.Inlines, leaf.Inline, toggle, leaf.Line); output.Add(fallback); break;
        }
    }

    private static void AddInlines(InlineCollection output, ContainerInline? container, Action<int, bool> toggle, int line)
    {
        if (container is null) return;
        foreach (var inline in container)
        {
            switch (inline)
            {
                case LiteralInline literal: output.Add(new Run(literal.Content.ToString())); break;
                case CodeInline code: output.Add(new Run(code.Content) { FontFamily = new FontFamily("Consolas"), Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(20, 0, 0, 0)) }); break;
                case LineBreakInline: output.Add(new LineBreak()); break;
                case TaskList task:
                    var checkbox = new CheckBox { IsChecked = task.Checked, Margin = new Thickness(0, 0, 7, 0), VerticalAlignment = VerticalAlignment.Center };
                    checkbox.Click += (_, _) => toggle(line, checkbox.IsChecked == true);
                    output.Add(new InlineUIContainer(checkbox) { BaselineAlignment = BaselineAlignment.Center }); break;
                case EmphasisInline emphasis:
                    Span span = emphasis.DelimiterChar == '~' ? new Span { TextDecorations = TextDecorations.Strikethrough } : emphasis.DelimiterCount >= 2 ? new Bold() : new Italic();
                    AddInlines(span.Inlines, emphasis, toggle, line); output.Add(span); break;
                case LinkInline link:
                    if (link.IsImage)
                    {
                        output.Add(new Run(L10n.Text("MarkdownView.Text01"))); AddInlines(output, link, toggle, line); output.Add(new Run("]")); break;
                    }
                    var hyperlink = new Hyperlink(); AddInlines(hyperlink.Inlines, link, toggle, line);
                    if (Uri.TryCreate(link.Url, UriKind.Absolute, out var uri) && uri.Scheme is "https" or "http" or "mailto" or "obsidian")
                    {
                        hyperlink.NavigateUri = uri;
                        hyperlink.RequestNavigate += (_, args) => { App.Current.Safe(() => Process.Start(new ProcessStartInfo(args.Uri.AbsoluteUri) { UseShellExecute = true })); args.Handled = true; };
                    }
                    output.Add(hyperlink); break;
                case AutolinkInline auto:
                    var text = auto.IsEmail ? "mailto:" + auto.Url : auto.Url;
                    if (Uri.TryCreate(text, UriKind.Absolute, out var autoUri) && autoUri.Scheme is "https" or "http" or "mailto")
                    {
                        var a = new Hyperlink(new Run(auto.Url)) { NavigateUri = autoUri };
                        a.RequestNavigate += (_, e) => { App.Current.Safe(() => Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true })); e.Handled = true; };
                        output.Add(a);
                    }
                    else output.Add(new Run(auto.Url));
                    break;
                case HtmlInline html: output.Add(new Run(html.Tag)); break;
                case ContainerInline nested: AddInlines(output, nested, toggle, line); break;
            }
        }
    }
}
