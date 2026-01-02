using UnityEngine;
using DG.Tweening;
using UnityEngine.InputSystem;

/// <summary>
/// 個別の風船を制御するクラス
/// マルチタッチ対応で浮上→タップ→破裂の流れを管理
/// </summary>
public class BalloonController : MonoBehaviour
{
    [Header("設定")]
    [SerializeField] protected Sprite balloonSprite;       // 風船のスプライト
    [SerializeField] protected float balloonSize = 1.0f;   // 統一する風船サイズ(Unity単位)
    [SerializeField] protected float colliderRadius = 0.5f; // コライダの半径(Inspector調整可能)
    [SerializeField] protected float floatSpeed = 1.0f;    // 浮上速度
    [SerializeField] protected float swayAmount = 0.5f;    // 横揺れの強さ
    [SerializeField] protected GameObject starParticlePrefab; // 星パーティクルPrefab
    [SerializeField] protected float particleScale = 1.0f; // パーティクルのスケール
    
    protected SpriteRenderer spriteRenderer;
    protected CircleCollider2D circleCollider;
    protected Rigidbody2D rb;
    protected Camera mainCamera;
    protected bool isPopped = false;
    public bool isDestroyedOffScreen { get; protected set; } = false; // 画面外で削除されたかどうか
    
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
        // Awake時点では仮の値を設定(後でSetColliderRadiusで上書きされる)
        circleCollider.radius = 0.5f;
        
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        
        mainCamera = Camera.main;
    }
    
    /// <summary>
    /// 風船を初期化
    /// </summary>
    public virtual void Initialize(Sprite sprite, Vector3 position, GameObject particlePrefab)
    {
        balloonSprite = sprite;
        starParticlePrefab = particlePrefab;
        transform.position = position;
        
        // スプライトを設定
        spriteRenderer.sprite = balloonSprite;
        spriteRenderer.sortingOrder = 1;
        
        // スプライトサイズを統一
        if (balloonSprite != null)
        {
            float spriteWidth = balloonSprite.bounds.size.x;
            float spriteHeight = balloonSprite.bounds.size.y;
            float currentSize = Mathf.Max(spriteWidth, spriteHeight);
            
            if (currentSize > 0)
            {
                float scale = balloonSize / currentSize;
                transform.localScale = Vector3.one * scale;
            }
        }
        
        // Rigidbody2Dの設定
        rb.gravityScale = 0f; // 重力無効
        rb.linearDamping = 0.2f; // 空気抵抗
        
        // 上向きの力を加える
        rb.linearVelocity = new Vector2(Random.Range(-swayAmount, swayAmount), floatSpeed);
    }
    
    /// <summary>
    /// コライダの半径を設定
    /// </summary>
    public void SetColliderRadius(float radius)
    {
        colliderRadius = radius;
        if (circleCollider != null)
        {
            circleCollider.radius = radius;
            Debug.Log($"[BalloonController] コライダサイズを設定: {radius}, 実際の値: {circleCollider.radius}");
        }
        else
        {
            Debug.LogWarning("[BalloonController] circleCollider が null です");
        }
    }
    
    void Update()
    {
        // マルチタッチ対応
        var touchscreen = Touchscreen.current;
        if (touchscreen != null)
        {
            foreach (var touch in touchscreen.touches)
            {
                if (!touch.press.wasPressedThisFrame) continue;
                if (!HitThisBalloon(touch.position.ReadValue())) continue;

                Pop();
                return;
            }
        }
        
        // エディタ用マウス操作
#if UNITY_EDITOR
        var mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.wasPressedThisFrame && HitThisBalloon(mouse.position.ReadValue()))
        {
            Pop();
            return;
        }
#endif
        
        // 画面外判定
        if (IsOutOfScreen())
        {
            isDestroyedOffScreen = true;
            Destroy(gameObject);
        }
    }
    
    void FixedUpdate()
    {
        // 横揺れ (Sin波)
        float sway = Mathf.Sin(Time.time * 2f) * swayAmount * 0.1f;
        rb.AddForce(new Vector2(sway, 0), ForceMode2D.Force);
    }
    
    /// <summary>
    /// 風船を破裂させる
    /// </summary>
    protected virtual void Pop()
    {
        if (isPopped) return;
        isPopped = true;
        
        // AnimalBalloonControllerがあればアニマルパラシュートを出現させる
        AnimalBalloonController animalController = GetComponent<AnimalBalloonController>();
        if (animalController != null)
        {
            animalController.SpawnAnimalParachute(transform.position);
        }
        
        // 星のパーティクルを生成
        if (starParticlePrefab != null)
        {
            GameObject particle = Instantiate(starParticlePrefab, transform.position, Quaternion.identity);
            particle.transform.localScale = Vector3.one * particleScale;
            Destroy(particle, 2f);
        }
        
        // 効果音を再生
        if (PopBalloonsSFXPlayer.Instance != null)
        {
            PopBalloonsSFXPlayer.Instance.PlayPopNormal();
        }
        
        // 即座に削除
        Destroy(gameObject);
    }
    
    /// <summary>
    /// タップ判定
    /// </summary>
    protected bool HitThisBalloon(Vector2 screenPosition)
    {
        if (mainCamera == null) return false;
        
        Vector2 worldPos = mainCamera.ScreenToWorldPoint(screenPosition);
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
        return hit && hit.collider != null && hit.collider.gameObject == gameObject;
    }
    
    /// <summary>
    /// 画面外判定
    /// </summary>
    protected bool IsOutOfScreen()
    {
        if (mainCamera == null) return false;
        
        Vector3 viewportPosition = mainCamera.WorldToViewportPoint(transform.position);
        return viewportPosition.y > 1.2f; // 画面上部を超えたら削除
    }
}
