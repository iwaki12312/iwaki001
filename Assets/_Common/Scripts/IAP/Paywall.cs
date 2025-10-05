using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace WakuWaku.IAP
{
    /// <summary>
    /// 購入画面（Paywall）を管理するクラス
    /// パック購入の説明、親ゲート、購入処理を統合
    /// </summary>
    public class Paywall : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject paywallPanel;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI priceText;
        [SerializeField] private Button purchaseButton;
        [SerializeField] private Button restoreButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private GameObject loadingIndicator;
        
        [Header("Settings")]
        [SerializeField] private float statusMessageDuration = 3f;
        
        private string currentPackId;
        private Action onPurchaseSuccess;
        private Action onClose;
        private bool isShowingSuccessResult = false;
        
        public static Paywall Instance { get; private set; }
        
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeUI();
                SubscribeToEvents();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        void Start()
        {
            // 初期状態では非表示
            if (paywallPanel != null)
                paywallPanel.SetActive(false);
        }
        
        /// <summary>
        /// UIの初期化
        /// </summary>
        private void InitializeUI()
        {
            if (purchaseButton != null)
            {
                purchaseButton.onClick.AddListener(OnPurchaseClicked);
            }
            
            if (restoreButton != null)
            {
                restoreButton.onClick.AddListener(OnRestoreClicked);
            }
            
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(OnCloseClicked);
            }
            
            if (statusText != null)
            {
                statusText.gameObject.SetActive(false);
            }
            
            if (loadingIndicator != null)
            {
                loadingIndicator.SetActive(false);
            }
        }
        
        /// <summary>
        /// イベントの購読
        /// </summary>
        private void SubscribeToEvents()
        {
            if (PurchaseService.Instance != null)
            {
                PurchaseService.Instance.OnPurchaseSuccess += OnPurchaseSuccess;
                PurchaseService.Instance.OnIAPPurchaseFailed += OnPurchaseFailed;
                PurchaseService.Instance.OnRestoreCompleted += OnRestoreCompleted;
            }
        }
        
        /// <summary>
        /// Paywallを表示
        /// </summary>
        /// <param name="packId">購入するパックID</param>
        /// <param name="onSuccess">購入成功時のコールバック</param>
        /// <param name="onClose">閉じる時のコールバック</param>
        public void ShowPaywall(string packId, Action onSuccess = null, Action onClose = null)
        {
            currentPackId = packId;
            this.onPurchaseSuccess = onSuccess;
            this.onClose = onClose;
            isShowingSuccessResult = false;
            
            UpdateUI();
            ShowPurchaseUI();
            
            if (paywallPanel != null)
            {
                paywallPanel.SetActive(true);
            }
            
            HideStatus();
            SetLoading(false);
            
            Debug.Log($"[Paywall] Paywall表示: {packId}");
        }
        
        /// <summary>
        /// Paywallを非表示
        /// </summary>
        public void HidePaywall()
        {
            if (paywallPanel != null)
            {
                paywallPanel.SetActive(false);
            }
            
            currentPackId = null;
            onPurchaseSuccess = null;
            onClose = null;
            isShowingSuccessResult = false;
            
            Debug.Log("[Paywall] Paywall非表示");
        }
        
        /// <summary>
        /// UIを更新
        /// </summary>
        private void UpdateUI()
        {
            if (string.IsNullOrEmpty(currentPackId))
                return;
            
            // パック情報を取得
            var gamesInPack = FeatureGate.GetGamesInPack(currentPackId);
            int gameCount = gamesInPack.Count;
            
            // タイトル設定
            if (titleText != null)
            {
                titleText.text = $"ゲームパック {GetPackDisplayName(currentPackId)}";
            }
            
            // 説明文設定
            if (descriptionText != null)
            {
                if (gameCount > 0)
                {
                    descriptionText.text = $"このパックの{gameCount}ゲームが解放されます";
                }
                else
                {
                    descriptionText.text = "新しいゲームが解放されます";
                }
            }
            
            // 価格設定
            if (priceText != null)
            {
                if (PurchaseService.Instance != null && PurchaseService.Instance.IsInitialized)
                {
                    string price = PurchaseService.Instance.GetProductPrice(currentPackId);
                    priceText.text = price;
                }
                else
                {
                    priceText.text = "読み込み中...";
                }
            }
            
            // ボタンの状態設定
            UpdateButtonStates();
        }
        
        /// <summary>
        /// ボタンの状態を更新
        /// </summary>
        private void UpdateButtonStates()
        {
            bool isPurchaseAvailable = PurchaseService.Instance != null && 
                                     PurchaseService.Instance.IsInitialized && 
                                     PurchaseService.Instance.IsProductAvailable(currentPackId);
            
            if (purchaseButton != null)
            {
                purchaseButton.interactable = isPurchaseAvailable;
            }
            
            if (restoreButton != null)
            {
                restoreButton.interactable = PurchaseService.Instance != null && 
                                           PurchaseService.Instance.IsInitialized;
            }
        }
        
        /// <summary>
        /// パック表示名を取得
        /// </summary>
        private string GetPackDisplayName(string packId)
        {
            switch (packId)
            {
                case "pack_01": return "1";
                case "pack_02": return "2";
                case "pack_03": return "3";
                default: return packId.Replace("pack_", "");
            }
        }
        
        /// <summary>
        /// 購入ボタンクリック時の処理
        /// </summary>
        private void OnPurchaseClicked()
        {
            if (string.IsNullOrEmpty(currentPackId))
                return;
            
            Debug.Log($"[Paywall] 購入ボタンクリック: {currentPackId}");
            
            // 親ゲートを表示
            if (ParentalGate.Instance != null)
            {
                ParentalGate.Instance.ShowGate(
                    onSuccess: () => {
                        // 親ゲート通過後に購入処理開始
                        StartPurchase();
                    },
                    onCancel: () => {
                        Debug.Log("[Paywall] 親ゲートがキャンセルされました");
                    }
                );
            }
            else
            {
                Debug.LogError("[Paywall] ParentalGateが見つかりません");
                ShowStatus("エラーが発生しました", true);
            }
        }
        
        /// <summary>
        /// 購入処理を開始
        /// </summary>
        private void StartPurchase()
        {
            if (PurchaseService.Instance == null)
            {
                ShowStatus("購入サービスが利用できません", true);
                return;
            }
            
            SetLoading(true);
            ShowStatus("購入処理中...", false);
            
            PurchaseService.Instance.PurchaseProduct(currentPackId);
        }
        
        /// <summary>
        /// 復元ボタンクリック時の処理
        /// </summary>
        private void OnRestoreClicked()
        {
            Debug.Log("[Paywall] 復元ボタンクリック");
            
            if (PurchaseService.Instance == null)
            {
                ShowStatus("購入サービスが利用できません", true);
                return;
            }
            
            SetLoading(true);
            ShowStatus("購入を復元中...", false);
            
            PurchaseService.Instance.RestorePurchases();
        }
        
        /// <summary>
        /// 閉じるボタンクリック時の処理
        /// </summary>
        private void OnCloseClicked()
        {
            Debug.Log("[Paywall] 閉じるボタンクリック");
            onClose?.Invoke();
            
            // if (isShowingSuccessResult)
            // {
            //     // 購入成功結果表示中の場合はメニューに戻る
            //     Debug.Log("[Paywall] 購入成功後のメニューへ戻る");
            //     onClose?.Invoke();
            // }
            // else
            // {
            //     // 通常の閉じる処理
            //     onClose?.Invoke();
            // }
            
            HidePaywall();
        }
        
        /// <summary>
        /// 購入成功時の処理
        /// </summary>
        private void OnPurchaseSuccess(string packId)
        {
            if (packId == currentPackId)
            {
                SetLoading(false);
                
                Debug.Log($"[Paywall] 購入成功: {packId}");
                
                // 購入成功結果画面を表示
                ShowSuccessResult();
                
                // コールバックを実行（ゲームのロック解除などの処理）
                onPurchaseSuccess.Invoke();
            }
        }
        
        /// <summary>
        /// 購入成功結果画面を表示
        /// </summary>
        private void ShowSuccessResult()
        {
            isShowingSuccessResult = true;
            
            // 購入UIを非表示
            HidePurchaseUI();
            
            // 結果テキストを表示
            if (titleText != null)
            {
                titleText.text = "購入完了";
            }
            
            if (descriptionText != null)
            {
                descriptionText.text = "ゲームパックの購入が完了しました！";
            }
            
            if (statusText != null)
            {
                statusText.text = "閉じるボタンを押してメニューに戻ります";
                statusText.color = Color.white;
                statusText.gameObject.SetActive(true);
            }
            
            // 閉じるボタンのみ表示
            if (closeButton != null)
            {
                closeButton.gameObject.SetActive(true);
            }
            
            Debug.Log("[Paywall] 購入成功結果画面を表示");
        }
        
        /// <summary>
        /// 購入UIを表示
        /// </summary>
        private void ShowPurchaseUI()
        {
            if (priceText != null)
            {
                priceText.gameObject.SetActive(true);
            }
            
            if (purchaseButton != null)
            {
                purchaseButton.gameObject.SetActive(true);
            }
            
            if (restoreButton != null)
            {
                restoreButton.gameObject.SetActive(true);
            }
        }
        
        /// <summary>
        /// 購入UIを非表示
        /// </summary>
        private void HidePurchaseUI()
        {
            if (priceText != null)
            {
                priceText.gameObject.SetActive(false);
            }
            
            if (purchaseButton != null)
            {
                purchaseButton.gameObject.SetActive(false);
            }
            
            if (restoreButton != null)
            {
                restoreButton.gameObject.SetActive(false);
            }
            
            if (loadingIndicator != null)
            {
                loadingIndicator.SetActive(false);
            }
        }
        
        /// <summary>
        /// 購入失敗時の処理
        /// </summary>
        private void OnPurchaseFailed(string packId, string error)
        {
            if (packId == currentPackId)
            {
                SetLoading(false);
                ShowStatus($"購入に失敗しました: {error}", true);
                
                Debug.LogError($"[Paywall] 購入失敗: {packId} - {error}");
            }
        }
        
        /// <summary>
        /// 復元完了時の処理
        /// </summary>
        private void OnRestoreCompleted()
        {
            SetLoading(false);
            
            Debug.Log("[Paywall] 復元完了");
            
            // 現在のパックが復元されたかチェック
            if (!string.IsNullOrEmpty(currentPackId) && FeatureGate.IsPackOwned(currentPackId))
            {
                // 復元されていたら成功結果画面を表示
                ShowSuccessResult();
                onPurchaseSuccess.Invoke();
            }
            else
            {
                ShowStatus("復元が完了しました", false);
            }
        }
        
        /// <summary>
        /// ステータスメッセージを表示
        /// </summary>
        private void ShowStatus(string message, bool isError)
        {
            if (statusText != null)
            {
                statusText.text = message;
                statusText.color = isError ? Color.red : Color.white;
                statusText.gameObject.SetActive(true);
                
                // 一定時間後に自動で非表示
                CancelInvoke(nameof(HideStatus));
                Invoke(nameof(HideStatus), statusMessageDuration);
            }
        }
        
        /// <summary>
        /// ステータスメッセージを非表示
        /// </summary>
        private void HideStatus()
        {
            if (statusText != null)
            {
                statusText.gameObject.SetActive(false);
            }
        }
        
        /// <summary>
        /// ローディング表示の切り替え
        /// </summary>
        private void SetLoading(bool isLoading)
        {
            if (loadingIndicator != null)
            {
                loadingIndicator.SetActive(isLoading);
            }
            
            // ローディング中はボタンを無効化
            if (purchaseButton != null)
            {
                purchaseButton.interactable = !isLoading && PurchaseService.Instance != null && 
                                            PurchaseService.Instance.IsProductAvailable(currentPackId);
            }
            
            if (restoreButton != null)
            {
                restoreButton.interactable = !isLoading && PurchaseService.Instance != null && 
                                           PurchaseService.Instance.IsInitialized;
            }
        }
        
        /// <summary>
        /// Paywallが表示中かチェック
        /// </summary>
        public bool IsShowing()
        {
            return paywallPanel != null && paywallPanel.activeInHierarchy;
        }
        
        void OnDestroy()
        {
            CancelInvoke();
            
            // イベントの購読解除
            if (PurchaseService.Instance != null)
            {
                PurchaseService.Instance.OnPurchaseSuccess -= OnPurchaseSuccess;
                PurchaseService.Instance.OnIAPPurchaseFailed -= OnPurchaseFailed;
                PurchaseService.Instance.OnRestoreCompleted -= OnRestoreCompleted;
            }
        }
    }
}
