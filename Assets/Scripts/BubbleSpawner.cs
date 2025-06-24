using UnityEngine;

// 画面タップ位置にシャボン玉を生成するクラス
public class BubbleSpawner : MonoBehaviour
{
    [SerializeField] private GameObject bubblePrefab; // シャボン玉プレハブ
    [SerializeField] private float minBubbleSize = 0.5f; // 最小サイズ
    [SerializeField] private float maxBubbleSize = 1.5f; // 最大サイズ

    void Update()
    {
        // タップ（クリック）された時
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("マウスクリックを検出しました");
            
            // メインカメラの確認
            if (Camera.main == null)
            {
                Debug.LogError("メインカメラが見つかりません");
                return;
            }

            // タップ位置をワールド座標に変換
            Vector2 touchPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Debug.Log($"タップ位置: {touchPos}");

            // タップ位置にシャボン玉があるか確認
            RaycastHit2D hit = Physics2D.Raycast(touchPos, Vector2.zero);
            if (hit.collider != null && hit.collider.CompareTag("Bubble"))
            {
                Debug.Log("シャボン玉上でのクリックのため、新しい生成をスキップします");
                return;
            }
            
            // プレハブの確認
            if (bubblePrefab == null)
            {
                Debug.LogError("シャボン玉プレハブが設定されていません");
                return;
            }
            
            // シャボン玉を生成
            SpawnBubble(touchPos);
            Debug.Log($"シャボン玉を生成しました: 位置={touchPos}");
        }
    }

    // シャボン玉を生成するメソッド
    private void SpawnBubble(Vector2 position)
    {
        // ランダムなサイズを決定
        float size = Random.Range(minBubbleSize, maxBubbleSize);
        
        // シャボン玉を生成
        GameObject bubble = Instantiate(bubblePrefab, position, Quaternion.identity);
        
        // ランダムなサイズを適用
        bubble.transform.localScale = new Vector3(size, size, 1f);
        
        // ランダムな色を設定（オプション）
        Color randomColor = new Color(
            Random.Range(0.7f, 1f),
            Random.Range(0.7f, 1f),
            Random.Range(0.7f, 1f),
            0.8f
        );
        bubble.GetComponent<SpriteRenderer>().color = randomColor;
    }
}
