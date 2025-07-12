using UnityEngine;

/// <summary>
/// バブルゲームの初期化を行うクラス
/// </summary>
public class GameInitializer : MonoBehaviour
{
    [SerializeField] private GameObject bubblePrefab; // バブルプレハブの参照
    
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
        
        // BackgroundInitializerが存在しない場合は作成
        if (FindObjectOfType<BackgroundInitializer>() == null)
        {
            Debug.Log("BackgroundInitializerが見つからないため、新しく作成します");
            GameObject backgroundInitializerObj = new GameObject("BackgroundInitializer");
            backgroundInitializerObj.AddComponent<BackgroundInitializer>();
        }
    }
    
    void Start()
    {
        // BGMを再生
        AudioManager.Instance.PlayBGM();
        Debug.Log("バブルゲーム開始時にBGMを初期化しました");
        
        // バブルプレハブが設定されていない場合は、シーンに配置されているか確認
        if (bubblePrefab == null)
        {
            // シーン内のBubbleプレハブを検索
            GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.name == "Bubble" && obj.CompareTag("Bubble"))
                {
                    bubblePrefab = obj;
                    Debug.Log("シーン内でバブルプレハブを見つけました: " + obj.name);
                    break;
                }
            }
            
            // 見つからない場合は、プレハブを直接インスタンス化
            if (bubblePrefab == null)
            {
                // バブルゲーム内のプレハブをインスタンス化
                GameObject tempBubble = Instantiate(Resources.Load<GameObject>("Games/BubbleGame/Prefabs/Bubble"));
                if (tempBubble != null)
                {
                    bubblePrefab = tempBubble;
                    bubblePrefab.SetActive(false); // 非表示にしておく
                    Debug.Log("バブルプレハブをResourcesから読み込みました");
                }
                else
                {
                    Debug.LogError("バブルプレハブがResourcesフォルダに見つかりません");
                    
                    // 最終手段として新しく作成
                    bubblePrefab = new GameObject("Bubble");
                    bubblePrefab.AddComponent<SpriteRenderer>();
                    bubblePrefab.AddComponent<CircleCollider2D>();
                    bubblePrefab.AddComponent<Rigidbody2D>();
                    bubblePrefab.AddComponent<BubbleController>();
                    bubblePrefab.tag = "Bubble";
                    bubblePrefab.SetActive(false);
                    Debug.Log("バブルプレハブを新規作成しました");
                }
            }
        }
    }
    
    /// <summary>
    /// バブルプレハブを取得するメソッド
    /// </summary>
    public GameObject GetBubblePrefab()
    {
        return bubblePrefab;
    }
}
