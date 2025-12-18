using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class BubbleController : MonoBehaviour
{
    [Header("基本設定")]
    [SerializeField] private float destroyDelay = 0.1f;
    [SerializeField] private GameObject bubbleSplashAnimPrefab;
    [SerializeField] private bool isStarBubble = false;
    [SerializeField] private bool isHeartBubble = false;
    [SerializeField] private bool isNoteBubble = false;

    [Header("Star Burst")]
    [SerializeField] private Sprite starBurstSprite;
    [SerializeField] private int starBurstCountMin = 6;
    [SerializeField] private int starBurstCountMax = 8;
    [SerializeField] private float starBurstSpeedMin = 2.0f;
    [SerializeField] private float starBurstSpeedMax = 4.0f;
    [SerializeField] private float starBurstLifetime = 0.7f;
    [SerializeField] private float starBurstAngularSpeedMin = -360f;
    [SerializeField] private float starBurstAngularSpeedMax = 360f;
    [SerializeField] private float starBurstScaleMultiplier = 0.6f;

    [Header("Heart Burst")]
    [SerializeField] private Sprite heartBurstSprite;
    [SerializeField] private int heartBurstCountMin = 6;
    [SerializeField] private int heartBurstCountMax = 8;
    [SerializeField] private float heartBurstSpeedMin = 2.0f;
    [SerializeField] private float heartBurstSpeedMax = 4.0f;
    [SerializeField] private float heartBurstLifetime = 0.7f;
    [SerializeField] private float heartBurstAngularSpeedMin = -360f;
    [SerializeField] private float heartBurstAngularSpeedMax = 360f;
    [SerializeField] private float heartBurstScaleMultiplier = 0.6f;

    [Header("Note Burst")]
    [SerializeField] private Sprite noteBurstSprite;
    [SerializeField] private int noteBurstCountMin = 6;
    [SerializeField] private int noteBurstCountMax = 8;
    [SerializeField] private float noteBurstSpeedMin = 2.0f;
    [SerializeField] private float noteBurstSpeedMax = 4.0f;
    [SerializeField] private float noteBurstLifetime = 0.7f;
    [SerializeField] private float noteBurstAngularSpeedMin = -360f;
    [SerializeField] private float noteBurstAngularSpeedMax = 360f;
    [SerializeField] private float noteBurstScaleMultiplier = 0.6f;

    [Header("移動設定")]
    [SerializeField] private float minSpeed = 0.3f;
    [SerializeField] private float maxSpeed = 0.8f;
    [SerializeField] private float maxMoveSpeed = 2f;

    [Header("物理設定")]
    [SerializeField] private float gravity = 0.05f; // 効かない　インスペクタから設定する
    [SerializeField][Range(0f, 1f)] private float bouncePower = 0.8f;
    [SerializeField] private float swayPower = 0.15f;
    [SerializeField] private float collisionForce = 1.0f;
    [SerializeField] private float airResistance = 1.0f;
    [SerializeField] private float collisionDrag = 1.5f;

    [Header("視覚効果")]
    [SerializeField] private Material bubbleMaterial;
    [SerializeField] private float highlightIntensity = 1.5f;
    [SerializeField] private float colorChangeSpeed = 0.5f;
    
    [Header("画面外削除設定")]
    [SerializeField] private float screenMargin = 0.1f; // 画面外判定のマージン

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private float swayTimer;
    private float colorTimer;
    static Camera mainCam;      // Camera.main を毎フレーム探さないようにキャッシュ

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        rb.gravityScale = gravity;
        rb.linearDamping = airResistance;

        // マテリアル設定
        if (bubbleMaterial != null)
        {
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

            if (bouncePower > 0)
            {
                rb.AddForce(Vector2.up * bouncePower * 2f, ForceMode2D.Impulse);
            }
        }

        if (!mainCam) mainCam = Camera.main;   // 一度だけ取得
    }

    void Update()
    {
        /* ❶ すべてのタッチを調べる（実機） */
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch t = Input.GetTouch(i);
            if (t.phase == TouchPhase.Began && HitThisBubble(t.position))
                Burst();                       // ← 元の OnMouseDown 相当
        }

        /* ❂ エディタ／マウス操作用フォールバック */
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0) && HitThisBubble(Input.mousePosition))
            Burst();
