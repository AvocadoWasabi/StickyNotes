[日本語](INSTALL.md) | [English](INSTALL.en.md) | [简体中文](INSTALL.zh-CN.md) | [繁體中文（台灣）](INSTALL.zh-TW.md) | [한국어](INSTALL.ko.md) | [Deutsch](INSTALL.de.md) | [Русский](INSTALL.ru.md) | [Español](INSTALL.es.md) | [Svenska](INSTALL.sv.md)

# インストールと最初の設定

[READMEへ戻る](../README.md) · [詳しい操作](USAGE.md)

## 1. アプリを用意する

Windows 10 / 11（x64）用です。管理者権限・.NETの追加インストールは不要です。

1. [Releases](https://github.com/AvocadoWasabi/StickyNotes/releases/latest)から **StickyNotes-win-x64.zip** をダウンロード（`Source code` は開発用）。
2. ZIPをすべて展開し、`Install.cmd` を実行。
3. スタートメニューの **Markdown Sticky Notes** を起動。

インストールせず使う場合は、展開先の `StickyNotes.exe` を起動し、フォルダ全体を保持します。アプリは未署名で、Windowsの確認が出る場合があります。自動起動は登録しません。

以下は現開発版の設定名です。公開ZIPと異なる場合は[変更履歴](../CHANGELOG.ja.md#未リリース)を確認してください。

## 2. 保存方法を選ぶ

### Obsidianを使う（新規設定の既定）

1. Obsidianの **設定 → 一般 → コマンドラインインターフェース** をオンにし、登録案内に従います。項目がなければ[公式CLIガイド](https://obsidian.md/help/cli)のインストーラー更新条件を確認してください。
2. 対象Vaultを開き、Obsidianを起動したままにします。
3. 付箋の **設定 → 一般** で `Obsidian.com` の絶対パスを確認します。通常は `C:\Program Files\Obsidian\Obsidian.com`。別の場所にインストールした場合は合わせます。
4. **Vault・デイリーノート** で対象登録のCLI使用を有効にし、ObsidianのVault名／IDを入力して **Vaultの保存フォルダをCLIから取得** を押します。空欄ならObsidianの既定選択が対象です。入力だけでは取得されません。
5. 取得したVaultとデイリー設定を確認し、**付箋の保存フォルダを選択…** で保存先を指定。最後に **保存して閉じる**。

保存先変更時の「はい」は既存のアプリ作成付箋も移行、「いいえ」は今後の保存先のみ変更、「キャンセル」は変更中止です。[移行範囲と復旧](STICKY-FOLDERS.md#日本語)。CLI接続失敗時にローカル読込へ自動で切り替わることはありません。

### Obsidianなしで使う

設定で **Obsidian CLIを使用する** をオフにし、ローカルの基準フォルダ・付箋保存先を選んで保存します。初期保存先は `Documents/StickyNotesData`。Tasks／Dataviewクエリは使えません。ローカルのデイリー連動はフォルダと日時タグ付き正規表現で設定できます。[デイリー設定例](USAGE.md#daily)。

## 3. 必要な連携だけ有効にする

| 連携 | Obsidian／Google側 | 付箋側 |
| --- | --- | --- |
| デイリーノート | コアのデイリーノートを有効化し、フォルダ・日付形式を設定。今日のノートを作成 | `… → 作成・開く → デイリーノートを表示…` で見出し選択。空欄なら全体 |
| Tasks | 対象VaultでTasksを有効化。現開発版は安定版8.2.2以降 | `tasks` コードブロックを含むノートを開く。[手順・制限](TASKS-CLI-PREVIEW.md#日本語) |
| Dataview | 対象VaultでDataviewを有効化 | `dataview` のLIST / TABLE / TASKを表示。[手順・制限](DATAVIEW.md#日本語) |
| Google | 自分のCloudプロジェクトでCalendar API・Tasks APIとデスクトップOAuthを準備 | JSONを取り込み認証後、独立した段落に `@calendar today`。[接続ガイド](GOOGLE-CALENDAR.md#setup) |

通常のMarkdownチェックボックスにTasksは不要です。タスク補完の既定は自動で、Tasks無効時は手動ONにするまで停止します。Google連携を使わなければGoogle認証は不要です。

## 4. 最初の付箋を使う

`… → 作成・開く` から新規作成または既存Markdownを開きます。本文クリック／`Ctrl+E`で編集、`Ctrl+S`／`Esc`で保存します。デイリーは初回に今日のファイルが必要で、自動作成しません。表示言語は設定から変更し、保存後の再起動で反映します。現開発版のUIはREADMEに記載した9言語に対応します。

## 困ったとき

| 症状 | 確認すること |
| --- | --- |
| CLI接続できない | Obsidianの起動、CLI有効化、`Obsidian.com`の絶対パス、Vault名／IDを確認し、取得ボタンを再実行 |
| 今日のノートが出ない | コアのデイリーノート設定と今日の実ファイルを確認。他のデイリープラグインの設定は自動取得しません |
| Tasksの更新案内が出る | 対象Vaultのコミュニティプラグイン更新からTasksを8.2.2以降にし、付箋のTasks / Dataviewを再取得 |
| 保存競合になる | 下書きを控えて再読込。古い内容での上書きを止める動作です |
| Google認証できない | [段階別の対処](GOOGLE-CALENDAR.md)でAPI、OAuth、テストユーザー、権限を確認 |

## 更新・削除とデータ

**更新：** 未保存の内容を保存して通知領域から終了し、新しいZIPを別フォルダに展開して `Install.cmd` を実行。アプリを更新してもノート・設定・バックアップは別保存です。

**v0.0.6以前から：** CLI選択がない旧設定もCLIがオンになります。ローカル方式を続けるならオフにして保存してください。ファイルは自動移行しません。v0.0.1の独自デイリー書式は手動修正が必要な場合があります。

| データ | 場所 |
| --- | --- |
| インストールされたアプリ | `%LOCALAPPDATA%/Programs/MarkdownStickyNotes` |
| 設定・認証情報 | `%LOCALAPPDATA%/StickyNotes` |
| 書込み前のバックアップ | `%LOCALAPPDATA%/StickyNotes/backups` |
| ノート | 選択したVault／フォルダ |

**削除：** アプリを終了し、インストール先フォルダとスタートメニューのショートカットを削除します。ノート・設定・バックアップは残るため、必要な内容を確認してから別途削除してください。
