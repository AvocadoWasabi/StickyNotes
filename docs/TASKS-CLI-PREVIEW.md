[English](#english) | [日本語](#日本語) | [简体中文](#简体中文)

# Tasks CLI Preview — 0.0.6 experimental

## English

This separate Windows x64 test build displays the installed Obsidian Tasks plugin's own **Copy results → Markdown** output in place of `tasks` code blocks. Regular Markdown, local checkboxes and multiple query blocks coexist. It does not implement a substitute query engine and requires no additional Obsidian plugin.

### Install and try

1. Build with `./build.ps1 -Publish -TasksPreview`. Output: `artifact/tasks-cli-preview/app` and `artifact/tasks-cli-preview/StickyNotes-TasksPreview-win-x64.zip` (with SHA-256).
2. Extract the whole ZIP, then run `Install.cmd`. From the repository you can run `powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\artifact\tasks-cli-preview\app\Install.ps1 -Preview`. Alternatively, launch the folder's `StickyNotes.exe` directly.
3. Start **Markdown Sticky Notes - Tasks Preview**. The installer uses `%LOCALAPPDATA%\Programs\MarkdownStickyNotes-TasksPreview`; settings, new notes and backups use `%LOCALAPPDATA%\StickyNotes-TasksPreview`. The normal app can run alongside it. Existing settings are not copied.
4. Start Obsidian, open the target vault, enable Tasks, and enable **Settings → General → Command line interface**. Wait for Tasks to finish loading. The installed Obsidian 1.14.4 / Tasks 7.23.1 combination was used to inspect the adapter; other versions require testing.
5. In the preview app's Settings, expand **Obsidian Tasks — CLI Preview**, enable the integration, enter the absolute path to `Obsidian.com` and the vault folder. Set vault name/ID only if the folder name does not identify the correct vault. Save.
6. Use the note menu to **open an existing Markdown note** containing a `tasks` block inside that vault, or link a heading/daily note. While integration is enabled, every note must be inside the configured vault. For new notes, choose an existing folder inside that vault as the notes folder; the separate profile's default folder is outside it.
7. The entire Markdown note is fetched through CLI before its queries run; text and results then refresh approximately every 30 seconds. The reload button fetches the note again; **Refresh Tasks (preview)** refreshes its query output. Compare with Tasks' **Copy results** in Obsidian, then edit a task there and check the next refresh.

### Boundaries and failure behavior

- When enabled, all Markdown reads (including heading previews) and daily-note discovery use Obsidian CLI. Failures never fall back to local file reads. Disabling integration restores the original file access; both sources feed the same Markdown renderer. Switching modes while editing requires reloading before saving, so preserve your draft first.
- Query results are **read-only**. Normal editing, metadata edits, heading creation and ordinary checkboxes save through Obsidian's Vault API in CLI mode. New notes also use that API. Saves check the acquired text hash inside `vault.process` and back up its original text before writing. Separate settings do not copy linked notes. Saving/link confirmation can block the UI up to the CLI timeout; after a timeout, reload to check whether it completed before retrying.
- Uses the source note path and its Obsidian metadata for query context, global settings and presets. Results are never written into the note. Cached metadata may briefly lag an external edit.
- Fetches the native Markdown export, not Obsidian's HTML/CSS, interactive toolbar, edit buttons or full visual layout. Wiki links, custom statuses, tree/column layouts and hide/show options may look different in this Markdown renderer. Inspect the source in edit mode. Empty export means “no displayed results,” not a guaranteed count of zero tasks.
- Notes/results are cached **in memory only**. A failed refresh retains the last CLI snapshot/results with an error; changing the source clears the cache, and an initial failure displays an error. Background reads do not overwrite drafts or accept obsolete responses. Obsidian must be running. Closing it during a call can race with CLI's auto-launch behavior; this app does not stop it.
- One CLI request at a time, a 25-second timeout including queue wait, a 2 MB source-note limit and 10,000 vault Markdown files for discovery. At most 20 queries / 8,000 query characters per note, 200,000 output characters per block and bounded total output. Compressed requests must also fit a conservative 4,000-byte budget (serialized arguments plus a framing reserve). Oversized requests are rejected before launching CLI; long edits may need saving in Obsidian. This mitigates the Obsidian 1.14.4 main-process JSON parsing error observed with long requests; it does not repair Obsidian itself. Add `limit` to large queries. A CLI timeout does not cancel JavaScript already executing inside Obsidian.
- Uses **internal Tasks APIs**, checked at runtime. Updates can break the adapter. Errors, missing Tasks and an unready index are reported rather than treated as empty results. No tasks engine or third-party plugin binaries are distributed.
- Enable only for trusted queries/vaults: Tasks JavaScript filters, presets and global queries run with Obsidian's privileges. Queries and note text are encoded JSON data; only the bundled bridge is evaluated as code. It does not change Tasks' JavaScript settings or access the clipboard. Note operations write only when creating/saving (including backups); query export does not save results into notes.

### Development and checks

`TasksBridge.js` constructs Tasks' query renderer without starting its listeners, performs native search/Markdown export, then unloads the child. `NotesBridge.js` uses `vault.adapter.readBinary` through CLI and strict UTF-8 decoding to retain the BOM that `vault.read` strips, matching `vault.process` for hash/conflict checks and rejecting invalid encoding like the original reader. `NoteSource` applies the same BOM removal as the original reader without trimming whitespace or changing line endings. `ObsidianTasksClient` validates the vault and serializes compressed, bounded requests. Tests compare actual WPF document structure for LF/CRLF/BOM, Unicode, headings, lists, tables, links, code, trailing blanks and text surrounding queries, and cover mode isolation, late responses, failures and command limits. Optional desktop smoke test: `dotnet run --project tests/StickyNotes.Tests -c Release -p:TasksPreviewBuild=true -- --tasks-cli-smoke`. It reads an existing note for comparison, creates a uniquely named synthetic note in the active vault, checks rendering, saving, backup and conflict behavior, then deletes that fixture. It prints assertions/version only. GUI comparison of real user queries remains part of this trial.

References: [Official CLI](https://obsidian.md/help/cli), [Tasks query renderer](https://github.com/obsidian-tasks-group/obsidian-tasks/blob/7.23.1/src/Renderer/QueryResultsRenderer.ts).

Remaining trial finding (low impact): one live save reported a conflict during UTF-8 verification; four subsequent runs passed. The cause of that transient conflict was not isolated. The save was rejected rather than overwriting the note. If it recurs, preserve the draft and reload; further investigation should compare the source hash immediately before saving. CLI saves rely on Obsidian's queued Vault API and do not lock out independent external editors.

## 日本語

Windows x64向けの独立した試験版です。インストール済みObsidian Tasksの **Copy results（結果コピー）のMarkdown出力**を、付箋の`tasks`コードブロックの位置に表示します。通常のMarkdown、通常のチェックボックス、複数のクエリが同居できます。独自の互換検索エンジンや追加のObsidianプラグインは使いません。

### インストールと確認

1. `./build.ps1 -Publish -TasksPreview`でビルドします。出力は`artifact/tasks-cli-preview/app`と`artifact/tasks-cli-preview/StickyNotes-TasksPreview-win-x64.zip`（SHA-256付き）です。
2. ZIP全体を展開し、`Install.cmd`を実行します。リポジトリからは`powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\artifact\tasks-cli-preview\app\Install.ps1 -Preview`でも導入できます。フォルダ内の`StickyNotes.exe`の直接起動も可能です。
3. **Markdown Sticky Notes - Tasks Preview**を起動します。導入先は`%LOCALAPPDATA%\Programs\MarkdownStickyNotes-TasksPreview`、設定・新規ノート・バックアップは`%LOCALAPPDATA%\StickyNotes-TasksPreview`です。通常版と同時起動でき、既存設定はコピーしません。
4. Obsidianで対象Vaultを開き、Tasksと **設定 → 一般 → コマンドラインインターフェース** を有効にして、Tasksの読み込みを待ちます。呼び出し口の調査には導入済みのObsidian 1.14.4／Tasks 7.23.1を使用しました。他の版は動作確認が必要です。
5. 試験版の設定の **Obsidian Tasks — CLI Preview** で連携を有効にし、`Obsidian.com`の絶対パスとVaultフォルダを指定して保存します。フォルダ名で対象Vaultを特定できない場合はVault名／IDも指定します。
6. 付箋メニューの **既存ノートを開く** から、そのVault内の`tasks`ブロックを含むMarkdownを選択するか、見出し・デイリーノートを連携します。有効時はすべてのノートが指定Vault内にある必要があります。新規作成には、ノート保存先をVault内の既存フォルダに設定してください。独立設定の初期保存先はVault外です。
7. 最初にMarkdown全体をCLIで取得してからクエリを実行し、その後は本文・結果を約30秒ごとに取得します。再読込ボタンは本文を再取得し、**Tasksを更新（試験版）** はクエリ結果を更新します。ObsidianのTasksの **Copy results** と比較し、Obsidian側で編集した後の更新も確認してください。

### 制約とエラー時の扱い

- 有効時は、見出しプレビューを含むすべてのMarkdown読込・デイリーノート検索をObsidian CLIへ統一します。失敗時にローカル読込へ切り替えません。無効時は従来方式となり、どちらの文字列も同じMarkdown描画器へ渡します。編集中に方式を切り替えた場合は保存前に再読込が必要なので、下書きを退避してください。
- 検索結果は **読み取り専用** です。CLI有効時は通常の本文・メタデータ編集、見出し作成、通常のチェックボックス、新規ノートもObsidianのVault APIで保存します。`vault.process`内で取得時のハッシュと照合し、元の文字列をバックアップしてから書き込みます。設定の分離はノートの複製を意味しません。保存・連携確定はCLIのタイムアウトまで画面を待たせる場合があります。タイムアウト後は再読込で保存結果を確認してから再試行してください。
- 元ノートのパスとObsidianのメタデータを使い、クエリ文脈・グローバル設定・プリセットを本家に処理させます。結果を本文へ保存しません。外部編集直後はメタデータの反映が遅れることがあります。
- 取得するのは本家のMarkdown出力です。ObsidianのHTML/CSS・操作ツールバー・編集ボタン・完全な画面レイアウトは再現しません。Wikiリンク、独自ステータス、ツリー／列表示、hide/showは付箋で見え方が異なる場合があります。クエリ原文は編集モードで確認できます。空の出力は「表示結果なし」であり、タスク0件を保証しません。
- 本文・結果は **メモリ内だけ** に保持します。更新失敗時は最後にCLIで取得した本文・結果とエラーを表示し、取得元変更時はキャッシュを消去、初回失敗時はエラーを表示します。非同期取得で下書きを上書きせず、古い応答も採用しません。Obsidianの起動が必要です。実行と同時に終了するとCLI側の自動起動と競合する可能性があります。本体を終了させる処理はありません。
- CLIは同時1要求、待機込み25秒でタイムアウトします。元ノートは2 MB、検索対象のVault内Markdownは10,000ファイルまでです。1付箋あたり最大20クエリ・合計8,000文字、1ブロックの出力は最大200,000文字で、総出力も制限します。圧縮要求には保守的な4,000バイト枠（JSON化した引数と通信形式の予備領域）も適用し、超過時はCLIを起動しません。長い本文の編集はObsidianでの保存が必要な場合があります。これは長い要求で観測したObsidian 1.14.4のメインプロセスJSON解析エラーへの回避策で、本体の修正ではありません。大きいクエリには`limit`を追加してください。タイムアウトしてもObsidian内で実行中のJavaScriptは停止しません。
- **Tasksの内部API** に依存し、実行時に呼び出し口を確認します。更新により動かなくなる場合があります。構文エラー・Tasks未有効・索引準備中を空の結果とは扱いません。本家のエンジンやプラグインバイナリは同梱しません。
- 信頼できるクエリとVaultでのみ有効にしてください。TasksのJavaScriptフィルター、プリセット、グローバルクエリはObsidianの権限で動作します。本文・クエリはエンコードしたJSONデータとして渡し、コードとして評価するのは同梱ブリッジのみです。TasksのJavaScript設定の変更・クリップボード操作はしません。ノートの作成・保存時のみバックアップを含む書き込みを行い、クエリ結果は本文へ保存しません。

### 開発と検証

`TasksBridge.js`はイベント購読せずに本家の描画器を構築し、検索・Markdown出力後に解放します。`NotesBridge.js`はCLI経由の`vault.adapter.readBinary`と厳密なUTF-8復号を使い、`vault.read`が除去するBOMを保持して`vault.process`の競合検出と揃え、不正な文字コードは従来どおり拒否します。`NoteSource`で従来と同じBOM除去を行い、空白・改行は変更しません。`ObsidianTasksClient`がVaultを照合し、圧縮・長さ制限した要求を直列実行します。テストはLF/CRLF/BOM、Unicode、見出し、リスト、表、リンク、コード、末尾空白、クエリ前後の本文のWPF描画構造を比較し、方式の分離・遅延応答・失敗・長さ制限も確認します。任意の実機テストは`dotnet run --project tests/StickyNotes.Tests -c Release -p:TasksPreviewBuild=true -- --tasks-cli-smoke`です。既存ノートを比較用に読み、アクティブVaultに一意な名前の合成テストノートを作成して、描画・保存・バックアップ・競合を確認後に削除します。出力は検証結果・バージョンのみです。実際に使用するクエリの画面比較は今回の試用で確認してください。

参照：[公式CLI](https://obsidian.md/help/cli)、[Tasksのクエリ描画器](https://github.com/obsidian-tasks-group/obsidian-tasks/blob/7.23.1/src/Renderer/QueryResultsRenderer.ts)。

残る試験上の指摘（影響度・低）：UTF-8検証中に実機保存が1回競合を報告し、その後4回は通過しました。一時的な競合の原因は未特定です。保存は拒否され、上書きされていません。再発時は下書きを保全して再読込し、保存直前の取得元ハッシュ比較で追加調査する必要があります。CLI保存はObsidianのキュー処理に依存し、独立した外部エディターをロックするものではありません。

## 简体中文

这是独立的 Windows x64 试验版。它将已安装的 Obsidian Tasks 的 **Copy results（复制结果）Markdown 输出**显示在便签的`tasks`代码块位置。普通 Markdown、普通复选框及多个查询可以共存。不使用替代查询引擎，也不需要额外的 Obsidian 插件。

### 安装与试用

1. 运行`./build.ps1 -Publish -TasksPreview`。输出为`artifact/tasks-cli-preview/app`及`artifact/tasks-cli-preview/StickyNotes-TasksPreview-win-x64.zip`（含 SHA-256）。
2. 解压整个 ZIP 并运行`Install.cmd`。也可从仓库运行`powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\artifact\tasks-cli-preview\app\Install.ps1 -Preview`。还可直接启动文件夹中的`StickyNotes.exe`。
3. 启动 **Markdown Sticky Notes - Tasks Preview**。安装目录为`%LOCALAPPDATA%\Programs\MarkdownStickyNotes-TasksPreview`；设置、新建笔记及备份保存在`%LOCALAPPDATA%\StickyNotes-TasksPreview`。可与普通版同时运行，不复制已有设置。
4. 在 Obsidian 打开目标仓库，启用 Tasks 及 **设置 → 常规 → 命令行界面**，等待 Tasks 完成加载。调用接口的检查使用已安装的 Obsidian 1.14.4／Tasks 7.23.1；其他版本需要测试。
5. 在试验版设置的 **Obsidian Tasks — CLI Preview** 中启用集成，填写`Obsidian.com`绝对路径及仓库文件夹后保存。若文件夹名无法识别目标仓库，请填写仓库名称／ID。
6. 使用便签菜单 **打开现有笔记**，选择该仓库中包含`tasks`块的 Markdown，或关联标题／日记。启用时，所有笔记都必须位于指定仓库内。新建笔记前，请将保存目录设为仓库内已有文件夹；独立配置的默认目录在仓库外。
7. 先通过 CLI 获取完整 Markdown 再执行查询，此后正文与结果约每30秒刷新。重新加载按钮获取正文，**刷新 Tasks（试验版）**更新查询结果。请与 Obsidian Tasks 的 **Copy results** 比较，并在 Obsidian 编辑后检查刷新。

### 限制与错误处理

- 启用时，包括标题预览在内的所有 Markdown 读取及日记查找都通过 Obsidian CLI，失败时不会回退到本地读取。禁用后恢复原方式，两种来源使用相同 Markdown 渲染器。编辑期间切换方式后必须重新加载才能保存，请先保留草稿。
- 查询结果 **只读**。CLI 模式下，普通正文、元数据、标题创建、普通复选框及新建笔记通过 Obsidian Vault API 保存。在`vault.process`中检查读取时的哈希，先备份原始文本再写入。独立设置不复制笔记。保存／关联确认可能阻塞界面至 CLI 超时；超时后请重新加载以确认是否已完成，再决定重试。
- 使用原笔记路径及 Obsidian 元数据，由原插件处理查询上下文、全局设置及预设。不把结果写入正文。外部编辑后元数据可能短暂滞后。
- 获取原生 Markdown 导出，不复制 Obsidian HTML/CSS、交互工具栏、编辑按钮或完整视觉布局。Wiki 链接、自定义状态、树／列布局和 hide/show 在便签中可能不同。编辑模式可查看查询原文。空输出表示“没有显示结果”，不保证任务数为零。
- 正文／结果仅 **缓存在内存**。刷新失败时保留上次 CLI 正文／结果并显示错误；来源变化时清空缓存，首次失败时显示错误。后台读取不会覆盖草稿，也不采用过时响应。要求 Obsidian 正在运行，同时关闭本体可能与 CLI 自动启动发生竞争。不会终止 Obsidian。
- CLI 同时仅一个请求，含排队25秒超时。源笔记限2 MB，查找最多10,000个仓库 Markdown 文件。每张便签最多20个查询／8,000查询字符，每块输出最多200,000字符，总输出也有限制。压缩请求还受保守的4,000字节预算限制（JSON 参数及通信格式预留空间），超限时不会启动 CLI；较长正文可能需要在 Obsidian 保存。这是针对长请求触发 Obsidian 1.14.4 主进程 JSON 解析错误的规避措施，并未修复 Obsidian 本体。大查询请添加`limit`。CLI 超时不会取消 Obsidian 内已经执行的 JavaScript。
- 依赖 **Tasks 内部 API** 并在运行时检查，更新可能导致失效。语法错误、Tasks 未启用、索引未就绪都会报错，不视为空结果。不分发 Tasks 引擎或第三方插件二进制文件。
- 仅对可信查询／仓库启用。Tasks JavaScript 筛选器、预设和全局查询以 Obsidian 权限运行。正文与查询作为编码 JSON 数据传输，仅将随附桥接作为代码求值。不修改 Tasks JavaScript 设置或访问剪贴板。仅新建／保存笔记时写入文件（含备份），不把查询结果保存到正文。

### 开发与验证

`TasksBridge.js`构造原插件渲染器而不订阅事件，调用查询及 Markdown 导出后释放对象。`NotesBridge.js`通过 CLI 使用`vault.adapter.readBinary`及严格 UTF-8 解码，保留被`vault.read`移除的 BOM，与`vault.process`冲突检查一致，并按原方式拒绝无效编码。`NoteSource`按原方式移除 BOM，不改变空白或换行。`ObsidianTasksClient`验证仓库，串行发送压缩且长度受限的请求。测试比较 LF/CRLF/BOM、Unicode、标题、列表、表格、链接、代码、末尾空白及查询周围正文的 WPF 结构，并检查模式隔离、过时响应、失败及长度限制。可选实机测试：`dotnet run --project tests/StickyNotes.Tests -c Release -p:TasksPreviewBuild=true -- --tasks-cli-smoke`。它读取已有笔记作比较，在活动仓库创建唯一名称的合成测试笔记，检查渲染、保存、备份及冲突后删除该笔记，仅输出验证结果和版本。实际查询的画面对比仍需在试用中确认。

参考：[官方 CLI](https://obsidian.md/help/cli)、[Tasks 查询渲染器](https://github.com/obsidian-tasks-group/obsidian-tasks/blob/7.23.1/src/Renderer/QueryResultsRenderer.ts)。

剩余试验问题（影响较低）：UTF-8 验证时实机保存曾报告一次冲突，随后四次运行通过。该临时冲突的原因尚未确定；保存被拒绝，未覆盖笔记。若复现，请保留草稿并重新加载，后续应比较保存前的源哈希。CLI 保存依赖 Obsidian 的队列 API，不会锁定独立的外部编辑器。
