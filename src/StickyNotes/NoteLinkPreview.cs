namespace StickyNotes;

public sealed class NoteLinkPreview : StackPanel
{
    internal TextBlock Status { get; } = new() { TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 6, 0, 4), Foreground = Brushes.DimGray };
    internal TextBox Body { get; } = new() { IsReadOnly = true, TextWrapping = TextWrapping.Wrap, Height = 150, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, Visibility = Visibility.Collapsed };

    public NoteLinkPreview()
    {
        Margin = new Thickness(0, 12, 0, 0);
        Children.Add(new TextBlock { Text = L10n.Text("NoteLinkPreview.Text01"), FontWeight = FontWeights.SemiBold });
        Children.Add(Status); Children.Add(Body);
        Update(null, "");
    }

    public void Update(FileSnapshot? source, string heading)
    {
        Body.Text = ""; Body.Visibility = Visibility.Collapsed; Status.Foreground = Brushes.DimGray;
        if (source is null) { Status.Text = L10n.Text("NoteLinkPreview.Text02"); return; }
        try
        {
            var candidate = SectionEditor.EnsureHeading(source.Text, heading);
            var added = candidate != source.Text;
            var text = heading.Length == 0 ? NoteStore.Split(source.Text).Body : SectionEditor.Find(candidate, heading).Content;
            var length = Math.Min(text.Length, DailyNotePreview.CharacterLimit);
            if (length < text.Length && length > 0 && char.IsHighSurrogate(text[length - 1])) length--;
            Body.Text = text[..length]; Body.Visibility = Visibility.Visible;
            Status.Text = Path.GetFileName(source.Path) + (added ? L10n.Text("NoteLinkPreview.Text03") : heading.Length == 0 ? L10n.Text("NoteLinkPreview.Text04") : " — " + heading) +
                (length < text.Length ? L10n.Text("NoteLinkPreview.Text05") : "");
        }
        catch (Exception ex) { Status.Text = L10n.Text("NoteLinkPreview.Text06") + ex.Message; Status.Foreground = Brushes.DarkRed; }
    }
}
