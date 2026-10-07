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
6. Use the note menu to **open an existing Markdown note** containing a `tasks` block inside that vault, or link a heading/daily note. A new standalone preview note outside the vault cannot execute a query.
7. Results appear within about 2 seconds on first display, then refresh approximately every 30 seconds. **Refresh Tasks (preview)** or the reload button retries immediately when no request is running. Compare the results with Tasks' **Copy results** in Obsidian. Edit a task in Obsidian and check the next refresh.

### Boundaries and failure behavior

- Query results are **read-only**; normal note editing and ordinary local checkboxes still modify the linked Markdown file. Separate settings do not create copies of linked vault notes.
- Uses the source note path and its Obsidian metadata for query context, global settings and presets. Results are never written into the note. Cached metadata may briefly lag an external edit.
- Fetches the native Markdown export, not Obsidian's HTML/CSS, interactive toolbar, edit buttons or full visual layout. Wiki links, custom statuses, tree/column layouts and hide/show options may look different in this Markdown renderer. Inspect the source in edit mode. Empty export means “no displayed results,” not a guaranteed count of zero tasks.
- Results are cached **in memory only**. A failed refresh keeps the last results and timestamp with an error; restarting clears that cache. Obsidian must be running. The preview checks for its process before calling CLI; closing it during a call can race with CLI's own auto-launch behavior. It does not stop Obsidian.
- One CLI request at a time, a 25-second timeout including queue wait, at most 20 queries / 8,000 query characters per note (long paths/escaping can reduce the limit), 200,000 output characters per block and bounded CLI output. Add `limit` to large queries. A timed-out CLI does not guarantee cancellation of JavaScript already executing inside Obsidian.
- Uses **internal Tasks APIs**, checked at runtime. Updates can break the adapter. Errors, missing Tasks and an unready index are reported rather than treated as empty results. No tasks engine or third-party plugin binaries are distributed.
- Enable only for trusted queries/vaults: Tasks JavaScript filters, presets and global queries run with Obsidian's privileges. The bridge passes query text as encoded data, does not change Tasks' JavaScript settings, and performs no file writes or clipboard operations itself.

### Development and checks

`TasksBridge.js` constructs Tasks' query renderer without loading its event listeners, calls native search and native Markdown export, then unloads the temporary child. `ObsidianTasksClient` transports bounded JSON through CLI `eval`, validates the actual vault, and serializes requests. Standard tests cover mixed Markdown, source line mapping, read-only results, path boundaries, encoded query data and response framing. Optional desktop smoke test: `dotnet run --project tests/StickyNotes.Tests -c Release -- --tasks-cli-smoke` with Obsidian/Tasks running; it queries the active vault and prints only assertions/version, not note contents. GUI comparison of real user queries remains part of this trial.

