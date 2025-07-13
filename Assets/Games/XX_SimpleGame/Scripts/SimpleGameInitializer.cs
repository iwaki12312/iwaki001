using UnityEngine;

/// <summary>
/// シンプルゲームの初期化を行うクラス
/// </summary>
public class SimpleGameInitializer : MonoBehaviour
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
    }
    
    void Start()
    {
        // BGMを再生
        BGMManager.Instance.PlayBGM();
        Debug.Log("シンプルゲーム開始時にBGMを初期化しました");
    }
}
