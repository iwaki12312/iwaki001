using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// メニュー画面の制御を行うクラス
/// </summary>
public class MenuController : MonoBehaviour
{
    void Start()
    {
        // Bubbleオブジェクトを検索
        GameObject bubbleObj = GameObject.Find("Bubble");
        if (bubbleObj != null)
        {
            // BoxCollider2Dがなければ追加
            if (bubbleObj.GetComponent<BoxCollider2D>() == null)
            {
                BoxCollider2D collider = bubbleObj.AddComponent<BoxCollider2D>();
                // スプライトのサイズに合わせる
                SpriteRenderer spriteRenderer = bubbleObj.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    collider.size = spriteRenderer.sprite.bounds.size;
                }
            }

            // GameButtonコンポーネントを追加
            GameButton bubbleButton = bubbleObj.AddComponent<GameButton>();
            bubbleButton.sceneName = "BubbleGame";
            bubbleButton.Initialize();
        }
        else
        {
            Debug.LogError("Bubbleオブジェクトが見つかりません");
        }

        // Moleオブジェクトを検索
        GameObject moleObj = GameObject.Find("Mole");
        if (moleObj != null)
        {
            // BoxCollider2Dがなければ追加
            if (moleObj.GetComponent<BoxCollider2D>() == null)
            {
                BoxCollider2D collider = moleObj.AddComponent<BoxCollider2D>();
                // スプライトのサイズに合わせる
                SpriteRenderer spriteRenderer = moleObj.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    collider.size = spriteRenderer.sprite.bounds.size;
                }
            }

            // GameButtonコンポーネントを追加
            GameButton moleButton = moleObj.AddComponent<GameButton>();
            moleButton.sceneName = "MoleGame";
            moleButton.Initialize();
        }
        else
        {
            Debug.LogError("Moleオブジェクトが見つかりません");
        }

        // 03_FlowerBloomingオブジェクトを検索
        GameObject flowerObj = GameObject.Find("03_FlowerBlooming");
        if (flowerObj != null)
        {
            // BoxCollider2Dがなければ追加
            if (flowerObj.GetComponent<BoxCollider2D>() == null)
            {
                BoxCollider2D collider = flowerObj.AddComponent<BoxCollider2D>();
                // スプライトのサイズに合わせる
                SpriteRenderer spriteRenderer = flowerObj.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    collider.size = spriteRenderer.sprite.bounds.size;
                }
            }
            // GameButtonコンポーネントを追加
            GameButton flowerButton = flowerObj.AddComponent<GameButton>();
            flowerButton.sceneName = "FlowerBlooming";
            flowerButton.Initialize();
        }
        else
        {
            Debug.LogError("03_FlowerBloomingオブジェクトが見つかりません");
        }

        // 04_Cookオブジェクトを検索
        GameObject cookObj = GameObject.Find("04_Cook");
        if (cookObj != null)
        {
            // BoxCollider2Dがなければ追加
            if (cookObj.GetComponent<BoxCollider2D>() == null)
            {
                BoxCollider2D collider = cookObj.AddComponent<BoxCollider2D>();
                // スプライトのサイズに合わせる
                SpriteRenderer spriteRenderer = cookObj.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    collider.size = spriteRenderer.sprite.bounds.size;
                }
            }

            // GameButtonコンポーネントを追加
            GameButton cookButton = cookObj.AddComponent<GameButton>();
            cookButton.sceneName = "Cook";
            cookButton.Initialize();
        }
        else
        {
            Debug.LogError("04_Cookオブジェクトが見つかりません");
        }
    }
}

/// <summary>
/// ゲームボタンの機能を提供するコンポーネント
/// </summary>
public class GameButton : MonoBehaviour
{
    public string sceneName; // 遷移先のシーン名
    private bool isInitialized = false;

    // ハイライト表示用の元の色と強調色
    private Color originalColor;
    private Color highlightColor;

    // SpriteRendererコンポーネント
    private SpriteRenderer spriteRenderer;

    Camera mainCam;
    Collider2D myCol;

    /// <summary>
    /// 初期化処理
    /// </summary>
    public void Initialize()
    {
        if (isInitialized) return;

        // SpriteRendererを取得
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            // 元の色を保存
            originalColor = spriteRenderer.color;

            // 強調色を設定（元の色より明るく）
            highlightColor = new Color(
                Mathf.Min(originalColor.r * 1.2f, 1f),
                Mathf.Min(originalColor.g * 1.2f, 1f),
                Mathf.Min(originalColor.b * 1.2f, 1f),
                originalColor.a
            );
        }

        mainCam = Camera.main;
        myCol = GetComponent<Collider2D>();

        isInitialized = true;
    }

    void Update()
    {
        if (!mainCam || !myCol) return;

        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch t = Input.GetTouch(i);
            if (t.phase != TouchPhase.Began) continue;

            Vector2 world = mainCam.ScreenToWorldPoint(t.position);
            if (myCol.OverlapPoint(world))
            {
                LoadGame();
                break;                       // 複数回呼ばれないように
            }
        }
    }

    void OnMouseEnter()
    {
        // マウスが重なったら色を変える
        if (spriteRenderer != null)
        {
            spriteRenderer.color = highlightColor;
        }

        // カーソルを変更
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    void OnMouseExit()
    {
        // マウスが離れたら元の色に戻す
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }

        // カーソルを元に戻す
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    void OnMouseDown()
    {
        // クリック時にシーン遷移
        LoadGame();
    }

    /// <summary>
    /// ゲームシーンをロードする
    /// </summary>
    public void LoadGame()
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("シーン名が設定されていません");
            return;
        }

        Debug.Log($"ゲームをロード: {sceneName}");

        // シーン遷移
        SceneManager.LoadScene(sceneName);
    }
}
