[English](USAGE.en.md) | [日本語](USAGE.md) | [简体中文](USAGE.zh-CN.md)

# Usage reference

For task keyword/date completion, Enter-to-continue checklists and native Tasks checkbox behavior, see [task editing](TASK-EDITING.md#english).

Use **Browse sticky-note folder…** from **…**, the tray menu or the taskbar menu to reopen closed notes with body/YAML previews. [Query checkbox updates and folder browsing](QUERY-EDITING-AND-BROWSER.md#english).

Current source builds also display Dataview LIST/TABLE/TASK queries through CLI, alongside Tasks and normal Markdown. Enable Dataview in the selected vault. Results refresh about every 30 seconds; verified task checkboxes update their source notes. DataviewJS, inline expressions and CALENDAR are unsupported. See [Dataview setup and limits](DATAVIEW.md#english). This is not included in the published v0.0.7 ZIP.

[Back to basic usage](../README.md)

Choose the section you need.

[Display language](#language) · [Editing and saving](#editing) · [Display and windows](#display) · [Storage and Obsidian](#storage) · [Daily notes](#daily) · [Headings and previews](#headings) · [Markdown support and data protection](#data)

<a id="language"></a>

## Display language

In Settings, choose **Display language → Use Windows language / 日本語 / English / 简体中文**, save, and restart the app. Windows language is the default: Japanese uses Japanese, Chinese uses Simplified Chinese, and other languages use English.

Menus, settings, guidance, app error messages, and new-note templates are translated. Existing note text, titles, headings, tags, and Google event content stay unchanged. Windows dialog buttons, external pages, and system/library errors may use another language.

Available from v0.0.5. Control names below follow the English UI.

<a id="editing"></a>

## Editing and saving

| Control | Action |
| --- | --- |
| `＋` | Create a note |
| `Edit` / `Ctrl+E` | Edit the Markdown source |
| `Save` / `Ctrl+S` / `Esc` | Save the file and return to reading mode |
| `Reload` | Reload the source file, with confirmation if edits are unsaved |
| `…` | Open properties, fetch events, open existing files, link a section, access settings, or exit |
| `×` | Close this note without deleting its Markdown file |
| `… → Exit app (save layout)`, or notification-area `Exit` | Exit the app |

In Settings, **Start editing body text with → Single click / Double click** selects the gesture for body text or blank space. The default remains single click, including existing settings; saving the setting applies it to open notes immediately. The editor places its caret at the corresponding Markdown source position and scrolls that line into view. Generated Tasks/Dataview results lead to their source query. Checkboxes, links, and scrollbars keep their usual actions. Selecting Edit again keeps your draft; saving returns to reading mode.

`Esc` saves and finishes editing without a confirmation, independently of the focus-loss setting. An unchanged note simply returns to reading; a save failure or conflict keeps the draft open. When a completion suggestion is open, the first `Esc` dismisses it and the next saves and finishes editing. IME composition and the editor context menu retain their own Escape handling.

Leaving the editor prompts only when text has changed.

| Choice | Result |
| --- | --- |
| Yes | Save |
| No | Discard and reload |
| Cancel | Keep the draft and continue editing |

Enable `Save automatically when the editor loses focus` to skip this prompt (off by default). Automatic saves still back up and detect conflicts; failures retain input. The context menu does not prompt. Closing a note or exiting still checks unsaved changes.

<a id="display"></a>

## Display and windows

Drag the top of a note to move it, or drag an edge or the bottom-right corner to resize it. Use `○ / ●` to toggle always-on-top mode. Notes have no standard title bar and normally stay out of the taskbar; the app runs in the notification area.

### Taskbar and menus

Enable `Show note icons in the taskbar` in Settings to show note icons in the taskbar (off by default). It applies to all notes and persists after restarting. The notification-area icon remains available.

Right-click the taskbar icon to access the same eight operations as the notification-area menu: New note, Open Markdown, Link a section, Display a daily note, Show all, Temporarily bring notes to the top, Settings, and Exit. Windows displays these as Jump List tasks alongside its own pin/close commands.

Show all also restores minimized notes. Operations are forwarded to the running app, retaining the usual save confirmation on exit.

If the app is stopped, selecting a task starts it and performs that operation; Exit alone does not start a new session. Restart the updated app once to register the menu.

The isolated `--data-dir` test mode does not register or replace the taskbar menu.

### Temporary always-on-top display

Choose `Bring all notes to the top (10 seconds)` from the notification-area or `…` menu to restore minimized notes and bring all notes to the top for 10 seconds. Repeat to extend the interval.

Each note then returns to its pin setting; the previous stacking order is not restored. Temporary display is not saved. Pressing `○ / ●` during the interval immediately applies a new permanent setting to that note.

### Button visibility

By default, Edit, Save, Reload, More, New, Pin, and Close are always visible in one row below the title. Edit has a blue background, Save green, Reload yellow, and More purple.

Enable `Show buttons when hovering over the title` in Settings to overlay buttons on the title. This removes the second row without shifting the body when buttons appear or disappear. Disable it to restore always-visible controls.

Use the left-edge handle to move the note, or `F6` then `Tab` for keyboard access. Buttons also remain visible while the title has focus or the More menu is open. The setting applies immediately to all notes and persists after restarting.

### Scale and placement

Use `… → Display scale` to set 50–200% (default 100%, ten-point increments). In reading mode, `Ctrl+wheel`, `Ctrl++` / `Ctrl+-`, and `Ctrl+0` (reset) also work.

Body content, events, and daily-note guidance scale together; titles, controls, and the editor do not. Each note retains its scale across reloads and restarts. Source Markdown and unsaved input remain unchanged.

The app saves note positions, sizes, and always-on-top settings as notes move and when you exit, then restores them on the next launch. It supports multiple monitors, negative screen coordinates, and PerMonitorV2 DPI. Notes on a disconnected display are moved back onto an available screen. Unsaved changes trigger a confirmation before exit.

<a id="storage"></a>

## Storage and Obsidian

After choosing a vault (CLI) or local base folder, select **Select sticky-note folder…**. Choose an existing folder or enter a new relative path; **Confirm** asks whether to migrate existing app-created sticky notes. **Yes** moves them, **No** changes only the destination, and **Cancel** changes nothing. See the [folder selection and migration guide](STICKY-FOLDERS.md#english) for scope, rollback and recovery.

An explicit sticky folder takes precedence over Obsidian's general new-note location. Without a selection, CLI mode retains Obsidian's default; local mode retains its saved folder (initially `Documents/StickyNotesData`). Daily-note settings remain independent.

### Properties and Bases

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

Use `… → Tags, title, status, and color` (Tags, title, status, and color) to edit properties. Available colors are `yellow / green / blue / pink / gray`. The `status` field accepts values such as `active / done / archived`; it is for organization and does not automatically hide or delete notes.

Copy [StickyNotes.base](../examples/StickyNotes.base) into your vault to list notes with `type: sticky`. To exclude sticky notes from other Bases, use a condition such as `type != "sticky"`, or exclude their folder with `!file.inFolder("Sticky Notes")`.

App layout and authentication data are kept outside the vault in `%LOCALAPPDATA%/StickyNotes`. The app checks for external file changes about every 30 seconds in CLI mode or two seconds in local mode. When you are editing a note, it reports a conflict instead of replacing your edits automatically.

References: [Obsidian Properties](https://obsidian.md/help/properties), [Bases syntax](https://obsidian.md/help/bases/syntax).

<a id="daily"></a>

## Daily notes

1. In CLI mode, configure Obsidian's core Daily notes plugin, then check its folder, format, template and today's path in Sticky Notes Settings. In local mode, configure the folder and tagged regex, check the matching filename, then select `Save and close`.
2. Select `… → Display a daily note…` (Display a daily note). The app loads today's file from Settings; no absolute-path input or daily-mode switch is needed.
3. In the shared heading selector, choose an existing heading or type a new name without `#`. Leave it blank to display the entire body.
4. Select `Display`. A new name creates a `## Heading` at the end of the original file after making a backup. Typing or cancelling does not write anything. Clicking a task checkbox updates the corresponding source line.

### Filename patterns

This subsection applies only to local mode. CLI mode resolves today and yesterday using Obsidian's native settings, without scanning the vault or matching a regex. Configure its date format in Obsidian; the waiting/retention options below apply to both modes.

Daily-note filenames use **date tags + a regular expression** only. There is no date-format mode or mode checkbox. The tags are the named groups `year`, `month`, and `day`. For names such as `2026-10-05(月).md`, choose the folder containing the notes and use:

```regex
(?<year>\d{4})-(?<month>\d{2})-(?<day>\d{2})(?:\([^)]+\))?\.md
```

A blank or whitespace-only field is filled with this default. The `Insert default pattern with date tags` (Insert default with date tags) button asks before replacing the entire input; **No** retains the existing expression. Opening Settings does not ask for confirmation. Changes update filename matching and are persisted when you save.

The Settings screen checks the unsaved folder and expression about 300 ms after you stop typing, displaying today's matching filename or an inline error. Changing the folder also refreshes matching. Searches run in the background, and outdated results are discarded.

<details>
<summary>Matching rules, legacy formats, and search limits</summary>

Each required tag must capture one numeric date part. Only files matching the target local date (normally today, or yesterday with retention enabled) are selected. Matching is case-insensitive and covers the **entire relative path, including `.md`**; the extension is not appended automatically. Use `/` for subfolders; for example, prefix the expression with `Diary/` when selecting the parent folder. This example excludes other dates and prefix variants such as `WeeklyTasksLog-…`.

Existing regex settings are preserved. When loading older settings, `yyyy-MM-dd` and `yyyy/MM/yyyy-MM-dd`, including versions ending in `(ddd)` or `(dddd)`, are converted to tagged regex. Other custom date formats are kept as text for manual correction in Settings; they are no longer evaluated as date formats. Files and folders are not moved by this conversion.

The search includes subfolders but skips symbolic links and junctions; the selected root must not contain these in its path. A missing daily note follows the waiting/retention behavior below; multiple matches produce an error. No files are created or chosen arbitrarily. Unsaved edits are retained on search errors. Invalid expressions are rejected when saving settings. Expressions are limited to 4,096 characters and 100 ms per match; each search checks a limit of 10,000 entries and one second between entries. Filesystem access itself can take longer. Select the daily-note folder directly to keep searches small.

</details>

### When today is missing

When the date changes, the app switches to today's file. If it has not been created, existing daily sticky notes show guidance in the body area: creating today's note in Obsidian will display it automatically, and Settings can keep yesterday's note visible. The app checks about every 30 seconds in CLI mode or two seconds in local mode.

Settings offers three choices under `When today's daily note is missing`:

- **Show waiting message (default)**: display guidance while today's note is missing.
- **1. Keep yesterday until today is created**: automatically switch when today's file appears.
- **2. Keep yesterday until Reload**: once yesterday is displayed because today is missing, keep it even after today's file appears, until you select `Reload`.

<details>
<summary>Retention and Reload details</summary>

If yesterday is also missing, show the waiting message; never go back further. Reload checks today in either retention mode; if missing, wait without returning to yesterday for the rest of that day.

Retention state lasts while the sticky note is open; restarting shows today if it already exists. The setting itself is saved.

A retained note displays its date and guidance; edits and task checks are saved to that dated source file. Automatic switching pauses during editing and preserves unsaved input.

Folder/regex errors, multiple matches, and missing headings remain errors. Files are not created automatically; new headings are added only when entered in the selection dialog and confirmed with Display.

Adding a daily sticky note for the first time requires today's file.

</details>

<a id="headings"></a>

## Headings and previews

For a fixed note, use the separate `… → Link part of a note…` (Link part of a note) menu. Select `Choose folder…` (Choose folder) to browse a folder and then select its Markdown file, or use `Choose Markdown file…` (Choose Markdown file) directly. There is no manual absolute-path field or daily-mode control. Both dialogs share the same heading selector: select an existing heading, enter a new one, or leave it blank for the whole body. Both menus are also available from the notification-area icon. Use `Reload` to refresh the heading list.

The read-only content preview is the final item in both the daily-note and fixed-note linking dialogs. Loading or reloading a file, selecting a heading, or typing a heading updates the Markdown preview of the chosen range, up to 4,000 characters. A blank heading previews the entire body; a new heading shows a pending append. Previewing never writes to the source file.

A section starts immediately after the selected heading and ends before the next heading at the same or a higher level. Subheadings are included. Other sections and YAML front matter are preserved. Duplicate matching headings prevent editing to avoid ambiguity. Headings inside fenced code are ignored. Use `#`-style (ATX) headings.

If the source changes externally before Display, the app asks you to reload without overwriting it. The same applies when the daily-note target changes, including at midnight. Appending preserves the original content, newlines, and UTF-8 BOM. Ambiguous duplicate headings and appends hidden inside an unclosed code fence are rejected.

<a id="data"></a>

## Markdown support and data protection

- Renders headings, bold, italic, strikethrough, ordered and unordered lists, checklists, quotes, code, tables, and links.
- Displays HTML as text rather than executing it. Images currently appear as their alternative text.
- Obsidian `[[wikilinks]]`, embeds, math rendering, and dedicated callout styling are not supported; their text is preserved. Dataview LIST/TABLE/TASK is supported through CLI in current source builds; DataviewJS, inline expressions and CALENDAR are unsupported. See [Dataview](DATAVIEW.md#english).
- Reads UTF-8 with or without a BOM and preserves that choice. Body edits follow the file's existing line endings.
- Editing properties reserializes YAML, so comments or formatting may change. Unknown property values and the note body are retained.
- Checks the whole file's hash before writing. External changes block stale writes; you can copy or save your edits elsewhere before reloading.
- Backs up pre-write contents in `%LOCALAPPDATA%/StickyNotes/backups`. Files include a date and GUID and can be restored manually. Backups are not automatically deleted.
- Uses exclusive file access while writing and attempts to restore content if a write fails. Keep your normal vault backups as well, including protection against power loss.
