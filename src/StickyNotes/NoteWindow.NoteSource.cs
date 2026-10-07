using System.Text.Json;
using System.Threading;

namespace StickyNotes;

public sealed partial class NoteWindow
{
    private CancellationTokenSource? cliReadCancellation;
    private long? lastCliRead;
    private string cliReadIdentity = "";
    private string? cliNoteError;
    private int cliReadVersion;

    internal void PauseFolderChange() { CancelCliRead(); CancelQueryPreviews(); }

    private string ReadIdentity() => JsonSerializer.Serialize(new { Placement.Path, Placement.Daily, Placement.Heading,
        app.Config.ObsidianTasksEnabled, app.Config.ObsidianCli, app.Config.ObsidianVaultFolder, app.Config.ObsidianVaultId,
        app.Config.DailyRetention, Date = today().Date });

    private void CancelCliRead()
    {
        if (cliReadIdentity.Length == 0 && cliReadCancellation is null) return;
        cliReadVersion++; cliReadCancellation?.Cancel(); cliReadCancellation = null;
        cliReadIdentity = ""; lastCliRead = null; cliNoteError = null;
    }

    private void AcceptSnapshot(FileSnapshot fresh)
    {
        var nextContent = Placement.Heading.Length > 0 ? SectionEditor.Find(fresh.Text, Placement.Heading).Content : NoteStore.Split(fresh.Text).Body;
        snapshot = fresh; content = nextContent; dirty = false; SetEditing(false);
        displayedDate = dailyDisplay.TargetDate;
        Render(); status.Text = Placement.Daily ? L10n.Text("NoteWindow.Text17") + Path.GetFileName(fresh.Path) : L10n.Text("NoteWindow.Text18") + Path.GetFileName(fresh.Path);
    }

    private async Task<string> ResolveCliPathAsync(NoteSource source, CancellationToken token)
    {
        if (!Placement.Daily) return Placement.Path;
        var identity = ReadIdentity();
        var date = today().Date;
        var daily = await source.GetDailyNotesAsync(date, token);
        token.ThrowIfCancellationRequested();
        if (closed || identity != ReadIdentity()) throw new OperationCanceledException(token);
        return dailyDisplay.Resolve(daily.Configuration, date, app.Config.DailyRetention, daily.Resolve);
    }

    private async Task ReloadCliNoteAsync(bool force)
    {
        if (closed || app.IsChangingFolder || !NoteSource.UsesCli(app.Config)) return;
        var identity = ReadIdentity();
        var changed = identity != cliReadIdentity;
        if (!force && !changed && (cliReadCancellation is not null || (lastCliRead is { } last && Environment.TickCount64 - last < 30_000))) return;
        cliReadCancellation?.Cancel();
        using var cancel = new CancellationTokenSource();
        cliReadCancellation = cancel;
        var version = ++cliReadVersion;
        cliReadIdentity = identity; lastCliRead = Environment.TickCount64;
        // A previous direct-file snapshot must never feed CLI query evaluation.
        if (!editing && !dirty && (changed || snapshot?.Hash.StartsWith("cli:", StringComparison.Ordinal) == false))
        {
            snapshot = null; content = ""; CancelQueryPreviews(); preview.Document?.Blocks.Clear();
        }
        status.Text = L10n.Text("CliNote.Loading");
        var source = app.NoteSources(app.Config);
        bool Current() => !closed && !cancel.IsCancellationRequested && version == cliReadVersion &&
            NoteSource.UsesCli(app.Config) && identity == ReadIdentity();
        try
        {
            var path = await ResolveCliPathAsync(source, cancel.Token);
            if (!Current()) return;
            var fresh = await source.ReadAsync(path, cancel.Token);
            if (!Current()) return;
            cliNoteError = null;
            if (editing || dirty)
            {
                if (snapshot?.Path != fresh.Path || snapshot.Hash != fresh.Hash) status.Text = L10n.Text("NoteWindow.Text37");
                else status.Text = L10n.Text("NoteWindow.Text27");
                return;
            }
            if (force || snapshot?.Path != fresh.Path || snapshot.Hash != fresh.Hash ||
                (Placement.Daily && (renderedToday != today().Date || renderedRetention != app.Config.DailyRetention))) AcceptSnapshot(fresh);
            else status.Text = L10n.Text("CliNote.Loaded");
            await RefreshQueryPreviews();
        }
        catch (OperationCanceledException) when (cancel.IsCancellationRequested) { }
        catch (Exception ex)
        {
            if (!Current()) return;
            cliNoteError = L10n.Text("CliNote.Error") + ex.Message;
            if (snapshot is null && !editing && !dirty) ShowReadError(ex);
            else if (!editing && !dirty) { RenderQueryPreviews(); }
            status.Text = cliNoteError;
        }
        finally { if (ReferenceEquals(cliReadCancellation, cancel)) cliReadCancellation = null; }
    }
}
