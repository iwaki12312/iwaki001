using UnityEngine;
using DG.Tweening;

/// <summary>
/// 個別のたまごを制御するクラス
/// タップでヒビを追加し、最終的に動物が生まれる
/// </summary>
public class EggController : MonoBehaviour
{
    [Header("たまご設定")]
    [SerializeField] private int maxCracks = 3;              // 割れるまでのタップ回数
    [SerializeField] private float shakeStrength = 0.2f;     // タップ時の揺れ強度
    [SerializeField] private float shakeDuration = 0.2f;     // タップ時の揺れ時間
    
    [Header("孵化アニメーション設定")]
    [SerializeField] private float hatchJumpHeight = 1.0f;   // 動物のジャンプ高さ
    [SerializeField] private float hatchDuration = 0.5f;     // 孵化アニメーション時間
    [SerializeField] private float displayDuration = 2.0f;   // 動物表示時間
    [SerializeField] private float fadeOutDuration = 0.5f;   // フェードアウト時間
    [SerializeField] private float respawnDelay = 0.3f;      // 再スポーン待機時間
    [SerializeField] private float fadeInDuration = 0.5f;    // フェードイン時間
    
    [Header("コライダー設定")]
    [SerializeField] private float colliderRadius = 1.5f;    // タップ判定の範囲
    
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
    private SpriteRenderer eggBaseRenderer;
    private SpriteRenderer crackLayer1Renderer;
    private SpriteRenderer crackLayer2Renderer;
    private SpriteRenderer crackLayer3Renderer;
    private SpriteRenderer animalRenderer;
    
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
    
    // ヒビスプライト（共通）
    private Sprite[] crackSprites;
    
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
        Transform eggBaseTransform = transform.Find("EggBase");
        Transform crack1Transform = transform.Find("CrackLayer1");
        Transform crack2Transform = transform.Find("CrackLayer2");
        Transform crack3Transform = transform.Find("CrackLayer3");
        Transform animalTransform = transform.Find("Animal");
        
        if (eggBaseTransform != null)
            eggBaseRenderer = eggBaseTransform.GetComponent<SpriteRenderer>();
        if (crack1Transform != null)
            crackLayer1Renderer = crack1Transform.GetComponent<SpriteRenderer>();
        if (crack2Transform != null)
            crackLayer2Renderer = crack2Transform.GetComponent<SpriteRenderer>();
        if (crack3Transform != null)
            crackLayer3Renderer = crack3Transform.GetComponent<SpriteRenderer>();
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
    public void Initialize(AnimalData animalData, Sprite[] cracks, Vector3 position, System.Action<EggController> respawnCallback)
    {
        currentAnimalData = animalData;
        crackSprites = cracks;
        isRare = animalData.isRare;
        originalPosition = position;
        transform.position = position;
        onReadyForRespawn = respawnCallback;
        
        // たまご本体を設定
        if (eggBaseRenderer != null)
        {
            eggBaseRenderer.sprite = animalData.eggSprite;
            eggBaseRenderer.color = Color.white;
            
            // レア卵の場合はランダムカラー適用
            if (isRare && rareEggColors.Length > 0)
            {
                eggBaseRenderer.color = rareEggColors[Random.Range(0, rareEggColors.Length)];
            }
        }
        
        // 動物スプライトを設定（初期は非表示）
        if (animalRenderer != null)
        {
            animalRenderer.sprite = animalData.animalSprite;
            animalRenderer.color = new Color(1, 1, 1, 0);
            animalRenderer.transform.localScale = Vector3.zero;
        }
        
        // ヒビを非表示
        HideAllCracks();
        
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
        if (eggBaseRenderer != null) eggBaseRenderer.DOKill();
        
        if (eggBaseRenderer != null)
        {
            eggBaseRenderer.color = new Color(eggBaseRenderer.color.r, eggBaseRenderer.color.g, eggBaseRenderer.color.b, 0);
            eggBaseRenderer.DOFade(1f, fadeInDuration).OnComplete(() =>
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
        
        // ヒビ音を再生
        if (EggHatchSFXPlayer.Instance != null)
        {
            EggHatchSFXPlayer.Instance.PlayCrack();
        }
        
        // たまごを揺らす
        transform.DOKill();
        transform.DOShakePosition(shakeDuration, shakeStrength, 10, 90, false, true)
            .OnComplete(() =>
            {
                // ヒビを追加
                AddCrack(currentCrackCount);
                
                // 最大ヒビ数に達したら孵化
                if (currentCrackCount >= maxCracks)
                {
                    PlayHatchAnimation();
                }
                else
                {
                    currentState = EggState.Idle;
                }
            });
    }
    
    /// <summary>
    /// ヒビを追加表示
    /// </summary>
    private void AddCrack(int crackStage)
    {
        if (crackSprites == null || crackSprites.Length == 0) return;
        
        switch (crackStage)
        {
            case 1:
                if (crackLayer1Renderer != null && crackSprites.Length > 0)
                {
                    crackLayer1Renderer.sprite = crackSprites[0];
                    crackLayer1Renderer.enabled = true;
                }
                break;
            case 2:
                if (crackLayer2Renderer != null && crackSprites.Length > 1)
                {
                    crackLayer2Renderer.sprite = crackSprites[1];
                    crackLayer2Renderer.enabled = true;
                }
                break;
            case 3:
                if (crackLayer3Renderer != null && crackSprites.Length > 2)
                {
                    crackLayer3Renderer.sprite = crackSprites[2];
                    crackLayer3Renderer.enabled = true;
                }
                break;
        }
    }
    
    /// <summary>
    /// すべてのヒビを非表示
    /// </summary>
    private void HideAllCracks()
    {
        if (crackLayer1Renderer != null) crackLayer1Renderer.enabled = false;
        if (crackLayer2Renderer != null) crackLayer2Renderer.enabled = false;
        if (crackLayer3Renderer != null) crackLayer3Renderer.enabled = false;
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
        if (eggBaseRenderer != null)
        {
            hatchSequence.Join(eggBaseRenderer.DOFade(0f, hatchDuration * 0.5f));
        }
        
        // ヒビもフェードアウト
        if (crackLayer1Renderer != null && crackLayer1Renderer.enabled)
            hatchSequence.Join(crackLayer1Renderer.DOFade(0f, hatchDuration * 0.5f));
        if (crackLayer2Renderer != null && crackLayer2Renderer.enabled)
            hatchSequence.Join(crackLayer2Renderer.DOFade(0f, hatchDuration * 0.5f));
        if (crackLayer3Renderer != null && crackLayer3Renderer.enabled)
            hatchSequence.Join(crackLayer3Renderer.DOFade(0f, hatchDuration * 0.5f));
        
        // 動物を登場させる
        if (animalRenderer != null)
        {
            animalRenderer.color = Color.white;
            animalRenderer.transform.localScale = Vector3.zero;
            
            // スケールアップ + ジャンプ
            hatchSequence.Append(animalRenderer.transform.DOScale(Vector3.one, hatchDuration).SetEase(Ease.OutBack));
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
