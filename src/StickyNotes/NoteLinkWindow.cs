namespace StickyNotes;

public sealed class NoteLinkWindow : Window
{
    private readonly bool daily;
    private readonly Func<string> resolveDaily;
    private FileSnapshot? source;
    private string? selectedPath;
    private string selectedFolder;
    private readonly TextBlock sourceLabel = new() { Text = "Markdownファイルを選択してください。", TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 8, 0, 12) };
    private readonly TextBlock status = new() { TextWrapping = TextWrapping.Wrap, Foreground = Brushes.DarkRed, Margin = new Thickness(0, 10, 0, 10) };
    internal HeadingPicker Headings { get; } = new();
    public NotePlacement? Result { get; private set; }

    public NoteLinkWindow(bool daily) : this(daily, () => DailyNoteResolver.Resolve(App.Current.Config.DailyFolder, App.Current.Config.DailyPattern, DateTime.Today)) { }

    internal NoteLinkWindow(bool daily, Func<string> resolveDaily)
    {
        this.daily = daily; this.resolveDaily = resolveDaily;
        selectedFolder = App.Current.Config.NotesFolder;
        Title = daily ? "デイリーノートを付箋にする" : "ノートの一部分を付箋にする";
        Width = 600; SizeToContent = SizeToContent.Height; MaxHeight = SystemParameters.WorkArea.Height;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        var panel = new StackPanel { Margin = new Thickness(22) };
        if (daily)
            panel.Children.Add(new TextBlock { Text = "設定のフォルダと正規表現から今日のノートを選び、日付が変わると自動で切り替えます。", TextWrapping = TextWrapping.Wrap });
        else
        {
            panel.Children.Add(Ui.Button("フォルダを選択…", () =>
            {
                var picker = new Microsoft.Win32.OpenFolderDialog();
                if (Directory.Exists(selectedFolder)) picker.InitialDirectory = selectedFolder;
                if (picker.ShowDialog(this) == true) { selectedFolder = picker.FolderName; PickFile(); }
            }));
            panel.Children.Add(Ui.Button("Markdownファイルを選択…", PickFile));
        }
        panel.Children.Add(sourceLabel);
        panel.Children.Add(Ui.Button("再読込", () => TryLoad(daily ? null : selectedPath)));
        panel.Children.Add(Headings); panel.Children.Add(status);
        var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
        buttons.Children.Add(Ui.Button("キャンセル", () => DialogResult = false));
        buttons.Children.Add(Ui.Button("表示", () =>
        {
            try { Result = Prepare(); DialogResult = true; }
            catch (Exception ex) { status.Text = ex.Message; }
        }));
        panel.Children.Add(buttons);
        Content = new ScrollViewer { Content = panel, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        Headings.Load(null);
        if (daily) Loaded += (_, _) => TryLoad(null);
    }

    private void PickFile()
    {
        var picker = new Microsoft.Win32.OpenFileDialog { Filter = "Markdown|*.md", CheckFileExists = true };
        if (Directory.Exists(selectedFolder)) picker.InitialDirectory = selectedFolder;
        if (picker.ShowDialog(this) == true) TryLoad(picker.FileName);
    }

    private void TryLoad(string? path)
    {
        try { LoadSource(path); status.Text = ""; }
        catch (Exception ex) { status.Text = ex.Message; }
    }

    internal void LoadSource(string? path = null)
    {
        source = null; Headings.Load(null); sourceLabel.Text = "元ノートを読み込めません。";
        path = daily ? resolveDaily() : path;
        if (string.IsNullOrWhiteSpace(path) || !Path.IsPathFullyQualified(path) || !Path.GetExtension(path).Equals(".md", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("既存のMarkdownファイルを選択してください。");
        selectedPath = path;
        var snapshot = NoteStore.Read(Path.GetFullPath(path));
        Headings.Load(snapshot.Text);
        source = snapshot;
        selectedFolder = Path.GetDirectoryName(snapshot.Path)!;
        sourceLabel.Text = (daily ? "今日のノート: " : "選択したノート: ") + snapshot.Path;
    }

    internal NotePlacement Prepare()
    {
        if (source is null) throw new InvalidOperationException("先にMarkdownファイルを読み込んでください。");
        if (daily && !string.Equals(Path.GetFullPath(resolveDaily()), source.Path, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("今日のノートが変わりました。再読込して見出しを選択してください。");
        return NoteLink.Prepare(source, Headings.Heading, daily, Path.Combine(App.DataDirectory, "backups"));
    }
}
