[English](#english) | [日本語](#日本語) | [简体中文](#简体中文)

# Markdown Sticky Notes v0.0.7

## 日本語

Windows 10 / 11（x64）向け。Obsidian CLI連携と、付箋の保存フォルダ選択・移行を通常配布版に追加しました。

- **ObsidianのTasksを利用**：インストール済みTasksの本家クエリ結果を通常本文と同居表示。結果は読み取り専用・約30秒更新で、クエリ原文を上書きしません。通常の本文・チェックボックスは編集できます。
- **本文・デイリー設定の取得を統一**：CLI有効時は読込・見出し・作成・保存をObsidian経由に統一し、ローカル読込へフォールバックしません。コアDaily notesのフォルダ・日付書式・テンプレートを取得し、Vault全件検索なしで今日・昨日を解決。未作成時の待機・前日保持を選べます。
- **付箋保存先を選択・作成**：Vaultまたはローカル基準フォルダの決定後、既存フォルダを選ぶか、新しい相対パスを入力。「決定」後に既存のアプリ作成付箋を移行するか「はい／いいえ／キャンセル」で選びます。下書き・配置を保持し、同名上書きを防止。移動・設定保存の失敗時は巻き戻します。
- **取得経路と配布を整理**：通常本文の描画・バックアップ・競合検出を維持。長いCLI要求を制限してObsidianのJSON解析エラーを抑えます。独立試験版はソースから任意に作成でき、通常版と設定を分離します。

### インストール・更新と初期設定

アプリを終了し、Assetsの **StickyNotes-win-x64.zip** を別フォルダへ全体展開して **Install.cmd** を実行してください。ノート・設定・バックアップは保持します。管理者権限や.NETの追加インストールは不要です。未署名です。SHA256は `.zip.sha256` で確認できます。

新規設定はCLIが既定です。Obsidianで **設定 → 一般 → コマンドラインインターフェース** を有効にし、対象Vaultを開いて起動状態を保ちます。付箋の **設定 → Obsidian CLI → 開いているVaultをObsidianから取得** で接続後、**付箋の保存フォルダを選択…** を使います。デイリーにはコアDaily notes、クエリにはTasksが必要です。既存の明示的なCLI無効設定は保持し、設定で無効にすればObsidianなしでローカルファイルを使えます。

**v0.0.6以前からの更新：** CLI選択項目がない旧設定もCLIが有効になるため、Vaultの設定が必要です。従来のローカル方式を続ける場合は、設定の **Obsidian CLIを使用する** を無効にして保存してください。既存ファイルは自動移行しません。

### 制限と検証

Obsidian 1.14.4／Tasks 7.23.1で確認。内部API依存のため他バージョンでは動作が変わる可能性があります。付箋側のWPF描画であり、ObsidianのHTML/CSS・操作部品の完全再現ではありません。長い編集はCLI上限によりObsidianでの保存が必要な場合があります。移行は旧保存先配下の`type: sticky`とUUIDの`id`を持つ付箋が対象。別Vault／取得方式間の移行と相対リンクの書換えは未対応。強制終了時には復旧記録の手動確認が必要です。

自動検証759件に加え、合成データで本家CLIの読込・保存・フォルダ移行・巻き戻しを検証しました。既知の低影響事項として、一時的なハッシュ不一致／保存競合をまれに観測しています。上書きは拒否され、再試行は通過しましたが原因未特定です。下書きを控え、再読込後に再試行してください。

[使い方](https://github.com/AvocadoWasabi/StickyNotes/blob/v0.0.7/README.ja.md) · [CLI設定・制限](https://github.com/AvocadoWasabi/StickyNotes/blob/v0.0.7/docs/TASKS-CLI-PREVIEW.md#日本語) · [保存先・移行・復旧](https://github.com/AvocadoWasabi/StickyNotes/blob/v0.0.7/docs/STICKY-FOLDERS.md#日本語) · [変更履歴](https://github.com/AvocadoWasabi/StickyNotes/blob/v0.0.7/CHANGELOG.ja.md)

## English

For Windows 10 / 11 (x64). The standard release now includes Obsidian CLI integration and sticky-note folder selection and migration.

- **Use Obsidian Tasks:** Show native query output from the installed Tasks plugin alongside ordinary text. Results are read-only, refresh about every 30 seconds and never overwrite query source. Ordinary text and checkboxes remain editable.
- **Unify text and daily settings:** With CLI enabled, reads, headings, creation and saves all go through Obsidian, without local-read fallback. Obtain core Daily notes folder, date format and template; resolve today/yesterday without searching the whole vault. Choose to wait or retain yesterday when today is missing.
- **Select or create a sticky-note folder:** After choosing a vault or local base, select an existing folder or enter a new relative path. Confirm asks Yes/No/Cancel about moving existing app-created sticky notes. Preserve drafts and positions, prevent same-name overwrites and roll back move/settings-save failures.
- **Consolidate access and packaging:** Retain ordinary-text rendering, backups and conflict checks. Bound long CLI requests to mitigate Obsidian JSON parsing errors. An optional isolated preview can still be built from source with a separate profile.

### Installation, updates and setup

Exit the app, extract all of **StickyNotes-win-x64.zip** from Assets into a separate folder, and run **Install.cmd**. Notes, settings and backups are retained. No administrator privileges or separate .NET installation are required. The app is unsigned; verify SHA256 with `.zip.sha256`.

New settings default to CLI. In Obsidian, enable **Settings → General → Command line interface**, open the intended vault and keep Obsidian running. In the sticky app, use **Settings → Obsidian CLI → Get open vault from Obsidian**, then **Select sticky-note folder…**. Daily notes require the core Daily notes plugin; queries require Tasks. Existing explicit CLI opt-outs are preserved. Disable CLI in Settings to use local files without Obsidian.

**Upgrading from v0.0.6 or earlier:** settings without a CLI selection also default to CLI and require vault setup. To continue with local files, uncheck **Use Obsidian CLI** in Settings and save. Existing files are not moved automatically.

### Limits and validation

Verified with Obsidian 1.14.4 / Tasks 7.23.1. Internal APIs may change in other versions. The sticky app uses WPF rendering; it does not fully reproduce Obsidian HTML/CSS or interactive controls. Large edits may exceed CLI limits and need saving in Obsidian. Migration covers notes under the previous save folder with `type: sticky` and a UUID `id`. Cross-vault/access-mode migration and relative-link rewriting are unsupported. Abrupt interruption may require manually checking the recovery record.

759 automated checks, plus native CLI read/save and folder move/rollback checks on synthetic data. Known low-impact issue: occasional transient hash mismatches/save conflicts in live tests. Overwrites were refused and retries passed, but the cause remains unisolated. Preserve draft text and reload before retrying.

[User guide](https://github.com/AvocadoWasabi/StickyNotes/blob/v0.0.7/README.md) · [CLI setup and limits](https://github.com/AvocadoWasabi/StickyNotes/blob/v0.0.7/docs/TASKS-CLI-PREVIEW.md#english) · [Folders, migration and recovery](https://github.com/AvocadoWasabi/StickyNotes/blob/v0.0.7/docs/STICKY-FOLDERS.md#english) · [Changelog](https://github.com/AvocadoWasabi/StickyNotes/blob/v0.0.7/CHANGELOG.md)

## 简体中文

支持 Windows 10 / 11（x64）。普通发布版新增 Obsidian CLI 集成，以及便签保存目录选择和迁移。

- **使用 Obsidian Tasks**：在普通正文中显示已安装 Tasks 插件的原生查询结果。结果只读、约30秒刷新，不覆盖查询原文。普通正文和复选框仍可编辑。
- **统一正文与日记设置**：启用 CLI 时，读取、标题、新建及保存均通过 Obsidian，不回退到本地读取。获取核心 Daily notes 的目录、日期格式及模板，无需搜索整个仓库即可解析今天、昨天。今日缺失时可等待或保留昨天。
- **选择或创建便签目录**：确定仓库或本地基准后，选择已有目录或输入新相对路径。确认后通过是／否／取消决定是否迁移已有应用便签。保留草稿和布局，防止同名覆盖，移动或保存设置失败时回滚。
- **统一访问和打包**：保留普通正文渲染、备份和冲突检查，限制长 CLI 请求以缓解 Obsidian JSON 解析错误。仍可从源码构建使用独立配置的可选试验版。

### 安装、更新及设置

退出应用，将 Assets 的 **StickyNotes-win-x64.zip** 全部解压到另一目录，运行 **Install.cmd**。保留笔记、设置和备份。无需管理员权限或另装 .NET。应用未签名，可通过 `.zip.sha256` 验证 SHA256。

新配置默认使用 CLI。在 Obsidian 启用 **设置 → 常规 → 命令行界面**，打开目标仓库并保持运行。在便签中通过 **设置 → Obsidian CLI → 从 Obsidian 获取当前仓库** 连接，再点击 **选择便签保存文件夹…**。日记需要核心 Daily notes 插件，查询需要 Tasks。保留已有明确禁用 CLI 的选择；也可在设置中禁用 CLI，不运行 Obsidian 而使用本地文件。

**从 v0.0.6 或更早版本升级：** 无 CLI 选项的旧配置也默认启用 CLI，需要设置仓库。继续使用本地文件时，请在设置中取消 **使用 Obsidian CLI** 并保存。已有文件不会自动迁移。

### 限制与验证

已验证 Obsidian 1.14.4／Tasks 7.23.1；内部 API 在其他版本中可能变化。便签使用 WPF 渲染，不完全重现 Obsidian HTML/CSS 和交互控件。长编辑可能超出 CLI 限制，需在 Obsidian 保存。迁移仅包含旧目录下具有`type: sticky`和 UUID `id`的便签。不支持跨仓库／访问模式迁移，也不改写相对链接。强制中断后可能需手动核对恢复记录。

通过759项自动检查，并以合成数据验证原生 CLI 读写、目录迁移和回滚。已知低影响问题：实机偶尔出现临时哈希不一致／保存冲突。覆盖被拒绝，重试通过，但原因仍未确定。请保留草稿，重新加载后重试。

[使用指南](https://github.com/AvocadoWasabi/StickyNotes/blob/v0.0.7/README.zh-CN.md) · [CLI 设置与限制](https://github.com/AvocadoWasabi/StickyNotes/blob/v0.0.7/docs/TASKS-CLI-PREVIEW.md#简体中文) · [目录、迁移及恢复](https://github.com/AvocadoWasabi/StickyNotes/blob/v0.0.7/docs/STICKY-FOLDERS.md#简体中文) · [更新日志](https://github.com/AvocadoWasabi/StickyNotes/blob/v0.0.7/CHANGELOG.zh-CN.md)
