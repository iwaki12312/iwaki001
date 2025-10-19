using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Linq;

/// <summary>
/// CatchInsectsゲームを自動セットアップするエディタ拡張
/// Tools → Setup CatchInsects Game から実行
/// </summary>
public static class CatchInsectsSetup
{
    [MenuItem("Tools/Setup CatchInsects Game")]
    public static void SetupGame()
    {
        // シーンを開く
        string scenePath = "Assets/Games/07_CatchInsects/Scenes/CatchInsects.unity";
        EditorSceneManager.OpenScene(scenePath);
        
        Debug.Log("[CatchInsectsSetup] セットアップ開始");
        
        // 既存のセットアップオブジェクトを削除
        CleanupExistingObjects();
        
        // 背景をセットアップ
        SetupBackground();
        
        // カメラをセットアップ
        SetupCamera();
        
        // GameManagerをセットアップ
        GameObject gameManager = SetupGameManager();
        
        // SFXPlayerをセットアップ
        SetupSFXPlayer();
        
        // InsectSpawnerをセットアップ
        SetupInsectSpawner();
        
        // UI(中央表示パネル)をセットアップ
        SetupDisplayUI(gameManager);
        
        // EventSystemを確認
        EnsureEventSystem();
        
        // シーンを保存
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        
        EditorUtility.DisplayDialog("完了", 
            "CatchInsectsゲームのセットアップが完了しました!\n" +
            "シーンを再生して動作を確認してください。", 
            "OK");
        
        Debug.Log("[CatchInsectsSetup] セットアップ完了");
    }
    
    private static void CleanupExistingObjects()
    {
        string[] objectNames = { "Background", "GameManager", "CatchInsectsSFXPlayer", "InsectSpawner", "DisplayPanel", "Canvas" };
        foreach (string name in objectNames)
        {
            GameObject obj = GameObject.Find(name);
            if (obj != null)
            {
                Object.DestroyImmediate(obj);
            }
        }
    }
    
    private static void SetupBackground()
    {
        // 背景スプライトをロード
        Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Games/07_CatchInsects/Sprites/work_bg.png");
        if (bgSprite == null)
        {
            Debug.LogWarning("[CatchInsectsSetup] 背景スプライトが見つかりません");
            return;
        }
        
        GameObject bg = new GameObject("Background");
        SpriteRenderer sr = bg.AddComponent<SpriteRenderer>();
        sr.sprite = bgSprite;
        sr.sortingOrder = -10;
        
        // カメラに合わせてスケール調整
        float screenHeight = Camera.main.orthographicSize * 2;
        float spriteHeight = bgSprite.bounds.size.y;
        float scale = screenHeight / spriteHeight;
        bg.transform.localScale = Vector3.one * scale;
        
        Debug.Log("[CatchInsectsSetup] 背景を配置しました");
    }
    
