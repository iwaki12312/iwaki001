using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Linq;

/// <summary>
/// Fishingゲームを自動セットアップするエディタ拡張
/// Tools → Setup Fishing Game から実行
/// </summary>
public static class FishingSetup
{
    [MenuItem("Tools/Setup Fishing Game")]
    public static void SetupGame()
    {
        // シーンを開く
        string scenePath = "Assets/Games/09_Fishing/Scenes/Fishing.unity";
        
        // シーンが存在しない場合は作成
        if (!System.IO.File.Exists(scenePath))
        {
            UnityEngine.SceneManagement.Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            EditorSceneManager.SaveScene(newScene, scenePath);
        }
        else
        {
            EditorSceneManager.OpenScene(scenePath);
        }
        
        Debug.Log("[FishingSetup] セットアップ開始");
        
        // 既存のセットアップオブジェクトを削除
        CleanupExistingObjects();
        
        // カメラをセットアップ
        SetupCamera();
        
        // 背景をセットアップ（空+海）
        SetupBackground();
        
        // 釣り船をセットアップ
        SetupBoat();
        
        // 釣り人をセットアップ
        SetupFisherman();
        
        // GameManagerをセットアップ
        SetupGameManager();
        
        // SFXPlayerをセットアップ
        SetupSFXPlayer();
        
        // Prefabsを先に作成
        CreateFishPrefab();
        CreateSeagullPrefab();
        
        // FishSpawnerをセットアップ（Prefab作成後）
        SetupFishSpawner();
        
        // EventSystemを確認
        EnsureEventSystem();
        
        // シーンを保存
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        
        EditorUtility.DisplayDialog("完了", 
            "Fishingゲームのセットアップが完了しました!\n" +
            "シーンを再生して動作を確認してください。", 
            "OK");
        
        Debug.Log("[FishingSetup] セットアップ完了");
    }
    
    private static void CleanupExistingObjects()
    {
        string[] objectNames = { 
            "Background", "Boat", "Fisherman", "GameManager", 
            "FishingSFXPlayer", "FishSpawner"
        };
        
        foreach (string name in objectNames)
        {
            GameObject obj = GameObject.Find(name);
            if (obj != null)
            {
                Object.DestroyImmediate(obj);
            }
        }
    }
    
