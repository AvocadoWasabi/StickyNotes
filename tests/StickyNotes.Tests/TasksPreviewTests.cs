using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using StickyNotes;

internal static partial class Program
{
    private static async Task TasksCliSmoke()
    {
        var cli = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Obsidian", "Obsidian.com");
        var start = new System.Diagnostics.ProcessStartInfo(cli) { UseShellExecute = false, CreateNoWindow = true,
            RedirectStandardOutput = true, RedirectStandardError = true, StandardOutputEncoding = Encoding.UTF8, StandardErrorEncoding = Encoding.UTF8 };
        start.ArgumentList.Add("eval");
        start.ArgumentList.Add("code=JSON.stringify({root:app.vault.adapter.getBasePath(),name:app.vault.getName(),path:app.vault.getMarkdownFiles()[0]?.path})");
        using var process = System.Diagnostics.Process.Start(start)!;
        using var timeout = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(30));
        var stdout = process.StandardOutput.ReadToEndAsync(timeout.Token);
        var stderr = process.StandardError.ReadToEndAsync(timeout.Token);
        try { await process.WaitForExitAsync(timeout.Token); }
        finally { if (!process.HasExited) process.Kill(); }
        Check(process.ExitCode == 0, "Tasks desktop: CLI metadata request succeeds");
        var output = (await stdout).Split('\n').Last(l => l.StartsWith("=> "))[3..];
        await stderr;
        using var data = JsonDocument.Parse(output);
        var root = data.RootElement.GetProperty("root").GetString()!;
        var path = Path.Combine(root, data.RootElement.GetProperty("path").GetString()!);
        var before = File.ReadAllBytes(path);
        var settings = new StickyNotes.Core.Settings { ObsidianCli = cli, ObsidianVaultFolder = root,
            ObsidianVaultId = data.RootElement.GetProperty("name").GetString()! };
        var cliNote = await new NoteSource(settings, true).ReadAsync(path, timeout.Token);
        Check(cliNote.Text == StickyNotes.Core.NoteStore.Read(path).Text, "CLI desktop: existing note text matches direct UTF-8 reading");
        var response = await ObsidianTasksClient.QueryAsync(settings, path,
            ["ignore global query\nnot done\nlimit 3\nsort by due\ngroup by filename",
             "invalid-instruction-sticky-preview",
             "ignore global query\npath includes sticky-preview-no-match-" + Guid.NewGuid().ToString("N"),
             "ignore global query\npath includes {{query.file.path}}\nlimit 3"], timeout.Token);
        Check(response.Results[0].Error is null && response.Results[0].Markdown is not null, "Tasks desktop: native filter/sort/group Markdown export succeeds");
        Check(response.Results[1].Error is not null, "Tasks desktop: invalid query reported per block");
        Check(response.Results[2].Error is null && !response.Results[2].Markdown!.Contains("- ["), "Tasks desktop: no-match query is a successful empty task result");
        Check(response.Results[3].Error is null, "Tasks desktop: source-note placeholder resolves");
        Check(File.ReadAllBytes(path).SequenceEqual(before), "Tasks desktop: source Markdown unchanged");
        Console.WriteLine("Tasks desktop adapter version: " + response.Version);
        await NativeDailyCliSmoke(settings);
        await CliNoteFixtureSmoke(settings);
    }

    private static void TasksPreviewTests(string root)
    {
        Check(NoteSource.UsesCli(new StickyNotes.Core.Settings()), "CLI integration: enabled by default in every build");
        Check(!JsonSerializer.Deserialize<StickyNotes.Core.Settings>("{\"ObsidianTasksEnabled\":false}")!.ObsidianTasksEnabled, "CLI integration: explicit legacy local mode survives loading");
        Check(NoteSource.UsesCli(new StickyNotes.Core.Settings { ObsidianTasksEnabled = true }), "CLI integration: settings enable it in the standard build too");
        var vault = Path.Combine(root, "vault");
        Check(ObsidianTasksClient.RelativeNotePath(vault, Path.Combine(vault, "日本語", "note.md")) == "日本語/note.md", "Tasks: vault relative Unicode path");
        Throws<InvalidOperationException>(() => ObsidianTasksClient.RelativeNotePath(vault, Path.Combine(root, "vault-other", "note.md")), "Tasks: sibling vault rejected");
        Throws<InvalidOperationException>(() => ObsidianTasksClient.RelativeNotePath(vault, Path.Combine(vault, "..", "outside.md")), "Tasks: traversal rejected");
        const string query = "description includes \"日本語\"\n# ');throw new Error('injected');//";
        var code = ObsidianTasksClient.BuildCode(vault, "note.md", [query]);
        var payload = code.Split("Buffer.from('")[1].Split("'")[0];
        using var compressed = new MemoryStream(Convert.FromBase64String(payload));
        using var inflated = new System.IO.Compression.ZLibStream(compressed, System.IO.Compression.CompressionMode.Decompress);
        using var data = JsonDocument.Parse(inflated);
        Check(data.RootElement.GetProperty("queries")[0].GetString() == query && !code.Contains(query), "Tasks: queries encoded as data, quotes and Unicode round trip");
        Throws<InvalidOperationException>(() => ObsidianTasksClient.BuildCode(vault, "note.md", [new string('x', 8001)]), "Tasks: command size bounded");
        ObsidianTasksClient.ValidateCommand("日本語Vault", code);
        Throws<InvalidOperationException>(() => ObsidianTasksClient.ValidateCommand(new string('語', 700), code), "CLI: encoded vault name included in IPC budget");
        Throws<InvalidOperationException>(() => ObsidianTasksClient.ValidateCommand("vault", new string('x', 4000)), "CLI: oversized request rejected before launching Obsidian");
        var largeText = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(5000));
        Throws<InvalidOperationException>(() => ObsidianTasksClient.BuildRequestCode(new { text = largeText }, notes: true), "CLI: incompressible note text rejected before launching Obsidian");
        var response = ObsidianTasksClient.ParseResponse("startup log\n=> STICKY_TASKS_PREVIEW:{\"version\":\"7.23.1\",\"results\":[{\"markdown\":\"- [ ] 日本語\\n\",\"error\":null}]}\n", 1);
        Check(response.Results[0].Markdown == "- [ ] 日本語\n", "Tasks: CLI framing and Markdown preserved");
        Throws<InvalidOperationException>(() => ObsidianTasksClient.ParseResponse("=> STICKY_TASKS_PREVIEW:{\"error\":\"wrong vault\"}", 1), "Tasks: adapter errors surfaced");
        Throws<InvalidOperationException>(() => ObsidianTasksClient.ParseResponse("=> STICKY_TASKS_PREVIEW:{\"version\":\"x\",\"results\":[]}", 1), "Tasks: partial response rejected");
        const string mixed = "Before\n\n```tasks\nnot done\n```\n\n- [ ] Local task\n\n> ~~~tasks\n> done\n> ~~~\n\n```text\n```tasks\n```\n\nAfter";
        Check(NoteWindow.FindTasksBlocks(mixed).Length == 2, "Tasks: nested Tasks blocks found, examples excluded");
        var calls = 0; var toggled = -1;
        var doc = MarkdownView.Render(mixed, (line, _) => toggled = line,
            _ => { calls++; return new Section(new Paragraph(new Run("Native result"))); });
        Check(calls == 2 && new TextRange(doc.ContentStart, doc.ContentEnd).Text.Contains("After"), "Tasks: results coexist with normal Markdown");
        var list = doc.Blocks.OfType<System.Windows.Documents.List>().Single();
        var checkbox = (CheckBox)list.ListItems.FirstListItem.Blocks.OfType<Paragraph>().Single().Inlines.OfType<InlineUIContainer>().Single().Child;
        checkbox.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
        Check(toggled == 6, "Tasks: surrounding local checkbox retains original line mapping");
        var readonlyDoc = MarkdownView.Render("- [ ] Native task", (_, _) => throw new Exception("Read-only callback invoked"), readOnly: true);
        var readonlyList = readonlyDoc.Blocks.OfType<System.Windows.Documents.List>().Single();
        var readonlyCheck = (CheckBox)readonlyList.ListItems.FirstListItem.Blocks.OfType<Paragraph>().Single().Inlines.OfType<InlineUIContainer>().Single().Child;
        readonlyCheck.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
        Check(!readonlyCheck.IsEnabled, "Tasks: native result has no write callback");
        TasksPreviewWindowTests(root);
    }

    private static void TasksPreviewWindowTests(string root)
    {
        var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        var settings = App.Current.Config;
        var enabled = settings.ObsidianTasksEnabled;
        var vault = settings.ObsidianVaultFolder;
        var previousSources = App.Current.NoteSources;
        var previousQueries = App.Current.TasksQueries;
        var path = Path.Combine(root, "tasks-preview-ui.md");
        const string source = "# Mixed Markdown / 同居テスト\n\n通常の説明文 **Markdown**\n\n- [ ] 通常のチェックリスト\n\n```tasks\nnot done\nlimit 3\n```\n\n## メモ\nクエリの後の文章も表示します。\n\n```tasks\ndone\nlimit 1\n```\n";
        File.WriteAllText(path, source);
        NoteWindow? window = null;
        try
        {
            settings.ObsidianTasksEnabled = true; settings.ObsidianVaultFolder = root;
            App.Current.NoteSources = config => new NoteSource(config, NoteSource.UsesCli(config), (_, _, _) => Task.FromResult(CliSnapshotEnvelope(source)));
            App.Current.TasksQueries = (_, _, queries, _) => Task.FromResult(new TasksResponse("7.23.1", queries.Select(_ => new TasksOutput("", null)).ToArray()));
            window = new NoteWindow(new StickyNotes.Core.NotePlacement { Path = path, Width = 520, Height = 780 });
            typeof(NoteWindow).GetMethod("Reload", flags)!.Invoke(window, null);
            typeof(NoteWindow).GetField("tasksResponse", flags)!.SetValue(window,
                new TasksResponse("7.23.1", [new TasksOutput("### Project\n- [ ] 本家の検索結果 📅 2026-10-07\n", null), new TasksOutput("- [x] 完了したタスク\n", null)]));
            typeof(NoteWindow).GetField("tasksFetched", flags)!.SetValue(window, DateTime.Now);
            typeof(NoteWindow).GetMethod("RenderTasksPreview", flags)!.Invoke(window, [true]);
            var viewer = (FlowDocumentScrollViewer)typeof(NoteWindow).GetField("preview", flags)!.GetValue(window)!;
            var text = new TextRange(viewer.Document.ContentStart, viewer.Document.ContentEnd).Text;
            Check(text.Contains("本家の検索結果") && text.Contains("完了したタスク") && text.Contains("クエリの後の文章"), "Tasks preview window: two result blocks and prose coexist: " + ((TextBlock)typeof(NoteWindow).GetField("status", flags)!.GetValue(window)!).Text);
            Check(File.ReadAllText(path) == source, "Tasks preview window: rendering does not replace query source");
            var content = (FrameworkElement)window.Content;
            content.Measure(new Size(520, 780)); content.Arrange(new Rect(0, 0, 520, 780)); content.UpdateLayout();
            RenderLocalizationPreview(content, "tasks-preview-mixed");
            typeof(NoteWindow).GetField("tasksError", flags)!.SetValue(window, "Disconnected (test)");
            typeof(NoteWindow).GetMethod("RenderTasksPreview", flags)!.Invoke(window, [true]);
            text = new TextRange(viewer.Document.ContentStart, viewer.Document.ContentEnd).Text;
            Check(text.Contains("Disconnected (test)") && text.Contains("本家の検索結果"), "Tasks preview window: stale result retained with failure notice");
            settings.ObsidianTasksEnabled = false;
            typeof(NoteWindow).GetMethod("RenderTasksPreview", flags)!.Invoke(window, [false]);
            text = new TextRange(viewer.Document.ContentStart, viewer.Document.ContentEnd).Text;
            Check(text.Contains("not done") && !text.Contains("本家の検索結果"), "Tasks preview window: disabling restores source and clears cache");
        }
        finally
        {
            window?.Close(); settings.ObsidianTasksEnabled = enabled; settings.ObsidianVaultFolder = vault;
            App.Current.NoteSources = previousSources; App.Current.TasksQueries = previousQueries;
        }
    }
}