    private static void SetupCamera()
    {
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.orthographicSize = 5f;
            mainCam.backgroundColor = new Color(0.8f, 0.9f, 1f); // 明るい青空色
        }
    }
    
    private static GameObject SetupGameManager()
    {
        GameObject gameManager = new GameObject("GameManager");
        gameManager.AddComponent<CatchInsectsGameManager>();
        
        Debug.Log("[CatchInsectsSetup] GameManagerを作成しました");
        return gameManager;
    }
    
    private static void SetupSFXPlayer()
    {
        GameObject sfxPlayer = new GameObject("CatchInsectsSFXPlayer");
        CatchInsectsSFXPlayer player = sfxPlayer.AddComponent<CatchInsectsSFXPlayer>();
        
        // 効果音をロード
        AudioClip sfx1 = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Games/07_CatchInsects/Audios/work_sfx1.mp3");
        AudioClip sfx2 = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Games/07_CatchInsects/Audios/work_sfx2.mp3");
        AudioClip sfx3 = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Games/07_CatchInsects/Audios/work_sfx3.mp3");
        
        SerializedObject so = new SerializedObject(player);
        so.FindProperty("netSwingSound").objectReferenceValue = sfx1;
        so.FindProperty("catchNormalSound").objectReferenceValue = sfx2;
        so.FindProperty("catchRareSound").objectReferenceValue = sfx3;
        so.ApplyModifiedProperties();
        
        Debug.Log("[CatchInsectsSetup] SFXPlayerを作成しました");
    }
    
    private static void SetupInsectSpawner()
    {
        GameObject spawner = new GameObject("InsectSpawner");
        InsectSpawner spawnerScript = spawner.AddComponent<InsectSpawner>();
        
        // 昆虫プレハブを作成
        GameObject insectPrefab = CreateInsectPrefab();
        
        // スプライトシートから個別スプライトを取得
        Sprite[] allSpritesA = LoadSpritesFromSheet("Assets/Games/07_CatchInsects/Sprites/work_sprite_a.png");
        Sprite[] allSpritesB = LoadSpritesFromSheet("Assets/Games/07_CatchInsects/Sprites/work_sprite_b.png");
        
        // 24種類のスプライトを割り当て (6種類 × 4バリエーション)
        if (allSpritesA.Length >= 15 && allSpritesB.Length >= 9)
        {
            InsectType[] insectTypes = new InsectType[6];
            
            // 種類A: blue_0~3
            insectTypes[0] = new InsectType
            {
                typeName = "Type A",
                spawnPosition = new Vector3(-6f, 3f, 0),
                normalSprite1 = allSpritesA[0],
                normalSprite2 = allSpritesA[1],
                normalSprite3 = allSpritesA[2],
                rareSprite = allSpritesA[3]
            };
            
            // 種類B: blue_4~7
            insectTypes[1] = new InsectType
            {
                typeName = "Type B",
                spawnPosition = new Vector3(6f, 3f, 0),
                normalSprite1 = allSpritesA[4],
                normalSprite2 = allSpritesA[5],
                normalSprite3 = allSpritesA[6],
                rareSprite = allSpritesA[7]
            };
            
            // 種類C: blue_8~11
            insectTypes[2] = new InsectType
            {
                typeName = "Type C",
                spawnPosition = new Vector3(-6f, 0f, 0),
                normalSprite1 = allSpritesA[8],
                normalSprite2 = allSpritesA[9],
                normalSprite3 = allSpritesA[10],
                rareSprite = allSpritesA[11]
            };
            
            // 種類D: blue_12~14, red_0
            insectTypes[3] = new InsectType
            {
                typeName = "Type D",
                spawnPosition = new Vector3(6f, 0f, 0),
                normalSprite1 = allSpritesA[12],
                normalSprite2 = allSpritesA[13],
                normalSprite3 = allSpritesA[14],
                rareSprite = allSpritesB[0]
            };
            
            // 種類E: red_1~4
            insectTypes[4] = new InsectType
            {
                typeName = "Type E",
                spawnPosition = new Vector3(-6f, -3f, 0),
                normalSprite1 = allSpritesB[1],
                normalSprite2 = allSpritesB[2],
                normalSprite3 = allSpritesB[3],
                rareSprite = allSpritesB[4]
            };
            
            // 種類F: red_5~8
            insectTypes[5] = new InsectType
            {
                typeName = "Type F",
                spawnPosition = new Vector3(6f, -3f, 0),
                normalSprite1 = allSpritesB[5],
                normalSprite2 = allSpritesB[6],
                normalSprite3 = allSpritesB[7],
                rareSprite = allSpritesB[8]
            };
            
            // SerializedObjectで設定
            SerializedObject so = new SerializedObject(spawnerScript);
            so.FindProperty("insectPrefab").objectReferenceValue = insectPrefab;
            
            SerializedProperty typesProp = so.FindProperty("insectTypes");
            typesProp.arraySize = 6;
            
            for (int i = 0; i < 6; i++)
            {
                SerializedProperty typeProp = typesProp.GetArrayElementAtIndex(i);
                typeProp.FindPropertyRelative("typeName").stringValue = insectTypes[i].typeName;
                typeProp.FindPropertyRelative("spawnPosition").vector3Value = insectTypes[i].spawnPosition;
                typeProp.FindPropertyRelative("normalSprite1").objectReferenceValue = insectTypes[i].normalSprite1;
                typeProp.FindPropertyRelative("normalSprite2").objectReferenceValue = insectTypes[i].normalSprite2;
                typeProp.FindPropertyRelative("normalSprite3").objectReferenceValue = insectTypes[i].normalSprite3;
                typeProp.FindPropertyRelative("rareSprite").objectReferenceValue = insectTypes[i].rareSprite;
            }
            
            so.ApplyModifiedProperties();
            
            Debug.Log($"[CatchInsectsSetup] InsectSpawnerを作成: 6種類×4バリエーション=24種類");
        }
        else
        {
            Debug.LogWarning("[CatchInsectsSetup] スプライトが不足しています");
        }
    }
    
    private static GameObject CreateInsectPrefab()
    {
        // プレハブ保存先
        string prefabPath = "Assets/Games/07_CatchInsects/Prefabs/Insect.prefab";
        
        // 既存のプレハブがあれば削除
        if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
        {
            AssetDatabase.DeleteAsset(prefabPath);
        }
        
        // 新しいゲームオブジェクト作成
        GameObject insect = new GameObject("Insect");
        insect.AddComponent<SpriteRenderer>();
        insect.AddComponent<CircleCollider2D>();
        InsectController controller = insect.AddComponent<InsectController>();
        
        // 虫取り網スプライトを設定 (work_sprite_b の最初のスプライトを使用)
        Sprite[] netSprites = LoadSpritesFromSheet("Assets/Games/07_CatchInsects/Sprites/work_sprite_b.png");
        if (netSprites.Length > 0)
        {
            SerializedObject so = new SerializedObject(controller);
            so.FindProperty("netSprite").objectReferenceValue = netSprites[0];
            
            // デフォルトのオフセットとスケールを設定
            so.FindProperty("netOffset").vector3Value = new Vector3(0, 0.5f, 0); // 少し上に表示
            so.FindProperty("netScale").floatValue = 1.5f; // 1.5倍のサイズ
            
            so.ApplyModifiedProperties();
            Debug.Log("[CatchInsectsSetup] 虫取り網スプライトとデフォルト設定を適用しました");
        }
        
        // プレハブとして保存
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(insect, prefabPath);
        Object.DestroyImmediate(insect);
        
        Debug.Log("[CatchInsectsSetup] 昆虫プレハブを作成しました");
        return prefab;
    }
    
    private static void SetupDisplayUI(GameObject gameManager)
    {
        // Canvas作成
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // DisplayPanel作成
        GameObject panel = new GameObject("DisplayPanel");
        panel.transform.SetParent(canvasObj.transform, false);
        
        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;
        
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.5f); // 半透明黒背景
        
        // 昆虫表示用Image作成
        GameObject displayImage = new GameObject("InsectImage");
        displayImage.transform.SetParent(panel.transform, false);
        
        RectTransform imageRect = displayImage.AddComponent<RectTransform>();
        imageRect.anchorMin = new Vector2(0.5f, 0.5f);
        imageRect.anchorMax = new Vector2(0.5f, 0.5f);
        imageRect.sizeDelta = new Vector2(400, 400);
        
        Image image = displayImage.AddComponent<Image>();
        image.preserveAspect = true;
        
        // GameManagerに参照を設定
        CatchInsectsGameManager manager = gameManager.GetComponent<CatchInsectsGameManager>();
        SerializedObject so = new SerializedObject(manager);
        so.FindProperty("displayPanel").objectReferenceValue = panel;
        so.FindProperty("displayImage").objectReferenceValue = image;
        so.ApplyModifiedProperties();
        
        Debug.Log("[CatchInsectsSetup] DisplayUIを作成しました");
    }
    
    private static void EnsureEventSystem()
    {
        if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
    }
    
    private static Sprite[] LoadSpritesFromSheet(string path)
    {
        Object[] sprites = AssetDatabase.LoadAllAssetsAtPath(path);
        return sprites.OfType<Sprite>().ToArray();
    }
}
