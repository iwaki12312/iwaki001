# このリポジトリ（Unity）でAIが即戦力になるための指示

## プロジェクト概要
- Unity 6（6000.3.0f1）。1〜2歳児向けの「タップのみ」ミニゲーム集。
- ターゲット: Android / iOS（パッケージは [Packages/manifest.json](Packages/manifest.json)）。

### 既存ミニゲーム一覧（概要）
- MakeBubbles: 少年をタップしてシャボン玉を発射し、シャボン玉をタップして割る
- WhackAMole: 地面から出てくるモグラをタップして叩く
- FlowerBlooming: プランターをタップすると花が咲く
- Cook: 鍋とフライパンをタップすると料理が飛び出す
- TouchTheStar: 星をタッチすると弾ける
- PianoAndViolin: ピアノやバイオリンをタップして音を鳴らす
- CatchInsects: 虫をタップして捕まえる
- PopBalloons: 風船をタップして割る
- Fishing: 魚をタップして釣り上げる
- VegetableDig: 土の中の野菜をタップして引き抜く（時々レア野菜が出現）
- EggHatch: たまごをタップしてヒビを入れて孵化（時々レア動物が出現）

### 新規ゲーム実装時の前提
- 指示がある場合を除き、既存ゲームのソースコードを参考にしない（ゲーム間の結合度を下げる）
- メニュー画面との遷移配線はユーザーが手動で実装する前提のため、AIは勝手に実装しない
- 効果音は「【ゲーム名】 + SFXPlayer」命名で、ゲームごとに一元管理する
- SFXPlayerは効果音ごとにボリュームを個別設定できるようにする
- マルチタッチ前提: 画面上の2か所以上を同時タップしても反応する作りにする
- 新規ゲームは 指示がない場合は[Assets/Games/_GameTemplate](Assets/Games/_GameTemplate) をコピーして作成し、仮スプライト/仮SEを一旦そのまま使って組み立てる
- ユーザー操作を極限まで減らすため、必要ならセットアップスクリプト等でオブジェクト配置/アタッチ/必須参照設定まで自動化する

### ゲームプレイ/シーン
- gameplay_style: タップのみ・シンプルUI
- scenes:
	- MainMenu
	- MakeBubbles
	- WhackAMole
	- FlowerBlooming
	- Cook
	- TouchTheStar
	- PianoAndViolin
	- CatchInsects
	- PopBalloons
	- Fishing
	- VegetableDig
	- EggHatch

## 全体構造（どこを触るか）
- 各ゲームは [Assets/Games](Assets/Games) 配下の `NN_ゲーム名` フォルダ（例: [Assets/Games/05_TouchTheStar](Assets/Games/05_TouchTheStar)）。
- 共通ロジックは [Assets/_Common](Assets/_Common)（例: ゲーム一覧・IAP）。
- `.csproj/.sln` はUnityが自動生成するため、通常は編集しない（編集は `Assets/**.cs` が中心）。

## メニュー/ゲーム一覧の仕組み（追加ゲームで重要）
- ゲーム一覧の単一ソースは [Assets/_Common/Scripts/GameInfo.cs](Assets/_Common/Scripts/GameInfo.cs)。ここに `sceneName / displayOrder / packID` を追加して管理する。
- メニューは [Assets/Games/00_Menu/Scripts/MenuController.cs](Assets/Games/00_Menu/Scripts/MenuController.cs) が `GameInfo.allGames` を走査し、Hierarchy上の `"{displayOrder:D2}_{sceneName}"` という名前のオブジェクトに `GameButton` を動的付与して初期化する。

## IAP（ゲームロック）の仕組み
- IAPコアは [Assets/_Common/Scripts/IAP](Assets/_Common/Scripts/IAP)（全体初期化: [IAPManager.cs](Assets/_Common/Scripts/IAP/IAPManager.cs)）。
- `pack_free` は常に所有（[EntitlementStore.cs](Assets/_Common/Scripts/IAP/EntitlementStore.cs)）。ロック判定は [FeatureGate.cs](Assets/_Common/Scripts/IAP/FeatureGate.cs)。
- 課金Product ID ↔ packId の対応は [PurchaseService.cs](Assets/_Common/Scripts/IAP/PurchaseService.cs) の `products` リスト。
- 詳細手順/デバッグ用ContextMenuは [Assets/_Common/Scripts/IAP/README.md](Assets/_Common/Scripts/IAP/README.md) を参照。

## 入力・UXのプロジェクト規約
- マルチタッチ前提: 既存実装は `Input.touchCount` をループして処理（例: [MenuController.cs](Assets/Games/00_Menu/Scripts/MenuController.cs) 内 `GameButton.Update()`）。新規ゲームでも「同時2点以上タップ」でも反応する作りにする。
- 新規ゲームは、起動に必要なオブジェクト配置/アタッチ/必須参照設定を極力自動化する（例: [TouchTheStarInitializer.cs](Assets/Games/05_TouchTheStar/Scripts/TouchTheStarInitializer.cs) が EventSystem や SFXPlayer 等を生成）。
- 効果音は「【ゲーム名】+ SFXPlayer」命名でゲーム単位に集約（例: `TouchTheStarSFXPlayer`）。
- 既存ゲームの踏襲は最小限に。特に [Assets/Games/01_MakeBubbles](Assets/Games/01_MakeBubbles) は品質が低い前提（[cline.yml](cline.yml)）。
- メニュー遷移の配線はユーザーが手作業で行う前提があるため、要件に明示がない限りAI側で勝手に実装しない（[cline.yml](cline.yml)）。

## 新規ゲーム追加（テンプレート）
- テンプレートは [Assets/Games/_GameTemplate](Assets/Games/_GameTemplate)。新規ゲームはこのフォルダをコピーして `NN_ゲーム名` にリネームし、`Scenes/`・`Sprites/`・`Audios/` 等の仮アセットを一旦そのまま使って組み立てる（詳細方針は [cline.yml](cline.yml)）。
- `Scripts/` は空が前提（新規ゲームのスクリプトはゼロから作成する）。

## テスト
- 現状、自動テスト運用はなし（Test Runner/CIの前提なし）。変更確認はUnity EditorのPlayで行い、ログ（例: `MenuController`/`IAPManager` の `Debug.Log`）で初期化やロック状態を確認する。

