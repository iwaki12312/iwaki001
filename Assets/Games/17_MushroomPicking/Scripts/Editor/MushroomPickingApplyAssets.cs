#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// 既存のMushroomPickingシーンにアセットを適用するエディタスクリプト
/// Tools → Apply MushroomPicking Assets から実行
/// </summary>
public static class MushroomPickingApplyAssets
{
    private const string GAME_PATH = "Assets/Games/17_MushroomPicking";
    private const string SCENE_PATH = "Assets/Games/17_MushroomPicking/Scenes/MushroomPicking.unity";

    [MenuItem("Tools/Apply MushroomPicking Assets")]
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
        MushroomPickingInitializer initializer = Object.FindFirstObjectByType<MushroomPickingInitializer>();

        if (initializer == null)
        {
            if (showDialog)
            {
                EditorUtility.DisplayDialog(
                    "エラー",
                    "MushroomPickingInitializerが見つかりません。\n" +
                    "Tools → Setup MushroomPicking Game を実行してください。",
                    "OK"
                );
            }
            else
            {
                Debug.LogError("[MushroomPickingApplyAssets] MushroomPickingInitializerが見つかりません");
            }
            return;
        }

        Debug.Log("[MushroomPickingApplyAssets] アセットの適用を開始...");

        SetupAssets(initializer);

        // シーンを保存
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log("[MushroomPickingApplyAssets] アセットの適用が完了しました！");

