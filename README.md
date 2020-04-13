# Health Report

[健康日記](https://www.htech-lab.co.jp/covid19/)と連携するウェブシステム．
送付されたCSV添付ファイルを統合し表示する．

## Quick start

1. docker と docker-compose をインストール

2. ソースコードの入手

```
git clonet https://github.com/kjmtks/HealthReport.git
cd HealthReport
```

4. `docker-compose.developemnt.override.yaml` の編集

```
$ vim docker-compose.developemnt.override.yaml
```

5. 実行

```
make development-up
```

しばらくしたら `http://localhost:8080` にアクセスしてください．

初期ユーザーとして，`admin` が使用可能です．
パスワードは `password` です．

## 停止

以下のコマンドで，アプリケーションを停止します．

```
make development-down
```

## 削除

以下のコマンドで，アプリケーションを停止するとともにデータベースも削除します．

```
make development-remove
```


## Run in Production

Install docker and docker-compose.

Run following commands:

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

## Gmail の IMAP を利用する場合

* 対象のメールアドレスでIMAPを有効化する
* 2段階認証を設定する
* アプリのパスワードを設定し，それをパスワードとして利用する
* 暗号化方式は `ssl` とする 

複数の割り当てをする場合は，続けて同様の記述をしてください．
