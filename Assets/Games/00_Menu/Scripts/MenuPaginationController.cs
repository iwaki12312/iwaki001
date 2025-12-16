using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// メニュー画面のページネーション機能を制御するクラス
/// </summary>
public class MenuPaginationController : MonoBehaviour
{
    [Header("ページネーション設定")]
    [SerializeField] private Button leftArrowButton;     // 左矢印ボタン
    [SerializeField] private Button rightArrowButton;    // 右矢印ボタン
    [SerializeField] private Transform gameContainer;    // ゲームオブジェクトの親コンテナ
    [SerializeField] private Transform pageIndicatorContainer; // ページインジケーターの親
    [SerializeField] private GameObject pageIndicatorPrefab;   // ページインジケーターのプレハブ
    [SerializeField] private TextMeshProUGUI packNameText;     // パック名表示用テキスト
    
    [Header("アニメーション設定")]
    [SerializeField] private float slideAnimationDuration = 0.5f;
    [SerializeField] private Ease slideEase = Ease.OutQuart;
    [SerializeField] private float buttonScaleAmount = 1.1f;
    [SerializeField] private float buttonScaleDuration = 0.1f;
    
    [Header("UI設定")]
    [SerializeField] private Color activeIndicatorColor = Color.white;
    [SerializeField] private Color inactiveIndicatorColor = Color.gray;
    
    [Header("インジケーター設定")]
    [SerializeField] private Vector2 indicatorSize = new Vector2(20f, 20f);           // 基本サイズ
    [SerializeField] private float indicatorSpacing = 30f;                           // 間隔
    [SerializeField] private float activeIndicatorScale = 1.2f;                      // アクティブ時のスケール
    [SerializeField] private bool useScreenSizeBasedIndicator = true;                // 画面サイズ基準
    [SerializeField] private float indicatorSizeScreenRatio = 0.02f;                 // 画面サイズ比率（画面高さに対する割合）
    
    private List<GameObject> allGameObjects = new List<GameObject>();
    private List<Image> pageIndicators = new List<Image>();
    private Dictionary<GameObject, Vector3> originalPositions = new Dictionary<GameObject, Vector3>();
    private bool isAnimating = false;
    
    void Start()
    {
        InitializePagination();
        SetupButtons();
        CreatePageIndicators();
        
        // UI要素を画面サイズに合わせて調整
        AdjustUIForScreenSize();
        
        ShowCurrentPage();
        
        // パック名を初期表示
        UpdatePackNameDisplay();
    }
    
    /// <summary>
    /// ページネーションの初期化
    /// </summary>
    private void InitializePagination()
    {
        // GameContainerが設定されていない場合は自動検索
        if (gameContainer == null)
        {
            GameObject containerObj = GameObject.Find("GameContainer");
            if (containerObj != null)
            {
                gameContainer = containerObj.transform;
                Debug.Log("GameContainerを自動検索で見つけました");
            }
            else
            {
                Debug.LogError("GameContainerが見つかりません。GameContainerオブジェクトを作成してください。");
                return;
            }
        }
        
        // GameContainer内の全ゲームオブジェクトを取得し、元の位置を保存
        allGameObjects.Clear();
        originalPositions.Clear();
        foreach (var game in GameInfo.allGames)
        {
            string objectName = $"{game.displayOrder:D2}_{game.sceneName}";
            Transform gameTransform = gameContainer.Find(objectName);
            
            if (gameTransform != null)
            {
                GameObject gameObj = gameTransform.gameObject;
                allGameObjects.Add(gameObj);
                
                // 元の位置を保存
                originalPositions[gameObj] = gameObj.transform.position;
                Debug.Log($"ゲームオブジェクト発見: {objectName}, 元の位置: {gameObj.transform.position}");
            }
            else
            {
                Debug.LogWarning($"GameContainer内にゲームオブジェクトが見つかりません: {objectName}");
            }
        }
        
        Debug.Log($"ページネーション初期化完了: {allGameObjects.Count}個のゲーム, {GameInfo.GetTotalPages()}ページ");
        
        // GameContainerの参照をインスペクターに反映
        if (gameContainer != null)
        {
            Debug.Log($"GameContainer設定完了: {gameContainer.name}");
        }
    }
    
