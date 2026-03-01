using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

/// <summary>
/// まな板上の1つのスロットを管理するコンポーネント
/// フルーツの3状態（Whole → Cut → Plated）を遷移させる
/// </summary>
public class FruitSlotController : MonoBehaviour
{
    /// <summary>
    /// フルーツの状態
    /// </summary>
    private enum FruitState
    {
        Empty,      // 何もない
        Whole,      // 丸ごと
        Cut,        // カット済み
        Plated      // お皿に盛り付け
    }

    [Header("設定")]
    [SerializeField] private float cutDisplayDuration = 0.5f;      // カット状態の表示時間
    [SerializeField] private float platedDisplayDuration = 1.0f;   // 盛り付け状態の表示時間
    [SerializeField] private float fadeOutDuration = 0.3f;         // フェードアウト時間
    [SerializeField] private float fadeInDuration = 0.3f;          // フェードイン時間

    private FruitSliceData currentFruit;
    private SpriteRenderer spriteRenderer;
    private CircleCollider2D circleCollider;
    private Camera mainCamera;
    private FruitState currentState = FruitState.Empty;
    private FruitSpawnManager spawnManager;
    private bool isBusy = false; // アニメーション中のタップを防ぐ

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        spriteRenderer.sortingOrder = 10;

        circleCollider = GetComponent<CircleCollider2D>();
        if (circleCollider == null)
        {
            circleCollider = gameObject.AddComponent<CircleCollider2D>();
        }

        mainCamera = Camera.main;
    }

    /// <summary>
    /// スロットを初期化
    /// </summary>
    public void Initialize(FruitSpawnManager manager, float colliderRadius)
    {
        spawnManager = manager;
        if (circleCollider == null)
        {
            circleCollider = GetComponent<CircleCollider2D>();
        }
        circleCollider.radius = colliderRadius;
    }

    /// <summary>
    /// フルーツをスポーンさせる
    /// </summary>
    public void SpawnFruit(FruitSliceData fruitData)
    {
        currentFruit = fruitData;
        currentState = FruitState.Whole;
        isBusy = false;

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (currentFruit != null && currentFruit.wholeSprite != null)
        {
            spriteRenderer.sprite = currentFruit.wholeSprite;
            spriteRenderer.color = Color.white;
        }
        else
        {
            // プレースホルダー: スプライトがない場合は仮表示
            spriteRenderer.color = Color.white;
        }

        // フェードインアニメーション
        Color c = spriteRenderer.color;
        spriteRenderer.color = new Color(c.r, c.g, c.b, 0f);
        spriteRenderer.DOFade(1f, fadeInDuration);

        // レア演出
        if (currentFruit != null && currentFruit.isRare)
        {
            // 虹色にキラキラ演出
            Sequence rareSeq = DOTween.Sequence();
            rareSeq.Append(spriteRenderer.DOColor(new Color(1f, 0.9f, 0.7f, 1f), 0.3f));
            rareSeq.Append(spriteRenderer.DOColor(Color.white, 0.3f));
            rareSeq.SetLoops(-1);
            rareSeq.SetId($"rare_glow_{gameObject.GetInstanceID()}");
        }
    }

    void Update()
    {
        if (currentState != FruitState.Whole || isBusy) return;
        HandleTouch();
    }

    /// <summary>
    /// タッチ処理（マルチタッチ対応）
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

    /// <summary>
    /// タップされた時の処理
    /// </summary>
    private void OnTapped()
    {
        if (currentState != FruitState.Whole || isBusy) return;
        if (currentFruit == null) return;

        isBusy = true;

        // レアのキラキラ停止
        DOTween.Kill($"rare_glow_{gameObject.GetInstanceID()}");
        spriteRenderer.color = Color.white;

        // カット状態に遷移
        TransitionToCut();
    }

    /// <summary>
    /// カット状態に遷移
    /// </summary>
    private void TransitionToCut()
    {
        currentState = FruitState.Cut;

        // スプライト切り替え
        if (currentFruit.cutSprite != null)
        {
            spriteRenderer.sprite = currentFruit.cutSprite;
        }

        // 効果音
        if (FruitSliceSFXPlayer.Instance != null)
        {
            FruitSliceSFXPlayer.Instance.PlayCut();
            if (currentFruit.isRare)
            {
                FruitSliceSFXPlayer.Instance.PlayRare();
            }
        }

        // スケールアニメーション（パンッ！と弾ける感じ）
        transform.DOScale(transform.localScale * 1.3f, 0.1f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                transform.DOScale(transform.localScale / 1.3f, 0.15f)
                    .SetEase(Ease.OutBounce);
            });

        // レア演出：カメラシェイク
        if (currentFruit.isRare && mainCamera != null)
        {
            mainCamera.transform.DOShakePosition(0.3f, 0.08f, 10, 90, false, true);
        }

        // 一定時間後に盛り付け状態へ
        DOVirtual.DelayedCall(cutDisplayDuration, TransitionToPlated);
    }

    /// <summary>
    /// 盛り付け状態に遷移
    /// </summary>
    private void TransitionToPlated()
    {
        currentState = FruitState.Plated;

        // スプライト切り替え
        if (currentFruit.platedSprite != null)
        {
            spriteRenderer.sprite = currentFruit.platedSprite;
        }

        // 効果音
        if (FruitSliceSFXPlayer.Instance != null)
        {
            FruitSliceSFXPlayer.Instance.PlayPlate();
        }

        // 軽い回転アニメーション
        transform.DORotate(new Vector3(0, 0, 360f), 0.3f, RotateMode.FastBeyond360)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                transform.rotation = Quaternion.identity;
            });

        // 一定時間後にクリア＆リスポーン
        DOVirtual.DelayedCall(platedDisplayDuration, ClearAndRespawn);
    }

    /// <summary>
    /// フルーツをクリアして新しいフルーツをスポーン
    /// </summary>
    private void ClearAndRespawn()
    {
        // フェードアウト
        spriteRenderer.DOFade(0f, fadeOutDuration).OnComplete(() =>
        {
            // 新しいフルーツをスポーン
            if (spawnManager != null)
            {
                FruitSliceData newFruit = spawnManager.GetRandomFruit();
                SpawnFruit(newFruit);
            }

            // フルーツ出現音
            if (FruitSliceSFXPlayer.Instance != null)
            {
                FruitSliceSFXPlayer.Instance.PlaySpawn();
            }
        });
    }

    void OnDestroy()
    {
        DOTween.Kill(transform);
        DOTween.Kill(spriteRenderer);
        DOTween.Kill($"rare_glow_{gameObject.GetInstanceID()}");
    }
}
