# このリポジトリ（Unity）でAIが即戦力になるための指示

> **この文書は `.github/copilot-instructions.md`、`CLAUDE.md`、`README.md` で同一内容を保持しています。**
> 変更時は3ファイルすべてを同期してください。

---

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

- 各ゲームは [Assets/Games](Assets/Games) 配下の `NN_ゲーム名` フォルダ（例: [Assets/Games/05_TouchTheStar](Assets/Games/05_TouchTheStar)）
- 共通ロジックは [Assets/_Common](Assets/_Common)（ゲーム一覧・IAP等）
- `.csproj/.sln` はUnityが自動生成するため編集しない（編集は `Assets/**/*.cs` が中心）

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

### 入力システム

**重要**: このプロジェクトは **Unityの新Input System**（`UnityEngine.InputSystem`）を使用。

**使用禁止**:
- `Input.GetMouseButtonDown()` / `Input.touchCount` / `UnityEngine.Input` クラス全般

**代わりに使用**:
- マルチタッチ: `Touchscreen.current.touches`
- マウス入力: `Mouse.current`
- 参考例: [CatchInsects/InsectController.cs](Assets/Games/07_CatchInsects/Scripts/InsectController.cs)

**マルチタッチ要件**: ゲームは2箇所以上の同時タップに対応する必要があります。

### 効果音アーキテクチャ

各ゲームは専用のSFXPlayerコンポーネントを持つ:
- 命名規則: `{ゲーム名}SFXPlayer`（例: `TouchTheStarSFXPlayer`, `AnimalVoiceSFXPlayer`）
- 効果音ごとの音量個別制御が可能
- ゲームのオーディオクリップ参照を一元管理

### ゲーム初期化パターン

ゲームはInitializerコンポーネントでシーン読み込み時に自動セットアップ:
- 例: [TouchTheStarInitializer.cs](Assets/Games/05_TouchTheStar/Scripts/TouchTheStarInitializer.cs)
- EventSystem、SFXPlayer、背景、ゲームオブジェクトを生成
- プログラムで参照を配線
- Unity Editorでの手動セットアップを最小化

### 外部ライブラリ

プロジェクトにはサードパーティライブラリが含まれています。ゼロから実装する前に利用可能か確認:
- **DOTween**: トゥイーン/アニメーション
- **Cartoon FX Remaster** (CFXR): パーティクルエフェクト（[Assets/JMO Assets/Cartoon FX Remaster](Assets/JMO%20Assets/Cartoon%20FX%20Remaster)）

### 主要Unityパッケージ

[Packages/manifest.json](Packages/manifest.json) 参照:
- `com.unity.inputsystem` (1.16.0) - 新Input System
- `com.unity.purchasing` (5.0.1) - IAP
- `com.unity.render-pipelines.universal` (17.3.0) - URP

---

## 新規ゲーム実装ルール

### 基本ルール

1. **分離**: 指示がある場合を除き、既存ゲームのソースコードを参考にしない（ゲーム間の結合度を下げる）
2. **テンプレートベース**: 指示がない場合は [Assets/Games/_GameTemplate](Assets/Games/_GameTemplate) をコピーして作成
3. **プレースホルダーアセット**: テンプレートの `work_*.png` と `work_sfx*.mp3` を最初に使用
4. **セットアップ自動化**: ユーザー操作を極限まで減らすため、Editorスクリプトでオブジェクト配置/アタッチ/必須参照設定まで自動化。セットアップスクリプトはユーザーが手動で実行する形にする
5. **Scene View編集可能**: 背景画像、固定位置でスポーンされるキャラクター、装飾オブジェクトなどは、完全な動的生成ではなく、Scene内に配置してScene Viewでドラッグ&ドロップ、Inspectorで位置・スケール・回転・動作パラメータを直感的に調整できる設計を優先する。開発者がPlay前にScene Viewで視覚的にレイアウトや動きを調整できることで、開発効率を高める
6. **マルチタッチ**: 画面上の2か所以上を同時タップしても反応する作りにする
7. **メニュー配線なし**: メニュー画面との遷移配線はユーザーが手動で実装する前提のため、AIは勝手に実装しない
8. **SFXPlayer**: 効果音は「【ゲーム名】+ SFXPlayer」命名でゲームごとに一元管理。効果音ごとにボリュームを個別設定できるようにする
9. **ビルド確認**: 新規実装時や大きく実装を直した後は、必ずコンパイルエラーがないことを確認してから作業を完了すること

