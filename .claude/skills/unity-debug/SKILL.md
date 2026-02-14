---
name: unity-debug
description: Unity Editorの自動テスト、デバッグ、トラブルシューティング。ゲーム動作確認やエラー調査時に使用。
---

# Unity自動化とデバッグ

## Unity Editor自動操作スクリプト

Unity EditorをPythonから自動操作してゲームの動作確認を行うスクリプトを用意しています。

### restart_unity_test.py

**場所**: `work/restart_unity_test.py`

**機能**:
1. Unity Editorを検索してフォーカス
2. 現在のPlay modeを停止（Ctrl+P）
3. スクリプトのコンパイル待機
4. トリガーファイルを作成して自動テストを開始
5. 5秒後にスクリーンショットを撮影
6. スクリーンショットを自動的に開く

**使用方法**:
```bash
cd work
python restart_unity_test.py
```

**出力**: `C:\dev\iwaki001\screenshot_animalvoice.png`

### auto_test_unity.py

**場所**: `work/auto_test_unity.py`

**機能**: 既に開いているUnity Editorでトリガーファイルを作成し、自動テストを開始（Play mode停止は行わない）

## Unity Editor自動テストシステム

### AnimalVoiceAutoTest.cs

**場所**: `Assets/Games/14_AnimalVoice/Scripts/Editor/AnimalVoiceAutoTest.cs`

**機能**:
- トリガーファイル（`trigger_autotest.txt`）を監視 → Play mode開始 → 5秒後にスクリーンショット撮影して停止
- トリガーファイル（`trigger_apply_and_test.txt`）を監視 → ApplyAssets実行 → Play mode開始 → スクリーンショット

**手動実行**: Unity Editor → `Tools → Auto Test AnimalVoice`

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
