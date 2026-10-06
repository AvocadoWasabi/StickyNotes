[English](INSTALL.en.md) | [日本語](INSTALL.md) | [简体中文](INSTALL.zh-CN.md)

# 安装、更新与卸载

## 环境要求

Windows 10 / 11 64 位（x64）。无需单独安装 .NET、管理员权限或 Obsidian 插件。

## 安装

1. 从 [Releases](https://github.com/AvocadoWasabi/StickyNotes/releases) 下载 `StickyNotes-win-x64.zip`。`Source code` 供开发者使用。
2. 右键点击 ZIP，选择“全部解压缩”。
3. 双击解压目录中的 `Install.cmd`。
4. 从开始菜单启动 Markdown Sticky Notes。

安装目录为 `%LOCALAPPDATA%\Programs\MarkdownStickyNotes`，不会设置开机自启。应用未签名，Windows 可能显示提示；请先确认来源和文件。

免安装使用时，运行解压目录中的 `StickyNotes.exe`。请保留整个文件夹，程序需要其他附带文件。

## 初次设置

源码构建可在设置中选择显示语言，保存后重启。[语言设置详情](USAGE.zh-CN.md#language)。已发布的 v0.0.4 仍为日语界面。

在通知区域图标的 `設定`（设置）中选择便签文件夹。与 Obsidian 共用时选择仓库内目录。每日笔记文件夹单独设置。

- [基本操作](../README.zh-CN.md#基本操作)
- [文件迁移与每日笔记设置](USAGE.zh-CN.md#storage)
- [Google Calendar 初次授权（可选）](GOOGLE-CALENDAR.zh-CN.md#setup)

不使用 Google 功能时无需授权。

## 更新

从通知区域的 `終了` 退出应用，将新 ZIP 解压到另一目录并运行 `Install.cmd`，覆盖原应用文件。笔记、设置和备份保存在安装目录以外。

从 0.0.1 更新后，请确认两个文件夹设置和今日匹配文件名。旧版标准日期格式会自动转为带日期标签的正则表达式，自定义格式需手动修正。免确认自动保存默认关闭。详见[更新日志](../CHANGELOG.zh-CN.md)。

## 卸载

1. 从通知区域的 `終了` 退出应用。
2. 删除 `%LOCALAPPDATA%\Programs\MarkdownStickyNotes`。
3. 右键点击开始菜单中的 Markdown Sticky Notes，打开文件位置并删除快捷方式。

设置、凭据和备份保留在 `%LOCALAPPDATA%\StickyNotes`；Markdown 保留在所选目录，默认 `Documents\StickyNotesData`。如需另外删除，请先确认要保留的数据。
