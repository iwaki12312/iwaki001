using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

namespace WakuWaku.IAP.Editor
{
    /// <summary>
    /// ParentalGate用の数字パッドUIを自動生成するエディタ拡張
    /// </summary>
    public static class NumberPadSetup
    {
        [MenuItem("Tools/Setup NumberPad for ParentalGate")]
        public static void SetupNumberPad()
        {
            // ParentalGateオブジェクトを検索
            ParentalGate parentalGate = Object.FindObjectOfType<ParentalGate>(true);
            if (parentalGate == null)
            {
                EditorUtility.DisplayDialog("エラー", "ParentalGateオブジェクトが見つかりません。", "OK");
                return;
            }

            GameObject parentalGateGO = parentalGate.gameObject;

            // 既存のNumberPadを削除
            Transform existingNumberPad = parentalGateGO.transform.Find("NumberPad");
            if (existingNumberPad != null)
            {
                Object.DestroyImmediate(existingNumberPad.gameObject);
            }

            // NumberPad親オブジェクトを作成
            GameObject numberPadGO = new GameObject("NumberPad");
            numberPadGO.transform.SetParent(parentalGateGO.transform, false);
            
            RectTransform numberPadRect = numberPadGO.AddComponent<RectTransform>();
            numberPadRect.anchorMin = new Vector2(0.5f, 0);
            numberPadRect.anchorMax = new Vector2(0.5f, 0);
            numberPadRect.pivot = new Vector2(0.5f, 0);
            numberPadRect.anchoredPosition = new Vector2(0, 100); // InputFieldの下に配置
            numberPadRect.sizeDelta = new Vector2(600, 250);

            // NumberPadControllerを追加
            NumberPadController controller = numberPadGO.AddComponent<NumberPadController>();

            // VerticalLayoutGroupを追加
            VerticalLayoutGroup verticalLayout = numberPadGO.AddComponent<VerticalLayoutGroup>();
            verticalLayout.spacing = 10;
            verticalLayout.childControlHeight = false;
            verticalLayout.childControlWidth = false;
            verticalLayout.childForceExpandHeight = false;
            verticalLayout.childForceExpandWidth = false;
            verticalLayout.childAlignment = TextAnchor.MiddleCenter;

            // ボタン配列を格納
            Button[] numberButtons = new Button[10];

            // 上段 (0-4)
            GameObject topRow = CreateRow(numberPadGO.transform, "TopRow");
            for (int i = 0; i <= 4; i++)
            {
                numberButtons[i] = CreateNumberButton(topRow.transform, i);
            }

            // 下段 (5-9)
            GameObject bottomRow = CreateRow(numberPadGO.transform, "BottomRow");
            for (int i = 5; i <= 9; i++)
            {
                numberButtons[i] = CreateNumberButton(bottomRow.transform, i);
            }

            // クリアボタン行
            GameObject clearRow = CreateRow(numberPadGO.transform, "ClearRow");
            Button clearButton = CreateClearButton(clearRow.transform);

            // Controllerに参照を設定
            SerializedObject so = new SerializedObject(controller);
            
            // numberButtons配列を設定
            SerializedProperty numberButtonsProp = so.FindProperty("numberButtons");
            numberButtonsProp.arraySize = 10;
            for (int i = 0; i < 10; i++)
            {
                numberButtonsProp.GetArrayElementAtIndex(i).objectReferenceValue = numberButtons[i];
            }

            // clearButtonを設定
            SerializedProperty clearButtonProp = so.FindProperty("clearButton");
            clearButtonProp.objectReferenceValue = clearButton;

            // targetInputFieldを検索して設定
            TMP_InputField inputField = parentalGateGO.GetComponentInChildren<TMP_InputField>();
            if (inputField != null)
            {
                SerializedProperty inputFieldProp = so.FindProperty("targetInputField");
                inputFieldProp.objectReferenceValue = inputField;
            }

            so.ApplyModifiedProperties();

            // シーンを保存
            EditorUtility.SetDirty(parentalGateGO);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(parentalGateGO.scene);

            EditorUtility.DisplayDialog("完了", 
                "NumberPadの生成が完了しました。\n" +
                "ParentalGateオブジェクト配下に「NumberPad」が作成されています。", 
                "OK");
        }

        /// <summary>
        /// 行オブジェクトを作成
        /// </summary>
        private static GameObject CreateRow(Transform parent, string name)
        {
            GameObject row = new GameObject(name);
            row.transform.SetParent(parent, false);

            RectTransform rowRect = row.AddComponent<RectTransform>();
            rowRect.sizeDelta = new Vector2(600, 80);

            HorizontalLayoutGroup horizontalLayout = row.AddComponent<HorizontalLayoutGroup>();
            horizontalLayout.spacing = 10;
            horizontalLayout.childControlHeight = false;
            horizontalLayout.childControlWidth = false;
            horizontalLayout.childForceExpandHeight = false;
            horizontalLayout.childForceExpandWidth = false;
            horizontalLayout.childAlignment = TextAnchor.MiddleCenter;

            return row;
        }

        /// <summary>
        /// 数字ボタンを作成
        /// </summary>
        private static Button CreateNumberButton(Transform parent, int number)
        {
            GameObject buttonGO = new GameObject($"Button_{number}");
            buttonGO.transform.SetParent(parent, false);

            // RectTransform
            RectTransform buttonRect = buttonGO.AddComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(100, 80);

            // Image (背景)
            Image buttonImage = buttonGO.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.6f, 1f, 1f); // 青色

            // Button
            Button button = buttonGO.AddComponent<Button>();
            button.targetGraphic = buttonImage;

            // テキスト
            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(buttonGO.transform, false);

            RectTransform textRect = textGO.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            TextMeshProUGUI text = textGO.AddComponent<TextMeshProUGUI>();
            text.text = number.ToString();
            text.fontSize = 48;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;

            return button;
        }

        /// <summary>
        /// クリアボタンを作成
        /// </summary>
        private static Button CreateClearButton(Transform parent)
        {
            GameObject buttonGO = new GameObject("Button_Clear");
            buttonGO.transform.SetParent(parent, false);

            // RectTransform
            RectTransform buttonRect = buttonGO.AddComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(150, 80);

            // Image (背景)
            Image buttonImage = buttonGO.AddComponent<Image>();
            buttonImage.color = new Color(1f, 0.3f, 0.3f, 1f); // 赤色

            // Button
            Button button = buttonGO.AddComponent<Button>();
            button.targetGraphic = buttonImage;

            // テキスト
            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(buttonGO.transform, false);

            RectTransform textRect = textGO.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            TextMeshProUGUI text = textGO.AddComponent<TextMeshProUGUI>();
            text.text = "クリア";
            text.fontSize = 36;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;

            return button;
        }
    }
}
