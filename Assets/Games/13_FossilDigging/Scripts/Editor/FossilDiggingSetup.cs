using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// FossilDiggingゲームを自動セットアップするエディタ拡張
/// Tools → Setup FossilDigging Game から実行
/// </summary>
public static class FossilDiggingSetup
{
    private const string GAME_PATH = "Assets/Games/13_FossilDigging";
    private const string SCENE_PATH = GAME_PATH + "/Scenes/FossilDigging.unity";
    private const string SPRITES_PATH = GAME_PATH + "/Sprites";
    private const string AUDIOS_PATH = GAME_PATH + "/Audios";
    private const string CFXR_PATH = "Assets/JMO Assets/Cartoon FX Remaster/CFXR Prefabs";

    [MenuItem("Tools/Setup FossilDigging Game")]
    public static void SetupGame()
    {
        // シーンを開く
        EditorSceneManager.OpenScene(SCENE_PATH);

        Debug.Log("[FossilDiggingSetup] セットアップ開始");

        // 既存のセットアップオブジェクトを削除（背景以外）
        CleanupExistingObjects();

        // カメラを設定
        SetupCamera();

        // GameManagerを作成
        SetupGameManager();

        // SFXPlayerを作成
        SetupSFXPlayer();

        // RockSpawnerを作成
        SetupRockSpawner();

        // シーンを保存
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());

        EditorUtility.DisplayDialog("完了",
            "FossilDiggingゲームのセットアップが完了しました!\n\n" +
            "シーンを再生して動作を確認してください。\n" +
            "RockSpawnerのInspectorから岩の出現位置やレア度確率を調整できます。",
            "OK");

