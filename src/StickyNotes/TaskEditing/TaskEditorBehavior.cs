using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using System.Threading;
using System.Text.RegularExpressions;

namespace StickyNotes;

// The window only forwards keys. All task suggestion UI, IME and list continuation live here.
internal sealed class TaskEditorBehavior
{
    private readonly Window owner;
    private readonly TextBox editor;
    private readonly Func<DateTime> today;
    private readonly Func<CancellationToken, Task<bool>> canSuggest;
    private CancellationTokenSource? availability;
    private bool completionEnabled;
    private readonly Popup popup;
    private readonly ListBox choices = new() { Focusable = false, DisplayMemberPath = nameof(TaskSuggestion.Label),
        MaxHeight = 230, MinWidth = 260, BorderThickness = new Thickness(0) };
    private readonly DispatcherTimer delay = new() { Interval = TimeSpan.FromMilliseconds(90) };
    private string? dismissedText, offeredText;
    private int dismissedCaret, offeredCaret;
    private bool inserting, closed;
    internal bool IsComposing { get; private set; }
    internal bool IsOpen => popup.IsOpen;

    internal TaskEditorBehavior(Window owner, TextBox editor, Func<DateTime> today, Func<CancellationToken, Task<bool>> canSuggest)
    {
        this.owner = owner; this.editor = editor; this.today = today; this.canSuggest = canSuggest;
        var panel = new StackPanel(); panel.Children.Add(choices);
        panel.Children.Add(new TextBlock { Text = L10n.Text("TaskEditing.Keys"), Margin = new Thickness(8), Foreground = Brushes.DimGray });
        popup = new Popup { PlacementTarget = editor, Placement = PlacementMode.Relative, StaysOpen = true,
            AllowsTransparency = true, Focusable = false,
            Child = new Border { Background = Brushes.White, BorderBrush = Brushes.SlateGray, BorderThickness = new Thickness(1), Child = panel } };
        choices.PreviewMouseDown += (_, e) =>
        {
            if (e.ChangedButton == MouseButton.Left && ItemsControl.ContainerFromElement(choices, e.OriginalSource as DependencyObject) is ListBoxItem item && item.Content is TaskSuggestion suggestion)
            { e.Handled = true; ApplySuggestion(suggestion); }
        };
        editor.TextChanged += (_, _) => Schedule();
        editor.SelectionChanged += (_, _) => Schedule();
        editor.IsKeyboardFocusWithinChanged += (_, _) =>
        {
            if (editor.IsKeyboardFocusWithin) _ = RefreshAvailability();
            else { CancelAvailability(); SetComposing(false); Close(); }
        };
        editor.IsVisibleChanged += (_, _) => { if (!editor.IsVisible) { CancelAvailability(); SetComposing(false); Close(); } };
        editor.AddHandler(ScrollViewer.ScrollChangedEvent, new ScrollChangedEventHandler((_, _) => Close()));
        TextCompositionManager.AddPreviewTextInputStartHandler(editor, (_, _) => SetComposing(true));
        editor.PreviewTextInput += (_, _) => { SetComposing(false); Schedule(); };
        owner.Deactivated += (_, _) => { CancelAvailability(); Close(); };
        owner.Activated += (_, _) => { if (editor.IsKeyboardFocusWithin) _ = RefreshAvailability(); };
        owner.LocationChanged += (_, _) => Close();
        owner.SizeChanged += (_, _) => Close();
        owner.Closed += (_, _) => { closed = true; CancelAvailability(); Close(); };
        delay.Tick += (_, _) => { delay.Stop(); Update(); };
    }

    internal void SetComposing(bool value) { IsComposing = value; if (value) Close(); }
    private void CancelAvailability() { availability?.Cancel(); availability = null; completionEnabled = false; }
    internal async Task RefreshAvailability()
    {
        CancelAvailability(); Close(); dismissedText = null;
        using var pending = new CancellationTokenSource(); availability = pending;
        try
        {
            var enabled = await canSuggest(pending.Token);
            if (closed || pending.IsCancellationRequested || !ReferenceEquals(availability, pending)) return;
            completionEnabled = enabled; Update();
        }
        catch (Exception) { /* Optional completion stays disabled when detection fails. */ }
        finally { if (ReferenceEquals(availability, pending)) availability = null; }
    }
    private void Schedule()
    {
        if (inserting || closed) return;
        Close(); delay.Start();
    }

    internal void Update()
    {
        delay.Stop();
        if (closed || !completionEnabled || inserting || IsComposing || !owner.IsActive || !editor.IsVisible || !editor.IsKeyboardFocusWithin ||
            (dismissedText == editor.Text && dismissedCaret == editor.CaretIndex)) { Close(); return; }
        TaskSuggestions candidates;
        try { candidates = TaskInput.Suggest(editor.Text, editor.CaretIndex, editor.SelectionLength, today()); }
        catch (RegexMatchTimeoutException) { Close(); return; }
        if (candidates.Items.Length == 0) { Close(); return; }
        var caret = editor.GetRectFromCharacterIndex(editor.CaretIndex);
        if (caret.IsEmpty) { Close(); return; }
        offeredText = editor.Text; offeredCaret = editor.CaretIndex;
        choices.ItemsSource = candidates.Items; choices.SelectedIndex = candidates.SelectFirst ? 0 : -1;
        popup.HorizontalOffset = caret.Left; popup.VerticalOffset = caret.Bottom + 4; popup.IsOpen = true;
    }

    internal bool HandleKey(Key key, ModifierKeys modifiers)
    {
        if (IsComposing || modifiers != ModifierKeys.None) return false;
        // Resolve pending local suggestions before Enter/Tab, even during fast typing.
        if (delay.IsEnabled && key is Key.Enter or Key.Tab or Key.Down or Key.Up or Key.Escape) Update();
        if (IsOpen)
        {
            if (key == Key.Escape)
            { dismissedText = editor.Text; dismissedCaret = editor.CaretIndex; Close(); return true; }
            if (key is Key.Up or Key.Down)
            {
                choices.SelectedIndex = key == Key.Down ? (choices.SelectedIndex + 1) % choices.Items.Count :
                    (choices.SelectedIndex <= 0 ? choices.Items.Count - 1 : choices.SelectedIndex - 1);
                choices.ScrollIntoView(choices.SelectedItem); return true;
            }
            if (key == Key.Tab || (key == Key.Enter && choices.SelectedIndex >= 0))
            { ApplySuggestion((TaskSuggestion)choices.Items[Math.Max(0, choices.SelectedIndex)]); return true; }
        }
        if (key == Key.Enter)
        {
            TaskTextEdit? edit;
            try { edit = TaskInput.ContinueList(editor.Text, editor.CaretIndex, editor.SelectionLength); }
            catch (RegexMatchTimeoutException) { return false; }
            if (edit is not null) { Apply(edit); return true; }
        }
        return false;
    }

    private void ApplySuggestion(TaskSuggestion suggestion)
    {
        if (!IsOpen || IsComposing || offeredText != editor.Text || offeredCaret != editor.CaretIndex || editor.SelectionLength != 0)
        { Close(); return; }
        Apply(suggestion.Edit);
    }

    private void Apply(TaskTextEdit edit)
    {
        inserting = true;
        try
        {
            editor.BeginChange();
            try { editor.Select(edit.Start, edit.Length); editor.SelectedText = edit.Text; editor.Select(edit.Start + edit.Text.Length, 0); }
            finally { editor.EndChange(); }
            Close();
        }
        finally { inserting = false; }
        Schedule();
    }

    private void Close() { delay.Stop(); popup.IsOpen = false; }
}
