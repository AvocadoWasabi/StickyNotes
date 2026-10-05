[English / 日本語](SCREENSHOTS.md) | [简体中文](SCREENSHOTS.zh-CN.md)

# 界面示例

这些图片于 2026-10-05 使用 0.0.2 版 WPF 控件和虚构 Markdown 示例渲染更新，展示便签内容区域，并非桌面截图。未使用个人笔记、设置或凭据。英文图片使用英文笔记内容，界面控件仍为日语；未提供中文版界面截图。

| 内容 | 日语示例 | 英语示例 |
| --- | --- | --- |
| 任务清单 | [图片](images/sticky-tasks-ja.jpg) | [图片](images/sticky-tasks-en.jpg) |
| Markdown 笔记 | [图片](images/sticky-markdown-ja.jpg) | [图片](images/sticky-markdown-en.jpg) |
| 关联每日笔记 | [图片](images/sticky-daily-ja.jpg) | [图片](images/sticky-daily-en.jpg) |

每张为 460 × 580 px JPEG，README 缩略图链接到原图。

## 在应用中显示相同示例

在 Windows 中从仓库根目录执行。如果应用已运行，先保存修改并从通知区域的 `終了` 退出；应用只允许一个运行实例。

```powershell
./build.ps1 -Publish

# 日语示例
$demoData = ./scripts/Prepare-ScreenshotDemo.ps1 -Language ja
& ./artifacts/app/StickyNotes.exe --data-dir $demoData

# 切换语言前先退出应用
$demoData = ./scripts/Prepare-ScreenshotDemo.ps1 -Language en
& ./artifacts/app/StickyNotes.exe --data-dir $demoData
```

脚本每次新建 `artifacts/screenshot-demo-<GUID>`，放入示例笔记、当天的每日笔记和隔离设置，不修改个人设置。将便签置于前台后，可单独截取各窗口。

[日语](../examples/screenshots/Tasks.md)与[英语](../examples/screenshots/en/Tasks.md)示例所在目录还包含 `Project.md` 和 `Daily.md`。分发 ZIP 附带示例和图片，截图准备脚本在源码仓库中。
