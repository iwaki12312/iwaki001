using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using UnityEngine.Purchasing.Security;

namespace WakuWaku.IAP
{
    /// <summary>
    /// Unity IAPを使用した購入処理を管理するクラス
    /// </summary>
    public class PurchaseService : MonoBehaviour, IStoreListener
    {
        [Header("Product Settings")]
        [SerializeField] private List<ProductInfo> products = new List<ProductInfo>
        {
            new ProductInfo("com.iw.wakuwaku.touchhiroba.pack.01", "pack_01", ProductType.NonConsumable),
            new ProductInfo("com.iw.wakuwaku.touchhiroba.pack.02", "pack_02", ProductType.NonConsumable),
            new ProductInfo("com.iw.wakuwaku.touchhiroba.pack.03", "pack_03", ProductType.NonConsumable),
            new ProductInfo("com.iw.wakuwaku.touchhiroba.pack.04", "pack_04", ProductType.NonConsumable),
            new ProductInfo("com.iw.wakuwaku.touchhiroba.pack.05", "pack_05", ProductType.NonConsumable),
            new ProductInfo("com.iw.wakuwaku.touchhiroba.pack.06", "pack_06", ProductType.NonConsumable),
            new ProductInfo("com.iw.wakuwaku.touchhiroba.pack.07", "pack_07", ProductType.NonConsumable),
            new ProductInfo("com.iw.wakuwaku.touchhiroba.pack.08", "pack_08", ProductType.NonConsumable),
            new ProductInfo("com.iw.wakuwaku.touchhiroba.pack.09", "pack_09", ProductType.NonConsumable),
            new ProductInfo("com.iw.wakuwaku.touchhiroba.pack.10", "pack_10", ProductType.NonConsumable),
        };
        
        private IStoreController storeController;
        private IExtensionProvider storeExtensionProvider;
        private bool isInitialized = false;
        
        public static PurchaseService Instance { get; private set; }
        
        public event Action OnIAPInitialized;
        
        public bool IsInitialized => isInitialized;
        
        // コールバック保持用
        private Action<string> currentOnSuccess;
        private Action<string, string> currentOnFailed;
        private Action<string> currentOnDeferred;
        private Action currentOnRestoreCompleted;
        
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializePurchasing();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        /// <summary>
        /// Unity IAPの初期化
        /// </summary>
        private void InitializePurchasing()
        {
            if (isInitialized)
                return;
                
            Debug.Log("[PurchaseService] Unity IAP初期化開始");
            
            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

#if UNITY_ANDROID
            builder.Configure<IGooglePlayConfiguration>().SetDeferredPurchaseListener(OnDeferredPurchase);
#endif
            
            // 商品を登録
            foreach (var product in products)
            {
                builder.AddProduct(product.productId, product.productType);
                Debug.Log($"[PurchaseService] 商品登録: {product.productId} ({product.packId})");
            }
            
            UnityPurchasing.Initialize(this, builder);
        }
        
        /// <summary>
        /// 商品を購入
        /// </summary>
        public void PurchaseProduct(string packId, 
            Action<string> onSuccess = null, 
            Action<string, string> onFailed = null,
            Action<string> onDeferred = null)
        {
            currentOnSuccess = onSuccess;
            currentOnFailed = onFailed;
            currentOnDeferred = onDeferred;
            
            if (!isInitialized)
            {
                Debug.LogError("[PurchaseService] Unity IAPが初期化されていません");
                currentOnFailed?.Invoke(packId, "Unity IAPが初期化されていません");
                return;
            }
            
            var productInfo = GetProductInfoByPackId(packId);
            if (productInfo == null)
            {
                Debug.LogError($"[PurchaseService] 不明なパックID: {packId}");
                currentOnFailed?.Invoke(packId, "不明なパックID");
                return;
            }
            
            var product = storeController.products.WithID(productInfo.productId);
            if (product != null && product.availableToPurchase)
            {
                Debug.Log($"[PurchaseService] 購入開始: {packId} ({productInfo.productId})");
                storeController.InitiatePurchase(product);
            }
            else
            {
                Debug.LogError($"[PurchaseService] 商品が購入できません: {packId}");
                currentOnFailed?.Invoke(packId, "商品が購入できません");
            }
        }

#if UNITY_ANDROID
        private void OnDeferredPurchase(Product product)
        {
            var productInfo = GetProductInfoByProductId(product.definition.id);
            if (productInfo == null)
                return;

            if (EntitlementStore.Instance != null)
            {
                EntitlementStore.Instance.MarkPackPending(productInfo.packId);
            }

            Debug.LogWarning($"[PurchaseService] Purchase deferred (pending): {productInfo.packId}");
            currentOnDeferred?.Invoke(productInfo.packId);
        }
#endif
        
