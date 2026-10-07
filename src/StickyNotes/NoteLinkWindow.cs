namespace StickyNotes;

public sealed class NoteLinkWindow : Window
{
    private readonly bool daily;
    private readonly Func<string>? resolveDaily;
    private int loadVersion;
    private bool closed;
    private FileSnapshot? source;
    private string? selectedPath;
    private string selectedFolder;
    private readonly TextBlock sourceLabel = new() { Text = L10n.Text("NoteLinkWindow.Text01"), TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 8, 0, 12) };
    private readonly TextBlock status = new() { TextWrapping = TextWrapping.Wrap, Foreground = Brushes.DarkRed, Margin = new Thickness(0, 10, 0, 10) };
    internal HeadingPicker Headings { get; } = new();
    internal NoteLinkPreview Preview { get; } = new();
    public NotePlacement? Result { get; private set; }

    public NoteLinkWindow(bool daily) : this(daily, null) { }

    internal NoteLinkWindow(bool daily, Func<string>? resolveDaily)
    {
        this.daily = daily; this.resolveDaily = resolveDaily;
        selectedFolder = App.Current.Config.NotesFolder;
        Title = daily ? L10n.Text("NoteLinkWindow.Text02") : L10n.Text("NoteLinkWindow.Text03");
        Width = 600; SizeToContent = SizeToContent.Height; MaxHeight = SystemParameters.WorkArea.Height;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        var panel = new StackPanel { Margin = new Thickness(22) };
        if (daily)
            panel.Children.Add(new TextBlock { Text = L10n.Text("NoteLinkWindow.Text04"), TextWrapping = TextWrapping.Wrap });
        else
        {
            panel.Children.Add(Ui.Button(L10n.Text("NoteLinkWindow.Text05"), () =>
            {
                var picker = new Microsoft.Win32.OpenFolderDialog();
                if (Directory.Exists(selectedFolder)) picker.InitialDirectory = selectedFolder;
                if (picker.ShowDialog(this) == true) { selectedFolder = picker.FolderName; PickFile(); }
            }));
            panel.Children.Add(Ui.Button(L10n.Text("NoteLinkWindow.Text06"), PickFile));
        }
        panel.Children.Add(sourceLabel);
        panel.Children.Add(Ui.Button(L10n.Text("NoteLinkWindow.Text07"), () => TryLoad(daily ? null : selectedPath)));
        panel.Children.Add(Headings); panel.Children.Add(status);
        var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
        buttons.Children.Add(Ui.Button(L10n.Text("NoteLinkWindow.Text08"), () => DialogResult = false));
        buttons.Children.Add(Ui.Button(L10n.Text("NoteLinkWindow.Text09"), () =>
        {
            try { Result = Prepare(); DialogResult = true; }
            catch (Exception ex) { status.Text = ex.Message; }
        }));
        panel.Children.Add(buttons);
        panel.Children.Add(Preview);
        Headings.Input.TextUpdated += () => Preview.Update(source, Headings.Heading);
        Content = new ScrollViewer { Content = panel, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        Headings.Load(null);
        Closed += (_, _) => { closed = true; loadVersion++; };
        if (daily) Loaded += (_, _) => TryLoad(null);
    }

    private void PickFile()
    {
        var picker = new Microsoft.Win32.OpenFileDialog { Filter = "Markdown|*.md", CheckFileExists = true };
        if (Directory.Exists(selectedFolder)) picker.InitialDirectory = selectedFolder;
        if (picker.ShowDialog(this) == true) TryLoad(picker.FileName);
    }

    private async void TryLoad(string? path)
    {
        var version = ++loadVersion;
        try
        {
            ClearSource();
            var access = App.Current.NoteSources(App.Current.Config);
            path = daily ? await ResolveDailyAsync(access) : path;
            ValidatePath(path);
            selectedPath = path;
            var snapshot = await access.ReadAsync(Path.GetFullPath(path!));
            if (closed || version != loadVersion) return;
            SetSource(snapshot); status.Text = "";
        }
        catch (Exception ex) { if (!closed && version == loadVersion) status.Text = ex.Message; }
    }

    internal void LoadSource(string? path = null)
    {
        ClearSource();
        var access = App.Current.NoteSources(App.Current.Config);
        path = daily ? Task.Run(() => ResolveDailyAsync(access)).GetAwaiter().GetResult() : path;
        ValidatePath(path);
        selectedPath = path;
        SetSource(access.Read(Path.GetFullPath(path!)));
    }

    private static void ValidatePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path) || !Path.IsPathFullyQualified(path) || !Path.GetExtension(path).Equals(".md", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(L10n.Text("NoteLinkWindow.Text11"));
    }

    private Task<string> ResolveDailyAsync(NoteSource access) => resolveDaily is not null ? Task.FromResult(resolveDaily()) :
        access.ResolveDailyAsync(DateTime.Today);

    private void ClearSource()
    {
        source = null; Headings.Load(null); Preview.Update(null, ""); sourceLabel.Text = L10n.Text("NoteLinkWindow.Text10");
    }

    private void SetSource(FileSnapshot snapshot)
    {
        Headings.Load(snapshot.Text);
        source = snapshot;
        Preview.Update(source, Headings.Heading);
        selectedFolder = Path.GetDirectoryName(snapshot.Path)!;
        sourceLabel.Text = (daily ? L10n.Text("NoteLinkWindow.Text12") : L10n.Text("NoteLinkWindow.Text13")) + snapshot.Path;
    }

    internal NotePlacement Prepare()
    {
        if (source is null) throw new InvalidOperationException(L10n.Text("NoteLinkWindow.Text14"));
        var access = App.Current.NoteSources(App.Current.Config);
        if (daily && !string.Equals(Path.GetFullPath(Task.Run(() => ResolveDailyAsync(access)).GetAwaiter().GetResult()), source.Path, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(L10n.Text("NoteLinkWindow.Text15"));
        return access.PrepareLink(source, Headings.Heading, daily, Path.Combine(App.DataDirectory, "backups"));
    }
}
