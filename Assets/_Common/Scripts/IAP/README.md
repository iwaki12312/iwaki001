# Unity ミニゲーム集 IAP実装

このドキュメントでは、Unity ミニゲーム集アプリに実装された課金パック解放機能について説明します。

## 概要

- **無料パック（Pack0）**: 最初の6ゲーム（1-6番）は常時解放
- **有料パック**: 7番目以降のゲームは6本ごとに非消耗型パックとして販売
  - Pack1: ゲーム7-12
  - Pack2: ゲーム13-18
  - Pack3: ゲーム19-24 (以降同様)

## 実装されたクラス

### コアシステム
- **EntitlementStore**: ユーザーの購入権利をローカル管理（PlayerPrefs使用）
- **PurchaseService**: Unity IAPを使用した購入処理
- **FeatureGate**: ゲームアクセス制御（パック所有状況チェック）
- **IAPManager**: システム全体の初期化と管理

### UI要素
- **Paywall**: 購入画面と親ゲートを統合（説明・価格・購入・復元ボタン、および親ゲートの掛け算クイズ）
- **NumberPadController**: 親ゲート用の数字パッド

### 統合
- **MenuController**: 既存メニューシステムにロック機能を統合

## セットアップ手順

### 1. Unity IAPパッケージ
既にインストール済み（com.unity.purchasing v5.0.1）

### 2. IAPManagerの配置
1. 空のGameObjectを作成し、「IAPManager」と命名
2. `IAPManager.cs`をアタッチ
3. MainMenuシーンに配置（DontDestroyOnLoadで永続化される）

### 3. UI Prefabの作成（必要に応じて）
以下のPrefabを作成してIAPManagerに設定：
- PurchaseContentPanel Prefab（購入画面と親ゲートを統合したUI。Paywallが制御）

## Product ID設定

PurchaseService.csで以下のProduct IDが設定されています：
```
com.iw.wakuwaku.touchhiroba.pack.01 → pack_01
com.iw.wakuwaku.touchhiroba.pack.02 → pack_02
com.iw.wakuwaku.touchhiroba.pack.03 → pack_03
```

## テスト手順

### エディタでのテスト

#### 1. 基本動作確認
1. MainMenuシーンを再生
2. Console で「[IAPManager] IAPシステム初期化完了」を確認
3. 無料ゲーム（1-6番）が正常に起動することを確認
4. 7番目のゲーム（NewGame）をタップしてPaywallが表示されることを確認

#### 2. デバッグ機能の使用
IAPManagerのContext Menuから以下を実行可能：
- **Initialize IAP System**: 手動初期化
- **Debug Clear All Entitlements**: 全権利クリア
- **Debug Grant Pack 01**: Pack1を付与
- **Debug Print Access Status**: アクセス状況表示

#### 3. 親ゲートのテスト
1. ロックされたゲームをタップ
2. Paywallで「購入」ボタンをクリック
3. 親ゲート（掛け算クイズ）が表示されることを確認
4. 正解を入力して通過できることを確認

### 実機でのテスト

#### 1. Google Play Console設定
1. アプリをアップロード（内部テスト版でも可）
2. アプリ内商品を登録：
   ```
   商品ID: com.iw.wakuwaku.touchhiroba.pack.01
   商品タイプ: 管理対象商品（非消耗型）
   価格: 適切な価格を設定
   ```
3. ライセンステストアカウントを設定

#### 2. テスト手順
1. テストアカウントでGoogle Playにログイン
2. アプリを起動
3. Console で「[PurchaseService] Unity IAP初期化完了」を確認
4. ロックされたゲームをタップ
5. 価格が正しく表示されることを確認
6. 購入フローをテスト（実際の課金は発生しない）

#### 3. 復元機能のテスト
1. 購入後にアプリをアンインストール
2. 再インストールして起動
3. Paywallで「購入を復元」ボタンをタップ
4. 購入済みパックが復元されることを確認

## トラブルシューティング

### よくある問題

#### 1. Unity IAPが初期化されない
- **症状**: 「Unity IAP初期化失敗」のログ
- **対処**: 
  - インターネット接続を確認
  - Google Play Console でアプリが公開されているか確認
  - Product IDが正しく設定されているか確認

#### 2. 価格が表示されない
- **症状**: 価格欄に「---」が表示される
- **対処**:
  - Unity IAPの初期化完了を待つ
  - Google Play Console でアプリ内商品が有効になっているか確認

#### 3. 購入が完了しない
- **症状**: 購入処理が「処理中」のまま
- **対処**:
  - Google Play の支払い方法を確認
  - テストアカウントの設定を確認
  - アプリを再起動

#### 4. 復元が動作しない
- **症状**: 「購入を復元」を押しても何も起こらない
- **対処**:
  - 同じGoogleアカウントでログインしているか確認
  - 購入が正常に完了していたか確認

### デバッグログの確認

重要なログメッセージ：
```
[IAPManager] IAPシステム初期化完了
[PurchaseService] Unity IAP初期化完了
[EntitlementStore] 権利情報読み込み完了
[FeatureGate] ゲームアクセスチェック
[Paywall] 購入成功
```

## 今後の拡張

### 新しいパックの追加
1. `GameInfo.cs`に新しいゲームを追加（packIDを指定）
2. `PurchaseService.cs`のproductsリストに新しいProduct IDを追加
3. Google Play Console で対応するアプリ内商品を登録

### UI のカスタマイズ
- ParentalGateのデザイン変更
- Paywall内の親ゲートト調整
- ロック表示の追加（ゲームアイコンにロックマーク等）

### 分析機能の追加
- 購入イベントの追跡
- ユーザー行動の分析
- A/Bテストの実装

## 注意事項

1. **テスト環境**: 必ずテストアカウントで動作確認を行う
2. **データ保護**: PlayerPrefsは簡単に改ざん可能なため、重要な場合はより安全な保存方法を検討
3. **オフライン対応**: 初回購入時はインターネット接続が必要
4. **プライバシー**: 購入情報の取り扱いに注意

## サポート

実装に関する質問や問題が発生した場合は、各クラスのデバッグ機能を活用してログを確認してください。

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
