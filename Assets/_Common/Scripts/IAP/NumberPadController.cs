using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace WakuWaku.IAP
{
    /// <summary>
    /// 親ゲート用の数字パッドを管理するコントローラー
    /// </summary>
    public class NumberPadController : MonoBehaviour
    {
        [Header("参照")]
        [SerializeField] private TMP_InputField targetInputField; // 入力値を反映するInputField
        [SerializeField] private Button[] numberButtons; // 0-9のボタン配列
        [SerializeField] private Button clearButton; // クリアボタン

        private const int MAX_DIGITS = 3; // 最大入力桁数

        private void Start()
        {
            // ボタンのリスナー設定
            for (int i = 0; i < numberButtons.Length && i < 10; i++)
            {
                int number = i; // クロージャ対策
                numberButtons[i].onClick.AddListener(() => OnNumberClicked(number));
            }

            if (clearButton != null)
            {
                clearButton.onClick.AddListener(OnClearClicked);
            }
        }

        /// <summary>
        /// 数字ボタンがクリックされたときの処理
        /// </summary>
        private void OnNumberClicked(int number)
        {
            if (targetInputField == null) return;

            string currentText = targetInputField.text;

            // 最大桁数チェック
            if (currentText.Length >= MAX_DIGITS)
            {
                return;
            }

            // 数字を追加
            targetInputField.text = currentText + number.ToString();
        }

        /// <summary>
        /// クリアボタンがクリックされたときの処理
        /// </summary>
        private void OnClearClicked()
        {
            if (targetInputField == null) return;

            targetInputField.text = "";
        }

        /// <summary>
        /// ターゲットのInputFieldを設定(エディタ拡張から呼ばれる)
        /// </summary>
        public void SetTargetInputField(TMP_InputField inputField)
        {
            targetInputField = inputField;
        }
    }
}
