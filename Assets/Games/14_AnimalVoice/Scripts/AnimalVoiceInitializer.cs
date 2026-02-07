using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

/// <summary>
/// AnimalVoiceゲームの自動初期化を行うクラス
/// </summary>
public class AnimalVoiceInitializer : MonoBehaviour
{
    [Header("=== 背景スプライト（必須）===")]
    [SerializeField] private Sprite morningBackground;
    [SerializeField] private Sprite daytimeBackground;
    [SerializeField] private Sprite nightBackground;
    
    [Header("=== 動物スプライト - 朝の動物 ===")]
    [SerializeField] private Sprite chickenNormal;
    [SerializeField] private Sprite chickenReaction;
    [SerializeField] private Sprite cowNormal;
    [SerializeField] private Sprite cowReaction;
    [SerializeField] private Sprite horseNormal;
    [SerializeField] private Sprite horseReaction;
    [SerializeField] private Sprite pigNormal;
    [SerializeField] private Sprite pigReaction;
    [SerializeField] private Sprite sheepNormal;
    [SerializeField] private Sprite sheepReaction;
    [SerializeField] private Sprite goatNormal;
    [SerializeField] private Sprite goatReaction;
    
    [Header("=== 動物スプライト - 昼の動物 ===")]
    [SerializeField] private Sprite dogNormal;
    [SerializeField] private Sprite dogReaction;
    [SerializeField] private Sprite catNormal;
    [SerializeField] private Sprite catReaction;
    [SerializeField] private Sprite elephantNormal;
    [SerializeField] private Sprite elephantReaction;
    [SerializeField] private Sprite lionNormal;
    [SerializeField] private Sprite lionReaction;
    [SerializeField] private Sprite frogNormal;
    [SerializeField] private Sprite frogReaction;
    [SerializeField] private Sprite chickNormal;
    [SerializeField] private Sprite chickReaction;
    
    [Header("=== 動物スプライト - 夜の動物 ===")]
    [SerializeField] private Sprite owlNormal;
    [SerializeField] private Sprite owlReaction;
    [SerializeField] private Sprite wolfNormal;
    [SerializeField] private Sprite wolfReaction;
    [SerializeField] private Sprite batNormal;
    [SerializeField] private Sprite batReaction;
    [SerializeField] private Sprite hedgehogNormal;
    [SerializeField] private Sprite hedgehogReaction;
    
    [Header("=== 動物スプライト - レア動物 ===")]
    [SerializeField] private Sprite dinosaurNormal;
    [SerializeField] private Sprite dinosaurReaction;
    [SerializeField] private Sprite dragonNormal;
    [SerializeField] private Sprite dragonReaction;
    [SerializeField] private Sprite unicornNormal;
    [SerializeField] private Sprite unicornReaction;
    [SerializeField] private Sprite pandaNormal;
    [SerializeField] private Sprite pandaReaction;
    
    [Header("=== 鳴き声 - 朝の動物 ===")]
    [SerializeField] private AudioClip chickenVoice;
    [SerializeField] private AudioClip cowVoice;
    [SerializeField] private AudioClip horseVoice;
    [SerializeField] private AudioClip pigVoice;
    [SerializeField] private AudioClip sheepVoice;
    [SerializeField] private AudioClip goatVoice;
    
    [Header("=== 鳴き声 - 昼の動物 ===")]
    [SerializeField] private AudioClip dogVoice;
    [SerializeField] private AudioClip catVoice;
    [SerializeField] private AudioClip elephantVoice;
    [SerializeField] private AudioClip lionVoice;
    [SerializeField] private AudioClip frogVoice;
    [SerializeField] private AudioClip chickVoice;
    
    [Header("=== 鳴き声 - 夜の動物 ===")]
    [SerializeField] private AudioClip owlVoice;
    [SerializeField] private AudioClip wolfVoice;
    [SerializeField] private AudioClip batVoice;
    [SerializeField] private AudioClip hedgehogVoice;
    
    [Header("=== 鳴き声 - レア動物 ===")]
    [SerializeField] private AudioClip dinosaurVoice;
    [SerializeField] private AudioClip dragonVoice;
    [SerializeField] private AudioClip unicornVoice;
    [SerializeField] private AudioClip pandaVoice;
    
    [Header("=== 共通効果音 ===")]
    [SerializeField] private AudioClip tapSound;
    [SerializeField] private AudioClip timeChangeSound;
    [SerializeField] private AudioClip rareAppearSound;
    
    [Header("=== パーティクル ===")]
    [SerializeField] private GameObject heartParticlePrefab;
    [SerializeField] private GameObject noteParticlePrefab;
    
