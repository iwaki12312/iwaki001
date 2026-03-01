# AnimalVoice（動物タッチ）ゲーム仕様書

## 1. ゲーム概要

### 1.1 コンセプト
- **ゲーム名**: AnimalVoice（動物タッチ）
- **ターゲット**: 0〜3歳児
- **操作**: タップのみ（マルチタッチ対応）
- **目的**: 画面に表示される動物をタップすると、その動物の鳴き声が聞こえてかわいくリアクションする

### 1.2 飽きさせない仕掛け
1. **背景の時間変化**: 30秒ごとに 朝→昼→夜→朝... とループ
2. **時間帯で動物が変わる**: 朝・昼・夜で出現する動物が異なる
3. **レア動物**: 10%の確率で恐竜・ドラゴン等のレア動物が出現

### 1.3 画面構成
```
┌─────────────────────────────────┐
│         背景（牧場）            │
│                                 │
│   🐕        🐱        🐮        │
│                                 │
│        🐷        🐔             │
│                                 │
│   🐴        🐘        🐸        │
│                                 │
└─────────────────────────────────┘
```
- 背景: 全画面スプライト（時間帯で切り替え）
- 動物: 画面内にランダム配置（6匹同時表示）

---

## 2. 時間帯システム

### 2.1 時間帯の種類
| 時間帯 | 背景 | 切り替え間隔 |
|--------|------|-------------|
| Morning（朝） | 朝焼けの牧場 | 30秒 |
| Daytime（昼） | 明るい草原 | 30秒 |
| Night（夜） | 星空の牧場 | 30秒 |

### 2.2 切り替え演出
1. フェードオーバーレイが黒にフェードイン（0.5秒）
2. 背景スプライト切り替え + 動物入れ替え
3. フェードオーバーレイが透明にフェードアウト（0.5秒）
4. 切り替え時に効果音再生

---

## 3. 動物データ

### 3.1 時間帯別の動物一覧

#### 朝の動物（6種）
| ID | 動物名 | 鳴き声テキスト |
|----|--------|---------------|
| Chicken | ニワトリ | コケコッコー |
| Cow | 牛 | モーモー |
| Horse | 馬 | ヒヒーン |
| Pig | 豚 | ブーブー |
| Sheep | 羊 | メェー |
| Goat | ヤギ | メェェー |

#### 昼の動物（6種）
| ID | 動物名 | 鳴き声テキスト |
|----|--------|---------------|
| Dog | 犬 | ワンワン |
| Cat | 猫 | ニャー |
| Elephant | ゾウ | パオーン |
| Lion | ライオン | ガオー |
| Frog | カエル | ケロケロ |
| Chick | ひよこ | ピヨピヨ |

#### 夜の動物（4種）
| ID | 動物名 | 鳴き声テキスト |
|----|--------|---------------|
| Owl | フクロウ | ホーホー |
| Wolf | オオカミ | アオーン |
| Bat | コウモリ | キィキィ |
| Hedgehog | ハリネズミ | キュッ |

#### レア動物（4種）- 全時間帯共通、出現確率10%
| ID | 動物名 | 鳴き声テキスト |
|----|--------|---------------|
| Dinosaur | 恐竜 | ガオォォー |
| Dragon | ドラゴン | グォォォ |
| Unicorn | ユニコーン | キラキラ音 |
| Panda | パンダ | クゥーン |

### 3.2 動物データ構造
```
AnimalVoiceData:
  - animalType: enum（動物の種類）
  - animalName: string（表示名）
  - normalSprite: Sprite（通常時のスプライト）
  - reactionSprite: Sprite（リアクション時のスプライト）
  - voiceClip: AudioClip（鳴き声）
  - isRare: bool（レア動物フラグ）
  - reactionDuration: float（リアクション時間、通常0.8秒、レア1.2秒）
  - scaleMultiplier: float（タップ時の拡大率、通常1.2、レア1.3）
```

---

## 4. タップ時の挙動

### 4.1 通常動物のリアクション
1. タップを検出（CircleCollider2Dで判定）
2. 鳴き声SE再生
3. スケールアップアニメーション（1.0 → 1.2、0.15秒、Ease.OutBack）
4. スプライトをリアクション用に切り替え
5. パーティクル生成（ハートまたは音符、50%ずつ）
6. reactionDuration（0.8秒）待機
7. 元のスプライトに戻す
8. スケールを元に戻す（0.2秒、Ease.OutQuad）

