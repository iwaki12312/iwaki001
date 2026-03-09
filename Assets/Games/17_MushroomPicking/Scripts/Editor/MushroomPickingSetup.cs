#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// MushroomPickingゲームのシーンセットアップを行うエディタ拡張
/// Tools → Setup MushroomPicking Game から実行
/// </summary>
public static class MushroomPickingSetup
{
    private const string GAME_PATH = "Assets/Games/17_MushroomPicking";
    private const string SCENE_NAME = "MushroomPicking";

    [MenuItem("Tools/Setup MushroomPicking Game")]
    public static void SetupGame()
    {
        // 新しいシーンを作成
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // カメラ設定
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            mainCamera.orthographic = true;
            mainCamera.orthographicSize = 5;
            mainCamera.backgroundColor = new Color(0.2f, 0.4f, 0.15f); // 深い緑（森っぽい）
            mainCamera.transform.position = new Vector3(0, 0, -10);
        }

        // Initializerオブジェクトを作成
        GameObject initializerObj = new GameObject("MushroomPickingInitializer");
        MushroomPickingInitializer initializer = initializerObj.AddComponent<MushroomPickingInitializer>();

        // 背景オブジェクトを作成
        CreateBackgroundObject();

        // 仮アセットを自動設定
        SetupPlaceholderAssets(initializer);

        // スポーンポイントを配置
        CreateSpawnPoints(initializer);

        // シーンを保存
        string scenePath = $"{GAME_PATH}/Scenes/{SCENE_NAME}.unity";

        string directory = Path.GetDirectoryName(scenePath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        EditorSceneManager.SaveScene(scene, scenePath);

        // Build Settingsにシーンを追加
        AddSceneToBuildSettings();

        AssetDatabase.Refresh();

        Debug.Log($"[MushroomPickingSetup] シーンを作成しました: {scenePath}");

        EditorUtility.DisplayDialog(
            "セットアップ完了",
            $"MushroomPickingゲームのセットアップが完了しました！\n\n" +
            $"シーン: {scenePath}\n\n" +
            "Playボタンで動作確認できます（仮アセット使用）\n\n" +
            "本番アセットはMushroomPickingInitializerのInspectorで設定してください。",
            "OK"
        );
    }

    /// <summary>
    /// 背景オブジェクトを作成
    /// </summary>
    private static void CreateBackgroundObject()
    {
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.position = Vector3.zero;
        bgObj.transform.localScale = new Vector3(10, 10, 1);

        SpriteRenderer sr = bgObj.AddComponent<SpriteRenderer>();
        sr.sortingOrder = -100;

        // 背景スプライトをロード（本番→JPG→仮の順でフォールバック）
        Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{GAME_PATH}/Sprites/bg_forest.png");
        if (bgSprite == null) bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{GAME_PATH}/Sprites/bg_forest.jpg");
        if (bgSprite == null) bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{GAME_PATH}/Sprites/work_bg.png");
        if (bgSprite != null)
        {
            sr.sprite = bgSprite;
        }

        Debug.Log("[MushroomPickingSetup] 背景オブジェクトを作成しました");
    }

