using UnityEngine;

/// <summary>
/// メニュー画面でBGMを初期化するクラス
/// </summary>
public class BGMInitializer : MonoBehaviour
{
    [SerializeField] private AudioClip menuBGM; // メニュー画面用のBGM
    
    void Awake()
    {
        // BGMManagerが存在しない場合は作成
        if (FindObjectOfType<BGMManager>() == null)
        {
            Debug.Log("BGMManagerが見つからないため、新しく作成します");
            GameObject bgmManagerObj = new GameObject("BGMManager");
            BGMManager bgmManager = bgmManagerObj.AddComponent<BGMManager>();
            DontDestroyOnLoad(bgmManagerObj);
            
            // 初期化を待ってからBGMを再生
            Invoke("PlayBGMDelayed", 0.5f);
        }
        else
        {
            // 既存のBGMManagerがある場合は直接再生
            PlayBGMDelayed();
        }
    }
    
    void PlayBGMDelayed()
    {
        // BGMManagerのインスタンスを取得し、BGMを再生
        if (menuBGM != null)
        {
            // 指定したBGMを再生
            BGMManager.Instance.PlayBGM(menuBGM);
        }
        else
        {
            // デフォルトのBGMを再生
            BGMManager.Instance.PlayBGM();
        }
    }
}
