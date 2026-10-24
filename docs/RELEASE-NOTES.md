# Markdown Sticky Notes v0.0.11

[日本語](#日本語) | [English](#english) | [简体中文](#简体中文)

## 日本語

Windows 10 / 11（x64）向け。v0.0.10以降の変更です。

- **9言語UI**：日本語・英語・簡体字中国語に、台湾華語（繁体字）・韓国語・ドイツ語・ロシア語・スペイン語・スウェーデン語を追加。設定で選択し、保存後の再起動で反映します。
- **READMEと導入資料の刷新**：特長・必要ツールと設定・機能一覧を中心に9言語で再構成。UI対応後に各言語の画面例27枚を再生成しました。
- **複数Vaultのデイリーノート**：接続・デイリー設定・表示名・既定Vaultを管理。既存設定はメインに移行し、固定IDで各ノートの接続先を保持。Vault名／IDを指定したCLIフォルダ取得とデイリー設定の確認に対応します。
- **Tasks 8.2.2対応**：グループ化・入れ子の検索結果と元タスクの対応を保持。連携には安定版8.2.2以降が必要です。旧版は書き込み前に停止し、対象Vaultでの更新を案内します。
- **付箋管理とメニュー**：確認付き削除、復旧ファイル、下書き／競合保護、閉じるボタン／Escを追加。操作メニューを整理し、Markdownファイル名を付箋タイトルとして表示。YAMLタイトルはメタデータとして保持します。
- **CLI長さ上限エラーを修正**：大きなフォルダ操作要求をアクセス制限・ハッシュ検証付き一時ファイルで転送。「一覧を更新」で初回取得の失敗も再試行できます。

### 更新と検証

編集を保存し、通知領域からアプリを終了してください。Assetsの **StickyNotes-win-x64.zip** を新しいフォルダに全展開し、**Install.cmd** を実行します。ノート・設定・バックアップは保持され、.NET導入や管理者権限は不要です。未署名アプリです。**StickyNotes-win-x64.zip.sha256** で照合できます。

Obsidian連携はObsidianの起動とCLI有効化が必要です。Tasksを使う各Vaultで **設定 → コミュニティプラグイン → アップデートを確認** から更新し、付箋の **連携データの更新 → Tasks / Dataviewを更新** を実行します。Obsidianなしで使う場合はこのアプリのCLI設定を無効にしてください。

自動テスト、模擬Obsidian API、9言語のWPF画面、配布ビルド、ZIP内容・SHA256、コード／セキュリティレビューで確認。実アカウントのGoogle操作、すべてのWindowsテーマ・DPI・物理IME、母語話者による校正は今回の検証対象外です。

### 制限

詳細資料は主に日英簡体字の3言語です。画面例は架空ノートを実WPFで描画したもので、デスクトップ撮影ではありません。Windows標準ダイアログ・外部ページ・ライブラリの文言は別言語の場合があります。既存ノート本文は翻訳しません。通常の新規付箋・保存先移行はメインVaultを使い、Vault間移行は追加していません。CLIは共通キュー・タイムアウトを使い、将来のObsidian／Tasks内部API変更には追加対応が必要になる場合があります。競合書き込みは拒否し、デイリーノートは自動作成しません。

[導入手順](https://github.com/AvocadoWasabi/StickyNotes/blob/v0.0.11/docs/INSTALL.md) · [変更履歴](https://github.com/AvocadoWasabi/StickyNotes/blob/v0.0.11/CHANGELOG.ja.md) · [画面例](https://github.com/AvocadoWasabi/StickyNotes/blob/v0.0.11/docs/SCREENSHOTS.md)

## English

For Windows 10 / 11 (x64). Changes since v0.0.10.

- **Nine-language UI:** Add Taiwanese Mandarin (Traditional Chinese), Korean, German, Russian, Spanish and Swedish alongside Japanese, English and Simplified Chinese. Choose a language in Settings, save and restart.
- **Rewritten README and onboarding:** Explain benefits, required tools/settings and features in nine languages. Regenerate 27 screen examples after implementing the UI translations.
- **Daily notes across multiple vaults:** Manage connections, daily settings, names and a default vault. Existing settings become Main; stable IDs preserve each note’s source. Retrieve the folder through CLI using the entered vault name/ID and inspect native daily settings.
- **Tasks 8.2.2 support:** Preserve source mappings for grouped and nested query results. Integration requires stable 8.2.2 or later. Older versions stop before writing and show update instructions for the affected vault.
- **Note management and menus:** Add confirmed deletion, recovery files, draft/conflict guards and Close/Esc. Organize actions and display Markdown filenames as note titles, retaining YAML titles as metadata.
- **CLI length-limit fix:** Transfer oversized folder requests through access-restricted temporary files with hash verification. Refresh list also retries initial loading after failure.

### Updating and validation

Save edits and exit from the notification area. Extract the complete **StickyNotes-win-x64.zip** from Assets into a new folder and run **Install.cmd**. Notes, settings and backups are retained. No separate .NET installation or administrator access is required. The app is unsigned. Verify with **StickyNotes-win-x64.zip.sha256**.

Keep Obsidian running with CLI enabled for integration. In every vault using Tasks, open **Settings → Community plugins → Check for updates**, update Tasks, then choose **Refresh linked data → Refresh Tasks / Dataview** in the sticky-note menu. Disable the app’s CLI setting to use local files without Obsidian.

Validation covers automated tests, synthetic Obsidian APIs, WPF views in nine languages, distribution builds, ZIP contents/SHA256, and code/security reviews. Real-account Google operations, every Windows theme/DPI/physical IME combination, and native-speaker proofreading were outside this validation.

### Limits

Detailed references remain mainly in Japanese, English and Simplified Chinese. Screen examples render fictional notes using actual WPF controls; they are not desktop captures. Windows dialogs, external pages and library messages may use another language. Existing note text is not translated. Ordinary new notes and storage migration use Main; cross-vault file migration is not added. CLI retains its shared queue and timeout; future Obsidian/Tasks internal API changes may require updates. Conflicting writes are rejected and daily notes are not created automatically.

[Setup](https://github.com/AvocadoWasabi/StickyNotes/blob/v0.0.11/docs/INSTALL.en.md) · [Changelog](https://github.com/AvocadoWasabi/StickyNotes/blob/v0.0.11/CHANGELOG.md) · [Screenshots](https://github.com/AvocadoWasabi/StickyNotes/blob/v0.0.11/docs/SCREENSHOTS.en.md)

## 简体中文

适用于Windows 10 / 11（x64）。以下为v0.0.10之后的变化。

- **九种界面语言**：除日语、英语和简体中文外，新增台湾华语（繁体）、韩语、德语、俄语、西班牙语和瑞典语。在设置中选择，保存并重启后生效。
- **重写README与入门资料**：以特点、工具与设置、功能列表为重点，提供九种语言。先实现界面翻译，再生成27张画面示例。
- **多个Vault的每日笔记**：分别管理连接、每日设置、名称和默认Vault。旧设置成为主Vault，以固定ID保持各笔记来源。支持按输入的Vault名称／ID通过CLI获取文件夹并检查原生每日设置。
- **Tasks 8.2.2支持**：保持分组、嵌套结果与源任务的映射。集成要求稳定版8.2.2以上。旧版在写入前停止，并提示在对应Vault更新插件。
- **便签管理和菜单**：新增确认删除、恢复文件、草稿／冲突保护以及关闭／Esc。整理操作菜单，用Markdown文件名作为便签标题，YAML标题仍保留为元数据。
- **修复CLI长度超限**：通过限制访问并校验哈希的临时文件传递过大的文件夹请求。刷新列表也可重试初次加载失败。

### 更新与验证

保存编辑并从通知区域退出，将Assets中的 **StickyNotes-win-x64.zip** 全部解压到新目录后运行 **Install.cmd**。笔记、设置和备份保留，无需另装.NET或管理员权限。应用未签名，可通过 **StickyNotes-win-x64.zip.sha256** 校验。

Obsidian集成需要保持Obsidian运行并启用CLI。在每个使用Tasks的Vault中，通过 **设置 → 第三方插件 → 检查更新** 更新Tasks，然后选择便签菜单的 **刷新关联数据 → 刷新 Tasks / Dataview**。不使用Obsidian时，请关闭本应用的CLI设置。

验证涵盖自动测试、模拟Obsidian API、九种语言WPF画面、分发构建、ZIP内容／SHA256以及代码和安全审查。本次未验证真实账号的Google操作、所有Windows主题／DPI／物理输入法组合，也未经母语使用者校对。

### 限制

详细资料主要保留日英简体三种语言。画面示例通过实际WPF控件渲染虚构笔记，并非桌面截图。Windows对话框、外部网页和库的文案可能使用其他语言。不会翻译现有正文。普通新便签和存储迁移仍使用主Vault，未新增跨Vault迁移。CLI保留共享队列和超时；将来Obsidian／Tasks内部API变化可能需要更新。冲突写入会被拒绝，每日笔记不会自动创建。

[安装指南](https://github.com/AvocadoWasabi/StickyNotes/blob/v0.0.11/docs/INSTALL.zh-CN.md) · [更新日志](https://github.com/AvocadoWasabi/StickyNotes/blob/v0.0.11/CHANGELOG.zh-CN.md) · [界面示例](https://github.com/AvocadoWasabi/StickyNotes/blob/v0.0.11/docs/SCREENSHOTS.zh-CN.md)
