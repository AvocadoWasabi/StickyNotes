[日本語](INSTALL.md) | [English](INSTALL.en.md) | [简体中文](INSTALL.zh-CN.md) | [繁體中文](INSTALL.zh-TW.md) | [한국어](INSTALL.ko.md)

# 安裝與首次設定

[返回 README](../README.zh-TW.md) · [詳細操作（英文）](USAGE.en.md)

## 1. 安裝應用程式

適用於 Windows 10 / 11（x64），無需系統管理員權限或另外安裝 .NET。

1. 從 [Releases](https://github.com/AvocadoWasabi/StickyNotes/releases/latest) 下載 **StickyNotes-win-x64.zip**；`Source code` 供開發使用。
2. 完整解壓縮後執行 `Install.cmd`。
3. 從開始功能表啟動 **Markdown Sticky Notes**。

免安裝使用時，直接執行解壓縮資料夾內的 `StickyNotes.exe`，請保留整個資料夾。應用程式未簽署，Windows 可能顯示確認。程式不會登錄自動啟動。

以下使用目前開發版的英文 UI 名稱；與公開 ZIP 不同時請查看[未發布紀錄（英文）](../CHANGELOG.md#unreleased)。

## 2. 選擇儲存方式

### 使用 Obsidian（新設定預設）

1. 啟用 Obsidian **Settings → General → Command line interface**，依提示註冊。如無此項，請確認[官方 CLI 指南](https://obsidian.md/help/cli)的安裝程式更新需求。
2. 開啟目標 Vault，保持 Obsidian 執行。
3. 在便利貼 **Settings → General** 確認 `Obsidian.com` 的絕對路徑，通常為 `C:\Program Files\Obsidian\Obsidian.com`。安裝在其他位置時請調整。
4. 在 **Vaults & daily notes** 為選取的項目啟用 CLI，輸入 Obsidian Vault 名稱／ID，按 **Get Vault folder via CLI**。留空使用 Obsidian 預設選擇；僅輸入不會取得資料。
5. 確認回傳的 Vault 與每日設定，按 **Select sticky-note folder…** 選擇儲存位置，最後 **Save and close**。

變更資料夾時，Yes 同時搬移既有應用程式便利貼，No 只變更未來的儲存位置，Cancel 中止。[搬移範圍與復原（英文）](STICKY-FOLDERS.md#english)。CLI 失敗不會自動改用本機讀取。

### 不使用 Obsidian

關閉 **Use Obsidian CLI**，選擇本機基準資料夾與便利貼儲存資料夾，再儲存。初始位置為 `Documents/StickyNotesData`。無法使用 Tasks／Dataview 查詢。本機每日筆記可透過資料夾與日期擷取正規表示式設定。[每日設定範例（英文）](USAGE.en.md#daily)。

## 3. 僅啟用需要的整合

| 整合 | Obsidian／Google 端 | 便利貼端 |
| --- | --- | --- |
| 每日筆記 | 啟用核心 Daily notes、設定資料夾／日期格式並建立今日筆記 | `… → Create & open → Display a daily note…` 選擇標題，留空顯示整篇 |
| Tasks | 在目標 Vault 啟用 Tasks；目前開發版需要穩定版8.2.2以上 | 開啟含 `tasks` 程式碼區塊的筆記。[步驟與限制（英文）](TASKS-CLI-PREVIEW.md#english) |
| Dataview | 在目標 Vault 啟用 Dataview | 顯示 `dataview` 的 LIST / TABLE / TASK。[步驟與限制（英文）](DATAVIEW.md#english) |
| Google | 在自己的 Cloud 專案設定 Calendar API、Tasks API 與桌面 OAuth | 匯入 JSON 並授權，在獨立段落輸入 `@calendar today`。[連線指南（英文）](GOOGLE-CALENDAR.en.md#setup) |

一般 Markdown 核取方塊不需要 Tasks。任務補全預設為自動，Tasks 未啟用時需手動開啟才生效。不使用 Google 整合就不需要 Google 授權。

## 4. 使用第一張便利貼

從 `… → Create & open` 新增或開啟 Markdown。點擊內文／`Ctrl+E` 編輯，`Ctrl+S`／`Esc` 儲存。首次連結每日筆記需有今日檔案，不會自動建立。顯示語言可在設定中變更並重新啟動生效；UI 僅支援日文、英文、簡體中文。繁體中文範例使用英文 UI。

## 常見問題

| 現象 | 檢查事項 |
| --- | --- |
| CLI 連線失敗 | Obsidian 是否執行、CLI 是否啟用、`Obsidian.com` 絕對路徑與 Vault 名稱／ID；重新按取得按鈕 |
| 今日筆記未顯示 | 核心 Daily notes 設定與今日檔案；不會推測其他每日筆記外掛的設定 |
| Tasks 提示更新 | 在目標 Vault 的社群外掛更新中升級至8.2.2以上，再重新整理便利貼的 Tasks / Dataview |
| 儲存衝突 | 保留草稿並重新載入；程式正在防止舊內容覆寫 |
| Google 授權失敗 | 依[分階段指南（英文）](GOOGLE-CALENDAR.en.md)檢查 API、OAuth、測試使用者與權限 |

## 更新、移除與資料

**更新：** 儲存修改並從通知區域結束應用程式，將新 ZIP 解壓縮到另一個資料夾後執行 `Install.cmd`。筆記、設定與備份和應用程式分開儲存。

**從v0.0.6以前升級：** 沒有 CLI 選項的舊設定也會啟用 CLI。繼續本機模式時請關閉並儲存；檔案不會自動搬移。v0.0.1 的自訂每日格式可能需要手動修正。

| 資料 | 位置 |
| --- | --- |
| 已安裝應用程式 | `%LOCALAPPDATA%/Programs/MarkdownStickyNotes` |
| 設定與憑證 | `%LOCALAPPDATA%/StickyNotes` |
| 寫入前備份 | `%LOCALAPPDATA%/StickyNotes/backups` |
| 筆記 | 所選 Vault／資料夾 |

**移除：** 結束應用程式，刪除安裝資料夾及開始功能表捷徑。筆記、設定與備份仍保留，請確認需要的資料後另外刪除。
