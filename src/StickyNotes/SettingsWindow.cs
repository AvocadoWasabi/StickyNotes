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
        var autoSave = new CheckBox { Name = "AutoSaveOnFocusLoss", Content = "編集欄からフォーカスが外れたら、確認せず自動保存する", IsChecked = app.Config.AutoSaveOnFocusLoss, Margin = new Thickness(0, 0, 0, 8) };
        panel.Children.Add(autoSave);
        panel.Children.Add(new TextBlock { Text = "オフの場合は変更の保存を確認します。保存エラー時は入力を保持します。", TextWrapping = TextWrapping.Wrap, Foreground = Brushes.DimGray });
        var taskbar = new CheckBox { Name = "ShowInTaskbar", Content = "タスクバーにも付箋のアイコンを表示する", IsChecked = app.Config.ShowInTaskbar, Margin = new Thickness(0, 8, 0, 8) };
        panel.Children.Add(taskbar);
        panel.Children.Add(new TextBlock { Text = "保存すると、開いているすべての付箋に反映します。通知領域のアイコンも引き続き利用できます。", TextWrapping = TextWrapping.Wrap, Foreground = Brushes.DimGray });
        var overlay = new CheckBox { Name = "TitleButtonOverlay", Content = "タイトルにマウスカーソルを重ねるとボタンを表示する", IsChecked = app.Config.TitleButtonOverlay, Margin = new Thickness(0, 8, 0, 8) };
        panel.Children.Add(overlay);
        panel.Children.Add(new TextBlock { Text = "オン: タイトルにマウスを重ねるとボタンを表示します。F6でも表示・キーボード操作ができます。左端の移動ハンドルをドラッグして付箋を動かせます。オフ: タイトルの下に常に表示します（既定）。保存すると開いているすべての付箋に反映します。", TextWrapping = TextWrapping.Wrap, Foreground = Brushes.DimGray });
        TextBox Add(string label, string value)
        {
            panel.Children.Add(new TextBlock { Text = label, TextWrapping = TextWrapping.Wrap });
            var input = new TextBox { Text = value }; panel.Children.Add(input); return input;
        }
        var notes = Add("付箋の保存フォルダ（Vault内の Sticky Notes など）", app.Config.NotesFolder);
        panel.Children.Add(Ui.Button("保存フォルダを選択", () => PickFolder(notes)));
        var daily = Add("デイリーノートのフォルダ", app.Config.DailyFolder);
        panel.Children.Add(Ui.Button("デイリーフォルダを選択", () => PickFolder(daily)));
        var pattern = Add("デイリーノートの形式（日時タグ + 正規表現）", app.Config.DailyPattern);
        bool FillEmptyRegex()
        {
            if (!string.IsNullOrWhiteSpace(pattern.Text)) return false;
            pattern.Text = DailyNoteResolver.RegexExample;
            pattern.SelectAll();
            return true;
        }
        panel.Children.Add(new TextBlock { TextWrapping = TextWrapping.Wrap, Foreground = Brushes.DimGray,
            Text = "year・month・day のタグで今日の日付を判別します。拡張子 .md を含む相対パス全体に照合し、区切りは / を使います。\n例: " + DailyNoteResolver.RegexExample });
        var previewStatus = new TextBlock { Name = "DailyPreviewStatus", TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 8, 0, 4) };
        var previewPanel = new StackPanel();
        previewPanel.Children.Add(previewStatus);
        previewPanel.Children.Add(new TextBlock { Text = "内容プレビューは、設定を保存後に「… → デイリーノートを表示…」を開くと末尾に表示します。固定ノートは「ノートの一部分を付箋にする…」から確認できます。", TextWrapping = TextWrapping.Wrap, Foreground = Brushes.DimGray });
        panel.Children.Add(previewPanel);
        panel.Children.Add(new TextBlock { Text = "今日のデイリーノートが未作成のとき", TextWrapping = TextWrapping.Wrap });
        var retention = new ComboBox { Name = "DailyRetention", ItemsSource = new[] {
            "待機メッセージを表示（既定）", "1. 今日の分が作成されるまで昨日の分を表示", "2. 再読込するまで昨日の分を表示" },
            SelectedIndex = Enum.IsDefined(app.Config.DailyRetention) ? (int)app.Config.DailyRetention : 0 };
        panel.Children.Add(retention);
        panel.Children.Add(new TextBlock { Text = "Obsidianで今日の分を作成すると自動表示します。設定で昨日の分を保持することもできます。1 は作成を検出すると自動切替、2 は作成後も付箋の「再読込」まで保持します。再読込時に今日の分がなければ待機表示になります。昨日の分もない場合は待機表示です。", TextWrapping = TextWrapping.Wrap, Foreground = Brushes.DimGray });
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
            previewStatus.Text = "今日のデイリーノートを確認中…";
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
                previewStatus.Text = "今日のノートが見つかりました: " + System.IO.Path.GetRelativePath(folder, path);
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
        var insertTags = Ui.Button("日時タグ付きの既定例を挿入", () =>
        {
            var insert = confirmRegexTemplate?.Invoke() ?? MessageBox.Show(this,
                "日時判定用のタグ year・month・day を含む正規表現を入力しますか？\n\n" + DailyNoteResolver.RegexExample +
                "\n\nはい: 入力欄全体をこの既定例に置き換えます。\nいいえ: 入力済みの式を維持します。",
                "日時タグ付き正規表現の挿入", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes;
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
        panel.Children.Add(new TextBlock { Text = "Google Calendar 接続ガイド（任意）", FontSize = 18,
            FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 20, 0, 8) });
        var guide = new StackPanel();
        void Explain(string text) => guide.Children.Add(new TextBlock { Text = text, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 4, 0, 4) });
        void Link(string label, string url) => guide.Children.Add(Ui.Button(label,
            () => System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url) { UseShellExecute = true })));
        Explain("1. Google側の準備\n以下のボタンで標準ブラウザを開きます。同じGoogle Cloudプロジェクトで順番に設定してください。登録とGoogleへのログイン・同意はブラウザで行います。");
        Link("① プロジェクトを選択・作成し、Calendar APIを有効にする", "https://console.cloud.google.com/apis/library/calendar-json.googleapis.com");
        Explain("Google Auth Platformのブランディング（アプリ名・連絡先）、対象（Audience）を設定します。外部向け・テスト中の場合は、使用するGoogleアカウントをテストユーザーに追加してください。");
        Link("② OAuth同意画面・テストユーザーを設定する", "https://console.cloud.google.com/auth/overview");
        Explain("データアクセスで calendar.events（予定の表示・編集）を設定します。このアプリもログイン時にこの権限を要求します。クライアントの種類は「デスクトップアプリ」を選び、作成後にJSONをダウンロードしてください。");
        Link("③ デスクトップ用OAuthクライアントを作成する", "https://console.cloud.google.com/auth/clients");
        Explain("テスト公開中は再認証が必要になることがあります。403の場合はAPIの有効化、テストユーザー、アクセス許可を確認してください。個人のJSONはリポジトリや共有Vaultに置かず、移動・削除せず保管してください。");
        panel.Children.Add(new Expander { Name = "GoogleSetupGuide", Header = "手順1: 初回準備のガイドを開く",
            IsExpanded = string.IsNullOrWhiteSpace(app.Config.GoogleCredentialsFile), Content = guide });
        var credentials = Add("手順2: Google OAuth デスクトップアプリのJSON", app.Config.GoogleCredentialsFile);
        credentials.Name = "GoogleCredentialsFile";
        var credentialStatus = new TextBlock { TextWrapping = TextWrapping.Wrap };
        credentials.TextChanged += (_, _) => credentialStatus.Text = "JSONはログインまたは接続確認時に検証します。";
        var pickCredentials = Ui.Button("認証JSONを選択して確認", () =>
        {
            var picker = new Microsoft.Win32.OpenFileDialog { Filter = "Google OAuth JSON|*.json" };
            if (picker.ShowDialog(this) != true) return;
            try
            {
                CalendarService.ValidateCredentials(picker.FileName);
                credentials.Text = picker.FileName;
                credentialStatus.Text = "デスクトップ用JSONを確認しました。手順3へ進んでください。";
            }
            catch (Exception ex) { credentialStatus.Text = ex.Message; }
        });
        panel.Children.Add(pickCredentials); panel.Children.Add(credentialStatus);
        var calendar = Add("Calendar ID（自分のメインカレンダーは primary）", app.Config.CalendarId);
        var status = new TextBlock { TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 10, 0, 10) };
        bool Save()
        {
            if (!Path.IsPathFullyQualified(notes.Text)) throw new InvalidOperationException("保存フォルダは絶対パスで指定してください。");
            if (daily.Text.Length > 0 && !Path.IsPathFullyQualified(daily.Text)) throw new InvalidOperationException("デイリーフォルダは絶対パスで指定してください。");
            DailyNoteResolver.Validate(pattern.Text);
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
                DailyRetention = (DailyNoteRetention)Math.Max(0, retention.SelectedIndex),
                AutoSaveOnFocusLoss = autoSave.IsChecked == true,
                TitleButtonOverlay = overlay.IsChecked == true,
                ShowInTaskbar = taskbar.IsChecked == true,
                GoogleCredentialsFile = credentials.Text, CalendarId = calendar.Text
            }, migrate);
            notes.Text = app.Config.NotesFolder; daily.Text = app.Config.DailyFolder;
            return true;
        }
        var login = new Button { Name = "GoogleLogin", Content = "手順3: 設定を保存してGoogleにログイン" };
        var verify = new Button { Name = "GoogleVerify", Content = "手順4: 保存して接続を確認（ログイン済みの場合）" };
        var cancel = new Button { Name = "GoogleCancel", Content = "認証・接続確認をキャンセル", IsEnabled = false };
        var save = Ui.Button("保存して閉じる", () => { if (Save()) Close(); });
        CancellationTokenSource? authentication = null;
        cancel.Click += (_, _) => authentication?.Cancel();
        Closed += (_, _) => authentication?.Cancel();
        async Task Connect(bool signIn)
        {
            using var cancellation = new CancellationTokenSource();
            authentication = cancellation;
            login.IsEnabled = verify.IsEnabled = credentials.IsEnabled = pickCredentials.IsEnabled = calendar.IsEnabled = save.IsEnabled = false;
            cancel.IsEnabled = true;
            var signedIn = false;
            try
            {
                CalendarService.ValidateCredentials(credentials.Text);
                if (!Save()) return;
                app.Calendar.Configure(credentials.Text);
                if (signIn)
                {
                    status.Text = "ブラウザでGoogleにログインし、カレンダーへのアクセスを許可してください（3分以内）。認証後は自動で接続確認に進みます。";
                    await app.Calendar.SignInAsync(cancellation.Token);
                    signedIn = true;
                }
                status.Text = "カレンダーへの接続を確認しています…";
                await app.Calendar.VerifyConnectionAsync(calendar.Text, cancellation.Token);
                status.Text = "接続確認が完了しました。付箋に @calendar 2026-10-06T09:00 のように入力できます。予定は変更していません。";
            }
            catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
            { status.Text = "キャンセルしました。ブラウザの認証タブを閉じてください。完了済みの認証情報は保持します。"; }
            catch (Exception ex) when (signedIn)
            { status.Text = "ログイン情報は保存しましたが、接続確認に失敗しました。Calendar ID、APIの有効化、アクセス許可を確認して手順4を再試行してください。\n" + ex.Message; }
            catch (Exception ex) { status.Text = ex.Message; }
            finally
            {
                authentication = null;
                login.IsEnabled = verify.IsEnabled = credentials.IsEnabled = pickCredentials.IsEnabled = calendar.IsEnabled = save.IsEnabled = true;
                cancel.IsEnabled = false;
            }
        }
        login.Click += async (_, _) => await Connect(true);
        verify.Click += async (_, _) => await Connect(false);
        panel.Children.Add(login); panel.Children.Add(verify); panel.Children.Add(cancel); panel.Children.Add(status);
        panel.Children.Add(new TextBlock { Text = "保存先を変更して保存すると、既存の付箋ファイルも移行するか確認します。\nバックアップ・配置・認証情報は %LOCALAPPDATA%\\StickyNotes に保存します。", TextWrapping = TextWrapping.Wrap, Foreground = Brushes.DimGray });
        panel.Children.Add(save);
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
