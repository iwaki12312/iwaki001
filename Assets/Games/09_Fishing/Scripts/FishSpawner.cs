using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 魚のスポーンを管理するクラス
/// </summary>
public class FishSpawner : MonoBehaviour
{
    [Header("魚設定")]
    [SerializeField] private GameObject fishPrefab;           // 魚Prefab
    [SerializeField] private Sprite[] normalFishSprites;      // 通常魚スプライト配列[20]
    [SerializeField] private Sprite[] rareFishSprites;        // レア魚スプライト配列[8]
    
    [Header("カモメ設定")]
    [SerializeField] private GameObject seagullPrefab;        // カモメPrefab
    
    [Header("スポーン設定")]
    [SerializeField] private float spawnInterval = 2f;        // スポーン間隔(秒)
    [SerializeField] private float spawnXPosition = -10f;     // スポーンX座標（画面左端）
    [SerializeField] private float spawnYMin = -3f;           // スポーンY最小値（海中の深さ）
    [SerializeField] private float spawnYMax = 1f;            // スポーンY最大値
    
    [Header("魚の移動設定")]
    [SerializeField] private float minSwimSpeed = 1.0f;       // 最小泳ぎ速度
    [SerializeField] private float maxSwimSpeed = 3.0f;       // 最大泳ぎ速度
    
    [Header("サイズ設定")]
    [SerializeField] private float baseFishSize = 2.0f;       // 魚の基準サイズ（Unity単位）
    
    [Header("確率設定")]
    [SerializeField] [Range(0f, 1f)] private float rareChance = 0.1f;      // レア出現確率
    [SerializeField] [Range(0f, 1f)] private float seagullChance = 0.1f;   // カモメ出現確率
    
    private List<GameObject> activeFish = new List<GameObject>();
    private float lastSpawnTime;
    
    public static FishSpawner Instance { get; private set; }
    
    /// <summary>
    /// カモメ出現確率を取得（FishControllerから参照）
    /// </summary>
    public float SeagullChance => seagullChance;
    
    /// <summary>
    /// カモメPrefabを取得（FishControllerから参照）
    /// </summary>
    public GameObject SeagullPrefab => seagullPrefab;
    
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
        lastSpawnTime = Time.time;
        Debug.Log($"[FishSpawner] Start - fishPrefab={fishPrefab != null}, normalSprites={normalFishSprites?.Length ?? 0}, rareSprites={rareFishSprites?.Length ?? 0}");
    }
    
    void Update()
    {
        // 非アクティブな魚をリストから削除
        activeFish.RemoveAll(fish => fish == null);
        
        // スポーン処理
        if (Time.time - lastSpawnTime >= spawnInterval)
        {
            Debug.Log($"[FishSpawner] スポーン試行: interval={spawnInterval}, elapsed={Time.time - lastSpawnTime}");
            SpawnFish();
            lastSpawnTime = Time.time;
        }
    }
    
    /// <summary>
    /// 魚をスポーン
    /// </summary>
    private void SpawnFish()
    {
        if (fishPrefab == null)
        {
            Debug.LogWarning("[FishSpawner] 魚Prefabが設定されていません");
            return;
        }
        
        // レア判定
        bool isRare = Random.value < rareChance;
        
        // スプライト選択
        Sprite selectedSprite = null;
        if (isRare && rareFishSprites != null && rareFishSprites.Length > 0)
        {
            selectedSprite = rareFishSprites[Random.Range(0, rareFishSprites.Length)];
        }
        else if (normalFishSprites != null && normalFishSprites.Length > 0)
        {
            selectedSprite = normalFishSprites[Random.Range(0, normalFishSprites.Length)];
            isRare = false; // レアスプライトがない場合は通常扱い
        }
        
        if (selectedSprite == null)
        {
            Debug.LogWarning("[FishSpawner] スプライトが設定されていません");
            return;
        }
        
        // ランダムなY座標
        Vector3 spawnPos = new Vector3(
            spawnXPosition,
            Random.Range(spawnYMin, spawnYMax),
            0
        );
        
        // ランダムな速度
        float speed = Random.Range(minSwimSpeed, maxSwimSpeed);
        
        // インスタンス化
        GameObject fishObj = Instantiate(fishPrefab, spawnPos, Quaternion.identity);
        fishObj.transform.SetParent(transform);
        
        // 初期化
        FishController controller = fishObj.GetComponent<FishController>();
        if (controller != null)
        {
            controller.Initialize(selectedSprite, spawnPos, speed, isRare, baseFishSize);
        }
        
        activeFish.Add(fishObj);
        
        Debug.Log($"[FishSpawner] 魚をスポーン: {(isRare ? "レア" : "通常")}");
    }
}