    private static void SetupCamera()
    {
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.orthographicSize = 5f;
            mainCam.backgroundColor = new Color(0.53f, 0.81f, 0.92f); // 空色
            Debug.Log("[FishingSetup] カメラ設定完了");
        }
    }
    
    private static void SetupBackground()
    {
        // 背景スプライトをロード
        Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Games/09_Fishing/Sprites/work_bg.png");
        if (bgSprite == null)
        {
            Debug.LogWarning("[FishingSetup] 背景スプライトが見つかりません");
            return;
        }
        
        GameObject bg = new GameObject("Background");
        SpriteRenderer sr = bg.AddComponent<SpriteRenderer>();
        sr.sprite = bgSprite;
        sr.sortingOrder = -10;
        
        // カメラに合わせてスケール調整
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            float screenHeight = mainCam.orthographicSize * 2;
            float spriteHeight = bgSprite.bounds.size.y;
            float scale = screenHeight / spriteHeight;
            bg.transform.localScale = Vector3.one * scale;
        }
        
        Debug.Log("[FishingSetup] 背景を配置しました");
    }
    
    private static void SetupBoat()
    {
        // 釣り船スプライトをロード
        Sprite boatSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Games/09_Fishing/Sprites/work_sprite_a.png");
        if (boatSprite == null)
        {
            Debug.LogWarning("[FishingSetup] 釣り船スプライトが見つかりません");
            return;
        }
        
        GameObject boat = new GameObject("Boat");
        SpriteRenderer sr = boat.AddComponent<SpriteRenderer>();
        sr.sprite = boatSprite;
        sr.sortingOrder = 2;
        
        // 画面右上に配置
        boat.transform.position = new Vector3(7f, 3f, 0);
        boat.transform.localScale = Vector3.one * 2f;
        
        Debug.Log("[FishingSetup] 釣り船を配置しました");
    }
    
    private static void SetupFisherman()
    {
        // 釣り人スプライトをロード（3枚）
        Sprite idleSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Games/09_Fishing/Sprites/work_sprite_b.png");
        Sprite hookedSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Games/09_Fishing/Sprites/work_sprite_b.png");
        Sprite pullSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Games/09_Fishing/Sprites/work_sprite_b.png");
        
        GameObject fisherman = new GameObject("Fisherman");
        SpriteRenderer sr = fisherman.AddComponent<SpriteRenderer>();
        sr.sprite = idleSprite;
        sr.sortingOrder = 3;
        
        // 船の先端に配置
        fisherman.transform.position = new Vector3(8.5f, 3.5f, 0);
        fisherman.transform.localScale = Vector3.one * 1.5f;
        
        // FishermanControllerを追加
        FishermanController controller = fisherman.AddComponent<FishermanController>();
        
        SerializedObject so = new SerializedObject(controller);
        so.FindProperty("idleSprite").objectReferenceValue = idleSprite;
        so.FindProperty("hookedSprite").objectReferenceValue = hookedSprite;
        so.FindProperty("pullSprite").objectReferenceValue = pullSprite;
        so.ApplyModifiedProperties();
        
        Debug.Log("[FishingSetup] 釣り人を配置しました");
    }
    
    private static void SetupGameManager()
    {
        GameObject gameManager = new GameObject("GameManager");
        gameManager.AddComponent<FishingGameManager>();
        Debug.Log("[FishingSetup] GameManagerを作成しました");
    }
    
    private static void SetupSFXPlayer()
    {
        GameObject sfxPlayer = new GameObject("FishingSFXPlayer");
        FishingSFXPlayer player = sfxPlayer.AddComponent<FishingSFXPlayer>();
        
        // 効果音をロード
        AudioClip sfx1 = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Games/09_Fishing/Audios/work_sfx1.mp3");
        AudioClip sfx2 = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Games/09_Fishing/Audios/work_sfx2.mp3");
        AudioClip sfx3 = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Games/09_Fishing/Audios/work_sfx3.mp3");
        AudioClip sfx4 = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Games/09_Fishing/Audios/work_sfx4.mp3");
        
        SerializedObject so = new SerializedObject(player);
        so.FindProperty("fishCatchSound").objectReferenceValue = sfx1;
        so.FindProperty("fishRareSound").objectReferenceValue = sfx2;
        so.FindProperty("seagullAppearSound").objectReferenceValue = sfx3;
        so.FindProperty("seagullCrySound").objectReferenceValue = sfx4;
        so.ApplyModifiedProperties();
        
        Debug.Log("[FishingSetup] SFXPlayerを作成しました");
    }
    
    private static void SetupFishSpawner()
    {
        GameObject spawner = new GameObject("FishSpawner");
        FishSpawner spawnerScript = spawner.AddComponent<FishSpawner>();
        
        // 魚スプライトをロード（仮で同じスプライトを20個、8個）
        Sprite normalFish = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Games/09_Fishing/Sprites/work_sprite_a.png");
        Sprite rareFish = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Games/09_Fishing/Sprites/work_sprite_b.png");
        
        // 通常魚スプライト配列（20個）
        Sprite[] normalSprites = new Sprite[20];
        for (int i = 0; i < 20; i++)
        {
            normalSprites[i] = normalFish;
        }
        
        // レア魚スプライト配列（8個）
        Sprite[] rareSprites = new Sprite[8];
        for (int i = 0; i < 8; i++)
        {
            rareSprites[i] = rareFish;
        }
        
        // Prefabを読み込み
        GameObject fishPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Games/09_Fishing/Prefabs/Fish.prefab");
        GameObject seagullPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Games/09_Fishing/Prefabs/Seagull.prefab");
        
        SerializedObject so = new SerializedObject(spawnerScript);
        so.FindProperty("fishPrefab").objectReferenceValue = fishPrefab;
        so.FindProperty("seagullPrefab").objectReferenceValue = seagullPrefab;
        
        SerializedProperty normalProp = so.FindProperty("normalFishSprites");
        normalProp.arraySize = normalSprites.Length;
        for (int i = 0; i < normalSprites.Length; i++)
        {
            normalProp.GetArrayElementAtIndex(i).objectReferenceValue = normalSprites[i];
        }
        
        SerializedProperty rareProp = so.FindProperty("rareFishSprites");
        rareProp.arraySize = rareSprites.Length;
        for (int i = 0; i < rareSprites.Length; i++)
        {
            rareProp.GetArrayElementAtIndex(i).objectReferenceValue = rareSprites[i];
        }
        
        so.ApplyModifiedProperties();
        
        Debug.Log("[FishingSetup] FishSpawnerを作成しました");
    }
    
    private static void CreateFishPrefab()
    {
        string prefabPath = "Assets/Games/09_Fishing/Prefabs/Fish.prefab";
        
        // Prefabsフォルダが存在しない場合は作成
        string prefabFolder = "Assets/Games/09_Fishing/Prefabs";
        if (!AssetDatabase.IsValidFolder(prefabFolder))
        {
            AssetDatabase.CreateFolder("Assets/Games/09_Fishing", "Prefabs");
        }
        
        // 既存のプレハブがあれば削除
        if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
        {
            AssetDatabase.DeleteAsset(prefabPath);
        }
        
        GameObject fish = new GameObject("Fish");
        fish.AddComponent<SpriteRenderer>();
        fish.AddComponent<CircleCollider2D>();
        FishController controller = fish.AddComponent<FishController>();
        
        // デフォルト値を設定
        SerializedObject so = new SerializedObject(controller);
        so.FindProperty("catchAngle").floatValue = 45f;
        so.FindProperty("catchDistance").floatValue = 8.0f;
        so.FindProperty("catchDuration").floatValue = 1.5f;
        so.FindProperty("fadeOutDuration").floatValue = 0.5f;
        so.ApplyModifiedProperties();
        
        PrefabUtility.SaveAsPrefabAsset(fish, prefabPath);
        Object.DestroyImmediate(fish);
        
        Debug.Log("[FishingSetup] Fish Prefabを作成しました");
    }
    
    private static void CreateSeagullPrefab()
    {
        string prefabPath = "Assets/Games/09_Fishing/Prefabs/Seagull.prefab";
        
        // 既存のプレハブがあれば削除
        if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
        {
            AssetDatabase.DeleteAsset(prefabPath);
        }
        
        GameObject seagull = new GameObject("Seagull");
        SpriteRenderer sr = seagull.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 10;
        
        SeagullController controller = seagull.AddComponent<SeagullController>();
        
        // カモメスプライトをロード（実際のスプライトに差し替えてください）
        Sprite seagullSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Games/09_Fishing/Sprites/seagull.png");
        if (seagullSprite == null)
        {
            // フォールバック: 仮スプライトを使用
            seagullSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Games/09_Fishing/Sprites/work_sprite_a.png");
        }
        sr.sprite = seagullSprite;
        
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(seagull, prefabPath);
        Object.DestroyImmediate(seagull);
        
        Debug.Log("[FishingSetup] Seagull Prefabを作成しました");
    }
    
    private static void EnsureEventSystem()
    {
        if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            Debug.Log("[FishingSetup] EventSystemを作成しました");
        }
        else
        {
            Debug.Log("[FishingSetup] EventSystemは既に存在します");
        }
    }
}
