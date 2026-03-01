using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

/// <summary>
/// CakeDecorationゲームのメインコントローラー
/// タップ検出 → デコレーション配置 → お祝い演出 → リセット
/// </summary>
public class CakeDecorationController : MonoBehaviour
{
    [Header("=== ケーキ設定 ===")]
    [SerializeField] private Transform cakeTransform;
    [SerializeField] private SpriteRenderer cakeRenderer;

    [Header("=== デコレーションスプライト ===")]
    [SerializeField] private Sprite[] normalDecorationSprites;   // 通常デコレーション（8種）
    [SerializeField] private Sprite[] rareDecorationSprites;     // レアデコレーション（2種）

    [Header("=== デコレーション設定 ===")]
    [SerializeField, Range(5, 30)] private int maxDecorations = 15;
    [SerializeField, Range(0f, 1f)] private float rareChance = 0.1f;
    [SerializeField, Range(0.3f, 2f)] private float decorationScale = 0.5f;
    [SerializeField, Range(0.3f, 2f)] private float rareDecorationScale = 0.7f;

    [Header("=== ケーキ上のデコレーション範囲 ===")]
    [SerializeField] private float cakeMinX = -2.5f;
    [SerializeField] private float cakeMaxX = 2.5f;
    [SerializeField] private float cakeMinY = -0.5f;
    [SerializeField] private float cakeMaxY = 3.0f;

    [Header("=== アニメーション設定 ===")]
    [SerializeField] private float dropHeight = 6f;      // 落下開始の高さ（ケーキ上面からの相対）
    [SerializeField] private float dropDuration = 0.5f;   // 落下時間
    [SerializeField] private float bouncePower = 0.3f;    // バウンドの強さ
    [SerializeField] private float celebrationDelay = 2.5f; // お祝い後リセットまでの待機

    private int currentDecorationCount = 0;
    private int nextSortingOrder = 10;
    private bool isCelebrating = false;
    private List<GameObject> placedDecorations = new List<GameObject>();
    private Camera mainCamera;

