namespace StickyNotes;

// Used by both fixed-note and daily-note linking dialogs.
public sealed class HeadingPicker : StackPanel
{
    internal ComboBox Input { get; } = new() { IsEditable = true, IsTextSearchEnabled = false, MinHeight = 32 };
    public string Heading => Input.Text.Trim();

    public HeadingPicker()
    {
        Children.Add(new TextBlock { Text = "表示する見出し名", TextWrapping = TextWrapping.Wrap });
        Children.Add(Input);
        Children.Add(new TextBlock { Text = "既存の見出しを選択、または新しい名前を入力してください（# は不要）。空欄なら本文を全表示します。新しい見出しは「表示」を押すと元ファイルの末尾に追加します。", TextWrapping = TextWrapping.Wrap, Foreground = Brushes.DimGray });
    }

    public void Load(string? markdown)
    {
        var text = Input.Text;
        Input.ItemsSource = markdown is null ? Array.Empty<string>() : SectionEditor.Headings(markdown);
        Input.Text = text;
        IsEnabled = markdown is not null;
    }
}
