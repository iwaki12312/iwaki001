using UnityEngine;

public class BoyTapHandler : MonoBehaviour
{
    [SerializeField] private GameObject bubblePrefab;
    [SerializeField] private float spawnOffset = 1f; // 少年から少し離れた位置に生成
    [SerializeField] private float bubbleSpeed = 5f; // 発射速度

    private void OnMouseDown()
    {
        // メインカメラの確認
        if (Camera.main == null)
        {
            Debug.LogError("メインカメラが見つかりません");
            return;
        }

        // プレハブの確認
        if (bubblePrefab == null)
        {
            Debug.LogError("シャボン玉プレハブが設定されていません");
            return;
        }

        // 少年の位置を取得
        Vector2 boyPosition = transform.position;
        
        // 画面中央方向へのベクトルを計算
        Vector2 screenCenter = Camera.main.ViewportToWorldPoint(new Vector2(0.5f, 1.5f));
        Vector2 direction = (screenCenter - boyPosition).normalized;
        
        // 少年から少し離れた位置を計算
        Vector2 spawnPosition = boyPosition + direction * spawnOffset;
        
        // シャボン玉を生成
        GameObject bubble = Instantiate(bubblePrefab, spawnPosition, Quaternion.identity);
        
        // ランダムなサイズを設定 (0.5～1.5)
        float size = Random.Range(0.5f, 1.5f);
        bubble.transform.localScale = new Vector3(size, size, 1f);
        
        // ランダムな色を設定
        SpriteRenderer renderer = bubble.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.color = new Color(
                Random.Range(0.7f, 1f),
                Random.Range(0.7f, 1f),
                Random.Range(0.7f, 1f),
                0.8f
            );
        }
        
        // シャボン玉に速度を設定
        Rigidbody2D rb = bubble.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = direction * bubbleSpeed;
        }
    }
}
