# FruitSlice ゲーム メンテナンスドキュメント

> **対象**: このゲームを今後メンテナンスするAIエージェント向け  
> **最終更新**: 2026-02-09

---

## ゲーム概要

まな板の上のフルーツをタップして切り、お皿に盛り付けるミニゲーム。0〜3歳児向け。  
フルーツは **丸ごと → カット（左右に割れる） → お皿に盛り付け → 消滅 → 新フルーツ出現** の3状態で遷移する。  
10%の確率でレアフルーツ（金のリンゴ・レインボーマンゴー・ダイヤモンドオレンジ）が出現する。

---

## フォルダ構成

```
Assets/Games/15_FruitSlice/
├── FruitSlice_GameDoc.md          # 本ファイル
├── Scenes/FruitSlice.unity
├── Scripts/
│   ├── FruitData.cs               # Enum + ScriptableObject定義
│   ├── FruitSlotController.cs     # スロット1個の状態遷移（タップ検出・アニメーション）
│   ├── FruitSpawnManager.cs       # 重み付きランダム選択 + レア判定（シングルトン）
│   ├── FruitSpawnPoint.cs         # Editモード配置可能なスポーンポイント
│   ├── FruitSliceSFXPlayer.cs     # 効果音管理（シングルトン）
│   ├── FruitSliceInitializer.cs   # ゲーム全体初期化（Awakeで実行）
│   └── Editor/
│       ├── FruitSliceSetup.cs        # 初回シーン作成 (Tools → Setup FruitSlice Game)
│       ├── FruitSliceApplyAssets.cs   # アセット再適用 (Tools → Apply FruitSlice Assets)
│       └── FruitSliceAutoTest.cs      # 自動テスト (Tools → Auto Test FruitSlice)
├── Sprites/   # 11フルーツ×3状態 + 背景1枚 = 34本番画像 (+テンプレwork_*3枚)
└── Audios/    # cut_sfx, plate_sfx, complete_sfx, rare_sfx, spawn_sfx (+仮work_sfx1〜8)
```

---

## フルーツ一覧

| 区分 | フルーツ | FruitType enum | スプライトファイル名 |
|------|---------|----------------|---------------------|
| 通常 (8種) | リンゴ | Apple | apple_whole/cut/plated |
| | オレンジ | Orange | orange_whole/cut/plated |
| | 桃 | Peach | peach_whole/cut/plated |
| | パイナップル | Pineapple | pineapple_whole/cut/plated |
| | スイカ | Watermelon | watermelon_whole/cut/plated |
| | 洋梨 | Pear | pear_whole/cut/plated |
| | キウイ | Kiwi | kiwi_whole/cut/plated |
| | レモン | Lemon | lemon_whole/cut/plated |
| レア (3種) | 金のリンゴ | GoldenApple | golden_apple_whole/cut/plated |
| | レインボーマンゴー | RainbowMango | rainbow_mango_whole/cut/plated |
| | ダイヤモンドオレンジ | DiamondOrange | diamond_orange_whole/cut/plated |

各フルーツにつき3枚のスプライト:
- `_whole.png` — 丸ごとの状態
- `_cut.png` — 左右に2つに割れた状態（断面が見える）
- `_plated.png` — お皿に盛り付けられた状態（俯瞰視点）

画像サイズ: スプライトは **256×256**、背景(`bg_cutting_board.png`)は **1536×1024**

---

## 状態遷移フロー（1スロットあたり）

```
[Empty] → SpawnFruit() → [Whole]（フェードイン）
                              │ タップ
                              ▼
                         [Cut]（左右に割れるアニメ + カット効果音）
                              │ cutDisplayDuration(0.5秒)後
                              ▼
                         [Plated]（回転アニメ + 盛り付けファンファーレ）
                              │ platedDisplayDuration(1.0秒)後
                              ▼
                         [Empty]（フェードアウト → GetRandomFruit() → リスポーン）
```

