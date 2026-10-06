[English](CHANGELOG.md) | [日本語](CHANGELOG.ja.md) | [简体中文](CHANGELOG.zh-CN.md)

# Changelog

User-facing changes are recorded here, newest releases first. Development commits are listed separately below.

See the [README](README.md) for usage and [GitHub Releases](https://github.com/AvocadoWasabi/StickyNotes/releases) for published packages when available.

## Unreleased

## [0.0.6](https://github.com/AvocadoWasabi/StickyNotes/releases/tag/v0.0.6) — 2026-10-07

- Explicitly state in Settings, completion help, and all three documentation languages that the Google Tasks API provides dates only and cannot retrieve scheduled times set in Google.

- Display dated, incomplete Google Tasks alongside `@calendar` events as read-only entries, including `today` rollover and keyword filtering. Add Tasks read-only OAuth permission, API setup/re-authorization guidance and connection verification. Bound pagination, preserve the other service's results on failure, and cancel requests when the note closes or its command changes.

- Add `@calendar today [keyword]` for the PC's current local day. Reuse the approximately 60-second refresh cycle to follow date changes and resume after sleep, use an interval unaffected by clock corrections, and discard results from the previous day. Add translated usage examples; explicit dates remain fixed.

## [0.0.5](https://github.com/AvocadoWasabi/StickyNotes/releases/tag/v0.0.5) — 2026-10-06

- Add Japanese, English, and Simplified Chinese UI with a Windows-language default and a Settings selector applied after restart. Translate menus, dialogs, Google setup guidance, errors, and new-note templates; preserve existing note and event content. Include translation resources in the app package.

## [0.0.4](https://github.com/AvocadoWasabi/StickyNotes/releases/tag/v0.0.4) — 2026-10-06

- Shorten the README to setup and basic usage. Move details into the [usage reference](docs/USAGE.en.md) and [Google Calendar guide](docs/GOOGLE-CALENDAR.en.md), remove duplicated installation-guide instructions, and align all three languages.

- Add `@calendar` completion when typing `@` in a separate paragraph, with floating examples, Tab/Enter/click insertion, and Esc dismissal. Insert today's date without a time zone or keyword and select the date for editing. Clarify keyword-free searches and Windows local-time defaults in the app and documentation; parse dates consistently across regional settings.

- Split Settings into two columns: move the Google Calendar guide, JSON management, authentication and connection checks into the right pane. Keep note and daily-note settings on the left, with independent scrolling in each pane.

- Add adjacent buttons to import OAuth JSON into the app-data directory and delete the managed copy. Preserve the original, save only the credential path immediately, and restore the copy if settings cannot be saved. Deletion retains tokens and Google-side authorization.

- Check the Google setup guide against official documentation and specify registration fields, External/Internal selection, test users, scope saving and when to download JSON. Add direct settings links, a selectable scope URL, the seven-day External/Testing token lifetime and troubleshooting by error stage.

- Add an in-app Google Calendar setup guide with Cloud Console links, desktop OAuth JSON validation, browser sign-in, and automatic read-only connection verification. Support retrying checks, cancelling pending authentication/checks, and clear timeout messages; keep tokens from completed sign-ins when verification fails.

- Add the same eight notification-area operations to the taskbar right-click Jump List. Route selections to the running app through a bounded, current-user command channel, and restore minimized notes with Show all. Keep the normal unsaved-edit confirmation on Exit.

- Add an opt-in setting to show note icons in the taskbar alongside the notification-area icon, applying immediately to open notes and persisting across restarts.
- Preserve saved note positions and sizes while minimized from the taskbar.
- Add a notification-area and note menu action to bring all open notes to the top for 10 seconds, restore minimized notes, and then return to each note's permanent pin setting. Repeating restarts the interval; pin changes take effect immediately without persisting temporary state.

## [0.0.3](https://github.com/AvocadoWasabi/StickyNotes/releases/tag/v0.0.3) — 2026-10-06

### Features

- Rename the button display setting to `タイトルにマウスカーソルを重ねるとボタンを表示する` (Show buttons when hovering the mouse cursor over the title), preserving behavior and saved preferences.
- Add an option to overlay note buttons on the title when hovered, with a drag handle and F6/Tab keyboard access. Switching between overlay and always-visible modes applies to all notes and persists, while preserving body position and edits.
- Add per-note display scaling from 50–200%, saved across restarts. Use the Display scale menu, Ctrl+wheel / Ctrl++ / Ctrl+- in reading mode, or Ctrl+0 to reset; Markdown content, events, and daily guidance scale together.
- Generate the self-contained app and installer files under `artifacts/app` on every normal build, and print the `Install.cmd` path on completion. `-Publish` adds ZIP and SHA256 generation.
- Show guidance inside daily sticky notes when today's file is missing, explaining automatic display after creation in Obsidian and the retention settings.
- Add settings to keep yesterday's note until today's is created or until the sticky note is reloaded, with a visible date and protection for unsaved edits.

### Documentation

- Explain using Obsidian Markdown notes and Google Calendar together, with a combined-use example and clear boundaries on synchronization and event editing.
- Add Simplified Chinese documentation and language navigation, and English versions of the installation, development, and icon guides. Keep all three languages synchronized and include the Chinese README/changelog in future builds.

## [0.0.2](https://github.com/AvocadoWasabi/StickyNotes/releases/tag/v0.0.2) — 2026-10-05

Easier Markdown editing, separate note-linking dialogs, tagged daily-note matching, and safer folder changes. When updating from 0.0.1, check the daily-note match and both folder settings; custom date formats may need manual correction.

### Fixed

- Stop the reading preview from overlapping the transparent Markdown editor. Switch views, focus the input after layout, and preserve unsaved text when Edit is selected again.

- Keep the notes folder and daily-note folder independent when saving settings, including when migrating notes from a shared or parent folder. Folder pickers open at their respective configured folders.

### Changed

- Place all note actions in one row beneath the title, with distinct backgrounds for Edit, Save, Reload, and More. Keep the row within the minimum note width.
- Move content previews from Settings to the end of both note-linking dialogs, updating the preview for the selected heading. Settings retain live filename matching and errors.

- Unify daily-note matching around date tags and regular expressions. Remove the format-mode checkbox, keep an explicit template-insertion button, and convert documented legacy date formats when loading settings. Preserve other custom formats for manual correction.

### Added

- Click the note body to edit Markdown while preserving checkbox, link, and scrollbar actions.
- Prompt to save, discard, or keep editing when the editor loses focus. Add an opt-in setting to save automatically without confirmation, retaining drafts on conflicts or write failures and preventing duplicate dialogs.

- Separate fixed-note and daily-note display menus. Add folder/Markdown file pickers for fixed notes and remove manual absolute-path and daily-switch inputs.
- Share an editable heading selector between both dialogs. Choose headings from the loaded note, leave the field blank for the entire body, or append a new heading at the end on Display with backups and external-change checks.

- Auto-fill a blank daily regex with the default named-date expression. The template-insertion button asks before replacing existing text with that example; declining retains a nonempty expression.

- Live daily-note filename matching in Settings, with inline errors and background searches that discard outdated results. Show a read-only content preview in the note-linking dialogs.
- Tagged regular expressions for daily-note paths, using named year/month/day groups to select today's file, including weekday suffixes and subfolders. Reject ambiguous matches and limit regex matching and search work.
- Ask whether to migrate existing Markdown files when changing the notes folder, with Yes / No / Cancel choices. Include subfolders and closed notes, and preserve open-note edits and placement.
- Prevent overwriting destination files and roll back moves if migration or settings persistence fails; report any files needing manual recovery.

## [0.0.1](https://github.com/AvocadoWasabi/StickyNotes/releases/tag/v0.0.1) — 2026-10-05

First public release for Windows x64.

### Added

- Windows desktop sticky notes backed by ordinary Markdown files, with multiple notes, resizing, always-on-top mode, and restored window positions.
- Markdown viewing and editing, interactive checklists, YAML properties, and Obsidian vault integration.
- Linked heading sections and daily note switching without an Obsidian plugin.
- Optional Google Calendar event search and title/description editing with confirmation.
- External-change detection, conflict protection, and backups before writing files.
- A custom app icon for the executable, windows, notification area, and installed shortcut.
- A Windows x64 self-contained ZIP, per-user installation script, Start menu shortcut, and SHA256 checksum.
- English and Japanese READMEs covering features, installation, usage, updates, removal, and limitations.
- Six actual app screenshots with Japanese and English sample notes, plus reproducible sample data.
- English and Japanese changelogs linked from the READMEs and included in the distribution ZIP.
- GitHub build/test checks and a manually triggered workflow that prepares a draft release.

### Changed

- Keep local development instructions (`AGENTS.md`) out of Git and distribution packages. Use separate branches and local commits, with code and security reviews before completion. The normal workflow does not include PR creation; merging and pushing require explicit instructions, and unresolved major findings block both.
- Buttons use a pale blue background with darker text and borders, making folder selection and other actions easier to distinguish from input fields.

### Current limitations

- App controls are currently in Japanese; English documentation and sample note content do not change the interface language.
- The app is unsigned. Google Calendar requires your own desktop OAuth configuration.
- Images are shown as alternative text. Obsidian-specific embeds, Dataview, math rendering, and dedicated callout styling are not supported.

## Development history

These milestones are source commits, **not published versions**. All dates below are 2026-10-05.

| Commit | Milestone |
| --- | --- |
| [8ffc61e](https://github.com/AvocadoWasabi/StickyNotes/commit/8ffc61e) | Added Japanese and English app screenshots, sample notes, and capture instructions |
| [06dbe79](https://github.com/AvocadoWasabi/StickyNotes/commit/06dbe79) | Added bilingual READMEs with first-line language links and bundled both languages |
| [de2d9bf](https://github.com/AvocadoWasabi/StickyNotes/commit/de2d9bf) | Added the app icon, installer, documentation, Git exclusions, and distribution workflows |
| [4e3a46a](https://github.com/AvocadoWasabi/StickyNotes/commit/4e3a46a) | Imported the initial Markdown sticky notes app, including the updated button colors |

## Maintaining this history

Record changes under **Unreleased** as features or fixes are completed, and keep English, Japanese, and Simplified Chinese in sync. When a release is explicitly authorized, move its entries to a heading containing the actual version and publication date, link to its GitHub Release, and start a new Unreleased section. Do not record a planned or draft release as published.

See the [development guide](docs/DEVELOPMENT.en.md) for the branch and release process. Creating this history does not create a tag or publish a release.
