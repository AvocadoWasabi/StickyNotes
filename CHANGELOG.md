[English](CHANGELOG.md) | [日本語](CHANGELOG.ja.md)

# Changelog

User-facing changes are recorded here, newest releases first. Development commits are listed separately below.

See the [README](README.md) for usage and [GitHub Releases](https://github.com/AvocadoWasabi/StickyNotes/releases) for published packages when available.

## Unreleased

No changes yet.

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

Record changes under **Unreleased** as features or fixes are completed, and keep both languages in sync. When a release is explicitly authorized, move its entries to a heading containing the actual version and publication date, link to its GitHub Release, and start a new Unreleased section. Do not record a planned or draft release as published.

See the [development guide](docs/DEVELOPMENT.md) for the branch and release process. Creating this history does not create a tag or publish a release.
