# 紹介用スクリーンショット / Screenshots

2026-10-05に、バージョン0.0.2のWPFコントロールでサンプルMarkdownを描画して更新しました。付箋の内容領域を画像化しており、デスクトップのキャプチャではありません。個人のノート・設定・認証情報は使用していません。

Updated on 2026-10-05 by rendering fictional Markdown samples with version 0.0.2's WPF controls. These images show each note's content area, rather than a desktop capture. No personal notes, settings, or credentials are included. English images use English note content with the app's current Japanese controls.

| 内容 / Content | 日本語 / Japanese | English |
| --- | --- | --- |
| チェックリスト / Checklist | [画像](images/sticky-tasks-ja.jpg) | [Image](images/sticky-tasks-en.jpg) |
| Markdownメモ / Markdown note | [画像](images/sticky-markdown-ja.jpg) | [Image](images/sticky-markdown-en.jpg) |
| デイリー連動 / Linked daily note | [画像](images/sticky-daily-ja.jpg) | [Image](images/sticky-daily-en.jpg) |

各画像は460 × 580 pxのJPEGです。READMEのサムネイルから原寸の画像を開けます。

Each image is a 460 × 580 px JPEG. README thumbnails link to the original images.

## 同じサンプルをアプリで表示 / View the samples in the app

Windowsでリポジトリのルートから実行します。起動中のSticky Notesがあれば、未保存の内容を保存して通知領域の「終了」で終了してください。アプリは同時に複数のプロセスを起動できません。

Run these commands from the repository root on Windows. If Sticky Notes is already running, save your edits and choose `終了` (Exit) from its notification-area menu first; the app allows only one running instance.

```powershell
./build.ps1 -Publish

# Japanese samples / 日本語サンプル
$demoData = ./scripts/Prepare-ScreenshotDemo.ps1 -Language ja
& ./artifacts/app/StickyNotes.exe --data-dir $demoData

# Exit the app before switching languages / 終了後に英語版を起動
$demoData = ./scripts/Prepare-ScreenshotDemo.ps1 -Language en
& ./artifacts/app/StickyNotes.exe --data-dir $demoData
```

スクリプトは実行ごとに `artifacts/screenshot-demo-<GUID>` を作り、その中にサンプル・デイリーノート・設定を配置します。個人の設定は変更しません。日付は実行日のものになります。各付箋を前面に出し、ウィンドウ単位で撮影してください。

The script creates a fresh `artifacts/screenshot-demo-<GUID>` directory containing sample notes, a daily note for the current date, and isolated settings. Personal settings are not changed. Bring each note to the foreground and capture only that window.

サンプルは [日本語](../examples/screenshots/Tasks.md) / [English](../examples/screenshots/en/Tasks.md) と同じフォルダの `Project.md`、`Daily.md` にあります。配布ZIPにもサンプルと画像を同梱しています。撮影スクリプトはソースリポジトリに含まれます。

The sample folders contain `Tasks.md`, `Project.md`, and `Daily.md` in [Japanese](../examples/screenshots/Tasks.md) and [English](../examples/screenshots/en/Tasks.md). The distribution ZIP includes the sample notes and screenshots; the capture preparation script is available in the source repository.
