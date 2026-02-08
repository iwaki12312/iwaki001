# AnimalVoice ゲーム メンテナンスドキュメント

> **対象**: このゲームを今後メンテナンスするAIエージェント向け  
> **最終更新**: 2026-02-07

---

## ゲーム概要

動物をタップすると鳴き声が聞こえるミニゲーム。0〜3歳児向け。  
背景が **朝→昼→夜** と30秒ごとに変化し、時間帯ごとに異なる動物が出現する。  
10%の確率でレア動物（恐竜・ドラゴン・ユニコーン・パンダ）が出現する。

---

## フォルダ構成

```
Assets/Games/14_AnimalVoice/
├── AnimalVoice_Specification.md   # 仕様書
├── AnimalVoice_GameDoc.md         # 本ファイル
├── Scenes/AnimalVoice.unity
├── Scripts/
│   ├── AnimalData.cs              # Enum + ScriptableObject定義
│   ├── AnimalController.cs        # 個別動物の制御（タップ検出・リアクション）
│   ├── AnimalSpawner.cs           # 動物配置管理（シングルトン）
│   ├── AnimalSpawnPoint.cs        # Editモード配置可能なスポーンポイント
│   ├── BackgroundTimeManager.cs   # 時間帯切替管理（シングルトン）
│   ├── AnimalVoiceSFXPlayer.cs    # 効果音管理（シングルトン）
│   ├── AnimalVoiceInitializer.cs  # ゲーム全体初期化（Awakeで実行）
│   ├── AnimalVoiceDebugCapture.cs # デバッグ用スクリーンショット
│   └── Editor/
│       ├── AnimalVoiceSetup.cs       # 初回シーン作成 (Tools → Setup AnimalVoice Game)
│       ├── AnimalVoiceApplyAssets.cs  # アセット再適用 (Tools → Apply AnimalVoice Assets)
│       └── AnimalVoiceAutoTest.cs     # 自動テスト (Tools → Auto Test AnimalVoice)
├── Sprites/   # 20動物×2状態 + 背景3枚 = 43本番画像 (+テンプレwork_*3枚)
├── Audios/    # work_sfx1〜8.mp3（仮効果音。本番音声は未実装）
└── Prefabs/   # ParticleHeart.prefab, ParticleNote.prefab
```

---

## 動物一覧

| 時間帯 | 動物 | スプライトファイル名 |
|--------|------|---------------------|
| 朝 (6種) | ニワトリ、牛、馬、豚、羊、ヤギ | chicken, cow, horse, pig, sheep, goat |
| 昼 (6種) | 犬、猫、ゾウ、ライオン、カエル、ひよこ | dog, cat, elephant, lion, frog, chick |
| 夜 (4種) | フクロウ、オオカミ、コウモリ、ネズミ | owl, wolf, bat, mouse |
| レア (4種) | 恐竜、ドラゴン、ユニコーン、サル | dinosaur, dragon, unicorn, monkey |

各動物につき `{name}_normal.png`（通常）と `{name}_reaction.png`（口を開けた状態）の2枚。

---

## スクリプト詳細

### AnimalVoiceInitializer.cs — 初期化の起点

`Awake()` で `InitializeGame()` を呼び、以下を順番に作成:

1. **EventSystem** — InputSystemUIInputModule付き
2. **AnimalVoiceSFXPlayer** — 効果音一元管理（AudioSource×2: 鳴き声用 + SE用）
3. **Background** — SpriteRenderer(sortingOrder=-100) + FadeOverlay(sortingOrder=100)
4. **AnimalSpawner** — 動物の配置管理。Prefab(非アクティブ)も内部で作成
5. **BackgroundTimeManager** — 時間帯ループ管理
6. **DebugCapture** — Play開始5秒後にスクリーンショット撮影

全ての動物データは `ScriptableObject.CreateInstance<AnimalVoiceData>()` でメモリ上に動的生成される（アセットファイルとしては保存しない）。

#### Inspector設定可能フィールド

