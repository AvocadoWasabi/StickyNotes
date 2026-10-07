[English](#english) | [日本語](#日本語) | [简体中文](#简体中文)

# Task editing / タスク入力 / 任务输入

## English

**Settings → Task input completion → Automatic / ON / OFF** is unchanged. Automatic is the default for new and existing settings: the selected Obsidian vault must have Tasks enabled. Missing/disabled/unreachable Tasks and local mode keep suggestions off until manual ON. OFF always hides them. Detection is asynchronous on editor focus; obsolete responses cannot enable a newer session. Candidates are generated locally, without per-keystroke CLI calls. Checkbox operations and Enter checklist continuation remain separate from this setting.

The standard emoji-format menu now follows the analyzed Tasks source, including these behaviors:

- `- [ ] Buy milk ` opens a menu with **⏎** selected, then due/start/scheduled dates, priorities, recurrence, created date and on-completion action. An empty task and unmatched words also offer the generic menu. Standard completed `[x]` tasks can show suggestions too.
- Enter on **⏎** creates the next unchecked item, preserving indentation/bullet/newlines; an empty item ends the list. ↓ then Enter inserts the selected field. Mouse selection also works. Esc dismisses the menu, then the next Esc saves and exits.
- Matching is case-insensitive and uses substrings: `du` finds both due and scheduled dates. Selecting a match replaces the whole word under the caret, including the portion after it. Existing field/priority markers are not duplicated.
- After `📅`, `⏳` or `🛫`, even without a space, the first date candidate is selected: Enter inserts today. Date values precede general fields. Recurrence offers `every`, day/week/month/year and weekday variants. A complete supported recurrence shows `✅`; its trailing space returns to the general menu. `🏁` offers `delete` and `keep`.
- Tab indents the task line instead of accepting a suggestion; Shift+Tab removes one tab or up to four spaces. IME-owned keys and selected text remain untouched; insertion/indentation is one undo step. Code blocks and inline code do not receive suggestions.

This aligns the **default menu and ordinary keyboard workflow**, not the complete Obsidian editor. The local defaults are minimum match 0 / maximum 20; plugin preferences, global filters, custom statuses, dependency/ID edits and Dataview field syntax are not imported. Dates support today/tomorrow/yesterday, weekdays, next week/month/year, numeric day/week/month/year intervals, English month-name dates and the trailing-space abbreviations `td`, `tm`, `yd`, `nw`, `we`, `weekend`. Full chrono-node natural-language parsing and full recurrence validation are not reproduced. Use Obsidian for unsupported input.

Ordinary CLI checkboxes still use Tasks' public toggle API (7.2+) for completion dates/status/recurrence, then the existing backup and whole-note conflict-checked save, including linked sections. Without Tasks, ordinary checkboxes use simple Markdown toggles; an installed but incompatible/failing plugin rejects the change. Local mode uses simple toggles. Query-result checkboxes retain their [provider-specific behavior](QUERY-EDITING-AND-BROWSER.md#english).

Analysis: cloned upstream revision [`a01526153c71ce0faf72ad5dd42675f722502c50`](https://github.com/obsidian-tasks-group/obsidian-tasks/tree/a01526153c71ce0faf72ad5dd42675f722502c50) (manifest 8.4.0); inspected [`Suggestor.ts`](https://github.com/obsidian-tasks-group/obsidian-tasks/blob/a01526153c71ce0faf72ad5dd42675f722502c50/src/Suggestor/Suggestor.ts), [`EditorSuggestorPopup.ts`](https://github.com/obsidian-tasks-group/obsidian-tasks/blob/a01526153c71ce0faf72ad5dd42675f722502c50/src/Suggestor/EditorSuggestorPopup.ts), date helpers and approved tests. The default-menu fixture is compared directly after excluding the two dependency fields (the upstream external-editor limitation). MIT attribution is in `THIRD-PARTY-NOTICES.txt`; the clone stays outside distribution.

All rules, local dates, popup/IME/keys, activation policy and checkbox adapter remain under `src/StickyNotes/TaskEditing/`. Automated checks cover source fixtures, space/Enter/Tab/mouse, replacement spans, IME, undo, activation and save conflicts. Live Obsidian and physical IME testing remain unverified. `./build.ps1 -Publish` bundles this guide and notices; install via `artifacts/app/Install.cmd`.

## 日本語

**設定 → タスク入力補完 → 自動／ON／OFF**は維持します。新規・既存設定の既定は自動で、対象Obsidian VaultのTasksが有効な場合のみ補完します。未導入・無効・接続確認失敗・ローカル方式では手動ONまで停止し、OFFでは常に非表示です。フォーカス時に非同期確認し、古い応答は破棄します。候補はローカル生成し、入力ごとのCLI通信はありません。チェック操作・Enterでのリスト継続はこの設定と独立しています。

解析したTasks本家のソースに合わせ、絵文字形式の標準操作を次のように修正しました。

- `- [ ] 牛乳を買う `のように末尾へスペースを入力すると、先頭の **⏎** を選択し、期限・開始日・予定日・優先度・繰り返し・作成日・完了時動作の候補を表示します。空のタスクやキーワード不一致でも一般候補を出し、通常の完了済み`[x]`でも補完できます。
- **⏎**でEnterを押すと、字下げ・箇条書き記号・改行を引き継いで次の未チェック項目を作ります。空項目ではリスト終了です。↓で候補を選んでEnter、または候補クリックで挿入します。Escは候補を閉じ、次のEscで保存して編集終了します。
- 大文字小文字を区別しない部分一致で絞り込みます。`du`はdueとscheduledの両方に一致します。確定時はカーソル位置の単語全体（カーソルより後ろも含む）を置換し、既存の日付・優先度記号は重複追加しません。
- `📅`・`⏳`・`🛫`の直後はスペースがなくても先頭の日付候補を選択し、Enterで今日の日付を挿入します。日付候補は一般候補より先です。繰り返しは`every`、日／週／月／年、曜日付き候補を表示します。対応する完全なルールには`✅`を付け、末尾スペースで一般候補へ戻します。`🏁`では`delete`／`keep`を選べます。
- Tabは候補確定ではなくタスク行の字下げです。Shift+Tabでタブ1個または空白最大4個を戻します。IME所有のキー・選択範囲は維持し、挿入・字下げは1回のUndoで戻せます。コードブロック・インラインコードでは補完しません。

一致させた範囲は**既定の候補メニューと通常のキー操作**です。Obsidianエディター全体ではありません。ローカル既定は最小一致0文字・最大20件で、プラグイン設定・グローバルフィルター・カスタム状態・依存関係／ID編集・Dataviewフィールド形式は取り込みません。日付はtoday／tomorrow／yesterday、曜日、next week／month／year、数値指定のday／week／month／year、英語の月名日付、末尾スペース付き略語`td`・`tm`・`yd`・`nw`・`we`・`weekend`に対応します。chrono-nodeの自然言語日付解釈全体や繰り返し規則の全検証は再現していません。未対応の入力はObsidianで行ってください。

通常のCLIチェックボックスは従来どおりTasks公開切替API（7.2以降）の完了日・状態・繰り返し処理を使い、見出し連携も含めバックアップ・ノート全体の競合検出付き保存を通します。Tasks未導入なら単純なMarkdown切替を維持し、導入済みプラグインのAPI未対応・失敗時は変更を拒否します。ローカル方式は単純な切替です。検索結果のチェックは[プロバイダー別の動作](QUERY-EDITING-AND-BROWSER.md#日本語)を維持します。

解析元：本家をcloneした[`a01526153c71ce0faf72ad5dd42675f722502c50`](https://github.com/obsidian-tasks-group/obsidian-tasks/tree/a01526153c71ce0faf72ad5dd42675f722502c50)（manifest 8.4.0）。[`Suggestor.ts`](https://github.com/obsidian-tasks-group/obsidian-tasks/blob/a01526153c71ce0faf72ad5dd42675f722502c50/src/Suggestor/Suggestor.ts)、[`EditorSuggestorPopup.ts`](https://github.com/obsidian-tasks-group/obsidian-tasks/blob/a01526153c71ce0faf72ad5dd42675f722502c50/src/Suggestor/EditorSuggestorPopup.ts)、日付補助処理・承認済みテストを確認しました。本家の一般候補テストデータから依存関係2項目（本家の外部エディター制限）を除いて直接照合します。MIT表記は`THIRD-PARTY-NOTICES.txt`へ追加し、cloneは配布対象外です。

入力規則・日付・候補UI／IME／キー・有効化判定・チェック連携は`src/StickyNotes/TaskEditing/`に集約しています。自動検証では本家資料との照合、Space／Enter／Tab／クリック、置換範囲、IME、Undo、有効化条件、保存競合を確認します。起動中のObsidianと物理IMEでの実機検証は未実施です。`./build.ps1 -Publish`で本資料・ライセンスを同梱し、`artifacts/app/Install.cmd`で導入できます。

## 简体中文

保留 **设置 → 任务输入补全 → 自动／ON／OFF**。新旧配置均默认自动，仅在目标Obsidian仓库已启用Tasks时显示候选。未安装、禁用、无法确认及本地模式下，手动ON前保持关闭；OFF始终关闭。获得焦点时异步检查，丢弃过期结果。候选在本地生成，不逐键调用CLI。复选框操作及Enter继续列表与此设置独立。

根据克隆并分析的Tasks源码，表情格式的标准行为调整如下：

- `- [ ] 买牛奶 `末尾输入空格后显示候选，默认选中 **⏎**，随后为截止／开始／计划日期、优先级、重复、创建日期及完成后动作。空任务或关键字不匹配也显示通用菜单，普通已完成`[x]`任务也可补全。
- **⏎**上按Enter创建下一未勾选项，保留缩进、列表符号及换行；空项结束列表。↓选择后Enter或点击插入。Esc先关闭候选，下次Esc保存并退出。
- 使用不区分大小写的子串匹配：`du`同时匹配due和scheduled。选择候选替换光标所在整个单词，包括光标后部分；不重复已有日期／优先级标记。
- 在`📅`、`⏳`、`🛫`后，即使无空格也默认选中首个日期，Enter插入今天。日期候选在通用字段前。重复规则提供`every`、日／周／月／年及星期选项；支持的完整规则显示`✅`，尾随空格返回通用菜单。`🏁`提供`delete`／`keep`。
- Tab缩进任务行，不接受候选；Shift+Tab移除一个Tab或最多四个空格。保留输入法按键和选择范围；插入／缩进可单步Undo。代码块和行内代码不补全。

对齐范围是**默认候选菜单和常用按键操作**，不是整个Obsidian编辑器。本地默认最小匹配0字符／最多20项，不导入插件配置、全局过滤器、自定义状态、依赖／ID编辑及Dataview字段格式。日期支持today／tomorrow／yesterday、星期、next week／month／year、数字day／week／month／year间隔、英文月份日期，以及带尾随空格的缩写`td`、`tm`、`yd`、`nw`、`we`、`weekend`。不完整复现chrono-node自然语言日期及全部重复规则验证；不支持的输入请在Obsidian中完成。

普通CLI复选框仍使用Tasks公开切换API（7.2+）处理完成日期、状态及重复任务，并通过备份和整篇笔记冲突检查保存，支持标题片段。未安装Tasks时保持简单Markdown切换；已安装但不兼容／失败则拒绝修改。本地模式使用简单切换。查询结果保留[各提供方行为](QUERY-EDITING-AND-BROWSER.md#简体中文)。

分析来源：克隆的上游[`a01526153c71ce0faf72ad5dd42675f722502c50`](https://github.com/obsidian-tasks-group/obsidian-tasks/tree/a01526153c71ce0faf72ad5dd42675f722502c50)（manifest 8.4.0）。检查了[`Suggestor.ts`](https://github.com/obsidian-tasks-group/obsidian-tasks/blob/a01526153c71ce0faf72ad5dd42675f722502c50/src/Suggestor/Suggestor.ts)、[`EditorSuggestorPopup.ts`](https://github.com/obsidian-tasks-group/obsidian-tasks/blob/a01526153c71ce0faf72ad5dd42675f722502c50/src/Suggestor/EditorSuggestorPopup.ts)、日期辅助逻辑及批准测试。直接比较上游通用菜单测试资料，排除两个依赖字段（上游外部编辑器限制）。MIT声明加入`THIRD-PARTY-NOTICES.txt`，克隆不进入分发包。

规则、日期、候选界面／输入法／按键、启用策略及复选框适配均集中于`src/StickyNotes/TaskEditing/`。自动验证覆盖上游资料、Space／Enter／Tab／点击、替换范围、输入法、Undo、启用条件和保存冲突。尚未实测运行中的Obsidian及物理输入法。`./build.ps1 -Publish`打包本指南和许可证，使用`artifacts/app/Install.cmd`安装。
