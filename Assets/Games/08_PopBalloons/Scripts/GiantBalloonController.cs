using UnityEngine;

/// <summary>
/// ジャイアントバルーンを制御するクラス
/// BalloonControllerを継承して巨大化と特別な演出を追加
/// </summary>
public class GiantBalloonController : BalloonController
{
    [Header("ジャイアント設定")]
    [SerializeField] private float giantScale = 2.5f;      // 通常の2.5倍
    [SerializeField] private float giantSpeedMultiplier = 0.7f; // 速度を0.7倍に
    
    public override void Initialize(Sprite sprite, Vector3 position, GameObject particlePrefab)
    {
        base.Initialize(sprite, position, particlePrefab);
        
        // スケールを拡大
        transform.localScale = Vector3.one * giantScale;
        
        // 速度を調整
        floatSpeed *= giantSpeedMultiplier;
        rb.linearVelocity = new Vector2(Random.Range(-swayAmount, swayAmount), floatSpeed);
        
        // Colliderのサイズも調整
        if (circleCollider != null)
        {
            circleCollider.radius = 0.5f * giantScale;
        }
    }
    
    protected override void Pop()
    {
        if (isPopped) return;
        isPopped = true;
        
        // 星のパーティクルを3倍の量で生成
        if (starParticlePrefab != null)
        {
            for (int i = 0; i < 3; i++)
            {
                Vector3 offset = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0);
                GameObject particle = Instantiate(starParticlePrefab, transform.position + offset, Quaternion.identity);
                Destroy(particle, 2f);
            }
        }
        
        // ジャイアント専用の効果音を再生
        if (PopBalloonsSFXPlayer.Instance != null)
        {
            PopBalloonsSFXPlayer.Instance.PlayPopGiant();
        }
        
        // 即座に削除
        Destroy(gameObject);
    }
}
