namespace StickyNotes;

public sealed class SettingsWindow : Window
{
    public SettingsWindow()
    {
        SetResourceReference(IconProperty, "AppIcon");
        Title = "Sticky Notes 設定"; Width = 600; SizeToContent = SizeToContent.Height;
        MaxHeight = SystemParameters.WorkArea.Height;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        var app = App.Current;
        var panel = new StackPanel { Margin = new Thickness(24) };
        panel.Children.Add(new TextBlock { Text = "Markdown Sticky Notes", FontSize = 24, FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 16) });
        TextBox Add(string label, string value)
        {
            panel.Children.Add(new TextBlock { Text = label, TextWrapping = TextWrapping.Wrap });
            var input = new TextBox { Text = value }; panel.Children.Add(input); return input;
        }
        var notes = Add("付箋の保存フォルダ（Vault内の Sticky Notes など）", app.Config.NotesFolder);
        panel.Children.Add(Ui.Button("保存フォルダを選択", () => PickFolder(notes)));
        var daily = Add("デイリーノートのフォルダ", app.Config.DailyFolder);
        panel.Children.Add(Ui.Button("デイリーフォルダを選択", () => PickFolder(daily)));
        var useRegex = new CheckBox { Content = "デイリーノートの形式に正規表現を使う", IsChecked = app.Config.DailyPatternIsRegex, Margin = new Thickness(0, 8, 0, 8) };
        panel.Children.Add(useRegex);
        var pattern = Add("デイリーノートの形式（日付書式 / 正規表現）", app.Config.DailyPattern);
        var patternHelp = new TextBlock { TextWrapping = TextWrapping.Wrap, Foreground = Brushes.DimGray };
        void UpdatePatternHelp() => patternHelp.Text = useRegex.IsChecked == true
            ? "拡張子を含む相対パス全体に照合します。区切りは /。year・month・day で今日の日付を判別します。\n例: " + DailyNoteResolver.RegexExample
            : "日付書式は.NET形式。例: yyyy-MM-dd / yyyy/MM/yyyy-MM-dd（.md は自動付加）";
        useRegex.Checked += (_, _) => UpdatePatternHelp();
        useRegex.Unchecked += (_, _) => UpdatePatternHelp();
        UpdatePatternHelp(); panel.Children.Add(patternHelp);
        var credentials = Add("Google OAuth デスクトップアプリのJSON", app.Config.GoogleCredentialsFile);
        panel.Children.Add(Ui.Button("認証JSONを選択", () =>
        {
            var picker = new Microsoft.Win32.OpenFileDialog { Filter = "Google OAuth JSON|*.json" };
            if (picker.ShowDialog(this) == true) credentials.Text = picker.FileName;
        }));
        var calendar = Add("Calendar ID（自分のメインカレンダーは primary）", app.Config.CalendarId);
        var status = new TextBlock { TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 10, 0, 10) };
        bool Save()
        {
            if (!Path.IsPathFullyQualified(notes.Text)) throw new InvalidOperationException("保存フォルダは絶対パスで指定してください。");
            if (daily.Text.Length > 0 && !Path.IsPathFullyQualified(daily.Text)) throw new InvalidOperationException("デイリーフォルダは絶対パスで指定してください。");
            DailyNoteResolver.Validate(pattern.Text, useRegex.IsChecked == true);
            if (string.IsNullOrWhiteSpace(calendar.Text)) throw new InvalidOperationException("Calendar IDを指定してください。");
            var folder = NoteFolderMigration.Normalize(notes.Text);
            var migrate = false;
            if (!NoteFolderMigration.SameFolder(app.Config.NotesFolder, folder))
            {
                var answer = MessageBox.Show(this,
                    $"付箋の保存ファイルもすべて移行しますか？\n\n移行元: {app.Config.NotesFolder}\n移行先: {folder}\n\nはい: 旧フォルダ内のすべての .md ファイルを、サブフォルダ・閉じている付箋も含めて移行します。同名ファイルは上書きしません。\nデイリーノートのフォルダ設定は自動変更しません。旧フォルダ内のデイリーノートも移動する場合は、その設定を別途変更してください。\nいいえ: 新規付箋の保存先だけ変更し、既存ファイルは残します。\nキャンセル: 設定の保存を中止します。",
                    "付箋ファイルの移行", MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Cancel);
                if (answer is not (MessageBoxResult.Yes or MessageBoxResult.No)) return false;
                migrate = answer == MessageBoxResult.Yes;
            }
            app.ApplySettings(new Settings
            {
                NotesFolder = folder, DailyFolder = daily.Text, DailyPattern = pattern.Text,
                DailyPatternIsRegex = useRegex.IsChecked == true,
                GoogleCredentialsFile = credentials.Text, CalendarId = calendar.Text
            }, migrate);
            notes.Text = app.Config.NotesFolder; daily.Text = app.Config.DailyFolder;
            return true;
        }
        var login = new Button { Content = "設定を保存してGoogleにログイン" };
        login.Click += async (_, _) =>
        {
            login.IsEnabled = false;
            try { if (!Save()) return; app.Calendar.Configure(credentials.Text); status.Text = "ブラウザで認証してください（3分以内）…"; await app.Calendar.SignInAsync(); status.Text = "Googleと接続しました。付箋に @calendar コマンドを入力できます。"; }
            catch (Exception ex) { status.Text = ex.Message; }
            finally { login.IsEnabled = true; }
        };
        panel.Children.Add(login); panel.Children.Add(status);
        panel.Children.Add(new TextBlock { Text = "保存先を変更して保存すると、既存の付箋ファイルも移行するか確認します。\nバックアップ・配置・認証情報は %LOCALAPPDATA%\\StickyNotes に保存します。", TextWrapping = TextWrapping.Wrap, Foreground = Brushes.DimGray });
        panel.Children.Add(Ui.Button("保存して閉じる", () => { if (Save()) Close(); }));
        Content = new ScrollViewer { Content = panel, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
    }

    private void PickFolder(TextBox input)
    {
        var picker = new Microsoft.Win32.OpenFolderDialog();
        if (Directory.Exists(input.Text)) picker.InitialDirectory = input.Text;
        if (picker.ShowDialog(this) == true) input.Text = picker.FolderName;
    }
}