    /// <summary>
    /// 矢印ボタンの設定
    /// </summary>
    private void SetupButtons()
    {
        Debug.Log("SetupButtons開始");
        
        if (leftArrowButton != null)
        {
            Debug.Log($"左矢印ボタン設定: {leftArrowButton.name}");
            leftArrowButton.onClick.AddListener(() => {
                Debug.Log("左矢印ボタンがクリックされました");
                ChangePage(-1);
            });
        }
        else
        {
            Debug.LogError("左矢印ボタンがnullです");
        }
        
        if (rightArrowButton != null)
        {
            Debug.Log($"右矢印ボタン設定: {rightArrowButton.name}");
            rightArrowButton.onClick.AddListener(() => {
                Debug.Log("右矢印ボタンがクリックされました");
                ChangePage(1);
            });
        }
        else
        {
            Debug.LogError("右矢印ボタンがnullです");
        }
        
        Debug.Log("SetupButtons完了、UpdateButtonStates呼び出し");
        UpdateButtonStates();
    }
    
    /// <summary>
    /// ページインジケーターの作成（サイズ調整対応）
    /// </summary>
    private void CreatePageIndicators()
    {
        if (pageIndicatorContainer == null || pageIndicatorPrefab == null) return;
        
        // 既存のインジケーターを削除
        foreach (Transform child in pageIndicatorContainer)
        {
            DestroyImmediate(child.gameObject);
        }
        pageIndicators.Clear();
        
        // インジケーターサイズの計算
        Vector2 finalIndicatorSize = useScreenSizeBasedIndicator 
            ? new Vector2(Screen.height * indicatorSizeScreenRatio, Screen.height * indicatorSizeScreenRatio)
            : indicatorSize;
        
        Debug.Log($"インジケーターサイズ設定: useScreenBased={useScreenSizeBasedIndicator}, finalSize={finalIndicatorSize}");
        
        // 新しいインジケーターを作成
        int totalPages = GameInfo.GetTotalPages();
        for (int i = 0; i < totalPages; i++)
        {
            GameObject indicator = Instantiate(pageIndicatorPrefab, pageIndicatorContainer);
            
            // サイズを設定
            RectTransform indicatorRect = indicator.GetComponent<RectTransform>();
            if (indicatorRect != null)
            {
                indicatorRect.sizeDelta = finalIndicatorSize;
                Debug.Log($"インジケーター{i}のサイズ設定: {finalIndicatorSize}");
            }
            
            Image indicatorImage = indicator.GetComponent<Image>();
            if (indicatorImage != null)
            {
                pageIndicators.Add(indicatorImage);
            }
        }
        
        // 間隔の設定
        HorizontalLayoutGroup layoutGroup = pageIndicatorContainer.GetComponent<HorizontalLayoutGroup>();
        if (layoutGroup != null)
        {
            layoutGroup.spacing = indicatorSpacing;
            Debug.Log($"インジケーター間隔設定: {indicatorSpacing}");
        }
        else
        {
            Debug.LogWarning("HorizontalLayoutGroupが見つかりません。インジケーターの間隔調整ができません。");
        }
        
        UpdatePageIndicators();
    }
    
    /// <summary>
    /// ページ変更
    /// </summary>
    /// <param name="direction">方向（-1: 前のページ, 1: 次のページ）</param>
    public void ChangePage(int direction)
    {
        Debug.Log($"ChangePage呼び出し: direction={direction}, currentPage={GameInfo.currentPage}, isAnimating={isAnimating}");
        
        if (isAnimating) 
        {
            Debug.Log("アニメーション中のため処理をスキップ");
            return;
        }
        
        int newPage = GameInfo.currentPage + direction;
        int totalPages = GameInfo.GetTotalPages();
        
        Debug.Log($"ページ計算: newPage={newPage}, totalPages={totalPages}");
        
        // ページ範囲チェック
        if (newPage < 0 || newPage >= totalPages) 
        {
            Debug.Log($"ページ範囲外のため処理をスキップ: newPage={newPage}, totalPages={totalPages}");
            return;
        }
        
        Debug.Log($"ページ変更実行: {GameInfo.currentPage} → {newPage}");
        
        // ボタンのスケールアニメーション
        Button clickedButton = direction > 0 ? rightArrowButton : leftArrowButton;
        if (clickedButton != null)
        {
            Debug.Log($"ボタンアニメーション開始: {clickedButton.name}");
            clickedButton.transform.DOScale(buttonScaleAmount, buttonScaleDuration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => {
                    clickedButton.transform.DOScale(1f, buttonScaleDuration);
                });
        }
        else
        {
            Debug.LogWarning($"クリックされたボタンが見つかりません: direction={direction}");
        }
        
        StartPageTransition(newPage, direction);
    }
    
