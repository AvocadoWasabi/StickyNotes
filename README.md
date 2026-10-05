# Markdown Sticky Notes

Windows / C# / WPFで作成した、ObsidianのMarkdownと連携する付箋アプリです。

## 起動

`artifacts/app/StickyNotes.exe` を実行してください。配布時は `app` フォルダ全体をコピーします。Windows x64向けの自己完結型ビルドなので、配布先の.NETインストールは不要です。

付箋の上部をドラッグして移動、端・右下をドラッグしてサイズ変更できます。`○ / ●` は最前面表示の切替です。タイトルバーのない付箋型ウィンドウで、タスクバーには表示せず、通知領域に常駐します。

- `＋`: 新しい付箋
- `編集` / `Ctrl+E`: Markdownソースの編集
- `保存` / `Ctrl+S`: ファイルに保存して表示モードへ
- `再読込`: 元ファイルを読み直す。未保存の編集がある場合は確認
- `…`: プロパティ・予定取得・既存ファイルを開く・見出し連動・設定・終了
- `×`: その付箋だけを閉じる。mdファイルは削除しません
- アプリ全体の終了: `… → アプリを終了`、または通知領域アイコンの `終了`

開いている付箋の配置・サイズ・最前面設定は移動時と終了時に記録し、次回起動時に復元します。複数モニター、負の画面座標、PerMonitorV2 DPIに対応し、接続を外したモニター上の付箋は利用可能な画面内に戻します。未保存の編集がある場合は終了時に確認します。

## 保存先とObsidian

初期保存先は `Documents/StickyNotesData` です。`… → 設定` でObsidian Vault内のフォルダ（例: `Vault/Sticky Notes`）を選んでください。変更後に作成する付箋から適用されます。既存ファイルの移動はしません。

付箋はUTF-8の通常の `.md` です。例えば次のプロパティを持ちます。

```yaml
---
type: sticky
id: 付箋ごとのID
title: 買い物
tags:
  - sticky
  - personal/shopping
status: active
color: yellow
created: 2026-10-05T09:00:00+09:00
updated: 2026-10-05T09:00:00+09:00
---
```

タグ・タイトル・状態・色は `… → タグ・タイトル・状態・色` で変更できます。色は `yellow / green / blue / pink / gray`。簡単な整理用に `status` を追加しており、`active / done / archived` など自由な値が使えます。状態は分類用で、自動削除・非表示はしません。

`examples/StickyNotes.base` をVaultにコピーすると `type: sticky` のノート一覧を作成できます。通常ノートだけのBaseには `type != "sticky"` を条件に追加するか、フォルダ除外 `!file.inFolder("Sticky Notes")` を使えます。

アプリ専用の配置情報や認証データはVaultに混ぜず、`%LOCALAPPDATA%/StickyNotes` に保存します。外部で変更されたファイルは約2秒間隔で読み直します。付箋側で編集中の場合は自動で置き換えず、競合を表示します。

