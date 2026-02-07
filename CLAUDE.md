# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## プロジェクト概要

Unity 6 (6000.3.0f1) - 0～3歳児向けタップ操作のミニゲーム集。対象プラットフォーム: Android/iOS。

**パッケージ名**: `com.iw.wakuwaku.touchhiroba`

## ビルド・開発コマンド

### Unity エディタ操作
このプロジェクトはUnity Editorでビルドします。コマンドラインビルドスクリプトはありません。

- **エディタで再生**: Unity EditorのPlayボタンでテスト
- **Android APKビルド**: File → Build Settings → Android → Build
- **iOSビルド**: File → Build Settings → iOS → Build

### コンパイル確認
コード変更後は、Unity Editorでビルドするか、Consoleでエラーを確認してコンパイルを検証すること。

## プロジェクトアーキテクチャ

### ゲーム管理システム

全ゲームは**単一の情報源**に登録: [GameInfo.cs](Assets/_Common/Scripts/GameInfo.cs)

各ゲームエントリは以下を定義:
- `sceneName`: シーン識別子（例: "TouchTheStar"）
- `displayOrder`: ゲーム番号（1から開始）
- `packID`: IAPパック識別子（例: "pack_free", "pack_01"）

メニューシステム（[MenuController.cs](Assets/Games/00_Menu/Scripts/MenuController.cs)）は動的に:
1. `GameInfo.allGames` を読み込む
2. `{displayOrder:D2}_{sceneName}` という名前のGameObjectを検索（例: "05_TouchTheStar"）
3. `GameButton` コンポーネントをアタッチし、シーン/パック参照を設定
4. IAP権利に基づいてロック状態を更新

### IAP・ゲームロックシステム

IAP実装は [Assets/_Common/Scripts/IAP](Assets/_Common/Scripts/IAP) にあります。

**主要コンポーネント**:
- **IAPManager**: 起動時にIAPシステム全体を初期化
- **EntitlementStore**: 所有パックIDのローカル保存（PlayerPrefs経由）
- **PurchaseService**: Unity IAP統合、レシート検証、付与/剥奪ロジック
- **FeatureGate**: 権利に基づくロック/アンロックロジック

**フロー**:
1. `pack_free` は常に付与される
2. 起動時、IAPManagerがEntitlementStore → PurchaseService を初期化 → 購入同期
3. レシート検証で現在の権利を判定
4. FeatureGateがゲームの `packID` が所有されているかチェック

**Product ID ↔ Pack ID のマッピング**は `PurchaseService.cs` の products リストにあります。

詳細なIAPフロー（オンライン/オフライン、保留購入、adbデバッグ）は [README.md](README.md) のIAPセクションを参照。

### 入力システム

**重要**: このプロジェクトは**Unityの新Input System**（`UnityEngine.InputSystem`）を使用。

**使用禁止**:
- `Input.GetMouseButtonDown()`
- `Input.touchCount`
- `UnityEngine.Input` クラス全般

**代わりに使用**:
- マルチタッチ: `Touchscreen.current.touches`
- マウス入力: `Mouse.current`
- 参考例: [CatchInsects/InsectController.cs](Assets/Games/07_CatchInsects/Scripts/InsectController.cs)

**マルチタッチ要件**: ゲームは2箇所以上の同時タップに対応する必要があります。

### ゲームテンプレートシステム

新規ゲームは [Assets/Games/_GameTemplate](Assets/Games/_GameTemplate) から開始:
1. `_GameTemplate` フォルダをコピー → `NN_ゲーム名` にリネーム（例: `14_AnimalVoice`）
2. プレースホルダーアセットを使用（スプライト: `work_bg.png`, `work_sprite_a/b.png`; オーディオ: `work_sfx1-8.mp3`）
3. `Scripts/` に新規ゲームスクリプトをゼロから作成（テンプレートのScriptsフォルダは空）
4. セットアップスクリプトを作成（例: `Scripts/Editor/` に `AnimalVoiceSetup.cs`）して自動化:
   - シーン作成
   - オブジェクト階層セットアップ
   - コンポーネントアタッチ
   - アセット参照の配線
