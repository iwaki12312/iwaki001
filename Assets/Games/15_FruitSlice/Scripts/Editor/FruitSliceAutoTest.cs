#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

/// <summary>
/// FruitSliceゲームの自動テストとスクリーンショット撮影
/// Tools → Auto Test FruitSlice から実行
/// </summary>
[InitializeOnLoad]
public static class FruitSliceAutoTest
{
    private const string SCENE_PATH = "Assets/Games/15_FruitSlice/Scenes/FruitSlice.unity";
    private const string SCREENSHOT_PATH = "C:/dev/iwaki001/screenshot_fruitslice.png";
    private const string TRIGGER_FILE = "C:/dev/iwaki001/trigger_autotest_fruitslice.txt";
    private const string TRIGGER_APPLY_FILE = "C:/dev/iwaki001/trigger_apply_and_test_fruitslice.txt";
    private const float CAPTURE_DELAY = 5f;

    private static bool isAutoTesting = false;
    private static double testStartTime;

    static FruitSliceAutoTest()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        EditorApplication.update += CheckTriggerFile;
    }

    private static void CheckTriggerFile()
    {
        if (File.Exists(TRIGGER_FILE) && !EditorApplication.isPlaying && !isAutoTesting)
        {
            File.Delete(TRIGGER_FILE);
            Debug.Log("[FruitSliceAutoTest] トリガーファイル検出。自動テストを開始します。");
            StartAutoTest();
        }

        if (File.Exists(TRIGGER_APPLY_FILE) && !EditorApplication.isPlaying && !isAutoTesting)
        {
            File.Delete(TRIGGER_APPLY_FILE);
            Debug.Log("[FruitSliceAutoTest] Apply+Testトリガー検出。");
            FruitSliceApplyAssets.ApplyAssetsSilent();
            StartAutoTest();
        }
    }

    [MenuItem("Tools/Auto Test FruitSlice")]
    public static void StartAutoTest()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogWarning("[FruitSliceAutoTest] 既にPlayモード中です。");
            return;
        }

        var scene = EditorSceneManager.OpenScene(SCENE_PATH);
        Debug.Log($"[FruitSliceAutoTest] シーンを開きました: {SCENE_PATH}");

        isAutoTesting = true;
        EditorApplication.isPlaying = true;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (!isAutoTesting) return;

        switch (state)
        {
            case PlayModeStateChange.EnteredPlayMode:
                Debug.Log($"[FruitSliceAutoTest] {CAPTURE_DELAY}秒後にスクリーンショット撮影...");
                testStartTime = EditorApplication.timeSinceStartup;
                EditorApplication.update += UpdateAutoTest;
                break;

            case PlayModeStateChange.ExitingPlayMode:
                EditorApplication.update -= UpdateAutoTest;
                break;

            case PlayModeStateChange.EnteredEditMode:
                if (isAutoTesting)
                {
                    isAutoTesting = false;
                    Debug.Log("[FruitSliceAutoTest] 自動テスト完了: " + SCREENSHOT_PATH);
                }
                break;
        }
    }

    private static void UpdateAutoTest()
    {
        if (!isAutoTesting || !EditorApplication.isPlaying) return;

        double elapsed = EditorApplication.timeSinceStartup - testStartTime;

        if (elapsed >= CAPTURE_DELAY)
        {
            CaptureScreenshot();
            EditorApplication.update -= UpdateAutoTest;
            EditorApplication.isPlaying = false;
        }
    }

    private static void CaptureScreenshot()
    {
        if (File.Exists(SCREENSHOT_PATH))
        {
            File.Delete(SCREENSHOT_PATH);
        }

        ScreenCapture.CaptureScreenshot(SCREENSHOT_PATH);
        Debug.Log($"[FruitSliceAutoTest] スクリーンショット保存: {SCREENSHOT_PATH}");
    }
}
#endif