参考: [Obsidian Properties](https://obsidian.md/help/properties)、[Bases syntax](https://obsidian.md/help/bases/syntax)

## デイリーノートの特定箇所を常に表示・編集

**対応しています。Obsidian CLIやプラグインは不要です。** 元のmdファイルを直接読み書きします。

1. 設定でデイリーノートのフォルダと日付書式を指定します。
2. `… → ノートの見出しを表示` を選びます。
3. 見出し名を `Tasks` などに設定し、毎日切替を `yes` にします。
4. 今日のノートの `## Tasks` 配下が表示され、チェックボックスを押すと元ファイルの対応行だけを更新します。

日付書式は.NET形式で、ObsidianのMoment形式とは大文字・小文字が異なります。例えばObsidianの `YYYY-MM-DD` はこのアプリでは `yyyy-MM-dd`、`YYYY-MM-DD(ddd)` は `yyyy-MM-dd(ddd)` です。曜日名はWindowsのカルチャ設定に従います。拡張子 `.md` は自動で付けます。サブフォルダ形式 `yyyy/MM/yyyy-MM-dd` も使えます。

固定ノートの場合は絶対パスを入力して毎日切替を `no` にしてください。毎日切替の `yes` ではパス欄を使いません。

表示範囲は対象見出しの直後から、同じまたは上位レベルの次の見出しの直前までです。小見出しも含みます。元ノートの他の箇所・YAMLは保持します。同名見出しが複数ある場合は安全のため編集を拒否します。コードフェンス内の見出しは無視します。対象見出しには `#` 形式（ATX）を使用してください。

日付が変わると今日のファイルに切り替えます。編集中の場合は保存・再読込後に切り替えます。ファイルや見出しがない場合はエラーを表示し、勝手に新規作成しません。Obsidian側で作成すると次のポーリングで表示します。

## Google Calendar

付箋本文の独立した行に次の形式で入力し、保存します。

```text
@calendar 2026-10-05T09:00+09:00 会議
```

検索語は省略できます。時差省略時はWindowsのローカル時刻です。指定時刻以降の予定を開始時刻順に最大100件取得し、付箋内に表示します。Googleの `timeMin` は予定の終了時刻に対する下限のため、指定時刻に進行中の予定も含みます。繰り返し予定は各回に展開します。約60秒間隔、または `… → 予定を今すぐ取得` で再取得します。1枚の付箋につき最初のコマンドが対象です。前後は空行で区切ってください。コードフェンス内のコマンド例は実行しません。

予定をクリックすると件名・説明を編集できます。その後、変更前後と参加者への通知を示す確認ダイアログで **OKした場合のみ** GoogleへPATCHします。日時・参加者・繰り返し規則は変更しません。繰り返し予定の編集は取得した回が対象です。取得後にGoogle側で変更があった場合はETagで競合を検出し、上書きせず再取得を促します。

### 初回認証

1. [Google Cloud Console](https://console.cloud.google.com/) でプロジェクトを選び、Google Calendar APIを有効にします。
2. OAuth同意画面を設定します。テスト公開中なら利用するGoogleアカウントをテストユーザーに追加します。
3. 種類 **デスクトップアプリ** のOAuthクライアントを作成し、JSONをダウンロードします。
4. アプリの設定画面でそのJSONを選択します。Calendar IDは通常 `primary` です。
5. `設定を保存してGoogleにログイン` からブラウザで認証します。

認証はシステムブラウザ + loopback + PKCEを使用します。トークンはWindows DPAPIのCurrentUserで暗号化し、`google-token.bin` に保存します。Google OAuth JSONはリポジトリやVaultの共有先に入れないでください。

実際のGoogleアカウントでの接続・更新確認はユーザーのOAuth設定後に必要です。自動テストでは模擬HTTPによる検索・PATCH・ETag競合を確認しています。

参考: [Google Desktop OAuth](https://developers.google.com/identity/protocols/oauth2/native-app)、[Calendar resource versions](https://developers.google.com/calendar/api/guides/version-resources)

## Markdown対応範囲・データ保護

- 見出し、太字、斜体、取り消し線、通常・番号付きリスト、チェックリスト、引用、コード、表、リンクを描画します。
- HTMLは実行せず文字列として表示します。画像は現状では代替テキスト表示です。
- Obsidianの `[[wikilink]]`、埋め込み、Dataview、数式、コールアウト専用装飾は未対応です。文字として保持します。
- 入力はUTF-8（BOM有無を保持）。本文編集時は既存の改行形式に合わせます。
- プロパティ編集時はYAMLを再シリアライズするため、コメントや書式は変化する場合があります。未知のプロパティ値と本文は維持します。
- 書込み前にファイル全体のハッシュを照合します。編集中に外部変更があれば上書きせず、編集内容をコピー・退避してから再読込できます。
- 書込み前の内容を `%LOCALAPPDATA%/StickyNotes/backups` に保存します。日付・GUID付きファイルから手動復旧できます。バックアップは自動削除しません。
- ファイルの書込みは排他ハンドルで行い、書込み例外時は復元を試みます。電源断などに備えて、Vault全体の通常のバックアップも利用してください。

## 開発・検証

必要環境: Windows / .NET 8 SDK。

```powershell
.\build.ps1
.\build.ps1 -Publish
```

作業フォルダ内に `.tools/dotnet` がある場合はそのSDKを優先します。テストは外部サービスや実際のVaultに接続せず一時ファイルと模擬APIを使います。

検証用の保存先を分離して起動する場合（このモードでは画面テスト用にタスクバーにも表示します）:

```powershell
.\artifacts\app\StickyNotes.exe --data-dir C:\Temp\StickyNotes-test
```

構成: `StickyNotes.Core`（Markdown保存・見出し編集・Calendar REST/OAuth）、`StickyNotes`（WPF付箋・描画・通知領域・モニター配置）、`StickyNotes.Tests`（データ保護・描画・APIの実行可能テスト）。
