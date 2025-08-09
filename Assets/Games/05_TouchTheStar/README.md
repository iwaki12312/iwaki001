# TouchTheStarゲーム - セットアップガイド

## 概要
TouchTheStarは1〜2歳児向けの星をタッチするシンプルなゲームです。
星が画面上にランダムに出現し、タップすると消えるゲームです。

## 機能
- 星が一定時間ごとにランダムな位置に出現（最大5つまで）
- 星はランダムな方向に移動し、回転します
- 星の色とスプライト（3×3分割）がランダムに選択されます
- タップすると星が消え、効果音が再生されます
- 画面外に出た星は自動的に削除されます

## セットアップ手順

### 1. シーンの準備
1. `Assets/Games/05_TouchTheStar/Scenes/TouchTheStar.unity` を開きます
2. 背景（bg）が既に配置されていることを確認します

### 2. TouchTheStarInitializerの追加
1. Hierarchy上で右クリック → `Create Empty` を選択
2. 作成したGameObjectの名前を `TouchTheStarInitializer` に変更
3. Inspector上で `Add Component` をクリック
4. `TouchTheStarInitializer` を検索して追加

### 3. 効果音とスプライトの設定（必須）
TouchTheStarInitializerコンポーネントのInspectorで、以下を手動で設定してください：
- **Star Appear Sound**: `Assets/Games/05_TouchTheStar/Audios/star_appear.mp3`をドラッグ&ドロップ
- **Star Disappear Sound**: `Assets/Games/05_TouchTheStar/Audios/star_disappear.mp3`をドラッグ&ドロップ
- **Star Sprite**: `Assets/Games/05_TouchTheStar/Sprites/star.png`をドラッグ&ドロップ

※これらの設定は必須です。設定しないと効果音が再生されず、星が正しく表示されません。

### 4. ゲームの開始
1. Playボタンを押してゲームを開始
2. コンソールで初期化メッセージを確認
3. 星が出現したらタップして動作を確認

## 自動生成されるオブジェクト
TouchTheStarInitializerにより以下のオブジェクトが自動的に作成されます：
- **EventSystem**: タップ検出に必要
- **TouchTheStarSFXPlayer**: 効果音管理
- **StarManager**: 星の生成と管理

## パラメータ調整
StarManagerコンポーネントで以下の設定を調整できます：
- **Max Stars**: 同時に表示する星の最大数（デフォルト: 5）
- **Min/Max Spawn Interval**: 星の生成間隔（デフォルト: 1.0〜2.0秒）
- **Spawn Margin**: 画面端からのマージン（デフォルト: 1.0）

Starコンポーネントで以下の設定を調整できます：
- **Min/Max Move Speed**: 星の移動速度（デフォルト: 0.5〜2.0）
- **Min/Max Rotation Speed**: 星の回転速度（デフォルト: 10〜50度/秒）
- **Saturation/Brightness**: 星の色の彩度と明度

## トラブルシューティング

### 星が表示されない場合
1. コンソールでエラーメッセージを確認
2. 星のスプライトが正しく分割されているか確認
3. TouchTheStarInitializerのInspectorで手動でスプライトを設定

### 効果音が再生されない場合
1. AudioClipが正しくインポートされているか確認
2. TouchTheStarInitializerのInspectorで手動で効果音を設定
3. AudioSourceの音量設定を確認

### タップが反応しない場合
1. EventSystemが存在するか確認
2. 星のCollider2Dが正しく設定されているか確認
3. カメラの設定を確認

## ファイル構成
```
Assets/Games/05_TouchTheStar/
├── Scripts/
│   ├── TouchTheStarInitializer.cs # ゲーム自動初期化
│   ├── TouchTheStarSFXPlayer.cs   # 効果音管理
│   ├── StarManager.cs             # 星の生成・管理
│   └── Star.cs                    # 星の挙動制御
├── Scenes/
│   └── TouchTheStar.unity         # ゲームシーン
├── Sprites/
│   ├── bg.png                     # 背景画像
│   └── star.png                   # 星スプライト（3×3分割）
├── Audios/
│   ├── star_appear.mp3            # 星出現効果音
│   └── star_disappear.mp3         # 星消滅効果音
└── README.md                      # このファイル
```

## 今後の拡張予定
- UFOオブジェクトの実装
- 大きな星（star_big）の実装
- スコアシステムの追加
- 特殊エフェクトの追加
