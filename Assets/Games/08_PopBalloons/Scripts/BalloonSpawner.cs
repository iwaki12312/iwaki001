using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 風船のスポーンを管理するクラス
/// </summary>
public class BalloonSpawner : MonoBehaviour
{
    [Header("風船設定")]
    [SerializeField] private GameObject balloonPrefab;           // 通常風船Prefab
    [SerializeField] private GameObject giantBalloonPrefab;      // ジャイアント風船Prefab
    [SerializeField] private Sprite[] balloonSprites;            // 風船スプライト配列(8色)
    [SerializeField] private float[] balloonColliderRadii = new float[8] 
        { 2.0f, 2.0f, 2.0f, 2.0f, 2.0f, 2.0f, 2.0f, 2.0f };     // 各色のコライダサイズ
    [SerializeField] private GameObject normalParticlePrefab;    // 通常風船用パーティクルPrefab
    [SerializeField] private GameObject giantParticlePrefab;     // ジャイアント風船用パーティクルPrefab
    
    [Header("アニマルパラシュート設定")]
    [SerializeField] private GameObject animalParachutePrefab;   // アニマルパラシュートPrefab
    [SerializeField] private Sprite[] animalParachuteSprites;    // パラシュート付き動物スプライト(3種類)
    
    [Header("スポーン設定")]
    [SerializeField] private float spawnInterval = 2f;           // スポーン間隔(秒)
    [SerializeField] private int maxSimultaneousBalloons = 8;    // 同時最大数
    [SerializeField] private float spawnYPosition = -6f;         // スポーン位置Y座標
    [SerializeField] private float spawnXMin = -8f;              // スポーン位置X最小
    [SerializeField] private float spawnXMax = 8f;               // スポーン位置X最大
    
    [Header("レアイベント確率")]
    [SerializeField] [Range(0f, 1f)] private float giantChance = 0.08f;  // ジャイアント出現確率
    [SerializeField] [Range(0f, 1f)] private float animalChance = 0.10f; // アニマルパラシュート確率
    [SerializeField] [Range(0f, 1f)] private float stormChance = 0.05f;  // バルーンストーム確率
    
    private List<GameObject> activeBalloons = new List<GameObject>();
    private float lastSpawnTime;
    private float lastStormTime; // 最後のストーム発生時刻
    private const float stormCooldown = 30f; // ストームクールダウン(秒)
    
    public static BalloonSpawner Instance { get; private set; }
    
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
        lastStormTime = -stormCooldown; // 最初からストーム発生可能
    }
    
    void Update()
    {
        // 非アクティブな風船をリストから削除
        activeBalloons.RemoveAll(balloon => balloon == null);
        
        // スポーン処理
        if (Time.time - lastSpawnTime >= spawnInterval)
        {
            if (activeBalloons.Count < maxSimultaneousBalloons)
            {
                // ストーム判定(クールダウン確認)
                if (Time.time - lastStormTime >= stormCooldown && Random.value < stormChance)
                {
                    TriggerBalloonStorm();
                    lastStormTime = Time.time;
                }
                else
                {
                    SpawnBalloon();
                }
            }
            lastSpawnTime = Time.time;
        }
    }
    
    /// <summary>
    /// 通常の風船をスポーン
    /// </summary>
    private void SpawnBalloon()
    {
        // ランダムな横位置
        Vector3 spawnPos = new Vector3(
            Random.Range(spawnXMin, spawnXMax),
            spawnYPosition,
            0
        );
        
        // ジャイアント判定
        bool isGiant = Random.value < giantChance;
        GameObject prefab = isGiant ? giantBalloonPrefab : balloonPrefab;
        
        if (prefab == null)
        {
            Debug.LogWarning("[BalloonSpawner] Prefabが設定されていません");
            return;
        }
        
        // ランダムな色
        int colorIndex = Random.Range(0, balloonSprites.Length);
        Sprite sprite = balloonSprites[colorIndex];
        
        // インスタンス化
        GameObject balloonObj = Instantiate(prefab, spawnPos, Quaternion.identity);
        balloonObj.transform.SetParent(transform);
        
        // 使用するパーティクルを決定
        GameObject particleToUse = isGiant ? giantParticlePrefab : normalParticlePrefab;
        
        // 初期化
        BalloonController controller = balloonObj.GetComponent<BalloonController>();
        if (controller != null)
        {
            controller.Initialize(sprite, spawnPos, particleToUse);
            
            // 色に応じたコライダサイズを設定
            if (colorIndex < balloonColliderRadii.Length)
            {
                controller.SetColliderRadius(balloonColliderRadii[colorIndex]);
            }
            
            // アニマルパラシュート判定(ジャイアント以外)
            if (!isGiant && Random.value < animalChance)
            {
                // AnimalBalloonコンポーネントを追加
                AnimalBalloonController animalController = balloonObj.AddComponent<AnimalBalloonController>();
                animalController.SetParticlePrefab(particleToUse);
                
                // AnimalParachuteの設定を渡す
                animalController.SetAnimalParachuteConfig(animalParachutePrefab, animalParachuteSprites);
            }
        }
        
        activeBalloons.Add(balloonObj);
    }
    
    /// <summary>
    /// バルーンストームを発動
    /// </summary>
    private void TriggerBalloonStorm()
    {
        // 効果音を再生
        if (PopBalloonsSFXPlayer.Instance != null)
        {
            PopBalloonsSFXPlayer.Instance.PlayStormStart();
        }
        
        // 20〜30個の風船を一斉にスポーン
        int stormCount = Random.Range(20, 31);
        
        for (int i = 0; i < stormCount; i++)
        {
            Vector3 spawnPos = new Vector3(
                Random.Range(spawnXMin, spawnXMax),
                spawnYPosition + Random.Range(-1f, 1f), // 少しバラつかせる
                0
            );
            
            int colorIndex = Random.Range(0, balloonSprites.Length);
            Sprite sprite = balloonSprites[colorIndex];
            
            GameObject balloonObj = Instantiate(balloonPrefab, spawnPos, Quaternion.identity);
            balloonObj.transform.SetParent(transform);
            
            BalloonController controller = balloonObj.GetComponent<BalloonController>();
            if (controller != null)
            {
                controller.Initialize(sprite, spawnPos, normalParticlePrefab);
                
                // 色に応じたコライダサイズを設定
                if (colorIndex < balloonColliderRadii.Length)
                {
                    controller.SetColliderRadius(balloonColliderRadii[colorIndex]);
                }
                
                // アニマルパラシュート判定(ストーム中も発生)
                if (Random.value < animalChance)
                {
                    AnimalBalloonController animalController = balloonObj.AddComponent<AnimalBalloonController>();
                    animalController.SetParticlePrefab(normalParticlePrefab);
                    animalController.SetAnimalParachuteConfig(animalParachutePrefab, animalParachuteSprites);
                }
            }
            
            activeBalloons.Add(balloonObj);
        }
        
        Debug.Log($"[BalloonSpawner] バルーンストーム発動! {stormCount}個の風船をスポーン");
    }
}
