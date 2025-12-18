using UnityEngine;

/// <summary>
/// シャボン玉生成を一元管理するクラス
/// </summary>
public class BubbleMakerManager : MonoBehaviour
{
    private enum BubbleKind
    {
        Normal,
        Star,
        Heart,
        Note,
    }

    [SerializeField] private GameObject bubblePrefab;
    [SerializeField] private int maxBubbles = 10; // 最大シャボン玉数
    [SerializeField] private GameObject starBubblePrefab;
    [SerializeField] [Range(0f, 1f)] private float starBubbleChance = 0.1f; // 星入りシャボン玉の出現確率
    [SerializeField] private GameObject heartBubblePrefab;
    [SerializeField] [Range(0f, 1f)] private float heartBubbleChance = 0.05f; // ハート入りシャボン玉の出現確率
    [SerializeField] private GameObject noteBubblePrefab;
    [SerializeField] [Range(0f, 1f)] private float noteBubbleChance = 0.05f; // 音符入りシャボン玉の出現確率

    [Header("Bubble Color")]
    [SerializeField] private Color[] bubbleColorPalette;
    [SerializeField] private Vector2 bubbleRgbRange = new Vector2(0.7f, 1f);
    [SerializeField] [Range(0f, 1f)] private float bubbleAlpha = 0.8f;
    
    private bool isCreatingBubble = false; // 現在シャボン玉を作成中かどうか
    private float cooldownTimer = 0f; // クールダウンタイマー
    [SerializeField] private float cooldownDuration = 1.0f; // 作成後のクールダウン時間
    
    // シャボン玉の正確なカウント管理
    private int activeBubbleCount = 0; // 現在アクティブなシャボン玉数
    
    // シングルトンパターン
    public static BubbleMakerManager Instance { get; private set; }

    private Color GetRandomBubbleColor()
    {
        if (bubbleColorPalette != null && bubbleColorPalette.Length > 0)
        {
            Color c = bubbleColorPalette[Random.Range(0, bubbleColorPalette.Length)];
            c.a = bubbleAlpha;
            return c;
        }

        float min = Mathf.Clamp01(Mathf.Min(bubbleRgbRange.x, bubbleRgbRange.y));
        float max = Mathf.Clamp01(Mathf.Max(bubbleRgbRange.x, bubbleRgbRange.y));
        if (Mathf.Approximately(min, max))
        {
            max = Mathf.Min(1f, min + 0.0001f);
        }

        return new Color(
            Random.Range(min, max),
            Random.Range(min, max),
            Random.Range(min, max),
            bubbleAlpha
        );
    }

    private BubbleKind RollBubbleKind()
    {
        float starChance = (starBubblePrefab != null) ? Mathf.Clamp01(starBubbleChance) : 0f;
        float heartChance = (heartBubblePrefab != null) ? Mathf.Clamp01(heartBubbleChance) : 0f;
        float noteChance = (noteBubblePrefab != null) ? Mathf.Clamp01(noteBubbleChance) : 0f;

        heartChance = Mathf.Min(heartChance, 1f - starChance);
        noteChance = Mathf.Min(noteChance, 1f - starChance - heartChance);

        float roll = Random.value;
        if (roll < starChance) return BubbleKind.Star;
        if (roll < starChance + heartChance) return BubbleKind.Heart;
        if (roll < starChance + heartChance + noteChance) return BubbleKind.Note;
        return BubbleKind.Normal;
    }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // シーン遷移時も維持
            Debug.Log("BubbleMakerManagerのインスタンスを作成しました");
            
