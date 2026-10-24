[日本語](SCREENSHOTS.md) | [English](SCREENSHOTS.en.md) | [简体中文](SCREENSHOTS.zh-CN.md) | [繁體中文](SCREENSHOTS.zh-TW.md) | [한국어](SCREENSHOTS.ko.md)

# 畫面範例與產生方式

使用目前開發版 WPF 控制項將虛構範例繪製為460 × 580像素 PNG。並非桌面擷取，也未連接實際 Obsidian／Google。不使用個人筆記、設定或憑證。

日文、英文與簡體中文的內文和 UI 語言一致。繁體中文與韓文僅翻譯內文，使用英文 UI，並未新增 UI 語言。每日檔案名稱的日期為產生日期。

| Language / 言語 / 语言 | Tasks | Markdown | Daily |
| --- | --- | --- | --- |
| 日本語 | [![日本語 tasks](images/sticky-tasks-ja.png)](images/sticky-tasks-ja.png) | [![日本語 markdown](images/sticky-markdown-ja.png)](images/sticky-markdown-ja.png) | [![日本語 daily](images/sticky-daily-ja.png)](images/sticky-daily-ja.png) |
| English | [![English tasks](images/sticky-tasks-en.png)](images/sticky-tasks-en.png) | [![English markdown](images/sticky-markdown-en.png)](images/sticky-markdown-en.png) | [![English daily](images/sticky-daily-en.png)](images/sticky-daily-en.png) |
| 简体中文 | [![简体中文 tasks](images/sticky-tasks-zh-CN.png)](images/sticky-tasks-zh-CN.png) | [![简体中文 markdown](images/sticky-markdown-zh-CN.png)](images/sticky-markdown-zh-CN.png) | [![简体中文 daily](images/sticky-daily-zh-CN.png)](images/sticky-daily-zh-CN.png) |
| 繁體中文 | [![繁體中文 tasks](images/sticky-tasks-zh-TW.png)](images/sticky-tasks-zh-TW.png) | [![繁體中文 markdown](images/sticky-markdown-zh-TW.png)](images/sticky-markdown-zh-TW.png) | [![繁體中文 daily](images/sticky-daily-zh-TW.png)](images/sticky-daily-zh-TW.png) |
| 한국어 | [![한국어 tasks](images/sticky-tasks-ko.png)](images/sticky-tasks-ko.png) | [![한국어 markdown](images/sticky-markdown-ko.png)](images/sticky-markdown-ko.png) | [![한국어 daily](images/sticky-daily-ko.png)](images/sticky-daily-ko.png) |

## 重新產生（開發者）

在 Windows 使用.NET 8 SDK，從儲存庫根目錄執行。使用本機 SDK 時，將dotnet替換為.tools/dotnet/dotnet.exe。共產生15張圖片。請檢查所有圖片的換行、裁切與缺字，並同步至少三種參考語言的說明。

```powershell
./build.ps1
dotnet run --project tests/StickyNotes.Tests -c Release --no-build -- --render-screenshots examples/screenshots docs/images
```

## 在應用程式中試用

Language支援ja / en / zh-CN / zh-TW / ko。每次在artifacts內建立新資料夾，使用隔離設定並停用CLI。若程式正在執行，請先儲存並結束，再啟動範例。

```powershell
$demoData = ./scripts/Prepare-ScreenshotDemo.ps1 -Language ko
& ./artifacts/app/StickyNotes.exe --data-dir $demoData
```

[日本語](../examples/screenshots/Tasks.md) · [English](../examples/screenshots/en/Tasks.md) · [简体中文](../examples/screenshots/zh-CN/Tasks.md) · [繁體中文](../examples/screenshots/zh-TW/Tasks.md) · [한국어](../examples/screenshots/ko/Tasks.md)

舊JPEG與compact-body-v0.0.9.png保留為歷史參考。README使用本次產生的PNG。
