[English](INSTALL.en.md) | [日本語](INSTALL.md) | [简体中文](INSTALL.zh-CN.md)

# Installation, updates, and removal

## Requirements

Windows 10 / 11 (x64). No separate .NET installation, administrator privileges, or Obsidian plugins are required.

## Installation

1. Download `StickyNotes-win-x64.zip` from [Releases](https://github.com/AvocadoWasabi/StickyNotes/releases). `Source code` is for developers.
2. Right-click the ZIP and select Extract All.
3. Double-click `Install.cmd` in the extracted folder.
4. Open Markdown Sticky Notes from the Start menu.

The app installs to `%LOCALAPPDATA%\Programs\MarkdownStickyNotes` without enabling automatic startup. It is unsigned; Windows may show a warning. Check the source and file before running it.

For portable use, run `StickyNotes.exe` from the extracted folder. Keep the whole folder; the other bundled files are required.

## First-time setup

Right-click the notification-area icon, open `設定` (Settings), and select the notes and daily-note folders. Choose a folder in your vault for Obsidian integration. Authentication is unnecessary unless you use Google integration. The two folders are saved independently. Saving a folder change asks whether to migrate existing Markdown: Yes includes subfolders, No changes only the location for new notes, and Cancel aborts saving settings. See the [README](../README.md#storage-and-obsidian) for scope and restrictions.

See the [README](../README.md) for full usage and Google Calendar setup. The `examples` directory contains daily-note and Obsidian Bases examples.

Daily notes use regular expressions with year/month/day date tags. Blank input receives the default; the template button asks before replacement. Settings show today's matching filename while typing, and content previews appear at the end of the fixed/daily-note selection dialogs. After saving settings, use `… → デイリーノートを表示…` to choose an existing heading. A new name is appended on `表示` (Display); a blank field shows the whole body. Fixed notes use `… → ノートの一部分を付箋にする…` with folder/Markdown file selection and the same heading field. See [daily notes](../README.md#display-and-edit-a-daily-note-section) for weekday-suffix examples.

Click the body or `編集` (Ctrl+E) to edit, and `保存` (Ctrl+S) to save. Changed text prompts to save, discard, or continue when focus leaves. Enable `編集欄からフォーカスが外れたら、確認せず自動保存する` in Settings to save without confirmation; it defaults to off. Failed saves retain input. Checkboxes, links, and scrollbars work directly in reading mode.

## Updates

Exit through `終了` in the notification-area menu, extract the new ZIP to a separate directory, and run `Install.cmd` to replace the app files. Notes, settings, and backups are stored separately from the installation.

After upgrading from 0.0.1, check both folder settings and today's matching filename. Documented legacy date formats convert automatically to tagged regular expressions; custom formats need manual correction. Saving without confirmation defaults to off. See the [changelog](../CHANGELOG.md).

## Removal

1. Exit through `終了` in the notification-area menu.
2. Delete `%LOCALAPPDATA%\Programs\MarkdownStickyNotes`.
3. Right-click Markdown Sticky Notes in the Start menu, open its file location, and delete the shortcut.

Settings, credentials, and backups remain in `%LOCALAPPDATA%\StickyNotes`; Markdown remains in your selected folder, or `Documents\StickyNotesData` by default. Check what you want to retain before deleting this data separately.
