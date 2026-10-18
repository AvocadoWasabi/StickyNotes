using System.Windows.Input;

namespace StickyNotes;

public sealed partial class NoteWindow
{
    private bool renameDialogOpen;
    private Point? titleDragStart;
    internal bool HasUnsavedChanges => dirty;
    internal bool CanRename => !Placement.Daily && Placement.Path.Length > 0 && snapshot is not null &&
        !closed && !IsSaving && !app.IsChangingFolder && !renameDialogOpen;

    private void ConfigureTitleRename()
    {
        title.ToolTip = L10n.Text(Placement.Daily ? "Rename.Daily" : "Rename.Hint");
        title.MouseLeftButtonDown += (_, e) =>
        {
            e.Handled = true;
            if (e.ClickCount == 2)
            {
                titleDragStart = null; title.ReleaseMouseCapture();
                _ = ShowRenameDialogAsync();
            }
            else { titleDragStart = e.GetPosition(this); title.CaptureMouse(); }
        };
        title.MouseLeftButtonUp += (_, e) => { titleDragStart = null; title.ReleaseMouseCapture(); e.Handled = true; };
        title.LostMouseCapture += (_, _) => titleDragStart = null;
        title.MouseMove += (_, e) =>
        {
            if (titleDragStart is not { } start || e.LeftButton != MouseButtonState.Pressed) return;
            var current = e.GetPosition(this);
            if (Math.Abs(current.X - start.X) < SystemParameters.MinimumHorizontalDragDistance &&
                Math.Abs(current.Y - start.Y) < SystemParameters.MinimumVerticalDragDistance) return;
            titleDragStart = null; title.ReleaseMouseCapture(); DragMove();
        };
    }

    internal MenuItem RenameMenuItem()
    {
        var item = new MenuItem { Header = L10n.Text("Rename.Menu"), IsEnabled = CanRename,
            ToolTip = Placement.Daily ? L10n.Text("Rename.Daily") : L10n.Text("Rename.Hint") };
        item.Click += async (_, _) => await ShowRenameDialogAsync();
        return item;
    }

    private async Task ShowRenameDialogAsync()
    {
        if (closed || renameDialogOpen) return;
        var unavailable = CanRename ? null : L10n.Text(Placement.Daily ? "Rename.Daily" :
            IsSaving ? "NoteWindow.Saving" : app.IsChangingFolder ? "StickyFolder.Working" : "Rename.Unavailable");
        renameDialogOpen = true;
        var previousDecision = decisionInProgress;
        decisionInProgress = true; focusLossTimer.Stop();
        try
        {
            if (unavailable is not null)
            {
                MessageBox.Show(this, unavailable, L10n.Text("Rename.Menu"), MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            var value = Ui.Prompt(L10n.Text("Rename.Menu"),
                [new("name", L10n.Text("Rename.Label"), Path.GetFileNameWithoutExtension(Placement.Path))], this, singleLineSubmit: true);
            if (value is null) return;
            var target = NoteRename.Target(Placement.Path, value["name"]);
            if (target == Placement.Path) return;
            if (!await SaveEditorAsync()) return;
            await app.RenameNoteAsync(this, snapshot!, target);
        }
        catch (Exception ex) { MessageBox.Show(this, ex.Message, L10n.Text("Rename.Menu"), MessageBoxButton.OK, MessageBoxImage.Warning); }
        finally { renameDialogOpen = false; decisionInProgress = previousDecision; ScheduleFocusLoss(); }
    }

    internal void RefreshAfterRename() { if (!dirty && !editing) Reload(); }
}
