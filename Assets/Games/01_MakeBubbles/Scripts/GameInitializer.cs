using UnityEngine;

/// <summary>
/// バブルゲームの初期化を行うクラス
/// </summary>
public class GameInitializer : MonoBehaviour
{
    [SerializeField] private GameObject bubblePrefab; // バブルプレハブの参照
    [SerializeField] private GameObject starBubblePrefab; // 星入りバブルプレハブ
    [SerializeField] [Range(0f, 1f)] private float starBubbleChance = 0.1f; // 星入り出現確率
    
    void Awake()
    {
        Debug.Log("GameInitializer.Awake()が開始されました");
        
        // BubbleMakerManagerを最初に作成（他のコンポーネントが依存する可能性があるため）
        if (FindObjectOfType<BubbleMakerManager>() == null)
        {
            Debug.Log("BubbleMakerManagerが見つからないため、新しく作成します");
            GameObject bubbleMakerManagerObj = new GameObject("BubbleMakerManager");
            BubbleMakerManager manager = bubbleMakerManagerObj.AddComponent<BubbleMakerManager>();
            Debug.Log("BubbleMakerManagerを作成しました: " + manager.GetInstanceID());
        }
        else
        {
            Debug.Log("BubbleMakerManagerは既に存在します");
        }
        
        // // BGMManagerが存在しない場合は作成
        // if (FindObjectOfType<BGMManager>() == null)
        // {
        //     Debug.Log("BGMManagerが見つからないため、新しく作成します");
        //     GameObject bgmManagerObj = new GameObject("BGMManager");
        //     bgmManagerObj.AddComponent<BGMManager>();
        //     DontDestroyOnLoad(bgmManagerObj);
        // }
        
        // BubbleSoundManagerが存在しない場合は作成
        if (FindObjectOfType<BubbleSoundManager>() == null)
        {
            Debug.Log("BubbleSoundManagerが見つからないため、新しく作成します");
            GameObject soundManagerObj = new GameObject("BubbleSoundManager");
            soundManagerObj.AddComponent<BubbleSoundManager>();
        }
        
        // BackgroundInitializerが存在しない場合は作成
        if (FindObjectOfType<BackgroundInitializer>() == null)
        {
            Debug.Log("BackgroundInitializerが見つからないため、新しく作成します");
            GameObject backgroundInitializerObj = new GameObject("BackgroundInitializer");
            backgroundInitializerObj.AddComponent<BackgroundInitializer>();
        }
        
        Debug.Log("GameInitializer.Awake()が完了しました");
    }
    
    void Start()
    {
        // // BGMを再生
        // if (BGMManager.Instance != null)
        // {
        //     BGMManager.Instance.PlayBGM();
        //     Debug.Log("バブルゲーム開始時にBGMを初期化しました");
        // }
        
        // バブルプレハブが設定されていない場合は、プレハブフォルダから読み込み
        if (bubblePrefab == null)
        {
            Debug.Log("バブルプレハブが設定されていないため、プレハブフォルダから読み込みを試行します");
            
            // プレハブフォルダから直接読み込み
            bubblePrefab = Resources.Load<GameObject>("Games/01_MakeBubbles/Prefabs/Bubble");
            if (bubblePrefab != null)
            {
                Debug.Log("プレハブフォルダからバブルプレハブを読み込みました");
            }
            else
            {
                // Resourcesフォルダにない場合は、シーン内のBubbleオブジェクトを検索
                GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
                foreach (GameObject obj in allObjects)
                {
                    if (obj.name.Contains("Bubble") && obj.CompareTag("Bubble"))
                    {
                        bubblePrefab = obj;
                        Debug.Log("シーン内でバブルプレハブを見つけました: " + obj.name);
                        break;
                    }
                }
                
                // それでも見つからない場合は、BubbleControllerを持つオブジェクトを検索
                if (bubblePrefab == null)
                {
                    BubbleController[] bubbleControllers = FindObjectsOfType<BubbleController>();
                    if (bubbleControllers.Length > 0)
                    {
                        bubblePrefab = bubbleControllers[0].gameObject;
                        Debug.Log("BubbleControllerを持つオブジェクトをバブルプレハブとして使用: " + bubblePrefab.name);
                    }
                }
                
                // 最終手段として新しく作成
                if (bubblePrefab == null)
                {
                    Debug.LogWarning("バブルプレハブが見つからないため、新規作成します");
                    CreateDefaultBubblePrefab();
                }
            }
        }
        else
        {
            Debug.Log("バブルプレハブは既に設定されています: " + bubblePrefab.name);
        }
        
        // BubbleMakerManagerにバブルプレハブを設定
        if (BubbleMakerManager.Instance != null && bubblePrefab != null)
        {
            BubbleMakerManager.Instance.SetBubblePrefab(bubblePrefab);
            Debug.Log("BubbleMakerManagerにバブルプレハブを設定しました");
        }

        if (BubbleMakerManager.Instance != null)
        {
            BubbleMakerManager.Instance.SetStarBubblePrefab(starBubblePrefab);
            BubbleMakerManager.Instance.SetStarBubbleChance(starBubbleChance);
        }
    }
    
    /// <summary>
    /// デフォルトのバブルプレハブを作成
    /// </summary>
    private void CreateDefaultBubblePrefab()
    {
        bubblePrefab = new GameObject("Bubble");
        
        // SpriteRendererを追加
        SpriteRenderer spriteRenderer = bubblePrefab.AddComponent<SpriteRenderer>();
        
        // Circle.pngスプライトを検索して設定
        Sprite[] allSprites = Resources.FindObjectsOfTypeAll<Sprite>();
        foreach (Sprite sprite in allSprites)
        {
            if (sprite.name == "Circle" || sprite.name.Contains("Bubble"))
            {
                spriteRenderer.sprite = sprite;
                Debug.Log("バブル用スプライトを設定しました: " + sprite.name);
                break;
            }
        }
        
        // 物理コンポーネントを追加
        CircleCollider2D collider = bubblePrefab.AddComponent<CircleCollider2D>();
        collider.isTrigger = false;
        
        Rigidbody2D rb = bubblePrefab.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0.1f;
        rb.linearDamping = 0.5f;
        
        // BubbleControllerを追加
        bubblePrefab.AddComponent<BubbleController>();
        
        // タグを設定
        bubblePrefab.tag = "Bubble";
        
        // 非表示にしておく
        bubblePrefab.SetActive(false);
        
        Debug.Log("デフォルトのバブルプレハブを作成しました");
    }
    
    /// <summary>
    /// バブルプレハブを取得するメソッド
    /// </summary>
    public GameObject GetBubblePrefab()
    {
        return bubblePrefab;
    }
}