    /// <summary>
    /// ページ遷移アニメーションの開始
    /// </summary>
    private void StartPageTransition(int newPage, int direction)
    {
        isAnimating = true;
        
        // 現在のページのゲームオブジェクトを取得
        List<GameObject> currentPageGames = GetCurrentPageGameObjects();
        
        // スライドアウトアニメーション
        float slideDistance = Screen.width;
        Vector3 slideDirection = direction > 0 ? Vector3.left : Vector3.right;
        
        Sequence slideSequence = DOTween.Sequence();
        
        // 現在のページをスライドアウト
        foreach (GameObject gameObj in currentPageGames)
        {
            if (gameObj != null)
            {
                Vector3 targetPos = gameObj.transform.position + slideDirection * slideDistance;
                slideSequence.Join(gameObj.transform.DOMove(targetPos, slideAnimationDuration).SetEase(slideEase));
            }
        }
        
        // ページ更新とスライドイン
        slideSequence.OnComplete(() => {
            GameInfo.currentPage = newPage;
            ShowCurrentPage();
            
            // 新しいページのゲームオブジェクトを反対側から開始
            List<GameObject> newPageGames = GetCurrentPageGameObjects();
            Vector3 startDirection = direction > 0 ? Vector3.right : Vector3.left;
            
            foreach (GameObject gameObj in newPageGames)
            {
                if (gameObj != null)
                {
                    Vector3 startPos = gameObj.transform.position + startDirection * slideDistance;
                    Vector3 endPos = gameObj.transform.position;
                    
                    gameObj.transform.position = startPos;
                    gameObj.transform.DOMove(endPos, slideAnimationDuration)
                        .SetEase(slideEase)
                        .OnComplete(() => {
                            isAnimating = false;
                        });
                }
            }
            
            UpdateButtonStates();
            UpdatePageIndicators();
            UpdatePackNameDisplay(); // パック名を更新
        });
    }
    
    /// <summary>
    /// 現在のページのゲームオブジェクトを取得
    /// </summary>
    private List<GameObject> GetCurrentPageGameObjects()
    {
        List<GameObject> currentPageGames = new List<GameObject>();
        List<GameInfo.GameData> currentGames = GameInfo.GetGamesForPage(GameInfo.currentPage);
        
        if (gameContainer == null)
        {
            Debug.LogError("GameContainerが設定されていません");
            return currentPageGames;
        }
        
        foreach (var game in currentGames)
        {
            string objectName = $"{game.displayOrder:D2}_{game.sceneName}";
            Transform gameTransform = gameContainer.Find(objectName);
            
            if (gameTransform != null)
            {
                currentPageGames.Add(gameTransform.gameObject);
            }
            else
            {
                Debug.LogWarning($"GameContainer内にゲームオブジェクトが見つかりません: {objectName}");
            }
        }
        
        return currentPageGames;
    }
    
    /// <summary>
    /// 現在のページを表示
    /// </summary>
    private void ShowCurrentPage()
    {
        if (gameContainer == null)
        {
            Debug.LogError("GameContainerが設定されていません");
            return;
        }
        
        List<GameInfo.GameData> currentGames = GameInfo.GetGamesForPage(GameInfo.currentPage);
        
        // 全ゲームオブジェクトを非表示
        foreach (GameObject gameObj in allGameObjects)
        {
            if (gameObj != null)
            {
                gameObj.SetActive(false);
            }
        }
        
        // 現在のページのゲームオブジェクトのみ表示し、位置を復元
        int displayedCount = 0;
        foreach (var game in currentGames)
        {
            string objectName = $"{game.displayOrder:D2}_{game.sceneName}";
            Transform gameTransform = gameContainer.Find(objectName);
            
            if (gameTransform != null)
            {
                GameObject gameObj = gameTransform.gameObject;
                gameObj.SetActive(true);
                
                // 元の位置に復元
                if (originalPositions.ContainsKey(gameObj))
                {
                    gameObj.transform.position = originalPositions[gameObj];
                    Debug.Log($"表示: {objectName}, 位置復元: {originalPositions[gameObj]}");
                }
                else
                {
                    Debug.LogWarning($"元の位置が保存されていません: {objectName}");
                }
                
                // GameButtonのロック状態を更新
                GameButton gameButton = gameObj.GetComponent<GameButton>();
                if (gameButton != null)
                {
                    gameButton.UpdateLockState();
                }
                
                displayedCount++;
            }
            else
            {
                Debug.LogWarning($"GameContainer内にゲームオブジェクトが見つかりません: {objectName}");
            }
        }
        
        Debug.Log($"ページ {GameInfo.currentPage + 1}/{GameInfo.GetTotalPages()} を表示中 ({displayedCount}個のゲーム表示)");
    }
    
