using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Windows.Controls;
using StickyNotes;
using StickyNotes.Core;

internal static partial class Program
{
    private sealed class TasksHandler : HttpMessageHandler
    {
        public readonly List<Uri> Requests = [];
        public Func<Uri, HttpResponseMessage> Reply = _ => JsonResponse("{}");
        public Func<Uri, CancellationToken, Task<HttpResponseMessage>>? Pending;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Check(request.Method == HttpMethod.Get && request.Content is null && request.RequestUri!.Scheme == "https" &&
                request.RequestUri.Host is "tasks.googleapis.com" or "www.googleapis.com" && request.Headers.Authorization?.Parameter == "fake-tasks-token",
                "task integration only reads from fixed Google HTTPS hosts using the stored token");
            Requests.Add(request.RequestUri!);
            return Pending is null ? Task.FromResult(Reply(request.RequestUri!)) : Pending(request.RequestUri!, cancellationToken);
        }
    }

    private static HttpResponseMessage JsonResponse(string text, HttpStatusCode status = HttpStatusCode.OK) =>
        new(status) { Content = new StringContent(text) };

    private static CalendarService TasksService(string root, TasksHandler handler)
    {
        var path = Path.Combine(root, "tasks-credentials.json");
        File.WriteAllText(path, "{\"installed\":{\"client_id\":\"tasks-test\"}}");
        var stored = JsonSerializer.Serialize(new { clientId = "tasks-test", tokens = new OAuthTokens
            { AccessToken = "fake-tasks-token", ExpiresAt = DateTimeOffset.UtcNow.AddHours(1) } });
        var service = new CalendarService(() => stored, _ => { }, handler);
        service.Configure(path);
        return service;
    }

    private static async Task GoogleTasksTests(string root)
    {
        var handler = new TasksHandler();
        using var service = TasksService(root, handler);
        var today = CalendarQuery.Parse("@calendar today meeting", new DateTime(2026, 10, 7));
        var pages = new Queue<string>([
            "{\"items\":[{\"id\":\"list/one?x=1\",\"title\":\"Work\"}],\"nextPageToken\":\"list&page\"}",
            """
            {"items":[
              {"id":"1","title":"Meeting A","status":"needsAction","due":"2026-10-07T00:00:00Z"},
              {"id":"2","title":"meeting completed","status":"completed","due":"2026-10-07T00:00:00Z"},
              {"id":"3","title":"meeting undated","status":"needsAction"},
              {"id":"4","title":"meeting yesterday","status":"needsAction","due":"2026-10-06T00:00:00Z"},
              {"id":"5","title":"meeting tomorrow","status":"needsAction","due":"2026-10-08T00:00:00Z"},
              {"id":"6","title":"meeting hidden","status":"needsAction","hidden":true,"due":"2026-10-07T00:00:00Z"},
              {"id":"7","title":"meeting deleted","status":"needsAction","deleted":true,"due":"2026-10-07T00:00:00Z"},
              {"id":"8","title":"other","status":"needsAction","due":"2026-10-07T00:00:00Z"}
            ],"nextPageToken":"task&page"}
            """,
            "{\"items\":[{\"id\":\"1\",\"title\":\"Meeting A\",\"status\":\"needsAction\",\"due\":\"2026-10-07T00:00:00Z\"}]}",
            "{\"items\":[{\"id\":\"two\",\"title\":\"Personal\"}]}",
            "{\"items\":[{\"id\":\"1\",\"title\":\"Prepare\",\"notes\":\"MEETING notes\",\"status\":\"needsAction\",\"due\":\"2026-10-07T00:00:00Z\"}]}"
        ]);
        handler.Reply = _ => JsonResponse(pages.Dequeue());
        var result = await service.SearchTasksAsync(today);
        Check(result.Items.Count == 2 && !result.Truncated && result.Items.All(t => t.Date == new DateOnly(2026, 10, 7)),
            "Tasks paginates lists and tasks, filters status/date/keyword and deduplicates within each list");
        Check(result.Items.Select(t => t.ListTitle).Order().SequenceEqual(new[] { "Personal", "Work" }), "Tasks preserves list names and IDs");
        var taskUri = handler.Requests[1];
        var parameters = System.Web.HttpUtility.ParseQueryString(taskUri.Query);
        Check(taskUri.OriginalString.Contains("list%2Fone%3Fx%3D1") && parameters["dueMin"] == "2026-10-07T00:00:00Z" &&
            parameters["dueMax"] == "2026-10-08T00:00:00Z" && parameters["showCompleted"] == "false" && parameters["showAssigned"] == "true",
            "Tasks date bounds are date labels and list IDs are escaped");
        Check(System.Web.HttpUtility.ParseQueryString(handler.Requests[2].Query)["pageToken"] == "task&page" &&
            System.Web.HttpUtility.ParseQueryString(handler.Requests[3].Query)["pageToken"] == "list&page", "both pagination tokens are escaped");

        handler.Requests.Clear();
        handler.Reply = uri => uri.AbsolutePath.EndsWith("/tasks")
            ? JsonResponse("{\"items\":[{\"id\":\"x\",\"title\":\"Later\",\"status\":\"needsAction\",\"due\":\"2026-10-08T00:00:00Z\"}]}")
            : JsonResponse("{\"items\":[{\"id\":\"one\"}]}");
        result = await service.SearchTasksAsync(CalendarQuery.Parse("@calendar 2026-10-07T23:00-07:00"));
        parameters = System.Web.HttpUtility.ParseQueryString(handler.Requests[1].Query);
        Check(result.Items.Count == 1 && parameters["dueMin"] == "2026-10-07T00:00:00Z" && parameters["dueMax"] is null,
            "explicit date Tasks queries ignore time and offset and include subsequent dates");

        handler.Reply = _ => JsonResponse("{}");
        Check((await service.SearchTasksAsync(today)).Items.Count == 0, "absent items is a valid empty Tasks response");
        await service.VerifyTasksConnectionAsync();
        Check(handler.Requests[^1].Query == "?maxResults=1", "Tasks connection verification uses a bounded read");
        handler.Reply = _ => JsonResponse("{\"items\":{}}");
        try { await service.SearchTasksAsync(today); throw new Exception("Expected malformed response"); }
        catch (FormatException) { Check(true, "malformed Tasks response is not mistaken for an empty result"); }
        foreach (var code in new[] { HttpStatusCode.Forbidden, HttpStatusCode.Unauthorized })
        {
            handler.Reply = _ => JsonResponse("private-server-details", code);
            try { await service.SearchTasksAsync(today); throw new Exception("Expected denied request"); }
            catch (InvalidOperationException ex) { Check(ex.Message == L10n.Text("GoogleTasks.Access"), "missing Tasks access gives safe reauthorization guidance: " + code); }
        }

        handler.Requests.Clear();
        handler.Reply = _ => JsonResponse("{\"nextPageToken\":\"repeated\"}");
        result = await service.SearchTasksAsync(today);
        Check(result.Truncated && handler.Requests.Count == 20, "Tasks request cap prevents unbounded pagination");
        handler.Requests.Clear();
        var hundred = Enumerable.Range(0, 100).Select(i => new { id = i.ToString(), title = "meeting " + i, status = "needsAction", due = "2026-10-07T00:00:00Z" });
        handler.Reply = uri => !uri.AbsolutePath.EndsWith("/tasks") ? JsonResponse("{\"items\":[{\"id\":\"one\"}]}") :
            uri.Query.Contains("pageToken") ? JsonResponse("{\"items\":[{\"id\":\"101\",\"title\":\"meeting extra\",\"status\":\"needsAction\",\"due\":\"2026-10-07T00:00:00Z\"}]}") :
            JsonResponse(JsonSerializer.Serialize(new { items = hundred, nextPageToken = "more" }));
        result = await service.SearchTasksAsync(today);
        Check(result.Truncated && result.Items.Count == 100, "Tasks display cap reports partial results");

        handler.Pending = async (_, ct) => { await Task.Delay(Timeout.Infinite, ct); return JsonResponse("{}"); };
        using var cancellation = new CancellationTokenSource();
        var pending = service.SearchTasksAsync(today, cancellation.Token);
        cancellation.Cancel();
        try { await pending; throw new Exception("Expected cancellation"); }
        catch (OperationCanceledException) { Check(true, "Tasks cancellation stops pending HTTP requests"); }
    }

    private static void GoogleTasksUiTests(string root)
    {
        var app = App.Current;
        var originalService = app.Calendar;
        var originalCredentials = app.Config.GoogleCredentialsFile;
        var handler = new TasksHandler();
        using var service = TasksService(root, handler);
        typeof(App).GetProperty(nameof(App.Calendar))!.SetValue(app, service);
        app.Config.GoogleCredentialsFile = Path.Combine(root, "tasks-credentials.json");
        var path = Path.Combine(root, "tasks-note.md");
        File.WriteAllText(path, "@calendar today");
        var day = new DateTime(2026, 10, 7);
        var window = new NoteWindow(new NotePlacement { Path = path }, null, null, () => day);
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        void Call(string name) => typeof(NoteWindow).GetMethod(name, flags)!.Invoke(window, null);
        void Refresh() => ((Task)typeof(NoteWindow).GetMethod("RefreshCalendar", flags)!.Invoke(window, null)!).GetAwaiter().GetResult();
        var panel = (StackPanel)typeof(NoteWindow).GetField("eventsPanel", flags)!.GetValue(window)!;
        StackPanel TasksPanel() => panel.Children.OfType<StackPanel>().Single();
        var calendarFail = false;
        var tasksFail = false;
        Action? onTasks = null;
        handler.Reply = uri =>
        {
            if (uri.Host == "www.googleapis.com") return JsonResponse(calendarFail ? "{}" :
                "{\"items\":[{\"id\":\"event\",\"summary\":\"Calendar event\",\"start\":{\"date\":\"2026-10-07\"},\"etag\":\"v1\"}]}",
                calendarFail ? HttpStatusCode.Forbidden : HttpStatusCode.OK);
            onTasks?.Invoke();
            if (tasksFail) return JsonResponse("{}", HttpStatusCode.Forbidden);
            return JsonResponse(uri.AbsolutePath.EndsWith("/tasks")
                ? JsonSerializer.Serialize(new { items = new[] { new { id = "task", title = "Task title", notes = "Task notes", status = "needsAction", due = day.ToString("yyyy-MM-dd") + "T00:00:00Z" } } })
                : "{\"items\":[{\"id\":\"one\",\"title\":\"My tasks\"}]}");
        };
        try
        {
            Call("Reload"); Refresh();
            Check(panel.Children.OfType<Button>().Count() == 1 && TasksPanel().Children.OfType<TextBlock>().Any(t => t.Text.Contains("Task title") && t.ToolTip?.ToString() == "Task notes") &&
                !TasksPanel().Children.OfType<Button>().Any() && !TasksPanel().Children.OfType<CheckBox>().Any(), "Tasks appears beside editable events as read-only text with notes");
            tasksFail = true; Refresh();
            Check(panel.Children.OfType<Button>().Count() == 1 && TasksPanel().Children.OfType<TextBlock>().Single().Text.Contains(L10n.Text("GoogleTasks.Access")),
                "missing Tasks permission preserves calendar events and displays reauthorization guidance");
            tasksFail = false; calendarFail = true; Refresh();
            Check(!panel.Children.OfType<Button>().Any() && TasksPanel().Children.OfType<TextBlock>().Any(t => t.Text.Contains("Task title")),
                "Tasks still displays when calendar retrieval fails");
            calendarFail = false;
            onTasks = () => { day = day.AddDays(1); onTasks = null; };
            Refresh();
            Check(panel.Children.Count == 0, "date change during Tasks retrieval discards stale combined results");
            Refresh();
            Check(TasksPanel().Children.OfType<TextBlock>().Any(t => t.Text.Contains("2026-10-08")), "Tasks follows the current date on retry");
            Check(File.ReadAllText(path) == "@calendar today", "Tasks refresh never rewrites the note");
            var normalReply = handler.Reply;
            handler.Reply = uri => uri.Host == "tasks.googleapis.com" ? JsonResponse("{\"nextPageToken\":\"more\"}") : normalReply(uri);
            Refresh();
            var messages = TasksPanel().Children.OfType<TextBlock>().Select(t => t.Text).ToArray();
            Check(messages.Contains(L10n.Text("GoogleTasks.Truncated")) && !messages.Contains(L10n.Text("GoogleTasks.Empty")),
                "an incomplete search with zero matches reports the cap without claiming no tasks exist");
            handler.Reply = normalReply;
            var cancelled = false;
            handler.Pending = async (uri, ct) =>
            {
                if (uri.Host == "www.googleapis.com") return handler.Reply(uri);
                try { await Task.Delay(Timeout.Infinite, ct); }
                catch (OperationCanceledException) { cancelled = true; throw; }
                return JsonResponse("{}");
            };
            var pending = (Task)typeof(NoteWindow).GetMethod("RefreshCalendar", flags)!.Invoke(window, null)!;
            File.WriteAllText(path, "plain note"); Call("Reload");
            WaitFor(() => pending.IsCompleted, "changing the command cancels in-flight Tasks retrieval");
            pending.GetAwaiter().GetResult();
            Check(cancelled && panel.Children.Count == 0, "cancelled Tasks response cannot populate a different note command");
            File.WriteAllText(path, "@calendar today"); Call("Reload"); cancelled = false;
            pending = (Task)typeof(NoteWindow).GetMethod("RefreshCalendar", flags)!.Invoke(window, null)!;
            window.Close();
            WaitFor(() => pending.IsCompleted, "closing a note cancels in-flight Tasks retrieval");
            pending.GetAwaiter().GetResult();
            Check(cancelled, "closing stops Tasks HTTP work");
        }
        finally
        {
            window.Close();
            typeof(App).GetProperty(nameof(App.Calendar))!.SetValue(app, originalService);
            app.Config.GoogleCredentialsFile = originalCredentials;
        }
    }
}