    /// <summary>
    /// 仮アセットを自動設定
    /// </summary>
    private static void SetupPlaceholderAssets(MushroomPickingInitializer initializer)
    {
        // フォールバック用の仮スプライト
        Sprite fallbackA = AssetDatabase.LoadAssetAtPath<Sprite>($"{GAME_PATH}/Sprites/work_sprite_a.png");
        Sprite fallbackB = AssetDatabase.LoadAssetAtPath<Sprite>($"{GAME_PATH}/Sprites/work_sprite_b.png");

        // フォールバック用の仮効果音
        AudioClip[] workSfx = new AudioClip[8];
        for (int i = 1; i <= 8; i++)
        {
            workSfx[i - 1] = AssetDatabase.LoadAssetAtPath<AudioClip>($"{GAME_PATH}/Audios/work_sfx{i}.mp3");
        }

        SerializedObject so = new SerializedObject(initializer);

        // 通常キノコスプライト（本番→仮の順でフォールバック）
        so.FindProperty("redMushroomSprite").objectReferenceValue = LoadSpriteOrFallback("mushroom_red", fallbackA);
        so.FindProperty("yellowMushroomSprite").objectReferenceValue = LoadSpriteOrFallback("mushroom_yellow", fallbackA);
        so.FindProperty("blueMushroomSprite").objectReferenceValue = LoadSpriteOrFallback("mushroom_blue", fallbackA);
        so.FindProperty("whiteMushroomSprite").objectReferenceValue = LoadSpriteOrFallback("mushroom_white", fallbackA);
        so.FindProperty("greenMushroomSprite").objectReferenceValue = LoadSpriteOrFallback("mushroom_green", fallbackA);
        so.FindProperty("pinkMushroomSprite").objectReferenceValue = LoadSpriteOrFallback("mushroom_pink", fallbackA);
        so.FindProperty("orangeMushroomSprite").objectReferenceValue = LoadSpriteOrFallback("mushroom_orange", fallbackA);
        so.FindProperty("purpleMushroomSprite").objectReferenceValue = LoadSpriteOrFallback("mushroom_purple", fallbackA);
        so.FindProperty("brownMushroomSprite").objectReferenceValue = LoadSpriteOrFallback("mushroom_brown", fallbackA);
        so.FindProperty("skyblueMushroomSprite").objectReferenceValue = LoadSpriteOrFallback("mushroom_skyblue", fallbackA);

        // レアキノコスプライト
        so.FindProperty("goldMushroomSprite").objectReferenceValue = LoadSpriteOrFallback("mushroom_gold", fallbackB);
        so.FindProperty("rainbowMushroomSprite").objectReferenceValue = LoadSpriteOrFallback("mushroom_rainbow", fallbackB);
        so.FindProperty("starMushroomSprite").objectReferenceValue = LoadSpriteOrFallback("mushroom_star", fallbackB);
        so.FindProperty("crystalMushroomSprite").objectReferenceValue = LoadSpriteOrFallback("mushroom_crystal", fallbackB);
        so.FindProperty("cosmicMushroomSprite").objectReferenceValue = LoadSpriteOrFallback("mushroom_cosmic", fallbackB);

        // スーパーレアキノコスプライト
        so.FindProperty("rabbitMushroomSprite").objectReferenceValue = LoadSpriteOrFallback("mushroom_rabbit", fallbackB);
        so.FindProperty("mouseMushroomSprite").objectReferenceValue = LoadSpriteOrFallback("mushroom_mouse", fallbackB);
        so.FindProperty("squirrelMushroomSprite").objectReferenceValue = LoadSpriteOrFallback("mushroom_squirrel", fallbackB);

        // カゴスプライト
        so.FindProperty("basketSprite").objectReferenceValue = LoadSpriteOrFallback("basket", fallbackB);

        // 効果音（本番→仮の順でフォールバック）
        so.FindProperty("growSound").objectReferenceValue = LoadAudioOrFallback("sfx_grow", workSfx[0]);
        so.FindProperty("pickSound").objectReferenceValue = LoadAudioOrFallback("sfx_pick", workSfx[1]);
        so.FindProperty("revealSound").objectReferenceValue = LoadAudioOrFallback("sfx_reveal", workSfx[6]);
        so.FindProperty("rarePickSound").objectReferenceValue = LoadAudioOrFallback("sfx_rare_pick", workSfx[2]);
        so.FindProperty("rareRevealSound").objectReferenceValue = LoadAudioOrFallback("sfx_rare_reveal", workSfx[7]);
        so.FindProperty("basketSound").objectReferenceValue = LoadAudioOrFallback("sfx_basket", workSfx[3]);
        so.FindProperty("hideSound").objectReferenceValue = LoadAudioOrFallback("sfx_hide", workSfx[4]);
        so.FindProperty("rareAppearSound").objectReferenceValue = LoadAudioOrFallback("sfx_rare_appear", workSfx[5]);
        so.FindProperty("superRareRevealSound").objectReferenceValue = LoadAudioOrFallback("sfx_super_rare_reveal", null);

        // パーティクルprefab
        GameObject sparkleParticle = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/JMO Assets/Cartoon FX Remaster/CFXR Prefabs/Misc/CFXR Magic Poof.prefab");
        if (sparkleParticle != null)
        {
            so.FindProperty("sparkleParticlePrefab").objectReferenceValue = sparkleParticle;
        }

        GameObject rareParticle = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/JMO Assets/Cartoon FX Remaster/CFXR Prefabs/Misc/CFXR Magic Poof.prefab");
        if (rareParticle != null)
        {
            so.FindProperty("rareParticlePrefab").objectReferenceValue = rareParticle;
        }

        GameObject superRareParticle = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/JMO Assets/Cartoon FX Remaster/CFXR Prefabs/Misc/CFXR Magical Source.prefab");
        if (superRareParticle != null)
        {
            so.FindProperty("superRareParticlePrefab").objectReferenceValue = superRareParticle;
        }

        so.ApplyModifiedProperties();

        Debug.Log("[MushroomPickingSetup] アセットを設定しました");
    }

