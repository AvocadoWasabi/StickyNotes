[English](#english) | [日本語](#日本語) | [简体中文](#简体中文)

# Markdown Sticky Notes v0.0.2

## 日本語

この文書の中国語訳は0.0.2公開後に追加しました。公開済みタグ・ZIPの同梱資料は当時のままです。

Windows 10 / 11（x64）向け。Markdownの編集操作、デイリーノートの選択、保存フォルダ変更時の移行を改善しました。

### 主な変更

- 本文エリアのクリックで編集開始。編集欄と閲覧表示が重なり、文字を編集できない問題を修正しました。
- 編集欄からフォーカスが外れると、保存・破棄・編集続行を確認します。設定で確認なしの自動保存も選べます（初期状態はオフ）。
- 編集・保存・再読込・「…」をほかの操作ボタンと同じ列に配置し、背景色を分けました。
- 固定ノートとデイリーノートの選択メニューを分離。固定ノートはフォルダ／ファイル選択から開けます。
- 共通の「表示する見出し名」で既存見出しを選択できます。新しい名前は「表示」時に元ファイルの末尾へ追加し、空欄なら本文を全表示します。両画面の末尾で対象範囲をプレビューできます。
- デイリーノートは `year`・`month`・`day` の日時タグ付き正規表現に統一。設定への入力中に今日の一致ファイル名を表示します。空欄は既定例で補完し、例の挿入ボタンは置き換え前に確認します。
- 付箋の保存先とデイリーノートのフォルダが同期してしまう問題を修正。保存先を変えると、既存Markdownをサブフォルダごと移行するか確認します。移行先の既存ファイルは上書きしません。

### インストール・更新

1. 更新の場合は通知領域の「終了」でアプリを終了します。
2. Assetsの **StickyNotes-win-x64.zip** をダウンロードし、別のフォルダへ「すべて展開」します。
3. **Install.cmd** を実行し、スタートメニューの **Markdown Sticky Notes** から起動します。

管理者権限や.NETの追加インストールは不要です。インストールせず、展開先の `StickyNotes.exe` から起動することもできます。EXE以外の同梱ファイルも保持してください。既存のノート・設定・バックアップは保持されます。

**0.0.1から更新する場合:** 設定で両方のフォルダと、今日の一致ファイル名を確認してください。既存の日付書式 `yyyy-MM-dd`、`yyyy/MM/yyyy-MM-dd` と、それぞれに `(ddd)` / `(dddd)` を付けた形式は自動変換します。その他の独自書式は正規表現へ手動修正が必要です。変換だけでノートを移動・作成することはありません。保存フォルダを変更して「はい」を選ぶと旧フォルダ内のすべてのMarkdownが移行対象になるため、移行先と範囲を確認してください。

アプリは未署名で、画面・操作ボタンは日本語です。Google Calendarは任意で、利用者自身のOAuth設定が必要です。SHA256チェックサムは `StickyNotes-win-x64.zip.sha256` に記載しています。

[使い方](https://github.com/AvocadoWasabi/StickyNotes/blob/v0.0.2/README.ja.md) · [変更履歴](https://github.com/AvocadoWasabi/StickyNotes/blob/v0.0.2/CHANGELOG.ja.md)

## English

The Chinese translation of this document was added after 0.0.2 was published. The published tag and ZIP retain their original documentation.

For **Windows 10 / 11 (x64)**. This release improves Markdown editing, daily-note selection, and migration when changing the notes folder.

### Changes

- Click the note body to start editing. Fix overlapping reading and editing views that prevented text editing.
- Choose to save, discard, or keep editing when focus leaves the editor. Optional saving without confirmation is available in Settings and is off by default.
- Place Edit, Save, Reload, and More in the same row as the other note controls, with distinct background colors.
- Separate fixed-note and daily-note selection menus. Open fixed notes through folder/file pickers.
- Choose existing headings using a shared editable selector. A new name is appended to the source file on Display; a blank field displays the whole body. Preview the selected range at the end of either dialog.
- Use regular expressions with named `year`, `month`, and `day` tags for daily notes. Settings show today's matching filename while you type. Blank expressions receive a default; inserting the example asks before replacing existing text.
- Fix the notes folder and daily-note folder incorrectly sharing a value. Changing the notes folder asks whether to move existing Markdown, including subfolders. Existing destination files are never overwritten.

### Installation and updates

1. If updating, exit the app using `終了` (Exit) in its notification-area menu.
2. Download **StickyNotes-win-x64.zip** under Assets and extract the entire ZIP to a separate folder.
3. Run **Install.cmd**, then open **Markdown Sticky Notes** from the Start menu.

No administrator privileges or separate .NET installation are required. For portable use, run `StickyNotes.exe` from the extracted folder and keep all accompanying files. Existing notes, settings, and backups are retained.

**Updating from 0.0.1:** check both folders and today's matching filename in Settings. Existing date formats `yyyy-MM-dd` and `yyyy/MM/yyyy-MM-dd`, including variants with `(ddd)` / `(dddd)`, are converted automatically. Other custom formats need manual conversion to a regular expression. This conversion does not move or create notes. If you change the notes folder and choose Yes, all Markdown within the old folder is included in migration; check the destination and scope first.

The app is unsigned and its controls are currently in Japanese. Google Calendar is optional and requires your own OAuth configuration. The SHA256 checksum is provided in `StickyNotes-win-x64.zip.sha256`.

[User guide](https://github.com/AvocadoWasabi/StickyNotes/blob/v0.0.2/README.md) · [Changelog](https://github.com/AvocadoWasabi/StickyNotes/blob/v0.0.2/CHANGELOG.md)

## 简体中文

本文中文译文在 0.0.2 发布后添加；已发布标签和 ZIP 保留当时的文档。

面向 **Windows 10 / 11（x64）**。本版本改进 Markdown 编辑、每日笔记选择和更改保存文件夹时的迁移。

### 主要变化

- 点击正文即可开始编辑。修复阅读视图和编辑器重叠、无法编辑文字的问题。
- 编辑器失去焦点时询问保存、放弃或继续编辑。可在设置中开启免确认自动保存，默认关闭。
- 编辑、保存、重新加载和更多与其他操作位于同一行，采用不同背景色。
- 分离固定笔记和每日笔记选择菜单。固定笔记通过文件夹／文件选择器打开。
- 共用的可编辑标题选择框可选择已有标题。新名称在点击显示时追加到原文件末尾，空白显示整个正文；两种窗口末尾可预览所选范围。
- 每日笔记统一为带 `year`、`month`、`day` 命名日期组的正则表达式。设置中实时显示今天的匹配文件名，空白自动填默认值；插入示例前确认替换。
- 修复便签文件夹和每日笔记文件夹的值错误联动。修改保存位置时询问是否迁移已有 Markdown（包含子文件夹），不覆盖目标文件。

### 安装与更新

1. 更新前，从通知区域菜单选择 `終了`（退出）。
2. 下载 Assets 中的 **StickyNotes-win-x64.zip**，完整解压到另一个文件夹。
3. 运行 **Install.cmd**，从开始菜单启动 **Markdown Sticky Notes**。

无需管理员权限或单独安装 .NET。免安装使用可直接运行解压目录中的 `StickyNotes.exe`，但须保留所有附带文件。已有笔记、设置和备份会保留。

**从 0.0.1 更新：**请在设置中确认两个文件夹和今天的匹配文件名。旧日期格式 `yyyy-MM-dd`、`yyyy/MM/yyyy-MM-dd` 及各自带 `(ddd)` / `(dddd)` 的变体会自动转换。其他自定义格式需要手动改写为正则表达式。转换本身不移动或创建笔记。更改保存文件夹后若选择是，旧目录中所有 Markdown 均属于迁移范围，请先确认范围和目标。

应用未签名，界面控件为日语。Google Calendar 为可选功能，需要用户自行配置 OAuth。SHA256 位于 `StickyNotes-win-x64.zip.sha256`。

[中文使用指南（main）](https://github.com/AvocadoWasabi/StickyNotes/blob/main/README.zh-CN.md) · [中文更新日志（main）](https://github.com/AvocadoWasabi/StickyNotes/blob/main/CHANGELOG.zh-CN.md)
