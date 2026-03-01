#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// 既存のAquariumシーンにアセットを適用するエディタスクリプト
/// Tools → Apply Aquarium Assets から実行
/// </summary>
public static class AquariumApplyAssets
{
    private const string GAME_PATH = "Assets/Games/19_Aquarium";
    private const string SCENE_PATH = "Assets/Games/19_Aquarium/Scenes/Aquarium.unity";

    [MenuItem("Tools/Apply Aquarium Assets")]
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

        // Initializerを検索
        AquariumInitializer initializer = Object.FindFirstObjectByType<AquariumInitializer>();

        if (initializer == null)
        {
            if (showDialog)
            {
                EditorUtility.DisplayDialog(
                    "エラー",
                    "AquariumInitializerが見つかりません。\n" +
                    "Tools → Setup Aquarium Game を実行してください。",
                    "OK"
                );
            }
            else
            {
                Debug.LogError("[AquariumApplyAssets] AquariumInitializerが見つかりません");
            }
            return;
        }

        Debug.Log("[AquariumApplyAssets] アセットの適用を開始...");

        SerializedObject so = new SerializedObject(initializer);

        // === スプライトの適用 ===
        // 通常生き物
        ApplySprite(so, "clownfishSprite", "clownfish", "work_sprite_a");
        ApplySprite(so, "angelfishSprite", "angelfish", "work_sprite_a");
        ApplySprite(so, "octopusSprite", "octopus", "work_sprite_a");
        ApplySprite(so, "seaTurtleSprite", "sea_turtle", "work_sprite_a");
        ApplySprite(so, "pufferfishSprite", "pufferfish", "work_sprite_b");
        ApplySprite(so, "dolphinSprite", "dolphin", "work_sprite_b");
        ApplySprite(so, "seahorseSprite", "seahorse", "work_sprite_b");
        ApplySprite(so, "sharkSprite", "shark", "work_sprite_b");

        // レア生き物
        ApplySprite(so, "whaleSharkSprite", "whale_shark", "work_sprite_a");
        ApplySprite(so, "mantaSprite", "manta", "work_sprite_b");

        // === 効果音の適用 ===
        ApplyAudio(so, "bubbleSound", "bubble", "work_sfx1");
        ApplyAudio(so, "spawnSound", "spawn", "work_sfx2");
        ApplyAudio(so, "tapSound1", "tap1", "work_sfx3");
        ApplyAudio(so, "tapSound2", "tap2", "work_sfx4");
        ApplyAudio(so, "rareSound", "rare", "work_sfx5");
        ApplyAudio(so, "completeSound", "complete", "work_sfx6");
        ApplyAudio(so, "resetSound", "reset", "work_sfx7");
        ApplyAudio(so, "ambientSound", "ambient", "work_sfx8");

        so.ApplyModifiedProperties();

        // === 背景オブジェクトの更新 ===
        UpdateBackground();

        // シーンを保存
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log("[AquariumApplyAssets] アセットの適用が完了しました！");

        if (showDialog)
        {
            EditorUtility.DisplayDialog(
                "適用完了",
                "Aquariumのアセットを適用しました。\n\nPlayボタンで動作確認してください。",
                "OK"
            );
        }
    }

    /// <summary>
    /// スプライトを適用（本番ファイルがあればそちら、なければフォールバック）
    /// </summary>
    private static void ApplySprite(SerializedObject so, string propertyName,
                                     string productionName, string fallbackName)
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{GAME_PATH}/Sprites/{productionName}.png");
        if (sprite == null)
        {
            sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{GAME_PATH}/Sprites/{productionName}.jpg");
        }
        if (sprite == null)
        {
            sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{GAME_PATH}/Sprites/{fallbackName}.png");
        }

        SerializedProperty prop = so.FindProperty(propertyName);
        if (prop != null)
        {
            prop.objectReferenceValue = sprite;
        }
    }

    /// <summary>
    /// オーディオクリップを適用
    /// </summary>
    private static void ApplyAudio(SerializedObject so, string propertyName,
                                    string productionName, string fallbackName)
    {
        AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>($"{GAME_PATH}/Audios/{productionName}.mp3");
        if (clip == null)
        {
            clip = AssetDatabase.LoadAssetAtPath<AudioClip>($"{GAME_PATH}/Audios/{productionName}.wav");
        }
        if (clip == null)
        {
            clip = AssetDatabase.LoadAssetAtPath<AudioClip>($"{GAME_PATH}/Audios/{fallbackName}.mp3");
        }

        SerializedProperty prop = so.FindProperty(propertyName);
        if (prop != null)
        {
            prop.objectReferenceValue = clip;
        }
    }

    /// <summary>
    /// 背景オブジェクトを更新
    /// </summary>
    private static void UpdateBackground()
    {
        GameObject bgObj = GameObject.Find("Background");
        if (bgObj == null)
        {
            bgObj = new GameObject("Background");
            bgObj.transform.position = Vector3.zero;
            bgObj.transform.localScale = new Vector3(10, 10, 1);
            SpriteRenderer sr = bgObj.AddComponent<SpriteRenderer>();
            sr.sortingOrder = -100;
        }

        SpriteRenderer bgRenderer = bgObj.GetComponent<SpriteRenderer>();
        if (bgRenderer != null)
        {
            Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{GAME_PATH}/Sprites/bg.png");
            if (bgSprite == null)
            {
                bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{GAME_PATH}/Sprites/bg.jpg");
            }
            if (bgSprite == null)
            {
                bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{GAME_PATH}/Sprites/work_bg.png");
            }
            bgRenderer.sprite = bgSprite;
        }

        Debug.Log("[AquariumApplyAssets] 背景を更新しました");
    }
}
#endif
