using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace WakuWaku.IAP
{
    /// <summary>
    /// 購入画面（Paywall）を管理するクラス
    /// パック購入の説明、親ゲート、購入処理を統合
    /// 同一パネル内で購入画面と親ゲートを切り替え
    /// </summary>
    public class Paywall : MonoBehaviour
    {
        [Header("Main Panel")]
        [SerializeField] private GameObject purchaseContentPanel;
        [SerializeField] private Button closeButton;
        
        [Header("Purchase Content")]
        [SerializeField] private GameObject purchaseContent;
        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private TextMeshProUGUI description;
        [SerializeField] private TextMeshProUGUI price;
        [SerializeField] private Button purchaseButton;
        [SerializeField] private Button restoreButton;
        [SerializeField] private TextMeshProUGUI statusText;
        
        [Header("Parental Gate Content")]
        [SerializeField] private GameObject parentalGateContent;
        [SerializeField] private TextMeshProUGUI parentalGateTitle;
        [SerializeField] private TextMeshProUGUI instruction;
        [SerializeField] private TextMeshProUGUI question;
        [SerializeField] private TMP_InputField answerInputField;
        [SerializeField] private Button submitButton;
        [SerializeField] private TextMeshProUGUI errorText;
        [SerializeField] private NumberPadController numberPad;
        
        [Header("Settings")]
        [SerializeField] private float autoHideErrorDelay = 3f;

        private string currentPackId;
        private Action onPurchaseSuccess;
        private Action onClose;
        private bool isShowingSuccessResult = false;
        private int correctAnswer;
        private bool isFullyInitialized = false;
        
        public static Paywall Instance { get; private set; }
        
        void Awake()
        {
            Debug.Log("[Paywall] Awake呼び出し");
            
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                
                Debug.Log("[Paywall] Instanceを設定しました");
                
                // 初期状態では非表示
                if (purchaseContentPanel != null)
                {
                    purchaseContentPanel.SetActive(false);
                    Debug.Log("[Paywall] purchaseContentPanelを非表示にしました");
                }
                else
                {
                    Debug.LogError("[Paywall] purchaseContentPanelが設定されていません！");
                }
                    
                InitializeUI();
                
                // 初期化完了
                isFullyInitialized = true;
                Debug.Log("[Paywall] 初期化完了");
            }
            else
            {
                Debug.Log("[Paywall] 既にInstanceが存在するため破棄します");
                Destroy(gameObject);
            }
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
            
            if (submitButton != null)
            {
                submitButton.onClick.AddListener(OnSubmitClicked);
            }
            
            if (answerInputField != null)
            {
                answerInputField.onEndEdit.AddListener(OnAnswerEndEdit);
                answerInputField.contentType = TMP_InputField.ContentType.IntegerNumber;
            }
            
            if (statusText != null)
            {
                statusText.gameObject.SetActive(false);
            }
            
            if (errorText != null)
            {
                errorText.gameObject.SetActive(false);
            }
            
            // 初期状態で親ゲートと購入コンテンツは非表示
            if (parentalGateContent != null)
            {
                parentalGateContent.SetActive(false);
            }
            
            if (purchaseContent != null)
            {
                purchaseContent.SetActive(false);
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
            if (!isFullyInitialized)
            {
                Debug.LogWarning("[Paywall] まだ初期化中です。1フレーム後に再試行します");
                // 非表示のGameObjectではコルーチンが使えないため、Invokeを使用
                Invoke(nameof(RetryShowPaywall), 0.1f);
                // パラメータを一時保存
                currentPackId = packId;
                this.onPurchaseSuccess = onSuccess;
                this.onClose = onClose;
                return;
            }
            
            currentPackId = packId;
            this.onPurchaseSuccess = onSuccess;
            this.onClose = onClose;
            isShowingSuccessResult = false;
            
            UpdateUI();
            ShowPurchaseUI();
            
            if (purchaseContentPanel != null)
            {
                purchaseContentPanel.SetActive(true);
            }
            
            HideStatus();
            SetLoading(false);
            
            Debug.Log($"[Paywall] Paywall表示: {packId}");
        }
        
        /// <summary>
        /// ShowPaywallを再試行
        /// </summary>
        private void RetryShowPaywall()
        {
            if (isFullyInitialized)
            {
                Debug.Log("[Paywall] 再試行: Paywallを表示します");
                ShowPaywall(currentPackId, onPurchaseSuccess, onClose);
            }
            else
            {
                Debug.LogWarning("[Paywall] まだ初期化中です。さらに再試行します");
                Invoke(nameof(RetryShowPaywall), 0.1f);
            }
        }
        
        /// <summary>
        /// Paywallを非表示
        /// </summary>
        public void HidePaywall()
        {
            if (purchaseContentPanel != null)
            {
                purchaseContentPanel.SetActive(false);
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
            if (title != null)
            {
                title.text = $"追加パック{GetPackDisplayName(currentPackId)}の購入";
            }
            
            // 説明文設定
            if (description != null)
            {
                if (gameCount > 0)
                {
                    description.text = $"このパックの{gameCount}ゲームが\n遊べるようになります";
                }
                else
                {
                    description.text = "新しいゲームが遊べるようになります";
                }
            }
            
            // 価格設定
            if (price != null)
            {
                if (PurchaseService.Instance != null && PurchaseService.Instance.IsInitialized)
                {
                    string priceStr = PurchaseService.Instance.GetProductPrice(currentPackId);
                    price.text = priceStr;
                }
                else
                {
                    price.text = "読み込み中...";
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
                case "pack_04": return "4";
                case "pack_05": return "5";
                case "pack_06": return "6";
                case "pack_07": return "7";
                case "pack_08": return "8";
                case "pack_09": return "9";
                case "pack_10": return "10";
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
            
            // 購入コンテンツを非表示にして親ゲートパネルを表示
            ShowParentalGate();
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
            
            // コールバック方式で購入
            PurchaseService.Instance.PurchaseProduct(
                currentPackId,
                onSuccess: OnPurchaseSuccess,
                onFailed: OnPurchaseFailed
            );
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
            
            // コールバック方式で復元
            PurchaseService.Instance.RestorePurchases(
                onCompleted: OnRestoreCompleted
            );
        }
        
        /// <summary>
        /// 閉じるボタンクリック時の処理
        /// </summary>
        private void OnCloseClicked()
        {
            Debug.Log("[Paywall] 閉じるボタンクリック");
            
            // 親ゲート表示中でも購入画面表示中でも、閉じるボタンは全体を閉じる
            onClose?.Invoke();
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
            if (title != null)
            {
                title.text = "購入完了";
            }
            
            if (description != null)
            {
                description.text = "ゲームパックの購入が完了しました。\n「×」ボタンを押して\nメニューに戻ります。";
            }
            
            // statusTextを直接表示
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
        /// 復元成功結果画面を表示
        /// </summary>
        private void ShowRestoreSuccessResult()
        {
            isShowingSuccessResult = true;
            
            // 購入UIを非表示
            HidePurchaseUI();
            
            // 結果テキストを表示
            if (title != null)
            {
                title.text = "復元完了";
            }
            
            if (description != null)
            {
                description.text = "ゲームパックが復元されました！";
            }
            
            // statusTextを直接表示
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
            
            Debug.Log("[Paywall] 復元成功結果画面を表示");
        }
        
        /// <summary>
        /// 購入UIを表示
        /// </summary>
        private void ShowPurchaseUI()
        {
            // 購入コンテンツを表示
            if (purchaseContent != null)
            {
                purchaseContent.SetActive(true);
            }
            
            // 親ゲートを非表示
            if (parentalGateContent != null)
            {
                parentalGateContent.SetActive(false);
            }
            
            if (price != null)
            {
                price.gameObject.SetActive(true);
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
            if (price != null)
            {
                price.gameObject.SetActive(false);
            }
            
            if (purchaseButton != null)
            {
                purchaseButton.gameObject.SetActive(false);
            }
            
            if (restoreButton != null)
            {
                restoreButton.gameObject.SetActive(false);
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
                // 復元されていたら復元成功結果画面を表示
                ShowRestoreSuccessResult();
                onPurchaseSuccess?.Invoke();
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
            return purchaseContentPanel != null && purchaseContentPanel.activeInHierarchy;
        }
        
        #region 親ゲート関連
        
        /// <summary>
        /// 親ゲートパネルを表示
        /// </summary>
        private void ShowParentalGate()
        {
            Debug.Log("[Paywall] ShowParentalGate呼び出し");
            
            // デバッグ: 参照が同じか確認
            if (purchaseContent == parentalGateContent)
            {
                Debug.LogError("[Paywall] purchaseContentとparentalGateContentが同じオブジェクトを参照しています！");
            }
            
            // メインパネルの状態を確認
            if (purchaseContentPanel != null)
            {
                Debug.Log($"[Paywall] purchaseContentPanel状態: activeInHierarchy={purchaseContentPanel.activeInHierarchy}, activeSelf={purchaseContentPanel.activeSelf}");
                if (!purchaseContentPanel.activeSelf)
                {
                    Debug.LogWarning("[Paywall] purchaseContentPanelが非表示です。表示します");
                    purchaseContentPanel.SetActive(true);
                }
            }
            
            // 購入コンテンツを非表示
            if (purchaseContent != null)
            {
                purchaseContent.SetActive(false);
                Debug.Log($"[Paywall] purchaseContent非表示 (activeInHierarchy={purchaseContent.activeInHierarchy})");
            }
            else
            {
                Debug.LogWarning("[Paywall] purchaseContentがnullです");
            }
            
            // 親ゲートパネルを表示
            if (parentalGateContent != null)
            {
                Debug.Log($"[Paywall] parentalGateContent表示前: activeInHierarchy={parentalGateContent.activeInHierarchy}, activeSelf={parentalGateContent.activeSelf}");
                parentalGateContent.SetActive(true);
                Debug.Log($"[Paywall] parentalGateContent表示後: activeInHierarchy={parentalGateContent.activeInHierarchy}, activeSelf={parentalGateContent.activeSelf}");
            }
            else
            {
                Debug.LogError("[Paywall] parentalGateContentがnullです！");
            }
            
            // 親ゲート内のUI要素を明示的に表示
            if (parentalGateTitle != null)
            {
                parentalGateTitle.gameObject.SetActive(true);
                parentalGateTitle.text = "保護者確認";
            }
            
            if (instruction != null)
            {
                instruction.gameObject.SetActive(true);
                instruction.text = "購入するには、以下の計算問題に\n答えてください";
            }
            
            if (question != null)
            {
                question.gameObject.SetActive(true);
            }
            
            if (answerInputField != null)
            {
                answerInputField.gameObject.SetActive(true);
            }
            
            if (submitButton != null)
            {
                submitButton.gameObject.SetActive(true);
            }
            
            if (numberPad != null)
            {
                numberPad.gameObject.SetActive(true);
            }
            
            // 問題を生成
            GenerateQuestion();
            
            // 入力フィールドをクリア
            if (answerInputField != null)
            {
                answerInputField.text = "";
                
                // NumberPadがある場合はキーボードを無効化
                if (numberPad != null)
                {
                    answerInputField.readOnly = true;
                }
                else
                {
                    answerInputField.Select();
                    answerInputField.ActivateInputField();
                }
            }
            
            HideError();
            
            Debug.Log("[Paywall] 親ゲートパネル表示");
        }
        
        /// <summary>
        /// 親ゲートパネルを非表示にして購入画面に戻る
        /// </summary>
        private void HideParentalGate()
        {
            // 親ゲートパネルを非表示
            if (parentalGateContent != null)
            {
                parentalGateContent.SetActive(false);
            }
            
            // 購入コンテンツを表示
            if (purchaseContent != null)
            {
                purchaseContent.SetActive(true);
            }
            
            Debug.Log("[Paywall] 購入画面に戻る");
        }
        
        /// <summary>
        /// 掛け算問題を生成（1を除外）
        /// </summary>
        private void GenerateQuestion()
        {
            // 2-9の範囲でランダムに2つの数字を選択（1を除外）
            int num1 = UnityEngine.Random.Range(2, 10);
            int num2 = UnityEngine.Random.Range(2, 10);
            
            correctAnswer = num1 * num2;
            
            if (question != null)
            {
                question.text = $"{num1} × {num2} = ?";
            }
            
            Debug.Log($"[Paywall] 問題生成: {num1} × {num2} = {correctAnswer}");
        }
        
        /// <summary>
        /// 送信ボタンクリック時の処理
        /// </summary>
        private void OnSubmitClicked()
        {
            CheckAnswer();
        }
        
        /// <summary>
        /// 入力フィールドでEnterキーが押された時の処理
        /// </summary>
        private void OnAnswerEndEdit(string value)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                CheckAnswer();
            }
        }
        
        /// <summary>
        /// 回答をチェック
        /// </summary>
        private void CheckAnswer()
        {
            if (answerInputField == null)
                return;
                
            string inputText = answerInputField.text.Trim();
            
            if (string.IsNullOrEmpty(inputText))
            {
                ShowError("答えを入力してください");
                return;
            }
            
            if (int.TryParse(inputText, out int userAnswer))
            {
                if (userAnswer == correctAnswer)
                {
                    Debug.Log("[Paywall] 正解！親ゲート通過");
                    HideError();
                    HideParentalGate();
                    
                    // 購入処理を開始
                    StartPurchase();
                }
                else
                {
                    Debug.Log($"[Paywall] 不正解: 入力={userAnswer}, 正解={correctAnswer}");
                    ShowError("答えが\n間違っています");
                    
                    // 不正解の場合は新しい問題を生成
                    GenerateQuestion();
                    answerInputField.text = "";
                    
                    if (numberPad == null)
                    {
                        answerInputField.Select();
                        answerInputField.ActivateInputField();
                    }
                }
            }
            else
            {
                ShowError("数字を入力してください");
            }
        }
        
        /// <summary>
        /// エラーメッセージを表示
        /// </summary>
        private void ShowError(string message)
        {
            if (errorText != null)
            {
                errorText.text = message;
                errorText.gameObject.SetActive(true);
                
                // 一定時間後に自動で非表示
                CancelInvoke(nameof(HideError));
                Invoke(nameof(HideError), autoHideErrorDelay);
            }
            
            Debug.LogWarning($"[Paywall] エラー: {message}");
        }
        
        /// <summary>
        /// エラーメッセージを非表示
        /// </summary>
        private void HideError()
        {
            if (errorText != null)
            {
                errorText.gameObject.SetActive(false);
            }
        }
        
        void OnDestroy()
        {
            CancelInvoke();
        }
        
        #endregion
        
        #region デバッグ用メソッド
        
        /// <summary>
        /// デバッグ用：正解を表示
        /// </summary>
        [ContextMenu("Debug Show Answer")]
        private void DebugShowAnswer()
        {
            Debug.Log($"[Paywall] デバッグ - 現在の正解: {correctAnswer}");
        }
        
        /// <summary>
        /// デバッグ用：自動正解（親ゲート通過）
        /// </summary>
        [ContextMenu("Debug Auto Correct")]
        private void DebugAutoCorrect()
        {
            if (parentalGateContent != null && parentalGateContent.activeInHierarchy)
            {
                Debug.Log("[Paywall] デバッグ - 自動正解");
                HideParentalGate();
                StartPurchase();
            }
        }
        
        #endregion
        
    }
}
