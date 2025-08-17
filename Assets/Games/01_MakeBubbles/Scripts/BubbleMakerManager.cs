using UnityEngine;

/// <summary>
/// シャボン玉生成を一元管理するクラス
/// </summary>
public class BubbleMakerManager : MonoBehaviour
{
    [SerializeField] private GameObject bubblePrefab;
    [SerializeField] private int maxBubbles = 10; // 最大シャボン玉数
    
    private bool isCreatingBubble = false; // 現在シャボン玉を作成中かどうか
    private float cooldownTimer = 0f; // クールダウンタイマー
    [SerializeField] private float cooldownDuration = 1.0f; // 作成後のクールダウン時間
    
    // シャボン玉の正確なカウント管理
    private int activeBubbleCount = 0; // 現在アクティブなシャボン玉数
    
    // シングルトンパターン
    public static BubbleMakerManager Instance { get; private set; }
    
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
        
        // 複数のシャボン玉を生成
        for (int i = 0; i < bubblesToCreate; i++)
        {
            // 少しずつ位置をずらして生成
            Vector2 adjustedOffset = new Vector2(
                spawnOffset.x + Random.Range(-0.3f, 0.3f),
                spawnOffset.y + Random.Range(-0.2f, 0.2f)
            );
            
            // シャボン玉生成
            SpawnBubble(creator, adjustedOffset, direction, bubbleSpeed);
        }
        
        // クールダウン開始
        isCreatingBubble = true;
        cooldownTimer = cooldownDuration;
        
        return true;
    }
    
    /// <summary>
    /// シャボン玉生成時にカウントを増加
    /// </summary>
    public void IncrementBubbleCount()
    {
        activeBubbleCount++;
        Debug.Log($"シャボン玉カウント増加: {activeBubbleCount}");
    }
    
    /// <summary>
    /// シャボン玉破壊時にカウントを減少
    /// </summary>
    public void DecrementBubbleCount()
    {
        activeBubbleCount = Mathf.Max(0, activeBubbleCount - 1);
        Debug.Log($"シャボン玉カウント減少: {activeBubbleCount}");
    }
    
    /// <summary>
    /// 現在アクティブなシャボン玉数を取得
    /// </summary>
    public int GetActiveBubbleCount()
    {
        return activeBubbleCount;
    }
    
    /// <summary>
    /// シャボン玉の数をカウント（外部からもアクセス可能）
    /// 新しいカウント方式を優先し、フォールバックとしてタグ検索を使用
    /// </summary>
    public int CountBubbles()
    {
        // 新しいカウント方式を使用
        int tagBasedCount = GameObject.FindGameObjectsWithTag("Bubble").Length;
        
        // デバッグ用：両方の値を比較
        if (activeBubbleCount != tagBasedCount)
        {
            Debug.LogWarning($"シャボン玉カウントの不一致: アクティブカウント={activeBubbleCount}, タグベース={tagBasedCount}");
            // タグベースの値で補正
            activeBubbleCount = tagBasedCount;
        }
        
        return activeBubbleCount;
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
    private void SpawnBubble(Transform creator, Vector2 spawnOffset, Vector2 direction, float bubbleSpeed)
    {
        if (bubblePrefab != null)
        {
            // 効果音を再生
            if (BubbleSoundManager.Instance != null)
            {
                BubbleSoundManager.Instance.PlayShotSound();
            }
            
            // 生成位置を計算
            Vector3 spawnPosition = creator.position + new Vector3(spawnOffset.x, spawnOffset.y, 0);
            GameObject bubble = Instantiate(bubblePrefab, spawnPosition, Quaternion.identity);

            // シャボン玉カウントを増加
            IncrementBubbleCount();

            // ランダムなサイズを設定
            float size = Random.Range(0.5f, 1.5f);
            bubble.transform.localScale = new Vector3(size, size, 1f);

            // ランダムな色を設定
            Color randomColor = new Color(
                Random.Range(0.7f, 1f),
                Random.Range(0.7f, 1f),
                Random.Range(0.7f, 1f),
                0.8f
            );
            SpriteRenderer renderer = bubble.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.color = randomColor;
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
}
