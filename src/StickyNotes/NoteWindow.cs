using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Windows.Shell;
using System.Windows.Threading;

namespace StickyNotes;

public sealed class NoteWindow : Window
{
    public NotePlacement Placement { get; }
    private readonly App app = App.Current;
    private readonly TextBox editor = new() { AcceptsReturn = true, AcceptsTab = true, TextWrapping = TextWrapping.Wrap, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, FontFamily = new FontFamily("Cascadia Mono,Consolas"), FontSize = 14, Background = Brushes.Transparent, BorderThickness = new Thickness(0), Margin = new Thickness(8), Visibility = Visibility.Collapsed };
    private readonly FlowDocumentScrollViewer preview = new() { IsToolBarVisible = false, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, Background = Brushes.Transparent };
    private readonly TextBlock title = new() { FontWeight = FontWeights.SemiBold, TextTrimming = TextTrimming.CharacterEllipsis, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(12, 0, 3, 0) };
    private readonly TextBlock status = new() { FontSize = 11, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(12, 6, 12, 8), Foreground = Brushes.DarkSlateGray };
    private readonly TextBlock tags = new() { FontSize = 11, Margin = new Thickness(13, 0, 10, 3), Foreground = Brushes.DimGray, TextWrapping = TextWrapping.Wrap };
    private readonly StackPanel eventsPanel = new() { Margin = new Thickness(10, 0, 10, 0) };
    private readonly DispatcherTimer poll = new() { Interval = TimeSpan.FromSeconds(2) };
    private readonly DispatcherTimer geometrySave = new() { Interval = TimeSpan.FromMilliseconds(600) };
    private FileSnapshot? snapshot;
    private string content = "";
    private bool editing, dirty, loading, initialized, busy, closed;
    private DateTime lastCalendarCheck = DateTime.MinValue;
    private string? activeCommand;

