using UnityEngine;
using DG.Tweening;

/// <summary>
/// 個別の野菜を制御するクラス
/// マルチタッチ対応で引き抜きアニメーションを管理
/// </summary>
public class VegetableController : MonoBehaviour
{
    [Header("野菜設定")]
    [SerializeField] private Sprite leafSprite;            // 葉っぱスプライト
    [SerializeField] private Sprite vegetableSprite;       // 野菜本体スプライト
    [SerializeField] private bool isRare = false;          // レア野菜フラグ
    
    [Header("引き抜きアニメーション設定")]
    [SerializeField] private float pullHeight = 2.0f;      // 引き抜き高さ
    [SerializeField] private float pullDuration = 0.5f;    // 引き抜き時間
    [SerializeField] private float displayDuration = 1.5f; // 表示時間
    [SerializeField] private float fadeOutDuration = 0.5f; // フェードアウト時間
    [SerializeField] private float respawnDelay = 0.5f;    // 再スポーンまでの待機時間
    [SerializeField] private float fadeInDuration = 0.5f;  // フェードイン時間
    
    [Header("コライダー設定")]
    [SerializeField] private float colliderRadius = 1.0f;  // タップ判定の範囲
    
    private SpriteRenderer leafRenderer;
    private SpriteRenderer vegetableRenderer;
    private CircleCollider2D circleCollider;
    private Camera mainCamera;
    private VegetableState currentState = VegetableState.Buried;
    private Vector3 originalPosition;
    
    // SpawnerからのコールバックでVegetableTypeを受け取る
    private System.Action<VegetableController> onReadyForRespawn;
    
    private enum VegetableState
    {
        Buried,      // 埋まっている（タップ可能）
        Pulling,     // 引き抜き中
        Displaying,  // 表示中
        Respawning   // 再スポーン中
    }
    
    void Awake()
    {
        mainCamera = Camera.main;
        
        // 子オブジェクトからレンダラーを取得
        Transform leafTransform = transform.Find("Leaf");
        Transform vegTransform = transform.Find("VegetableBody");
        
        if (leafTransform != null)
        {
            leafRenderer = leafTransform.GetComponent<SpriteRenderer>();
        }
        if (vegTransform != null)
        {
            vegetableRenderer = vegTransform.GetComponent<SpriteRenderer>();
        }
        
        // コライダーを取得または追加
        circleCollider = GetComponent<CircleCollider2D>();
        if (circleCollider == null)
        {
            circleCollider = gameObject.AddComponent<CircleCollider2D>();
        }
        circleCollider.radius = colliderRadius;
    }
    
    /// <summary>
    /// 野菜を初期化
    /// </summary>
    public void Initialize(Sprite leaf, Sprite vegetable, bool rare, Vector3 position, System.Action<VegetableController> respawnCallback)
    {
        leafSprite = leaf;
        vegetableSprite = vegetable;
        isRare = rare;
        originalPosition = position;
        transform.position = position;
        onReadyForRespawn = respawnCallback;
        
        // スプライトを設定
        if (leafRenderer != null)
        {
            leafRenderer.sprite = leafSprite;
            leafRenderer.color = Color.white;
        }
        if (vegetableRenderer != null)
        {
            vegetableRenderer.sprite = vegetableSprite;
            vegetableRenderer.color = new Color(1, 1, 1, 0); // 最初は透明
        }
        
        currentState = VegetableState.Buried;
        
        Debug.Log($"[VegetableController] 野菜初期化: レア={isRare}, 位置={position}");
    }
    
    /// <summary>
    /// フェードインで出現
    /// </summary>
    public void FadeIn()
    {
        currentState = VegetableState.Respawning;
        
        // 葉っぱを透明から開始
        if (leafRenderer != null)
        {
            leafRenderer.color = new Color(1, 1, 1, 0);
            leafRenderer.DOFade(1f, fadeInDuration).OnComplete(() =>
            {
                currentState = VegetableState.Buried;
                Debug.Log("[VegetableController] フェードイン完了、タップ可能");
            });
        }
    }
    
