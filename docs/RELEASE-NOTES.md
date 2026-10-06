[English](#english) | [日本語](#日本語) | [简体中文](#简体中文)

# Markdown Sticky Notes v0.0.6

## 日本語

Windows 10 / 11（x64）向け。`@calendar today` とGoogle Tasksの読み取り専用表示を追加しました。

- **当日の予定に自動追従**：`@calendar today` でPCの当日の予定を表示し、約60秒ごとに日付確認・再取得します。キーワード絞り込みに対応し、編集中は自動更新を一時停止します。
- **Google Tasksを併記**：全リストの日付付き・未完了タスクをリスト名とともに表示。`today` は当日、明示した日付はその日以降が対象です。日付なし・完了済み・非表示・削除済みは除外します。読み取り専用で、編集・完了操作・作成・Markdownとの同期には対応していません。
- **時刻の制限を明記**：Google Tasks APIの仕様上、取得できるのは日付のみで、予定時刻は取得できません。Google側で時刻を設定していても「時刻なし」と表示します。設定画面と3言語の資料に明記しました。
- **取得の制御**：Tasksは1回につき最大100件・20リクエスト・30秒。件数や通信回数の上限で一部のみの場合は案内を表示します。片方のサービスが失敗しても他方の結果を表示し、付箋を閉じたときやコマンド変更時は取得をキャンセルします。

### Google連携を利用中の方へ

同じGoogle Cloudプロジェクトで **Google Tasks API** を有効にし、`https://www.googleapis.com/auth/tasks.readonly` を追加してください。設定の **手順3で再認証** し、Tasksの読み取りを許可します。既存のデスクトップ用JSONをそのまま使えます。手順4は接続確認のみで、追加権限を付与できません。

### インストール・更新

アプリを終了し、Assetsの **StickyNotes-win-x64.zip** を別フォルダへ展開して **Install.cmd** を実行してください。ノート・設定・バックアップは保持されます。直接 `StickyNotes.exe` を起動する場合も同梱ファイル全体を保持してください。管理者権限や.NETの追加インストールは不要です。アプリは未署名です。SHA256は同梱の `.zip.sha256` で確認できます。

画面・資料は日本語・英語・簡体字中国語に対応しています。Google連携は任意で、利用者自身のOAuth設定が必要です。実Googleアカウントでの接続・更新は自動テストの対象外です。

[使い方](https://github.com/AvocadoWasabi/StickyNotes/blob/v0.0.6/README.ja.md) · [Calendar / Tasksの設定と制限](https://github.com/AvocadoWasabi/StickyNotes/blob/v0.0.6/docs/GOOGLE-CALENDAR.md#tasks) · [変更履歴](https://github.com/AvocadoWasabi/StickyNotes/blob/v0.0.6/CHANGELOG.ja.md)

## English

For Windows 10 / 11 (x64). Adds `@calendar today` and read-only Google Tasks display.

- **Follow the current day:** `@calendar today` displays the PC's current-day events and checks the date and refreshes about every 60 seconds. Keyword filtering is supported. Automatic refresh pauses while editing.
- **Show Google Tasks:** Display dated, incomplete tasks from all lists with their list names. `today` selects the current day; an explicit date selects that date and later dates. Undated, completed, hidden and deleted tasks are excluded. Tasks is read-only; editing, completion, creation and Markdown synchronization are unsupported.
- **Explain the time limitation:** The Google Tasks API provides dates only and cannot return scheduled times. Even when a time is set in Google, this app displays “no time”. This is stated in Settings and all three documentation languages.
- **Bound retrieval:** Each Tasks refresh is limited to 100 tasks, 20 requests and 30 seconds. A warning identifies partial results when the result/request limit is reached. If one service fails, the other's results remain available. Closing a note or changing its command cancels retrieval.

### For existing Google integration users

Enable **Google Tasks API** in the same Google Cloud project and add `https://www.googleapis.com/auth/tasks.readonly`. **Sign in again using Step 3** in Settings and grant Tasks read access. Keep the existing desktop JSON. Step 4 only checks access and cannot grant the additional permission.

### Installation and updates

Exit the app, extract **StickyNotes-win-x64.zip** from Assets into a separate folder, and run **Install.cmd**. Notes, settings and backups are retained. For portable use, run `StickyNotes.exe` and keep all accompanying files. No administrator privileges or separate .NET installation are required. The app is unsigned. Verify SHA256 using the accompanying `.zip.sha256` file.

The UI and documentation support Japanese, English and Simplified Chinese. Google integration is optional and requires your own OAuth setup. Automated tests do not connect to or update a real Google account.

[User guide](https://github.com/AvocadoWasabi/StickyNotes/blob/v0.0.6/README.md) · [Calendar / Tasks setup and limitations](https://github.com/AvocadoWasabi/StickyNotes/blob/v0.0.6/docs/GOOGLE-CALENDAR.en.md#tasks) · [Changelog](https://github.com/AvocadoWasabi/StickyNotes/blob/v0.0.6/CHANGELOG.md)

## 简体中文

支持 Windows 10 / 11（x64）。新增 `@calendar today` 和 Google Tasks 只读显示。

- **自动跟随当天**：`@calendar today` 显示电脑当天的日程，约每60秒检查日期并刷新，支持关键词筛选。编辑期间暂停自动刷新。
- **显示 Google Tasks**：显示所有列表中有日期的未完成任务及列表名。`today` 选择当天，明确指定日期则包含该日及之后的任务。不包含无日期、已完成、隐藏或已删除的任务。Tasks 为只读，不支持编辑、完成、创建或与 Markdown 同步。
- **明确时间限制**：Google Tasks API 的规格仅提供日期，无法获取计划时间。即使在 Google 中设置了时间，本应用仍显示“无时间”。设置和三种语言文档中均已明确说明。
- **限制获取量**：每次 Tasks 获取最多100项任务、20次请求、30秒。因数量或请求次数上限只显示部分结果时会提示。某一服务失败时仍显示另一服务的结果，关闭便签或更改命令时取消获取。

### 已使用 Google 集成的用户

在同一 Google Cloud 项目启用 **Google Tasks API**，添加 `https://www.googleapis.com/auth/tasks.readonly`。在设置中通过**步骤3重新登录**，允许读取 Tasks。继续使用现有桌面客户端 JSON 即可。步骤4仅检查连接，无法添加权限。

### 安装与更新

退出应用，将 Assets 中的 **StickyNotes-win-x64.zip** 解压到另一文件夹，运行 **Install.cmd**。笔记、设置和备份会保留。免安装使用时运行 `StickyNotes.exe`，并保留全部附带文件。无需管理员权限或另装 .NET。应用未签名，可使用附带的 `.zip.sha256` 验证 SHA256。

界面和文档支持日语、英语及简体中文。Google 集成为可选功能，需要自行配置 OAuth。自动测试不连接或修改真实 Google 账号。

[使用指南](https://github.com/AvocadoWasabi/StickyNotes/blob/v0.0.6/README.zh-CN.md) · [Calendar / Tasks 设置与限制](https://github.com/AvocadoWasabi/StickyNotes/blob/v0.0.6/docs/GOOGLE-CALENDAR.zh-CN.md#tasks) · [更新日志](https://github.com/AvocadoWasabi/StickyNotes/blob/v0.0.6/CHANGELOG.zh-CN.md)
