[日本語](README.md) | [English](README.en.md) | [简体中文](README.zh-CN.md) | [繁體中文（台灣）](README.zh-TW.md) | [한국어](README.ko.md) | [Deutsch](README.de.md) | [Русский](README.ru.md) | [Español](README.es.md) | [Svenska](README.sv.md)

# Markdown Sticky Notes

[v0.0.11](https://github.com/AvocadoWasabi/StickyNotes/releases/tag/v0.0.11) · Windows 10 / 11 (x64)

用于在便签窗口中显示和编辑 Markdown 文件的 Windows 应用。支持本地文件、Obsidian 笔记以及每日笔记中的特定标题区域。编辑内容和复选框更改会保存到原 Markdown 文件。

支持通过 Obsidian CLI 显示 Tasks、Dataview 查询结果，并获取 Google Calendar 日程与 Google Tasks。

[下载](https://github.com/AvocadoWasabi/StickyNotes/releases/latest) · [安装设置](docs/INSTALL.zh-CN.md) · [使用指南](docs/USAGE.zh-CN.md) · [更新日志](CHANGELOG.zh-CN.md)

## 界面示例

| 复选框操作 | Markdown 显示 | 每日笔记标题区域显示 |
| :---: | :---: | :---: |
| [<img src="docs/images/sticky-tasks-zh-CN.png" alt="复选框操作" width="280">](docs/images/sticky-tasks-zh-CN.png) | [<img src="docs/images/sticky-markdown-zh-CN.png" alt="Markdown 显示" width="280">](docs/images/sticky-markdown-zh-CN.png) | [<img src="docs/images/sticky-daily-zh-CN.png" alt="每日笔记标题区域显示" width="280">](docs/images/sticky-daily-zh-CN.png) |

使用应用的 WPF 控件渲染示例笔记，并非桌面截图。请参阅[各语言界面示例与生成步骤](docs/SCREENSHOTS.zh-CN.md)。

## 运行环境与依赖

**Windows 10 / 11（x64）**。ZIP 包含 .NET，无需管理员权限。

| 使用目的 | 所需工具 | 初始设置 |
| --- | --- | --- |
| 仅使用 Markdown 便签 | 本应用 | 在设置中关闭 **使用 Obsidian CLI**，选择本地目录 |
| 编辑 Obsidian 笔记 | 正在运行的 Obsidian＋Obsidian CLI | 启用 Obsidian **设置 → 常规 → 命令行界面**，在本应用连接 Vault |
| 今日每日笔记 | 上述 CLI＋核心 **日记** 插件 | 在 Obsidian 配置目录和日期格式，创建今日笔记 |
| Tasks 查询与原生勾选行为 | 上述 CLI＋第三方 **Tasks** 插件 | 在目标 Vault 启用；v0.0.11要求稳定版 **8.2.2 或更高版本** |
| Dataview 查询 | 上述 CLI＋**Dataview** | 在目标 Vault 启用；支持 LIST / TABLE / TASK |
| Google 日程与任务 | Google 账号＋自己的 OAuth 配置 | 启用 Calendar API / Tasks API，准备桌面 OAuth JSON 并[授权](docs/GOOGLE-CALENDAR.zh-CN.md#setup) |

**新配置默认启用 CLI。** 不使用 Obsidian 时请关闭。CLI 内置于 Obsidian，无需专用的 Sticky Notes Obsidian 插件。[官方 CLI 要求](https://obsidian.md/help/cli) · [兼容性与限制](docs/TASKS-CLI-PREVIEW.md#简体中文)。

## 安装与初始设置

1. 从 [Releases](https://github.com/AvocadoWasabi/StickyNotes/releases/latest) 下载 `StickyNotes-win-x64.zip`，全部解压后运行 `Install.cmd`。
2. 在 Obsidian 启用 CLI、打开目标 Vault，然后启动 Markdown Sticky Notes。
3. 在应用设置中确认 `Obsidian.com` 的绝对路径，输入 Vault 名称／ID，点击 **通过 CLI 获取 Vault 保存目录**。选择便签目录，再 **保存并关闭**。
4. 从 `… → 创建与打开` 新建便签、打开 Markdown、关联笔记的一部分或显示每日笔记。

本地文件设置、更新和卸载请参阅[安装指南](docs/INSTALL.zh-CN.md)。此处的功能与设置适用于 v0.0.11。各版本变更见[更新日志](CHANGELOG.zh-CN.md#v0-0-11)。

## 功能列表

| 功能 | 内容 |
| --- | --- |
| Markdown 显示与编辑 | 标题、列表、复选框、表格、代码、链接；保存为普通 `.md` |
| 标题关联与每日笔记 | 显示和编辑笔记的一部分，随日期切换，今日笔记不存在时保留前一天的笔记 |
| Tasks / Dataview | 显示查询结果，勾选已验证源位置的任务，保留查询原文 |
| 任务输入 | 关键词／日期补全、Enter 续写列表、Tab 缩进、可配置勾选快捷键 |
| Google 集成 | `@calendar today` 显示日程和有日期的未完成 Google Tasks；确认后修改日程标题／说明 |
| 外观与窗口 | 颜色、大小、50–200% 缩放、置顶、临时置顶、任务栏显示 |
| 便签管理 | 选择／迁移保存目录，重新显示已关闭便签，浏览／预览，重命名和删除 |
| 备份与冲突检测 | 写入前备份、外部修改冲突检测 |
| 多 Vault | 各 Vault 的每日笔记、自定义名称和默认 Vault |

## 基本操作

| 操作 | 方法 |
| --- | --- |
| 编辑 | 点击正文（可配置单击／双击），或 `Ctrl+E` |
| 保存并结束编辑 | `Ctrl+S` / `Esc`；显示补全候选时，第一次 Esc 关闭候选 |
| 勾选任务 | 在阅读模式点击复选框 |
| 置顶 | `○ / ●` 固定；`Alt+Z` 将全部便签置顶 10 秒 |
| 关闭／退出 | `… → 关闭此便签`／从通知区域退出应用 |

## 支持范围与限制

- v0.0.11的 UI、README、安装指南和界面示例支持**日语、英语、简体中文、台湾华语（繁体中文）、韩语、德语、俄语、西班牙语、瑞典语**九种语言。详细资料通常仍为三种语言。语言更改在保存并重启后生效。
- CLI 集成需要 Obsidian 持续运行；内部 API 变化可能影响兼容性。
- 不自动创建每日笔记，首次添加前请准备今日笔记。
- 不完整复现 Obsidian 主题和插件视图。图片显示替代文字；不支持 DataviewJS、内联表达式、CALENDAR。
- Google Tasks 为只读，到期信息仅获取日期。不支持到期时刻、与 Markdown 任务自动同步或新建 Google Calendar 日程。
- 备份目录：`%LOCALAPPDATA%/StickyNotes/backups`。发生冲突时保留草稿，重新加载后重试。

## 相关资料与问题反馈

[保存与迁移](docs/STICKY-FOLDERS.md#简体中文) · [Tasks / CLI](docs/TASKS-CLI-PREVIEW.md#简体中文) · [任务输入](docs/TASK-EDITING.md#简体中文) · [Dataview](docs/DATAVIEW.md#简体中文) · [多 Vault](docs/MULTIPLE-VAULTS.md#简体中文) · [便签管理](docs/QUERY-EDITING-AND-BROWSER.md#简体中文) · [Google](docs/GOOGLE-CALENDAR.zh-CN.md) · [开发](docs/DEVELOPMENT.zh-CN.md) · [许可声明](THIRD-PARTY-NOTICES.txt)

[报告问题或建议](https://github.com/AvocadoWasabi/StickyNotes/issues)时，请注明应用／Windows／插件版本、CLI 或本地模式及重现步骤。不要上传个人笔记、OAuth JSON 或令牌。

## 许可证

本项目中由 AvocadoWasabi 持有著作权的代码和文档采用 [MIT 许可证](LICENSE)。第三方代码和库仍适用各自的许可证。

Markdig 采用 BSD 2-Clause。YamlDotNet、源自 Obsidian Tasks 的补全代码和测试资料，以及 .NET / Windows Desktop / System.Security.Cryptography.ProtectedData 采用 MIT。著作权声明和条款见[第三方许可声明](THIRD-PARTY-NOTICES.txt)。发布 ZIP 包含本项目的 LICENSE、第三方声明，以及 licenses/ 中的 .NET 许可证和其他第三方声明。
