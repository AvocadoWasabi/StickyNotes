[English](INSTALL.en.md) | [日本語](INSTALL.md) | [简体中文](INSTALL.zh-CN.md)

# インストール・更新・削除

## 必要な環境

Windows 10 / 11の64bit（x64）環境向けです。.NETの追加インストール・管理者権限は不要です。ローカルモードではObsidianプラグインも不要です。v0.0.7の新規設定はCLIが既定で、CLIを有効にしたObsidianの起動が必要です。デイリーにはコアDaily notes、TasksクエリにはTasksプラグインを使います。[CLI設定手順](TASKS-CLI-PREVIEW.md#日本語)を参照してください。

**v0.0.6以前からの更新：** CLI選択項目がない旧設定もCLIが有効になるため、Vaultの設定が必要です。従来のローカル方式を続ける場合は、設定の **Obsidian CLIを使用する** を無効にして保存してください。既存ファイルは自動移行しません。

## インストール

1. [Releases](https://github.com/AvocadoWasabi/StickyNotes/releases)で `StickyNotes-win-x64.zip` をダウンロードします。`Source code` は開発者向けです。
2. ZIPを右クリックして「すべて展開」します。
3. 展開先の `Install.cmd` をダブルクリックします。
4. スタートメニューの「Markdown Sticky Notes」から起動します。

アプリは `%LOCALAPPDATA%\Programs\MarkdownStickyNotes` に配置されます。自動起動は登録しません。未署名のためWindowsが確認を表示することがあります。配布元とファイルを確認してから実行してください。

インストールせず使う場合は、展開したフォルダの `StickyNotes.exe` を起動します。EXE以外のファイルも必要なので、フォルダ全体を保持してください。

## 最初の設定

設定で表示言語を選び、保存して再起動できます。[言語設定の詳細](USAGE.md#language)。

「設定 → Obsidian CLI」の「開いているVaultをObsidianから取得」で接続し、デイリー設定を確認します。Vault（CLI）またはローカルの基準フォルダを決めたら、**付箋の保存フォルダを選択…**を押します。一覧選択または新しい相対パスの入力後、**決定**で既存のアプリ作成付箋も移行するか選びます。**はい**は移行、**いいえ**は今後の保存先だけ変更、**キャンセル**は変更しません。対象範囲・巻き戻し・復旧については[保存先と移行のガイド](STICKY-FOLDERS.md#日本語)を参照してください。

- [基本操作](../README.ja.md#基本操作)
- [保存先の移行・デイリーノート設定](USAGE.md#storage)
- [Google Calendarの初回認証（任意）](GOOGLE-CALENDAR.md#setup)

Google連携を使わなければ認証設定は不要です。

## 更新

通知領域の「終了」でアプリを終了し、新しいZIPを別のフォルダへ展開して `Install.cmd` を実行します。既存のアプリファイルを上書きします。ノート・設定・バックアップはインストール先とは別に保存されます。

0.0.1から更新した場合は、付箋の保存フォルダ・デイリーノートのフォルダと今日の一致ファイル名を設定で確認してください。旧版の標準的な日付書式は日時タグ付き正規表現へ自動変換しますが、独自の書式は手動修正が必要です。確認なしの自動保存は初期状態ではオフです。詳しくは [変更履歴](../CHANGELOG.ja.md) を参照してください。

## 削除

1. 通知領域の「終了」でアプリを終了します。
2. `%LOCALAPPDATA%\Programs\MarkdownStickyNotes` を削除します。
3. スタートメニューの「Markdown Sticky Notes」を右クリックしてファイルの場所を開き、ショートカットを削除します。

設定・認証情報・バックアップは `%LOCALAPPDATA%\StickyNotes`、Markdownは設定で選んだ保存先（初期値は `Documents\StickyNotesData`）に残ります。必要なデータを確認してから別途削除してください。
