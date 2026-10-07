[English](#english) | [日本語](#日本語) | [简体中文](#简体中文)

# Query checkboxes and note browser / 検索結果のチェック操作と付箋一覧 / 查询复选框及便签列表

## English

Current source builds let you change checkboxes in Obsidian **Tasks** results and Dataview **TASK** results. Start Obsidian with CLI and the corresponding plugin enabled. Clicking changes the original task's note, not the query text. Results refresh after saving. Sorted, grouped, duplicate and nested results use explicit source paths and line numbers rather than matching visible descriptions. Only rows with verified source mappings are enabled (up to 1,000 per block); missing/stale mappings and arbitrary checkbox text in Dataview LIST/TABLE output remain disabled.

Tasks uses the installed plugin's native status cycle and recurrence output, including its completion dates. Dataview changes only the selected task's checkbox to `x` or space; it does not apply Dataview's optional completion-date tracking or recursive child completion. Original YAML, BOM, newline style and unrelated lines are preserved. A whole-note hash check and backup run before writing through `vault.process`. External changes reject the write; refresh and retry. A failed/timed-out update clears actionable cached results; work already running in Obsidian may still finish, so refresh before retrying. Query source remains editable as before.

**Browse sticky-note folder…** is available from a note's **…** menu, the notification-area menu, and the Windows taskbar menu. It starts in the configured sticky-note folder (or Obsidian's default new-note folder) and lists Markdown files recursively, including closed notes. Choose another folder under the configured base/vault, filter by filename/path, then select a note. **Body preview** renders ordinary Markdown; **YAML** displays the original frontmatter. Both are read-only; embedded queries/scripts are not executed in this picker. Select **Open as sticky note** or double-click to open it. An already-open note is activated and restored if minimized.

Lists are limited to 1,000 Markdown notes; select a smaller folder if needed. Hidden/system/link directories are skipped in local mode. Preview text is limited to 50,000 characters; opening the note uses its full contents. CLI mode lists and reads through Obsidian, with no local fallback. Changing selection or closing the picker discards late preview responses. Missing files and connection errors are shown without modifying notes.

Validation: normal WPF tests cover menus, source validation, successful/failed writes, duplicate-click blocking, folder boundaries, read-only previews, YAML and stale selection responses. `dotnet run --project tests/StickyNotes.Tests -c Release -- --query-task-smoke` additionally uses Node.js to execute generated CLI commands on synthetic notes with mocked Tasks/Dataview APIs, verifying grouped/nested mappings, duplicate descriptions, backups, conflicts and recurrence output. Internal Tasks APIs were checked against 7.23.1; Dataview APIs against upstream source. These changes have not been tested against a running Obsidian instance. Build/install: `./build.ps1 -Publish`, then `artifacts/app/Install.cmd`. Published v0.0.7 packages are unchanged.

## 日本語

現在のソースビルドは、Obsidianの **Tasks** 検索結果とDataviewの **TASK** 検索結果のチェック操作に対応します。CLIと対応プラグインを有効にしてObsidianを起動してください。クリックはクエリ文字列ではなく、元タスクのノートへ反映し、保存後に結果を再取得します。並べ替え・グループ化・同名・入れ子のタスクを、表示文字列で推測せず元のパス・行番号で特定します。元ノートを照合できた行だけ操作できます（1ブロック最大1,000件）。対応付けがない・古い行や、Dataview LIST／TABLEに任意の文字列として含まれるチェックボックスは無効です。

Tasksは、本家プラグインの状態遷移と繰り返し処理を利用し、完了日などもそれに従います。Dataviewは選択したタスクのチェック記号だけを`x`／空白へ変更し、任意設定の完了日記録や子タスクの再帰的な完了処理は適用しません。元のYAML・BOM・改行形式・無関係な行を保持します。`vault.process`でノート全体のハッシュを照合し、バックアップ後に書き込みます。外部変更があれば保存を拒否するため、更新後に再試行してください。失敗・タイムアウト時は操作可能な結果キャッシュを消します。Obsidian内の開始済み処理が後から完了する場合があるため、再試行前に更新してください。クエリ原文は従来どおり編集できます。

