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
    [SerializeField] private float grabDuration = 0.3f;      // 魚を掘むまでの時間
    
    [Header("魚の保持位置設定")]
    [SerializeField] private Vector3 fishHoldOffset = new Vector3(0, -1.5f, 0);  // カモメの足の位置（魚をくわえる位置）
    
    private SpriteRenderer spriteRenderer;
    private FishController targetFish;
    private bool hasGrabbed = false;
    private bool forceGrabScheduled = false;
    
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

        // 念のため、Tweenが途中で止まった場合でも一定時間後に掴む
        if (!forceGrabScheduled)
        {
            forceGrabScheduled = true;
            DOVirtual.DelayedCall(grabDuration + 0.05f, ForceGrabIfNeeded);
        }
    }
    
    /// <summary>
    /// 飛行アニメーション
    /// </summary>
    private void StartFlyAnimation(Vector3 fishPosition)
    {
        Sequence flySeq = DOTween.Sequence();
        
        // 1. 魚の位置まで飛ぶ（カモメの位置は魚の位置に固定）
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
        
        // 4. 魚を掴んだまま画面外へ飛んでいく
        Vector3 exitPos = new Vector3(fishPosition.x + 10f, fishPosition.y + 10f, 0);
        flySeq.Append(transform.DOMove(exitPos, 1.0f).SetEase(Ease.InQuad));
        
        // 5. 完了後に削除（魚も一緒に削除される）
        flySeq.OnComplete(() => {
            if (targetFish != null)
            {
                Destroy(targetFish.gameObject);
            }
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
        
        Debug.Log("[SeagullController] GrabFish開始");
        
        if (targetFish != null)
        {
            Debug.Log($"[SeagullController] 魚発見: {targetFish.name}");
            
            // 魚のコライダーを無効化（再度タップされないように）
            CircleCollider2D fishCollider = targetFish.GetComponent<CircleCollider2D>();
            if (fishCollider != null)
            {
                fishCollider.enabled = false;
                Debug.Log("[SeagullController] 魚のコライダー無効化");
            }

            // カモメが掴むタイミングで再表示（事前にFish側で一時非表示）
            SpriteRenderer fishRenderer = targetFish.GetSpriteRenderer();
            if (fishRenderer != null)
            {
                fishRenderer.enabled = true;
                var color = fishRenderer.color;
                color.a = 1f;
                fishRenderer.color = color;
            }
            
            // 魚の進行中のアニメーションを停止
            DOTween.Kill(targetFish.transform);
            DOTween.Kill(targetFish.GetComponent<SpriteRenderer>());
            Debug.Log("[SeagullController] 魚のアニメーション停止");
            
            // 魚をカモメの子オブジェクトにする（カモメと一緒に移動）
            targetFish.transform.SetParent(transform);
            Debug.Log($"[SeagullController] 魚を子オブジェクト化: parent={targetFish.transform.parent.name}");

            // 魚をカモメの足の位置に配置（毎フレーム補正でズレ防止）
            ApplyFishHoldPosition();
            
            // 釣り人を待機状態に戻す
            if (FishermanController.Instance != null)
            {
                FishermanController.Instance.ReturnToIdle();
                Debug.Log("[SeagullController] 釣り人を待機状態に戻しました");
            }
            
            Debug.Log("[SeagullController] GrabFish完了");
        }
        else
        {
            Debug.LogWarning("[SeagullController] targetFishがnullです");
        }
    }

    /// <summary>
    /// fishHoldOffsetに基づき魚の位置を更新（親のスケールに依存しない見た目のオフセットにする）
    /// </summary>
    private void ApplyFishHoldPosition()
    {
        if (targetFish == null) return;

        Vector3 adjustedOffset = new Vector3(
            fishHoldOffset.x / (transform.localScale.x != 0 ? transform.localScale.x : 1),
            fishHoldOffset.y / (transform.localScale.y != 0 ? transform.localScale.y : 1),
            fishHoldOffset.z / (transform.localScale.z != 0 ? transform.localScale.z : 1)
        );
        targetFish.transform.localPosition = adjustedOffset;
    }

    /// <summary>
    /// Tweenがキャンセルされた場合でも掴み処理を保証するフォールバック
    /// </summary>
    private void ForceGrabIfNeeded()
    {
        if (!hasGrabbed)
        {
            Debug.Log("[SeagullController] ForceGrab fallback実行");
            GrabFish();
        }
    }

    private void LateUpdate()
    {
        // ほかのTweenやスクリプトが動かしても足元に固定する
        if (hasGrabbed && targetFish != null)
        {
            ApplyFishHoldPosition();
        }
    }
}
