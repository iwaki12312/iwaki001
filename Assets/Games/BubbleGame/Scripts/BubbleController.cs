using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class BubbleController : MonoBehaviour
{
    [Header("基本設定")]
    [SerializeField] private float destroyDelay = 0.1f;
    [SerializeField] private GameObject bubbleSplashAnimPrefab;
    
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
        // シャボン玉がはじけるエフェクトを生成
        CreateBubbleSplashEffect();
        
        // シャボン玉を破棄
        Destroy(gameObject, destroyDelay);
    }
    
    // シャボン玉がはじけるエフェクトを生成するメソッド
    private void CreateBubbleSplashEffect()
    {
        // プレハブが設定されていない場合は、Resources.Loadを試みる
        if (bubbleSplashAnimPrefab == null)
        {
            // 新しいプレハブを読み込む
            bubbleSplashAnimPrefab = Resources.Load<GameObject>("BubbleSplashAnim");
            
            // 見つかった場合はログ出力
            if (bubbleSplashAnimPrefab != null)
            {
                Debug.Log("BubbleSplashAnimプレハブを見つけました: " + bubbleSplashAnimPrefab.name);
            }
            else
            {
                // 直接パスを指定して読み込み
                bubbleSplashAnimPrefab = Resources.Load<GameObject>("Games/BubbleGame/Prefabs/BubbleSplashAnim");
                
                // それでも見つからない場合はプロジェクト内を検索
                if (bubbleSplashAnimPrefab == null)
                {
                    // 最後の手段として、プレハブを直接検索
                    GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
                    foreach (GameObject obj in allObjects)
                    {
                        if (obj.name == "BubbleSplashAnim")
                        {
                            bubbleSplashAnimPrefab = obj;
                            Debug.Log("シーン内でBubbleSplashAnimプレハブを見つけました");
                            break;
                        }
                    }
                    
                    if (bubbleSplashAnimPrefab == null)
                    {
                        Debug.LogWarning("BubbleSplashAnimプレハブが見つかりません。アニメーションが再生されない可能性があります。");
                    }
                }
            }
        }
        
        // プレハブが見つかった場合
        if (bubbleSplashAnimPrefab != null)
        {
            // シャボン玉の左側にエフェクトを生成
            Vector3 spawnPosition = transform.position + new Vector3(-0.5f, 0f, 0f);
            GameObject splashEffect = Instantiate(bubbleSplashAnimPrefab, spawnPosition, Quaternion.identity);
            
            // シャボン玉の色とサイズを反映
            BubbleSplashAnimController controller = splashEffect.GetComponent<BubbleSplashAnimController>();
            if (controller == null)
            {
                controller = splashEffect.AddComponent<BubbleSplashAnimController>();
            }
            
            if (controller != null)
            {
                controller.SetBubbleProperties(spriteRenderer.color, transform.localScale);
            }
            
            Debug.Log("シャボン玉がはじけるエフェクトを生成しました: " + splashEffect.name);
        }
        else
        {
            // プレハブが見つからない場合は直接インスタンス化
            GameObject splashEffect = new GameObject("BubbleSplashEffect");
            splashEffect.transform.position = transform.position + new Vector3(-0.5f, 0f, 0f);
            
            // BubbleSplashAnimプレハブを直接参照
            GameObject prefab = Resources.Load<GameObject>("Assets/Games/BubbleGame/Prefabs/BubbleSplashAnim");
            if (prefab != null)
            {
                splashEffect = Instantiate(prefab, transform.position + new Vector3(-0.5f, 0f, 0f), Quaternion.identity);
                
                // シャボン玉の色とサイズを反映
                BubbleSplashAnimController controller = splashEffect.AddComponent<BubbleSplashAnimController>();
                if (controller != null)
                {
                    controller.SetBubbleProperties(spriteRenderer.color, transform.localScale);
                }
            }
            else
            {
                Debug.LogWarning("BubbleSplashAnimプレハブが見つかりません");
            }
        }
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
