# Markdown Sticky Notes v0.0.9

[日本語](#日本語) | [English](#english) | [简体中文](#简体中文)

## 日本語

Windows 10 / 11（x64）向け。v0.0.8以降の変更をまとめ、別作業ブランチのチェック切替ショートカットも統合しました。

- **見出しフィルター**：付箋ごとに見出しを検索・複数選択して保存。子見出しを含め原文順に表示し、原文は変更しません。単一選択は見出し名、複数は件数を表示。改名・削除・重複時は通知し、別の内容へ自動で切り替えません。
- **コンパクトな表示**：操作欄をタイトルと同じ1行へ整理。編集・再読込・新規・閉じるは「…」、保存は編集中だけ表示します。淡い配色、詳細情報の開閉、成功通知の一時表示、右揃えの見出し選択画面を追加。
- **タスクの読みやすさ**：行間を縮め、折り返しを本文の左端に整列。重複する箇条書きの点を除去し、空行付きリスト・子リスト・表の余白を調整。チェックボックスはWindowsテーマの自然幅を使い、右端の見切れを修正しました。
- **Alt+Z**：全付箋を10秒間最前面へ。再入力で延長し、終了後は各付箋の固定設定へ戻ります。他アプリとのキー登録競合は通知します。
- **編集時のチェック切替**：設定で任意のショートカット（例：Ctrl+Enter）を登録できます。既定は未設定。カーソル行の通常チェックだけを切り替え、Undo・通常保存に対応します。Alt+Zと標準編集キーは予約済みです。Tasksの繰り返し・完了日処理は行いません。
- **資料**：GitHubの既定READMEを日本語にし、英語・簡体字中国語と利用手順を同期しました。

### 更新と確認範囲

アプリを終了し、Assetsの **StickyNotes-win-x64.zip** を新しいフォルダへ全展開して **Install.cmd** を実行してください。ノート・設定・バックアップは保持されます。管理者権限や別途.NET導入は不要です。未署名のため、必要に応じて **StickyNotes-win-x64.zip.sha256** で照合できます。

自動テスト、配布ビルド、ZIP内容・ハッシュ、コード／セキュリティレビューを実施。280／360幅のWPF描画画像で本文を確認しました。デスクトップ画面取得の不具合により、最終版の全画面・ポップアップの実機目視確認には制限があります。実環境のObsidian・物理IME・別Windowsテーマは今回再検証していません。CLIの既知の制約と以前観測された一時的な保存競合は継続し、競合時は上書きを拒否します。フィルターは文書直下の見出しを対象とし、既存の見出しリンク付き付箋ではリンク範囲内に適用します。

[利用手順](https://github.com/AvocadoWasabi/StickyNotes/blob/v0.0.9/docs/USAGE.md) · [チェック切替と制限](https://github.com/AvocadoWasabi/StickyNotes/blob/v0.0.9/docs/TASK-EDITING.md#日本語) · [変更履歴](https://github.com/AvocadoWasabi/StickyNotes/blob/v0.0.9/CHANGELOG.ja.md)

## English

For Windows 10 / 11 (x64). Includes all changes since v0.0.8 and the editing checkbox shortcut from a separate development branch.

- **Heading filters:** Search, select multiple headings and persist the selection per note. Include child sections in source order without modifying Markdown. Show the selected name or count. Missing, renamed or ambiguous headings show a notice instead of falling back to other content.
- **Compact controls:** Place controls on one title row. Edit, Reload, New and Close move into More; Save appears while editing. Add pale colors, collapsible details, transient success notices and a right-aligned heading picker.
- **Readable tasks:** Tighten spacing, align wrapped text, remove redundant task bullets and adjust loose lists, nesting and tables. Native Windows checkbox sizing fixes clipped right edges.
- **Alt+Z:** Bring all notes to the foreground for ten seconds; repeat to extend. Restore each pin setting afterward. Notify on registration conflicts with other apps.
- **Editing checkbox shortcut:** Assign a shortcut such as Ctrl+Enter in Settings; unassigned by default. Toggle an ordinary checkbox on the caret line with undo and normal saving. Alt+Z and standard editing keys are reserved. This action does not process Tasks recurrence or completion dates.
- **Documentation:** Make Japanese the default GitHub README and synchronize English, Japanese and Simplified Chinese guides.

### Updating and validation

Exit the app, extract the complete **StickyNotes-win-x64.zip** from Assets into a new folder and run **Install.cmd**. Notes, settings and backups are retained. No administrator privileges or separate .NET installation are required. The app is unsigned; verify with **StickyNotes-win-x64.zip.sha256** when needed.

Validation covers automated tests, the distribution build, ZIP contents/checksum, and code/security reviews. WPF-generated images verify body layout at widths 280/360. Desktop capture failures limited final live-screen and popup visual checks. Live Obsidian, physical IME input and other Windows themes were not revalidated. Existing CLI limits and previously observed transient save conflicts remain; conflicting writes are refused. Filters target document-level headings and remain within the existing section for heading-linked notes.

[User guide](https://github.com/AvocadoWasabi/StickyNotes/blob/v0.0.9/docs/USAGE.en.md) · [Checkbox shortcut and limits](https://github.com/AvocadoWasabi/StickyNotes/blob/v0.0.9/docs/TASK-EDITING.md#english) · [Changelog](https://github.com/AvocadoWasabi/StickyNotes/blob/v0.0.9/CHANGELOG.md)

## 简体中文

适用于 Windows 10 / 11（x64）。汇总 v0.0.8 之后的更新，并整合另一开发分支中的编辑复选框快捷键。

- **标题筛选**：搜索、多选标题并按便签保存。按原文顺序包含子章节，不修改 Markdown。显示选定名称或数量；标题改名、删除或重复时提示，不自动显示其他内容。
- **紧凑控件**：控件与标题共用一行。编辑、重新加载、新建、关闭移到更多菜单，保存仅在编辑时显示。新增淡色、可折叠详情、临时成功提示及右对齐标题面板。
- **任务可读性**：缩小行距、对齐折行、移除多余圆点，并调整空行列表、嵌套及表格间距。使用 Windows 主题的自然复选框宽度，修复右侧裁切。
- **Alt+Z**：将所有便签置顶十秒，再按可延长，之后恢复各自置顶设置。与其他应用注册冲突时提示。
- **编辑复选框快捷键**：在设置中指定如 Ctrl+Enter 的按键，默认未设置。切换光标行的普通复选框，支持撤销和正常保存。Alt+Z及标准编辑键保留。此操作不处理 Tasks 重复任务或完成日期。
- **文档**：GitHub 默认 README 改为日语，并同步日语、英语及简体中文指南。

### 更新与验证

退出应用，将 Assets 中的 **StickyNotes-win-x64.zip** 全部解压到新目录并运行 **Install.cmd**。保留笔记、设置和备份，无需管理员权限或另装.NET。应用未签名，可通过 **StickyNotes-win-x64.zip.sha256** 校验。

验证涵盖自动测试、分发构建、ZIP内容与校验和、代码及安全审查。使用WPF生成图像检查280／360宽度的正文布局。桌面截图故障限制了最终实机整窗和弹出面板的目视检查。本次未重新验证真实Obsidian、物理输入法及其他Windows主题。现有CLI限制和此前观察到的临时保存冲突仍存在，冲突写入会被拒绝。筛选仅针对文档顶层标题，已有标题链接便签仍限于其链接章节。

[使用指南](https://github.com/AvocadoWasabi/StickyNotes/blob/v0.0.9/docs/USAGE.zh-CN.md) · [快捷键及限制](https://github.com/AvocadoWasabi/StickyNotes/blob/v0.0.9/docs/TASK-EDITING.md#简体中文) · [更新日志](https://github.com/AvocadoWasabi/StickyNotes/blob/v0.0.9/CHANGELOG.zh-CN.md)
