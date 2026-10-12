namespace StickyNotes;

public static class Ui
{
    public sealed record Field(string Key, string Label, string Value, bool Multiline = false);
    public static Button Button(string label, Action action, string? tooltip = null)
    {
        var button = new Button { Content = label, ToolTip = tooltip ?? label };
        button.SetResourceReference(ContentControl.ContentTemplateProperty, "WrappingButtonContent");
        button.Click += (_, _) => App.Current.Safe(action);
        return button;
    }

    public static Dictionary<string, string>? Prompt(string title, IEnumerable<Field> fields, Window? owner = null)
    {
        var window = new Window { Title = title, Width = 540, SizeToContent = SizeToContent.Height, MaxHeight = 760,
            WindowStartupLocation = owner is null ? WindowStartupLocation.CenterScreen : WindowStartupLocation.CenterOwner, Owner = owner,
            Background = Brushes.WhiteSmoke, ResizeMode = ResizeMode.CanResize };
        var panel = new StackPanel { Margin = new Thickness(22) };
        var inputs = new Dictionary<string, TextBox>();
        foreach (var field in fields)
        {
            panel.Children.Add(new TextBlock { Text = field.Label, TextWrapping = TextWrapping.Wrap });
            var input = new TextBox { Text = field.Value, AcceptsReturn = field.Multiline, TextWrapping = TextWrapping.Wrap,
                MinHeight = field.Multiline ? 130 : 32, MaxHeight = field.Multiline ? 270 : 70, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
            inputs[field.Key] = input;
            panel.Children.Add(input);
        }
        var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
        buttons.Children.Add(Button(L10n.Text("Ui.Text01"), () => window.DialogResult = false));
        buttons.Children.Add(Button("OK", () => window.DialogResult = true));
        panel.Children.Add(buttons);
        window.Content = new ScrollViewer { Content = panel, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        return window.ShowDialog() == true ? inputs.ToDictionary(p => p.Key, p => p.Value.Text) : null;
    }

    public static Brush Color(string name) => (Brush)new BrushConverter().ConvertFromString(name switch
    {
        "green" => "#EDF4E8", "blue" => "#EDF3F9", "pink" => "#FAEEF1", "gray" => "#F1F0ED", _ => "#FFF8DE"
    })!;
}
