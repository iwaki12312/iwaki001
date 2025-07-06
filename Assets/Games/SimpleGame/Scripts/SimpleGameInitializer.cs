using UnityEngine;

/// <summary>
/// シンプルゲームの初期化を行うクラス
/// </summary>
public class SimpleGameInitializer : MonoBehaviour
{
    void Awake()
    {
        // AudioManagerが存在しない場合は作成
        if (FindObjectOfType<AudioManager>() == null)
        {
            Debug.Log("AudioManagerが見つからないため、新しく作成します");
            GameObject audioManagerObj = new GameObject("AudioManager");
            audioManagerObj.AddComponent<AudioManager>();
            DontDestroyOnLoad(audioManagerObj);
        }
    }
    
    void Start()
    {
        // BGMを再生
        AudioManager.Instance.PlayBGM();
        Debug.Log("シンプルゲーム開始時にBGMを初期化しました");
    }
}
