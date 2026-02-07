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
    /// 仮スプライト・SEを自動設定
    /// </summary>
    private static void SetupPlaceholderAssets(AnimalVoiceInitializer initializer)
    {
        // 仮スプライトをロード
        Sprite workBg = AssetDatabase.LoadAssetAtPath<Sprite>($"{GAME_PATH}/Sprites/work_bg.png");
        Sprite workSpriteA = AssetDatabase.LoadAssetAtPath<Sprite>($"{GAME_PATH}/Sprites/work_sprite_a.png");
        Sprite workSpriteB = AssetDatabase.LoadAssetAtPath<Sprite>($"{GAME_PATH}/Sprites/work_sprite_b.png");
        
        // 仮効果音をロード
        AudioClip[] workSfx = new AudioClip[8];
        for (int i = 1; i <= 8; i++)
        {
            workSfx[i - 1] = AssetDatabase.LoadAssetAtPath<AudioClip>($"{GAME_PATH}/Audios/work_sfx{i}.mp3");
        }
        
        // SerializedObjectで設定
        SerializedObject so = new SerializedObject(initializer);
        
        // 背景（3枚とも同じ仮画像を使用）
        so.FindProperty("morningBackground").objectReferenceValue = workBg;
        so.FindProperty("daytimeBackground").objectReferenceValue = workBg;
        so.FindProperty("nightBackground").objectReferenceValue = workBg;
        
        // 朝の動物（仮スプライト）
        SetAnimalSprites(so, "chicken", workSpriteA, workSpriteB);
        SetAnimalSprites(so, "cow", workSpriteA, workSpriteB);
        SetAnimalSprites(so, "horse", workSpriteA, workSpriteB);
        SetAnimalSprites(so, "pig", workSpriteA, workSpriteB);
        SetAnimalSprites(so, "sheep", workSpriteA, workSpriteB);
        SetAnimalSprites(so, "goat", workSpriteA, workSpriteB);
        
        // 昼の動物
        SetAnimalSprites(so, "dog", workSpriteA, workSpriteB);
        SetAnimalSprites(so, "cat", workSpriteA, workSpriteB);
        SetAnimalSprites(so, "elephant", workSpriteA, workSpriteB);
        SetAnimalSprites(so, "lion", workSpriteA, workSpriteB);
        SetAnimalSprites(so, "frog", workSpriteA, workSpriteB);
        SetAnimalSprites(so, "chick", workSpriteA, workSpriteB);
        
        // 夜の動物
        SetAnimalSprites(so, "owl", workSpriteA, workSpriteB);
        SetAnimalSprites(so, "wolf", workSpriteA, workSpriteB);
        SetAnimalSprites(so, "bat", workSpriteA, workSpriteB);
        SetAnimalSprites(so, "hedgehog", workSpriteA, workSpriteB);
        
        // レア動物
        SetAnimalSprites(so, "dinosaur", workSpriteA, workSpriteB);
        SetAnimalSprites(so, "dragon", workSpriteA, workSpriteB);
        SetAnimalSprites(so, "unicorn", workSpriteA, workSpriteB);
        SetAnimalSprites(so, "panda", workSpriteA, workSpriteB);
        
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
        so.FindProperty("hedgehogVoice").objectReferenceValue = workSfx[3];
        
        so.FindProperty("dinosaurVoice").objectReferenceValue = workSfx[6];
        so.FindProperty("dragonVoice").objectReferenceValue = workSfx[6];
        so.FindProperty("unicornVoice").objectReferenceValue = workSfx[7];
        so.FindProperty("pandaVoice").objectReferenceValue = workSfx[7];
        
        // 共通SE
        so.FindProperty("tapSound").objectReferenceValue = workSfx[0];
        so.FindProperty("timeChangeSound").objectReferenceValue = workSfx[7];
        so.FindProperty("rareAppearSound").objectReferenceValue = workSfx[6];
        
        so.ApplyModifiedProperties();
    }
    
    private static void SetAnimalSprites(SerializedObject so, string animalName, Sprite normal, Sprite reaction)
    {
        so.FindProperty($"{animalName}Normal").objectReferenceValue = normal;
        so.FindProperty($"{animalName}Reaction").objectReferenceValue = reaction;
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
