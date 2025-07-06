using UnityEngine;

/// <summary>
/// メニュー画面でBGMを初期化するクラス
/// </summary>
public class BGMInitializer : MonoBehaviour
{
    void Awake()
    {
        // AudioManagerが存在しない場合は作成
        if (FindObjectOfType<AudioManager>() == null)
        {
            Debug.Log("AudioManagerが見つからないため、新しく作成します");
            GameObject audioManagerObj = new GameObject("AudioManager");
            AudioManager audioManager = audioManagerObj.AddComponent<AudioManager>();
            DontDestroyOnLoad(audioManagerObj);
            
            // 初期化を待ってからBGMを再生
            Invoke("PlayBGMDelayed", 0.5f);
        }
        else
        {
            // 既存のAudioManagerがある場合は直接再生
            PlayBGMDelayed();
        }
    }
    
    void Start()
    {
        // 念のためStartでも再生を試みる
        PlayBGMDelayed();
    }
    
    void PlayBGMDelayed()
    {
        // AudioManagerのインスタンスを取得し、BGMを再生
        AudioManager.Instance.PlayBGM();
        Debug.Log("メニュー画面でBGMを初期化しました（PlayBGMDelayed）");
    }
    
    void OnEnable()
    {
        // シーンがアクティブになったときにも再生を試みる
        Invoke("PlayBGMDelayed", 0.2f);
    }
}
