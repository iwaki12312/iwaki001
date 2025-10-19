using UnityEngine;
using UnityEngine.SceneManagement;
using WakuWaku.IAP;

/// <summary>
/// メニュー画面の制御を行うクラス
/// </summary>
public class MenuController : MonoBehaviour
{
    [Header("ページネーション")]
    [SerializeField] private MenuPaginationController paginationController;
    
    void OnEnable()
    {
        // デバッグログ
        Debug.Log("MenuControllerが開始されました");

        // ページネーションコントローラーを自動検索
        if (paginationController == null)
        {
            paginationController = FindObjectOfType<MenuPaginationController>();
        }

        // EntitlementStoreのイベント購読
        SubscribeToEntitlementEvents();

        // 各オブジェクトを検索してGameButtonコンポーネントを追加し、初期化する
        // GameInfoクラスのallGamesリストを使用して動的に処理
        foreach (var game in GameInfo.allGames)
        {
            // ゲームオブジェクトを検索（オブジェクト名は00_【シーン名】）※00の部分はゲーム番号を2桁でゼロ埋め
            GameObject gameObj = GameObject.Find($"{game.displayOrder:D2}_{game.sceneName}");
            if (gameObj != null)
            {
                // BoxCollider2Dがなければ追加
                if (gameObj.GetComponent<BoxCollider2D>() == null)
                {
                    BoxCollider2D collider = gameObj.AddComponent<BoxCollider2D>();
                    // スプライトのサイズに合わせる
                    SpriteRenderer spriteRenderer = gameObj.GetComponent<SpriteRenderer>();
                    if (spriteRenderer != null)
                    {
                        collider.size = spriteRenderer.sprite.bounds.size;
                    }
                }

                // GameButtonコンポーネントを追加
                GameButton gameButton = gameObj.AddComponent<GameButton>();
                gameButton.sceneName = game.sceneName;
                gameButton.packId = game.packID; // パックIDを設定
                gameButton.Initialize();
                gameButton.UpdateLockState(); // ロック状態を初期化
            }
            else
            {
                Debug.LogWarning($"{game.sceneName}オブジェクトが見つかりません（ページネーション対応のため警告レベルに変更）");
            }
        }
        
        Debug.Log($"MenuController初期化完了: {GameInfo.allGames.Count}個のゲーム登録");
    }

    void OnDisable()
    {
        // イベント購読解除
        UnsubscribeFromEntitlementEvents();
    }

    /// <summary>
    /// EntitlementStoreのイベント購読
    /// </summary>
    private void SubscribeToEntitlementEvents()
    {
        if (EntitlementStore.Instance != null)
        {
            EntitlementStore.Instance.OnPackUnlocked += OnPackUnlocked;
            EntitlementStore.Instance.OnPackRevoked += OnPackRevoked;
            Debug.Log("[MenuController] EntitlementStoreイベント購読完了");
        }
    }

    /// <summary>
    /// EntitlementStoreのイベント購読解除
    /// </summary>
    private void UnsubscribeFromEntitlementEvents()
    {
        if (EntitlementStore.Instance != null)
        {
            EntitlementStore.Instance.OnPackUnlocked -= OnPackUnlocked;
            EntitlementStore.Instance.OnPackRevoked -= OnPackRevoked;
        }
    }

    /// <summary>
    /// パックがアンロックされたときの処理
    /// </summary>
    private void OnPackUnlocked(string packId)
    {
        Debug.Log($"[MenuController] パックアンロック検知: {packId}");
        UpdateAllGameButtons();
    }

    /// <summary>
    /// パックが取り消されたときの処理
    /// </summary>
    private void OnPackRevoked(string packId)
    {
        Debug.Log($"[MenuController] パック取り消し検知: {packId}");
        UpdateAllGameButtons();
    }

    /// <summary>
    /// すべてのGameButtonのロック状態を更新
    /// </summary>
    private void UpdateAllGameButtons()
    {
        GameButton[] gameButtons = FindObjectsOfType<GameButton>();
        foreach (var button in gameButtons)
        {
            button.UpdateLockState();
        }
        Debug.Log($"[MenuController] {gameButtons.Length}個のGameButtonを更新しました");
    }
    
    /// <summary>
    /// ページネーションコントローラーの参照を取得
    /// </summary>
    public MenuPaginationController GetPaginationController()
    {
        return paginationController;
    }
}

/// <summary>
/// ゲームボタンの機能を提供するコンポーネント
/// </summary>
public class GameButton : MonoBehaviour
{
    public string sceneName; // 遷移先のシーン名
    public string packId;    // 必要なパックID
    private bool isInitialized = false;
    private bool isLocked = false; // ロック状態

    // ハイライト表示用の元の色と強調色
    private Color originalColor;
    private Color highlightColor;
    private Color lockedColor = new Color(0.4f, 0.4f, 0.4f, 0.6f); // ロック時の色(グレー+半透明)

    // SpriteRendererコンポーネント
    private SpriteRenderer spriteRenderer;

    Camera mainCam;
    Collider2D myCol;

