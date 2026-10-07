[English](#english) | [日本語](#日本語) | [简体中文](#简体中文)

# Task editing / タスク入力 / 任务输入

## English

**Settings → Task input completion** offers **Automatic / ON / OFF**. Automatic is the default for new and existing settings: completion starts only after CLI confirms that Tasks is enabled in the selected Obsidian vault. Missing/disabled Tasks, connection failures and local mode keep it off until you select ON manually. ON works without Tasks; OFF always hides suggestions. The choice persists. Detection is asynchronous when the editor receives focus; pending/obsolete responses cannot enable a newer session. Candidate generation makes no per-keystroke CLI calls. Enter checklist continuation and checkbox operations are separate from this setting.

Current source builds offer lightweight completion inside unchecked Markdown task lines (`- [ ] `, `* [ ] `, `+ [ ] ` and numbered lists). Type `due`, `scheduled`, `start`, `created`, `high`, `highest`, `medium`, `low`, `lowest`, `repeat` or `every week`. For example, `- [ ] Buy milk du` offers `📅`; after inserting it, type `tomorrow` to insert tomorrow's local date. Date choices include `today`, `tomorrow`, `next week`, `next month` and `next year`. Recurrence choices include every day/week/month/year and, after `🔁`, `every week when done`.

Use ↑/↓ to select and Enter/Tab or a mouse click to insert. Concrete keyword matches select the first candidate. Menus shown after an empty date/recurrence field have no selection, so Enter continues the list; Tab chooses the first candidate. Esc first dismisses suggestions; the next Esc saves and exits editing. Japanese/Chinese IME composition retains its keys. Insertion is one undo step. Code blocks, inline code, completed tasks and selected text do not receive suggestions. Existing date/priority markers are not duplicated.

Enter within a task creates an unchecked item, retaining indentation and bullet style (numbered lists increment). Enter on an empty task removes its list marker. Shift+Enter retains the editor's normal behavior. Suggestions run locally without CLI requests. These are fixed English keywords and Tasks emoji syntax, independent of Tasks global-filter/autocomplete settings; they do not reproduce arbitrary natural-language dates, dependency search, custom statuses or Dataview field syntax. For full Tasks editing, open the note in Obsidian.

In CLI mode, ordinary body checkboxes now use the installed Tasks **public toggle API** (Tasks 7.2+) for its completion dates, status transitions and recurrence output. Tasks returns the changed text; Sticky Notes uses the existing whole-note conflict check and backup save, including linked sections. If Tasks is absent, ordinary checkboxes remain simple Markdown toggles. If an installed Tasks plugin lacks the API or fails, the edit is rejected rather than silently losing native behavior. Local mode uses simple toggles. Query-result checkboxes retain their [existing provider-specific behavior](QUERY-EDITING-AND-BROWSER.md#english). WPF still renders standard Markdown checkboxes; custom status symbols are not a full Tasks editor.

Architecture: `src/StickyNotes/TaskEditing/` contains pure `TaskInput` rules, `TaskEditorBehavior` for popup/keys/IME, `NoteSource.Tasks` for body transformation and `TaskCheckboxBridge.js` for the public API adapter. `NoteWindow` only attaches the behavior and delegates checks; existing CLI transport and save code remain shared. Tests cover context, dates, keyboard/undo/IME, linked sections and conflicts; `--query-task-smoke` also executes the generated adapter in Node.js with a simulated plugin. Live Obsidian validation is not included. `./build.ps1 -Publish` packages this guide in all three languages; install using `artifacts/app/Install.cmd`. Published v0.0.7 assets are unchanged.

## 日本語

**設定 → タスク入力補完**で **自動／ON／OFF** を選べます。新規・既存設定の既定は自動で、CLIにより対象Obsidian VaultのTasksが有効と確認できた場合のみ補完を開始します。Tasks未導入・無効・接続確認失敗・ローカル方式では、手動でONにするまで候補を表示しません。ONはTasksなしでも利用でき、OFFは常に候補を非表示にします。選択は保存されます。確認はエディターがフォーカスを受けた際に非同期で行い、古い応答は新しい編集状態を有効化しません。候補生成に入力ごとのCLI通信はありません。Enterでのチェックリスト継続・チェック操作はこの設定と独立しています。

現在のソースビルドは、未完了のMarkdownタスク行（`- [ ] `・`* [ ] `・`+ [ ] `・番号付きリスト）で軽量な入力補完を利用できます。`due`・`scheduled`・`start`・`created`・`high`・`highest`・`medium`・`low`・`lowest`・`repeat`・`every week`などを入力します。例：`- [ ] 牛乳を買う du`で`📅`を挿入し、続けて`tomorrow`でPCの翌日の日付を挿入できます。日付候補は`today`・`tomorrow`・`next week`・`next month`・`next year`。繰り返しは毎日／週／月／年に加え、`🔁`の後で`every week when done`を選べます。

↑↓で選択し、Enter／Tabまたは候補クリックで確定します。入力したキーワードに一致する候補は先頭を選択します。日付・繰り返し記号の直後の空欄では未選択にし、Enterは次のチェック項目を作成、Tabは先頭候補を確定します。Escは先に候補を閉じ、次のEscで保存して編集終了します。日本語・中国語のIME変換中は変換キーを優先します。候補挿入は1回のUndoで戻せます。コードブロック・インラインコード・完了済みタスク・範囲選択中には補完しません。既存の日付・優先度記号は重複追加しません。

タスク行のEnterは、字下げ・箇条書き記号を引き継いで未チェックの項目を作ります（番号付きは加算）。空のタスク行でEnterを押すとリスト記号を取り除きます。Shift+Enterは通常のエディター動作です。補完はローカルで行い、入力ごとのCLI通信はありません。固定の英語キーワードとTasks絵文字形式に対応し、Tasksのグローバルフィルター・補完設定とは独立しています。任意の自然言語日付・依存タスク検索・カスタム状態・Dataviewフィールド形式の完全再現はしません。本家の編集機能が必要な場合はObsidianでノートを開いてください。

CLI有効時は、通常の本文チェックボックスもTasksの**公開切替API**（Tasks 7.2以降）を利用し、完了日・状態遷移・繰り返し出力を反映します。本家から変更後の文字列を受け取り、従来のノート全体の競合検出・バックアップ付き保存を通します。見出し連携にも適用します。Tasks未導入なら通常のMarkdown切替を維持します。導入済みTasksのAPIが未対応・失敗した場合は、単純な切替で代用せず変更を拒否します。ローカル方式は単純な切替です。検索結果のチェックは[従来のプロバイダー別動作](QUERY-EDITING-AND-BROWSER.md#日本語)を維持します。WPFのチェックボックス描画は標準Markdownの範囲で、カスタム状態を含むTasksエディター全体ではありません。

設計：`src/StickyNotes/TaskEditing/`に、純粋な入力規則`TaskInput`、候補UI・キー・IMEの`TaskEditorBehavior`、本文変換の`NoteSource.Tasks`、公開API連携の`TaskCheckboxBridge.js`を集約します。`NoteWindow`は接続と委譲だけを担当し、既存のCLI転送・保存処理を共用します。テストは入力文脈・日付・キー／Undo／IME・見出し連携・競合を確認し、`--query-task-smoke`では生成したアダプターをNode.jsの模擬プラグインで実行します。起動中のObsidianとの実機検証は含みません。`./build.ps1 -Publish`で3言語の本資料を同梱し、`artifacts/app/Install.cmd`で導入できます。公開済みv0.0.7の配布物は変更しません。

## 简体中文

**设置 → 任务输入补全**提供 **自动／ON／OFF**。新旧配置均默认为自动，仅在CLI确认目标Obsidian仓库已启用Tasks后显示补全。Tasks未安装、禁用、连接失败及本地模式下，在手动选择ON前不显示候选。ON无需Tasks，OFF始终关闭候选，选择会保存。编辑器获得焦点时异步检查，过期响应不会启用新的编辑会话；候选生成不逐键调用CLI。Enter继续列表和复选框操作独立于此设置。

当前源码构建在未完成的 Markdown 任务行（`- [ ] `、`* [ ] `、`+ [ ] `及有序列表）中提供轻量补全。输入 `due`、`scheduled`、`start`、`created`、`high`、`highest`、`medium`、`low`、`lowest`、`repeat` 或 `every week`。例如 `- [ ] 买牛奶 du` 可插入 `📅`，随后输入 `tomorrow` 插入电脑本地明天的日期。日期候选包括 `today`、`tomorrow`、`next week`、`next month`、`next year`；重复规则包括每天／周／月／年，在 `🔁` 后还可选择 `every week when done`。

↑↓ 选择，Enter／Tab 或点击候选插入。具体关键字匹配默认选择第一项；日期／重复标记后的空字段不默认选择，Enter 继续列表，Tab 选择第一项。Esc 先关闭候选，下次 Esc 保存并退出编辑。日文／中文输入法组合期间保留输入法按键。一次 Undo 撤销一次插入。代码块、行内代码、已完成任务和选中文本不提供补全；不重复插入已有日期／优先级标记。

任务行中的 Enter 创建未勾选项，保留缩进和列表符号（有序列表递增）。空任务上的 Enter 移除列表标记。Shift+Enter 保持编辑器通常行为。补全在本地运行，不逐键调用 CLI。使用固定英文关键字和 Tasks 表情格式，独立于 Tasks 全局过滤器／补全设置；不完整复现任意自然语言日期、依赖任务搜索、自定义状态及 Dataview 字段格式。完整 Tasks 编辑请在 Obsidian 中打开笔记。

CLI 模式下，普通正文复选框也使用已安装 Tasks 的**公开切换 API**（Tasks 7.2+），应用完成日期、状态转换和重复任务输出。接收修改后的文本后，仍通过整篇笔记冲突检查和备份保存，支持标题片段关联。未安装 Tasks 时保持普通 Markdown 切换；已安装插件缺少 API 或执行失败时拒绝修改，不静默降级。本地模式使用简单切换。查询结果复选框保持[原有的各提供方行为](QUERY-EDITING-AND-BROWSER.md#简体中文)。WPF 仍渲染标准 Markdown 复选框，不是包含自定义状态的完整 Tasks 编辑器。

设计：`src/StickyNotes/TaskEditing/` 集中管理纯输入规则 `TaskInput`、候选界面／按键／输入法 `TaskEditorBehavior`、正文转换 `NoteSource.Tasks`、公开 API 适配 `TaskCheckboxBridge.js`。`NoteWindow` 只连接并委托功能，复用已有 CLI 传输和保存流程。测试覆盖上下文、日期、按键／Undo／输入法、标题片段及冲突；`--query-task-smoke` 在 Node.js 中用模拟插件执行生成的适配器。尚未在运行中的 Obsidian 上实机验证。`./build.ps1 -Publish` 将本指南三种语言打包，使用 `artifacts/app/Install.cmd` 安装。已发布 v0.0.7 资源保持不变。
