[English](#english) | [日本語](#日本語) | [简体中文](#简体中文)

# Multiple Vaults / 複数Vault / 多 Vault

## English

Since v0.0.11: existing settings become **Main**; the first added Vault starts as **Sub**. These display names can be changed at any time and do not select the Obsidian connection.

1. Open **Settings → Vaults & daily notes**. Select a registration on the left, or **Add Vault**. Edit its display name and select CLI or local access. The common `Obsidian.com` executable path is under **General**.
2. For CLI, enter the Obsidian **Vault name/ID** (not the Main/Sub display name), then click **Get Vault folder via CLI** to fill the absolute Vault root. Typing alone does not fetch the root. An empty name uses Obsidian's default selection (working-directory Vault, otherwise active Vault) and fills its name too. The screen shows fetching, success or failure; Save is disabled during fetching. Failure keeps the inputs; edits, profile switches or closing discard stale results. Explicit IDs are preserved. Keep the target Vault open in Obsidian with the required plugins enabled. Manual root entry remains available; this does not enumerate all saved Vaults. See the [official CLI targeting rules](https://help.obsidian.md/cli#Target+a+vault).
   After retrieval, the native daily-note folder is checked automatically (connection edits also refresh the preview). This does not choose the ordinary sticky-note subfolder; select that separately for Main. Retrieved fields remain drafts until **Save and close**. For multiple Vaults, specify each CLI target explicitly.
3. **Check connection / today’s note** shows the native Daily notes folder, date format, template, today's path and existence. These are read-only in CLI mode. In local mode, edit the daily folder and tagged regex and check the resolved filename. Choose a missing-today retention policy separately for each Vault.
4. Choose the **Default Vault for new daily notes**, then **Save and close**. Profile edits and sticky-folder/migration choices remain drafts until this save. Cancel or closing the settings window discards them. Explicit Google credential import/deletion and completed authentication keep their existing immediate effects; Google login/check no longer saves unrelated settings.
5. **Display a daily note** starts on the default Vault. Choose a Vault, then a heading. Section linking also offers a Vault selector. The title stays the Markdown filename without `.md`; a separate small label identifies the Vault.

Placements store an immutable registration ID. Renaming a display name or changing the default never redirects existing notes, drafts, or recently closed placements. Reads, writes, Tasks/Dataview queries, query checkbox changes and task completion use that registration. A disconnected Vault shows an error; it never falls back to another Vault or direct local access. Changing a connection while it has unsaved edits is rejected. A stale snapshot cannot be saved through a changed connection. Both identical dates/headings in different Vaults can stay open.

**Remove registration** never deletes Markdown or the Vault folder. The main registration, registrations used by open notes, and registrations in the latest closed-note group cannot be removed. Restoring or replacing that closed group releases its history references. Changing an existing connection clears only that registration's closed history; other Vaults are retained. Register the new Vault separately when you want to keep the old history.

Ordinary new notes, tray file opening and the sticky-folder migration setting continue using the main registration, even after renaming its display name. A note's folder-management menu uses that note's Vault; the tray folder-management menu uses Main. No cross-Vault file migration is added. The same root cannot be registered twice. CLI calls retain the existing shared queue and timeout; slow calls can delay another Vault. Existing flat source settings remain the main registration's backing fields for migration compatibility; additional registrations are stored in `AdditionalVaults`, with IDs in placements and `DefaultDailyVaultId`.

Validation uses synthetic local Vaults and simulated CLI transport for independent daily reads/writes, query routing, failure isolation, rename/default persistence, cancel and removal guards. Running two real Obsidian Vaults concurrently remains a manual verification step. See [CLI setup](TASKS-CLI-PREVIEW.md#english) and [folder migration](STICKY-FOLDERS.md#english).

## 日本語

v0.0.11以降の機能です。既存設定は **メイン**、最初に追加するVaultは **サブ** になります。表示名はあとから変更でき、Obsidianへの接続先名とは別です。

1. **設定 → Vault・デイリーノート**を開きます。左の登録を選ぶか **Vaultを追加** し、表示名とCLI／ローカル方式を設定します。共通の `Obsidian.com` 実行ファイルは **一般** にあります。
2. CLIではObsidianの **Vault名／ID**（メイン／サブの表示名とは別）を入力し、**Vaultの保存フォルダをCLIから取得**を押すと、Vaultルートの絶対パスが入力されます。名前の入力だけではルートを取得しません。名前が空欄ならObsidianの既定選択（作業ディレクトリのVault、該当しなければアクティブなVault）を使い、名前も入力します。取得中・完了・失敗を表示し、取得中は設定を保存できません。失敗時は入力内容を保持し、編集・登録切替・画面を閉じた後の古い結果は破棄します。明示したIDも保持します。対象VaultをObsidianで開き、必要なプラグインを有効にしてください。ルートの手入力も可能で、保存済みVaultの一括列挙は行いません。[本家CLIの接続先指定](https://help.obsidian.md/cli#Target+a+vault)も参照してください。
   取得後は本家デイリーノートの保存先を自動確認します（接続先の編集時もプレビューを更新）。通常の付箋用サブフォルダは別途メインで選択します。取得した設定は **保存して閉じる** まで下書きです。複数Vaultでは各CLI接続先名を明示してください。
3. **接続・今日のノートを確認**で、CLIでは本家のDaily notesフォルダ・日付書式・テンプレート・今日のパスと存在状態を読み取り専用で表示します。ローカル方式ではデイリーフォルダと日時タグ付き正規表現を編集し、一致ファイル名を確認します。今日の分がない場合の保持方法もVaultごとに選べます。
4. **新しいデイリー付箋の既定Vault**を選び、**保存して閉じる**を押します。Vault設定と付箋フォルダ・移行方法の選択はここまで下書きで、キャンセルや設定画面を閉じる操作で破棄します。Google認証JSONの明示的な取り込み・削除と完了した認証は従来どおり即時反映しますが、Googleログイン・接続確認は他の未保存設定を保存しません。
5. **デイリーノートを表示**は既定Vaultから始まり、Vaultを選んでから見出しを指定します。見出し連携にもVault選択があります。タイトルはMarkdownのファイル名（`.md`を除く）を保ち、その下に小さくVault名を表示します。

付箋は変更されない登録IDを保持します。表示名・既定Vaultを変更しても、既存付箋・下書き・直前に閉じた付箋の参照先は変わりません。本文の読み書き、Tasks／Dataviewの検索・検索結果のチェック操作、タスク補完はその登録を使います。切断時は対象付箋にエラーを表示し、別Vaultやローカル取得へ切り替えません。未保存編集がある接続先の変更は拒否し、変更前の古い表示内容を変更後の接続から保存することも防止します。同じ日付・見出しでも異なるVaultなら同時に開けます。

**登録を解除**してもMarkdownやVaultフォルダは削除しません。メイン登録、表示中の付箋が使用する登録、直前に閉じた付箋グループが使用する登録は解除できません。そのグループを復元または別の閉じる操作で置き換えると、履歴からの参照がなくなります。既存の接続先を変更した場合はその登録の閉じた履歴だけを整理し、他のVaultの履歴を保持します。旧履歴を保持する場合は新しいVaultを別途登録してください。

通常の付箋の新規作成・常駐メニューからのファイル選択・保存先移行設定は、表示名の変更後もメイン登録を使います。付箋のフォルダ管理メニューはその付箋のVault、常駐メニューのフォルダ管理はメインが対象です。Vault間のファイル移行は追加していません。同じルートの二重登録はできません。CLIは既存の共有キューとタイムアウトを維持するため、遅い要求が他のVaultを待たせることがあります。旧形式の設定項目をメインの保存先として保持し、追加登録を `AdditionalVaults`、付箋の参照IDと既定を各配置および `DefaultDailyVaultId` に保存します。

合成したローカルVaultと模擬CLIで、デイリーの独立した読み書き・クエリ接続先・切断時の分離・表示名／既定設定の永続化・キャンセル・登録解除の保護を検証します。実際に2つのObsidian Vaultを同時利用する確認は手動検証として残ります。[CLI設定](TASKS-CLI-PREVIEW.md#日本語)・[保存先移行](STICKY-FOLDERS.md#日本語)も参照してください。

## 简体中文

从v0.0.11起：现有设置成为 **主 Vault**，首次添加的 Vault 默认为 **副 Vault**。显示名称可随时修改，与 Obsidian 连接名称相互独立。

1. 打开 **设置 → Vault 与每日笔记**，选择左侧注册项或 **添加 Vault**，填写显示名称并选择 CLI 或本地方式。共用的 `Obsidian.com` 路径位于 **常规**。
2. CLI 方式先输入 Obsidian **Vault 名称／ID**（与主／副显示名称不同），再点击 **通过 CLI 获取 Vault 保存目录**，填入 Vault 根目录绝对路径。仅输入名称不会获取根目录。名称留空时使用 Obsidian 的默认选择（工作目录中的 Vault，否则为活动 Vault），并填入其名称。界面显示获取中、成功或失败，获取期间无法保存设置。失败保留输入；编辑、切换注册项或关闭窗口后丢弃过期结果。显式 ID 也会保留。请在 Obsidian 中打开目标 Vault 并启用所需插件。仍可手动输入根目录，不枚举全部已保存 Vault。参见[官方 CLI 目标规则](https://help.obsidian.md/cli#Target+a+vault)。
   获取后自动检查原生每日笔记保存目录（编辑连接时也会更新预览）。普通便签子目录需在主注册中另行选择。获取的设置在 **保存并关闭** 前仍为草稿。使用多个 Vault 时需明确指定各 CLI 目标。
3. **检查连接及今日笔记**在 CLI 方式下只读显示原生 Daily notes 文件夹、日期格式、模板、今日路径及存在状态。本地方式可编辑每日笔记文件夹和带日期标签的正则表达式并检查匹配文件名。今日笔记不存在时的保留策略按 Vault 分别设置。
4. 选择 **新建每日便签的默认 Vault**，然后 **保存并关闭**。Vault 设置及便签文件夹／迁移选择在此之前均为草稿，取消或关闭设置窗口会丢弃它们。Google 凭据 JSON 的显式导入／删除及已完成的认证仍立即生效，但 Google 登录／连接检查不再保存其他未保存设置。
5. **显示每日笔记**从默认 Vault 开始，先选择 Vault 再选择标题。章节关联也提供 Vault 选择。便签标题保持 Markdown 文件名（不含 `.md`），下方的小标签显示 Vault 名称。

便签保存不可变的注册 ID。修改显示名称或默认 Vault 不会改变已有便签、草稿及最近关闭便签的来源。正文读写、Tasks／Dataview 查询、结果复选框操作及任务补全均使用该注册。断开时仅在相关便签显示错误，不回退到其他 Vault 或本地访问。有未保存修改时拒绝变更连接，旧快照也不能经更改后的连接保存。不同 Vault 的同日期、同标题便签可同时打开。

**移除注册**不会删除 Markdown 或 Vault 文件夹。主注册、打开便签使用的注册，以及最近关闭便签组使用的注册无法移除。恢复或用另一组关闭操作替换该组可释放历史引用。更改现有连接只清理该注册的关闭历史，保留其他 Vault 的历史；若需保留旧历史，请单独添加新 Vault。

普通便签的新建、托盘菜单的文件选择及保存文件夹迁移始终使用主注册，即使已更改显示名称。便签中的文件夹管理使用该便签的 Vault，托盘中的文件夹管理使用主注册。不新增跨 Vault 文件迁移，也不允许重复注册同一根目录。CLI 保留共用队列及超时设置，因此慢请求可能延迟其他 Vault。旧的平面来源字段继续存储主注册，附加注册存入 `AdditionalVaults`，引用 ID 存入各便签，默认值存入 `DefaultDailyVaultId`。

使用合成本地 Vault 和模拟 CLI 验证每日笔记独立读写、查询路由、故障隔离、名称／默认值持久化、取消及移除保护。两个实际 Obsidian Vault 同时运行仍需手动验证。参见 [CLI 设置](TASKS-CLI-PREVIEW.md#简体中文)及[文件夹迁移](STICKY-FOLDERS.md#简体中文)。
