# 開発と公開

## ブランチ運用

1. 新しい変更の実装前に専用ブランチを作成します（例: `git switch -c feat/example`）。
2. 実装・資料更新・検証を行い、ローカルでコミットします。
3. **ユーザーが明示的に指示するまでmainへのマージとGitHubへのpushは行いません。** タグのpushやReleaseの公開も指示が必要です。

継続作業時のルールは [AGENTS.md](../AGENTS.md) にも記載しています。公開先は [AvocadoWasabi/StickyNotes](https://github.com/AvocadoWasabi/StickyNotes) です。

## ビルドと検証

Windowsと.NET 8 SDKを使用します。ローカルの `.tools/dotnet` があればそちらを優先します。

```powershell
./build.ps1
./build.ps1 -Publish
```

前者は復元・Releaseビルド・既存テスト、後者はさらに自己完結型アプリ、インストール用スクリプト、資料、ライセンスを同梱した `artifacts/StickyNotes-win-x64.zip` とSHA256チェックサムを生成します。

`src/StickyNotes.Core` は保存・見出し編集・Calendar連携、`src/StickyNotes` はWPF UI、`tests` はデータ保護・描画・APIのテストです。

## 公開手順（指示を受けた場合のみ）

1. 変更を確認し、指示されたブランチをmainにマージしてpushします。
2. GitHub Actionsの「Build and test」が成功したことを確認します。
3. Releaseの公開指示を受けたら、「Package release」をmain上で手動実行し、新しいタグ（例: `v1.0.0`）を指定します。
4. ワークフローが検証とZIP生成を実行し、**下書きRelease**を作成します。添付ZIP・チェックサム・説明を確認してから公開します。

pushだけでReleaseは作成・公開されません。ZIPはGit履歴には含めず、Releaseの添付ファイルとして配布します。

## 公開対象からの除外

`.gitignore` でSDK、NuGetキャッシュ、ビルド出力、個人設定、OAuth認証JSON、トークン、ローカルデータを除外します。コミット前に `git status --short` と差分を確認してください。

## アイコン

元画像は `docs/images/StickyNotes.png`、ICOは `src/StickyNotes/Assets` にあります。Windows用の複数サイズICOを再生成するには `./scripts/Convert-AppIcon.ps1` を実行します。生成履歴は [ICON.md](ICON.md) を参照してください。