    /// <summary>
    /// ボタンの状態を更新
    /// </summary>
    private void UpdateButtonStates()
    {
        Debug.Log($"UpdateButtonStates呼び出し: currentPage={GameInfo.currentPage}, totalPages={GameInfo.GetTotalPages()}");
        
        if (leftArrowButton != null)
        {
            bool leftInteractable = GameInfo.currentPage > 0;
            leftArrowButton.interactable = leftInteractable;
            Debug.Log($"左矢印ボタン: 設定値={leftInteractable}, 実際の値={leftArrowButton.interactable}");
        }
        else
        {
            Debug.LogWarning("左矢印ボタンが設定されていません");
        }
        
        if (rightArrowButton != null)
        {
            bool rightInteractable = GameInfo.currentPage < GameInfo.GetTotalPages() - 1;
            rightArrowButton.interactable = rightInteractable;
            Debug.Log($"右矢印ボタン: 設定値={rightInteractable}, 実際の値={rightArrowButton.interactable}");
            
            // 追加の詳細情報
            Debug.Log($"右矢印ボタン詳細: name={rightArrowButton.name}, enabled={rightArrowButton.enabled}, gameObject.activeInHierarchy={rightArrowButton.gameObject.activeInHierarchy}");
        }
        else
        {
            Debug.LogWarning("右矢印ボタンが設定されていません");
        }
        
        // UpdateButtonStates完了後に1フレーム待ってから再確認
        StartCoroutine(CheckButtonStatesAfterFrame());
    }
    
    /// <summary>
    /// 1フレーム後にボタンの状態を再確認
    /// </summary>
    private System.Collections.IEnumerator CheckButtonStatesAfterFrame()
    {
        yield return null; // 1フレーム待機
        
        Debug.Log("=== 1フレーム後のボタン状態確認 ===");
        if (leftArrowButton != null)
        {
            Debug.Log($"左矢印ボタン（1フレーム後）: interactable={leftArrowButton.interactable}");
        }
        
        if (rightArrowButton != null)
        {
            Debug.Log($"右矢印ボタン（1フレーム後）: interactable={rightArrowButton.interactable}");
        }
    }
    
    /// <summary>
    /// ページインジケーターの更新（インスペクター設定対応）
    /// </summary>
    private void UpdatePageIndicators()
    {
        for (int i = 0; i < pageIndicators.Count; i++)
        {
            if (pageIndicators[i] != null)
            {
                // 色の設定
                pageIndicators[i].color = (i == GameInfo.currentPage) ? activeIndicatorColor : inactiveIndicatorColor;
                
                // アクティブなインジケーターのスケール設定（インスペクターから調整可能）
                float scale = (i == GameInfo.currentPage) ? activeIndicatorScale : 1f;
                pageIndicators[i].transform.DOScale(scale, 0.2f).SetEase(Ease.OutQuad);
                
                Debug.Log($"インジケーター{i}更新: active={i == GameInfo.currentPage}, scale={scale}, color={pageIndicators[i].color}");
            }
        }
    }
    
    /// <summary>
    /// 指定ページに直接移動
    /// </summary>
    public void GoToPage(int pageIndex)
    {
        if (isAnimating || pageIndex < 0 || pageIndex >= GameInfo.GetTotalPages()) return;
        
        int direction = pageIndex > GameInfo.currentPage ? 1 : -1;
        GameInfo.currentPage = pageIndex;
        ShowCurrentPage();
        UpdateButtonStates();
        UpdatePageIndicators();
    }
    
    /// <summary>
    /// マルチタッチ対応のUpdate
    /// </summary>
    void Update()
    {
        // スワイプジェスチャーの検出（オプション）
        HandleSwipeGesture();
    }
    
