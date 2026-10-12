using System.Windows.Controls.Primitives;

namespace StickyNotes;

public sealed partial class NoteWindow
{
    private readonly Button headingFilterButton;
    private Popup? headingFilterPopup;

    private void UpdateHeadingFilterStatus()
    {
        var filter = new HeadingFilter(content, Placement.HeadingFilters);
        headingFilterButton.Content = filter.Active ? "≡" + (Placement.HeadingFilters.Count > 9 ? "9+" : Placement.HeadingFilters.Count.ToString()) : "≡";
        System.Windows.Automation.AutomationProperties.SetName(headingFilterButton, L10n.Text("HeadingFilter.Title"));
        headingFilterButton.Background = filter.Active ? Brushes.LightSkyBlue : new SolidColorBrush(Color.FromRgb(243, 241, 235));
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
                var check = new CheckBox { Content = new TextBlock { Text = heading.Label, TextWrapping = TextWrapping.Wrap },
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
        headingFilterPopup = new Popup { PlacementTarget = headingFilterButton, Placement = PlacementMode.Bottom, StaysOpen = false,
            Child = new Border { Child = panel, Padding = new Thickness(10), Background = Brushes.White, BorderBrush = Brushes.SlateGray, BorderThickness = new Thickness(1) } };
        var popup = headingFilterPopup;
        panel.PreviewKeyDown += (_, e) => { if (e.Key == System.Windows.Input.Key.Escape) { popup.IsOpen = false; e.Handled = true; headingFilterButton.Focus(); } };
        popup.Closed += (_, _) => { toolbarMenuOpen = false; UpdateButtonOverlay(); };
        toolbarMenuOpen = true; UpdateButtonOverlay(); Populate(); popup.IsOpen = true; search.Focus();
    }
}
