using System.Threading;
using System.Windows.Threading;

namespace StickyNotes;

public sealed class SettingsWindow : Window
{
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
        var notes = Add(L10n.Text("SettingsWindow.Text08"), app.Config.NotesFolder);
        var tasksEnabled = new CheckBox { Content = L10n.Text("TasksPreview.Enable"), IsChecked = app.Config.ObsidianTasksEnabled };
        var tasksPanel = new StackPanel();
        tasksPanel.Children.Add(tasksEnabled);
        tasksPanel.Children.Add(new TextBlock { Text = L10n.Text("TasksPreview.Help"), TextWrapping = TextWrapping.Wrap });
        var tasksCli = Add(L10n.Text("TasksPreview.Cli"), app.Config.ObsidianCli, tasksPanel);
        var tasksVault = Add(L10n.Text("TasksPreview.Vault"), app.Config.ObsidianVaultFolder, tasksPanel);
        tasksPanel.Children.Add(Ui.Button(L10n.Text("SettingsWindow.Text09"), () => PickFolder(tasksVault)));
        var tasksId = Add(L10n.Text("TasksPreview.VaultId"), app.Config.ObsidianVaultId, tasksPanel);
        if (BuildFlavor.TasksPreview)
            panel.Children.Insert(panel.Children.IndexOf(notes) - 1, new Expander { Header = "Obsidian Tasks — CLI Preview", IsExpanded = true, Content = tasksPanel });
        panel.Children.Add(Ui.Button(L10n.Text("SettingsWindow.Text09"), () => PickFolder(notes)));
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
        var previewStatus = new TextBlock { Name = "DailyPreviewStatus", TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 8, 0, 4) };
        var previewPanel = new StackPanel();
        previewPanel.Children.Add(previewStatus);
        previewPanel.Children.Add(new TextBlock { Text = L10n.Text("SettingsWindow.Text14"), TextWrapping = TextWrapping.Wrap, Foreground = Brushes.DimGray });
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
                string path;
                try { path = await Task.Run(() => DailyNoteResolver.Resolve(folder, expression, today, cancellation.Token)); }
                finally { previewGate.Release(); }
                if (previewClosed || version != previewVersion) return;
                previewStatus.Foreground = Brushes.DarkGreen;
                previewStatus.Text = L10n.Text("SettingsWindow.Text21") + System.IO.Path.GetRelativePath(folder, path);
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
        Loaded += (_, _) => SchedulePreview();
        Closed += (_, _) => { previewClosed = true; previewVersion++; previewTimer.Stop(); previewCancellation?.Cancel(); };
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
        bool Save()
        {
            if (!Path.IsPathFullyQualified(notes.Text)) throw new InvalidOperationException(L10n.Text("SettingsWindow.Text60"));
            if (daily.Text.Length > 0 && !Path.IsPathFullyQualified(daily.Text)) throw new InvalidOperationException(L10n.Text("SettingsWindow.Text61"));
            DailyNoteResolver.Validate(pattern.Text);
            if (string.IsNullOrWhiteSpace(calendar.Text)) throw new InvalidOperationException(L10n.Text("SettingsWindow.Text62"));
            var folder = NoteFolderMigration.Normalize(notes.Text);
            var migrate = false;
            if (!NoteFolderMigration.SameFolder(app.Config.NotesFolder, folder))
            {
                var answer = MessageBox.Show(this,
                    L10n.Format("SettingsWindow.Text63", app.Config.NotesFolder, folder),
                    L10n.Text("SettingsWindow.Text64"), MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Cancel);
                if (answer is not (MessageBoxResult.Yes or MessageBoxResult.No)) return false;
                migrate = answer == MessageBoxResult.Yes;
            }
            app.ApplySettings(new Settings
            {
                Language = languageCodes[Math.Max(0, language.SelectedIndex)],
                NotesFolder = folder, DailyFolder = daily.Text, DailyPattern = pattern.Text,
                DailyRetention = (DailyNoteRetention)Math.Max(0, retention.SelectedIndex),
                AutoSaveOnFocusLoss = autoSave.IsChecked == true,
                TitleButtonOverlay = overlay.IsChecked == true,
                ShowInTaskbar = taskbar.IsChecked == true,
                GoogleCredentialsFile = credentials.Text, CalendarId = calendar.Text,
                ObsidianTasksEnabled = tasksEnabled.IsChecked == true,
                ObsidianCli = tasksCli.Text.Trim(), ObsidianVaultFolder = tasksVault.Text.Trim(), ObsidianVaultId = tasksId.Text.Trim()
            }, migrate);
            notes.Text = app.Config.NotesFolder; daily.Text = app.Config.DailyFolder;
            return true;
        }
        var login = new Button { Name = "GoogleLogin", Content = L10n.Text("SettingsWindow.Text65") };
        var verify = new Button { Name = "GoogleVerify", Content = L10n.Text("SettingsWindow.Text66") };
        var cancel = new Button { Name = "GoogleCancel", Content = L10n.Text("SettingsWindow.Text67"), IsEnabled = false };
        foreach (var button in new[] { login, verify, cancel })
            button.SetResourceReference(ContentControl.ContentTemplateProperty, "WrappingButtonContent");
        var save = Ui.Button(L10n.Text("SettingsWindow.Text68"), () => { if (Save()) Close(); });
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
                if (!Save()) return;
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
        panel.Children.Add(new TextBlock { Text = L10n.Text("SettingsWindow.Text74"), TextWrapping = TextWrapping.Wrap, Foreground = Brushes.DimGray });
        panel.Children.Add(save);
        var layout = new Grid();
        layout.ColumnDefinitions.Add(new ColumnDefinition());
        layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1) });
        layout.ColumnDefinitions.Add(new ColumnDefinition());
        var generalPane = new ScrollViewer { Name = "GeneralSettingsPane", Content = panel,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Auto };
        var googlePane = new ScrollViewer { Name = "GoogleSettingsPane", Content = googlePanel,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Auto };
        // Preserve readable controls on small screens; each pane scrolls independently.
        panel.Width = googlePanel.Width = 440;
        void FitPane(ScrollViewer pane, StackPanel content) => content.Width = Math.Max(440,
            pane.ActualWidth - content.Margin.Left - content.Margin.Right - SystemParameters.VerticalScrollBarWidth);
        generalPane.SizeChanged += (_, _) => FitPane(generalPane, panel);
        googlePane.SizeChanged += (_, _) => FitPane(googlePane, googlePanel);
        var divider = new Border { Background = Brushes.LightGray };
        Grid.SetColumn(divider, 1); Grid.SetColumn(googlePane, 2);
        layout.Children.Add(generalPane); layout.Children.Add(divider); layout.Children.Add(googlePane);
        Content = layout;
        FillEmptyRegex();
    }

    private void PickFolder(TextBox input)
    {
        var picker = new Microsoft.Win32.OpenFolderDialog();
        if (Directory.Exists(input.Text)) picker.InitialDirectory = input.Text;
        if (picker.ShowDialog(this) == true) input.Text = picker.FolderName;
    }
}
