[English](#english) | [日本語](#日本語) | [简体中文](#简体中文)

# Obsidian CLI / Obsidian CLI連携 / Obsidian CLI 集成

## English

Current source builds also display Dataview LIST/TABLE/TASK queries through CLI, alongside Tasks and normal Markdown. Enable Dataview in the selected vault. Results are read-only and refresh about every 30 seconds; DataviewJS, inline expressions and CALENDAR are unsupported. See [Dataview setup and limits](DATAVIEW.md#english). This is not included in the published v0.0.7 ZIP.

Since v0.0.7, new settings use Obsidian CLI by default. The existing standard profile is retained; an explicitly saved local-only selection remains respected. Disable CLI in Settings to use local files without Obsidian. This integration depends on internal Obsidian/Tasks APIs; the tested versions and limits are below.

**Upgrading from v0.0.6 or earlier:** settings without a CLI selection also default to CLI and require vault setup. To continue with local files, uncheck **Use Obsidian CLI** in Settings and save. Existing files are not moved automatically.

### Setup

1. Download the [v0.0.7 ZIP](https://github.com/AvocadoWasabi/StickyNotes/releases/tag/v0.0.7), extract all files and run `Install.cmd`. Start **Markdown Sticky Notes**. For a source build, run `./build.ps1 -Publish`, then `artifacts/app/Install.cmd`. The optional isolated build remains `./build.ps1 -Publish -TasksPreview`, with its installer/ZIP under `artifacts/tasks-cli-preview` and separate `StickyNotes-TasksPreview` profile.
2. Start Obsidian, open the intended vault and enable **Settings → General → Command line interface**. For daily notes, enable the **Daily notes core plugin**; for `tasks` queries, enable **Tasks**. Tested with Obsidian 1.14.4 / Tasks 7.23.1.
3. In sticky-note **Settings → Obsidian CLI**, leave **Use Obsidian CLI** checked, verify the absolute path to `Obsidian.com`, and click **Get open vault from Obsidian**. Review the resulting vault folder/name and save. Manual connection entry remains available. A new profile without a vault opens Settings first.
4. Open a Markdown note inside that vault, or use **Display a daily note** and choose its heading. Missing daily files are not created automatically.

### One source of settings

- Daily folder, date format and template come from Obsidian and are displayed as read-only information. The native path routine used by `daily:path` resolves today and yesterday, including locale-dependent weekdays and nested date folders. The app no longer scans every vault Markdown file or translates the date format into a local regex.
- Choose a sticky-note folder from the retrieved vault list, or type a new relative name. Confirm asks Yes/No/Cancel about moving existing app-created sticky notes. Without a selection, Obsidian's new-note location remains the default. Local base-folder and daily-regex fields remain hidden in CLI mode. See [folder selection, migration and recovery](STICKY-FOLDERS.md#english).
- Today's missing-note behavior remains configurable in the sticky app: wait, keep yesterday until today exists, or keep yesterday until manual refresh. These are display preferences, not duplicate Obsidian settings. Changing Obsidian's path settings resets retained-date state.
- Fixed notes, heading previews, reads, creation and saves all use CLI in this mode. There is no fallback to direct local reads. Text and query results refresh about every 30 seconds; reload retries the note immediately. CLI failures retain prior CLI content with an error and preserve drafts. Changing mode while editing requires reloading before saving.

### Tasks, preservation and limits

Tasks runs the installed plugin's native search and Markdown export; results are read-only and never replace the query source. Ordinary text and checkboxes remain editable. The sticky app renders the returned Markdown, so Obsidian HTML/CSS, interactive controls and some layouts are not reproduced. Use trusted queries: Tasks JavaScript filters, global queries and presets execute with Obsidian's privileges.

UTF-8 is decoded strictly, preserving newline/whitespace behavior and the original reader's BOM policy. Saves verify a hash inside `vault.process` and back up the original text before writing. They rely on Obsidian's queue and do not lock independent external editors. Live tests have occasionally reported transient snapshot/hash mismatches or save conflicts; retries passed, but the cause remains unisolated (low impact: overwrite was rejected). Keep any draft text and reload before retrying.

CLI calls are serialized with a 25-second timeout including queue wait. Source and saved notes are limited to 2 MB (UTF-8 bytes including BOM); queries to 20 blocks / 8,000 characters, export to 200,000 characters per block, and total output is bounded. Arguments plus framing reserve must fit 4,000 bytes, mitigating Obsidian 1.14.4's observed long-request JSON parsing error. Current source builds pass oversized note requests through a temporary file restricted to the current Windows user, verify its SHA-256 before executing the bundled adapter, and delete it on success, failure or cancellation. Note I/O still uses Obsidian APIs with backups and conflict checks; there is no local note-write fallback. Published v0.0.7 packages still have the old long-edit restriction. Synchronous save/link confirmation can wait for the timeout; after a timeout, reload before retrying because executing JavaScript is not cancelled.

Daily settings/path resolution and Tasks use internal APIs with runtime checks. Unsupported/disabled plugins report errors. This daily integration targets the core Daily notes plugin; it does not infer settings from Periodic Notes or other daily-note plugins. Obsidian must remain running; closing it during a CLI call can race with the CLI's auto-launch behavior.

### Development

`DailyBridge.js` obtains native settings and two date paths without creating files. `NoteSource` supplies the shared retention, heading and rendering logic. `NotesBridge.js` uses Vault APIs for note I/O; `TasksBridge.js` exports native query results. Encoded JSON stays data; only bundled adapters are evaluated as code. No clipboard access or third-party plugin binaries.

Tests cover native paths, missing dates, retention, settings changes, vault boundaries, hidden legacy-field preservation, CLI failures, obsolete responses and rendering parity. The normal build also checks temporary-transfer permissions, text preservation and cleanup on failure. With Node.js installed, `dotnet run --project tests/StickyNotes.Tests -c Release -- --note-transport-smoke` executes the generated transfer loader and bundled bridge against synthetic files and mocked Vault APIs, including 2 MB saves, BOM, backups and conflict/failure preservation; it does not require Obsidian. Optional desktop smoke: `dotnet run --project tests/StickyNotes.Tests -c Release -- --tasks-cli-smoke`. It reads native settings and an existing note, creates a uniquely named synthetic fixture for encoding/save/backup/conflict checks, then deletes that fixture. Only assertions/version are printed.

References: [Official CLI](https://obsidian.md/help/cli), [Tasks query renderer](https://github.com/obsidian-tasks-group/obsidian-tasks/blob/7.23.1/src/Renderer/QueryResultsRenderer.ts).

## 日本語

現在のソースビルドは、DataviewのLIST・TABLE・TASKをCLI経由でTasksや通常のMarkdownと同時に表示できます。対象VaultでDataviewを有効にしてください。結果は読み取り専用・約30秒更新で、DataviewJS・インライン式・CALENDARは未対応です。[Dataviewの設定と制約](DATAVIEW.md#日本語)を参照してください。公開済みv0.0.7 ZIPには含まれません。

v0.0.7から、新規設定ではObsidian CLIを既定とします。通常プロファイルと以前に明示保存したローカル専用の選択を維持します。設定でCLIを無効にすればObsidianなしでローカルファイルを使えます。Obsidian／Tasksの内部APIに依存するため、確認済みバージョンと制限を以下に記載します。

**v0.0.6以前からの更新：** CLI選択項目がない旧設定もCLIが有効になるため、Vaultの設定が必要です。従来のローカル方式を続ける場合は、設定の **Obsidian CLIを使用する** を無効にして保存してください。既存ファイルは自動移行しません。

### 導入

1. [v0.0.7のZIP](https://github.com/AvocadoWasabi/StickyNotes/releases/tag/v0.0.7)をダウンロードし、全体を展開して`Install.cmd`を実行後、**Markdown Sticky Notes** を起動します。ソースからの場合は`./build.ps1 -Publish`でビルドし、`artifacts/app/Install.cmd`で導入します。独立試験版は引き続き`./build.ps1 -Publish -TasksPreview`で作成でき、インストーラー・ZIPは`artifacts/tasks-cli-preview`、設定は別の`StickyNotes-TasksPreview`です。
2. Obsidianで対象Vaultを開き、**設定 → 一般 → コマンドラインインターフェース**を有効にします。デイリーにはコアプラグインの **デイリーノート**、`tasks`クエリには **Tasks** が必要です。Obsidian 1.14.4／Tasks 7.23.1で確認しています。
3. 付箋の **設定 → Obsidian CLI** で **Obsidian CLIを使用する** を有効にし、`Obsidian.com`の絶対パスを確認して **開いているVaultをObsidianから取得** を押します。取得されたVaultフォルダ・名前を確認して保存します。接続先の手入力も可能です。Vault未設定の新規プロファイルでは設定画面を先に開きます。
4. Vault内のMarkdownを開くか、**デイリーノートを表示**から見出しを選びます。未作成のデイリーノートは自動作成しません。

### 設定の一元化

- デイリーフォルダ・日付形式・テンプレートをObsidianから読み、変更不可の情報として表示します。`daily:path`が使う本家の処理で今日・前日のパスを解決するため、曜日の言語や日付別サブフォルダもObsidianに従います。Vault全体のMarkdown一覧検索や、付箋側での日付形式から正規表現への変換は行いません。
- Vaultのフォルダ一覧から付箋保存先を選ぶか、新しい相対名を入力できます。決定後に既存のアプリ作成付箋の移行を「はい／いいえ／キャンセル」で選びます。未指定時はObsidianの新規保存先が既定です。CLI時はローカル基準フォルダ・デイリー正規表現を隠します。[保存先選択・移行・復旧](STICKY-FOLDERS.md#日本語)を参照してください。
- 今日の分がない場合の「待機」「今日ができるまで前日保持」「手動更新まで前日保持」は付箋側で選べます。Obsidianの設定と重複しない表示上の設定です。Obsidianのパス設定が変われば保持状態をリセットします。
- 固定ノート・見出しプレビュー・読込・作成・保存をCLIへ統一し、直接のローカル読込にフォールバックしません。本文・結果は約30秒ごとに更新し、再読込で本文を即時取得します。失敗時は前回CLI本文とエラーを表示し、下書きを保全します。編集中に方式を切り替えた場合は保存前に再読込が必要です。

### Tasks・データ保持・制約

Tasksはインストール済み本家の検索とMarkdown出力を使います。結果は読み取り専用で、クエリ原文を置き換えて保存しません。通常の本文・チェックボックスは編集できます。取得結果は付箋側でMarkdown描画するため、ObsidianのHTML/CSSや操作部品、一部のレイアウトは再現しません。JavaScriptフィルター・グローバルクエリ・プリセットはObsidianの権限で実行するため、信頼できるクエリを使用してください。

UTF-8を厳密に復号し、改行・空白と従来のBOM処理を維持します。保存は`vault.process`内でハッシュを照合し、元の文字列をバックアップしてから書き込みます。Obsidianのキュー処理に依存し、独立した外部エディターをロックするものではありません。実機テストでは一時的なスナップショット／ハッシュ不一致や保存競合がまれに発生しています。再試行は通過しましたが、原因は未特定です（影響度・低：上書きは拒否）。下書きを控え、再読込後に再試行してください。

CLIは同時1要求、待機込み25秒のタイムアウトです。元ノート・保存後のノートは2 MB（BOMを含むUTF-8バイト数）、クエリ20ブロック／合計8,000文字、各結果200,000文字までで、総出力にも上限があります。引数と通信形式の予備領域を4,000バイト以内に抑え、Obsidian 1.14.4で観測した長い要求のJSON解析エラーを回避します。現在のソースビルドは長いノート要求を現在のWindowsユーザーだけが読める一時ファイルで渡し、SHA-256照合後に同梱アダプターを実行し、成功・失敗・キャンセル時に削除します。本文の読み書きはObsidian APIを使い、バックアップ・競合検出を維持し、ローカル書き込みへフォールバックしません。公開済みv0.0.7の配布物には従来の長文編集制限が残ります。保存・連携確定はタイムアウトまで画面を待たせる場合があり、実行済みJavaScriptはキャンセルされないため、タイムアウト後は再読込してから再試行してください。

デイリー設定・パス解決とTasksは内部APIに依存し、実行時に確認します。未対応・無効なプラグインはエラーを表示します。デイリー連携はコアのデイリーノートが対象で、Periodic Notesなど他プラグインの設定を推測しません。Obsidianの常時起動が必要で、CLI実行と同時の終了はCLI側の自動起動と競合する可能性があります。

### 開発

`DailyBridge.js`がファイルを作成せずに本家設定と2日分のパスを取得し、`NoteSource`から共通の前日保持・見出し・描画処理へ渡します。`NotesBridge.js`はVault APIで本文を扱い、`TasksBridge.js`は本家の検索結果を出力します。JSONはデータとして扱い、コード評価は同梱アダプターのみです。クリップボード操作や本家プラグインのバイナリ同梱はありません。

テストはネイティブパス、未作成、前日保持、設定変更、Vault境界、隠れた旧設定の保全、CLI失敗、古い応答、描画一致を検証します。通常ビルドでは一時ファイルの権限・本文保持・失敗時の削除も確認します。Node.js導入済みなら`dotnet run --project tests/StickyNotes.Tests -c Release -- --note-transport-smoke`で、生成した受け渡しコードと同梱アダプターを合成ファイル・模擬Vault APIに対して実行し、2 MB保存・BOM・バックアップ・競合／失敗時の保全を検証できます。Obsidianは不要です。任意の実機テストは`dotnet run --project tests/StickyNotes.Tests -c Release -- --tasks-cli-smoke`です。本家設定と既存ノートを読み、一意な名前の合成ノートで文字コード・保存・バックアップ・競合を確認後、その合成ノートを削除します。出力は検証結果・バージョンのみです。

参照：[公式CLI](https://obsidian.md/help/cli)、[Tasksのクエリ描画器](https://github.com/obsidian-tasks-group/obsidian-tasks/blob/7.23.1/src/Renderer/QueryResultsRenderer.ts)。

## 简体中文

当前源码构建支持通过 CLI 将 Dataview LIST／TABLE／TASK 与 Tasks 和普通 Markdown 同时显示。请在目标仓库启用 Dataview。结果只读，约每 30 秒刷新；不支持 DataviewJS、内联表达式或 CALENDAR。参见 [Dataview 设置和限制](DATAVIEW.md#简体中文)。已发布的 v0.0.7 ZIP 不包含此功能。

从 v0.0.7 起，新配置默认使用 Obsidian CLI。保留普通配置及以前明确保存的本地模式选择。可在设置中禁用 CLI，不运行 Obsidian 而使用本地文件。集成依赖 Obsidian／Tasks 内部 API，已验证版本和限制见下文。

**从 v0.0.6 或更早版本升级：** 无 CLI 选项的旧配置也默认启用 CLI，需要设置仓库。继续使用本地文件时，请在设置中取消 **使用 Obsidian CLI** 并保存。已有文件不会自动迁移。

### 安装

1. 下载 [v0.0.7 ZIP](https://github.com/AvocadoWasabi/StickyNotes/releases/tag/v0.0.7)，解压全部文件并运行`Install.cmd`，启动 **Markdown Sticky Notes**。从源码构建时，运行`./build.ps1 -Publish`后使用`artifacts/app/Install.cmd`安装。独立试验版仍可用`./build.ps1 -Publish -TasksPreview`生成，安装程序／ZIP 位于`artifacts/tasks-cli-preview`，使用独立的`StickyNotes-TasksPreview`配置。
2. 在 Obsidian 打开目标仓库并启用 **设置 → 常规 → 命令行界面**。日记需要核心 **日记** 插件；`tasks`查询需要 **Tasks**。已验证 Obsidian 1.14.4／Tasks 7.23.1。
3. 在便签 **设置 → Obsidian CLI** 中启用 **使用 Obsidian CLI**，确认`Obsidian.com`绝对路径并点击 **从 Obsidian 获取当前仓库**。检查仓库目录／名称后保存，也可手动填写连接信息。未配置仓库的新配置会先打开设置。
4. 打开仓库内的 Markdown，或通过 **显示每日笔记** 选择标题。缺失的日记不会自动创建。

### 统一设置来源

- 日记目录、日期格式及模板从 Obsidian 读取并只读显示。使用`daily:path`对应的原生处理解析今天及昨天的路径，包括语言相关星期及按日期划分的子目录。不再扫描整个仓库的 Markdown 列表，也不把日期格式转换为本地正则。
- 可从仓库目录列表选择便签保存位置，或输入新的相对名称。确定后选择是／否／取消，决定是否迁移已有应用便签。未选择时仍采用Obsidian新笔记位置。CLI模式隐藏本地基准目录及日记正则字段。参见[目录选择、迁移及恢复](STICKY-FOLDERS.md#简体中文)。
- 今日缺失时仍可选择等待、保留昨天直到今日创建、或保留到手动刷新。这些是便签显示偏好，并非重复的 Obsidian 设置。Obsidian 路径设置改变时重置日期保留状态。
- 固定笔记、标题预览、读取、创建和保存统一走 CLI，失败时不回退到本地读取。正文／结果约每30秒刷新，重新加载可立即取正文。失败保留上次 CLI 正文并显示错误，保护草稿。编辑期间切换模式后，保存前须重新加载。

### Tasks、数据保护与限制

Tasks 使用已安装原插件的查询及 Markdown 导出。结果只读，不会覆盖查询原文；普通正文和复选框仍可编辑。便签使用自身 Markdown 渲染器，不重现 Obsidian HTML/CSS、交互控件及部分布局。JavaScript 筛选器、全局查询和预设以 Obsidian 权限执行，请使用可信查询。

严格解码 UTF-8，保留换行、空白及原有 BOM 处理。保存时在`vault.process`内核对哈希，先备份原文本再写入。依赖 Obsidian 队列，不锁定独立外部编辑器。实机测试偶尔出现临时快照／哈希不一致或保存冲突；重试通过，但原因仍未确定（影响较低：覆盖被拒绝）。请保留草稿，重新加载后重试。

CLI 串行执行，含排队25秒超时。源笔记及保存后的笔记限2 MB（含 BOM 的 UTF-8 字节数），查询20块／共8,000字符，每块结果200,000字符，总输出也有限制。参数及通信格式预留空间须在4,000字节内，以规避 Obsidian 1.14.4 已观察到的长请求 JSON 解析错误。当前源码构建通过仅当前 Windows 用户可读的临时文件传递较长的笔记请求，校验 SHA-256 后执行随附适配器，在成功、失败或取消时删除。笔记读写仍使用 Obsidian API，保留备份及冲突检查，不回退到本地写入。已发布的 v0.0.7 包仍有旧的长文本编辑限制。同步保存／关联确认可能阻塞至超时；已执行的 JavaScript 不会取消，超时后应重新加载再重试。

日记设置／路径解析及 Tasks 依赖内部 API，并在运行时检查。未支持或禁用的插件显示错误。日记仅针对核心日记插件，不推断 Periodic Notes 等其他插件的设置。Obsidian 必须保持运行；CLI 调用时同时关闭本体可能与 CLI 自动启动竞争。

### 开发

`DailyBridge.js`读取原生设置及两天路径，不创建文件；`NoteSource`连接共用的日期保留、标题和渲染逻辑。`NotesBridge.js`使用 Vault API 处理正文，`TasksBridge.js`导出原生查询结果。JSON 保持为数据，仅对随附适配器求值；不操作剪贴板或分发原插件二进制。

测试覆盖原生路径、缺失日期、昨日保留、设置变动、仓库边界、隐藏旧设置保留、CLI 失败、过时响应及渲染一致性。普通构建还检查临时文件权限、正文保留及失败清理。安装 Node.js 后，可用`dotnet run --project tests/StickyNotes.Tests -c Release -- --note-transport-smoke`在合成文件及模拟 Vault API 上执行生成的传输代码与随附适配器，验证2 MB 保存、BOM、备份及冲突／失败时的数据保护，无需 Obsidian。可选实机测试：`dotnet run --project tests/StickyNotes.Tests -c Release -- --tasks-cli-smoke`。读取原生设置及已有笔记，创建唯一名称的合成笔记验证编码／保存／备份／冲突后删除，仅输出验证结果／版本。

参考：[官方 CLI](https://obsidian.md/help/cli)、[Tasks 查询渲染器](https://github.com/obsidian-tasks-group/obsidian-tasks/blob/7.23.1/src/Renderer/QueryResultsRenderer.ts)。
