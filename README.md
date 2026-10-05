[English](README.md) | [日本語](README.ja.md) | [简体中文](README.zh-CN.md)

# Markdown Sticky Notes — Obsidian and Google Calendar on your Windows desktop

<img src="docs/images/StickyNotes.png" alt="Markdown Sticky Notes icon" width="112">

A Windows Markdown sticky notes app for keeping **Obsidian notes and Google Calendar events together on your desktop**. Open a daily-note task list beside a note showing calendar events, or display notes and events in the same sticky note.

Edit the original Markdown files in your Obsidian vault, check off tasks, and review appointments without leaving the sticky notes. Calendar event titles and descriptions can be edited after confirmation. Google Calendar is optional and requires your own OAuth setup; Markdown notes also work on their own, without Obsidian.

## Use Obsidian notes and Google Calendar together

| Workflow | How it works |
| --- | --- |
| Keep tasks and appointments side by side | Link the Tasks heading in today's daily note, then open another sticky note with a Calendar query |
| Put meeting notes beside the event details | Add an `@calendar` command to the Markdown body shown in a sticky note; matching events appear below the rendered note |
| Update information at its source | Task checks and Markdown edits are saved to the original `.md`; confirmed event title/description edits are sent to Google Calendar |

For example, after [setting up Google Calendar](#initial-authentication), save this in a note. Replace the example date, time, time zone, and search word with your own:

```markdown
## Meeting preparation
- [ ] Review the agenda in Obsidian
- [ ] Write down questions

@calendar 2026-10-05T09:00+09:00 meeting
```

The command searches events from the specified time; it does not automatically mean “today.” Calendar events refresh about every 60 seconds or on request. When linking only a heading, place the command within the displayed section. **Markdown tasks and calendar events remain separate data:** the app does not convert tasks into events, copy events into Markdown, or synchronize them automatically. Event times and attendees cannot be changed here.

[Download for Windows](#download-and-install) · [Display an Obsidian daily note](#display-and-edit-a-daily-note-section) · [Google Calendar setup](#initial-authentication)

## Screenshots

Version 0.0.2 note views rendered with the app's WPF controls and English sample notes. The app's controls are currently in Japanese. Click an image to view it at full size.

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
| Daily notes | Match today's file using a tagged regex and check off tasks from the sticky note |
| Google Calendar (optional) | Search and display events; edit titles and descriptions after confirmation |
| Data protection | Detect external changes, prevent conflicting overwrites, and back up files before writing |

The app's interface and menus are currently in Japanese. Documentation is available in English, Japanese, and Simplified Chinese.

## Download and install

Requires **Windows 10 / 11 (x64)**. No administrator privileges, separate .NET installation, Obsidian CLI, or Obsidian plugins are required.

1. Open [GitHub Releases](https://github.com/AvocadoWasabi/StickyNotes/releases).
2. Download `StickyNotes-win-x64.zip`. The `Source code` downloads are for developers.
3. Right-click the ZIP and choose **Extract All**.
4. Double-click `Install.cmd` in the extracted folder.
5. Launch **Markdown Sticky Notes** from the Start menu.

The installer places the app in `%LOCALAPPDATA%\Programs\MarkdownStickyNotes`. It does not enable automatic startup. [Download v0.0.3](https://github.com/AvocadoWasabi/StickyNotes/releases/tag/v0.0.3).

For portable use, run `StickyNotes.exe` directly from the extracted folder. Keep all accompanying files in either case. The app is unsigned; if Windows displays a warning, check the download source and file before proceeding.

### First-time setup

Right-click the notification-area icon and select `設定` (Settings), then choose a folder for your notes. Select a folder inside your vault to use the notes with Obsidian. To use daily notes, also set their folder and tagged regular expression. Google authentication is only needed if you use Google Calendar.

### Update and uninstall

To update, select `終了` (Exit) from the notification-area menu, extract the new ZIP to a separate folder, and run `Install.cmd`. Your notes and settings are retained.

**Updating from 0.0.1:** daily-note matching now uses named date tags and regular expressions. Documented date formats are converted automatically; check the matching filename in Settings and correct other custom formats manually. The notes folder and daily-note folder are now saved independently, so check both if they were previously mixed up. Saving on focus loss without confirmation is optional and off by default. See the [0.0.2 changelog](CHANGELOG.md) for all changes.

To uninstall, exit the app, delete `%LOCALAPPDATA%\Programs\MarkdownStickyNotes`, and remove its Start menu shortcut. You can locate the shortcut using **Open file location** from its context menu. Settings, credentials, and backups remain in `%LOCALAPPDATA%\StickyNotes`; Markdown files remain in the folder you selected, or `Documents/StickyNotesData` by default. Review any data you want to keep before deleting it separately.

## Documentation and examples

- [Changelog / version history](CHANGELOG.md)
- [日本語 / Japanese README](README.ja.md) / [简体中文 / Simplified Chinese README](README.zh-CN.md)
- [Installation, updates, and removal](docs/INSTALL.en.md)
- [Development, branch workflow, and releases](docs/DEVELOPMENT.en.md)
- [Third-party licenses](THIRD-PARTY-NOTICES.txt)
- [Obsidian Bases example](examples/StickyNotes.base) / [Daily note example](examples/Daily.md)

## Using the app

Launch the installed app from the Start menu, or run `StickyNotes.exe` from the extracted ZIP. When building from source, the published executable is `artifacts/app/StickyNotes.exe`.

Drag the top of a note to move it, or drag an edge or the bottom-right corner to resize it. Use `○ / ●` to toggle always-on-top mode. Notes have no standard title bar and normally stay out of the taskbar; the app runs in the notification area.

To also show note icons in the taskbar, enable `タスクバーにも付箋のアイコンを表示する` in Settings and save. This applies to all open notes and new notes, persists after restarting, and can be disabled again. The notification-area icon remains available. Existing settings default to off.

Right-click the taskbar icon to access the same eight operations as the notification-area menu: New note, Open Markdown, Link a section, Display a daily note, Show all, Temporarily bring notes to the top, Settings, and Exit. Windows displays these as Jump List tasks alongside its own pin/close commands. Show all also restores minimized notes. Operations are forwarded to the running app, retaining the usual save confirmation on exit. If the app is stopped, selecting a task starts it and performs that operation; Exit alone does not start a new session. Restart the updated app once to register the menu. The isolated `--data-dir` test mode does not register or replace the taskbar menu.

Choose `一時的に付箋を最前面に表示する（10秒間）` from the notification-area menu or a note's `…` menu to bring all open notes to the top for **10 seconds**, including restoring minimized notes. Choosing it again restarts the interval. Afterward, each note returns to its permanent pin setting; this does not restore the previous window stacking order. Temporary display is not saved as pinning. Using `○ / ●` during the interval cancels temporary display for that note and immediately applies the new permanent setting. Closing a note still closes it normally.

By default, Edit, Save, Reload, More, New, Pin, and Close are always visible in one row below the title. Edit has a blue background, Save green, Reload yellow, and More purple.

In Settings, enable `タイトルにマウスカーソルを重ねるとボタンを表示する` (Show buttons when hovering the mouse cursor over the title) and save to show buttons over the title when you hover there. This removes the second header row; revealing or hiding buttons does not shift the body. Drag the handle at the left edge to move the note. Press `F6` to focus the title, then `Tab` to select buttons for keyboard operation. Buttons also remain visible while keyboard focus is within the title area or the More menu is open. The setting applies immediately to all open notes and persists after restarting. Disable it and save to restore the always-visible row.

| Control | Action |
| --- | --- |
| `＋` | Create a note |
| `編集` / `Ctrl+E` | Edit the Markdown source |
| `保存` / `Ctrl+S` | Save the file and return to reading mode |
| `再読込` | Reload the source file, with confirmation if edits are unsaved |
| `…` | Open properties, fetch events, open existing files, link a section, access settings, or exit |
| `×` | Close this note without deleting its Markdown file |
| `… → アプリを終了`, or notification-area `終了` | Exit the app |

Use `… → 表示スケール` (Display scale) to resize a note's rendered content from **50–200%** (default 100%). Select a percentage or adjust by ten percentage points. In reading mode, use `Ctrl+mouse wheel` over the body, or `Ctrl++` / `Ctrl+-`; `Ctrl+0` resets to 100%. Headings, body text, tables, code, and checkboxes keep their proportions; Calendar results and daily-note guidance use the same scale. Buttons, title, tags, status, and the Markdown editor keep their normal sizes. Each note saves its own scale across reloads and restarts. Scaling does not modify the source Markdown or unsaved input.

The app saves note positions, sizes, and always-on-top settings as notes move and when you exit, then restores them on the next launch. It supports multiple monitors, negative screen coordinates, and PerMonitorV2 DPI. Notes on a disconnected display are moved back onto an available screen. Unsaved changes trigger a confirmation before exit.

Click body text or blank space in the body area to enter the Markdown editor. `編集` (Edit) and `Ctrl+E` also work. Checkboxes, links, and scrollbars keep their own actions without entering editing. Select `保存` (Save) or press `Ctrl+S` to save and return to reading. Selecting Edit again preserves your draft.

When focus leaves the editor for another control or application, changed text triggers a save dialog: **Yes** saves, **No** discards and reloads, and **Cancel** keeps the draft for continued editing. Unchanged text does not prompt. To save without confirmation, enable `編集欄からフォーカスが外れたら、確認せず自動保存する` in Settings and save the setting; it defaults to off. Automatic saves still create backups and check for external changes. A failed save reports the error and retains your input. The editor's context menu does not trigger this confirmation. Closing a note or exiting the app keeps the existing unsaved-change confirmation.

## Storage and Obsidian

New notes are saved in `Documents/StickyNotesData` by default. Open `… → 設定` (Settings) to select a folder in your Obsidian vault, such as `Vault/Sticky Notes`. When you save a folder change, the app asks whether to migrate existing note files. Choose **Yes** to move every `.md` file in the old folder, including subfolders and closed notes; **No** changes only the folder for new notes; **Cancel** leaves settings unchanged. Open notes keep their placement and unsaved edits. The notes folder and daily-note folder are independent settings: changing or migrating the notes folder never rewrites the daily-note folder value. If the old notes folder also contains daily Markdown files, those files are included in the move; explicitly update the daily-note folder if you want it to follow them. Files outside the old folder and non-Markdown files stay in place.

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

1. In Settings, configure your daily notes folder and tagged regular expression, check the matching filename, then select `保存して閉じる` (Save and close).
2. Select `… → デイリーノートを表示…` (Display a daily note). The app loads today's file from Settings; no absolute-path input or daily-mode switch is needed.
3. In the shared heading selector, choose an existing heading or type a new name without `#`. Leave it blank to display the entire body.
4. Select `表示` (Display). A new name creates a `## Heading` at the end of the original file after making a backup. Typing or cancelling does not write anything. Clicking a task checkbox updates the corresponding source line.

Daily-note filenames use **date tags + a regular expression** only. There is no date-format mode or mode checkbox. The tags are the named groups `year`, `month`, and `day`. For names such as `2026-10-05(月).md`, choose the folder containing the notes and use:

```regex
(?<year>\d{4})-(?<month>\d{2})-(?<day>\d{2})(?:\([^)]+\))?\.md
```

A blank or whitespace-only field is filled with this default. The `日時タグ付きの既定例を挿入` (Insert default with date tags) button asks before replacing the entire input; **No** retains the existing expression. Opening Settings does not ask for confirmation. Changes update filename matching and are persisted when you save.

Each required tag must capture one numeric date part. Only files matching the target local date (normally today, or yesterday with retention enabled) are selected. Matching is case-insensitive and covers the **entire relative path, including `.md`**; the extension is not appended automatically. Use `/` for subfolders; for example, prefix the expression with `Diary/` when selecting the parent folder. This example excludes other dates and prefix variants such as `WeeklyTasksLog-…`.

Existing regex settings are preserved. When loading older settings, `yyyy-MM-dd` and `yyyy/MM/yyyy-MM-dd`, including versions ending in `(ddd)` or `(dddd)`, are converted to tagged regex. Other custom date formats are kept as text for manual correction in Settings; they are no longer evaluated as date formats. Files and folders are not moved by this conversion.

The search includes subfolders but skips symbolic links and junctions; the selected root must not contain these in its path. A missing daily note follows the waiting/retention behavior below; multiple matches produce an error. No files are created or chosen arbitrarily. Unsaved edits are retained on search errors. Invalid expressions are rejected when saving settings. Expressions are limited to 4,096 characters and 100 ms per match; each search checks a limit of 10,000 entries and one second between entries. Filesystem access itself can take longer. Select the daily-note folder directly to keep searches small.

The Settings screen checks the unsaved folder and expression about 300 ms after you stop typing, displaying today's matching filename or an inline error. Changing the folder also refreshes matching. Searches run in the background, and outdated results are discarded.

The read-only content preview is the final item in both the daily-note and fixed-note linking dialogs. Loading or reloading a file, selecting a heading, or typing a heading updates the Markdown preview of the chosen range, up to 4,000 characters. A blank heading previews the entire body; a new heading shows a pending append. Previewing never writes to the source file.

For a fixed note, use the separate `… → ノートの一部分を付箋にする…` (Link part of a note) menu. Select `フォルダを選択…` (Choose folder) to browse a folder and then select its Markdown file, or use `Markdownファイルを選択…` (Choose Markdown file) directly. There is no manual absolute-path field or daily-mode control. Both dialogs share the same heading selector: select an existing heading, enter a new one, or leave it blank for the whole body. Both menus are also available from the notification-area icon. Use `再読込` (Reload) to refresh the heading list.

If the source changes externally before Display, the app asks you to reload without overwriting it. The same applies when the daily-note target changes, including at midnight. Appending preserves the original content, newlines, and UTF-8 BOM. Ambiguous duplicate headings and appends hidden inside an unclosed code fence are rejected.

A section starts immediately after the selected heading and ends before the next heading at the same or a higher level. Subheadings are included. Other sections and YAML front matter are preserved. Duplicate matching headings prevent editing to avoid ambiguity. Headings inside fenced code are ignored. Use `#`-style (ATX) headings.

When the date changes, the app switches to today's file. If it has not been created, existing daily sticky notes show guidance in the body area: creating today's note in Obsidian will display it automatically, and Settings can keep yesterday's note visible. The app checks about every two seconds.

Settings offers three choices under `今日のデイリーノートが未作成のとき` (When today's daily note is missing):

- **Show waiting message (default)**: display guidance while today's note is missing.
- **1. Keep yesterday until today is created**: automatically switch when today's file appears.
- **2. Keep yesterday until Reload**: once yesterday is displayed because today is missing, keep it even after today's file appears, until you select `再読込` (Reload).

If yesterday is also missing, show the waiting message; never go back further. Reload checks today in either retention mode; if missing, wait without returning to yesterday for the rest of that day. Retention state lasts while the sticky note is open; restarting shows today if it already exists. The setting itself is saved. A retained note displays its date and guidance; edits and task checks are saved to that dated source file. Automatic switching pauses during editing and preserves unsaved input. Folder/regex errors, multiple matches, and missing headings remain errors. Files are not created automatically; new headings are added only when entered in the selection dialog and confirmed with Display. Adding a daily sticky note for the first time requires today's file.

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

The build uses `.tools/dotnet` when a local SDK is present. Tests use temporary files and simulated APIs, without accessing real vaults or external services. Every normal build generates a self-contained app, installer scripts, documentation, licenses, and READMEs/changelogs in all three languages under `artifacts/app`. On completion, exit any running Sticky Notes from the notification area, then run `artifacts/app/Install.cmd` to install locally. The build prints this path; installation is not automatic.

`-Publish` additionally creates or updates `artifacts/StickyNotes-win-x64.zip` and its SHA256 checksum. Normal builds leave existing ZIPs unchanged. Builds from this source include all three documentation languages; previously published ZIPs retain their original documentation.

To launch with a separate data directory for testing (notes also appear in the taskbar in this mode):

```powershell
.\artifacts\app\StickyNotes.exe --data-dir C:\Temp\StickyNotes-test
```

Project layout: `StickyNotes.Core` handles Markdown storage, section editing, and Calendar REST/OAuth; `StickyNotes` contains the WPF notes, rendering, notification-area integration, and monitor layout; `StickyNotes.Tests` contains executable data protection, rendering, and API tests.

Work takes place on separate branches, with verification, code review, and security review before completion, and is committed locally. Unresolved critical or high-severity findings block merging and pushing. The normal workflow does not include creating a pull request. Merging into `main`, pushing any branch to GitHub, and publishing releases require an explicit instruction from the repository owner. Keep the English, Japanese, and Simplified Chinese documentation in sync. See the [development guide](docs/DEVELOPMENT.en.md) for the release workflow.
