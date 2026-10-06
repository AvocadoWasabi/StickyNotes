[English](GOOGLE-CALENDAR.en.md) | [日本語](GOOGLE-CALENDAR.md) | [简体中文](GOOGLE-CALENDAR.zh-CN.md)

# Google Calendar 使用与配置

[返回基本操作](../README.zh-CN.md)

[搜索与编辑](#usage) · [初次授权](#setup) · [删除已导入的 JSON](#json) · [故障排查](#troubleshooting)

<a id="usage"></a>

## 搜索与编辑

在笔记中单独写一行命令并保存：

```text
@calendar 2026-10-06T09:00
```

**如上例所示，无需关键词或时区偏移也能搜索。** 省略时区偏移时使用 Windows 本地时间。需要筛选时可追加关键词，例如 `@calendar 2026-10-06T09:00 会议`；也仍可明确指定偏移，例如 `2026-10-06T09:00+09:00`。最多获取 100 条日程，按开始时间排序。Google 的 `timeMin` 按结束时间过滤，所以指定时间已经开始、尚未结束的日程也可能出现。重复日程展开为各次实例。

编辑时，在独立段落开头（前面有正文时先留一个空行）输入 `@`，即可浮动显示 `@calendar` 候选及输入示例。按 Tab、Enter 或点击候选，可插入今天 `00:00` 的示例，无需时区或关键词。日期和时间会被选中，修改后保存即可。插入的日期固定，不会自动变为次日。Esc 关闭候选。补全操作本身不会保存笔记，电子邮件地址或代码块内不会显示候选。

日程约每 60 秒刷新，或通过 `… → 予定を今すぐ取得` 立即获取。每张便签只使用第一条命令。请在命令前后留空行。围栏代码块中的示例不会执行。

点击日程可编辑标题和说明。确认对话框显示修改及参与者通知方式，**点击 OK 后才发送到 Google**。不修改时间、参与者或重复规则。编辑重复日程仅影响本次获取的实例。如果日程在 Google 上已变化，ETag 检查会阻止覆盖并要求刷新。

<a id="setup"></a>

## 初次授权

打开设置窗口右侧 `Google Calendar` 窗格中的连接指南。左侧窗格包含便签和每日笔记设置，左右可独立滚动。以下适用于使用个人 Google 账号为自己配置，已于 2026-10-06 与官方文档核对。按钮打开系统浏览器，Google 注册和授权在浏览器中完成。界面名称随显示语言而异。

### 1-1：启用 API

打开 [Google Calendar API](https://console.cloud.google.com/apis/library/calendar-json.googleapis.com)，从顶部项目选择器选择现有项目，或通过“新建项目（New project）”输入名称（例如 `StickyNotes Personal`）并创建、选中。点击“启用（Enable）”；显示“管理（Manage）”则表示已启用。后续步骤始终选择**同一项目**。

### 1-2：初次注册

[品牌信息（Branding）](https://console.cloud.google.com/auth/branding)尚未配置时，点击“开始（Get started）”。输入应用名（例如 `StickyNotes Personal`）和可接收邮件的用户支持邮箱，点击“下一步”。个人账号选择“外部（External）”，点击“下一步”。输入联系邮箱，再点击“下一步”。阅读政策，同意时勾选并点击“继续”→“创建”。已配置时检查内容后继续。“内部（Internal）”适用于仅限 Google Cloud 组织成员使用的情况。

### 1-3：添加要登录的账号

在[受众（Audience）](https://console.cloud.google.com/auth/audience)中，若为“外部”且发布状态为“测试中（Testing）”，进入“测试用户（Test users）”→“添加用户（Add users）”，输入**将要访问日历的账号**邮箱并“保存（Save）”。该账号可能与 Cloud 管理账号不同。此流程无需发布应用。

### 1-4：保存权限范围

对于外部应用，进入[数据访问（Data Access）](https://console.cloud.google.com/auth/scopes)→“添加或移除范围（Add or Remove Scopes）”，选择 `https://www.googleapis.com/auth/calendar.events`。找不到时，在“手动添加范围（Manually add scopes）”中输入完整 URL 并添加。点击“更新（Update）”返回，再“保存（Save）”。这是日程查看和编辑权限，与 `calendar.events.readonly` 不同；应用中的 Calendar ID 不会将 OAuth 授权限制为仅该日历。

### 1-5：获取桌面客户端 JSON

进入[客户端（Clients）](https://console.cloud.google.com/auth/clients)→“创建客户端（Create client）”，应用类型选择“桌面应用（Desktop app）”，输入名称（例如 `StickyNotes Desktop`）并“创建（Create）”。在创建结果中点击“下载 JSON（Download JSON）”，**关闭窗口前保存**；之后可能无法再次获取密钥。无需配置 Web 应用的重定向 URI 或 JavaScript 来源。不使用 API 密钥或服务账号 JSON。

### 应用步骤2：选择并导入 JSON

点击 `認証JSONを選択して確認` 选择下载的文件，再点击旁边的 `アプリに取り込む`（导入应用）。应用验证格式后复制到 `%LOCALAPPDATA%\StickyNotes\credentials-google.json`，并立即保存使用路径。再次导入会替换已导入的副本。原文件保持不变，成功导入后可移动或删除。若跳过导入、继续使用原路径，请勿移动或删除该文件。无需重命名。自己的主日历使用 `primary`。

### 应用步骤3：登录

点击 `設定を保存してGoogleにログイン`，在三分钟内于浏览器中选择1-3添加的账号，检查应用名称并允许日历访问。应用自动接收结果。关闭浏览器标签页、返回设置，确认出现 `接続確認が完了しました`。应用保存令牌后检查指定日历的读取权限，不修改日程。

### 应用步骤4：仅重新检查连接

已登录时无需打开登录页面即可检查。步骤3和4会保存设置窗口中的所有输入，包括 Google 以外的设置。连接检查成功不保证拥有日程编辑权限。

<a id="cancel"></a>

## 取消与重试

等待登录或连接检查时可以取消；关闭设置也会取消操作。取消后请关闭浏览器中的授权标签页，已完成的授权会保留。授权超时后从步骤3重试。

<a id="json"></a>

## 删除已导入的 JSON

旁边的 `取り込み済みJSONを削除`（删除已导入 JSON）在确认后仅删除应用管理的副本；若保存的路径指向该副本，也会清空路径。不会删除原文件、外部路径的文件或授权令牌，也不会撤销 Google 端的授权。导入和删除立即生效，不保存其他设置字段的未保存输入。设置保存失败时恢复原副本。授权或连接检查期间无法操作这些按钮。保存位置是当前 Windows 用户的应用数据目录，而非可执行文件所在目录。

<a id="troubleshooting"></a>

## 故障排查

| 情况 | 检查或操作 |
| --- | --- |
| Google 登录页出现 `access_denied` | 检查1-3的测试用户和登录账号；组织政策阻止时请联系管理员 |
| 未验证应用警告 | 确认应用名称和账号属于自己创建的客户端；不确定时停止 |
| 登录成功后连接检查失败 | 检查 Calendar ID、同一项目的 API 启用情况和授权。令牌保留。步骤4重新检查，步骤3重新授权 |
| 外部／测试中应用数天后要求重新登录 | 此权限的刷新令牌在七天后过期；用步骤3重新登录 |

<a id="credentials"></a>

## 凭据保管

授权使用系统浏览器、回环重定向和 PKCE。令牌经 Windows DPAPI 按当前用户加密，保存在 `google-token.bin`。不要把 OAuth JSON 放入代码仓库或共享的笔记目录。

<details>
<summary>来源与技术细节</summary>

配置依据：[同意屏幕配置](https://developers.google.com/workspace/guides/configure-oauth-consent)、[受众和测试用户](https://support.google.com/cloud/answer/15549945)、[Calendar 权限范围](https://developers.google.com/workspace/calendar/api/auth)、[客户端创建和 JSON 保管](https://support.google.com/cloud/answer/15549257)、[刷新令牌期限](https://developers.google.com/identity/protocols/oauth2#expiration)。

真实账号的连接和更新验证需要自己的 OAuth 配置。自动测试使用模拟 Google 响应，验证回环回调、拒绝错误 state 和路径、PKCE、授权拒绝、取消、无效令牌响应、保存失败、只读连接检查、查询、更新和 ETag 冲突。

参考：[Google 桌面 OAuth](https://developers.google.com/identity/protocols/oauth2/native-app)、[Calendar 资源版本](https://developers.google.com/calendar/api/guides/version-resources)。

</details>
