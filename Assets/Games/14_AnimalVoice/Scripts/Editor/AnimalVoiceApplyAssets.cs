#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// 既存のAnimalVoiceシーンにアセットを適用するエディタスクリプト
/// Tools → Apply AnimalVoice Assets から実行
/// </summary>
public static class AnimalVoiceApplyAssets
{
    private const string GAME_PATH = "Assets/Games/14_AnimalVoice";
    private const string SCENE_PATH = "Assets/Games/14_AnimalVoice/Scenes/AnimalVoice.unity";

    [MenuItem("Tools/Apply AnimalVoice Assets")]
    public static void ApplyAssets()
    {
        ApplyAssetsInternal(showDialog: true);
    }
    
    /// <summary>
    /// ダイアログなしでアセットを適用（自動テスト用）
    /// </summary>
    public static void ApplyAssetsSilent()
    {
        ApplyAssetsInternal(showDialog: false);
    }

    private static void ApplyAssetsInternal(bool showDialog)
    {
        // シーンを開く
        var scene = EditorSceneManager.OpenScene(SCENE_PATH);

        // AnimalVoiceInitializerを検索
        AnimalVoiceInitializer initializer = Object.FindFirstObjectByType<AnimalVoiceInitializer>();

        if (initializer == null)
        {
            if (showDialog)
            {
                EditorUtility.DisplayDialog(
                    "エラー",
                    "AnimalVoiceInitializerが見つかりません。\n" +
                    "Tools → Setup AnimalVoice Game を実行してください。",
                    "OK"
                );
            }
            else
            {
                Debug.LogError("[AnimalVoiceApplyAssets] AnimalVoiceInitializerが見つかりません");
            }
            return;
        }

        Debug.Log("[AnimalVoiceApplyAssets] アセットの適用を開始...");

        // アセットを設定
        SetupAssets(initializer);

        // シーンを保存
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log("[AnimalVoiceApplyAssets] アセットの適用が完了しました！");

        if (showDialog)
        {
            EditorUtility.DisplayDialog(
                "完了",
                "AnimalVoiceゲームのアセット設定が完了しました！\n\n" +
                "Playボタンで動作確認してください。",
                "OK"
            );
        }
    }

    private static void SetupAssets(AnimalVoiceInitializer initializer)
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
        so.FindProperty("hedgehogVoice").objectReferenceValue = workSfx[3];

        so.FindProperty("dinosaurVoice").objectReferenceValue = workSfx[6];
        so.FindProperty("dragonVoice").objectReferenceValue = workSfx[6];
        so.FindProperty("unicornVoice").objectReferenceValue = workSfx[7];
        so.FindProperty("pandaVoice").objectReferenceValue = workSfx[7];

        // 共通SE
        so.FindProperty("tapSound").objectReferenceValue = workSfx[0];
        so.FindProperty("timeChangeSound").objectReferenceValue = workSfx[7];
        so.FindProperty("rareAppearSound").objectReferenceValue = workSfx[6];

        // パーティクルプレハブ
        so.FindProperty("heartParticlePrefab").objectReferenceValue = heartParticle;
        so.FindProperty("noteParticlePrefab").objectReferenceValue = noteParticle;

        so.ApplyModifiedProperties();

        // 既存のスポーンポイントのプレビュースプライトを更新
        UpdateSpawnPointPreviews(initializer);

        // ログ出力
        Debug.Log($"[AnimalVoiceApplyAssets] 背景スプライト設定: Morning={bgMorning != null}, Day={bgDaytime != null}, Night={bgNight != null}");
        Debug.Log($"[AnimalVoiceApplyAssets] パーティクル設定: Heart={heartParticle != null}, Note={noteParticle != null}");
    }

    private static void SetAnimalSpritesFromPath(SerializedObject so, string animalName)
    {
        Sprite normal = AssetDatabase.LoadAssetAtPath<Sprite>($"{GAME_PATH}/Sprites/{animalName}_normal.png");
        Sprite reaction = AssetDatabase.LoadAssetAtPath<Sprite>($"{GAME_PATH}/Sprites/{animalName}_reaction.png");

        so.FindProperty($"{animalName}Normal").objectReferenceValue = normal;
        so.FindProperty($"{animalName}Reaction").objectReferenceValue = reaction;

        if (normal == null || reaction == null)
        {
            Debug.LogWarning($"[AnimalVoiceApplyAssets] {animalName}のスプライトが見つかりません: normal={normal != null}, reaction={reaction != null}");
        }
    }

    /// <summary>
    /// 既存のスポーンポイントのプレビュースプライトを更新
    /// </summary>
    private static void UpdateSpawnPointPreviews(AnimalVoiceInitializer initializer)
    {
        string[] previewAnimals = { "chicken", "cow", "horse", "pig", "sheep", "goat" };

        var spawnPoints = Object.FindObjectsByType<AnimalSpawnPoint>(FindObjectsSortMode.None);
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            string animalName = previewAnimals[i % previewAnimals.Length];
            Sprite previewSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{GAME_PATH}/Sprites/{animalName}_normal.png");

            SerializedObject spSO = new SerializedObject(spawnPoints[i]);
            spSO.FindProperty("previewSprite").objectReferenceValue = previewSprite;
            spSO.ApplyModifiedProperties();
        }

        // Initializerのスポーンポイントリストも更新
        SerializedObject initSO = new SerializedObject(initializer);
        SerializedProperty spawnPointsProp = initSO.FindProperty("spawnPoints");
        spawnPointsProp.ClearArray();

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            spawnPointsProp.InsertArrayElementAtIndex(i);
            spawnPointsProp.GetArrayElementAtIndex(i).objectReferenceValue = spawnPoints[i];
        }

        initSO.ApplyModifiedProperties();

        Debug.Log($"[AnimalVoiceApplyAssets] {spawnPoints.Length}個のスポーンポイントを更新しました");
    }
}
#endif
