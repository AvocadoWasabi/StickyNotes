[English](#english) | [日本語](#日本語) | [简体中文](#简体中文)

# Query checkboxes and note browser / 検索結果のチェック操作と付箋一覧 / 查询复选框及便签列表

## English

The note's **…** menu puts Edit, Reload, Rename and opening the source file in its default app first. **Display & properties**, **Refresh linked data**, and **Create & open** group the remaining note actions. Folder management remains directly accessible; Settings and closing the current note follow, with Exit app separated at the bottom. The manager has a **Close** button at the bottom right (also Esc); closing does not delete or open a note. Close is disabled while deletion is in progress. Footer buttons wrap in narrow windows.

Unreleased fix: folder operations now use the same bounded CLI transport as note saves. When the bundled code/request exceeds the inline budget, it is sent through a temporary file restricted to the current user, verified by hash, and removed after success, failure or cancellation. The CLI length limit stays in place. This fixes the length-limit error when opening folder management, including with ordinary Vault names; folder selection, moves, renames and deletions share the fix. **Refresh list** also retries the initial folder catalog after a connection error, without requiring the window to be reopened.

v0.0.8 lets you change checkboxes in Obsidian **Tasks** results and Dataview **TASK** results. Start Obsidian with CLI and the corresponding plugin enabled. Clicking changes the original task's note, not the query text. Results refresh after saving. Sorted, grouped, duplicate and nested results use explicit source paths and line numbers rather than matching visible descriptions. Only rows with verified source mappings are enabled (up to 1,000 per block); missing/stale mappings and arbitrary checkbox text in Dataview LIST/TABLE output remain disabled.

Tasks uses the installed plugin's native status cycle and recurrence output, including its completion dates. Dataview changes only the selected task's checkbox to `x` or space; it does not apply Dataview's optional completion-date tracking or recursive child completion. Original YAML, BOM, newline style and unrelated lines are preserved. A whole-note hash check and backup run before writing through `vault.process`. External changes reject the write; refresh and retry. A failed/timed-out update clears actionable cached results; work already running in Obsidian may still finish, so refresh before retrying. Query source remains editable as before.

**Manage sticky-note folder…** is available from a note's **…** menu, the notification-area menu, and the Windows taskbar menu. It starts in the configured sticky-note folder (or Obsidian's default new-note folder) and lists Markdown files recursively, including closed notes. Choose another folder under the configured base/vault, filter by filename/path, then select a note. **Body preview** renders ordinary Markdown; **YAML** displays the original frontmatter. Both are read-only; embedded queries/scripts are not executed in this picker. Select **Open as sticky note** or double-click to open it. An already-open note is activated and restored if minimized.

