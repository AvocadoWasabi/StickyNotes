using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using StickyNotes;
using StickyNotes.Core;

internal static class Program
{
    private static int count;
    private static void Check(bool condition, string name)
    {
        if (!condition) throw new Exception("FAIL: " + name);
        Console.WriteLine("PASS: " + name); count++;
    }
    private static void Throws<T>(Action action, string name) where T : Exception
    {
        try { action(); } catch (T) { Check(true, name); return; }
        throw new Exception("FAIL: " + name);
    }

    [STAThread]
    public static void Main()
    {
        var root = Path.Combine(Path.GetTempPath(), "StickyNotes.Tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var backups = Path.Combine(root, "backups");
        try
        {
            const string original = "---\r\ntags: [daily]\r\ncustom: keep\r\n---\r\n# Journal\r\nintro\r\n## Tasks\r\n- [ ] One\r\n### Sub\r\n- [x] Two\r\n```md\r\n## fake\r\n```\r\n## Log\r\nuntouched\r\n";
            var span = SectionEditor.Find(original, "Tasks");
            Check(span.Content.Contains("## fake") && !span.Content.Contains("untouched"), "section includes children and ignores fenced headings");
            var changed = SectionEditor.Replace(original, "Tasks", "- [x] One\n");
            Check(changed.StartsWith(original[..span.Start]) && changed.EndsWith("## Log\r\nuntouched\r\n"), "section edit preserves prefix, following section and CRLF");
            Check(changed.Contains("- [x] One\r\n## Log"), "section edit uses existing newline convention");
            Throws<InvalidOperationException>(() => SectionEditor.Find("## Tasks\na\n## Tasks\nb", "Tasks"), "duplicate headings fail safely");
            Throws<InvalidOperationException>(() => SectionEditor.Find(original, "Missing"), "missing heading fails safely");
            Throws<InvalidOperationException>(() => SectionEditor.Replace(original, "Tasks", "## Unexpected\n"), "peer heading cannot escape linked region");
            Check(SectionEditor.Find("~~~\n## Tasks\n~~~\n## Tasks\nyes\n", "Tasks").Content == "yes\n", "tilde fenced headings ignored");
            var task = SectionEditor.ToggleTaskAtLine("- [ ] first\r\n  - [ ] second\r\n", 1, true);
            Check(task == "- [ ] first\r\n  - [x] second\r\n", "nested task update affects only selected line");
            Throws<InvalidOperationException>(() => SectionEditor.ToggleTaskAtLine("# heading", 0, true), "non-task cannot be toggled");

            var note = NoteStore.Create(root);
            var snap = NoteStore.Read(note);
            var meta = NoteStore.Metadata(snap.Text);
            Check(meta.Tags.SequenceEqual(new[] { "sticky" }) && meta.Color == "yellow", "new note has Obsidian properties");
            var metadataText = NoteStore.WithMetadata(original, new("a: title", ["project/日本語", "task"], "done", "blue"));
            Check(NoteStore.Split(metadataText).Body == NoteStore.Split(original).Body && metadataText.Contains("custom: keep"), "metadata edit retains body and unknown property");
            Check(NoteStore.Metadata(metadataText).Title == "a: title", "YAML escaping round trip");
            var saved = NoteStore.Save(snap, snap.Text + "\nnew", backups);
            Check(File.ReadAllText(note) == saved.Text && Directory.GetFiles(backups).Length == 1, "save writes note and recoverable backup");
            File.AppendAllText(note, " external");
            Throws<ConflictException>(() => NoteStore.Save(saved, "bad", backups), "external edits block stale write");
            Check(File.ReadAllText(note).EndsWith(" external"), "conflict preserves external content");
            var bomPath = Path.Combine(root, "bom.md");
            File.WriteAllText(bomPath, "# UTF8\r\n日本語", new UTF8Encoding(true));
            var bom = NoteStore.Read(bomPath);
            NoteStore.Save(bom, bom.Text + "!", backups);
            Check(File.ReadAllBytes(bomPath).Take(3).SequenceEqual(new byte[] { 239, 187, 191 }), "UTF8 BOM preserved");
            MigrationTests(root);

            var query = CalendarQuery.Parse("@calendar 2026-10-05T09:00+09:00 設計 会議");
            Check(query.From.Offset == TimeSpan.FromHours(9) && query.Search == "設計 会議", "calendar query parses offset and multiword search");
            Throws<FormatException>(() => CalendarQuery.Parse("@calendar invalid"), "invalid date rejected");
            Check(CalendarQuery.Parse("@calendar 2026-10-05").Search == "", "calendar search is optional");
            CalendarTests(root).GetAwaiter().GetResult();

            var toggledLine = -1;
            var doc = MarkdownView.Render("# Title\n\n- [ ] task\n\n```md\n- [ ] example\n```\n\n| A | B |\n|---|---|\n| x | y |", (line, _) => toggledLine = line);
            Check(doc.Blocks.OfType<Paragraph>().Any(p => p.FontSize == 25), "Markdown heading rendered");
            Check(doc.Blocks.OfType<Table>().Count() == 1, "Markdown table rendered");
            var list = doc.Blocks.OfType<System.Windows.Documents.List>().Single();
            var paragraph = list.ListItems.FirstListItem.Blocks.OfType<Paragraph>().Single();
            var checkbox = (CheckBox)paragraph.Inlines.OfType<InlineUIContainer>().Single().Child;
            checkbox.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
            Check(toggledLine == 2, "rendered task callback maps to source line, excluding fenced example");
            Check(MarkdownView.FindCalendarCommand("```text\n@calendar 2026-10-05 example\n```\n\n@calendar 2026-10-06 real") == "@calendar 2026-10-06 real", "calendar examples in fenced code are not executed");
            Console.WriteLine($"\n{count} tests passed.");
        }
        finally { Directory.Delete(root, true); }
    }

    private static void MigrationTests(string root)
    {
        var source = Path.Combine(root, "migration-source");
        var destination = Path.Combine(root, "migration-destination");
        Directory.CreateDirectory(Path.Combine(source, "nested"));
        var first = Path.Combine(source, "a.md");
        var second = Path.Combine(source, "nested", "b.MD");
        File.WriteAllText(first, "---\r\ntype: sticky\r\n---\r\n日本語", new UTF8Encoding(true));
        File.WriteAllText(second, "closed note");
        File.WriteAllText(Path.Combine(source, "keep.txt"), "unrelated");
        var bytes = File.ReadAllBytes(first);
        Check(NoteFolderMigration.SameFolder(source, source.ToUpperInvariant() + Path.DirectorySeparatorChar), "folder comparison normalizes case and trailing separator");
        Throws<InvalidOperationException>(() => NoteFolderMigration.Normalize("relative"), "migration rejects relative paths");
        Throws<IOException>(() => NoteFolderMigration.Move(source, Path.Combine(source, "child"), _ => { }), "migration rejects descendant destination");
        Throws<IOException>(() => NoteFolderMigration.Move(source, root, _ => { }), "migration rejects ancestor destination");
        Directory.CreateDirectory(Path.Combine(destination, "nested"));
        File.WriteAllText(Path.Combine(destination, "nested", "b.MD"), "existing");
        Throws<IOException>(() => NoteFolderMigration.Move(source, destination, _ => throw new Exception("must not commit")), "collision preflight stops all moves");
        Check(File.Exists(first) && File.ReadAllText(Path.Combine(destination, "nested", "b.MD")) == "existing", "collision preserves both folders");
        File.Delete(Path.Combine(destination, "nested", "b.MD"));
        using (var locked = new FileStream(second, FileMode.Open, FileAccess.Read, FileShare.Read))
            Throws<IOException>(() => NoteFolderMigration.Move(source, destination, _ => throw new Exception("must not commit")), "locked file rolls back earlier moves");
        Check(File.Exists(first) && File.Exists(second) && !File.Exists(Path.Combine(destination, "a.md")), "move failure restores source files");
        Throws<InvalidOperationException>(() => NoteFolderMigration.Move(source, destination, _ => throw new InvalidOperationException("settings write failed")), "settings failure rolls back migration");
        Check(File.ReadAllBytes(first).SequenceEqual(bytes) && File.Exists(second), "rollback preserves exact note bytes");
        NoteFolderMigration.Move(source, destination, paths =>
        {
            Check(paths.Count == 2 && paths[second] == Path.Combine(destination, "nested", "b.MD"), "migration maps all Markdown including closed and nested notes");
            Check(paths.Values.All(File.Exists), "settings committed only after all files move");
        });
        Check(!File.Exists(first) && !File.Exists(second) && File.ReadAllBytes(Path.Combine(destination, "a.md")).SequenceEqual(bytes), "migration preserves BOM and bytes and removes originals");
        Check(File.Exists(Path.Combine(source, "keep.txt")), "migration leaves non-Markdown files in place");
        NoteFolderMigration.Move(Path.Combine(root, "missing-source"), Path.Combine(root, "empty-destination"), paths => Check(paths.Count == 0, "missing old folder allows first-time setup"));
        NoteFolderMigration.Move(destination, destination + Path.DirectorySeparatorChar, paths => Check(paths.Count == 0, "same folder does not move files"));
        var recoverySource = Path.Combine(root, "recovery-source");
        var recoveryDestination = Path.Combine(root, "recovery-destination");
        Directory.CreateDirectory(recoverySource);
        File.WriteAllText(Path.Combine(recoverySource, "note.md"), "original");
        try
        {
            NoteFolderMigration.Move(recoverySource, recoveryDestination, _ =>
            {
                File.WriteAllText(Path.Combine(recoverySource, "note.md"), "concurrent creation");
                throw new IOException("settings failure");
            });
            throw new Exception("Expected rollback failure");
        }
        catch (IOException error)
        {
            Check(error.Message.Contains(Path.Combine(recoveryDestination, "note.md")), "rollback failure reports recoverable file location");
        }
        Check(File.ReadAllText(Path.Combine(recoverySource, "note.md")) == "concurrent creation" && File.ReadAllText(Path.Combine(recoveryDestination, "note.md")) == "original", "rollback never overwrites concurrent source files");

        var app = new App();
        typeof(App).GetProperty(nameof(App.DataDirectory))!.SetValue(null, Path.Combine(root, "app-settings"));
        Directory.CreateDirectory(App.DataDirectory);
        app.Config.NotesFolder = destination;
        app.Config.DailyFolder = Path.Combine(destination, "nested");
        var placement = new NotePlacement { Path = Path.Combine(destination, "a.md"), Left = 234, Pinned = true };
        var window = new NoteWindow(placement);
        app.Notes.Add(window);
        var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        var snapshotField = typeof(NoteWindow).GetField("snapshot", flags)!;
        var dirtyField = typeof(NoteWindow).GetField("dirty", flags)!;
        var editor = (TextBox)typeof(NoteWindow).GetField("editor", flags)!.GetValue(window)!;
        var snapshot = NoteStore.Read(placement.Path);
        snapshotField.SetValue(window, snapshot);
        editor.Text = "unsaved edit";
        typeof(NoteWindow).GetField("editing", flags)!.SetValue(window, true);
        var dailyWindow = new NoteWindow(new NotePlacement { Daily = true, Heading = "Tasks" });
        snapshotField.SetValue(dailyWindow, NoteStore.Read(Path.Combine(destination, "nested", "b.MD")));
        app.Notes.Add(dailyWindow);
        var outside = new NoteWindow(new NotePlacement { Path = Path.Combine(root, "bom.md"), Heading = "Tasks" });
        app.Notes.Add(outside);
        var next = Path.Combine(root, "app-migrated");
        app.ApplySettings(new Settings { NotesFolder = next, DailyFolder = app.Config.DailyFolder }, true);
        Check(placement.Path == Path.Combine(next, "a.md") && placement.Left == 234 && placement.Pinned, "open window follows migration and retains placement");
        Check(((FileSnapshot)snapshotField.GetValue(window)!).Path == placement.Path && editor.Text == "unsaved edit" && (bool)dirtyField.GetValue(window)!, "migration retains unsaved editor and updates snapshot path");
        Check(app.Config.DailyFolder == Path.Combine(next, "nested"), "daily folder inside source follows migration");
        Check(((FileSnapshot)snapshotField.GetValue(dailyWindow)!).Path == Path.Combine(next, "nested", "b.MD") && outside.Placement.Path == Path.Combine(root, "bom.md"), "daily snapshot follows move while outside linked note stays in place");
        Check(JsonSerializer.Deserialize<Settings>(File.ReadAllText(Path.Combine(App.DataDirectory, "settings.json")))!.Windows[0].Path == placement.Path, "restart settings contain migrated note paths");
        var oldConfig = app.Config;
        // Block the temporary settings file to exercise application + filesystem rollback together.
        Directory.CreateDirectory(Path.Combine(App.DataDirectory, "settings.json.tmp"));
        Throws<UnauthorizedAccessException>(() => app.ApplySettings(new Settings { NotesFolder = destination }, true), "settings persistence error aborts application migration");
        Check(ReferenceEquals(app.Config, oldConfig) && File.Exists(placement.Path) && ((FileSnapshot)snapshotField.GetValue(window)!).Path == placement.Path, "settings failure restores config, placement, snapshot, and files");
        Directory.Delete(Path.Combine(App.DataDirectory, "settings.json.tmp"));
        Check((bool)typeof(NoteWindow).GetMethod("Save", flags)!.Invoke(window, null)! && File.ReadAllText(placement.Path).Contains("unsaved edit") && !File.Exists(Path.Combine(destination, "a.md")), "unsaved editing saves to migrated file without recreating original");
        app.ApplySettings(new Settings { NotesFolder = destination }, false);
        Check(app.Config.NotesFolder == destination && placement.Path == Path.Combine(next, "a.md") && File.Exists(placement.Path), "declining migration changes only new-note folder");
    }

    private static async Task CalendarTests(string root)
    {
        var credentials = Path.Combine(root, "test-oauth.json");
        File.WriteAllText(credentials, "{\"installed\":{\"client_id\":\"test-client\"}}");
        var stored = JsonSerializer.Serialize(new { clientId = "test-client", tokens = new OAuthTokens { AccessToken = "test-token", RefreshToken = "test-refresh", ExpiresAt = DateTimeOffset.UtcNow.AddHours(1) } });
        var handler = new FakeHandler();
        using var service = new CalendarService(() => stored, _ => { }, handler);
        service.Configure(credentials);
        var events = await service.SearchAsync("primary", CalendarQuery.Parse("@calendar 2026-10-05T09:00+09:00 会議"));
        Check(events.Count == 1 && events[0].Summary == "Meeting", "calendar response decoded");
        Check(handler.Uri!.Contains("singleEvents=true") && handler.Uri.Contains("timeMin=") && handler.Uri.Contains("q="), "calendar search bounded, ordered and filtered");
        await service.UpdateAsync("primary", events[0], "Changed", "Description");
        Check(handler.Method == HttpMethod.Patch && handler.ETag == "\"v1\"", "calendar update uses PATCH and If-Match");
        using var body = JsonDocument.Parse(handler.Body!);
        Check(body.RootElement.GetProperty("summary").GetString() == "Changed" && !body.RootElement.TryGetProperty("start", out _), "calendar edit changes only title and description");
        handler.Conflict = true;
        try { await service.UpdateAsync("primary", events[0], "Changed", ""); throw new Exception("Expected conflict"); }
        catch (ConflictException) { Check(true, "Google etag conflict becomes safe user-facing error"); }
    }

    private sealed class FakeHandler : HttpMessageHandler
    {
        public string? Uri, ETag, Body;
        public HttpMethod? Method;
        public bool Conflict;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Uri = request.RequestUri!.OriginalString; Method = request.Method;
            ETag = request.Headers.TryGetValues("If-Match", out var values) ? values.Single() : null;
            Body = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
            const string item = "{\"id\":\"event-1\",\"summary\":\"Meeting\",\"description\":\"old\",\"start\":{\"dateTime\":\"2026-10-05T10:00:00+09:00\"},\"etag\":\"\\\"v1\\\"\"}";
            return new HttpResponseMessage(Conflict ? HttpStatusCode.PreconditionFailed : HttpStatusCode.OK) { Content = new StringContent(request.Method == HttpMethod.Get ? "{\"items\":[" + item + "]}" : item) };
        }
    }
}
