using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using StickyNotes;
using StickyNotes.Core;

internal static partial class Program
{
    private static IEnumerable<DependencyObject> LogicalDescendants(DependencyObject root)
    {
        foreach (var child in LogicalTreeHelper.GetChildren(root).OfType<DependencyObject>())
        {
            yield return child;
            foreach (var descendant in LogicalDescendants(child)) yield return descendant;
        }
    }

    private static void MultipleVaultTests(string root)
    {
        var legacy = JsonSerializer.Deserialize<Settings>("{\"ObsidianTasksEnabled\":false,\"Windows\":[{\"Daily\":true,\"Heading\":\"Tasks\"}],\"RecentlyClosedWindows\":[{\"Path\":\"closed.md\"}]}")!;
        Check(legacy.GetVaultProfiles().Single().Id == Settings.MainVaultId && legacy.GetVaultProfiles()[0].Name == L10n.Text("Vault.Main") &&
            legacy.Windows[0].VaultId == Settings.MainVaultId && legacy.RecentlyClosedWindows[0].VaultId == Settings.MainVaultId,
            "Multiple Vaults: legacy settings, placements and history migrate to Main without changing files");
        Throws<InvalidOperationException>(() => legacy.ForVault("missing"), "Multiple Vaults: unknown ID never falls back to Main");
        legacy.ObsidianVaultFolder = Path.Combine(root, "inactive-cli"); legacy.NotesRoot = Path.Combine(root, "active-local");
        legacy.SetVaultProfiles(legacy.GetVaultProfiles());
        Check(legacy.ObsidianVaultFolder == Path.Combine(root, "inactive-cli") && legacy.NotesRoot == Path.Combine(root, "active-local"),
            "Multiple Vaults: saving profiles preserves inactive-mode connection settings");
        var mainRoot = Path.Combine(root, "multi-main"); var subRoot = Path.Combine(root, "multi-sub");
        Directory.CreateDirectory(mainRoot); Directory.CreateDirectory(subRoot);
        var filename = DateTime.Today.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture) + ".md";
        var mainPath = Path.Combine(mainRoot, filename); var subPath = Path.Combine(subRoot, filename);
        File.WriteAllText(mainPath, "## Tasks\n- [ ] Main\n"); File.WriteAllText(subPath, "## Tasks\n- [ ] Sub\n");
        var app = App.Current; var previous = app.Config; var oldNotes = app.Notes.ToArray();
        var factory = app.NoteSources; var tasks = app.TasksQueries; var dataview = app.DataviewQueries;
        var flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var snapshotField = typeof(NoteWindow).GetField("snapshot", flags)!;
        var reload = typeof(NoteWindow).GetMethod("Reload", flags)!;
        var saveContent = typeof(NoteWindow).GetMethod("SaveContent", flags)!;
        var windows = new List<Window>();
        try
        {
            app.Notes.Clear();
            app.ApplySettings(new Settings { ObsidianTasksEnabled = false, NotesFolder = mainRoot, NotesRoot = mainRoot, DailyFolder = mainRoot }, false);
            app.Config.RecentlyClosedWindows.Clear(); app.SaveConfig();
            var before = File.ReadAllBytes(Path.Combine(App.DataDirectory, "settings.json"));
            var settings = new SettingsWindow(); windows.Add(settings);
            var add = LogicalDescendants(settings).OfType<Button>().Single(b => b.Name == "AddVault");
            add.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
            var names = LogicalDescendants(settings).OfType<TextBox>().Single(t => t.Name == "VaultDisplayName");
            Check(names.Text == L10n.Text("Vault.Sub"), "Multiple Vaults: first added profile starts as Sub");
            names.Text = "Discard me"; settings.Close();
            Check(app.Config.AdditionalVaults.Count == 0 && File.ReadAllBytes(Path.Combine(App.DataDirectory, "settings.json")).SequenceEqual(before),
                "Multiple Vaults: closing settings discards added profiles and edited names");

            settings = new SettingsWindow(); windows.Add(settings);
            Check(settings.GeneralPanel.Children.OfType<StackPanel>().SelectMany(p => p.Children.OfType<TextBox>()).Any(t => t.Text == app.Config.ObsidianCli && t.Visibility == Visibility.Visible),
                "Multiple Vaults: shared CLI executable stays visible in General even in local mode");
            settings.Categories.SelectedIndex = 1;
            LogicalDescendants(settings).OfType<Button>().Single(b => b.Name == "AddVault").RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
            names = LogicalDescendants(settings).OfType<TextBox>().Single(t => t.Name == "VaultDisplayName"); names.Text = "Archive";
            var fields = settings.VaultDetailsPanel.Children.OfType<TextBox>().ToArray(); fields[0].Text = subRoot; fields[1].Text = subRoot;
            var vaultList = LogicalDescendants(settings).OfType<ListBox>().Single(l => l.Name == "VaultList");
            var subId = ((VaultProfile)vaultList.SelectedItem).Id;
            var defaults = LogicalDescendants(settings).OfType<ComboBox>().Single(c => c.Name == "DefaultDailyVault"); defaults.SelectedValue = subId;
            vaultList.SelectedIndex = 0;
            Check(fields[0].Text == mainRoot && names.Text == L10n.Text("Vault.Main"), "Multiple Vaults: switching editors preserves separate roots and names");
            names.Text = "Primary"; vaultList.SelectedIndex = 1;
            Check(fields[0].Text == subRoot && names.Text == "Archive", "Multiple Vaults: returning to Sub restores unsaved details");
            var layout = (FrameworkElement)settings.Content;
            layout.Measure(new Size(1000, 740)); layout.Arrange(new Rect(0, 0, 1000, 740)); layout.UpdateLayout();
            var vaultPanes = LogicalDescendants((DependencyObject)((TabItem)settings.Categories.Items[1]).Content).OfType<ScrollViewer>().ToArray();
            Check(vaultPanes.Length == 2 && vaultPanes.All(p => p.ActualWidth > 0 && p.ScrollableWidth == 0), "Multiple Vaults: list and detail panels fit the settings viewport");
            RenderLocalizationPreview(layout, "multiple-vault-settings");
            settings.Footer.Children.OfType<Button>().Last().RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
            Check(app.Config.VaultName(Settings.MainVaultId) == "Primary" && app.Config.VaultName(subId) == "Archive" && app.Config.DefaultDailyVaultId == subId,
                "Multiple Vaults: Save commits editable names and new-note default together");
            var roundtrip = JsonSerializer.Deserialize<Settings>(JsonSerializer.Serialize(app.Config))!;
            Check(roundtrip.ForVault(subId).DailyFolder == subRoot && roundtrip.ForVault(Settings.MainVaultId).DailyFolder == mainRoot &&
                roundtrip.AdditionalVaults.Single().Id == subId, "Multiple Vaults: IDs and per-Vault source settings survive restart");

            var dialog = new NoteLinkWindow(true); windows.Add(dialog); dialog.LoadSource(); dialog.Headings.Input.Text = "Tasks";
            var dailySub = dialog.Prepare(); dialog.Close();
            Check(dailySub.VaultId == subId && dailySub.Daily && dailySub.Path == "", "Multiple Vaults: daily picker starts on default Vault and binds its ID");
            var main = new NoteWindow(new() { Daily = true, Heading = "Tasks" }); windows.Add(main);
            var sub = new NoteWindow(dailySub); windows.Add(sub);
            reload.Invoke(main, null); reload.Invoke(sub, null);
            Check(((FileSnapshot)snapshotField.GetValue(main)!).Path == mainPath && ((FileSnapshot)snapshotField.GetValue(sub)!).Path == subPath,
                "Multiple Vaults: same-date same-heading local daily notes resolve independently");
            saveContent.Invoke(sub, ["- [x] Sub changed\n"]);
            Check(File.ReadAllText(subPath).Contains("Sub changed") && File.ReadAllText(mainPath).Contains("- [ ] Main"),
                "Multiple Vaults: editing Sub never writes the matching Main daily note");
            app.Notes.Add(main); app.Notes.Add(sub);
            app.ShowNote(dailySub.Copy());
            Check(app.Notes.Count == 2, "Multiple Vaults: duplicate detection includes the stable Vault ID");
            var renamed = JsonSerializer.Deserialize<Settings>(JsonSerializer.Serialize(app.Config))!;
            renamed.AdditionalVaults[0].Name = "Renamed"; renamed.DefaultDailyVaultId = Settings.MainVaultId;
            app.ApplySettings(renamed, false);
            Check(sub.SourceSettings.DailyFolder == subRoot && sub.Placement.VaultId == subId && sub.Title == Path.GetFileNameWithoutExtension(filename),
                "Multiple Vaults: name and default changes preserve existing source and filename title");
            Check(LogicalDescendants(sub).OfType<TextBlock>().Single(t => t.Name == "VaultLabel").Text == "Renamed", "Multiple Vaults: source label updates independently of filename title");
            var removed = JsonSerializer.Deserialize<Settings>(JsonSerializer.Serialize(app.Config))!; removed.AdditionalVaults.Clear();
            Throws<InvalidOperationException>(() => app.ApplySettings(removed, false), "Multiple Vaults: open note prevents registry removal");
            main.Close(); sub.Close();
            Check(app.Config.RecentlyClosedWindows.Any(p => p.VaultId == subId), "Multiple Vaults: close history preserves source ID");
            Throws<InvalidOperationException>(() => app.ApplySettings(removed, false), "Multiple Vaults: recently closed note prevents registry removal");
            app.RestoreRecentlyClosedNotes();
            Check(app.Notes.Any(n => n.Placement.VaultId == subId && n.SourceSettings.DailyFolder == subRoot), "Multiple Vaults: restoring closed notes retains the original Vault");
            foreach (var note in app.Notes.ToArray()) note.Close();
            app.Config.RecentlyClosedWindows.Clear();
            app.ApplySettings(removed, false);
            Check(File.Exists(subPath), "Multiple Vaults: removing an unused registration never deletes its files");

            // Simulated CLI transport exercises native daily reads, both query providers and writes with identical relative paths.
            var cli = new Settings { ObsidianTasksEnabled = true, ObsidianVaultFolder = mainRoot, ObsidianVaultId = "MainTarget", NotesFolder = mainRoot };
            cli.AdditionalVaults.Add(new() { Id = subId, Name = "Sub", UseCli = true, Folder = subRoot, CliTarget = "SubTarget", NotesFolder = subRoot });
            app.ApplySettings(cli, false);
            var requests = new System.Collections.Concurrent.ConcurrentQueue<(string Target, string Mode)>();
            var queries = new List<string>();
            TaskCompletionSource<string>? pendingMutation = null;
            var failSub = false;
            const string query = "## Tasks\n```tasks\nnot done\n```\n```dataview\nTASK\n```\n";
            app.NoteSources = config => new NoteSource(config, true, (captured, code, _) =>
            {
                var request = CliRequest(code); var mode = request.TryGetProperty("mode", out var value) ? value.GetString()! : "daily";
                requests.Enqueue((captured.ObsidianVaultId, mode));
                if (mode == "toggle-query-task" && pendingMutation is not null) return pendingMutation.Task;
                if (failSub && captured.ObsidianVaultId == "SubTarget") return Task.FromException<string>(new IOException("Sub disconnected"));
                if (mode == "daily") return Task.FromResult(DailyEnvelope(DateTime.Today, path: filename));
                return Task.FromResult(CliSnapshotEnvelope(mode == "save" ? request.GetProperty("text").GetString()! : query));
            });
            Task<TasksResponse> Query(Settings c, string path, string[] q, System.Threading.CancellationToken __)
            {
                Check(path == Path.Combine(c.ObsidianVaultId == "SubTarget" ? subRoot : mainRoot, filename), "Multiple Vaults: query path belongs to its CLI target");
                queries.Add(c.ObsidianVaultId);
                return Task.FromResult(new TasksResponse("test", q.Select(_ => new TasksOutput("- [ ] Task", null, [new(0, "target.md", 1, "cli:" + new string('a', 64))])).ToArray()));
            }
            app.TasksQueries = Query; app.DataviewQueries = Query;
            main = new NoteWindow(new() { Daily = true, Heading = "Tasks" }); sub = new NoteWindow(new() { Daily = true, Heading = "Tasks", VaultId = subId });
            windows.Add(main); windows.Add(sub); app.Notes.Add(main); app.Notes.Add(sub);
            reload.Invoke(main, null); reload.Invoke(sub, null);
            WaitFor(() => queries.Count(v => v == "MainTarget") >= 2 && queries.Count(v => v == "SubTarget") >= 2, "Multiple Vaults: Tasks and Dataview load for both registered Vaults");
            Check(queries.All(v => v is "MainTarget" or "SubTarget"),
                "Multiple Vaults: each query provider receives the placement's CLI target");
            var viewer = (FlowDocumentScrollViewer)typeof(NoteWindow).GetField("preview", flags)!.GetValue(sub)!;
            var checkbox = viewer.Document.Blocks.OfType<Section>().First().Blocks.OfType<System.Windows.Documents.List>().Single().ListItems.FirstListItem.Blocks.OfType<Paragraph>().Single().Inlines.OfType<InlineUIContainer>().Select(i => (CheckBox)i.Child).Single();
            pendingMutation = new(TaskCreationOptions.RunContinuationsAsynchronously);
            checkbox.IsChecked = true; checkbox.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
            Throws<InvalidOperationException>(() => app.ApplySettings(JsonSerializer.Deserialize<Settings>(JsonSerializer.Serialize(app.Config))!, false),
                "Multiple Vaults: settings cannot change during an in-flight query write");
            pendingMutation.SetResult(CliSnapshotEnvelope("- [x] Updated"));
            WaitFor(() => !sub.HasPendingWrite, "Multiple Vaults: query task write completes");
            Check(requests.Any(r => r == ("SubTarget", "toggle-query-task")) && !requests.Any(r => r == ("MainTarget", "toggle-query-task")),
                "Multiple Vaults: query checkbox updates only the bound Vault");
            saveContent.Invoke(sub, ["Sub CLI edit\n"]);
            Check(requests.Any(r => r == ("SubTarget", "save")) && !requests.Any(r => r == ("MainTarget", "save")), "Multiple Vaults: body saves use the bound CLI target");
            app.Config.AdditionalVaults[0].CliTarget = "OtherTarget";
            Throws<InvalidOperationException>(sub.EnsureWritableSource, "Multiple Vaults: changing a source blocks stale writes");
            var callsBeforeStaleSave = requests.Count;
            Throws<TargetInvocationException>(() => saveContent.Invoke(sub, ["stale"]), "Multiple Vaults: actual save rejects a stale source snapshot");
            Check(requests.Count == callsBeforeStaleSave, "Multiple Vaults: stale save is rejected before invoking any CLI");
            app.Config.AdditionalVaults[0].CliTarget = "SubTarget";
            typeof(NoteWindow).GetField("dirty", flags)!.SetValue(sub, true);
            var changed = JsonSerializer.Deserialize<Settings>(JsonSerializer.Serialize(app.Config))!; changed.AdditionalVaults[0].CliTarget = "OtherTarget";
            Throws<InvalidOperationException>(() => app.ApplySettings(changed, false), "Multiple Vaults: settings cannot redirect an unsaved draft");
            typeof(NoteWindow).GetField("dirty", flags)!.SetValue(sub, false);
            failSub = true; var beforeFailure = requests.Count;
            reload.Invoke(sub, null); reload.Invoke(main, null);
            WaitFor(() => ((TextBlock)typeof(NoteWindow).GetField("status", flags)!.GetValue(sub)!).Text.Contains("Sub disconnected"), "Multiple Vaults: connection failure is visible on the affected note");
            Check(((FileSnapshot)snapshotField.GetValue(main)!).Path == mainPath && requests.Skip(beforeFailure).Any(r => r.Target == "MainTarget") &&
                File.ReadAllText(subPath).Contains("Sub changed"), "Multiple Vaults: disconnected CLI Vault does not use local files or interrupt the other Vault");
        }
        finally
        {
            foreach (var note in app.Notes.ToArray()) { typeof(NoteWindow).GetField("dirty", flags)!.SetValue(note, false); note.Close(); }
            foreach (var window in windows) window.Close();
            app.Config.RecentlyClosedWindows.Clear(); app.NoteSources = factory; app.TasksQueries = tasks; app.DataviewQueries = dataview;
            app.ApplySettings(previous, false); app.Notes.AddRange(oldNotes); app.SaveConfig();
        }
    }
}
