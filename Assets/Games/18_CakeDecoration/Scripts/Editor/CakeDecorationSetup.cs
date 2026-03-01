#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

/// <summary>
/// CakeDecorationゲームのシーンセットアップを行うエディタ拡張
/// Tools → Setup CakeDecoration Game から実行
/// </summary>
public static class CakeDecorationSetup
{
    private const string GAME_PATH = "Assets/Games/18_CakeDecoration";
    private const string SCENE_NAME = "CakeDecoration";

    [MenuItem("Tools/Setup CakeDecoration Game")]
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
            mainCamera.backgroundColor = new Color(1f, 0.92f, 0.85f); // パステルピーチ色
            mainCamera.transform.position = new Vector3(0, 0, -10);
        }

        // Initializerオブジェクトを作成
        GameObject initializerObj = new GameObject("CakeDecorationInitializer");
        CakeDecorationInitializer initializer = initializerObj.AddComponent<CakeDecorationInitializer>();

        // 背景オブジェクトを作成
        CreateBackgroundObject();

        // ケーキオブジェクトを作成（Scene Viewで位置調整可能）
        CreateCakeObject();

        // プレースホルダーアセットを自動設定
        SetupPlaceholderAssets(initializer);

        // シーンを保存
        string scenePath = $"{GAME_PATH}/Scenes/{SCENE_NAME}.unity";
        string directory = Path.GetDirectoryName(scenePath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings();
        AssetDatabase.Refresh();

        Debug.Log($"[CakeDecorationSetup] シーンを作成しました: {scenePath}");

        EditorUtility.DisplayDialog(
            "セットアップ完了",
            $"CakeDecorationゲームのセットアップが完了しました！\n\n" +
            $"シーン: {scenePath}\n\n" +
            "Playボタンで動作確認できます（仮アセット使用）\n\n" +
            "ケーキの位置はScene Viewで直接調整できます。",
            "OK"
        );
    }

    /// <summary>
    /// 背景オブジェクトを作成
    /// </summary>
    private static void CreateBackgroundObject()
    {
        Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{GAME_PATH}/Sprites/work_bg.png");

        GameObject bgObj = new GameObject("Background");
        bgObj.transform.position = Vector3.zero;
        bgObj.transform.localScale = new Vector3(10, 10, 1);
        SpriteRenderer bgRenderer = bgObj.AddComponent<SpriteRenderer>();
        bgRenderer.sortingOrder = -100;
        bgRenderer.sprite = bgSprite;

        Debug.Log("[CakeDecorationSetup] 背景オブジェクトを作成しました");
    }

    /// <summary>
    /// ケーキオブジェクトをシーンに配置（Scene Viewで位置調整可能）
    /// </summary>
    private static void CreateCakeObject()
    {
        Sprite cakeSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{GAME_PATH}/Sprites/work_sprite_a.png");

        GameObject cakeObj = new GameObject("Cake");
        cakeObj.transform.position = new Vector3(0f, -1.0f, 0f);
        cakeObj.transform.localScale = Vector3.one * 2f;

        SpriteRenderer sr = cakeObj.AddComponent<SpriteRenderer>();
        sr.sprite = cakeSprite;
        sr.sortingOrder = 5;

        Debug.Log("[CakeDecorationSetup] ケーキオブジェクトを作成しました");
    }

    /// <summary>
    /// プレースホルダーアセットを自動設定
    /// </summary>
    private static void SetupPlaceholderAssets(CakeDecorationInitializer initializer)
    {
        // スプライト
        Sprite cakeSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{GAME_PATH}/Sprites/work_sprite_a.png");
        Sprite decoSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{GAME_PATH}/Sprites/work_sprite_b.png");

        // 効果音
        AudioClip[] workSfx = new AudioClip[8];
        for (int i = 1; i <= 8; i++)
        {
            workSfx[i - 1] = AssetDatabase.LoadAssetAtPath<AudioClip>($"{GAME_PATH}/Audios/work_sfx{i}.mp3");
        }

        // SerializedObjectで設定
        SerializedObject so = new SerializedObject(initializer);

        // ケーキスプライト
        so.FindProperty("cakeSprite").objectReferenceValue = cakeSprite;

        // 通常デコレーション（全てプレースホルダー）
        so.FindProperty("decoStrawberry").objectReferenceValue = decoSprite;
        so.FindProperty("decoBlueberry").objectReferenceValue = decoSprite;
        so.FindProperty("decoChocolate").objectReferenceValue = decoSprite;
        so.FindProperty("decoCream").objectReferenceValue = decoSprite;
        so.FindProperty("decoCandy").objectReferenceValue = decoSprite;
        so.FindProperty("decoCookie").objectReferenceValue = decoSprite;
        so.FindProperty("decoMacaron").objectReferenceValue = decoSprite;
        so.FindProperty("decoFruit").objectReferenceValue = decoSprite;

        // レアデコレーション
        so.FindProperty("decoGoldenStar").objectReferenceValue = decoSprite;
        so.FindProperty("decoRainbowCandy").objectReferenceValue = decoSprite;

        // 効果音
        so.FindProperty("decorateSound1").objectReferenceValue = workSfx[0];
        so.FindProperty("decorateSound2").objectReferenceValue = workSfx[1];
        so.FindProperty("decorateSound3").objectReferenceValue = workSfx[2];
        so.FindProperty("rareSound").objectReferenceValue = workSfx[3];
        so.FindProperty("celebrationSound").objectReferenceValue = workSfx[4];
        so.FindProperty("bounceSound").objectReferenceValue = workSfx[5];
        so.FindProperty("resetSound").objectReferenceValue = workSfx[6];
        so.FindProperty("sparkleSound").objectReferenceValue = workSfx[7];

        so.ApplyModifiedProperties();

        Debug.Log("[CakeDecorationSetup] プレースホルダーアセットを設定しました");
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
                Debug.Log("[CakeDecorationSetup] シーンは既にBuild Settingsに追加されています");
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

        Debug.Log($"[CakeDecorationSetup] Build Settingsにシーンを追加しました: {scenePath}");
    }
}
#endif
