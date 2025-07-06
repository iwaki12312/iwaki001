using UnityEngine;

/// <summary>
/// バブルゲームの初期化を行うクラス
/// </summary>
public class GameInitializer : MonoBehaviour
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
        Debug.Log("バブルゲーム開始時にBGMを初期化しました");
    }
}
