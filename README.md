# わくわくタッチひろば

0～3歳児向けの「タップのみ」ミニゲーム集（Unity 6）

## 概要

- **Unity 6（6000.3.0f1）**
- **ターゲット**: Android / iOS
- **パッケージ名**: `com.iw.wakuwaku.touchhiroba`

## ミニゲーム一覧

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

## プロジェクト構成

```
Assets/
├── _Common/          # 共通ロジック（ゲーム一覧・IAP等）
├── Games/
│   ├── 00_Menu/      # メニュー画面
│   ├── 01_MakeBubbles/
│   ├── ...
│   ├── 14_AnimalVoice/
│   └── _GameTemplate/ # 新規ゲーム作成用テンプレート
└── JMO Assets/       # Cartoon FX Remaster
work/
├── images/           # 画像生成スクリプト群
└── sfx/              # 効果音生成スクリプト群
```

## 主要技術スタック

- **Input System**: Unity新Input System（`UnityEngine.InputSystem`）
- **レンダリング**: URP（Universal Render Pipeline）
- **アニメーション**: DOTween
- **エフェクト**: Cartoon FX Remaster
- **課金**: Unity IAP

## AI向け指示書・スキル

AIコーディングエージェント向けの指示は以下に分散管理されています：

| ファイル | 用途 |
|---------|------|
| `CLAUDE.md` | Claude Code向け常時ロード指示（プロジェクト概要・制約・ルール） |
| `.github/copilot-instructions.md` | GitHub Copilot向け常時ロード指示（同上） |

### スキル（`.github/skills/`）

詳細なワークフロー手順はスキルとして格納。エージェントが必要に応じてオンデマンドで参照します。

| スキル | 内容 |
|--------|------|
| `new-game` | 新規ゲームの作成手順（テンプレートコピー〜動作確認） |
| `generate-images` | OpenAI APIでの画像生成ワークフロー（スプライトシート含む） |
| `generate-sfx` | ElevenLabs APIでの効果音生成ワークフロー |
| `unity-debug` | Unity自動テスト・トラブルシューティング |
| `iap-system` | IAP課金システムのアーキテクチャとAndroidデバッグ |
