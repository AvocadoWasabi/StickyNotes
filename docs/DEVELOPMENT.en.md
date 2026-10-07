[English](DEVELOPMENT.en.md) | [日本語](DEVELOPMENT.md) | [简体中文](DEVELOPMENT.zh-CN.md)

# Development and releases

Completion is compared against a pinned Tasks upstream approved fixture. All changes stay in TaskEditing, including the bounded local date helper. The clone is excluded; MIT notices are bundled. See [source revision, tests and parity limits](TASK-EDITING.md#english).

Task input rules, popup/IME behavior and the public Tasks checkbox adapter are grouped in `src/StickyNotes/TaskEditing/`. The window delegates to this module, sharing existing CLI transport and conflict-safe saves. See [architecture and validation](TASK-EDITING.md#english).

## Branch workflow

1. Create a dedicated branch before implementing a change, for example `git switch -c feat/example`.
2. Implement, update documentation, and validate. Review the complete diff for code quality and security before completion.
3. **Stop merging and pushing if serious findings remain unresolved** (Critical / High, P0 / P1, or major data loss or disclosure). Do not report completion if required reviews or validation cannot be finished. Fix, retest, and review again.
4. Commit verified changes locally and report validation, both reviews, and remaining limitations. Creating a PR is not part of the normal process.
5. **Merging into main and pushing any branch require an explicit instruction from the owner.** Pushing tags and publishing releases also require authorization.

`AGENTS.md` is local-only and excluded from Git and distributions. The public repository is [AvocadoWasabi/StickyNotes](https://github.com/AvocadoWasabi/StickyNotes).

## Build and verify

Use Windows and the .NET 8 SDK. A local `.tools/dotnet` SDK takes priority when present.

```powershell
./build.ps1
./build.ps1 -Publish
```

Both commands restore, build Release, run tests, and generate a self-contained app, installer scripts, documentation in all three languages, and licenses under `artifacts/app`. After a normal `./build.ps1`, run `artifacts/app/Install.cmd` to install locally. Exit any running Sticky Notes from the notification area first. The build prints the installer script path on completion; it does not run the installer itself.

`-Publish` additionally creates or updates `artifacts/StickyNotes-win-x64.zip` and its SHA256 checksum. A normal build leaves any existing ZIP unchanged, so use `-Publish` when distributing a ZIP.

Standard local builds default to Obsidian CLI; existing explicit local-mode settings are preserved. Configure **Settings → Obsidian CLI**. Obsidian supplies Daily notes settings; an optional sticky-folder override uses `FoldersBridge` for folder listing, creation and moves, with a shared local/CLI migration coordinator; `DailyBridge` resolves today/yesterday without listing the vault, and both sources share retention logic. The standard profile remains separate from the optional `./build.ps1 -Publish -TasksPreview` output in `artifacts/tasks-cli-preview`. See [CLI setup, architecture and limitations](TASKS-CLI-PREVIEW.md#english).

`DataviewBridge.js` uses the installed plugin's `query` + Markdown export APIs with the source-note path. Tasks and Dataview share rendering/lifecycle code but keep independent state and errors. Normal tests cover rendering and stale responses; the optional Node.js `--dataview-bridge-smoke` executes the actual generated CLI code with synthetic API data. See [Dataview architecture, limits and validation status](DATAVIEW.md#english).

`QueryTaskTargets.js` binds exported checkboxes to validated source snapshots; `NotesBridge.js` updates them with backups and hash conflict checks. `NoteBrowserWindow` lists notes via `NoteSource` and previews ordinary Markdown/YAML without executing queries. The optional `--query-task-smoke` checks the generated adapters against synthetic notes. See [checkbox and browser behavior](QUERY-EDITING-AND-BROWSER.md#english).

`src/StickyNotes.Core` handles storage, section editing, and Calendar integration; `src/StickyNotes` contains the WPF UI; `tests` covers data protection, rendering, and APIs.

Google Tasks uses the same OAuth client/token with the additional `tasks.readonly` scope. `GoogleTasks.cs` performs GET-only, paginated reads from a fixed HTTPS endpoint, treats scheduled dates as date labels rather than UTC instants, and caps each refresh at 100 matching tasks / 20 requests / 30 seconds. Tests cover pagination, bounds, filters, cancellation, partial failures, and rollover without real credentials.

### Testing with isolated data

Tests use temporary files and simulated APIs, without connecting to a real vault or Google. Real-account connection and update checks require your own OAuth setup.

Launch with a separate data directory using the following command. This mode shows notes in the taskbar without registering or replacing Jump Lists.

```powershell
.\artifacts\app\StickyNotes.exe --data-dir C:\Temp\StickyNotes-test
```

## Documentation languages

Update English, Japanese, and Simplified Chinese (zh-CN) together for READMEs, changelogs, installation/development guides, related documentation, and release descriptions. Match features, limitations, examples, versions, and links. A single file with all language sections is acceptable. Include language navigation and check packaging. Documentation translation does not imply translated UI or sample data. Do not replace published tags or ZIPs without an instruction.

## Publishing (only when requested)

Record completed features and fixes under Unreleased in the [English](../CHANGELOG.md), [Japanese](../CHANGELOG.ja.md), and [Simplified Chinese](../CHANGELOG.zh-CN.md) changelogs. On publication, record the actual version, date, and Release link consistently. Do not present drafts or plans as published. Update `docs/RELEASE-NOTES.md` for that release too.

1. Align `Version` in `src/StickyNotes/StickyNotes.csproj`, download links in all three READMEs, changelogs, and release notes. Complete `./build.ps1 -Publish` and both reviews, commit locally, then merge the requested branch into main and push.
2. Confirm GitHub Actions **Build and test** succeeds.
3. When release publication is authorized, manually run **Package release** on main with a new tag, for example `v1.0.0`.
4. The workflow validates, packages, and creates a **draft release**. Before publishing, check the ZIP's SHA256, executable version, tag target commit, bundled documentation, and absence of personal settings or other private files.

A push alone does not create or publish a Release. ZIPs are Release assets, not Git history.

Keep custom local test packages under `artifacts` and do not commit or upload them. Official releases use the ZIP and checksum freshly generated from main by the workflow.

## Excluded files

`.gitignore` excludes SDKs, NuGet caches, build output, personal settings, OAuth JSON, tokens, and local data. Check `git status --short` and the diff before committing.

## Icon

The source image is `docs/images/StickyNotes.png`, with the ICO under `src/StickyNotes/Assets`. Run `./scripts/Convert-AppIcon.ps1` to regenerate the multi-size Windows ICO. See [icon provenance](ICON.en.md).

## Display language

UI strings live in `src/StickyNotes.Core/Strings.resx` (Japanese), `Strings.en.resx`, and `Strings.zh-CN.resx`. Keep keys and format placeholders aligned, and retrieve strings through `L10n`. Initialize the language at startup; saving Settings defers the change until restart. Do not translate persisted note data or protocol keys. Build tests cover resource completeness, language persistence, menus, and data preservation. Distributions must include `en/StickyNotes.Core.resources.dll` and `zh-CN/StickyNotes.Core.resources.dll`.
