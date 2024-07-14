<div align="center">
  <a href="https://jiayisoftware.github.io/launcher"><img src="https://github.com/JiayiSoftware/JiayiLauncher/blob/master/.github/assets/logo.png" alt="Jiayi Launcher" width="700"></a>
</div>

### Minecraft Windows EditionのModマネージャー (統合版)

[![Build](https://github.com/JiayiSoftware/JiayiLauncher/actions/workflows/dotnet.yml/badge.svg)](https://github.com/JiayiSoftware/JiayiLauncher/actions/workflows/dotnet.yml)
[![Discord](https://img.shields.io/badge/chat-on%20discord-7289da.svg)](https://jiayisoftware.github.io/discord)

## 機能
> * シンプルで使いやすいユーザーインターフェイス
> * 内部および外部のMODを管理し、ローカルおよびインターネットから起動
> * Modマネージャーと連携するバージョンマネージャー
> * プロフィールを使用して異なるゲームデータフォルダー（リソースパック、ワールド、設定など）を切り替え
> * ランチャーの外観からソースコードまでの極度のカスタマイズ性
> * RenderDragonシェーダーを簡単に適用する

## インストール
通常のユーザーの場合、[こちら](https://phased.tech/download/JiayiInstaller.exe)からインストーラーをダウンロードできます。サイズは約6 MBです。

過去のリリースは[Releases](https://github.com/JiayiSoftware/JiayiLauncher/releases)ページで見つけることができます。

以下の手順に従ってリポジトリをクローンし、自分でJiayi Launcherをビルドすることもできます。

## ビルド
### 必要条件
* Windows 10以降を実行しているコンピュータ
* [Git](https://git-scm.com/)
* [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
* 任意: .NET 8をサポートするIDE (例: [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) または [JetBrains Rider](https://www.jetbrains.com/rider/))

### 手順
1. リポジトリをクローン
* ```git clone --recursive https://github.com/JiayiSoftware/JiayiLauncher.git```

**Visual StudioなどのIDEを使用する場合:**

2. IDEでソリューションファイルを開く
3. ソリューションをビルド

**IDEを使用しない場合:**

2. リポジトリのルートディレクトリに移動
3. ```dotnet build```を実行

これにより、デバッグモードでランチャーがビルドされます。  
リリースモードでビルドするには、```-c Release```を追加できます。

## 貢献
もしJiayi Launcherに貢献したい場合は、リポジトリをフォークしてプルリクエストを送信してください。

JiayiはC#で書かれており、Blazor Hybridを介してUIにHTMLを使用しています。

コードは率直に言ってめちゃくちゃですので、特定のコードスタイルに従う必要はありません。ただし、コードを清潔で読みやすく保つように心がけてください。将来的にはコードスタイルガイドが用意される予定です。

## ライセンス
Jiayi LauncherはGNU GPL-3.0ライセンスの下で公開されており、[こちら](https://github.com/JiayiSoftware/JiayiLauncher/blob/master/LICENSE)で読むことができます。

全文を読みたくない方のために簡単な要約:
- コードを自由に改変、共有、販売できます
- ただし、これを行うプロジェクトは互換性のあるGPLライセンスの下でオープンソースにする必要があります
- 変更は文書化する必要があります
