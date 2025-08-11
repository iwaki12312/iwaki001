using UnityEngine;

/// <summary>
/// メニュー画面の初期化を行うクラス
/// </summary>
public class MenuInitializer : MonoBehaviour
{
    [Header("BGM設定")]
    [SerializeField] private AudioClip menuBGM; // メニュー画面用のBGM
    
    void Awake()
    {
        Debug.Log("MenuInitializer.Awake()が呼ばれました");
        
        // BGMManagerが存在しない場合は作成
        if (FindObjectOfType<BGMManager>() == null)
        {
            Debug.Log("BGMManagerが見つからないため、新しく作成します");
            GameObject bgmManagerObj = new GameObject("BGMManager");
            bgmManagerObj.AddComponent<BGMManager>();
            DontDestroyOnLoad(bgmManagerObj);
        }
        
        // メニューBGMを設定して再生
        if (menuBGM != null)
        {
            Debug.Log("メニューBGMを設定して再生します");
            BGMManager.Instance.PlayBGM(menuBGM);
        }
        else
        {
            Debug.Log("デフォルトBGMで再生します");
            BGMManager.Instance.PlayBGM();
        }
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
