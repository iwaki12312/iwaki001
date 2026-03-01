using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

/// <summary>
/// 個別のキノコを制御するクラス
/// シルエット状態→タップ→収穫アニメーション→カゴへ飛ぶ
/// </summary>
public class MushroomController : MonoBehaviour
{
    [Header("キノコデータ")]
    [SerializeField] private MushroomPickingData mushroomData;

    [Header("設定")]
    [SerializeField] private float colliderRadius = 0.8f;
    [SerializeField] private float hideTimeout = 7f;        // タップしないと引っ込むまでの時間
    [SerializeField] private float growDuration = 0.5f;      // にょきっと生える時間

    private SpriteRenderer spriteRenderer;
    private CircleCollider2D circleCollider;
    private Camera mainCamera;

    private bool isSilhouette = true;           // シルエット状態か
    private bool isAnimating = false;           // アニメーション中か
    private bool isPickedUp = false;            // 収穫済みか
    private Vector3 originalScale;
    private Vector3 spawnPosition;
    private float spawnTime;

    // カゴ参照
    private Transform basketTransform;

    // 削除時のイベント（Spawnerに通知）
    public System.Action OnPickedUp;
    public System.Action OnHidden;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }

        circleCollider = GetComponent<CircleCollider2D>();
        if (circleCollider == null)
        {
            circleCollider = gameObject.AddComponent<CircleCollider2D>();
        }

        circleCollider.radius = colliderRadius;
        mainCamera = Camera.main;
    }

    /// <summary>
    /// キノコを初期化
    /// </summary>
    public void Initialize(MushroomPickingData data, Vector3 position,
                           Transform basket, float baseScale, float colRadius, float timeout)
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null) spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        if (circleCollider == null)
        {
            circleCollider = GetComponent<CircleCollider2D>();
            if (circleCollider == null) circleCollider = gameObject.AddComponent<CircleCollider2D>();
        }
        if (mainCamera == null) mainCamera = Camera.main;

        mushroomData = data;
        spawnPosition = position;
        transform.position = position;
        basketTransform = basket;
        colliderRadius = colRadius;
        hideTimeout = timeout;
        circleCollider.radius = colliderRadius;

        // スケール設定
        float scale = baseScale;
        if (mushroomData != null && mushroomData.isRare)
        {
            scale *= 1.2f;
        }
        transform.localScale = Vector3.one * scale;
        originalScale = transform.localScale;

        // シルエットモードで表示（黒く表示）
        isSilhouette = true;
        isAnimating = false;
        isPickedUp = false;
        spriteRenderer.sortingOrder = 10;

        if (mushroomData != null && mushroomData.mushroomSprite != null)
        {
            spriteRenderer.sprite = mushroomData.mushroomSprite;
        }

        // シルエット表示（黒色で表示）
        spriteRenderer.color = new Color(0.1f, 0.1f, 0.1f, 1f);

        // レアの場合はシルエットが少し光る
        if (mushroomData != null && mushroomData.isRare)
        {
            spriteRenderer.color = new Color(0.2f, 0.15f, 0.05f, 1f);
        }

        // にょきっと生えるアニメーション
        PlayGrowAnimation();

        spawnTime = Time.time;
        Debug.Log($"[MushroomController] キノコスポーン: {mushroomData?.mushroomType}, レア={mushroomData?.isRare}");
    }

    /// <summary>
    /// にょきっと生えるアニメーション
    /// </summary>
    private void PlayGrowAnimation()
    {
        isAnimating = true;

        // 下から生えてくる：初期は小さく、成長する
        transform.localScale = new Vector3(originalScale.x, 0f, originalScale.z);
        Vector3 startPos = spawnPosition + Vector3.down * 0.3f;
        transform.position = startPos;

        Sequence growSeq = DOTween.Sequence();
        growSeq.Append(transform.DOScaleY(originalScale.y, growDuration).SetEase(Ease.OutBack));
        growSeq.Join(transform.DOMove(spawnPosition, growDuration).SetEase(Ease.OutBack));
        growSeq.OnComplete(() =>
        {
            isAnimating = false;
        });

        // 生えてくる効果音
        if (MushroomPickingSFXPlayer.Instance != null)
        {
            MushroomPickingSFXPlayer.Instance.PlayGrowSound();
        }

        // レア出現予告エフェクト
        if (mushroomData != null && mushroomData.isRare)
        {
            if (MushroomPickingSFXPlayer.Instance != null)
            {
                MushroomPickingSFXPlayer.Instance.PlayRareAppearSound();
            }
        }
    }

    void Update()
    {
        if (isPickedUp || isAnimating) return;

        // タイムアウトチェック
        if (isSilhouette && Time.time - spawnTime > hideTimeout + growDuration)
        {
            HideBack();
            return;
        }

        HandleTouch();
    }

    /// <summary>
    /// タッチ処理（マルチタッチ対応）
    /// </summary>
    private void HandleTouch()
    {
        // タッチスクリーン入力
        var touchscreen = Touchscreen.current;
        if (touchscreen != null)
        {
            foreach (var touch in touchscreen.touches)
            {
                if (!touch.press.wasPressedThisFrame) continue;

                Vector3 worldPos = mainCamera.ScreenToWorldPoint(touch.position.ReadValue());
                worldPos.z = 0;

                if (circleCollider.OverlapPoint(worldPos))
                {
                    OnTapped();
                    return;
                }
            }
        }

        // エディタ用：マウスクリック
        var mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
        {
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(mouse.position.ReadValue());
            worldPos.z = 0;

            if (circleCollider.OverlapPoint(worldPos))
            {
                OnTapped();
            }
        }
    }

    /// <summary>
    /// タップされたときの処理
    /// </summary>
    private void OnTapped()
    {
        if (isAnimating || isPickedUp) return;

        isAnimating = true;
        isPickedUp = true;

        // 効果音再生
        if (MushroomPickingSFXPlayer.Instance != null)
        {
            AudioClip clip = mushroomData?.pickSound;
            bool isRare = mushroomData != null && mushroomData.isRare;
            MushroomPickingSFXPlayer.Instance.PlayPickSound(clip, isRare);
        }

        // 収穫アニメーション開始
        PlayPickAnimation();
    }

    /// <summary>
    /// 収穫アニメーション：
    /// 1. 上に跳ね上がりつつ回転
    /// 2. シルエット解除（色が戻る）
    /// 3. カゴに飛んでいく
    /// </summary>
    private void PlayPickAnimation()
    {
        float jumpHeight = mushroomData != null ? mushroomData.jumpHeight : 1.5f;
        float revealDuration = mushroomData != null ? mushroomData.revealDuration : 0.6f;
        float flyDuration = mushroomData != null ? mushroomData.flyToBasketDuration : 0.8f;
        float spinSpeed = mushroomData != null ? mushroomData.spinSpeed : 720f;
        bool isRare = mushroomData != null && mushroomData.isRare;

        Sequence pickSeq = DOTween.Sequence();

        // フェーズ1: 上に跳ね上がり + 回転
        Vector3 jumpTarget = transform.position + Vector3.up * jumpHeight;

        pickSeq.Append(transform.DOMove(jumpTarget, revealDuration * 0.6f).SetEase(Ease.OutQuad));

        // スケールの弾み
        pickSeq.Join(transform.DOScale(originalScale * 1.3f, revealDuration * 0.3f)
            .SetEase(Ease.OutBack));

        // 回転（Z軸）- 360°の倍数にして元の角度に戻す
        int fullRotations = Mathf.Max(1, Mathf.RoundToInt(spinSpeed * revealDuration / 360f));
        float targetRotation = fullRotations * 360f;
        pickSeq.Join(transform.DORotate(new Vector3(0, 0, targetRotation), revealDuration,
            RotateMode.FastBeyond360).SetEase(Ease.OutQuad));

        // フェーズ2: シルエット解除（色を元に戻す）
        pickSeq.Join(spriteRenderer.DOColor(Color.white, revealDuration * 0.5f)
            .SetDelay(revealDuration * 0.2f));

        // スケールを戻す
        pickSeq.Append(transform.DOScale(originalScale, revealDuration * 0.3f).SetEase(Ease.InOutQuad));

        // レアの場合、特別エフェクト
        if (isRare)
        {
            pickSeq.AppendCallback(() => PlayRareRevealEffect());
            pickSeq.AppendInterval(0.3f);
        }

        // フェーズ3: カゴに飛んでいく
        if (basketTransform != null)
        {
            pickSeq.Append(transform.DOMove(basketTransform.position, flyDuration)
                .SetEase(Ease.InBack));
            pickSeq.Join(transform.DOScale(originalScale * 0.3f, flyDuration)
                .SetEase(Ease.InQuad));
            pickSeq.Join(transform.DORotate(new Vector3(0, 0, 360f), flyDuration,
                RotateMode.FastBeyond360));
        }
        else
        {
            // カゴがない場合はフェードアウト
            pickSeq.Append(spriteRenderer.DOFade(0f, flyDuration));
        }

        pickSeq.OnComplete(() =>
        {
            // カゴに入る音
            if (MushroomPickingSFXPlayer.Instance != null)
            {
                MushroomPickingSFXPlayer.Instance.PlayBasketSound();
            }

            // カゴの揺れアニメーション
            if (basketTransform != null)
            {
                BasketController basket = basketTransform.GetComponent<BasketController>();
                if (basket != null)
                {
                    basket.OnMushroomCollected();
                }
            }

            OnPickedUp?.Invoke();
            Destroy(gameObject);
        });
    }

    /// <summary>
    /// レアキノコの特別な明かし演出
    /// </summary>
    private void PlayRareRevealEffect()
    {
        // カメラシェイク
        if (mainCamera != null)
        {
            mainCamera.transform.DOShakePosition(0.3f, 0.1f, 10, 90, false, true);
        }

        // 虹色フラッシュ
        Sequence colorSeq = DOTween.Sequence();
        colorSeq.Append(spriteRenderer.DOColor(new Color(1f, 0.8f, 0.8f), 0.05f));
        colorSeq.Append(spriteRenderer.DOColor(new Color(1f, 1f, 0.5f), 0.05f));
        colorSeq.Append(spriteRenderer.DOColor(new Color(0.5f, 1f, 0.5f), 0.05f));
        colorSeq.Append(spriteRenderer.DOColor(new Color(0.5f, 0.8f, 1f), 0.05f));
        colorSeq.Append(spriteRenderer.DOColor(Color.white, 0.05f));
    }

    /// <summary>
    /// タップされずに引っ込むアニメーション
    /// </summary>
    private void HideBack()
    {
        if (isAnimating || isPickedUp) return;

        isAnimating = true;

        // 引っ込む効果音
        if (MushroomPickingSFXPlayer.Instance != null)
        {
            MushroomPickingSFXPlayer.Instance.PlayHideSound();
        }

        Sequence hideSeq = DOTween.Sequence();
        hideSeq.Append(transform.DOScaleY(0f, 0.4f).SetEase(Ease.InBack));
        hideSeq.Join(transform.DOMove(spawnPosition + Vector3.down * 0.3f, 0.4f).SetEase(Ease.InBack));
        hideSeq.OnComplete(() =>
        {
            OnHidden?.Invoke();
            Destroy(gameObject);
        });
    }

    void OnDestroy()
    {
        DOTween.Kill(transform);
        if (spriteRenderer != null) DOTween.Kill(spriteRenderer);
    }
}
