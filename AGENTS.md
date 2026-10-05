# このリポジトリの作業ルール

- 機能追加・修正の実装前に、変更専用のブランチを作る。mainで直接実装しない。
- 作業完了時に検証して、変更をローカルでgit commitする。
- mainへのマージ、GitHubへのpush、タグのpush、Releaseの公開は、それぞれユーザーから明示的な指示があったときだけ行う。
- 公開リポジトリは https://github.com/AvocadoWasabi/StickyNotes （public）。
- 認証情報、個人のノート、設定、バックアップ、SDK、ビルド出力をコミットしない。
- 通常の検証は `./build.ps1`。配布に関わる変更は `./build.ps1 -Publish` でZIPも生成・検証する。
- 利用者向けの説明は日本語で、導入手順と関連資料を実装と合わせて更新する。
