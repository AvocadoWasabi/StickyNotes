namespace StickyNotes;

public sealed class SettingsWindow : Window
{
    public SettingsWindow()
    {
        Title = "Sticky Notes 設定"; Width = 600; SizeToContent = SizeToContent.Height;
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
        var pattern = Add("日付のファイル名書式（.NET形式。例: yyyy-MM-dd / yyyy/MM/yyyy-MM-dd）", app.Config.DailyPattern);
        var credentials = Add("Google OAuth デスクトップアプリのJSON", app.Config.GoogleCredentialsFile);
        panel.Children.Add(Ui.Button("認証JSONを選択", () =>
        {
            var picker = new Microsoft.Win32.OpenFileDialog { Filter = "Google OAuth JSON|*.json" };
            if (picker.ShowDialog(this) == true) credentials.Text = picker.FileName;
        }));
        var calendar = Add("Calendar ID（自分のメインカレンダーは primary）", app.Config.CalendarId);
        var status = new TextBlock { TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 10, 0, 10) };
        void Save()
        {
            if (!Path.IsPathFullyQualified(notes.Text)) throw new InvalidOperationException("保存フォルダは絶対パスで指定してください。");
            if (daily.Text.Length > 0 && !Path.IsPathFullyQualified(daily.Text)) throw new InvalidOperationException("デイリーフォルダは絶対パスで指定してください。");
            if (string.IsNullOrWhiteSpace(pattern.Text)) throw new InvalidOperationException("日付の書式を指定してください。");
            _ = DateTime.Today.ToString(pattern.Text);
            if (string.IsNullOrWhiteSpace(calendar.Text)) throw new InvalidOperationException("Calendar IDを指定してください。");
            app.Config.NotesFolder = notes.Text; app.Config.DailyFolder = daily.Text;
            app.Config.DailyPattern = pattern.Text; app.Config.GoogleCredentialsFile = credentials.Text;
            app.Config.CalendarId = calendar.Text;
            app.SaveConfig();
        }
        var login = new Button { Content = "設定を保存してGoogleにログイン" };
        login.Click += async (_, _) =>
        {
            login.IsEnabled = false;
            try { Save(); app.Calendar.Configure(credentials.Text); status.Text = "ブラウザで認証してください（3分以内）…"; await app.Calendar.SignInAsync(); status.Text = "Googleと接続しました。付箋に @calendar コマンドを入力できます。"; }
            catch (Exception ex) { status.Text = ex.Message; }
            finally { login.IsEnabled = true; }
        };
        panel.Children.Add(login); panel.Children.Add(status);
        panel.Children.Add(new TextBlock { Text = "保存先の変更は新規付箋に適用されます。既存ファイルは移動しません。\nバックアップ・配置・認証情報は %LOCALAPPDATA%\\StickyNotes に保存します。", TextWrapping = TextWrapping.Wrap, Foreground = Brushes.DimGray });
        panel.Children.Add(Ui.Button("保存して閉じる", () => { Save(); Close(); }));
        Content = new ScrollViewer { Content = panel, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
    }

    private void PickFolder(TextBox input)
    {
        var picker = new Microsoft.Win32.OpenFolderDialog();
        if (picker.ShowDialog(this) == true) input.Text = picker.FolderName;
    }
}
