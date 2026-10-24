[日本語](README.md) | [English](README.en.md) | [简体中文](README.zh-CN.md) | [繁體中文（台灣）](README.zh-TW.md) | [한국어](README.ko.md) | [Deutsch](README.de.md) | [Русский](README.ru.md) | [Español](README.es.md) | [Svenska](README.sv.md)

# Markdown Sticky Notes

[v0.0.11](https://github.com/AvocadoWasabi/StickyNotes/releases/tag/v0.0.11) · Windows 10 / 11 (x64)

用於在便利貼視窗中顯示與編輯 Markdown 檔案的 Windows 應用程式。支援本機檔案、Obsidian 筆記，以及每日筆記中的特定標題區段。編輯內容與核取方塊變更會儲存到原始 Markdown 檔案。

支援透過 Obsidian CLI 顯示 Tasks、Dataview 查詢結果，並取得 Google Calendar 行程與 Google Tasks。

[下載](https://github.com/AvocadoWasabi/StickyNotes/releases/latest) · [安裝設定](docs/INSTALL.zh-TW.md) · [使用指南（英文）](docs/USAGE.en.md) · [更新紀錄（英文）](CHANGELOG.md)

## 畫面範例

| 核取方塊操作 | Markdown 顯示 | 每日筆記標題區段顯示 |
| :---: | :---: | :---: |
| [<img src="docs/images/sticky-tasks-zh-TW.png" alt="核取方塊操作" width="280">](docs/images/sticky-tasks-zh-TW.png) | [<img src="docs/images/sticky-markdown-zh-TW.png" alt="Markdown 顯示" width="280">](docs/images/sticky-markdown-zh-TW.png) | [<img src="docs/images/sticky-daily-zh-TW.png" alt="每日筆記標題區段顯示" width="280">](docs/images/sticky-daily-zh-TW.png) |

使用應用程式的 WPF 控制項繪製範例筆記，並非桌面擷取。請參閱[各語言畫面範例與產生步驟](docs/SCREENSHOTS.zh-TW.md)。

## 執行環境與相依項目

**Windows 10 / 11（x64）**。ZIP 內含 .NET，無需系統管理員權限。

| 使用目的 | 所需工具 | 初始設定 |
| --- | --- | --- |
| 僅使用 Markdown 便利貼 | 本應用程式 | 在設定中關閉 **使用 Obsidian CLI（預設）**，選擇本機資料夾 |
| 編輯 Obsidian 筆記 | 執行中的 Obsidian＋Obsidian CLI | 啟用 Obsidian **Settings → General → Command line interface**，在本應用程式連接 Vault |
| 今日每日筆記 | 上述 CLI＋核心 **Daily notes** 外掛 | 在 Obsidian 設定資料夾與日期格式，建立今日筆記 |
| Tasks 查詢與原生勾選行為 | 上述 CLI＋社群 **Tasks** 外掛 | 在目標 Vault 啟用；v0.0.11需要穩定版 **8.2.2 以上** |
| Dataview 查詢 | 上述 CLI＋**Dataview** | 在目標 Vault 啟用；支援 LIST / TABLE / TASK |
| Google 行程與任務 | Google 帳戶＋自己的 OAuth 設定 | 啟用 Calendar API / Tasks API，準備桌面版 OAuth JSON 並[授權（英文）](docs/GOOGLE-CALENDAR.en.md#setup) |

**新設定預設啟用 CLI。** 不使用 Obsidian 時請關閉。CLI 為 Obsidian 內建功能，無需專用的 Sticky Notes Obsidian 外掛。[官方 CLI 需求](https://obsidian.md/help/cli) · [相容性與限制（英文）](docs/TASKS-CLI-PREVIEW.md#english)。

## 安裝與初始設定

1. 從 [Releases](https://github.com/AvocadoWasabi/StickyNotes/releases/latest) 下載 `StickyNotes-win-x64.zip`，完整解壓縮後執行 `Install.cmd`。
2. 在 Obsidian 啟用 CLI、開啟目標 Vault，再啟動 Markdown Sticky Notes。
3. 在應用程式設定中確認 `Obsidian.com` 的絕對路徑，輸入 Vault 名稱／ID，按 **透過 CLI 取得 Vault 儲存目錄**。選擇儲存資料夾，再按 **儲存並關閉**。
4. 從 `… → 建立與開啟` 新增便利貼、開啟 Markdown、連結筆記區段或顯示每日筆記。

本機檔案設定、更新與移除請參閱[安裝指南](docs/INSTALL.zh-TW.md)。此處的功能與設定適用於 v0.0.11。各版本變更見[更新紀錄（英文）](CHANGELOG.md#v0-0-11)。

## 功能清單

| 功能 | 內容 |
| --- | --- |
| Markdown 顯示與編輯 | 標題、清單、核取方塊、表格、程式碼、連結；儲存為一般 `.md` |
| 標題連結與每日筆記 | 顯示與編輯筆記的一部分，隨日期切換，今日筆記不存在時保留前一天的筆記 |
| Tasks / Dataview | 顯示查詢結果，勾選已驗證來源位置的任務，保留查詢原文 |
| 任務輸入 | 關鍵字／日期補全、Enter 延續清單、Tab 縮排、可設定勾選快捷鍵 |
| Google 整合 | `@calendar today` 顯示行程與有日期的未完成 Google Tasks；確認後修改行程標題／說明 |
| 外觀與視窗 | 色彩、大小、50–200% 縮放、置頂、暫時置頂、工作列顯示 |
| 便利貼管理 | 選擇／搬移儲存資料夾、重開已關閉便利貼、瀏覽／預覽、更名與刪除 |
| 備份與衝突偵測 | 寫入前備份、外部修改衝突偵測 |
| 多 Vault | 各 Vault 的每日筆記、自訂名稱與預設 Vault |

## 基本操作

| 操作 | 方法 |
| --- | --- |
| 編輯 | 點擊內文（可設定單擊／雙擊），或 `Ctrl+E` |
| 儲存並結束編輯 | `Ctrl+S` / `Esc`；補全候選開啟時，第一次 Esc 關閉候選 |
| 勾選任務 | 在閱讀模式點擊核取方塊 |
| 置頂 | `○ / ●` 固定；`Alt+Z` 將所有便利貼置頂 10 秒 |
| 關閉／結束 | `… → 關閉此便利貼（保留檔案）`／從通知區域結束應用程式 |

## 支援範圍與限制

- v0.0.11的 UI、README、安裝指南與畫面範例支援**日文、英文、簡體中文、台灣華語（繁體中文）、韓文、德文、俄文、西班牙文、瑞典文**九種語言。詳細參考資料通常仍為三種語言。變更語言後請儲存並重新啟動。
- CLI 整合需要 Obsidian 持續執行；內部 API 變動可能影響相容性。
- 不會自動建立每日筆記，首次新增前請準備今日筆記。
- 不完整重現 Obsidian 主題與外掛檢視。圖片顯示替代文字；不支援 DataviewJS、行內運算式與 CALENDAR。
- Google Tasks 為唯讀，到期資訊僅取得日期。不支援到期時間、與 Markdown 任務自動同步或新增 Google Calendar 行程。
- 備份位置：`%LOCALAPPDATA%/StickyNotes/backups`。發生衝突時保留草稿，重新載入後重試。

## 相關資料與問題回報

以下參考資料為英文：[儲存與搬移](docs/STICKY-FOLDERS.md#english) · [Tasks / CLI](docs/TASKS-CLI-PREVIEW.md#english) · [任務輸入](docs/TASK-EDITING.md#english) · [Dataview](docs/DATAVIEW.md#english) · [多 Vault](docs/MULTIPLE-VAULTS.md#english) · [便利貼管理](docs/QUERY-EDITING-AND-BROWSER.md#english) · [Google](docs/GOOGLE-CALENDAR.en.md) · [開發](docs/DEVELOPMENT.en.md) · [授權聲明](THIRD-PARTY-NOTICES.txt)

[回報問題或建議](https://github.com/AvocadoWasabi/StickyNotes/issues)時，請附上應用程式／Windows／外掛版本、CLI 或本機模式與重現步驟。請勿上傳個人筆記、OAuth JSON 或權杖。