### 4.2 レア動物のリアクション（追加演出）
- 通常のリアクションに加えて：
  - カメラシェイク（0.3秒、振幅0.1）
  - スプライト色を虹色に変化（0.5秒）
  - レア出現SE再生

### 4.3 マルチタッチ対応
- `Touchscreen.current.touches`をループして全タッチを処理
- 同時に複数の動物をタップ可能
- 各動物は独立してリアクション

---

## 5. スポーンシステム

### 5.1 配置ルール
- **同時表示数**: 6匹
- **配置範囲**: X: -4〜4, Y: -3〜2
- **最小距離**: 1.5（動物同士が重ならないように）
- **レア出現確率**: 10%

### 5.2 スポーン処理
1. 既存の動物を全てフェードアウト（0.5秒）
2. 現在の時間帯に対応した動物リストを取得
3. 6匹分ループ：
   - 10%の確率でレア動物を選択、それ以外は通常動物からランダム選択
   - 既存位置と1.5以上離れた位置を探索（最大30回試行）
   - 動物を生成してフェードイン（0.5秒）

---

## 6. クラス設計

### 6.1 クラス一覧
| クラス名 | 役割 |
|---------|------|
| AnimalVoiceData | 動物データを保持するScriptableObject |
| AnimalController | 個別の動物の挙動（タップ検出・リアクション） |
| AnimalSpawner | 動物の配置管理（シングルトン） |
| BackgroundTimeManager | 背景の時間変化管理（シングルトン） |
| AnimalVoiceSFXPlayer | 効果音管理（シングルトン） |
| AnimalVoiceInitializer | ゲーム初期化（シーン上に配置） |

### 6.2 Enum定義
```csharp
public enum AnimalVoiceAnimalType
{
    // 朝の動物
    Chicken, Cow, Horse, Pig, Sheep, Goat,
    // 昼の動物
    Dog, Cat, Elephant, Lion, Frog, Chick,
    // 夜の動物
    Owl, Wolf, Bat, Hedgehog,
    // レア動物
    Dinosaur, Dragon, Unicorn, Panda
}

public enum AnimalVoiceTimeOfDay
{
    Morning,  // 朝
    Daytime,  // 昼
    Night     // 夜
}
```

---

## 7. 必要なアセット

### 7.1 スプライト
| 種類 | 枚数 | 備考 |
|------|------|------|
| 背景 | 3枚 | 朝・昼・夜 |
| 通常動物（通常） | 16枚 | 各動物1枚 |
| 通常動物（リアクション） | 16枚 | 各動物1枚 |
| レア動物（通常） | 4枚 | 各動物1枚 |
| レア動物（リアクション） | 4枚 | 各動物1枚 |
| パーティクル | 2枚 | ハート・音符 |
| **合計** | **45枚** | |

### 7.2 効果音
| 種類 | 数 | 備考 |
|------|-----|------|
| 通常動物の鳴き声 | 16種 | 各動物1つ |
| レア動物の鳴き声 | 4種 | 各動物1つ |
| タップ音 | 1種 | ポップ音 |
| 背景切り替え音 | 1種 | 環境音変化 |
| レア出現音 | 1種 | キラキラ音 |
| **合計** | **23種** | |

---

## 8. パラメータ一覧

### 8.1 タイミング
| パラメータ | 値 | 説明 |
|-----------|-----|------|
| 時間帯切り替え間隔 | 30秒 | 朝→昼→夜のループ |
| フェード時間 | 1秒 | 背景切り替え時のフェード |
| リアクション時間（通常） | 0.8秒 | タップ後の演出時間 |
| リアクション時間（レア） | 1.2秒 | レア動物の演出時間 |
| スケールアップ時間 | 0.15秒 | タップ時の拡大アニメーション |
| スケールダウン時間 | 0.2秒 | 元に戻るアニメーション |
| フェードイン/アウト | 0.5秒 | 動物の出現/消失 |

### 8.2 サイズ・位置
| パラメータ | 値 | 説明 |
|-----------|-----|------|
| スポーン範囲X | -4〜4 | 横方向の配置範囲 |
| スポーン範囲Y | -3〜2 | 縦方向の配置範囲 |
| 最小距離 | 1.5 | 動物同士の最小距離 |
| タップ判定半径 | 1.0 | CircleCollider2Dの半径 |
| 拡大率（通常） | 1.2倍 | タップ時のスケール |
| 拡大率（レア） | 1.3倍 | レア動物のスケール |

