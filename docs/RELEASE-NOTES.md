[English](#english) | [日本語](#日本語) | [简体中文](#简体中文)

# Markdown Sticky Notes v0.0.4

## 日本語

Windows 10 / 11（x64）向け。Google Calendarの初期設定と入力補完、タスクバー操作を改善しました。

- **Google連携ガイド**：設定の右ペインに準備手順・ブラウザ認証・接続確認を集約。OAuth JSONの取り込み・コピーの削除、認証のキャンセルに対応しました。
- **`@` で補完**：独立した段落で入力すると候補と例を表示。Tab・Enter・クリックで今日の日時を挿入できます。タイムゾーン・検索キーワードは省略できます。
- **タスクバー操作**：付箋アイコンの表示を選択でき、右クリックから通知領域と同じ8項目を操作できます。全付箋を10秒間最前面に表示する機能も追加しました。
- **資料を整理**：READMEを短くし、詳しい操作とGoogle設定を別ガイドへ分離。3言語を揃えました。

### インストール・更新

アプリを終了し、Assetsの **StickyNotes-win-x64.zip** を別フォルダへ展開して **Install.cmd** を実行してください。ノート・設定・バックアップは保持されます。直接 `StickyNotes.exe` を起動する場合も、同梱ファイル全体を保持してください。

管理者権限や.NETの追加インストールは不要です。アプリは未署名です。SHA256は同梱の `.zip.sha256` で確認できます。画面は日本語です。

Google連携は任意で、利用者自身のOAuth設定が必要です。JSONの削除は取り込み済みコピーだけが対象で、トークンやGoogle側の許可は削除しません。実Googleアカウントでの接続・更新は自動テストの対象外です。

[使い方](https://github.com/AvocadoWasabi/StickyNotes/blob/v0.0.4/README.ja.md) · [Google設定](https://github.com/AvocadoWasabi/StickyNotes/blob/v0.0.4/docs/GOOGLE-CALENDAR.md) · [変更履歴](https://github.com/AvocadoWasabi/StickyNotes/blob/v0.0.4/CHANGELOG.ja.md)

## English

For Windows 10 / 11 (x64). This release improves Google Calendar setup, command completion, and taskbar controls.

- **Google setup guide:** A dedicated right pane covers preparation, browser sign-in, and connection checks. Import or delete the managed OAuth JSON copy and cancel authentication when needed.
- **Complete with `@`:** Type it in a separate paragraph to see a suggestion and examples. Tab, Enter, or a click inserts today's date and time. Time zone offsets and search keywords are optional.
- **Taskbar controls:** Optionally show note icons and access the same eight actions as the notification-area menu. Bring all notes to the top for 10 seconds.
- **Shorter documentation:** Keep basic usage in the README and move detailed controls and Google setup into separate guides, in all three languages.

### Installation and updates

Exit the app, extract **StickyNotes-win-x64.zip** from Assets into a separate folder, and run **Install.cmd**. Notes, settings, and backups are retained. For portable use, run `StickyNotes.exe` and keep all accompanying files.

No administrator privileges or separate .NET installation are required. The app is unsigned. Verify the SHA256 using the accompanying `.zip.sha256` file. The UI is Japanese.

Google integration is optional and requires your own OAuth setup. Deleting JSON removes only the imported copy, not tokens or Google-side authorization. Automated tests do not connect to or update a real Google account.

[User guide](https://github.com/AvocadoWasabi/StickyNotes/blob/v0.0.4/README.md) · [Google setup](https://github.com/AvocadoWasabi/StickyNotes/blob/v0.0.4/docs/GOOGLE-CALENDAR.en.md) · [Changelog](https://github.com/AvocadoWasabi/StickyNotes/blob/v0.0.4/CHANGELOG.md)

## 简体中文

支持 Windows 10 / 11（x64）。本版本改进 Google Calendar 初始配置、命令补全和任务栏操作。

- **Google 配置指南**：右侧独立窗格集中显示准备步骤、浏览器登录和连接检查。支持导入或删除 OAuth JSON 的托管副本，以及取消授权。
- **输入 `@` 补全**：在独立段落输入即可显示候选和示例。Tab、Enter 或点击可插入今天的日期时间；时区偏移和搜索关键词均可省略。
- **任务栏操作**：可选择显示便签图标，右键使用与通知区域相同的八项操作。新增将全部便签临时置顶10秒的功能。
- **精简文档**：README 保留基本操作，详细用法和 Google 配置移至独立指南，三种语言同步更新。

### 安装与更新

退出应用，将 Assets 中的 **StickyNotes-win-x64.zip** 解压到另一文件夹，运行 **Install.cmd**。笔记、设置和备份会保留。免安装使用时运行 `StickyNotes.exe`，并保留全部附带文件。

无需管理员权限或另装 .NET。应用未签名，可使用附带的 `.zip.sha256` 文件验证 SHA256。界面为日语。

Google 集成为可选功能，需要自行配置 OAuth。删除 JSON 仅删除导入副本，不删除令牌或 Google 端授权。自动测试不连接或修改真实 Google 账号。

[使用指南](https://github.com/AvocadoWasabi/StickyNotes/blob/v0.0.4/README.zh-CN.md) · [Google 配置](https://github.com/AvocadoWasabi/StickyNotes/blob/v0.0.4/docs/GOOGLE-CALENDAR.zh-CN.md) · [更新日志](https://github.com/AvocadoWasabi/StickyNotes/blob/v0.0.4/CHANGELOG.zh-CN.md)
