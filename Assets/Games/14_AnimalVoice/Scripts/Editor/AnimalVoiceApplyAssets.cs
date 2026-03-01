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
        // 背景スプライトをロード（jpg）
        Sprite bgMorning = AssetDatabase.LoadAssetAtPath<Sprite>($"{GAME_PATH}/Sprites/bg_morning.jpg");
        Sprite bgDaytime = AssetDatabase.LoadAssetAtPath<Sprite>($"{GAME_PATH}/Sprites/bg_daytime.jpg");
        Sprite bgNight = AssetDatabase.LoadAssetAtPath<Sprite>($"{GAME_PATH}/Sprites/bg_night.jpg");
        
        // 背景オブジェクトにスプライトを設定
        ApplyBackgroundSprites(bgMorning, bgDaytime, bgNight);

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

        // 鳴き声（本番があれば使用、なければ仮SE）
        // 朝の動物
        so.FindProperty("chickenVoice").objectReferenceValue = LoadVoiceOrFallback("chicken_voice", workSfx[0]);
        so.FindProperty("cowVoice").objectReferenceValue = LoadVoiceOrFallback("cow_voice", workSfx[1]);
        so.FindProperty("horseVoice").objectReferenceValue = LoadVoiceOrFallback("horse_voice", workSfx[2]);
        so.FindProperty("pigVoice").objectReferenceValue = LoadVoiceOrFallback("pig_voice", workSfx[3]);
        so.FindProperty("sheepVoice").objectReferenceValue = LoadVoiceOrFallback("sheep_voice", workSfx[4]);
        so.FindProperty("goatVoice").objectReferenceValue = LoadVoiceOrFallback("goat_voice", workSfx[5]);

        // 昼の動物
        so.FindProperty("dogVoice").objectReferenceValue = LoadVoiceOrFallback("dog_voice", workSfx[0]);
        so.FindProperty("catVoice").objectReferenceValue = LoadVoiceOrFallback("cat_voice", workSfx[1]);
        so.FindProperty("elephantVoice").objectReferenceValue = LoadVoiceOrFallback("elephant_voice", workSfx[2]);
        so.FindProperty("lionVoice").objectReferenceValue = LoadVoiceOrFallback("lion_voice", workSfx[3]);
        so.FindProperty("frogVoice").objectReferenceValue = LoadVoiceOrFallback("frog_voice", workSfx[4]);
        so.FindProperty("chickVoice").objectReferenceValue = LoadVoiceOrFallback("chick_voice", workSfx[5]);

        // 夜の動物
        so.FindProperty("owlVoice").objectReferenceValue = LoadVoiceOrFallback("owl_voice", workSfx[0]);
        so.FindProperty("wolfVoice").objectReferenceValue = LoadVoiceOrFallback("wolf_voice", workSfx[1]);
        so.FindProperty("batVoice").objectReferenceValue = LoadVoiceOrFallback("bat_voice", workSfx[2]);
        so.FindProperty("mouseVoice").objectReferenceValue = LoadVoiceOrFallback("mouse_voice", workSfx[3]);

        // レア動物
        so.FindProperty("dinosaurVoice").objectReferenceValue = LoadVoiceOrFallback("dinosaur_voice", workSfx[6]);
        so.FindProperty("dragonVoice").objectReferenceValue = LoadVoiceOrFallback("dragon_voice", workSfx[6]);
        so.FindProperty("unicornVoice").objectReferenceValue = LoadVoiceOrFallback("unicorn_voice", workSfx[7]);
        so.FindProperty("monkeyVoice").objectReferenceValue = LoadVoiceOrFallback("monkey_voice", workSfx[7]);

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
    /// 本番の鳴き声ファイルがあればそれを使用、なければフォールバック
    /// </summary>
    private static AudioClip LoadVoiceOrFallback(string voiceName, AudioClip fallback)
    {
        AudioClip voice = AssetDatabase.LoadAssetAtPath<AudioClip>($"{GAME_PATH}/Audios/{voiceName}.mp3");
        if (voice != null)
        {
            Debug.Log($"[AnimalVoiceApplyAssets] 本番鳴き声を使用: {voiceName}.mp3");
            return voice;
        }
        return fallback;
    }

    /// <summary>
    /// 背景オブジェクトのSpriteRendererにスプライトを設定
    /// </summary>
    private static void ApplyBackgroundSprites(Sprite morning, Sprite daytime, Sprite night)
    {
        GameObject bgRoot = GameObject.Find("Background");
        if (bgRoot == null)
        {
            Debug.LogWarning("[AnimalVoiceApplyAssets] 背景オブジェクトが見つかりません。作成します。");
            CreateBackgroundObjects();
            bgRoot = GameObject.Find("Background");
            if (bgRoot == null)
            {
                Debug.LogError("[AnimalVoiceApplyAssets] 背景オブジェクトの作成に失敗しました");
                return;
            }
        }
        
        Transform morningTransform = bgRoot.transform.Find("Background_Morning");
        Transform daytimeTransform = bgRoot.transform.Find("Background_Daytime");
        Transform nightTransform = bgRoot.transform.Find("Background_Night");
        
        if (morningTransform != null)
        {
            SpriteRenderer sr = morningTransform.GetComponent<SpriteRenderer>();
            if (sr != null) sr.sprite = morning;
        }
        
        if (daytimeTransform != null)
        {
            SpriteRenderer sr = daytimeTransform.GetComponent<SpriteRenderer>();
            if (sr != null) sr.sprite = daytime;
        }
        
        if (nightTransform != null)
        {
            SpriteRenderer sr = nightTransform.GetComponent<SpriteRenderer>();
            if (sr != null) sr.sprite = night;
        }
        
        Debug.Log("[AnimalVoiceApplyAssets] 背景スプライトを設定しました");
    }
    
    /// <summary>
    /// 背景オブジェクトを作成（Setup.csと同じ実装）
    /// </summary>
    private static void CreateBackgroundObjects()
    {
        // ルートオブジェクト
        GameObject bgRoot = new GameObject("Background");
        
        // 朝の背景
        GameObject morningObj = new GameObject("Background_Morning");
        morningObj.transform.SetParent(bgRoot.transform);
        morningObj.transform.localPosition = Vector3.zero;
        morningObj.transform.localScale = new Vector3(10, 10, 1);
        SpriteRenderer morningRenderer = morningObj.AddComponent<SpriteRenderer>();
        morningRenderer.sortingOrder = -100;
        
        // 昼の背景
        GameObject daytimeObj = new GameObject("Background_Daytime");
        daytimeObj.transform.SetParent(bgRoot.transform);
        daytimeObj.transform.localPosition = Vector3.zero;
        daytimeObj.transform.localScale = new Vector3(10, 10, 1);
        SpriteRenderer daytimeRenderer = daytimeObj.AddComponent<SpriteRenderer>();
        daytimeRenderer.sortingOrder = -100;
        daytimeRenderer.enabled = false; // 初期は非表示
        
        // 夜の背景
        GameObject nightObj = new GameObject("Background_Night");
        nightObj.transform.SetParent(bgRoot.transform);
        nightObj.transform.localPosition = Vector3.zero;
        nightObj.transform.localScale = new Vector3(10, 10, 1);
        SpriteRenderer nightRenderer = nightObj.AddComponent<SpriteRenderer>();
        nightRenderer.sortingOrder = -100;
        nightRenderer.enabled = false; // 初期は非表示
        
        // フェードオーバーレイ
        GameObject fadeObj = new GameObject("FadeOverlay");
        fadeObj.transform.SetParent(bgRoot.transform);
        fadeObj.transform.localPosition = Vector3.zero;
        fadeObj.transform.localScale = new Vector3(20, 20, 1);
        SpriteRenderer fadeRenderer = fadeObj.AddComponent<SpriteRenderer>();
        fadeRenderer.sortingOrder = 100;
        fadeRenderer.color = new Color(0, 0, 0, 0);
        
        // 1x1の黒いスプライトを作成
        Texture2D blackTex = new Texture2D(1, 1);
        blackTex.SetPixel(0, 0, Color.black);
        blackTex.Apply();
        Sprite blackSprite = Sprite.Create(blackTex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);
        fadeRenderer.sprite = blackSprite;
        
        Debug.Log("[AnimalVoiceApplyAssets] 背景オブジェクトを作成しました");
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
