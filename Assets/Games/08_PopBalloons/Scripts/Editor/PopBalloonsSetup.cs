using UnityEngine;
using UnityEditor;

/// <summary>
/// PopBalloonsゲームの自動セットアップを行うエディタ拡張
/// </summary>
public class PopBalloonsSetup
{
    [MenuItem("Tools/Setup PopBalloons Game")]
    public static void SetupGame()
    {
        Debug.Log("[PopBalloonsSetup] ゲームのセットアップを開始します");
        
        // カメラを作成
        SetupMainCamera();
        
        // GameManagerを作成
        SetupGameManager();
        
        // SFXPlayerを作成
        SetupSFXPlayer();
        
        // Spawnerを作成
        SetupBalloonSpawner();
        
        // 背景を作成
        SetupBackground();
        
        // Prefabsを作成
        CreateBalloonPrefab();
        CreateGiantBalloonPrefab();
        CreateAnimalParachutePrefab();
        CreateStarParticlePrefab();
        
        Debug.Log("[PopBalloonsSetup] セットアップ完了!");
    }
    
    private static void SetupMainCamera()
    {
        // 既存のメインカメラを検索
        Camera mainCam = Camera.main;
        
        if (mainCam == null)
        {
            // カメラが無い場合は新規作成
            GameObject camObj = new GameObject("Main Camera");
            mainCam = camObj.AddComponent<Camera>();
            camObj.tag = "MainCamera";
            
            Debug.Log("[PopBalloonsSetup] メインカメラを作成しました");
        }
        else
        {
            Debug.Log("[PopBalloonsSetup] 既存のメインカメラを使用します");
        }
        
        // カメラ設定
        mainCam.transform.position = new Vector3(0, 0, -10);
        mainCam.orthographic = true;
        mainCam.orthographicSize = 5f;
        mainCam.backgroundColor = new Color(0.53f, 0.81f, 0.92f); // 空色 #87CEEB
        mainCam.clearFlags = CameraClearFlags.SolidColor;
        
        Debug.Log("[PopBalloonsSetup] カメラ設定を完了しました");
    }
    
    private static void SetupGameManager()
    {
        GameObject gameManager = new GameObject("GameManager");
        gameManager.AddComponent<PopBalloonsGameManager>();
        Debug.Log("[PopBalloonsSetup] GameManagerを作成しました");
    }
    
    private static void SetupSFXPlayer()
    {
        GameObject sfxPlayer = new GameObject("PopBalloonsSFXPlayer");
        PopBalloonsSFXPlayer player = sfxPlayer.AddComponent<PopBalloonsSFXPlayer>();
        
        // 効果音をロード
        AudioClip sfx1 = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Games/08_PopBalloons/Audios/work_sfx1.mp3");
        AudioClip sfx2 = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Games/08_PopBalloons/Audios/work_sfx2.mp3");
        AudioClip sfx3 = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Games/08_PopBalloons/Audios/work_sfx3.mp3");
        AudioClip sfx4 = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Games/08_PopBalloons/Audios/work_sfx4.mp3");
        AudioClip sfx5 = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Games/08_PopBalloons/Audios/work_sfx5.mp3");
        AudioClip sfx6 = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Games/08_PopBalloons/Audios/work_sfx6.mp3");
        AudioClip sfx7 = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Games/08_PopBalloons/Audios/work_sfx7.mp3");
        AudioClip sfx8 = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Games/08_PopBalloons/Audios/work_sfx8.mp3");
        
        SerializedObject so = new SerializedObject(player);
        so.FindProperty("popNormalSound").objectReferenceValue = sfx1;
        so.FindProperty("popGiantSound").objectReferenceValue = sfx2;
        so.FindProperty("stormStartSound").objectReferenceValue = sfx3;
        so.FindProperty("animalAppearSound").objectReferenceValue = sfx4;
        so.FindProperty("rabbitVoiceSound").objectReferenceValue = sfx5;
        so.FindProperty("bearVoiceSound").objectReferenceValue = sfx6;
        so.FindProperty("catVoiceSound").objectReferenceValue = sfx7;
        so.ApplyModifiedProperties();
        
        Debug.Log("[PopBalloonsSetup] SFXPlayerを作成しました");
    }
    
