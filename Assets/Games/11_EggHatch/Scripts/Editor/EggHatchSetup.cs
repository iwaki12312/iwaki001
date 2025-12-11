using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// EggHatchゲームのセットアップを自動化するエディタスクリプト
/// </summary>
public class EggHatchSetup : Editor
{
    private const string GAME_PATH = "Assets/Games/11_EggHatch";
    private const string SCENE_PATH = GAME_PATH + "/Scenes/EggHatch.unity";
    private const string PREFAB_PATH = GAME_PATH + "/Prefabs/Egg.prefab";
    
    [MenuItem("Tools/EggHatch/Setup Scene")]
    public static void SetupScene()
    {
        // 新しいシーンを作成
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        
        // 背景を設定
        SetupBackground();
        
        // カメラを設定
        SetupCamera();
        
        // SFXPlayerを作成
        SetupSFXPlayer();
        
        // たまごプレハブを作成
        GameObject eggPrefab = CreateEggPrefab();
        
        // Spawnerを作成
        SetupEggSpawner(eggPrefab);
        
        // シーンを保存
        System.IO.Directory.CreateDirectory(GAME_PATH + "/Scenes");
        EditorSceneManager.SaveScene(scene, SCENE_PATH);
        
        Debug.Log("[EggHatchSetup] シーンのセットアップが完了しました");
    }
    
    private static void SetupBackground()
    {
        // 背景スプライトをロード
        Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(GAME_PATH + "/Sprites/work_bg.png");
        
        GameObject bg = new GameObject("Background");
        SpriteRenderer sr = bg.AddComponent<SpriteRenderer>();
        sr.sprite = bgSprite;
        sr.sortingOrder = -10;
        
        // 背景を適切なサイズに調整
        bg.transform.localScale = new Vector3(2f, 2f, 1f);
        
        Debug.Log("[EggHatchSetup] 背景を作成しました");
    }
    
