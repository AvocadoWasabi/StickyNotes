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
    internal StackPanel NoteControls { get; } = new() { Orientation = Orientation.Horizontal, Margin = new Thickness(4, 0, 4, 3) };
    internal Grid NoteHeader { get; } = new() { Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(12, 0, 0, 0)), Focusable = true };
    private readonly Border controlsHost = new();
    private readonly TextBlock dragHandle = new() { Text = "⠿", Width = 24, TextAlignment = TextAlignment.Center, VerticalAlignment = VerticalAlignment.Stretch, Padding = new Thickness(0, 5, 0, 0), Cursor = Cursors.SizeAll, ToolTip = L10n.Text("NoteWindow.Text01"), Background = Brushes.Transparent };
    private readonly Dictionary<Button, double> controlWidths = new();
    private bool toolbarMenuOpen, headerHovered;
    private readonly App app = App.Current;
    private readonly TextBox editor = new() { AcceptsReturn = true, AcceptsTab = true, TextWrapping = TextWrapping.Wrap, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, FontFamily = new FontFamily("Cascadia Mono,Consolas"), FontSize = 14, Background = Brushes.Transparent, BorderThickness = new Thickness(0), Margin = new Thickness(8), Visibility = Visibility.Collapsed };
    private readonly CalendarCompletion calendarCompletion;
    private readonly FlowDocumentScrollViewer preview = new() { IsToolBarVisible = false, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, Background = Brushes.Transparent };
    private static readonly DependencyPropertyDescriptor PreviewZoom = DependencyPropertyDescriptor.FromProperty(FlowDocumentScrollViewer.ZoomProperty, typeof(FlowDocumentScrollViewer));
    private bool applyingScale;
    private readonly DockPanel reading = new();
    private readonly TextBlock title = new() { FontWeight = FontWeights.SemiBold, TextTrimming = TextTrimming.CharacterEllipsis, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(12, 0, 3, 0) };
    private readonly TextBlock status = new() { FontSize = 11, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(12, 6, 12, 8), Foreground = Brushes.DarkSlateGray };
    private readonly TextBlock tags = new() { FontSize = 11, Margin = new Thickness(13, 0, 10, 3), Foreground = Brushes.DimGray, TextWrapping = TextWrapping.Wrap };
    private readonly StackPanel eventsPanel = new() { Margin = new Thickness(10, 0, 10, 0) };
    private readonly DispatcherTimer poll = new() { Interval = TimeSpan.FromSeconds(2) };
    private readonly DispatcherTimer geometrySave = new() { Interval = TimeSpan.FromMilliseconds(600) };
    private readonly DispatcherTimer focusLossTimer = new() { Interval = TimeSpan.FromMilliseconds(150) };
    private readonly DispatcherTimer temporaryFrontTimer = new() { Interval = TimeSpan.FromSeconds(10) };
    private readonly Func<MessageBoxResult>? confirmFocusSave;
    private readonly Action<Exception>? reportSaveError;
    private bool decisionInProgress, editorContextMenuOpen;
    private FileSnapshot? snapshot;
    private string content = "";
    private bool editing, dirty, loading, initialized, busy, closed;
    private DateTime lastCalendarCheck = DateTime.MinValue;
    private string? activeCommand;
    private readonly DailyNoteDisplay dailyDisplay = new();
    private readonly Func<DateTime> today;
    private DateTime displayedDate;
    private DateTime renderedToday;
    private DailyNoteRetention renderedRetention;
    private readonly TextBlock dailyNotice = new() { TextWrapping = TextWrapping.Wrap, Margin = new Thickness(12, 8, 12, 8), Visibility = Visibility.Collapsed };
    internal static string DailyWaitingMessage => L10n.Text("NoteWindow.Text02");

    public NoteWindow(NotePlacement placement) : this(placement, null, null) { }

    internal NoteWindow(NotePlacement placement, Func<MessageBoxResult>? confirmFocusSave, Action<Exception>? reportSaveError, Func<DateTime>? today = null)
    {
        this.today = today ?? (() => DateTime.Today);
        this.confirmFocusSave = confirmFocusSave; this.reportSaveError = reportSaveError;
        SetResourceReference(IconProperty, "AppIcon");
        Placement = placement;
        Title = "Markdown Sticky Notes";
        Width = Math.Clamp(placement.Width, 280, 1800); Height = Math.Clamp(placement.Height, 240, 1500);
        MinWidth = 280; MinHeight = 240;
        Left = placement.Left; Top = placement.Top;
        WindowStyle = WindowStyle.None; ResizeMode = ResizeMode.CanResizeWithGrip;
        ApplyTaskbarDisplay(); Topmost = placement.Pinned;
        temporaryFrontTimer.Tick += (_, _) => EndTemporaryFront();
        WindowChrome.SetWindowChrome(this, new WindowChrome { CaptionHeight = 0, ResizeBorderThickness = new Thickness(6), CornerRadius = new CornerRadius(0), GlassFrameThickness = new Thickness(0) });
        var outer = new Border { BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(45, 70, 60, 30)), BorderThickness = new Thickness(1) };
        var dock = new DockPanel(); outer.Child = dock; Content = outer;
        var header = NoteHeader;
        header.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        header.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        title.Height = 27;
        header.Children.Add(title);
        Button Control(string label, Action action, double width, string color, string? tooltip = null)
        {
            var button = Ui.Button(label, action, tooltip);
            button.Width = width; button.Height = 28; button.FontSize = 12;
            button.Padding = new Thickness(0); button.Margin = new Thickness(1);
            button.Background = (Brush)new BrushConverter().ConvertFromString(color)!;
            button.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(37, 52, 69));
            button.BorderBrush = Brushes.SlateGray;
            NoteControls.Children.Add(button);
            controlWidths[button] = width;
            return button;
        }
        Control(L10n.Text("NoteWindow.Text03"), BeginEdit, 40, "#DFEAF7", L10n.Text("NoteWindow.Text04"));
        Control(L10n.Text("NoteWindow.Text05"), () => Save(), 40, "#DCEEDC", L10n.Text("NoteWindow.Text06"));
        Control(L10n.Text("NoteWindow.Text07"), ReloadAsked, 58, "#FFF0CC");
        Control("…", Menu, 30, "#E9E0F2", L10n.Text("NoteWindow.Text08"));
        Control("＋", app.NewNote, 28, "#F3F1EB", L10n.Text("NoteWindow.Text09"));
        var pin = Control(placement.Pinned ? "●" : "○", () => { Placement.Pinned = !Placement.Pinned; EndTemporaryFront(); app.SaveConfig(); }, 28, "#F3F1EB", L10n.Text("NoteWindow.Text10"));
        pin.Click += (_, _) => pin.Content = Placement.Pinned ? "●" : "○";
        Control("×", Close, 28, "#F3F1EB", L10n.Text("NoteWindow.Text11"));
        var controlsRow = new DockPanel();
        DockPanel.SetDock(dragHandle, Dock.Left); controlsRow.Children.Add(dragHandle); controlsRow.Children.Add(NoteControls);
        controlsHost.Child = controlsRow; header.Children.Add(controlsHost);
        header.MouseLeftButtonDown += (_, e) => { if (e.OriginalSource == title || e.OriginalSource == header || e.OriginalSource == dragHandle) { DragMove(); e.Handled = true; } };
        header.MouseEnter += (_, _) => { headerHovered = true; UpdateButtonOverlay(); };
        header.MouseLeave += (_, _) => { headerHovered = false; UpdateButtonOverlay(); };
        header.IsKeyboardFocusWithinChanged += (_, _) => UpdateButtonOverlay();
        ApplyButtonDisplay();
        DockPanel.SetDock(header, Dock.Top); dock.Children.Add(header);
        DockPanel.SetDock(tags, Dock.Top); dock.Children.Add(tags);
        DockPanel.SetDock(status, Dock.Bottom); dock.Children.Add(status);
        var grid = new Grid();
        var eventScroll = new ScrollViewer { Content = eventsPanel, MaxHeight = 210, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        var noticeScroll = new ScrollViewer { Content = dailyNotice, MaxHeight = 150, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        noticeScroll.SetBinding(VisibilityProperty, new System.Windows.Data.Binding(nameof(Visibility)) { Source = dailyNotice });
        DockPanel.SetDock(noticeScroll, Dock.Top); reading.Children.Add(noticeScroll);
        DockPanel.SetDock(eventScroll, Dock.Bottom); reading.Children.Add(eventScroll); reading.Children.Add(preview);
        grid.Children.Add(reading); grid.Children.Add(editor); dock.Children.Add(grid);
        calendarCompletion = new CalendarCompletion(this, editor, this.today);
        preview.MinZoom = NotePlacement.MinContentScale; preview.MaxZoom = NotePlacement.MaxContentScale; preview.ZoomIncrement = 10;
        ApplyContentScale();
        PreviewZoom.AddValueChanged(preview, OnPreviewZoomChanged);
        reading.PreviewMouseWheel += (_, e) =>
        {
            if (HandleScaleWheel(e.Delta, Keyboard.Modifiers)) e.Handled = true;
        };
        preview.PreviewMouseLeftButtonDown += (_, e) =>
        {
            if (!IsBodyEditTarget(e.OriginalSource as DependencyObject)) return;
            BeginEdit(); e.Handled = true;
        };
        editor.TextChanged += (_, _) => { if (!loading) { dirty = editor.Text != content; status.Text = L10n.Text("NoteWindow.Text12"); } };
        editor.IsKeyboardFocusWithinChanged += (_, _) => { if (editor.IsKeyboardFocusWithin) focusLossTimer.Stop(); else ScheduleFocusLoss(); };
        Deactivated += (_, _) => ScheduleFocusLoss();
        editor.ContextMenuOpening += (_, _) => { editorContextMenuOpen = true; focusLossTimer.Stop(); };
        editor.ContextMenuClosing += (_, _) => { editorContextMenuOpen = false; ScheduleFocusLoss(); };
        focusLossTimer.Tick += (_, _) =>
        {
            // Let toolbar mouse-up/Click run before deciding whether a blur still needs saving.
            if (Mouse.LeftButton == MouseButtonState.Pressed) return;
            focusLossTimer.Stop();
            if (editorContextMenuOpen || (IsActive && editor.IsKeyboardFocusWithin)) return;
            FinishFocusEditing();
        };
        PreviewKeyDown += (_, e) =>
        {
            if (editor.IsKeyboardFocusWithin && calendarCompletion.HandleKey(e.Key, Keyboard.Modifiers)) { e.Handled = true; return; }
            if (e.Key == Key.F6 && Keyboard.Modifiers == ModifierKeys.None) { NoteHeader.Focus(); e.Handled = true; return; }
            if (HandleScaleKey(e.Key, Keyboard.Modifiers)) { e.Handled = true; return; }
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.S) { Save(); e.Handled = true; }
            else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.E) { BeginEdit(); e.Handled = true; }
        };
        Loaded += (_, _) => { MonitorLayout.Restore(this, Placement); initialized = true; Reload(); poll.Start(); QueueGeometry(); };
        LocationChanged += (_, _) => QueueGeometry(); SizeChanged += (_, _) => QueueGeometry();
        geometrySave.Tick += (_, _) => { geometrySave.Stop(); if (initialized && !closed) app.Safe(() => { MonitorLayout.Capture(this, Placement); app.SaveConfig(); }); };
        poll.Tick += async (_, _) => await Tick();
        Closing += OnClosing;
        Background = Ui.Color(placement.Color);
    }

    private void QueueGeometry() { if (initialized) { MonitorLayout.Capture(this, Placement); geometrySave.Stop(); geometrySave.Start(); } }

    internal void ApplyTaskbarDisplay() => ShowInTaskbar = app.Config.ShowInTaskbar || app.TestMode;

    internal void BringToFrontTemporarily()
    {
        if (closed) return;
        temporaryFrontTimer.Stop();
        // Raising every note must not move keyboard focus away from an unsaved editor.
        var activateOnShow = ShowActivated;
        try
        {
            ShowActivated = false;
            Show();
            if (WindowState == WindowState.Minimized) WindowState = WindowState.Normal;
            Topmost = true;
        }
        finally { ShowActivated = activateOnShow; }
        temporaryFrontTimer.Start();
    }

    private void EndTemporaryFront()
    {
        temporaryFrontTimer.Stop();
        Topmost = Placement.Pinned;
    }

    internal void ApplyButtonDisplay()
    {
        var overlay = app.Config.TitleButtonOverlay;
        headerHovered = NoteHeader.IsMouseOver;
        Grid.SetRow(controlsHost, overlay ? 0 : 1);
        NoteHeader.RowDefinitions[0].MinHeight = overlay ? 33 : 27;
        dragHandle.Visibility = overlay ? Visibility.Visible : Visibility.Collapsed;
        foreach (var (button, width) in controlWidths) button.Width = overlay ? width - 3 : width;
        if (overlay) controlsHost.SetBinding(Border.BackgroundProperty, new System.Windows.Data.Binding(nameof(Background)) { Source = this });
        else { controlsHost.ClearValue(Border.BackgroundProperty); controlsHost.Background = Brushes.Transparent; }
        UpdateButtonOverlay();
    }

    private void UpdateButtonOverlay()
    {
        controlsHost.Visibility = !app.Config.TitleButtonOverlay || headerHovered || NoteHeader.IsKeyboardFocusWithin || toolbarMenuOpen
            ? Visibility.Visible : Visibility.Collapsed;
    }

    private void ApplyContentScale()
    {
        applyingScale = true;
        try
        {
            preview.Zoom = Placement.ContentScale;
            var scale = Placement.ContentScale / 100.0;
            // Zoom the document and the other rendered content, keeping window controls at their normal size.
            eventsPanel.LayoutTransform = new ScaleTransform(scale, scale);
            dailyNotice.LayoutTransform = new ScaleTransform(scale, scale);
        }
        finally { applyingScale = false; }
    }

    private void OnPreviewZoomChanged(object? sender, EventArgs e)
    {
        if (!applyingScale && !closed) app.Safe(() => SetContentScale((int)Math.Round(preview.Zoom)));
    }

    internal void SetContentScale(int value)
    {
        if (closed) return;
        var previous = Placement.ContentScale;
        Placement.ContentScale = value;
        ApplyContentScale();
        if (Placement.ContentScale == previous) return;
        try { app.SaveConfig(); }
        catch { Placement.ContentScale = previous; ApplyContentScale(); throw; }
    }

    internal bool HandleScaleKey(Key key, ModifierKeys modifiers)
    {
        if (editing || (modifiers & ModifierKeys.Control) == 0 || (modifiers & ~(ModifierKeys.Control | ModifierKeys.Shift)) != 0) return false;
        int? next = key switch
        {
            Key.Add or Key.OemPlus => Placement.ContentScale + 10,
            Key.Subtract or Key.OemMinus => Placement.ContentScale - 10,
            Key.D0 or Key.NumPad0 => 100,
            _ => null
        };
        if (next is null) return false;
        app.Safe(() => SetContentScale(next.Value));
        return true;
    }

    internal bool HandleScaleWheel(int delta, ModifierKeys modifiers)
    {
        if (editing || modifiers != ModifierKeys.Control || delta == 0) return false;
        app.Safe(() => SetContentScale(Placement.ContentScale + (delta > 0 ? 10 : -10)));
        return true;
    }

    internal MenuItem ContentScaleMenu()
    {
        var menu = new MenuItem { Header = L10n.Format("NoteWindow.Text13", Placement.ContentScale) };
        void Add(string label, int value, bool enabled = true, bool checkable = false)
        {
            var item = new MenuItem { Header = label, IsEnabled = enabled, IsCheckable = checkable, IsChecked = checkable && Placement.ContentScale == value };
            item.Click += (_, _) => app.Safe(() => SetContentScale(value));
            menu.Items.Add(item);
        }
        Add(L10n.Text("NoteWindow.Text14"), Placement.ContentScale + 10, Placement.ContentScale < NotePlacement.MaxContentScale);
        Add(L10n.Text("NoteWindow.Text15"), Placement.ContentScale - 10, Placement.ContentScale > NotePlacement.MinContentScale);
        Add(L10n.Text("NoteWindow.Text16"), 100);
        menu.Items.Add(new Separator());
        foreach (var value in new[] { 50, 75, 100, 125, 150, 175, 200 }) Add($"{value}%", value, checkable: true);
        return menu;
    }

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
        return dailyDisplay.Resolve(app.Config.DailyFolder, app.Config.DailyPattern, today(), app.Config.DailyRetention);
    }

    private void Reload()
    {
        try
        {
            var fresh = NoteStore.Read(ResolvePath());
            var nextContent = Placement.Heading.Length > 0 ? SectionEditor.Find(fresh.Text, Placement.Heading).Content : NoteStore.Split(fresh.Text).Body;
            snapshot = fresh; content = nextContent; dirty = false; SetEditing(false);
            displayedDate = dailyDisplay.TargetDate;
            Render(); status.Text = Placement.Daily ? L10n.Text("NoteWindow.Text17") + Path.GetFileName(fresh.Path) : L10n.Text("NoteWindow.Text18") + Path.GetFileName(fresh.Path);
        }
        catch (Exception ex) { ShowReadError(ex); }
    }

    private void ShowReadError(Exception error)
    {
        snapshot = null; content = ""; activeCommand = null;
        dirty = false; SetEditing(false);
        preview.Document = new System.Windows.Documents.FlowDocument();
        dailyNotice.Text = Placement.Daily && error is DailyNoteMissingException ? DailyWaitingMessage : "";
        dailyNotice.Visibility = dailyNotice.Text.Length > 0 ? Visibility.Visible : Visibility.Collapsed;
        eventsPanel.Children.Clear(); tags.Text = "";
        title.Text = Placement.Daily ? L10n.Text("NoteWindow.Text19") : L10n.Text("NoteWindow.Text20");
        status.Text = error.Message;
    }

    private void Render()
    {
        if (snapshot is null) return;
        renderedToday = today().Date; renderedRetention = app.Config.DailyRetention;
        var yesterday = Placement.Daily && displayedDate < today().Date;
        dailyNotice.Text = yesterday ? L10n.Format("NoteWindow.Text21", displayedDate) +
            (app.Config.DailyRetention == DailyNoteRetention.UntilRefresh ? L10n.Text("NoteWindow.Text22") : L10n.Text("NoteWindow.Text23")) : "";
        dailyNotice.Visibility = yesterday ? Visibility.Visible : Visibility.Collapsed;
        var metadata = NoteStore.Metadata(snapshot.Text);
        if (Placement.Heading.Length == 0) Placement.Color = metadata.Color;
        Background = Ui.Color(Placement.Color);
        title.Text = Placement.Heading.Length > 0 ? (Placement.Daily ? (yesterday ? L10n.Text("NoteWindow.Text24") : L10n.Text("NoteWindow.Text25")) : L10n.Text("NoteWindow.Text26")) + Placement.Heading : metadata.Title;
        Title = title.Text;
        tags.Text = string.Join("  ", metadata.Tags.Select(x => "#" + x)) + (Placement.Heading.Length == 0 ? "   · " + metadata.Status : "");
        preview.Document = MarkdownView.Render(content, ToggleTask);
        var command = MarkdownView.FindCalendarCommand(content);
        if (activeCommand != command) { activeCommand = command; eventsPanel.Children.Clear(); lastCalendarCheck = DateTime.MinValue; }
    }

    private void BeginEdit()
    {
        focusLossTimer.Stop();
        if (snapshot is null) { Reload(); if (snapshot is null) return; }
        if (!editing)
        {
            loading = true; editor.Text = content; loading = false;
            SetEditing(true);
        }
        editor.Focus();
        // Focus again after layout so a just-revealed editor can receive keyboard/IME input.
        Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(() =>
        {
            if (!closed && editing && IsActive) Keyboard.Focus(editor);
        }));
        status.Text = L10n.Text("NoteWindow.Text27");
    }

    private void SetEditing(bool value)
    {
        focusLossTimer.Stop();
        editing = value;
        reading.Visibility = value ? Visibility.Collapsed : Visibility.Visible;
        editor.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
    }

    private bool Save()
    {
        focusLossTimer.Stop();
        var previousDecision = decisionInProgress;
        decisionInProgress = true;
        try { return SaveCore(); }
        finally { decisionInProgress = previousDecision; }
    }

    private bool SaveCore()
    {
        if (snapshot is null) return false;
        if (!editing || !dirty) { SetEditing(false); return true; }
        try
        {
            SaveContent(editor.Text); dirty = false; SetEditing(false);
            Render(); status.Text = L10n.Text("NoteWindow.Text28"); return true;
        }
        catch (Exception ex)
        {
            status.Text = ex.Message;
            if (reportSaveError is not null) reportSaveError(ex);
            else MessageBox.Show(this, ex.Message, L10n.Text("NoteWindow.Text29"), MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }
    }

    internal static bool IsBodyEditTarget(DependencyObject? target)
    {
        for (var current = target; current is not null;)
        {
            if (current is System.Windows.Controls.Primitives.ButtonBase or System.Windows.Controls.Primitives.ScrollBar or System.Windows.Controls.Primitives.Thumb or System.Windows.Documents.Hyperlink) return false;
            current = current is FrameworkContentElement contentElement ? contentElement.Parent
                : current is Visual ? VisualTreeHelper.GetParent(current) : LogicalTreeHelper.GetParent(current);
        }
        return target is not null;
    }

    private void ScheduleFocusLoss()
    {
        if (closed || !editing || decisionInProgress || editorContextMenuOpen) return;
        focusLossTimer.Stop(); focusLossTimer.Start();
    }

    internal void FinishFocusEditing()
    {
        focusLossTimer.Stop();
        if (closed || !editing || decisionInProgress || editorContextMenuOpen) return;
        decisionInProgress = true;
        try
        {
            if (!dirty) { SetEditing(false); return; }
            var answer = app.Config.AutoSaveOnFocusLoss ? MessageBoxResult.Yes : confirmFocusSave?.Invoke() ?? MessageBox.Show(this,
                L10n.Text("NoteWindow.Text30"),
                L10n.Text("NoteWindow.Text31"), MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Cancel);
            if (answer == MessageBoxResult.Yes) { if (!Save() && IsActive) editor.Focus(); }
            else if (answer == MessageBoxResult.No) Reload();
            else if (IsActive) editor.Focus();
        }
        finally { decisionInProgress = false; }
    }

    private void SaveContent(string next)
    {
        if (snapshot is null) throw new InvalidOperationException(L10n.Text("NoteWindow.Text32"));
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
            if (editing) throw new InvalidOperationException(L10n.Text("NoteWindow.Text33"));
            SaveContent(SectionEditor.ToggleTaskAtLine(content, line, value)); Render(); status.Text = L10n.Text("NoteWindow.Text34");
        }
        catch (Exception ex) { status.Text = ex.Message; Render(); }
    }

    private void ReloadAsked()
    {
        focusLossTimer.Stop();
        decisionInProgress = true;
        try
        {
            if (dirty && MessageBox.Show(this, L10n.Text("NoteWindow.Text35"), L10n.Text("NoteWindow.Text36"), MessageBoxButton.OKCancel) != MessageBoxResult.OK) return;
            dailyDisplay.Refresh(today());
            Reload();
        }
        finally { decisionInProgress = false; }
    }

    private async Task Tick()
    {
        if (closed) return;
        try
        {
            var path = ResolvePath();
            if (!dirty && !editing)
            {
                if (!File.Exists(path) || snapshot is null || snapshot.Path != path || NoteStore.Read(path).Hash != snapshot.Hash ||
                    (Placement.Daily && (renderedToday != today().Date || renderedRetention != app.Config.DailyRetention))) Reload();
            }
            else if (snapshot is not null && (snapshot.Path != path || NoteStore.Read(snapshot.Path).Hash != snapshot.Hash))
                status.Text = L10n.Text("NoteWindow.Text37");
            if (!editing && snapshot is not null && (DateTime.Now - lastCalendarCheck).TotalSeconds >= 60) await RefreshCalendar();
        }
        catch (Exception ex)
        {
            if (!dirty && !editing) ShowReadError(ex);
            else status.Text = ex.Message;
        }
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
            eventsPanel.Children.Add(new TextBlock { Text = L10n.Format("NoteWindow.Text38", items.Count), FontSize = 11, FontWeight = FontWeights.Bold });
            foreach (var item in items)
            {
                var captured = item;
                var button = new Button { HorizontalContentAlignment = HorizontalAlignment.Left, Content = new TextBlock { Text = $"{item.Start}\n{item.Summary}", TextWrapping = TextWrapping.Wrap }, ToolTip = L10n.Text("NoteWindow.Text39") };
                button.Click += async (_, _) => await EditEvent(calendarId, captured);
                eventsPanel.Children.Add(button);
            }
            if (items.Count == 0) eventsPanel.Children.Add(new TextBlock { Text = L10n.Text("NoteWindow.Text40") });
            status.Text = L10n.Text("NoteWindow.Text41") + DateTime.Now.ToString("HH:mm");
        }
        catch (Exception ex) { status.Text = "Calendar: " + ex.Message; }
        finally { busy = false; }
    }

    private async Task EditEvent(string calendarId, CalendarEvent item)
    {
        var values = Ui.Prompt(L10n.Text("NoteWindow.Text42"), [new("summary", L10n.Text("NoteWindow.Text43"), item.Summary), new("description", L10n.Text("NoteWindow.Text44"), item.Description, true)], this);
        if (values is null || (values["summary"] == item.Summary && values["description"] == item.Description)) return;
        var confirm = L10n.Format("NoteWindow.Text45", item.Start, item.Summary, values["summary"], item.Description, values["description"]);
        if (MessageBox.Show(this, confirm, L10n.Text("NoteWindow.Text46"), MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK) return;
        try { await app.Calendar.UpdateAsync(calendarId, item, values["summary"], values["description"]); status.Text = L10n.Text("NoteWindow.Text47"); await RefreshCalendar(); }
        catch (Exception ex) { MessageBox.Show(this, ex.Message, L10n.Text("NoteWindow.Text48"), MessageBoxButton.OK, MessageBoxImage.Warning); }
    }

    private void Menu()
    {
        var menu = new ContextMenu();
        toolbarMenuOpen = true; UpdateButtonOverlay();
        menu.Closed += (_, _) => { toolbarMenuOpen = false; UpdateButtonOverlay(); };
        void Add(string label, Action action) { var item = new MenuItem { Header = label }; item.Click += (_, _) => app.Safe(action); menu.Items.Add(item); }
        Add(L10n.Text("NoteWindow.Text49"), EditMetadata);
        menu.Items.Add(ContentScaleMenu());
        Add(L10n.Text("NoteWindow.Text50"), app.BringNotesToFrontTemporarily);
        Add(L10n.Text("NoteWindow.Text51"), () => { _ = RefreshCalendar(); });
        Add(L10n.Text("NoteWindow.Text52"), () => Process.Start(new ProcessStartInfo(ResolvePath()) { UseShellExecute = true }));
        Add(L10n.Text("NoteWindow.Text53"), app.OpenNote);
        Add(L10n.Text("NoteWindow.Text54"), app.LinkSection);
        Add(L10n.Text("NoteWindow.Text55"), app.LinkDaily);
        Add(L10n.Text("NoteWindow.Text56"), () => new SettingsWindow().ShowDialog());
        Add(L10n.Text("NoteWindow.Text57"), app.Quit);
        menu.IsOpen = true;
    }

    private void EditMetadata()
    {
        if (Placement.Heading.Length > 0) { MessageBox.Show(this, L10n.Text("NoteWindow.Text58")); return; }
        if (snapshot is null || !Save()) return;
        var original = NoteStore.Metadata(snapshot.Text);
        var values = Ui.Prompt(L10n.Text("NoteWindow.Text59"), [new("title", L10n.Text("NoteWindow.Text60"), original.Title), new("tags", L10n.Text("NoteWindow.Text61"), string.Join(" ", original.Tags)), new("status", L10n.Text("NoteWindow.Text62"), original.Status), new("color", L10n.Text("NoteWindow.Text63"), original.Color)], this);
        if (values is null) return;
        var list = values["tags"].Split([' ', ',', '、', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries).Select(x => x.TrimStart('#')).Distinct().ToArray();
        if (list.Any(t => !Regex.IsMatch(t, @"^[\p{L}\p{N}_/-]+$") || t.All(char.IsDigit))) throw new InvalidOperationException(L10n.Text("NoteWindow.Text64"));
        snapshot = NoteStore.Save(snapshot, NoteStore.WithMetadata(snapshot.Text, new(values["title"], list, values["status"], values["color"])), Path.Combine(App.DataDirectory, "backups"));
        Reload(); app.SaveConfig();
    }

    public bool CanClose()
    {
        if (decisionInProgress) return false;
        focusLossTimer.Stop();
        decisionInProgress = true;
        try
        {
            if (!dirty) return true;
            return MessageBox.Show(this, L10n.Text("NoteWindow.Text65"), title.Text, MessageBoxButton.YesNoCancel) switch
            {
                MessageBoxResult.Yes => Save(), MessageBoxResult.No => true, _ => false
            };
        }
        finally { decisionInProgress = false; }
    }

    private void OnClosing(object? sender, CancelEventArgs e)
    {
        if (!app.Exiting && !CanClose()) { e.Cancel = true; return; }
        closed = true; poll.Stop(); geometrySave.Stop(); focusLossTimer.Stop(); temporaryFrontTimer.Stop();
        PreviewZoom.RemoveValueChanged(preview, OnPreviewZoomChanged);
        if (!app.Exiting) { app.Notes.Remove(this); app.SaveConfig(); }
    }
}
