[日本語](README.md) | [English](README.en.md) | [简体中文](README.zh-CN.md) | [繁體中文](README.zh-TW.md) | [한국어](README.ko.md)

# Markdown Sticky Notes

**Keep today's Obsidian tasks on your desktop. Check them off right there.**

Turn a whole note or a single daily-note heading into a Windows sticky note. Edits save to the original Markdown. Keep Tasks and Dataview results or Google Calendar events close at hand, too.

[Download](https://github.com/AvocadoWasabi/StickyNotes/releases/latest) · [Setup](docs/INSTALL.en.md) · [Usage](docs/USAGE.en.md) · [Changelog](CHANGELOG.md)

## Why use it?

- **Keep today's work in sight** — display just the Tasks section of your daily note and follow the date automatically.
- **Keep one copy of your notes** — edits and checks update the original `.md`. Closing a window keeps the file.
- **Glance at notes while working** — pin them on top, or press `Alt+Z` to bring all notes forward for 10 seconds.

## Screenshots

| Check off tasks | Write in Markdown | Follow today's daily note |
| :---: | :---: | :---: |
| [<img src="docs/images/sticky-tasks-en.png" alt="Check off tasks" width="280">](docs/images/sticky-tasks-en.png) | [<img src="docs/images/sticky-markdown-en.png" alt="Write in Markdown" width="280">](docs/images/sticky-markdown-en.png) | [<img src="docs/images/sticky-daily-en.png" alt="Follow today's daily note" width="280">](docs/images/sticky-daily-en.png) |

Fictional notes rendered with the current development build's WPF controls, rather than desktop captures. [Five languages and reproduction steps](docs/SCREENSHOTS.en.md).

## Tools and settings you need

**Windows 10 / 11 (x64)**. The ZIP includes .NET; administrator access is not required.

| Use case | Required tools | Initial setting |
| --- | --- | --- |
| Markdown sticky notes only | This app | Turn off **Use Obsidian CLI** in Settings and choose a local folder |
| Edit Obsidian notes | Running Obsidian + Obsidian CLI | Enable Obsidian **Settings → General → Command line interface**, then connect the vault in this app |
| Today's daily note | CLI above + core **Daily notes** | Configure its folder/date format in Obsidian and create today's note |
| Tasks queries and native checkbox behavior | CLI above + community **Tasks** plugin | Enable in the target vault; current development build requires stable **8.2.2+** |
| Dataview queries | CLI above + **Dataview** | Enable in the target vault; LIST / TABLE / TASK supported |
| Google events and tasks | Google account + your own OAuth setup | Enable Calendar API / Tasks API, obtain desktop OAuth JSON and [authorize](docs/GOOGLE-CALENDAR.en.md#setup) |

**New settings enable CLI by default.** Turn it off to work without Obsidian. CLI is built into Obsidian; no dedicated Sticky Notes Obsidian plugin is needed. [Official CLI requirements](https://obsidian.md/help/cli) · [Compatibility and limits](docs/TASKS-CLI-PREVIEW.md#english).

## Get started

1. Download `StickyNotes-win-x64.zip` from [Releases](https://github.com/AvocadoWasabi/StickyNotes/releases/latest), extract everything and run `Install.cmd`.
2. Enable CLI in Obsidian, open your vault and start Markdown Sticky Notes.
3. In app Settings, check the absolute path to `Obsidian.com`, enter the vault name/ID and click **Get Vault folder via CLI**. Choose a note folder, then **Save and close**.
4. Use `… → Create & open` to create a sticky note, open Markdown, link a section or display a daily note.

See [Setup](docs/INSTALL.en.md) for local mode, setting locations, updating and removal. Settings and images here describe the current development build. Check [Unreleased](CHANGELOG.md#unreleased) for differences from the published ZIP, including multiple vaults and Tasks 8.2.2 support.

## Features

| Feature | What it does |
| --- | --- |
| Markdown display and editing | Headings, lists, checkboxes, tables, code and links; save to regular `.md` files |
| Linked sections and daily notes | Display/edit part of a note, follow date changes and optionally retain yesterday's note |
| Tasks / Dataview | Display query results and check tasks with verified source mappings; preserve query source |
| Task entry | Keyword/date completion, Enter to continue lists, Tab to indent, configurable checkbox shortcut |
| Google integration | `@calendar today` shows events and dated incomplete Google Tasks; edit event titles/descriptions after confirmation |
| Windows and appearance | Colors, resizing, 50–200% scale, pinning, temporary foreground and taskbar visibility |
| Note management | Select/migrate storage, reopen closed notes, browse/preview files, rename and delete |
| Data protection | Backups before writes and external-change conflict checks |
| Multiple vaults (unreleased) | Daily notes per vault, editable names and a default vault |

## Basic usage

| Action | How |
| --- | --- |
| Edit | Click the body (single/double click is configurable), or `Ctrl+E` |
| Save and leave editing | `Ctrl+S` / `Esc`; the first Esc dismisses open completion suggestions |
| Check a task | Click its checkbox in reading mode |
| Bring forward | Pin with `○ / ●`; `Alt+Z` brings all notes forward for 10 seconds |
| Close / exit | `… → Close this note` / exit the app from the notification area |

## Before you start

- App UI: **Japanese, English and Simplified Chinese**. README, setup and screenshots also cover **Traditional Chinese and Korean**. Detailed references generally remain in three languages.
- Obsidian must stay running for CLI integration. Internal API changes can affect compatibility.
- Daily notes are not created automatically. Prepare today's note before adding the first daily sticky.
- Obsidian themes and all plugin views are not reproduced. Images show alt text. DataviewJS, inline expressions and CALENDAR are unsupported.
- Google Tasks is read-only and exposes dates only. Scheduled times, automatic Markdown task sync and creating events are unsupported.
- Backups: `%LOCALAPPDATA%/StickyNotes/backups`. On a conflict, keep your draft, reload and retry.

## Learn more or report a problem

[Storage and migration](docs/STICKY-FOLDERS.md#english) · [Tasks / CLI](docs/TASKS-CLI-PREVIEW.md#english) · [Task entry](docs/TASK-EDITING.md#english) · [Dataview](docs/DATAVIEW.md#english) · [Multiple vaults](docs/MULTIPLE-VAULTS.md#english) · [Note management](docs/QUERY-EDITING-AND-BROWSER.md#english) · [Google](docs/GOOGLE-CALENDAR.en.md) · [Development](docs/DEVELOPMENT.en.md) · [License notices](THIRD-PARTY-NOTICES.txt)

For [bugs and requests](https://github.com/AvocadoWasabi/StickyNotes/issues), include app/Windows/plugin versions, CLI or local mode, and reproduction steps. Do not attach personal notes, OAuth JSON or tokens.