### ゲームテンプレートシステム

テンプレートは [Assets/Games/_GameTemplate](Assets/Games/_GameTemplate):
1. `_GameTemplate` フォルダをコピー → `NN_ゲーム名` にリネーム（例: `14_AnimalVoice`）
2. プレースホルダーアセットを使用（スプライト: `work_bg.png`, `work_sprite_a/b.png`; オーディオ: `work_sfx1-8.mp3`）
3. `Scripts/` に新規ゲームスクリプトをゼロから作成（テンプレートのScriptsフォルダは空）
4. セットアップスクリプトを作成（例: `Scripts/Editor/` に setup スクリプト）して自動化

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

[画像アセット生成ワークフロー](#画像アセット生成ワークフロー) を参照。

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
```bash
cd makeImage
python restart_unity_test.py
```

#### ステップ6: アセットの本番化（オプション）

初回実装では低品質（`quality="low"`）の画像を使用します。動作確認後、高品質版を生成:
1. `makeImages.py`の該当ジョブで`quality="standard"`に変更
2. 再度生成: `python makeImages.py`
3. 既存の画像を上書き
4. Unity Editorで`Tools → Apply {ゲーム名} Assets`を実行

---

## 画像アセット生成ワークフロー

### OpenAI Image Generation APIを使用した素材作成

**スクリプトの場所**: [makeImage/](makeImage/) ディレクトリ

### 基本的な画像生成

**スクリプト**: [makeImage/makeImages.py](makeImage/makeImages.py)

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

### スプライトシート方式（同一キャラの状態違いスプライト生成）

**重要ルール**: 同一キャラクターの「通常時」と「リアクション時」など、**同じキャラクターデザインで表情・ポーズだけが異なるスプライトを作る場合は、必ずスプライトシート方式を使用すること**。

#### なぜスプライトシート方式が必要か

画像生成AIに別々のプロンプトで個別に生成させると、プロンプトを揃えてもキャラクターの色味・体型・細部のデザインに差異が出る。1枚の横長画像内に複数の状態を同時に生成させることで、同一キャラクターの一貫性を保証する。

#### 使い方

**スクリプト**: [makeImage/makeImages_spritesheet.py](makeImage/makeImages_spritesheet.py)

```python
COMMON_PREFIX = (
    "2体の同じ動物キャラクターを描いたスプライトシート画像。"
    "カラフルでポップな2Dアイコン、太めの黒アウトライン、背景透過、子供向けゲーム用。"
    "画像の左半分に1体目、右半分に2体目を配置。"
    "2体は全く同じキャラクターデザイン・同じ色・同じ体型で、口の開閉だけが異なる。"
    "左側: 口を閉じた通常の表情。右側: 口を大きく開けて鳴いている表情。"
    "どちらも正面向き、可愛いデザイン、0～3歳児向け。"
)

jobs = [
    {
        "name": "chicken",
        "prompt": f"{COMMON_PREFIX}動物: ニワトリ。赤いトサカ、白い体、黄色いくちばし。",
    },
    # ...
]
```

1. **生成**: `python makeImages_spritesheet.py`
   - 1536×1024の横長画像を生成
   - 自動的に左右に分割して `{name}_normal.png` / `{name}_reaction.png` を出力
   - スプライトシート原本も `{name}_sheet.png` として保存
2. **デプロイ**: `python deploy_spritesheet.py`
   - 既存スプライトのバックアップを作成
   - 分割画像をSpritesフォルダにコピー
   - 必要に応じて.metaファイルを生成
3. **反映**: Unity Editorで `Tools → Apply {ゲーム名} Assets` を実行

#### スプライトシート方式を使うべき場面

| 場面 | 方式 |
|------|------|
| 同一キャラの状態違い（通常/リアクション） | **スプライトシート方式** |
| 同一キャラのアニメーションフレーム | **スプライトシート方式** |
| 別々のキャラクター | 個別生成（通常方式） |
| 背景画像 | 個別生成（通常方式） |
| UI要素 | 個別生成（通常方式） |

#### Unityへの取り込みについて

スプライトシートはPythonスクリプト側で左右に分割し、個別ファイル（`_normal.png` / `_reaction.png`）としてUnityに取り込む。理由：
- 既存のInitializer、ApplyAssets、Setupスクリプトが個別ファイル参照を前提としている
- Unityの「Multiple」スプライトモード対応にすると全てのアセット参照コードの改修が必要になる
- 個別ファイル方式なら既存コードに変更不要

### 画像生成の共通ルール

- **レート制限と並列化**: OpenAI APIはレート制限があるが、`quality="low"`なら20枚程度を`asyncio.gather`で同時並列生成可能。`quality="medium"`なら10枚ずつのバッチ並列が安全。逐次処理（3.5秒待機）は不要
- **品質設定**: 背景は`quality="high"`、キャラクタースプライトは`quality="medium"`で生成。初回テスト時のみ`quality="low"`を使用可
- **プロンプト保存**: 全てのプロンプトは`makeImages.py`（または`makeImages_spritesheet.py`）に保存されているため、再生成可能
- **背景透過**: キャラクター/オブジェクトのプロンプトには必ず「背景透過」を含める
- **白色→アイボリールール**: 画像生成AIでは純白（#FFFFFF）が透過として扱われることがあるため、**白色を使う場合は必ず「アイボリー（淡いクリーム色）」と指定する**こと。対象例: 羊の毛、ヤギの体、ユニコーンの体、パンダの白い部分、牛の白い部分、ニワトリの体など
- **背景画像は横長**: `size="1536x1024"`（ランドスケープ）で生成。実機は16:9等さらに横長の端末があるため、主要な要素は画像中央60%に収め、上下左右の端は空・草原等の連続パターンで埋めて端が切れても違和感がないようにする
- **アートスタイル統一**: ゲーム内の全素材で一貫したスタイルを使用（例: "カラフルでポップな2Dアイコン、太めの黒アウトライン"）
- **プロンプトのバックアップ**: 生成方式を変更する場合は、既存のプロンプトファイルを `_backup_xxx.py` としてバックアップしてから変更すること

### 生成画像の検証ルール

画像生成AIはプロンプト通りの出力を保証しないため、**生成後は必ず画像を検証し、想定通りでなければプロンプトを修正して再生成すること**。

- **スプライトシートの2コマ検証**: スプライトシート画像は左右の2体だけのはずが、8コマ等に分割されて生成されることがある。生成後に不透明領域のブロック数をプログラムで検証し、2ブロックでなければ自動リトライすること。`makeImages_spritesheet.py` には `count_blocks()` による自動検証とリトライが組み込まれている
- **プロンプトで「ちょうど2体だけ」を明示**: 「必ず2体だけを描くこと、3体以上や小さなコマを複数並べてはいけない」とプロンプトに明記する
- **白色が透過になっていないか確認**: 白色を含む動物（羊、ニワトリ、ヤギ等）は特に注意。透けていたらプロンプトの「アイボリー」指定が不十分なので強調して再生成
- **結果が想定と違う場合の対応**: プロンプトの言い回しを変える、制約を追加するなどして反復的に改善すること。一度で満足な結果が得られるとは限らない

### 画像生成関連スクリプト一覧

| スクリプト | 用途 |
|-----------|------|
| [makeImage/makeImages.py](makeImage/makeImages.py) | 個別画像生成（背景、独立キャラクター等） |
| [makeImage/makeImages_spritesheet.py](makeImage/makeImages_spritesheet.py) | スプライトシート生成（同一キャラの状態違い） |
| [makeImage/makeImages_backup_individual.py](makeImage/makeImages_backup_individual.py) | 旧個別生成プロンプトのバックアップ |
| [makeImage/deploy_spritesheet.py](makeImage/deploy_spritesheet.py) | スプライトシート分割画像のデプロイ |
| [makeImage/generate_meta.py](makeImage/generate_meta.py) | Unity .metaファイル生成 |
| [makeImage/resize_images.py](makeImage/resize_images.py) | 画像リサイズ |
| [makeImage/editImages.py](makeImage/editImages.py) | OpenAI Edit APIでの画像編集 |

---

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

**出力**: `C:\dev\iwaki001\screenshot_animalvoice.png`

#### auto_test_unity.py

**場所**: [makeImage/auto_test_unity.py](makeImage/auto_test_unity.py)

**機能**: 既に開いているUnity Editorでトリガーファイルを作成し、自動テストを開始（Play mode停止は行わない）

### Unity Editor自動テストシステム

#### AnimalVoiceAutoTest.cs

**場所**: [Assets/Games/14_AnimalVoice/Scripts/Editor/AnimalVoiceAutoTest.cs](Assets/Games/14_AnimalVoice/Scripts/Editor/AnimalVoiceAutoTest.cs)

**機能**:
- トリガーファイル（`trigger_autotest.txt`）を監視 → Play mode開始 → 5秒後にスクリーンショット撮影して停止
- トリガーファイル（`trigger_apply_and_test.txt`）を監視 → ApplyAssets実行 → Play mode開始 → スクリーンショット

**手動実行**: Unity Editor → `Tools → Auto Test AnimalVoice`

### 重要な実装上の注意点

#### Unity Editorのコンパイルタイミング

**重要**: Unity Editorは**バックグラウンドにある間はスクリプトをコンパイルしません**。
- ファイルを編集した後は、Unity Editorにフォーカスを当てる必要があります
- コンパイル完了まで数秒待機してからPlay modeを開始すること

#### SpriteRendererのソート順設定

Unity 2Dゲームでスプライトを正しく表示するには、**sortingOrderを必ず設定**:

```csharp
SpriteRenderer sr = gameObject.AddComponent<SpriteRenderer>();
sr.sortingOrder = 10;  // 背景より前面に表示
```

**推奨値**:
- 背景: `-100`
- ゲームオブジェクト（キャラクター、アイテム等）: `0` ～ `50`
- UI要素、エフェクト: `50` ～ `100`
- フェードオーバーレイ: `100` 以上

#### 既存シーンでの変更反映

Unity Editorスクリプトで新しいPrefabを作成した場合、**既存のシーンには反映されません**。以下のいずれかを実行:
1. **新しいシーンを作成**: `Tools → Setup [GameName] Game`
2. **既存シーンに再適用**: `Tools → Apply [GameName] Assets`

#### .metaファイルの必要性

Unityにアセットをインポートするには、`.meta`ファイルが必須です:
- 画像ファイルを`Assets/`フォルダにコピーしただけでは認識されません
- [makeImage/generate_meta.py](makeImage/generate_meta.py)を使用して`.meta`ファイルを生成
- Unityが自動生成することもありますが、Editorにフォーカスを当てる必要があります

---

## テスト

- 自動テスト基盤はありません。検証はUnity EditorのPlayモードとConsoleログで実施
- **新規実装時や大きく実装を直した後は、必ずコンパイルエラーがないことを確認してから作業を完了すること**

---

## トラブルシューティング

### Unity Editorが応答しない / 変更が反映されない

1. Unity Editorウィンドウにフォーカスを当てる
2. 数秒待ってコンパイルが完了するのを確認（下部のプログレスバー）
3. Unity Consoleでコンパイルエラーを確認

### 画像生成APIのレート制限エラー

`Rate limit reached for gpt-image... Limit 20, Used 20` → `makeImages.py`は自動的に3.5秒の遅延を入れています。手動実行する場合は20画像ごとに1分待機。

### Unity Consoleログの確認方法

**ログファイルの場所**: `C:\Users\{ユーザー名}\AppData\Local\Unity\Editor\Editor.log`

```powershell
# PowerShellから共有モードで読み取り
$p = [IO.Path]::Combine($env:LOCALAPPDATA,"Unity","Editor","Editor.log")
$fs = [IO.FileStream]::new($p, [IO.FileMode]::Open, [IO.FileAccess]::Read, [IO.FileShare]::ReadWrite)
$sr = [IO.StreamReader]::new($fs)
$text = $sr.ReadToEnd(); $sr.Close(); $fs.Close()
$lines = $text -split "`n"
$lines[-100..-1] | Where-Object { $_ -match "error|Error" }
```

### スクリーンショットが撮影されない

1. Unity Editorがバックグラウンドにある → フォーカスを当てる
2. トリガーファイルが検出されていない → Unity Editorを再起動
3. AutoTest.csがコンパイルされていない → Unity Consoleでエラー確認

### よくあるゲーム実装の問題

| 問題 | 原因 | 解決 |
|------|------|------|
| オブジェクトが表示されない | sortingOrder未設定 / スプライトがnull | SpriteRendererにsortingOrderを設定（推奨値: 10） |
| タップが反応しない | Collider未設定 / EventSystem無し | CircleCollider2D追加、EventSystem存在確認 |
| アセット参照がnull | .metaファイル未生成 / パス間違い | `generate_meta.py`実行、パス確認 |

---

## 参考実装

### 推奨参考ゲーム

- **AnimalVoice** ([Assets/Games/14_AnimalVoice](Assets/Games/14_AnimalVoice)) - 最新の実装パターン、自動化スクリプト完備
- **TouchTheStar** ([Assets/Games/05_TouchTheStar](Assets/Games/05_TouchTheStar)) - シンプルなタップゲームの好例
- **CatchInsects** ([Assets/Games/07_CatchInsects](Assets/Games/07_CatchInsects)) - マルチタッチの実装例

### 参照禁止

[Assets/Games/01_MakeBubbles](Assets/Games/01_MakeBubbles) は品質が低いとされています。新規実装の参考にしないでください。

### AnimalVoiceゲームの構成

**場所**: [Assets/Games/14_AnimalVoice](Assets/Games/14_AnimalVoice)

**主要スクリプト**:
- [AnimalVoiceInitializer.cs](Assets/Games/14_AnimalVoice/Scripts/AnimalVoiceInitializer.cs) - ゲーム初期化、動的ScriptableObject生成
- [AnimalController.cs](Assets/Games/14_AnimalVoice/Scripts/AnimalController.cs) - 動物の個別制御、タップ検出
- [AnimalSpawner.cs](Assets/Games/14_AnimalVoice/Scripts/AnimalSpawner.cs) - 動物のスポーン管理（SpawnPoint対応）
- [AnimalSpawnPoint.cs](Assets/Games/14_AnimalVoice/Scripts/AnimalSpawnPoint.cs) - Edit mode配置可能なスポーンポイント
- [BackgroundTimeManager.cs](Assets/Games/14_AnimalVoice/Scripts/BackgroundTimeManager.cs) - 時間帯の切り替え

**Editorスクリプト**:
- [AnimalVoiceSetup.cs](Assets/Games/14_AnimalVoice/Scripts/Editor/AnimalVoiceSetup.cs) - 初回シーン作成
- [AnimalVoiceApplyAssets.cs](Assets/Games/14_AnimalVoice/Scripts/Editor/AnimalVoiceApplyAssets.cs) - アセット再適用
- [AnimalVoiceAutoTest.cs](Assets/Games/14_AnimalVoice/Scripts/Editor/AnimalVoiceAutoTest.cs) - 自動テスト

**特徴**:
- 完全にプログラムで生成されるゲームオブジェクト階層
- ScriptableObjectを動的に作成してデータ管理
- DOTweenを使用したアニメーション
- マルチタッチ対応
- レア要素（10%確率）
- 時間帯による背景・動物の切り替え（30秒ごと）
- SpawnPointシステムによるEdit mode配置対応

---

## IAP（課金・ゲームロック）システム

### 関係コンポーネント

IAPコアは [Assets/_Common/Scripts/IAP](Assets/_Common/Scripts/IAP):

| コンポーネント | 役割 |
|-------------|------|
| [IAPManager.cs](Assets/_Common/Scripts/IAP/IAPManager.cs) | 起動時の初期化と同期トリガー |
| [PurchaseService.cs](Assets/_Common/Scripts/IAP/PurchaseService.cs) | Unity IAP、復元、レシート検証、付与/剥奪 |
| [EntitlementStore.cs](Assets/_Common/Scripts/IAP/EntitlementStore.cs) | 権利のローカル保存（`PlayerPrefs`） |
| [FeatureGate.cs](Assets/_Common/Scripts/IAP/FeatureGate.cs) | 権利に基づくゲーム開始可否判定 |
| [Paywall.cs](Assets/_Common/Scripts/IAP/Paywall.cs) | 購入/復元UI |

`pack_free` は常に所有。Product ID ↔ packId の対応は `PurchaseService.cs` の `products` リスト。

### 起動時フロー（全体像）

```
起動
  |
IAPManager.InitializeIAPSystem
  |
  +--> EntitlementStore 起動
  |       - PlayerPrefs から権利読込
  |       - pack_free は常に付与
  |
  +--> PurchaseService 起動（Unity IAP Initialize）
  |       |
  |       +--> OnInitialized
  |               |
  |               +--> ValidateAndGrantPurchases（レシート検証 → 付与/剥奪）
  |
  +--> SyncPurchases（復元/同期）
          |
          +--> PurchaseService.RestorePurchases
                  |
                  +-- [オンライン] Google RestoreTransactions → ValidateAndGrantPurchases
                  |
                  +-- [オフライン] ValidateAndGrantPurchases（ローカルキャッシュのレシート）
```

### オンライン時 vs オフライン時

| | オンライン | オフライン |
|---|---|---|
| **レシート情報** | Google Play Store から最新取得 | Unity IAP のキャッシュを使用 |
| **返金反映** | 即座に反映 | 次回オンライン時まで反映されない |
| **新規購入の復元** | 他端末の購入も復元可能 | キャッシュにある分のみ |

### データの流れ

```
┌─────────────────┐
│ Google Play    │ ←── オンライン時のみ問い合わせ
│ Store          │
└───────┬────────┘
        │
        ▼
┌─────────────────┐
│ Unity IAP      │ ←── レシートをキャッシュ
│ (内部キャッシュ) │     オフライン時はここを参照
└───────┬────────┘
        │
        ▼
┌─────────────────┐
│ PurchaseService│ ←── レシート検証
└───────┬────────┘
        │
        ▼
┌─────────────────┐
│EntitlementStore│ ←── 所有パックIDを管理
│ (PlayerPrefs)  │
└───────┬────────┘
        │
        ▼
┌─────────────────┐
│ FeatureGate    │ ←── ロック判定
└─────────────────┘
```

### レシート検証と権利更新

`PurchaseService.ValidateAndGrantPurchases()` は:
1. Unity IAPの全商品を走査し、レシートがあるものを検証
2. 「有効（Purchased）」なパックID集合を作る
3. 集合に含まれる → `EntitlementStore.GrantPack(packId)` / 含まれない → `EntitlementStore.RevokePack(packId)`

### 購入フロー（Paywall経由）

```
Paywall 購入 → PurchaseService.PurchaseProduct → Unity IAP 購入完了
  → PurchaseService.ProcessPurchase → EntitlementStore.GrantPack → UIコールバック
```

### 保留購入（Pending/Deferred）の扱い

- 検出: `PurchaseService` がGoogle Play拡張の情報から「保留」を検出
- 権利: 保留中はパックを解放しない（`pending_packs`として別途保存）
- UI: `Paywall` は保留中のパックを検出すると説明文を差し替え、購入ボタンを非活性化
- 反映: 支払い確定後、次回オンライン同期で `purchased_packs` に移行

### 重要な前提（オフライン）

- `product.hasReceipt` / `product.receipt` は **Unity IAPのローカルキャッシュ**で `PlayerPrefs` とは別物
- オフライン時はキャッシュに残っているレシート情報を元に判断
- 返金は**次回オンラインで同期成功したタイミング**でロック状態に反映

### 用語・データの所在

| データ | 保存先 | 特徴 |
|--------|--------|------|
| PlayerPrefs（ローカル権利） | アプリのデータ領域 | 「ストレージ削除」で消える |
| レシート（Unity IAPキャッシュ） | Unity IAPがアプリ内に保持 | 「ストレージ削除」で消える |
| Google Play Billing（原本） | Googleのサーバ | アプリ側のデータ削除とは独立 |

---

## デバッグ：Android端末のPlayerPrefsを書き換えて擬似ロックする

### 前提
- 端末で「USBデバッグ」を有効化
- `adb` が使えること（Unity同梱のものを使用）
- 対象アプリが **debuggableビルド** でインストールされていること

### 変数（環境に合わせて）

```powershell
$adb  = "C:\Program Files\Unity\Hub\Editor\6000.3.0f1\Editor\Data\PlaybackEngines\AndroidPlayer\SDK\platform-tools\adb.exe"
$pkg  = "com.iw.wakuwaku.touchhiroba"
$prefs = "com.iw.wakuwaku.touchhiroba.v2.playerprefs.xml"
```

### 手順

```powershell
# 1. 端末接続確認
& $adb devices

# 2. アプリ停止（重要：起動中だと終了時に上書きされる）
& $adb shell am force-stop $pkg

# 3. prefsファイル名確認
& $adb shell run-as $pkg ls shared_prefs

# 4. PCに取り出し
& $adb exec-out run-as $pkg cat "shared_prefs/$prefs" | Out-File -Encoding utf8 .\playerprefs.xml
```

### PlayerPrefsの編集

`purchased_packs` はURLエンコードされたJSONとして保存されています:

```powershell
# デコード例
[System.Uri]::UnescapeDataString("%7B%22ownedPacks%22%3A%5B%22pack_free%22%2C%22pack_01%22%5D%7D")
```

`pack_01` をロックしたい場合（`pack_free`のみ残す）:
```text
%7B%22ownedPacks%22%3A%5B%22pack_free%22%5D%7D
```

### 端末へ戻す

```powershell
# 停止
& $adb shell am force-stop $pkg

# 一時領域へ送る
& $adb push ".\playerprefs.xml" /data/local/tmp/playerprefs.xml

# アプリ領域にコピー
& $adb shell run-as $pkg mkdir -p shared_prefs
& $adb shell run-as $pkg cp /data/local/tmp/playerprefs.xml shared_prefs/$prefs

# 確認
& $adb shell run-as $pkg cat shared_prefs/$prefs | Select-String purchased_packs

# 起動
& $adb shell monkey -p $pkg -c android.intent.category.LAUNCHER 1
```

### 注意

- 起動後に購入同期が走ると `PlayerPrefs` が自動で上書きされる。擬似ロック確認は**機内モード（オフライン）**で起動すると安定する。

---

## 重要なファイル一覧

| ファイル | 役割 |
|---------|------|
| [GameInfo.cs](Assets/_Common/Scripts/GameInfo.cs) | ゲームの中央レジストリ |
| [MenuController.cs](Assets/Games/00_Menu/Scripts/MenuController.cs) | 動的メニュー生成 |
| [IAPManager.cs](Assets/_Common/Scripts/IAP/IAPManager.cs) | IAPシステム初期化 |
| [EntitlementStore.cs](Assets/_Common/Scripts/IAP/EntitlementStore.cs) | パック所有権のストレージ |
| [FeatureGate.cs](Assets/_Common/Scripts/IAP/FeatureGate.cs) | ロック/アンロックロジック |
| [.github/copilot-instructions.md](.github/copilot-instructions.md) | AI向けガイドライン（本ファイルと同一内容） |
| [CLAUDE.md](CLAUDE.md) | AI向けガイドライン（本ファイルと同一内容） |
| [README.md](README.md) | AI向けガイドライン（本ファイルと同一内容） |
