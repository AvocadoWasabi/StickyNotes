[English](GOOGLE-CALENDAR.en.md) | [日本語](GOOGLE-CALENDAR.md) | [简体中文](GOOGLE-CALENDAR.zh-CN.md)

[表示言語の設定](USAGE.md#language)（未リリース）

# Google Calendarの使い方と設定

[基本操作に戻る](../README.ja.md)

[検索と編集](#usage) · [初回認証](#setup) · [取り込んだJSONの削除](#json) · [困ったとき](#troubleshooting)

<a id="usage"></a>

## 検索と編集

付箋本文の独立した行に次の形式で入力し、保存します。

```text
@calendar 2026-10-06T09:00
```

**上の例のように、キーワード・タイムゾーンなしで検索できます。** タイムゾーン省略時はWindowsのローカル時刻です。

絞り込む場合は `@calendar 2026-10-06T09:00 会議` のようにキーワードを追加します。`2026-10-06T09:00+09:00` のような時差の指定も引き続き使えます。

指定時刻以降の予定を開始時刻順に最大100件取得し、付箋内に表示します。Googleの `timeMin` は予定の終了時刻に対する下限のため、指定時刻に進行中の予定も含みます。

繰り返し予定は各回に展開します。約60秒間隔、または `… → 予定を今すぐ取得` で再取得します。

1枚の付箋につき最初のコマンドが対象です。前後は空行で区切ってください。

コードフェンス内のコマンド例は実行しません。

編集中、独立した段落の先頭（前に本文がある場合は空行の後）で `@` を入力すると、`@calendar` の候補と入力例をフロート表示します。Tab・Enter・候補のクリックで、今日の `00:00` を使ったタイムゾーン・キーワードなしの例を挿入します。日時部分が選択されるので、必要な日時に変更して保存してください。挿入した日付は固定で、自動では翌日に変わりません。Escで候補を閉じます。補完操作自体では保存せず、メールアドレスやコードブロック内では候補を表示しません。

予定をクリックすると件名・説明を編集できます。その後、変更前後と参加者への通知を示す確認ダイアログで **OKした場合のみ** GoogleへPATCHします。日時・参加者・繰り返し規則は変更しません。繰り返し予定の編集は取得した回が対象です。取得後にGoogle側で変更があった場合はETagで競合を検出し、上書きせず再取得を促します。

<a id="setup"></a>

## 初回認証

設定画面の右ペイン `Google Calendar` にある接続ガイドを開きます。左ペインは付箋・デイリーノートの設定で、左右を独立してスクロールできます。以下は個人のGoogleアカウントで自分用に設定する手順です（2026-10-06に公式資料と照合）。ボタンは標準ブラウザを開きます。Google側の登録・同意はブラウザで行います。画面名は表示言語により異なります。

### 1-1: APIを有効化

[Google Calendar API](https://console.cloud.google.com/apis/library/calendar-json.googleapis.com)で、上部のプロジェクト選択から既存のものを選ぶか、「新しいプロジェクト」で名前（例: `StickyNotes Personal`）を入力して作成・選択します。「有効にする（Enable）」を押します。「管理（Manage）」が表示されていれば有効です。以降も画面上部で**同じプロジェクト**を選択してください。

### 1-2: 初回登録

[ブランディング（Branding）](https://console.cloud.google.com/auth/branding)が未設定なら「開始（Get started）」を押します。アプリ名（例: `StickyNotes Personal`）と自分が受信できるユーザーサポートメールを設定して「次へ」。対象は個人アカウントなら「外部（External）」を選んで「次へ」。連絡先メールを入力して「次へ」。ポリシーを確認し、同意する場合はチェックして「続行」→「作成」と進みます。設定済みなら内容を確認して次へ進みます。「内部（Internal）」はGoogle Cloud組織内の利用者に限定する場合の選択肢です。

### 1-3: ログインするアカウントを登録

[対象（Audience）](https://console.cloud.google.com/auth/audience)で「外部」かつ「テスト中（Testing）」の場合、「テストユーザー（Test users）」→「ユーザーを追加（Add users）」から、**カレンダーにログインするアカウント**のメールアドレスを追加して「保存（Save）」。Cloudの管理用アカウントと同じとは限りません。この手順で「アプリを公開」を押す必要はありません。

### 1-4: スコープを保存

外部向けの設定では、[データアクセス（Data Access）](https://console.cloud.google.com/auth/scopes)→「スコープを追加または削除（Add or Remove Scopes）」から `https://www.googleapis.com/auth/calendar.events` を選択します。見つからなければ「スコープを手動で追加（Manually add scopes）」にURL全体を入力して追加します。

「更新（Update）」で戻り、「保存（Save）」します。これは予定の閲覧・編集権限です。

`calendar.events.readonly` とは異なり、アプリのCalendar IDだけに許可範囲を限定するものでもありません。

### 1-5: デスクトップ用JSONを取得

[クライアント（Clients）](https://console.cloud.google.com/auth/clients)→「クライアントを作成（Create client）」→「アプリケーションの種類: デスクトップアプリ（Desktop app）」を選択。名前（例: `StickyNotes Desktop`）を入力して「作成（Create）」し、作成結果の「JSONをダウンロード（Download JSON）」で**画面を閉じる前に保存**します。

後からシークレットを再取得できない場合があります。Webアプリ用のリダイレクトURI・JavaScript生成元は設定しません。

APIキーやサービスアカウントのJSONは使用しません。

### アプリの手順2: JSONを選択・取り込み

「認証JSONを選択して確認」でダウンロードしたファイルを選び、横の「アプリに取り込む」を押します。形式を検証後、`%LOCALAPPDATA%\StickyNotes\credentials-google.json` にコピーし、使用するパスを即時保存します。取り込み済みならコピーを置き換えます。元ファイルは変更しないため、取り込み成功後は移動・削除しても使えます。取り込みを省略して元のパスを使用する場合は、そのファイルを移動・削除しないでください。名前の変更は不要です。自分のメインカレンダーならCalendar IDを `primary` にします。

### アプリの手順3: ログイン

「設定を保存してGoogleにログイン」を押し、3分以内にブラウザで1-3のアカウントを選び、アプリ名を確認してカレンダーへのアクセスを許可します。結果は自動で受信されます。ブラウザのタブを閉じて設定画面に戻り、「接続確認が完了しました」を確認してください。認証情報の保存後、予定を変更せず指定カレンダーを読み取れるか確認します。

### アプリの手順4: 接続だけ再確認

ログイン済みなら、ブラウザのログイン画面を開かず確認できます。手順3・4は、Google関連以外も含む設定画面の入力内容を保存します。接続確認の成功は、予定の編集権限を保証しません。

<a id="cancel"></a>

## キャンセル・再試行

認証・接続確認はキャンセルでき、設定画面を閉じた場合も中止します。キャンセル後はブラウザの認証タブを閉じてください。完了済みの認証情報は保持します。タイムアウト時は手順3からやり直します。

<a id="json"></a>

## 取り込んだJSONの削除

横の「取り込み済みJSONを削除」は確認後、アプリ専用フォルダのコピーだけを削除します。保存済みの使用パスがそのコピーなら空に戻します。元ファイル・外部パスのファイル・認証トークンは削除せず、Google側のアクセス許可も取り消しません。取り込みと削除はその場で反映し、他の設定欄の未保存入力は保存しません。設定保存に失敗した場合はコピーを元に戻します。認証・接続確認中はこれらのボタンを操作できません。保存先は実行ファイルのフォルダではなく、現在のWindowsユーザーのアプリデータ領域です。

<a id="troubleshooting"></a>

## 困ったとき

| 状況 | 確認・操作 |
| --- | --- |
| Googleのログイン画面で `access_denied` | 1-3のテストユーザーとログイン先を確認。組織のポリシーによるブロックは管理者に確認 |
| 未確認アプリの警告 | 自分が作成したクライアントのアプリ名・アカウントであることを確認。不明な場合は進まず中止 |
| ログイン後に接続確認が失敗 | Calendar ID、同じプロジェクトでのAPI有効化、許可した権限を確認。認証情報は保持。再確認は手順4、権限の再許可は手順3 |
| 外部／テスト中で、数日後に再認証を要求される | このスコープの更新トークンは7日で期限切れになるため、手順3で再ログイン |

<a id="credentials"></a>

## 認証情報の保管

認証はシステムブラウザ + loopback + PKCEを使用します。トークンはWindows DPAPIのCurrentUserで暗号化し、`google-token.bin` に保存します。Google OAuth JSONはリポジトリやVaultの共有先に入れないでください。

<details>
<summary>設定の根拠・技術情報</summary>

設定の根拠: [同意画面の設定](https://developers.google.com/workspace/guides/configure-oauth-consent)、[対象・テストユーザー](https://support.google.com/cloud/answer/15549945)、[Calendarのスコープ](https://developers.google.com/workspace/calendar/api/auth)、[クライアントの作成・JSONの保管](https://support.google.com/cloud/answer/15549257)、[更新トークンの期限](https://developers.google.com/identity/protocols/oauth2#expiration)。

実際のGoogleアカウントでの接続・更新確認はユーザーのOAuth設定後に必要です。自動テストではGoogle応答を模擬し、loopbackでの認証結果受信、state・パスの拒否、PKCE、認証拒否、キャンセル、不正なトークン応答、保存失敗、読み取りのみの接続確認、検索・PATCH・ETag競合を確認しています。

参考: [Google Desktop OAuth](https://developers.google.com/identity/protocols/oauth2/native-app)、[Calendar resource versions](https://developers.google.com/calendar/api/guides/version-resources)

</details>
