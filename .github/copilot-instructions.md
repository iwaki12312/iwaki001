# このリポジトリ（Unity）でAIが即戦力になるための指示

## プロジェクト概要
- Unity 6（6000.3.0f1）。1〜2歳児向けの「タップのみ」ミニゲーム集。概要と追加ゲーム時の前提は [cline.yml](cline.yml) を最優先で読む。
- ターゲット: Android / iOS（パッケージは [Packages/manifest.json](Packages/manifest.json)）。

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

## 開発・デバッグ（VS Code）
- 推奨拡張: Visual Studio Tools for Unity（[.vscode/extensions.json](.vscode/extensions.json)）。
- デバッグは [Attach to Unity](.vscode/launch.json) を使い、Unity Editor起動後にアタッチする。
- VS Codeの表示/検索ではUnity生成物やバイナリが除外される（[.vscode/settings.json](.vscode/settings.json)）。見えない場合は除外設定を確認する。

## テスト
- 現状、自動テスト運用はなし（Test Runner/CIの前提なし）。変更確認はUnity EditorのPlayで行い、ログ（例: `MenuController`/`IAPManager` の `Debug.Log`）で初期化やロック状態を確認する。

