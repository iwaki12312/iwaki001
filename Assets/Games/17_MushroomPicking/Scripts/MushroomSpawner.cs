using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// キノコのスポーン管理を行うクラス
/// 一定時間ごとにスポーンポイントにキノコを配置
/// </summary>
public class MushroomSpawner : MonoBehaviour
{
    public static MushroomSpawner Instance { get; private set; }

    [Header("スポーン設定")]
    [SerializeField] private int maxSimultaneous = 6;           // 同時表示の最大数
    [SerializeField] private float spawnInterval = 2f;          // スポーン間隔（秒）
    [SerializeField] private float rareSpawnChance = 0.1f;      // レア出現確率（10%）
    [SerializeField] private float superRareSpawnChance = 0.03f; // スーパーレア出現確率（3%）
    [SerializeField] private float hideTimeout = 7f;            // タップしないと引っ込むまでの時間

    [Header("キノコサイズ")]
    [SerializeField] private float mushroomBaseScale = 1f;
    [SerializeField] private float colliderRadius = 1.2f;

    [Header("キノコデータ")]
    [SerializeField] private List<MushroomPickingData> normalMushrooms;
    [SerializeField] private List<MushroomPickingData> rareMushrooms;
    [SerializeField] private List<MushroomPickingData> superRareMushrooms;

    [Header("Prefab")]
    [SerializeField] private GameObject mushroomPrefab;

    [Header("パーティクル")]
    [SerializeField] private GameObject sparkleParticlePrefab;
    [SerializeField] private GameObject rareParticlePrefab;
    [SerializeField] private GameObject superRareParticlePrefab;

    [Header("カゴ")]
    [SerializeField] private Transform basketTransform;

    [Header("スポーンポイント")]
    [SerializeField] private List<MushroomSpawnPoint> spawnPoints = new List<MushroomSpawnPoint>();

    private List<MushroomController> activeMushrooms = new List<MushroomController>();
    private Dictionary<MushroomSpawnPoint, bool> spawnPointOccupied = new Dictionary<MushroomSpawnPoint, bool>();
    private float nextSpawnTime;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 初期化
        foreach (var sp in spawnPoints)
        {
            spawnPointOccupied[sp] = false;
        }

