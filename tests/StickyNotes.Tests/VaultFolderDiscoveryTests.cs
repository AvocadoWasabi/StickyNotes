using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using StickyNotes;
using StickyNotes.Core;

internal static partial class Program
{
    private static void VaultFolderDiscoveryTests(string root)
    {
        var app = App.Current;
        var previous = app.Config; var factory = app.NoteSources;
        SettingsWindow? window = null;
        var calls = new List<Settings>();
        var response = new TaskCompletionSource<string>();
        var discoveredRoot = Path.Combine(root, "discovered-vault");
        var envelope = "=> STICKY_TASKS_PREVIEW:" + JsonSerializer.Serialize(new { root = discoveredRoot, name = "Named Vault" });
        try
        {
            app.ApplySettings(new Settings { ObsidianVaultFolder = root }, false);
            var saved = File.ReadAllBytes(Path.Combine(App.DataDirectory, "settings.json"));
            app.NoteSources = settings => new NoteSource(settings, NoteSource.UsesCli(settings), (connection, code, _) =>
            {
                if (!code.Contains("root:app.vault")) return Task.FromResult(DailyEnvelope(DateTime.Today));
                calls.Add(connection);
                return response.Task;
            });
            window = new SettingsWindow();
            window.Categories.SelectedIndex = 1;
            var controls = LogicalDescendants(window).ToArray();
            var target = controls.OfType<TextBox>().Single(t => t.Name == "VaultCliTarget");
            var folder = controls.OfType<TextBox>().Single(t => t.Name == "VaultRoot");
            var detect = controls.OfType<Button>().Single(b => b.Name == "DetectObsidianVault");
            var status = controls.OfType<TextBlock>().Single(t => t.Name == "VaultDiscoveryStatus");
            var preview = controls.OfType<TextBlock>().Single(t => t.Name == "DailyPreviewStatus");
            var save = window.Footer.Children.OfType<Button>().Last();
            void Fetch() => detect.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));

            target.Text = "  Named Vault  ";
            WaitFor(() => preview.Text.Contains("Templates/Daily"), "Vault folder: typing still previews daily settings");
            Check(calls.Count == 0 && folder.Text == root, "Vault folder: typing alone does not discover or overwrite the root");
            Fetch();
            Check(calls.Count == 1 && calls[0].ObsidianVaultId == "Named Vault" && calls[0].ObsidianVaultFolder.Length == 0,
                "Vault folder: CLI discovery targets entered name independently of the old root");
            Check(!detect.IsEnabled && !save.IsEnabled && status.Text == L10n.Text("Vault.DiscoveryPending"),
                "Vault folder: pending lookup is visible and cannot be saved or submitted twice");
            response.SetResult(envelope);
            WaitFor(() => detect.IsEnabled, "Vault folder: named lookup completes");
            Check(folder.Text == discoveredRoot && target.Text == "  Named Vault  " && status.Text.Contains(discoveredRoot),
                "Vault folder: result fills the root and shows completion without changing explicit target");
            WaitFor(() => preview.Text.Contains("Templates/Daily"), "Vault folder: daily settings refresh automatically after lookup");
            Check(save.IsEnabled && status.Text.Contains(discoveredRoot) && File.ReadAllBytes(Path.Combine(App.DataDirectory, "settings.json")).SequenceEqual(saved),
                "Vault folder: completion survives daily preview and remains an unsaved draft");
            var layout = (FrameworkElement)window.Content;
            layout.Measure(new Size(1100, 760)); layout.Arrange(new Rect(0, 0, 1100, 760)); layout.UpdateLayout();
            RenderLocalizationPreview(layout, "vault-folder-discovery");

            response = new TaskCompletionSource<string>();
            target.Text = "0123456789abcdef"; Fetch();
            response.SetResult(envelope);
            WaitFor(() => detect.IsEnabled, "Vault folder: ID lookup completes");
            Check(calls.Last().ObsidianVaultId == target.Text && target.Text == "0123456789abcdef", "Vault folder: explicit ID survives discovery of the display name");

            response = new TaskCompletionSource<string>();
            target.Text = "Missing Vault"; Fetch();
            response.SetException(new IOException("Vault unavailable"));
            WaitFor(() => detect.IsEnabled, "Vault folder: failed lookup completes");
            Check(status.Text.Contains("Vault unavailable") && folder.Text == discoveredRoot && target.Text == "Missing Vault" && save.IsEnabled,
                "Vault folder: failure keeps inputs and exposes the reason");

            response = new TaskCompletionSource<string>();
            Fetch(); target.Text = "Changed"; target.Text = "Missing Vault";
            response.SetResult(envelope);
            WaitFor(() => detect.IsEnabled, "Vault folder: stale lookup completes");
            Check(status.Text.Length == 0 && target.Text == "Missing Vault", "Vault folder: edits invalidate a lookup even if the input is restored");

            response = new TaskCompletionSource<string>();
            Fetch();
            controls.OfType<Button>().Single(b => b.Name == "AddVault").RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
            response.SetResult(envelope);
            WaitFor(() => detect.IsEnabled, "Vault folder: profile switch discards late lookup");
            Check(folder.Text.Length == 0 && target.Text.Length == 0 && status.Text.Length == 0,
                "Vault folder: a late result never populates another profile");

            response = new TaskCompletionSource<string>();
            Fetch();
            Check(calls.Last().ObsidianVaultId.Length == 0 && calls.Last().ObsidianVaultFolder.Length == 0,
                "Vault folder: empty name requests Obsidian's default Vault");
            window.Close(); response.SetResult(envelope);
            var drained = false;
            window.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.ApplicationIdle, new Action(() => drained = true));
            WaitFor(() => drained, "Vault folder: closed lookup continuation drains");
            Check(folder.Text.Length == 0 && File.ReadAllBytes(Path.Combine(App.DataDirectory, "settings.json")).SequenceEqual(saved),
                "Vault folder: closing settings rejects late results and never persists folders or new profiles");
        }
        finally { window?.Close(); app.NoteSources = factory; app.ApplySettings(previous, false); }
    }
}