        /// <summary>
        /// 購入の復元
        /// </summary>
        public void RestorePurchases(Action onCompleted = null)
        {
            currentOnRestoreCompleted = onCompleted;
            
            if (!isInitialized)
            {
                Debug.LogError("[PurchaseService] Unity IAPが初期化されていません");
                currentOnRestoreCompleted?.Invoke();
                return;
            }
            
            Debug.Log("[PurchaseService] 購入復元開始");
            
            // Androidでは自動的に復元されるため、手動で既存の購入をチェック（返金チェック含む）

#if UNITY_ANDROID
            if (Application.internetReachability != NetworkReachability.NotReachable && storeExtensionProvider != null)
            {
                var googleExtensions = storeExtensionProvider.GetExtension<IGooglePlayStoreExtensions>();
                if (googleExtensions != null)
                {
                    googleExtensions.RestoreTransactions((success, error) =>
                    {
                        if (!success)
                        {
                            Debug.LogWarning($"[PurchaseService] RestoreTransactions failed: {error}");
                        }
                        ValidateAndGrantPurchases();
                        Debug.Log("[PurchaseService] 購入復元完了");
                        currentOnRestoreCompleted?.Invoke();
                    });
                    return;
                }
            }
#endif
            ValidateAndGrantPurchases();
            
            Debug.Log("[PurchaseService] 購入復元完了");
            currentOnRestoreCompleted?.Invoke();
        }
        
        /// <summary>
        /// 商品価格を取得
        /// </summary>
        public string GetProductPrice(string packId)
        {
            if (!isInitialized)
                return "---";
                
            var productInfo = GetProductInfoByPackId(packId);
            if (productInfo == null)
                return "---";
                
            var product = storeController.products.WithID(productInfo.productId);
            return product?.metadata.localizedPriceString ?? "---";
        }
        
        /// <summary>
        /// 商品が購入可能かチェック
        /// </summary>
        public bool IsProductAvailable(string packId)
        {
            if (!isInitialized)
                return false;
                
            var productInfo = GetProductInfoByPackId(packId);
            if (productInfo == null)
                return false;
                
            var product = storeController.products.WithID(productInfo.productId);
            return product != null && product.availableToPurchase;
        }
        
        private ProductInfo GetProductInfoByPackId(string packId)
        {
            return products.Find(p => p.packId == packId);
        }
        
        private ProductInfo GetProductInfoByProductId(string productId)
        {
            return products.Find(p => p.productId == productId);
        }
        
        /// <summary>
        /// レシートを検証して購入状態を確認（返金チェック含む）
        /// </summary>
        private void ValidateAndGrantPurchases()
        {
#if (UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE_OSX) && RECEIPT_VALIDATION && !UNITY_EDITOR
            // レシート検証器を初期化（実機のみ）
            var validator = new CrossPlatformValidator(
                GooglePlayTangle.Data(),
                AppleTangle.Data(),
                Application.identifier
            );
            
            var validPackIds = new HashSet<string>();
            var deferredPackIds = new HashSet<string>();

#if UNITY_ANDROID
            var googleExtensions = storeExtensionProvider?.GetExtension<IGooglePlayStoreExtensions>();
#endif

            foreach (var product in storeController.products.all)
            {
                var productInfo = GetProductInfoByProductId(product.definition.id);
                if (productInfo == null)
                    continue;

#if UNITY_ANDROID
                if (googleExtensions != null && googleExtensions.IsPurchasedProductDeferred(product))
                {
                    deferredPackIds.Add(productInfo.packId);
                    EntitlementStore.Instance.MarkPackPending(productInfo.packId);
                    Debug.LogWarning($"[PurchaseService] Purchase pending: {productInfo.packId}");
                    continue;
                }
#endif

                if (!product.hasReceipt)
                    continue;

                try
                {
                    var result = validator.Validate(product.receipt);

                    foreach (IPurchaseReceipt receipt in result)
                    {
                        GooglePlayReceipt googleReceipt = receipt as GooglePlayReceipt;
                        if (googleReceipt == null)
                            continue;

                        if (googleReceipt.purchaseState == GooglePurchaseState.Purchased)
                        {
                            validPackIds.Add(productInfo.packId);
                        }
                        else
                        {
                            Debug.LogWarning($"[PurchaseService] Purchase cancelled/refunded: {productInfo.packId} ({googleReceipt.purchaseState})");
                        }
                        break;
                    }
                }
                catch (IAPSecurityException ex)
                {
                    Debug.LogError($"[PurchaseService] Invalid receipt: {productInfo.packId} - {ex.Message}");
                }
            }

            foreach (var product in products)
            {
                if (!deferredPackIds.Contains(product.packId))
                {
                    EntitlementStore.Instance.ClearPackPending(product.packId);
                }
            }

            int grantedCount = 0;
            int revokedCount = 0;
            foreach (var product in products)
            {
                bool isOwned = EntitlementStore.Instance.HasPack(product.packId);
                if (validPackIds.Contains(product.packId))
                {
                    if (!isOwned)
                        grantedCount++;
                    EntitlementStore.Instance.GrantPack(product.packId);
                }
                else
                {
                    if (isOwned)
                        revokedCount++;
                    EntitlementStore.Instance.RevokePack(product.packId);
                }
            }
            Debug.Log($"[PurchaseService] レシート検証完了 - 付与: {grantedCount}個, 剥奪: {revokedCount}個");
#else
            // レシート検証が無効な場合、または対応プラットフォーム以外の場合は検証なしで付与
            Debug.LogWarning("[PurchaseService] レシート検証は無効です。返金チェックは行われません。");
            foreach (var product in storeController.products.all)
            {
                if (product.hasReceipt)
                {
                    var productInfo = GetProductInfoByProductId(product.definition.id);
                    if (productInfo != null)
                    {
                        EntitlementStore.Instance.GrantPack(productInfo.packId);
                        Debug.Log($"[PurchaseService] 既存購入を確認（検証なし）: {productInfo.packId}");
                    }
                }
            }
#endif
        }
        
