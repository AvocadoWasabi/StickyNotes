[English](#english) | [日本語](#日本語) | [简体中文](#简体中文)

# Markdown Sticky Notes v0.0.8

## 日本語

Windows 10 / 11（x64）向け。長文保存の修正、Tasksに沿った入力補完、Dataview表示、検索結果のチェック操作、閉じた付箋の再表示を追加しました。

- **長文保存**：編集後のCLIコマンド長エラーを修正。長い要求を現在のWindowsユーザーだけが読める一時ファイルで転送・検証・削除し、Obsidian APIによる保存・バックアップ・競合検出・Unicode・改行を保持します。ノート上限は2 MBです。
- **編集操作**：シングル／ダブルクリックを選択し、クリック位置に対応するソースへカーソルを移動。Escで保存して終了します。候補表示中の最初のEscは候補を閉じ、保存失敗時は下書きを保持します。
- **Tasks入力**：本文＋スペースで候補表示。先頭の「⏎」でEnterは次項目へ、↓で選択してEnterは挿入、Tabは字下げ。本家ソースをclone・解析し、既定の候補順・部分一致・日付／繰り返し候補を合わせました。補完は既定で自動とし、Tasks未導入・無効・確認失敗・ローカル方式では手動ONまで停止します。
- **Dataviewとチェック操作**：LIST／TABLE／TASKをCLI経由で表示し、Tasksと個別に約30秒更新。照合済みのTasks／Dataview TASKチェックボックスから元ノートを更新できます。Tasksは本家の状態・繰り返し処理、Dataviewは選択したチェック記号を更新し、バックアップと競合検出を通します。
- **付箋の再表示**：「…」・常駐・タスクバーメニューの「付箋フォルダから開く…」で検索し、通常Markdownの本文／YAMLプレビューから開き直せます。選択画面ではクエリを実行しません。

### インストール・更新

アプリを終了し、Assetsの **StickyNotes-win-x64.zip** を別フォルダへ全体展開して **Install.cmd** を実行します。ノート・設定・バックアップは保持します。管理者権限や.NETの追加インストールは不要です。未署名です。SHA256は `.zip.sha256` で確認できます。

CLIを有効にしたObsidianで対象Vaultを開き、起動状態を保ってください。付箋の「設定 → Obsidian CLI」で接続し、TasksクエリにはTasks、DataviewクエリにはDataview、デイリーにはコアDaily notesを有効にします。CLIを無効にすればローカルファイルを使えます。v0.0.6以前のCLI選択がない旧設定もCLIが既定になるため、ローカル方式を続ける場合は「Obsidian CLIを使用する」を無効にして保存します。既存ファイルは自動移行しません。

### 制限と検証

補完は既定の絵文字メニュー・通常キー操作の再現です。Tasks独自設定・カスタム状態・依存関係／ID編集・自然言語日付や繰り返し規則の全解釈は未対応です。DataviewJS・インライン式・CALENDAR、ObsidianのHTML/CSSの完全再現も未対応です。元ノートを照合できないチェックボックスは無効です。

957件の自動テストと配布ビルド、コード／セキュリティレビューを実施。今回追加したDataview・チェック連携と補完は模擬API・WPF・本家テスト資料で検証し、起動中のObsidianや物理IMEでの実機検証は未実施です。内部APIの変更で動作が変わる場合があります。以前のCLI実機検証で観測した一時的なハッシュ不一致／保存競合は原因未特定です。上書きは拒否されます。下書きを控え、再読込して再試行してください。

[使い方](https://github.com/AvocadoWasabi/StickyNotes/blob/v0.0.8/README.ja.md) · [入力補完](https://github.com/AvocadoWasabi/StickyNotes/blob/v0.0.8/docs/TASK-EDITING.md#日本語) · [Dataview](https://github.com/AvocadoWasabi/StickyNotes/blob/v0.0.8/docs/DATAVIEW.md#日本語) · [チェック操作・付箋一覧](https://github.com/AvocadoWasabi/StickyNotes/blob/v0.0.8/docs/QUERY-EDITING-AND-BROWSER.md#日本語) · [変更履歴](https://github.com/AvocadoWasabi/StickyNotes/blob/v0.0.8/CHANGELOG.ja.md)

## English

For Windows 10 / 11 (x64). This release fixes long-note saves and adds Tasks-style completion, Dataview output, editable query checkboxes and a browser for closed notes.

- **Long-note saves:** Fix the CLI command-length error after editing. Oversized requests use a verified temporary transfer restricted to the current Windows user, then delete it. Saves still use Obsidian APIs with backups, conflict checks, Unicode and newline preservation. The note limit remains 2 MB.
- **Editing controls:** Choose single or double click to edit, position the caret at the corresponding source, and press Esc to save and exit. The first Esc dismisses an open menu; failed saves preserve the draft.
- **Tasks input:** A space after task text opens suggestions. Enter on the first “⏎” continues the checklist; ↓ then Enter inserts; Tab indents. Cloned upstream source and fixtures inform default order, substring matching, date and recurrence menus. Automatic is the default: missing/disabled/unreachable Tasks and local mode keep completion off until manual ON.
- **Dataview and checkboxes:** Display LIST/TABLE/TASK through CLI, refreshing independently of Tasks about every 30 seconds. Verified Tasks and Dataview TASK checkboxes update their source notes. Tasks uses native status/recurrence behavior; Dataview changes the selected checkbox only. Both retain backups and conflict checks.
- **Reopen notes:** Use “Browse sticky-note folder…” from “…” menus, tray or taskbar. Filter files and inspect ordinary Markdown body/YAML previews before reopening. The picker does not run queries.

### Installation and updates

Exit the app, extract all of **StickyNotes-win-x64.zip** from Assets into a separate folder, and run **Install.cmd**. Notes, settings and backups are retained. No administrator privileges or separate .NET installation are required. The app is unsigned; verify SHA256 with `.zip.sha256`.

Enable Obsidian CLI, open the intended vault and keep Obsidian running. Connect through “Settings → Obsidian CLI” in Sticky Notes. Enable Tasks for Tasks queries, Dataview for Dataview queries, and core Daily notes for daily notes. Disable CLI to use local files. Settings from v0.0.6 or earlier without a CLI selection also default to CLI; to keep using local files, uncheck “Use Obsidian CLI” and save. Existing files are not moved automatically.

### Limits and validation

Completion reproduces the default emoji menu and ordinary keys. Tasks preferences, custom statuses, dependency/ID edits and full natural-language date/recurrence parsing are unsupported. DataviewJS, inline expressions, CALENDAR and full Obsidian HTML/CSS rendering are unsupported. Checkboxes without verified source mappings stay disabled.

957 automated tests, a distribution build, and code/security reviews completed. New Dataview, checkbox integration and completion were checked with mocked APIs, WPF and upstream fixtures; they have not been tested with live Obsidian or physical IME input. Internal API changes may affect compatibility. Earlier live CLI tests occasionally showed transient hash mismatches/save conflicts with an unisolated cause. Overwrites are refused; preserve draft text, reload and retry.

[User guide](https://github.com/AvocadoWasabi/StickyNotes/blob/v0.0.8/README.md) · [Completion](https://github.com/AvocadoWasabi/StickyNotes/blob/v0.0.8/docs/TASK-EDITING.md#english) · [Dataview](https://github.com/AvocadoWasabi/StickyNotes/blob/v0.0.8/docs/DATAVIEW.md#english) · [Checkboxes and note browser](https://github.com/AvocadoWasabi/StickyNotes/blob/v0.0.8/docs/QUERY-EDITING-AND-BROWSER.md#english) · [Changelog](https://github.com/AvocadoWasabi/StickyNotes/blob/v0.0.8/CHANGELOG.md)

## 简体中文

支持 Windows 10 / 11（x64）。本版修复长笔记保存，新增 Tasks 风格补全、Dataview 显示、查询复选框编辑及关闭便签的浏览器。

- **长笔记保存**：修复编辑后 CLI 命令长度超限。长请求通过仅当前 Windows 用户可读的临时文件传递、验证并删除。仍使用 Obsidian API 保存，保留备份、冲突检查、Unicode 和换行；笔记上限仍为 2 MB。
- **编辑操作**：设置单击／双击编辑，光标定位到对应源码，Esc 保存并退出。候选打开时第一次 Esc 关闭候选；保存失败保留草稿。
- **Tasks 输入**：任务正文后空格显示候选，首项“⏎”上 Enter 继续列表；↓选择后 Enter 插入，Tab 缩进。克隆分析上游源码并使用其测试资料，对齐默认顺序、子串匹配、日期和重复候选。默认自动，Tasks 未安装／禁用／无法确认及本地模式下，手动 ON 前保持关闭。
- **Dataview 和复选框**：通过 CLI 显示 LIST／TABLE／TASK，与 Tasks 独立约每30秒刷新。已验证的 Tasks 和 Dataview TASK 复选框可更新原笔记。Tasks 使用原生状态／重复逻辑；Dataview 只修改选定复选框。两者保留备份和冲突检查。
- **重新打开便签**：在“…”、通知区域或任务栏菜单选择“从便签文件夹打开…”，筛选并预览普通 Markdown 正文／YAML 后打开。选择器不运行查询。

### 安装和更新

退出应用，将 Assets 的 **StickyNotes-win-x64.zip** 全部解压到另一目录，运行 **Install.cmd**。保留笔记、设置和备份。无需管理员权限或另装 .NET。应用未签名，可通过 `.zip.sha256` 验证 SHA256。

启用 Obsidian CLI，打开目标仓库并保持运行。在便签“设置 → Obsidian CLI”连接。Tasks 查询需启用 Tasks，Dataview 查询需 Dataview，日记需核心 Daily notes。禁用 CLI 可使用本地文件。v0.0.6 或更早且无 CLI 选项的旧配置也默认启用 CLI；继续本地模式时，取消“使用 Obsidian CLI”并保存。已有文件不会自动迁移。

### 限制与验证

补全仅复现默认表情菜单和常用按键。不支持 Tasks 自定义设置／状态、依赖／ID 编辑及完整自然语言日期／重复规则解析。不支持 DataviewJS、内联表达式、CALENDAR 及完整 Obsidian HTML/CSS 渲染；缺少已验证来源的复选框保持禁用。

已完成957项自动测试、分发构建和代码／安全审查。新增 Dataview、复选框集成及补全使用模拟 API、WPF 和上游测试资料验证，尚未实测运行中的 Obsidian 及物理输入法。内部 API 变化可能影响兼容性。此前 CLI 实机测试偶尔出现临时哈希不一致／保存冲突，原因未明；覆盖会被拒绝，请保留草稿后重新加载并重试。

[使用指南](https://github.com/AvocadoWasabi/StickyNotes/blob/v0.0.8/README.zh-CN.md) · [输入补全](https://github.com/AvocadoWasabi/StickyNotes/blob/v0.0.8/docs/TASK-EDITING.md#简体中文) · [Dataview](https://github.com/AvocadoWasabi/StickyNotes/blob/v0.0.8/docs/DATAVIEW.md#简体中文) · [复选框及便签列表](https://github.com/AvocadoWasabi/StickyNotes/blob/v0.0.8/docs/QUERY-EDITING-AND-BROWSER.md#简体中文) · [更新日志](https://github.com/AvocadoWasabi/StickyNotes/blob/v0.0.8/CHANGELOG.zh-CN.md)
