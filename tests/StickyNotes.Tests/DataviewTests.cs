using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using StickyNotes;
using StickyNotes.Core;

internal static partial class Program
{
    private static void DataviewTests(string root)
    {
        const string query = "TABLE file.name WHERE contains(file.name, \"日本語');throw 1;//\")";
        var code = ObsidianTasksClient.BuildDataviewCode(root, "日本語/note.md", [query]);
        using var command = ObsidianTasksClient.PrepareQueryCommand(new Settings { ObsidianVaultFolder = root, ObsidianVaultId = "日本語Vault" }, root, "日本語/note.md", [query], true);
        ObsidianTasksClient.ValidateCommand("日本語Vault", command.Code);
        using var compressed = new MemoryStream(Convert.FromBase64String(code.Split("Buffer.from('")[1].Split("'")[0]));
        using var inflated = new System.IO.Compression.ZLibStream(compressed, System.IO.Compression.CompressionMode.Decompress);
        using var data = JsonDocument.Parse(inflated);
        Check(data.RootElement.GetProperty("queries")[0].GetString() == query &&
            data.RootElement.GetProperty("path").GetString() == "日本語/note.md" && !code.Contains(query), "Dataview: source path and Unicode query encoded as JSON data");
        Throws<InvalidOperationException>(() => ObsidianTasksClient.BuildDataviewCode(root, "note.md", new string[21].Select(_ => "LIST").ToArray()), "Dataview: block count bounded");
        Throws<InvalidOperationException>(() => ObsidianTasksClient.BuildDataviewCode(root, "note.md", [new string('a', 8001)]), "Dataview: total query length bounded");
        var response = ObsidianTasksClient.ParseDataviewResponse("=> STICKY_TASKS_PREVIEW:{\"version\":\"test\",\"results\":[{\"markdown\":\"| Column |\\n\",\"error\":null},{\"markdown\":null,\"error\":\"STICKY_DATAVIEW_UnsupportedQuery\"}]}", 2);
        Check(response.Results[0].Markdown == "| Column |\n" && response.Results[1].Error == L10n.Text("DataviewPreview.UnsupportedQuery"), "Dataview: partial block failure localized, successful output preserved");
        Throws<InvalidOperationException>(() => ObsidianTasksClient.ParseDataviewResponse("=> STICKY_TASKS_PREVIEW:{\"version\":\"test\",\"results\":[]}", 1), "Dataview: incomplete response rejected");
        try { ObsidianTasksClient.ParseDataviewResponse("=> STICKY_TASKS_PREVIEW:{\"error\":\"STICKY_DATAVIEW_Loading\"}", 1); throw new Exception("Expected loading error"); }
        catch (InvalidOperationException ex) { Check(ex.Message == L10n.Text("DataviewPreview.Loading"), "Dataview: indexing error localized"); }
        const string nested = "> ~~~dataview\n> LIST\n> ~~~\n\n```dataviewjs\ndv.list([])\n```\n\n````text\n```dataview\nLIST\n```\n````\n\n`= this.file.name`";
        Check(NoteWindow.FindQueryBlocks(nested, "dataview").Length == 1, "Dataview: find nested fences but exclude examples, DataviewJS and inline expressions");
        var calls = 0;
        var doc = MarkdownView.Render(nested, (_, _) => { }, _ => { calls++; return new Section(new Paragraph(new Run("query result"))); });
        var text = new TextRange(doc.ContentStart, doc.ContentEnd).Text;
        Check(calls == 1 && text.Contains("dv.list([])") && text.Contains("= this.file.name"), "Dataview: unsupported forms remain source text");
        DataviewWindowTests(root);
    }

