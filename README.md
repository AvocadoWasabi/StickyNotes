[日本語](README.md) | [English](README.en.md) | [简体中文](README.zh-CN.md) | [繁體中文（台灣）](README.zh-TW.md) | [한국어](README.ko.md) | [Deutsch](README.de.md) | [Русский](README.ru.md) | [Español](README.es.md) | [Svenska](README.sv.md)

# Markdown Sticky Notes

[v0.0.11](https://github.com/AvocadoWasabi/StickyNotes/releases/tag/v0.0.11) · Windows 10 / 11 (x64)

Markdownファイルを付箋形式のウィンドウで表示・編集するWindowsアプリケーションです。ローカルファイルのほか、Obsidianのノートやデイリーノート内の特定の見出しを表示できます。編集内容とチェックボックスの変更は、元のMarkdownファイルに保存します。

Obsidian CLIによるTasks・Dataviewのクエリ結果表示と、Google Calendar・Google Tasksの取得に対応しています。

[ダウンロード](https://github.com/AvocadoWasabi/StickyNotes/releases/latest) · [導入ガイド](docs/INSTALL.md) · [操作ガイド](docs/USAGE.md) · [変更履歴](CHANGELOG.ja.md)

## スクリーンショット

| チェックボックスの操作 | Markdownの表示 | デイリーノートの見出し表示 |
| :---: | :---: | :---: |
| [<img src="docs/images/sticky-tasks-ja.png" alt="チェックボックスの操作" width="280">](docs/images/sticky-tasks-ja.png) | [<img src="docs/images/sticky-markdown-ja.png" alt="Markdownの表示" width="280">](docs/images/sticky-markdown-ja.png) | [<img src="docs/images/sticky-daily-ja.png" alt="デイリーノートの見出し表示" width="280">](docs/images/sticky-daily-ja.png) |

アプリのWPFコントロールでサンプルノートを描画した画像です。デスクトップを撮影したものではありません。[各言語の画面例と生成手順](docs/SCREENSHOTS.md)を参照してください。

## 動作環境と依存関係

対応OSはWindows 10 / 11（x64）です。配布ZIPに.NETランタイムを同梱しています。インストールに管理者権限は必要ありません。

| 用途 | 必要なツール | 設定 |
| --- | --- | --- |
| ローカルMarkdownの表示・編集 | Markdown Sticky Notes | 設定で **Obsidian CLIを使用する** をオフにし、ローカル保存先を選択 |
| Obsidianのノートを編集 | 起動中のObsidian＋Obsidian CLI | Obsidianの **設定 → 一般 → コマンドラインインターフェース** を有効化し、アプリにVaultを登録 |
| デイリーノートの表示 | 上記＋コア **デイリーノート** | Obsidianで保存フォルダ・日付形式を設定し、今日のノートを作成 |
| Tasksクエリ・チェック処理 | 上記CLI＋コミュニティプラグイン **Tasks** | 対象Vaultで有効化。v0.0.11は安定版 **8.2.2以降**が必要 |
| Dataviewクエリ | 上記CLI＋**Dataview** | 対象Vaultで有効化。LIST / TABLE / TASKに対応 |
| Googleの予定・タスク | Googleアカウント＋利用者自身のOAuth設定 | Calendar API / Tasks API、デスクトップ用OAuth JSONを用意し[認証](docs/GOOGLE-CALENDAR.md#setup) |

新規設定ではObsidian CLIが有効です。ローカルファイルのみを使用する場合は無効にしてください。CLIはObsidianの付属機能です。専用のObsidianプラグインを別途インストールする必要はありません。[公式CLIの導入条件](https://obsidian.md/help/cli)と[互換性・制約](docs/TASKS-CLI-PREVIEW.md#日本語)を参照してください。

## インストールと初期設定

1. [Releases](https://github.com/AvocadoWasabi/StickyNotes/releases/latest)から `StickyNotes-win-x64.zip` を取得し、すべて展開して `Install.cmd` を実行します。
2. ObsidianでCLIを有効にして対象Vaultを開き、Markdown Sticky Notesを起動します。
3. アプリの設定で `Obsidian.com` の絶対パスを確認し、Vault名／IDを入力し、**Vaultの保存フォルダをCLIから取得** をクリックします。保存先を選択し、**保存して閉じる** をクリックします。
4. `… → 作成・開く` から、新規作成・既存Markdownの表示・見出しの連携・デイリーノートの表示を選択します。

Obsidianを使用しない場合の設定、更新、アンインストールは[導入ガイド](docs/INSTALL.md)を参照してください。アプリの機能・設定はv0.0.11に対応しています。バージョンごとの変更は[変更履歴](CHANGELOG.ja.md#v0-0-11)に記載しています。

## 機能一覧

| 機能 | 内容 |
| --- | --- |
| Markdown表示・編集 | 見出し、リスト、チェックボックス、表、コード、リンク。通常の `.md` に保存 |
| 見出し連携・デイリーノート | ノートの一部分を表示・編集。日付切替、今日がない場合の前日保持 |
| Tasks / Dataview | 検索結果を表示し、元ノートを照合できたタスクをチェック。クエリ原文は保持 |
| タスク入力 | キーワード・日付補完、Enterで項目継続、Tabで字下げ、設定可能なチェック切替キー |
| Google連携 | `@calendar today` で予定と日付付き未完了Google Tasks。予定の件名・説明を確認後に編集 |
| 表示・ウィンドウ | 色、サイズ、50〜200%表示倍率、最前面、一時前面表示、タスクバー表示 |
| 付箋管理 | 保存先選択・移行、閉じた付箋の再表示、一覧とプレビュー、ファイル名変更・削除 |
| バックアップ・競合検出 | 書込み前のバックアップ、外部変更との競合検出 |
| 複数Vault | Vaultごとのデイリーノート、名前変更、既定Vaultの選択 |

## 基本操作

| 操作 | 方法 |
| --- | --- |
| 編集 | 本文クリック（シングル／ダブルは設定可能）、または `Ctrl+E` |
| 保存して編集終了 | `Ctrl+S` / `Esc`。補完表示中の最初のEscは候補を閉じます |
| チェック | 表示中のチェックボックスをクリック |
| 最前面 | `○ / ●` で固定、`Alt+Z` で全付箋を10秒間表示 |
| 閉じる / 終了 | `… → この付箋を閉じる` / 通知領域からアプリ終了 |

## 対応範囲と制約

- v0.0.11のUI・README・導入ガイド・画面例は**日本語・英語・簡体字中国語・台湾華語（繁体字）・韓国語・ドイツ語・ロシア語・スペイン語・スウェーデン語**の9言語です。詳細資料は原則3言語です。言語変更は保存して再起動すると反映されます。
- Obsidian連携中はObsidianの常時起動が必要です。内部APIに依存し、更新で互換性が変わる場合があります。
- デイリーノートは自動作成しません。初回は今日のノートを用意してください。
- Obsidianのテーマや全プラグインの表示は再現しません。画像は代替テキストで表示。DataviewJS・インライン式・CALENDARは未対応です。
- Google Tasksは読み取り専用で、期限は日付のみ取得できます。期限の時刻・Markdownタスクとの自動同期・Google Calendarの予定の新規作成には対応しません。
- バックアップは `%LOCALAPPDATA%/StickyNotes/backups`。競合時は下書きを控え、再読込してから再試行してください。

## 関連資料と不具合報告

[保存先と移行](docs/STICKY-FOLDERS.md#日本語) · [Tasks / CLI](docs/TASKS-CLI-PREVIEW.md#日本語) · [タスク入力](docs/TASK-EDITING.md#日本語) · [Dataview](docs/DATAVIEW.md#日本語) · [複数Vault](docs/MULTIPLE-VAULTS.md#日本語) · [付箋管理](docs/QUERY-EDITING-AND-BROWSER.md#日本語) · [Google連携](docs/GOOGLE-CALENDAR.md) · [開発](docs/DEVELOPMENT.md) · [ライセンス表記](THIRD-PARTY-NOTICES.txt)

[不具合・要望](https://github.com/AvocadoWasabi/StickyNotes/issues)には、アプリ／Windows／関連プラグインのバージョン、CLIまたはローカルの利用方式、再現手順を添えてください。個人ノート・OAuth JSON・トークンは添付しないでください。