        Debug.Log("[FossilDiggingSetup] セットアップ完了");
    }

    private static void CleanupExistingObjects()
    {
        // 背景(bg_0)は残して、他のセットアップオブジェクトを削除
        string[] objectNames = { "GameManager", "FossilDiggingSFXPlayer", "RockSpawner" };
        foreach (string name in objectNames)
        {
            GameObject obj = GameObject.Find(name);
            if (obj != null)
            {
                Object.DestroyImmediate(obj);
            }
        }

        // 既存のRockオブジェクトも削除
        var rocks = Object.FindObjectsOfType<RockController>();
        foreach (var rock in rocks)
        {
            Object.DestroyImmediate(rock.gameObject);
        }
    }

    private static void SetupCamera()
    {
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.orthographicSize = 5f;
            mainCam.backgroundColor = new Color(0.8f, 0.7f, 0.5f); // 砂漠色
        }
    }

    private static void SetupGameManager()
    {
        GameObject gameManager = new GameObject("GameManager");
        gameManager.AddComponent<FossilDiggingManager>();

        Debug.Log("[FossilDiggingSetup] GameManagerを作成しました");
    }

    private static void SetupSFXPlayer()
    {
        GameObject sfxPlayer = new GameObject("FossilDiggingSFXPlayer");
        FossilDiggingSFXPlayer player = sfxPlayer.AddComponent<FossilDiggingSFXPlayer>();

        // 効果音をロード
        AudioClip attackSound = AssetDatabase.LoadAssetAtPath<AudioClip>(AUDIOS_PATH + "/attack.mp3");
        AudioClip brokenSound = AssetDatabase.LoadAssetAtPath<AudioClip>(AUDIOS_PATH + "/broken.mp3");
        AudioClip normalFanfare = AssetDatabase.LoadAssetAtPath<AudioClip>(AUDIOS_PATH + "/n_fanfare.mp3");
        AudioClip rareFanfare = AssetDatabase.LoadAssetAtPath<AudioClip>(AUDIOS_PATH + "/r_fanfare.mp3");
        AudioClip superRareFanfare = AssetDatabase.LoadAssetAtPath<AudioClip>(AUDIOS_PATH + "/sr_fanfare.mp3");

        // SerializedObjectで設定
        SerializedObject so = new SerializedObject(player);
        so.FindProperty("attackSound").objectReferenceValue = attackSound;
        so.FindProperty("brokenSound").objectReferenceValue = brokenSound;
        so.FindProperty("normalFanfare").objectReferenceValue = normalFanfare;
        so.FindProperty("rareFanfare").objectReferenceValue = rareFanfare;
        so.FindProperty("superRareFanfare").objectReferenceValue = superRareFanfare;
        so.ApplyModifiedProperties();

        Debug.Log("[FossilDiggingSetup] SFXPlayerを作成しました");
    }

    private static void SetupRockSpawner()
    {
        GameObject spawnerObj = new GameObject("RockSpawner");
        RockSpawner spawner = spawnerObj.AddComponent<RockSpawner>();

        // スプライトをロード
        Sprite[] rockSprites = LoadRockSprites();
        Debug.Log($"[FossilDiggingSetup] 岩スプライト: {rockSprites.Length}個ロード");
        
        Sprite pickaxeSprite = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITES_PATH + "/turuhashi.png");
        Debug.Log($"[FossilDiggingSetup] ツルハシスプライト: {(pickaxeSprite != null ? "OK" : "NULL")}");
        
        Sprite[] brokenPieceSprites = LoadBrokenPieceSprites();
        Debug.Log($"[FossilDiggingSetup] 破片スプライト: {brokenPieceSprites.Length}個ロード");
        
        Sprite[] normalTreasures = LoadTreasureSprites("n");
        Debug.Log($"[FossilDiggingSetup] ノーマル宝物: {normalTreasures.Length}個ロード");
        
        Sprite[] rareTreasures = LoadTreasureSprites("r");
        Debug.Log($"[FossilDiggingSetup] レア宝物: {rareTreasures.Length}個ロード");
        
        Sprite[] superRareTreasures = LoadTreasureSprites("sr");
        Debug.Log($"[FossilDiggingSetup] スーパーレア宝物: {superRareTreasures.Length}個ロード");

        // エフェクトプレハブをロード
        GameObject hitEffect = AssetDatabase.LoadAssetAtPath<GameObject>(CFXR_PATH + "/Impacts/CFXR Hit A (Red).prefab");
        GameObject breakEffect = AssetDatabase.LoadAssetAtPath<GameObject>(CFXR_PATH + "/Misc/CFXR Smoke Poof.prefab");

        // 出現位置を設定（2行3列）
        RockSpawnPoint[] spawnPoints = CreateDefaultSpawnPoints();

        // SerializedObjectで設定
        SerializedObject so = new SerializedObject(spawner);

        // 岩スプライト
        SetSpriteArray(so, "rockSprites", rockSprites);

        // ツルハシスプライト
        so.FindProperty("pickaxeSprite").objectReferenceValue = pickaxeSprite;

        // 破片スプライト
        SetSpriteArray(so, "brokenPieceSprites", brokenPieceSprites);

        // 宝物スプライト
        SetSpriteArray(so, "normalTreasureSprites", normalTreasures);
        SetSpriteArray(so, "rareTreasureSprites", rareTreasures);
        SetSpriteArray(so, "superRareTreasureSprites", superRareTreasures);

        // エフェクト
        so.FindProperty("hitEffectPrefab").objectReferenceValue = hitEffect;
        so.FindProperty("breakEffectPrefab").objectReferenceValue = breakEffect;

        // 出現位置
        SerializedProperty spawnPointsProp = so.FindProperty("spawnPoints");
        spawnPointsProp.arraySize = spawnPoints.Length;
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            SerializedProperty pointProp = spawnPointsProp.GetArrayElementAtIndex(i);
            pointProp.FindPropertyRelative("pointName").stringValue = spawnPoints[i].pointName;
            pointProp.FindPropertyRelative("position").vector3Value = spawnPoints[i].position;
            pointProp.FindPropertyRelative("scale").floatValue = spawnPoints[i].scale;
        }

        so.ApplyModifiedProperties();
        
        // 設定確認ログ
        Debug.Log($"[FossilDiggingSetup] spawnPoints: {spawnPoints.Length}個設定");
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            Debug.Log($"  Point[{i}]: {spawnPoints[i].position}");
        }

        Debug.Log("[FossilDiggingSetup] RockSpawnerを作成しました");
    }

    /// <summary>
    /// 岩スプライトをロード（1.png〜6.png）
    /// </summary>
    private static Sprite[] LoadRockSprites()
    {
        List<Sprite> sprites = new List<Sprite>();
        for (int i = 1; i <= 6; i++)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{SPRITES_PATH}/{i}.png");
            if (sprite != null)
            {
                sprites.Add(sprite);
            }
        }
        return sprites.ToArray();
    }

    /// <summary>
    /// 破片スプライトをロード（スプライトシートから）
    /// </summary>
    private static Sprite[] LoadBrokenPieceSprites()
    {
        // スプライトシートの全サブスプライトをロード
        Object[] allSprites = AssetDatabase.LoadAllAssetsAtPath(SPRITES_PATH + "/brokenPiece.png");
        List<Sprite> sprites = new List<Sprite>();
        foreach (Object obj in allSprites)
        {
            if (obj is Sprite sprite && sprite.name != "brokenPiece")
            {
                sprites.Add(sprite);
            }
        }

        // サブスプライトがない場合はメインスプライトを使用
        if (sprites.Count == 0)
        {
            Sprite mainSprite = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITES_PATH + "/brokenPiece.png");
            if (mainSprite != null)
            {
                sprites.Add(mainSprite);
            }
        }

        return sprites.ToArray();
    }

    /// <summary>
    /// 宝物スプライトをロード
    /// </summary>
    private static Sprite[] LoadTreasureSprites(string rarity)
    {
        string folderPath = $"{SPRITES_PATH}/{rarity}";
        List<Sprite> sprites = new List<Sprite>();

        // フォルダ内のpngファイルをすべてロード
        string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { folderPath });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            // スプライトシートの場合はサブスプライトをロード
            Object[] allSprites = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (Object obj in allSprites)
            {
                if (obj is Sprite sprite)
                {
                    sprites.Add(sprite);
                }
            }
        }

        return sprites.ToArray();
    }

    /// <summary>
    /// デフォルトの出現位置を作成（2行3列）
    /// </summary>
    private static RockSpawnPoint[] CreateDefaultSpawnPoints()
    {
        RockSpawnPoint[] points = new RockSpawnPoint[6];

        // 画面下部に2行3列で配置
        float startX = -4f;
        float startY = -1f;
        float spacingX = 4f;
        float spacingY = 3f;

        for (int row = 0; row < 2; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                int index = row * 3 + col;
                points[index] = new RockSpawnPoint
                {
                    pointName = $"Point_{index}",
                    position = new Vector3(startX + col * spacingX, startY - row * spacingY, 0),
                    scale = 1f
                };
            }
        }

        return points;
    }

    /// <summary>
    /// SerializedPropertyにスプライト配列を設定
    /// </summary>
    private static void SetSpriteArray(SerializedObject so, string propertyName, Sprite[] sprites)
    {
        SerializedProperty prop = so.FindProperty(propertyName);
        if (prop != null)
        {
            prop.arraySize = sprites.Length;
            for (int i = 0; i < sprites.Length; i++)
            {
                prop.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];
            }
        }
    }
}
