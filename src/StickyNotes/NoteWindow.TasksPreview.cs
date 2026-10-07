using System.Text.Json;
using System.Threading;
using System.Windows.Documents;
using Markdig;
using Markdig.Syntax;

namespace StickyNotes;

public sealed partial class NoteWindow
{
    private string tasksIdentity = "";
    private string[] tasksQueries = [];
    private Dictionary<int, int> tasksLines = [];
    private TasksResponse? tasksResponse;
    private string? tasksError;
    private DateTime? tasksFetched;
    private long? lastTasksCheck;
    private CancellationTokenSource? tasksCancellation;
    private bool tasksBusy;

    internal static FencedCodeBlock[] FindTasksBlocks(string markdown) =>
        Markdown.Parse(markdown, MarkdownView.Pipeline).Descendants<FencedCodeBlock>()
            .Where(b => b.Info == "tasks").ToArray();

    private void CancelTasksPreview()
    {
        tasksCancellation?.Cancel();
        tasksIdentity = ""; tasksQueries = []; tasksLines.Clear();
        tasksResponse = null; tasksError = null; tasksFetched = null; lastTasksCheck = null;
    }

    private void RenderTasksPreview(bool force = true)
    {
        if (!app.Config.ObsidianTasksEnabled)
        {
            if (force || tasksIdentity.Length > 0)
            {
                CancelTasksPreview();
                preview.Document = MarkdownView.Render(content, ToggleTask);
            }
            return;
        }
        var identity = JsonSerializer.Serialize(new { snapshot?.Path, content, app.Config.ObsidianCli,
            app.Config.ObsidianVaultFolder, app.Config.ObsidianVaultId });
        var changed = identity != tasksIdentity;
        if (changed)
        {
            CancelTasksPreview(); tasksIdentity = identity;
            var blocks = FindTasksBlocks(content);
            tasksQueries = blocks.Select(b => b.Lines.ToString()).ToArray();
            tasksLines = blocks.Select((b, i) => (b.Line, Index: i)).ToDictionary(p => p.Line, p => p.Index);
        }
        if (!force && !changed) return;
        preview.Document = MarkdownView.Render(content, ToggleTask, block =>
        {
            var section = new Section { BorderBrush = Brushes.DarkSeaGreen, BorderThickness = new Thickness(2, 0, 0, 0), Padding = new Thickness(8, 2, 0, 2) };
            var index = tasksLines[block.Line];
            var result = tasksResponse?.Results[index];
            var label = tasksFetched is null ? L10n.Text("TasksPreview.Waiting") :
                L10n.Format("TasksPreview.Updated", tasksResponse!.Version, tasksFetched.Value.ToString("HH:mm:ss"));
            section.Blocks.Add(new Paragraph(new Run(label)) { FontSize = 11, Foreground = Brushes.DarkSlateGray });
            if (tasksError is not null || result?.Error is not null)
                section.Blocks.Add(new Paragraph(new Run(L10n.Text("TasksPreview.Error") + (tasksError ?? result!.Error))) { Foreground = Brushes.DarkRed });
            if (result?.Markdown is { } markdown)
            {
                var rendered = MarkdownView.Render(markdown, (_, _) => { }, readOnly: true);
                while (rendered.Blocks.FirstBlock is { } child)
                {
                    rendered.Blocks.Remove(child); section.Blocks.Add(child);
                }
                if (string.IsNullOrWhiteSpace(markdown)) section.Blocks.Add(new Paragraph(new Run(L10n.Text("TasksPreview.Empty"))));
            }
            return section;
        });
    }

    private async Task RefreshTasksPreview()
    {
        if (closed || editing || snapshot is null || cliNoteError is not null) return;
        if (NoteSource.UsesCli(app.Config) && !snapshot.Hash.StartsWith("cli:", StringComparison.Ordinal))
        { await ReloadCliNoteAsync(true); return; }
        // Settings may change without the note text changing.
        RenderTasksPreview(false);
        if (!app.Config.ObsidianTasksEnabled || tasksQueries.Length == 0 || tasksBusy ||
            (lastTasksCheck is { } last && Environment.TickCount64 - last < 30_000)) return;
        tasksBusy = true; lastTasksCheck = Environment.TickCount64;
        var identity = tasksIdentity;
        using var cancellation = new CancellationTokenSource();
        tasksCancellation = cancellation;
        try
        {
            var response = await app.TasksQueries(app.Config, snapshot.Path, tasksQueries, cancellation.Token);
            if (closed || editing || tasksIdentity != identity || cancellation.IsCancellationRequested) return;
            tasksResponse = response; tasksFetched = DateTime.Now; tasksError = null;
            RenderTasksPreview();
        }
        catch (OperationCanceledException)
        {
            if (!closed && tasksIdentity == identity && !cancellation.IsCancellationRequested)
            { tasksError = L10n.Text("TasksPreview.Timeout"); RenderTasksPreview(); }
        }
        catch (Exception ex)
        {
            if (!closed && tasksIdentity == identity && !cancellation.IsCancellationRequested)
            { tasksError = ex.Message; RenderTasksPreview(); }
        }
        finally { if (ReferenceEquals(tasksCancellation, cancellation)) tasksCancellation = null; tasksBusy = false; }
    }
}
