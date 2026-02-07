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

### 新規ゲーム実装の完全な手順

#### ステップ1: テンプレートのコピーとスクリプト作成

1. `Assets/Games/_GameTemplate`フォルダをコピー → `NN_ゲーム名`にリネーム
2. 仕様書を作成（オプション）: `Assets/Games/NN_ゲーム名/ゲーム名_Specification.md`
3. ゲームロジックのスクリプトを`Scripts/`フォルダに作成:
   - `{ゲーム名}Initializer.cs` - ゲーム全体の初期化
   - `{ゲーム名}Controller.cs` - メインゲームロジック
   - `{ゲーム名}SFXPlayer.cs` - 効果音管理
   - その他必要なスクリプト

#### ステップ2: Editorセットアップスクリプトの作成

`Scripts/Editor/`フォルダに以下のスクリプトを作成:

**{ゲーム名}Setup.cs** - 初回シーン作成用:
```csharp
[MenuItem("Tools/Setup {ゲーム名} Game")]
public static void SetupGame()
{
    // 1. 新しいシーンを作成
    // 2. カメラ設定
    // 3. Initializerオブジェクトを作成
    // 4. プレースホルダーアセットを自動設定
    // 5. シーンを保存
    // 6. Build Settingsにシーンを追加
}
```

**{ゲーム名}ApplyAssets.cs** - アセット再適用用:
```csharp
[MenuItem("Tools/Apply {ゲーム名} Assets")]
public static void ApplyAssets()
{
    // 1. シーンを開く
    // 2. Initializerを検索
    // 3. SerializedObjectでアセット参照を設定
    // 4. シーンを保存
}
```

#### ステップ3: 画像アセットの生成

1. `makeImage/makeImages.py`に画像生成ジョブを追加
2. 必要な画像リストを作成（背景、キャラクター、UI要素など）
3. OpenAI APIで生成: `python makeImages.py`
4. 生成された画像を`Assets/Games/NN_ゲーム名/Sprites/`にコピー
5. .metaファイルを生成: `python generate_meta.py`

#### ステップ4: Unity Editorでのセットアップ

1. Unity Editorを開く（既に開いている場合はフォーカスを当てる）
2. スクリプトのコンパイル完了を待つ（数秒）
3. `Tools → Setup {ゲーム名} Game`を実行
4. Unity Consoleでエラーがないか確認

#### ステップ5: 動作確認

手動テスト:
1. Unity Editorの**Playボタン**を押す
2. ゲームが正しく動作するか確認
3. 問題があればUnity Consoleのログを確認

自動テスト（推奨）:
1. Unity Editorを開いた状態で実行:
```bash
cd makeImage
python restart_unity_test.py
```
2. 生成されたスクリーンショットを確認
3. 問題があればUnity Consoleのログを確認

#### ステップ6: デバッグと修正

よくある問題と解決方法:

**問題**: オブジェクトが表示されない
- **原因**: sortingOrderが設定されていない、またはスプライトがnull
- **解決**: SpriteRendererにsortingOrderを設定（推奨値: 10）
- **確認**: Unity Consoleのログでスプライトのロード状況を確認

**問題**: タップが反応しない
- **原因**: Colliderが設定されていない、またはEventSystemがない
- **解決**: CircleCollider2DまたはBoxCollider2Dを追加、EventSystemが存在するか確認

**問題**: アセット参照がnull
- **原因**: .metaファイルが生成されていない、またはパスが間違っている
- **解決**: `generate_meta.py`を実行、AssetDatabase.LoadAssetAtPathのパスを確認

**問題**: Unity Editorが変更を認識しない
- **原因**: Unity Editorがバックグラウンドにある
- **解決**: Unity Editorにフォーカスを当てて、コンパイルが完了するまで待つ

#### ステップ7: アセットの本番化（オプション）

初回実装では低品質（`quality="low"`）の画像を使用します。動作確認後、高品質版を生成:

1. `makeImages.py`の該当ジョブで`quality="standard"`に変更
2. 再度生成: `python makeImages.py`
3. 既存の画像を上書き
4. Unity Editorで`Tools → Apply {ゲーム名} Assets`を実行

### 新規ゲーム設計ガイドライン

0～3歳児向け:
- タップのみの操作（複雑なジェスチャーなし）
- スコア/UIテキストなし（読めないため）
- タップごとに視覚的フィードバック
- シンプルで繰り返しの仕組み
- レア/サプライズ要素で飽きさせない
- 明るくカラフルな見た目

## 画像アセット生成ワークフロー

### OpenAI Image Generation APIを使用した素材作成

**スクリプトの場所**: [makeImage/makeImages.py](makeImage/makeImages.py)

#### 使用方法

