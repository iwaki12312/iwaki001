using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// メニューページネーション用のUI要素を自動生成するヘルパークラス
/// </summary>
/// 
/// 使ってないけど一応残しておく
/// メニューのオブジェクト（空でいい）にアタッチしてインスペクタで右クリック　setup pagination で使う
public class MenuPaginationUISetup : MonoBehaviour
{
    [Header("UI生成設定")]
    [SerializeField] private Canvas targetCanvas;
    [SerializeField] private bool autoSetupOnStart = true;
    
    [Header("矢印ボタン設定")]
    [SerializeField] private Vector2 leftArrowPosition = new Vector2(-400, 0);
    [SerializeField] private Vector2 rightArrowPosition = new Vector2(400, 0);
    [SerializeField] private Vector2 arrowButtonSize = new Vector2(80, 80);
    [SerializeField] private Color arrowButtonColor = Color.white;
    
    [Header("ページインジケーター設定")]
    [SerializeField] private Vector2 indicatorPosition = new Vector2(0, -250);
    [SerializeField] private Vector2 indicatorSize = new Vector2(20, 20);
    [SerializeField] private float indicatorSpacing = 30f;
    [SerializeField] private Color indicatorColor = Color.white;
    
    private MenuPaginationController paginationController;
    
    void Start()
    {
        if (autoSetupOnStart)
        {
            SetupPaginationUI();
        }
    }
    
    /// <summary>
    /// ページネーション用UIを自動生成
    /// </summary>
    [ContextMenu("Setup Pagination UI")]
    public void SetupPaginationUI()
    {
        if (targetCanvas == null)
        {
            targetCanvas = FindObjectOfType<Canvas>();
            if (targetCanvas == null)
            {
                Debug.LogError("Canvasが見つかりません");
                return;
            }
        }
        
        // ページネーションコントローラーを取得または作成
        paginationController = FindObjectOfType<MenuPaginationController>();
        if (paginationController == null)
        {
            GameObject paginationObj = new GameObject("MenuPaginationController");
            paginationController = paginationObj.AddComponent<MenuPaginationController>();
        }
        
        // 左矢印ボタンを作成
        Button leftArrow = CreateArrowButton("LeftArrowButton", leftArrowPosition, "◀");
        
        // 右矢印ボタンを作成
        Button rightArrow = CreateArrowButton("RightArrowButton", rightArrowPosition, "▶");
        
        // ページインジケーターコンテナを作成
        GameObject indicatorContainer = CreateIndicatorContainer();
        
        // ページインジケータープレハブを作成
        GameObject indicatorPrefab = CreateIndicatorPrefab();
        
        // ページネーションコントローラーに参照を設定
        SetPaginationControllerReferences(leftArrow, rightArrow, indicatorContainer, indicatorPrefab);
        
        Debug.Log("ページネーション用UI生成完了");
    }
    
    /// <summary>
    /// 矢印ボタンを作成
    /// </summary>
    private Button CreateArrowButton(string name, Vector2 position, string text)
    {
        // ボタンオブジェクト作成
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(targetCanvas.transform, false);
        
        // RectTransform設定
        RectTransform rectTransform = buttonObj.AddComponent<RectTransform>();
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = arrowButtonSize;
        
        // Image設定
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = arrowButtonColor;
        
        // Button設定
        Button button = buttonObj.AddComponent<Button>();
        
        // テキスト子オブジェクト作成
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        Text buttonText = textObj.AddComponent<Text>();
        buttonText.text = text;
        buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        buttonText.fontSize = 24;
        buttonText.color = Color.black;
        buttonText.alignment = TextAnchor.MiddleCenter;
        
        // 子供向けの大きなタッチ領域設定
        button.targetGraphic = buttonImage;
        
        return button;
    }
    
    /// <summary>
    /// ページインジケーターコンテナを作成
    /// </summary>
    private GameObject CreateIndicatorContainer()
    {
        GameObject container = new GameObject("PageIndicatorContainer");
        container.transform.SetParent(targetCanvas.transform, false);
        
        RectTransform rectTransform = container.AddComponent<RectTransform>();
        rectTransform.anchoredPosition = indicatorPosition;
        
        // HorizontalLayoutGroup追加
        HorizontalLayoutGroup layoutGroup = container.AddComponent<HorizontalLayoutGroup>();
        layoutGroup.spacing = indicatorSpacing;
        layoutGroup.childAlignment = TextAnchor.MiddleCenter;
        layoutGroup.childControlWidth = false;
        layoutGroup.childControlHeight = false;
        
        return container;
    }
    
    /// <summary>
    /// ページインジケータープレハブを作成
    /// </summary>
    private GameObject CreateIndicatorPrefab()
    {
        GameObject prefab = new GameObject("PageIndicatorPrefab");
        
        RectTransform rectTransform = prefab.AddComponent<RectTransform>();
        rectTransform.sizeDelta = indicatorSize;
        
        Image image = prefab.AddComponent<Image>();
        image.color = indicatorColor;
        
        // 円形にする
        image.sprite = CreateCircleSprite();
        
        return prefab;
    }
    
    /// <summary>
    /// 円形スプライトを作成
    /// </summary>
    private Sprite CreateCircleSprite()
    {
        // 簡単な円形テクスチャを作成
        int size = 32;
        Texture2D texture = new Texture2D(size, size);
        Color[] colors = new Color[size * size];
        
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f - 1;
        
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                colors[y * size + x] = distance <= radius ? Color.white : Color.clear;
            }
        }
        
        texture.SetPixels(colors);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }
    
    /// <summary>
    /// ページネーションコントローラーに参照を設定
    /// </summary>
    private void SetPaginationControllerReferences(Button leftArrow, Button rightArrow, 
        GameObject indicatorContainer, GameObject indicatorPrefab)
    {
        // リフレクションを使用してprivateフィールドに値を設定
        var leftArrowField = typeof(MenuPaginationController).GetField("leftArrowButton", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var rightArrowField = typeof(MenuPaginationController).GetField("rightArrowButton", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var indicatorContainerField = typeof(MenuPaginationController).GetField("pageIndicatorContainer", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var indicatorPrefabField = typeof(MenuPaginationController).GetField("pageIndicatorPrefab", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (leftArrowField != null) leftArrowField.SetValue(paginationController, leftArrow);
        if (rightArrowField != null) rightArrowField.SetValue(paginationController, rightArrow);
        if (indicatorContainerField != null) indicatorContainerField.SetValue(paginationController, indicatorContainer.transform);
        if (indicatorPrefabField != null) indicatorPrefabField.SetValue(paginationController, indicatorPrefab);
        
        Debug.Log("ページネーションコントローラーの参照設定完了");
    }
    
    /// <summary>
    /// 既存のUI要素をクリーンアップ
    /// </summary>
    [ContextMenu("Clean Up Pagination UI")]
    public void CleanUpPaginationUI()
    {
        // 既存の要素を削除
        GameObject leftArrow = GameObject.Find("LeftArrowButton");
        GameObject rightArrow = GameObject.Find("RightArrowButton");
        GameObject indicatorContainer = GameObject.Find("PageIndicatorContainer");
        GameObject indicatorPrefab = GameObject.Find("PageIndicatorPrefab");
        
        if (leftArrow != null) DestroyImmediate(leftArrow);
        if (rightArrow != null) DestroyImmediate(rightArrow);
        if (indicatorContainer != null) DestroyImmediate(indicatorContainer);
        if (indicatorPrefab != null) DestroyImmediate(indicatorPrefab);
        
        Debug.Log("ページネーション用UIクリーンアップ完了");
    }
}
