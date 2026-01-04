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

## デバッグ：Android端末のPlayerPrefsを書き換えて擬似ロックする

前提：
- 端末で「USBデバッグ」を有効化
- `adb` が使えること（このプロジェクトは Unity 同梱の `adb.exe` を使う）
- 対象アプリが **debuggable ビルド**（Development Build 等）でインストールされていること（`run-as` を使うため）。Play配信の通常ビルドでは失敗することがあります。

このプロジェクトのAndroidパッケージ名：`com.iw.wakuwaku.touchhiroba`

### 1) adb.exe の場所（Unity同梱）
Unity 6000.3.0f1 の例：

```powershell
$adb = "C:\Program Files\Unity\Hub\Editor\6000.3.0f1\Editor\Data\PlaybackEngines\AndroidPlayer\SDK\platform-tools\adb.exe"
& $adb devices
```

### 2) PlayerPrefs（SharedPreferences）を読む
1. アプリ停止（起動中だと終了時に上書きされることがあります）

```powershell
$pkg = "com.iw.wakuwaku.touchhiroba"
& $adb shell am force-stop $pkg
```

2. `shared_prefs` のファイル名を確認

```powershell
& $adb shell run-as $pkg ls shared_prefs
```

3. ファイルを表示（例：`com.iw.wakuwaku.touchhiroba.v2.playerprefs.xml`）

```powershell
$prefs = "com.iw.wakuwaku.touchhiroba.v2.playerprefs.xml"
& $adb shell run-as $pkg cat "shared_prefs/$prefs"
```

`purchased_packs`（権利情報）はURLエンコードされたJSONとして保存されています。デコード例：

```powershell
# %7B...%7D の部分を貼り付けてデコード
[System.Uri]::UnescapeDataString("%7B%22ownedPacks%22%3A%5B%22pack_free%22%2C%22pack_01%22%5D%7D")
```

### 3) PlayerPrefsを書き換える（擬似ロック/擬似アンロック）
1. 端末からXMLを取り出してPCで編集

```powershell
& $adb exec-out run-as $pkg cat "shared_prefs/$prefs" | Out-File -Encoding utf8 .\playerprefs.xml
```

2. `.\playerprefs.xml` の `<string name="purchased_packs">...</string>` を編集
   - 例：`pack_01` をロックしたい場合は `ownedPacks` から `pack_01` を削除

3. 端末へ上書き（PowerShellでは `< file` リダイレクトが使えないためパイプで流す）

```powershell
Get-Content -Raw .\playerprefs.xml | & $adb shell run-as $pkg sh -c "cat > shared_prefs/$prefs"
```

4. 確認

```powershell
& $adb shell run-as $pkg cat "shared_prefs/$prefs" | Select-String purchased_packs
```

### 4) アプリを起動して確認

```powershell
& $adb shell monkey -p $pkg -c android.intent.category.LAUNCHER 1
```

### 注意
- 起動後に購入同期が走ると、`PlayerPrefs` を書き換えても自動で上書きされることがあります。擬似ロック状態だけ確認したい場合は、機内モード（オフライン）で起動して確認すると安定します。

## 以前にやった手順メモ（コピペ用）

### 変数（環境に合わせて）
```powershell
$adb  = "C:\Program Files\Unity\Hub\Editor\6000.3.0f1\Editor\Data\PlaybackEngines\AndroidPlayer\SDK\platform-tools\adb.exe"
$pkg  = "com.iw.wakuwaku.touchhiroba"
$prefs = "com.iw.wakuwaku.touchhiroba.v2.playerprefs.xml"
```

### 端末接続確認

```powershell
& $adb devices
```

### アプリ停止（重要）

```powershell
& $adb shell am force-stop $pkg
```

### prefsファイル名確認

```powershell
& $adb shell run-as $pkg ls shared_prefs
```

### PCに取り出し

```powershell
& $adb exec-out run-as $pkg cat "shared_prefs/$prefs" | Out-File -Encoding utf8 .\playerprefs.xml
```

### `playerprefs.xml` を編集
`<string name="purchased_packs">...</string>` の中身（URLエンコードされたJSON）を編集します。

例：`pack_01` をロックしたい場合（`pack_free` のみ残す）
```text
%7B%22ownedPacks%22%3A%5B%22pack_free%22%5D%7D
```

### 端末へ戻す（上書き）
PowerShellでは `< file` リダイレクトが使えないため、パイプで流します。

```powershell
& $adb shell am force-stop $pkg
Get-Content -Raw .\playerprefs.xml | & $adb shell run-as $pkg sh -c "cat > shared_prefs/$prefs"
& $adb shell run-as $pkg cat "shared_prefs/$prefs" | Select-String purchased_packs
```

### 起動

```powershell
& $adb shell monkey -p $pkg -c android.intent.category.LAUNCHER 1
```

### メインアクティビティを明示して起動（任意）

```powershell
& $adb shell cmd package resolve-activity --brief $pkg
# 出力された Activity を使って起動（例）
& $adb shell am start -n "$pkg/com.unity3d.player.UnityPlayerGameActivity"
```
