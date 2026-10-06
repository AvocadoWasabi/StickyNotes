namespace StickyNotes;

// Used by both fixed-note and daily-note linking dialogs.
public sealed class HeadingPicker : StackPanel
{
    internal HeadingComboBox Input { get; } = new() { IsEditable = true, IsTextSearchEnabled = false, MinHeight = 32 };
    public string Heading => Input.Text.Trim();

    public HeadingPicker()
    {
        Children.Add(new TextBlock { Text = L10n.Text("HeadingPicker.Text01"), TextWrapping = TextWrapping.Wrap });
        Children.Add(Input);
        Children.Add(new TextBlock { Text = L10n.Text("HeadingPicker.Text02"), TextWrapping = TextWrapping.Wrap, Foreground = Brushes.DimGray });
    }

    public void Load(string? markdown)
    {
        var text = Input.Text;
        Input.ItemsSource = markdown is null ? Array.Empty<string>() : SectionEditor.Headings(markdown);
        Input.Text = text;
        IsEnabled = markdown is not null;
    }
}

internal sealed class HeadingComboBox : ComboBox
{
    public event Action? TextUpdated;
    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.Property == TextProperty) TextUpdated?.Invoke();
    }
}