| フィールド | デフォルト値 | 説明 |
|-----------|------------|------|
| spawnCount | 6 | 同時表示する動物の数 |
| rareSpawnChance | 0.1 (10%) | レア動物の出現確率 |
| spawnMinX / maxX | -4 / 4 | スポーン範囲X |
| spawnMinY / maxY | -3 / 2 | スポーン範囲Y |
| minDistanceBetweenAnimals | 1.5 | 動物間の最小距離 |
| animalBaseScale | 1.0 | 通常動物のスケール |
| rareAnimalScale | 1.3 | レア動物のスケール |
| colliderRadius | 1.0 | タップ判定の半径 |
| timeChangeInterval | 30秒 | 時間帯切替の間隔 |
| spawnPoints | List | 空ならランダム配置、設定ありなら固定位置 |

### AnimalController.cs — タップ検出とリアクション

- **マルチタッチ対応**: `Touchscreen.current.touches` でループ + エディタ用にMouse入力も対応
- **タップ時の動作**: DOTweenでスケールアップ → reactionSpriteに切替 → パーティクル(ハートor音符) → 鳴き声再生
- **リアクション後の動作**: フェードアウトして削除 → 新しい動物が補充される
- **レア動物の特別演出**: カメラシェイク(0.3秒) + 虹色変化(5色×0.1秒)
- **リアクション時間**: 通常0.8秒、レア1.2秒

### AnimalSpawner.cs — 動物配置管理

- 4つのリスト（朝・昼・夜・レア）で動物データを管理
- 時間帯変更時: 既存動物をフェードアウト → 新しい時間帯の動物をスポーン
- **動物タップ後の補充**: 動物が削除されると、同じ位置に現在の時間帯から別の動物を再スポーン（10%でレア動物）
- **SpawnPointあり**: SpawnPointの位置に固定配置 → タップ後も同じ位置に補充
- **SpawnPointなし**: ランダム位置（最大30回試行で既存動物との距離を確保）→ タップ後も同じ位置に補充
- **重要**: 朝は朝の動物のみ、昼は昼の動物のみ、夜は夜の動物のみで循環（時間帯をまたいだ混在はしない）

### AnimalSpawnPoint.cs — Editモード配置

- `[ExecuteAlways]` でSceneビューにプレビュー表示（半透明スプライト）
- `overrideScale`: 0ならデフォルト、>0なら個別スケール指定
- Gizmos表示: 非選択=緑WireSphere、選択=黄WireSphere+ラベル

### BackgroundTimeManager.cs — 時間帯ループ

- **ループ**: 朝→昼→夜→朝→… （30秒ごと）
- **遷移**: FadeOverlayで0.5秒フェードアウト → 背景切替 + 動物リスポーン → 0.5秒フェードイン
- **依存**: AnimalSpawner.SpawnAnimalsForTimeOfDay() を呼んで動物を入れ替え

### AnimalVoiceSFXPlayer.cs — 効果音

| 効果音 | デフォルト音量 |
|--------|--------------|
| 鳴き声（通常） | 1.0 |
| 鳴き声（レア） | 1.0 |
| タップ音 | 0.4 (sfxVolume * 0.5) |
| 時間切替音 | 0.8 |
| レア出現音 | 0.8 |

---

## 重要な数値定数

| 定数 | 値 | 使用箇所 |
|------|-----|---------|
| sortingOrder: 背景 | -100 | Initializer |
| sortingOrder: 動物 | 10 | Initializer / SpawnPoint |
| sortingOrder: フェードオーバーレイ | 100 | Initializer |
| カメラ orthographicSize | 5 | Setup.cs |
| カメラ位置 | (0, 0, -10) | Setup.cs |
| フェード時間 | 1秒（0.5+0.5） | BackgroundTimeManager |
| パーティクル寿命 | 2秒 | AnimalController |
| 位置探索の最大試行 | 30回 | AnimalSpawner |
| スクリーンショット撮影遅延 | 5秒 | AutoTest / DebugCapture |

---

## スクリプト間の依存関係

