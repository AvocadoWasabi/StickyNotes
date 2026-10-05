namespace StickyNotes;

public sealed class NoteLinkPreview : StackPanel
{
    internal TextBlock Status { get; } = new() { TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 6, 0, 4), Foreground = Brushes.DimGray };
    internal TextBox Body { get; } = new() { IsReadOnly = true, TextWrapping = TextWrapping.Wrap, Height = 150, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, Visibility = Visibility.Collapsed };

    public NoteLinkPreview()
    {
        Margin = new Thickness(0, 12, 0, 0);
        Children.Add(new TextBlock { Text = "内容プレビュー（Markdown・読み取り専用）", FontWeight = FontWeights.SemiBold });
        Children.Add(Status); Children.Add(Body);
        Update(null, "");
    }

    public void Update(FileSnapshot? source, string heading)
    {
        Body.Text = ""; Body.Visibility = Visibility.Collapsed; Status.Foreground = Brushes.DimGray;
        if (source is null) { Status.Text = "ノートを読み込むと表示します。"; return; }
        try
        {
            var candidate = SectionEditor.EnsureHeading(source.Text, heading);
            var added = candidate != source.Text;
            var text = heading.Length == 0 ? NoteStore.Split(source.Text).Body : SectionEditor.Find(candidate, heading).Content;
            var length = Math.Min(text.Length, DailyNotePreview.CharacterLimit);
            if (length < text.Length && length > 0 && char.IsHighSurrogate(text[length - 1])) length--;
            Body.Text = text[..length]; Body.Visibility = Visibility.Visible;
            Status.Text = Path.GetFileName(source.Path) + (added ? " — 新しい見出しを末尾に追加予定（未保存）" : heading.Length == 0 ? " — 本文全体" : " — " + heading) +
                (length < text.Length ? " — 先頭4000文字まで" : "");
        }
        catch (Exception ex) { Status.Text = "確認できません: " + ex.Message; Status.Foreground = Brushes.DarkRed; }
    }
}