    public NoteWindow(NotePlacement placement)
    {
        SetResourceReference(IconProperty, "AppIcon");
        Placement = placement;
        Title = "Markdown Sticky Notes";
        Width = Math.Clamp(placement.Width, 280, 1800); Height = Math.Clamp(placement.Height, 240, 1500);
        MinWidth = 280; MinHeight = 240;
        Left = placement.Left; Top = placement.Top;
        WindowStyle = WindowStyle.None; ResizeMode = ResizeMode.CanResizeWithGrip;
        ShowInTaskbar = app.TestMode; Topmost = placement.Pinned;
        WindowChrome.SetWindowChrome(this, new WindowChrome { CaptionHeight = 0, ResizeBorderThickness = new Thickness(6), CornerRadius = new CornerRadius(0), GlassFrameThickness = new Thickness(0) });
        var outer = new Border { BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(45, 70, 60, 30)), BorderThickness = new Thickness(1) };
        var dock = new DockPanel(); outer.Child = dock; Content = outer;
        var header = new DockPanel { Height = 39, Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(12, 0, 0, 0)) };
        var controls = new StackPanel { Orientation = Orientation.Horizontal };
        controls.Children.Add(Ui.Button("＋", app.NewNote, "新しい付箋"));
        var pin = Ui.Button(placement.Pinned ? "●" : "○", () => { Topmost = !Topmost; Placement.Pinned = Topmost; app.SaveConfig(); }, "最前面を切り替え");
        pin.Click += (_, _) => pin.Content = Topmost ? "●" : "○";
        controls.Children.Add(pin);
        controls.Children.Add(Ui.Button("×", Close, "この付箋を閉じる（ファイルは残ります）"));
        DockPanel.SetDock(controls, Dock.Right); header.Children.Add(controls); header.Children.Add(title);
        header.MouseLeftButtonDown += (_, e) => { if (e.OriginalSource is TextBlock || e.OriginalSource == header) { DragMove(); e.Handled = true; } };
        DockPanel.SetDock(header, Dock.Top); dock.Children.Add(header);
        var toolbar = new WrapPanel { Margin = new Thickness(7, 5, 7, 3) };
        toolbar.Children.Add(Ui.Button("編集", BeginEdit)); toolbar.Children.Add(Ui.Button("保存", () => Save()));
        toolbar.Children.Add(Ui.Button("再読込", ReloadAsked));
        toolbar.Children.Add(Ui.Button("…", Menu));
        DockPanel.SetDock(toolbar, Dock.Top); dock.Children.Add(toolbar);
        DockPanel.SetDock(tags, Dock.Top); dock.Children.Add(tags);
        DockPanel.SetDock(status, Dock.Bottom); dock.Children.Add(status);
        var grid = new Grid();
        var reading = new DockPanel();
        var eventScroll = new ScrollViewer { Content = eventsPanel, MaxHeight = 210, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        DockPanel.SetDock(eventScroll, Dock.Bottom); reading.Children.Add(eventScroll); reading.Children.Add(preview);
        grid.Children.Add(reading); grid.Children.Add(editor); dock.Children.Add(grid);
        editor.TextChanged += (_, _) => { if (!loading) { dirty = true; status.Text = "編集中 • Ctrl+S で保存"; } };
        PreviewKeyDown += (_, e) => { if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.S) { Save(); e.Handled = true; } else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.E) { BeginEdit(); e.Handled = true; } };
        Loaded += (_, _) => { MonitorLayout.Restore(this, Placement); initialized = true; Reload(); poll.Start(); QueueGeometry(); };
        LocationChanged += (_, _) => QueueGeometry(); SizeChanged += (_, _) => QueueGeometry();
        geometrySave.Tick += (_, _) => { geometrySave.Stop(); if (initialized && !closed) app.Safe(() => { MonitorLayout.Capture(this, Placement); app.SaveConfig(); }); };
        poll.Tick += async (_, _) => await Tick();
        Closing += OnClosing;
        Background = Ui.Color(placement.Color);
    }

    private void QueueGeometry() { if (initialized) { MonitorLayout.Capture(this, Placement); geometrySave.Stop(); geometrySave.Start(); } }

    public Action Relocate(IReadOnlyDictionary<string, string> paths)
    {
        var previousPath = Placement.Path;
        var previousSnapshot = snapshot;
        var nextPath = Placement.Path;
        var nextSnapshot = snapshot;
        if (Placement.Path.Length > 0 && paths.TryGetValue(Path.GetFullPath(Placement.Path), out var path)) nextPath = path;
        // Retain the hash and editor state so unsaved text and conflict detection survive the move.
        if (snapshot is not null && paths.TryGetValue(Path.GetFullPath(snapshot.Path), out var snapshotPath))
            nextSnapshot = snapshot with { Path = snapshotPath };
        Placement.Path = nextPath; snapshot = nextSnapshot;
        return () => { Placement.Path = previousPath; snapshot = previousSnapshot; };
    }

    private string ResolvePath()
    {
        if (!Placement.Daily) return Placement.Path;
        if (string.IsNullOrWhiteSpace(app.Config.DailyFolder)) throw new InvalidOperationException("設定でデイリーノートフォルダを選んでください。");
        var root = Path.GetFullPath(app.Config.DailyFolder).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var path = Path.GetFullPath(Path.Combine(root, DateTime.Today.ToString(app.Config.DailyPattern) + ".md"));
        if (!path.StartsWith(root, StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("日付書式がデイリーフォルダ外を指しています。");
        return path;
    }

    private void Reload()
    {
        try
        {
            var fresh = NoteStore.Read(ResolvePath());
            var nextContent = Placement.Heading.Length > 0 ? SectionEditor.Find(fresh.Text, Placement.Heading).Content : NoteStore.Split(fresh.Text).Body;
            snapshot = fresh; content = nextContent; dirty = false; editing = false;
            editor.Visibility = Visibility.Collapsed;
            Render(); status.Text = Placement.Daily ? "今日のノートと連動 • " + Path.GetFileName(fresh.Path) : "保存済み • " + Path.GetFileName(fresh.Path);
        }
        catch (Exception ex)
        {
            snapshot = null; content = ""; activeCommand = null;
            editing = false; dirty = false; editor.Visibility = Visibility.Collapsed;
            preview.Document = new System.Windows.Documents.FlowDocument();
            eventsPanel.Children.Clear(); tags.Text = "";
            title.Text = Placement.Daily ? "今日のノートを待機中" : "元ノートを読み込めません";
            status.Text = ex.Message;
        }
    }

    private void Render()
    {
        if (snapshot is null) return;
        var metadata = NoteStore.Metadata(snapshot.Text);
        if (Placement.Heading.Length == 0) Placement.Color = metadata.Color;
        Background = Ui.Color(Placement.Color);
        title.Text = Placement.Heading.Length > 0 ? (Placement.Daily ? "今日 / " : "連動 / ") + Placement.Heading : metadata.Title;
        Title = title.Text;
        tags.Text = string.Join("  ", metadata.Tags.Select(x => "#" + x)) + (Placement.Heading.Length == 0 ? "   · " + metadata.Status : "");
        preview.Document = MarkdownView.Render(content, ToggleTask);
        var command = MarkdownView.FindCalendarCommand(content);
        if (activeCommand != command) { activeCommand = command; eventsPanel.Children.Clear(); lastCalendarCheck = DateTime.MinValue; }
    }

    private void BeginEdit()
    {
        if (snapshot is null) { Reload(); if (snapshot is null) return; }
        if (editing) return;
        editing = true; loading = true; editor.Text = content; loading = false;
        editor.Visibility = Visibility.Visible; editor.Focus();
        status.Text = "Markdownを編集 • Ctrl+S で保存 • @calendar 日時 検索語";
    }

    private bool Save()
    {
        if (snapshot is null) return false;
        if (!editing || !dirty) { editing = false; editor.Visibility = Visibility.Collapsed; return true; }
        try
        {
            SaveContent(editor.Text); editing = false; dirty = false; editor.Visibility = Visibility.Collapsed;
            Render(); status.Text = "保存しました"; return true;
        }
        catch (Exception ex) { status.Text = ex.Message; MessageBox.Show(this, ex.Message, "保存できません", MessageBoxButton.OK, MessageBoxImage.Warning); return false; }
    }

    private void SaveContent(string next)
    {
        if (snapshot is null) throw new InvalidOperationException("元ファイルを読み込めません。");
        string updated;
        if (Placement.Heading.Length > 0) updated = SectionEditor.Replace(snapshot.Text, Placement.Heading, next);
        else
        {
            var split = NoteStore.Split(snapshot.Text);
            var prefix = snapshot.Text[..(snapshot.Text.Length - split.Body.Length)];
            var newline = snapshot.Text.Contains("\r\n") ? "\r\n" : "\n";
            // Keep all unrelated YAML bytes when only editing the body.
            prefix = Regex.Replace(prefix, @"(?m)^updated:[^\r\n]*", m => "updated: " + DateTimeOffset.Now.ToString("o"), RegexOptions.None, TimeSpan.FromSeconds(1));
            updated = prefix + next.Replace("\r\n", "\n").Replace("\n", newline);
        }
        snapshot = NoteStore.Save(snapshot, updated, Path.Combine(App.DataDirectory, "backups"));
        content = Placement.Heading.Length > 0 ? SectionEditor.Find(updated, Placement.Heading).Content : NoteStore.Split(updated).Body;
    }

    private void ToggleTask(int line, bool value)
    {
        try
        {
            if (editing) throw new InvalidOperationException("先に編集中の内容を保存してください。");
            SaveContent(SectionEditor.ToggleTaskAtLine(content, line, value)); Render(); status.Text = "タスクを保存しました";
        }
        catch (Exception ex) { status.Text = ex.Message; Render(); }
    }

    private void ReloadAsked()
    {
        if (dirty && MessageBox.Show(this, "未保存の編集を破棄して再読込しますか？", "再読込", MessageBoxButton.OKCancel) != MessageBoxResult.OK) return;
        Reload();
    }

    private async Task Tick()
    {
        if (closed) return;
        try
        {
            var path = ResolvePath();
            if (!dirty && !editing)
            {
                if (!File.Exists(path) || snapshot is null || snapshot.Path != path || NoteStore.Read(path).Hash != snapshot.Hash) Reload();
            }
            else if (snapshot is not null && (snapshot.Path != path || NoteStore.Read(snapshot.Path).Hash != snapshot.Hash))
                status.Text = "元ノートの変更または日付切替を検出。編集を保存・退避してから再読込してください。";
            if (!editing && snapshot is not null && (DateTime.Now - lastCalendarCheck).TotalSeconds >= 60) await RefreshCalendar();
        }
        catch (Exception ex) { status.Text = ex.Message; }
    }

    private async Task RefreshCalendar()
    {
        if (busy || string.IsNullOrWhiteSpace(activeCommand)) return;
        busy = true; lastCalendarCheck = DateTime.Now;
        var command = activeCommand; var calendarId = app.Config.CalendarId;
        try
        {
            app.Calendar.Configure(app.Config.GoogleCredentialsFile);
            var items = await app.Calendar.SearchAsync(calendarId, CalendarQuery.Parse(command));
            if (closed || activeCommand != command) return;
            eventsPanel.Children.Clear();
            eventsPanel.Children.Add(new TextBlock { Text = $"GOOGLE CALENDAR · {items.Count}件（最大100件）", FontSize = 11, FontWeight = FontWeights.Bold });
            foreach (var item in items)
            {
                var captured = item;
                var button = new Button { HorizontalContentAlignment = HorizontalAlignment.Left, Content = new TextBlock { Text = $"{item.Start}\n{item.Summary}", TextWrapping = TextWrapping.Wrap }, ToolTip = "クリックして件名・説明を編集" };
                button.Click += async (_, _) => await EditEvent(calendarId, captured);
                eventsPanel.Children.Add(button);
            }
            if (items.Count == 0) eventsPanel.Children.Add(new TextBlock { Text = "該当する予定はありません。" });
            status.Text = "予定を取得しました • " + DateTime.Now.ToString("HH:mm");
        }
        catch (Exception ex) { status.Text = "Calendar: " + ex.Message; }
        finally { busy = false; }
    }

    private async Task EditEvent(string calendarId, CalendarEvent item)
    {
        var values = Ui.Prompt("予定を編集", [new("summary", "件名", item.Summary), new("description", "説明（Googleの元データ）", item.Description, true)], this);
        if (values is null || (values["summary"] == item.Summary && values["description"] == item.Description)) return;
        var confirm = $"次の内容をGoogle Calendarへ送信します。\n参加者がいる場合は変更通知が送信されます。\n\n日時: {item.Start}\n\n件名（変更前）: {item.Summary}\n件名（変更後）: {values["summary"]}\n\n説明（変更前）:\n{item.Description}\n\n説明（変更後）:\n{values["description"]}";
        if (MessageBox.Show(this, confirm, "Google Calendarを更新しますか？", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK) return;
        try { await app.Calendar.UpdateAsync(calendarId, item, values["summary"], values["description"]); status.Text = "Google Calendarを更新しました"; await RefreshCalendar(); }
        catch (Exception ex) { MessageBox.Show(this, ex.Message, "更新できません", MessageBoxButton.OK, MessageBoxImage.Warning); }
    }

    private void Menu()
    {
        var menu = new ContextMenu();
        void Add(string label, Action action) { var item = new MenuItem { Header = label }; item.Click += (_, _) => app.Safe(action); menu.Items.Add(item); }
        Add("タグ・タイトル・状態・色", EditMetadata);
        Add("予定を今すぐ取得", () => { _ = RefreshCalendar(); });
        Add("元ファイルを既定アプリで開く", () => Process.Start(new ProcessStartInfo(ResolvePath()) { UseShellExecute = true }));
        Add("Markdownを開く…", app.OpenNote);
        Add("ノートの見出しを表示…", app.LinkSection);
        Add("設定…", () => new SettingsWindow().ShowDialog());
        Add("アプリを終了（配置を保存）", app.Quit);
        menu.IsOpen = true;
    }

    private void EditMetadata()
    {
        if (Placement.Heading.Length > 0) { MessageBox.Show(this, "連動付箋は元ノートのタグを表示します。タグは元ノート側で編集してください。"); return; }
        if (snapshot is null || !Save()) return;
        var original = NoteStore.Metadata(snapshot.Text);
        var values = Ui.Prompt("付箋のプロパティ", [new("title", "タイトル", original.Title), new("tags", "タグ（空白またはカンマ区切り。階層は project/name）", string.Join(" ", original.Tags)), new("status", "状態（例: active / done / archived）", original.Status), new("color", "色: yellow / green / blue / pink / gray", original.Color)], this);
        if (values is null) return;
        var list = values["tags"].Split([' ', ',', '、', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries).Select(x => x.TrimStart('#')).Distinct().ToArray();
        if (list.Any(t => !Regex.IsMatch(t, @"^[\p{L}\p{N}_/-]+$") || t.All(char.IsDigit))) throw new InvalidOperationException("タグには文字・数字・_・-・/を使用できます。数字のみは使えません。");
        snapshot = NoteStore.Save(snapshot, NoteStore.WithMetadata(snapshot.Text, new(values["title"], list, values["status"], values["color"])), Path.Combine(App.DataDirectory, "backups"));
        Reload(); app.SaveConfig();
    }

    public bool CanClose()
    {
        if (!dirty) return true;
        return MessageBox.Show(this, "変更を保存しますか？", title.Text, MessageBoxButton.YesNoCancel) switch
        {
            MessageBoxResult.Yes => Save(), MessageBoxResult.No => true, _ => false
        };
    }

    private void OnClosing(object? sender, CancelEventArgs e)
    {
        if (!app.Exiting && !CanClose()) { e.Cancel = true; return; }
        closed = true; poll.Stop(); geometrySave.Stop();
        if (!app.Exiting) { app.Notes.Remove(this); app.SaveConfig(); }
    }
}
