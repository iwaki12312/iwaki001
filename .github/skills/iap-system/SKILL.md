---
name: iap-system
description: IAP（アプリ内課金）システムのアーキテクチャとデバッグ。課金・ロック機能の実装・調査時に使用。
---

# IAP（課金・ゲームロック）システム

## 関係コンポーネント

IAPコアは `Assets/_Common/Scripts/IAP`:

| コンポーネント | 役割 |
|-------------|------|
| `IAPManager.cs` | 起動時の初期化と同期トリガー |
| `PurchaseService.cs` | Unity IAP、復元、レシート検証、付与/剥奪 |
| `EntitlementStore.cs` | 権利のローカル保存（`PlayerPrefs`） |
| `FeatureGate.cs` | 権利に基づくゲーム開始可否判定 |
| `Paywall.cs` | 購入/復元UI |

`pack_free` は常に所有。Product ID <-> packId の対応は `PurchaseService.cs` の `products` リスト。

## 起動時フロー（全体像）

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

## オンライン時 vs オフライン時

| | オンライン | オフライン |
|---|---|---|
| **レシート情報** | Google Play Store から最新取得 | Unity IAP のキャッシュを使用 |
| **返金反映** | 即座に反映 | 次回オンライン時まで反映されない |
| **新規購入の復元** | 他端末の購入も復元可能 | キャッシュにある分のみ |

## データの流れ

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

## レシート検証と権利更新

`PurchaseService.ValidateAndGrantPurchases()` は:
1. Unity IAPの全商品を走査し、レシートがあるものを検証
2. 「有効（Purchased）」なパックID集合を作る
3. 集合に含まれる → `EntitlementStore.GrantPack(packId)` / 含まれない → `EntitlementStore.RevokePack(packId)`

## 購入フロー（Paywall経由）

```
Paywall 購入 → PurchaseService.PurchaseProduct → Unity IAP 購入完了
  → PurchaseService.ProcessPurchase → EntitlementStore.GrantPack → UIコールバック
```

## 保留購入（Pending/Deferred）の扱い

- 検出: `PurchaseService` がGoogle Play拡張の情報から「保留」を検出
- 権利: 保留中はパックを解放しない（`pending_packs`として別途保存）
- UI: `Paywall` は保留中のパックを検出すると説明文を差し替え、購入ボタンを非活性化
- 反映: 支払い確定後、次回オンライン同期で `purchased_packs` に移行

## 重要な前提（オフライン）

- `product.hasReceipt` / `product.receipt` は **Unity IAPのローカルキャッシュ**で `PlayerPrefs` とは別物
- オフライン時はキャッシュに残っているレシート情報を元に判断
- 返金は**次回オンラインで同期成功したタイミング**でロック状態に反映

## 用語・データの所在

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

`purchased_packs` はURLエンコードされたJSONとして保存:

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
