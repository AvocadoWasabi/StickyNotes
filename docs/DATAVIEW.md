[English](#english) | [日本語](#日本語) | [简体中文](#简体中文)

# Dataview

## English

Current source builds display fenced `dataview` **LIST, TABLE and TASK** queries through Obsidian CLI. This addition is not in the published v0.0.7 ZIP. Build with `./build.ps1 -Publish` and install with `artifacts/app/Install.cmd`.

Start Obsidian with CLI enabled, enable **Dataview** in the selected vault, and configure **Settings → Obsidian CLI** in Sticky Notes. Tasks is required only for `tasks` blocks. See [CLI setup](TASKS-CLI-PREVIEW.md#english). For example:

````markdown
```dataview
TABLE file.name, file.mtime
WHERE file.folder = this.file.folder
SORT file.mtime DESC
LIMIT 10
```
````

Results appear beside ordinary Markdown and Tasks blocks and never replace the saved query. Each provider refreshes about every 30 seconds outside editing; **Refresh Tasks / Dataview** retries immediately. Each has its own last-fetch time and error. A failed request retains the previous results with an error notice. Clicking generated output to edit goes to its source query. Verified TASK checkboxes can update their source notes with conflict checks and backups. See [checkbox behavior and the note browser](QUERY-EDITING-AND-BROWSER.md#english).

Only Markdown output is supported. `dataviewjs` fences and inline expressions such as `` `= this.file.name` `` remain visible source and are not executed. CALENDAR displays an unsupported-query message. Obsidian HTML/CSS, interactive controls and native wiki-link navigation are not reproduced. Disabling CLI shows the original fenced code. A disabled plugin, unsupported API or unfinished index reports an error and retries on refresh.

The bridge calls the installed plugin's `api.query(query, sourceNotePath)` + `markdownList` / `markdownTable` / `markdownTaskList`, so `this` and relative references use the note containing the query. It checks the selected vault and Markdown source and creates no render component, subscription or note write. Query text is compressed JSON data; only the bundled adapter is evaluated as JavaScript. Limits are 20 blocks / 8,000 query characters **per provider per note**, 200,000 output characters per block, and the shared CLI argument/output limits and 25-second timeout (including queue wait). Large or slow queries should use `LIMIT`; timing out does not cancel work already running inside Obsidian.

Normal tests use synthetic note sources and verify mixed rendering, mapped checkbox updates, independent failures, source preservation, throttling and stale-response cancellation. With Node.js installed, `dotnet run --project tests/StickyNotes.Tests -c Release -- --dataview-bridge-smoke` executes the generated CLI code against a mocked Dataview API. No real vault is used. The API contract was checked against [Dataview's source](https://github.com/blacksmithgu/obsidian-dataview/blob/master/src/api/plugin-api.ts); this addition has not been validated with a running Obsidian/Dataview instance.

## 日本語

現在のソースビルドは、`dataview`コードブロックの **LIST・TABLE・TASK** をObsidian CLI経由で表示します。公開済みv0.0.7 ZIPには含まれません。`./build.ps1 -Publish`でビルドし、`artifacts/app/Install.cmd`で導入してください。

CLIを有効にしたObsidianを起動し、対象Vaultで **Dataview** を有効にして、付箋の **設定 → Obsidian CLI** で接続します。Tasksは`tasks`ブロックだけに必要です。[CLI設定](TASKS-CLI-PREVIEW.md#日本語)も参照してください。例：

````markdown
```dataview
TABLE file.name, file.mtime
WHERE file.folder = this.file.folder
SORT file.mtime DESC
LIMIT 10
```
````

結果は通常のMarkdownやTasksブロックと同居し、保存するクエリ原文を置き換えません。編集中以外は各プラグインを約30秒ごとに取得し、**Tasks / Dataviewを更新**で即時再取得できます。取得時刻・エラーを個別に表示し、取得失敗時は前回結果とエラー表示を保持します。生成結果のクリックから編集すると元のクエリへ移動します。元ノートを照合できたTASKのチェックボックスは、競合検出・バックアップ付きで元ノートを更新できます。[チェック操作と付箋一覧](QUERY-EDITING-AND-BROWSER.md#日本語)を参照してください。

対応するのはMarkdown出力です。`dataviewjs`ブロックと `` `= this.file.name` `` などのインライン式は原文を表示し、実行しません。CALENDARには未対応の案内を表示します。ObsidianのHTML/CSS・操作部品・本家のWikiリンク移動は再現しません。CLI無効時は元のコードブロックを表示します。プラグイン無効・API未対応・索引作成中はエラーを表示し、次回更新で再試行します。

同梱アダプターはインストール済みプラグインの`api.query(query, sourceNotePath)` + `markdownList` / `markdownTable` / `markdownTaskList`を呼び、`this`や相対参照をクエリのあるノートを基準に解決します。対象VaultとMarkdownファイルを確認し、描画コンポーネント・購読・ノート書き込みは作成しません。クエリは圧縮JSONデータで渡し、JavaScriptとして評価するのは同梱アダプターだけです。上限は**各プラグイン・各ノートごと**に20ブロック／合計8,000文字、結果は各ブロック200,000文字で、CLI共通の引数・出力上限と待機込み25秒のタイムアウトも適用します。大量・低速クエリは`LIMIT`で絞ってください。タイムアウトしてもObsidian内で開始済みの処理は停止しません。

通常テストは合成ノートで混在表示・元タスクのチェック更新・個別の失敗・原文保持・更新間隔・古い応答の破棄を検証します。Node.js導入済みなら`dotnet run --project tests/StickyNotes.Tests -c Release -- --dataview-bridge-smoke`で、生成したCLIコードを模擬Dataview APIに対して実行できます。実際のVaultは使用しません。API仕様は[Dataviewのソース](https://github.com/blacksmithgu/obsidian-dataview/blob/master/src/api/plugin-api.ts)で確認しましたが、この追加機能は起動中のObsidian／Dataviewとの実機検証は未実施です。

## 简体中文

当前源码构建通过 Obsidian CLI 显示 `dataview` 代码块中的 **LIST、TABLE 和 TASK** 查询。已发布的 v0.0.7 ZIP 不包含此功能。请运行 `./build.ps1 -Publish` 构建，再用 `artifacts/app/Install.cmd` 安装。

启动已启用 CLI 的 Obsidian，在目标仓库启用 **Dataview**，并在便签的 **设置 → Obsidian CLI** 中配置连接。只有 `tasks` 块需要 Tasks 插件。参见 [CLI 设置](TASKS-CLI-PREVIEW.md#简体中文)。例如：

````markdown
```dataview
TABLE file.name, file.mtime
WHERE file.folder = this.file.folder
SORT file.mtime DESC
LIMIT 10
```
````

结果与普通 Markdown 和 Tasks 块同时显示，不会替换保存的查询原文。非编辑状态下每个插件约每 30 秒刷新，**刷新 Tasks / Dataview** 可立即重试。各自显示获取时间和错误；请求失败时保留上次结果并标注错误。点击生成结果开始编辑会定位到原查询。已验证的 TASK 复选框可在冲突检查和备份后更新原笔记。参见 [复选框和便签列表](QUERY-EDITING-AND-BROWSER.md#简体中文)。

仅支持 Markdown 输出。`dataviewjs` 块及 `` `= this.file.name` `` 等内联表达式保留原文，不会执行。CALENDAR 显示不支持提示。不复现 Obsidian 的 HTML/CSS、交互控件或原生 Wiki 链接导航。禁用 CLI 时显示原代码块。插件未启用、API 不兼容或索引未完成时显示错误，并在下次刷新重试。

随附适配器调用已安装插件的 `api.query(query, sourceNotePath)` + `markdownList` / `markdownTable` / `markdownTaskList`，使 `this` 和相对引用以查询所在笔记为基准。检查目标仓库及 Markdown 文件，不创建渲染组件、订阅或笔记写入。查询作为压缩 JSON 数据传递，仅将随附适配器作为 JavaScript 求值。**每个插件、每篇笔记**最多 20 块／合计 8,000 查询字符，每块结果最多 200,000 字符；同时适用 CLI 共用的参数／输出限制和包含排队时间的 25 秒超时。较大或缓慢的查询请用 `LIMIT` 缩小范围；超时不会停止 Obsidian 内已经开始的处理。

常规测试使用合成笔记，验证混合渲染、原任务复选框更新、独立失败、原文保留、刷新间隔和过期响应丢弃。安装 Node.js 后，可运行 `dotnet run --project tests/StickyNotes.Tests -c Release -- --dataview-bridge-smoke`，对模拟 Dataview API 执行生成的 CLI 代码，不使用真实仓库。API 约定已根据 [Dataview 源码](https://github.com/blacksmithgu/obsidian-dataview/blob/master/src/api/plugin-api.ts)检查；此新增功能尚未与运行中的 Obsidian／Dataview 进行实机验证。
