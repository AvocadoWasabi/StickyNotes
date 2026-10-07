[English](DEVELOPMENT.en.md) | [日本語](DEVELOPMENT.md) | [简体中文](DEVELOPMENT.zh-CN.md)

# 開発と公開

## ブランチ運用

1. 新しい変更の実装前に専用ブランチを作成します（例: `git switch -c feat/example`）。
2. 実装・資料更新・検証を行い、作業完了前に変更差分全体をコードレビュー・セキュリティレビューします。
3. **重大な未解決指摘（Critical / High、P0 / P1、重大なデータ損失・情報流出など）があればマージ・pushを中止します。** 必要なレビュー・検証を完了できない場合も完了扱いにせず、状況を報告します。修正後は再検証・再レビューします。
4. 検証済みの変更をローカルでコミットし、検証結果・両レビューの結果・残る制限を報告します。通常の手順ではPRを作成しません。
5. **ユーザーの明示的な指示があるまでmainへのマージと、作業ブランチを含むGitHubへのpushは行いません。** タグのpushやReleaseの公開も指示が必要です。

`AGENTS.md` はローカル開発用のためGit管理・配布の対象外です。公開先は [AvocadoWasabi/StickyNotes](https://github.com/AvocadoWasabi/StickyNotes) です。

## ビルドと検証

Windowsと.NET 8 SDKを使用します。ローカルの `.tools/dotnet` があればそちらを優先します。

```powershell
./build.ps1
./build.ps1 -Publish
```

どちらも復元・Releaseビルド・既存テストを実行し、自己完結型アプリ、インストール用スクリプト、3言語の資料、ライセンスを `artifacts/app` に生成します。通常の `./build.ps1` 完了後、そのまま `artifacts/app/Install.cmd` からローカルインストールできます。起動中のSticky Notesを通知領域から終了してから実行してください。ビルド完了時にもインストール先のスクリプトを案内します。ビルド自体はインストールを実行しません。

`-Publish` は追加で `artifacts/StickyNotes-win-x64.zip` とSHA256チェックサムを生成・更新します。通常のビルドでは既存ZIPは更新しないため、ZIPを配布する場合は `-Publish` を使用してください。

通常ローカルビルドはObsidian CLIを既定とし、既存の明示的なローカル設定は維持します。「設定 → Obsidian CLI」で接続します。Daily notes設定はObsidianを参照します。付箋保存先の上書き指定は`FoldersBridge`で一覧・作成・移動を扱い、移行制御はCLI／ローカル共通です。`DailyBridge`がVaultを列挙せず今日・昨日を解決、保持処理は両取得方式で共用します。通常プロファイルと、`./build.ps1 -Publish -TasksPreview`による`artifacts/tasks-cli-preview`の独立プロファイルは分離したままです。[CLI手順・構成・制約](TASKS-CLI-PREVIEW.md#日本語)を参照してください。

`DataviewBridge.js`は元ノートのパスを渡して本家の`query` + Markdown export APIsを呼びます。TasksとDataviewは描画・ライフサイクル処理を共用し、状態とエラーは独立しています。通常テストで表示と古い応答を検証し、任意のNode.js `--dataview-bridge-smoke`で生成した実際のCLIコードを合成APIデータで検証できます。[Dataviewの構成・制限・検証状況](DATAVIEW.md#日本語)を参照してください。

`QueryTaskTargets.js`は出力したチェックボックスを照合済みの元ノートに対応付け、`NotesBridge.js`がバックアップ・ハッシュ競合検出付きで更新します。`NoteBrowserWindow`は`NoteSource`から一覧を取得し、クエリを実行せず通常のMarkdown／YAMLを表示します。任意の`--query-task-smoke`で生成したアダプターを合成ノートで検証できます。[チェック操作と一覧の仕様](QUERY-EDITING-AND-BROWSER.md#日本語)を参照してください。

`src/StickyNotes.Core` は保存・見出し編集・Calendar連携、`src/StickyNotes` はWPF UI、`tests` はデータ保護・描画・APIのテストです。

Google Tasksは同じOAuthクライアント・トークンに `tasks.readonly` スコープを追加して利用します。`GoogleTasks.cs` は固定HTTPSエンドポイントにGETのみを送り、ページ送りと予定日の「日付」としての処理（UTCからローカル時刻への変換なし）を行います。1回の取得上限は一致する100件・20リクエスト・30秒です。ページ送り、日付境界、絞り込み、キャンセル、片方だけの失敗、日付変更を実認証情報なしでテストします。

### 独立データでの動作確認

テストは一時ファイルと模擬APIを使用し、実際のVaultやGoogleには接続しません。実アカウントでの接続・更新確認には利用者自身のOAuth設定が必要です。

次のコマンドは保存先を分離して起動します。このモードではタスクバーに付箋を表示し、ジャンプリストを登録・変更しません。

```powershell
.\artifacts\app\StickyNotes.exe --data-dir C:\Temp\StickyNotes-test
```

## ドキュメントの言語

README・変更履歴・導入／開発ガイド・関連資料・リリース説明を編集するときは、英語・日本語・簡体字中国語（zh-CN）を同じ変更内で同期します。機能、制約、例、バージョン、リンクを揃え、単一ファイル内の言語別併記も可とします。言語切替とビルド時の同梱を確認し、ドキュメントの翻訳とUI・サンプルの翻訳を区別します。公開済みタグ・ZIPは指示なく差し替えません。

## 公開手順（指示を受けた場合のみ）

機能追加・修正の完了時に [日本語のバージョン履歴](../CHANGELOG.ja.md)、[英語の変更履歴](../CHANGELOG.md)、[簡体字中国語の変更履歴](../CHANGELOG.zh-CN.md) の未リリース欄を更新します。正式公開時には実際のバージョン番号・公開日・Releaseリンクを記録し、3言語の内容を揃えます。下書きや予定を公開済みとして記載しないでください。`docs/RELEASE-NOTES.md` にも、そのリリースでの主な変更を反映します。

1. `src/StickyNotes/StickyNotes.csproj` の `Version`、3言語READMEのダウンロードリンク、変更履歴、リリースノートを公開バージョンに揃えます。`./build.ps1 -Publish` と両レビューを完了し、ローカルコミット後、指示されたブランチをmainにマージしてpushします。
2. GitHub Actionsの「Build and test」が成功したことを確認します。
3. Releaseの公開指示を受けたら、「Package release」をmain上で手動実行し、新しいタグ（例: `v1.0.0`）を指定します。
4. ワークフローが検証とZIP生成を実行し、**下書きRelease**を作成します。添付ZIPのSHA256、実行ファイルのバージョン、タグの対象コミット、同梱資料、個人設定などの混入がないことを確認してから公開します。

pushだけでReleaseは作成・公開されません。ZIPはGit履歴には含めず、Releaseの添付ファイルとして配布します。

ローカル確認用の独自パッケージは `artifacts` 内に置き、コミット・アップロードしません。正式なReleaseにはワークフローがmainから新しく生成したZIPとチェックサムを使用します。

## 公開対象からの除外

`.gitignore` でSDK、NuGetキャッシュ、ビルド出力、個人設定、OAuth認証JSON、トークン、ローカルデータを除外します。コミット前に `git status --short` と差分を確認してください。

## アイコン

元画像は `docs/images/StickyNotes.png`、ICOは `src/StickyNotes/Assets` にあります。Windows用の複数サイズICOを再生成するには `./scripts/Convert-AppIcon.ps1` を実行します。生成履歴は [ICON.md](ICON.md) を参照してください。

## 表示言語

UI文言は `src/StickyNotes.Core/Strings.resx`（日本語）、`Strings.en.resx`、`Strings.zh-CN.resx` で管理します。キーと書式の引数を揃え、`L10n` から取得してください。言語は起動時に初期化し、設定保存時の変更は次回起動まで保留します。保存済みノートや通信のキーは翻訳しません。テストではリソースの不足、言語設定の永続化、メニュー、データ保持を確認します。配布には `en/StickyNotes.Core.resources.dll` と `zh-CN/StickyNotes.Core.resources.dll` が必要です。
