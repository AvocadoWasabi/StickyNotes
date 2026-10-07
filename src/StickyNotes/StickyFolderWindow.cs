using System.ComponentModel;
using System.Threading;

namespace StickyNotes;

internal sealed class StickyFolderWindow : Window
{
    internal ComboBox Folder { get; } = new() { Name = "StickyFolder", IsEditable = true, IsTextSearchEnabled = false };
    internal Button Confirm { get; } = new() { Name = "ConfirmStickyFolder", Content = L10n.Text("StickyFolder.Confirm"), IsEnabled = false };
    internal TextBlock Status { get; } = new() { TextWrapping = TextWrapping.Wrap };
    internal bool Committed { get; private set; }
    private bool working, closed;
    private readonly CancellationTokenSource cancellation = new();

    internal StickyFolderWindow(NoteSource source, string? selected, Func<string, bool, Task> save,
        Func<MessageBoxResult>? confirmMigration = null)
    {
        Title = L10n.Text("StickyFolder.Title"); Width = 560; Height = 360;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        var panel = new StackPanel { Margin = new Thickness(20) };
        panel.Children.Add(new TextBlock { Text = source.FolderRoot, TextWrapping = TextWrapping.Wrap, FontWeight = FontWeights.SemiBold });
        panel.Children.Add(new TextBlock { Text = L10n.Text("StickyFolder.Help"), TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 12, 0, 12) });
        panel.Children.Add(Folder); panel.Children.Add(Confirm); panel.Children.Add(Status);
        Content = new ScrollViewer { Content = panel, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        Loaded += async (_, _) =>
        {
            try
            {
                var catalog = await source.GetFoldersAsync(cancellation.Token);
                if (closed) return;
                Folder.ItemsSource = catalog.Folders;
                Folder.Text = selected ?? catalog.DefaultFolder;
                Confirm.IsEnabled = true;
            }
            catch (OperationCanceledException) { }
            catch (Exception ex) { if (!closed) Status.Text = ex.Message; }
        };
        Confirm.Click += async (_, _) =>
        {
            try
            {
                var relative = StickyFolderPath.Normalize(Folder.Text);
                var answer = confirmMigration?.Invoke() ?? MessageBox.Show(this,
                    L10n.Format("StickyFolder.Ask", StickyFolderPath.Absolute(source.FolderRoot, relative)),
                    Title, MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Cancel);
                if (answer is not (MessageBoxResult.Yes or MessageBoxResult.No)) return;
                working = true; Confirm.IsEnabled = Folder.IsEnabled = false;
                Status.Text = L10n.Text("StickyFolder.Working");
                await save(relative, answer == MessageBoxResult.Yes);
                Committed = true; working = false; Close();
            }
            catch (Exception ex) { Status.Text = ex.Message; }
            finally { working = false; if (!closed) Confirm.IsEnabled = Folder.IsEnabled = true; }
        };
        Closing += (_, e) => { if (working) e.Cancel = true; };
        Closed += (_, _) => { closed = true; cancellation.Cancel(); };
    }
}
