using UnityEngine;
using DG.Tweening;

/// <summary>
/// 個別の風船を制御するクラス
/// マルチタッチ対応で浮上→タップ→破裂の流れを管理
/// </summary>
public class BalloonController : MonoBehaviour
{
    [Header("設定")]
    [SerializeField] protected Sprite balloonSprite;       // 風船のスプライト
    [SerializeField] protected float floatSpeed = 1.0f;    // 浮上速度
    [SerializeField] protected float swayAmount = 0.5f;    // 横揺れの強さ
    [SerializeField] protected GameObject starParticlePrefab; // 星パーティクルPrefab
    
    protected SpriteRenderer spriteRenderer;
    protected CircleCollider2D circleCollider;
    protected Rigidbody2D rb;
    protected Camera mainCamera;
    protected bool isPopped = false;
    
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
            circleCollider.radius = 0.5f;
        }
        
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
        
        // Rigidbody2Dの設定
        rb.gravityScale = 0f; // 重力無効
        rb.linearDamping = 0.2f; // 空気抵抗
        
        // 上向きの力を加える
        rb.linearVelocity = new Vector2(Random.Range(-swayAmount, swayAmount), floatSpeed);
    }
    
    void Update()
    {
        // マルチタッチ対応
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);
            if (touch.phase == TouchPhase.Began && HitThisBalloon(touch.position))
            {
                Pop();
                return;
            }
        }
        
        // エディタ用マウス操作
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0) && HitThisBalloon(Input.mousePosition))
        {
            Pop();
            return;
        }
#endif
        
        // 画面外判定
        if (IsOutOfScreen())
        {
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
        
        // 星のパーティクルを生成
        if (starParticlePrefab != null)
        {
            GameObject particle = Instantiate(starParticlePrefab, transform.position, Quaternion.identity);
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