            // BubblePrefabの取得処理
            InitializeBubblePrefab();
        }
        else
        {
            Debug.Log("BubbleMakerManagerのインスタンスが既に存在するため、このインスタンスを破棄します");
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        // Startでも再度確認（他のオブジェクトが初期化された後）
        if (bubblePrefab == null)
        {
            Debug.LogWarning("Startでバブルプレハブの再取得を試行します");
            InitializeBubblePrefab();
        }
    }
    
    /// <summary>
    /// バブルプレハブの初期化処理
    /// </summary>
    private void InitializeBubblePrefab()
    {
        if (bubblePrefab != null)
        {
            Debug.Log("バブルプレハブは既に設定されています");
            return;
        }
        
        // GameInitializerからバブルプレハブを取得
        GameInitializer gameInitializer = FindObjectOfType<GameInitializer>();
        if (gameInitializer != null)
        {
            bubblePrefab = gameInitializer.GetBubblePrefab();
            if (bubblePrefab != null)
            {
                Debug.Log("GameInitializerからバブルプレハブを取得しました");
                return;
            }
        }
        
        // シーン内のBubbleオブジェクトを検索
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains("Bubble") && obj.CompareTag("Bubble"))
            {
                bubblePrefab = obj;
                Debug.Log($"シーン内でバブルプレハブを見つけました: {obj.name}");
                return;
            }
        }
        
        // BubbleControllerコンポーネントを持つオブジェクトを検索
        BubbleController[] bubbleControllers = FindObjectsOfType<BubbleController>();
        if (bubbleControllers.Length > 0)
        {
            bubblePrefab = bubbleControllers[0].gameObject;
            Debug.Log($"BubbleControllerを持つオブジェクトをバブルプレハブとして使用: {bubblePrefab.name}");
            return;
        }
        
        Debug.LogError("バブルプレハブが見つかりません。シーン内にBubbleオブジェクトまたはBubbleControllerコンポーネントを持つオブジェクトが必要です。");
    }
    
    void Update()
    {
        // クールダウン処理
        if (isCreatingBubble && cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0)
            {
                isCreatingBubble = false;
                Debug.Log("シャボン玉作成のクールダウンが終了しました");
            }
        }
    }
    
    /// <summary>
    /// シャボン玉作成リクエスト（犬や猫から呼び出される）
    /// </summary>
    public bool RequestBubbleCreation(Transform creator, Vector2 spawnOffset, Vector2 direction, float bubbleSpeed)
    {
        // すでに作成中または残りシャボン玉枠が3個未満の場合は拒否
        int currentBubbles = CountBubbles();
        if (isCreatingBubble || currentBubbles > maxBubbles - 3)
        {
            return false;
        }
        
        // 3～5個のシャボン玉をランダムに生成
        int bubblesToCreate = Random.Range(3, 6);
        
        // 残り枠を超えないように調整
        bubblesToCreate = Mathf.Min(bubblesToCreate, maxBubbles - currentBubbles);
        
        Debug.Log($"{creator.name}が{bubblesToCreate}個のシャボン玉を作成します");

        BubbleKind[] kinds = new BubbleKind[bubblesToCreate];
        bool containsNormal = false;
        bool containsSpecial = false;
        for (int i = 0; i < bubblesToCreate; i++)
        {
            kinds[i] = RollBubbleKind();
            if (kinds[i] == BubbleKind.Normal) containsNormal = true;
            else containsSpecial = true;
        }

        // 出現音はバッチで1回だけ（特殊が混ざっていればスター音に統一）
        if (BubbleSoundManager.Instance != null)
        {
            if (containsNormal)
                BubbleSoundManager.Instance.PlayShotSound();
            if (containsSpecial)
                BubbleSoundManager.Instance.PlayStarShotSound();
        }
        
        // 複数のシャボン玉を生成
        for (int i = 0; i < bubblesToCreate; i++)
        {
            // 少しずつ位置をずらして生成
            Vector2 adjustedOffset = new Vector2(
                spawnOffset.x + Random.Range(-0.3f, 0.3f),
                spawnOffset.y + Random.Range(-0.2f, 0.2f)
            );
            
            // シャボン玉生成
            SpawnBubble(creator, adjustedOffset, direction, bubbleSpeed, kinds[i]);
        }
        
        // クールダウン開始
        isCreatingBubble = true;
        cooldownTimer = cooldownDuration;
        
        return true;
    }
    
    /// <summary>
    /// シャボン玉の数をカウント（外部からもアクセス可能）
    /// </summary>
    public int CountBubbles()
    {
        return GameObject.FindGameObjectsWithTag("Bubble").Length;
    }
    
    /// <summary>
    /// 現在シャボン玉を作成中かどうか（外部からアクセス可能）
    /// </summary>
    public bool IsCreatingBubble => isCreatingBubble;
    
    /// <summary>
    /// 最大シャボン玉数（外部からアクセス可能）
    /// </summary>
    public int MaxBubbles => maxBubbles;
    
    /// <summary>
    /// シャボン玉生成
    /// </summary>
    private void SpawnBubble(Transform creator, Vector2 spawnOffset, Vector2 direction, float bubbleSpeed, BubbleKind kind)
    {
        if (bubblePrefab != null)
        {
            GameObject prefabToUse = bubblePrefab;
            if (kind == BubbleKind.Star && starBubblePrefab != null) prefabToUse = starBubblePrefab;
            else if (kind == BubbleKind.Heart && heartBubblePrefab != null) prefabToUse = heartBubblePrefab;
            else if (kind == BubbleKind.Note && noteBubblePrefab != null) prefabToUse = noteBubblePrefab;
            
            // 生成位置を計算
            Vector3 spawnPosition = creator.position + new Vector3(spawnOffset.x, spawnOffset.y, 0);
            GameObject bubble = Instantiate(prefabToUse, spawnPosition, Quaternion.identity);

            // ランダムなサイズを設定
            float size = Random.Range(0.5f, 1.5f);
            bubble.transform.localScale = new Vector3(size, size, 1f);

            SpriteRenderer renderer = bubble.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.color = GetRandomBubbleColor();
            }

            // シャボン玉に指定方向への初速度を設定
            Rigidbody2D rb = bubble.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                // 方向にランダム性を追加
                Vector2 randomizedDirection = new Vector2(
                    direction.x + Random.Range(-0.2f, 0.2f),
                    direction.y + Random.Range(-0.1f, 0.3f)
                ).normalized;
                
                rb.linearVelocity = randomizedDirection * bubbleSpeed * Random.Range(0.8f, 1.2f);
            }

            BubbleController bubbleController = bubble.GetComponent<BubbleController>();
            if (bubbleController != null)
            {
                bubbleController.SetStarBubble(kind == BubbleKind.Star);
                bubbleController.SetHeartBubble(kind == BubbleKind.Heart);
                bubbleController.SetNoteBubble(kind == BubbleKind.Note);
            }
            
            Debug.Log($"シャボン玉を生成しました: 位置={spawnPosition}, 方向={direction}");
        }
        else
        {
            Debug.LogError("バブルプレハブが設定されていません");
        }
    }
    
    /// <summary>
    /// バブルプレハブを取得
    /// </summary>
    public GameObject GetBubblePrefab()
    {
        return bubblePrefab;
    }
    
    /// <summary>
    /// バブルプレハブを設定
    /// </summary>
    public void SetBubblePrefab(GameObject prefab)
    {
        if (prefab != null)
        {
            bubblePrefab = prefab;
            Debug.Log($"バブルプレハブを設定しました: {prefab.name}");
        }
        else
        {
            Debug.LogWarning("設定しようとしたバブルプレハブがnullです");
        }
    }

    public void SetStarBubblePrefab(GameObject prefab)
    {
        starBubblePrefab = prefab;
        if (starBubblePrefab != null)
        {
            Debug.Log($"星入りバブルプレハブを設定しました: {starBubblePrefab.name}");
        }
        else
        {
            Debug.Log("星入りバブルプレハブが未設定です（星入りは出現しません）");
        }
    }

    public void SetStarBubbleChance(float chance)
    {
        starBubbleChance = Mathf.Clamp01(chance);
        Debug.Log($"星入りバブル出現確率を設定しました: {starBubbleChance:P0}");
    }

    public void SetHeartBubblePrefab(GameObject prefab)
    {
        heartBubblePrefab = prefab;
        if (heartBubblePrefab != null)
        {
            Debug.Log($"ハート入りバブルプレハブを設定しました: {heartBubblePrefab.name}");
        }
        else
        {
            Debug.Log("ハート入りバブルプレハブが未設定です（ハート入りは出現しません）");
        }
    }

    public void SetHeartBubbleChance(float chance)
    {
        heartBubbleChance = Mathf.Clamp01(chance);
        Debug.Log($"ハート入りバブル出現確率を設定しました: {heartBubbleChance:P0}");
    }

    public void SetNoteBubblePrefab(GameObject prefab)
    {
        noteBubblePrefab = prefab;
        if (noteBubblePrefab != null)
        {
            Debug.Log($"音符入りバブルプレハブを設定しました: {noteBubblePrefab.name}");
        }
        else
        {
            Debug.Log("音符入りバブルプレハブが未設定です（音符入りは出現しません）");
        }
    }

    public void SetNoteBubbleChance(float chance)
    {
        noteBubbleChance = Mathf.Clamp01(chance);
        Debug.Log($"音符入りバブル出現確率を設定しました: {noteBubbleChance:P0}");
    }
}