    // タッチ管理（マルチタッチ対応）
    private HashSet<int> processedTouchIds = new HashSet<int>();

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (isCelebrating) return;
        HandleMultiTouch();
    }

    /// <summary>
    /// マルチタッチ入力処理（新Input System使用）
    /// </summary>
    private void HandleMultiTouch()
    {
        // タッチスクリーン入力
        if (Touchscreen.current != null)
        {
            foreach (var touch in Touchscreen.current.touches)
            {
                if (touch.press.wasPressedThisFrame)
                {
                    int touchId = touch.touchId.ReadValue();
                    if (!processedTouchIds.Contains(touchId))
                    {
                        processedTouchIds.Add(touchId);
                        Vector2 screenPos = touch.position.ReadValue();
                        OnTap(screenPos);
                    }
                }
                if (touch.press.wasReleasedThisFrame)
                {
                    int touchId = touch.touchId.ReadValue();
                    processedTouchIds.Remove(touchId);
                }
            }
        }

        // マウス入力（エディタ用）
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 screenPos = Mouse.current.position.ReadValue();
            OnTap(screenPos);
        }
    }

    /// <summary>
    /// タップ処理
    /// </summary>
    private void OnTap(Vector2 screenPosition)
    {
        if (isCelebrating) return;
        if (currentDecorationCount >= maxDecorations) return;

        // スクリーン座標 → ワールド座標
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 0));
        worldPos.z = 0;

        // タップ位置のX座標を利用してケーキ上の横位置を決定
        // ケーキ範囲にクランプ
        float decoX = Mathf.Clamp(worldPos.x, cakeMinX, cakeMaxX);
        // Y座標はケーキ上面の範囲内でランダム
        float decoY = Random.Range(cakeMinY, cakeMaxY);

        // レア判定
        bool isRare = Random.value < rareChance;

        // デコレーション生成
        SpawnDecoration(new Vector2(decoX, decoY), isRare);
    }

    /// <summary>
    /// デコレーションを生成
    /// </summary>
    private void SpawnDecoration(Vector2 targetPos, bool isRare)
    {
        // スプライトを選択
        Sprite decoSprite = null;
        if (isRare && rareDecorationSprites != null && rareDecorationSprites.Length > 0)
        {
            decoSprite = rareDecorationSprites[Random.Range(0, rareDecorationSprites.Length)];
        }
        else if (normalDecorationSprites != null && normalDecorationSprites.Length > 0)
        {
            decoSprite = normalDecorationSprites[Random.Range(0, normalDecorationSprites.Length)];
            isRare = false; // スプライトがない場合は通常扱い
        }

        if (decoSprite == null)
        {
            Debug.LogWarning("[CakeDecoration] デコレーションスプライトが設定されていません");
            return;
        }

        // GameObjectを作成
        GameObject decoObj = new GameObject($"Decoration_{currentDecorationCount:D2}");
        SpriteRenderer sr = decoObj.AddComponent<SpriteRenderer>();
        sr.sprite = decoSprite;
        sr.sortingOrder = nextSortingOrder++;
        
        float scale = isRare ? rareDecorationScale : decorationScale;
        decoObj.transform.localScale = Vector3.one * scale;

        // 落下開始位置（ターゲット位置の上空）
        Vector3 startPos = new Vector3(targetPos.x, targetPos.y + dropHeight, 0);
        decoObj.transform.position = startPos;

        // 落下アニメーション
        AnimateDropDecoration(decoObj, targetPos, isRare);

        placedDecorations.Add(decoObj);
        currentDecorationCount++;

        // SE再生
        if (CakeDecorationSFXPlayer.Instance != null)
        {
            CakeDecorationSFXPlayer.Instance.PlayDecorateSound();
        }
    }

    /// <summary>
    /// デコレーションの落下＋バウンドアニメーション
    /// </summary>
    private void AnimateDropDecoration(GameObject decoObj, Vector2 targetPos, bool isRare)
    {
        Transform t = decoObj.transform;
        Vector3 target = new Vector3(targetPos.x, targetPos.y, 0);

        // 初期スケール0から → 目標サイズへポップ
        Vector3 finalScale = t.localScale;
        t.localScale = Vector3.zero;

        // 落下シーケンス
        Sequence seq = DOTween.Sequence();

        // 落下
        seq.Append(t.DOMove(target, dropDuration).SetEase(Ease.InQuad));

        // 落下中にスケールアップ
        seq.Join(t.DOScale(finalScale, dropDuration).SetEase(Ease.OutBack));

        // バウンド
        seq.Append(t.DOMove(target + Vector3.up * bouncePower, 0.15f).SetEase(Ease.OutQuad));
        seq.Append(t.DOMove(target, 0.15f).SetEase(Ease.InQuad));

        // 着地時のエフェクト
        seq.AppendCallback(() =>
        {
            if (CakeDecorationSFXPlayer.Instance != null)
            {
                CakeDecorationSFXPlayer.Instance.PlayBounceSound();
            }
            // 着地パーティクル
            SpawnLandingParticle(target, isRare);
        });

        // 着地後の微小バウンド（スクイッシュ）
        seq.Append(t.DOScaleY(finalScale.y * 0.8f, 0.08f).SetEase(Ease.InQuad));
        seq.Append(t.DOScaleY(finalScale.y, 0.1f).SetEase(Ease.OutBounce));

        // レアの場合の特別演出
        if (isRare)
        {
            seq.AppendCallback(() =>
            {
                if (CakeDecorationSFXPlayer.Instance != null)
                {
                    CakeDecorationSFXPlayer.Instance.PlayRareSound();
                    CakeDecorationSFXPlayer.Instance.PlaySparkleSound();
                }
                // カメラシェイク
                if (mainCamera != null)
                {
                    mainCamera.transform.DOShakePosition(0.3f, 0.15f, 10, 90, false, true);
                }
                // レインボーフラッシュ  
                StartCoroutine(RainbowFlash(decoObj));
            });
        }

        // お祝いチェック
        seq.AppendCallback(() =>
        {
            if (currentDecorationCount >= maxDecorations && !isCelebrating)
            {
                StartCoroutine(CelebrationSequence());
            }
        });
    }

    /// <summary>
    /// レア出現時のレインボーフラッシュ演出
    /// </summary>
    private IEnumerator RainbowFlash(GameObject decoObj)
    {
        if (decoObj == null) yield break;

        SpriteRenderer sr = decoObj.GetComponent<SpriteRenderer>();
        if (sr == null) yield break;

        Color[] colors = new Color[]
        {
            Color.red, new Color(1f, 0.5f, 0f), Color.yellow,
            Color.green, Color.cyan, Color.blue, new Color(0.5f, 0f, 1f)
        };

        for (int i = 0; i < 14; i++)
        {
            if (decoObj == null) yield break;
            sr.color = colors[i % colors.Length];
            yield return new WaitForSeconds(0.05f);
        }

        if (decoObj != null)
        {
            sr.color = Color.white;
        }
    }

    /// <summary>
    /// 着地時のパーティクルエフェクト
    /// </summary>
    private void SpawnLandingParticle(Vector3 position, bool isRare)
    {
        // シンプルなパーティクル（コードで生成）
        int particleCount = isRare ? 12 : 6;
        float spread = isRare ? 1.5f : 0.8f;

        for (int i = 0; i < particleCount; i++)
        {
            GameObject particle = new GameObject("Particle");
            SpriteRenderer sr = particle.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 80;

            // 1x1テクスチャでシンプルなドットパーティクル
            Texture2D tex = new Texture2D(4, 4);
            Color particleColor = isRare
                ? Color.HSVToRGB(Random.value, 0.8f, 1f)
                : new Color[] { Color.yellow, new Color(1f, 0.7f, 0.8f), Color.white, Color.cyan }[Random.Range(0, 4)];
            for (int px = 0; px < 4; px++)
                for (int py = 0; py < 4; py++)
                    tex.SetPixel(px, py, particleColor);
            tex.Apply();
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4);

            particle.transform.position = position;
            particle.transform.localScale = Vector3.one * Random.Range(0.1f, 0.25f);

            // ランダム方向に飛散
            Vector3 targetPos = position + new Vector3(
                Random.Range(-spread, spread),
                Random.Range(0f, spread),
                0
            );

            Sequence particleSeq = DOTween.Sequence();
            particleSeq.Append(particle.transform.DOMove(targetPos, 0.4f).SetEase(Ease.OutQuad));
            particleSeq.Join(particle.transform.DOScale(0f, 0.4f).SetEase(Ease.InQuad));
            particleSeq.AppendCallback(() => Destroy(particle));
        }
    }

    /// <summary>
    /// お祝い演出シーケンス
    /// </summary>
    private IEnumerator CelebrationSequence()
    {
        isCelebrating = true;

        Debug.Log("[CakeDecoration] お祝い演出開始！");

        // ファンファーレ
        if (CakeDecorationSFXPlayer.Instance != null)
        {
            CakeDecorationSFXPlayer.Instance.PlayCelebrationSound();
        }

        // ケーキ全体を揺らす
        if (cakeTransform != null)
        {
            cakeTransform.DOShakeScale(0.5f, 0.2f, 10, 90);
        }

        // 紙吹雪エフェクト
        StartCoroutine(SpawnConfetti());

        // キラキラエフェクト
        if (CakeDecorationSFXPlayer.Instance != null)
        {
            CakeDecorationSFXPlayer.Instance.PlaySparkleSound();
        }

        // 全デコレーションを一斉にポップアニメーション
        foreach (var deco in placedDecorations)
        {
            if (deco != null)
            {
                deco.transform.DOPunchScale(Vector3.one * 0.3f, 0.3f, 5);
            }
        }

        yield return new WaitForSeconds(celebrationDelay);

        // リセット
        ResetCake();
    }

    /// <summary>
    /// 紙吹雪演出
    /// </summary>
    private IEnumerator SpawnConfetti()
    {
        for (int wave = 0; wave < 3; wave++)
        {
            for (int i = 0; i < 15; i++)
            {
                GameObject confetti = new GameObject("Confetti");
                SpriteRenderer sr = confetti.AddComponent<SpriteRenderer>();
                sr.sortingOrder = 90;

                // カラフルな紙吹雪
                Texture2D tex = new Texture2D(4, 4);
                Color confettiColor = Color.HSVToRGB(Random.value, 0.7f, 1f);
                for (int px = 0; px < 4; px++)
                    for (int py = 0; py < 4; py++)
                        tex.SetPixel(px, py, confettiColor);
                tex.Apply();
                sr.sprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4);

                // 画面上部のランダム位置から
                float startX = Random.Range(-5f, 5f);
                float startY = Random.Range(5f, 7f);
                confetti.transform.position = new Vector3(startX, startY, 0);
                confetti.transform.localScale = Vector3.one * Random.Range(0.15f, 0.35f);
                confetti.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));

                // ひらひら落下
                float endY = Random.Range(-6f, -4f);
                float sway = Random.Range(-2f, 2f);
                float duration = Random.Range(1.5f, 2.5f);

                Sequence confettiSeq = DOTween.Sequence();
                confettiSeq.Append(confetti.transform.DOMoveY(endY, duration).SetEase(Ease.InOutSine));
                confettiSeq.Join(confetti.transform.DOMoveX(startX + sway, duration).SetEase(Ease.InOutSine));
                confettiSeq.Join(confetti.transform.DORotate(new Vector3(0, 0, Random.Range(-720f, 720f)), duration, RotateMode.FastBeyond360));
                confettiSeq.Join(sr.DOFade(0f, duration * 0.8f).SetDelay(duration * 0.2f));
                confettiSeq.AppendCallback(() => Destroy(confetti));
            }
            yield return new WaitForSeconds(0.3f);
        }
    }

    /// <summary>
    /// ケーキをリセット
    /// </summary>
    private void ResetCake()
    {
        Debug.Log("[CakeDecoration] ケーキをリセット");

        // SE再生
        if (CakeDecorationSFXPlayer.Instance != null)
        {
            CakeDecorationSFXPlayer.Instance.PlayResetSound();
        }

        // 全デコレーションをフェードアウトして削除
        foreach (var deco in placedDecorations)
        {
            if (deco != null)
            {
                SpriteRenderer sr = deco.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    Sequence fadeSeq = DOTween.Sequence();
                    fadeSeq.Append(sr.DOFade(0f, 0.5f));
                    fadeSeq.Join(deco.transform.DOScale(0f, 0.5f).SetEase(Ease.InBack));
                    fadeSeq.AppendCallback(() => Destroy(deco));
                }
                else
                {
                    Destroy(deco);
                }
            }
        }

        placedDecorations.Clear();
        currentDecorationCount = 0;
        nextSortingOrder = 10;
        isCelebrating = false;
    }

    void OnDestroy()
    {
        DOTween.Kill(transform);
        if (mainCamera != null)
        {
            DOTween.Kill(mainCamera.transform);
        }
        foreach (var deco in placedDecorations)
        {
            if (deco != null)
            {
                DOTween.Kill(deco.transform);
            }
        }
    }
}
