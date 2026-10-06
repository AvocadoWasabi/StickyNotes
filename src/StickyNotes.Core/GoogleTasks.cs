using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace StickyNotes.Core;

public sealed record GoogleTask(string Id, string ListId, string ListTitle, string Title, string Notes, DateOnly Date);
public sealed record GoogleTaskSearch(IReadOnlyList<GoogleTask> Items, bool Truncated);

public sealed partial class CalendarService
{
    private async Task<JsonDocument> ReadTasks(string path, CancellationToken cancellationToken)
    {
        // Only locally constructed paths are accepted; never follow response URLs with a bearer token.
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://tasks.googleapis.com/tasks/v1/" + path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await AccessToken(cancellationToken));
        using var response = await http.SendAsync(request, cancellationToken);
        if (response.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.Unauthorized)
            throw new InvalidOperationException(L10n.Text("GoogleTasks.Access"));
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException(L10n.Format("GoogleTasks.Error", (int)response.StatusCode));
        return JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
    }

    private static string TaskValue(JsonElement item, string key) =>
        item.TryGetProperty(key, out var value) ? value.GetString() ?? "" : "";

    private static IEnumerable<JsonElement> TaskItems(JsonElement page)
    {
        if (page.ValueKind != JsonValueKind.Object) throw new FormatException(L10n.Text("GoogleTasks.Invalid"));
        if (!page.TryGetProperty("items", out var items)) return [];
        if (items.ValueKind != JsonValueKind.Array) throw new FormatException(L10n.Text("GoogleTasks.Invalid"));
        return items.EnumerateArray();
    }

    public async Task VerifyTasksConnectionAsync(CancellationToken cancellationToken = default)
    {
        using var json = await ReadTasks("users/@me/lists?maxResults=1", cancellationToken);
        _ = TaskItems(json.RootElement);
    }

    public async Task<GoogleTaskSearch> SearchTasksAsync(CalendarQuery query, CancellationToken cancellationToken = default)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeout.Token);
        var ct = linked.Token;
        var found = new List<GoogleTask>();
        var seen = new HashSet<(string List, string Id)>();
        var requests = 0;
        // Tasks due is a date label, not an instant: do not convert UTC midnight into local time.
        var from = DateOnly.FromDateTime(query.From.Date);
        var until = query.Until is { } end ? DateOnly.FromDateTime(end.Date) : (DateOnly?)null;
        string Bound(DateOnly day) => day.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + "T00:00:00Z";
        GoogleTaskSearch Result(bool truncated) => new(found.OrderBy(t => t.Date).ThenBy(t => t.ListTitle, StringComparer.OrdinalIgnoreCase)
            .ThenBy(t => t.Title, StringComparer.OrdinalIgnoreCase).ToList(), truncated);
        string listPage = "";
        try
        {
            do
            {
                ct.ThrowIfCancellationRequested();
                if (requests++ >= 20) return Result(true);
                using var lists = await ReadTasks("users/@me/lists?maxResults=100" +
                    (listPage.Length == 0 ? "" : "&pageToken=" + Escape(listPage)), ct);
                foreach (var list in TaskItems(lists.RootElement))
                {
                    var listId = TaskValue(list, "id");
                    if (listId.Length == 0) throw new FormatException(L10n.Text("GoogleTasks.Invalid"));
                    var listTitle = TaskValue(list, "title");
                    string taskPage = "";
                    do
                    {
                        ct.ThrowIfCancellationRequested();
                        if (requests++ >= 20) return Result(true);
                        var path = $"lists/{Escape(listId)}/tasks?maxResults=100&showCompleted=false&showDeleted=false&showHidden=false&showAssigned=true&dueMin={Escape(Bound(from))}";
                        if (until is { } limit) path += "&dueMax=" + Escape(Bound(limit));
                        if (taskPage.Length > 0) path += "&pageToken=" + Escape(taskPage);
                        using var tasks = await ReadTasks(path, ct);
                        foreach (var task in TaskItems(tasks.RootElement))
                        {
                            if (TaskValue(task, "status") != "needsAction" ||
                                (task.TryGetProperty("deleted", out var deleted) && deleted.GetBoolean()) ||
                                (task.TryGetProperty("hidden", out var hidden) && hidden.GetBoolean())) continue;
                            var due = TaskValue(task, "due");
                            if (!DateTimeOffset.TryParse(due, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)) continue;
                            var day = DateOnly.FromDateTime(date.Date);
                            if (day < from || (until is { } upper && day >= upper)) continue;
                            var title = TaskValue(task, "title");
                            var notes = TaskValue(task, "notes");
                            if (query.Search.Length > 0 && !title.Contains(query.Search, StringComparison.OrdinalIgnoreCase) &&
                                !notes.Contains(query.Search, StringComparison.OrdinalIgnoreCase)) continue;
                            var id = TaskValue(task, "id");
                            if (id.Length == 0 || !seen.Add((listId, id))) continue;
                            if (found.Count == 100) return Result(true);
                            found.Add(new(id, listId, listTitle, title, notes, day));
                        }
                        taskPage = TaskValue(tasks.RootElement, "nextPageToken");
                    } while (taskPage.Length > 0);
                }
                listPage = TaskValue(lists.RootElement, "nextPageToken");
            } while (listPage.Length > 0);
            return Result(false);
        }
        catch (OperationCanceledException) when (timeout.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        { throw new TimeoutException(L10n.Text("GoogleTasks.Timeout")); }
    }
}