    private static void SetupBalloonSpawner()
    {
        GameObject spawner = new GameObject("BalloonSpawner");
        BalloonSpawner spawnerScript = spawner.AddComponent<BalloonSpawner>();
        
        // スプライトシートから風船スプライトを取得
        Sprite[] balloonSprites = LoadSpritesFromSheet("Assets/Games/08_PopBalloons/Sprites/work_sprite_a.png");
        
        // 8色の風船スプライトを設定
        Sprite[] selectedSprites = new Sprite[8];
        for (int i = 0; i < 8 && i < balloonSprites.Length; i++)
        {
            selectedSprites[i] = balloonSprites[i];
        }
        
        // Prefabを読み込み
        GameObject balloonPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Games/08_PopBalloons/Prefabs/Balloon.prefab");
        GameObject giantPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Games/08_PopBalloons/Prefabs/GiantBalloon.prefab");
        GameObject starPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Games/08_PopBalloons/Prefabs/StarParticle.prefab");
        GameObject animalParachutePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Games/08_PopBalloons/Prefabs/AnimalParachute.prefab");
        
        // アニマルパラシュートスプライトを取得
        Sprite[] spritesB = LoadSpritesFromSheet("Assets/Games/08_PopBalloons/Sprites/work_sprite_b.png");
        Sprite[] animalParachuteSprites = new Sprite[3];
        for (int i = 0; i < 3 && i + 1 < spritesB.Length; i++)
        {
            animalParachuteSprites[i] = spritesB[i + 1]; // red_1~3
        }
        
        SerializedObject so = new SerializedObject(spawnerScript);
        so.FindProperty("balloonPrefab").objectReferenceValue = balloonPrefab;
        so.FindProperty("giantBalloonPrefab").objectReferenceValue = giantPrefab;
        so.FindProperty("normalParticlePrefab").objectReferenceValue = starPrefab;
        so.FindProperty("giantParticlePrefab").objectReferenceValue = starPrefab; // 初期は同じ
        so.FindProperty("animalParachutePrefab").objectReferenceValue = animalParachutePrefab;
        
        SerializedProperty spritesProperty = so.FindProperty("balloonSprites");
        spritesProperty.arraySize = selectedSprites.Length;
        for (int i = 0; i < selectedSprites.Length; i++)
        {
            spritesProperty.GetArrayElementAtIndex(i).objectReferenceValue = selectedSprites[i];
        }
        
        SerializedProperty animalSpritesProperty = so.FindProperty("animalParachuteSprites");
        animalSpritesProperty.arraySize = animalParachuteSprites.Length;
        for (int i = 0; i < animalParachuteSprites.Length; i++)
        {
            animalSpritesProperty.GetArrayElementAtIndex(i).objectReferenceValue = animalParachuteSprites[i];
        }
        
        so.ApplyModifiedProperties();
        
        Debug.Log("[PopBalloonsSetup] BalloonSpawnerを作成しました");
    }
    
    private static void SetupBackground()
    {
        GameObject bg = new GameObject("Background");
        SpriteRenderer renderer = bg.AddComponent<SpriteRenderer>();
        
        Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Games/08_PopBalloons/Sprites/work_bg.png");
        renderer.sprite = bgSprite;
        renderer.sortingOrder = -1;
        
        // カメラサイズに合わせてスケール調整
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            float height = 2f * mainCam.orthographicSize;
            float width = height * mainCam.aspect;
            
            if (bgSprite != null)
            {
                bg.transform.localScale = new Vector3(
                    width / bgSprite.bounds.size.x,
                    height / bgSprite.bounds.size.y,
                    1
                );
            }
        }
        
