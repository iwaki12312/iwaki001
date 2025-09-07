using UnityEngine;

public class BGMController : MonoBehaviour
{
    void Awake()
    {
        // BGMManagerが存在しない場合は作成
        if (FindObjectOfType<BGMManager>() == null)
        {
            Debug.Log("BGMManagerが見つからないため、新しく作成します");
            GameObject bgmManagerObj = new GameObject("BGMManager");
            bgmManagerObj.AddComponent<BGMManager>();
            DontDestroyOnLoad(bgmManagerObj);
        }

        // BGMを設定して再生
        BGMManager.Instance.PlayBGM();

    }

    void Start()
    {
        Debug.Log("MenuInitializer.Start()が呼ばれました");

        // BGM再生処理を削除（Awakeで行うため）
        // 念のため、BGMが再生されていない場合のみ再生
        if (BGMManager.Instance != null && !BGMManager.Instance.GetComponent<AudioSource>().isPlaying)
        {
            Debug.Log("BGMが再生されていないため、Start()で再生を試行します");
            BGMManager.Instance.PlayBGM();
        }
    }
}
