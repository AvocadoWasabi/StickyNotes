using System.Threading;
using System.Windows.Threading;
using System.Windows.Input;

namespace StickyNotes;

public sealed class SettingsWindow : Window
{
    internal StackPanel GeneralPanel { get; private set; } = null!;
    internal StackPanel VaultDetailsPanel { get; private set; } = null!;
    internal StackPanel GooglePanel { get; private set; } = null!;
    internal StackPanel Footer { get; private set; } = null!;
    internal TabControl Categories { get; private set; } = null!;
    public SettingsWindow() : this(null) { }

    internal SettingsWindow(Func<bool>? confirmRegexTemplate, Func<bool>? confirmCredentialDeletion = null)
    {
        SetResourceReference(IconProperty, "AppIcon");
        Title = L10n.Text("SettingsWindow.Text01");
        Width = Math.Min(1120, SystemParameters.WorkArea.Width);
        Height = Math.Min(820, SystemParameters.WorkArea.Height);
        MinWidth = Math.Min(800, SystemParameters.WorkArea.Width);
        MinHeight = Math.Min(400, SystemParameters.WorkArea.Height);
        MaxHeight = SystemParameters.WorkArea.Height;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        var app = App.Current;
        var profiles = app.Config.GetVaultProfiles();
        var selectedProfile = profiles[0];
        var pendingMigration = false;
        var folderSelectionPending = false;
        var savingSettings = false;
        Closing += (_, e) => { if (savingSettings) e.Cancel = true; };
        var loadingProfile = false;
        var defaultId = app.Config.DefaultDailyVaultId;
        var profileName = new TextBox { Name = "VaultDisplayName", Text = selectedProfile.Name };
        var vaultList = new ListBox { Name = "VaultList", DisplayMemberPath = "Name", MinHeight = 120, MaxHeight = 220 };
        var defaultVault = new ComboBox { Name = "DefaultDailyVault", DisplayMemberPath = "Name", SelectedValuePath = "Id" };
        var panel = new StackPanel { Margin = new Thickness(24) };
        var googlePanel = new StackPanel { Margin = new Thickness(24) };
        panel.Children.Add(new TextBlock { Text = "Markdown Sticky Notes", FontSize = 24, FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 16) });
        var languagePanel = new StackPanel { Margin = new Thickness(0, 0, 0, 16) };
        languagePanel.Children.Add(new TextBlock { Text = L10n.Text("Language.Label") });
        var languageCodes = new[] { "", "ja", "en", "zh-CN" };
        var language = new ComboBox { Name = "DisplayLanguage", ItemsSource = new[] {
            L10n.Text("Language.Automatic"), "日本語", "English", "简体中文" },
            SelectedIndex = Math.Max(0, Array.IndexOf(languageCodes, app.Config.Language)) };
        languagePanel.Children.Add(language);
        languagePanel.Children.Add(new TextBlock { Text = L10n.Text("Language.Restart"), TextWrapping = TextWrapping.Wrap, Foreground = Brushes.DimGray });
        panel.Children.Add(languagePanel);
        panel.Children.Add(new TextBlock { Text = L10n.Text("Editing.Activation"), TextWrapping = TextWrapping.Wrap });
        var editGesture = new ComboBox { Name = "EditGesture", ItemsSource = new[] {
            L10n.Text("Editing.SingleClick"), L10n.Text("Editing.DoubleClick") },
            SelectedIndex = app.Config.DoubleClickToEdit ? 1 : 0, Margin = new Thickness(0, 0, 0, 8) };
        panel.Children.Add(editGesture);
        panel.Children.Add(new TextBlock { Text = L10n.Text("TaskEditing.Label") });
        var taskCompletion = new ComboBox { Name = "TaskCompletion", ItemsSource = new[] {
            L10n.Text("TaskEditing.Automatic"), L10n.Text("TaskEditing.On"), L10n.Text("TaskEditing.Off") },
            SelectedIndex = Enum.IsDefined(app.Config.TaskCompletion) ? (int)app.Config.TaskCompletion : 0,
            Margin = new Thickness(0, 0, 0, 8) };
        panel.Children.Add(taskCompletion);
        panel.Children.Add(new TextBlock { Text = L10n.Text("TaskEditing.PolicyHelp"), TextWrapping = TextWrapping.Wrap, Foreground = Brushes.DimGray });
        panel.Children.Add(new TextBlock { Text = L10n.Text("TaskEditing.ToggleShortcut") });
        var taskToggleShortcut = new TextBox { Name = "TaskToggleShortcut", IsReadOnly = true,
            Text = TaskToggleShortcut.Parse(app.Config.TaskToggleShortcut) is { } shortcut
                ? TaskToggleShortcut.Format(shortcut.Key, shortcut.Modifiers) : "" };
        var shortcutHelp = new TextBlock { Text = L10n.Text("TaskEditing.ShortcutHelp"), TextWrapping = TextWrapping.Wrap, Foreground = Brushes.DimGray };
        taskToggleShortcut.PreviewKeyDown += (_, e) =>
        {
            var key = e.Key == Key.System ? e.SystemKey : e.Key;
            var modifiers = Keyboard.Modifiers;
            if (key == Key.Tab) return;
            e.Handled = true;
            if (key is Key.LeftCtrl or Key.RightCtrl or Key.LeftAlt or Key.RightAlt or Key.LeftShift or Key.RightShift or Key.ImeProcessed) return;
            if (modifiers == ModifierKeys.None && key is Key.Delete or Key.Back)
                taskToggleShortcut.Text = "";
            else if (TaskToggleShortcut.IsAllowed(key, modifiers))
                taskToggleShortcut.Text = TaskToggleShortcut.Format(key, modifiers);
            else { shortcutHelp.Text = L10n.Text("TaskEditing.ShortcutInvalid"); return; }
            shortcutHelp.Text = L10n.Text("TaskEditing.ShortcutHelp");
        };
        var shortcutPanel = new StackPanel();
        shortcutPanel.Children.Add(taskToggleShortcut);
        panel.Children.Add(shortcutPanel);
        panel.Children.Add(Ui.Button(L10n.Text("TaskEditing.ShortcutClear"), () =>
        { taskToggleShortcut.Text = ""; shortcutHelp.Text = L10n.Text("TaskEditing.ShortcutHelp"); }));
        panel.Children.Add(shortcutHelp);
        var autoSave = new CheckBox { Name = "AutoSaveOnFocusLoss", Content = L10n.Text("SettingsWindow.Text02"), IsChecked = app.Config.AutoSaveOnFocusLoss, Margin = new Thickness(0, 0, 0, 8) };
        panel.Children.Add(autoSave);
        panel.Children.Add(new TextBlock { Text = L10n.Text("SettingsWindow.Text03"), TextWrapping = TextWrapping.Wrap, Foreground = Brushes.DimGray });
        var taskbar = new CheckBox { Name = "ShowInTaskbar", Content = L10n.Text("SettingsWindow.Text04"), IsChecked = app.Config.ShowInTaskbar, Margin = new Thickness(0, 8, 0, 8) };
        panel.Children.Add(taskbar);
        panel.Children.Add(new TextBlock { Text = L10n.Text("SettingsWindow.Text05"), TextWrapping = TextWrapping.Wrap, Foreground = Brushes.DimGray });
        var overlay = new CheckBox { Name = "TitleButtonOverlay", Content = L10n.Text("SettingsWindow.Text06"), IsChecked = app.Config.TitleButtonOverlay, Margin = new Thickness(0, 8, 0, 8) };
        panel.Children.Add(overlay);
        panel.Children.Add(new TextBlock { Text = L10n.Text("SettingsWindow.Text07"), TextWrapping = TextWrapping.Wrap, Foreground = Brushes.DimGray });
        TextBox Add(string label, string value, StackPanel? target = null)
        {
            target ??= panel;
            target.Children.Add(new TextBlock { Text = label, TextWrapping = TextWrapping.Wrap });
            var input = new TextBox { Text = value }; target.Children.Add(input); return input;
        }
        var sourceStart = panel.Children.Count;
        var notes = Add(L10n.Text("StickyFolder.Root"), string.IsNullOrWhiteSpace(app.Config.NotesRoot) ? app.Config.NotesFolder : app.Config.NotesRoot);
        var notesLabel = panel.Children[panel.Children.IndexOf(notes) - 1];
        var tasksEnabled = new CheckBox { Name = "UseObsidianCli", Content = L10n.Text("TasksPreview.Enable"), IsChecked = app.Config.ObsidianTasksEnabled };
        var tasksPanel = new StackPanel();
        tasksPanel.Children.Add(tasksEnabled);
        tasksPanel.Children.Add(new TextBlock { Text = L10n.Text("TasksPreview.Help"), TextWrapping = TextWrapping.Wrap });
        var tasksCli = Add(L10n.Text("TasksPreview.Cli"), app.Config.ObsidianCli, tasksPanel);
        var tasksVault = Add(L10n.Text("TasksPreview.Vault"), app.Config.ObsidianVaultFolder, tasksPanel);
        tasksPanel.Children.Add(Ui.Button(L10n.Text("SettingsWindow.Text09"), () => PickFolder(tasksVault)));
        var tasksId = Add(L10n.Text("TasksPreview.VaultId"), app.Config.ObsidianVaultId, tasksPanel);
        var detectVault = new Button { Name = "DetectObsidianVault", Content = L10n.Text("CliDaily.Detect") };
        tasksPanel.Children.Add(detectVault);
        panel.Children.Insert(panel.Children.IndexOf(notes) - 1, new Expander { Header = "Obsidian CLI", IsExpanded = true, Content = tasksPanel });
        var notesBrowse = Ui.Button(L10n.Text("SettingsWindow.Text09"), () => PickFolder(notes));
        panel.Children.Add(notesBrowse);
        var chooseStickyFolder = new Button { Name = "ChooseStickyFolder", Content = L10n.Text("StickyFolder.Title") };
        var stickyFolderStatus = new TextBlock { Name = "StickyFolderStatus", TextWrapping = TextWrapping.Wrap };
        panel.Children.Add(chooseStickyFolder); panel.Children.Add(stickyFolderStatus);
        void ShowStickyFolder()
        {
            stickyFolderStatus.Text = tasksEnabled.IsChecked == true
                ? L10n.Format("StickyFolder.Current", selectedProfile.ObsidianNotesFolder ?? L10n.Text("CliDaily.Default"))
                : L10n.Format("StickyFolder.Current", selectedProfile.NotesFolder);
            chooseStickyFolder.IsEnabled = selectedProfile.Id == Settings.MainVaultId && Path.IsPathFullyQualified(tasksEnabled.IsChecked == true ? tasksVault.Text.Trim() : notes.Text.Trim());
        }
        notes.TextChanged += (_, _) => ShowStickyFolder();
        tasksVault.TextChanged += (_, _) => ShowStickyFolder();
        var manualDailyStart = panel.Children.Count;
        var daily = Add(L10n.Text("SettingsWindow.Text10"), app.Config.DailyFolder);
        panel.Children.Add(Ui.Button(L10n.Text("SettingsWindow.Text11"), () => PickFolder(daily)));
        var pattern = Add(L10n.Text("SettingsWindow.Text12"), app.Config.DailyPattern);
        bool FillEmptyRegex()
        {
            if (!string.IsNullOrWhiteSpace(pattern.Text)) return false;
            pattern.Text = DailyNoteResolver.RegexExample;
            pattern.SelectAll();
            return true;
        }
        panel.Children.Add(new TextBlock { TextWrapping = TextWrapping.Wrap, Foreground = Brushes.DimGray,
            Text = L10n.Text("SettingsWindow.Text13") + DailyNoteResolver.RegexExample });
        var manualDailyControls = panel.Children.Cast<UIElement>().Skip(manualDailyStart).ToArray();
        var previewStatus = new TextBlock { Name = "DailyPreviewStatus", TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 8, 0, 4) };
        var previewPanel = new StackPanel();
        previewPanel.Children.Add(previewStatus);
        var dailyHelp = new TextBlock { TextWrapping = TextWrapping.Wrap, Foreground = Brushes.DimGray };
        previewPanel.Children.Add(dailyHelp);
        panel.Children.Add(previewPanel);
        panel.Children.Add(new TextBlock { Text = L10n.Text("SettingsWindow.Text15"), TextWrapping = TextWrapping.Wrap });
        var retention = new ComboBox { Name = "DailyRetention", ItemsSource = new[] {
            L10n.Text("SettingsWindow.Text16"), L10n.Text("SettingsWindow.Text17"), L10n.Text("SettingsWindow.Text18") },
            SelectedIndex = Enum.IsDefined(app.Config.DailyRetention) ? (int)app.Config.DailyRetention : 0 };
        panel.Children.Add(retention);
        panel.Children.Add(new TextBlock { Text = L10n.Text("SettingsWindow.Text19"), TextWrapping = TextWrapping.Wrap, Foreground = Brushes.DimGray });
        var previewTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
        var previewGate = new SemaphoreSlim(1, 1);
        CancellationTokenSource? previewCancellation = null;
        var previewVersion = 0;
        var previewClosed = false;
        void SchedulePreview()
        {
            if (previewClosed) return;
            previewVersion++;
            previewCancellation?.Cancel();
            previewTimer.Stop();
            previewStatus.Foreground = Brushes.DimGray;
            previewStatus.Text = L10n.Text("SettingsWindow.Text20");
            previewTimer.Start();
        }
        previewTimer.Tick += async (_, _) =>
        {
            previewTimer.Stop();
            var version = previewVersion;
            var folder = daily.Text; var expression = pattern.Text;
            var today = DateTime.Today;
            using var cancellation = new CancellationTokenSource();
            previewCancellation = cancellation;
            try
            {
                await previewGate.WaitAsync(cancellation.Token);
                string description;
                try
                {
                    var selected = new Settings { ObsidianTasksEnabled = tasksEnabled.IsChecked == true, ObsidianCli = tasksCli.Text.Trim(),
                        ObsidianVaultFolder = tasksVault.Text.Trim(), ObsidianVaultId = tasksId.Text.Trim(), DailyFolder = folder, DailyPattern = expression };
                    var access = app.NoteSources(selected);
                    if (access.IsCli)
                    {
                        var native = await access.GetDailyNotesAsync(today, cancellation.Token);
                        var target = native.Targets.Single(t => t.Date == ObsidianDailyNotes.DateKey(today));
                        description = L10n.Format("CliDaily.Summary", native.Folder.Length == 0 ? L10n.Text("CliDaily.Default") : native.Folder,
                            native.Format, native.Template.Length == 0 ? L10n.Text("CliDaily.None") : native.Template,
                            Path.GetRelativePath(selected.ObsidianVaultFolder, target.Path), target.Exists ? L10n.Text("CliDaily.Exists") : L10n.Text("CliDaily.Missing"));
                    }
                    else
                    {
                        var path = await Task.Run(() => access.ResolveDailyAsync(today, cancellation.Token));
                        description = L10n.Text("SettingsWindow.Text21") + Path.GetRelativePath(folder, path);
                    }
                }
                finally { previewGate.Release(); }
                if (previewClosed || version != previewVersion) return;
                previewStatus.Foreground = Brushes.DarkGreen;
                previewStatus.Text = description;
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                if (previewClosed || version != previewVersion) return;
                previewStatus.Foreground = Brushes.DarkRed;
                previewStatus.Text = L10n.Text("SettingsWindow.Text22") + ex.Message;
            }
            finally
            {
                if (ReferenceEquals(previewCancellation, cancellation)) previewCancellation = null;
            }
        };
        daily.TextChanged += (_, _) => SchedulePreview();
        tasksCli.TextChanged += (_, _) => SchedulePreview();
        tasksVault.TextChanged += (_, _) => SchedulePreview();
        tasksId.TextChanged += (_, _) => SchedulePreview();
        pattern.TextChanged += (_, _) => { if (!FillEmptyRegex()) SchedulePreview(); };
        var insertTags = Ui.Button(L10n.Text("SettingsWindow.Text23"), () =>
        {
            var insert = confirmRegexTemplate?.Invoke() ?? MessageBox.Show(this,
                L10n.Text("SettingsWindow.Text24") + DailyNoteResolver.RegexExample +
                L10n.Text("SettingsWindow.Text25"),
                L10n.Text("SettingsWindow.Text26"), MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes;
            if (insert)
            {
                pattern.Text = DailyNoteResolver.RegexExample;
                pattern.Focus(); pattern.SelectAll();
            }
            FillEmptyRegex();
            SchedulePreview();
        });
        panel.Children.Insert(panel.Children.IndexOf(previewPanel), insertTags);
        void UpdateSourceMode()
        {
            var cli = tasksEnabled.IsChecked == true;
            var visibility = cli ? Visibility.Collapsed : Visibility.Visible;
            foreach (var control in tasksPanel.Children.Cast<UIElement>().Where(c => !ReferenceEquals(c, tasksEnabled)))
                control.Visibility = cli ? Visibility.Visible : Visibility.Collapsed;
            foreach (var control in manualDailyControls) control.Visibility = visibility;
            notes.Visibility = notesLabel.Visibility = notesBrowse.Visibility = insertTags.Visibility = visibility;
            dailyHelp.Text = L10n.Text(cli ? "CliDaily.Help" : "SettingsWindow.Text14");
            ShowStickyFolder();
            SchedulePreview();
        }
        tasksEnabled.Checked += (_, _) => UpdateSourceMode();
        tasksEnabled.Unchecked += (_, _) => UpdateSourceMode();
        UpdateSourceMode();
        detectVault.Click += async (_, _) =>
        {
            detectVault.IsEnabled = false;
            var profileId = selectedProfile.Id;
            var selectedCli = tasksCli.Text;
            var previousVault = tasksVault.Text; var previousId = tasksId.Text;
            bool CurrentDetection() => !previewClosed && profileId == selectedProfile.Id && tasksCli.Text == selectedCli &&
                tasksVault.Text == previousVault && tasksId.Text == previousId;
            try
            {
                var connection = await app.NoteSources(new Settings { ObsidianCli = selectedCli }).DiscoverVaultAsync();
                if (!CurrentDetection()) return;
                tasksVault.Text = connection.Root; tasksId.Text = connection.Name;
            }
            catch (Exception ex) { if (CurrentDetection()) { previewStatus.Foreground = Brushes.DarkRed; previewStatus.Text = ex.Message; } }
            finally { if (!previewClosed) detectVault.IsEnabled = true; }
        };
        Loaded += (_, _) => SchedulePreview();
        Closed += (_, _) => { previewClosed = true; previewVersion++; previewTimer.Stop(); previewCancellation?.Cancel(); };
        var sourceControls = panel.Children.Cast<UIElement>().Skip(sourceStart).ToArray();
        googlePanel.Children.Add(new TextBlock { Text = "Google Calendar / Tasks", FontSize = 24,
            FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 16) });
        googlePanel.Children.Add(new TextBlock { Text = L10n.Text("SettingsWindow.Text27"), Margin = new Thickness(0, 0, 0, 8) });
        googlePanel.Children.Add(new TextBlock { Text = L10n.Text("SettingsWindow.Text28"), TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 12) });
        var guide = new StackPanel();
        void Explain(string text) => guide.Children.Add(new TextBlock { Text = text, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 4, 0, 4) });
        void Link(string label, string url) => guide.Children.Add(Ui.Button(label,
            () => System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url) { UseShellExecute = true })));
        Explain(L10n.Text("SettingsWindow.Text29"));
        Link(L10n.Text("SettingsWindow.Text30"), "https://console.cloud.google.com/apis/library/calendar-json.googleapis.com");
        Link(L10n.Text("GoogleTasks.Enable"), "https://console.cloud.google.com/apis/library/tasks.googleapis.com");
        Explain(L10n.Text("SettingsWindow.Text31"));
        Link(L10n.Text("SettingsWindow.Text32"), "https://console.cloud.google.com/auth/branding");
        Explain(L10n.Text("SettingsWindow.Text33"));
        Link(L10n.Text("SettingsWindow.Text34"), "https://console.cloud.google.com/auth/audience");
        Explain(L10n.Text("SettingsWindow.Text35"));
        Link(L10n.Text("SettingsWindow.Text36"), "https://console.cloud.google.com/auth/scopes");
        Explain(L10n.Text("SettingsWindow.Text37"));
        var scope = new TextBox { Text = "https://www.googleapis.com/auth/calendar.events", IsReadOnly = true, TextWrapping = TextWrapping.Wrap };
        guide.Children.Add(scope);
        guide.Children.Add(new TextBox { Text = "https://www.googleapis.com/auth/tasks.readonly", IsReadOnly = true, TextWrapping = TextWrapping.Wrap });
        Explain(L10n.Text("GoogleTasks.Setup"));
        Explain(L10n.Text("SettingsWindow.Text38"));
        Link(L10n.Text("SettingsWindow.Text39"), "https://console.cloud.google.com/auth/clients");
        Explain(L10n.Text("SettingsWindow.Text40"));
        Explain(L10n.Text("SettingsWindow.Text41"));
        Explain(L10n.Text("SettingsWindow.Text42"));
        Link(L10n.Text("SettingsWindow.Text43"), "https://developers.google.com/workspace/guides/configure-oauth-consent");
        Link(L10n.Text("SettingsWindow.Text44"), "https://support.google.com/cloud/answer/15549257");
        googlePanel.Children.Add(new Expander { Name = "GoogleSetupGuide", Header = L10n.Text("SettingsWindow.Text45"),
            IsExpanded = string.IsNullOrWhiteSpace(app.Config.GoogleCredentialsFile), Content = guide });
        var credentials = Add(L10n.Text("SettingsWindow.Text46"), app.Config.GoogleCredentialsFile, googlePanel);
        credentials.Name = "GoogleCredentialsFile";
        var credentialStatus = new TextBlock { TextWrapping = TextWrapping.Wrap };
        credentials.TextChanged += (_, _) => credentialStatus.Text = L10n.Text("SettingsWindow.Text47");
        var pickCredentials = Ui.Button(L10n.Text("SettingsWindow.Text48"), () =>
        {
            var picker = new Microsoft.Win32.OpenFileDialog { Filter = "Google OAuth JSON|*.json" };
            if (picker.ShowDialog(this) != true) return;
            try
            {
                CalendarService.ValidateCredentials(picker.FileName);
                credentials.Text = picker.FileName;
                credentialStatus.Text = L10n.Text("SettingsWindow.Text49");
            }
            catch (Exception ex) { credentialStatus.Text = ex.Message; }
        });
        var credentialStore = new GoogleCredentialsStore(App.DataDirectory);
        var importCredentials = Ui.Button(L10n.Text("SettingsWindow.Text50"), () =>
        {
            try
            {
                var alreadyManaged = credentialStore.IsManagedPath(credentials.Text);
                credentialStore.Import(credentials.Text, app.SaveGoogleCredentialsPath);
                credentials.Text = credentialStore.FilePath;
                credentialStatus.Text = alreadyManaged ? L10n.Text("SettingsWindow.Text51") :
                    L10n.Text("SettingsWindow.Text52");
            }
            catch (Exception ex) { credentialStatus.Text = ex.Message; }
        });
        importCredentials.Name = "GoogleImportCredentials";
        var deleteCredentials = Ui.Button(L10n.Text("SettingsWindow.Text53"), () =>
        {
            try
            {
                if (confirmCredentialDeletion?.Invoke() ?? MessageBox.Show(this,
                    L10n.Text("SettingsWindow.Text54") + credentialStore.FilePath +
                    L10n.Text("SettingsWindow.Text55"),
                    L10n.Text("SettingsWindow.Text56"), MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
                {
                    credentialStore.Delete(() =>
                    {
                        if (credentialStore.IsManagedPath(app.Config.GoogleCredentialsFile)) app.SaveGoogleCredentialsPath("");
                    });
                    if (credentialStore.IsManagedPath(credentials.Text)) credentials.Text = "";
                    credentialStatus.Text = L10n.Text("SettingsWindow.Text57");
                }
            }
            catch (Exception ex) { credentialStatus.Text = ex.Message; }
        });
        deleteCredentials.Name = "GoogleDeleteCredentials";
        var credentialButtons = new WrapPanel();
        credentialButtons.Children.Add(pickCredentials);
        credentialButtons.Children.Add(importCredentials);
        credentialButtons.Children.Add(deleteCredentials);
        googlePanel.Children.Add(credentialButtons); googlePanel.Children.Add(credentialStatus);
        googlePanel.Children.Add(new TextBlock { Text = L10n.Text("SettingsWindow.Text58"), TextWrapping = TextWrapping.Wrap, Foreground = Brushes.DimGray });
        var calendar = Add(L10n.Text("SettingsWindow.Text59"), app.Config.CalendarId, googlePanel);
        var status = new TextBlock { TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 10, 0, 10) };
        void CaptureProfile()
        {
            if (loadingProfile) return;
            var cli = tasksEnabled.IsChecked == true;
            var folder = (cli ? tasksVault.Text : notes.Text).Trim();
            var previousFolder = cli == selectedProfile.UseCli ? selectedProfile.Folder : cli ? selectedProfile.CliFolder : selectedProfile.LocalRoot;
            if (!string.Equals(folder.TrimEnd('\\', '/'), previousFolder.TrimEnd('\\', '/'), StringComparison.OrdinalIgnoreCase))
            {
                if (selectedProfile.Id == Settings.MainVaultId) { pendingMigration = false; folderSelectionPending = false; }
                if (cli) selectedProfile.ObsidianNotesFolder = null;
                else selectedProfile.NotesFolder = folder;
            }
            selectedProfile.Name = profileName.Text.Trim();
            selectedProfile.UseCli = cli; selectedProfile.Folder = folder; selectedProfile.CliTarget = tasksId.Text.Trim();
            selectedProfile.CliFolder = tasksVault.Text.Trim(); selectedProfile.LocalRoot = notes.Text.Trim();
            selectedProfile.DailyFolder = daily.Text.Trim(); selectedProfile.DailyPattern = pattern.Text;
            selectedProfile.DailyRetention = (DailyNoteRetention)Math.Max(0, retention.SelectedIndex);
        }
        void RefreshProfiles()
        {
            loadingProfile = true;
            vaultList.ItemsSource = null; vaultList.ItemsSource = profiles; vaultList.SelectedItem = selectedProfile;
            defaultVault.ItemsSource = null; defaultVault.ItemsSource = profiles; defaultVault.SelectedValue = defaultId;
            loadingProfile = false;
        }
        void LoadProfile(VaultProfile profile)
        {
            loadingProfile = true; selectedProfile = profile;
            profileName.Text = profile.Name;
            tasksVault.Text = profile.UseCli ? profile.Folder : profile.CliFolder;
            notes.Text = !profile.UseCli ? profile.Folder : profile.LocalRoot;
            tasksId.Text = profile.CliTarget;
            daily.Text = profile.DailyFolder; pattern.Text = profile.DailyPattern;
            retention.SelectedIndex = (int)profile.DailyRetention; tasksEnabled.IsChecked = profile.UseCli;
            loadingProfile = false; UpdateSourceMode();
        }
        Settings ReadSettings()
        {
            CaptureProfile();
            foreach (var profile in profiles)
            {
                if (string.IsNullOrWhiteSpace(profile.Name)) throw new InvalidOperationException(L10n.Text("Vault.Invalid"));
                if (!Path.IsPathFullyQualified(profile.Folder)) throw new InvalidOperationException(L10n.Text("TasksPreview.VaultRequired"));
                if (profile.UseCli)
                {
                    if (!Path.IsPathFullyQualified(tasksCli.Text.Trim())) throw new InvalidOperationException(L10n.Text("TasksPreview.CliRequired"));
                    // Multiple connections use explicit targets instead of inferring names from folders.
                    if (profiles.Count > 1 && string.IsNullOrWhiteSpace(profile.CliTarget)) throw new InvalidOperationException(L10n.Text("Vault.TargetRequired"));
                }
                else
                {
                    if (profile.DailyFolder.Length > 0 && !Path.IsPathFullyQualified(profile.DailyFolder)) throw new InvalidOperationException(L10n.Text("SettingsWindow.Text61"));
                    DailyNoteResolver.Validate(profile.DailyPattern);
                }
            }
            if (profiles.Select(p => Path.GetFullPath(p.Folder).TrimEnd('\\', '/')).Distinct(StringComparer.OrdinalIgnoreCase).Count() != profiles.Count)
                throw new InvalidOperationException(L10n.Text("Vault.Duplicate"));
            if (string.IsNullOrWhiteSpace(calendar.Text)) throw new InvalidOperationException(L10n.Text("SettingsWindow.Text62"));
            var next = new Settings
            {
                Language = languageCodes[Math.Max(0, language.SelectedIndex)],
                NotesRoot = app.Config.NotesRoot,
                AutoSaveOnFocusLoss = autoSave.IsChecked == true,
                DoubleClickToEdit = editGesture.SelectedIndex == 1,
                TaskCompletion = (TaskCompletionMode)Math.Max(0, taskCompletion.SelectedIndex),
                TaskToggleShortcut = taskToggleShortcut.Text,
                TitleButtonOverlay = overlay.IsChecked == true,
                ShowInTaskbar = taskbar.IsChecked == true,
                GoogleCredentialsFile = credentials.Text, CalendarId = calendar.Text,
                ObsidianCli = tasksCli.Text.Trim(), DefaultDailyVaultId = defaultId
            };
            next.SetVaultProfiles(profiles);
            return next;
        }
        void SelectStickyFolder()
        {
            CaptureProfile();
            var next = new Settings { NotesRoot = app.Config.NotesRoot, ObsidianCli = tasksCli.Text.Trim() };
            next.SetVaultProfiles([selectedProfile]);
            var source = app.NoteSources(next);
            var selected = source.IsCli ? next.ObsidianNotesFolder : StickyFolderPath.Relative(source.FolderRoot, next.NotesFolder);
            var picker = new StickyFolderWindow(source, selected, (relative, migrate) =>
            {
                if (source.IsCli) selectedProfile.ObsidianNotesFolder = relative;
                else selectedProfile.NotesFolder = StickyFolderPath.Absolute(source.FolderRoot, relative);
                pendingMigration = migrate; folderSelectionPending = true; ShowStickyFolder();
                return Task.CompletedTask;
            }) { Owner = this };
            picker.ShowDialog();
        }
        chooseStickyFolder.Click += (_, _) => app.Safe(SelectStickyFolder);
        async Task Save()
        {
            var next = ReadSettings();
            if (folderSelectionPending) await app.ApplyFolderSettingsAsync(next, pendingMigration);
            else app.ApplySettings(next, false);
            pendingMigration = false; folderSelectionPending = false;
        }
        var login = new Button { Name = "GoogleLogin", Content = L10n.Text("SettingsWindow.Text65") };
        var verify = new Button { Name = "GoogleVerify", Content = L10n.Text("SettingsWindow.Text66") };
        var cancel = new Button { Name = "GoogleCancel", Content = L10n.Text("SettingsWindow.Text67"), IsEnabled = false };
        foreach (var button in new[] { login, verify, cancel })
            button.SetResourceReference(ContentControl.ContentTemplateProperty, "WrappingButtonContent");
        var save = new Button { Content = L10n.Text("SettingsWindow.Text68") };
        save.Click += async (_, _) =>
        {
            savingSettings = true; IsEnabled = false;
            try { await Save(); savingSettings = false; Close(); }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, Title, MessageBoxButton.OK, MessageBoxImage.Warning); }
            finally { savingSettings = false; IsEnabled = true; }
        };
        CancellationTokenSource? authentication = null;
        cancel.Click += (_, _) => authentication?.Cancel();
        Closed += (_, _) => authentication?.Cancel();
        async Task Connect(bool signIn)
        {
            using var cancellation = new CancellationTokenSource();
            authentication = cancellation;
            login.IsEnabled = verify.IsEnabled = credentials.IsEnabled = pickCredentials.IsEnabled = calendar.IsEnabled = save.IsEnabled = false;
            credentialButtons.IsEnabled = false;
            cancel.IsEnabled = true;
            var signedIn = false;
            try
            {
                CalendarService.ValidateCredentials(credentials.Text);
                app.Calendar.Configure(credentials.Text);
                if (signIn)
                {
                    status.Text = L10n.Text("SettingsWindow.Text69");
                    await app.Calendar.SignInAsync(cancellation.Token);
                    signedIn = true;
                }
                status.Text = L10n.Text("SettingsWindow.Text70");
                await app.Calendar.VerifyConnectionAsync(calendar.Text, cancellation.Token);
                await app.Calendar.VerifyTasksConnectionAsync(cancellation.Token);
                status.Text = L10n.Text("SettingsWindow.Text71");
            }
            catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
            { status.Text = L10n.Text("SettingsWindow.Text72"); }
            catch (Exception ex) when (signedIn)
            { status.Text = L10n.Text("SettingsWindow.Text73") + ex.Message; }
            catch (Exception ex) { status.Text = ex.Message; }
            finally
            {
                authentication = null;
                login.IsEnabled = verify.IsEnabled = credentials.IsEnabled = pickCredentials.IsEnabled = calendar.IsEnabled = save.IsEnabled = true;
                credentialButtons.IsEnabled = true;
                cancel.IsEnabled = false;
            }
        }
        login.Click += async (_, _) => await Connect(true);
        verify.Click += async (_, _) => await Connect(false);
        googlePanel.Children.Add(login); googlePanel.Children.Add(verify); googlePanel.Children.Add(cancel); googlePanel.Children.Add(status);
        GeneralPanel = panel; GooglePanel = googlePanel;
        VaultDetailsPanel = new StackPanel { Margin = new Thickness(20) };
        foreach (var control in sourceControls) { panel.Children.Remove(control); VaultDetailsPanel.Children.Add(control); }
        var cliLabel = tasksPanel.Children[tasksPanel.Children.IndexOf(tasksCli) - 1];
        tasksPanel.Children.Remove(cliLabel); tasksPanel.Children.Remove(tasksCli);
        cliLabel.Visibility = tasksCli.Visibility = Visibility.Visible;
        var sharedCli = new StackPanel(); sharedCli.Children.Add(cliLabel); sharedCli.Children.Add(tasksCli); panel.Children.Add(sharedCli);
        var vaultPane = new Grid();
        vaultPane.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(200) });
        vaultPane.ColumnDefinitions.Add(new ColumnDefinition());
        var listPanel = new StackPanel { Margin = new Thickness(16) };
        listPanel.Children.Add(vaultList);
        var addVault = Ui.Button(L10n.Text("Vault.Add"), () =>
        {
            CaptureProfile();
            var number = profiles.Count(p => p.Id != Settings.MainVaultId) + 1;
            var profile = new VaultProfile { Name = L10n.Text("Vault.Sub") + (number == 1 ? "" : " " + number), UseCli = selectedProfile.UseCli };
            profiles.Add(profile); LoadProfile(profile); RefreshProfiles();
        });
        addVault.Name = "AddVault"; listPanel.Children.Add(addVault);
        var removeVault = Ui.Button(L10n.Text("Vault.Remove"), () =>
        {
            if (selectedProfile.Id == Settings.MainVaultId || app.Notes.Any(n => n.Placement.VaultId == selectedProfile.Id) ||
                app.Config.RecentlyClosedWindows.Any(p => p.VaultId == selectedProfile.Id))
                throw new InvalidOperationException(L10n.Text("Vault.InUse"));
            profiles.Remove(selectedProfile);
            if (defaultId == selectedProfile.Id) defaultId = Settings.MainVaultId;
            LoadProfile(profiles[0]); RefreshProfiles();
        });
        removeVault.Name = "RemoveVault"; listPanel.Children.Add(removeVault);
        listPanel.Children.Add(new TextBlock { Text = L10n.Text("Vault.Default"), TextWrapping = TextWrapping.Wrap });
        listPanel.Children.Add(defaultVault);
        listPanel.Children.Add(new TextBlock { Text = L10n.Text("Vault.DefaultHelp"), TextWrapping = TextWrapping.Wrap, Foreground = Brushes.DimGray });
        var details = new StackPanel();
        var namePanel = new StackPanel { Margin = new Thickness(20, 16, 20, 0) };
        namePanel.Children.Add(new TextBlock { Text = L10n.Text("Vault.Name") }); namePanel.Children.Add(profileName);
        namePanel.Children.Add(new TextBlock { Text = L10n.Text("Vault.FolderHelp"), TextWrapping = TextWrapping.Wrap, Foreground = Brushes.DimGray });
        details.Children.Add(namePanel); details.Children.Add(VaultDetailsPanel);
        var connectionCheck = Ui.Button(L10n.Text("Vault.Check"), SchedulePreview); connectionCheck.Name = "CheckVault";
        namePanel.Children.Add(connectionCheck);
        vaultList.SelectionChanged += (_, _) =>
        {
            if (loadingProfile || vaultList.SelectedItem is not VaultProfile profile || ReferenceEquals(profile, selectedProfile)) return;
            CaptureProfile(); LoadProfile(profile); RefreshProfiles();
        };
        profileName.LostFocus += (_, _) => { if (!loadingProfile) { CaptureProfile(); RefreshProfiles(); } };
        defaultVault.SelectionChanged += (_, _) => { if (!loadingProfile && defaultVault.SelectedValue is string id) defaultId = id; };
        RefreshProfiles();
        var detailsPane = new ScrollViewer { Content = details, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        Grid.SetColumn(detailsPane, 1);
        vaultPane.Children.Add(new ScrollViewer { Content = listPanel, VerticalScrollBarVisibility = ScrollBarVisibility.Auto });
        vaultPane.Children.Add(detailsPane);
        Categories = new TabControl { Name = "SettingsCategories" };
        Categories.Items.Add(new TabItem { Header = L10n.Text("Vault.General"), Content = new ScrollViewer { Name = "GeneralSettingsPane", Content = panel, VerticalScrollBarVisibility = ScrollBarVisibility.Auto } });
        Categories.Items.Add(new TabItem { Header = L10n.Text("Vault.Category"), Content = vaultPane });
        Categories.Items.Add(new TabItem { Header = "Google", Content = new ScrollViewer { Name = "GoogleSettingsPane", Content = googlePanel, VerticalScrollBarVisibility = ScrollBarVisibility.Auto } });
        Footer = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(12) };
        Footer.Children.Add(Ui.Button(L10n.Text("Ui.Text01"), Close)); Footer.Children.Add(save);
        var layout = new Grid(); layout.RowDefinitions.Add(new RowDefinition()); layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        layout.Children.Add(Categories); Grid.SetRow(Footer, 1); layout.Children.Add(Footer); Content = layout;
        FillEmptyRegex();
    }

    private void PickFolder(TextBox input)
    {
        var picker = new Microsoft.Win32.OpenFolderDialog();
        if (Directory.Exists(input.Text)) picker.InitialDirectory = input.Text;
        if (picker.ShowDialog(this) == true) input.Text = picker.FolderName;
    }
}
