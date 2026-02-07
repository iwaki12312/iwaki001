#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

/// <summary>
/// AnimalVoiceゲームの自動テストとスクリーンショット撮影
/// Tools → Auto Test AnimalVoice から実行
/// </summary>
[InitializeOnLoad]
public static class AnimalVoiceAutoTest
{
    private const string SCENE_PATH = "Assets/Games/14_AnimalVoice/Scenes/AnimalVoice.unity";
    private const string SCREENSHOT_PATH = "C:/dev/iwaki001/screenshot_animalvoice.png";
    private const string TRIGGER_FILE = "C:/dev/iwaki001/trigger_autotest.txt";
    private const float CAPTURE_DELAY = 5f;

    private static bool isAutoTesting = false;
    private static double testStartTime;

    static AnimalVoiceAutoTest()
    {
        // Playモード変更イベントを監視
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

        // トリガーファイルを監視（既に開いているUnity Editorでも動作）
        EditorApplication.update += CheckTriggerFile;
    }

    private static void CheckTriggerFile()
    {
        if (File.Exists(TRIGGER_FILE) && !EditorApplication.isPlaying && !isAutoTesting)
        {
            File.Delete(TRIGGER_FILE);
            Debug.Log("[AnimalVoiceAutoTest] トリガーファイル検出。自動テストを開始します。");
            StartAutoTest();
        }
    }

    [MenuItem("Tools/Auto Test AnimalVoice")]
    public static void StartAutoTest()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogWarning("[AnimalVoiceAutoTest] 既にPlayモード中です。先に停止してください。");
            return;
        }

        // シーンを開く
        var scene = EditorSceneManager.OpenScene(SCENE_PATH);
        Debug.Log($"[AnimalVoiceAutoTest] シーンを開きました: {SCENE_PATH}");

        // 自動テストフラグを立てる
        isAutoTesting = true;

        // Playモードに入る
        EditorApplication.isPlaying = true;
        Debug.Log("[AnimalVoiceAutoTest] Playモードを開始します...");
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (!isAutoTesting) return;

        switch (state)
        {
            case PlayModeStateChange.EnteredPlayMode:
                Debug.Log($"[AnimalVoiceAutoTest] Playモードに入りました。{CAPTURE_DELAY}秒後にスクリーンショットを撮影します...");
                testStartTime = EditorApplication.timeSinceStartup;
                EditorApplication.update += UpdateAutoTest;
                break;

            case PlayModeStateChange.ExitingPlayMode:
                EditorApplication.update -= UpdateAutoTest;
                Debug.Log("[AnimalVoiceAutoTest] Playモードを終了します...");
                break;

            case PlayModeStateChange.EnteredEditMode:
                if (isAutoTesting)
                {
                    isAutoTesting = false;
                    Debug.Log("[AnimalVoiceAutoTest] 自動テスト完了。スクリーンショットを確認してください: " + SCREENSHOT_PATH);
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
            // スクリーンショットを撮影
            CaptureScreenshot();

            // Playモードを停止
            EditorApplication.update -= UpdateAutoTest;
            EditorApplication.isPlaying = false;
        }
    }

    private static void CaptureScreenshot()
    {
        Debug.Log("[AnimalVoiceAutoTest] スクリーンショットを撮影中...");

        // 既存のスクリーンショットを削除
        if (File.Exists(SCREENSHOT_PATH))
        {
            File.Delete(SCREENSHOT_PATH);
        }

        // Game Viewのサイズでスクリーンショットを撮影
        ScreenCapture.CaptureScreenshot(SCREENSHOT_PATH);

        Debug.Log($"[AnimalVoiceAutoTest] スクリーンショット保存: {SCREENSHOT_PATH}");
    }
}
#endif
