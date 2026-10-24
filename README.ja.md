[日本語](README.md) | [English](README.en.md) | [简体中文](README.zh-CN.md) | [繁體中文](README.zh-TW.md) | [한국어](README.ko.md)

# Markdown Sticky Notes

**Obsidianの「今日やること」を、デスクトップに置いて、その場でチェック。**

ノート全体でも、デイリーノートの見出しひとつでも。必要な部分だけをWindowsの付箋にして、編集を元のMarkdownへ保存します。Tasks・Dataviewの検索結果やGoogle Calendarの予定も手元に置けます。

[ダウンロード](https://github.com/AvocadoWasabi/StickyNotes/releases/latest) · [導入ガイド](docs/INSTALL.md) · [操作ガイド](docs/USAGE.md) · [変更履歴](CHANGELOG.ja.md)

## こんな使い方に

- **今日のタスクを見失わない** — デイリーノートの「Tasks」だけを表示し、日付に合わせて切り替え。
- **メモを二重管理しない** — 編集・チェックは元の `.md` に反映。閉じてもファイルは残ります。
- **作業中にすぐ見返せる** — 最前面に固定、または `Alt+Z` で全付箋を10秒間表示。

## スクリーンショット

| タスクをその場でチェック | Markdownでメモ | 今日のデイリーと連動 |
| :---: | :---: | :---: |
| [<img src="docs/images/sticky-tasks-ja.png" alt="タスクをその場でチェック" width="280">](docs/images/sticky-tasks-ja.png) | [<img src="docs/images/sticky-markdown-ja.png" alt="Markdownでメモ" width="280">](docs/images/sticky-markdown-ja.png) | [<img src="docs/images/sticky-daily-ja.png" alt="今日のデイリーと連動" width="280">](docs/images/sticky-daily-ja.png) |

現在の開発版のWPFコントロールで架空のノートを描画した画面例です（デスクトップの撮影ではありません）。[5言語の画面例・再生成手順](docs/SCREENSHOTS.md)。

## 必要なツールと設定

**Windows 10 / 11（x64）**。配布ZIPには.NETを同梱し、管理者権限は不要です。

| 使いたいこと | 必要なもの | 最初の設定 |
| --- | --- | --- |
| Markdown付箋だけ | このアプリ | 設定で **Obsidian CLIを使用する** をオフにし、ローカル保存先を選択 |
| Obsidianのノートを編集 | 起動中のObsidian＋Obsidian CLI | Obsidianの **設定 → 一般 → コマンドラインインターフェース** を有効化し、アプリにVaultを登録 |
| 今日のデイリーノート | 上記＋コア **デイリーノート** | Obsidianで保存フォルダ・日付形式を設定し、今日のノートを作成 |
| Tasks検索・本家のチェック処理 | 上記CLI＋コミュニティプラグイン **Tasks** | 対象Vaultで有効化。現開発版は安定版 **8.2.2以降**が必要 |
| Dataview検索 | 上記CLI＋**Dataview** | 対象Vaultで有効化。LIST / TABLE / TASKに対応 |
| Googleの予定・タスク | Googleアカウント＋利用者自身のOAuth設定 | Calendar API / Tasks API、デスクトップ用OAuth JSONを用意し[認証](docs/GOOGLE-CALENDAR.md#setup) |

**新規設定はCLIがオンです。** Obsidianを使わない場合はオフにしてください。CLIはObsidian付属機能で、このアプリ専用のObsidianプラグインは不要です。[公式CLIの導入条件](https://obsidian.md/help/cli)・[互換性と制限](docs/TASKS-CLI-PREVIEW.md#日本語)。

## はじめる

1. [Releases](https://github.com/AvocadoWasabi/StickyNotes/releases/latest)から `StickyNotes-win-x64.zip` を取得し、すべて展開して `Install.cmd` を実行。
2. ObsidianでCLIを有効にして対象Vaultを開き、Markdown Sticky Notesを起動。
3. アプリの設定で `Obsidian.com` の絶対パスを確認し、Vault名／IDを入力して **Vaultの保存フォルダをCLIから取得**。保存先を選び、**保存して閉じる**。
4. `… → 作成・開く` から新しい付箋、既存Markdown、ノートの一部分、デイリーノートを選択。

CLIを使わない手順、設定場所、更新・削除は[導入ガイド](docs/INSTALL.md)へ。ここで案内する設定画面・画像は現開発版です。公開ZIPとの差分（複数Vault・Tasks 8.2.2対応など）は[未リリース欄](CHANGELOG.ja.md#未リリース)を確認してください。

## 機能一覧

| 機能 | できること |
| --- | --- |
| Markdown表示・編集 | 見出し、リスト、チェックボックス、表、コード、リンク。通常の `.md` に保存 |
| 見出し連動・デイリー | ノートの一部分を表示・編集。日付切替、今日がない場合の前日保持 |
| Tasks / Dataview | 検索結果を表示し、元ノートを照合できたタスクをチェック。クエリ原文は保持 |
| タスク入力 | キーワード・日付補完、Enterで項目継続、Tabで字下げ、設定可能なチェック切替キー |
| Google連携 | `@calendar today` で予定と日付付き未完了Google Tasks。予定の件名・説明を確認後に編集 |
| 表示・ウィンドウ | 色、サイズ、50〜200%表示倍率、最前面、一時前面表示、タスクバー表示 |
| 付箋管理 | 保存先選択・移行、閉じた付箋の再表示、一覧とプレビュー、ファイル名変更・削除 |
| データ保護 | 書込み前のバックアップ、外部変更との競合検出 |
| 複数Vault（未リリース） | Vaultごとのデイリーノート、名前変更、既定Vaultの選択 |

## 基本操作

| 操作 | 方法 |
| --- | --- |
| 編集 | 本文クリック（シングル／ダブルは設定可能）、または `Ctrl+E` |
| 保存して編集終了 | `Ctrl+S` / `Esc`。補完表示中の最初のEscは候補を閉じます |
| チェック | 表示中のチェックボックスをクリック |
| 最前面 | `○ / ●` で固定、`Alt+Z` で全付箋を10秒間表示 |
| 閉じる / 終了 | `… → この付箋を閉じる` / 通知領域からアプリ終了 |

## 知っておきたい制限

- UIは**日本語・英語・簡体字中国語**。README・導入・画面例は**繁体字中国語・韓国語**も用意しています。詳細資料は原則3言語です。
- Obsidian連携中はObsidianの常時起動が必要です。内部APIに依存し、更新で互換性が変わる場合があります。
- デイリーノートは自動作成しません。初回は今日のノートを用意してください。
- Obsidianのテーマや全プラグインの表示は再現しません。画像は代替テキストで表示。DataviewJS・インライン式・CALENDARは未対応です。
- Google Tasksは読み取り専用で、APIから取得できるのは日付のみです。時刻・Markdownタスクとの自動同期・予定の新規作成には対応しません。
- バックアップは `%LOCALAPPDATA%/StickyNotes/backups`。競合時は下書きを控え、再読込してから再試行してください。

## 詳しく知る・問題を報告する

[保存先と移行](docs/STICKY-FOLDERS.md#日本語) · [Tasks / CLI](docs/TASKS-CLI-PREVIEW.md#日本語) · [タスク入力](docs/TASK-EDITING.md#日本語) · [Dataview](docs/DATAVIEW.md#日本語) · [複数Vault](docs/MULTIPLE-VAULTS.md#日本語) · [付箋管理](docs/QUERY-EDITING-AND-BROWSER.md#日本語) · [Google連携](docs/GOOGLE-CALENDAR.md) · [開発](docs/DEVELOPMENT.md) · [ライセンス表記](THIRD-PARTY-NOTICES.txt)

[不具合・要望](https://github.com/AvocadoWasabi/StickyNotes/issues)には、アプリ／Windows／関連プラグインの版、CLIかローカルか、再現手順を添えてください。個人ノート・OAuth JSON・トークンは添付しないでください。