        Debug.Log("[PopBalloonsSetup] 背景を作成しました");
    }
    
    private static void CreateBalloonPrefab()
    {
        string prefabPath = "Assets/Games/08_PopBalloons/Prefabs/Balloon.prefab";
        
        if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
        {
            AssetDatabase.DeleteAsset(prefabPath);
        }
        
        GameObject balloon = new GameObject("Balloon");
        balloon.AddComponent<SpriteRenderer>();
        balloon.AddComponent<CircleCollider2D>();
        balloon.AddComponent<Rigidbody2D>();
        BalloonController controller = balloon.AddComponent<BalloonController>();
        
        // デフォルトのサイズを設定
        SerializedObject so = new SerializedObject(controller);
        so.FindProperty("balloonSize").floatValue = 3.0f;
        so.FindProperty("colliderRadius").floatValue = 2.0f;
        so.ApplyModifiedProperties();
        
        PrefabUtility.SaveAsPrefabAsset(balloon, prefabPath);
        Object.DestroyImmediate(balloon);
        
        Debug.Log("[PopBalloonsSetup] Balloon Prefabを作成しました");
    }
    
    private static void CreateGiantBalloonPrefab()
    {
        string prefabPath = "Assets/Games/08_PopBalloons/Prefabs/GiantBalloon.prefab";
        
        if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
        {
            AssetDatabase.DeleteAsset(prefabPath);
        }
        
        GameObject giantBalloon = new GameObject("GiantBalloon");
        giantBalloon.AddComponent<SpriteRenderer>();
        giantBalloon.AddComponent<CircleCollider2D>();
        giantBalloon.AddComponent<Rigidbody2D>();
        GiantBalloonController controller = giantBalloon.AddComponent<GiantBalloonController>();
        
        // デフォルトのサイズを設定
        SerializedObject so = new SerializedObject(controller);
        so.FindProperty("balloonSize").floatValue = 3.0f;
        so.FindProperty("colliderRadius").floatValue = 2.0f;
        so.ApplyModifiedProperties();
        
        PrefabUtility.SaveAsPrefabAsset(giantBalloon, prefabPath);
        Object.DestroyImmediate(giantBalloon);
        
        Debug.Log("[PopBalloonsSetup] GiantBalloon Prefabを作成しました");
    }
    
    private static void CreateAnimalParachutePrefab()
    {
        string prefabPath = "Assets/Games/08_PopBalloons/Prefabs/AnimalParachute.prefab";
        
        if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
        {
            AssetDatabase.DeleteAsset(prefabPath);
        }
        
        GameObject animalParachute = new GameObject("AnimalParachute");
        AnimalParachuteController controller = animalParachute.AddComponent<AnimalParachuteController>();
        
        // パラシュート付き動物スプライトを設定
        Sprite[] spritesB = LoadSpritesFromSheet("Assets/Games/08_PopBalloons/Sprites/work_sprite_b.png");
        
        SerializedObject so = new SerializedObject(controller);
        SerializedProperty animalSpritesProperty = so.FindProperty("animalParachuteSprites");
        animalSpritesProperty.arraySize = 3;
        for (int i = 0; i < 3 && i + 1 < spritesB.Length; i++)
        {
            animalSpritesProperty.GetArrayElementAtIndex(i).objectReferenceValue = spritesB[i + 1]; // red_1~3
        }
        so.ApplyModifiedProperties();
        
        PrefabUtility.SaveAsPrefabAsset(animalParachute, prefabPath);
        Object.DestroyImmediate(animalParachute);
        
        Debug.Log("[PopBalloonsSetup] AnimalParachute Prefabを作成しました");
    }
    
    private static void CreateStarParticlePrefab()
    {
        string prefabPath = "Assets/Games/08_PopBalloons/Prefabs/StarParticle.prefab";
        
        if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
        {
            AssetDatabase.DeleteAsset(prefabPath);
        }
        
        GameObject starParticle = new GameObject("StarParticle");
        ParticleSystem ps = starParticle.AddComponent<ParticleSystem>();
        
        // パーティクル設定
        var main = ps.main;
        main.startLifetime = 1.5f;
        main.startSpeed = 3f;
        main.startSize = 0.2f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        
        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 10) });
        
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.5f;
        
        PrefabUtility.SaveAsPrefabAsset(starParticle, prefabPath);
        Object.DestroyImmediate(starParticle);
        
        Debug.Log("[PopBalloonsSetup] StarParticle Prefabを作成しました");
    }
    
    private static Sprite[] LoadSpritesFromSheet(string path)
    {
        Object[] sprites = AssetDatabase.LoadAllAssetsAtPath(path);
        System.Collections.Generic.List<Sprite> spriteList = new System.Collections.Generic.List<Sprite>();
        
        foreach (Object obj in sprites)
        {
            if (obj is Sprite)
            {
                spriteList.Add(obj as Sprite);
            }
        }
        
        return spriteList.ToArray();
    }
}