        if (showDialog)
        {
            EditorUtility.DisplayDialog(
                "完了",
                "MushroomPickingゲームのアセット設定が完了しました！\n\n" +
                "Playボタンで動作確認してください。",
                "OK"
            );
        }
    }

    private static void SetupAssets(MushroomPickingInitializer initializer)
    {
        // 背景スプライトをロード
        Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{GAME_PATH}/Sprites/bg_forest.png");
        if (bgSprite == null)
        {
            bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{GAME_PATH}/Sprites/bg_forest.jpg");
        }
        if (bgSprite == null)
        {
            bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{GAME_PATH}/Sprites/work_bg.png");
        }

        // 背景オブジェクトにスプライトを設定
        ApplyBackgroundSprite(bgSprite);

        // 仮スプライトをロード（本番がなければ仮を使用）
        Sprite spriteA = AssetDatabase.LoadAssetAtPath<Sprite>($"{GAME_PATH}/Sprites/work_sprite_a.png");
        Sprite spriteB = AssetDatabase.LoadAssetAtPath<Sprite>($"{GAME_PATH}/Sprites/work_sprite_b.png");

        // 仮効果音をロード
        AudioClip[] workSfx = new AudioClip[8];
        for (int i = 1; i <= 8; i++)
        {
            workSfx[i - 1] = AssetDatabase.LoadAssetAtPath<AudioClip>($"{GAME_PATH}/Audios/work_sfx{i}.mp3");
        }

        SerializedObject so = new SerializedObject(initializer);

        // 通常キノコスプライト
        SetSpriteWithFallback(so, "redMushroomSprite", "mushroom_red", spriteA);
        SetSpriteWithFallback(so, "yellowMushroomSprite", "mushroom_yellow", spriteA);
        SetSpriteWithFallback(so, "blueMushroomSprite", "mushroom_blue", spriteA);
        SetSpriteWithFallback(so, "whiteMushroomSprite", "mushroom_white", spriteA);
        SetSpriteWithFallback(so, "greenMushroomSprite", "mushroom_green", spriteA);
        SetSpriteWithFallback(so, "pinkMushroomSprite", "mushroom_pink", spriteA);
        SetSpriteWithFallback(so, "orangeMushroomSprite", "mushroom_orange", spriteA);
        SetSpriteWithFallback(so, "purpleMushroomSprite", "mushroom_purple", spriteA);
        SetSpriteWithFallback(so, "brownMushroomSprite", "mushroom_brown", spriteA);
        SetSpriteWithFallback(so, "skyblueMushroomSprite", "mushroom_skyblue", spriteA);

        // レアキノコスプライト
        SetSpriteWithFallback(so, "goldMushroomSprite", "mushroom_gold", spriteB);
        SetSpriteWithFallback(so, "rainbowMushroomSprite", "mushroom_rainbow", spriteB);
        SetSpriteWithFallback(so, "starMushroomSprite", "mushroom_star", spriteB);
        SetSpriteWithFallback(so, "crystalMushroomSprite", "mushroom_crystal", spriteB);
        SetSpriteWithFallback(so, "cosmicMushroomSprite", "mushroom_cosmic", spriteB);

        // スーパーレアキノコスプライト
        SetSpriteWithFallback(so, "rabbitMushroomSprite", "mushroom_rabbit", spriteB);
        SetSpriteWithFallback(so, "mouseMushroomSprite", "mushroom_mouse", spriteB);
        SetSpriteWithFallback(so, "squirrelMushroomSprite", "mushroom_squirrel", spriteB);

        // カゴスプライト
        SetSpriteWithFallback(so, "basketSprite", "basket", spriteB);

        // 効果音
        so.FindProperty("growSound").objectReferenceValue = LoadAudioOrFallback("sfx_grow", workSfx[0]);
        so.FindProperty("pickSound").objectReferenceValue = LoadAudioOrFallback("sfx_pick", workSfx[1]);
        so.FindProperty("revealSound").objectReferenceValue = LoadAudioOrFallback("sfx_reveal", workSfx[6]);
        so.FindProperty("rarePickSound").objectReferenceValue = LoadAudioOrFallback("sfx_rare_pick", workSfx[2]);
        so.FindProperty("rareRevealSound").objectReferenceValue = LoadAudioOrFallback("sfx_rare_reveal", workSfx[7]);
        so.FindProperty("basketSound").objectReferenceValue = LoadAudioOrFallback("sfx_basket", workSfx[3]);
        so.FindProperty("hideSound").objectReferenceValue = LoadAudioOrFallback("sfx_hide", workSfx[4]);
        so.FindProperty("rareAppearSound").objectReferenceValue = LoadAudioOrFallback("sfx_rare_appear", workSfx[5]);
        so.FindProperty("superRareRevealSound").objectReferenceValue = LoadAudioOrFallback("sfx_super_rare_reveal", null);

        // スーパーレア用パーティクルprefab
        GameObject superRareParticle = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/JMO Assets/Cartoon FX Remaster/CFXR Prefabs/Misc/CFXR Magical Source.prefab");
        if (superRareParticle != null)
        {
            so.FindProperty("superRareParticlePrefab").objectReferenceValue = superRareParticle;
        }

        so.ApplyModifiedProperties();

        // スポーンポイントのプレビューを更新
        UpdateSpawnPointPreviews(initializer);

        Debug.Log("[MushroomPickingApplyAssets] アセット設定完了");
    }

    private static void SetSpriteWithFallback(SerializedObject so, string propertyName,
                                               string spriteName, Sprite fallback)
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{GAME_PATH}/Sprites/{spriteName}.png");
        if (sprite == null)
        {
            sprite = fallback;
        }
        so.FindProperty(propertyName).objectReferenceValue = sprite;
    }

    private static AudioClip LoadAudioOrFallback(string audioName, AudioClip fallback)
    {
        AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>($"{GAME_PATH}/Audios/{audioName}.mp3");
        if (clip != null)
        {
            Debug.Log($"[MushroomPickingApplyAssets] 本番音声を使用: {audioName}.mp3");
            return clip;
        }
        return fallback;
    }

    private static void ApplyBackgroundSprite(Sprite bgSprite)
    {
        GameObject bgObj = GameObject.Find("Background");
        if (bgObj == null)
        {
            Debug.LogWarning("[MushroomPickingApplyAssets] 背景オブジェクトが見つかりません。作成します。");
            bgObj = new GameObject("Background");
            bgObj.transform.position = Vector3.zero;
            bgObj.transform.localScale = new Vector3(10, 10, 1);
            SpriteRenderer sr = bgObj.AddComponent<SpriteRenderer>();
            sr.sortingOrder = -100;
        }

        SpriteRenderer bgRenderer = bgObj.GetComponent<SpriteRenderer>();
        if (bgRenderer != null && bgSprite != null)
        {
            bgRenderer.sprite = bgSprite;
        }

        Debug.Log("[MushroomPickingApplyAssets] 背景スプライトを設定しました");
    }

    private static void UpdateSpawnPointPreviews(MushroomPickingInitializer initializer)
    {
        Sprite previewSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{GAME_PATH}/Sprites/mushroom_red.png");
        if (previewSprite == null)
        {
            previewSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{GAME_PATH}/Sprites/work_sprite_a.png");
        }

        var spawnPoints = Object.FindObjectsByType<MushroomSpawnPoint>(FindObjectsSortMode.None);
        for (int i = 0; i < spawnPoints.Length; i++)
        {
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

        Debug.Log($"[MushroomPickingApplyAssets] {spawnPoints.Length}個のスポーンポイントを更新しました");
    }
}
#endif
