using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using StickyNotes;
using StickyNotes.Core;

internal static partial class Program
{
    private static void FolderCommandTests(string root)
    {
        foreach (var target in new[] { "Main", "日本語の Vault", new string('名', 100) })
        {
            var settings = new Settings { ObsidianVaultFolder = root, ObsidianVaultId = target, ObsidianNotesFolder = "Notes" };
            var modes = new List<string>();
            var transfers = new List<string>();
            var source = new NoteSource(settings, true, (connection, code, _) =>
            {
                // The mock must enforce the production transport's serialized UTF-8 budget.
                ObsidianTasksClient.ValidateCommand(connection.ObsidianVaultId, code);
                if (code.Contains("readFileSync(Buffer.from('"))
                {
                    var path = Encoding.UTF8.GetString(Convert.FromBase64String(code.Split("Buffer.from('")[1].Split("'")[0]));
                    Check(File.Exists(path), "Folder command: transfer is readable during execution");
                    transfers.Add(path);
                }
                using var data = DecodeCliRequest(code);
                var request = data.RootElement;
                Check(request.GetProperty("root").GetString() == root && connection.ObsidianVaultId == target,
                    "Folder command: Vault root and explicit target survive transport");
                var mode = request.GetProperty("mode").GetString()!;
                modes.Add(mode);
                return Task.FromResult(mode switch
                {
                    "folders" => "STICKY_TASKS_PREVIEW:{\"folders\":[\".\",\"Notes\"],\"defaultFolder\":\"Notes\"}",
                    "browse" => "STICKY_TASKS_PREVIEW:{\"notes\":[\"Notes/example.md\"]}",
                    "read" => CliSnapshotEnvelope("---\ntitle: Example\n---\n# Preview\n"),
                    _ => throw new InvalidOperationException("Unexpected mode: " + mode)
                });
            });
            var browser = new NoteBrowserWindow(source, _ => { });
            try
            {
                var load = browser.InitializeAsync();
                WaitFor(() => load.IsCompleted, "Folder command: manager initialization completes");
                load.GetAwaiter().GetResult();
                Check(browser.Folder.Items.Contains("Notes") && browser.Notes.Items.Count == 1,
                    "Folder command: manager loads its folders and notes within CLI budget: " + browser.Status.Text);
                WaitFor(() => browser.Open.IsEnabled, "Folder command: selected note preview loads");
                Check(browser.Yaml.Text.Contains("title: Example") && modes.Contains("folders") && modes.Contains("browse") && modes.Contains("read"),
                    "Folder command: manager reaches folder listing, browsing and read-only preview");
                Check(transfers.Count > 0 && transfers.All(path => !File.Exists(path)), "Folder command: completed operations remove transfer files");
            }
            finally { browser.Close(); }
        }
        FolderTransferLifetimeTests(root);
        FolderRetryTests(root);
    }

    private static void FolderTransferLifetimeTests(string root)
    {
        var settings = new Settings { ObsidianVaultFolder = root, ObsidianVaultId = new string('名', 100) };
        foreach (var outcome in new[] { "success", "failure", "cancel" })
        {
            string? transferPath = null;
            var response = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
            var source = new NoteSource(settings, true, (connection, code, _) =>
            {
                ObsidianTasksClient.ValidateCommand(connection.ObsidianVaultId, code);
                transferPath = Encoding.UTF8.GetString(Convert.FromBase64String(code.Split("Buffer.from('")[1].Split("'")[0]));
                return response.Task;
            });
            var operation = source.GetFoldersAsync();
            Check(!operation.IsCompleted && transferPath is not null && File.Exists(transferPath), "Folder command: pending operation retains its private transfer");
            if (outcome == "success")
            {
                response.SetResult("STICKY_TASKS_PREVIEW:{\"folders\":[\".\"],\"defaultFolder\":\".\"}");
                Check(operation.GetAwaiter().GetResult().DefaultFolder == ".", "Folder command: delayed response is decoded before cleanup");
            }
            else if (outcome == "failure")
            {
                response.SetException(new IOException("Synthetic disconnect"));
                Throws<IOException>(() => operation.GetAwaiter().GetResult(), "Folder command: failed execution has no local fallback");
            }
            else
            {
                response.SetCanceled(new CancellationToken(true));
                Throws<OperationCanceledException>(() => operation.GetAwaiter().GetResult(), "Folder command: canceled execution is propagated");
            }
            Check(!File.Exists(transferPath), "Folder command: transfer removed after " + outcome);
        }
    }

    private static void FolderRetryTests(string root)
    {
        var fail = true;
        var source = new NoteSource(new Settings { ObsidianVaultFolder = root, ObsidianVaultId = "Main", ObsidianNotesFolder = "." }, true,
            (connection, code, _) =>
            {
                ObsidianTasksClient.ValidateCommand(connection.ObsidianVaultId, code);
                if (fail) throw new IOException("Disconnected");
                using var request = DecodeCliRequest(code);
                return Task.FromResult(request.RootElement.GetProperty("mode").GetString() == "folders"
                    ? "STICKY_TASKS_PREVIEW:{\"folders\":[\".\"],\"defaultFolder\":\".\"}"
                    : "STICKY_TASKS_PREVIEW:{\"notes\":[]}");
            });
        var browser = new NoteBrowserWindow(source, _ => { });
        try
        {
            browser.InitializeAsync().GetAwaiter().GetResult();
            Check(browser.Folder.SelectedItem is null && browser.Status.Text == "Disconnected", "Folder manager: initial connection failure remains visible");
            fail = false;
            LogicalDescendants(browser).OfType<Button>().Single(b => b.Name == "RefreshNotes")
                .RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
            WaitFor(() => browser.Folder.SelectedItem is string, "Folder manager: Refresh retries catalog after initialization failure");
            Check(browser.Status.Text != "Disconnected" && browser.Notes.Items.Count == 0, "Folder manager: successful retry shows an empty folder without reopening the window");
        }
        finally { browser.Close(); }
    }
}
