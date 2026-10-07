[English](README.md) | [日本語](README.ja.md) | [简体中文](README.zh-CN.md)

# Markdown Sticky Notes

試験機能：[Tasks CLIの独立テスト版・導入手順](docs/TASKS-CLI-PREVIEW.md#日本語)（`./build.ps1 -Publish -TasksPreview`）。
連携を有効にすると、本文取得・デイリーノート検索をObsidian CLIへ統一し、ローカル読込にはフォールバックしません。無効時は従来のファイルアクセスを維持します。

<img src="docs/images/StickyNotes.png" alt="Markdown Sticky Notes アイコン" width="112">

Windows向けのMarkdown付箋アプリです。Obsidianのノートやデイリーノートを直接編集し、Google Calendarの予定を並べて表示できます。Obsidianなしでも使えます。

**Windows 10 / 11（x64）対応。** 画面・資料ともに英語・日本語・簡体字中国語に対応しています。 [表示言語](docs/USAGE.md#language)。Google連携は任意で、利用者自身のOAuth設定が必要です。

[ダウンロード](#ダウンロードとインストール) · [基本操作](#基本操作) · [Google Calendar](#google-calendar) · [補足資料](#補足資料)

<details>
<summary>スクリーンショット（クリックで展開）</summary>

バージョン0.0.2のWPFコントロールで、紹介用サンプルを描画した付箋画面です。画像をクリックすると元のサイズで表示できます。

| 手元にチェックリスト | Markdownでメモ | デイリーノートと連動 |
| :---: | :---: | :---: |
| [<img src="docs/images/sticky-tasks-ja.jpg" alt="日本語のチェックリストと完了済みタスクを表示した黄色の付箋" width="300">](docs/images/sticky-tasks-ja.jpg) | [<img src="docs/images/sticky-markdown-ja.jpg" alt="日本語の見出し・番号付きリスト・表をMarkdownで表示した青い付箋" width="300">](docs/images/sticky-markdown-ja.jpg) | [<img src="docs/images/sticky-daily-ja.jpg" alt="今日のデイリーノートのTasks見出しと連動する緑の付箋" width="300">](docs/images/sticky-daily-ja.jpg) |

撮影用の架空データを使用しています。[サンプルファイルと撮影手順](docs/SCREENSHOTS.md)も同梱しています。

</details>

## ダウンロードとインストール

1. [v0.0.6の配布ページ](https://github.com/AvocadoWasabi/StickyNotes/releases/tag/v0.0.6)から `StickyNotes-win-x64.zip` をダウンロードして展開します。
2. `Install.cmd` を実行し、スタートメニューの「Markdown Sticky Notes」から起動します。
3. 通知領域アイコンの「設定」で付箋の保存先を選びます。

管理者権限や.NETの追加インストールは不要です。未署名のためWindowsの確認が出る場合があります。更新・削除・インストールせず使う方法は[導入ガイド](docs/INSTALL.md)を参照してください。

<a id="起動"></a>

## 基本操作

| 操作 | 方法 |
| --- | --- |
| 新しい付箋 | `＋`、または通知領域の「新しい付箋」 |
| 編集・保存 | 本文をクリック／`Ctrl+E` → 編集 → `Ctrl+S` |
| タスクの完了 | チェックボックスをクリックして元のMarkdownへ保存 |
| 移動・サイズ変更 | 上部をドラッグ／端や右下をドラッグ |
| 最前面表示 | `○ / ●`。全付箋の10秒間の一時表示は `…` メニュー |
| 表示サイズ | `… → 表示スケール`（50～200%） |
| 閉じる・終了 | `×` は付箋だけ閉じる。アプリ終了は通知領域の「終了」 |

編集欄からフォーカスが外れると、変更の保存を確認します。確認なしの自動保存は設定で有効にできます。閉じてもMarkdownは削除されません。

タスクバー表示、操作ボタンの表示方法、ショートカットの詳細は[操作・設定の補足](docs/USAGE.md#editing)へ。

## 保存先とObsidian

初期保存先は `Documents/StickyNotesData`。設定でVault内のフォルダを選ぶと、Obsidianと同じ `.md` を編集できます。CLIやプラグインは不要です。

保存先変更時は既存Markdownの移行も選べます。移行対象と制約は[保存先の補足](docs/USAGE.md#storage)を確認してください。

## デイリーノートの特定箇所を常に表示・編集

1. 設定でデイリーノートのフォルダと日時タグ付き正規表現を指定し、一致ファイル名を確認して保存します。
2. `… → デイリーノートを表示…` から見出しを選び、「表示」を押します。空欄なら本文全体を表示します。
3. 付箋で本文を編集したり、タスクをチェックしたりすると元のノートに反映されます。

初回追加には今日のファイルが必要です。以降、今日の分が未作成なら待機するか、設定で昨日の分を表示できます。[ファイル名の例と切替設定](docs/USAGE.md#daily)を参照してください。

固定ノートの一部分を表示するには `… → ノートの一部分を付箋にする…` を使います。[見出しの選択](docs/USAGE.md#headings)へ。

## Google Calendar

<a id="初回認証"></a>

最初に設定画面の右ペインで[接続ガイド](docs/GOOGLE-CALENDAR.md#setup)に従って認証します。その後、独立した段落に次の行を書いて保存します。

```text
@calendar 2026-10-06T09:00
```

**Google Tasksの予定時刻はAPIの仕様上取得できません。** Google側で時刻を設定していても、APIから取得できるのは日付のみのため、このアプリでは「時刻なし」と表示します。

Google Tasksも読み取り専用で表示します。`@calendar today` では当日の日付が付いた未完了タスクを取得します。既存ユーザーはTasks APIの有効化とTasks読み取りを許可する再認証が必要です。[Tasksの設定手順](docs/GOOGLE-CALENDAR.md#tasks)を参照してください。

`@calendar today` はPCの当日（ローカル時刻の0時から翌日0時まで）に自動で追従します。`@calendar today 会議` のように絞り込みもできます。付箋の表示中は約60秒ごとに日付確認と予定取得を行い、スリープ復帰後も追従します。編集中は自動更新を一時停止します。

**キーワード・タイムゾーンなしで検索できます。** 省略時はWindowsのローカル時刻を使用します。絞り込む場合は末尾に `会議` などを追加します。

編集中に独立した段落で `@` を入力すると、補完候補と入力例が表示されます。Tab・Enter・クリックで今日の `00:00` を挿入し、選択された日時を変更できます。挿入した日付は固定です。

予定の件名・説明はクリックして編集し、確認後にGoogleへ反映できます。Markdownタスクとの自動同期や予定の作成には対応していません。[検索条件・JSON管理・トラブル対処](docs/GOOGLE-CALENDAR.md)へ。

## Markdown対応範囲・データ保護

見出し・リスト・タスク・表・コード・リンクに対応します。画像は代替テキストで表示し、Obsidian固有の記法には未対応のものがあります。

外部変更との競合時は上書きを止め、書込み前の内容を `%LOCALAPPDATA%/StickyNotes/backups` に保存します。[対応範囲と復旧の補足](docs/USAGE.md#data)へ。

## 補足資料

| 調べたいこと | 資料 |
| --- | --- |
| インストール・更新・削除 | [導入ガイド](docs/INSTALL.md) |
| 操作・保存先・デイリーノート | [操作・設定の補足](docs/USAGE.md) |
| Googleの認証・予定検索 | [Google Calendarガイド](docs/GOOGLE-CALENDAR.md) |
| バージョンごとの変更 | [変更履歴](CHANGELOG.ja.md) |
| ビルド・検証・公開 | [開発ガイド](docs/DEVELOPMENT.md) |
| サンプル・ライセンス | [デイリーノート](examples/Daily.md) · [Obsidian Bases](examples/StickyNotes.base) · [ライセンス](THIRD-PARTY-NOTICES.txt) |
