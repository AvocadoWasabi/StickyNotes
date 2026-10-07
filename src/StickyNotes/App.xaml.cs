using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Security.Principal;
using System.Windows.Shell;
using Forms = System.Windows.Forms;

namespace StickyNotes;

public partial class App : Application
{
    public static new App Current => (App)Application.Current;
    public Settings Config { get; private set; } = new();
    public CalendarService Calendar { get; private set; } = null!;
    public List<NoteWindow> Notes { get; } = [];
    public bool Exiting { get; private set; }
    public bool TestMode { get; private set; }
    internal bool IsChangingFolder { get; private set; }
    internal Func<Settings, NoteSource> NoteSources { get; set; } = NoteSource.Create;
    internal Func<Settings, string, string[], CancellationToken, Task<TasksResponse>> TasksQueries { get; set; } = ObsidianTasksClient.QueryAsync;
    internal Func<Settings, string, string[], CancellationToken, Task<TasksResponse>> DataviewQueries { get; set; } = ObsidianTasksClient.QueryDataviewAsync;
    private Forms.NotifyIcon? tray;
    private System.Drawing.Icon? trayIcon;
    private Mutex? mutex;
    private bool ownsMutex;
    private AppCommandPipe? commandPipe;
    private static string CommandPipeName
    {
        get
        {
            using var identity = WindowsIdentity.GetCurrent();
            using var process = Process.GetCurrentProcess();
            return $"{BuildFlavor.Profile}.{identity.User!.Value}.{process.SessionId}";
        }
    }
    public static string DataDirectory { get; private set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), BuildFlavor.Profile);
    private static string ConfigPath => Path.Combine(DataDirectory, "settings.json");
    private static string TokenPath => Path.Combine(DataDirectory, "google-token.bin");

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        L10n.Initialize(null);
        var command = AppCommands.Parse(e.Args);
        if (BuildFlavor.TasksPreview)
        {
            TestMode = true;
            Config.NotesFolder = Path.Combine(DataDirectory, "notes");
        }
        if (e.Args.Length == 2 && e.Args[0] == "--data-dir")
        {
            TestMode = true;
            DataDirectory = Path.GetFullPath(e.Args[1]);
            Config.NotesFolder = Path.Combine(DataDirectory, "notes");
        }
        else if (e.Args.Length > 0 && command is null)
        {
            MessageBox.Show(L10n.Text("App.xaml.Text01")); Shutdown(); return;
        }
        mutex = new Mutex(true, "Local\\" + BuildFlavor.Profile + ".Desktop", out ownsMutex);
        if (!ownsMutex)
        {
            if (command is not null)
            {
                try { AppCommandPipe.Send(CommandPipeName, command).GetAwaiter().GetResult(); }
                catch (Exception ex) { MessageBox.Show(L10n.Text("App.xaml.Text02") + ex.Message); }
            }
            else MessageBox.Show(L10n.Text("App.xaml.Text03"));
            Shutdown(); return;
        }
        if (command?.Id == "exit") { Shutdown(); return; }
        Directory.CreateDirectory(DataDirectory);
        if (File.Exists(ConfigPath))
        {
            try { Config = JsonSerializer.Deserialize<Settings>(File.ReadAllText(ConfigPath)) ?? new(); }
            catch (Exception ex) { MessageBox.Show(L10n.Text("App.xaml.Text04") + ex.Message); Shutdown(); return; }
        }
        L10n.Initialize(Config.Language);
        Calendar = new CalendarService(
            () => File.Exists(TokenPath) ? Encoding.UTF8.GetString(ProtectedData.Unprotect(File.ReadAllBytes(TokenPath), null, DataProtectionScope.CurrentUser)) : null,
            value => File.WriteAllBytes(TokenPath, ProtectedData.Protect(Encoding.UTF8.GetBytes(value), null, DataProtectionScope.CurrentUser)));
        var menu = AppCommands.CreateTrayMenu(ExecuteCommand);
        Safe(() => commandPipe = new AppCommandPipe(CommandPipeName, received =>
            Dispatcher.BeginInvoke(new Action(() => { if (!Exiting) ExecuteCommand(received); }))));
        if (!TestMode)
            Safe(() => JumpList.SetJumpList(this, AppCommands.CreateJumpList(Environment.ProcessPath!)));
        using (var iconStream = GetResourceStream(new Uri("pack://application:,,,/StickyNotes;component/Assets/StickyNotes.ico")).Stream)
            trayIcon = new System.Drawing.Icon(iconStream, System.Windows.Forms.SystemInformation.SmallIconSize);
        tray = new Forms.NotifyIcon { Icon = trayIcon, Text = BuildFlavor.TasksPreview ? "Sticky Notes — Tasks CLI Preview" : "Markdown Sticky Notes", ContextMenuStrip = menu, Visible = true };
        tray.DoubleClick += (_, _) => { if (Notes.Count == 0) Safe(NewNote); else { Notes[0].Show(); Notes[0].Activate(); } };
        var needsCliSetup = NoteSource.UsesCli(Config) && string.IsNullOrWhiteSpace(Config.ObsidianVaultFolder);
        foreach (var placement in Config.Windows.ToArray()) Safe(() => ShowNote(placement));
        if (needsCliSetup) Dispatcher.BeginInvoke(new Action(() => Safe(() => new SettingsWindow().ShowDialog())));
        else
        {
            if (Notes.Count == 0 && command?.Id != "new") Safe(NewNote);
            if (command is not null) Dispatcher.BeginInvoke(new Action(() => ExecuteCommand(command)));
        }
        SessionEnding += (_, args) => { if (!PrepareExit()) args.Cancel = true; };
    }

    public void Safe(Action action)
    {
        try { action(); }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Sticky Notes", MessageBoxButton.OK, MessageBoxImage.Warning); }
    }

    public void NewNote()
    {
        if (IsChangingFolder) throw new InvalidOperationException(L10n.Text("StickyFolder.Working"));
        ShowNote(new NotePlacement { Path = NoteSources(Config).CreateNote(Config.NotesFolder), Left = 100 + Notes.Count * 24, Top = 100 + Notes.Count * 24 });
    }

    public void OpenNote()
    {
        var picker = new Microsoft.Win32.OpenFileDialog { Filter = "Markdown|*.md", InitialDirectory = Config.NotesFolder };
        if (picker.ShowDialog() == true) ShowNote(new() { Path = picker.FileName });
    }

    public void LinkSection()
    {
        var dialog = new NoteLinkWindow(false);
        if (dialog.ShowDialog() == true && dialog.Result is { } placement) ShowNote(placement);
    }

    public void LinkDaily()
    {
        var dialog = new NoteLinkWindow(true);
        if (dialog.ShowDialog() == true && dialog.Result is { } placement) ShowNote(placement);
    }

    public void ShowNote(NotePlacement placement)
    {
        var existing = Notes.FirstOrDefault(n => n.Placement.Daily == placement.Daily &&
            n.Placement.Heading == placement.Heading && string.Equals(n.Placement.Path, placement.Path, StringComparison.OrdinalIgnoreCase));
        if (existing is not null) { existing.Show(); existing.Activate(); return; }
        var window = new NoteWindow(placement);
        Notes.Add(window);
        window.Show();
        SaveConfig();
    }

    public void SaveConfig()
    {
        Config.Windows = Notes.Select(x => x.Placement).ToList();
        var temporary = ConfigPath + ".tmp";
        File.WriteAllText(temporary, JsonSerializer.Serialize(Config, new JsonSerializerOptions { WriteIndented = true }));
        File.Move(temporary, ConfigPath, true);
    }

    public void SaveGoogleCredentialsPath(string path)
    {
        var previous = Config.GoogleCredentialsFile;
        try { Config.GoogleCredentialsFile = path; SaveConfig(); }
        catch { Config.GoogleCredentialsFile = previous; throw; }
    }

    internal void ExecuteCommand(AppCommand command) => Safe(() =>
    {
        switch (command.Id)
        {
            case "new": NewNote(); break;
            case "open": OpenNote(); break;
            case "link-section": LinkSection(); break;
            case "link-daily": LinkDaily(); break;
            case "show-all":
                foreach (var note in Notes.ToArray())
                {
                    note.Show();
                    if (note.WindowState == WindowState.Minimized) note.WindowState = WindowState.Normal;
                    note.Activate();
                }
                break;
            case "temporary-front": BringNotesToFrontTemporarily(); break;
            case "settings": new SettingsWindow().ShowDialog(); break;
            case "exit": Quit(); break;
        }
    });

    public void BringNotesToFrontTemporarily()
    {
        foreach (var note in Notes.ToArray()) note.BringToFrontTemporarily();
    }

    public void ApplySettings(Settings next, bool migrate)
    {
        if (IsChangingFolder) throw new InvalidOperationException(L10n.Text("StickyFolder.Working"));
        if (migrate) NoteFolderMigration.Move(Config.NotesFolder, next.NotesFolder, paths => CommitSettings(next, paths));
        else CommitSettings(next, new Dictionary<string, string>());
        RefreshSettingsWindows();
    }

    internal async Task ApplyFolderSettingsAsync(Settings next, bool migrate)
    {
        if (IsChangingFolder) throw new InvalidOperationException(L10n.Text("StickyFolder.Working"));
        IsChangingFolder = true;
        foreach (var note in Notes) note.PauseFolderChange();
        try { await StickyFolderChange.ApplyAsync(Config, next, migrate, NoteSources,
            paths => CommitSettings(next, paths), Path.Combine(DataDirectory, "folder-migrations")); }
        finally { IsChangingFolder = false; }
        RefreshSettingsWindows();
    }

    private void CommitSettings(Settings next, IReadOnlyDictionary<string, string> paths)
    {
        var previous = Config;
        var restore = new List<Action>();
        try
        {
            if (paths.Count > 0)
                foreach (var note in Notes) restore.Add(note.Relocate(paths));
            Config = next;
            SaveConfig();
        }
        catch
        {
            Config = previous;
            foreach (var undo in restore) undo();
            throw;
        }
    }

    private void RefreshSettingsWindows()
    {
        foreach (var note in Notes)
        {
            note.ApplyButtonDisplay();
            note.ApplyTaskbarDisplay();
        }
    }

    private bool PrepareExit()
    {
        if (IsChangingFolder) return false;
        foreach (var note in Notes.ToArray()) if (!note.CanClose()) return false;
        SaveConfig();
        Exiting = true;
        return true;
    }

    public void Quit() { if (PrepareExit()) Shutdown(); }
    protected override void OnExit(ExitEventArgs e)
    {
        commandPipe?.Dispose();
        tray?.Dispose();
        trayIcon?.Dispose();
        Calendar?.Dispose();
        if (ownsMutex) mutex?.ReleaseMutex();
        mutex?.Dispose();
        base.OnExit(e);
    }
}