    void Update()
    {
        // タップ可能な状態のみ処理
        if (currentState == VegetableState.Buried)
        {
            HandleTouch();
        }
    }
    
    /// <summary>
    /// タッチ処理（マルチタッチ対応）
    /// </summary>
    private void HandleTouch()
    {
        // すべてのタッチを処理
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);
            if (touch.phase == TouchPhase.Began)
            {
                Vector3 worldPos = mainCamera.ScreenToWorldPoint(touch.position);
                worldPos.z = 0;
                if (circleCollider.OverlapPoint(worldPos))
                {
                    OnTapped();
                    return;
                }
            }
        }
        
        // エディタ用: マウスクリック
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            worldPos.z = 0;
            if (circleCollider.OverlapPoint(worldPos))
            {
                OnTapped();
            }
        }
    }
    
    /// <summary>
    /// タップされた時の処理
    /// </summary>
    private void OnTapped()
    {
        if (currentState != VegetableState.Buried) return;
        
        currentState = VegetableState.Pulling;
        
        // 引き抜き音を再生（ポン）
        if (VegetableDigSFXPlayer.Instance != null)
        {
            VegetableDigSFXPlayer.Instance.PlayPull();
        }
        
        // 引き抜きアニメーション開始
        PlayPullAnimation();
    }
    
    /// <summary>
    /// 引き抜きアニメーション
    /// </summary>
    private void PlayPullAnimation()
    {
        // 野菜本体を表示
        if (vegetableRenderer != null)
        {
            vegetableRenderer.color = Color.white;
        }
        
        // 上方向に移動
        Sequence pullSequence = DOTween.Sequence();
        
        // 引き抜き（上に移動）
        pullSequence.Append(transform.DOMoveY(originalPosition.y + pullHeight, pullDuration)
            .SetEase(Ease.OutBack));
        
        // 少し揺らす演出
        pullSequence.Join(transform.DOShakeRotation(pullDuration, new Vector3(0, 0, 10f), 10, 90, false));
        
        // 引き抜き完了時にファンファーレを再生
        pullSequence.AppendCallback(() =>
        {
            if (VegetableDigSFXPlayer.Instance != null)
            {
                if (isRare)
                {
                    VegetableDigSFXPlayer.Instance.PlayRareFanfare();
                }
                else
                {
                    VegetableDigSFXPlayer.Instance.PlayNormalFanfare();
                }
            }
        });
        
        // 表示時間待機
        pullSequence.AppendInterval(displayDuration);
        
        // 状態を表示中に
        pullSequence.OnComplete(() =>
        {
            currentState = VegetableState.Displaying;
            StartRespawnSequence();
        });
    }
    
    /// <summary>
    /// 再スポーンシーケンス
    /// </summary>
    private void StartRespawnSequence()
    {
        // フェードアウト
        Sequence respawnSequence = DOTween.Sequence();
        
        if (leafRenderer != null)
        {
            respawnSequence.Join(leafRenderer.DOFade(0f, fadeOutDuration));
        }
        if (vegetableRenderer != null)
        {
            respawnSequence.Join(vegetableRenderer.DOFade(0f, fadeOutDuration));
        }
        
        respawnSequence.AppendInterval(respawnDelay);
        
        respawnSequence.OnComplete(() =>
        {
            // 元の位置に戻す
            transform.position = originalPosition;
            
            // 野菜本体を非表示に
            if (vegetableRenderer != null)
            {
                vegetableRenderer.color = new Color(1, 1, 1, 0);
            }
            
            // Spawnerに再スポーン準備完了を通知
            onReadyForRespawn?.Invoke(this);
        });
    }
    
    /// <summary>
    /// コライダー半径を設定
    /// </summary>
    public void SetColliderRadius(float radius)
    {
        colliderRadius = radius;
        if (circleCollider != null)
        {
            circleCollider.radius = radius;
        }
    }
}
