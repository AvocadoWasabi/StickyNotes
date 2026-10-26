[日本語](README.md) | [English](README.en.md) | [简体中文](README.zh-CN.md) | [繁體中文（台灣）](README.zh-TW.md) | [한국어](README.ko.md) | [Deutsch](README.de.md) | [Русский](README.ru.md) | [Español](README.es.md) | [Svenska](README.sv.md)

# Markdown Sticky Notes

[v0.0.11](https://github.com/AvocadoWasabi/StickyNotes/releases/tag/v0.0.11) · Windows 10 / 11 (x64)

A Windows application for displaying and editing Markdown files in sticky-note windows. It supports local files, Obsidian notes and specific headings in daily notes. Edits and checkbox changes are saved to the original Markdown file.

It supports Tasks and Dataview query results through Obsidian CLI, and retrieval of Google Calendar events and Google Tasks.

[Download](https://github.com/AvocadoWasabi/StickyNotes/releases/latest) · [Setup](docs/INSTALL.en.md) · [Usage](docs/USAGE.en.md) · [Changelog](CHANGELOG.md)

## Screenshots

| Checkbox interaction | Markdown display | Daily-note section display |
| :---: | :---: | :---: |
| [<img src="docs/images/sticky-tasks-en.png" alt="Checkbox interaction" width="280">](docs/images/sticky-tasks-en.png) | [<img src="docs/images/sticky-markdown-en.png" alt="Markdown display" width="280">](docs/images/sticky-markdown-en.png) | [<img src="docs/images/sticky-daily-en.png" alt="Daily-note section display" width="280">](docs/images/sticky-daily-en.png) |

Sample notes rendered with the application’s WPF controls. These are not desktop captures. See [examples in each language and generation instructions](docs/SCREENSHOTS.en.md).

## System requirements and dependencies

**Windows 10 / 11 (x64)**. The ZIP includes .NET; administrator access is not required.

| Use case | Required tools | Initial setting |
| --- | --- | --- |
| Markdown sticky notes only | This app | Turn off **Use Obsidian CLI** in Settings and choose a local folder |
| Edit Obsidian notes | Running Obsidian + Obsidian CLI | Enable Obsidian **Settings → General → Command line interface**, then connect the vault in this app |
| Today's daily note | CLI above + core **Daily notes** | Configure its folder/date format in Obsidian and create today's note |
| Tasks queries and native checkbox behavior | CLI above + community **Tasks** plugin | Enable in the target vault; v0.0.11 requires stable **8.2.2+** |
| Dataview queries | CLI above + **Dataview** | Enable in the target vault; LIST / TABLE / TASK supported |
| Google events and tasks | Google account + your own OAuth setup | Enable Calendar API / Tasks API, obtain desktop OAuth JSON and [authorize](docs/GOOGLE-CALENDAR.en.md#setup) |

**New settings enable CLI by default.** Turn it off to work without Obsidian. CLI is built into Obsidian; no dedicated Sticky Notes Obsidian plugin is needed. [Official CLI requirements](https://obsidian.md/help/cli) · [Compatibility and limits](docs/TASKS-CLI-PREVIEW.md#english).

## Installation and initial setup

1. Download `StickyNotes-win-x64.zip` from [Releases](https://github.com/AvocadoWasabi/StickyNotes/releases/latest), extract everything and run `Install.cmd`.
2. Enable CLI in Obsidian, open your vault and start Markdown Sticky Notes.
3. In app Settings, check the absolute path to `Obsidian.com`, enter the vault name/ID and click **Get Vault folder via CLI**. Choose a note folder, then **Save and close**.
4. Use `… → Create & open` to create a sticky note, open Markdown, link a section or display a daily note.

For local-file settings, updates and removal, see the [installation guide](docs/INSTALL.en.md). The features and settings described here apply to v0.0.11. Changes by version are recorded in the [changelog](CHANGELOG.md#v0-0-11).

## Features

| Feature | What it does |
| --- | --- |
| Markdown display and editing | Headings, lists, checkboxes, tables, code and links; save to regular `.md` files |
| Linked sections and daily notes | Display and edit a section, follow date changes and retain yesterday’s note when today’s note is missing |
| Tasks / Dataview | Display query results and check tasks with verified source mappings; preserve query source |
| Task entry | Keyword/date completion, Enter to continue lists, Tab to indent, configurable checkbox shortcut |
| Google integration | `@calendar today` shows events and dated incomplete Google Tasks; edit event titles/descriptions after confirmation |
| Windows and appearance | Colors, resizing, 50–200% scale, pinning, temporary foreground and taskbar visibility |
| Note management | Select/migrate storage, reopen closed notes, browse/preview files, rename and delete |
| Backups and conflict detection | Backups before writes and external-change conflict checks |
| Multiple vaults | Daily notes per vault, editable names and a default vault |

## Basic usage

| Action | How |
| --- | --- |
| Edit | Click the body (single/double click is configurable), or `Ctrl+E` |
| Save and leave editing | `Ctrl+S` / `Esc`; the first Esc dismisses open completion suggestions |
| Check a task | Click its checkbox in reading mode |
| Bring forward | Pin with `○ / ●`; `Alt+Z` brings all notes forward for 10 seconds |
| Close / exit | `… → Close this note` / exit the app from the notification area |

## Supported scope and limitations

- The UI in v0.0.11, README, setup and screenshots support **Japanese, English, Simplified Chinese, Taiwanese Mandarin (Traditional Chinese), Korean, German, Russian, Spanish and Swedish**. Detailed references generally remain in three languages. Save and restart to apply a language change.
- Obsidian must stay running for CLI integration. Internal API changes can affect compatibility.
- Daily notes are not created automatically. Prepare today's note before adding the first daily sticky.
- Obsidian themes and all plugin views are not reproduced. Images show alt text. DataviewJS, inline expressions and CALENDAR are unsupported.
- Google Tasks is read-only; due dates are retrieved as dates only. Due times, automatic synchronization with Markdown tasks and creating Google Calendar events are not supported.
- Backups: `%LOCALAPPDATA%/StickyNotes/backups`. On a conflict, keep your draft, reload and retry.

## Related documentation and issue reports

[Storage and migration](docs/STICKY-FOLDERS.md#english) · [Tasks / CLI](docs/TASKS-CLI-PREVIEW.md#english) · [Task entry](docs/TASK-EDITING.md#english) · [Dataview](docs/DATAVIEW.md#english) · [Multiple vaults](docs/MULTIPLE-VAULTS.md#english) · [Note management](docs/QUERY-EDITING-AND-BROWSER.md#english) · [Google](docs/GOOGLE-CALENDAR.en.md) · [Development](docs/DEVELOPMENT.en.md) · [License notices](THIRD-PARTY-NOTICES.txt)

For [bugs and requests](https://github.com/AvocadoWasabi/StickyNotes/issues), include app/Windows/plugin versions, CLI or local mode, and reproduction steps. Do not attach personal notes, OAuth JSON or tokens.

## License

The code and documentation in this project copyrighted by AvocadoWasabi are licensed under the [MIT License](LICENSE). Third-party code and libraries retain their respective licenses.

Markdig uses BSD 2-Clause. YamlDotNet, suggestion code and test material derived from Obsidian Tasks, and .NET / Windows Desktop / System.Security.Cryptography.ProtectedData use MIT. See [third-party notices](THIRD-PARTY-NOTICES.txt) for copyright notices and terms. Distribution ZIPs include the project's LICENSE, third-party notices, and .NET licenses and additional third-party notices in licenses/.
