[日本語](SCREENSHOTS.md) | [English](SCREENSHOTS.en.md) | [简体中文](SCREENSHOTS.zh-CN.md) | [繁體中文（台灣）](SCREENSHOTS.zh-TW.md) | [한국어](SCREENSHOTS.ko.md) | [Deutsch](SCREENSHOTS.de.md) | [Русский](SCREENSHOTS.ru.md) | [Español](SCREENSHOTS.es.md) | [Svenska](SCREENSHOTS.sv.md)

# Screenshots and reproduction

Fictional notes rendered as 460 × 580 px PNGs with WPF controls from v0.0.11. These are not desktop captures or live Obsidian/Google sessions. No personal notes, settings or credentials are used.

Japanese descriptions and sample notes are the source; the other languages are translations. The 27 images show three examples in each of the nine supported UI languages, with matching note text and UI. Taiwanese Mandarin uses Traditional Chinese (zh-TW). The daily filename shows the generation date.

| Language | Checkboxes | Markdown | Daily notes |
| --- | --- | --- | --- |
| 日本語 | [![日本語 tasks](images/sticky-tasks-ja.png)](images/sticky-tasks-ja.png) | [![日本語 markdown](images/sticky-markdown-ja.png)](images/sticky-markdown-ja.png) | [![日本語 daily](images/sticky-daily-ja.png)](images/sticky-daily-ja.png) |
| English | [![English tasks](images/sticky-tasks-en.png)](images/sticky-tasks-en.png) | [![English markdown](images/sticky-markdown-en.png)](images/sticky-markdown-en.png) | [![English daily](images/sticky-daily-en.png)](images/sticky-daily-en.png) |
| 简体中文 | [![简体中文 tasks](images/sticky-tasks-zh-CN.png)](images/sticky-tasks-zh-CN.png) | [![简体中文 markdown](images/sticky-markdown-zh-CN.png)](images/sticky-markdown-zh-CN.png) | [![简体中文 daily](images/sticky-daily-zh-CN.png)](images/sticky-daily-zh-CN.png) |
| 繁體中文（台灣） | [![繁體中文（台灣） tasks](images/sticky-tasks-zh-TW.png)](images/sticky-tasks-zh-TW.png) | [![繁體中文（台灣） markdown](images/sticky-markdown-zh-TW.png)](images/sticky-markdown-zh-TW.png) | [![繁體中文（台灣） daily](images/sticky-daily-zh-TW.png)](images/sticky-daily-zh-TW.png) |
| 한국어 | [![한국어 tasks](images/sticky-tasks-ko.png)](images/sticky-tasks-ko.png) | [![한국어 markdown](images/sticky-markdown-ko.png)](images/sticky-markdown-ko.png) | [![한국어 daily](images/sticky-daily-ko.png)](images/sticky-daily-ko.png) |
| Deutsch | [![Deutsch tasks](images/sticky-tasks-de.png)](images/sticky-tasks-de.png) | [![Deutsch markdown](images/sticky-markdown-de.png)](images/sticky-markdown-de.png) | [![Deutsch daily](images/sticky-daily-de.png)](images/sticky-daily-de.png) |
| Русский | [![Русский tasks](images/sticky-tasks-ru.png)](images/sticky-tasks-ru.png) | [![Русский markdown](images/sticky-markdown-ru.png)](images/sticky-markdown-ru.png) | [![Русский daily](images/sticky-daily-ru.png)](images/sticky-daily-ru.png) |
| Español | [![Español tasks](images/sticky-tasks-es.png)](images/sticky-tasks-es.png) | [![Español markdown](images/sticky-markdown-es.png)](images/sticky-markdown-es.png) | [![Español daily](images/sticky-daily-es.png)](images/sticky-daily-es.png) |
| Svenska | [![Svenska tasks](images/sticky-tasks-sv.png)](images/sticky-tasks-sv.png) | [![Svenska markdown](images/sticky-markdown-sv.png)](images/sticky-markdown-sv.png) | [![Svenska daily](images/sticky-daily-sv.png)](images/sticky-daily-sv.png) |

## Regenerate (developers)

Run from the repository root on Windows with .NET 8 SDK. For the local SDK, replace dotnet with .tools/dotnet/dotnet.exe. Review all 27 images for wrapping, clipping and missing glyphs, and keep descriptions aligned in all nine languages.

```powershell
./build.ps1
dotnet run --project tests/StickyNotes.Tests -c Release --no-build -- --render-screenshots examples/screenshots docs/images
```

## Try in the app

Language accepts ja / en / zh-CN / zh-TW / ko / de / ru / es / sv. Each run creates a fresh data folder under artifacts with the chosen UI language and CLI disabled. Save and exit any running instance before launching the demo.

```powershell
$demoData = ./scripts/Prepare-ScreenshotDemo.ps1 -Language en
& ./artifacts/app/StickyNotes.exe --data-dir $demoData
```

[日本語](../examples/screenshots/Tasks.md) · [English](../examples/screenshots/en/Tasks.md) · [简体中文](../examples/screenshots/zh-CN/Tasks.md) · [繁體中文（台灣）](../examples/screenshots/zh-TW/Tasks.md) · [한국어](../examples/screenshots/ko/Tasks.md) · [Deutsch](../examples/screenshots/de/Tasks.md) · [Русский](../examples/screenshots/ru/Tasks.md) · [Español](../examples/screenshots/es/Tasks.md) · [Svenska](../examples/screenshots/sv/Tasks.md)

Older JPEGs and compact-body-v0.0.9.png remain as historical references. The READMEs use the newly generated PNGs.
