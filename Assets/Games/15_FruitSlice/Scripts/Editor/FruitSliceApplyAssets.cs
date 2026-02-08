#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// 既存のFruitSliceシーンにアセットを再適用するエディタスクリプト
/// Tools → Apply FruitSlice Assets から実行
/// </summary>
public static class FruitSliceApplyAssets
{
    private const string GAME_PATH = "Assets/Games/15_FruitSlice";
    private const string SCENE_PATH = "Assets/Games/15_FruitSlice/Scenes/FruitSlice.unity";

    [MenuItem("Tools/Apply FruitSlice Assets")]
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

        // FruitSliceInitializerを検索
        FruitSliceInitializer initializer = Object.FindFirstObjectByType<FruitSliceInitializer>();

        if (initializer == null)
        {
            if (showDialog)
            {
                EditorUtility.DisplayDialog(
                    "エラー",
                    "FruitSliceInitializerが見つかりません。\n" +
                    "Tools → Setup FruitSlice Game を実行してください。",
                    "OK"
                );
            }
            else
            {
                Debug.LogError("[FruitSliceApplyAssets] FruitSliceInitializerが見つかりません");
            }
            return;
        }

        Debug.Log("[FruitSliceApplyAssets] アセットの適用を開始...");

        SetupAssets(initializer);
        ApplyBackgroundSprite();
        UpdateSpawnPointPreviews(initializer);

        // シーンを保存
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log("[FruitSliceApplyAssets] アセットの適用が完了しました！");

        if (showDialog)
        {
            EditorUtility.DisplayDialog(
                "完了",
                "FruitSliceゲームのアセット設定が完了しました！\n\n" +
                "Playボタンで動作確認してください。",
                "OK"
            );
        }
    }

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

        // 通常フルーツ
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
        AudioClip[] workSfx = new AudioClip[8];
        for (int i = 1; i <= 8; i++)
        {
            workSfx[i - 1] = AssetDatabase.LoadAssetAtPath<AudioClip>($"{GAME_PATH}/Audios/work_sfx{i}.mp3");
        }

        so.FindProperty("cutSound").objectReferenceValue = workSfx[0];
        so.FindProperty("plateSound").objectReferenceValue = workSfx[1];
        so.FindProperty("completeSound").objectReferenceValue = workSfx[2];
        so.FindProperty("rareSound").objectReferenceValue = workSfx[3];
        so.FindProperty("spawnSound").objectReferenceValue = workSfx[4];

        so.ApplyModifiedProperties();
    }

    private static void SetFruitSpritesFromPath(SerializedObject so, string propertyName)
    {
        SetFruitSpritesFromPath(so, propertyName, propertyName);
    }

    private static void SetFruitSpritesFromPath(SerializedObject so, string propertyName, string fileName)
    {
        Sprite whole = AssetDatabase.LoadAssetAtPath<Sprite>($"{GAME_PATH}/Sprites/{fileName}_whole.png");
        Sprite cut = AssetDatabase.LoadAssetAtPath<Sprite>($"{GAME_PATH}/Sprites/{fileName}_cut.png");
        Sprite plated = AssetDatabase.LoadAssetAtPath<Sprite>($"{GAME_PATH}/Sprites/{fileName}_plated.png");

        // フォールバック
        if (whole == null) whole = AssetDatabase.LoadAssetAtPath<Sprite>($"{GAME_PATH}/Sprites/work_sprite_a.png");
        if (cut == null) cut = AssetDatabase.LoadAssetAtPath<Sprite>($"{GAME_PATH}/Sprites/work_sprite_b.png");
        if (plated == null) plated = AssetDatabase.LoadAssetAtPath<Sprite>($"{GAME_PATH}/Sprites/work_sprite_a.png");

        so.FindProperty($"{propertyName}Whole").objectReferenceValue = whole;
        so.FindProperty($"{propertyName}Cut").objectReferenceValue = cut;
        so.FindProperty($"{propertyName}Plated").objectReferenceValue = plated;

        if (whole == null || cut == null || plated == null)
        {
            Debug.LogWarning($"[FruitSliceApplyAssets] {propertyName}のスプライトが一部見つかりません");
        }
    }

    /// <summary>
    /// シーン中の背景オブジェクトにスプライトを設定
    /// </summary>
    private static void ApplyBackgroundSprite()
    {
        Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{GAME_PATH}/Sprites/bg_cutting_board.png");
        if (bgSprite == null)
        {
            bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{GAME_PATH}/Sprites/work_bg.png");
        }

        GameObject bgObj = GameObject.Find("Background_CuttingBoard");
        if (bgObj != null)
        {
            SpriteRenderer sr = bgObj.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sprite = bgSprite;
            }
        }
        else
        {
            Debug.LogWarning("[FruitSliceApplyAssets] Background_CuttingBoardが見つかりません");
        }
    }

    /// <summary>
    /// 既存のスポーンポイントのプレビュースプライトを更新
    /// </summary>
    private static void UpdateSpawnPointPreviews(FruitSliceInitializer initializer)
    {
        string[] previewFruits = { "apple", "orange", "peach", "pineapple", "watermelon" };

        var spawnPoints = Object.FindObjectsByType<FruitSpawnPoint>(FindObjectsSortMode.None);
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            string fruitName = previewFruits[i % previewFruits.Length];
            Sprite previewSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{GAME_PATH}/Sprites/{fruitName}_whole.png");
            if (previewSprite == null)
            {
                previewSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{GAME_PATH}/Sprites/work_sprite_a.png");
            }

            SerializedObject spSO = new SerializedObject(spawnPoints[i]);
            spSO.FindProperty("previewSprite").objectReferenceValue = previewSprite;
            spSO.ApplyModifiedProperties();
        }

        // InitializerのspawnPointsリストも更新
        SerializedObject initSO = new SerializedObject(initializer);
        SerializedProperty spawnPointsProp = initSO.FindProperty("spawnPoints");
        spawnPointsProp.ClearArray();

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            spawnPointsProp.InsertArrayElementAtIndex(i);
            spawnPointsProp.GetArrayElementAtIndex(i).objectReferenceValue = spawnPoints[i];
        }

        initSO.ApplyModifiedProperties();
        Debug.Log($"[FruitSliceApplyAssets] {spawnPoints.Length}個のスポーンポイントを更新しました");
    }
}
#endif
