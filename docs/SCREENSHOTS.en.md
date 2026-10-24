[日本語](SCREENSHOTS.md) | [English](SCREENSHOTS.en.md) | [简体中文](SCREENSHOTS.zh-CN.md) | [繁體中文](SCREENSHOTS.zh-TW.md) | [한국어](SCREENSHOTS.ko.md)

# Screenshots and reproduction

Fictional samples rendered as 460 × 580 px PNGs with the current development build’s WPF controls. These are not desktop captures or live Obsidian/Google sessions. No personal notes, settings or credentials are used.

Japanese, English and Simplified Chinese match note text and UI. Traditional Chinese and Korean translate the note text and use English UI; no new UI locales are implied. The daily filename shows the generation date.

| Language / 言語 / 语言 | Tasks | Markdown | Daily |
| --- | --- | --- | --- |
| 日本語 | [![日本語 tasks](images/sticky-tasks-ja.png)](images/sticky-tasks-ja.png) | [![日本語 markdown](images/sticky-markdown-ja.png)](images/sticky-markdown-ja.png) | [![日本語 daily](images/sticky-daily-ja.png)](images/sticky-daily-ja.png) |
| English | [![English tasks](images/sticky-tasks-en.png)](images/sticky-tasks-en.png) | [![English markdown](images/sticky-markdown-en.png)](images/sticky-markdown-en.png) | [![English daily](images/sticky-daily-en.png)](images/sticky-daily-en.png) |
| 简体中文 | [![简体中文 tasks](images/sticky-tasks-zh-CN.png)](images/sticky-tasks-zh-CN.png) | [![简体中文 markdown](images/sticky-markdown-zh-CN.png)](images/sticky-markdown-zh-CN.png) | [![简体中文 daily](images/sticky-daily-zh-CN.png)](images/sticky-daily-zh-CN.png) |
| 繁體中文 | [![繁體中文 tasks](images/sticky-tasks-zh-TW.png)](images/sticky-tasks-zh-TW.png) | [![繁體中文 markdown](images/sticky-markdown-zh-TW.png)](images/sticky-markdown-zh-TW.png) | [![繁體中文 daily](images/sticky-daily-zh-TW.png)](images/sticky-daily-zh-TW.png) |
| 한국어 | [![한국어 tasks](images/sticky-tasks-ko.png)](images/sticky-tasks-ko.png) | [![한국어 markdown](images/sticky-markdown-ko.png)](images/sticky-markdown-ko.png) | [![한국어 daily](images/sticky-daily-ko.png)](images/sticky-daily-ko.png) |

## Regenerate (developers)

Run from the repository root on Windows with .NET 8 SDK. If using the local SDK, replace dotnet with .tools/dotnet/dotnet.exe. This generates 15 images. Review all images for wrapping, clipping and missing glyphs, and keep descriptions aligned in at least the three reference languages.

```powershell
./build.ps1
dotnet run --project tests/StickyNotes.Tests -c Release --no-build -- --render-screenshots examples/screenshots docs/images
```

## Try in the app

Language accepts ja / en / zh-CN / zh-TW / ko. Each run creates a fresh data folder under artifacts with isolated settings and CLI disabled. Save and exit any running instance before launching the demo.

```powershell
$demoData = ./scripts/Prepare-ScreenshotDemo.ps1 -Language ko
& ./artifacts/app/StickyNotes.exe --data-dir $demoData
```

[日本語](../examples/screenshots/Tasks.md) · [English](../examples/screenshots/en/Tasks.md) · [简体中文](../examples/screenshots/zh-CN/Tasks.md) · [繁體中文](../examples/screenshots/zh-TW/Tasks.md) · [한국어](../examples/screenshots/ko/Tasks.md)

Older JPEGs and compact-body-v0.0.9.png remain as historical references. The READMEs use the newly generated PNGs.
