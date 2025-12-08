using UnityEngine;
using DG.Tweening;

/// <summary>
/// 個別の昆虫を制御するクラス
/// マルチタッチ対応でシルエット→捕獲→表示の流れを管理
/// </summary>
public class InsectController : MonoBehaviour
{
    [Header("設定")]
    [SerializeField] private Sprite insectSprite;      // 昆虫のスプライト
    [SerializeField] private bool isRare = false;      // レア昆虫フラグ
    [SerializeField] private float lifetime = 10f;     // 寿命(秒)
    [SerializeField] private float fadeInDuration = 0.5f;  // フェードイン時間
    [SerializeField] private float fadeOutDuration = 0.5f; // フェードアウト時間
    [SerializeField] private float colliderRadius = 1.0f;  // タップ判定の範囲(Inspector調整可能)
    
    [Header("虫取り網")]
    [SerializeField] private Sprite netSprite;         // 虫取り網のスプライト
    [SerializeField] private Vector3 netOffset = new Vector3(0, 0.5f, 0); // 網の表示位置オフセット(Inspector調整可能)
    [SerializeField] private float netScale = 1.5f;    // 網のスケール
    [SerializeField] private float netAnimDuration = 0.3f; // 網アニメーション時間
    
    private SpriteRenderer spriteRenderer;
    private CircleCollider2D circleCollider;
    private InsectState currentState = InsectState.Spawning;
    private float spawnTime;
    private Camera mainCamera;
    private int positionIndex = -1; // このインセクトが使用しているポジションインデックス
    
    // 昆虫が削除された時のイベント
    public System.Action OnDestroyed;
    
    private enum InsectState
    {
        Spawning,    // フェードイン中
        Idle,        // 待機中(タップ可能)
        Caught,      // 捕獲中
        Displaying,  // 中央表示中
        Despawning   // フェードアウト中
    }
    
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        
        circleCollider = GetComponent<CircleCollider2D>();
        if (circleCollider == null)
        {
            circleCollider = gameObject.AddComponent<CircleCollider2D>();
        }
        
        // タップ判定の範囲を設定
        circleCollider.radius = colliderRadius;
        
