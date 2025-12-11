using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 動物データ
/// </summary>
[System.Serializable]
public class AnimalData
{
    public string name = "Animal";       // 動物名
    public Sprite animalSprite;          // 動物スプライト
    public bool isRare = false;          // レアフラグ
}

/// <summary>
/// たまごのスポーン位置データ
/// </summary>
[System.Serializable]
public class EggSpot
{
    public Vector3 position = Vector3.zero;  // たまごの位置
    public float scale = 1f;                  // スケール調整
    public float colliderRadius = 1.5f;       // タップ判定の範囲
}

/// <summary>
/// たまごのスポーン位置と動物種類を管理するクラス
/// </summary>
public class EggSpawner : MonoBehaviour
{
    [Header("スポーン位置設定")]
    [SerializeField] private EggSpot[] eggSpots = new EggSpot[4];
    
    [Header("動物設定")]
    [SerializeField] private AnimalData[] normalAnimals;    // 通常動物リスト
    [SerializeField] private AnimalData[] rareAnimals;      // レア動物リスト
    
    [Header("卵スプライト設定（5枚）")]
    [SerializeField] private Sprite eggSprite0;             // 通常の卵
    [SerializeField] private Sprite eggSprite1;             // ひび割れ段階1
    [SerializeField] private Sprite eggSprite2;             // ひび割れ段階2
    [SerializeField] private Sprite eggSprite3;             // ひび割れ段階3
    [SerializeField] private Sprite eggSprite4;             // 卵が割れた瞬間
    
    [Header("スポーン設定")]
    [SerializeField] private GameObject eggPrefab;          // たまごプレハブ
    [SerializeField] [Range(0f, 1f)] private float rareChance = 0.15f;  // レア出現確率
    
    private List<EggController> activeEggs = new List<EggController>();
    private Sprite[] eggStageSprites;  // 5枚の卵スプライト配列
    
    public static EggSpawner Instance { get; private set; }
    
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
        
        // 卵段階スプライト配列を作成（5枚）
        eggStageSprites = new Sprite[] { eggSprite0, eggSprite1, eggSprite2, eggSprite3, eggSprite4 };
    }
    
    void Start()
    {
        SpawnAllEggs();
    }
    
    /// <summary>
    /// すべてのスポット位置にたまごを生成
    /// </summary>
    private void SpawnAllEggs()
    {
        if (eggPrefab == null)
        {
            Debug.LogError("[EggSpawner] たまごプレハブが設定されていません");
            return;
        }
        
        for (int i = 0; i < eggSpots.Length; i++)
        {
            SpawnEggAt(eggSpots[i]);
        }
        
        Debug.Log($"[EggSpawner] {eggSpots.Length}個のたまごをスポーンしました");
    }
    
    /// <summary>
    /// 指定位置にたまごを生成
    /// </summary>
    private void SpawnEggAt(EggSpot spot)
    {
        // ランダムに動物を選択
        AnimalData selectedAnimal = SelectRandomAnimal();
        if (selectedAnimal == null)
        {
            Debug.LogWarning("[EggSpawner] 動物データが設定されていません");
            return;
        }
        
        // プレハブから生成
        GameObject eggObj = Instantiate(eggPrefab, spot.position, Quaternion.identity);
        eggObj.transform.SetParent(transform);
        
        // スケールを設定
        Vector3 prefabScale = eggPrefab.transform.localScale;
        eggObj.transform.localScale = new Vector3(
            prefabScale.x * spot.scale,
            prefabScale.y * spot.scale,
            prefabScale.z * spot.scale
        );
        eggObj.name = $"Egg_{selectedAnimal.name}";
        
        // コントローラーを初期化
        EggController controller = eggObj.GetComponent<EggController>();
        if (controller != null)
        {
            controller.Initialize(selectedAnimal, eggStageSprites, spot.position, OnEggReadyForRespawn);
            controller.SetColliderRadius(spot.colliderRadius);
            activeEggs.Add(controller);
        }
    }
    
    /// <summary>
    /// ランダムに動物を選択（レア判定込み）
    /// </summary>
    private AnimalData SelectRandomAnimal()
    {
        bool isRare = Random.value < rareChance;
        
        AnimalData selectedAnimal = null;
        
        if (isRare && rareAnimals != null && rareAnimals.Length > 0)
        {
            // レア動物を選択
            selectedAnimal = rareAnimals[Random.Range(0, rareAnimals.Length)];
        }
        // 通常動物を選択
        else if (normalAnimals != null && normalAnimals.Length > 0)
        {
            // 通常動物を選択
            selectedAnimal = normalAnimals[Random.Range(0, normalAnimals.Length)];
        }
        
        // レアフラグを設定したコピーを返す
        if (selectedAnimal != null)
        {
            AnimalData animalData = new AnimalData
            {
                name = selectedAnimal.name,
                animalSprite = selectedAnimal.animalSprite,
                isRare = isRare
            };
            return animalData;
        }
        
        return selectedAnimal;
    }
    
    /// <summary>
    /// たまごが再スポーン準備完了時のコールバック
    /// </summary>
    private void OnEggReadyForRespawn(EggController controller)
    {
        // 新しい動物を選択
        AnimalData newAnimal = SelectRandomAnimal();
        if (newAnimal == null) return;
        
        // 再初期化してフェードイン
        controller.Initialize(newAnimal, eggStageSprites, controller.transform.position, OnEggReadyForRespawn);
        controller.FadeIn();
        
        Debug.Log($"[EggSpawner] たまごを再スポーン: {newAnimal.name}, レア={newAnimal.isRare}");
    }
}