    private static void SetupCamera()
    {
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.orthographicSize = 5f;
            mainCam.backgroundColor = new Color(0.5f, 0.8f, 0.5f); // 草原色
        }
    }
    
    private static void SetupSFXPlayer()
    {
        GameObject sfxPlayer = new GameObject("EggHatchSFXPlayer");
        EggHatchSFXPlayer player = sfxPlayer.AddComponent<EggHatchSFXPlayer>();
        
        // 効果音をロード
        AudioClip crackSound = AssetDatabase.LoadAssetAtPath<AudioClip>(GAME_PATH + "/Audios/work_sfx1.mp3");
        AudioClip hatchSound = AssetDatabase.LoadAssetAtPath<AudioClip>(GAME_PATH + "/Audios/work_sfx2.mp3");
        AudioClip normalFanfare = AssetDatabase.LoadAssetAtPath<AudioClip>(GAME_PATH + "/Audios/work_sfx3.mp3");
        AudioClip rareFanfare = AssetDatabase.LoadAssetAtPath<AudioClip>(GAME_PATH + "/Audios/work_sfx3.mp3");
        
        SerializedObject so = new SerializedObject(player);
        so.FindProperty("crackSound").objectReferenceValue = crackSound;
        so.FindProperty("hatchSound").objectReferenceValue = hatchSound;
        so.FindProperty("normalFanfare").objectReferenceValue = normalFanfare;
        so.FindProperty("rareFanfare").objectReferenceValue = rareFanfare;
        so.ApplyModifiedProperties();
        
        Debug.Log("[EggHatchSetup] SFXPlayerを作成しました");
    }
    
    private static GameObject CreateEggPrefab()
    {
        // 既存のプレハブがあれば削除して再作成
        if (System.IO.File.Exists(PREFAB_PATH))
        {
            AssetDatabase.DeleteAsset(PREFAB_PATH);
        }
        
        // 新規作成
        GameObject egg = new GameObject("Egg");
        
        // EggControllerを追加
        EggController controller = egg.AddComponent<EggController>();
        
        // パーティクルプレハブを設定
        GameObject particlePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(GAME_PATH + "/Prefabs/Shine-1-Particles.prefab");
        if (particlePrefab != null)
        {
            SerializedObject controllerSO = new SerializedObject(controller);
            controllerSO.FindProperty("rareParticlePrefab").objectReferenceValue = particlePrefab;
            controllerSO.ApplyModifiedProperties();
        }
        
        // 仮スプライトをロード
        Sprite workSpriteA = AssetDatabase.LoadAssetAtPath<Sprite>(GAME_PATH + "/Sprites/work_sprite_a.png");
        Sprite workSpriteB = AssetDatabase.LoadAssetAtPath<Sprite>(GAME_PATH + "/Sprites/work_sprite_b.png");
        
        // たまご本体（5段階でスプライト切り替え）
        GameObject eggObj = new GameObject("Egg");
        eggObj.transform.SetParent(egg.transform);
        eggObj.transform.localPosition = Vector3.zero;
        SpriteRenderer eggSr = eggObj.AddComponent<SpriteRenderer>();
        eggSr.sprite = workSpriteA;
        eggSr.sortingOrder = 0;
        
        // 動物
        GameObject animal = new GameObject("Animal");
        animal.transform.SetParent(egg.transform);
        animal.transform.localPosition = Vector3.zero;
        SpriteRenderer animalSr = animal.AddComponent<SpriteRenderer>();
        animalSr.sprite = workSpriteB;
        animalSr.sortingOrder = 2;
        
        // プレハブとして保存
        System.IO.Directory.CreateDirectory(GAME_PATH + "/Prefabs");
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(egg, PREFAB_PATH);
        Object.DestroyImmediate(egg);
        
        Debug.Log("[EggHatchSetup] たまごプレハブを作成しました");
        return prefab;
    }
    
    private static void SetupEggSpawner(GameObject prefab)
    {
        GameObject spawner = new GameObject("EggSpawner");
        EggSpawner spawnerScript = spawner.AddComponent<EggSpawner>();
        
        // 仮スプライトをロード
        Sprite spriteA = AssetDatabase.LoadAssetAtPath<Sprite>(GAME_PATH + "/Sprites/work_sprite_a.png");
        Sprite spriteB = AssetDatabase.LoadAssetAtPath<Sprite>(GAME_PATH + "/Sprites/work_sprite_b.png");
        
        // SerializedObjectで設定
        SerializedObject so = new SerializedObject(spawnerScript);
        
        // プレハブを設定
        so.FindProperty("eggPrefab").objectReferenceValue = prefab;
        
        // 卵段階スプライト（5枚・仮）を設定
        so.FindProperty("eggSprite0").objectReferenceValue = spriteA;
        so.FindProperty("eggSprite1").objectReferenceValue = spriteB;
        so.FindProperty("eggSprite2").objectReferenceValue = spriteB;
        so.FindProperty("eggSprite3").objectReferenceValue = spriteB;
        so.FindProperty("eggSprite4").objectReferenceValue = spriteB;
        
        // スポット位置を設定（4箇所、2行2列の配置）
        SerializedProperty spotsProp = so.FindProperty("eggSpots");
        spotsProp.arraySize = 4;
        
        float[] xPositions = { -3f, 3f };
        float[] yPositions = { 1f, -2f };
        
        int spotIndex = 0;
        for (int row = 0; row < 2; row++)
        {
            for (int col = 0; col < 2; col++)
            {
                SerializedProperty spotProp = spotsProp.GetArrayElementAtIndex(spotIndex);
                spotProp.FindPropertyRelative("position").vector3Value = new Vector3(xPositions[col], yPositions[row], 0);
                spotProp.FindPropertyRelative("scale").floatValue = 1f;
                spotProp.FindPropertyRelative("colliderRadius").floatValue = 1.5f;
                spotIndex++;
            }
        }
        
        // 通常動物を設定（仮スプライト使用）
        SerializedProperty normalAnimalsProp = so.FindProperty("normalAnimals");
        normalAnimalsProp.arraySize = 8;
        
        string[] normalAnimalNames = { "Chick", "Ostrich", "Penguin", "Owl", "Tortoise", "Lizard", "Snake", "Chameleon" };
        for (int i = 0; i < normalAnimalNames.Length; i++)
        {
            SerializedProperty animalProp = normalAnimalsProp.GetArrayElementAtIndex(i);
            animalProp.FindPropertyRelative("name").stringValue = normalAnimalNames[i];
            animalProp.FindPropertyRelative("animalSprite").objectReferenceValue = spriteB;
            animalProp.FindPropertyRelative("isRare").boolValue = false;
        }
        
        // レア動物を設定（仮スプライト使用）
        SerializedProperty rareAnimalsProp = so.FindProperty("rareAnimals");
        rareAnimalsProp.arraySize = 7;
        
        string[] rareAnimalNames = { "Dragon", "Phoenix", "TRex", "Triceratops", "Pteranodon", "Velociraptor", "Brachiosaurus" };
        for (int i = 0; i < rareAnimalNames.Length; i++)
        {
            SerializedProperty animalProp = rareAnimalsProp.GetArrayElementAtIndex(i);
            animalProp.FindPropertyRelative("name").stringValue = rareAnimalNames[i];
            animalProp.FindPropertyRelative("animalSprite").objectReferenceValue = spriteB;
            animalProp.FindPropertyRelative("isRare").boolValue = true;
        }
        
        // レア確率を設定
        so.FindProperty("rareChance").floatValue = 0.15f;
        
        so.ApplyModifiedProperties();
        
        Debug.Log("[EggHatchSetup] EggSpawnerを作成しました（4箇所、2行2列配置）");
    }
}