**Delete note** asks you to confirm the full path and removes the entire selected Markdown file, including YAML and all sections. Unsaved edits in any view of that file block deletion. Successful deletion closes its open views and removes ordinary-file entries from recently closed history; daily-note rules remain configured. Local mode moves the file to the app data folder under `deleted-notes/<operation-id>/`; CLI mode saves a copy there before moving it to the vault trash through [Obsidian’s API](https://docs.obsidian.md/Reference/TypeScript+API/Vault/trash). Each recovery folder contains `source.json` with the original location. Restore manually by copying the Markdown file back after checking for name conflicts. Refresh before retrying a conflict or connection error; a timed-out Obsidian operation can still finish. Recovery files are not automatically purged.

Lists are limited to 1,000 Markdown notes; select a smaller folder if needed. Hidden/system/link directories are skipped in local mode. Preview text is limited to 50,000 characters; opening the note uses its full contents. CLI mode lists and reads through Obsidian, with no local fallback. Changing selection or closing the picker discards late preview responses. Missing files and connection errors are shown without modifying notes.

Validation: normal WPF tests cover menus, source validation, successful/failed writes, duplicate-click blocking, folder boundaries, read-only previews, YAML and stale selection responses. `dotnet run --project tests/StickyNotes.Tests -c Release -- --query-task-smoke` additionally uses Node.js to execute generated CLI commands on synthetic notes with mocked Tasks/Dataview APIs, verifying grouped/nested mappings, duplicate descriptions, backups, conflicts and recurrence output. Internal Tasks APIs were checked against 7.23.1; Dataview APIs against upstream source. These changes have not been tested against a running Obsidian instance. Build/install: `./build.ps1 -Publish`, then `artifacts/app/Install.cmd`.

## 日本語

付箋の **…** メニューは、編集・再読込・名前変更・元ファイルを既定アプリで開くを上段に配置しました。その他の操作は **表示・プロパティ**、**連携データの更新**、**作成・開く** に整理しています。付箋管理は直接選べる位置に残し、設定・この付箋を閉じるを下段に、アプリ終了を区切った最下段に置きます。管理画面の右下に **閉じる** ボタンを追加し、Escでも閉じられます。閉じる操作では付箋を開いたり削除したりしません。削除処理中は閉じるボタンを無効にし、狭い画面では下部のボタンを折り返します。

未リリースの修正：フォルダ操作にも本文保存と同じCLI転送方式を適用しました。同梱コード・要求が引数の上限を超える場合は、現在のユーザーのみがアクセスできる一時ファイルで渡し、ハッシュを検証し、成功・失敗・取消後に削除します。CLIの長さ上限は維持しています。通常のVault名でも付箋管理を開けなくなる長さ上限エラーを解消し、フォルダ選択・移動・名前変更・削除にも適用します。初回のフォルダ取得に失敗した場合も、画面を開き直さず **一覧を更新** で再試行できます。

v0.0.8は、Obsidianの **Tasks** 検索結果とDataviewの **TASK** 検索結果のチェック操作に対応します。CLIと対応プラグインを有効にしてObsidianを起動してください。クリックはクエリ文字列ではなく、元タスクのノートへ反映し、保存後に結果を再取得します。並べ替え・グループ化・同名・入れ子のタスクを、表示文字列で推測せず元のパス・行番号で特定します。元ノートを照合できた行だけ操作できます（1ブロック最大1,000件）。対応付けがない・古い行や、Dataview LIST／TABLEに任意の文字列として含まれるチェックボックスは無効です。

Tasksは、本家プラグインの状態遷移と繰り返し処理を利用し、完了日などもそれに従います。Dataviewは選択したタスクのチェック記号だけを`x`／空白へ変更し、任意設定の完了日記録や子タスクの再帰的な完了処理は適用しません。元のYAML・BOM・改行形式・無関係な行を保持します。`vault.process`でノート全体のハッシュを照合し、バックアップ後に書き込みます。外部変更があれば保存を拒否するため、更新後に再試行してください。失敗・タイムアウト時は操作可能な結果キャッシュを消します。Obsidian内の開始済み処理が後から完了する場合があるため、再試行前に更新してください。クエリ原文は従来どおり編集できます。

**付箋フォルダを管理する…**を、付箋の **…** メニュー・通知領域の常駐メニュー・Windowsタスクバーメニューに追加しました。設定済みの付箋フォルダ（未指定ならObsidianの新規保存先）から始まり、閉じた付箋を含めMarkdownをサブフォルダまで一覧表示します。基準フォルダ／Vault内の別フォルダへ切り替え、ファイル名・パスで絞り込み、ノートを選べます。**本文プレビュー**は通常のMarkdownを描画し、**YAML**は元のフロントマターを表示します。どちらも読み取り専用で、この画面ではクエリやスクリプトを実行しません。**付箋として開く**またはダブルクリックで開きます。既に開いている付箋は前面へ移し、最小化されていれば復元します。

**付箋を削除**はフルパスを確認後、YAML・全見出しを含む選択したMarkdownファイル全体を削除します。同じファイルを表示中の付箋に未保存の編集がある場合は拒否します。成功時は表示中の付箋を閉じ、通常ファイルの閉じた履歴から除外します。デイリーノートの表示ルールは保持します。通常方式はアプリのデータフォルダ内の `deleted-notes/<操作ID>/` へ移動し、CLI方式はそこへコピーを残してから [Obsidian API](https://docs.obsidian.md/Reference/TypeScript+API/Vault/trash) でVaultのゴミ箱へ移します。各復元フォルダの `source.json` に元の場所を記録します。復元するには同名ファイルがないか確認してMarkdownを元の場所へ手動でコピーしてください。競合・接続エラー時は更新してから再試行してください。タイムアウトしたObsidianの処理が後から完了する場合があります。復元ファイルは自動削除しません。

一覧はMarkdown最大1,000件で、多い場合は小さいフォルダを選んでください。ローカル方式では隠し・システム・リンク先のディレクトリを除外します。本文プレビューは先頭50,000文字までですが、付箋として開くと全文を扱います。CLI方式の一覧・本文はObsidian経由で取得し、ローカルへフォールバックしません。選択変更や画面を閉じた後の古い応答は破棄します。ファイルの削除や接続エラーは、ノートを変更せず画面に表示します。

検証：通常のWPFテストでメニュー、元ノート情報の検証、保存成功・失敗、連打防止、フォルダ境界、読み取り専用プレビュー、YAML、古い選択応答を確認します。`dotnet run --project tests/StickyNotes.Tests -c Release -- --query-task-smoke`はNode.jsで生成したCLIコマンドを合成ノートと模擬Tasks／Dataview APIに対して実行し、グループ・入れ子・同名タスクの対応、バックアップ、競合、繰り返し出力を確認します。内部APIはTasks 7.23.1とDataviewの上流ソースを参照しました。今回の変更は起動中のObsidianとの実機検証は未実施です。`./build.ps1 -Publish`後に`artifacts/app/Install.cmd`で導入できます。

## 简体中文

便签的 **…** 菜单将编辑、重新加载、重命名及用默认应用打开原文件放在顶部，其他操作归入 **显示与属性**、**刷新关联数据**、**创建与打开**。文件夹管理仍可直接选择；设置和关闭当前便签位于下方，退出应用单独放在底部。管理窗口右下方新增 **关闭** 按钮，也可按 Esc。关闭不会打开或删除笔记；删除过程中禁用关闭按钮，窄窗口中的底部按钮会自动换行。

尚未发布的修复：文件夹操作改用与正文保存相同的 CLI 传输方式。同捆代码／请求超过参数预算时，通过仅当前用户可访问的临时文件传递，校验哈希，并在成功、失败或取消后删除。CLI 长度上限保持不变。修复了普通 Vault 名称也可能触发的管理窗口长度超限错误，文件夹选择、移动、重命名及删除同样适用。初次获取文件夹失败后，可点击 **刷新列表** 重试，无需重新打开窗口。

v0.0.8支持修改 Obsidian **Tasks** 和 Dataview **TASK** 查询结果中的复选框。请启动已启用 CLI 及对应插件的 Obsidian。点击会更新原任务所在笔记，不会修改查询文本；保存后重新获取结果。排序、分组、同名及嵌套任务均通过明确的原路径和行号定位，不按显示文字猜测。只有已验证原任务的行可操作（每块最多 1,000 项）。缺失／过期映射及 Dataview LIST／TABLE 中任意文本形式的复选框保持禁用。

Tasks 使用已安装插件的原生状态循环和重复任务输出，包括完成日期。Dataview 仅将选中任务的复选框标记改为 `x` 或空格，不应用其可选的完成日期跟踪或递归完成子任务设置。保留原 YAML、BOM、换行格式及无关行。通过 `vault.process` 检查整篇笔记哈希并备份后写入。外部修改会拒绝保存，请刷新后重试。更新失败／超时时清除可操作的缓存结果；Obsidian 内已开始的操作可能随后完成，因此重试前先刷新。查询原文仍可正常编辑。

便签的 **…** 菜单、通知区域菜单和 Windows 任务栏菜单均提供 **管理便签文件夹…**。默认从已配置的便签文件夹（未指定时为 Obsidian 新笔记目录）开始，递归列出 Markdown，包括已关闭的便签。可在配置的基础目录／仓库内选择其他文件夹，按文件名或路径筛选并选择笔记。**正文预览** 渲染普通 Markdown，**YAML** 显示原 frontmatter。两者均只读，选择器不执行查询或脚本。点击 **作为便签打开** 或双击即可打开；已打开的便签会激活，最小化时会恢复。

**删除便签**会确认完整路径，并删除所选 Markdown 文件的全部内容，包括 YAML 和所有章节。同一文件的任一便签有未保存编辑时拒绝删除。成功后关闭显示该文件的便签，并从最近关闭历史中移除普通文件记录；保留每日笔记显示规则。本地模式将文件移动到应用数据目录的 `deleted-notes/<操作ID>/`；CLI 模式先在那里保存副本，再通过 [Obsidian API](https://docs.obsidian.md/Reference/TypeScript+API/Vault/trash) 移入仓库回收站。每个恢复文件夹的 `source.json` 记录原路径。恢复时先检查重名冲突，再手动将 Markdown 复制回原位置。遇到冲突或连接错误请刷新后重试；超时的 Obsidian 操作可能随后完成。恢复文件不会自动清理。

每次最多列出 1,000 篇 Markdown，超出时请选择更小的文件夹。本地模式跳过隐藏、系统及链接目录。正文预览最多 50,000 字符，作为便签打开时使用完整内容。CLI 模式通过 Obsidian 列举和读取，不回退到本地。切换选择或关闭窗口后丢弃过期响应。文件缺失及连接错误会在界面显示，不修改笔记。

验证：常规 WPF 测试覆盖菜单、原任务信息验证、写入成功／失败、防止重复点击、目录边界、只读预览、YAML 及过期选择响应。`dotnet run --project tests/StickyNotes.Tests -c Release -- --query-task-smoke` 使用 Node.js 对合成笔记和模拟 Tasks／Dataview API 执行生成的 CLI 命令，验证分组／嵌套／同名任务映射、备份、冲突及重复任务输出。内部 API 参照 Tasks 7.23.1 及 Dataview 上游源码；本次修改尚未在运行中的 Obsidian 上进行实机验证。运行 `./build.ps1 -Publish` 后使用 `artifacts/app/Install.cmd` 安装。
