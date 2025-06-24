ばぶるがusing UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class BubbleController : MonoBehaviour
{
    [Header("基本設定")]
    [SerializeField] private float destroyDelay = 0.1f;
    
    [Header("移動設定")]
    [SerializeField] private float minSpeed = 0.3f;
    [SerializeField] private float maxSpeed = 0.8f;
    [SerializeField] private float maxMoveSpeed = 2f;
    
    [Header("物理設定")]
    [SerializeField] private float gravity = 3.0f;
    [SerializeField] [Range(0f, 1f)] private float bouncePower = 0.8f;
    [SerializeField] private float swayPower = 0.15f;
    [SerializeField] private float collisionForce = 1.0f;
    [SerializeField] private float airResistance = 1.0f;
    [SerializeField] private float collisionDrag = 1.5f;

    [Header("視覚効果")]
    [SerializeField] private Material bubbleMaterial;
    [SerializeField] private float highlightIntensity = 1.5f;
    [SerializeField] private float colorChangeSpeed = 0.5f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private float swayTimer;
    private float colorTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        rb.gravityScale = gravity;
        rb.linearDamping = airResistance;
        
        // マテリアル設定
        if(bubbleMaterial != null) {
            spriteRenderer.material = new Material(bubbleMaterial);
            spriteRenderer.material.SetFloat("_HighlightIntensity", highlightIntensity);
        }

        // 初期速度が設定されていない場合のみランダムな動きを設定
        if (rb.linearVelocity == Vector2.zero)
        {
            rb.linearVelocity = new Vector2(
                Random.Range(-swayPower, swayPower),
                -Random.Range(minSpeed, maxSpeed)
            );
            
            if(bouncePower > 0) {
                rb.AddForce(Vector2.up * bouncePower * 2f, ForceMode2D.Impulse);
            }
        }
    }

    void Update()
    {
        // 虹色効果
        colorTimer += Time.deltaTime * colorChangeSpeed;
        float hue = Mathf.PingPong(colorTimer, 1f);
        Color highlightColor = Color.HSVToRGB(hue, 0.3f, 1f);
        spriteRenderer.material.SetColor("_HighlightColor", highlightColor);
    }

    void FixedUpdate()
    {
        // 速度制限
        if (Mathf.Abs(rb.linearVelocity.x) > maxMoveSpeed) {
            rb.linearVelocity = new Vector2(
                Mathf.Sign(rb.linearVelocity.x) * maxMoveSpeed,
                rb.linearVelocity.y
            );
        }
        
        // ランダムな横揺れ
        swayTimer += Time.fixedDeltaTime;
        if(swayTimer > 0.3f) {
            rb.AddForce(new Vector2(
                Random.Range(-swayPower, swayPower),
                0
            ), ForceMode2D.Impulse);
            swayTimer = 0f;
        }
    }

    void OnMouseDown()
    {
        Destroy(gameObject, destroyDelay);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Bubble"))
        {
            Vector2 direction = (transform.position - collision.transform.position).normalized;
            float force = collisionForce * rb.linearVelocity.magnitude;
            
            rb.AddForce(direction * force, ForceMode2D.Impulse);
            rb.linearDamping = collisionDrag;
            Invoke(nameof(ResetDrag), 0.5f);
        }
    }

    private void ResetDrag()
    {
        rb.linearDamping = airResistance;
    }
}