### 8.3 確率
| パラメータ | 値 |
|-----------|-----|
| レア出現確率 | 10% |
| ハートパーティクル | 50% |
| 音符パーティクル | 50% |

---

## 9. 技術要件

### 9.1 Unity設定
- **Unityバージョン**: Unity 6 (6000.x)
- **カメラ**: Orthographic、Size=5
- **入力システム**: 新Input System（UnityEngine.InputSystem）

### 9.2 外部ライブラリ
- **DOTween**: アニメーション（スケール、フェード、シェイク等）

### 9.3 入力処理
```csharp
// タッチ入力
var touchscreen = Touchscreen.current;
if (touchscreen != null)
{
    foreach (var touch in touchscreen.touches)
    {
        if (!touch.press.wasPressedThisFrame) continue;
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(touch.position.ReadValue());
        // 判定処理
    }
}

// マウス入力（エディタ用）
var mouse = Mouse.current;
if (mouse != null && mouse.leftButton.wasPressedThisFrame)
{
    Vector3 worldPos = Camera.main.ScreenToWorldPoint(mouse.position.ReadValue());
    // 判定処理
}
```

---

## 10. フォルダ構成

```
Assets/Games/14_AnimalVoice/
├── Scenes/
│   └── AnimalVoice.unity
├── Scripts/
│   ├── AnimalData.cs              # Enum + ScriptableObject定義
│   ├── AnimalController.cs        # 個別動物の制御
│   ├── AnimalSpawner.cs           # 動物配置管理
│   ├── BackgroundTimeManager.cs   # 時間変化管理
│   ├── AnimalVoiceSFXPlayer.cs    # 効果音管理
│   ├── AnimalVoiceInitializer.cs  # 初期化（任意）
│   └── Editor/
│       └── AnimalVoiceSetup.cs    # エディタ拡張（任意）
├── Sprites/
│   ├── bg_morning.png
│   ├── bg_daytime.png
│   ├── bg_night.png
│   ├── animal_*.png               # 各動物スプライト
│   └── particle_*.png             # パーティクル
├── Audios/
│   ├── voice_*.mp3                # 各動物の鳴き声
│   ├── sfx_tap.mp3
│   ├── sfx_timechange.mp3
│   └── sfx_rare.mp3
└── Prefabs/
    └── Animal.prefab              # 動物のベースPrefab
```

---

## 11. 処理フロー

### 11.1 ゲーム開始時
```
1. AnimalVoiceInitializerがAwakeで初期化開始
2. EventSystem作成（なければ）
3. AnimalVoiceSFXPlayer作成
4. 背景オブジェクト作成（Background + FadeOverlay）
5. AnimalSpawner作成、動物データ設定
6. BackgroundTimeManager作成
7. 初期時間帯（Morning）の背景設定
8. 朝の動物6匹をスポーン
```

### 11.2 ゲームループ
```
毎フレーム:
  BackgroundTimeManager:
    timer += deltaTime
    if timer >= 30秒:
      TransitionToNextTime()
  
  各AnimalController:
    if タッチ検出 && !isReacting:
      OnTapped()
```

### 11.3 時間帯切り替え
```
1. isTransitioning = true
2. 切り替えSE再生
3. FadeOverlay を黒にフェードイン (0.5秒)
4. 背景スプライト切り替え
5. AnimalSpawner.SpawnAnimalsForTimeOfDay(nextTime)
   - 既存動物フェードアウト
   - 新しい動物6匹をスポーン
6. FadeOverlay を透明にフェードアウト (0.5秒)
7. isTransitioning = false
```

---

## 12. 注意事項

1. **型名の重複回避**: 他ゲームとの競合を避けるため、Enum/クラス名に`AnimalVoice`プレフィックスを付ける
2. **マルチタッチ必須**: 2箇所以上の同時タップに対応すること
3. **DOTween依存**: アニメーションにDOTweenを使用（プロジェクトにインポート済み前提）
4. **シングルトンパターン**: Spawner, TimeManager, SFXPlayerはシングルトンで実装
5. **テキスト不要**: 0〜3歳児向けのため、画面上にテキストは表示しない

---

この仕様書に従って実装すれば、AnimalVoiceゲームを0から再現できます。