**付箋フォルダから開く…**を、付箋の **…** メニュー・通知領域の常駐メニュー・Windowsタスクバーメニューに追加しました。設定済みの付箋フォルダ（未指定ならObsidianの新規保存先）から始まり、閉じた付箋を含めMarkdownをサブフォルダまで一覧表示します。基準フォルダ／Vault内の別フォルダへ切り替え、ファイル名・パスで絞り込み、ノートを選べます。**本文プレビュー**は通常のMarkdownを描画し、**YAML**は元のフロントマターを表示します。どちらも読み取り専用で、この画面ではクエリやスクリプトを実行しません。**付箋として開く**またはダブルクリックで開きます。既に開いている付箋は前面へ移し、最小化されていれば復元します。

一覧はMarkdown最大1,000件で、多い場合は小さいフォルダを選んでください。ローカル方式では隠し・システム・リンク先のディレクトリを除外します。本文プレビューは先頭50,000文字までですが、付箋として開くと全文を扱います。CLI方式の一覧・本文はObsidian経由で取得し、ローカルへフォールバックしません。選択変更や画面を閉じた後の古い応答は破棄します。ファイルの削除や接続エラーは、ノートを変更せず画面に表示します。

検証：通常のWPFテストでメニュー、元ノート情報の検証、保存成功・失敗、連打防止、フォルダ境界、読み取り専用プレビュー、YAML、古い選択応答を確認します。`dotnet run --project tests/StickyNotes.Tests -c Release -- --query-task-smoke`はNode.jsで生成したCLIコマンドを合成ノートと模擬Tasks／Dataview APIに対して実行し、グループ・入れ子・同名タスクの対応、バックアップ、競合、繰り返し出力を確認します。内部APIはTasks 7.23.1とDataviewの上流ソースを参照しました。今回の変更は起動中のObsidianとの実機検証は未実施です。`./build.ps1 -Publish`後に`artifacts/app/Install.cmd`で導入できます。公開済みv0.0.7の配布物は変更しません。

## 简体中文

当前源码构建支持修改 Obsidian **Tasks** 和 Dataview **TASK** 查询结果中的复选框。请启动已启用 CLI 及对应插件的 Obsidian。点击会更新原任务所在笔记，不会修改查询文本；保存后重新获取结果。排序、分组、同名及嵌套任务均通过明确的原路径和行号定位，不按显示文字猜测。只有已验证原任务的行可操作（每块最多 1,000 项）。缺失／过期映射及 Dataview LIST／TABLE 中任意文本形式的复选框保持禁用。

Tasks 使用已安装插件的原生状态循环和重复任务输出，包括完成日期。Dataview 仅将选中任务的复选框标记改为 `x` 或空格，不应用其可选的完成日期跟踪或递归完成子任务设置。保留原 YAML、BOM、换行格式及无关行。通过 `vault.process` 检查整篇笔记哈希并备份后写入。外部修改会拒绝保存，请刷新后重试。更新失败／超时时清除可操作的缓存结果；Obsidian 内已开始的操作可能随后完成，因此重试前先刷新。查询原文仍可正常编辑。

便签的 **…** 菜单、通知区域菜单和 Windows 任务栏菜单均提供 **从便签文件夹打开…**。默认从已配置的便签文件夹（未指定时为 Obsidian 新笔记目录）开始，递归列出 Markdown，包括已关闭的便签。可在配置的基础目录／仓库内选择其他文件夹，按文件名或路径筛选并选择笔记。**正文预览** 渲染普通 Markdown，**YAML** 显示原 frontmatter。两者均只读，选择器不执行查询或脚本。点击 **作为便签打开** 或双击即可打开；已打开的便签会激活，最小化时会恢复。

每次最多列出 1,000 篇 Markdown，超出时请选择更小的文件夹。本地模式跳过隐藏、系统及链接目录。正文预览最多 50,000 字符，作为便签打开时使用完整内容。CLI 模式通过 Obsidian 列举和读取，不回退到本地。切换选择或关闭窗口后丢弃过期响应。文件缺失及连接错误会在界面显示，不修改笔记。

验证：常规 WPF 测试覆盖菜单、原任务信息验证、写入成功／失败、防止重复点击、目录边界、只读预览、YAML 及过期选择响应。`dotnet run --project tests/StickyNotes.Tests -c Release -- --query-task-smoke` 使用 Node.js 对合成笔记和模拟 Tasks／Dataview API 执行生成的 CLI 命令，验证分组／嵌套／同名任务映射、备份、冲突及重复任务输出。内部 API 参照 Tasks 7.23.1 及 Dataview 上游源码；本次修改尚未在运行中的 Obsidian 上进行实机验证。运行 `./build.ps1 -Publish` 后使用 `artifacts/app/Install.cmd` 安装。已发布的 v0.0.7 包保持不变。
