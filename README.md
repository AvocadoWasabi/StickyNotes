[English](README.md) | [日本語](README.ja.md)

# Markdown Sticky Notes

<img src="docs/images/StickyNotes.png" alt="Markdown Sticky Notes icon" width="112">

A Windows desktop sticky notes app that works with Markdown files and Obsidian vaults, built with C# and WPF.

Keep tasks, notes, or a section of today's daily note on your desktop and edit them in place. Notes are ordinary Markdown files, so you can use the same files in Obsidian. The app also works on its own without Obsidian.

## Screenshots

Actual app windows with English sample notes. The app's controls are currently in Japanese. Click an image to view it at full size.

| Checklists at hand | Markdown notes | Linked daily notes |
| :---: | :---: | :---: |
| [<img src="docs/images/sticky-tasks-en.jpg" alt="Yellow sticky note with an English checklist and checked tasks" width="300">](docs/images/sticky-tasks-en.jpg) | [<img src="docs/images/sticky-markdown-en.jpg" alt="Blue sticky note displaying English Markdown headings, a numbered list, and a table" width="300">](docs/images/sticky-markdown-en.jpg) | [<img src="docs/images/sticky-daily-en.jpg" alt="Green sticky note linked to the Tasks section of a daily note, with English sample tasks" width="300">](docs/images/sticky-daily-en.jpg) |

The screenshots use fictional sample content. [Sample files and capture instructions](docs/SCREENSHOTS.md) are included for reproduction.

## Features

| Feature | What it does |
| --- | --- |
| Desktop sticky notes | Open multiple notes, move and resize them, keep them on top, and restore their layout |
| Markdown viewing and editing | Display headings, lists, checkboxes, tables, and code, and edit the Markdown source |
| Obsidian integration | Read and write `.md` files in your vault; store titles, tags, status, and colors in YAML properties |
| Linked sections | Display and edit a specific heading's section while preserving the rest of the source note |
| Daily notes | Switch to today's file using a date pattern and check off tasks from the sticky note |
| Google Calendar (optional) | Search and display events; edit titles and descriptions after confirmation |
| Data protection | Detect external changes, prevent conflicting overwrites, and back up files before writing |

The app's interface and menus are currently in Japanese. This README is available in English and Japanese.

## Download and install

Requires **Windows 10 / 11 (x64)**. No administrator privileges, separate .NET installation, Obsidian CLI, or Obsidian plugins are required.

