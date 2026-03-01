#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

/// <summary>
/// Aquariumゲームのシーンセットアップを行うエディタ拡張
/// Tools → Setup Aquarium Game から実行
/// </summary>
public static class AquariumSetup
{
    private const string GAME_PATH = "Assets/Games/19_Aquarium";
    private const string SCENE_NAME = "Aquarium";

    [MenuItem("Tools/Setup Aquarium Game")]
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
            mainCamera.backgroundColor = new Color(0.1f, 0.3f, 0.6f); // 深い海色
            mainCamera.transform.position = new Vector3(0, 0, -10);
        }

        // Initializerオブジェクトを作成
        GameObject initializerObj = new GameObject("AquariumInitializer");
        AquariumInitializer initializer = initializerObj.AddComponent<AquariumInitializer>();

        // 背景オブジェクトを作成
        CreateBackgroundObject();

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

        Debug.Log($"[AquariumSetup] シーンを作成しました: {scenePath}");

        EditorUtility.DisplayDialog(
            "セットアップ完了",
            $"Aquariumゲームのセットアップが完了しました！\n\n" +
            $"シーン: {scenePath}\n\n" +
            "Playボタンで動作確認できます（仮アセット使用）",
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

        Debug.Log("[AquariumSetup] 背景オブジェクトを作成しました");
    }

    /// <summary>
    /// プレースホルダーアセットを自動設定
    /// </summary>
    private static void SetupPlaceholderAssets(AquariumInitializer initializer)
    {
        Sprite spriteA = AssetDatabase.LoadAssetAtPath<Sprite>($"{GAME_PATH}/Sprites/work_sprite_a.png");
        Sprite spriteB = AssetDatabase.LoadAssetAtPath<Sprite>($"{GAME_PATH}/Sprites/work_sprite_b.png");

        AudioClip[] workSfx = new AudioClip[8];
        for (int i = 1; i <= 8; i++)
        {
            workSfx[i - 1] = AssetDatabase.LoadAssetAtPath<AudioClip>($"{GAME_PATH}/Audios/work_sfx{i}.mp3");
        }

        SerializedObject so = new SerializedObject(initializer);

        // 通常生き物（プレースホルダー）
        so.FindProperty("clownfishSprite").objectReferenceValue = spriteA;
        so.FindProperty("angelfishSprite").objectReferenceValue = spriteA;
        so.FindProperty("octopusSprite").objectReferenceValue = spriteA;
        so.FindProperty("seaTurtleSprite").objectReferenceValue = spriteA;
        so.FindProperty("pufferfishSprite").objectReferenceValue = spriteB;
        so.FindProperty("jellyfishSprite").objectReferenceValue = spriteB;
        so.FindProperty("seahorseSprite").objectReferenceValue = spriteB;
        so.FindProperty("starfishSprite").objectReferenceValue = spriteB;

        // レア生き物
        so.FindProperty("whaleSharkSprite").objectReferenceValue = spriteA;
        so.FindProperty("mantaSprite").objectReferenceValue = spriteB;

        // 効果音
        so.FindProperty("bubbleSound").objectReferenceValue = workSfx[0];
        so.FindProperty("spawnSound").objectReferenceValue = workSfx[1];
        so.FindProperty("tapSound1").objectReferenceValue = workSfx[2];
        so.FindProperty("tapSound2").objectReferenceValue = workSfx[3];
        so.FindProperty("rareSound").objectReferenceValue = workSfx[4];
        so.FindProperty("completeSound").objectReferenceValue = workSfx[5];
        so.FindProperty("resetSound").objectReferenceValue = workSfx[6];
        so.FindProperty("ambientSound").objectReferenceValue = workSfx[7];

        so.ApplyModifiedProperties();

        Debug.Log("[AquariumSetup] プレースホルダーアセットを設定しました");
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
                Debug.Log("[AquariumSetup] シーンは既にBuild Settingsに追加されています");
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

        Debug.Log($"[AquariumSetup] Build Settingsにシーンを追加しました: {scenePath}");
    }
}
#endif
