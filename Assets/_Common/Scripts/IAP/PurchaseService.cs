using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

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
            Action<string, string> onFailed = null)
        {
            currentOnSuccess = onSuccess;
            currentOnFailed = onFailed;
            
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
            
            // Androidでは自動的に復元されるため、手動で既存の購入をチェック
            int restoredCount = 0;
            foreach (var product in storeController.products.all)
            {
                if (product.hasReceipt)
                {
                    var productInfo = GetProductInfoByProductId(product.definition.id);
                    if (productInfo != null)
                    {
                        EntitlementStore.Instance.GrantPack(productInfo.packId);
                        restoredCount++;
                        Debug.Log($"[PurchaseService] 復元: {productInfo.packId}");
                    }
                }
            }
            
            Debug.Log($"[PurchaseService] 購入復元完了: {restoredCount}個の商品を復元");
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
        
        #region IStoreListener Implementation
        
        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            Debug.Log("[PurchaseService] Unity IAP初期化完了");
            
            storeController = controller;
            storeExtensionProvider = extensions;
            isInitialized = true;
            
            // 既存の購入を確認して権利を付与
            foreach (var product in controller.products.all)
            {
                if (product.hasReceipt)
                {
                    var productInfo = GetProductInfoByProductId(product.definition.id);
                    if (productInfo != null)
                    {
                        EntitlementStore.Instance.GrantPack(productInfo.packId);
                        Debug.Log($"[PurchaseService] 既存購入を確認: {productInfo.packId}");
                    }
                }
            }
            
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
