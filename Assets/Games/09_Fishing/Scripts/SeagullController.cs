using UnityEngine;
using DG.Tweening;

/// <summary>
/// カモメの制御クラス
/// 画面外から飛んできて魚を掴んで画面外に消える
/// </summary>
public class SeagullController : MonoBehaviour
{
    [Header("飛行設定")]
    [SerializeField] private float flySpeed = 5.0f;          // 飛行速度
    [SerializeField] private float grabDuration = 0.3f;      // 魚を掴むまでの時間
    
    private SpriteRenderer spriteRenderer;
    private FishController targetFish;
    private bool hasGrabbed = false;
    
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
    }
    
    /// <summary>
    /// 初期化
    /// </summary>
    public void Initialize(Vector3 fishPosition, FishController fish)
    {
        Debug.Log($"[SeagullController] Initialize: fishPos={fishPosition}");
        targetFish = fish;
        
        // 画面外の開始位置（魚の上方、画面外）
        Vector3 startPos = new Vector3(fishPosition.x + 5f, fishPosition.y + 10f, 0);
        transform.position = startPos;
        
        // カモメ出現音を再生
        if (FishingSFXPlayer.Instance != null)
        {
            FishingSFXPlayer.Instance.PlaySeagullAppear();
            Debug.Log("[SeagullController] カモメ出現音再生");
        }
        
        // 飛行アニメーション開始
        StartFlyAnimation(fishPosition);
    }
    
    /// <summary>
    /// 飛行アニメーション
    /// </summary>
    private void StartFlyAnimation(Vector3 fishPosition)
    {
        Sequence flySeq = DOTween.Sequence();
        
        // 1. 魚の位置まで飛ぶ
        flySeq.Append(transform.DOMove(fishPosition, grabDuration).SetEase(Ease.Linear));
        
        // 2. 魚を掴む
        flySeq.AppendCallback(() => {
            GrabFish();
        });
        
        // 3. カモメの鳴き声
        flySeq.AppendCallback(() => {
            if (FishingSFXPlayer.Instance != null)
            {
                FishingSFXPlayer.Instance.PlaySeagullCry();
            }
        });
        
        // 4. 画面外へ飛んでいく
        Vector3 exitPos = new Vector3(fishPosition.x + 10f, fishPosition.y + 10f, 0);
        flySeq.Append(transform.DOMove(exitPos, 1.0f).SetEase(Ease.InQuad));
        
        // 5. 完了後に削除
        flySeq.OnComplete(() => {
            Destroy(gameObject);
        });
    }
    
    /// <summary>
    /// 魚を掴む処理
    /// </summary>
    private void GrabFish()
    {
        if (hasGrabbed) return;
        hasGrabbed = true;
        
        if (targetFish != null)
        {
            // 魚に奪われたことを通知
            targetFish.OnStolenBySeagull();
        }
    }
}
