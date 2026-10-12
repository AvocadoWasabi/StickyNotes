using System.Text.Json;
using StickyNotes;
using StickyNotes.Core;
using System.Windows.Controls;
using System.Windows.Documents;

internal static partial class Program
{
    private static void HeadingFilterTests()
    {
        const string source = "intro\r\n# Work\r\n## Today\r\n- [ ] visible\r\n### Memo\r\nchild\r\n```md\r\n# fake\r\n```\r\n## Tomorrow\r\nhidden\r\n# Personal\r\n## Today\r\nother\r\n";
        var all = new HeadingFilter(source, []);
        Check(all.Headings.Length == 6 && all.Headings[1].Label == "Work › Today", "heading filter: hierarchy and fenced heading exclusion");
        var selection = new[] { all.Headings[1].Key, all.Headings[2].Key };
        var filter = new HeadingFilter(source, selection);
        var document = MarkdownView.Render(source, (_, _) => { }, include: filter.Includes);
        var text = new TextRange(document.ContentStart, document.ContentEnd).Text;
        Check(text.Contains("visible") && text.Contains("child") && !text.Contains("hidden") && !text.Contains("intro") && !text.Contains("other"), "heading filter: selected parent includes children only");
        Check(DocumentRuns(document).Count(r => r.Text == "child") == 1, "heading filter: overlapping selection renders once");
        var run = DocumentRuns(document).Single(r => r.Text.Contains("visible"));
        Check(MarkdownSourceMap.Position(run.ContentStart.GetPositionAtOffset(run.Text.IndexOf("visible", StringComparison.Ordinal))) == source.IndexOf("visible", StringComparison.Ordinal), "heading filter: edit click retains original source offset");
        var toggled = -1;
        document = MarkdownView.Render(source, (line, _) => toggled = line, include: filter.Includes);
        var checkbox = document.Blocks.OfType<System.Windows.Documents.List>().Single().ListItems.FirstListItem.Blocks.OfType<Paragraph>().Single().Inlines.OfType<InlineUIContainer>().Single().Child as CheckBox;
        checkbox!.RaiseEvent(new System.Windows.RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
        Check(toggled == 3, "heading filter: checkbox retains original line");
        var missing = new HeadingFilter(source.Replace("## Today", "## Renamed"), selection.Take(1));
        Check(missing.Missing.Length == 1 && !missing.Includes(0) && missing.Selected.Length == 0, "heading filter: renamed selection does not fall back to full text");
        var duplicate = new HeadingFilter("# Work\n## Today\none\n## Today\ntwo", selection.Take(1));
        Check(duplicate.Missing.Length == 1 && duplicate.Selected.Length == 0, "heading filter: ambiguous duplicate fails closed");
        Check(new HeadingFilter("preface\n\n" + source, selection).Selected.Length == 2, "heading filter: selection survives preceding text insertion");
        var setext = new HeadingFilter("Title\n=====\nbody\nSub\n---\ntext", []);
        Check(setext.Headings.Length == 2 && setext.Headings[1].Level == 2, "heading filter: Setext headings supported");
        var placement = JsonSerializer.Deserialize<NotePlacement>(JsonSerializer.Serialize(new NotePlacement { HeadingFilters = selection.ToList() }))!;
        Check(placement.HeadingFilters.SequenceEqual(selection) && JsonSerializer.Deserialize<NotePlacement>("{}")!.HeadingFilters.Count == 0, "heading filter: saved selection round trip and legacy default");
        var commandSource = "# A\n@calendar today\n# B\nbody";
        var commandFilter = new HeadingFilter(commandSource, [new HeadingFilter(commandSource, []).Headings[1].Key]);
        Check(MarkdownView.FindCalendarCommand(commandSource, commandFilter.Includes) == "", "heading filter: hidden calendar command excluded");
        Check(JsonSerializer.Deserialize<NotePlacement>("{\"HeadingFilters\":null}")!.HeadingFilters.Count == 0, "heading filter: null saved selection uses unfiltered default");
    }

    private static void HeadingFilterWindowTests(NoteWindow note, string path)
    {
        const string markdown = "# First\nhidden\n# Second\nshown\n";
        System.IO.File.WriteAllText(path, markdown);
        var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        T Field<T>(string name) => (T)typeof(NoteWindow).GetField(name, flags)!.GetValue(note)!;
        void Call(string name) => typeof(NoteWindow).GetMethod(name, flags)!.Invoke(note, null);
        note.Placement.HeadingFilters = [new HeadingFilter(markdown, []).Headings[1].Key];
        Call("Reload");
        var preview = Field<FlowDocumentScrollViewer>("preview");
        var text = new TextRange(preview.Document.ContentStart, preview.Document.ContentEnd).Text;
        Check(text.Contains("shown") && !text.Contains("hidden") && Field<Button>("headingFilterButton").Content is DockPanel caption && caption.Children.OfType<TextBlock>().Single().Text == "Second", "heading filter: window preview and active badge reflect saved filter");
        if (Environment.GetEnvironmentVariable("STICKYNOTES_RENDER_DIR") is { Length: > 0 } output)
        {
            var surface = (System.Windows.FrameworkElement)note.Content;
            surface.Measure(new System.Windows.Size(280, 320));
            surface.Arrange(new System.Windows.Rect(0, 0, 280, 320)); surface.UpdateLayout();
            var bitmap = new System.Windows.Media.Imaging.RenderTargetBitmap(280, 320, 96, 96, System.Windows.Media.PixelFormats.Pbgra32);
            var visual = new System.Windows.Media.DrawingVisual();
            using (var drawing = visual.RenderOpen())
            {
                drawing.DrawRectangle(note.Background, null, new System.Windows.Rect(0, 0, 280, 320));
                drawing.DrawRectangle(new System.Windows.Media.VisualBrush(surface), null, new System.Windows.Rect(0, 0, 280, 320));
            }
            bitmap.Render(visual);
            var encoder = new System.Windows.Media.Imaging.PngBitmapEncoder();
            encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bitmap));
            using var stream = System.IO.File.Create(System.IO.Path.Combine(output, "compact-header.png")); encoder.Save(stream);
        }
        Call("BeginEdit");
        Check(Field<TextBox>("editor").Text == markdown && !Field<Button>("headingFilterButton").IsEnabled, "heading filter: editing keeps complete source and disables picker");
        note.FinishFocusEditing();
        Check(System.IO.File.ReadAllText(path) == markdown && Field<Button>("headingFilterButton").IsEnabled, "heading filter: leaving unchanged editor preserves file and restores picker");
    }
}