        #region IStoreListener Implementation
        
        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            Debug.Log("[PurchaseService] Unity IAP初期化完了");
            
            storeController = controller;
            storeExtensionProvider = extensions;
            isInitialized = true;
            
            // 既存の購入を確認して権利を付与（返金チェック含む）
            ValidateAndGrantPurchases();
            
            OnIAPInitialized?.Invoke();
        }
        
        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            Debug.LogError($"[PurchaseService] Unity IAP初期化失敗: {error} - {message}");
        }
        
        public void OnInitializeFailed(InitializationFailureReason error)
        {
            OnInitializeFailed(error, "");
        }
        
        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            var productInfo = GetProductInfoByProductId(args.purchasedProduct.definition.id);
            if (productInfo != null)
            {
                Debug.Log($"[PurchaseService] 購入成功: {productInfo.packId}");
#if UNITY_ANDROID
                var googleExtensions = storeExtensionProvider?.GetExtension<IGooglePlayStoreExtensions>();
                if (googleExtensions != null && googleExtensions.IsPurchasedProductDeferred(args.purchasedProduct))
                {
                    EntitlementStore.Instance.MarkPackPending(productInfo.packId);
                    Debug.LogWarning($"[PurchaseService] Purchase deferred (pending): {productInfo.packId}");
                    currentOnDeferred?.Invoke(productInfo.packId);
                    return PurchaseProcessingResult.Complete;
                }
#endif

                EntitlementStore.Instance.ClearPackPending(productInfo.packId);
                EntitlementStore.Instance.GrantPack(productInfo.packId);
                currentOnSuccess?.Invoke(productInfo.packId);
            }
            else
            {
                Debug.LogError($"[PurchaseService] 不明な商品の購入: {args.purchasedProduct.definition.id}");
            }
            
            return PurchaseProcessingResult.Complete;
        }
        
        public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
        {
            var productInfo = GetProductInfoByProductId(product.definition.id);
            string packId = productInfo?.packId ?? product.definition.id;
            
            Debug.LogError($"[PurchaseService] 購入失敗: {packId} - {failureDescription.reason}: {failureDescription.message}");
            
            if (failureDescription.reason == PurchaseFailureReason.DuplicateTransaction)
            {
                // 重複購入の場合は権利を付与
                if (productInfo != null)
                {
#if UNITY_ANDROID
                    var googleExtensions = storeExtensionProvider?.GetExtension<IGooglePlayStoreExtensions>();
                    if (googleExtensions != null && googleExtensions.IsPurchasedProductDeferred(product))
                    {
                        EntitlementStore.Instance.MarkPackPending(productInfo.packId);
                        Debug.LogWarning($"[PurchaseService] Purchase deferred (pending): {productInfo.packId}");
                        currentOnDeferred?.Invoke(productInfo.packId);
                        return;
                    }
#endif

                    EntitlementStore.Instance.ClearPackPending(productInfo.packId);
                    EntitlementStore.Instance.GrantPack(productInfo.packId);
                    currentOnSuccess?.Invoke(productInfo.packId);
                }
            }
            else
            {
                currentOnFailed?.Invoke(packId, failureDescription.message);
            }
        }
        
        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            // v4互換性のため、PurchaseFailureDescriptionを作成してv5メソッドに委譲
            var failureDescription = new PurchaseFailureDescription(product, failureReason, failureReason.ToString());
            OnPurchaseFailed(product, failureDescription);
        }
        
        #endregion
        
        [System.Serializable]
        public class ProductInfo
        {
            public string productId;
            public string packId;
            public ProductType productType;
            
            public ProductInfo(string productId, string packId, ProductType productType)
            {
                this.productId = productId;
                this.packId = packId;
                this.productType = productType;
            }
        }
    }
}
