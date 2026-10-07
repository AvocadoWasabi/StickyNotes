[English](INSTALL.en.md) | [日本語](INSTALL.md) | [简体中文](INSTALL.zh-CN.md)

# Installation, updates, and removal

## Requirements

Windows 10 / 11 (x64). No separate .NET installation or administrator privileges are required. Local mode needs no Obsidian plugins. Unreleased local builds default to CLI mode: Obsidian must be running with CLI enabled; daily notes require its core Daily notes plugin and Tasks queries require Tasks. See [CLI setup](TASKS-CLI-PREVIEW.md#english).

## Installation

1. Download `StickyNotes-win-x64.zip` from [Releases](https://github.com/AvocadoWasabi/StickyNotes/releases). `Source code` is for developers.
2. Right-click the ZIP and select Extract All.
3. Double-click `Install.cmd` in the extracted folder.
4. Open Markdown Sticky Notes from the Start menu.

The app installs to `%LOCALAPPDATA%\Programs\MarkdownStickyNotes` without enabling automatic startup. It is unsigned; Windows may show a warning. Check the source and file before running it.

For portable use, run `StickyNotes.exe` from the extracted folder. Keep the whole folder; the other bundled files are required.

## First-time setup

Choose the display language in Settings, save, and restart. [Language details](USAGE.en.md#language).

For the current local build, open **Settings → Obsidian CLI**, use **Get open vault from Obsidian**, check the retrieved daily settings, then save. New-note and daily-note locations come from Obsidian; no duplicate folder or regex setup is needed. Published v0.0.6 and CLI-disabled local mode use manual note and daily-note folders. Existing explicit local-mode settings are preserved.

- [Basic usage](../README.md#basic-usage)
- [Folder migration and daily-note settings](USAGE.en.md#storage)
- [Optional Google Calendar setup](GOOGLE-CALENDAR.en.md#setup)

Authentication is unnecessary unless you use Google integration.

## Updates

Exit through `Exit` in the notification-area menu, extract the new ZIP to a separate directory, and run `Install.cmd` to replace the app files. Notes, settings, and backups are stored separately from the installation.

After upgrading from 0.0.1, check both folder settings and today's matching filename. Documented legacy date formats convert automatically to tagged regular expressions; custom formats need manual correction. Saving without confirmation defaults to off. See the [changelog](../CHANGELOG.md).

## Removal

1. Exit through `Exit` in the notification-area menu.
2. Delete `%LOCALAPPDATA%\Programs\MarkdownStickyNotes`.
3. Right-click Markdown Sticky Notes in the Start menu, open its file location, and delete the shortcut.

Settings, credentials, and backups remain in `%LOCALAPPDATA%\StickyNotes`; Markdown remains in your selected folder, or `Documents\StickyNotesData` by default. Check what you want to retain before deleting this data separately.
