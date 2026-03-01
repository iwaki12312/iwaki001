using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

/// <summary>
/// 個別の雪だるまの3段階進行を管理するクラス
/// タップ1回目: 大雪玉出現 → 2回目: 小雪玉が乗る → 3回目: 装飾が付いて完成 → フェードアウト
/// </summary>
public class SnowmanController : MonoBehaviour
{
    /// <summary>雪だるまの構築段階</summary>
    private enum Phase { BottomBall, TopBall, Complete, FadingOut }

    [Header("スプライト参照（Initializerが設定）")]
    [SerializeField] private Sprite snowballSprite;
    [SerializeField] private Sprite[] decorationSets;  // 完成時の装飾済みスプライト（バリエーション）

    [Header("レア")]
    [SerializeField] private Sprite[] rareDecorationSets; // レア完成スプライト
    [SerializeField, Range(0f, 1f)] private float rareChance = 0.1f;

    [Header("設定")]
    [SerializeField] private float topBallOffsetY = 0.9f;
    [SerializeField] private float topBallScale = 0.7f;
    [SerializeField] private float fadeOutDelay = 2.5f;
    [SerializeField] private float fadeOutDuration = 1.0f;
    [SerializeField] private float wobbleDuration = 2.0f;
    [SerializeField] private float wobbleAngle = 5f;
    [SerializeField] private float colliderRadius = 0.8f;

    /// <summary>雪だるま全体の基本スケール</summary>
    private float baseScale = 1f;

    private Phase currentPhase = Phase.BottomBall;
    private SpriteRenderer bottomBallRenderer;
    private GameObject topBallObj;
    private SpriteRenderer topBallRenderer;
    private SpriteRenderer decoRenderer;
    private CircleCollider2D circleCollider;
    private Camera mainCamera;
    private bool isRare;

    // 完成後のコールバック（SnowmanBuilderControllerに通知）
    public System.Action OnSnowmanDestroyed;

    void Awake()
    {
        mainCamera = Camera.main;
    }

    /// <summary>
    /// 外部から雪玉スプライト等を設定して初期化
    /// </summary>
    public void Initialize(Sprite snowball, Sprite[] decoSets, Sprite[] rareDecoSets,
                           float rare, Vector3 position, float scale = 1f)
    {
        snowballSprite = snowball;
        decorationSets = decoSets;
        rareDecorationSets = rareDecoSets;
        rareChance = rare;
        baseScale = scale;

        transform.position = position;

        // 下の雪玉（胴体）のSpriteRenderer
        bottomBallRenderer = GetComponent<SpriteRenderer>();
        if (bottomBallRenderer == null)
            bottomBallRenderer = gameObject.AddComponent<SpriteRenderer>();

        bottomBallRenderer.sprite = snowballSprite;
        bottomBallRenderer.sortingOrder = 10;

        // コライダー（タップ判定）
        circleCollider = GetComponent<CircleCollider2D>();
        if (circleCollider == null)
            circleCollider = gameObject.AddComponent<CircleCollider2D>();
        circleCollider.radius = colliderRadius;

        // 出現アニメーション
        transform.localScale = Vector3.zero;
        transform.DOScale(Vector3.one * baseScale, 0.3f).SetEase(Ease.OutBack);

        // 雪エフェクト
        SpawnSnowPuff(position);

        // SE
        if (SnowmanBuilderSFXPlayer.Instance != null)
            SnowmanBuilderSFXPlayer.Instance.PlaySnowballAppear();

        currentPhase = Phase.BottomBall;
    }

    void Update()
    {
        if (currentPhase == Phase.FadingOut || currentPhase == Phase.Complete) return;
        HandleTouch();
    }