1. Open [GitHub Releases](https://github.com/AvocadoWasabi/StickyNotes/releases).
2. Download `StickyNotes-win-x64.zip`. The `Source code` downloads are for developers.
3. Right-click the ZIP and choose **Extract All**.
4. Double-click `Install.cmd` in the extracted folder.
5. Launch **Markdown Sticky Notes** from the Start menu.

The installer places the app in `%LOCALAPPDATA%\Programs\MarkdownStickyNotes`. It does not enable automatic startup. [Download v0.0.1](https://github.com/AvocadoWasabi/StickyNotes/releases/tag/v0.0.1).

For portable use, run `StickyNotes.exe` directly from the extracted folder. Keep all accompanying files in either case. The app is unsigned; if Windows displays a warning, check the download source and file before proceeding.

### First-time setup

Right-click the notification-area icon and select `設定` (Settings), then choose a folder for your notes. Select a folder inside your vault to use the notes with Obsidian. To use daily notes, also set their folder and date pattern. Google authentication is only needed if you use Google Calendar.

### Update and uninstall

To update, select `終了` (Exit) from the notification-area menu, extract the new ZIP to a separate folder, and run `Install.cmd`. Your notes and settings are retained.

To uninstall, exit the app, delete `%LOCALAPPDATA%\Programs\MarkdownStickyNotes`, and remove its Start menu shortcut. You can locate the shortcut using **Open file location** from its context menu. Settings, credentials, and backups remain in `%LOCALAPPDATA%\StickyNotes`; Markdown files remain in the folder you selected, or `Documents/StickyNotesData` by default. Review any data you want to keep before deleting it separately.

## Documentation and examples

- [Changelog / version history](CHANGELOG.md)
- [日本語 / Japanese README](README.ja.md)
- [Installation, updates, and removal (Japanese)](docs/INSTALL.md)
- [Development, branch workflow, and releases (Japanese)](docs/DEVELOPMENT.md)
- [Third-party licenses](THIRD-PARTY-NOTICES.txt)
- [Obsidian Bases example](examples/StickyNotes.base) / [Daily note example](examples/Daily.md)

## Using the app

Launch the installed app from the Start menu, or run `StickyNotes.exe` from the extracted ZIP. When building from source, the published executable is `artifacts/app/StickyNotes.exe`.

Drag the top of a note to move it, or drag an edge or the bottom-right corner to resize it. Use `○ / ●` to toggle always-on-top mode. Notes have no standard title bar and normally stay out of the taskbar; the app runs in the notification area.

| Control | Action |
| --- | --- |
| `＋` | Create a note |
| `編集` / `Ctrl+E` | Edit the Markdown source |
| `保存` / `Ctrl+S` | Save the file and return to reading mode |
| `再読込` | Reload the source file, with confirmation if edits are unsaved |
| `…` | Open properties, fetch events, open existing files, link a section, access settings, or exit |
| `×` | Close this note without deleting its Markdown file |
| `… → アプリを終了`, or notification-area `終了` | Exit the app |

The app saves note positions, sizes, and always-on-top settings as notes move and when you exit, then restores them on the next launch. It supports multiple monitors, negative screen coordinates, and PerMonitorV2 DPI. Notes on a disconnected display are moved back onto an available screen. Unsaved changes trigger a confirmation before exit.

## Storage and Obsidian

New notes are saved in `Documents/StickyNotesData` by default. Open `… → 設定` (Settings) to select a folder in your Obsidian vault, such as `Vault/Sticky Notes`. When you save a folder change, the app asks whether to migrate existing note files. Choose **Yes** to move every `.md` file in the old folder, including subfolders and closed notes; **No** changes only the folder for new notes; **Cancel** leaves settings unchanged. Open notes keep their placement and unsaved edits, and daily-note folders inside the old folder follow the move. Files outside the old folder and non-Markdown files stay in place.

Migration never overwrites an existing file. Name collisions stop the move; other move or settings-save failures trigger a rollback. If a file cannot be restored, the error lists its location for manual recovery. Parent/child folder pairs and paths containing links or junctions cannot be migrated. Relative links to files left behind may need updating. Back up your notes before a large move: interruption or power loss can leave files in both folders, so check both before retrying.

Notes are regular UTF-8 `.md` files with properties such as:

```yaml
---
type: sticky
id: unique-note-id
title: Shopping
tags:
  - sticky
  - personal/shopping
status: active
color: yellow
created: 2026-10-05T09:00:00+09:00
updated: 2026-10-05T09:00:00+09:00
---
```

Use `… → タグ・タイトル・状態・色` (Tags, title, status, and color) to edit properties. Available colors are `yellow / green / blue / pink / gray`. The `status` field accepts values such as `active / done / archived`; it is for organization and does not automatically hide or delete notes.

Copy `examples/StickyNotes.base` into your vault to list notes with `type: sticky`. To exclude sticky notes from other Bases, use a condition such as `type != "sticky"`, or exclude their folder with `!file.inFolder("Sticky Notes")`.

App layout and authentication data are kept outside the vault in `%LOCALAPPDATA%/StickyNotes`. The app checks for external file changes about every two seconds. When you are editing a note, it reports a conflict instead of replacing your edits automatically.

References: [Obsidian Properties](https://obsidian.md/help/properties), [Bases syntax](https://obsidian.md/help/bases/syntax).

## Display and edit a daily note section

The app reads and writes the original Markdown file directly. No Obsidian CLI or plugin is needed.

1. In Settings, configure your daily notes folder and date pattern.
2. Select `… → ノートの見出しを表示` (Display a note heading).
3. Enter a heading such as `Tasks`, and set daily switching to `yes`.
4. The content under today's `## Tasks` heading appears in the sticky note. Clicking a checkbox updates the corresponding line in the source file.

Date patterns use .NET syntax, which differs from Obsidian's Moment syntax and is case-sensitive. For example, use `yyyy-MM-dd` instead of `YYYY-MM-DD`, or `yyyy-MM-dd(ddd)` instead of `YYYY-MM-DD(ddd)`. Weekday names follow your Windows culture settings. The `.md` extension is added automatically. Subfolder patterns such as `yyyy/MM/yyyy-MM-dd` are supported.

To link a fixed note, enter its absolute path and set daily switching to `no`. When daily switching is `yes`, the path field is unused.

A section starts immediately after the selected heading and ends before the next heading at the same or a higher level. Subheadings are included. Other sections and YAML front matter are preserved. Duplicate matching headings prevent editing to avoid ambiguity. Headings inside fenced code are ignored. Use `#`-style (ATX) headings.

When the date changes, the app switches to today's file. If you are editing, switching waits until you save or reload. Missing files or headings produce an error and are not created automatically. Once you create them in Obsidian, the app displays them on a subsequent refresh.

## Google Calendar

Enter a command on its own line in a note and save:

```text
@calendar 2026-10-05T09:00+09:00 meeting
```

The search term is optional. If you omit the time zone offset, the app uses Windows local time. It retrieves up to 100 events, ordered by start time. Google's `timeMin` filters by event end time, so events already in progress at the specified time may also appear. Recurring events are expanded into individual occurrences.

Events refresh about every 60 seconds, or when you choose `… → 予定を今すぐ取得` (Fetch events now). Only the first command in each note is used. Leave blank lines around the command. Examples inside fenced code blocks are not executed.

Click an event to edit its title and description. Changes are sent to Google **only after you select OK** in the confirmation dialog, which shows the changes and attendee notification behavior. Times, attendees, and recurrence rules are not changed. Editing a recurring event affects the retrieved occurrence. If the event has changed on Google since it was loaded, an ETag check prevents overwriting it and asks you to refresh.

### Initial authentication

1. Select a project in [Google Cloud Console](https://console.cloud.google.com/) and enable the Google Calendar API.
2. Configure the OAuth consent screen. If the app is in testing, add your Google account as a test user.
3. Create an OAuth client of type **Desktop app** and download its JSON file.
4. Select that JSON file in the app's Settings. The Calendar ID for your main calendar is usually `primary`.
5. Select `設定を保存してGoogleにログイン` (Save settings and sign in to Google), then authenticate in your browser.

Authentication uses the system browser, a loopback redirect, and PKCE. Tokens are encrypted with Windows DPAPI for the current user and stored in `google-token.bin`. Keep the OAuth JSON out of repositories and shared vault folders.

A real account connection and update check requires your own OAuth configuration. Automated tests use simulated HTTP responses to verify searches, updates, and ETag conflicts.

References: [Google Desktop OAuth](https://developers.google.com/identity/protocols/oauth2/native-app), [Calendar resource versions](https://developers.google.com/calendar/api/guides/version-resources).

## Markdown support and data protection

- Renders headings, bold, italic, strikethrough, ordered and unordered lists, checklists, quotes, code, tables, and links.
- Displays HTML as text rather than executing it. Images currently appear as their alternative text.
- Obsidian `[[wikilinks]]`, embeds, Dataview, math rendering, and dedicated callout styling are not supported; their text is preserved.
- Reads UTF-8 with or without a BOM and preserves that choice. Body edits follow the file's existing line endings.
- Editing properties reserializes YAML, so comments or formatting may change. Unknown property values and the note body are retained.
- Checks the whole file's hash before writing. External changes block stale writes; you can copy or save your edits elsewhere before reloading.
- Backs up pre-write contents in `%LOCALAPPDATA%/StickyNotes/backups`. Files include a date and GUID and can be restored manually. Backups are not automatically deleted.
- Uses exclusive file access while writing and attempts to restore content if a write fails. Keep your normal vault backups as well, including protection against power loss.

## Development and verification

Requires Windows and the .NET 8 SDK.

```powershell
.\build.ps1
.\build.ps1 -Publish
```

The build uses `.tools/dotnet` when a local SDK is present. Tests use temporary files and simulated APIs, without accessing real vaults or external services. `-Publish` also creates a self-contained app, installer scripts, documentation, and `artifacts/StickyNotes-win-x64.zip` with a SHA256 checksum. Both README languages are included in the ZIP.

To launch with a separate data directory for testing (notes also appear in the taskbar in this mode):

```powershell
.\artifacts\app\StickyNotes.exe --data-dir C:\Temp\StickyNotes-test
```

Project layout: `StickyNotes.Core` handles Markdown storage, section editing, and Calendar REST/OAuth; `StickyNotes` contains the WPF notes, rendering, notification-area integration, and monitor layout; `StickyNotes.Tests` contains executable data protection, rendering, and API tests.

Work takes place on separate branches, with verification, code review, and security review before completion, and is committed locally. Unresolved critical or high-severity findings block merging and pushing. The normal workflow does not include creating a pull request. Merging into `main`, pushing any branch to GitHub, and publishing releases require an explicit instruction from the repository owner. See the [development guide (Japanese)](docs/DEVELOPMENT.md) for the release workflow.