        // 最初のスポーン
        nextSpawnTime = Time.time + 0.5f;
    }

    void Update()
    {
        // 定期的にキノコをスポーン
        if (Time.time >= nextSpawnTime && activeMushrooms.Count < maxSimultaneous)
        {
            SpawnMushroomAtRandomPoint();
            nextSpawnTime = Time.time + spawnInterval;
        }
    }

    /// <summary>
    /// キノコデータを設定
    /// </summary>
    public void SetMushroomData(List<MushroomPickingData> normal, List<MushroomPickingData> rare,
                                List<MushroomPickingData> superRare = null)
    {
        normalMushrooms = normal;
        rareMushrooms = rare;
        superRareMushrooms = superRare ?? new List<MushroomPickingData>();
    }

    /// <summary>
    /// Prefabを設定
    /// </summary>
    public void SetPrefabs(GameObject mushroom, GameObject sparkle, GameObject rareParticle,
                           GameObject superRareParticle = null)
    {
        mushroomPrefab = mushroom;
        sparkleParticlePrefab = sparkle;
        rareParticlePrefab = rareParticle;
        superRareParticlePrefab = superRareParticle;
    }

    /// <summary>
    /// スポーンポイントを設定
    /// </summary>
    public void SetSpawnPoints(List<MushroomSpawnPoint> points)
    {
        spawnPoints = points;
        spawnPointOccupied.Clear();
        foreach (var sp in spawnPoints)
        {
            spawnPointOccupied[sp] = false;
        }
    }

    /// <summary>
    /// カゴのTransformを設定
    /// </summary>
    public void SetBasket(Transform basket)
    {
        basketTransform = basket;
    }

    /// <summary>
    /// スポーン設定を一括反映
    /// </summary>
    public void SetSpawnConfig(int maxSim, float interval, float rareChance,
                                float baseScale, float colRadius, float timeout,
                                float superRareChance = 0.03f)
    {
        maxSimultaneous = maxSim;
        spawnInterval = interval;
        rareSpawnChance = rareChance;
        superRareSpawnChance = superRareChance;
        mushroomBaseScale = baseScale;
        colliderRadius = colRadius;
        hideTimeout = timeout;
    }

    /// <summary>
    /// 空いているスポーンポイントにキノコをスポーン
    /// </summary>
    private void SpawnMushroomAtRandomPoint()
    {
        // 空いているスポーンポイントを取得
        List<MushroomSpawnPoint> availablePoints = new List<MushroomSpawnPoint>();
        foreach (var sp in spawnPoints)
        {
            if (!spawnPointOccupied.ContainsKey(sp)) spawnPointOccupied[sp] = false;
            if (!spawnPointOccupied[sp])
            {
                availablePoints.Add(sp);
            }
        }

        if (availablePoints.Count == 0) return;

        // ランダムにスポーンポイントを選択
        MushroomSpawnPoint selectedPoint = availablePoints[Random.Range(0, availablePoints.Count)];

        // スーパーレア → レア → 通常 の優先判定
        float roll = Random.value;
        bool spawnSuperRare = roll < superRareSpawnChance && superRareMushrooms != null && superRareMushrooms.Count > 0;
        bool spawnRare = !spawnSuperRare && roll < rareSpawnChance && rareMushrooms != null && rareMushrooms.Count > 0;

        MushroomPickingData dataToSpawn;
        if (spawnSuperRare)
        {
            dataToSpawn = superRareMushrooms[Random.Range(0, superRareMushrooms.Count)];
        }
        else if (spawnRare)
        {
            dataToSpawn = rareMushrooms[Random.Range(0, rareMushrooms.Count)];
        }
        else
        {
            if (normalMushrooms == null || normalMushrooms.Count == 0)
            {
                Debug.LogWarning("[MushroomSpawner] 通常キノコデータがありません");
                return;
            }
            dataToSpawn = normalMushrooms[Random.Range(0, normalMushrooms.Count)];
        }

        // キノコを生成
        SpawnMushroom(dataToSpawn, selectedPoint);
    }

    /// <summary>
    /// 指定スポーンポイントにキノコを生成
    /// </summary>
    private void SpawnMushroom(MushroomPickingData data, MushroomSpawnPoint point)
    {
        if (mushroomPrefab == null)
        {
            Debug.LogError("[MushroomSpawner] mushroomPrefabが設定されていません");
            return;
        }

        Vector3 position = point.transform.position;
        float scaleOverride = point.OverrideScale;

        GameObject mushroomObj = Instantiate(mushroomPrefab, position, Quaternion.identity);
        mushroomObj.SetActive(true);

        MushroomController controller = mushroomObj.GetComponent<MushroomController>();
        if (controller == null)
        {
            controller = mushroomObj.AddComponent<MushroomController>();
        }

        float scale = scaleOverride > 0f ? scaleOverride : mushroomBaseScale;
        controller.Initialize(data, position, basketTransform, scale, colliderRadius, hideTimeout);

        // スポーンポイントを占有
        spawnPointOccupied[point] = true;

        // パーティクル生成
        SpawnParticle(position, data.isRare || data.isSuperRare);

        // 収穫時のコールバック
        controller.OnPickedUp += () =>
        {
            activeMushrooms.Remove(controller);
            spawnPointOccupied[point] = false;
            // 収穫後、少し待ってから次のキノコが出現
        };

        // 引っ込み時のコールバック
        controller.OnHidden += () =>
        {
            activeMushrooms.Remove(controller);
            spawnPointOccupied[point] = false;
        };

        activeMushrooms.Add(controller);
    }

    /// <summary>
    /// スーパーレア用パーティクルprefabを取得（MushroomControllerから参照）
    /// </summary>
    public GameObject GetSuperRareParticlePrefab()
    {
        return superRareParticlePrefab;
    }

    /// <summary>
    /// パーティクルを生成
    /// </summary>
    private void SpawnParticle(Vector3 position, bool isRare)
    {
        GameObject prefab = isRare ? rareParticlePrefab : sparkleParticlePrefab;
        if (prefab != null)
        {
            Vector3 particlePos = position + new Vector3(0, 0.3f, 0);
            GameObject particle = Instantiate(prefab, particlePos, Quaternion.identity);
            Destroy(particle, 2f);
        }
    }
}