    private static void DataviewWindowTests(string root)
    {
        var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        var app = App.Current; var settings = app.Config;
        var enabled = settings.ObsidianTasksEnabled; var vault = settings.ObsidianVaultFolder; var vaultId = settings.ObsidianVaultId;
        var sources = app.NoteSources; var tasksQuery = app.TasksQueries; var dataviewQuery = app.DataviewQueries;
        var path = Path.Combine(root, "dataview-ui.md");
        const string source = "Prose\n\n```tasks\nnot done\n```\n\n```dataview\nTABLE file.name\n```\n\n```dataview\nTASK\n```\n\nAfter\n";
        File.WriteAllText(path, source);
        NoteWindow? window = null;
        var taskCalls = 0; var dvCalls = 0;
        var result = new TasksResponse("test", [new TasksOutput("| Name | Value |\n| --- | --- |\n| Entry | 1 |\n", null), new TasksOutput("- [ ] Queried task", null)]);
        try
        {
            settings.ObsidianTasksEnabled = true; settings.ObsidianVaultFolder = root;
            app.NoteSources = config => new NoteSource(config, true, (_, _, _) => Task.FromResult(CliSnapshotEnvelope(source)));
            app.TasksQueries = (_, _, _, _) => { taskCalls++; return Task.FromException<TasksResponse>(new IOException("Tasks unavailable")); };
            app.DataviewQueries = (_, origin, queries, _) =>
            {
                dvCalls++;
                Check(origin == path && queries.Length == 2 && queries[0].StartsWith("TABLE"), "Dataview window: exact source note and ordered queries passed");
                return Task.FromResult(result);
            };
            window = new NoteWindow(new NotePlacement { Path = path });
            object? Invoke(string method, params object[] args) => typeof(NoteWindow).GetMethod(method, flags)!.Invoke(window, args);
            QueryPreviewState State(string field) => (QueryPreviewState)typeof(NoteWindow).GetField(field, flags)!.GetValue(window)!;
            void Field(string field, object value) => typeof(NoteWindow).GetField(field, flags)!.SetValue(window, value);
            Task Refresh() => (Task)Invoke("RefreshQueryPreviews")!;
            Invoke("Reload");
            var viewer = (FlowDocumentScrollViewer)typeof(NoteWindow).GetField("preview", flags)!.GetValue(window)!;
            string Text() => new TextRange(viewer.Document.ContentStart, viewer.Document.ContentEnd).Text;
            Check(taskCalls == 1 && dvCalls == 1 && Text().Contains("Tasks unavailable") && Text().Contains("Entry") && Text().Contains("After"), "Dataview window: independent Tasks failure, table and prose coexist");
            var sections = viewer.Document.Blocks.OfType<Section>().ToArray();
            Check(sections[1].Blocks.OfType<Table>().Any(), "Dataview window: Markdown table rendered as a table");
            var list = sections[2].Blocks.OfType<System.Windows.Documents.List>().Single();
            var checkbox = (CheckBox)list.ListItems.FirstListItem.Blocks.OfType<Paragraph>().Single().Inlines.OfType<InlineUIContainer>().Single().Child;
            checkbox.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
            Check(!checkbox.IsEnabled && File.ReadAllText(path) == source, "Dataview window: generated task checkbox is read-only and source file retained");
            Check(MarkdownSourceMap.Position(sections[1].Blocks.FirstBlock.ContentStart) == source.IndexOf("```dataview", StringComparison.Ordinal), "Dataview window: generated result maps to original query position");
            var refreshed = Refresh();
            Check(refreshed.IsCompleted && taskCalls == 1 && dvCalls == 1, "Dataview window: provider refreshes throttled for 30 seconds");
            Invoke("ResetQueryRefresh"); Refresh().GetAwaiter().GetResult();
            Check(taskCalls == 2 && dvCalls == 2, "Dataview window: manual refresh resets both provider timers");
            app.TasksQueries = (_, _, _, _) => { taskCalls++; return Task.FromResult(new TasksResponse("test", [new TasksOutput("Recovered Tasks", null)])); };
            app.DataviewQueries = (_, _, _, _) => Task.FromException<TasksResponse>(new IOException("Dataview unavailable"));
            Invoke("ResetQueryRefresh"); Refresh().GetAwaiter().GetResult();
            Check(Text().Contains("Recovered Tasks") && Text().Contains("Dataview unavailable") && Text().Contains("Entry"), "Dataview window: failure retains dated cache while Tasks succeeds");
            foreach (var failure in new[] { false, true })
            {
                var pending = new TaskCompletionSource<TasksResponse>(TaskCreationOptions.RunContinuationsAsynchronously);
                app.DataviewQueries = (_, _, _, _) => pending.Task;
                Invoke("ResetQueryRefresh"); var old = Refresh();
                Invoke("BeginEdit");
                Check(((TextBox)typeof(NoteWindow).GetField("editor", flags)!.GetValue(window)!).Text == source, "Dataview window: editing shows query source, never generated results");
                if (failure) pending.SetException(new IOException("Late error")); else pending.SetResult(new TasksResponse("late", result.Results));
                WaitFor(() => old.IsCompleted, "Dataview window: in-flight edit response settles");
                Check(State("dataviewPreview").Response!.Version == "test" && State("dataviewPreview").Error == "Dataview unavailable", "Dataview window: editing discards late " + (failure ? "error" : "success"));
                Field("editing", false);
            }
            var changing = new TaskCompletionSource<TasksResponse>(TaskCreationOptions.RunContinuationsAsynchronously);
            app.DataviewQueries = (_, _, _, _) => changing.Task;
            Invoke("ResetQueryRefresh"); var changingTask = Refresh();
            settings.ObsidianVaultId += "-changed";
            changing.SetException(new IOException("Wrong vault error"));
            WaitFor(() => changingTask.IsCompleted, "Dataview window: changed-settings response settles");
            Check(State("dataviewPreview").Error == "Dataview unavailable", "Dataview window: config change discards stale error before re-render");
            settings.ObsidianVaultId = vaultId;
            System.Threading.CancellationToken oldToken = default;
            var cancelled = new TaskCompletionSource<TasksResponse>(TaskCreationOptions.RunContinuationsAsynchronously);
            app.DataviewQueries = (_, _, _, token) => { oldToken = token; return cancelled.Task; };
            Invoke("ResetQueryRefresh"); var cancelledTask = Refresh();
            Field("content", "```dataview\nLIST\n```");
            app.DataviewQueries = (_, _, _, _) => Task.FromResult(new TasksResponse("new", [new TasksOutput("New result", null)]));
            Refresh().GetAwaiter().GetResult();
            Check(oldToken.IsCancellationRequested && State("dataviewPreview").Response!.Version == "new", "Dataview window: source change cancels old request and starts new one immediately");
            cancelled.SetResult(result); WaitFor(() => cancelledTask.IsCompleted, "Dataview window: cancelled request settles");
            Check(Text().Contains("New result") && !Text().Contains("Entry"), "Dataview window: old result cannot replace changed query");
            var previousCalls = taskCalls;
            Invoke("ResetQueryRefresh"); Refresh().GetAwaiter().GetResult();
            Check(taskCalls == previousCalls, "Dataview window: Dataview-only note does not invoke Tasks");
            settings.ObsidianTasksEnabled = false; Invoke("RenderQueryPreviews", false);
            Check(Text().Contains("LIST") && !Text().Contains("New result") && State("dataviewPreview").Response is null, "Dataview window: disabling CLI restores source and clears results");
            settings.ObsidianTasksEnabled = true;
            var closing = new TaskCompletionSource<TasksResponse>(TaskCreationOptions.RunContinuationsAsynchronously);
            app.DataviewQueries = (_, _, _, token) => { oldToken = token; return closing.Task; };
            var closingTask = Refresh(); window.Close();
            Check(oldToken.IsCancellationRequested, "Dataview window: closing cancels pending query");
            closing.SetException(new IOException("Closed error")); WaitFor(() => closingTask.IsCompleted, "Dataview window: closed response settles");
            Check(State("dataviewPreview").Error is null && State("dataviewPreview").Response is null, "Dataview window: closed note ignores late errors");
        }
        finally
        {
            window?.Close(); settings.ObsidianTasksEnabled = enabled; settings.ObsidianVaultFolder = vault; settings.ObsidianVaultId = vaultId;
            app.NoteSources = sources; app.TasksQueries = tasksQuery; app.DataviewQueries = dataviewQuery;
        }
    }

