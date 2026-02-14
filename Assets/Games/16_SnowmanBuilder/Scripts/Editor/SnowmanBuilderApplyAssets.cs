#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// 既存のSnowmanBuilderシーンにアセットを再適用するエディタスクリプト
/// Tools → Apply SnowmanBuilder Assets から実行
/// </summary>
public static class SnowmanBuilderApplyAssets
{
    private const string GAME_PATH = "Assets/Games/16_SnowmanBuilder";
    private const string SCENE_PATH = "Assets/Games/16_SnowmanBuilder/Scenes/SnowmanBuilder.unity";

    [MenuItem("Tools/Apply SnowmanBuilder Assets")]
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
        SnowmanBuilderInitializer initializer = Object.FindFirstObjectByType<SnowmanBuilderInitializer>();

        if (initializer == null)
        {
            if (showDialog)
            {
                EditorUtility.DisplayDialog(
                    "エラー",
                    "SnowmanBuilderInitializerが見つかりません。\n" +
                    "Tools → Setup SnowmanBuilder Game を実行してください。",
                    "OK"
                );
            }
            else
            {
                Debug.LogError("[SnowmanBuilderApplyAssets] SnowmanBuilderInitializerが見つかりません");
            }
            return;
        }

        Debug.Log("[SnowmanBuilderApplyAssets] アセットの適用を開始...");

        SetupAssets(initializer);

        // シーンを保存
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log("[SnowmanBuilderApplyAssets] アセットの適用が完了しました！");

        if (showDialog)
        {
            EditorUtility.DisplayDialog(
                "完了",
                "SnowmanBuilderゲームのアセット設定が完了しました！\n\n" +
                "Playボタンで動作確認してください。",
                "OK"
            );
        }
    }

    private static void SetupAssets(SnowmanBuilderInitializer initializer)
    {
        SerializedObject so = new SerializedObject(initializer);

        // === スプライト ===
        SetSpriteProperty(so, "snowballSprite", "snowball.png");

        SetSpriteProperty(so, "snowmanComplete1", "snowman_complete_1.png");
        SetSpriteProperty(so, "snowmanComplete2", "snowman_complete_2.png");
        SetSpriteProperty(so, "snowmanComplete3", "snowman_complete_3.png");
        SetSpriteProperty(so, "snowmanComplete4", "snowman_complete_4.png");
        SetSpriteProperty(so, "snowmanComplete5", "snowman_complete_5.png");

        SetSpriteProperty(so, "snowmanRare1", "snowman_rare_rabbit.png");
        SetSpriteProperty(so, "snowmanRare2", "snowman_rare_cat.png");
        SetSpriteProperty(so, "snowmanRare3", "snowman_rare_bear.png");

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

        Debug.Log("[SnowmanBuilderApplyAssets] 全アセットを設定しました");
    }

    private static Sprite LoadSprite(string fileName)
    {
        return AssetDatabase.LoadAssetAtPath<Sprite>($"{GAME_PATH}/Sprites/{fileName}");
    }

    private static void SetSpriteProperty(SerializedObject so, string propertyName, string fileName)
    {
        Sprite s = LoadSprite(fileName);
        var prop = so.FindProperty(propertyName);
        if (prop != null)
        {
            prop.objectReferenceValue = s;
        }
        if (s == null)
        {
            Debug.LogWarning($"[SnowmanBuilderApplyAssets] スプライトが見つかりません: {fileName}");
        }
    }

    private static void SetAudioProperty(SerializedObject so, string propertyName, string primaryName, string fallbackName)
    {
        AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>($"{GAME_PATH}/Audios/{primaryName}");
        if (clip == null)
        {
            clip = AssetDatabase.LoadAssetAtPath<AudioClip>($"{GAME_PATH}/Audios/{fallbackName}");
        }
        var prop = so.FindProperty(propertyName);
        if (prop != null)
        {
            prop.objectReferenceValue = clip;
        }
    }
}
#endif
