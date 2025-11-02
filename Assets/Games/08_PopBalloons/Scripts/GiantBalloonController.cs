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
    [SerializeField] private GameObject giantParticlePrefab;   // ジャイアント専用パーティクル(未設定時はstarParticlePrefabを使用)
    
    public override void Initialize(Sprite sprite, Vector3 position, GameObject particlePrefab)
    {
        base.Initialize(sprite, position, particlePrefab);
        
        // 基準サイズから巨大化
        transform.localScale = transform.localScale * giantScale;
        
        // 速度を調整
        floatSpeed *= giantSpeedMultiplier;
        rb.linearVelocity = new Vector2(Random.Range(-swayAmount, swayAmount), floatSpeed);
        
        // Colliderのサイズも巨大化に合わせて拡大
        if (circleCollider != null)
        {
            circleCollider.radius = colliderRadius * giantScale;
        }
    }
    
    protected override void Pop()
    {
        if (isPopped) return;
        isPopped = true;
        
        // 使用するパーティクルを決定(giantParticlePrefab優先、未設定時はstarParticlePrefab)
        GameObject particleToUse = giantParticlePrefab != null ? giantParticlePrefab : starParticlePrefab;
        
        // 星のパーティクルを3倍の量で生成
        if (particleToUse != null)
        {
            for (int i = 0; i < 3; i++)
            {
                Vector3 offset = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0);
                GameObject particle = Instantiate(particleToUse, transform.position + offset, Quaternion.identity);
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