---

## スクリプト詳細

### FruitSliceInitializer.cs — 初期化の起点

`Awake()` で `InitializeGame()` を呼び、以下を順番に作成:

1. **EventSystem** — InputSystemUIInputModule付き
2. **FruitSliceSFXPlayer** — 効果音一元管理
3. **Background** — `Background_CuttingBoard`を検索、なければ生成(sortingOrder=-100, scale 10×10)
4. **FruitSpawnManager** — フルーツデータを動的ScriptableObjectとして生成・登録
5. **FruitSlots** — SpawnPointの位置にスロットを生成、初期フルーツをスポーン

全てのフルーツデータは `ScriptableObject.CreateInstance<FruitSliceData>()` でメモリ上に動的生成される（アセットファイルとしては保存しない）。

#### Inspector設定可能フィールド

| フィールド | デフォルト値 | Range | 説明 |
|-----------|------------|-------|------|
| rareSpawnChance | 0.1 (10%) | 0–1 | レアフルーツの出現確率 |
| defaultColliderRadius | 1.0 | 0.5–3 | タップ判定の半径 |
| defaultFruitScale | 1.2 | 0.5–3 | フルーツの表示スケール |
| spawnPoints | List (空) | — | 空ならシーン内を自動検索 |

**スプライトフィールド**: 通常8種×3状態=24 + レア3種×3状態=9 + 背景1 = **34スプライト**  
**効果音フィールド**: cutSound, plateSound, completeSound, rareSound, spawnSound = **5クリップ**

### FruitSlotController.cs — タップ検出と状態遷移

- **マルチタッチ対応**: `Touchscreen.current.touches` でループ + エディタ用にMouse入力も対応
- **状態enum**: `Empty`, `Whole`, `Cut`, `Plated`

| パラメータ | デフォルト | 説明 |
|-----------|----------|------|
| cutDisplayDuration | 0.5秒 | カット状態の表示時間 |
| platedDisplayDuration | 1.0秒 | 盛り付け状態の表示時間 |
| fadeOutDuration | 0.3秒 | フェードアウト時間 |
| fadeInDuration | 0.3秒 | フェードイン時間 |

**アニメーション詳細** (DOTween使用):
- **スポーン**: フェードイン + レア時は虹色キラキラ（DOColorの無限ループ）
- **カット**: スケールバウンス(×1.3) + カット効果音。レア時はカメラシェイク(0.3秒, 0.08強度)
- **盛り付け**: 360度回転(0.3秒) + 盛り付けファンファーレ
- **消滅→リスポーン**: フェードアウト後に `spawnManager.GetRandomFruit()` で新フルーツ

### FruitSpawnManager.cs — フルーツ選択ロジック

- `Random.value < rareChance` でレア判定（デフォルト10%）
- レア → 均等ランダム（3種から）
- 通常 → `spawnWeight` による重み付きランダム（通常: 1.0、レア: 0.5）

### FruitSpawnPoint.cs — Editモード配置

- `[ExecuteAlways]` でSceneビューにプレビュー表示（半透明スプライト）
- `overrideScale` (Range 0–5): 0ならデフォルト、>0なら個別スケール指定
- `overrideColliderRadius` (Range 0–5): 0ならデフォルト、>0なら個別指定
- Gizmos表示: 非選択=オレンジWireSphere、選択=黄WireSphere+ラベル

### FruitSliceSFXPlayer.cs — 効果音

| 効果音 | メソッド | デフォルト音量 | 説明 |
|--------|---------|--------------|------|
| カット | PlayCut() | 0.7 | フルーツを切る「ザクッ」音 |
| 盛り付け | PlayPlate() | 0.6 | お皿に盛り付けるファンファーレ |
| 完成 | PlayComplete() | 0.8 | タスク完成ジングル |
| レア | PlayRare() | 0.9 | レアフルーツ出現キラキラ |
| スポーン | PlaySpawn() | 0.3 | フルーツ出現ポップ音 |

