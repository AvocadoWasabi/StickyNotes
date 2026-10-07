using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using StickyNotes;
using StickyNotes.Core;

internal static partial class Program
{
    private static string LongNoteText() => RenderingCorpus.Replace("\n", "\r\n") +
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(25000)) + "\r\n日本語 中文 😀  \r\n\r\n";

    private static void NoteCommandTests(string root)
    {
        var settings = new Settings { ObsidianVaultFolder = root, ObsidianVaultId = "テストVault" };
        var text = LongNoteText();
        string path;
        using (var command = ObsidianTasksClient.PrepareNoteCommand(settings, new { mode = "save", text }))
        {
            path = command.TemporaryPath!;
            Check(path is not null && File.Exists(path), "CLI transfer: long note uses a temporary request");
            ObsidianTasksClient.ValidateCommand(settings.ObsidianVaultId, command.Code);
            Check(!command.Code.Contains(text) && command.Code.Length < 2000, "CLI transfer: long note stays out of bounded command arguments");
            using var input = new FileStream(path!, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            using var inflated = new ZLibStream(input, CompressionMode.Decompress);
            using var data = JsonDocument.Parse(inflated);
            Check(data.RootElement.GetProperty("request").GetProperty("text").GetString() == text,
                "CLI transfer: Unicode, CRLF, quotes and trailing blanks round trip");
            using var identity = WindowsIdentity.GetCurrent();
            var acl = new FileInfo(path!).GetAccessControl();
            var rules = acl.GetAccessRules(true, true, typeof(SecurityIdentifier)).Cast<FileSystemAccessRule>().ToArray();
            Check(acl.AreAccessRulesProtected && rules.Length == 1 && rules[0].IdentityReference.Equals(identity.User),
                "CLI transfer: access restricted to current user without inherited grants");
            Throws<IOException>(() => { using var write = File.OpenWrite(path!); }, "CLI transfer: concurrent modification blocked");
        }
        Check(!File.Exists(path), "CLI transfer: dispose deletes note data");

        var note = Path.Combine(root, "transfer-test.md");
        var original = "original";
        using var snapshotData = ObsidianTasksClient.ParseEnvelope(CliSnapshotEnvelope(original));
        var snapshot = NoteSource.DecodeSnapshot(note, snapshotData.RootElement);
        string? transferPath = null;
        var source = new NoteSource(settings, true, (_, code, _) =>
        {
            transferPath = Encoding.UTF8.GetString(Convert.FromBase64String(code.Split("Buffer.from('")[1].Split("'")[0]));
            Check(File.Exists(transferPath), "CLI transfer: request remains available during execution");
            throw new IOException("Synthetic CLI failure");
        });
        Throws<IOException>(() => source.Save(snapshot, text, Path.Combine(root, "backup")), "CLI transfer: execution failure is surfaced");
        Check(transferPath is not null && !File.Exists(transferPath) && !File.Exists(note),
            "CLI transfer: failure deletes transfer and never writes note through a local fallback");
        source = new NoteSource(settings, true, (_, code, _) =>
        {
            transferPath = Encoding.UTF8.GetString(Convert.FromBase64String(code.Split("Buffer.from('")[1].Split("'")[0]));
            return Task.FromCanceled<string>(new System.Threading.CancellationToken(true));
        });
        Throws<OperationCanceledException>(() => source.Save(snapshot, text, Path.Combine(root, "backup")), "CLI transfer: cancellation is surfaced");
        Check(!File.Exists(transferPath), "CLI transfer: cancellation deletes temporary note data");
    }

    // Optional Node-backed smoke executes the exact generated JavaScript and shipped bridge.
    // It uses only synthetic files under its own temporary directory, with mocked Vault APIs.
    private static async Task NoteTransportSmoke()
    {
        var root = Path.Combine(Path.GetTempPath(), "StickyNotes-TransportTest-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var settings = new Settings { ObsidianVaultFolder = root, ObsidianVaultId = "日本語テスト" };
            var transfers = new List<string>();
            async Task<string> Execute(Settings config, string code, System.Threading.CancellationToken token)
            {
                ObsidianTasksClient.ValidateCommand(config.ObsidianVaultId, code);
                if (code.Contains("readFileSync(Buffer.from('"))
                    transfers.Add(Encoding.UTF8.GetString(Convert.FromBase64String(code.Split("Buffer.from('")[1].Split("'")[0])));
                var start = new ProcessStartInfo("node") { UseShellExecute = false, CreateNoWindow = true,
                    RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true,
                    StandardInputEncoding = new UTF8Encoding(false), StandardOutputEncoding = Encoding.UTF8, StandardErrorEncoding = Encoding.UTF8 };
                start.ArgumentList.Add(Path.Combine(AppContext.BaseDirectory, "NoteTransportHarness.js"));
                using var child = Process.Start(start)!;
                using var timeout = System.Threading.CancellationTokenSource.CreateLinkedTokenSource(token);
                timeout.CancelAfter(TimeSpan.FromSeconds(20));
                try
                {
                    var output = child.StandardOutput.ReadToEndAsync(timeout.Token);
                    var error = child.StandardError.ReadToEndAsync(timeout.Token);
                    await child.StandardInput.WriteAsync(JsonSerializer.Serialize(new { root, code }));
                    child.StandardInput.Close();
                    await child.WaitForExitAsync(timeout.Token);
                    var diagnostic = await error;
                    if (child.ExitCode != 0) throw new Exception(diagnostic);
                    return await output;
                }
                finally { if (!child.HasExited) child.Kill(); }
            }
            var source = new NoteSource(settings, true, Execute);
            var path = Path.Combine(root, "note.md");
            var backup = Path.Combine(root, "backup");
            File.WriteAllText(path, "original\r\n", new UTF8Encoding(true));
            var before = File.ReadAllBytes(path);
            var snapshot = await source.ReadAsync(path);
            var text = LongNoteText() + "');throw Error('note text executed');//";
            var saved = source.Save(snapshot, text, backup);
            Check(saved.Text == text && (await source.ReadAsync(path)).Text == text, "CLI bridge: long edited note round trips via generated file loader");
            Check(File.ReadAllBytes(Directory.GetFiles(backup).Single()).SequenceEqual(before), "CLI bridge: backup retains original BOM and CRLF");
            Check(File.ReadAllBytes(path).Take(3).SequenceEqual(new byte[] { 239, 187, 191 }), "CLI bridge: long save preserves BOM");
            Throws<ConflictException>(() => source.Save(snapshot, text + "stale", backup), "CLI bridge: long stale save rejected");
            Check(Directory.GetFiles(backup).Length == 1 && (await source.ReadAsync(path)).Text == text, "CLI bridge: conflict leaves current note and backups unchanged");
            var deniedBackup = Path.Combine(root, "not-a-directory");
            File.WriteAllText(deniedBackup, "fixture");
            Throws<InvalidOperationException>(() => source.Save(saved, text + "changed", deniedBackup), "CLI bridge: backup failure blocks long save");
            Check((await source.ReadAsync(path)).Text == text, "CLI bridge: backup failure preserves note");
            using (var command = ObsidianTasksClient.PrepareNoteCommand(settings,
                new { mode = "save", root, path = "note.md", text = text + "changed", hash = saved.Hash, backup }))
            {
                var invalidHash = System.Text.RegularExpressions.Regex.Replace(command.Code,
                    "(?<=digest\\('hex'\\)!==')[a-f0-9]{64}", new string('0', 64));
                Check(invalidHash != command.Code, "CLI bridge: integrity test changes the expected digest");
                var output = await Execute(settings, invalidHash, default);
                Throws<InvalidOperationException>(() => { using var result = ObsidianTasksClient.ParseEnvelope(output); },
                    "CLI bridge: integrity mismatch rejected before adapter execution");
                Check(output.Contains("Invalid note transfer") && (await source.ReadAsync(path)).Text == text && Directory.GetFiles(backup).Length == 1,
                    "CLI bridge: failed integrity check preserves note and backups");
            }
            var maximum = Convert.ToBase64String(RandomNumberGenerator.GetBytes(1_499_997)) + "x"; // 1,999,997 bytes + BOM.
            saved = source.Save(saved, maximum, backup);
            Check(new FileInfo(path).Length == 2_000_000 && (await source.ReadAsync(path)).Text == maximum,
                "CLI bridge: full 2 MB note can be saved and reloaded");
            Throws<InvalidOperationException>(() => source.Save(saved, maximum + "x", backup), "CLI bridge: save exceeding 2 MB rejected before backup or write");
            Check(Directory.GetFiles(backup).Length == 2 && (await source.ReadAsync(path)).Text == maximum,
                "CLI bridge: rejected oversized save preserves note and backups");
            Check(transfers.Count >= 5 && transfers.All(p => !File.Exists(p)), "CLI bridge: all successful and failed transfers removed");
        }
        finally { Directory.Delete(root, true); }
    }
}
