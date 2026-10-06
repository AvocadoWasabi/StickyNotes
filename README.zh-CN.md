[English](README.md) | [日本語](README.ja.md) | [简体中文](README.zh-CN.md)

# Markdown Sticky Notes

<img src="docs/images/StickyNotes.png" alt="Markdown Sticky Notes 图标" width="112">

Windows Markdown 桌面便签应用。直接编辑 Obsidian 笔记和每日笔记，并在旁边显示 Google Calendar 日程。也可不使用 Obsidian。

**支持 Windows 10 / 11（x64）。** 界面和文档支持英语、日语和简体中文。 [显示语言](docs/USAGE.zh-CN.md#language)。Google 集成为可选功能，需要自行配置 OAuth。

[下载](#下载与安装) · [基本操作](#基本操作) · [Google Calendar](#google-calendar) · [补充资料](#补充资料)

<details>
<summary>界面示例（点击展开）</summary>

以下为使用 0.0.2 版 WPF 控件和英文示例笔记渲染的便签内容。这些旧版图片中的控件为日语。点击图片可查看原图。

| 随手查看清单 | Markdown 笔记 | 关联每日笔记 |
| :---: | :---: | :---: |
| [<img src="docs/images/sticky-tasks-en.jpg" alt="黄色 Markdown 桌面便签中的英文任务清单" width="300">](docs/images/sticky-tasks-en.jpg) | [<img src="docs/images/sticky-markdown-en.jpg" alt="蓝色便签中的 Markdown 标题、列表和表格" width="300">](docs/images/sticky-markdown-en.jpg) | [<img src="docs/images/sticky-daily-en.jpg" alt="显示每日笔记 Tasks 部分的绿色便签" width="300">](docs/images/sticky-daily-en.jpg) |

图片使用虚构的示例内容。参见[示例文件与显示步骤](docs/SCREENSHOTS.zh-CN.md)。

</details>

## 下载与安装

1. 从 [v0.0.5 发布页](https://github.com/AvocadoWasabi/StickyNotes/releases/tag/v0.0.5) 下载 `StickyNotes-win-x64.zip` 并解压。
2. 运行 `Install.cmd`，从开始菜单启动 Markdown Sticky Notes。
3. 在通知区域图标的 `设置` 中选择便签文件夹。

无需管理员权限或另外安装 .NET。应用未签名，Windows 可能要求确认。更新、卸载和免安装使用见[安装指南](docs/INSTALL.zh-CN.md)。

<a id="使用应用"></a>

## 基本操作

| 操作 | 方法 |
| --- | --- |
| 新建便签 | `＋`，或通知区域菜单的 `新便签` |
| 编辑与保存 | 点击正文／`Ctrl+E` → 编辑 → `Ctrl+S` |
| 完成任务 | 点击复选框，保存到原始 Markdown |
| 移动与调整大小 | 拖动顶部／边缘或右下角 |
| 置顶 | `○ / ●`；`…` 菜单可将所有便签临时置顶 10 秒 |
| 显示缩放 | `… → 显示缩放`（50～200%） |
| 关闭与退出 | `×` 仅关闭便签；通知区域的 `退出` 退出应用 |

编辑器失去焦点且内容有变化时，会询问是否保存。可在设置中启用免确认保存。关闭便签不会删除 Markdown 文件。

任务栏图标、按钮显示和快捷键的详情见[操作与设置补充](docs/USAGE.zh-CN.md#editing)。

## 文件存储与 Obsidian

默认保存到 `Documents/StickyNotesData`。在设置中选择仓库内的文件夹，即可与 Obsidian 编辑同一 `.md` 文件，无需 CLI 或插件。

更改保存位置时也可迁移已有 Markdown。移动前请查看[迁移范围与限制](docs/USAGE.zh-CN.md#storage)。

## 显示和编辑每日笔记的指定部分

1. 在设置中选择每日笔记文件夹及带日期标签的正则表达式，确认匹配文件名并保存。
2. 从 `… → 显示每日笔记…` 选择标题，点击 `显示`。留空则显示整个正文。
3. 在便签中编辑正文或勾选任务，修改会保存到原笔记。

首次添加需要今日文件。之后若今日笔记尚未创建，可按设置显示等待提示或保留昨天。详见[文件名示例与切换选项](docs/USAGE.zh-CN.md#daily)。

固定笔记使用 `… → 显示笔记的一部分…`。详见[标题选择](docs/USAGE.zh-CN.md#headings)。

## Google Calendar

<a id="初次授权"></a>

先在设置右侧窗格按照[连接指南](docs/GOOGLE-CALENDAR.zh-CN.md#setup)完成授权，再将以下命令作为独立段落保存：

```text
@calendar 2026-10-06T09:00
```

也以只读方式显示 Google Tasks。`@calendar today` 获取当天有日期的未完成任务。现有用户需启用 Tasks API 并重新授权 Tasks 读取权限，详见[Tasks 设置指南](docs/GOOGLE-CALENDAR.zh-CN.md#tasks)。

`@calendar today` 自动跟随电脑当天的日期（本地时间0点至次日0点）。可用 `@calendar today 会议` 按关键词筛选。查看便签时约每60秒检查日期并获取日程，从睡眠恢复后也会跟随当前日期；编辑期间暂停自动刷新。

**无需关键词或时区偏移也能搜索。** 省略时区时使用 Windows 本地时间。如需筛选，在末尾追加 `会议` 等关键词。

编辑时，在独立段落输入 `@` 可显示补全候选与示例。Tab、Enter 或点击候选会插入今天的 `00:00`，并选中日期时间供修改。插入的日期保持固定。

点击日程可编辑标题和说明，确认后发送到 Google。不支持创建日程或自动同步 Markdown 任务。详见[搜索规则、JSON 管理与故障排查](docs/GOOGLE-CALENDAR.zh-CN.md)。

## Markdown 支持与数据保护

支持标题、列表、任务、表格、代码和链接。图片显示为替代文本，部分 Obsidian 专用语法尚不支持。

外部修改冲突时阻止覆盖，写入前备份保存在 `%LOCALAPPDATA%/StickyNotes/backups`。详见[支持范围与恢复说明](docs/USAGE.zh-CN.md#data)。

## 补充资料

| 内容 | 资料 |
| --- | --- |
| 安装、更新与卸载 | [安装指南](docs/INSTALL.zh-CN.md) |
| 操作、保存位置与每日笔记 | [操作与设置补充](docs/USAGE.zh-CN.md) |
| Google 授权与日程搜索 | [Google Calendar 指南](docs/GOOGLE-CALENDAR.zh-CN.md) |
| 各版本变化 | [更新日志](CHANGELOG.zh-CN.md) |
| 构建、验证与发布 | [开发指南](docs/DEVELOPMENT.zh-CN.md) |
| 示例与许可证 | [每日笔记](examples/Daily.md) · [Obsidian Bases](examples/StickyNotes.base) · [许可证](THIRD-PARTY-NOTICES.txt) |