---

## 重要な数値定数

| 定数 | 値 | 使用箇所 |
|------|-----|---------|
| sortingOrder: 背景 | -100 | Initializer |
| sortingOrder: フルーツスロット | 10 | Initializer |
| カメラ orthographicSize | 5 | Setup.cs |
| カメラ位置 | (0, 0, -10) | Setup.cs |
| カメラ背景色 | (0.6, 0.45, 0.3) 茶色 | Setup.cs |
| 背景スケール | (10, 10, 1) | Initializer |
| レア出現確率 | 10% | Initializer (rareSpawnChance) |
| 通常フルーツ spawnWeight | 1.0 | Initializer |
| レアフルーツ spawnWeight | 0.5 | Initializer |
| スクリーンショット撮影遅延 | 5秒 | AutoTest |

---

## スクリプト間の依存関係

```
FruitSliceInitializer（起点・Awakeで実行）
  ├─→ FruitSliceSFXPlayer（singleton作成 + clip設定）
  ├─→ FruitSpawnManager（singleton作成 + normalFruits/rareFruits/rareChance設定）
  ├─→ FruitSpawnPoint[]（シーン内から自動検出）
  └─→ FruitSlotController[]（各SpawnPoint位置に生成）

FruitSlotController
  ├─→ FruitSpawnManager.Instance.GetRandomFruit()（リスポーン時）
  └─→ FruitSliceSFXPlayer.Instance.Play*()（各状態遷移時）
```

---

## Editorスクリプトの操作

| メニューコマンド | 用途 | 使うタイミング |
|-----------------|------|--------------|
| Tools → Setup FruitSlice Game | シーン新規作成+全オブジェクト配置 | 初回のみ |
| Tools → Apply FruitSlice Assets | 既存シーンにアセット参照を再適用 | 画像・音声差替え後 |
| Tools → Auto Test FruitSlice | Play mode開始+5秒後スクリーンショット | 動作確認 |

### 自動テストのトリガーファイル

Python等からUnity Editorを制御する場合:
- `C:/dev/iwaki001/trigger_autotest_fruitslice.txt` を作成 → Play modeテスト開始
- `C:/dev/iwaki001/trigger_apply_and_test_fruitslice.txt` を作成 → ApplyAssets + テスト開始
- スクリーンショット出力先: `C:/dev/iwaki001/screenshot_fruitslice.png`

---

## 画像アセットの生成・更新方法

### 画像生成スクリプト

スクリプト: `makeImage/makeImages_fruitslice.py`

- 通常フルーツ8種×3状態 + レア3種×3状態 + 背景1枚 = **34枚**
- 品質: `quality="medium"` (本番用)、`quality="low"` (テスト用)
- 背景サイズ: 1536×1024、フルーツサイズ: 1024×1024（→リサイズで256×256に縮小）
- バッチサイズ: mediumなら10枚ずつ、lowなら15枚ずつ

### 画像リサイズ

生成後、フルーツスプライト(1024×1024)を256×256にリサイズする:
```bash
cd makeImage
python -c "
from PIL import Image; import pathlib
for f in pathlib.Path('out/FruitSlice').glob('*.png'):
    if 'bg_' not in f.name:
        Image.open(f).resize((256,256), Image.Resampling.LANCZOS).save(f, 'PNG')
"
```

### Unity反映手順

1. `makeImage/out/FruitSlice/` の画像を `Assets/Games/15_FruitSlice/Sprites/` にコピー
2. `.meta` ファイルがない場合は `makeImage/generate_meta_fruitslice.py` で生成
3. Unity Editorにフォーカスを当てる（コンパイル待機）
4. `Tools → Apply FruitSlice Assets` を実行

### 画像プロンプトの設計方針

