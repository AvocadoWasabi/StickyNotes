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
            return $"StickyNotes.{identity.User!.Value}.{process.SessionId}";
        }
    }
    public static string DataDirectory { get; private set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "StickyNotes");
    private static string ConfigPath => Path.Combine(DataDirectory, "settings.json");
    private static string TokenPath => Path.Combine(DataDirectory, "google-token.bin");

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        var command = AppCommands.Parse(e.Args);
        if (e.Args.Length == 2 && e.Args[0] == "--data-dir")
        {
            TestMode = true;
            DataDirectory = Path.GetFullPath(e.Args[1]);
            Config.NotesFolder = Path.Combine(DataDirectory, "notes");
        }
        else if (e.Args.Length > 0 && command is null)
        {
            MessageBox.Show("起動引数が正しくありません。"); Shutdown(); return;
        }
        mutex = new Mutex(true, "Local\\StickyNotes.Desktop", out ownsMutex);
        if (!ownsMutex)
        {
            if (command is not null)
            {
                try { AppCommandPipe.Send(CommandPipeName, command).GetAwaiter().GetResult(); }
                catch (Exception ex) { MessageBox.Show("起動中のアプリに操作を渡せませんでした。通知領域から操作するか、アプリを再起動してください。\n" + ex.Message); }
            }
            else MessageBox.Show("付箋アプリは起動済みです。通知領域またはタスクバーのメニューから操作できます。");
            Shutdown(); return;
        }
        if (command?.Id == "exit") { Shutdown(); return; }
        Directory.CreateDirectory(DataDirectory);
        if (File.Exists(ConfigPath))
        {
            try { Config = JsonSerializer.Deserialize<Settings>(File.ReadAllText(ConfigPath)) ?? new(); }
            catch (Exception ex) { MessageBox.Show("設定を読み込めません。元ファイルを保護するため終了します。\n" + ex.Message); Shutdown(); return; }
        }
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
        tray = new Forms.NotifyIcon { Icon = trayIcon, Text = "Markdown Sticky Notes", ContextMenuStrip = menu, Visible = true };
        tray.DoubleClick += (_, _) => { if (Notes.Count == 0) Safe(NewNote); else { Notes[0].Show(); Notes[0].Activate(); } };
        foreach (var placement in Config.Windows.ToArray()) Safe(() => ShowNote(placement));
        if (Notes.Count == 0 && command?.Id != "new") Safe(NewNote);
        if (command is not null) Dispatcher.BeginInvoke(new Action(() => ExecuteCommand(command)));
        SessionEnding += (_, args) => { if (!PrepareExit()) args.Cancel = true; };
    }

    public void Safe(Action action)
    {
        try { action(); }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Sticky Notes", MessageBoxButton.OK, MessageBoxImage.Warning); }
    }

    public void NewNote() => ShowNote(new NotePlacement { Path = NoteStore.Create(Config.NotesFolder), Left = 100 + Notes.Count * 24, Top = 100 + Notes.Count * 24 });

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
        var previous = Config;
        void Commit(IReadOnlyDictionary<string, string> paths)
        {
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
        if (migrate)
            NoteFolderMigration.Move(previous.NotesFolder, next.NotesFolder, Commit);
        else Commit(new Dictionary<string, string>());
        foreach (var note in Notes)
        {
            note.ApplyButtonDisplay();
            note.ApplyTaskbarDisplay();
        }
    }

    private bool PrepareExit()
    {
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
