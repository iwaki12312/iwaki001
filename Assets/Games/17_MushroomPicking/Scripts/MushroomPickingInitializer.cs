using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

/// <summary>
/// MushroomPickingゲームの自動初期化を行うクラス
/// </summary>
public class MushroomPickingInitializer : MonoBehaviour
{
    [Header("=== キノコスプライト - 通常キノコ ===")]
    [SerializeField] private Sprite redMushroomSprite;
    [SerializeField] private Sprite yellowMushroomSprite;
    [SerializeField] private Sprite blueMushroomSprite;
    [SerializeField] private Sprite whiteMushroomSprite;
    [SerializeField] private Sprite greenMushroomSprite;
    [SerializeField] private Sprite pinkMushroomSprite;
    [SerializeField] private Sprite orangeMushroomSprite;
    [SerializeField] private Sprite purpleMushroomSprite;
    [SerializeField] private Sprite brownMushroomSprite;
    [SerializeField] private Sprite skyblueMushroomSprite;

    [Header("=== キノコスプライト - レアキノコ ===")]
    [SerializeField] private Sprite goldMushroomSprite;
    [SerializeField] private Sprite rainbowMushroomSprite;
    [SerializeField] private Sprite starMushroomSprite;
    [SerializeField] private Sprite crystalMushroomSprite;
    [SerializeField] private Sprite cosmicMushroomSprite;

    [Header("=== キノコスプライト - スーパーレアキノコ ===")]
    [SerializeField] private Sprite rabbitMushroomSprite;
    [SerializeField] private Sprite mouseMushroomSprite;
    [SerializeField] private Sprite squirrelMushroomSprite;

    [Header("=== カゴスプライト ===")]
    [SerializeField] private Sprite basketSprite;

    [Header("=== 効果音 ===")]
    [SerializeField] private AudioClip growSound;
    [SerializeField] private AudioClip pickSound;
    [SerializeField] private AudioClip revealSound;
    [SerializeField] private AudioClip rarePickSound;
    [SerializeField] private AudioClip rareRevealSound;
    [SerializeField] private AudioClip basketSound;
    [SerializeField] private AudioClip hideSound;
    [SerializeField] private AudioClip rareAppearSound;
    [SerializeField] private AudioClip superRareRevealSound;

    [Header("=== パーティクル ===")]
    [SerializeField] private GameObject sparkleParticlePrefab;
    [SerializeField] private GameObject rareParticlePrefab;
    [SerializeField] private GameObject superRareParticlePrefab;

    [Header("=== スポーン設定 ===")]
    [SerializeField, Range(1, 6)] private int maxSimultaneous = 6;
    [SerializeField, Range(0.5f, 10f)] private float spawnInterval = 2f;
    [SerializeField, Range(0f, 1f)] private float rareSpawnChance = 0.1f;
    [SerializeField, Range(0f, 0.2f)] private float superRareSpawnChance = 0.03f;
    [SerializeField, Range(3f, 15f)] private float hideTimeout = 7f;

    [Header("=== キノコの大きさ ===")]
    [SerializeField, Range(0.1f, 5f)] private float mushroomBaseScale = 1f;
    [SerializeField, Range(0.3f, 5f)] private float colliderRadius = 1.2f;

    [Header("=== カゴ位置 ===")]
    [SerializeField] private Vector3 basketPosition = new Vector3(0f, -4f, 0f);
    [SerializeField, Range(0.5f, 3f)] private float basketScale = 1.5f;

    [Header("=== スポーンポイント（シーン上に配置。空ならデフォルト配置）===")]
    [SerializeField] private List<MushroomSpawnPoint> spawnPoints = new List<MushroomSpawnPoint>();

    void Awake()
    {
        InitializeGame();
    }

    private void InitializeGame()
    {
        Debug.Log("[MushroomPickingInitializer] ゲームの初期化を開始...");

        CreateEventSystemIfNeeded();
        CreateSFXPlayer();
        FindAndSetupBackground();
        CreateBasket();
        CreateMushroomSpawner();

        Debug.Log("[MushroomPickingInitializer] ゲームの初期化が完了しました！");
    }

