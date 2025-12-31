using UnityEngine;
using System.Collections;
using UnityEngine.Serialization;

namespace WakuWaku.IAP
{
    /// <summary>
    /// IAP全体を管理し、起動時の初期化と同期を行うクラス
    /// </summary>
    public class IAPManager : MonoBehaviour
    {
        [Header("Prefab References")]
        [SerializeField] private GameObject entitlementStorePrefab;
        [SerializeField] private GameObject purchaseServicePrefab;
[FormerlySerializedAs("paywallPrefab")]
[SerializeField] private GameObject purchaseContentPanelPrefab;

        [Header("Settings")]
        [SerializeField] private bool autoInitializeOnStart = true;
        [SerializeField] private float initializationTimeout = 30f;

        public static IAPManager Instance { get; private set; }

        public bool IsInitialized { get; private set; }

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);

                if (autoInitializeOnStart)
                {
                    StartCoroutine(InitializeIAPSystem());
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// IAPシステムの初期化
        /// </summary>
        public IEnumerator InitializeIAPSystem()
        {
            Debug.Log("[IAPManager] IAPシステム初期化開始");

            // 1. EntitlementStoreの初期化
            yield return StartCoroutine(InitializeEntitlementStore());

            // 2. PurchaseServiceの初期化
            yield return StartCoroutine(InitializePurchaseService());

            // 3. UI要素の初期化
            yield return StartCoroutine(InitializeUIComponents());

            // 4. 購入状況の同期
            yield return StartCoroutine(SyncPurchases());

            IsInitialized = true;
            Debug.Log("[IAPManager] IAPシステム初期化完了");

            // デバッグ用：アクセス状況を表示
            FeatureGate.DebugPrintAllGameAccess();
        }

        /// <summary>
        /// EntitlementStoreの初期化
        /// </summary>
        private IEnumerator InitializeEntitlementStore()
        {
            if (EntitlementStore.Instance == null)
            {
                if (entitlementStorePrefab != null)
                {
                    Instantiate(entitlementStorePrefab);
                }
                else
                {
                    // Prefabが設定されていない場合は動的に作成
                    GameObject entitlementStoreObj = new GameObject("EntitlementStore");
                    entitlementStoreObj.AddComponent<EntitlementStore>();
                }

                // EntitlementStoreの初期化を待つ
                float timeout = 5f;
                float elapsed = 0f;
                while (EntitlementStore.Instance == null && elapsed < timeout)
                {
                    elapsed += Time.deltaTime;
                    yield return null;
                }

                if (EntitlementStore.Instance != null)
                {
                    Debug.Log("[IAPManager] EntitlementStore初期化完了");
                }
                else
                {
                    Debug.LogError("[IAPManager] EntitlementStore初期化失敗");
                }
            }
        }

        /// <summary>
        /// PurchaseServiceの初期化
        /// </summary>
        private IEnumerator InitializePurchaseService()
        {
            if (PurchaseService.Instance == null)
            {
                if (purchaseServicePrefab != null)
                {
                    Instantiate(purchaseServicePrefab);
                }
                else
                {
                    // Prefabが設定されていない場合は動的に作成
                    GameObject purchaseServiceObj = new GameObject("PurchaseService");
                    purchaseServiceObj.AddComponent<PurchaseService>();
                }

                // PurchaseServiceの初期化を待つ
                float elapsed = 0f;
                while (PurchaseService.Instance == null && elapsed < initializationTimeout)
                {
                    elapsed += Time.deltaTime;
                    yield return null;
                }

                if (PurchaseService.Instance != null)
                {
                    Debug.Log("[IAPManager] PurchaseService作成完了");

                    // Unity IAPの初期化を待つ
                    elapsed = 0f;
                    while (!PurchaseService.Instance.IsInitialized && elapsed < initializationTimeout)
                    {
                        elapsed += Time.deltaTime;
                        yield return null;
                    }

                    if (PurchaseService.Instance.IsInitialized)
                    {
                        Debug.Log("[IAPManager] Unity IAP初期化完了");
                    }
                    else
                    {
                        Debug.LogWarning("[IAPManager] Unity IAP初期化タイムアウト");
                    }
                }
                else
                {
                    Debug.LogError("[IAPManager] PurchaseService初期化失敗");
                }
            }
        }

        /// <summary>
        /// UI要素の初期化
        /// </summary>
        private IEnumerator InitializeUIComponents()
        {
            // Paywallの初期化（親ゲート機能も含む）
            if (Paywall.Instance == null)
            {
                if (purchaseContentPanelPrefab != null)
                {
                    Instantiate(purchaseContentPanelPrefab);
                }
                else
                {
                    Debug.LogWarning("[IAPManager] PurchaseContentPanel Prefabが設定されていません");
                }
            }

            yield return null;
            Debug.Log("[IAPManager] UI要素初期化完了");
        }

        /// <summary>
        /// 購入状況の同期
        /// </summary>
        private IEnumerator SyncPurchases()
        {
            if (PurchaseService.Instance != null && PurchaseService.Instance.IsInitialized)
            {
                Debug.Log("[IAPManager] 購入状況同期開始");

                bool isCompleted = false;
                
                // 復元処理を実行（既存の購入を確認）
                PurchaseService.Instance.RestorePurchases(
                    onCompleted: () => {
                        isCompleted = true;
                        Debug.Log("[IAPManager] 購入状況同期完了");
                    }
                );

                // 復元完了を待つ（タイムアウトあり）
                float elapsed = 0f;
                float timeout = 10f;
                while (!isCompleted && elapsed < timeout)
                {
                    elapsed += Time.deltaTime;
                    yield return null;
                }
                
                if (!isCompleted)
                {
                    Debug.LogWarning("[IAPManager] 購入状況同期タイムアウト");
                }
            }
            else
            {
                Debug.LogWarning("[IAPManager] PurchaseServiceが利用できないため購入状況同期をスキップ");
            }
        }

        /// <summary>
        /// 手動でIAPシステムを初期化
        /// </summary>
        [ContextMenu("Initialize IAP System")]
        public void ManualInitialize()
        {
            if (!IsInitialized)
            {
                StartCoroutine(InitializeIAPSystem());
            }
            else
            {
                Debug.Log("[IAPManager] IAPシステムは既に初期化済みです");
            }
        }

        /// <summary>
        /// デバッグ用：全権利をクリア
        /// </summary>
        [ContextMenu("Debug Clear All Entitlements")]
        public void DebugClearAllEntitlements()
        {
            if (EntitlementStore.Instance != null)
            {
                EntitlementStore.Instance.ClearAllEntitlements();
                Debug.Log("[IAPManager] デバッグ：全権利をクリアしました");
            }
        }

        /// <summary>
        /// デバッグ用：Pack1を付与
        /// </summary>
        [ContextMenu("Debug Grant Pack 01")]
        public void DebugGrantPack01()
        {
            if (EntitlementStore.Instance != null)
            {
                EntitlementStore.Instance.GrantPack("pack_01");
                Debug.Log("[IAPManager] デバッグ：Pack01を付与しました");
            }
        }

        /// <summary>
        /// デバッグ用：アクセス状況を表示
        /// </summary>
        [ContextMenu("Debug Print Access Status")]
        public void DebugPrintAccessStatus()
        {
            FeatureGate.DebugPrintAllGameAccess();

            if (EntitlementStore.Instance != null)
            {
                EntitlementStore.Instance.DebugPrintEntitlements();
            }
        }

        void OnApplicationPause(bool pauseStatus)
        {
            // アプリが復帰した時に購入状況を再同期
            if (!pauseStatus && IsInitialized)
            {
                StartCoroutine(SyncPurchases());
            }
        }
        
        // このメソッドがあると購入状態が頻繁に同期されるのでコメントアウト
        // void OnApplicationFocus(bool hasFocus)
        // {
        //     // アプリがフォーカスを取得した時に購入状況を再同期
        //     if (hasFocus && IsInitialized)
        //     {
        //         StartCoroutine(SyncPurchases());
        //     }
        // }
    }
}