    [Header("=== スポーン設定（Inspectorで調整可能）===")]
    [SerializeField, Range(1, 12)] private int spawnCount = 6;
    [SerializeField, Range(0f, 1f)] private float rareSpawnChance = 0.1f;
    
    [Header("=== スポーン範囲 ===")]
    [SerializeField] private float spawnMinX = -4f;
    [SerializeField] private float spawnMaxX = 4f;
    [SerializeField] private float spawnMinY = -3f;
    [SerializeField] private float spawnMaxY = 2f;
    [SerializeField, Range(0.5f, 5f)] private float minDistanceBetweenAnimals = 1.5f;
    
    [Header("=== 動物の大きさ ===")]
    [SerializeField, Range(0.1f, 5f)] private float animalBaseScale = 1f;
    [SerializeField, Range(0.1f, 5f)] private float rareAnimalScale = 1.3f;
    [SerializeField, Range(0.5f, 3f)] private float colliderRadius = 1.0f;
    
    [Header("=== 時間帯設定 ===")]
    [SerializeField, Range(5f, 120f)] private float timeChangeInterval = 30f;
    
    [Header("=== スポーンポイント（シーン上に配置。空ならランダム配置）===")]
    [SerializeField] private List<AnimalSpawnPoint> spawnPoints = new List<AnimalSpawnPoint>();
    
    void Awake()
    {
        InitializeGame();
    }
    
    /// <summary>
    /// ゲームの初期化
    /// </summary>
    private void InitializeGame()
    {
        Debug.Log("[AnimalVoiceInitializer] ゲームの初期化を開始...");
        
        // EventSystemの確認・作成
        CreateEventSystemIfNeeded();
        
        // SFXPlayerの作成
        CreateSFXPlayer();
        
        // 背景の作成
        CreateBackground();
        
        // AnimalSpawnerの作成
        CreateAnimalSpawner();
        
        // BackgroundTimeManagerの作成
        CreateBackgroundTimeManager();

        // デバッグキャプチャーを追加
        CreateDebugCapture();

        Debug.Log("[AnimalVoiceInitializer] ゲームの初期化が完了しました！");
    }

