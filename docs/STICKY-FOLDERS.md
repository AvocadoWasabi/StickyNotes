[English](#english) | [日本語](#日本語) | [简体中文](#简体中文)

# Sticky-note folders / 付箋の保存先 / 便签保存位置

## English

This feature is in the unreleased local build; published v0.0.6 packages are unchanged.

1. In Settings, select the Obsidian vault, or disable CLI and choose a local base folder.
2. Select **Select sticky-note folder…**. The app retrieves folders under that root. Choose one from the editable list, or type a new relative path such as `Sticky Notes/Work`. `.` selects the root.
3. Select **Confirm**, then **Yes** to also migrate existing app-created sticky notes, **No** to change only the destination for new notes, or **Cancel** to make no changes. Confirming saves the Settings form; a new folder is created only after Yes/No. Saving a changed vault/base folder also opens this selection flow.

CLI mode lists/creates folders, reads candidates and moves notes through Obsidian, without direct local Markdown access or fallback. An explicit sticky folder takes precedence over Obsidian's general new-note location; older profiles with no selection continue using Obsidian's location. Local mode uses the selected absolute base and its chosen subfolder. Daily settings remain separate. There is no automatic migration merely from switching access modes.

Migration covers notes in the previous save folder and subfolders whose frontmatter has `type: sticky` and a UUID `id`, including closed sticky notes. Ordinary linked notes and daily notes without these app markers are excluded. Relative subpaths are retained; the destination subtree is excluded when moving into a child folder. Open source paths and snapshots follow the move while drafts and positions remain. Markdown contents and relative links are not rewritten; links may need adjustment after moving. Cross-vault and CLI/local-mode migration is rejected; choose No to change only the destination.

Existing target files are never overwritten. Content hashes are checked before moves; a move or settings-persistence failure triggers rollback. Empty folders created during a failed operation may remain. If rollback cannot complete (for example, Obsidian disconnects), the error identifies both paths and a private `%LOCALAPPDATA%/StickyNotes/folder-migrations/*.json` recovery record. Do not retry blindly: check both paths against the record. Abrupt process/power loss can leave partially moved notes; there is no automatic journal replay. Normal exit and note writes are blocked during migration. CLI operations are also queued inside Obsidian so rollback follows any earlier timed-out operation.

Local enumeration is bounded to 10,000 entries; CLI folder listing to 10,000 folders; migration to 1,000 app-marked notes. The CLI's existing request/response limits and 25-second per-request timeout still apply. Folder names cannot escape the selected root or name hidden/reserved paths. Obsidian metadata indexing may lag recently created or edited markers; refresh/retry after indexing to include those notes. CLI support depends on Obsidian's Vault APIs. Private recovery records, user notes, settings and build output are not committed or included in packages.

Validation: local migration/rollback/collision and UI confirmation tests run with the normal suite. `dotnet run --project tests/StickyNotes.Tests -c Release -- --folders-cli-smoke` creates a unique synthetic tree in the active vault, verifies native moves and rollback, and deletes only that tree.

## 日本語

未リリースのローカルビルドの機能です。公開済みv0.0.6の配布物は変更しません。

1. 設定でObsidianのVaultを指定します。ローカル方式ではCLIを無効にし、基準フォルダを選びます。
2. **付箋の保存フォルダを選択…** を押すと、基準内のフォルダ一覧を取得します。一覧から選ぶか、`付箋/仕事`のような新しい相対パスを入力します。`.`は直下です。
3. **決定**後、**はい**で既存のアプリ作成付箋も移行、**いいえ**で今後の保存先だけ変更、**キャンセル**で変更しません。確定時に設定画面の内容を保存し、新規フォルダは「はい／いいえ」の後に作成します。Vault・基準フォルダを変更して設定保存した場合も、この選択画面を開きます。

CLIモードでは一覧取得・フォルダ作成・対象本文の取得・移動をObsidian経由に統一し、ローカルMarkdownの直接取得へ切り替えません。指定した付箋フォルダはObsidian全般の新規保存先より優先します。未指定の旧設定はObsidianの保存先を継続使用します。ローカル方式では絶対パスの基準フォルダと選択したサブフォルダを使います。デイリー設定は独立しており、取得方式を切り替えただけでは移行しません。

移行対象は以前の保存先とサブフォルダ内で、フロントマターに`type: sticky`とUUID形式の`id`がある付箋です。閉じている付箋も含みます。通常のリンク先ノートや、これらのアプリ用印を持たないデイリーは対象外です。相対的な階層を維持し、子フォルダへ移す場合は移行先の中を対象から除外します。開いている付箋の参照先・スナップショットを更新し、下書き・配置を保持します。本文・相対リンクは書き換えないため、移動後にリンクの調整が必要な場合があります。異なるVault間・CLI／ローカル方式間の移行は拒否し、「いいえ」なら保存先だけ変更できます。

同名の移行先は上書きせず、移動前に本文ハッシュを確認します。移動・設定保存の失敗時は巻き戻します。途中で作成した空フォルダが残る場合はあります。Obsidian切断などで巻き戻せない場合、エラーに両方のパスと、非公開の`%LOCALAPPDATA%/StickyNotes/folder-migrations/*.json`復旧記録を表示します。そのまま再実行せず、記録と両パスを確認してください。強制終了・電源断では移行途中の状態が残る可能性があり、記録の自動再実行は行いません。移行中は通常終了とノートへの書込を止めます。Obsidian内でも処理を直列化し、タイムアウトした先行処理の後に巻き戻します。

ローカル列挙は10,000項目、CLI一覧は10,000フォルダ、移行はアプリ用印のある付箋1,000件までです。CLI既存の通信量制限・1要求25秒のタイムアウトも適用されます。基準外・隠しパス・予約名は指定できません。作成・印の編集直後はObsidianのメタデータ索引に遅れがあり、索引更新後の再取得が必要な場合があります。CLI側はObsidianのVault APIに依存します。復旧記録・個人ノート・設定・ビルド出力はコミットや配布物に含めません。

検証：通常テストでローカル移行・巻き戻し・衝突・確認UIを検証します。`dotnet run --project tests/StickyNotes.Tests -c Release -- --folders-cli-smoke`は開いているVaultに一意な合成フォルダを作り、本家の移動・巻き戻しを確認後、その合成フォルダのみ削除します。

## 简体中文

此功能用于未发布的本地构建；已发布的v0.0.6安装包不变。

1. 在设置中选择Obsidian仓库；本地模式则禁用CLI并选择基准文件夹。
2. 点击**选择便签保存文件夹…**，获取基准内的目录列表。选择已有目录，或输入`便签/工作`等新的相对路径。`.`表示根目录。
3. 点击**确定**后，选择**是**迁移已有的应用便签、**否**仅更改新便签保存位置，或**取消**不做更改。确认会保存设置表单；新文件夹仅在选择是／否后创建。更改仓库或基准目录并保存设置时，也会打开此选择流程。

CLI模式通过Obsidian完成列表获取、建目录、读取候选正文及移动，不回退到直接读取本地Markdown。显式便签目录优先于Obsidian的一般新笔记位置；未选择目录的旧配置继续使用Obsidian位置。本地模式使用绝对基准目录及所选子目录。日记设置独立，单纯切换访问模式不会自动迁移。

迁移范围为旧保存目录及子目录中，前置元数据含`type: sticky`及UUID格式`id`的便签，包括已关闭便签。普通关联笔记及无这些应用标记的日记不迁移。保留相对子路径；移入子目录时排除目标子树。打开便签的引用路径及快照随之更新，草稿及布局保留。正文及相对链接不改写，移动后可能需调整链接。不支持跨仓库或CLI／本地模式迁移，选择否可仅更改保存位置。

不覆盖同名目标文件，移动前核对正文哈希。移动或设置保存失败会回滚，但已创建的空目录可能保留。Obsidian断开等导致回滚失败时，错误会显示两处路径及私有`%LOCALAPPDATA%/StickyNotes/folder-migrations/*.json`恢复记录。请核对记录及两处路径后再操作，不要盲目重试。强制结束进程或断电可能留下部分移动状态，记录不会自动重放。迁移时禁止普通退出及笔记写入；Obsidian内也串行处理，确保回滚在先前超时操作之后执行。

本地枚举限10,000项，CLI列表限10,000目录，迁移限1,000个带应用标记的便签；既有CLI传输限制及每请求25秒超时仍适用。目录不可越出基准或使用隐藏／保留路径。刚创建或编辑标记时Obsidian索引可能延迟，应在索引更新后刷新重试。CLI依赖Obsidian Vault API。私有恢复记录、个人笔记、设置及构建输出不提交或打包。

验证：常规测试覆盖本地迁移、回滚、冲突及确认UI。`dotnet run --project tests/StickyNotes.Tests -c Release -- --folders-cli-smoke`在活动仓库创建唯一合成目录，验证原生移动和回滚后仅删除该合成目录。