```
AnimalVoiceInitializer（起点・Awakeで実行）
  ├─→ AnimalVoiceSFXPlayer（singleton作成 + clip設定）
  ├─→ BackgroundTimeManager（singleton作成 + renderer/sprite/interval設定）
  ├─→ AnimalSpawner（singleton作成 + データ/prefab/config/spawnpoints設定）
  ├─→ AnimalSpawnPoint（シーン内から自動検出）
  └─→ AnimalVoiceDebugCapture（作成のみ）

BackgroundTimeManager
  ├─→ AnimalSpawner.Instance.SpawnAnimalsForTimeOfDay()（時間帯変更時）
  └─→ AnimalVoiceSFXPlayer.Instance.PlayTimeChangeSound()

AnimalSpawner
  └─→ AnimalController.Initialize()（動物生成時）

AnimalController
  └─→ AnimalVoiceSFXPlayer.Instance.PlayVoice()（タップ時）
```

---

## Editorスクリプトの操作

| メニューコマンド | 用途 | 使うタイミング |
|-----------------|------|--------------|
| Tools → Setup AnimalVoice Game | シーン新規作成+全オブジェクト配置 | 初回のみ |
| Tools → Apply AnimalVoice Assets | 既存シーンにアセット参照を再適用 | 画像差替え後 |
| Tools → Auto Test AnimalVoice | Play mode開始+5秒後スクリーンショット | 動作確認 |

### 自動テストのトリガーファイル

Python等からUnity Editorを制御する場合:
- `C:/dev/iwaki001/trigger_autotest.txt` を作成 → Play modeテスト開始
- `C:/dev/iwaki001/trigger_apply_and_test.txt` を作成 → ApplyAssets + テスト開始
- スクリーンショット出力先: `C:/dev/iwaki001/screenshot_animalvoice.png`

---

## 画像アセットの生成・更新方法

### スプライトシート方式（動物）

スクリプト: `makeImage/makeImages_spritesheet.py`

1. 1枚の横長画像（1536×1024）に左=通常、右=リアクションの2体を同時生成
2. 自動で左右に分割 → `{name}_normal.png` / `{name}_reaction.png`
3. 生成後に `count_blocks()` で2コマ検証 → NGなら自動リトライ

```bash
cd makeImage
python makeImages_spritesheet.py   # 20枚生成（10枚×2バッチ、MEDIUM品質）
python deploy_spritesheet.py        # Sprites/ フォルダにコピー
```

### 背景画像

スクリプト: `makeImage/makeImages.py`

```bash
cd makeImage
python makeImages.py               # 3枚生成（HIGH品質、1536×1024横長）
# 手動で Sprites/ フォルダにコピー
```

### Unity反映

画像をSprites/に配置した後:
1. Unity Editorにフォーカスを当てる（コンパイル待機）
2. `Tools → Apply AnimalVoice Assets` を実行

---

## 既知の注意点

### 画像生成関連
- **白色→透過問題**: 画像生成AIで純白が透過扱いされるため、白い動物（羊・ニワトリ・ヤギ・牛・ユニコーン・パンダ）はプロンプトで「アイボリー（淡いクリーム色）」と指定している
- **2コマ問題**: スプライトシートが8コマ等に分割生成されることがある。プロンプトで「必ず2体だけ」を明示し、生成後にブロック数を検証している
- **並列生成**: LOW品質なら20枚同時、MEDIUM品質なら10枚バッチで並列生成可能（逐次3.5秒待機は不要）

### コード関連
- **入力システム**: `UnityEngine.InputSystem`（新Input System）を使用。旧`Input`クラスは使用禁止
- **InitializerのBackgroundTimeManager参照順序**: `CreateBackground()` が先に `BackgroundTimeManager.Instance` を参照するが、その時点ではまだ作成されていない。`CreateBackgroundTimeManager()` で改めてrenderer設定をやり直すことで対応している
- **動物データはメモリ上のみ**: ScriptableObjectはアセットファイルとして保存せず、`CreateInstance` でランタイム生成

### アセット関連
- **音声は仮**: `work_sfx1〜8.mp3` は仮の効果音。本番の動物鳴き声音声は未実装
- **.metaファイル**: 新しい画像を追加した場合は `makeImage/generate_meta.py` で.metaを生成するか、Unity Editorにフォーカスして自動生成させる

---

## デフォルトSpawnPoint配置

Setupスクリプトが初回に配置する6箇所（2行×3列）:

```
(-3, 1.5)   (0, 1.5)   (3, 1.5)
(-3, -1.5)  (0, -1.5)  (3, -1.5)
```

SpawnPointはSceneビューでドラッグして自由に移動可能。
