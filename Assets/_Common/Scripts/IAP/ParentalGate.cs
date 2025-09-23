using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace WakuWaku.IAP
{
    /// <summary>
    /// 親ゲート（掛け算クイズ）を管理するクラス
    /// 1を含まない1桁×1桁の掛け算を出題
    /// </summary>
    public class ParentalGate : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject gatePanel;
        [SerializeField] private TextMeshProUGUI questionText;
        [SerializeField] private TMP_InputField answerInput;
        [SerializeField] private Button submitButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private TextMeshProUGUI errorText;
        
        [Header("Settings")]
        [SerializeField] private float autoHideErrorDelay = 3f;
        
        private int correctAnswer;
        private Action onSuccess;
        private Action onCancel;
        
        public static ParentalGate Instance { get; private set; }
        
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeUI();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        void Start()
        {
            // 初期状態では非表示
            if (gatePanel != null)
                gatePanel.SetActive(false);
        }
        
        /// <summary>
        /// UIの初期化
        /// </summary>
        private void InitializeUI()
        {
            if (submitButton != null)
            {
                submitButton.onClick.AddListener(OnSubmitClicked);
            }
            
            if (cancelButton != null)
            {
                cancelButton.onClick.AddListener(OnCancelClicked);
            }
            
            if (answerInput != null)
            {
                answerInput.onEndEdit.AddListener(OnAnswerEndEdit);
                answerInput.contentType = TMP_InputField.ContentType.IntegerNumber;
            }
            
            if (errorText != null)
            {
                errorText.gameObject.SetActive(false);
            }
        }
        
        /// <summary>
        /// 親ゲートを表示
        /// </summary>
        /// <param name="onSuccess">成功時のコールバック</param>
        /// <param name="onCancel">キャンセル時のコールバック</param>
        public void ShowGate(Action onSuccess, Action onCancel = null)
        {
            this.onSuccess = onSuccess;
            this.onCancel = onCancel;
            
            GenerateQuestion();
            
            if (gatePanel != null)
            {
                gatePanel.SetActive(true);
            }
            
            if (answerInput != null)
            {
                answerInput.text = "";
                answerInput.Select();
                answerInput.ActivateInputField();
            }
            
            HideError();
            
            Debug.Log($"[ParentalGate] 親ゲート表示: 正解 = {correctAnswer}");
        }
        
        /// <summary>
        /// 親ゲートを非表示
        /// </summary>
        public void HideGate()
        {
            if (gatePanel != null)
            {
                gatePanel.SetActive(false);
            }
            
            onSuccess = null;
            onCancel = null;
            
            Debug.Log("[ParentalGate] 親ゲート非表示");
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
            
            if (questionText != null)
            {
                questionText.text = $"{num1} × {num2} = ?";
            }
            
            Debug.Log($"[ParentalGate] 問題生成: {num1} × {num2} = {correctAnswer}");
        }
        
        /// <summary>
        /// 送信ボタンクリック時の処理
        /// </summary>
        private void OnSubmitClicked()
        {
            CheckAnswer();
        }
        
        /// <summary>
        /// キャンセルボタンクリック時の処理
        /// </summary>
        private void OnCancelClicked()
        {
            Debug.Log("[ParentalGate] キャンセルされました");
            onCancel?.Invoke();
            HideGate();
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
            if (answerInput == null)
                return;
                
            string inputText = answerInput.text.Trim();
            
            if (string.IsNullOrEmpty(inputText))
            {
                ShowError("答えを入力してください");
                return;
            }
            
            if (int.TryParse(inputText, out int userAnswer))
            {
                if (userAnswer == correctAnswer)
                {
                    Debug.Log("[ParentalGate] 正解！親ゲート通過");
                    onSuccess.Invoke();
                    HideGate();
                }
                else
                {
                    Debug.Log($"[ParentalGate] 不正解: 入力={userAnswer}, 正解={correctAnswer}");
                    ShowError("答えが間違っています");
                    
                    // 不正解の場合は新しい問題を生成
                    GenerateQuestion();
                    answerInput.text = "";
                    answerInput.Select();
                    answerInput.ActivateInputField();
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
            
            Debug.LogWarning($"[ParentalGate] エラー: {message}");
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
        
        /// <summary>
        /// 親ゲートが表示中かチェック
        /// </summary>
        public bool IsShowing()
        {
            return gatePanel != null && gatePanel.activeInHierarchy;
        }
        
        void OnDestroy()
        {
            CancelInvoke();
        }
        
        #region デバッグ用メソッド
        
        /// <summary>
        /// デバッグ用：正解を表示
        /// </summary>
        [ContextMenu("Debug Show Answer")]
        private void DebugShowAnswer()
        {
            Debug.Log($"[ParentalGate] デバッグ - 現在の正解: {correctAnswer}");
        }
        
        /// <summary>
        /// デバッグ用：自動正解
        /// </summary>
        [ContextMenu("Debug Auto Correct")]
        private void DebugAutoCorrect()
        {
            if (answerInput != null)
            {
                answerInput.text = correctAnswer.ToString();
            }
        }
        
        #endregion
    }
}
