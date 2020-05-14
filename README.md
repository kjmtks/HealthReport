# Health Report

[健康日記アプリ](https://www.htech-lab.co.jp/covid19/)と連携するウェブシステムです。
指定されたメールサーバに対してIMAPで接続を行い、アプリから送付されてきたCSV添付ファイルを取得・統合し表示します。

ウェブアプリケーションとして運用することも，ブラウザをインタフェースとするスタンドアロンのアプリケーションとして個人的に利用することも可能です．

## 動作環境

- Dockerが動作する環境が必要です。（Linux, Windows, Mac）
- メールサーバにIMAP接続できる必要があります。

## 動作確認方法

動作を確認するためにシステムを起動する手順を説明します．
Windowsを使用する場合は WSL で操作を行って下さい．

1. docker と docker-compose をインストール

* [Docker](https://docs.docker.com/get-docker/)
* [docker-compose](https://docs.docker.com/compose/install/)

2. ソースコードの入手

```
git clone https://github.com/kjmtks/HealthReport.git
cd HealthReport
```

4. `docker-compose.developemnt.override.yaml` の編集（特に必要なければ、そのままで可）

```
$ vim docker-compose.developemnt.override.yaml
```

5. 実行

```
make development-up
```

しばらくしたら `http://localhost:8080` にアクセスしてください．

初期ユーザーとして，`admin` が使用可能です．
初期パスワードは `password` です．

### 停止

以下のコマンドで，アプリケーションを停止します．

```
make development-down
```

### 削除

以下のコマンドで，アプリケーションを停止するとともにデータベースも削除します．

```
make development-remove
```

---

## 本番環境での実行

### HTTPSでの実行

docker と docker-compose をインストール後、以下のコマンドを実行してください。

1. ソースの入手と設定

```
$ git clone https://github.com/kjmtks/HealthReport.git
$ cd HealthReport
$ vim docker-compose.production.default.yaml
(or $ vim docker-compose.production.override.yaml)
```

2. サーバー証明書の準備

```
$ make pfx KEY=your-key-file.key CER=your-cert-file.cer
$ cp your-ca-file.cer certs/
```

`your-key-file.key`, `your-cert-file.cer`, `your-ca-file.cer` はそれぞれ秘密鍵, 証明書, 中間証明書であるとします．

3. 起動

```
$ make production-up
```

しばらくしてから， `https://localhost` (or `https://yourhost`) にアクセスして下さい．

4. 停止

以下のコマンドで停止することができます．

```
$ make production-down
```

### HTTPでの実行

HTTPで実行することもできます．証明書等は不要です．
ローカルネットワークで運用し，本システム利用者（管理者）を限定できる場合はHTTPで問題ないと思います．

docker と docker-compose をインストール後、以下のコマンドを実行してください。

1. ソースの入手と設定

```
$ git clone https://github.com/kjmtks/HealthReport.git
$ cd HealthReport
$ vim docker-compose.production.http.yaml
```

2. 起動

```
$ make production-http-up
```

しばらくしてから， `http://localhost` (or `https://yourhost`) にアクセスして下さい．

3. 停止

以下のコマンドで停止することができます．

```
$ make production-http-down
```

---

## 利用方法


### 管理者

1. 「登録・更新」より観察対象者の情報を入力してください。（スペースまたはタブ区切り）

LDAPの設定をすることで，本システムを用いて観察対象者の情報を収集・登録することができますが，
Google Forms などを使用して収集したデータを一括登録することもできます．
フォームのテンプレートは [こちら](https://docs.google.com/forms/d/1HdGAWrvjN7U2wMmpLs40T5Dbam6fGCpDBYuDxY-9ocw/edit)．

2. 「グループ」の「＋」ボタンよりメールサーバの情報を新規追加する

3. 追加したグループ名をクリックすることで、そのコースの画面に移動する

4. 「Check new mail」ボタンでメールを取得する　絞り込み条件を指定してデータの絞り込みができる


---

## Gmail の IMAP を利用する場合

* 対象のメールアドレスでIMAPを有効化する（Gmailの設定　＞　メール転送とPOP/IMAP　＞　IMAP を有効にする）
* 2段階認証を設定する（Googleアカウントを管理　＞　セキュリティ　＞　2 段階認証プロセス）
* アプリのパスワードを設定し，それをパスワードとして利用する（Googleアカウントを管理　＞　セキュリティ　＞　アプリパスワード）
* 本システムに登録する際の暗号化方式は `ssl` とする 

---

## 環境変数

`docker-compose*.yaml` で以下の環境変数を設定することでシステムの振る舞いを調整できます:

|変数名   |例       | 説明    |
|--------|---------|--------|
| `TITLE` | `Health Report` | タイトル |
| `TIMEFRAME` | ※1 | 一日に複数回提出させる場合に設定する．運用途中での変更は非推奨． |
| `MAIL_SUBJECT` | ※2 | 健康日記アプリから送付される非感染者用メールのサブジェクト．この文字列をサブジェクトに含むメールを収集対象とする． |
| `MAIL_INFECTED_SUBJECT` | ※3 | 健康日記アプリから送付される感染者用メールのサブジェクト． |
| `OVERRIDE` | `1` | 重複した健康データがある場合に，過去のデータを上書きする場合は何らかの文字列を指定．上書きしない場合は設定しない|
| `SUBDIR` | `/subdir` | サブディレクトリで動作させる場合に指定． |

※1 `"午前 00:00:00-11:59:59; 午後 12:00:00-23:59:59"`　のように `;` 区切りで複数の時間帯を記述する．

※2 規定値は現在のアプリのバージョンで用いられている `健康フォローアップ用健康観察データの報告`

※3 規定値は現在のアプリのバージョンで用いられている `【感染者用】健康観察データの報告`

### LDAP関連

同様に，以下を設定することで LDAP を使用したスタッフの認証機能が利用できます:

|変数名   |例       | 説明    |
|--------|---------|--------|
| `LDAP_REGEX_STAFF` | `^c0000[0-9]{5}$$` | ログインしたユーザーがスタッフであることを判定するための，アカウントに対する正規表現 |
| `LDAP_HOST` | `ldap1.foo.jp` | LDAPホスト |
| `LDAP_PORT` | `636` | LDAPのポート番号 |
| `LDAP_BASE` | `dc=foo,dc=jp` | ベースDN |
| `LDAP_ID_ATTR` | `uid` | アカウントを示す属性 |
| `LDAP_NAME_ATTR` | `displayName;lang-ja` | 名前を示す属性 |

加えて，以下を設定することで LDAP を使用した観察対象者の登録機能が利用できます:

|変数名   |例       | 説明    |
|--------|---------|--------|
| `LDAP_SEARCH_USER_ACCOUNT` | `user1` | ユーザー検索を行うアカウント |
| `LDAP_SEARCH_USER_PASSWORD` | `user1-password` | 上記アカウントのパスワード |

## フィルタ

### 初期フィルタ

コース毎に絞り込み条件の初期値を設定することができます．
コースの作成・編集画面で，「初期フィルタ」欄にフィルタ式を記述します．

### フィルタボタン

コース毎に絞り込み検索を簡便に行うためのボタンを任意に設定することができます．
コースの作成・編集画面で，「フィルタボタン」欄に以下の形式で1行につきひとつを記述します．

```
ボタンのラベル: フィルタ式
```

### フィルタ式の構文

「フィルタ式」には検索条件を表す論理式を記述します．

* 観察対象番号: `user=="観察対象者番号"`, `user!="観察対象者番号"`, `user=*"観察対象者番号"`, `user*="観察対象者番号"`
  * `==`, `!=`: それぞれ，指定した観察対象者番号に一致する, 一致しない
  * `=*`, `*=`: それぞれ，前方一致，後方一致
* 体温: `temp==体温`, `temp!=体温`, `temp>=体温`, `temp<=体温`, `temp>体温`,`temp<体温`
  * 体温は正の実数
* 測定年月日: `date==年月日`, `date!=年月日`, `date>=年月日`, `date<=年月日`, `date>年月日`, `date<年月日`
  * 年月日には以下が設定可能:
    * `yyyy/MM/dd`
    * `yyyy/MM/dd+n`: `yyyy/MM/dd` から n 日後の年月日
    * `yyyy/MM/dd-n`: `yyyy/MM/dd` から n 日前の年月日
    * `today`: 当日の年月日
    * `today+n`: 当日から n 日後の年月日
    * `today-n`: 当日から n 日前の年月日
    * `thisweek`: 直前の月曜日の年月日
    * `thisweek+n`: 直前の月曜日から n 日後の年月日
    * `thisweek-n`: 直前の月曜日から n 日前の年月日
    * `thismonth`: 当月 1 日の年月日
    * `thismonth+n`: 当月 1 日から n ヶ月後の年月日
    * `thismonth-n`: 当月 1 日から n ヶ月前の年月日
* 異常の有無: `error==真偽値`, `error!=真偽値`
  * 真偽値は `true` または `false`
* 提出されているか否か: `submitted==真偽値`, `submitted!=真偽値`
* 感染しているか否か: `infected==真偽値`, `infected!=真偽値`
* 時間帯: `timeframe=="時間帯"`, `timeframe!="時間帯"`
* タグ: `tag->"タグ"`
  * タグを含むか否か
  
構文の詳細は以下の通りです:

```
<StringExpr> ::= <string>
               | "user" | "timeframe" | "tag"

<DecimalExpr> ::= <decimal>
                | "temp"
                | "-" <DecimalExpr>
                | <DecimalExpr> ("+" | "-" | "*" | "\") <DecimalExpr>

<DateExpr> ::= <date>
             | "date" | "today" | "thisweek" | "thismonth"
             | <DateExpr> ("+" | "-") <DecimalExpr> | <DecimalExpr> ("+" | "-") <DateExpr>

<BooleanExpr> ::= "true" | "false"
                | "error" | "warning" | "submitted" | "infected"
                | <StringExpr> ("==" | "!=" | "*=" | "=*" | "->") <StringExpr>
                | <DecimalExpr> ("==" | "!=" | ">" | ">=" | "<" | "<=") <DecimalExpr>
                | <DateExpr> ("==" | "!=" | ">" | ">=" | "<" | "<=") <DateExpr>
                | <BooleanExpr> ("==" | "!=") <BooleanExpr>
                | "!" <BooleanExpr>
                | <BooleanExpr> ("&&" | "||" | "->") <BooleanExpr>
```

`/` 記号は年月日リテラルで使用される記号であるため， `<DecimalExpr>` 同士の割り算には `\` を割り当てていることに注意．


## 開発環境

- C#
- .NET Core 3.0
- Blazor