    private static Sprite LoadSpriteOrFallback(string name, Sprite fallback)
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{GAME_PATH}/Sprites/{name}.png");
        return sprite != null ? sprite : fallback;
    }

    private static AudioClip LoadAudioOrFallback(string name, AudioClip fallback)
    {
        AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>($"{GAME_PATH}/Audios/{name}.mp3");
        return clip != null ? clip : fallback;
    }

    /// <summary>
    /// スポーンポイントをシーンに配置（5箇所、散らばるように）
    /// </summary>
    private static void CreateSpawnPoints(MushroomPickingInitializer initializer)
    {
        Vector3[] defaultPositions = new Vector3[]
        {
            new Vector3(-3.5f, -1f, 0),     // 左下
            new Vector3(-1f,   0.5f, 0),     // 左上寄り
            new Vector3( 1.5f, -0.5f, 0),    // 中央右下
            new Vector3( 3.5f,  0f, 0),      // 右
            new Vector3( 0f,   -2f, 0),      // 中央下
            new Vector3(-2f,   -2.5f, 0),    // 左下端
        };

        Sprite previewSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{GAME_PATH}/Sprites/work_sprite_a.png");

        SerializedObject so = new SerializedObject(initializer);
        SerializedProperty spawnPointsProp = so.FindProperty("spawnPoints");
        spawnPointsProp.ClearArray();

        for (int i = 0; i < defaultPositions.Length; i++)
        {
            GameObject spObj = new GameObject($"MushroomSpawnPoint_{i:D2}");
            spObj.transform.position = defaultPositions[i];

            MushroomSpawnPoint sp = spObj.AddComponent<MushroomSpawnPoint>();

            // プレビュースプライトをSerializedObjectで設定
            SerializedObject spSO = new SerializedObject(sp);
            spSO.FindProperty("previewSprite").objectReferenceValue = previewSprite;
            spSO.ApplyModifiedProperties();

            // リストに追加
            spawnPointsProp.InsertArrayElementAtIndex(i);
            spawnPointsProp.GetArrayElementAtIndex(i).objectReferenceValue = sp;
        }

        so.ApplyModifiedProperties();
        Debug.Log($"[MushroomPickingSetup] {defaultPositions.Length}個のスポーンポイントを配置しました");
    }

    /// <summary>
    /// Build Settingsにシーンを追加
    /// </summary>
    private static void AddSceneToBuildSettings()
    {
        string scenePath = $"{GAME_PATH}/Scenes/{SCENE_NAME}.unity";

        var scenes = EditorBuildSettings.scenes;
        foreach (var scene in scenes)
        {
            if (scene.path == scenePath)
            {
                Debug.Log("[MushroomPickingSetup] シーンは既にBuild Settingsに追加されています");
                return;
            }
        }

        var newScenes = new EditorBuildSettingsScene[scenes.Length + 1];
        for (int i = 0; i < scenes.Length; i++)
        {
            newScenes[i] = scenes[i];
        }
        newScenes[scenes.Length] = new EditorBuildSettingsScene(scenePath, true);
        EditorBuildSettings.scenes = newScenes;

        Debug.Log($"[MushroomPickingSetup] Build Settingsにシーンを追加しました: {scenePath}");
    }
}
#endif
