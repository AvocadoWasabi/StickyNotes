using System.Globalization;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using Markdig;
using Markdig.Syntax;

namespace StickyNotes;

internal sealed class CalendarCompletion
{
    internal const string Usage = "@calendar 日時 [検索キーワード]\nタイムゾーンは省略できます（Windowsのローカル時刻）。\nキーワードなしでも検索できます。指定日時以降の予定を検索します。";
    private readonly TextBox editor;
    private readonly Window owner;
    private readonly Func<DateTime> today;
    private readonly Popup popup;
    private readonly TextBlock examples;
    private bool composing, inserting;
    private string? dismissedText;
    private int dismissedCaret;
    internal bool IsOpen => popup.IsOpen;

    internal CalendarCompletion(Window owner, TextBox editor, Func<DateTime>? today = null)
    {
        this.owner = owner; this.editor = editor; this.today = today ?? (() => DateTime.Today);
        var panel = new StackPanel { Margin = new Thickness(12), Width = 310 };
        var choose = new Button { Content = "@calendar  —  Google Calendarの予定を検索", Focusable = false,
            HorizontalContentAlignment = HorizontalAlignment.Left, Padding = new Thickness(6) };
        // Keep keyboard focus in the editor so clicking a suggestion cannot trigger blur-save.
        choose.PreviewMouseLeftButtonDown += (_, e) => { e.Handled = true; Complete(); };
        panel.Children.Add(choose);
        panel.Children.Add(new TextBlock { Text = Usage, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 8, 0, 8) });
        examples = new TextBlock { TextWrapping = TextWrapping.Wrap, FontFamily = new FontFamily("Consolas") };
        panel.Children.Add(examples);
        panel.Children.Add(new TextBlock { Text = "Tab / Enter / クリックで挿入 · Escで閉じる\n挿入後、選択された日時を変更して保存してください。", TextWrapping = TextWrapping.Wrap,
            Foreground = Brushes.DimGray, Margin = new Thickness(0, 8, 0, 0) });
        popup = new Popup { PlacementTarget = editor, Placement = PlacementMode.Relative, StaysOpen = true,
            AllowsTransparency = true, Focusable = false,
            Child = new Border { Background = Brushes.White, BorderBrush = Brushes.SlateGray, BorderThickness = new Thickness(1), Child = panel } };
        editor.TextChanged += (_, _) => Update();
        editor.SelectionChanged += (_, _) => Update();
        editor.IsKeyboardFocusWithinChanged += (_, _) => { if (!editor.IsKeyboardFocusWithin) { composing = false; Close(); } };
        editor.IsVisibleChanged += (_, _) => { if (!editor.IsVisible) Close(); };
        editor.AddHandler(ScrollViewer.ScrollChangedEvent, new ScrollChangedEventHandler((_, _) => Close()));
        TextCompositionManager.AddPreviewTextInputStartHandler(editor, (_, _) => { composing = true; Close(); });
        editor.PreviewTextInput += (_, _) =>
        {
            composing = false;
            editor.Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(Update));
        };
        owner.Deactivated += (_, _) => Close();
        owner.LocationChanged += (_, _) => Close();
        owner.SizeChanged += (_, _) => Close();
        owner.Closed += (_, _) => Close();
    }

    internal static int FindStart(string text, int caret, int selectionLength)
    {
        if (selectionLength != 0 || caret <= 0 || caret > text.Length) return -1;
        var start = text.LastIndexOf('\n', caret - 1) + 1;
        var prefix = text[start..caret];
        if (!prefix.StartsWith('@') || !"@calendar".StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) return -1;
        var end = text.IndexOf('\n', caret);
        if (!string.IsNullOrWhiteSpace(text[caret..(end < 0 ? text.Length : end)])) return -1;
        // Match the same top-level paragraph position used by the calendar renderer.
        return Markdown.Parse(text, MarkdownView.Pipeline).OfType<ParagraphBlock>().Any(p => p.Span.Start == start) ? start : -1;
    }

    private string DateExample => today().ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + "T00:00";

    internal void Update()
    {
        if (inserting) return;
        if (composing || !editor.IsVisible || !editor.IsKeyboardFocusWithin || !owner.IsActive ||
            (dismissedText == editor.Text && dismissedCaret == editor.CaretIndex) ||
            FindStart(editor.Text, editor.CaretIndex, editor.SelectionLength) < 0)
        { Close(); return; }
        var caret = editor.GetRectFromCharacterIndex(editor.CaretIndex);
        if (caret.IsEmpty) { Close(); return; }
        examples.Text = $"キーワードなし: @calendar {DateExample}\n絞り込む場合: @calendar {DateExample} 会議";
        popup.HorizontalOffset = caret.Left;
        popup.VerticalOffset = caret.Bottom + 4;
        popup.IsOpen = true;
    }

    internal bool HandleKey(Key key, ModifierKeys modifiers)
    {
        if (!IsOpen || composing || modifiers != ModifierKeys.None) return false;
        if (key == Key.Escape)
        {
            dismissedText = editor.Text; dismissedCaret = editor.CaretIndex; Close(); return true;
        }
        if (key is not (Key.Tab or Key.Enter)) return false;
        Complete(); return true;
    }

    internal void Complete()
    {
        var start = FindStart(editor.Text, editor.CaretIndex, editor.SelectionLength);
        if (!IsOpen || composing || start < 0) return;
        var date = DateExample;
        inserting = true;
        try
        {
            editor.BeginChange();
            try
            {
                editor.Select(start, editor.CaretIndex - start);
                editor.SelectedText = "@calendar " + date;
                editor.Select(start + "@calendar ".Length, date.Length);
            }
            finally { editor.EndChange(); }
            Close();
        }
        finally { inserting = false; }
    }

    internal void Close() => popup.IsOpen = false;
}
