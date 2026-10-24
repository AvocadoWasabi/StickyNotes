[日本語](SCREENSHOTS.md) | [English](SCREENSHOTS.en.md) | [简体中文](SCREENSHOTS.zh-CN.md) | [繁體中文](SCREENSHOTS.zh-TW.md) | [한국어](SCREENSHOTS.ko.md)

# 界面示例与生成方法

使用当前开发版 WPF 控件将虚构示例渲染为460 × 580像素 PNG。并非桌面截图，也未连接实际 Obsidian／Google。不使用个人笔记、设置或凭据。

日语、英语和简体中文的正文与 UI 语言一致。繁体中文和韩语仅翻译正文，使用英文 UI，并未增加 UI 语言。每日文件名中的日期为生成日期。

| Language / 言語 / 语言 | Tasks | Markdown | Daily |
| --- | --- | --- | --- |
| 日本語 | [![日本語 tasks](images/sticky-tasks-ja.png)](images/sticky-tasks-ja.png) | [![日本語 markdown](images/sticky-markdown-ja.png)](images/sticky-markdown-ja.png) | [![日本語 daily](images/sticky-daily-ja.png)](images/sticky-daily-ja.png) |
| English | [![English tasks](images/sticky-tasks-en.png)](images/sticky-tasks-en.png) | [![English markdown](images/sticky-markdown-en.png)](images/sticky-markdown-en.png) | [![English daily](images/sticky-daily-en.png)](images/sticky-daily-en.png) |
| 简体中文 | [![简体中文 tasks](images/sticky-tasks-zh-CN.png)](images/sticky-tasks-zh-CN.png) | [![简体中文 markdown](images/sticky-markdown-zh-CN.png)](images/sticky-markdown-zh-CN.png) | [![简体中文 daily](images/sticky-daily-zh-CN.png)](images/sticky-daily-zh-CN.png) |
| 繁體中文 | [![繁體中文 tasks](images/sticky-tasks-zh-TW.png)](images/sticky-tasks-zh-TW.png) | [![繁體中文 markdown](images/sticky-markdown-zh-TW.png)](images/sticky-markdown-zh-TW.png) | [![繁體中文 daily](images/sticky-daily-zh-TW.png)](images/sticky-daily-zh-TW.png) |
| 한국어 | [![한국어 tasks](images/sticky-tasks-ko.png)](images/sticky-tasks-ko.png) | [![한국어 markdown](images/sticky-markdown-ko.png)](images/sticky-markdown-ko.png) | [![한국어 daily](images/sticky-daily-ko.png)](images/sticky-daily-ko.png) |

## 重新生成（开发者）

在 Windows 上使用.NET 8 SDK，从仓库根目录运行。如使用本地 SDK，将dotnet替换为.tools/dotnet/dotnet.exe。共生成15张图片。生成后检查所有图片的换行、裁切和缺字，并同步至少三种参考语言的说明。

```powershell
./build.ps1
dotnet run --project tests/StickyNotes.Tests -c Release --no-build -- --render-screenshots examples/screenshots docs/images
```

## 在应用中试用

Language支持ja / en / zh-CN / zh-TW / ko。每次在artifacts内新建数据目录，采用隔离设置并关闭CLI。如果应用正在运行，请先保存并退出，再启动示例。

```powershell
$demoData = ./scripts/Prepare-ScreenshotDemo.ps1 -Language ko
& ./artifacts/app/StickyNotes.exe --data-dir $demoData
```

[日本語](../examples/screenshots/Tasks.md) · [English](../examples/screenshots/en/Tasks.md) · [简体中文](../examples/screenshots/zh-CN/Tasks.md) · [繁體中文](../examples/screenshots/zh-TW/Tasks.md) · [한국어](../examples/screenshots/ko/Tasks.md)

旧JPEG和compact-body-v0.0.9.png作为历史参考保留。README使用本次生成的PNG。
