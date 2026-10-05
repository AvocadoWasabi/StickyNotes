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

通知領域のアイコンを右クリックして「設定」を開き、付箋とデイリーノートの保存先を指定します。Obsidianと連携する場合はVault内のフォルダを選びます。Google連携を使わなければ認証設定は不要です。付箋の保存フォルダとデイリーノートのフォルダは独立して保存されます。保存先を変更して保存すると、旧フォルダ内のMarkdownファイルも移行するか確認します。「はい」でサブフォルダも含めて移行、「いいえ」で新規付箋の保存先のみ変更、「キャンセル」で設定保存を中止します。移行範囲・制限は [日本語README](../README.ja.md#保存先とobsidian) を参照してください。

詳しい操作・Google Calendarの設定は [日本語README](../README.ja.md) を参照してください。`examples` にデイリーノートとObsidian Basesの例を同梱しています。

デイリーノートの形式は従来の.NET日付書式のほか、設定のチェックボックスで正規表現に切り替えられます。曜日付きファイル名などの指定例は [デイリーノートの説明](../README.ja.md#デイリーノートの特定箇所を常に表示編集) を参照してください。

## 更新

通知領域の「終了」でアプリを終了し、新しいZIPを別のフォルダへ展開して `Install.cmd` を実行します。既存のアプリファイルを上書きします。ノート・設定・バックアップはインストール先とは別に保存されます。

## 削除

1. 通知領域の「終了」でアプリを終了します。
2. `%LOCALAPPDATA%\Programs\MarkdownStickyNotes` を削除します。
3. スタートメニューの「Markdown Sticky Notes」を右クリックしてファイルの場所を開き、ショートカットを削除します。

設定・認証情報・バックアップは `%LOCALAPPDATA%\StickyNotes`、Markdownは設定で選んだ保存先（初期値は `Documents\StickyNotesData`）に残ります。必要なデータを確認してから別途削除してください。
