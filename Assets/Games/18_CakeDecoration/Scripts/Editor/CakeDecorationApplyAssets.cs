#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// 既存のCakeDecorationシーンにアセットを適用するエディタスクリプト
/// Tools → Apply CakeDecoration Assets から実行
/// </summary>
public static class CakeDecorationApplyAssets
{
    private const string GAME_PATH = "Assets/Games/18_CakeDecoration";
    private const string SCENE_PATH = "Assets/Games/18_CakeDecoration/Scenes/CakeDecoration.unity";

    [MenuItem("Tools/Apply CakeDecoration Assets")]
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
        CakeDecorationInitializer initializer = Object.FindFirstObjectByType<CakeDecorationInitializer>();

        if (initializer == null)
        {
            if (showDialog)
            {
                EditorUtility.DisplayDialog(
                    "エラー",
                    "CakeDecorationInitializerが見つかりません。\n" +
                    "Tools → Setup CakeDecoration Game を実行してください。",
                    "OK"
                );
            }
            else
            {
                Debug.LogError("[CakeDecorationApplyAssets] CakeDecorationInitializerが見つかりません");
            }
            return;
        }

        SerializedObject so = new SerializedObject(initializer);

        // === スプライトの適用 ===
        // ケーキスプライト: 本番 → プレースホルダー
        ApplySprite(so, "cakeSprite", "cake", "work_sprite_a");

        // 通常デコレーション
        ApplySprite(so, "decoStrawberry", "deco_strawberry", "work_sprite_b");
        ApplySprite(so, "decoBlueberry", "deco_blueberry", "work_sprite_b");
        ApplySprite(so, "decoChocolate", "deco_chocolate", "work_sprite_b");
        ApplySprite(so, "decoCream", "deco_cream", "work_sprite_b");
        ApplySprite(so, "decoCandy", "deco_candy", "work_sprite_b");
        ApplySprite(so, "decoCookie", "deco_cookie", "work_sprite_b");
        ApplySprite(so, "decoMacaron", "deco_macaron", "work_sprite_b");
        ApplySprite(so, "decoFruit", "deco_fruit", "work_sprite_b");

        // レアデコレーション
        ApplySprite(so, "decoGoldenStar", "deco_golden_star", "work_sprite_b");
        ApplySprite(so, "decoRainbowCandy", "deco_rainbow_candy", "work_sprite_b");

        // === 効果音の適用 ===
        ApplyAudio(so, "decorateSound1", "decorate1", "work_sfx1");
        ApplyAudio(so, "decorateSound2", "decorate2", "work_sfx2");
        ApplyAudio(so, "decorateSound3", "decorate3", "work_sfx3");
        ApplyAudio(so, "rareSound", "rare", "work_sfx4");
        ApplyAudio(so, "celebrationSound", "celebration", "work_sfx5");
        ApplyAudio(so, "bounceSound", "bounce", "work_sfx6");
        ApplyAudio(so, "resetSound", "reset", "work_sfx7");
        ApplyAudio(so, "sparkleSound", "sparkle", "work_sfx8");

        so.ApplyModifiedProperties();

        // === 背景オブジェクトの更新 ===
        UpdateBackground();

        // === ケーキオブジェクトの更新 ===
        UpdateCakeObject(so);

        // シーンを保存
        EditorSceneManager.SaveScene(scene);

        if (showDialog)
        {
            EditorUtility.DisplayDialog(
                "適用完了",
                "CakeDecorationのアセットを適用しました。",
                "OK"
            );
        }

        Debug.Log("[CakeDecorationApplyAssets] アセット適用完了");
    }

    /// <summary>
    /// スプライトを適用（本番ファイルがあればそちら、なければフォールバック）
    /// </summary>
    private static void ApplySprite(SerializedObject so, string propertyName,
                                     string productionName, string fallbackName)
    {
        // 本番スプライト（png / jpg）を検索
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{GAME_PATH}/Sprites/{productionName}.png");
        if (sprite == null)
        {
            sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{GAME_PATH}/Sprites/{productionName}.jpg");
        }

        // フォールバック
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
        // 本番音声を検索
        AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>($"{GAME_PATH}/Audios/{productionName}.mp3");
        if (clip == null)
        {
            clip = AssetDatabase.LoadAssetAtPath<AudioClip>($"{GAME_PATH}/Audios/{productionName}.wav");
        }

        // フォールバック
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
            // 背景がなければ作成
            bgObj = new GameObject("Background");
            bgObj.transform.position = Vector3.zero;
            bgObj.transform.localScale = new Vector3(10, 10, 1);
            SpriteRenderer sr = bgObj.AddComponent<SpriteRenderer>();
            sr.sortingOrder = -100;
        }

        SpriteRenderer bgRenderer = bgObj.GetComponent<SpriteRenderer>();
        if (bgRenderer != null)
        {
            // 本番背景を検索
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

        Debug.Log("[CakeDecorationApplyAssets] 背景を更新しました");
    }

    /// <summary>
    /// ケーキオブジェクトを更新
    /// </summary>
    private static void UpdateCakeObject(SerializedObject so)
    {
        GameObject cakeObj = GameObject.Find("Cake");
        if (cakeObj == null) return;

        SpriteRenderer sr = cakeObj.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            // 本番ケーキスプライトを検索
            Sprite cakeSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{GAME_PATH}/Sprites/cake.png");
            if (cakeSprite == null)
            {
                cakeSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{GAME_PATH}/Sprites/cake.jpg");
            }
            if (cakeSprite == null)
            {
                cakeSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{GAME_PATH}/Sprites/work_sprite_a.png");
            }
            sr.sprite = cakeSprite;
        }

        Debug.Log("[CakeDecorationApplyAssets] ケーキオブジェクトを更新しました");
    }
}
#endif
