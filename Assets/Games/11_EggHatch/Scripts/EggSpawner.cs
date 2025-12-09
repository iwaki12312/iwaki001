using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 動物データ
/// </summary>
[System.Serializable]
public class AnimalData
{
    public string name = "Animal";       // 動物名
    public Sprite eggSprite;             // たまご本体スプライト
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
    
    [Header("レア卵設定")]
    [SerializeField] private Sprite rareEggSprite;          // レア卵共通スプライト
    
    [Header("ヒビスプライト（共通）")]
    [SerializeField] private Sprite crackSprite1;           // ヒビ段階1
    [SerializeField] private Sprite crackSprite2;           // ヒビ段階2
    [SerializeField] private Sprite crackSprite3;           // ヒビ段階3
    
    [Header("スポーン設定")]
    [SerializeField] private GameObject eggPrefab;          // たまごプレハブ
    [SerializeField] [Range(0f, 1f)] private float rareChance = 0.15f;  // レア出現確率
    
    private List<EggController> activeEggs = new List<EggController>();
    private Sprite[] crackSprites;
    
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
        
        // ヒビスプライト配列を作成
        crackSprites = new Sprite[] { crackSprite1, crackSprite2, crackSprite3 };
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
            controller.Initialize(selectedAnimal, crackSprites, spot.position, OnEggReadyForRespawn);
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
        
        if (isRare && rareAnimals != null && rareAnimals.Length > 0)
        {
            // レア動物を選択
            AnimalData rareAnimal = rareAnimals[Random.Range(0, rareAnimals.Length)];
            
            // レア卵共通スプライトを使用
            if (rareEggSprite != null)
            {
                // コピーを作成してeggSpriteを上書き
                AnimalData rareWithEgg = new AnimalData
                {
                    name = rareAnimal.name,
                    animalSprite = rareAnimal.animalSprite,
                    eggSprite = rareEggSprite,
                    isRare = true
                };
                return rareWithEgg;
            }
            return rareAnimal;
        }
        else if (normalAnimals != null && normalAnimals.Length > 0)
        {
            return normalAnimals[Random.Range(0, normalAnimals.Length)];
        }
        
        return null;
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
        controller.Initialize(newAnimal, crackSprites, controller.transform.position, OnEggReadyForRespawn);
        controller.FadeIn();
        
        Debug.Log($"[EggSpawner] たまごを再スポーン: {newAnimal.name}, レア={newAnimal.isRare}");
    }
}
