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

## 保留購入（Pending/Deferred）の扱い
Google Playの購入は、支払いの確定が後になる「保留（Pending/Deferred）」状態になることがあります（アプリ側で意図的に発生させる機能を作らなくても起こり得ます）。

- 検出: `PurchaseService` が Google Play拡張の情報から「保留」を検出します
  - `IGooglePlayConfiguration.SetDeferredPurchaseListener`（購入が保留になった通知）
  - `IGooglePlayStoreExtensions.IsPurchasedProductDeferred`（該当Productが保留かどうか判定）
- 権利: 保留中は **パックを解放しません**
  - `EntitlementStore` に保留パックIDを `pending_packs` として保存します（所持権利 `purchased_packs` とは別）
  - `ValidateAndGrantPurchases` でも保留中のパックは「有効購入」に含めないため、ロックされたままになります
- UI: `Paywall` は保留中のパックを検出すると説明文を差し替え、購入ボタンを非活性にします
  - 表示例: 「購入は保留中です。Google Play側で確定後に反映されます。」
- 反映: 支払いが確定し、次回オンライン同期（初期化/復元）で保留状態が解消されると
  - `pending_packs` から削除
  - `purchased_packs` に付与され、ゲームが解放されます

## 重要な前提（オフライン）
- `product.hasReceipt` / `product.receipt` が参照する「レシート」は **Unity IAPのローカルキャッシュ**で、`PlayerPrefs` とは別物です。
- オフライン時はストアと同期できないため、**キャッシュに残っているレシート情報を元に判断**します。
- 返金は即時反映されない場合があり、**次回オンラインで同期（初期化/復元）が成功したタイミング**でロック状態に反映されます。

## 用語・データの所在（Google Play Billing / PlayerPrefs / レシート）
このプロジェクトでは「権利（ロック解除）」と「レシート（購入の証跡）」と「ストア側の購入状態」が別物として存在します。

### PlayerPrefs（本プロジェクトのローカル権利）
- 保存先: アプリのデータ領域（端末内）
- 用途: `EntitlementStore` が「所持パック（packId）」を保存し、`FeatureGate` がゲーム開始可否を判定
- 特徴: アプリの「ストレージ削除（データ消去）」で消える

### レシート（Unity IAPが参照するローカルキャッシュ）
- 参照元: `Product.receipt` / `product.hasReceipt`
- 保存先: Unity IAPがアプリ内に保持するローカルデータ（端末内、アプリ領域）
- 用途: `PurchaseService.ValidateAndGrantPurchases()` が検証して「有効購入（Purchased）」かどうかを判定
- 特徴: アプリの「ストレージ削除（データ消去）」で消える（ただし後述のGoogle Play Billing側から再取得されることがある）

### Google Play Billing（ストア側の購入状態＝原本）
- 原本: Googleのサーバ上の購入状態（購入/返金/キャンセル等）
- 端末側: Google Play ストア / Google Play 開発者サービスが購入情報を保持し、アプリ（Unity IAP）が初期化時に問い合わせる
- 特徴:
  - アプリのストレージ削除とは独立しているため、アプリ側のデータを消しても「購入済み情報」が端末/ストア側から返ることがある
  - オフライン中は最新状態に更新できず、次回オンラインで問い合わせが成功したタイミングで最新状態に近づく（反映に遅延が出る場合あり）

