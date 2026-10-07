[English](README.md) | [日本語](README.ja.md) | [简体中文](README.zh-CN.md)

# Markdown Sticky Notes

Experimental, unreleased: the standard local build (`./build.ps1`) now includes [Obsidian Tasks CLI integration](docs/TASKS-CLI-PREVIEW.md#english), disabled by default. The separate profile build remains available with `./build.ps1 -Publish -TasksPreview`.
When enabled, this preview obtains all note text and daily-note discovery through Obsidian CLI, without local-read fallback. Disabling it keeps the original file access.

<img src="docs/images/StickyNotes.png" alt="Markdown Sticky Notes icon" width="112">

A Markdown sticky-note app for Windows. Edit Obsidian notes and daily notes directly, and display Google Calendar events alongside them. Obsidian is optional.

**Windows 10 / 11 (x64).** The UI and documentation support English, Japanese, and Simplified Chinese. [Display language](docs/USAGE.en.md#language).  Google integration is optional and requires your own OAuth setup.

[Download](#download-and-install) · [Basic usage](#basic-usage) · [Google Calendar](#google-calendar) · [Further reading](#further-reading)

<details>
<summary>Screenshots (expand to view)</summary>

Version 0.0.2 note views rendered with the app's WPF controls and English sample notes. The controls in these older images are Japanese. Click an image to view it at full size.

| Checklists at hand | Markdown notes | Linked daily notes |
| :---: | :---: | :---: |
| [<img src="docs/images/sticky-tasks-en.jpg" alt="Yellow sticky note with an English checklist and checked tasks" width="300">](docs/images/sticky-tasks-en.jpg) | [<img src="docs/images/sticky-markdown-en.jpg" alt="Blue sticky note displaying English Markdown headings, a numbered list, and a table" width="300">](docs/images/sticky-markdown-en.jpg) | [<img src="docs/images/sticky-daily-en.jpg" alt="Green sticky note linked to the Tasks section of a daily note, with English sample tasks" width="300">](docs/images/sticky-daily-en.jpg) |

The screenshots use fictional sample content. [Sample files and capture instructions](docs/SCREENSHOTS.md) are included for reproduction.

</details>

## Download and install

1. Download `StickyNotes-win-x64.zip` from the [v0.0.6 release](https://github.com/AvocadoWasabi/StickyNotes/releases/tag/v0.0.6) and extract it.
2. Run `Install.cmd`, then open Markdown Sticky Notes from the Start menu.
3. Open `Settings` from the notification-area icon and choose a notes folder.

No administrator privileges or separate .NET installation are required. The app is unsigned, so Windows may ask for confirmation. See the [installation guide](docs/INSTALL.en.md) for updates, removal, and portable use.

<a id="using-the-app"></a>

## Basic usage

| Action | Control |
| --- | --- |
| Create a note | `＋`, or `New note` in the notification-area menu |
| Edit and save | Click the body / `Ctrl+E` → edit → `Ctrl+S` |
| Complete a task | Click its checkbox to update the source Markdown |
| Move or resize | Drag the top / an edge or the bottom-right corner |
| Keep on top | `○ / ●`; the `…` menu can bring all notes to the top for 10 seconds |
| Scale content | `… → Display scale` (50–200%) |
| Close or exit | `×` closes a note; notification-area `Exit` exits the app |

Leaving the editor prompts to save changed text. Settings can enable saving without confirmation. Closing a note does not delete its Markdown file.

See the [usage reference](docs/USAGE.en.md#editing) for taskbar icons, button visibility, and shortcuts.

## Storage and Obsidian

New notes default to `Documents/StickyNotesData`. Choose a folder in your vault to edit the same `.md` files as Obsidian. No CLI or plugin is needed.

Changing the folder can also migrate existing Markdown. Check the [migration scope and restrictions](docs/USAGE.en.md#storage) before moving files.

## Display and edit a daily note section

1. In Settings, choose the daily-note folder and tagged regular expression. Check the matching filename and save.
2. Use `… → Display a daily note…`, choose a heading, and select `Display`. A blank heading shows the whole body.
3. Edit text or check tasks in the sticky note to update the source note.

The first addition requires today's file. Later, missing daily notes can show a waiting message or retain yesterday according to Settings. See [filename examples and switching options](docs/USAGE.en.md#daily).

For a fixed note, use `… → Link part of a note…`. See [heading selection](docs/USAGE.en.md#headings).

## Google Calendar

<a id="initial-authentication"></a>

First authenticate using the [connection guide](docs/GOOGLE-CALENDAR.en.md#setup) in the right pane of Settings. Then save this line in a separate paragraph:

```text
@calendar 2026-10-06T09:00
```

**Scheduled times cannot be retrieved through the Google Tasks API.** Even when a time is set in Google, the API provides only the date, so this app displays “no time”.

Google Tasks is also displayed as read-only. `@calendar today` includes today's dated, incomplete tasks. Existing users must enable the Tasks API and sign in again with Tasks read access; see the [Tasks setup guide](docs/GOOGLE-CALENDAR.en.md#tasks).

Use `@calendar today` to follow the PC's current day automatically (local midnight to the next midnight). `@calendar today meeting` filters by keyword. The date is checked and events are refreshed about every 60 seconds while viewing the note, including after waking from sleep; automatic refresh pauses while editing.

**Keywords and time zone offsets are optional.** Without an offset, the app uses Windows local time. Append a keyword such as `meeting` to filter results.

While editing, type `@` in a separate paragraph to show a suggestion and examples. Tab, Enter, or a click inserts today's date at `00:00` and selects the date and time for editing. The inserted date stays fixed.

Click an event to edit its title or description, then confirm to send changes to Google. Creating events and automatically syncing Markdown tasks are unsupported. See [search rules, JSON management, and troubleshooting](docs/GOOGLE-CALENDAR.en.md).

## Markdown support and data protection

Headings, lists, tasks, tables, code, and links are supported. Images display as alternative text; some Obsidian-specific syntax is unsupported.

Conflicting external changes block overwrites. Pre-write backups are saved under `%LOCALAPPDATA%/StickyNotes/backups`. See [supported syntax and recovery details](docs/USAGE.en.md#data).

## Further reading

| Topic | Guide |
| --- | --- |
| Install, update, or remove | [Installation](docs/INSTALL.en.md) |
| Controls, storage, and daily notes | [Usage reference](docs/USAGE.en.md) |
| Google authentication and events | [Google Calendar](docs/GOOGLE-CALENDAR.en.md) |
| Changes by version | [Changelog](CHANGELOG.md) |
| Build, test, and release | [Development](docs/DEVELOPMENT.en.md) |
| Examples and licenses | [Daily note](examples/Daily.md) · [Obsidian Bases](examples/StickyNotes.base) · [Licenses](THIRD-PARTY-NOTICES.txt) |