    /// <summary>
    /// デバッグキャプチャーコンポーネントを追加
    /// </summary>
    private void CreateDebugCapture()
    {
        GameObject debugObj = new GameObject("DebugCapture");
        debugObj.AddComponent<AnimalVoiceDebugCapture>();
        Debug.Log("[AnimalVoiceInitializer] AnimalVoiceDebugCaptureを作成しました");
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
            Debug.Log("[AnimalVoiceInitializer] EventSystemを作成しました");
        }
    }
    
    /// <summary>
    /// SFXPlayerを作成
    /// </summary>
    private void CreateSFXPlayer()
    {
        if (AnimalVoiceSFXPlayer.Instance != null) return;
        
        GameObject sfxObj = new GameObject("AnimalVoiceSFXPlayer");
        AnimalVoiceSFXPlayer sfxPlayer = sfxObj.AddComponent<AnimalVoiceSFXPlayer>();
        sfxPlayer.SetSoundClips(tapSound, timeChangeSound, rareAppearSound);
        
        Debug.Log("[AnimalVoiceInitializer] AnimalVoiceSFXPlayerを作成しました");
    }
    
    /// <summary>
    /// 背景を作成
    /// </summary>
    private void CreateBackground()
    {
        // メイン背景
        GameObject bgObj = new GameObject("Background");
        SpriteRenderer bgRenderer = bgObj.AddComponent<SpriteRenderer>();
        bgRenderer.sortingOrder = -100;
        
        // フェード用オーバーレイ
        GameObject fadeObj = new GameObject("FadeOverlay");
        fadeObj.transform.SetParent(bgObj.transform);
        SpriteRenderer fadeRenderer = fadeObj.AddComponent<SpriteRenderer>();
        fadeRenderer.sortingOrder = 100;
        fadeRenderer.color = new Color(0, 0, 0, 0);
        
        // 1x1の黒いスプライトを作成
        Texture2D blackTex = new Texture2D(1, 1);
        blackTex.SetPixel(0, 0, Color.black);
        blackTex.Apply();
        Sprite blackSprite = Sprite.Create(blackTex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);
        fadeRenderer.sprite = blackSprite;
        fadeObj.transform.localScale = new Vector3(20, 20, 1);
        
        // BackgroundTimeManagerに渡す
        if (BackgroundTimeManager.Instance != null)
        {
            BackgroundTimeManager.Instance.SetRenderers(bgRenderer, fadeRenderer);
            BackgroundTimeManager.Instance.SetBackgrounds(morningBackground, daytimeBackground, nightBackground);
        }
        
        Debug.Log("[AnimalVoiceInitializer] 背景を作成しました");
    }
    
    /// <summary>
    /// AnimalSpawnerを作成
    /// </summary>
    private void CreateAnimalSpawner()
    {
        if (AnimalSpawner.Instance != null) return;
        
        GameObject spawnerObj = new GameObject("AnimalSpawner");
        AnimalSpawner spawner = spawnerObj.AddComponent<AnimalSpawner>();
        
        // 動物データを作成
        List<AnimalVoiceData> morningAnimals = CreateMorningAnimalData();
        List<AnimalVoiceData> daytimeAnimals = CreateDaytimeAnimalData();
        List<AnimalVoiceData> nightAnimals = CreateNightAnimalData();
        List<AnimalVoiceData> rareAnimals = CreateRareAnimalData();
        
        spawner.SetAnimalData(morningAnimals, daytimeAnimals, nightAnimals, rareAnimals);
        
        // スポーン設定を反映
        spawner.SetSpawnConfig(spawnCount, rareSpawnChance, spawnMinX, spawnMaxX, spawnMinY, spawnMaxY, minDistanceBetweenAnimals, animalBaseScale, rareAnimalScale, colliderRadius);
        
        // スポーンポイントを設定（空でない場合はシーン上のポイントを利用）
        if (spawnPoints == null || spawnPoints.Count == 0)
        {
            // 自動検索
            var foundPoints = new List<AnimalSpawnPoint>(FindObjectsByType<AnimalSpawnPoint>(FindObjectsSortMode.None));
            if (foundPoints.Count > 0)
            {
                spawnPoints = foundPoints;
            }
        }
        if (spawnPoints != null && spawnPoints.Count > 0)
        {
            spawner.SetSpawnPoints(spawnPoints);
            Debug.Log($"[AnimalVoiceInitializer] {spawnPoints.Count}個のスポーンポイントを検出");
        }
        
        // 動物Prefabを作成
        GameObject animalPrefab = CreateAnimalPrefab();
        spawner.SetPrefabs(animalPrefab, heartParticlePrefab, noteParticlePrefab);
        
        Debug.Log("[AnimalVoiceInitializer] AnimalSpawnerを作成しました");
    }
    
    /// <summary>
    /// 動物Prefabを作成
    /// </summary>
    private GameObject CreateAnimalPrefab()
    {
        GameObject prefab = new GameObject("AnimalPrefab");
        SpriteRenderer sr = prefab.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 10;  // 背景(-100)より前、フェードオーバーレイ(100)より後ろ
        
        CircleCollider2D col = prefab.AddComponent<CircleCollider2D>();
        col.radius = colliderRadius;
        
        AnimalController controller = prefab.AddComponent<AnimalController>();
        controller.SetColliderRadius(colliderRadius);
        
        prefab.transform.localScale = Vector3.one * animalBaseScale;
        prefab.SetActive(false);

        return prefab;
    }
    
    /// <summary>
    /// 朝の動物データを作成
    /// </summary>
    private List<AnimalVoiceData> CreateMorningAnimalData()
    {
        List<AnimalVoiceData> list = new List<AnimalVoiceData>();
        
        list.Add(CreateAnimalDataInstance(AnimalVoiceAnimalType.Chicken, "ニワトリ", chickenNormal, chickenReaction, chickenVoice, false));
        list.Add(CreateAnimalDataInstance(AnimalVoiceAnimalType.Cow, "牛", cowNormal, cowReaction, cowVoice, false));
        list.Add(CreateAnimalDataInstance(AnimalVoiceAnimalType.Horse, "馬", horseNormal, horseReaction, horseVoice, false));
        list.Add(CreateAnimalDataInstance(AnimalVoiceAnimalType.Pig, "豚", pigNormal, pigReaction, pigVoice, false));
        list.Add(CreateAnimalDataInstance(AnimalVoiceAnimalType.Sheep, "羊", sheepNormal, sheepReaction, sheepVoice, false));
        list.Add(CreateAnimalDataInstance(AnimalVoiceAnimalType.Goat, "ヤギ", goatNormal, goatReaction, goatVoice, false));
        
        return list;
    }
    
    /// <summary>
    /// 昼の動物データを作成
    /// </summary>
    private List<AnimalVoiceData> CreateDaytimeAnimalData()
    {
        List<AnimalVoiceData> list = new List<AnimalVoiceData>();
        
        list.Add(CreateAnimalDataInstance(AnimalVoiceAnimalType.Dog, "犬", dogNormal, dogReaction, dogVoice, false));
        list.Add(CreateAnimalDataInstance(AnimalVoiceAnimalType.Cat, "猫", catNormal, catReaction, catVoice, false));
        list.Add(CreateAnimalDataInstance(AnimalVoiceAnimalType.Elephant, "ゾウ", elephantNormal, elephantReaction, elephantVoice, false));
        list.Add(CreateAnimalDataInstance(AnimalVoiceAnimalType.Lion, "ライオン", lionNormal, lionReaction, lionVoice, false));
        list.Add(CreateAnimalDataInstance(AnimalVoiceAnimalType.Frog, "カエル", frogNormal, frogReaction, frogVoice, false));
        list.Add(CreateAnimalDataInstance(AnimalVoiceAnimalType.Chick, "ひよこ", chickNormal, chickReaction, chickVoice, false));
        
        return list;
    }
    
    /// <summary>
    /// 夜の動物データを作成
    /// </summary>
    private List<AnimalVoiceData> CreateNightAnimalData()
    {
        List<AnimalVoiceData> list = new List<AnimalVoiceData>();
        
        list.Add(CreateAnimalDataInstance(AnimalVoiceAnimalType.Owl, "フクロウ", owlNormal, owlReaction, owlVoice, false));
        list.Add(CreateAnimalDataInstance(AnimalVoiceAnimalType.Wolf, "オオカミ", wolfNormal, wolfReaction, wolfVoice, false));
        list.Add(CreateAnimalDataInstance(AnimalVoiceAnimalType.Bat, "コウモリ", batNormal, batReaction, batVoice, false));
        list.Add(CreateAnimalDataInstance(AnimalVoiceAnimalType.Hedgehog, "ハリネズミ", hedgehogNormal, hedgehogReaction, hedgehogVoice, false));
        
        return list;
    }
    
    /// <summary>
    /// レア動物データを作成
    /// </summary>
    private List<AnimalVoiceData> CreateRareAnimalData()
    {
        List<AnimalVoiceData> list = new List<AnimalVoiceData>();
        
        list.Add(CreateAnimalDataInstance(AnimalVoiceAnimalType.Dinosaur, "恐竜", dinosaurNormal, dinosaurReaction, dinosaurVoice, true));
        list.Add(CreateAnimalDataInstance(AnimalVoiceAnimalType.Dragon, "ドラゴン", dragonNormal, dragonReaction, dragonVoice, true));
        list.Add(CreateAnimalDataInstance(AnimalVoiceAnimalType.Unicorn, "ユニコーン", unicornNormal, unicornReaction, unicornVoice, true));
        list.Add(CreateAnimalDataInstance(AnimalVoiceAnimalType.Panda, "パンダ", pandaNormal, pandaReaction, pandaVoice, true));
        
        return list;
    }
    
    /// <summary>
    /// AnimalVoiceDataインスタンスを作成
    /// </summary>
    private AnimalVoiceData CreateAnimalDataInstance(AnimalVoiceAnimalType type, string name, Sprite normal, Sprite reaction, AudioClip voice, bool isRare)
    {
        AnimalVoiceData data = ScriptableObject.CreateInstance<AnimalVoiceData>();
        data.animalType = type;
        data.animalName = name;
        data.normalSprite = normal;
        data.reactionSprite = reaction;
        data.voiceClip = voice;
        data.isRare = isRare;
        data.reactionDuration = isRare ? 1.2f : 0.8f;
        data.scaleMultiplier = isRare ? 1.3f : 1.2f;
        
        return data;
    }
    
    /// <summary>
    /// BackgroundTimeManagerを作成
    /// </summary>
    private void CreateBackgroundTimeManager()
    {
        if (BackgroundTimeManager.Instance != null) return;
        
        GameObject managerObj = new GameObject("BackgroundTimeManager");
        BackgroundTimeManager manager = managerObj.AddComponent<BackgroundTimeManager>();
        manager.SetChangeInterval(timeChangeInterval);
        
        // 背景のRendererを検索して設定
        GameObject bgObj = GameObject.Find("Background");
        if (bgObj != null)
        {
            SpriteRenderer bgRenderer = bgObj.GetComponent<SpriteRenderer>();
            Transform fadeTransform = bgObj.transform.Find("FadeOverlay");
            SpriteRenderer fadeRenderer = fadeTransform?.GetComponent<SpriteRenderer>();
            
            manager.SetRenderers(bgRenderer, fadeRenderer);
            manager.SetBackgrounds(morningBackground, daytimeBackground, nightBackground);
        }
        
        Debug.Log("[AnimalVoiceInitializer] BackgroundTimeManagerを作成しました");
    }
}
