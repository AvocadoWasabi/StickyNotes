using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using StickyNotes;

internal static partial class Program
{
    private static void CompactRenderingTests()
    {
        var tight = MarkdownView.Render("- [ ] one\n- [x] two\n", (_, _) => { });
        var list = tight.Blocks.OfType<System.Windows.Documents.List>().Single();
        Check(list.MarkerStyle == TextMarkerStyle.None, "compact tasks omit redundant bullets");
        var paragraph = list.ListItems.FirstListItem.Blocks.OfType<Paragraph>().Single();
        Check(paragraph.LineHeight == 21 && paragraph.Margin.Bottom == 2 && paragraph.Margin.Left == 20 && paragraph.TextIndent == -20,
            "compact tasks reserve a checkbox column and align wrapped text");
        Check(paragraph.Inlines.OfType<Run>().First().Text == "one", "task marker whitespace does not misalign the first line");
        var loose = MarkdownView.Render("- [ ] one\n\n- [ ] two\n", (_, _) => { }).Blocks.OfType<System.Windows.Documents.List>().Single();
        Check(loose.ListItems.FirstListItem.Margin.Bottom > list.ListItems.FirstListItem.Margin.Bottom, "intentional blank lines keep a looser list rhythm");
        var mixed = MarkdownView.Render("- plain\n- [ ] task\n- plain again\n", (_, _) => { }).Blocks.OfType<System.Windows.Documents.List>().ToArray();
        Check(mixed.Select(l => l.MarkerStyle).SequenceEqual(new[] { TextMarkerStyle.Disc, TextMarkerStyle.None, TextMarkerStyle.Disc }), "mixed lists retain bullets only on ordinary items");
        var ordered = MarkdownView.Render("3. third\n4. fourth\n", (_, _) => { }).Blocks.OfType<System.Windows.Documents.List>().Single();
        Check(ordered.StartIndex == 3 && ordered.MarkerStyle == TextMarkerStyle.Decimal, "ordered lists retain the original starting number");
        var table = MarkdownView.Render("| A |\n| --- |\n| B |", (_, _) => { }).Blocks.OfType<Table>().Single();
        Check(table.RowGroups[0].Rows[0].Cells[0].Blocks.OfType<Paragraph>().Single().Margin == new Thickness(0), "table spacing belongs to cells without extra paragraph margins");
        var nested = MarkdownView.Render("- [ ] parent\n  - [ ] child", (_, _) => { }).Blocks.OfType<System.Windows.Documents.List>().Single();
        Check(nested.ListItems.FirstListItem.Blocks.OfType<System.Windows.Documents.List>().Single().Margin.Left == 18, "nested tasks use a compact distinct indent");
        var output = Environment.GetEnvironmentVariable("STICKYNOTES_RENDER_DIR");
        if (!string.IsNullOrEmpty(output))
        {
            System.IO.Directory.CreateDirectory(output);
            var document = MarkdownView.Render("# 今日の仕事\n## 今日\n- [ ] 短いタスク A\n- [ ] 短いタスク B\n- [x] 完了したタスク C\n- [ ] 折り返し確認：長いタスクの文章が二行になった場合の行間と左端の位置を比較する\n  - [ ] 子タスク A\n  - [ ] 子タスク B\n\n### 通常の箇条書き\n- 項目 A\n- 項目 B\n\n### 空行のあるリスト\n- [ ] 項目 A\n\n- [ ] 項目 B\n", (_, _) => { });
            foreach (var width in new[] { 280, 360 })
            {
                document.PageWidth = width; document.PageHeight = 700; document.ColumnWidth = width;
                var paginator = ((IDocumentPaginatorSource)document).DocumentPaginator;
                var page = paginator.GetPage(0);
                var visual = new System.Windows.Media.DrawingVisual();
                using (var drawing = visual.RenderOpen())
                {
                    drawing.DrawRectangle(Ui.Color("yellow"), null, new Rect(0, 0, width, 700));
                    drawing.DrawRectangle(new System.Windows.Media.VisualBrush(page.Visual), null, new Rect(0, 0, width, 700));
                }
                var bitmap = new System.Windows.Media.Imaging.RenderTargetBitmap(width, 700, 96, 96, System.Windows.Media.PixelFormats.Pbgra32);
                bitmap.Render(visual);
                var encoder = new System.Windows.Media.Imaging.PngBitmapEncoder();
                encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bitmap));
                using var stream = System.IO.File.Create(System.IO.Path.Combine(output, $"compact-{width}.png"));
                encoder.Save(stream);
            }
        }
    }
}
