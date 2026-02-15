#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

/// <summary>
/// MushroomPickingゲームの自動テストとスクリーンショット撮影
/// Tools → Auto Test MushroomPicking から実行
/// </summary>
[InitializeOnLoad]
public static class MushroomPickingAutoTest
{
    private const string SCENE_PATH = "Assets/Games/17_MushroomPicking/Scenes/MushroomPicking.unity";
    private const string SCREENSHOT_PATH = "C:/dev/iwaki001/screenshot_mushroompicking.png";
    private const string TRIGGER_FILE = "C:/dev/iwaki001/trigger_autotest_mushroompicking.txt";
    private const string TRIGGER_APPLY_FILE = "C:/dev/iwaki001/trigger_apply_and_test_mushroompicking.txt";
    private const float CAPTURE_DELAY = 5f;

    private static bool isAutoTesting = false;
    private static double testStartTime;

    static MushroomPickingAutoTest()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        EditorApplication.update += CheckTriggerFile;
    }

    private static void CheckTriggerFile()
    {
        if (File.Exists(TRIGGER_FILE) && !EditorApplication.isPlaying && !isAutoTesting)
        {
            File.Delete(TRIGGER_FILE);
            Debug.Log("[MushroomPickingAutoTest] トリガーファイル検出。自動テストを開始します。");
            StartAutoTest();
        }

        if (File.Exists(TRIGGER_APPLY_FILE) && !EditorApplication.isPlaying && !isAutoTesting)
        {
            File.Delete(TRIGGER_APPLY_FILE);
            Debug.Log("[MushroomPickingAutoTest] Apply+Testトリガー検出。アセット適用後にテストを開始します。");
            MushroomPickingApplyAssets.ApplyAssetsSilent();
            StartAutoTest();
        }
    }

    [MenuItem("Tools/Auto Test MushroomPicking")]
    public static void StartAutoTest()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogWarning("[MushroomPickingAutoTest] 既にPlayモード中です。先に停止してください。");
            return;
        }

        var scene = EditorSceneManager.OpenScene(SCENE_PATH);
        Debug.Log($"[MushroomPickingAutoTest] シーンを開きました: {SCENE_PATH}");

        isAutoTesting = true;
        EditorApplication.isPlaying = true;
        Debug.Log("[MushroomPickingAutoTest] Playモードを開始します...");
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (!isAutoTesting) return;

        switch (state)
        {
            case PlayModeStateChange.EnteredPlayMode:
                Debug.Log($"[MushroomPickingAutoTest] Playモードに入りました。{CAPTURE_DELAY}秒後にスクリーンショットを撮影します...");
                testStartTime = EditorApplication.timeSinceStartup;
                EditorApplication.update += UpdateAutoTest;
                break;

            case PlayModeStateChange.ExitingPlayMode:
                EditorApplication.update -= UpdateAutoTest;
                Debug.Log("[MushroomPickingAutoTest] Playモードを終了します...");
                break;

            case PlayModeStateChange.EnteredEditMode:
                if (isAutoTesting)
                {
                    isAutoTesting = false;
                    Debug.Log("[MushroomPickingAutoTest] 自動テスト完了。スクリーンショットを確認してください: " + SCREENSHOT_PATH);
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
        Debug.Log("[MushroomPickingAutoTest] スクリーンショットを撮影中...");

        if (File.Exists(SCREENSHOT_PATH))
        {
            File.Delete(SCREENSHOT_PATH);
        }

        ScreenCapture.CaptureScreenshot(SCREENSHOT_PATH);
        Debug.Log($"[MushroomPickingAutoTest] スクリーンショット保存: {SCREENSHOT_PATH}");
    }
}
#endif
