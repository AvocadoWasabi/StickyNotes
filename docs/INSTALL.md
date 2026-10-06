[English](INSTALL.en.md) | [日本語](INSTALL.md) | [简体中文](INSTALL.zh-CN.md)

# インストール・更新・削除

## 必要な環境

Windows 10 / 11の64bit（x64）環境向けです。.NETの追加インストール、管理者権限、Obsidianプラグインは不要です。

## インストール

1. [Releases](https://github.com/AvocadoWasabi/StickyNotes/releases)で `StickyNotes-win-x64.zip` をダウンロードします。`Source code` は開発者向けです。
2. ZIPを右クリックして「すべて展開」します。
3. 展開先の `Install.cmd` をダブルクリックします。
4. スタートメニューの「Markdown Sticky Notes」から起動します。

アプリは `%LOCALAPPDATA%\Programs\MarkdownStickyNotes` に配置されます。自動起動は登録しません。未署名のためWindowsが確認を表示することがあります。配布元とファイルを確認してから実行してください。

インストールせず使う場合は、展開したフォルダの `StickyNotes.exe` を起動します。EXE以外のファイルも必要なので、フォルダ全体を保持してください。

## 最初の設定

ソースをビルドしたアプリでは、設定で表示言語を選び、保存して再起動できます。[言語設定の詳細](USAGE.md#language)。公開済みv0.0.4の画面は日本語です。

通知領域アイコンの「設定」で付箋の保存先を選びます。Obsidianと使う場合はVault内のフォルダを指定してください。デイリーノートのフォルダは別に設定します。

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
