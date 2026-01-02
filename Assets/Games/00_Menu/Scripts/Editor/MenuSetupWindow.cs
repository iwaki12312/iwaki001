using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// メニュー画面のセットアップを行うエディタウィンドウ
/// </summary>
public class MenuSetupWindow : EditorWindow
{
    private Sprite bubbleSprite;
    private Sprite moleSprite;
    private Sprite backgroundSprite;
    private Vector2 bubblePosition = new Vector2(-2, 0);
    private Vector2 molePosition = new Vector2(2, 0);
    private Vector3 bubbleScale = Vector3.one;
    private Vector3 moleScale = Vector3.one;
    private Color bubbleColor = Color.white;
    private Color moleColor = Color.white;
    
    [MenuItem("Tools/メニュー画面セットアップ")]
    public static void ShowWindow()
    {
        GetWindow<MenuSetupWindow>("メニュー画面セットアップ");
    }
    
    void OnGUI()
    {
        GUILayout.Label("メニュー画面セットアップ", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        // スプライト設定
        EditorGUILayout.LabelField("スプライト設定", EditorStyles.boldLabel);
        bubbleSprite = (Sprite)EditorGUILayout.ObjectField("バブルアイコン", bubbleSprite, typeof(Sprite), false);
        moleSprite = (Sprite)EditorGUILayout.ObjectField("モグラアイコン", moleSprite, typeof(Sprite), false);
        backgroundSprite = (Sprite)EditorGUILayout.ObjectField("背景", backgroundSprite, typeof(Sprite), false);
        
        EditorGUILayout.Space();
        
        // 位置・サイズ設定
        EditorGUILayout.LabelField("位置・サイズ設定", EditorStyles.boldLabel);
        bubblePosition = EditorGUILayout.Vector2Field("バブル位置", bubblePosition);
        molePosition = EditorGUILayout.Vector2Field("モグラ位置", molePosition);
        bubbleScale = EditorGUILayout.Vector3Field("バブルスケール", bubbleScale);
        moleScale = EditorGUILayout.Vector3Field("モグラスケール", moleScale);
        
        EditorGUILayout.Space();
        
        // 色設定
        EditorGUILayout.LabelField("色設定", EditorStyles.boldLabel);
        bubbleColor = EditorGUILayout.ColorField("バブル色", bubbleColor);
        moleColor = EditorGUILayout.ColorField("モグラ色", moleColor);
        
        EditorGUILayout.Space();
        
        // セットアップボタン
        if (GUILayout.Button("メニュー画面をセットアップ"))
        {
            SetupMenuScene();
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("このツールは、メニュー画面のセットアップを自動的に行います。\n" +
                               "1. バブルとモグラのアイコンを設定\n" +
                               "2. 位置とサイズを調整\n" +
                               "3. 「メニュー画面をセットアップ」ボタンをクリック", MessageType.Info);
    }
    
    /// <summary>
    /// メニュー画面をセットアップする
    /// </summary>
    private void SetupMenuScene()
    {
        // 現在のシーンを保存
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            // メニューシーンのパス
            string menuScenePath = "Assets/Games/00_Menu/Scenes/Menu.unity";
            
            // メニューシーンが存在するか確認
            if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), menuScenePath)))
            {
                // メニューシーンを開く
                EditorSceneManager.OpenScene(menuScenePath);
            }
            else
            {
                // メニューシーンが存在しない場合は新規作成
                EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects);
                
                // シーンを保存
                if (!Directory.Exists("Assets/Games/00_Menu/Scenes"))
                {
                    Directory.CreateDirectory("Assets/Games/00_Menu/Scenes");
                }
                EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), menuScenePath);
                Debug.Log($"新しいメニューシーンを作成しました: {menuScenePath}");
            }
            
            // メニュー画面をセットアップ
            SetupMenuObjects();
            
            // シーンを保存
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            Debug.Log("メニュー画面のセットアップが完了しました");
        }
    }
    
    /// <summary>
    /// メニュー画面のオブジェクトをセットアップする
    /// </summary>
    private void SetupMenuObjects()
    {
        // EventSystemの確認
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            // EventSystemを作成
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            Debug.Log("EventSystemを作成しました");
        }
        
        // 背景の設定
        GameObject backgroundObj = GameObject.Find("BackGround");
        if (backgroundObj == null)
        {
            backgroundObj = new GameObject("BackGround");
            backgroundObj.AddComponent<SpriteRenderer>();
        }
        
        SpriteRenderer backgroundRenderer = backgroundObj.GetComponent<SpriteRenderer>();
        if (backgroundSprite != null)
        {
            backgroundRenderer.sprite = backgroundSprite;
        }
        backgroundRenderer.sortingOrder = -1;
        
        // バブルの設定
        GameObject bubbleObj = GameObject.Find("Bubble");
        if (bubbleObj == null)
        {
            bubbleObj = new GameObject("Bubble");
            bubbleObj.AddComponent<SpriteRenderer>();
        }
        
        SpriteRenderer bubbleRenderer = bubbleObj.GetComponent<SpriteRenderer>();
        if (bubbleSprite != null)
        {
            bubbleRenderer.sprite = bubbleSprite;
        }
        bubbleRenderer.color = bubbleColor;
        bubbleObj.transform.position = new Vector3(bubblePosition.x, bubblePosition.y, 0);
        bubbleObj.transform.localScale = bubbleScale;
        
        // モグラの設定
        GameObject moleObj = GameObject.Find("Mole");
        if (moleObj == null)
        {
            moleObj = new GameObject("Mole");
            moleObj.AddComponent<SpriteRenderer>();
        }
        
        SpriteRenderer moleRenderer = moleObj.GetComponent<SpriteRenderer>();
        if (moleSprite != null)
        {
            moleRenderer.sprite = moleSprite;
        }
        moleRenderer.color = moleColor;
        moleObj.transform.position = new Vector3(molePosition.x, molePosition.y, 0);
        moleObj.transform.localScale = moleScale;
        
        // MenuControllerの設定
        GameObject controllerObj = GameObject.Find("MenuController");
        if (controllerObj == null)
        {
            controllerObj = new GameObject("MenuController");
        }
        
        if (controllerObj.GetComponent<MenuController>() == null)
        {
            controllerObj.AddComponent<MenuController>();
        }
        
        // ビルド設定の確認
        CheckBuildSettings();
    }
    
    /// <summary>
    /// ビルド設定を確認する
    /// </summary>
    private void CheckBuildSettings()
    {
        // 必要なシーンパス
        string menuScenePath = "Assets/Games/00_Menu/Scenes/Menu.unity";
        string bubbleScenePath = "Assets/Games/01_MakeBubbles/Scenes/MakeBubbles.unity";
        string moleScenePath = "Assets/Games/02_WhackAMole/Scenes/WhackAMole.unity";
        
        // 現在のビルド設定を取得
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        
        // 各シーンがビルド設定に含まれているか確認
        bool hasMenuScene = false;
        bool hasBubbleScene = false;
        bool hasMoleScene = false;
        
        foreach (var scene in scenes)
        {
            if (scene.path == menuScenePath) hasMenuScene = true;
            if (scene.path == bubbleScenePath) hasBubbleScene = true;
            if (scene.path == moleScenePath) hasMoleScene = true;
        }
        
        // 不足しているシーンを追加
        bool needsUpdate = false;
        
        if (!hasMenuScene && File.Exists(Path.Combine(Directory.GetCurrentDirectory(), menuScenePath)))
        {
            scenes.Add(new EditorBuildSettingsScene(menuScenePath, true));
            needsUpdate = true;
            Debug.Log($"ビルド設定にメニューシーンを追加しました: {menuScenePath}");
        }
        
        if (!hasBubbleScene && File.Exists(Path.Combine(Directory.GetCurrentDirectory(), bubbleScenePath)))
        {
            scenes.Add(new EditorBuildSettingsScene(bubbleScenePath, true));
            needsUpdate = true;
            Debug.Log($"ビルド設定にバブルゲームシーンを追加しました: {bubbleScenePath}");
        }
        
        if (!hasMoleScene && File.Exists(Path.Combine(Directory.GetCurrentDirectory(), moleScenePath)))
        {
            scenes.Add(new EditorBuildSettingsScene(moleScenePath, true));
            needsUpdate = true;
            Debug.Log($"ビルド設定にモグラたたきゲームシーンを追加しました: {moleScenePath}");
        }
        
        // ビルド設定を更新
        if (needsUpdate)
        {
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
