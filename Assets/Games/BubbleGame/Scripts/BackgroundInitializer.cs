using UnityEngine;

/// <summary>
/// バブルゲームの背景画像を初期化するクラス
/// </summary>
public class BackgroundInitializer : MonoBehaviour
{
    [SerializeField] private Sprite backgroundSprite;
    
    void Start()
    {
        // 背景画像が設定されていない場合は、プロジェクト内から検索
        if (backgroundSprite == null)
        {
            Debug.Log("背景画像が設定されていません。プロジェクト内から検索します。");
            
            // プロジェクト内の背景画像を直接検索
            Sprite[] allSprites = Resources.FindObjectsOfTypeAll<Sprite>();
            foreach (Sprite sprite in allSprites)
            {
                if (sprite.name.Contains("BackGround"))
                {
                    backgroundSprite = sprite;
                    Debug.Log("背景画像を見つけました: " + sprite.name);
                    break;
                }
            }
            
            if (backgroundSprite == null)
            {
                Debug.LogError("背景画像が見つかりません");
                return;
            }
        }
        
        // 背景用のGameObjectを検索
        GameObject background = GameObject.FindGameObjectWithTag("Background");
        if (background == null)
        {
            // 背景用のGameObjectが見つからない場合は、名前で検索
            background = GameObject.Find("Background");
            
            // それでも見つからない場合は新しく作成
            if (background == null)
            {
                Debug.Log("背景用のGameObjectが見つからないため、新しく作成します");
                background = new GameObject("Background");
                background.tag = "Background";
                
                // カメラの後ろに配置
                background.transform.position = new Vector3(0, 0, 10);
                
                // SpriteRendererコンポーネントを追加
                SpriteRenderer renderer = background.AddComponent<SpriteRenderer>();
                renderer.sortingOrder = -100; // 最背面に表示
                
                // カメラに合わせてスケールを調整
                Camera mainCamera = Camera.main;
                if (mainCamera != null)
                {
                    float height = mainCamera.orthographicSize * 2;
                    float width = height * mainCamera.aspect;
                    
                    // スプライトのサイズを取得
                    float spriteWidth = backgroundSprite.bounds.size.x;
                    float spriteHeight = backgroundSprite.bounds.size.y;
                    
                    // スケールを計算
                    float scaleX = width / spriteWidth;
                    float scaleY = height / spriteHeight;
                    
                    // 大きい方のスケールを使用して、画面全体をカバー
                    float scale = Mathf.Max(scaleX, scaleY);
                    background.transform.localScale = new Vector3(scale, scale, 1);
                }
            }
        }
        
        // 背景画像を設定
        SpriteRenderer spriteRenderer = background.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = backgroundSprite;
            Debug.Log("背景画像を設定しました: " + backgroundSprite.name);
        }
        else
        {
            Debug.LogError("背景用のGameObjectにSpriteRendererコンポーネントがありません");
        }
    }
}