    /// <summary>
    /// EventSystemを作成
    /// </summary>
    private void CreateEventSystemIfNeeded()
    {
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<InputSystemUIInputModule>();
            Debug.Log("[MushroomPickingInitializer] EventSystemを作成しました");
        }
    }

    /// <summary>
    /// SFXPlayerを作成
    /// </summary>
    private void CreateSFXPlayer()
    {
        if (MushroomPickingSFXPlayer.Instance != null) return;

        GameObject sfxObj = new GameObject("MushroomPickingSFXPlayer");
        MushroomPickingSFXPlayer sfxPlayer = sfxObj.AddComponent<MushroomPickingSFXPlayer>();
        sfxPlayer.SetSoundClips(growSound, pickSound, revealSound, rarePickSound, rareRevealSound, basketSound, hideSound, rareAppearSound, superRareRevealSound);

        Debug.Log("[MushroomPickingInitializer] MushroomPickingSFXPlayerを作成しました");
    }

    /// <summary>
    /// 既存の背景オブジェクトを検索して設定
    /// </summary>
    private void FindAndSetupBackground()
    {
        GameObject bgObj = GameObject.Find("Background");
        if (bgObj == null)
        {
            Debug.LogWarning("[MushroomPickingInitializer] Scene内にBackgroundオブジェクトが見つかりません");
        }
        else
        {
            Debug.Log("[MushroomPickingInitializer] 背景オブジェクトを検出しました");
        }
    }

    /// <summary>
    /// カゴオブジェクトを作成
    /// </summary>
    private void CreateBasket()
    {
        // 既にシーンにあるか確認
        GameObject existingBasket = GameObject.Find("Basket");
        if (existingBasket != null)
        {
            Debug.Log("[MushroomPickingInitializer] 既存のBasketを検出しました");
            return;
        }

        GameObject basketObj = new GameObject("Basket");
        basketObj.transform.position = basketPosition;
        basketObj.transform.localScale = Vector3.one * basketScale;

        SpriteRenderer sr = basketObj.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 5;
        if (basketSprite != null)
        {
            sr.sprite = basketSprite;
        }

        basketObj.AddComponent<BasketController>();

        Debug.Log("[MushroomPickingInitializer] Basketを作成しました");
    }

    /// <summary>
    /// MushroomSpawnerを作成
    /// </summary>
    private void CreateMushroomSpawner()
    {
        if (MushroomSpawner.Instance != null) return;

        GameObject spawnerObj = new GameObject("MushroomSpawner");
        MushroomSpawner spawner = spawnerObj.AddComponent<MushroomSpawner>();

        // キノコデータを作成
        List<MushroomPickingData> normalMushrooms = CreateNormalMushroomData();
        List<MushroomPickingData> rareMushrooms = CreateRareMushroomData();
        List<MushroomPickingData> superRareMushrooms = CreateSuperRareMushroomData();

        spawner.SetMushroomData(normalMushrooms, rareMushrooms, superRareMushrooms);

        // スポーン設定を反映
        spawner.SetSpawnConfig(maxSimultaneous, spawnInterval, rareSpawnChance,
                                mushroomBaseScale, colliderRadius, hideTimeout,
                                superRareSpawnChance);

        // スポーンポイントを設定
        if (spawnPoints == null || spawnPoints.Count == 0)
        {
            var foundPoints = new List<MushroomSpawnPoint>(
                FindObjectsByType<MushroomSpawnPoint>(FindObjectsSortMode.None));
            if (foundPoints.Count > 0)
            {
                spawnPoints = foundPoints;
            }
        }

        if (spawnPoints != null && spawnPoints.Count > 0)
        {
            // Play時にプレビューを非表示
            foreach (var sp in spawnPoints)
            {
                sp.HidePreview();
            }
            spawner.SetSpawnPoints(spawnPoints);
            Debug.Log($"[MushroomPickingInitializer] {spawnPoints.Count}個のスポーンポイントを検出");
        }

        // カゴを設定
        GameObject basketObj = GameObject.Find("Basket");
        if (basketObj != null)
        {
            spawner.SetBasket(basketObj.transform);
        }

        // キノコPrefabを作成
        GameObject mushroomPrefab = CreateMushroomPrefab();
        spawner.SetPrefabs(mushroomPrefab, sparkleParticlePrefab, rareParticlePrefab, superRareParticlePrefab);

        Debug.Log("[MushroomPickingInitializer] MushroomSpawnerを作成しました");
    }

    /// <summary>
    /// キノコPrefabをランタイムで作成
    /// </summary>
    private GameObject CreateMushroomPrefab()
    {
        GameObject prefab = new GameObject("MushroomPrefab");
        SpriteRenderer sr = prefab.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 10;

        CircleCollider2D col = prefab.AddComponent<CircleCollider2D>();
        col.radius = colliderRadius;

        prefab.AddComponent<MushroomController>();

        prefab.transform.localScale = Vector3.one * mushroomBaseScale;
        prefab.SetActive(false);

        return prefab;
    }

    /// <summary>
    /// 通常キノコデータを作成
    /// </summary>
    private List<MushroomPickingData> CreateNormalMushroomData()
    {
        List<MushroomPickingData> list = new List<MushroomPickingData>();

        list.Add(CreateMushroomDataInstance(MushroomType.RedMushroom, "赤キノコ",
            redMushroomSprite, pickSound, false));
        list.Add(CreateMushroomDataInstance(MushroomType.YellowMushroom, "黄キノコ",
            yellowMushroomSprite, pickSound, false));
        list.Add(CreateMushroomDataInstance(MushroomType.BlueMushroom, "青キノコ",
            blueMushroomSprite, pickSound, false));
        list.Add(CreateMushroomDataInstance(MushroomType.WhiteMushroom, "白キノコ",
            whiteMushroomSprite, pickSound, false));
        list.Add(CreateMushroomDataInstance(MushroomType.GreenMushroom, "緑キノコ",
            greenMushroomSprite, pickSound, false));
        list.Add(CreateMushroomDataInstance(MushroomType.PinkMushroom, "ピンクキノコ",
            pinkMushroomSprite, pickSound, false));
        list.Add(CreateMushroomDataInstance(MushroomType.OrangeMushroom, "オレンジキノコ",
            orangeMushroomSprite, pickSound, false));
        list.Add(CreateMushroomDataInstance(MushroomType.PurpleMushroom, "紫キノコ",
            purpleMushroomSprite, pickSound, false));
        list.Add(CreateMushroomDataInstance(MushroomType.BrownMushroom, "茶キノコ",
            brownMushroomSprite, pickSound, false));
        list.Add(CreateMushroomDataInstance(MushroomType.SkyBlueMushroom, "水色キノコ",
            skyblueMushroomSprite, pickSound, false));

        return list;
    }

    /// <summary>
    /// レアキノコデータを作成
    /// </summary>
    private List<MushroomPickingData> CreateRareMushroomData()
    {
        List<MushroomPickingData> list = new List<MushroomPickingData>();

        list.Add(CreateMushroomDataInstance(MushroomType.GoldMushroom, "金キノコ",
            goldMushroomSprite, rarePickSound, true));
        list.Add(CreateMushroomDataInstance(MushroomType.RainbowMushroom, "虹キノコ",
            rainbowMushroomSprite, rarePickSound, true));
        list.Add(CreateMushroomDataInstance(MushroomType.StarMushroom, "星キノコ",
            starMushroomSprite, rarePickSound, true));
        list.Add(CreateMushroomDataInstance(MushroomType.CrystalMushroom, "クリスタルキノコ",
            crystalMushroomSprite, rarePickSound, true));
        list.Add(CreateMushroomDataInstance(MushroomType.CosmicMushroom, "宇宙キノコ",
            cosmicMushroomSprite, rarePickSound, true));

        return list;
    }

    /// <summary>
    /// スーパーレアキノコデータを作成
    /// </summary>
    private List<MushroomPickingData> CreateSuperRareMushroomData()
    {
        List<MushroomPickingData> list = new List<MushroomPickingData>();

        list.Add(CreateMushroomDataInstance(MushroomType.RabbitMushroom, "うさぎキノコ",
            rabbitMushroomSprite, rarePickSound, false, true));
        list.Add(CreateMushroomDataInstance(MushroomType.MouseMushroom, "ネズミキノコ",
            mouseMushroomSprite, rarePickSound, false, true));
        list.Add(CreateMushroomDataInstance(MushroomType.SquirrelMushroom, "リスキノコ",
            squirrelMushroomSprite, rarePickSound, false, true));

        return list;
    }

    /// <summary>
    /// MushroomPickingDataインスタンスを作成
    /// </summary>
    private MushroomPickingData CreateMushroomDataInstance(MushroomType type, string name,
        Sprite sprite, AudioClip sound, bool isRare, bool isSuperRare = false)
    {
        MushroomPickingData data = ScriptableObject.CreateInstance<MushroomPickingData>();
        data.mushroomType = type;
        data.mushroomName = name;
        data.mushroomSprite = sprite;
        data.pickSound = sound;
        data.isRare = isRare;
        data.isSuperRare = isSuperRare;

        if (isSuperRare)
        {
            data.jumpHeight = 2.5f;
            data.spinSpeed = 1440f;
            data.revealDuration = 1f;
            data.flyToBasketDuration = 1.2f;
        }
        else if (isRare)
        {
            data.jumpHeight = 2f;
            data.spinSpeed = 1080f;
            data.revealDuration = 0.8f;
            data.flyToBasketDuration = 1f;
        }
        else
        {
            data.jumpHeight = 1.5f;
            data.spinSpeed = 720f;
            data.revealDuration = 0.6f;
            data.flyToBasketDuration = 0.8f;
        }

        return data;
    }
}
