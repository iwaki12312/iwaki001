using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 動物の配置を管理するクラス
/// 時間帯に応じた動物をスポーンする
/// </summary>
public class AnimalSpawner : MonoBehaviour
{
    public static AnimalSpawner Instance { get; private set; }
    
    [Header("スポーン設定")]
    [SerializeField] private int spawnCount = 6;                    // 同時表示数
    [SerializeField] private float rareSpawnChance = 0.1f;          // レア出現確率（10%）
    
    [Header("スポーン範囲")]
    [SerializeField] private float minX = -4f;
    [SerializeField] private float maxX = 4f;
    [SerializeField] private float minY = -3f;
    [SerializeField] private float maxY = 2f;
    [SerializeField] private float minDistance = 1.5f;              // 動物間の最小距離
    
    [Header("動物サイズ")]
    [SerializeField] private float animalBaseScale = 1f;
    [SerializeField] private float rareAnimalScale = 1.3f;
    [SerializeField] private float colliderRadius = 1.0f;
    
    [Header("動物データ")]
    [SerializeField] private List<AnimalVoiceData> morningAnimals;       // 朝の動物
    [SerializeField] private List<AnimalVoiceData> daytimeAnimals;       // 昼の動物
    [SerializeField] private List<AnimalVoiceData> nightAnimals;         // 夜の動物
    [SerializeField] private List<AnimalVoiceData> rareAnimals;          // レア動物
    
    [Header("Prefab")]
    [SerializeField] private GameObject animalPrefab;
    
    [Header("パーティクル")]
    [SerializeField] private GameObject heartParticlePrefab;
    [SerializeField] private GameObject noteParticlePrefab;
    
    [Header("スポーンポイント")]
    [SerializeField] private List<AnimalSpawnPoint> spawnPoints = new List<AnimalSpawnPoint>();
    
    private List<AnimalController> activeAnimals = new List<AnimalController>();
    private AnimalVoiceTimeOfDay currentTimeOfDay = AnimalVoiceTimeOfDay.Morning;
    
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
    
    /// <summary>
    /// 動物データリストを設定（Initializerから呼び出し）
    /// </summary>
    public void SetAnimalData(List<AnimalVoiceData> morning, List<AnimalVoiceData> daytime, List<AnimalVoiceData> night, List<AnimalVoiceData> rare)
    {
        morningAnimals = morning;
        daytimeAnimals = daytime;
        nightAnimals = night;
        rareAnimals = rare;
    }
    
    /// <summary>
    /// Prefabを設定（Initializerから呼び出し）
    /// </summary>
    public void SetPrefabs(GameObject animal, GameObject heart, GameObject note)
    {
        animalPrefab = animal;
        heartParticlePrefab = heart;
        noteParticlePrefab = note;
    }
    
    /// <summary>
    /// スポーンポイントを設定（Initializerから呼び出し）
    /// </summary>
    public void SetSpawnPoints(List<AnimalSpawnPoint> points)
    {
        spawnPoints = points;
    }
    
    /// <summary>
    /// スポーン設定を一括で反映（Initializerから呼び出し）
    /// </summary>
    public void SetSpawnConfig(int count, float rareChance, float xMin, float xMax, float yMin, float yMax, float minDist, float baseScale, float rareScale, float colRadius)
    {
        spawnCount = count;
        rareSpawnChance = rareChance;
        minX = xMin;
        maxX = xMax;
        minY = yMin;
        maxY = yMax;
        minDistance = minDist;
        animalBaseScale = baseScale;
        rareAnimalScale = rareScale;
        colliderRadius = colRadius;
    }
    
    /// <summary>
    /// 指定した時間帯の動物をスポーン
    /// </summary>
    public void SpawnAnimalsForTimeOfDay(AnimalVoiceTimeOfDay timeOfDay)
    {
        currentTimeOfDay = timeOfDay;
        
        // 既存の動物をフェードアウト
        ClearAllAnimals();
        
        // 新しい動物をスポーン
        List<AnimalVoiceData> availableAnimals = GetAnimalsForTimeOfDay(timeOfDay);
        
        if (availableAnimals == null || availableAnimals.Count == 0)
        {
            Debug.LogWarning($"[AnimalSpawner] {timeOfDay}の動物データがありません");
            return;
        }
        
        // スポーンポイントがあればその位置を使用、なければランダム配置
        bool useSpawnPoints = spawnPoints != null && spawnPoints.Count > 0;
        int count = useSpawnPoints ? spawnPoints.Count : spawnCount;
        
        List<Vector3> usedPositions = new List<Vector3>();
        
        for (int i = 0; i < count; i++)
        {
            // レア動物の判定
            bool spawnRare = Random.value < rareSpawnChance && rareAnimals != null && rareAnimals.Count > 0;
            
            AnimalVoiceData dataToSpawn;
            if (spawnRare)
            {
                dataToSpawn = rareAnimals[Random.Range(0, rareAnimals.Count)];
            }
            else
            {
                dataToSpawn = availableAnimals[Random.Range(0, availableAnimals.Count)];
            }
            
            // スポーン位置を決定
            Vector3 spawnPos;
            float scaleOverride = 0f;
            if (useSpawnPoints)
            {
                var sp = spawnPoints[i];
                spawnPos = sp.transform.position;
                scaleOverride = sp.OverrideScale;
                sp.HidePreview(); // Play時プレビュー非表示
            }
            else
            {
                spawnPos = GetValidSpawnPosition(usedPositions);
                usedPositions.Add(spawnPos);
            }
            
            // 動物を生成
            SpawnAnimal(dataToSpawn, spawnPos, scaleOverride);
        }
        
        Debug.Log($"[AnimalSpawner] {timeOfDay}の動物を{count}匹スポーン");
    }
    
