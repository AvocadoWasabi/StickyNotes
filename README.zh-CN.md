[日本語](README.md) | [English](README.en.md) | [简体中文](README.zh-CN.md) | [繁體中文](README.zh-TW.md) | [한국어](README.ko.md)

# Markdown Sticky Notes

**把 Obsidian 的今日任务放到桌面，直接勾选完成。**

将整篇笔记或每日笔记中的一个标题区域变成 Windows 便签，编辑直接保存到原 Markdown。也可随时查看 Tasks、Dataview 查询结果和 Google Calendar 日程。

[下载](https://github.com/AvocadoWasabi/StickyNotes/releases/latest) · [安装设置](docs/INSTALL.zh-CN.md) · [使用指南](docs/USAGE.zh-CN.md) · [更新日志](CHANGELOG.zh-CN.md)

## 适合怎样使用？

- **随时看到今日任务** — 仅显示每日笔记的 Tasks 部分，并随日期切换。
- **只维护一份笔记** — 编辑和勾选更新原 `.md`；关闭便签仍保留文件。
- **工作时快速查看** — 固定置顶，或按 `Alt+Z` 将所有便签置顶 10 秒。

## 界面示例

| 直接勾选任务 | Markdown 笔记 | 关联今日笔记 |
| :---: | :---: | :---: |
| [<img src="docs/images/sticky-tasks-zh-CN.png" alt="直接勾选任务" width="280">](docs/images/sticky-tasks-zh-CN.png) | [<img src="docs/images/sticky-markdown-zh-CN.png" alt="Markdown 笔记" width="280">](docs/images/sticky-markdown-zh-CN.png) | [<img src="docs/images/sticky-daily-zh-CN.png" alt="关联今日笔记" width="280">](docs/images/sticky-daily-zh-CN.png) |

使用当前开发版 WPF 控件渲染虚构笔记，并非桌面截图。[五种语言的示例与生成步骤](docs/SCREENSHOTS.zh-CN.md)。

## 所需工具和设置

**Windows 10 / 11（x64）**。ZIP 包含 .NET，无需管理员权限。

| 使用目的 | 所需工具 | 初始设置 |
| --- | --- | --- |
| 仅使用 Markdown 便签 | 本应用 | 在设置中关闭 **使用 Obsidian CLI**，选择本地目录 |
| 编辑 Obsidian 笔记 | 正在运行的 Obsidian＋Obsidian CLI | 启用 Obsidian **设置 → 常规 → 命令行界面**，在本应用连接 Vault |
| 今日每日笔记 | 上述 CLI＋核心 **日记** 插件 | 在 Obsidian 配置目录和日期格式，创建今日笔记 |
| Tasks 查询与原生勾选行为 | 上述 CLI＋第三方 **Tasks** 插件 | 在目标 Vault 启用；当前开发版要求稳定版 **8.2.2 或更高版本** |
| Dataview 查询 | 上述 CLI＋**Dataview** | 在目标 Vault 启用；支持 LIST / TABLE / TASK |
| Google 日程与任务 | Google 账号＋自己的 OAuth 配置 | 启用 Calendar API / Tasks API，准备桌面 OAuth JSON 并[授权](docs/GOOGLE-CALENDAR.zh-CN.md#setup) |

**新配置默认启用 CLI。** 不使用 Obsidian 时请关闭。CLI 内置于 Obsidian，无需专用的 Sticky Notes Obsidian 插件。[官方 CLI 要求](https://obsidian.md/help/cli) · [兼容性与限制](docs/TASKS-CLI-PREVIEW.md#简体中文)。

## 开始使用

1. 从 [Releases](https://github.com/AvocadoWasabi/StickyNotes/releases/latest) 下载 `StickyNotes-win-x64.zip`，全部解压后运行 `Install.cmd`。
2. 在 Obsidian 启用 CLI、打开目标 Vault，然后启动 Markdown Sticky Notes。
3. 在应用设置中确认 `Obsidian.com` 的绝对路径，输入 Vault 名称／ID，点击 **通过 CLI 获取 Vault 保存目录**。选择便签目录，再 **保存并关闭**。
4. 从 `… → 创建与打开` 新建便签、打开 Markdown、关联笔记的一部分或显示每日笔记。

本地模式、设置位置、更新和卸载见[安装指南](docs/INSTALL.zh-CN.md)。此处设置和图片对应当前开发版；与已发布 ZIP 的差异（多 Vault、Tasks 8.2.2 支持等）见[未发布](CHANGELOG.zh-CN.md#未发布)。

## 功能列表

| 功能 | 内容 |
| --- | --- |
| Markdown 显示与编辑 | 标题、列表、复选框、表格、代码、链接；保存为普通 `.md` |
| 标题关联与每日笔记 | 显示／编辑部分内容，随日期切换，可保留昨日笔记 |
| Tasks / Dataview | 显示查询结果，勾选已验证源位置的任务，保留查询原文 |
| 任务输入 | 关键词／日期补全、Enter 续写列表、Tab 缩进、可配置勾选快捷键 |
| Google 集成 | `@calendar today` 显示日程和有日期的未完成 Google Tasks；确认后修改日程标题／说明 |
| 外观与窗口 | 颜色、大小、50–200% 缩放、置顶、临时置顶、任务栏显示 |
| 便签管理 | 选择／迁移保存目录，重新显示已关闭便签，浏览／预览，重命名和删除 |
| 数据保护 | 写入前备份、外部修改冲突检测 |
| 多 Vault（未发布） | 各 Vault 的每日笔记、自定义名称和默认 Vault |

## 基本操作

| 操作 | 方法 |
| --- | --- |
| 编辑 | 点击正文（可配置单击／双击），或 `Ctrl+E` |
| 保存并结束编辑 | `Ctrl+S` / `Esc`；显示补全候选时，第一次 Esc 关闭候选 |
| 勾选任务 | 在阅读模式点击复选框 |
| 置顶 | `○ / ●` 固定；`Alt+Z` 将全部便签置顶 10 秒 |
| 关闭／退出 | `… → 关闭此便签`／从通知区域退出应用 |

## 使用前须知

- UI 支持**日语、英语、简体中文**。README、安装和界面示例还提供**繁体中文、韩语**。详细参考资料通常为三种语言。
- CLI 集成需要 Obsidian 持续运行；内部 API 变化可能影响兼容性。
- 不自动创建每日笔记，首次添加前请准备今日笔记。
- 不完整复现 Obsidian 主题和插件视图。图片显示替代文字；不支持 DataviewJS、内联表达式、CALENDAR。
- Google Tasks 只读且仅能获取日期。不支持计划时刻、与 Markdown 任务自动同步或新建日程。
- 备份目录：`%LOCALAPPDATA%/StickyNotes/backups`。发生冲突时保留草稿，重新加载后重试。

## 详细资料与问题反馈

[保存与迁移](docs/STICKY-FOLDERS.md#简体中文) · [Tasks / CLI](docs/TASKS-CLI-PREVIEW.md#简体中文) · [任务输入](docs/TASK-EDITING.md#简体中文) · [Dataview](docs/DATAVIEW.md#简体中文) · [多 Vault](docs/MULTIPLE-VAULTS.md#简体中文) · [便签管理](docs/QUERY-EDITING-AND-BROWSER.md#简体中文) · [Google](docs/GOOGLE-CALENDAR.zh-CN.md) · [开发](docs/DEVELOPMENT.zh-CN.md) · [许可声明](THIRD-PARTY-NOTICES.txt)

[报告问题或建议](https://github.com/AvocadoWasabi/StickyNotes/issues)时，请注明应用／Windows／插件版本、CLI 或本地模式及重现步骤。不要上传个人笔记、OAuth JSON 或令牌。