    /// <summary>
    /// 初期化処理
    /// </summary>
    public void Initialize()
    {
        if (isInitialized) return;

        // SpriteRendererを取得
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            // 元の色を保存
            originalColor = spriteRenderer.color;

            // 強調色を設定（元の色より明るく）
            highlightColor = new Color(
                Mathf.Min(originalColor.r * 1.2f, 1f),
                Mathf.Min(originalColor.g * 1.2f, 1f),
                Mathf.Min(originalColor.b * 1.2f, 1f),
                originalColor.a
            );
        }

        mainCam = Camera.main;
        myCol = GetComponent<Collider2D>();

        isInitialized = true;
    }

    /// <summary>
    /// ロック状態を更新
    /// </summary>
    public void UpdateLockState()
    {
        if (string.IsNullOrEmpty(packId))
        {
            Debug.LogWarning($"[GameButton] packIdが設定されていません: {sceneName}");
            isLocked = false;
            return;
        }

        // EntitlementStoreでパック所有状況をチェック
        if (EntitlementStore.Instance != null)
        {
            isLocked = !EntitlementStore.Instance.HasPack(packId);
        }
        else
        {
            // EntitlementStoreが初期化されていない場合、pack_freeのみアンロック
            isLocked = (packId != "pack_free");
        }

        UpdateVisuals();
        
        Debug.Log($"[GameButton] ロック状態更新: {sceneName} (packId={packId}) → isLocked={isLocked}");
    }

    /// <summary>
    /// 見た目を更新
    /// </summary>
    private void UpdateVisuals()
    {
        if (spriteRenderer == null) return;

        if (isLocked)
        {
            // ロック中: グレーアウト
            spriteRenderer.color = lockedColor;
        }
        else
        {
            // アンロック: 元の色
            spriteRenderer.color = originalColor;
        }
    }

    void Update()
    {
        if (!mainCam || !myCol) return;

        // モーダル表示中は入力を無視
        if (IsModalShowing()) return;

        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch t = Input.GetTouch(i);
            if (t.phase != TouchPhase.Began) continue;

            Vector2 world = mainCam.ScreenToWorldPoint(t.position);
            if (myCol.OverlapPoint(world))
            {
                LoadGame();
                break;                       // 複数回呼ばれないように
            }
        }
    }

    void OnMouseEnter()
    {
        // ロック中はハイライトしない
        if (isLocked) return;

        // マウスが重なったら色を変える
        if (spriteRenderer != null)
        {
            spriteRenderer.color = highlightColor;
        }

        // カーソルを変更
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    void OnMouseExit()
    {
        // ロック中は元の色に戻さない(ロック色を維持)
        if (isLocked)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = lockedColor;
            }
            return;
        }

        // マウスが離れたら元の色に戻す
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }

        // カーソルを元に戻す
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    void OnMouseDown()
    {
        // モーダル表示中は入力を無視
        if (IsModalShowing()) return;

        // クリック時にシーン遷移
        LoadGame();
    }

    /// <summary>
    /// モーダルウィンドウが表示中かチェック
    /// </summary>
    private bool IsModalShowing()
    {
        // Paywall表示中
        if (Paywall.Instance != null && Paywall.Instance.IsShowing())
        {
            return true;
        }
        
        // ParentalGate表示中
        if (ParentalGate.Instance != null && ParentalGate.Instance.IsShowing())
        {
            return true;
        }
        
        return false;
    }

    /// <summary>
    /// ゲームシーンをロードする
    /// </summary>
    public void LoadGame()
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("シーン名が設定されていません");
            return;
        }

        Debug.Log($"ゲームをロード: {sceneName}");

        // アクセス制御チェック
        if (!FeatureGate.CanStartGameBySceneName(sceneName))
        {
            Debug.Log($"[GameButton] ゲームがロックされています: {sceneName}");
            ShowPaywall();
            return;
        }

        // UpdateGameHistoryを呼び出して履歴を更新
        ChangeGameManager.UpdateGameHistory(sceneName);

        // シーン遷移
        SceneManager.LoadScene(sceneName);
    }
    
    /// <summary>
    /// Paywallを表示
    /// </summary>
    private void ShowPaywall()
    {
        // GameInfo.allGamesからシーン名に対応するゲームを検索
        var gameData = GameInfo.allGames.Find(g => g.sceneName == sceneName);
        if (gameData == null)
        {
            Debug.LogError($"[GameButton] ゲーム情報が見つかりません: {sceneName}");
            return;
        }
        
        if (Paywall.Instance != null)
        {
            Paywall.Instance.ShowPaywall(
                gameData.packID,
                onSuccess: () => {
                    Debug.Log($"[GameButton] 購入成功後にゲーム開始: {sceneName}");
                    // // 購入成功後にゲームを開始
                    // ChangeGameManager.UpdateGameHistory(sceneName);
                    // SceneManager.LoadScene(sceneName);
                },
                onClose: () => {
                    Debug.Log($"[GameButton] Paywall閉じられました: {sceneName}");
                }
            );
        }
        else
        {
            Debug.LogError("[GameButton] Paywallが見つかりません");
        }
    }
}