- **カット状態**: 全フルーツ共通で「真ん中で縦に半分に切られた、左右に2つに分かれた状態、2つのピースが少し離れている、断面がはっきり見える」
- **盛り付け状態**: 「白い丸皿の上にきれいに並べられている、俯瞰視点」
- **白色→アイボリー**: リンゴ・桃・梨の果肉は「アイボリー色」と指定（透過防止）
- **アートスタイル**: 「カラフルでポップな2Dイラスト、太めの黒アウトライン、背景透過、子供向けゲーム用」で統一

---

## 効果音の生成・更新方法

### 効果音生成スクリプト

スクリプト: `work/sfx/makeSFX.py` (FruitSliceセクション)

| ファイル | 内容 | duration | prompt_influence |
|---------|------|----------|-----------------|
| cut_sfx.mp3 | 包丁でフルーツをザクッと切る音 | 0.7秒 | 0.8 |
| plate_sfx.mp3 | 勝利ファンファーレ（タダー！） | 1.2秒 | 0.7 |
| complete_sfx.mp3 | 成功ジングル（ベル＋木琴） | 1.5秒 | 0.6 |
| rare_sfx.mp3 | 魔法のキラキラシマー音 | 1.5秒 | 0.6 |
| spawn_sfx.mp3 | 柔らかいポップ音 | 0.5秒 | 0.7 |

生成コマンド:
```bash
cd work/sfx
python makeSFX.py
```
→ 自動でUnityのAudiosフォルダにデプロイ + .meta生成される

---

## デフォルトSpawnPoint配置

Setupスクリプトが初回に配置する5箇所:

```
(-2.8, 1.8)          (2.8, 1.8)
           (0, 0)
(-2.8, -2.0)         (2.8, -2.0)
```

SpawnPointはSceneビューでドラッグして自由に移動可能。  
各SpawnPointで `overrideScale` / `overrideColliderRadius` を個別設定可能（0=デフォルト使用）。

---

## GameInfo登録情報

- **sceneName**: `"FruitSlice"`
- **displayOrder**: `15`
- **packID**: `"pack_01"`

---

## 既知の注意点

### 画像生成関連
- **白色→透過問題**: 画像生成AIで純白が透過扱いされるため、果肉が白い果物（リンゴ・桃・梨）はプロンプトで「アイボリー」と指定している
- **カット画像の表現**: 「左右に2つに割れた状態」を明示しないと、単なる断面図になってしまう
- **並列生成**: LOW品質なら15枚バッチ、MEDIUM品質なら10枚バッチで並列生成可能

### コード関連
- **入力システム**: `UnityEngine.InputSystem`（新Input System）を使用。旧`Input`クラスは使用禁止
- **DOTween依存**: FruitSlotControllerのアニメーション全般がDOTweenに依存
- **フルーツデータはメモリ上のみ**: ScriptableObjectはアセットファイルとして保存せず、`CreateInstance` でランタイム生成
- **SpawnPointの自動検索**: Initializerの`spawnPoints`リストが空の場合、`FindObjectsByType<FruitSpawnPoint>()` でシーン内を自動検索する

### アセット関連
- **画像サイズ**: フルーツスプライトは256×256、背景は1536×1024
- **仮アセット**: `work_bg.png`, `work_sprite_a/b.png`, `work_sfx1〜8.mp3` はテンプレートの仮アセット。本番アセットが無い場合のフォールバックとして使われる
- **.metaファイル**: 新しい画像を追加した場合は `generate_meta_fruitslice.py` で.metaを生成するか、Unity Editorにフォーカスして自動生成させる

### フルーツ選定の経緯
- **除外されたフルーツ**: バナナ・ぶどう・いちご — 「切る」イメージが弱いため除外
- **除外されたレア**: レインボーいちご — カット後の絵の描き分けが難しいため除外
- **採用理由**: 桃・パイナップル・梨・レインボーマンゴー — 切った時の断面が特徴的で、丸ごと→カット→盛り付けの3状態を描き分けやすい