        mainCamera = Camera.main;
    }
    
    /// <summary>
    /// 昆虫を初期化してスポーン
    /// </summary>
    public void Initialize(Sprite sprite, bool rare, Vector3 position, bool flipX = false)
    {
        insectSprite = sprite;
        isRare = rare;
        transform.position = position;
        spawnTime = Time.time;
        
        // スプライトを設定
        spriteRenderer.sprite = insectSprite;
        
        // 左右反転を適用
        spriteRenderer.flipX = flipX;
        
        // シルエット表示(黒)
        spriteRenderer.color = new Color(0, 0, 0, 0); // 完全透明から開始
        
        // フェードイン開始
        FadeIn();
        
        Debug.Log($"[InsectController] 昆虫スポーン: レア={isRare}, 位置={position}, 反転={flipX}");
    }
    
    /// <summary>
    /// 虫取り網スプライトを設定(エディタ拡張から)
    /// </summary>
    public void SetNetSprite(Sprite net)
    {
        netSprite = net;
    }
    
    /// <summary>
    /// ポジションインデックスを設定
    /// </summary>
    public void SetPositionIndex(int index)
    {
        positionIndex = index;
    }
    
    /// <summary>
    /// 削除時にポジションを解放
    /// </summary>
    void OnDestroy()
    {
        OnDestroyed?.Invoke();
    }
    
    void Update()
    {
        // 捕獲中または中央表示中はゲーム進行を停止
        if (CatchInsectsGameManager.Instance != null && CatchInsectsGameManager.Instance.IsBusy)
        {
            return;
        }
        
        // 寿命チェック
        if (currentState == InsectState.Idle && Time.time - spawnTime > lifetime)
        {
            Despawn();
        }
        
        // マルチタッチ対応
        if (currentState == InsectState.Idle)
        {
            HandleTouch();
        }
    }
    
    /// <summary>
    /// タッチ処理(マルチタッチ対応)
    /// </summary>
    private void HandleTouch()
    {
        // 捕獲中または中央表示中はタップを無視
        if (CatchInsectsGameManager.Instance != null && CatchInsectsGameManager.Instance.IsBusy)
        {
            return;
        }
        
        // すべてのタッチを処理
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);
            if (touch.phase == TouchPhase.Began)
            {
                Vector3 worldPos = mainCamera.ScreenToWorldPoint(touch.position);
                if (circleCollider.OverlapPoint(worldPos))
                {
                    // 同一フレームで複数の昆虫が捕獲されないように制御
                    if (InsectSpawner.Instance != null && !InsectSpawner.Instance.TryClaimCatch())
                    {
                        return;
                    }
                    OnTapped();
                    return;
                }
            }
        }
        
        // エディタ用: マウスクリック
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            if (circleCollider.OverlapPoint(worldPos))
            {
                // 同一フレームで複数の昆虫が捕獲されないように制御
                if (InsectSpawner.Instance != null && !InsectSpawner.Instance.TryClaimCatch())
                {
                    return;
                }
                OnTapped();
            }
        }
    }
    
    /// <summary>
    /// タップされたときの処理
    /// </summary>
    private void OnTapped()
    {
        if (currentState != InsectState.Idle) return;
        
        currentState = InsectState.Caught;
        
        // 捕獲開始を通知（他の虫のタップを無効化）
        if (CatchInsectsGameManager.Instance != null)
        {
            CatchInsectsGameManager.Instance.StartCatching();
        }
        
        // 虫取り網の音を再生
        if (CatchInsectsSFXPlayer.Instance != null)
        {
            CatchInsectsSFXPlayer.Instance.PlayNetSwing();
        }
        
        // 捕獲アニメーション
        PlayCatchAnimation();
    }
    
    /// <summary>
    /// 捕獲アニメーション(虫取り網)
    /// </summary>
    private void PlayCatchAnimation()
    {
        if (netSprite == null)
        {
            Debug.LogWarning("[InsectController] 虫取り網スプライトが設定されていません。スキップします。");
            OnCatchComplete();
            return;
        }
        
        // 虫取り網オブジェクトを生成
        GameObject netObj = new GameObject("BugNet");
        netObj.transform.position = transform.position + netOffset; // オフセット適用
        netObj.transform.SetParent(transform);
        
        SpriteRenderer netRenderer = netObj.AddComponent<SpriteRenderer>();
        netRenderer.sprite = netSprite;
        netRenderer.sortingOrder = spriteRenderer.sortingOrder + 1; // 昆虫の上に表示
        
        // 初期サイズと回転
        netObj.transform.localScale = Vector3.one * netScale; // Inspectorで調整可能
        netObj.transform.rotation = Quaternion.Euler(0, 0, -30f);
        
        // 回転アニメーション: -30° → 30° → 0.5秒静止
        Sequence netSequence = DOTween.Sequence();
        netSequence.Append(netObj.transform.DORotate(new Vector3(0, 0, 30f), netAnimDuration))
                   .AppendInterval(0.5f) // 30°で0.5秒静止
                   .OnComplete(() =>
                   {
                       Destroy(netObj);
                       OnCatchComplete();
                   });
        
        Debug.Log("[InsectController] 虫取り網アニメーション再生: 回転→静止");
    }
    
    /// <summary>
    /// 捕獲完了時の処理
    /// </summary>
    private void OnCatchComplete()
    {
        // 効果音再生
        if (CatchInsectsSFXPlayer.Instance != null)
        {
            if (isRare)
            {
                CatchInsectsSFXPlayer.Instance.PlayCatchRare();
            }
            else
            {
                CatchInsectsSFXPlayer.Instance.PlayCatchNormal();
            }
        }
        
        // GameManagerに通知して中央表示
        if (CatchInsectsGameManager.Instance != null)
        {
            CatchInsectsGameManager.Instance.DisplayInsect(insectSprite, isRare);
        }
        
        // この昆虫オブジェクトを破棄
        Destroy(gameObject);
    }
    
    /// <summary>
    /// フェードイン
    /// </summary>
    private void FadeIn()
    {
        currentState = InsectState.Spawning;
        
        spriteRenderer.DOColor(Color.black, fadeInDuration)
            .OnComplete(() =>
            {
                currentState = InsectState.Idle;
                Debug.Log("[InsectController] フェードイン完了、タップ可能");
            });
    }
    
    /// <summary>
    /// フェードアウトして消滅
    /// </summary>
    private void Despawn()
    {
        if (currentState == InsectState.Despawning) return;
        
        currentState = InsectState.Despawning;
        
        spriteRenderer.DOFade(0, fadeOutDuration)
            .OnComplete(() =>
            {
                Debug.Log("[InsectController] フェードアウト完了、破棄");
                Destroy(gameObject);
            });
    }
}
