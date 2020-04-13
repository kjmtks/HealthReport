# Health Report

[健康日記アプリ](https://www.htech-lab.co.jp/covid19/)と連携するウェブシステムです。
指定されたメールサーバに対してIMAPで接続を行い、アプリから送付されてきたCSV添付ファイルを取得・統合し表示します。

## 動作環境

- Dockerが動作する環境が必要です。（Linux, Windows Pro, Mac）
- メールサーバにIMAP接続できる必要があります。


## Quick start

1. docker と docker-compose をインストール

2. ソースコードの入手

```
git clonet https://github.com/kjmtks/HealthReport.git
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


## HTTPS環境での実行

docker と docker-compose をインストール後、以下のコマンドを実行してください。

```
$ git clonet https://github.com/kjmtks/HealthReport.git
$ cd HealthReport
$ vim docker-compose.production.default.yaml
(or $ vim docker-compose.production.override.yaml)
$ make pfx KEY=your-key-file.key CER=your-cert-file.cer
$ cp your-ca-file.cer certs/
$ make production-up
```

then, open `https://localhost` (or `https://yourhost`) in your browser.

### Stop

```
$ make production-down
```

---

## 利用方法

1. 「登録・更新」より観察対象者の情報を入力してください。（スペースまたはタブ区切り）

2. 「コース」の「＋」ボタンよりメールサーバの情報を新規追加する

3. 追加したコース名をクリックすることで、そのコースの画面に移動する

4. 「Check new mail」ボタンでメールを取得する　絞り込み条件を指定してデータの絞り込みができる


---

## Gmail の IMAP を利用する場合

* 対象のメールアドレスでIMAPを有効化する（Gmailの設定　＞　メール転送とPOP/IMAP　＞　IMAP を有効にする）
* 2段階認証を設定する（Googleアカウントを管理　＞　セキュリティ　＞　2 段階認証プロセス）
* アプリのパスワードを設定し，それをパスワードとして利用する（Googleアカウントを管理　＞　セキュリティ　＞　アプリパスワード）
* 暗号化方式は `ssl` とする 

複数の割り当てをする場合は，続けて同様の記述をしてください．


---

## 開発環境

- C#
