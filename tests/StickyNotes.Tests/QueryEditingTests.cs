using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using StickyNotes;
using StickyNotes.Core;

internal static partial class Program
{
    private static async Task QueryTaskSmoke()
    {
        var root = Path.Combine(Path.GetTempPath(), "StickyNotes-QueryTest-" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(root);
        var commands = new List<ObsidianTasksClient.NoteCommand>();
        try
        {
            const string text = "\uFEFF---\r\ntitle: Keep\r\n---\r\n- [ ] Same\r\n- [ ] Same\r\n- [ ] Recurring\r\n  - [ ] Child\r\nSuffix";
            var settings = new Settings { ObsidianVaultFolder = root, ObsidianVaultId = "Synthetic" };
            var hash = "cli:" + Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();
            var backup = Path.Combine(root, "backup"); var denied = Path.Combine(root, "denied");
            string Query(bool dataview)
            {
                var command = ObsidianTasksClient.PrepareQueryCommand(settings, root, "query.md", [dataview ? "TASK" : "not done"], dataview); commands.Add(command);
                ObsidianTasksClient.ValidateCommand(settings.ObsidianVaultId, command.Code); return command.Code;
            }
            string Toggle(string provider, int line, string destination)
            {
                var command = ObsidianTasksClient.PrepareNoteCommand(settings, new { mode = "toggle-query-task", root, path = "target.md", hash, provider, line, @checked = true, backup = destination });
                commands.Add(command); ObsidianTasksClient.ValidateCommand(settings.ObsidianVaultId, command.Code); return command.Code;
            }
            string Transform(string line, bool value = true, string? vaultRoot = null)
            {
                var command = ObsidianTasksClient.PrepareTaskCheckboxCommand(settings, new { root = vaultRoot ?? root, path = "target.md", line, @checked = value });
                commands.Add(command); ObsidianTasksClient.ValidateCommand(settings.ObsidianVaultId, command.Code); return command.Code;
            }
            var availability = ObsidianTasksClient.PrepareTaskCheckboxCommand(settings, new { root, mode = "availability" }); commands.Add(availability);
            var payload = new { root, text, backup, denied, availabilityCode = availability.Code, tasksCode = Query(false), dataviewCode = Query(true),
                folderCode = ObsidianTasksClient.BuildFoldersCode(new { mode = "browse", root, path = "." }),
                dataviewToggle = Toggle("dataview", 4, backup), tasksToggle = Toggle("tasks", 5, backup),
                deniedToggle = Toggle("dataview", 4, denied), invalidToggle = Toggle("invalid", 4, backup),
                transform = Transform("- [ ] Recurring `literal` ${notCode} 日本語"), transformDone = Transform("- [x] Done", false),
                transformNoop = Transform("- [x] Done"), transformInvalid = Transform("- [ ] one\n- [ ] two"),
                transformWrongVault = Transform("- [ ] Task", vaultRoot: root + "-other") };
            ObsidianTasksClient.ValidateCommand(settings.ObsidianVaultId, payload.folderCode);
            var start = new System.Diagnostics.ProcessStartInfo("node") { UseShellExecute = false, CreateNoWindow = true,
                RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true, StandardInputEncoding = new UTF8Encoding(false) };
            start.ArgumentList.Add(Path.Combine(AppContext.BaseDirectory, "QueryTaskHarness.js"));
            using var process = System.Diagnostics.Process.Start(start)!;
            using var timeout = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(30));
            var stdout = process.StandardOutput.ReadToEndAsync(timeout.Token); var stderr = process.StandardError.ReadToEndAsync(timeout.Token);
            try
            {
                await process.StandardInput.WriteAsync(JsonSerializer.Serialize(payload)); process.StandardInput.Close();
                await process.WaitForExitAsync(timeout.Token);
                var output = await stdout; var errors = await stderr; Console.Write(output);
                Check(process.ExitCode == 0, "Generated query and mutation bridge smoke: " + errors);
            }
            finally { if (!process.HasExited) process.Kill(); }
        }
        finally { foreach (var command in commands) command.Dispose(); Directory.Delete(root, true); }
    }

