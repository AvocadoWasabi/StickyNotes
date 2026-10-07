using System.Text.Json;
using System.Threading;
using System.Windows.Documents;
using Markdig;
using Markdig.Syntax;

namespace StickyNotes;

internal sealed class QueryPreviewState(string language, string resourcePrefix)
{
    internal readonly string Language = language, ResourcePrefix = resourcePrefix;
    internal string[] Queries = [];
    internal Dictionary<int, int> Lines = [];
    internal TasksResponse? Response;
    internal string? Error;
    internal DateTime? Fetched;
    internal long? LastCheck;
    internal CancellationTokenSource? Cancellation;

    internal void Reset()
    {
        Cancellation?.Cancel(); Cancellation = null;
        Queries = []; Lines.Clear(); Response = null; Error = null; Fetched = null; LastCheck = null;
    }
}

public sealed partial class NoteWindow
{
    private string queryIdentity = "";
    private readonly QueryPreviewState tasksPreview = new("tasks", "TasksPreview");
    private readonly QueryPreviewState dataviewPreview = new("dataview", "DataviewPreview");

    internal static FencedCodeBlock[] FindQueryBlocks(string markdown, string language) =>
        Markdown.Parse(markdown, MarkdownView.Pipeline).Descendants<FencedCodeBlock>()
            .Where(b => b.Info == language).ToArray();

    internal static FencedCodeBlock[] FindTasksBlocks(string markdown) => FindQueryBlocks(markdown, "tasks");

    private string QueryIdentity() => JsonSerializer.Serialize(new { snapshot?.Path, content, app.Config.ObsidianCli,
        app.Config.ObsidianVaultFolder, app.Config.ObsidianVaultId });

    private void CancelQueryPreviews()
    {
        tasksPreview.Reset(); dataviewPreview.Reset(); queryIdentity = "";
    }

    private void ResetQueryRefresh() { tasksPreview.LastCheck = null; dataviewPreview.LastCheck = null; }

    private void RenderQueryPreviews(bool force = true)
    {
        if (!app.Config.ObsidianTasksEnabled)
        {
            if (force || queryIdentity.Length > 0)
            {
                CancelQueryPreviews();
                preview.Document = MarkdownView.Render(content, ToggleTask);
            }
            return;
        }
        var identity = QueryIdentity();
        var changed = identity != queryIdentity;
        if (changed)
        {
            CancelQueryPreviews(); queryIdentity = identity;
            var blocks = Markdown.Parse(content, MarkdownView.Pipeline).Descendants<FencedCodeBlock>().ToArray();
            foreach (var state in new[] { tasksPreview, dataviewPreview })
            {
                var matching = blocks.Where(b => b.Info == state.Language).ToArray();
                state.Queries = matching.Select(b => b.Lines.ToString()).ToArray();
                state.Lines = matching.Select((b, i) => (b.Line, Index: i)).ToDictionary(p => p.Line, p => p.Index);
            }
        }
        if (!force && !changed) return;
        preview.Document = MarkdownView.Render(content, ToggleTask, block =>
        {
            var state = block.Info == "dataview" ? dataviewPreview : tasksPreview;
            var section = new Section { BorderBrush = Brushes.DarkSeaGreen, BorderThickness = new Thickness(2, 0, 0, 0), Padding = new Thickness(8, 2, 0, 2) };
            var index = state.Lines[block.Line];
            var result = state.Response?.Results[index];
            var label = state.Fetched is null ? L10n.Text(state.ResourcePrefix + ".Waiting") :
                L10n.Format(state.ResourcePrefix + ".Updated", state.Response!.Version, state.Fetched.Value.ToString("HH:mm:ss"));
            section.Blocks.Add(new Paragraph(new Run(label)) { FontSize = 11, Foreground = Brushes.DarkSlateGray });
            var error = cliNoteError ?? state.Error ?? result?.Error;
            if (error is not null)
                section.Blocks.Add(new Paragraph(new Run(L10n.Text(state.ResourcePrefix + ".Error") + error)) { Foreground = Brushes.DarkRed });
            if (result?.Markdown is { } markdown)
            {
                var rendered = MarkdownView.Render(markdown, (_, _) => { }, readOnly: true);
                while (rendered.Blocks.FirstBlock is { } child)
                {
                    rendered.Blocks.Remove(child); section.Blocks.Add(child);
                }
                if (string.IsNullOrWhiteSpace(markdown)) section.Blocks.Add(new Paragraph(new Run(L10n.Text(state.ResourcePrefix + ".Empty"))));
            }
            return section;
        });
    }

    private async Task RefreshQueryPreviews()
    {
        if (closed || editing || dirty || snapshot is null || cliNoteError is not null) return;
        if (NoteSource.UsesCli(app.Config) && !snapshot.Hash.StartsWith("cli:", StringComparison.Ordinal))
        { await ReloadCliNoteAsync(true); return; }
        RenderQueryPreviews(false);
        if (!app.Config.ObsidianTasksEnabled) return;
        await Task.WhenAll(RefreshQueryPreview(tasksPreview, app.TasksQueries), RefreshQueryPreview(dataviewPreview, app.DataviewQueries));
    }

    private async Task RefreshQueryPreview(QueryPreviewState state,
        Func<Settings, string, string[], CancellationToken, Task<TasksResponse>> query)
    {
        if (state.Queries.Length == 0 || state.Cancellation is not null ||
            (state.LastCheck is { } last && Environment.TickCount64 - last < 30_000)) return;
        state.LastCheck = Environment.TickCount64;
        var identity = queryIdentity;
        using var cancellation = new CancellationTokenSource();
        state.Cancellation = cancellation;
        bool Current() => !closed && !editing && !dirty && cliNoteError is null && app.Config.ObsidianTasksEnabled &&
            queryIdentity == identity && QueryIdentity() == identity && ReferenceEquals(state.Cancellation, cancellation) &&
            !cancellation.IsCancellationRequested;
        try
        {
            var response = await query(app.Config, snapshot!.Path, state.Queries, cancellation.Token);
            if (!Current()) return;
            state.Response = response; state.Fetched = DateTime.Now; state.Error = null;
            RenderQueryPreviews();
        }
        catch (OperationCanceledException)
        {
            if (Current()) { state.Error = L10n.Text(state.ResourcePrefix + ".Timeout"); RenderQueryPreviews(); }
        }
        catch (Exception ex)
        {
            if (Current()) { state.Error = ex.Message; RenderQueryPreviews(); }
        }
        finally { if (ReferenceEquals(state.Cancellation, cancellation)) state.Cancellation = null; }
    }
}
