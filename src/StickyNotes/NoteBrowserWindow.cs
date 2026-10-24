using System.Threading;
using System.Windows.Documents;

namespace StickyNotes;

internal sealed class NoteBrowserWindow : Window
{
    private sealed record Entry(string Path, string Label) { public override string ToString() => Label; }
    private readonly NoteSource source;
    private readonly Action<string> open;
    private readonly Func<FileSnapshot, Task>? delete;
    private readonly Func<string, bool>? confirmDelete;
    private FileSnapshot? selectedSnapshot;
    private bool deleting;
    private readonly CancellationTokenSource lifetime = new();
    private CancellationTokenSource? listing, reading;
    private bool closed, initializing;
    private string[] paths = [];
    internal ComboBox Folder { get; } = new() { MinWidth = 200 };
    internal TextBox Search { get; } = new();
    internal ListBox Notes { get; } = new();
    internal FlowDocumentScrollViewer Preview { get; } = new() { IsToolBarVisible = false, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
    internal TextBox Yaml { get; } = new() { IsReadOnly = true, AcceptsReturn = true, TextWrapping = TextWrapping.Wrap, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
    internal TextBlock Status { get; } = new() { TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 8, 0, 8) };
    internal Button Open { get; } = new() { Content = L10n.Text("NoteBrowser.Open"), IsEnabled = false, MinWidth = 100 };
    internal Button Delete { get; } = new() { Content = L10n.Text("NoteBrowser.Delete"), IsEnabled = false, MinWidth = 100 };
    internal Button CloseButton { get; } = new() { Name = "CloseBrowser", Content = L10n.Text("NoteBrowser.Close"), IsCancel = true, MinWidth = 100 };
    private readonly Button refresh;

    internal NoteBrowserWindow(NoteSource source, Action<string> open, Func<FileSnapshot, Task>? delete = null,
        Func<string, bool>? confirmDelete = null)
    {
        this.source = source; this.open = open;
        this.delete = delete; this.confirmDelete = confirmDelete;
        Title = L10n.Text("NoteBrowser.Title"); Width = 900; Height = 620; MinWidth = 600; MinHeight = 400;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        var root = new DockPanel { Margin = new Thickness(16) };
        var header = new StackPanel(); DockPanel.SetDock(header, Dock.Top);
        header.Children.Add(new TextBlock { Text = L10n.Text("NoteBrowser.Help"), TextWrapping = TextWrapping.Wrap });
        header.Children.Add(Folder);
        header.Children.Add(new TextBlock { Text = L10n.Text("NoteBrowser.Search"), Margin = new Thickness(0, 8, 0, 2) });
        header.Children.Add(Search); header.Children.Add(Status); root.Children.Add(header);
        var footer = new WrapPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 10, 0, 0) };
        DockPanel.SetDock(footer, Dock.Bottom);
        refresh = Ui.Button(L10n.Text("NoteBrowser.Refresh"), () => _ = (Folder.SelectedItem is null ? InitializeAsync() : LoadFolderAsync()));
        refresh.Name = "RefreshNotes";
        footer.Children.Add(refresh); footer.Children.Add(Delete);
        footer.Children.Add(Open); footer.Children.Add(CloseButton); root.Children.Add(footer);
        var grid = new Grid(); grid.ColumnDefinitions.Add(new() { Width = new GridLength(2, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new() { Width = new GridLength(3, GridUnitType.Star) });
        grid.Children.Add(Notes);
        var tabs = new TabControl { Margin = new Thickness(12, 0, 0, 0) }; Grid.SetColumn(tabs, 1);
        tabs.Items.Add(new TabItem { Header = L10n.Text("NoteBrowser.Preview"), Content = Preview });
        tabs.Items.Add(new TabItem { Header = "YAML", Content = Yaml }); grid.Children.Add(tabs);
        root.Children.Add(grid); Content = root;
        Search.TextChanged += (_, _) => Filter();
        Folder.SelectionChanged += (_, _) => { if (!initializing) _ = LoadFolderAsync(); };
        Notes.SelectionChanged += (_, _) => _ = LoadPreviewAsync();
        Notes.MouseDoubleClick += (_, e) => { if (e.OriginalSource is DependencyObject element && ItemsControl.ContainerFromElement(Notes, element) is ListBoxItem) OpenSelected(); };
        Open.Click += (_, _) => OpenSelected();
        CloseButton.Click += (_, _) => Close();
        Delete.Click += async (_, _) => await DeleteSelectedAsync();
        Loaded += async (_, _) => await InitializeAsync();
        Closing += (_, e) => { if (deleting) e.Cancel = true; };
        Closed += (_, _) => { closed = true; lifetime.Cancel(); listing?.Cancel(); reading?.Cancel(); lifetime.Dispose(); };
    }

    internal async Task InitializeAsync()
    {
        if (initializing || closed) return;
        initializing = true;
        try
        {
            Status.Text = L10n.Text("NoteBrowser.Loading");
            var catalog = await source.GetFoldersAsync(lifetime.Token);
            var initial = await source.EffectiveNotesFolderAsync();
            if (closed) return;
            Folder.ItemsSource = catalog.Folders;
            Folder.SelectedItem = StickyFolderPath.Relative(source.FolderRoot, initial);
            if (Folder.SelectedIndex < 0) Folder.SelectedItem = ".";
        }
        catch (Exception ex) { if (!closed) Status.Text = ex.Message; }
        finally { initializing = false; }
        if (!closed && Folder.SelectedItem is not null) await LoadFolderAsync();
    }

    internal async Task LoadFolderAsync()
    {
        if (closed || deleting || Folder.SelectedItem is not string relative) return;
        listing?.Cancel(); reading?.Cancel();
        using var cancel = CancellationTokenSource.CreateLinkedTokenSource(lifetime.Token); listing = cancel;
        paths = []; Filter(); Status.Text = L10n.Text("NoteBrowser.Loading");
        try
        {
            var result = await source.BrowseNotesAsync(StickyFolderPath.Absolute(source.FolderRoot, relative), cancel.Token);
            if (closed || cancel.IsCancellationRequested) return;
            paths = result; Status.Text = L10n.Format("NoteBrowser.Count", paths.Length); Filter();
        }
        catch (OperationCanceledException) when (cancel.IsCancellationRequested) { }
        catch (Exception ex) { if (!closed && !cancel.IsCancellationRequested) Status.Text = ex.Message; }
        finally { if (ReferenceEquals(listing, cancel)) listing = null; }
    }

    private void Filter()
    {
        var selected = (Notes.SelectedItem as Entry)?.Path;
        Notes.ItemsSource = paths.Where(p => Path.GetRelativePath(source.FolderRoot, p).Contains(Search.Text, StringComparison.CurrentCultureIgnoreCase))
            .Select(p => new Entry(p, Path.GetRelativePath(source.FolderRoot, p))).ToArray();
        Notes.SelectedItem = Notes.Items.Cast<Entry>().FirstOrDefault(e => e.Path == selected);
        if (Notes.SelectedItem is null && Notes.Items.Count > 0) Notes.SelectedIndex = 0;
        if (Notes.SelectedItem is null) { reading?.Cancel(); ClearPreview(); }
    }

    private void ClearPreview() { Preview.Document = new FlowDocument(); Yaml.Text = ""; selectedSnapshot = null; Open.IsEnabled = Delete.IsEnabled = false; }

    internal async Task LoadPreviewAsync()
    {
        reading?.Cancel(); ClearPreview();
        if (closed || deleting || Notes.SelectedItem is not Entry entry) return;
        using var cancel = CancellationTokenSource.CreateLinkedTokenSource(lifetime.Token); reading = cancel;
        try
        {
            var snapshot = await source.ReadAsync(entry.Path, cancel.Token);
            if (closed || cancel.IsCancellationRequested) return;
            var split = NoteStore.Split(snapshot.Text);
            // Previews never execute embedded queries, scripts or write callbacks.
            var length = Math.Min(50000, split.Body.Length);
            if (length < split.Body.Length && char.IsHighSurrogate(split.Body[length - 1])) length--;
            var body = split.Body[..length];
            Preview.Document = MarkdownView.Render(body, (_, _) => { }, readOnly: true);
            Yaml.Text = split.Frontmatter; Open.IsEnabled = true;
            selectedSnapshot = snapshot; Delete.IsEnabled = delete is not null;
            Status.Text = entry.Label + (split.Body.Length > body.Length ? L10n.Text("NoteBrowser.Truncated") : "");
        }
        catch (OperationCanceledException) when (cancel.IsCancellationRequested) { }
        catch (Exception ex) { if (!closed && !cancel.IsCancellationRequested) Status.Text = ex.Message; }
        finally { if (ReferenceEquals(reading, cancel)) reading = null; }
    }

    internal void OpenSelected()
    {
        if (closed || deleting || !Open.IsEnabled || Notes.SelectedItem is not Entry entry) return;
        try { open(entry.Path); Close(); }
        catch (Exception ex) { Status.Text = ex.Message; }
    }

    internal async Task DeleteSelectedAsync()
    {
        if (closed || deleting || !Delete.IsEnabled || delete is null || selectedSnapshot is not { } expected) return;
        deleting = true;
        Folder.IsEnabled = Search.IsEnabled = Notes.IsEnabled = refresh.IsEnabled = Open.IsEnabled = Delete.IsEnabled = CloseButton.IsEnabled = false;
        var deleted = false;
        string? error = null;
        try
        {
            var confirmed = confirmDelete?.Invoke(expected.Path) ?? MessageBox.Show(this,
                L10n.Format("NoteBrowser.DeleteConfirm", expected.Path), L10n.Text("NoteBrowser.Delete"),
                MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.Yes;
            if (!confirmed) return;
            Status.Text = L10n.Text("NoteBrowser.Deleting");
            await delete(expected);
            deleted = true;
        }
        catch (Exception ex) { error = ex.Message; }
        finally
        {
            deleting = false;
            Folder.IsEnabled = Search.IsEnabled = Notes.IsEnabled = refresh.IsEnabled = CloseButton.IsEnabled = true;
            if (deleted) await LoadFolderAsync();
            else if (error is not null) ClearPreview();
            else Open.IsEnabled = Delete.IsEnabled = true;
            if (error is not null) Status.Text = error;
        }
    }
}
