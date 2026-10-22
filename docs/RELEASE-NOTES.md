# Markdown Sticky Notes v0.0.10

[日本語](#日本語) | [English](#english) | [简体中文](#简体中文)

## 日本語

Windows 10 / 11（x64）向け。v0.0.9以降の変更をまとめました。

- **閉じた付箋の一括復元**：通知領域・タスクバー・付箋メニューから、直前に閉じた一組を位置・サイズ・固定・倍率・見出しフィルター付きで復元。最新の一組は再起動後も保持します。
- **保存時のフリーズ修正**：「…」メニューを開いている間はフォーカス離脱時の自動保存・確認を保留。CLI編集保存は画面を止めずに待機し、失敗時は下書きを保持します。保存中の重複保存・終了・設定変更を抑止します。
- **ファイル名変更**：「… → ファイル名を変更…」またはタイトルのダブルクリックから変更。表示設定・開いている付箋・閉じた履歴の参照先を保持し、CLI方式ではObsidianのリンク更新設定に従います。同名・不正名・本文の競合を拒否し、失敗時は復旧記録を残します。
- **変更不可時の案内**：デイリー付箋・保存中・読み込み未完了などでタイトルをダブルクリックしたとき、変更できない理由を表示します。

### 更新・検証・制限

アプリを終了し、Assetsの **StickyNotes-win-x64.zip** を新しいフォルダへ全展開して **Install.cmd** を実行してください。ノート・設定・バックアップは保持されます。管理者権限や別途.NET導入は不要です。未署名のため、必要に応じて **StickyNotes-win-x64.zip.sha256** で照合できます。

自動テスト、模擬Obsidian API、配布ビルド、ZIP内容・SHA256、コード／セキュリティレビューで確認。実マウス操作・実環境Obsidianでのリンク更新・物理IME・別Windowsテーマは今回再検証していません。

復元は最新の一組のみで、2秒以内の連続クローズも同じ組に含みます。破棄した編集や削除済みファイルは戻りません。デイリー付箋は名前変更の対象外。大文字小文字だけの変更は一度別名を経由してください。ローカル方式では他ノートのリンクを更新しません。変更結果が不明な場合は両方のパスとアプリデータ内の `note-renames` 記録を確認してください。閉じる際の保存・メタデータ保存・新規作成のCLI処理は引き続き同期処理です。既存CLI制限と以前観測された一時的な保存競合は継続し、競合時は上書きを拒否します。

[利用手順と制限](https://github.com/AvocadoWasabi/StickyNotes/blob/v0.0.10/docs/USAGE.md) · [変更履歴](https://github.com/AvocadoWasabi/StickyNotes/blob/v0.0.10/CHANGELOG.ja.md)

## English

For Windows 10 / 11 (x64). Includes the changes since v0.0.9.

- **Reopen closed note groups:** Restore the most recently closed group from the tray, taskbar or note menu, including position, size, pinning, zoom and heading filters. The latest group survives app restarts.
- **Save freeze fix:** Defer focus-loss saves and prompts while the More menu is open. CLI editor saves wait without blocking the UI and retain drafts on failure. Prevent overlapping saves, closing the saving note and settings changes during a write.
- **File renaming:** Use More → Rename file or double-click the title. Preserve display settings and update open windows and closed history. CLI mode follows Obsidian's link-update settings. Reject duplicate/invalid names and content conflicts, and retain recovery records on failure.
- **Unavailable-action guidance:** Explain why title double-click cannot rename daily notes, notes being saved, notes that have not loaded, and other unavailable states.

### Updating, validation and limits

Exit the app, extract the complete **StickyNotes-win-x64.zip** from Assets into a new folder and run **Install.cmd**. Notes, settings and backups are retained. No administrator privileges or separate .NET installation are required. The app is unsigned; verify with **StickyNotes-win-x64.zip.sha256** when needed.

Validation covers automated tests, a synthetic Obsidian API, the distribution build, ZIP contents/SHA256, and code/security reviews. Real mouse gestures, link updates in live Obsidian, physical IME input and other Windows themes were not revalidated.

Only the latest group is restored; consecutive closes within two seconds belong to the same group. Discarded edits and deleted files cannot be recovered. Daily notes cannot be renamed. Case-only changes require an intermediate name. Local mode does not update links in other notes. If the outcome is unclear, check both paths and the `note-renames` records in the app-data directory. Save-on-close, metadata saves and new-note CLI creation remain synchronous. Existing CLI limits and previously observed transient save conflicts remain; conflicting writes are refused.

[User guide and limits](https://github.com/AvocadoWasabi/StickyNotes/blob/v0.0.10/docs/USAGE.en.md) · [Changelog](https://github.com/AvocadoWasabi/StickyNotes/blob/v0.0.10/CHANGELOG.md)

## 简体中文

适用于 Windows 10 / 11（x64）。汇总 v0.0.9 之后的更新。

- **成组恢复关闭的便签**：从通知区域、任务栏或便签菜单恢复最近关闭的一组便签，包括位置、大小、置顶、缩放及标题筛选。重启后仍保留最近一组。
- **修复保存卡顿**：打开更多菜单时暂缓失焦保存和确认。CLI编辑保存不阻塞界面，失败时保留草稿；写入期间阻止重复保存、关闭正在保存的便签及修改设置。
- **文件重命名**：通过更多菜单的重命名操作或双击标题修改文件名。保留显示设置并更新打开的便签及关闭历史中的引用。CLI模式遵循Obsidian的链接更新设置；拒绝重复／无效名称及内容冲突，失败时保留恢复记录。
- **不可用时说明原因**：双击每日便签、正在保存或尚未加载的便签标题等情况下，显示无法重命名的原因。

### 更新、验证与限制

退出应用，将 Assets 中的 **StickyNotes-win-x64.zip** 全部解压到新目录并运行 **Install.cmd**。保留笔记、设置和备份，无需管理员权限或另装.NET。应用未签名，可通过 **StickyNotes-win-x64.zip.sha256** 校验。

验证涵盖自动测试、模拟Obsidian API、分发构建、ZIP内容与SHA256、代码及安全审查。本次未重新验证真实鼠标操作、实际Obsidian链接更新、物理输入法及其他Windows主题。

仅恢复最近一组，两秒内连续关闭也归入同组；无法恢复已丢弃的编辑或已删除的文件。每日便签不能重命名。仅改变大小写时需先改为其他名称。本地模式不更新其他笔记中的链接。如结果不明确，请检查两个路径及应用数据目录中的 `note-renames` 记录。关闭时保存、元数据保存和新建笔记的CLI处理仍为同步操作。现有CLI限制及此前观察到的临时保存冲突仍存在，冲突写入会被拒绝。

[使用指南与限制](https://github.com/AvocadoWasabi/StickyNotes/blob/v0.0.10/docs/USAGE.zh-CN.md) · [更新日志](https://github.com/AvocadoWasabi/StickyNotes/blob/v0.0.10/CHANGELOG.zh-CN.md)
