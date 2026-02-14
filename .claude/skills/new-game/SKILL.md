---
name: new-game
description: 新規ミニゲームをテンプレートから作成する完全な手順。新しいゲームの追加・実装時に使用。
---

# 新規ゲーム実装手順

## ゲームテンプレートシステム

テンプレートは `Assets/Games/_GameTemplate`:
1. `_GameTemplate` フォルダをコピー → `NN_ゲーム名` にリネーム（例: `14_AnimalVoice`）
2. プレースホルダーアセットを使用（スプライト: `work_bg.png`, `work_sprite_a/b.png`; オーディオ: `work_sfx1-8.mp3`）
3. `Scripts/` に新規ゲームスクリプトをゼロから作成（テンプレートのScriptsフォルダは空）
4. セットアップスクリプトを作成（例: `Scripts/Editor/` に setup スクリプト）して自動化

---

## ステップ1: テンプレートのコピーとスクリプト作成

1. `Assets/Games/_GameTemplate`フォルダをコピー → `NN_ゲーム名`にリネーム
2. 仕様書を作成（オプション）: `Assets/Games/NN_ゲーム名/ゲーム名_Specification.md`
3. ゲームロジックのスクリプトを`Scripts/`フォルダに作成:
   - `{ゲーム名}Initializer.cs` - ゲーム全体の初期化
   - `{ゲーム名}Controller.cs` - メインゲームロジック
   - `{ゲーム名}SFXPlayer.cs` - 効果音管理
   - その他必要なスクリプト

## ステップ2: Editorセットアップスクリプトの作成

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

## ステップ3: 画像アセットの生成

スキル `/generate-images` を参照。

## ステップ3.5: 効果音の生成

スキル `/generate-sfx` を参照。

## ステップ4: Unity Editorでのセットアップ

1. Unity Editorを開く（既に開いている場合はフォーカスを当てる）
2. スクリプトのコンパイル完了を待つ（数秒）
3. `Tools → Setup {ゲーム名} Game`を実行
4. Unity Consoleでエラーがないか確認

## ステップ5: 動作確認

手動テスト:
1. Unity Editorの**Playボタン**を押す
2. ゲームが正しく動作するか確認
3. 問題があればUnity Consoleのログを確認

自動テスト（推奨）:
```bash
cd work
python restart_unity_test.py
```

## ステップ6: アセットの本番化（オプション）

初回実装では低品質（`quality="low"`）の画像を使用。動作確認後、高品質版を生成:
1. `makeImages.py`の該当ジョブで`quality="standard"`に変更
2. 再度生成: `python makeImages.py`
3. 既存の画像を上書き
4. Unity Editorで`Tools → Apply {ゲーム名} Assets`を実行
