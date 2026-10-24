[日本語](SCREENSHOTS.md) | [English](SCREENSHOTS.en.md) | [简体中文](SCREENSHOTS.zh-CN.md) | [繁體中文](SCREENSHOTS.zh-TW.md) | [한국어](SCREENSHOTS.ko.md)

# スクリーンショットと再生成

現在の開発版のWPFコントロールで、架空のサンプルを460 × 580 pxのPNGに描画しています。デスクトップ撮影・Obsidian／Googleへの接続ではありません。個人ノート・設定・認証情報は使いません。

日本語・英語・簡体字中国語は本文とUIを合わせています。繁体字中国語・韓国語は本文のみ翻訳し、UIは英語です。UIの対応言語は増えていません。画像のファイル名の日付は生成日のものです。

| Language / 言語 / 语言 | Tasks | Markdown | Daily |
| --- | --- | --- | --- |
| 日本語 | [![日本語 tasks](images/sticky-tasks-ja.png)](images/sticky-tasks-ja.png) | [![日本語 markdown](images/sticky-markdown-ja.png)](images/sticky-markdown-ja.png) | [![日本語 daily](images/sticky-daily-ja.png)](images/sticky-daily-ja.png) |
| English | [![English tasks](images/sticky-tasks-en.png)](images/sticky-tasks-en.png) | [![English markdown](images/sticky-markdown-en.png)](images/sticky-markdown-en.png) | [![English daily](images/sticky-daily-en.png)](images/sticky-daily-en.png) |
| 简体中文 | [![简体中文 tasks](images/sticky-tasks-zh-CN.png)](images/sticky-tasks-zh-CN.png) | [![简体中文 markdown](images/sticky-markdown-zh-CN.png)](images/sticky-markdown-zh-CN.png) | [![简体中文 daily](images/sticky-daily-zh-CN.png)](images/sticky-daily-zh-CN.png) |
| 繁體中文 | [![繁體中文 tasks](images/sticky-tasks-zh-TW.png)](images/sticky-tasks-zh-TW.png) | [![繁體中文 markdown](images/sticky-markdown-zh-TW.png)](images/sticky-markdown-zh-TW.png) | [![繁體中文 daily](images/sticky-daily-zh-TW.png)](images/sticky-daily-zh-TW.png) |
| 한국어 | [![한국어 tasks](images/sticky-tasks-ko.png)](images/sticky-tasks-ko.png) | [![한국어 markdown](images/sticky-markdown-ko.png)](images/sticky-markdown-ko.png) | [![한국어 daily](images/sticky-daily-ko.png)](images/sticky-daily-ko.png) |

## 再生成（開発者向け）

Windowsと.NET 8 SDKでリポジトリのルートから実行します。ローカルSDKを使う場合はdotnetを.tools/dotnet/dotnet.exeに読み替えてください。15枚を生成します。生成後は全画像の折返し・欠け・文字化けを確認し、3言語以上の説明を同期してください。

```powershell
./build.ps1
dotnet run --project tests/StickyNotes.Tests -c Release --no-build -- --render-screenshots examples/screenshots docs/images
```

## アプリで試す

Languageにはja / en / zh-CN / zh-TW / koを指定できます。毎回artifacts内に新しいデータフォルダを作り、CLIを無効にした独立設定を使います。既存アプリが動作中なら未保存の内容を保存して終了後に起動してください。

```powershell
$demoData = ./scripts/Prepare-ScreenshotDemo.ps1 -Language ko
& ./artifacts/app/StickyNotes.exe --data-dir $demoData
```

[日本語](../examples/screenshots/Tasks.md) · [English](../examples/screenshots/en/Tasks.md) · [简体中文](../examples/screenshots/zh-CN/Tasks.md) · [繁體中文](../examples/screenshots/zh-TW/Tasks.md) · [한국어](../examples/screenshots/ko/Tasks.md)

過去のJPEGとcompact-body-v0.0.9.pngは旧版の参考画像として保持しています。READMEは今回生成したPNGを参照します。