1. **画像生成ジョブの定義** - `makeImages.py`の`jobs`配列に生成したい画像を追加:
```python
jobs = [
    {
        "out": r"out\GameName\sprite_name.png",
        "prompt": "カラフルでポップな2Dイラスト、太めの黒アウトライン...",
    },
]
```

2. **生成実行**:
```bash
cd makeImage
python makeImages.py
```

3. **Unityへのコピー** - 生成された画像を`Assets/Games/NN_GameName/Sprites/`にコピー

4. **.metaファイル生成** - [makeImage/generate_meta.py](makeImage/generate_meta.py)を使用:
```bash
cd makeImage
python generate_meta.py
```

#### 重要な注意点

- **レート制限**: OpenAI APIは20画像/分の制限があります。`makeImages.py`は自動的に3.5秒の遅延を入れて対応
- **品質設定**: デフォルトは`quality="low"`（コスト削減のため）。高品質版が必要な場合は後で再生成
- **プロンプト保存**: 全てのプロンプトは`makeImages.py`に保存されているため、高品質で再生成可能
- **背景透過**: キャラクター/オブジェクトのプロンプトには必ず「背景透過」を含める
- **アートスタイル統一**: ゲーム内の全素材で一貫したスタイルを使用（例: "カラフルでポップな2Dアイコン、太めの黒アウトライン"）

## Unity自動化とデバッグ

### Unity Editor自動操作スクリプト

Unity EditorをPythonから自動操作してゲームの動作確認を行うスクリプトを用意しています。

#### restart_unity_test.py

**場所**: [makeImage/restart_unity_test.py](makeImage/restart_unity_test.py)

**機能**:
1. Unity Editorを検索してフォーカス
2. 現在のPlay modeを停止（Ctrl+P）
3. スクリプトのコンパイル待機
4. トリガーファイルを作成して自動テストを開始
5. 5秒後にスクリーンショットを撮影
6. スクリーンショットを自動的に開く

**使用方法**:
```bash
cd makeImage
python restart_unity_test.py
```

**出力**: `C:\dev\iwaki001\screenshot_animalvoice.png`にスクリーンショットが保存される

#### auto_test_unity.py

**場所**: [makeImage/auto_test_unity.py](makeImage/auto_test_unity.py)

**機能**: 既に開いているUnity Editorでトリガーファイルを作成し、自動テストを開始（Play mode停止は行わない）

### Unity Editor自動テストシステム

#### AnimalVoiceAutoTest.cs

**場所**: [Assets/Games/14_AnimalVoice/Scripts/Editor/AnimalVoiceAutoTest.cs](Assets/Games/14_AnimalVoice/Scripts/Editor/AnimalVoiceAutoTest.cs)

**機能**:
- トリガーファイル（`C:\dev\iwaki001\trigger_autotest.txt`）を監視
- ファイル検出時に自動的にPlay modeを開始
- 5秒後にスクリーンショットを撮影して停止

**手動実行**: Unity Editor → `Tools → Auto Test AnimalVoice`

#### AnimalVoiceDebugCapture.cs

**場所**: [Assets/Games/14_AnimalVoice/Scripts/AnimalVoiceDebugCapture.cs](Assets/Games/14_AnimalVoice/Scripts/AnimalVoiceDebugCapture.cs)

**機能**:
- Play mode開始5秒後に自動でスクリーンショットを撮影
- ゲーム状態をUnity Consoleに詳細ログ出力（GameObject数、AnimalController数、Sprite設定など）

### 重要な実装上の注意点

#### Unity Editorのコンパイルタイミング

**重要**: Unity Editorは**バックグラウンドにある間はスクリプトをコンパイルしません**。
- ファイルを編集した後は、Unity Editorにフォーカスを当てる必要があります
- コンパイル完了まで数秒待機してからPlay modeを開始すること

#### SpriteRendererのソート順設定

Unity 2Dゲームでスプライトを正しく表示するには、**sortingOrderを必ず設定**してください:

```csharp
SpriteRenderer sr = gameObject.AddComponent<SpriteRenderer>();
sr.sortingOrder = 10;  // 背景より前面に表示
```

**推奨値**:
- 背景: `-100`
- ゲームオブジェクト（キャラクター、アイテム等）: `0` ～ `50`
- UI要素、エフェクト: `50` ～ `100`
- フェードオーバーレイ: `100` 以上

sortingOrderを設定しないと、デフォルト値`0`になり、背景や他の要素と重なって表示されない場合があります。

#### 既存シーンでの変更反映

Unity Editorスクリプトで新しいPrefabを作成した場合、**既存のシーンには反映されません**。以下のいずれかを実行:

1. **新しいシーンを作成**: `Tools → Setup [GameName] Game`
2. **既存シーンに再適用**: `Tools → Apply [GameName] Assets`

