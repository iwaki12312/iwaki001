using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// VegetableDigゲームを自動セットアップするエディタ拡張
/// Tools → Setup VegetableDig Game から実行
/// </summary>
public static class VegetableDigSetup
{
    private const string GAME_PATH = "Assets/Games/10_VegetableDig";
    private const string SCENE_PATH = GAME_PATH + "/Scenes/VegetableDig.unity";
    private const string PREFAB_PATH = GAME_PATH + "/Prefabs/Vegetable.prefab";
    
    [MenuItem("Tools/Setup VegetableDig Game")]
    public static void SetupGame()
    {
        // シーンを作成または開く
        if (!System.IO.File.Exists(SCENE_PATH))
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene, SCENE_PATH);
        }
        EditorSceneManager.OpenScene(SCENE_PATH);
        
        Debug.Log("[VegetableDigSetup] セットアップ開始");
        
        // 既存のセットアップオブジェクトを削除
        CleanupExistingObjects();
        
        // 背景をセットアップ
        SetupBackground();
        
        // カメラをセットアップ
        SetupCamera();
        
        // SFXPlayerをセットアップ
        SetupSFXPlayer();
        
        // 野菜プレハブを作成
        GameObject prefab = CreateVegetablePrefab();
        
        // VegetableSpawnerをセットアップ
        SetupVegetableSpawner(prefab);
        
        // シーンを保存
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        
        EditorUtility.DisplayDialog("完了", 
            "VegetableDigゲームのセットアップが完了しました!\n\n" +
            "【次のステップ】\n" +
            "1. シーンを再生して動作を確認\n" +
            "2. 野菜のスプライトを本物に差し替え\n" +
            "3. 効果音を本物に差し替え\n" +
            "4. インスペクターでスポット位置を調整", 
            "OK");
        
        Debug.Log("[VegetableDigSetup] セットアップ完了");
    }
    
    private static void CleanupExistingObjects()
    {
        string[] objectNames = { "Background", "VegetableDigSFXPlayer", "VegetableSpawner" };
        foreach (string name in objectNames)
        {
            GameObject obj = GameObject.Find(name);
            if (obj != null)
            {
                Object.DestroyImmediate(obj);
            }
        }
    }
    
    private static void SetupBackground()
    {
        // 背景スプライトをロード
        Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(GAME_PATH + "/Sprites/work_bg.png");
        if (bgSprite == null)
        {
            Debug.LogWarning("[VegetableDigSetup] 背景スプライトが見つかりません");
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
            float screenWidth = screenHeight * mainCam.aspect;
            float spriteHeight = bgSprite.bounds.size.y;
            float spriteWidth = bgSprite.bounds.size.x;
            
            // 画面全体を覆うようにスケール
            float scaleY = screenHeight / spriteHeight;
            float scaleX = screenWidth / spriteWidth;
            float scale = Mathf.Max(scaleX, scaleY);
            bg.transform.localScale = Vector3.one * scale;
        }
        
        Debug.Log("[VegetableDigSetup] 背景を配置しました");
    }
    
    private static void SetupCamera()
    {
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.orthographicSize = 5f;
            mainCam.backgroundColor = new Color(0.6f, 0.4f, 0.2f); // 土色
        }
    }
    
    private static void SetupSFXPlayer()
    {
        GameObject sfxPlayer = new GameObject("VegetableDigSFXPlayer");
        VegetableDigSFXPlayer player = sfxPlayer.AddComponent<VegetableDigSFXPlayer>();
        
        // 効果音をロード
        AudioClip pullSound = AssetDatabase.LoadAssetAtPath<AudioClip>(GAME_PATH + "/Audios/work_sfx1.mp3");
        AudioClip normalFanfare = AssetDatabase.LoadAssetAtPath<AudioClip>(GAME_PATH + "/Audios/work_sfx2.mp3");
        AudioClip rareFanfare = AssetDatabase.LoadAssetAtPath<AudioClip>(GAME_PATH + "/Audios/work_sfx3.mp3");
        
        SerializedObject so = new SerializedObject(player);
        so.FindProperty("pullSound").objectReferenceValue = pullSound;
        so.FindProperty("normalFanfare").objectReferenceValue = normalFanfare;
        so.FindProperty("rareFanfare").objectReferenceValue = rareFanfare;
        so.ApplyModifiedProperties();
        
        Debug.Log("[VegetableDigSetup] SFXPlayerを作成しました");
    }
    
    private static GameObject CreateVegetablePrefab()
    {
        // 既存のプレハブがあればロード
        GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_PATH);
        if (existingPrefab != null)
        {
            Debug.Log("[VegetableDigSetup] 既存のプレハブを使用");
            return existingPrefab;
        }
        
        // 新規作成
        GameObject vegetable = new GameObject("Vegetable");
        
        // VegetableControllerを追加
        vegetable.AddComponent<VegetableController>();
        
        // 葉っぱ子オブジェクト
        GameObject leaf = new GameObject("Leaf");
        leaf.transform.SetParent(vegetable.transform);
        leaf.transform.localPosition = new Vector3(0, 0.5f, 0); // 葉っぱは少し上
        SpriteRenderer leafSr = leaf.AddComponent<SpriteRenderer>();
        leafSr.sortingOrder = 1;
        
        // 野菜本体子オブジェクト
        GameObject body = new GameObject("VegetableBody");
        body.transform.SetParent(vegetable.transform);
        body.transform.localPosition = Vector3.zero;
        SpriteRenderer bodySr = body.AddComponent<SpriteRenderer>();
        bodySr.sortingOrder = 0;
        
        // レア野菜用パーティクルプレハブをロード
        GameObject shineParticle = AssetDatabase.LoadAssetAtPath<GameObject>(GAME_PATH + "/Prefabs/Shine-1-Particles.prefab");
        if (shineParticle != null)
        {
            VegetableController controller = vegetable.GetComponent<VegetableController>();
            SerializedObject so = new SerializedObject(controller);
            so.FindProperty("shineParticlePrefab").objectReferenceValue = shineParticle;
            so.ApplyModifiedProperties();
            Debug.Log("[VegetableDigSetup] レア野菜用パーティクルを設定しました");
        }
        else
        {
            Debug.LogWarning("[VegetableDigSetup] Shine-1-Particles.prefab が見つかりません");
        }
        
        // プレハブとして保存
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(vegetable, PREFAB_PATH);
        Object.DestroyImmediate(vegetable);
        
        Debug.Log("[VegetableDigSetup] 野菜プレハブを作成しました");
        return prefab;
    }
    
    private static void SetupVegetableSpawner(GameObject prefab)
    {
        GameObject spawner = new GameObject("VegetableSpawner");
        VegetableSpawner spawnerScript = spawner.AddComponent<VegetableSpawner>();
        
        // 仮スプライトをロード
        Sprite spriteA = AssetDatabase.LoadAssetAtPath<Sprite>(GAME_PATH + "/Sprites/work_sprite_a.png");
        Sprite spriteB = AssetDatabase.LoadAssetAtPath<Sprite>(GAME_PATH + "/Sprites/work_sprite_b.png");
        
        // SerializedObjectで設定
        SerializedObject so = new SerializedObject(spawnerScript);
        
        // プレハブを設定
        so.FindProperty("vegetablePrefab").objectReferenceValue = prefab;
        
        // スポット位置を設定（6箇所、2行3列の配置）
        SerializedProperty spotsProp = so.FindProperty("vegetableSpots");
        spotsProp.arraySize = 6;
        
        float[] xPositions = { -4f, 0f, 4f };
        float[] yPositions = { 1.5f, -2f };
        
        int spotIndex = 0;
        for (int row = 0; row < 2; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                SerializedProperty spotProp = spotsProp.GetArrayElementAtIndex(spotIndex);
                spotProp.FindPropertyRelative("position").vector3Value = new Vector3(xPositions[col], yPositions[row], 0);
                spotProp.FindPropertyRelative("scale").floatValue = 1f;
                spotProp.FindPropertyRelative("colliderRadius").floatValue = 1.2f;
                spotIndex++;
            }
        }
        
        // 通常野菜を設定（仮スプライト使用）
        SerializedProperty normalVegProp = so.FindProperty("normalVegetables");
        normalVegProp.arraySize = 2;
        
        SerializedProperty normal1 = normalVegProp.GetArrayElementAtIndex(0);
        normal1.FindPropertyRelative("name").stringValue = "VegetableA";
        normal1.FindPropertyRelative("leafSprite").objectReferenceValue = spriteA;
        normal1.FindPropertyRelative("vegetableSprite").objectReferenceValue = spriteA;
        normal1.FindPropertyRelative("isRare").boolValue = false;
        
        SerializedProperty normal2 = normalVegProp.GetArrayElementAtIndex(1);
        normal2.FindPropertyRelative("name").stringValue = "VegetableB";
        normal2.FindPropertyRelative("leafSprite").objectReferenceValue = spriteB;
        normal2.FindPropertyRelative("vegetableSprite").objectReferenceValue = spriteB;
        normal2.FindPropertyRelative("isRare").boolValue = false;
        
        // レア野菜を設定（仮スプライト使用）
        SerializedProperty rareVegProp = so.FindProperty("rareVegetables");
        rareVegProp.arraySize = 1;
        
        SerializedProperty rare1 = rareVegProp.GetArrayElementAtIndex(0);
        rare1.FindPropertyRelative("name").stringValue = "RareVegetable";
        rare1.FindPropertyRelative("leafSprite").objectReferenceValue = spriteB;
        rare1.FindPropertyRelative("vegetableSprite").objectReferenceValue = spriteB;
        rare1.FindPropertyRelative("isRare").boolValue = true;
        
        // レア確率を設定
        so.FindProperty("rareChance").floatValue = 0.15f;
        
        so.ApplyModifiedProperties();
        
        Debug.Log("[VegetableDigSetup] VegetableSpawnerを作成しました（6箇所、2行3列配置）");
    }
}
