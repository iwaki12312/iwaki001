using UnityEngine;
using DG.Tweening;

/// <summary>
/// 個別のたまごを制御するクラス
/// タップでヒビを追加し、最終的に動物が生まれる
/// </summary>
public class EggController : MonoBehaviour
{
    [Header("たまご設定")]
    [SerializeField] private int maxCracks = 4;              // 割れるまでのタップ回数（スプライト0→1→2→3→4）
    [SerializeField] private float shakeStrength = 0.2f;     // タップ時の揺れ強度
    [SerializeField] private float shakeDuration = 0.2f;     // タップ時の揺れ時間
    [SerializeField] private float crackedDisplayTime = 0.3f; // 卵が割れた瞬間の表示時間
    
    [Header("孵化アニメーション設定")]
    [SerializeField] private float animalScale = 1.0f;       // 動物のサイズ（卵に対する倍率）
    [SerializeField] private float hatchJumpHeight = 1.0f;   // 動物のジャンプ高さ
    [SerializeField] private float hatchDuration = 0.5f;     // 孵化アニメーション時間
    [SerializeField] private float displayDuration = 2.0f;   // 動物表示時間
    [SerializeField] private float fadeOutDuration = 0.5f;   // フェードアウト時間
    [SerializeField] private float respawnDelay = 0.3f;      // 再スポーン待機時間
    [SerializeField] private float fadeInDuration = 0.5f;    // フェードイン時間
    
    [Header("コライダー設定")]
    [SerializeField] private float colliderRadius = 1.5f;    // タップ判定の範囲
    
    [Header("エフェクト設定")]
    [SerializeField] private GameObject rareParticlePrefab; // レア動物孵化時のパーティクル
    
    [Header("通常卵カラー設定")]
    [SerializeField] private Color[] normalEggColors = new Color[]
    {
        new Color(1.0f, 1.0f, 0.94f),   // アイボリー
        new Color(0.88f, 1.0f, 0.88f),  // ライトグリーン
        new Color(0.88f, 0.94f, 1.0f),  // ライトブルー
        new Color(1.0f, 0.88f, 0.94f),  // ライトピンク
        new Color(0.94f, 0.88f, 1.0f),  // ライトパープル
        new Color(1.0f, 1.0f, 0.88f),   // ライトイエロー
    };
    
    [Header("レア卵カラー設定")]
    [SerializeField] private Color[] rareEggColors = new Color[]
    {
        new Color(1.0f, 0.84f, 0.0f),   // ゴールド
        new Color(1.0f, 0.6f, 0.0f),    // オレンジゴールド
        new Color(0.85f, 0.65f, 0.13f), // ブロンズゴールド
        new Color(1.0f, 0.75f, 0.8f),   // ローズゴールド
        new Color(0.75f, 0.75f, 0.75f), // シルバー
    };
    
    // 子オブジェクトのレンダラー
    private SpriteRenderer eggRenderer;          // 卵本体（5段階で切り替え）
    private SpriteRenderer animalRenderer;       // 動物
    
    private CircleCollider2D circleCollider;
    private Camera mainCamera;
    
    // 現在の状態
    private int currentCrackCount = 0;
    private EggState currentState = EggState.Idle;
    private bool isRare = false;
    private Vector3 originalPosition;
    
    // Spawnerからのコールバック
    private System.Action<EggController> onReadyForRespawn;
    
    // 現在の動物データ
    private AnimalData currentAnimalData;
    
    // 卵段階スプライト（5枚）
    private Sprite[] eggStageSprites;
    
    private enum EggState
    {
        Idle,       // タップ待機
        Cracking,   // ヒビ追加中
        Hatching,   // 孵化中
        Displaying, // 動物表示中
        Respawning  // 再スポーン中
    }
    
    void Awake()
    {
        mainCamera = Camera.main;
        
        // 子オブジェクトからレンダラーを取得
        Transform eggTransform = transform.Find("Egg");
        Transform animalTransform = transform.Find("Animal");
        
        if (eggTransform != null)
            eggRenderer = eggTransform.GetComponent<SpriteRenderer>();
        if (animalTransform != null)
            animalRenderer = animalTransform.GetComponent<SpriteRenderer>();
        
        // コライダーを取得または追加
        circleCollider = GetComponent<CircleCollider2D>();
        if (circleCollider == null)
        {
            circleCollider = gameObject.AddComponent<CircleCollider2D>();
        }
        circleCollider.radius = colliderRadius;
    }
    
    /// <summary>
    /// たまごを初期化
    /// </summary>
    public void Initialize(AnimalData animalData, Sprite[] eggStages, Vector3 position, System.Action<EggController> respawnCallback)
    {
        currentAnimalData = animalData;
        eggStageSprites = eggStages;
        isRare = animalData.isRare;
        originalPosition = position;
        transform.position = position;
        onReadyForRespawn = respawnCallback;
        
        // たまご本体を設定（段階0: 通常の卵）
        if (eggRenderer != null && eggStageSprites != null && eggStageSprites.Length > 0)
        {
            eggRenderer.sprite = eggStageSprites[0];
            
            // 通常/レアに応じてランダムカラー適用
            if (isRare && rareEggColors.Length > 0)
            {
                eggRenderer.color = rareEggColors[Random.Range(0, rareEggColors.Length)];
            }
            else if (!isRare && normalEggColors.Length > 0)
            {
                eggRenderer.color = normalEggColors[Random.Range(0, normalEggColors.Length)];
            }
            else
            {
                eggRenderer.color = Color.white;
            }
        }
        
        // 動物スプライトを設定（初期は非表示）
        if (animalRenderer != null)
        {
            animalRenderer.sprite = animalData.animalSprite;
            animalRenderer.color = new Color(1, 1, 1, 0);
            animalRenderer.transform.localScale = Vector3.zero;
        }
        
        currentCrackCount = 0;
        currentState = EggState.Idle;
    }
    
