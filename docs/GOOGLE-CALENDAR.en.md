[English](GOOGLE-CALENDAR.en.md) | [日本語](GOOGLE-CALENDAR.md) | [简体中文](GOOGLE-CALENDAR.zh-CN.md)

[Display language](USAGE.en.md#language)

# Google Calendar usage and setup

[Back to basic usage](../README.md)

[Search and edit](#usage) · [Initial authentication](#setup) · [Delete imported JSON](#json) · [Troubleshooting](#troubleshooting)

<a id="usage"></a>

## Search and edit

Enter a command on its own line in a note and save:

```text
@calendar 2026-10-06T09:00
```

Use `@calendar today` to follow the PC's current day automatically (local midnight to the next midnight). `@calendar today meeting` filters by keyword. The date is checked and events are refreshed about every 60 seconds while viewing the note, including after waking from sleep; automatic refresh pauses while editing.

`today` includes events overlapping the current day, including all-day and multi-day events, up to 100 results. Each refresh resolves the PC's date again without changing the saved command. Explicit dates remain fixed and have no upper date limit. If a new day's retrieval fails, the previous day's results are cleared and retrieval is retried at the normal interval. The existing refresh timer is reused; changing the PC clock backwards does not pause the refresh interval.

**You can search without a keyword or time zone offset**, as in the example above. Without an offset, the app uses Windows local time. To filter events, append a keyword, for example `@calendar 2026-10-06T09:00 meeting`. Explicit offsets such as `2026-10-06T09:00+09:00` are also accepted. It retrieves up to 100 events, ordered by start time. Google's `timeMin` filters by event end time, so events already in progress at the specified time may also appear. Recurring events are expanded into individual occurrences.

While editing, type `@` at the start of a separate paragraph (leave a blank line after preceding text) to show the `@calendar` suggestion and floating usage examples. Press Tab or Enter, or click the suggestion, to insert today's date at `00:00` without an offset or keyword. The date and time are selected so you can replace them, then save the note. The inserted date stays fixed; it does not advance automatically. Esc closes the suggestion. Completion does not save the note itself and does not appear inside email addresses or code blocks.

Events refresh about every 60 seconds, or when you choose `… → Fetch events now` (Fetch events now). Only the first command in each note is used. Leave blank lines around the command. Examples inside fenced code blocks are not executed.

Click an event to edit its title and description. Changes are sent to Google **only after you select OK** in the confirmation dialog, which shows the changes and attendee notification behavior. Times, attendees, and recurrence rules are not changed. Editing a recurring event affects the retrieved occurrence. If the event has changed on Google since it was loaded, an ETag check prevents overwriting it and asks you to refresh.

<a id="tasks"></a>

## Google Tasks

`@calendar` also retrieves dated, incomplete Google Tasks from all lists of the signed-in account, including assigned tasks. Calendar ID only selects the calendar, not a task list. `today` selects the PC's current date; an explicit date selects that date and later dates, ignoring its time and offset for Tasks. Undated, completed, hidden and deleted tasks are excluded.

Tasks appear separately below events with their list name, date and a “no time” label; hover to read notes. They are read-only: editing, completion, creation and Markdown synchronization are unsupported. A keyword matches the full entered phrase in the title or notes, ignoring case. Google exposes the scheduled date but not the scheduled time through the [Tasks API](https://developers.google.com/workspace/tasks/reference/rest/v1/tasks).

The existing approximately 60-second refresh also refreshes Tasks, pauses during editing, and follows date changes. Each note's Tasks refresh is bounded to 100 matching tasks, 20 API requests and 30 seconds. Lists and tasks are paginated within these limits; a visible warning identifies partial results, which are sorted by date, list and title. A Tasks failure leaves calendar events visible, and a calendar retrieval failure still allows Tasks to load. Each section shows its own error.

**Existing users:** In the same Google Cloud project, enable [Google Tasks API](https://console.cloud.google.com/apis/library/tasks.googleapis.com), add `https://www.googleapis.com/auth/tasks.readonly` under Data Access, then use **Step 3** in Settings to sign in again and grant Tasks read access. Keep the existing desktop JSON. Step 4 checks access but cannot grant the additional permission. An unavailable API or denied Tasks permission shows this guidance in the note. [Tasks authorization](https://developers.google.com/workspace/tasks/auth).

<a id="setup"></a>

## Initial authentication

Open the connection guide in the right-hand `Google Calendar / Tasks` pane of Settings. The left pane contains note and daily-note settings; each pane scrolls independently. These instructions cover personal use with your own Google account and were checked against official documentation on 2026-10-06. Buttons open your system browser, where you complete Google registration and consent. Labels vary with the display language.

### 1-1: Enable the API

Open [Google Calendar API](https://console.cloud.google.com/apis/library/calendar-json.googleapis.com). Use the project selector at the top to select a project, or create and select one with **New project**, using a name such as `StickyNotes Personal`. Click **Enable**; **Manage** indicates it is already enabled. Keep **the same project** selected throughout the remaining steps.

Also enable [Google Tasks API](https://console.cloud.google.com/apis/library/tasks.googleapis.com) in this same project.

### 1-2: Register the app

In [Branding](https://console.cloud.google.com/auth/branding), click **Get started** if not configured. Enter an app name such as `StickyNotes Personal` and a support email you can receive, then **Next**. For a personal account choose **External**, then **Next**. Enter your contact email, then **Next**. Review the policy and, if you agree, check the agreement and choose **Continue → Create**. If already configured, review it and continue. **Internal** is for limiting access to members of a Google Cloud organization.

### 1-3: Register the account that will sign in

In [Audience](https://console.cloud.google.com/auth/audience), when the user type is **External** and status is **Testing**, use **Test users → Add users**, enter the email of the account whose calendar you will access, and **Save**. This can differ from your Cloud administration account. You do not need to publish the app for this procedure.

### 1-4: Save the scope

Add and save both scopes: `https://www.googleapis.com/auth/calendar.events` for viewing/editing events, and `https://www.googleapis.com/auth/tasks.readonly` for read-only Tasks access.

For an external app, open [Data Access](https://console.cloud.google.com/auth/scopes) → **Add or Remove Scopes** and select `https://www.googleapis.com/auth/calendar.events`. If absent, enter the full URL under **Manually add scopes** and add it. Choose **Update**, then **Save**. This permits viewing and editing events; it differs from `calendar.events.readonly`. The app's Calendar ID does not restrict the OAuth grant to that single calendar.

### 1-5: Obtain desktop credentials

Open [Clients](https://console.cloud.google.com/auth/clients) → **Create client**, set **Application type** to **Desktop app**, enter a name such as `StickyNotes Desktop`, and choose **Create**. Use **Download JSON** in the creation result **before closing it**; the secret may not be retrievable later. Do not configure web-app redirect URIs or JavaScript origins. An API key or service-account JSON is not suitable.

### App Step 2: Select and import the JSON

Select the downloaded file with `Select and check JSON`, then click the adjacent `Import into app`. The app validates the format, copies it to `%LOCALAPPDATA%\StickyNotes\credentials-google.json`, and immediately saves this path. Importing again replaces the managed copy. The source remains unchanged and may be moved or deleted after successful import. If you skip importing and use the original path, keep that file in place. Renaming is unnecessary. Use `primary` for your own main calendar.

### App Step 3: Sign in

Click `Step 3: Save settings and sign in to Google`. Within three minutes, select the account added in 1-3, verify the app name and allow Calendar access and Tasks read access in the browser. The app receives the result automatically. Close the browser tab, return to Settings and check for `Connection verified`. The app saves the token and verifies read access to the selected calendar and task lists without modifying events or tasks.

### App Step 4: Recheck the connection

When signed in, this checks access without opening the login screen. Steps 3 and 4 save all entered settings, including those unrelated to Google. Successful verification does not guarantee permission to edit events.

<a id="cancel"></a>

## Cancel and retry

You can cancel a login/check; closing Settings also cancels it. Close the browser's login tab after cancellation. Completed authentication remains saved. After an authentication timeout, restart from Step 3.

<a id="json"></a>

## Delete imported JSON

The adjacent `Delete imported JSON` (Delete imported JSON) asks for confirmation and deletes only the app's managed copy, clearing the saved path if it points to that copy. It leaves the original file, external files and authentication tokens intact and does not revoke access at Google. Import and deletion take effect immediately without saving unsaved entries in other settings fields. A settings-save failure restores the previous copy. These buttons are unavailable during authentication/connection checks. Storage is in the current Windows user's application-data directory, not beside the executable.

<a id="troubleshooting"></a>

## Troubleshooting

| Situation | What to check or do |
| --- | --- |
| `access_denied` on Google's login page | Check the test user from 1-3 and the account signing in. Contact your administrator for organization policy blocks |
| Unverified-app warning | Verify it is your own client's app name and account; stop if unsure |
| Connection check fails after login | Check Calendar ID, API enablement in the same project and granted access. Tokens are retained. Use Step 4 to recheck, or Step 3 to grant access again |
| External/Testing app asks for login again after several days | Refresh tokens for this scope expire after seven days; sign in again with Step 3 |

<a id="credentials"></a>

## Credential storage

Authentication uses the system browser, a loopback redirect, and PKCE. Tokens are encrypted with Windows DPAPI for the current user and stored in `google-token.bin`. Keep the OAuth JSON out of repositories and shared vault folders.

<details>
<summary>Sources and technical details</summary>

Setup references: [Consent configuration](https://developers.google.com/workspace/guides/configure-oauth-consent), [audience and test users](https://support.google.com/cloud/answer/15549945), [Calendar scopes](https://developers.google.com/workspace/calendar/api/auth), [client creation and JSON storage](https://support.google.com/cloud/answer/15549257), [refresh-token expiration](https://developers.google.com/identity/protocols/oauth2#expiration).

A real account connection and update check requires your own OAuth configuration. Automated tests cover loopback callbacks, state/path rejection, PKCE, denial, cancellation, invalid token responses, storage failure, read-only connection checks, searches, updates and ETag conflicts using simulated Google responses.

References: [Google Desktop OAuth](https://developers.google.com/identity/protocols/oauth2/native-app), [Calendar resource versions](https://developers.google.com/calendar/api/guides/version-resources).

</details>