    private static async Task DataviewBridgeSmoke()
    {
        var code = ObsidianTasksClient.BuildDataviewCode("C:/SyntheticVault", "日本語/note.md",
            ["LIST file.name", "TABLE file.name WHERE file.path = this.file.path", "TASK WHERE !completed", "broken query", "CALENDAR file.day", "oversized", "invalid response", "throws"]);
        var start = new ProcessStartInfo("node") { UseShellExecute = false, CreateNoWindow = true,
            RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true, StandardInputEncoding = Encoding.UTF8 };
        start.ArgumentList.Add(Path.Combine(AppContext.BaseDirectory, "DataviewBridgeHarness.js"));
        using var process = Process.Start(start)!;
        using var timeout = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(30));
        var stdout = process.StandardOutput.ReadToEndAsync(timeout.Token); var stderr = process.StandardError.ReadToEndAsync(timeout.Token);
        await process.StandardInput.WriteAsync(JsonSerializer.Serialize(new { code })); process.StandardInput.Close();
        try { await process.WaitForExitAsync(timeout.Token); }
        finally { if (!process.HasExited) process.Kill(); }
        var output = await stdout; var errors = await stderr;
        Check(process.ExitCode == 0, "Dataview bridge: generated CLI adapter smoke passed: " + errors);
        Console.Write(output);
    }
}