    /// <summary>
    /// スワイプジェスチャーの処理
    /// </summary>
    private void HandleSwipeGesture()
    {
        if (isAnimating) return;
        
        // タッチ入力の処理
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            
            if (touch.phase == TouchPhase.Ended)
            {
                Vector2 deltaPosition = touch.deltaPosition;
                float swipeThreshold = 50f;
                
                if (Mathf.Abs(deltaPosition.x) > swipeThreshold)
                {
                    if (deltaPosition.x > 0)
                    {
                        ChangePage(-1); // 右スワイプで前のページ
                    }
                    else
                    {
                        ChangePage(1);  // 左スワイプで次のページ
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// UI要素を画面サイズに合わせて相対配置に調整
    /// </summary>
    private void AdjustUIForScreenSize()
    {
        Debug.Log("AdjustUIForScreenSize開始");
        
        // Safe Areaを考慮した配置計算
        Rect safeArea = Screen.safeArea;
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);
        
        // Safe Area内での配置計算
        float leftSafeMargin = safeArea.xMin / screenSize.x;
        float rightSafeMargin = (screenSize.x - safeArea.xMax) / screenSize.x;
        float bottomSafeMargin = safeArea.yMin / screenSize.y;
        
        Debug.Log($"Screen: {screenSize}, SafeArea: {safeArea}");
        Debug.Log($"SafeMargins - Left: {leftSafeMargin:F3}, Right: {rightSafeMargin:F3}, Bottom: {bottomSafeMargin:F3}");
        
        // 左矢印ボタンの相対配置
        if (leftArrowButton != null)
        {
            RectTransform leftRect = leftArrowButton.GetComponent<RectTransform>();
            if (leftRect != null)
            {
                // Safe Area + 8%マージンで左端から配置
                float leftPosition = leftSafeMargin + 0.08f;
                leftRect.anchorMin = new Vector2(leftPosition, 0.5f);
                leftRect.anchorMax = new Vector2(leftPosition, 0.5f);
                leftRect.anchoredPosition = Vector2.zero;

                // 画面サイズに応じたボタンサイズ
                // ここでボタンサイズを調整している　インスペクタから調整可能にしたい
                float buttonSize = Mathf.Min(Screen.width, Screen.height) * 0.24f;
                leftRect.sizeDelta = new Vector2(buttonSize, buttonSize);
                
                Debug.Log($"左矢印ボタン調整: position={leftPosition:F3}, size={buttonSize:F1}");
            }
        }
        
        // 右矢印ボタンの相対配置
        if (rightArrowButton != null)
        {
            RectTransform rightRect = rightArrowButton.GetComponent<RectTransform>();
            if (rightRect != null)
            {
                // Safe Area + 8%マージンで右端から配置
                float rightPosition = 1.0f - rightSafeMargin - 0.08f;
                rightRect.anchorMin = new Vector2(rightPosition, 0.5f);
                rightRect.anchorMax = new Vector2(rightPosition, 0.5f);
                rightRect.anchoredPosition = Vector2.zero;

                // 画面サイズに応じたボタンサイズ
                // ここでボタンサイズを調整している　インスペクタから調整可能にしたい
                float buttonSize = Mathf.Min(Screen.width, Screen.height) * 0.24f;
                rightRect.sizeDelta = new Vector2(buttonSize, buttonSize);
                
                Debug.Log($"右矢印ボタン調整: position={rightPosition:F3}, size={buttonSize:F1}");
            }
        }
        
        // ページインジケーターの相対配置
        if (pageIndicatorContainer != null)
        {
            RectTransform indicatorRect = pageIndicatorContainer.GetComponent<RectTransform>();
            if (indicatorRect != null)
            {
                // Safe Area + 5%マージンで下部中央に配置
                float bottomPosition = bottomSafeMargin + 0.05f;
                indicatorRect.anchorMin = new Vector2(0.5f, bottomPosition);
                indicatorRect.anchorMax = new Vector2(0.5f, bottomPosition);
                indicatorRect.anchoredPosition = Vector2.zero;
                
                Debug.Log($"インジケーター調整: bottomPosition={bottomPosition:F3}");
            }
        }
        
        Debug.Log("AdjustUIForScreenSize完了");
    }
    
    /// <summary>
    /// パック名の表示を更新
    /// </summary>
    private void UpdatePackNameDisplay()
    {
        if (packNameText == null)
        {
            return; // packNameTextが設定されていない場合は何もしない
        }
        
        // 現在のページのゲーム一覧を取得
        List<GameInfo.GameData> currentGames = GameInfo.GetGamesForPage(GameInfo.currentPage);
        
        if (currentGames.Count > 0)
        {
            // 最初のゲームのパックIDを取得
            string packId = currentGames[0].packID;
            
            // パック名を表示
            packNameText.text = GameInfo.GetPackDisplayName(packId);
            
            Debug.Log($"[MenuPaginationController] パック名更新: {packId} → {packNameText.text}");
        }
        else
        {
            packNameText.text = "";
            Debug.LogWarning("[MenuPaginationController] 現在のページにゲームがありません");
        }
    }
}
