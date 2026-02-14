# プロジェクト指示書

## プロジェクト概要

- **Unity 6（6000.3.0f1）**: 0～3歳児向けの「タップのみ」ミニゲーム集
- **ターゲット**: Android / iOS
- **パッケージ名**: `com.iw.wakuwaku.touchhiroba`
- **パッケージ定義**: [Packages/manifest.json](Packages/manifest.json)

### 既存ミニゲーム一覧

| # | シーン名 | 概要 |
|---|---------|------|
| 1 | MakeBubbles | 少年をタップしてシャボン玉を発射し、シャボン玉をタップして割る |
| 2 | WhackAMole | 地面から出てくるモグラをタップして叩く |
| 3 | FlowerBlooming | プランターをタップすると花が咲く |
| 4 | Cook | 鍋とフライパンをタップすると料理が飛び出す |
| 5 | TouchTheStar | 星をタッチすると弾ける |
| 6 | PianoAndViolin | ピアノやバイオリンをタップして音を鳴らす |
| 7 | CatchInsects | 虫をタップして捕まえる |
| 8 | PopBalloons | 風船をタップして割る |
| 9 | Fishing | 魚をタップして釣り上げる |
| 10 | VegetableDig | 土の中の野菜をタップして引き抜く（時々レア野菜が出現） |
| 11 | EggHatch | たまごをタップしてヒビを入れて孵化（時々レア動物が出現） |
| 12 | Fireworks | 花火を打ち上げる |
| 13 | FossilDigging | 化石を発掘する |
| 14 | AnimalVoice | 動物をタップすると鳴き声が聞こえる（背景が朝→昼→夜と変化、レア動物も出現） |

### ゲーム設計ガイドライン（0～3歳児向け）

- シンプルにタップだけで遊べて、難しいルールは不要
- スコア等のUIは不要
- 文字やテキストも不要（読めないため）
- 視覚的にわかりやすく、ポップなデザイン
- 子供が好きなテーマを採用
- タップしたら何かが起きて、飽きさせない工夫を入れる（レア要素など）

---

## プロジェクトアーキテクチャ

### フォルダ構成

- 各ゲームは `Assets/Games` 配下の `NN_ゲーム名` フォルダ（例: `Assets/Games/05_TouchTheStar`）
- 共通ロジックは `Assets/_Common`（ゲーム一覧・IAP等）
- `.csproj/.sln` はUnityが自動生成するため編集しない（編集は `Assets/**/*.cs` が中心）

### ゲーム管理システム

全ゲームは**単一の情報源**に登録: `Assets/_Common/Scripts/GameInfo.cs`

各ゲームエントリは以下を定義:
- `sceneName`: シーン識別子（例: "TouchTheStar"）
- `displayOrder`: ゲーム番号（1から開始）
- `packID`: IAPパック識別子（例: "pack_free", "pack_01"）

メニューシステム（`Assets/Games/00_Menu/Scripts/MenuController.cs`）は動的に:
1. `GameInfo.allGames` を読み込む
2. `{displayOrder:D2}_{sceneName}` という名前のGameObjectを検索（例: "05_TouchTheStar"）
3. `GameButton` コンポーネントをアタッチし、シーン/パック参照を設定
4. IAP権利に基づいてロック状態を更新

### 入力システム

**重要**: このプロジェクトは **Unityの新Input System**（`UnityEngine.InputSystem`）を使用。

**使用禁止**:
- `Input.GetMouseButtonDown()` / `Input.touchCount` / `UnityEngine.Input` クラス全般

**代わりに使用**:
- マルチタッチ: `Touchscreen.current.touches`
- マウス入力: `Mouse.current`
- 参考例: `Assets/Games/07_CatchInsects/Scripts/InsectController.cs`

**マルチタッチ要件**: ゲームは2箇所以上の同時タップに対応する必要があります。

### 効果音アーキテクチャ

各ゲームは専用のSFXPlayerコンポーネントを持つ:
- 命名規則: `{ゲーム名}SFXPlayer`（例: `TouchTheStarSFXPlayer`）
- 効果音ごとの音量個別制御が可能
- ゲームのオーディオクリップ参照を一元管理

### ゲーム初期化パターン

ゲームはInitializerコンポーネントでシーン読み込み時に自動セットアップ:
- 例: `Assets/Games/05_TouchTheStar/Scripts/TouchTheStarInitializer.cs`
- EventSystem、SFXPlayer、背景、ゲームオブジェクトを生成
- プログラムで参照を配線

### 外部ライブラリ

- **DOTween**: トゥイーン/アニメーション
- **Cartoon FX Remaster** (CFXR): パーティクルエフェクト（`Assets/JMO Assets/Cartoon FX Remaster`）

### 主要Unityパッケージ

- `com.unity.inputsystem` (1.16.0) - 新Input System
- `com.unity.purchasing` (5.0.1) - IAP
- `com.unity.render-pipelines.universal` (17.3.0) - URP

