#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

/// <summary>
/// FruitSliceゲームのシーンセットアップ
/// Tools → Setup FruitSlice Game から実行
/// </summary>
public static class FruitSliceSetup
{
    private const string GAME_PATH = "Assets/Games/15_FruitSlice";
    private const string SCENE_NAME = "FruitSlice";

    [MenuItem("Tools/Setup FruitSlice Game")]
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
            mainCamera.backgroundColor = new Color(0.6f, 0.45f, 0.3f);  // 木目っぽい茶色
            mainCamera.transform.position = new Vector3(0, 0, -10);
        }

        // 背景オブジェクトを作成
        CreateBackgroundObject();

        // Initializerオブジェクトを作成
        GameObject initializerObj = new GameObject("FruitSliceInitializer");
        FruitSliceInitializer initializer = initializerObj.AddComponent<FruitSliceInitializer>();

        // アセットを自動設定
        SetupAssets(initializer);

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

        Debug.Log($"[FruitSliceSetup] シーンを作成しました: {scenePath}");

        EditorUtility.DisplayDialog(
            "セットアップ完了",
            $"FruitSliceゲームのセットアップが完了しました！\n\n" +
            $"シーン: {scenePath}\n\n" +
            "Playボタンで動作確認できます。",
            "OK"
        );
    }

    /// <summary>
    /// 背景オブジェクト（まな板）を作成
    /// </summary>
    private static void CreateBackgroundObject()
    {
        Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{GAME_PATH}/Sprites/bg_cutting_board.png");

        // work_bg をフォールバック
        if (bgSprite == null)
        {
            bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{GAME_PATH}/Sprites/work_bg.png");
        }

        GameObject bgObj = new GameObject("Background_CuttingBoard");
        SpriteRenderer sr = bgObj.AddComponent<SpriteRenderer>();
        sr.sortingOrder = -100;
        sr.sprite = bgSprite;
        bgObj.transform.localScale = new Vector3(10f, 10f, 1f);

        Debug.Log("[FruitSliceSetup] 背景オブジェクトを作成しました");
    }

    /// <summary>
    /// アセットを自動設定
    /// </summary>
    private static void SetupAssets(FruitSliceInitializer initializer)
    {
        SerializedObject so = new SerializedObject(initializer);

        // 背景スプライト
        Sprite cuttingBoard = AssetDatabase.LoadAssetAtPath<Sprite>($"{GAME_PATH}/Sprites/bg_cutting_board.png");
        if (cuttingBoard == null)
        {
            cuttingBoard = AssetDatabase.LoadAssetAtPath<Sprite>($"{GAME_PATH}/Sprites/work_bg.png");
        }
        so.FindProperty("cuttingBoardSprite").objectReferenceValue = cuttingBoard;

        // 通常フルーツ（3状態）
        string[] normalFruits = { "apple", "orange", "peach", "pineapple", "watermelon", "pear", "kiwi", "lemon" };
        foreach (string fruit in normalFruits)
        {
            SetFruitSpritesFromPath(so, fruit);
        }

        // レアフルーツ
        SetFruitSpritesFromPath(so, "goldenApple", "golden_apple");
        SetFruitSpritesFromPath(so, "rainbowMango", "rainbow_mango");
        SetFruitSpritesFromPath(so, "diamondOrange", "diamond_orange");

        // 効果音
        so.FindProperty("cutSound").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>($"{GAME_PATH}/Audios/cut_sfx.mp3");
        so.FindProperty("plateSound").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>($"{GAME_PATH}/Audios/plate_sfx.mp3");
        so.FindProperty("completeSound").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>($"{GAME_PATH}/Audios/complete_sfx.mp3");
        so.FindProperty("rareSound").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>($"{GAME_PATH}/Audios/rare_sfx.mp3");
        so.FindProperty("spawnSound").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>($"{GAME_PATH}/Audios/spawn_sfx.mp3");

        so.ApplyModifiedProperties();

        Debug.Log("[FruitSliceSetup] アセットを設定しました");
    }

    /// <summary>
    /// フルーツの3状態スプライトを設定（プロパティ名とファイル名が同じ場合）
    /// </summary>
    private static void SetFruitSpritesFromPath(SerializedObject so, string propertyName)
    {
        SetFruitSpritesFromPath(so, propertyName, propertyName);
    }

    /// <summary>
    /// フルーツの3状態スプライトを設定（プロパティ名とファイル名が異なる場合）
    /// </summary>
    private static void SetFruitSpritesFromPath(SerializedObject so, string propertyName, string fileName)
    {
        Sprite whole = AssetDatabase.LoadAssetAtPath<Sprite>($"{GAME_PATH}/Sprites/{fileName}_whole.png");
        Sprite cut = AssetDatabase.LoadAssetAtPath<Sprite>($"{GAME_PATH}/Sprites/{fileName}_cut.png");
        Sprite plated = AssetDatabase.LoadAssetAtPath<Sprite>($"{GAME_PATH}/Sprites/{fileName}_plated.png");

        // フォールバック: work_sprite_a / work_sprite_b をプレースホルダーとして使用
        if (whole == null) whole = AssetDatabase.LoadAssetAtPath<Sprite>($"{GAME_PATH}/Sprites/work_sprite_a.png");
        if (cut == null) cut = AssetDatabase.LoadAssetAtPath<Sprite>($"{GAME_PATH}/Sprites/work_sprite_b.png");
        if (plated == null) plated = AssetDatabase.LoadAssetAtPath<Sprite>($"{GAME_PATH}/Sprites/work_sprite_a.png");

        so.FindProperty($"{propertyName}Whole").objectReferenceValue = whole;
        so.FindProperty($"{propertyName}Cut").objectReferenceValue = cut;
        so.FindProperty($"{propertyName}Plated").objectReferenceValue = plated;
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
                Debug.Log("[FruitSliceSetup] シーンは既にBuild Settingsに追加されています");
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

        Debug.Log($"[FruitSliceSetup] Build Settingsにシーンを追加しました: {scenePath}");
    }

    /// <summary>
    /// スポーンポイントをシーンに配置
    /// </summary>
    private static void CreateSpawnPoints(FruitSliceInitializer initializer)
    {
        // 5箇所のデフォルト配置
        Vector3[] defaultPositions = new Vector3[]
        {
            new Vector3(-2.8f,  1.8f, 0f),   // 左上
            new Vector3( 2.8f,  1.8f, 0f),   // 右上
            new Vector3( 0.0f,  0.0f, 0f),   // 中央
            new Vector3(-2.8f, -2.0f, 0f),   // 左下
            new Vector3( 2.8f, -2.0f, 0f),   // 右下
        };

        // プレビュー用スプライトをロード
        string[] previewFruits = { "apple", "orange", "banana", "strawberry", "watermelon" };

        SerializedObject so = new SerializedObject(initializer);
        SerializedProperty spawnPointsProp = so.FindProperty("spawnPoints");
        spawnPointsProp.ClearArray();

        for (int i = 0; i < defaultPositions.Length; i++)
        {
            string fruitName = previewFruits[i % previewFruits.Length];
            Sprite previewSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{GAME_PATH}/Sprites/{fruitName}_whole.png");
            if (previewSprite == null)
            {
                previewSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{GAME_PATH}/Sprites/work_sprite_a.png");
            }

            GameObject spObj = new GameObject($"FruitSpawnPoint_{i:D2}");
            spObj.transform.position = defaultPositions[i];

            FruitSpawnPoint sp = spObj.AddComponent<FruitSpawnPoint>();

            // プレビュースプライトをSerializedObjectで設定
            SerializedObject spSO = new SerializedObject(sp);
            spSO.FindProperty("previewSprite").objectReferenceValue = previewSprite;
            spSO.ApplyModifiedProperties();

            // リストに追加
            spawnPointsProp.InsertArrayElementAtIndex(i);
            spawnPointsProp.GetArrayElementAtIndex(i).objectReferenceValue = sp;
        }

        so.ApplyModifiedProperties();
        Debug.Log($"[FruitSliceSetup] {defaultPositions.Length}個のスポーンポイントを配置しました");
    }
}
#endif
