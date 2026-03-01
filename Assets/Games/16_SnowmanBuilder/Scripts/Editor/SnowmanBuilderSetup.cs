#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

/// <summary>
/// SnowmanBuilderゲームのシーンセットアップを行うエディタ拡張
/// Tools → Setup SnowmanBuilder Game から実行
/// </summary>
public static class SnowmanBuilderSetup
{
    private const string GAME_PATH = "Assets/Games/16_SnowmanBuilder";
    private const string SCENE_NAME = "SnowmanBuilder";

    [MenuItem("Tools/Setup SnowmanBuilder Game")]
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
            mainCamera.backgroundColor = new Color(0.78f, 0.88f, 0.97f); // 冬空の青白
            mainCamera.transform.position = new Vector3(0, 0, -10);
        }

        // Initializerオブジェクトを作成
        GameObject initializerObj = new GameObject("SnowmanBuilderInitializer");
        SnowmanBuilderInitializer initializer = initializerObj.AddComponent<SnowmanBuilderInitializer>();

        // 背景オブジェクトを作成
        CreateBackgroundObject();

        // アセットを自動設定
        SetupAssets(initializer);

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

        Debug.Log($"[SnowmanBuilderSetup] シーンを作成しました: {scenePath}");

        EditorUtility.DisplayDialog(
            "セットアップ完了",
            $"SnowmanBuilderゲームのセットアップが完了しました！\n\n" +
            $"シーン: {scenePath}\n\n" +
            "Playボタンで動作確認できます。",
            "OK"
        );
    }

    private static void CreateBackgroundObject()
    {
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.position = Vector3.zero;
        bgObj.transform.localScale = new Vector3(10, 10, 1);
        SpriteRenderer sr = bgObj.AddComponent<SpriteRenderer>();
        sr.sortingOrder = -100;

        // 背景スプライトをロード
        Sprite bgSprite = LoadSprite("bg_snow.png");
        if (bgSprite == null)
            bgSprite = LoadSprite("bg_snow.jpg");
        if (bgSprite != null)
        {
            sr.sprite = bgSprite;
        }
        else
        {
            // 仮の背景色
            sr.color = new Color(0.85f, 0.92f, 1f);
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);
        }

        Debug.Log("[SnowmanBuilderSetup] 背景オブジェクトを作成しました");
    }

    private static void SetupAssets(SnowmanBuilderInitializer initializer)
    {
        SerializedObject so = new SerializedObject(initializer);

        // === スプライト ===
        // 雪玉
        SetSpriteProperty(so, "snowballSprite", "snowball.png");

        // 通常完成バリエーション
        SetSpriteProperty(so, "snowmanComplete1", "snowman_complete_1.png");
        SetSpriteProperty(so, "snowmanComplete2", "snowman_complete_2.png");
        SetSpriteProperty(so, "snowmanComplete3", "snowman_complete_3.png");
        SetSpriteProperty(so, "snowmanComplete4", "snowman_complete_4.png");
        SetSpriteProperty(so, "snowmanComplete5", "snowman_complete_5.png");

        // レアバリエーション
        SetSpriteProperty(so, "snowmanRare1", "snowman_rare_rabbit.png");
        SetSpriteProperty(so, "snowmanRare2", "snowman_rare_cat.png");
        SetSpriteProperty(so, "snowmanRare3", "snowman_rare_bear.png");

        // 背景
        Sprite bgSprite = LoadSprite("bg_snow.png");
        if (bgSprite == null) bgSprite = LoadSprite("bg_snow.jpg");
        so.FindProperty("backgroundSprite").objectReferenceValue = bgSprite;

        // === 効果音 ===
        SetAudioProperty(so, "snowballAppearSound", "snowball_appear.mp3", "work_sfx1.mp3");
        SetAudioProperty(so, "snowballStackSound", "snowball_stack.mp3", "work_sfx2.mp3");
        SetAudioProperty(so, "completeSound", "complete.mp3", "work_sfx3.mp3");
        SetAudioProperty(so, "rareCompleteSound", "rare_complete.mp3", "work_sfx4.mp3");
        SetAudioProperty(so, "fadeOutSound", "fadeout.mp3", "work_sfx5.mp3");

        so.ApplyModifiedProperties();

        // 背景オブジェクトにもスプライトを設定
        GameObject bgObj = GameObject.Find("Background");
        if (bgObj != null && bgSprite != null)
        {
            SpriteRenderer sr = bgObj.GetComponent<SpriteRenderer>();
            if (sr != null) sr.sprite = bgSprite;
        }

        Debug.Log("[SnowmanBuilderSetup] アセットを設定しました");
    }

    private static Sprite LoadSprite(string fileName)
    {
        Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>($"{GAME_PATH}/Sprites/{fileName}");
        return s;
    }

    private static void SetSpriteProperty(SerializedObject so, string propertyName, string fileName)
    {
        Sprite s = LoadSprite(fileName);
        so.FindProperty(propertyName).objectReferenceValue = s;
    }

    private static void SetAudioProperty(SerializedObject so, string propertyName, string primaryName, string fallbackName)
    {
        AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>($"{GAME_PATH}/Audios/{primaryName}");
        if (clip == null)
        {
            clip = AssetDatabase.LoadAssetAtPath<AudioClip>($"{GAME_PATH}/Audios/{fallbackName}");
        }
        so.FindProperty(propertyName).objectReferenceValue = clip;
    }

    private static void AddSceneToBuildSettings()
    {
        string scenePath = $"{GAME_PATH}/Scenes/{SCENE_NAME}.unity";

        var scenes = EditorBuildSettings.scenes;
        foreach (var scene in scenes)
        {
            if (scene.path == scenePath)
            {
                Debug.Log("[SnowmanBuilderSetup] シーンは既にBuild Settingsに追加されています");
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

        Debug.Log($"[SnowmanBuilderSetup] Build Settingsにシーンを追加しました: {scenePath}");
    }
}
#endif
