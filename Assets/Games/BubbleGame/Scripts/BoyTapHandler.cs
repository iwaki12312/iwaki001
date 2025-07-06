using UnityEngine;

public class BoyTapHandler : MonoBehaviour
{
    [SerializeField] private GameObject bubblePrefab;
    [SerializeField] private float spawnOffset = -1f;
    [SerializeField] private float bubbleSpeed = 5f;

    private void Start()
    {
        // Prefabの参照が設定されていない場合は、Resources.Loadを試みる
        if (bubblePrefab == null)
        {
            bubblePrefab = Resources.Load<GameObject>("Prefabs/Bubble");
            if (bubblePrefab == null)
            {
                Debug.LogError("バブルプレハブが見つかりません。インスペクターで設定してください。");
            }
            else
            {
                Debug.Log("バブルプレハブをResourcesから読み込みました。");
            }
        }
    }

    private void Update()
    {
        // マウスクリックまたはタッチ入力を検出
        if (Input.GetMouseButtonDown(0))
        {
            // マウス位置をワールド座標に変換
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;

            // クリックがこのオブジェクトのコライダー内かどうかを確認
            Collider2D collider = GetComponent<Collider2D>();
            if (collider != null && collider.OverlapPoint(mousePos))
            {
                SpawnBubble();
                Debug.Log("少年をタップしました！シャボン玉を生成します。");
            }
        }
    }

    // シャボン玉を生成するメソッド
    private void SpawnBubble()
    {
        if (bubblePrefab != null)
        {
            // 効果音を再生
            AudioManager.Instance.PlayShotSound();
            
            // 少年の左上から少し離れた位置にシャボン玉を生成
            Vector3 spawnPosition = transform.position + new Vector3(spawnOffset, 1.5f, 0);
            GameObject bubble = Instantiate(bubblePrefab, spawnPosition, Quaternion.identity);
            
            // ランダムなサイズを設定
            float size = Random.Range(0.5f, 1.5f);
            bubble.transform.localScale = new Vector3(size, size, 1f);
            
            // ランダムな色を設定
            Color randomColor = new Color(
                Random.Range(0.7f, 1f),
                Random.Range(0.7f, 1f),
                Random.Range(0.7f, 1f),
                0.8f
            );
            SpriteRenderer renderer = bubble.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.color = randomColor;
            }
            
            // シャボン玉に左上方向への初速度を設定
            Rigidbody2D rb = bubble.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = new Vector2(-1.5f, 1f) * bubbleSpeed;
            }
            
            // BubbleControllerの処理も適用される
            Debug.Log("シャボン玉を生成しました。BubbleControllerの初期設定が適用されます。");
        }
        else
        {
            Debug.LogError("バブルプレハブが設定されていません。");
        }
    }
}