#endif

        /* ❸ 画面外判定 */
        if (IsOutOfScreen())
        {
            Debug.Log($"シャボン玉 {gameObject.name} が画面外に出たため削除します");
            Destroy(gameObject);
        }
    }

    /* -------- Bubble を割る本処理（旧 OnMouseDown の中身） -------- */
    void Burst()
    {
        if (!isStarBubble && !isHeartBubble && !isNoteBubble)
        {
            CreateBubbleSplashEffect();
        }

        if (BubbleSoundManager.Instance != null)
        {
            if (isStarBubble)
                BubbleSoundManager.Instance.PlayStarSplashSound();
            else if (isHeartBubble)
                BubbleSoundManager.Instance.PlayHeartSplashSound();
            else if (isNoteBubble)
                BubbleSoundManager.Instance.PlayNoteSplashSound();
            else
                BubbleSoundManager.Instance.PlaySplashSound();
        }
        else
            Debug.LogWarning("BubbleSoundManager が見つかりません。効果音が再生されません。");

        if (isStarBubble) SpawnStarBurst();
        else if (isHeartBubble) SpawnHeartBurst();
        else if (isNoteBubble) SpawnNoteBurst();

        Destroy(gameObject, destroyDelay);
    }

    public void SetStarBubble(bool isStar)
    {
        isStarBubble = isStar;
        if (isStar)
        {
            isHeartBubble = false;
            isNoteBubble = false;
        }
    }

    public void SetHeartBubble(bool isHeart)
    {
        isHeartBubble = isHeart;
        if (isHeart)
        {
            isStarBubble = false;
            isNoteBubble = false;
        }
    }

    public void SetNoteBubble(bool isNote)
    {
        isNoteBubble = isNote;
        if (isNote)
        {
            isStarBubble = false;
            isHeartBubble = false;
        }
    }

    private void SpawnStarBurst()
    {
        if (starBurstSprite == null)
        {
            Debug.LogWarning("StarBurstSprite が未設定のため、星バースト演出をスキップします。");
            return;
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        Color burstColor = spriteRenderer != null ? spriteRenderer.color : Color.white;

        int countMin = Mathf.Max(1, starBurstCountMin);
        int countMax = Mathf.Max(countMin, starBurstCountMax);
        int starCount = Random.Range(countMin, countMax + 1);

        float speedMin = Mathf.Max(0f, starBurstSpeedMin);
        float speedMax = Mathf.Max(speedMin, starBurstSpeedMax);

        float lifetime = Mathf.Max(0.01f, starBurstLifetime);

        float scaleMultiplier = Mathf.Max(0.01f, starBurstScaleMultiplier);
        Vector3 starScale = transform.localScale * scaleMultiplier;

        int sortingLayerId = spriteRenderer != null ? spriteRenderer.sortingLayerID : 0;
        int sortingOrder = spriteRenderer != null ? spriteRenderer.sortingOrder + 1 : 1;

        for (int i = 0; i < starCount; i++)
        {
            Vector2 direction = Random.insideUnitCircle;
            if (direction == Vector2.zero) direction = Vector2.right;
            direction.Normalize();

            float speed = Random.Range(speedMin, speedMax);
            float angularSpeed = Random.Range(starBurstAngularSpeedMin, starBurstAngularSpeedMax);

            GameObject starObj = new GameObject("StarPiece");
            starObj.transform.position = transform.position;
            starObj.transform.localScale = starScale;

            SpriteRenderer starRenderer = starObj.AddComponent<SpriteRenderer>();
            starRenderer.sprite = starBurstSprite;
            starRenderer.color = burstColor;
            starRenderer.sortingLayerID = sortingLayerId;
            starRenderer.sortingOrder = sortingOrder;

            StarPieceController controller = starObj.AddComponent<StarPieceController>();
            controller.Initialize(direction * speed, angularSpeed, lifetime, burstColor);
        }
    }

    /* -------- 画面座標 pos がこのバブルに当たったか？ -------- */
    private void SpawnHeartBurst()
    {
        if (heartBurstSprite == null)
        {
            Debug.LogWarning("HeartBurstSprite が未設定のため、ハートバースト演出をスキップします。");
            return;
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        Color burstColor = spriteRenderer != null ? spriteRenderer.color : Color.white;

        int countMin = Mathf.Max(1, heartBurstCountMin);
        int countMax = Mathf.Max(countMin, heartBurstCountMax);
        int heartCount = Random.Range(countMin, countMax + 1);

        float speedMin = Mathf.Max(0f, heartBurstSpeedMin);
        float speedMax = Mathf.Max(speedMin, heartBurstSpeedMax);

        float lifetime = Mathf.Max(0.01f, heartBurstLifetime);

        float scaleMultiplier = Mathf.Max(0.01f, heartBurstScaleMultiplier);
        Vector3 heartScale = transform.localScale * scaleMultiplier;

        int sortingLayerId = spriteRenderer != null ? spriteRenderer.sortingLayerID : 0;
        int sortingOrder = spriteRenderer != null ? spriteRenderer.sortingOrder + 1 : 1;

        for (int i = 0; i < heartCount; i++)
        {
            Vector2 direction = Random.insideUnitCircle;
            if (direction == Vector2.zero) direction = Vector2.right;
            direction.Normalize();

            float speed = Random.Range(speedMin, speedMax);
            float angularSpeed = Random.Range(heartBurstAngularSpeedMin, heartBurstAngularSpeedMax);

            GameObject heartObj = new GameObject("HeartPiece");
            heartObj.transform.position = transform.position;
            heartObj.transform.localScale = heartScale;

            SpriteRenderer heartRenderer = heartObj.AddComponent<SpriteRenderer>();
            heartRenderer.sprite = heartBurstSprite;
            heartRenderer.color = burstColor;
            heartRenderer.sortingLayerID = sortingLayerId;
            heartRenderer.sortingOrder = sortingOrder;

            StarPieceController controller = heartObj.AddComponent<StarPieceController>();
            controller.Initialize(direction * speed, angularSpeed, lifetime, burstColor);
        }
    }

    bool HitThisBubble(Vector2 pos)
    {
        Vector2 world = mainCam.ScreenToWorldPoint(pos);
        // Collider2D を使わず SpriteRenderer.bounds にヒットさせてもよいが、物理演算に合わせて Raycast
        RaycastHit2D hit = Physics2D.Raycast(world, Vector2.zero);
        return hit && hit.collider != null && hit.collider.gameObject == gameObject;
    }

    /* -------- シャボン玉が画面外に出たかどうかを判定 -------- */
    private bool IsOutOfScreen()
    {
        if (mainCam == null) return false;
        
        // カメラのビューポート座標に変換（0〜1の範囲）
        Vector3 viewportPosition = mainCam.WorldToViewportPoint(transform.position);
        
        // ビューポート座標が0〜1の範囲外なら画面外と判定
        // マージンを設けて、完全に見えなくなってから削除
        return viewportPosition.x < -screenMargin || 
               viewportPosition.x > 1 + screenMargin || 
               viewportPosition.y < -screenMargin || 
               viewportPosition.y > 1 + screenMargin;
    }

    private void OnMouseDown() => Burst(); // マウス操作のフォールバック


    void FixedUpdate()
    {
        // 速度制限
        if (Mathf.Abs(rb.linearVelocity.x) > maxMoveSpeed)
        {
            rb.linearVelocity = new Vector2(
                Mathf.Sign(rb.linearVelocity.x) * maxMoveSpeed,
                rb.linearVelocity.y
            );
        }

        // ランダムな横揺れ
        swayTimer += Time.fixedDeltaTime;
        if (swayTimer > 0.3f)
        {
            rb.AddForce(new Vector2(
                Random.Range(-swayPower, swayPower),
                0
            ), ForceMode2D.Impulse);
            swayTimer = 0f;
        }
    }

    // シャボン玉がはじけるエフェクトを生成するメソッド
    private void CreateBubbleSplashEffect()
    {
        // プレハブが設定されていない場合は、プロジェクト内から検索
        if (bubbleSplashAnimPrefab == null)
        {
            Debug.Log("BubbleSplashAnimプレハブの読み込みを開始します。プロジェクト内から検索します。");

            // シーン内のBubbleSplashAnimプレハブを検索
            GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.name.Contains("BubbleSplashAnim"))
                {
                    bubbleSplashAnimPrefab = obj;
                    Debug.Log("BubbleSplashAnimプレハブを見つけました: " + obj.name);
                    break;
                }
            }

            // それでも見つからない場合は新しく作成
            if (bubbleSplashAnimPrefab == null)
            {
                Debug.Log("BubbleSplashAnimプレハブが見つからないため、新しく作成します。");

                bubbleSplashAnimPrefab = new GameObject("BubbleSplashAnim");

                // 必要なコンポーネントを追加
                SpriteRenderer renderer = bubbleSplashAnimPrefab.AddComponent<SpriteRenderer>();
                bubbleSplashAnimPrefab.AddComponent<BubbleSplashAnimController>();

                // スプライトを設定
                Sprite[] allSprites = Resources.FindObjectsOfTypeAll<Sprite>();
                foreach (Sprite sprite in allSprites)
                {
                    if (sprite.name.Contains("BubbleSplashResources"))
                    {
                        renderer.sprite = sprite;
                        Debug.Log("BubbleSplashAnim用スプライトを見つけました: " + sprite.name);
                        break;
                    }
                }

                // 非表示にして、プレハブとして使用
                bubbleSplashAnimPrefab.SetActive(false);
            }
        }

        // プレハブが見つかった場合
        if (bubbleSplashAnimPrefab != null)
        {
            // シャボン玉の左側にエフェクトを生成
            Vector3 spawnPosition = transform.position + new Vector3(-0.5f, 0f, 0f);

            // プレハブのスケールを保存
            Vector3 originalScale = bubbleSplashAnimPrefab.transform.localScale;

            // インスタンス化
            GameObject splashEffect = Instantiate(bubbleSplashAnimPrefab, spawnPosition, Quaternion.identity);

            // インスタンス化直後のスケールをログ出力
            Debug.Log("インスタンス化直後のエフェクトスケール: " + splashEffect.transform.localScale);

            // シャボン玉の色とサイズを反映
            BubbleSplashAnimController controller = splashEffect.GetComponent<BubbleSplashAnimController>();
            if (controller == null)
            {
                controller = splashEffect.AddComponent<BubbleSplashAnimController>();
            }

            // アニメーターによるスケール変更を無効化するコンポーネントを追加
            DisableAnimatorScale scaleController = splashEffect.GetComponent<DisableAnimatorScale>();
            if (scaleController == null)
            {
                scaleController = splashEffect.AddComponent<DisableAnimatorScale>();
            }

            if (controller != null)
            {
                // シャボン玉のサイズに合わせてエフェクトのサイズを調整
                Vector3 bubbleScale = transform.localScale;
                Debug.Log("シャボン玉のサイズ: " + bubbleScale);

                // エフェクトのサイズを明示的に設定
                splashEffect.transform.localScale = bubbleScale;
                Debug.Log("エフェクトのスケールを直接設定: " + splashEffect.transform.localScale);

                // スケールコントローラーを更新
                if (scaleController != null)
                {
                    scaleController.UpdateScale(bubbleScale);
                }

                // コントローラーにも設定
                controller.SetBubbleProperties(spriteRenderer.color, bubbleScale);
                Debug.Log("SetBubbleProperties呼び出し後のエフェクトスケール: " + splashEffect.transform.localScale);
            }

            Debug.Log("シャボン玉がはじけるエフェクトを生成しました: " + splashEffect.name);
        }
        else
        {
            Debug.LogWarning("BubbleSplashAnimプレハブが見つかりません");
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

    private void SpawnNoteBurst()
    {
        if (noteBurstSprite == null)
        {
            Debug.LogWarning("NoteBurstSprite が未設定のため、音符バースト演出をスキップします。");
            return;
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        Color burstColor = spriteRenderer != null ? spriteRenderer.color : Color.white;

        int countMin = Mathf.Max(1, noteBurstCountMin);
        int countMax = Mathf.Max(countMin, noteBurstCountMax);
        int noteCount = Random.Range(countMin, countMax + 1);

        float speedMin = Mathf.Max(0f, noteBurstSpeedMin);
        float speedMax = Mathf.Max(speedMin, noteBurstSpeedMax);

        float lifetime = Mathf.Max(0.01f, noteBurstLifetime);

        float scaleMultiplier = Mathf.Max(0.01f, noteBurstScaleMultiplier);
        Vector3 noteScale = transform.localScale * scaleMultiplier;

        int sortingLayerId = spriteRenderer != null ? spriteRenderer.sortingLayerID : 0;
        int sortingOrder = spriteRenderer != null ? spriteRenderer.sortingOrder + 1 : 1;

        for (int i = 0; i < noteCount; i++)
        {
            Vector2 direction = Random.insideUnitCircle;
            if (direction == Vector2.zero) direction = Vector2.right;
            direction.Normalize();

            float speed = Random.Range(speedMin, speedMax);
            float angularSpeed = Random.Range(noteBurstAngularSpeedMin, noteBurstAngularSpeedMax);

            GameObject noteObj = new GameObject("NotePiece");
            noteObj.transform.position = transform.position;
            noteObj.transform.localScale = noteScale;

            SpriteRenderer noteRenderer = noteObj.AddComponent<SpriteRenderer>();
            noteRenderer.sprite = noteBurstSprite;
            noteRenderer.color = burstColor;
            noteRenderer.sortingLayerID = sortingLayerId;
            noteRenderer.sortingOrder = sortingOrder;

            StarPieceController controller = noteObj.AddComponent<StarPieceController>();
            controller.Initialize(direction * speed, angularSpeed, lifetime, burstColor);
        }
    }
}
