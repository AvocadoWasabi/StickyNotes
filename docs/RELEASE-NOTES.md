[English](#english) | [日本語](#日本語) | [简体中文](#简体中文)

# Markdown Sticky Notes v0.0.3

## 日本語

Windows 10 / 11（x64）向け。デイリーノートの待機表示、付箋の表示スケール、タイトル上の操作ボタンを追加しました。

### 主な変更

- 今日のデイリーノートが未作成の場合、Obsidianで作成すると自動表示される案内を付箋内に表示します。設定で「今日の分が作成されるまで」または「再読込するまで」昨日のノートを表示できます。昨日の表示には対象日と保存先の案内が付き、未保存の編集を保護します。
- 付箋ごとに表示スケールを50～200%で保存できます。「…」の表示スケール、閲覧中のCtrl＋ホイール／Ctrl＋＋／Ctrl＋－で変更し、Ctrl＋0で100%に戻します。Markdown本文・予定・待機案内が拡大縮小されます。
- 設定「タイトルにマウスカーソルを重ねるとボタンを表示する」で、操作ボタンをタイトルに重ねて表示できます。ドラッグ用の持ち手とF6／Tabによるキーボード操作にも対応します。
- 通常のビルドでも `artifacts/app/Install.cmd` を生成し、すぐにローカルインストールできます。`-Publish` はZIPとSHA256も生成します。
- 英語・日本語・簡体字中国語の資料を同梱し、ObsidianとGoogle Calendarの併用例を追加しました。アプリの画面・操作ボタンは日本語です。

### インストール・更新

1. 更新の場合は通知領域の「終了」でアプリを終了します。
2. Assetsの **StickyNotes-win-x64.zip** をダウンロードし、別のフォルダへ「すべて展開」します。
3. **Install.cmd** を実行し、スタートメニューの **Markdown Sticky Notes** から起動します。

管理者権限や.NETの追加インストールは不要です。展開先の `StickyNotes.exe` から直接起動することもできます。EXE以外の同梱ファイルも保持してください。既存のノート・設定・バックアップは保持されます。

新しい設定の初期値は「今日の分を待機」「表示スケール100%」「操作ボタンを常時表示」です。デイリーノートを新しく連携する際は、今日の一致ファイルが必要です。「再読込まで」の保持状態は付箋を開いている間だけ有効です。0.0.1から更新する場合は、使い方のフォルダ・日付形式の移行案内も確認してください。

アプリは未署名です。Google Calendarは任意で、利用者自身のOAuth設定が必要です。SHA256は `StickyNotes-win-x64.zip.sha256` に記載しています。

[使い方](https://github.com/AvocadoWasabi/StickyNotes/blob/v0.0.3/README.ja.md) · [変更履歴](https://github.com/AvocadoWasabi/StickyNotes/blob/v0.0.3/CHANGELOG.ja.md)

## English

For **Windows 10 / 11 (x64)**. This release adds daily-note waiting guidance, note display scaling, and title-hover controls.

### Changes

- When today's daily note is missing, show guidance explaining that creating it in Obsidian displays it automatically. Settings can retain yesterday's note until today's is created or until Reload. Retained notes show their date and save destination, and unsaved edits are protected.
- Save a display scale of 50–200% for each note. Use Display scale under More, Ctrl+wheel / Ctrl++ / Ctrl+- while reading, or Ctrl+0 to reset to 100%. Markdown, events, and waiting guidance scale together.
- Enable `タイトルにマウスカーソルを重ねるとボタンを表示する` (Show buttons when hovering the mouse cursor over the title) to overlay controls on the title. A drag handle and F6/Tab keyboard access are available.
- Normal builds generate `artifacts/app/Install.cmd` for immediate local installation. `-Publish` additionally creates the ZIP and SHA256 checksum.
- Bundle English, Japanese, and Simplified Chinese documentation, including examples of using Obsidian and Google Calendar together. The app controls remain in Japanese.

### Installation and updates

1. If updating, exit the app using `終了` (Exit) in its notification-area menu.
2. Download **StickyNotes-win-x64.zip** under Assets and extract the entire ZIP to a separate folder.
3. Run **Install.cmd**, then open **Markdown Sticky Notes** from the Start menu.

No administrator privileges or separate .NET installation are required. For portable use, run `StickyNotes.exe` from the extracted folder and keep all accompanying files. Existing notes, settings, and backups are retained.

New settings default to waiting for today's note, 100% display scale, and always-visible controls. Linking a new daily sticky note requires today's matching file. Retention until Reload lasts only while the note stays open. When updating from 0.0.1, also check the user guide for folder and date-format migration instructions.

The app is unsigned. Google Calendar is optional and requires your own OAuth configuration. The SHA256 checksum is provided in `StickyNotes-win-x64.zip.sha256`.

[User guide](https://github.com/AvocadoWasabi/StickyNotes/blob/v0.0.3/README.md) · [Changelog](https://github.com/AvocadoWasabi/StickyNotes/blob/v0.0.3/CHANGELOG.md)

## 简体中文

面向 **Windows 10 / 11（x64）**。本版本添加每日笔记等待提示、便签显示缩放和标题悬停操作按钮。

### 主要变化

- 今天的每日笔记尚未创建时，在便签内提示：在 Obsidian 创建后会自动显示。可在设置中保留昨天的笔记，直到今天的文件创建或点击重新加载。保留的笔记会标明日期和保存目标，并保护未保存的编辑。
- 每张便签可保存50～200%的显示比例。通过更多菜单中的显示缩放、阅读时的 Ctrl＋滚轮／Ctrl＋＋／Ctrl＋－调整，Ctrl＋0恢复100%。Markdown 正文、日程和等待提示一起缩放。
- 启用 `タイトルにマウスカーソルを重ねるとボタンを表示する`（鼠标悬停在标题上时显示按钮），即可将按钮叠加显示在标题上。支持拖动手柄和 F6／Tab 键盘操作。
- 普通构建也生成 `artifacts/app/Install.cmd`，可立即本地安装。`-Publish` 还生成 ZIP 和 SHA256 校验文件。
- 随附英语、日语和简体中文文档，添加 Obsidian 与 Google Calendar 的组合使用示例。应用界面控件仍为日语。

### 安装与更新

1. 更新前，从通知区域菜单选择 `終了`（退出）。
2. 下载 Assets 中的 **StickyNotes-win-x64.zip**，完整解压到另一个文件夹。
3. 运行 **Install.cmd**，从开始菜单启动 **Markdown Sticky Notes**。

无需管理员权限或单独安装 .NET。免安装使用可直接运行解压目录中的 `StickyNotes.exe`，但须保留所有附带文件。已有笔记、设置和备份会保留。

新设置默认等待今天的笔记、显示比例100%、始终显示操作按钮。新建每日便签关联时，需要今天的匹配文件。“保留至重新加载”的状态仅在便签保持打开期间有效。从0.0.1更新时，还请查看使用指南中的文件夹和日期格式迁移说明。

应用未签名。Google Calendar 为可选功能，需要用户自行配置 OAuth。SHA256 位于 `StickyNotes-win-x64.zip.sha256`。

[使用指南](https://github.com/AvocadoWasabi/StickyNotes/blob/v0.0.3/README.zh-CN.md) · [更新日志](https://github.com/AvocadoWasabi/StickyNotes/blob/v0.0.3/CHANGELOG.zh-CN.md)