    /// <summary>
    /// フェードインで出現
    /// </summary>
    public void FadeIn()
    {
        currentState = EggState.Respawning;
        
        // 既存のTweenをキャンセル
        transform.DOKill();
        if (eggRenderer != null) eggRenderer.DOKill();
        
        if (eggRenderer != null)
        {
            eggRenderer.color = new Color(eggRenderer.color.r, eggRenderer.color.g, eggRenderer.color.b, 0);
            eggRenderer.DOFade(1f, fadeInDuration).OnComplete(() =>
            {
                currentState = EggState.Idle;
            });
        }
    }
    
    void Update()
    {
        if (currentState == EggState.Idle)
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
        if (currentState != EggState.Idle) return;
        
        currentState = EggState.Cracking;
        currentCrackCount++;
        
        // 4回目（最後）のタップ時は割れる音、それ以外はヒビ音
        if (EggHatchSFXPlayer.Instance != null)
        {
            if (currentCrackCount >= maxCracks)
            {
                EggHatchSFXPlayer.Instance.PlayHatch();
            }
            else
            {
                EggHatchSFXPlayer.Instance.PlayCrack();
            }
        }
        
        // たまごを揺らす
        transform.DOKill();
        transform.DOShakePosition(shakeDuration, shakeStrength, 10, 90, false, true)
            .OnComplete(() =>
            {
                // ヒビを追加
                AddCrack(currentCrackCount);
                
                // 最大ヒビ数に達したら、割れた瞬間を表示してから孵化
                if (currentCrackCount >= maxCracks)
                {
                    // 割れた瞬間のスプライト表示時間
                    DOVirtual.DelayedCall(crackedDisplayTime, () =>
                    {
                        PlayHatchAnimation();
                    });
                }
                else
                {
                    currentState = EggState.Idle;
                }
            });
    }
    
    /// <summary>
    /// ヒビを進行（卵スプライトを切り替え）
    /// </summary>
    private void AddCrack(int crackStage)
    {
        if (eggRenderer == null || eggStageSprites == null || eggStageSprites.Length < 5) return;
        
        // crackStage 1~4に対応してスプライトを切り替え（0:通常, 1~3:ひび, 4:割れた瞬間）
        if (crackStage >= 1 && crackStage <= 4)
        {
            eggRenderer.sprite = eggStageSprites[crackStage];
        }
    }
    
    /// <summary>
    /// 孵化アニメーション
    /// </summary>
    private void PlayHatchAnimation()
    {
        currentState = EggState.Hatching;
        
        // ファンファーレを再生
        if (EggHatchSFXPlayer.Instance != null)
        {
            if (isRare)
            {
                EggHatchSFXPlayer.Instance.PlayRareFanfare();
            }
            else
            {
                EggHatchSFXPlayer.Instance.PlayNormalFanfare();
            }
        }
        
        Sequence hatchSequence = DOTween.Sequence();
        
        // たまごをフェードアウト
        if (eggRenderer != null)
        {
            hatchSequence.Join(eggRenderer.DOFade(0f, hatchDuration * 0.5f));
        }
        
        // レア動物の場合、パーティクルを表示
        if (isRare && rareParticlePrefab != null)
        {
            GameObject particle = Instantiate(rareParticlePrefab, transform.position, Quaternion.identity);
            
            // パーティクルのSorting Orderを動物より後ろに設定（背景側）
            ParticleSystemRenderer particleRenderer = particle.GetComponent<ParticleSystemRenderer>();
            if (particleRenderer != null)
            {
                particleRenderer.sortingOrder = 1; // 動物(2)より後ろ、卵(0)より前
            }
            
            Destroy(particle, 3f); // 3秒後に自動削除
        }
        
        // 動物を登場させる
        if (animalRenderer != null)
        {
            animalRenderer.color = Color.white;
            animalRenderer.transform.localScale = Vector3.zero;
            
            // スケールアップ + ジャンプ（animalScaleを使用）
            Vector3 targetScale = Vector3.one * animalScale;
            hatchSequence.Append(animalRenderer.transform.DOScale(targetScale, hatchDuration).SetEase(Ease.OutBack));
            hatchSequence.Join(animalRenderer.transform.DOLocalJump(Vector3.zero, hatchJumpHeight, 1, hatchDuration));
        }
        
        // 表示時間待機後、再スポーン処理
        hatchSequence.AppendInterval(displayDuration);
        
        hatchSequence.OnComplete(() =>
        {
            currentState = EggState.Displaying;
            StartRespawnSequence();
        });
    }
    
    /// <summary>
    /// 再スポーンシーケンス
    /// </summary>
    private void StartRespawnSequence()
    {
        // 既存のTweenをキャンセル
        transform.DOKill();
        if (animalRenderer != null) animalRenderer.DOKill();
        
        Sequence respawnSequence = DOTween.Sequence();
        
        // 動物をフェードアウト
        if (animalRenderer != null)
        {
            respawnSequence.Append(animalRenderer.DOFade(0f, fadeOutDuration));
        }
        
        respawnSequence.AppendInterval(respawnDelay);
        
        respawnSequence.OnComplete(() =>
        {
            // 動物を非表示に
            if (animalRenderer != null)
            {
                animalRenderer.color = new Color(1, 1, 1, 0);
                animalRenderer.transform.localScale = Vector3.zero;
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
