#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// トリガーファイルを監視して SnowmanBuilder のセットアップを自動実行する
/// </summary>
[InitializeOnLoad]
public static class SnowmanBuilderAutoSetup
{
    private const string TRIGGER_FILE = "trigger_setup_snowman.txt";

    static SnowmanBuilderAutoSetup()
    {
        EditorApplication.update += CheckTrigger;
    }

    private static void CheckTrigger()
    {
        string triggerPath = Path.Combine(Application.dataPath, "..", TRIGGER_FILE);
        if (File.Exists(triggerPath))
        {
            File.Delete(triggerPath);
            Debug.Log("[SnowmanBuilderAutoSetup] トリガーを検出。セットアップを開始します...");

            // 少し待ってからセットアップ実行（コンパイル完了を確実にするため）
            EditorApplication.delayCall += () =>
            {
                SnowmanBuilderSetup.SetupGame();
                Debug.Log("[SnowmanBuilderAutoSetup] セットアップ完了！");

                // 完了フラグファイルを作成
                string donePath = Path.Combine(Application.dataPath, "..", "trigger_setup_snowman_done.txt");
                File.WriteAllText(donePath, "done");
            };
        }
    }
}
#endif
