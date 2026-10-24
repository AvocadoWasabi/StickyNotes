[日本語](INSTALL.md) | [English](INSTALL.en.md) | [简体中文](INSTALL.zh-CN.md) | [繁體中文（台灣）](INSTALL.zh-TW.md) | [한국어](INSTALL.ko.md) | [Deutsch](INSTALL.de.md) | [Русский](INSTALL.ru.md) | [Español](INSTALL.es.md) | [Svenska](INSTALL.sv.md)

# 安装与初始设置

[返回 README](../README.zh-CN.md) · [详细操作](USAGE.zh-CN.md)

## 1. 安装

适用于 Windows 10 / 11（x64），无需管理员权限或单独安装 .NET。

1. 从 [Releases](https://github.com/AvocadoWasabi/StickyNotes/releases/latest) 下载 **StickyNotes-win-x64.zip**；`Source code` 用于开发。
2. 全部解压，运行 `Install.cmd`。
3. 从开始菜单启动 **Markdown Sticky Notes**。

免安装使用时，在解压目录运行 `StickyNotes.exe`，请保留整个目录。应用未签名，Windows 可能提示确认。不会注册自动启动。

本文档适用于v0.0.11。多 Vault、Tasks 8.2.2支持及九种 UI 语言见[更新日志](../CHANGELOG.zh-CN.md#v0-0-11)。

## 2. 保存位置与连接方式

### 使用 Obsidian CLI（默认）

1. 启用 Obsidian **设置 → 常规 → 命令行界面**，按提示注册。如无此项，请检查[官方 CLI 指南](https://obsidian.md/help/cli)中的安装程序更新要求。
2. 打开目标 Vault，保持 Obsidian 运行。
3. 在便签 **设置 → 常规** 确认 `Obsidian.com` 的绝对路径，通常是 `C:\Program Files\Obsidian\Obsidian.com`。如安装在其他位置请调整。
4. 在 **Vault 与每日笔记** 为选中的条目启用 CLI，输入 Obsidian Vault 名称／ID，点击 **通过 CLI 获取 Vault 保存目录**。留空使用 Obsidian 默认选择；仅输入不会获取。
5. 确认返回的 Vault 和每日设置，选择便签保存目录，最后 **保存并关闭**。

更改目录时，“是”同时迁移已有应用便签，“否”只更改今后保存位置，“取消”中止。[迁移范围与恢复](STICKY-FOLDERS.md#简体中文)。CLI 失败不会自动改为本地读取。

### 使用本地文件

关闭 **使用 Obsidian CLI**，选择本地基准目录和便签保存目录，再保存。初始位置为 `Documents/StickyNotesData`。不能使用 Tasks／Dataview 查询。本地每日笔记可通过目录和日期捕获正则表达式设置。[每日设置示例](USAGE.zh-CN.md#daily)。

## 3. 可选集成功能

| 集成 | Obsidian／Google 侧 | 便签侧 |
| --- | --- | --- |
| 每日笔记 | 启用核心日记，设置目录／日期格式，创建今日笔记 | 从 `… → 创建与打开` 显示每日笔记并选择标题；空白表示整篇 |
| Tasks | 在目标 Vault 启用 Tasks；v0.0.11要求稳定版8.2.2以上 | 打开含 `tasks` 代码块的笔记。[步骤与限制](TASKS-CLI-PREVIEW.md#简体中文) |
| Dataview | 在目标 Vault 启用 Dataview | 显示 `dataview` 的 LIST / TABLE / TASK。[步骤与限制](DATAVIEW.md#简体中文) |
| Google | 在自己的 Cloud 项目配置 Calendar API、Tasks API 和桌面 OAuth | 导入 JSON 并授权，在独立段落输入 `@calendar today`。[连接指南](GOOGLE-CALENDAR.zh-CN.md#setup) |

普通 Markdown 复选框不需要 Tasks。任务补全默认为自动，未启用 Tasks 时须手动开启才工作。不使用 Google 集成就不需要 Google 授权。

## 4. 创建和编辑便签

从 `… → 创建与打开` 新建或打开 Markdown。点击正文／`Ctrl+E` 编辑，`Ctrl+S`／`Esc` 保存。首次关联每日笔记需要今日文件，不会自动创建。在设置中选择显示语言，保存并重启后生效。v0.0.11 UI 支持 README 列出的九种语言。

## 常见问题

| 现象 | 检查内容 |
| --- | --- |
| CLI 连接失败 | Obsidian 是否运行、CLI 是否启用、`Obsidian.com` 绝对路径和 Vault 名称／ID是否正确；再次点击获取 |
| 今日笔记不显示 | 核心日记设置和今日文件；不会推测其他日记插件的设置 |
| Tasks 提示更新 | 在目标 Vault 的第三方插件更新中升级到8.2.2以上，然后刷新便签的 Tasks / Dataview |
| 保存冲突 | 保留草稿并重新加载，应用正在阻止旧内容覆盖 |
| Google 授权失败 | 按[分阶段指南](GOOGLE-CALENDAR.zh-CN.md)检查 API、OAuth、测试用户和权限 |

## 更新、卸载和数据

**更新：** 保存修改并从通知区域退出应用，将新 ZIP 解压到另一目录后运行 `Install.cmd`。笔记、设置和备份与应用分开保存。

**从v0.0.6及以前升级：** 无 CLI 选择的旧配置也会开启 CLI。继续本地模式时请关闭并保存；不会自动迁移文件。v0.0.1 的自定义每日格式可能需要手动修正。

| 数据 | 位置 |
| --- | --- |
| 已安装应用 | `%LOCALAPPDATA%/Programs/MarkdownStickyNotes` |
| 设置和凭据 | `%LOCALAPPDATA%/StickyNotes` |
| 写入前备份 | `%LOCALAPPDATA%/StickyNotes/backups` |
| 笔记 | 所选 Vault／目录 |

**卸载：** 退出应用，删除安装目录和开始菜单快捷方式。笔记、设置和备份仍保留，请确认需要的数据后另行删除。