---

## 新規ゲーム実装の基本ルール

1. **分離**: 指示がある場合を除き、既存ゲームのソースコードを参考にしない（ゲーム間の結合度を下げる）
2. **テンプレートベース**: 指示がない場合は `Assets/Games/_GameTemplate` をコピーして作成
3. **プレースホルダーアセット**: テンプレートの `work_*.png` と `work_sfx*.mp3` を最初に使用
4. **セットアップ自動化**: ユーザー操作を極限まで減らすため、Editorスクリプトでオブジェクト配置/アタッチ/必須参照設定まで自動化。セットアップスクリプトはユーザーが手動で実行する形にする
5. **Scene View編集可能**: 背景画像、固定位置でスポーンされるキャラクター、装飾オブジェクトなどは、Scene内に配置してScene Viewで直感的に調整できる設計を優先する
6. **マルチタッチ**: 画面上の2か所以上を同時タップしても反応する作りにする
7. **メニュー配線なし**: メニュー画面との遷移配線はユーザーが手動で実装する前提のため、AIは勝手に実装しない
8. **SFXPlayer**: 効果音は「【ゲーム名】+ SFXPlayer」命名でゲームごとに一元管理。効果音ごとにボリュームを個別設定できるようにする
9. **ビルド確認**: 新規実装時や大きく実装を直した後は、必ずコンパイルエラーがないことを確認してから作業を完了すること

---

## 実装上の注意点

### SpriteRendererのソート順設定

```csharp
SpriteRenderer sr = gameObject.AddComponent<SpriteRenderer>();
sr.sortingOrder = 10;  // 背景より前面に表示
```

**推奨値**: 背景: `-100` / ゲームオブジェクト: `0`〜`50` / UI・エフェクト: `50`〜`100` / フェードオーバーレイ: `100`以上

### Unity Editorのコンパイルタイミング

**重要**: Unity Editorは**バックグラウンドにある間はスクリプトをコンパイルしません**。ファイル編集後はUnity Editorにフォーカスを当て、コンパイル完了まで待機すること。

### .metaファイルの必要性

画像ファイルを`Assets/`にコピーしただけでは認識されない。`work/images/generate_meta.py`で`.meta`ファイルを生成するか、Unity Editorにフォーカスを当てて自動生成させる。

### 既存シーンでの変更反映

新しいPrefabを作成した場合、既存シーンには反映されない。`Tools → Setup [GameName] Game` で新規作成するか、`Tools → Apply [GameName] Assets` で再適用。

---

## 参考実装

### 推奨参考ゲーム

- **AnimalVoice** (`Assets/Games/14_AnimalVoice`) - 最新の実装パターン、自動化スクリプト完備
- **TouchTheStar** (`Assets/Games/05_TouchTheStar`) - シンプルなタップゲームの好例
- **CatchInsects** (`Assets/Games/07_CatchInsects`) - マルチタッチの実装例

### 参照禁止

`Assets/Games/01_MakeBubbles` は品質が低いとされています。新規実装の参考にしないでください。

---

## テスト

- 自動テスト基盤はありません。検証はUnity EditorのPlayモードとConsoleログで実施
- **新規実装時や大きく実装を直した後は、必ずコンパイルエラーがないことを確認してから作業を完了すること**

---

## 重要なファイル一覧

| ファイル | 役割 |
|---------|------|
| `Assets/_Common/Scripts/GameInfo.cs` | ゲームの中央レジストリ |
| `Assets/Games/00_Menu/Scripts/MenuController.cs` | 動的メニュー生成 |
| `Assets/_Common/Scripts/IAP/IAPManager.cs` | IAPシステム初期化 |
| `Assets/_Common/Scripts/IAP/EntitlementStore.cs` | パック所有権のストレージ |
| `Assets/_Common/Scripts/IAP/FeatureGate.cs` | ロック/アンロックロジック |
| `work/images/makeImages.py` | 画像生成スクリプト |
| `work/sfx/makeSFX.py` | 効果音生成スクリプト |

---

## スキル一覧（詳細ワークフロー）

以下の詳細手順・リファレンスは `.github/skills/` にスキルとして格納されています。
必要に応じてオンデマンドで参照されます。

| スキル | 内容 | 使用場面 |
|--------|------|---------|
| `new-game` | 新規ゲームの作成手順（テンプレートコピー〜動作確認） | ゲーム追加時 |
| `generate-images` | OpenAI APIでの画像生成ワークフロー（スプライトシート含む） | 画像アセット作成時 |
| `generate-sfx` | ElevenLabs APIでの効果音生成ワークフロー | 効果音作成時 |
| `unity-debug` | Unity自動テスト・トラブルシューティング | デバッグ・動作確認時 |
| `iap-system` | IAP課金システムのアーキテクチャとAndroidデバッグ | 課金機能の実装・調査時 |
