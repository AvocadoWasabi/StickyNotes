using System.Threading;
using System.Windows.Threading;

namespace StickyNotes;

public sealed class SettingsWindow : Window
{
    public SettingsWindow() : this(null) { }

    internal SettingsWindow(Func<bool>? confirmRegexTemplate)
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
        bool FillEmptyRegex()
        {
            if (useRegex.IsChecked != true || !string.IsNullOrWhiteSpace(pattern.Text)) return false;
            pattern.Text = DailyNoteResolver.RegexExample;
            pattern.SelectAll();
            return true;
        }
        var patternHelp = new TextBlock { TextWrapping = TextWrapping.Wrap, Foreground = Brushes.DimGray };
        void UpdatePatternHelp() => patternHelp.Text = useRegex.IsChecked == true
            ? "拡張子を含む相対パス全体に照合します。区切りは /。year・month・day で今日の日付を判別します。\n例: " + DailyNoteResolver.RegexExample
            : "日付書式は.NET形式。例: yyyy-MM-dd / yyyy/MM/yyyy-MM-dd（.md は自動付加）";
        useRegex.Checked += (_, _) => UpdatePatternHelp();
        useRegex.Unchecked += (_, _) => UpdatePatternHelp();
        UpdatePatternHelp(); panel.Children.Add(patternHelp);
        var previewStatus = new TextBlock { Name = "DailyPreviewStatus", TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 8, 0, 4) };
        var previewText = new TextBox { Name = "DailyPreviewText", IsReadOnly = true, TextWrapping = TextWrapping.Wrap,
            Height = 150, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, Visibility = Visibility.Collapsed };
        var previewPanel = new StackPanel();
        previewPanel.Children.Add(previewStatus); previewPanel.Children.Add(previewText);
        previewPanel.Children.Add(new TextBlock { Text = "表示するには設定を保存し、付箋の「… → ノートの見出しを表示…」で見出し名（# は不要）を入力して、毎日切替を yes にしてください。今日のノートの場合、パス欄は空欄です。", TextWrapping = TextWrapping.Wrap, Foreground = Brushes.DimGray });
        panel.Children.Add(previewPanel);
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
            previewText.Text = ""; previewText.Visibility = Visibility.Collapsed;
            previewStatus.Foreground = Brushes.DimGray;
            previewStatus.Text = "今日のデイリーノートを確認中…";
            previewTimer.Start();
        }
        previewTimer.Tick += async (_, _) =>
        {
            previewTimer.Stop();
            var version = previewVersion;
            var folder = daily.Text; var expression = pattern.Text; var regex = useRegex.IsChecked == true;
            var today = DateTime.Today;
            using var cancellation = new CancellationTokenSource();
            previewCancellation = cancellation;
            try
            {
                await previewGate.WaitAsync(cancellation.Token);
                DailyNotePreview result;
                try { result = await Task.Run(() => DailyNotePreview.Read(folder, expression, regex, today, cancellation.Token)); }
                finally { previewGate.Release(); }
                if (previewClosed || version != previewVersion) return;
                previewStatus.Foreground = Brushes.DarkGreen;
                previewStatus.Text = "今日のノートが見つかりました: " + System.IO.Path.GetRelativePath(folder, result.Path) +
                    "\n内容プレビュー（Markdown・読み取り専用）" + (result.Truncated ? " — 先頭4000文字まで" : "");
                previewText.Text = result.Text; previewText.Visibility = Visibility.Visible;
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                if (previewClosed || version != previewVersion) return;
                previewStatus.Foreground = Brushes.DarkRed;
                previewStatus.Text = "確認できません: " + ex.Message;
            }
            finally
            {
                if (ReferenceEquals(previewCancellation, cancellation)) previewCancellation = null;
            }
        };
        daily.TextChanged += (_, _) => SchedulePreview();
        pattern.TextChanged += (_, _) => { if (!FillEmptyRegex()) SchedulePreview(); };
        useRegex.Checked += (_, _) =>
        {
            var insert = confirmRegexTemplate?.Invoke() ?? MessageBox.Show(this,
                "日時判定用のタグ year・month・day を含む正規表現を入力しますか？\n\n" + DailyNoteResolver.RegexExample +
                "\n\nはい: 入力欄全体をこの既定例に置き換えます。\nいいえ: 入力済みの式を維持します。\n空欄の場合は、どちらを選んでも既定例を自動補完します。",
                "日時タグ付き正規表現の挿入", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes;
            if (insert)
            {
                pattern.Text = DailyNoteResolver.RegexExample;
                pattern.Focus(); pattern.SelectAll();
            }
            FillEmptyRegex();
            SchedulePreview();
        };
        useRegex.Unchecked += (_, _) => SchedulePreview();
        Loaded += (_, _) => SchedulePreview();
        Closed += (_, _) => { previewClosed = true; previewVersion++; previewTimer.Stop(); previewCancellation?.Cancel(); };
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
        FillEmptyRegex();
    }

    private void PickFolder(TextBox input)
    {
        var picker = new Microsoft.Win32.OpenFolderDialog();
        if (Directory.Exists(input.Text)) picker.InitialDirectory = input.Text;
        if (picker.ShowDialog(this) == true) input.Text = picker.FolderName;
    }
}
