# このプロジェクトの課金（追加パック）フロー概要（Android）

当面は **Androidのみ** を対象にしています。リリースビルドでは `RECEIPT_VALIDATION` を有効にし、購入/返金状態は **レシート検証結果**に基づいてパックのロック状態（権利）を更新します。

## 関係コンポーネント
- `Assets/_Common/Scripts/IAP/IAPManager.cs`：起動時の初期化と同期（復元）トリガー
- `Assets/_Common/Scripts/IAP/PurchaseService.cs`：Unity IAP、復元（オンライン同期）、レシート検証、付与/剥奪
- `Assets/_Common/Scripts/IAP/EntitlementStore.cs`：権利のローカル保存（`PlayerPrefs`）
- `Assets/_Common/Scripts/IAP/FeatureGate.cs`：権利に基づくゲーム開始可否判定
- `Assets/_Common/Scripts/IAP/Paywall.cs`：購入/復元UI

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

## レシート検証と権利更新の考え方
`PurchaseService.ValidateAndGrantPurchases()` は、概ね次の手順で「正しい権利状態」に寄せます。

1) Unity IAPの全商品を走査し、レシートがあるものを検証  
2) 「有効（Purchased）」なパックID集合を作る  
3) すべてのパックについて権利を再計算  
   - 集合に含まれる → `EntitlementStore.GrantPack(packId)`  
   - 集合に含まれない → `EntitlementStore.RevokePack(packId)`（返金/キャンセル/レシート無しを含む）

## 購入フロー（Paywall経由）
```
Paywall 購入
  |
  v
PurchaseService.PurchaseProduct
  |
  v
Unity IAP 購入完了
  |
  v
PurchaseService.ProcessPurchase → EntitlementStore.GrantPack → UIコールバック
```

## 重要な前提（オフライン）
- `product.hasReceipt` / `product.receipt` が参照する「レシート」は **Unity IAPのローカルキャッシュ**で、`PlayerPrefs` とは別物です。
- オフライン時はストアと同期できないため、**キャッシュに残っているレシート情報を元に判断**します。
- 返金は即時反映されない場合があり、**次回オンラインで同期（初期化/復元）が成功したタイミング**でロック状態に反映されます。


