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
        
        // BGMManagerが存在しない場合は作成
        if (FindObjectOfType<BGMManager>() == null)
        {
            Debug.Log("BGMManagerが見つからないため、新しく作成します");
            GameObject bgmManagerObj = new GameObject("BGMManager");
            bgmManagerObj.AddComponent<BGMManager>();
            DontDestroyOnLoad(bgmManagerObj);
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
        BGMManager.Instance.PlayBGM();
    }
}