    /// <summary>
    /// マルチタッチ対応のタップ検出
    /// </summary>
    private void HandleTouch()
    {
        // タッチスクリーン
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

        // マウス（エディタ用）
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

    private void OnTapped()
    {
        switch (currentPhase)
        {
            case Phase.BottomBall:
                StackTopBall();
                break;
            case Phase.TopBall:
                CompleteSnowman();
                break;
        }
    }

    /// <summary>
    /// 2回目タップ: 上に小さな雪玉を載せる
    /// </summary>
    private void StackTopBall()
    {
        currentPhase = Phase.TopBall;

        // 上の雪玉を子オブジェクトとして生成
        topBallObj = new GameObject("TopBall");
        topBallObj.transform.SetParent(transform);
        topBallObj.transform.localPosition = new Vector3(0, topBallOffsetY, 0);
        topBallObj.transform.localScale = Vector3.one * topBallScale;

        topBallRenderer = topBallObj.AddComponent<SpriteRenderer>();
        topBallRenderer.sprite = snowballSprite;
        topBallRenderer.sortingOrder = 11;

        // ポンッとアニメーション
        topBallObj.transform.localScale = Vector3.zero;
        topBallObj.transform.DOScale(Vector3.one * topBallScale, 0.25f).SetEase(Ease.OutBack);
        // Note: topBallScale is relative to parent, so baseScale is inherited automatically

        // コライダーを広げる（上の雪玉も含む）
        circleCollider.offset = new Vector2(0, topBallOffsetY * 0.5f);
        circleCollider.radius = colliderRadius * 1.3f;

        SpawnSnowPuff(topBallObj.transform.position);

        if (SnowmanBuilderSFXPlayer.Instance != null)
            SnowmanBuilderSFXPlayer.Instance.PlaySnowballStack();
    }

    /// <summary>
    /// 3回目タップ: 装飾を付けて完成
    /// </summary>
    private void CompleteSnowman()
    {
        currentPhase = Phase.Complete;

        // レア判定
        isRare = Random.value < rareChance;

        // 装飾済みスプライトをランダム選択
        Sprite[] pool = isRare ? rareDecorationSets : decorationSets;
        Sprite decoSprite = null;
        if (pool != null && pool.Length > 0)
        {
            decoSprite = pool[Random.Range(0, pool.Length)];
        }

        if (decoSprite != null)
        {
            // 下の雪玉と上の雪玉を非表示にし、完成スプライトに差し替え
            bottomBallRenderer.sprite = decoSprite;
            bottomBallRenderer.sortingOrder = 10;

            if (topBallObj != null)
            {
                topBallRenderer.enabled = false;
            }
        }

        // ポンッとスケールアニメーション
        transform.DOScale(Vector3.one * baseScale * 1.15f, 0.15f).SetEase(Ease.OutBack)
            .OnComplete(() => transform.DOScale(Vector3.one * baseScale, 0.1f));

        // キラキラパーティクル
        SpawnSparkle(transform.position + Vector3.up * topBallOffsetY);

        // レアなら虹色フラッシュ
        if (isRare)
        {
            PlayRareEffect();
        }

        // SE
        if (SnowmanBuilderSFXPlayer.Instance != null)
            SnowmanBuilderSFXPlayer.Instance.PlayComplete(isRare);

        // ゆらゆら揺れ → フェードアウト
        StartWobbleAndFadeOut();
    }

    /// <summary>
    /// ゆらゆら首振り → フェードアウト
    /// </summary>
    private void StartWobbleAndFadeOut()
    {
        // ゆらゆら揺れ
        transform.DORotate(new Vector3(0, 0, wobbleAngle), wobbleDuration * 0.25f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);

        // 一定時間後にフェードアウト
        DOVirtual.DelayedCall(fadeOutDelay, () =>
        {
            currentPhase = Phase.FadingOut;
            DOTween.Kill(transform);  // 揺れ停止

            if (SnowmanBuilderSFXPlayer.Instance != null)
                SnowmanBuilderSFXPlayer.Instance.PlayFadeOut();

            // 全SpriteRendererをフェードアウト
            FadeAllRenderers(fadeOutDuration, () =>
            {
                OnSnowmanDestroyed?.Invoke();
                Destroy(gameObject);
            });
        });
    }

    /// <summary>
    /// 自身の全SpriteRendererをフェードアウト
    /// </summary>
    private void FadeAllRenderers(float duration, System.Action onComplete)
    {
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
        int remaining = renderers.Length;
        if (remaining == 0)
        {
            onComplete?.Invoke();
            return;
        }
        foreach (var sr in renderers)
        {
            sr.DOFade(0f, duration).OnComplete(() =>
            {
                remaining--;
                if (remaining <= 0)
                    onComplete?.Invoke();
            });
        }
    }

    /// <summary>
    /// レア時の虹色エフェクト
    /// </summary>
    private void PlayRareEffect()
    {
        if (mainCamera != null)
        {
            mainCamera.transform.DOShakePosition(0.3f, 0.1f, 10, 90, false, true);
        }

        Sequence colorSeq = DOTween.Sequence();
        colorSeq.Append(bottomBallRenderer.DOColor(new Color(1f, 0.8f, 0.8f), 0.1f));
        colorSeq.Append(bottomBallRenderer.DOColor(new Color(1f, 1f, 0.8f), 0.1f));
        colorSeq.Append(bottomBallRenderer.DOColor(new Color(0.8f, 1f, 0.8f), 0.1f));
        colorSeq.Append(bottomBallRenderer.DOColor(new Color(0.8f, 1f, 1f), 0.1f));
        colorSeq.Append(bottomBallRenderer.DOColor(Color.white, 0.1f));
    }

    /// <summary>
    /// 雪が舞うパフ エフェクト（パーティクルシステム生成）
    /// </summary>
    private void SpawnSnowPuff(Vector3 pos)
    {
        // 簡易パーティクル: 白い丸をいくつか生成して散らす
        for (int i = 0; i < 6; i++)
        {
            GameObject puff = new GameObject("SnowPuff");
            puff.transform.position = pos;
            SpriteRenderer sr = puff.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 50;
            sr.color = new Color(1f, 1f, 1f, 0.8f);

            // 1x1白テクスチャで小さな円を表現
            Texture2D tex = new Texture2D(16, 16);
            for (int x = 0; x < 16; x++)
                for (int y = 0; y < 16; y++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(8, 8));
                    tex.SetPixel(x, y, dist < 7 ? Color.white : Color.clear);
                }
            tex.Apply();
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 16);

            // ランダムな方向に散らすアニメ
            Vector3 dir = new Vector3(Random.Range(-1f, 1f), Random.Range(0.2f, 1.2f), 0).normalized;
            float dist2 = Random.Range(0.3f, 0.8f);
            puff.transform.localScale = Vector3.one * Random.Range(0.15f, 0.35f);

            puff.transform.DOMove(pos + dir * dist2, 0.5f).SetEase(Ease.OutQuad);
            sr.DOFade(0f, 0.5f).SetDelay(0.1f).OnComplete(() => Destroy(puff));
        }
    }

    /// <summary>
    /// キラキラエフェクト
    /// </summary>
    private void SpawnSparkle(Vector3 pos)
    {
        for (int i = 0; i < 8; i++)
        {
            GameObject sparkle = new GameObject("Sparkle");
            sparkle.transform.position = pos;
            SpriteRenderer sr = sparkle.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 60;

            // 黄色い星型を簡易的に表現（四角に回転でキラキラ）
            Texture2D tex = new Texture2D(8, 8);
            for (int x = 0; x < 8; x++)
                for (int y = 0; y < 8; y++)
                {
                    bool isDiamond = Mathf.Abs(x - 4) + Mathf.Abs(y - 4) < 4;
                    tex.SetPixel(x, y, isDiamond ? Color.yellow : Color.clear);
                }
            tex.Apply();
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, 8, 8), new Vector2(0.5f, 0.5f), 8);

            if (isRare)
                sr.color = new Color(1f, 0.8f, 0.2f, 1f); // ゴールド
            else
                sr.color = Color.yellow;

            Vector3 dir = new Vector3(Random.Range(-1f, 1f), Random.Range(-0.5f, 1.5f), 0).normalized;
            float dist = Random.Range(0.4f, 1.2f);
            sparkle.transform.localScale = Vector3.one * Random.Range(0.2f, 0.5f);
            sparkle.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));

            sparkle.transform.DOMove(pos + dir * dist, 0.6f).SetEase(Ease.OutQuad);
            sparkle.transform.DORotate(new Vector3(0, 0, Random.Range(180f, 360f)), 0.6f, RotateMode.FastBeyond360);
            sr.DOFade(0f, 0.6f).SetDelay(0.15f).OnComplete(() => Destroy(sparkle));
        }
    }

    void OnDestroy()
    {
        DOTween.Kill(transform);
        if (bottomBallRenderer != null) DOTween.Kill(bottomBallRenderer);
        if (topBallRenderer != null) DOTween.Kill(topBallRenderer);
    }
}