References: [Official CLI](https://obsidian.md/help/cli), [Tasks query renderer](https://github.com/obsidian-tasks-group/obsidian-tasks/blob/7.23.1/src/Renderer/QueryResultsRenderer.ts).

## 日本語

Windows x64向けの独立した試験版です。インストール済みObsidian Tasksの **Copy results（結果コピー）のMarkdown出力**を、付箋の`tasks`コードブロックの位置に表示します。通常のMarkdown、通常のチェックボックス、複数のクエリが同居できます。独自の互換検索エンジンや追加のObsidianプラグインは使いません。

### インストールと確認

1. `./build.ps1 -Publish -TasksPreview`でビルドします。出力は`artifact/tasks-cli-preview/app`と`artifact/tasks-cli-preview/StickyNotes-TasksPreview-win-x64.zip`（SHA-256付き）です。
2. ZIP全体を展開し、`Install.cmd`を実行します。リポジトリからは`powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\artifact\tasks-cli-preview\app\Install.ps1 -Preview`でも導入できます。フォルダ内の`StickyNotes.exe`の直接起動も可能です。
3. **Markdown Sticky Notes - Tasks Preview**を起動します。導入先は`%LOCALAPPDATA%\Programs\MarkdownStickyNotes-TasksPreview`、設定・新規ノート・バックアップは`%LOCALAPPDATA%\StickyNotes-TasksPreview`です。通常版と同時起動でき、既存設定はコピーしません。
4. Obsidianで対象Vaultを開き、Tasksと **設定 → 一般 → コマンドラインインターフェース** を有効にして、Tasksの読み込みを待ちます。呼び出し口の調査には導入済みのObsidian 1.14.4／Tasks 7.23.1を使用しました。他の版は動作確認が必要です。
5. 試験版の設定の **Obsidian Tasks — CLI Preview** で連携を有効にし、`Obsidian.com`の絶対パスとVaultフォルダを指定して保存します。フォルダ名で対象Vaultを特定できない場合はVault名／IDも指定します。
6. 付箋メニューの **既存ノートを開く** から、そのVault内の`tasks`ブロックを含むMarkdownを選択するか、見出し・デイリーノートを連携します。Vault外にある試験版の新規付箋ではクエリを実行できません。
7. 初回は約2秒後、その後は約30秒ごとに取得します。**Tasksを更新（試験版）** または再読込ボタンで、取得中でなければ即時再取得できます。ObsidianのTasksの **Copy results** と結果を比較してください。Obsidian側でタスクを編集して、次の更新も確認できます。

### 制約とエラー時の扱い

- 検索結果は **読み取り専用** です。通常の本文編集・通常のチェックボックスは連携先Markdownを変更します。設定の分離は、連携先Vaultノートの複製を意味しません。
- 元ノートのパスとObsidianのメタデータを使い、クエリ文脈・グローバル設定・プリセットを本家に処理させます。結果を本文へ保存しません。外部編集直後はメタデータの反映が遅れることがあります。
- 取得するのは本家のMarkdown出力です。ObsidianのHTML/CSS・操作ツールバー・編集ボタン・完全な画面レイアウトは再現しません。Wikiリンク、独自ステータス、ツリー／列表示、hide/showは付箋で見え方が異なる場合があります。クエリ原文は編集モードで確認できます。空の出力は「表示結果なし」であり、タスク0件を保証しません。
- 最後の結果は **メモリ内だけ** に保持します。取得失敗時は前回結果・取得時刻・エラーを表示し、再起動でキャッシュは消えます。Obsidianの起動が必要です。CLI実行前に本体プロセスを確認しますが、実行と同時に終了するとCLI側の自動起動と競合する可能性があります。本体を終了させる処理はありません。
- CLIは同時1要求、待機込み25秒でタイムアウトします。1付箋あたり最大20クエリ・合計8,000文字（長いパスやエスケープで上限が下がる場合あり）、1ブロックの出力は最大200,000文字で、CLI全体の出力も制限します。大きいクエリには`limit`を追加してください。CLIがタイムアウトしても、Obsidian内ですでに実行中のJavaScriptの停止は保証できません。
- **Tasksの内部API** に依存し、実行時に呼び出し口を確認します。更新により動かなくなる場合があります。構文エラー・Tasks未有効・索引準備中を空の結果とは扱いません。本家のエンジンやプラグインバイナリは同梱しません。
- 信頼できるクエリとVaultでのみ有効にしてください。TasksのJavaScriptフィルター、プリセット、グローバルクエリはObsidianの権限で動作します。連携処理はクエリをエンコード済みデータとして渡し、TasksのJavaScript設定を変更せず、直接のファイル書き込み・クリップボード操作を行いません。

### 開発と検証

`TasksBridge.js`はイベント購読を開始せずに本家のクエリ描画器を構築し、本家の検索・Markdown出力を呼んで一時オブジェクトを解放します。`ObsidianTasksClient`がCLIの`eval`で上限付きJSONを受け渡し、実際のVaultを照合して要求を直列化します。通常テストではMarkdownの同居、元の行番号、結果の読み取り専用、パス境界、クエリのデータ化、応答形式を検証します。実環境用の任意テストは、Obsidian／Tasks起動中に`dotnet run --project tests/StickyNotes.Tests -c Release -- --tasks-cli-smoke`です。アクティブVaultに問い合わせ、本文を出力せず検証結果とバージョンのみ表示します。実際に使用するクエリの画面比較は今回の試用で確認してください。

参照：[公式CLI](https://obsidian.md/help/cli)、[Tasksのクエリ描画器](https://github.com/obsidian-tasks-group/obsidian-tasks/blob/7.23.1/src/Renderer/QueryResultsRenderer.ts)。

## 简体中文

这是独立的 Windows x64 试验版。它将已安装的 Obsidian Tasks 的 **Copy results（复制结果）Markdown 输出**显示在便签的`tasks`代码块位置。普通 Markdown、普通复选框及多个查询可以共存。不使用替代查询引擎，也不需要额外的 Obsidian 插件。

### 安装与试用

1. 运行`./build.ps1 -Publish -TasksPreview`。输出为`artifact/tasks-cli-preview/app`及`artifact/tasks-cli-preview/StickyNotes-TasksPreview-win-x64.zip`（含 SHA-256）。
2. 解压整个 ZIP 并运行`Install.cmd`。也可从仓库运行`powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\artifact\tasks-cli-preview\app\Install.ps1 -Preview`。还可直接启动文件夹中的`StickyNotes.exe`。
3. 启动 **Markdown Sticky Notes - Tasks Preview**。安装目录为`%LOCALAPPDATA%\Programs\MarkdownStickyNotes-TasksPreview`；设置、新建笔记及备份保存在`%LOCALAPPDATA%\StickyNotes-TasksPreview`。可与普通版同时运行，不复制已有设置。
4. 在 Obsidian 打开目标仓库，启用 Tasks 及 **设置 → 常规 → 命令行界面**，等待 Tasks 完成加载。调用接口的检查使用已安装的 Obsidian 1.14.4／Tasks 7.23.1；其他版本需要测试。
5. 在试验版设置的 **Obsidian Tasks — CLI Preview** 中启用集成，填写`Obsidian.com`绝对路径及仓库文件夹后保存。若文件夹名无法识别目标仓库，请填写仓库名称／ID。
6. 使用便签菜单 **打开现有笔记**，选择该仓库中包含`tasks`块的 Markdown，或关联标题／日记。位于仓库外的新建试验便签无法执行查询。
7. 首次约2秒后获取，此后约每30秒刷新。无请求正在执行时，**刷新 Tasks（试验版）**或重新加载按钮可立即重试。请与 Obsidian Tasks 的 **Copy results** 比较；也可在 Obsidian 修改任务并检查下次刷新。

### 限制与错误处理

- 查询结果 **只读**。普通正文编辑及普通复选框仍会修改关联的 Markdown。独立设置不代表复制关联的仓库笔记。
- 使用原笔记路径及 Obsidian 元数据，由原插件处理查询上下文、全局设置及预设。不把结果写入正文。外部编辑后元数据可能短暂滞后。
- 获取原生 Markdown 导出，不复制 Obsidian HTML/CSS、交互工具栏、编辑按钮或完整视觉布局。Wiki 链接、自定义状态、树／列布局和 hide/show 在便签中可能不同。编辑模式可查看查询原文。空输出表示“没有显示结果”，不保证任务数为零。
- 结果仅 **缓存在内存**。刷新失败时保留上次结果、时间和错误；重启后清除。要求 Obsidian 正在运行。调用 CLI 前检查进程，但同时关闭本体时可能与 CLI 的自动启动行为发生竞争。不会终止 Obsidian。
- CLI 同时仅一个请求，含排队25秒超时。每张便签最多20个查询／8,000查询字符（长路径或转义可能降低限制），每块输出最多200,000字符，CLI 总输出也有限制。大查询请添加`limit`。CLI 超时不保证取消 Obsidian 内已经运行的 JavaScript。
- 依赖 **Tasks 内部 API** 并在运行时检查，更新可能导致失效。语法错误、Tasks 未启用、索引未就绪都会报错，不视为空结果。不分发 Tasks 引擎或第三方插件二进制文件。
- 仅对可信查询／仓库启用。Tasks JavaScript 筛选器、预设和全局查询以 Obsidian 权限运行。桥接将查询作为编码数据传递，不修改 Tasks 的 JavaScript 设置，本身不写文件或操作剪贴板。

### 开发与验证

`TasksBridge.js`构造原插件查询渲染器而不启动事件订阅，调用原生查询和 Markdown 导出后释放临时对象。`ObsidianTasksClient`通过 CLI `eval`传输有上限的 JSON，验证实际仓库并串行执行请求。常规测试覆盖 Markdown 共存、原始行号、只读结果、路径边界、查询数据编码及响应格式。可选桌面测试：启动 Obsidian／Tasks 后运行`dotnet run --project tests/StickyNotes.Tests -c Release -- --tasks-cli-smoke`。它查询当前仓库，只输出验证结果与版本，不输出笔记内容。实际查询的画面对比仍需在本次试用中确认。

参考：[官方 CLI](https://obsidian.md/help/cli)、[Tasks 查询渲染器](https://github.com/obsidian-tasks-group/obsidian-tasks/blob/7.23.1/src/Renderer/QueryResultsRenderer.ts)。
