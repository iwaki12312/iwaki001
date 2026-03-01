using UnityEngine;
using DG.Tweening;

/// <summary>
/// 画面下部の収穫カゴを制御するクラス
/// キノコが入るたびに揺れるアニメーションを再生
/// </summary>
public class BasketController : MonoBehaviour
{
    [Header("揺れ設定")]
    [SerializeField] private float shakeStrength = 0.15f;
    [SerializeField] private float shakeDuration = 0.3f;
    [SerializeField] private int shakeVibrato = 10;

    private Vector3 originalPosition;
    private Vector3 originalScale;
    private bool isShaking = false;

    void Awake()
    {
        originalPosition = transform.position;
        originalScale = transform.localScale;
    }

    /// <summary>
    /// キノコがカゴに入った時の演出
    /// </summary>
    public void OnMushroomCollected()
    {
        if (isShaking) return;
        isShaking = true;

        // 揺れアニメーション
        transform.DOShakePosition(shakeDuration, shakeStrength, shakeVibrato, 90, false, true)
            .OnComplete(() =>
            {
                transform.position = originalPosition;
                isShaking = false;
            });

        // 少し跳ねる
        Sequence bounceSeq = DOTween.Sequence();
        bounceSeq.Append(transform.DOScaleY(originalScale.y * 1.1f, 0.1f).SetEase(Ease.OutQuad));
        bounceSeq.Append(transform.DOScaleY(originalScale.y, 0.15f).SetEase(Ease.InBounce));
    }

    void OnDestroy()
    {
        DOTween.Kill(transform);
    }
}