    private static JsonElement CliRequest(string code)
    {
        var payload = Convert.FromBase64String(code.Split("Buffer.from('")[1].Split("'")[0]);
        var transfer = code.Contains("readFileSync(Buffer.from('");
        if (transfer)
        {
            using var file = new FileStream(Encoding.UTF8.GetString(payload), FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            using var bytes = new MemoryStream(); file.CopyTo(bytes); payload = bytes.ToArray();
        }
        using var input = new MemoryStream(payload);
        using var inflate = new ZLibStream(input, CompressionMode.Decompress);
        using var doc = JsonDocument.Parse(inflate);
        return (transfer ? doc.RootElement.GetProperty("request") : doc.RootElement).Clone();
    }

    private static void QueryEditingTests(string root)
    {
        var hash = "cli:" + new string('a', 64);
        string Envelope(QueryTaskTarget target) => "STICKY_TASKS_PREVIEW:" + JsonSerializer.Serialize(new TasksResponse("test", [new("- [ ] Task", null, [target])]));
        var valid = new QueryTaskTarget(0, "nested/task.md", 3, hash);
        Check(ObsidianTasksClient.ParseResponse(Envelope(valid), 1).Results[0].Tasks!.Single() == valid, "Query task: structured source mapping decoded");
        foreach (var invalid in new[] { valid with { Path = "../other.md" }, valid with { Path = "C:/other.md" },
            valid with { Line = -1 }, valid with { OutputLine = 2 }, valid with { Hash = "bad" } })
            Throws<InvalidOperationException>(() => ObsidianTasksClient.ParseResponse(Envelope(invalid), 1), "Query task: invalid source mapping rejected");
        var clicked = -1;
        var doc = MarkdownView.Render("- [ ] Enabled\n- [ ] Disabled", (line, _) => clicked = line, canToggle: line => line == 0);
        var checkboxes = doc.Blocks.OfType<System.Windows.Documents.List>().Single().ListItems
            .SelectMany(i => i.Blocks.OfType<Paragraph>()).SelectMany(p => p.Inlines.OfType<InlineUIContainer>()).Select(i => (CheckBox)i.Child).ToArray();
        checkboxes[0].RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
        Check(checkboxes[0].IsEnabled && !checkboxes[1].IsEnabled && clicked == 0, "Query task: only mapped result rows can be clicked");
        var source = new NoteSource(new Settings { ObsidianVaultFolder = root }, true, (_, _, _) => throw new Exception("must not execute"));
        Throws<InvalidOperationException>(() => source.ToggleQueryTaskAsync(valid with { Path = "../outside.md" }, "tasks", true, root).GetAwaiter().GetResult(), "Query task: target traversal rejected before CLI");
        QueryEditingWindowTests(root);
        NoteBrowserTests(root);
    }

    private static void QueryEditingWindowTests(string root)
    {
        var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        var app = App.Current; var enabled = app.Config.ObsidianTasksEnabled; var vault = app.Config.ObsidianVaultFolder;
        var factory = app.NoteSources; var tasks = app.TasksQueries; var dataview = app.DataviewQueries;
        try
        {
            app.Config.ObsidianTasksEnabled = true; app.Config.ObsidianVaultFolder = root;
            foreach (var provider in new[] { "tasks", "dataview" })
            {
                var query = "```" + provider + "\n" + (provider == "tasks" ? "not done" : "TASK") + "\n```";
                var pending = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
                JsonElement? mutation = null; var reads = 0;
                app.NoteSources = settings => new NoteSource(settings, true, (_, code, _) =>
                {
                    var request = CliRequest(code);
                    if (request.GetProperty("mode").GetString() == "toggle-query-task") { mutation = request; return pending.Task; }
                    reads++; return Task.FromResult(CliSnapshotEnvelope(query));
                });
                Task<TasksResponse> Result(Settings _, string __, string[] ___, System.Threading.CancellationToken ____) => Task.FromResult(
                    new TasksResponse("test", [new TasksOutput("- [ ] Rendered task", null, [new(0, "target.md", 4, "cli:" + new string('a', 64))])]));
                app.TasksQueries = Result; app.DataviewQueries = Result;
                var window = new NoteWindow(new NotePlacement { Path = Path.Combine(root, "query.md") });
                try
                {
                    typeof(NoteWindow).GetMethod("Reload", flags)!.Invoke(window, null);
                    var viewer = (FlowDocumentScrollViewer)typeof(NoteWindow).GetField("preview", flags)!.GetValue(window)!;
                    CheckBox Checkbox() => (CheckBox)viewer.Document.Blocks.OfType<Section>().Single().Blocks.OfType<System.Windows.Documents.List>().Single()
                        .ListItems.FirstListItem.Blocks.OfType<Paragraph>().Single().Inlines.OfType<InlineUIContainer>().Single().Child;
                    var box = Checkbox(); Check(box.IsEnabled, provider + " query: mapped checkbox is enabled");
                    box.IsChecked = true; box.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
                    Check(mutation?.GetProperty("path").GetString() == "target.md" && mutation?.GetProperty("line").GetInt32() == 4 &&
                        mutation?.GetProperty("provider").GetString() == provider && mutation?.GetProperty("checked").GetBoolean() == true,
                        provider + " query: click updates source target, never query note");
                    Check(!Checkbox().IsEnabled, provider + " query: duplicate clicks disabled while saving");
                    pending.SetResult(CliSnapshotEnvelope("- [x] Updated"));
                    WaitFor(() => !(bool)typeof(NoteWindow).GetField("queryTaskSaving", flags)!.GetValue(window)!, provider + " query: save settles");
                    Check(reads >= 2 && (string)typeof(NoteWindow).GetField("content", flags)!.GetValue(window)! == query,
                        provider + " query: save refreshes results and preserves original query");
                    var failed = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
                    app.NoteSources = settings => new NoteSource(settings, true, (_, _, _) => failed.Task);
                    box = Checkbox(); box.IsChecked = true; box.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
                    failed.SetException(new ConflictException("Synthetic conflict"));
                    WaitFor(() => !(bool)typeof(NoteWindow).GetField("queryTaskSaving", flags)!.GetValue(window)!, provider + " query: conflict settles");
                    Check(new TextRange(viewer.Document.ContentStart, viewer.Document.ContentEnd).Text.Contains("Synthetic conflict") &&
                        !viewer.Document.Blocks.OfType<Section>().Single().Blocks.OfType<System.Windows.Documents.List>().Any(), provider + " query: failed save clears actionable stale results and displays error");
                }
                finally { window.Close(); }
            }
        }
        finally { app.Config.ObsidianTasksEnabled = enabled; app.Config.ObsidianVaultFolder = vault; app.NoteSources = factory; app.TasksQueries = tasks; app.DataviewQueries = dataview; }
    }

    private static void NoteBrowserTests(string root)
    {
        var basis = Path.Combine(root, "browser"); var folder = Path.Combine(basis, "付箋"); Directory.CreateDirectory(folder);
        var first = Path.Combine(folder, "closed.md"); const string note = "---\ntitle: Closed note\ncustom: keep\n---\n# Body preview\n- [ ] Read only\n```dataview\nTASK\n```";
        File.WriteAllText(first, note); File.WriteAllText(Path.Combine(folder, "other.md"), "# Other");
        Directory.CreateDirectory(Path.Combine(folder, ".hidden")); File.WriteAllText(Path.Combine(folder, ".hidden", "secret.md"), "Hidden");
        File.WriteAllText(Path.Combine(folder, "not-markdown.txt"), "Ignore");
        var settings = new Settings { ObsidianTasksEnabled = false, NotesRoot = basis, NotesFolder = folder };
        var source = new NoteSource(settings, false); string? opened = null;
        var browser = new NoteBrowserWindow(source, path => opened = path);
        try
        {
            var load = browser.InitializeAsync(); WaitFor(() => load.IsCompleted && browser.Open.IsEnabled, "Note browser: initial folder and preview load"); load.GetAwaiter().GetResult();
            Check((string)browser.Folder.SelectedItem == "付箋" && browser.Notes.Items.Count == 2, "Note browser: starts at configured folder, includes closed notes and excludes hidden/non-Markdown files");
            Check(new TextRange(browser.Preview.Document.ContentStart, browser.Preview.Document.ContentEnd).Text.Contains("Body preview") && browser.Yaml.Text.Contains("custom: keep"), "Note browser: rendered body and original YAML shown separately");
            var checkbox = (CheckBox)browser.Preview.Document.Blocks.OfType<System.Windows.Documents.List>().Single().ListItems.FirstListItem.Blocks.OfType<Paragraph>().Single().Inlines.OfType<InlineUIContainer>().Single().Child;
            Check(!checkbox.IsEnabled && File.ReadAllText(first) == note, "Note browser: preview has no write action");
            var content = (FrameworkElement)browser.Content; content.Measure(new Size(880, 570)); content.Arrange(new Rect(0, 0, 880, 570)); content.UpdateLayout();
            RenderLocalizationPreview(content, "note-browser");
            browser.Search.Text = "other"; WaitFor(() => browser.Open.IsEnabled, "Note browser: filtered preview loads");
            Check(browser.Notes.Items.Count == 1, "Note browser: filename filtering works");
            browser.Search.Text = "closed"; WaitFor(() => browser.Open.IsEnabled, "Note browser: closed-note preview loads");
            browser.OpenSelected(); Check(opened == first, "Note browser: selected closed note opened by its full path");
        }
        finally { browser.Close(); }
        File.WriteAllBytes(Path.Combine(folder, "a-invalid.md"), [255]);
        browser = new NoteBrowserWindow(source, _ => { });
        try
        {
            var load = browser.InitializeAsync(); WaitFor(() => load.IsCompleted, "Note browser: invalid UTF-8 preview settles");
            Check(!browser.Open.IsEnabled && browser.Status.Text != L10n.Format("NoteBrowser.Count", 3) && browser.Status.Text.Length > 0,
                "Note browser: synchronous preview failure remains visible instead of being replaced by note count");
        }
        finally { browser.Close(); }
        Throws<InvalidOperationException>(() => source.BrowseNotesAsync(Path.Combine(root, "outside")).GetAwaiter().GetResult(), "Note browser: folder cannot escape configured base");
        var cli = new NoteSource(new Settings { ObsidianVaultFolder = basis }, true, (_, _, _) => Task.FromResult("STICKY_TASKS_PREVIEW:{\"notes\":[\"../outside.md\"]}"));
        Throws<InvalidOperationException>(() => cli.BrowseNotesAsync(basis).GetAwaiter().GetResult(), "Note browser: CLI path traversal response rejected");
        cli = new NoteSource(new Settings { ObsidianVaultFolder = basis }, true, (_, _, _) => Task.FromException<string>(new IOException("CLI unavailable")));
        Throws<IOException>(() => cli.BrowseNotesAsync(basis).GetAwaiter().GetResult(), "Note browser: CLI failure never falls back to local enumeration");
        var pending = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        cli = new NoteSource(new Settings { ObsidianVaultFolder = basis, ObsidianNotesFolder = "." }, true, (_, code, _) =>
        {
            var request = CliRequest(code);
            return request.GetProperty("mode").GetString() switch
            {
                "folders" => Task.FromResult("STICKY_TASKS_PREVIEW:{\"folders\":[\".\"],\"defaultFolder\":\".\"}"),
                "browse" => Task.FromResult("STICKY_TASKS_PREVIEW:{\"notes\":[\"a.md\",\"b.md\"]}"),
                _ => request.GetProperty("path").GetString() == "a.md" ? pending.Task : Task.FromResult(CliSnapshotEnvelope("# Latest"))
            };
        });
        browser = new NoteBrowserWindow(cli, _ => { });
        try
        {
            var loading = browser.InitializeAsync(); WaitFor(() => loading.IsCompleted, "Note browser: CLI file list loads independently of preview");
            browser.Notes.SelectedIndex = 1;
            WaitFor(() => browser.Open.IsEnabled, "Note browser: newer selection preview loads");
            pending.SetException(new IOException("Old preview error"));
            var settle = System.Diagnostics.Stopwatch.StartNew(); WaitFor(() => settle.ElapsedMilliseconds >= 100, "Note browser: old preview request settles");
            Check(new TextRange(browser.Preview.Document.ContentStart, browser.Preview.Document.ContentEnd).Text.Contains("Latest") && !browser.Status.Text.Contains("Old preview"), "Note browser: late selection errors do not replace latest preview");
        }
        finally { browser.Close(); }
    }
}
