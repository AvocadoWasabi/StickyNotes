[日本語](INSTALL.md) | [English](INSTALL.en.md) | [简体中文](INSTALL.zh-CN.md) | [繁體中文（台灣）](INSTALL.zh-TW.md) | [한국어](INSTALL.ko.md) | [Deutsch](INSTALL.de.md) | [Русский](INSTALL.ru.md) | [Español](INSTALL.es.md) | [Svenska](INSTALL.sv.md)

# Installation and first setup

[Back to README](../README.en.md) · [Full usage reference](USAGE.en.md)

## 1. Get the app

For Windows 10 / 11 (x64). No administrator access or separate .NET installation required.

1. Download **StickyNotes-win-x64.zip** from [Releases](https://github.com/AvocadoWasabi/StickyNotes/releases/latest); `Source code` is for development.
2. Extract everything and run `Install.cmd`.
3. Start **Markdown Sticky Notes** from the Start menu.

To run without installing, launch `StickyNotes.exe` in the extracted folder and keep all accompanying files. The app is unsigned, so Windows may show a confirmation. Automatic startup is not registered.

This guide covers v0.0.11. See the [changelog](../CHANGELOG.md#v0-0-11) for multiple vaults, Tasks 8.2.2 support and the nine-language UI.

## 2. Choose how to store notes

### With Obsidian (default for new settings)

1. Enable Obsidian **Settings → General → Command line interface** and follow its registration prompt. If missing, check the installer update requirements in the [official CLI guide](https://obsidian.md/help/cli).
2. Open your vault and keep Obsidian running.
3. In sticky-note **Settings → General**, check the absolute path to `Obsidian.com`, normally `C:\Program Files\Obsidian\Obsidian.com`. Adjust it if installed elsewhere.
4. Under **Vaults & daily notes**, enable CLI for the selected entry, enter the Obsidian vault name/ID and click **Get Vault folder via CLI**. Blank uses Obsidian's default selection; typing alone does not fetch the folder.
5. Check the returned vault and daily settings, use **Select sticky-note folder…**, then **Save and close**.

When changing folders, Yes also migrates existing app-created notes, No changes future storage only, and Cancel aborts. [Migration scope and recovery](STICKY-FOLDERS.md#english). CLI failures do not fall back to local reads.

### Without Obsidian

Turn off **Use Obsidian CLI**, choose a local base and note folder, then save. Initial storage is `Documents/StickyNotesData`. Tasks/Dataview queries are unavailable. Local daily notes can use a folder and date-capture regular expression. [Daily setup examples](USAGE.en.md#daily).

## 3. Enable only the integrations you need

| Integration | In Obsidian / Google | In Sticky Notes |
| --- | --- | --- |
| Daily notes | Enable core Daily notes, set folder/date format and create today's note | `… → Create & open → Display a daily note…`; choose a heading or leave blank for the whole note |
| Tasks | Enable Tasks in the target vault; v0.0.11 requires stable 8.2.2+ | Open a note containing a `tasks` code block. [Setup and limits](TASKS-CLI-PREVIEW.md#english) |
| Dataview | Enable Dataview in the target vault | Display LIST / TABLE / TASK in `dataview` blocks. [Setup and limits](DATAVIEW.md#english) |
| Google | Set up Calendar API, Tasks API and desktop OAuth in your own Cloud project | Import JSON, authorize and write `@calendar today` in a separate paragraph. [Connection guide](GOOGLE-CALENDAR.en.md#setup) |

Ordinary Markdown checkboxes do not require Tasks. Task suggestions default to Automatic and stays off without Tasks unless manually enabled. No Google authorization is needed unless you use Google integration.

## 4. Use your first note

Use `… → Create & open` to create or open Markdown. Click the body / press `Ctrl+E` to edit and `Ctrl+S` / `Esc` to save. Daily linking initially needs today's file; the app does not create it. Choose a display language in Settings, save and restart. The v0.0.11 UI supports the nine languages listed in the README.

## Troubleshooting

| Symptom | Check |
| --- | --- |
| CLI cannot connect | Running Obsidian, CLI enabled, absolute `Obsidian.com` path and vault name/ID; retry the retrieval button |
| Today's note is missing | Core Daily notes settings and today's file; settings from other daily-note plugins are not inferred |
| Tasks asks for an update | Update Tasks to 8.2.2+ in the target vault's community plugins, then refresh Tasks / Dataview in the sticky note |
| Save conflict | Keep the draft and reload; the app is preventing a stale overwrite |
| Google authorization fails | Check APIs, OAuth, test users and permissions in the [stage-by-stage guide](GOOGLE-CALENDAR.en.md) |

## Updating, removal and data

**Update:** save edits, exit from the notification area, extract the new ZIP into a different folder and run `Install.cmd`. Notes, settings and backups are stored separately from the installed app.

**From v0.0.6 or earlier:** settings without a CLI selection now enable CLI. Disable it and save to keep using local mode; files do not migrate automatically. Custom daily patterns from v0.0.1 may need manual correction.

| Data | Location |
| --- | --- |
| Installed app | `%LOCALAPPDATA%/Programs/MarkdownStickyNotes` |
| Settings and credentials | `%LOCALAPPDATA%/StickyNotes` |
| Pre-write backups | `%LOCALAPPDATA%/StickyNotes/backups` |
| Notes | Your selected vault/folder |

**Remove:** exit the app, delete its installation folder and Start menu shortcut. Notes, settings and backups remain; review them before deleting separately.
