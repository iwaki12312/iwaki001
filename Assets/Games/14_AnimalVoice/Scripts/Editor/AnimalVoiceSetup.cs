#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

/// <summary>
/// AnimalVoiceゲームのシーンセットアップを行うエディタ拡張
/// Tools → Setup AnimalVoice Game から実行
/// </summary>
public static class AnimalVoiceSetup
{
    private const string GAME_PATH = "Assets/Games/14_AnimalVoice";
    private const string SCENE_NAME = "AnimalVoice";
    
    [MenuItem("Tools/Setup AnimalVoice Game")]
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
            mainCamera.backgroundColor = new Color(0.5f, 0.7f, 1f); // 明るい空色
            mainCamera.transform.position = new Vector3(0, 0, -10);
        }
        
        // Initializerオブジェクトを作成
        GameObject initializerObj = new GameObject("AnimalVoiceInitializer");
        AnimalVoiceInitializer initializer = initializerObj.AddComponent<AnimalVoiceInitializer>();
        
        // 仮アセットを自動設定
        SetupPlaceholderAssets(initializer);
        
        // スポーンポイントを配置
        CreateSpawnPoints(initializer);
        
        // シーンを保存
        string scenePath = $"{GAME_PATH}/Scenes/{SCENE_NAME}.unity";
        
        // ディレクトリが存在しない場合は作成
        string directory = Path.GetDirectoryName(scenePath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        
        EditorSceneManager.SaveScene(scene, scenePath);
        
        // Build Settingsにシーンを追加
        AddSceneToBuildSettings();
        
        AssetDatabase.Refresh();
        
        Debug.Log($"[AnimalVoiceSetup] シーンを作成しました: {scenePath}");
        
        EditorUtility.DisplayDialog(
            "セットアップ完了",
            $"AnimalVoiceゲームのセットアップが完了しました！\n\n" +
            $"シーン: {scenePath}\n\n" +
            "Playボタンで動作確認できます（仮アセット使用）\n\n" +
            "本番アセットはAnimalVoiceInitializerのInspectorで設定してください。",
            "OK"
        );
    }
    
    /// <summary>
    /// 生成された画像とアセットを自動設定
    /// </summary>
    private static void SetupPlaceholderAssets(AnimalVoiceInitializer initializer)
    {
        // 背景スプライトをロード
        Sprite bgMorning = AssetDatabase.LoadAssetAtPath<Sprite>($"{GAME_PATH}/Sprites/bg_morning.png");
        Sprite bgDaytime = AssetDatabase.LoadAssetAtPath<Sprite>($"{GAME_PATH}/Sprites/bg_daytime.png");
        Sprite bgNight = AssetDatabase.LoadAssetAtPath<Sprite>($"{GAME_PATH}/Sprites/bg_night.png");

        // 仮効果音をロード
        AudioClip[] workSfx = new AudioClip[8];
        for (int i = 1; i <= 8; i++)
        {
            workSfx[i - 1] = AssetDatabase.LoadAssetAtPath<AudioClip>($"{GAME_PATH}/Audios/work_sfx{i}.mp3");
        }

        // パーティクルプレハブをロード
        GameObject heartParticle = AssetDatabase.LoadAssetAtPath<GameObject>($"{GAME_PATH}/Prefabs/ParticleHeart.prefab");
        GameObject noteParticle = AssetDatabase.LoadAssetAtPath<GameObject>($"{GAME_PATH}/Prefabs/ParticleNote.prefab");

        // SerializedObjectで設定
        SerializedObject so = new SerializedObject(initializer);

        // 背景
        so.FindProperty("morningBackground").objectReferenceValue = bgMorning;
        so.FindProperty("daytimeBackground").objectReferenceValue = bgDaytime;
        so.FindProperty("nightBackground").objectReferenceValue = bgNight;
        
        // 朝の動物
        SetAnimalSpritesFromPath(so, "chicken");
        SetAnimalSpritesFromPath(so, "cow");
        SetAnimalSpritesFromPath(so, "horse");
        SetAnimalSpritesFromPath(so, "pig");
        SetAnimalSpritesFromPath(so, "sheep");
        SetAnimalSpritesFromPath(so, "goat");

        // 昼の動物
        SetAnimalSpritesFromPath(so, "dog");
        SetAnimalSpritesFromPath(so, "cat");
        SetAnimalSpritesFromPath(so, "elephant");
        SetAnimalSpritesFromPath(so, "lion");
        SetAnimalSpritesFromPath(so, "frog");
        SetAnimalSpritesFromPath(so, "chick");

        // 夜の動物
        SetAnimalSpritesFromPath(so, "owl");
        SetAnimalSpritesFromPath(so, "wolf");
        SetAnimalSpritesFromPath(so, "bat");
        SetAnimalSpritesFromPath(so, "mouse");

        // レア動物
        SetAnimalSpritesFromPath(so, "dinosaur");
        SetAnimalSpritesFromPath(so, "dragon");
        SetAnimalSpritesFromPath(so, "unicorn");
        SetAnimalSpritesFromPath(so, "monkey");
        
        // 鳴き声（仮SE）
        so.FindProperty("chickenVoice").objectReferenceValue = workSfx[0];
        so.FindProperty("cowVoice").objectReferenceValue = workSfx[1];
        so.FindProperty("horseVoice").objectReferenceValue = workSfx[2];
        so.FindProperty("pigVoice").objectReferenceValue = workSfx[3];
        so.FindProperty("sheepVoice").objectReferenceValue = workSfx[4];
        so.FindProperty("goatVoice").objectReferenceValue = workSfx[5];
        
        so.FindProperty("dogVoice").objectReferenceValue = workSfx[0];
        so.FindProperty("catVoice").objectReferenceValue = workSfx[1];
        so.FindProperty("elephantVoice").objectReferenceValue = workSfx[2];
        so.FindProperty("lionVoice").objectReferenceValue = workSfx[3];
        so.FindProperty("frogVoice").objectReferenceValue = workSfx[4];
        so.FindProperty("chickVoice").objectReferenceValue = workSfx[5];
        
        so.FindProperty("owlVoice").objectReferenceValue = workSfx[0];
        so.FindProperty("wolfVoice").objectReferenceValue = workSfx[1];
        so.FindProperty("batVoice").objectReferenceValue = workSfx[2];
        so.FindProperty("mouseVoice").objectReferenceValue = workSfx[3];
        
        so.FindProperty("dinosaurVoice").objectReferenceValue = workSfx[6];
        so.FindProperty("dragonVoice").objectReferenceValue = workSfx[6];
        so.FindProperty("unicornVoice").objectReferenceValue = workSfx[7];
        so.FindProperty("monkeyVoice").objectReferenceValue = workSfx[7];
        
        // 共通SE
        so.FindProperty("tapSound").objectReferenceValue = workSfx[0];
        so.FindProperty("timeChangeSound").objectReferenceValue = workSfx[7];
        so.FindProperty("rareAppearSound").objectReferenceValue = workSfx[6];

        // パーティクルプレハブ
        so.FindProperty("heartParticlePrefab").objectReferenceValue = heartParticle;
        so.FindProperty("noteParticlePrefab").objectReferenceValue = noteParticle;

        so.ApplyModifiedProperties();
    }

    private static void SetAnimalSpritesFromPath(SerializedObject so, string animalName)
    {
        Sprite normal = AssetDatabase.LoadAssetAtPath<Sprite>($"{GAME_PATH}/Sprites/{animalName}_normal.png");
        Sprite reaction = AssetDatabase.LoadAssetAtPath<Sprite>($"{GAME_PATH}/Sprites/{animalName}_reaction.png");
        so.FindProperty($"{animalName}Normal").objectReferenceValue = normal;
        so.FindProperty($"{animalName}Reaction").objectReferenceValue = reaction;
    }
    
    /// <summary>
    /// スポーンポイントをシーンに配置
    /// </summary>
    private static void CreateSpawnPoints(AnimalVoiceInitializer initializer)
    {
        // 6箇所のデフォルト配置（2行×3列）
        Vector3[] defaultPositions = new Vector3[]
        {
            new Vector3(-3f,  1.5f, 0),
            new Vector3( 0f,  1.5f, 0),
            new Vector3( 3f,  1.5f, 0),
            new Vector3(-3f, -1.5f, 0),
            new Vector3( 0f, -1.5f, 0),
            new Vector3( 3f, -1.5f, 0),
        };
        
        // プレビュー用のスプライトをロード（朝の動物をデフォルトプレビューとして使用）
        string[] previewAnimals = { "chicken", "cow", "horse", "pig", "sheep", "goat" };
        
        SerializedObject so = new SerializedObject(initializer);
        SerializedProperty spawnPointsProp = so.FindProperty("spawnPoints");
        spawnPointsProp.ClearArray();
        
        for (int i = 0; i < defaultPositions.Length; i++)
        {
            string animalName = previewAnimals[i % previewAnimals.Length];
            Sprite previewSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{GAME_PATH}/Sprites/{animalName}_normal.png");
            
            GameObject spObj = new GameObject($"SpawnPoint_{i:D2}");
            spObj.transform.position = defaultPositions[i];
            
            AnimalSpawnPoint sp = spObj.AddComponent<AnimalSpawnPoint>();
            
            // プレビュースプライトをSerializedObjectで設定
            SerializedObject spSO = new SerializedObject(sp);
            spSO.FindProperty("previewSprite").objectReferenceValue = previewSprite;
            spSO.ApplyModifiedProperties();
            
            // リストに追加
            spawnPointsProp.InsertArrayElementAtIndex(i);
            spawnPointsProp.GetArrayElementAtIndex(i).objectReferenceValue = sp;
        }
        
        so.ApplyModifiedProperties();
        Debug.Log($"[AnimalVoiceSetup] {defaultPositions.Length}個のスポーンポイントを配置しました");
    }
    
    /// <summary>
    /// Build Settingsにシーンを追加
    /// </summary>
    private static void AddSceneToBuildSettings()
    {
        string scenePath = $"{GAME_PATH}/Scenes/{SCENE_NAME}.unity";
        
        // 既に追加されているかチェック
        var scenes = EditorBuildSettings.scenes;
        foreach (var scene in scenes)
        {
            if (scene.path == scenePath)
            {
                Debug.Log("[AnimalVoiceSetup] シーンは既にBuild Settingsに追加されています");
                return;
            }
        }
        
        // シーンを追加
        var newScenes = new EditorBuildSettingsScene[scenes.Length + 1];
        for (int i = 0; i < scenes.Length; i++)
        {
            newScenes[i] = scenes[i];
        }
        newScenes[scenes.Length] = new EditorBuildSettingsScene(scenePath, true);
        EditorBuildSettings.scenes = newScenes;
        
        Debug.Log($"[AnimalVoiceSetup] Build Settingsにシーンを追加しました: {scenePath}");
    }
}
#endif