    /// <summary>
    /// 時間帯に対応した動物リストを取得
    /// </summary>
    private List<AnimalVoiceData> GetAnimalsForTimeOfDay(AnimalVoiceTimeOfDay timeOfDay)
    {
        switch (timeOfDay)
        {
            case AnimalVoiceTimeOfDay.Morning:
                return morningAnimals;
            case AnimalVoiceTimeOfDay.Daytime:
                return daytimeAnimals;
            case AnimalVoiceTimeOfDay.Night:
                return nightAnimals;
            default:
                return daytimeAnimals;
        }
    }
    
    /// <summary>
    /// 有効なスポーン位置を取得
    /// </summary>
    private Vector3 GetValidSpawnPosition(List<Vector3> usedPositions)
    {
        int maxAttempts = 30;
        
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            Vector3 pos = new Vector3(
                Random.Range(minX, maxX),
                Random.Range(minY, maxY),
                0
            );
            
            bool isValid = true;
            foreach (Vector3 usedPos in usedPositions)
            {
                if (Vector3.Distance(pos, usedPos) < minDistance)
                {
                    isValid = false;
                    break;
                }
            }
            
            if (isValid)
            {
                return pos;
            }
        }
        
        // 有効な位置が見つからない場合はランダム位置を返す
        return new Vector3(
            Random.Range(minX, maxX),
            Random.Range(minY, maxY),
            0
        );
    }
    
    /// <summary>
    /// 動物を生成
    /// </summary>
    private void SpawnAnimal(AnimalVoiceData data, Vector3 position, float scaleOverride = 0f)
    {
        if (animalPrefab == null)
        {
            Debug.LogError("[AnimalSpawner] animalPrefabが設定されていません");
            return;
        }
        
        GameObject animalObj = Instantiate(animalPrefab, position, Quaternion.identity);
        
        // Prefabが非アクティブのため、クローンをアクティブにしてAwake()を呼ばせる
        animalObj.SetActive(true);
        
        AnimalController controller = animalObj.GetComponent<AnimalController>();
        
        if (controller == null)
        {
            controller = animalObj.AddComponent<AnimalController>();
        }
        
        // スケールの決定: スポーンポイントのオーバーライド > レア/通常デフォルト
        float scale;
        if (scaleOverride > 0f)
        {
            scale = scaleOverride;
        }
        else
        {
            scale = data.isRare ? rareAnimalScale : animalBaseScale;
        }
        controller.Initialize(data, position, heartParticlePrefab, noteParticlePrefab, scale, colliderRadius);
        
        // 削除時にリストから除外し、新しい動物を補充
        controller.OnDestroyed += () =>
        {
            activeAnimals.Remove(controller);
            RespawnAnimal(position, scaleOverride);
        };
        
        activeAnimals.Add(controller);
    }
    
    /// <summary>
    /// 削除された動物を補充（同じ時間帯の別の動物）
    /// </summary>
    private void RespawnAnimal(Vector3 position, float scaleOverride)
    {
        // 時間帯切り替え中は補充しない
        if (activeAnimals.Count == 0) return;
        
        // 現在の時間帯の動物リストを取得
        List<AnimalVoiceData> availableAnimals = GetAnimalsForTimeOfDay(currentTimeOfDay);
        
        if (availableAnimals == null || availableAnimals.Count == 0)
        {
            Debug.LogWarning($"[AnimalSpawner] {currentTimeOfDay}の動物データがありません");
            return;
        }
        
        // レア動物の判定
        bool spawnRare = Random.value < rareSpawnChance && rareAnimals != null && rareAnimals.Count > 0;
        
        AnimalVoiceData dataToSpawn;
        if (spawnRare)
        {
            dataToSpawn = rareAnimals[Random.Range(0, rareAnimals.Count)];
        }
        else
        {
            dataToSpawn = availableAnimals[Random.Range(0, availableAnimals.Count)];
        }
        
        // 同じ位置に新しい動物を生成
        SpawnAnimal(dataToSpawn, position, scaleOverride);
    }
    
    /// <summary>
    /// すべての動物をクリア
    /// </summary>
    public void ClearAllAnimals()
    {
        // OnDestroyedイベントで補充されないように、先にリストをクリア
        var animalsToRemove = new List<AnimalController>(activeAnimals);
        activeAnimals.Clear();
        
        foreach (var animal in animalsToRemove)
        {
            if (animal != null)
            {
                animal.OnDestroyed = null; // 時間帯切替時は補充を抑制
                animal.FadeOutAndDestroy();
            }
        }
    }
    
    /// <summary>
    /// 現在の時間帯を取得
    /// </summary>
    public AnimalVoiceTimeOfDay GetCurrentTimeOfDay()
    {
        return currentTimeOfDay;
    }
}