#### .metaファイルの必要性

Unityにアセットをインポートするには、`.meta`ファイルが必須です:
- 画像ファイルを`Assets/`フォルダにコピーしただけでは認識されません
- [makeImage/generate_meta.py](makeImage/generate_meta.py)を使用して`.meta`ファイルを生成
- Unityが自動生成することもありますが、Editorにフォーカスを当てる必要があります

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

## トラブルシューティング

### Unity Editorが応答しない

**症状**: スクリプト変更が反映されない、コンパイルが完了しない

**解決方法**:
1. Unity Editorウィンドウにフォーカスを当てる
2. 数秒待ってコンパイルが完了するのを確認（下部のプログレスバー）
3. Unity Consoleでコンパイルエラーを確認

### 画像生成APIのレート制限エラー

**症状**: `Rate limit reached for gpt-image... Limit 20, Used 20`

**解決方法**:
- `makeImages.py`は自動的に3.5秒の遅延を入れています
- 手動で実行する場合は、20画像ごとに1分待機してください

### Unity Consoleログの確認方法

**ログファイルの場所**: `C:\Users\{ユーザー名}\AppData\Local\Unity\Editor\Editor.log`

**コマンドラインから確認**:
```bash
# 最新100行を表示
tail -100 "C:\Users\{ユーザー名}\AppData\Local\Unity\Editor\Editor.log"

# 特定のキーワードで検索
grep -i "error\|exception" "C:\Users\{ユーザー名}\AppData\Local\Unity\Editor\Editor.log"
```

### スクリーンショットが撮影されない

**原因**:
1. Unity Editorがバックグラウンドにある → フォーカスを当てる
2. トリガーファイルが検出されていない → Unity Editorを再起動
3. AnimalVoiceAutoTest.csがコンパイルされていない → Unity Consoleでエラー確認

## 参考実装

### 推奨参考ゲーム

以下のゲームは実装品質が高く、参考にすることを推奨:

- **AnimalVoice** ([Assets/Games/14_AnimalVoice](Assets/Games/14_AnimalVoice)) - 最新の実装パターン、自動化スクリプト完備
- **TouchTheStar** ([Assets/Games/05_TouchTheStar](Assets/Games/05_TouchTheStar)) - シンプルなタップゲームの好例
- **CatchInsects** ([Assets/Games/07_CatchInsects](Assets/Games/07_CatchInsects)) - マルチタッチの実装例

### AnimalVoiceゲームの構成

**場所**: [Assets/Games/14_AnimalVoice](Assets/Games/14_AnimalVoice)

**主要スクリプト**:
- [AnimalVoiceInitializer.cs](Assets/Games/14_AnimalVoice/Scripts/AnimalVoiceInitializer.cs) - ゲーム初期化、動的なScriptableObject生成
- [AnimalController.cs](Assets/Games/14_AnimalVoice/Scripts/AnimalController.cs) - 動物の個別制御、タップ検出
- [AnimalSpawner.cs](Assets/Games/14_AnimalVoice/Scripts/AnimalSpawner.cs) - 動物のスポーン管理
- [BackgroundTimeManager.cs](Assets/Games/14_AnimalVoice/Scripts/BackgroundTimeManager.cs) - 時間帯の切り替え

**Editorスクリプト**:
- [AnimalVoiceSetup.cs](Assets/Games/14_AnimalVoice/Scripts/Editor/AnimalVoiceSetup.cs) - 初回シーン作成
- [AnimalVoiceApplyAssets.cs](Assets/Games/14_AnimalVoice/Scripts/Editor/AnimalVoiceApplyAssets.cs) - アセット再適用
- [AnimalVoiceAutoTest.cs](Assets/Games/14_AnimalVoice/Scripts/Editor/AnimalVoiceAutoTest.cs) - 自動テスト

**自動化スクリプト**:
- [makeImage/makeImages.py](makeImage/makeImages.py) - 43枚の画像を生成（背景3枚 + 動物40枚）
- [makeImage/generate_meta.py](makeImage/generate_meta.py) - .metaファイル生成
- [makeImage/restart_unity_test.py](makeImage/restart_unity_test.py) - Unity自動テスト
- [makeImage/auto_test_unity.py](makeImage/auto_test_unity.py) - シンプル版自動テスト

**特徴**:
- 完全にプログラムで生成されるゲームオブジェクト階層
- ScriptableObjectを動的に作成してデータ管理
- DOTweenを使用したアニメーション
- マルチタッチ対応
- レア要素（10%確率）
- 時間帯による背景・動物の切り替え（30秒ごと）

## 参照禁止

[Assets/Games/01_MakeBubbles](Assets/Games/01_MakeBubbles) は品質が低いとされています - 新規実装の参考にしないでください。