5. ユーザーがUnityメニューから手動実行（例: `Tools → Setup AnimalVoice Game`）

### 効果音アーキテクチャ

各ゲームは専用のSFXPlayerコンポーネントを持つ（例: `TouchTheStarSFXPlayer`, `AnimalVoiceSFXPlayer`）:
- 命名規則: `{ゲーム名}SFXPlayer`
- 効果音ごとの音量個別制御が可能
- ゲームのオーディオクリップ参照を一元管理

### ゲーム初期化パターン

ゲームはInitializerコンポーネントでシーン読み込み時に自動セットアップ:
- 例: [TouchTheStarInitializer.cs](Assets/Games/05_TouchTheStar/Scripts/TouchTheStarInitializer.cs)
- EventSystem、SFXPlayer、背景、ゲームオブジェクトを生成
- プログラムで参照を配線
- Unity Editorでの手動セットアップを最小化

## 新規ゲーム実装ルール

新規ゲーム実装時:

1. **分離**: 他のゲーム実装を参照しない（結合度を低く保つ）
2. **テンプレートベース**: [Assets/Games/_GameTemplate](Assets/Games/_GameTemplate) をコピーして開始
3. **プレースホルダーアセット**: テンプレートの `work_*.png` と `work_sfx*.mp3` を最初に使用
4. **セットアップ自動化**: Editorスクリプトでシーン/オブジェクトセットアップを自動化
5. **マルチタッチ**: 同時タップが動作するようにする（`Touchscreen.current.touches` をループ処理）
6. **メニュー配線なし**: AIはメニューシーン遷移を実装しない（ユーザーが手動で行う）
7. **SFXPlayer**: ゲーム専用の `{ゲーム名}SFXPlayer` をクリップごとの音量設定で作成
8. **ビルド確認**: 実装後、Unity Consoleでコンパイルを確認

### 新規ゲーム設計ガイドライン

0～3歳児向け:
- タップのみの操作（複雑なジェスチャーなし）
- スコア/UIテキストなし（読めないため）
- タップごとに視覚的フィードバック
- シンプルで繰り返しの仕組み
- レア/サプライズ要素で飽きさせない
- 明るくカラフルな見た目

## 外部ライブラリ

プロジェクトにはサードパーティライブラリが含まれています - ゼロから実装する前に利用可能か確認:
- **DOTween**: トゥイーン/アニメーション（インポートされている場合）
- **Cartoon FX Remaster** (CFXR): パーティクルエフェクト（[Assets/JMO Assets/Cartoon FX Remaster](Assets/JMO%20Assets/Cartoon%20FX%20Remaster)）

新機能実装前に関連アセットを検索すること。

## Unity パッケージ

主要パッケージ（[Packages/manifest.json](Packages/manifest.json) 参照）:
- `com.unity.inputsystem` (1.16.0) - 新Input System
- `com.unity.purchasing` (5.0.1) - IAP
- `com.unity.render-pipelines.universal` (17.3.0) - URP

## テスト

自動テスト基盤はありません。検証は以下で実施:
- Unity EditorのPlayモード
- Consoleログ確認（IAPManager、MenuControllerが初期化をログ出力）
- 最終確認用のデバイスビルド

## 重要なファイル

- [GameInfo.cs](Assets/_Common/Scripts/GameInfo.cs) - ゲームの中央レジストリ
- [MenuController.cs](Assets/Games/00_Menu/Scripts/MenuController.cs) - 動的メニュー生成
- [IAPManager.cs](Assets/_Common/Scripts/IAP/IAPManager.cs) - IAPシステム初期化
- [EntitlementStore.cs](Assets/_Common/Scripts/IAP/EntitlementStore.cs) - パック所有権のストレージ
- [FeatureGate.cs](Assets/_Common/Scripts/IAP/FeatureGate.cs) - ロック/アンロックロジック
- [README.md](README.md) - IAPフローの詳細ドキュメント（adbデバッグコマンド含む）
- [.github/copilot-instructions.md](.github/copilot-instructions.md) - 追加の開発ガイドライン

## 参照禁止

[Assets/Games/01_MakeBubbles](Assets/Games/01_MakeBubbles) は品質が低いとされています - 新規実装の参考にしないでください。
