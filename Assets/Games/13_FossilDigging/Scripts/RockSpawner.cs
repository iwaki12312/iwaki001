using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 岩の出現位置設定
/// </summary>
[System.Serializable]
public class RockSpawnPoint
{
    public string pointName = "Point";           // 位置名（Inspector表示用）
    public Vector3 position = Vector3.zero;      // 出現位置
    public float scale = 1f;                     // スケール
}

/// <summary>
/// 岩の出現を管理するクラス
/// </summary>
public class RockSpawner : MonoBehaviour
{
    public static RockSpawner Instance { get; private set; }

    [Header("岩出現位置設定 (6か所)")]
    [SerializeField] private RockSpawnPoint[] spawnPoints = new RockSpawnPoint[6];

    [Header("レア度出現確率")]
    [Range(0f, 1f)] [SerializeField] private float normalChance = 0.70f;
    [Range(0f, 1f)] [SerializeField] private float rareChance = 0.25f;
    [Range(0f, 1f)] [SerializeField] private float superRareChance = 0.05f;

    [Header("岩プレハブ")]
    [SerializeField] private GameObject rockPrefab;

    [Header("スプライト参照")]
    [SerializeField] private Sprite[] rockSprites;               // 岩スプライト（6種類）
    [SerializeField] private Sprite pickaxeSprite;               // ツルハシスプライト
    [SerializeField] private Sprite[] brokenPieceSprites;        // 破片スプライト
    [SerializeField] private Sprite[] normalTreasureSprites;     // ノーマル化石/宝石
    [SerializeField] private Sprite[] rareTreasureSprites;       // レア化石/宝石
    [SerializeField] private Sprite[] superRareTreasureSprites;  // スーパーレア化石/宝石

    [Header("エフェクト参照")]
    [SerializeField] private GameObject hitEffectPrefab;         // ヒット時エフェクト
    [SerializeField] private GameObject breakEffectPrefab;       // 破壊時エフェクト

    private Dictionary<int, GameObject> activeRocks = new Dictionary<int, GameObject>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        Debug.Log($"[RockSpawner] Start - spawnPoints.Length={spawnPoints?.Length ?? 0}");
        Debug.Log($"[RockSpawner] rockSprites.Length={rockSprites?.Length ?? 0}");
        
        // spawnPointsが空の場合はデフォルト位置を設定
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("[RockSpawner] spawnPointsが空です。デフォルト位置を設定します。");
            InitializeDefaultSpawnPoints();
        }
        
        // 初期化時に全位置に岩を生成
        SpawnAllRocks();
    }

    /// <summary>
    /// デフォルトの出現位置を初期化
    /// </summary>
    private void InitializeDefaultSpawnPoints()
    {
        spawnPoints = new RockSpawnPoint[6];
        float startX = -4f;
        float startY = -1f;
        float spacingX = 4f;
        float spacingY = 3f;

        for (int row = 0; row < 2; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                int index = row * 3 + col;
                spawnPoints[index] = new RockSpawnPoint
                {
                    pointName = $"Point_{index}",
                    position = new Vector3(startX + col * spacingX, startY - row * spacingY, 0),
                    scale = 1f
                };
            }
        }
    }

    /// <summary>
    /// 全位置に岩を生成
    /// </summary>
    public void SpawnAllRocks()
    {
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (!activeRocks.ContainsKey(i) || activeRocks[i] == null)
            {
                SpawnRockAt(i);
            }
        }
    }

    /// <summary>
    /// 指定位置に岩を生成
    /// </summary>
    public void SpawnRockAt(int pointIndex)
    {
        if (pointIndex < 0 || pointIndex >= spawnPoints.Length)
        {
            Debug.LogWarning($"[RockSpawner] 無効なポイントインデックス: {pointIndex}");
            return;
        }

        RockSpawnPoint point = spawnPoints[pointIndex];

        // 岩オブジェクトを生成
        GameObject rockObj;
        if (rockPrefab != null)
        {
            rockObj = Instantiate(rockPrefab, point.position, Quaternion.identity, transform);
        }
        else
        {
            rockObj = new GameObject($"Rock_{pointIndex}");
            rockObj.transform.position = point.position;
            rockObj.transform.SetParent(transform);
            SpriteRenderer sr = rockObj.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 10; // 背景より前面に表示
            rockObj.AddComponent<RockController>();
        }

        rockObj.name = $"Rock_{pointIndex}";
        rockObj.transform.localScale = Vector3.one * point.scale;

        // SpriteRendererのsortingOrderを確実に設定
        SpriteRenderer spriteRenderer = rockObj.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = 10;
        }

        // RockControllerを設定
        RockController controller = rockObj.GetComponent<RockController>();
        if (controller == null)
        {
            controller = rockObj.AddComponent<RockController>();
        }

        // スプライトを設定
        controller.SetSprites(rockSprites, pickaxeSprite, brokenPieceSprites,
                             normalTreasureSprites, rareTreasureSprites, superRareTreasureSprites);

        // エフェクトを設定
        controller.SetEffects(hitEffectPrefab, breakEffectPrefab);

        // 初期化
        int rockSpriteIndex = Random.Range(0, rockSprites?.Length ?? 1);
        controller.Initialize(rockSpriteIndex, pointIndex);

        // イベント登録
        controller.OnRockBroken += OnRockBroken;

        // アクティブな岩として登録
        activeRocks[pointIndex] = rockObj;

        Debug.Log($"[RockSpawner] 岩を生成: PointIndex={pointIndex}, Position={point.position}");
    }

    /// <summary>
    /// 岩が壊れた時のコールバック
    /// </summary>
    private void OnRockBroken(int pointIndex)
    {
        if (activeRocks.ContainsKey(pointIndex))
        {
            activeRocks.Remove(pointIndex);
        }

        // 少し遅延して新しい岩を生成
        StartCoroutine(DelayedSpawn(pointIndex, 0.5f));
    }

    /// <summary>
    /// 遅延して岩を生成
    /// </summary>
    private System.Collections.IEnumerator DelayedSpawn(int pointIndex, float delay)
    {
        yield return new WaitForSeconds(delay);
        SpawnRockAt(pointIndex);
    }

    /// <summary>
    /// レア度を決定
    /// </summary>
    public TreasureRarity DetermineRarity()
    {
        float total = normalChance + rareChance + superRareChance;
        float random = Random.Range(0f, total);

        if (random < superRareChance)
        {
            return TreasureRarity.SuperRare;
        }
        else if (random < superRareChance + rareChance)
        {
            return TreasureRarity.Rare;
        }
        else
        {
            return TreasureRarity.Normal;
        }
    }

    /// <summary>
    /// 出現位置を取得（エディタ用）
    /// </summary>
    public RockSpawnPoint[] GetSpawnPoints()
    {
        return spawnPoints;
    }

    /// <summary>
    /// 出現位置を設定（エディタ用）
    /// </summary>
    public void SetSpawnPoints(RockSpawnPoint[] points)
    {
        spawnPoints = points;
    }

#if UNITY_EDITOR
    /// <summary>
    /// エディタ上で出現位置を可視化
    /// </summary>
    void OnDrawGizmos()
    {
        if (spawnPoints == null) return;

        Gizmos.color = Color.yellow;
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (spawnPoints[i] != null)
            {
                Vector3 pos = spawnPoints[i].position;
                float size = spawnPoints[i].scale * 0.5f;
                Gizmos.DrawWireSphere(pos, size);
                UnityEditor.Handles.Label(pos + Vector3.up * size, $"Rock {i}");
            }
        }
    }
#endif
}
