using System.Windows.Controls.Primitives;

namespace StickyNotes;

public sealed partial class NoteWindow
{
    private readonly Button headingFilterButton;
    private Popup? headingFilterPopup;

    private void UpdateHeadingFilterStatus()
    {
        var filter = new HeadingFilter(content, Placement.HeadingFilters);
        var label = !filter.Active ? L10n.Text("HeadingFilter.Short") :
            filter.Selected.Length == 1 && Placement.HeadingFilters.Count == 1 ? filter.Selected[0].Title : L10n.Format("HeadingFilter.Count", Placement.HeadingFilters.Count);
        var caption = new DockPanel { Width = 78, LastChildFill = true };
        var arrow = new System.Windows.Shapes.Path { Data = Geometry.Parse("M 0,0 L 4,4 L 8,0"), Stroke = Brushes.SlateGray,
            StrokeThickness = 1.5, Width = 9, Height = 5, Margin = new Thickness(4, 0, 1, 0), VerticalAlignment = VerticalAlignment.Center };
        DockPanel.SetDock(arrow, Dock.Right); caption.Children.Add(arrow);
        caption.Children.Add(new TextBlock { Text = label, TextTrimming = TextTrimming.CharacterEllipsis, VerticalAlignment = VerticalAlignment.Center });
        headingFilterButton.Content = caption;
        System.Windows.Automation.AutomationProperties.SetName(headingFilterButton, L10n.Text("HeadingFilter.Title"));
        headingFilterButton.Background = filter.Active ? new SolidColorBrush(Color.FromRgb(222, 235, 242)) : Brushes.Transparent;
        headingFilterButton.ToolTip = L10n.Text("HeadingFilter.Title") + (filter.Active ? "\n" + string.Join("\n", filter.Selected.Select(h => h.Label)) : "");
        headingFilterButton.IsEnabled = !editing && snapshot is not null;
    }

    private void SetHeadingFilters(List<string> keys)
    {
        var previous = Placement.HeadingFilters;
        Placement.HeadingFilters = keys;
        try { app.SaveConfig(); }
        catch { Placement.HeadingFilters = previous; throw; }
        Render();
    }

    private void OpenHeadingFilter()
    {
        if (editing || snapshot is null) return;
        if (headingFilterPopup?.IsOpen == true) { headingFilterPopup.IsOpen = false; return; }
        var panel = new StackPanel { Width = Math.Max(180, Math.Min(320, ActualWidth - 28)) };
        panel.Children.Add(new TextBlock { Text = L10n.Text("HeadingFilter.Title"), FontWeight = FontWeights.Bold });
        panel.Children.Add(new TextBlock { Text = L10n.Text("HeadingFilter.Search"), Foreground = Brushes.DimGray, FontSize = 11, Margin = new Thickness(0, 6, 0, 0) });
        var search = new TextBox { Margin = new Thickness(0, 6, 0, 6), ToolTip = L10n.Text("HeadingFilter.Search") };
        System.Windows.Automation.AutomationProperties.SetName(search, L10n.Text("HeadingFilter.Search"));
        panel.Children.Add(search);
        var rows = new StackPanel();
        panel.Children.Add(new ScrollViewer { Content = rows, MaxHeight = Math.Max(90, Math.Min(300, ActualHeight - 140)), VerticalScrollBarVisibility = ScrollBarVisibility.Auto });
        void Populate()
        {
            rows.Children.Clear();
            var filter = new HeadingFilter(content, Placement.HeadingFilters);
            foreach (var heading in filter.Headings.Where(h => h.Label.Contains(search.Text, StringComparison.CurrentCultureIgnoreCase)))
            {
                var unique = filter.Headings.Count(h => h.Key == heading.Key) == 1;
                var showPath = search.Text.Length > 0 || filter.Headings.Count(h => h.Title == heading.Title) > 1;
                var check = new CheckBox { Content = new TextBlock { Text = showPath ? heading.Label : heading.Title, TextWrapping = TextWrapping.Wrap },
                    IsChecked = Placement.HeadingFilters.Contains(heading.Key), IsEnabled = unique,
                    Margin = new Thickness(Math.Min(heading.Level - 1, 3) * 8, 4, 0, 4),
                    ToolTip = unique ? heading.Label : L10n.Text("HeadingFilter.Missing") };
                check.Click += (_, _) => app.Safe(() =>
                {
                    var keys = Placement.HeadingFilters.ToList();
                    if (check.IsChecked == true) keys.Add(heading.Key); else keys.Remove(heading.Key);
                    try { SetHeadingFilters(keys); }
                    catch { Populate(); throw; }
                });
                rows.Children.Add(check);
            }
            if (filter.Headings.Length == 0) rows.Children.Add(new TextBlock { Text = L10n.Text("HeadingFilter.Empty"), TextWrapping = TextWrapping.Wrap });
            if (filter.Missing.Length > 0) rows.Children.Add(new TextBlock { Text = L10n.Text("HeadingFilter.Missing"), TextWrapping = TextWrapping.Wrap });
        }
        search.TextChanged += (_, _) => Populate();
        panel.Children.Add(Ui.Button(L10n.Text("HeadingFilter.Clear"), () => { SetHeadingFilters([]); Populate(); }));
        headingFilterPopup = new Popup { PlacementTarget = NoteHeader, Placement = PlacementMode.Custom, StaysOpen = false,
            CustomPopupPlacementCallback = (popupSize, targetSize, _) => [new CustomPopupPlacement(new Point(targetSize.Width - popupSize.Width, targetSize.Height), PopupPrimaryAxis.Horizontal)],
            Child = new Border { Child = panel, Padding = new Thickness(10), Background = Brushes.White, BorderBrush = Brushes.SlateGray, BorderThickness = new Thickness(1) } };
        var popup = headingFilterPopup;
        panel.PreviewKeyDown += (_, e) => { if (e.Key == System.Windows.Input.Key.Escape) { popup.IsOpen = false; e.Handled = true; headingFilterButton.Focus(); } };
        popup.Closed += (_, _) => { toolbarMenuOpen = false; UpdateButtonOverlay(); };
        toolbarMenuOpen = true; UpdateButtonOverlay(); Populate(); popup.IsOpen = true; search.Focus();
    }
}
