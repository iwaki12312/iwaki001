---
name: generate-sfx
description: ElevenLabs Text-to-Sound-Effects APIで効果音を生成するワークフロー。ゲームのSFX作成時に使用。
---

# 効果音（SFX）生成ワークフロー

## ElevenLabs Text-to-Sound-Effects APIを使用した効果音作成

**スクリプトの場所**: `work/sfx/` ディレクトリ

**APIキー**: `.env.yaml` の `ELEVENLABS_API_KEY` に設定済み

## 基本的な効果音生成

**スクリプト**: `work/sfx/makeSFX.py`

1. **SFX生成ジョブの定義** - `makeSFX.py`の`jobs`配列に生成したい効果音を追加:
```python
jobs = [
    {
        "out": r"out\GameName\pop.mp3",
        "text": "Cute cartoon pop sound, bubbly and playful, suitable for a children's game",
        "duration_seconds": 1.5,      # 0.5〜30秒（省略時はAIが最適長を推定）
        "prompt_influence": 0.4,       # 0〜1（省略時0.3、高いほどプロンプトに忠実）
    },
]
```

2. **生成実行**:
```bash
cd work/sfx
python makeSFX.py
```

3. **Unityへのコピー** - 生成されたMP3を`Assets/Games/NN_GameName/Audio/`にコピー

## SFX生成のルール

- **プロンプトは英語で書く**: ElevenLabsのモデルは英語プロンプトの精度が高い
- **短い効果音**: ゲーム用SFXは通常0.5〜3秒程度。`duration_seconds`で明示的に指定すると安定する
- **prompt_influence**: 0.3〜0.5が推奨。高すぎると不自然、低すぎるとプロンプトから外れる
- **出力形式**: デフォルトはMP3。Unityでそのまま使用可能
- **APIキー管理**: `.env.yaml` から自動読み込み。コードにAPIキーをハードコードしない
- **レート制限**: ジョブ間に1秒の待機を自動挿入

## 新規ゲーム実装時の効果音作成手順

1. `work/sfx/makeSFX.py` の `jobs` にゲーム用SFXを追加
2. `python makeSFX.py` で生成
3. 生成されたMP3を `Assets/Games/NN_GameName/Audio/` にコピー
4. `.meta` ファイルはUnity Editorがフォーカス時に自動生成（画像と異なり手動生成不要）
5. SFXPlayerスクリプトでAudioClipとして参照
