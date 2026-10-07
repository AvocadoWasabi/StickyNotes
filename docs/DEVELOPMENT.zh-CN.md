[English](DEVELOPMENT.en.md) | [日本語](DEVELOPMENT.md) | [简体中文](DEVELOPMENT.zh-CN.md)

# 开发与发布

## 分支流程

1. 修改前创建专用分支，例如 `git switch -c feat/example`。
2. 实现、更新文档并验证；完成前对全部差异进行代码和安全审查。
3. **若有未解决的重大问题（Critical / High、P0 / P1、严重数据丢失或泄露等），停止合并和推送。** 无法完成必要审查或验证时，也不能报告完成。修复后重新验证和审查。
4. 将已验证修改本地提交，报告验证、两项审查及剩余限制。常规流程不创建 PR。
5. **合并到 main 和推送任何分支均需所有者明确指示。** 推送标签和发布 Release 也需要授权。

`AGENTS.md` 仅用于本地，不纳入 Git 或分发包。公开仓库为 [AvocadoWasabi/StickyNotes](https://github.com/AvocadoWasabi/StickyNotes)。

## 构建与验证

使用 Windows 和 .NET 8 SDK。存在本地 `.tools/dotnet` 时优先使用。

```powershell
./build.ps1
./build.ps1 -Publish
```

两种命令都会还原依赖、构建 Release、运行测试，并在 `artifacts/app` 中生成自包含应用、安装脚本、三种语言的文档和许可。普通 `./build.ps1` 完成后，即可运行 `artifacts/app/Install.cmd` 本地安装。请先从通知区域退出正在运行的 Sticky Notes。构建完成时会提示安装脚本路径，但不会自动执行安装。

`-Publish` 额外生成或更新 `artifacts/StickyNotes-win-x64.zip` 和 SHA256 校验文件。普通构建不会更新已有 ZIP；分发 ZIP 时请使用 `-Publish`。

普通本地构建包含默认禁用的 Obsidian Tasks CLI 集成，可在 **设置 → Obsidian Tasks — CLI Preview** 启用。使用普通配置及笔记目录，不导入独立试验版设置。需要独立配置时，可用`./build.ps1 -Publish -TasksPreview`输出到`artifacts/tasks-cli-preview`。请参阅 [CLI 设置及限制](TASKS-CLI-PREVIEW.md#简体中文)。

`src/StickyNotes.Core` 负责存储、标题部分编辑和 Calendar 集成；`src/StickyNotes` 为 WPF UI；`tests` 覆盖数据保护、渲染和 API。

Google Tasks 复用同一 OAuth 客户端和令牌，增加 `tasks.readonly` 范围。`GoogleTasks.cs` 仅向固定 HTTPS 端点发送 GET，处理分页并将计划日期作为日期标签（不从 UTC 转换为本地时间）。每次最多获取100项匹配任务、20次请求、30秒。测试无需真实凭据，覆盖分页、日期边界、筛选、取消、部分失败及日期切换。

### 使用独立数据验证

测试使用临时文件和模拟 API，不连接真实仓库或 Google。真实账号的连接和更新验证需要自行配置 OAuth。

以下命令使用独立数据目录启动。该模式在任务栏显示便签，但不注册或替换跳转列表。

```powershell
.\artifacts\app\StickyNotes.exe --data-dir C:\Temp\StickyNotes-test
```

## 文档语言

修改 README、更新日志、安装／开发指南、相关资料和发布说明时，同步英语、日语和简体中文（zh-CN）。功能、限制、示例、版本和链接须对应；允许在同一文件中分语言列出。提供语言导航并检查构建时是否附带。文档翻译不代表界面或示例数据也已翻译。未经指示，不替换已发布的标签或 ZIP。

## 发布流程（仅在收到指示时）

功能或修复完成后，更新[英语](../CHANGELOG.md)、[日语](../CHANGELOG.ja.md)、[简体中文](../CHANGELOG.zh-CN.md)更新日志的 Unreleased 部分。正式发布时一致记录实际版本、日期及 Release 链接，不把草稿或计划写成已发布。`docs/RELEASE-NOTES.md` 也应反映该版本的主要变化。

1. 统一 `src/StickyNotes/StickyNotes.csproj` 中的 `Version`、三种语言 README 下载链接、更新日志和发布说明。完成 `./build.ps1 -Publish` 和两项审查，本地提交后将指定分支合并到 main 并推送。
2. 确认 GitHub Actions 的 **Build and test** 成功。
3. 获得发布指示后，在 main 手动运行 **Package release**，指定新标签，如 `v1.0.0`。
4. 工作流验证并生成 ZIP 和 **Release 草稿**。发布前检查 ZIP 的 SHA256、程序版本、标签目标提交、附带文档，以及是否混入个人设置等文件。

仅推送不会创建或发布 Release。ZIP 作为 Release 附件分发，不进入 Git 历史。

自定义本地测试包保存在 `artifacts` 内，不提交或上传。正式 Release 使用工作流从 main 新生成的 ZIP 和校验文件。

## 排除的文件

`.gitignore` 排除 SDK、NuGet 缓存、构建产物、个人设置、OAuth JSON、令牌和本地数据。提交前检查 `git status --short` 及差异。

## 图标

原图为 `docs/images/StickyNotes.png`，ICO 位于 `src/StickyNotes/Assets`。运行 `./scripts/Convert-AppIcon.ps1` 可重新生成 Windows 多尺寸 ICO。生成记录见[图标说明](ICON.zh-CN.md)。

## 显示语言

UI 文本存放于 `src/StickyNotes.Core/Strings.resx`（日语）、`Strings.en.resx` 和 `Strings.zh-CN.resx`。保持键名和格式占位符一致，通过 `L10n` 读取。启动时初始化语言，保存设置后到下次启动才应用。不要翻译已保存笔记的数据或协议键名。测试检查资源完整性、语言设置持久化、菜单及数据保留。分发包必须包含 `en/StickyNotes.Core.resources.dll` 和 `zh-CN/StickyNotes.Core.resources.dll`。
