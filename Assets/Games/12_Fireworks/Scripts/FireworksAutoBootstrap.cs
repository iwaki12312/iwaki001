using UnityEngine;
using UnityEngine.SceneManagement;

public static class FireworksAutoBootstrap
{
    private const string TargetSceneName = "Fireworks";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AfterSceneLoad()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || scene.name != TargetSceneName)
        {
            return;
        }

        if (Object.FindObjectOfType<FireworksInitializer>() != null)
        {
            return;
        }

        var obj = new GameObject("FireworksInitializer");
        obj.AddComponent<FireworksInitializer>();
    }
}
