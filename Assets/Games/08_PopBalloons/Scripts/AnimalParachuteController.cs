using UnityEngine;

/// <summary>
/// 動物とパラシュートを制御するクラス
/// </summary>
public class AnimalParachuteController : MonoBehaviour
{
    [Header("降下設定")]
    [SerializeField] private float fallSpeed = 0.3f;       // 降下速度
    [SerializeField] private float swayAmount = 0.3f;      // 横揺れの強さ
    
    private SpriteRenderer animalRenderer;
    private SpriteRenderer parachuteRenderer;
    private Rigidbody2D rb;
    private AnimalType animalType;
    private Camera mainCamera;
    private bool hasPlayedVoice = false;
    
    void Awake()
    {
        // 動物用のSpriteRenderer
        GameObject animalObj = new GameObject("Animal");
        animalObj.transform.SetParent(transform);
        animalObj.transform.localPosition = Vector3.zero;
        animalRenderer = animalObj.AddComponent<SpriteRenderer>();
        animalRenderer.sortingOrder = 2;
        
        // パラシュート用のSpriteRenderer
        GameObject parachuteObj = new GameObject("Parachute");
        parachuteObj.transform.SetParent(transform);
        parachuteObj.transform.localPosition = new Vector3(0, 0.8f, 0); // 少し上
        parachuteRenderer = parachuteObj.AddComponent<SpriteRenderer>();
        parachuteRenderer.sortingOrder = 1;
        
        // Rigidbody2D
        rb = gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        
        mainCamera = Camera.main;
    }
    
    /// <summary>
    /// 初期化
    /// </summary>
    public void Initialize(AnimalType type, Sprite animalSprite, Sprite parachuteSprite)
    {
        animalType = type;
        
        // スプライトを設定
        if (animalRenderer != null && animalSprite != null)
        {
            animalRenderer.sprite = animalSprite;
        }
        
        if (parachuteRenderer != null && parachuteSprite != null)
        {
            parachuteRenderer.sprite = parachuteSprite;
        }
        
        // 下向きの速度を設定
        rb.linearVelocity = new Vector2(0, -fallSpeed);
        
        Debug.Log($"[AnimalParachute] {type} が出現しました");
    }
    
    void Update()
    {
        // 鳴き声を1回だけ再生
        if (!hasPlayedVoice)
        {
            hasPlayedVoice = true;
            if (PopBalloonsSFXPlayer.Instance != null)
            {
                PopBalloonsSFXPlayer.Instance.PlayAnimalVoice(animalType);
            }
        }
        
        // 画面外判定
        if (IsOutOfScreen())
        {
            Destroy(gameObject);
        }
    }
    
    void FixedUpdate()
    {
        // 横揺れ (Sin波)
        float sway = Mathf.Sin(Time.time * 1.5f) * swayAmount * 0.1f;
        rb.AddForce(new Vector2(sway, 0), ForceMode2D.Force);
    }
    
    /// <summary>
    /// 画面外判定
    /// </summary>
    private bool IsOutOfScreen()
    {
        if (mainCamera == null) return false;
        
        Vector3 viewportPosition = mainCamera.WorldToViewportPoint(transform.position);
        return viewportPosition.y < -0.2f; // 画面下部を超えたら削除
    }
}
