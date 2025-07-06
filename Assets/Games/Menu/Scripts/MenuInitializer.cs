using UnityEngine;

/// <summary>
/// メニュー画面の初期化を行うクラス
/// </summary>
public class MenuInitializer : MonoBehaviour
{
    [SerializeField] private GameObject bgmInitializerPrefab;
    
    void Awake()
    {
        Debug.Log("MenuInitializer.Awake()が呼ばれました");
        
        // AudioManagerが存在しない場合は作成
        if (FindObjectOfType<AudioManager>() == null)
        {
            Debug.Log("AudioManagerが見つからないため、新しく作成します");
            GameObject audioManagerObj = new GameObject("AudioManager");
            audioManagerObj.AddComponent<AudioManager>();
            DontDestroyOnLoad(audioManagerObj);
        }
        
        // BGMInitializerが存在しない場合は作成
        if (FindObjectOfType<BGMInitializer>() == null)
        {
            Debug.Log("BGMInitializerが見つからないため、新しく作成します");
            if (bgmInitializerPrefab != null)
            {
                Instantiate(bgmInitializerPrefab);
            }
            else
            {
                GameObject bgmInitializerObj = new GameObject("BGMInitializer");
                bgmInitializerObj.AddComponent<BGMInitializer>();
            }
        }
    }
    
    void Start()
    {
        Debug.Log("MenuInitializer.Start()が呼ばれました");
        
        // 直接BGMを再生
        AudioManager.Instance.PlayBGM();
    }
}
