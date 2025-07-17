using UnityEngine;
using System.Collections;

public class HoleController : MonoBehaviour
{
    public MoleController moleController;
    public SpriteRenderer holeEdge;
    public GameObject sweatEffect;
    public Vector2 sweatOffset = new Vector2(0.3f, 0.3f); // 汗エフェクトのオフセット位置

    private bool isActive = false;
    private bool isShocked = false;

    private Coroutine activeCoroutine;

    // マウスクリック（タップ）イベント
    private void OnMouseDown()
    {
        TapMole();
    }

    private void Awake()
    {
        // 初期状態では汗エフェクトを非表示
        if (sweatEffect != null)
            sweatEffect.SetActive(false);
    }

    private void OnEnable()
    {
        // モグラのショック状態変更イベントを購読
        if (moleController != null)
            moleController.OnShockStateChanged += OnMoleShockStateChanged;

    }

    private void OnDisable()
    {
        // イベント購読を解除
        if (moleController != null)
            moleController.OnShockStateChanged -= OnMoleShockStateChanged;
    }

    // モグラのショック状態が変更された時の処理
    private void OnMoleShockStateChanged(bool isShockedLocal)
    {
        if (sweatEffect != null)
        {
            Debug.Log($"OnMoleShockStateChanged: isShocked={isShockedLocal}, sweatEffect={sweatEffect.name}");

            // 汗エフェクトの表示/非表示を切り替え
            sweatEffect.SetActive(isShockedLocal);

            if (isShockedLocal)
            {
                // モグラの位置を基準に汗エフェクトの位置を設定
                Vector3 molePosition = moleController.transform.position;
                sweatEffect.transform.position = new Vector3(
                    molePosition.x + sweatOffset.x,
                    molePosition.y + sweatOffset.y,
                    molePosition.z
                );

                // SpriteRendererコンポーネントを取得
                SpriteRenderer sweatRenderer = sweatEffect.GetComponent<SpriteRenderer>();
                if (sweatRenderer != null)
                {
                    // ソーティングオーダーを設定（モグラよりも前面に表示）
                    sweatRenderer.sortingOrder = moleController.moleRenderer.sortingOrder + 1;

                    // スケールとColorのアルファ値を確認
                    Debug.Log($"SweatEffect: scale={sweatEffect.transform.localScale}, color={sweatRenderer.color}");
                }
                else
                {
                    Debug.LogError("SweatEffect has no SpriteRenderer component!");
                }
            }
        }
        else
        {
            Debug.LogError("SweatEffect is not assigned!");
        }
    }

    // モグラを出現させる
    public void ShowMole(MoleData moleData, float duration)
    {
        if (isActive) return;

        isActive = true;
        moleController.gameObject.SetActive(true);
        moleController.SetMoleData(moleData);

        // 汗エフェクトを非表示
        if (sweatEffect != null)
            sweatEffect.SetActive(false);

        // 出現音を再生
        SfxPlayer.Instance.PlayOneShot(moleData.popSound);

        // ショック状態を初期化
        isShocked = false;

        // 一定時間後に自動で戻る
        activeCoroutine = StartCoroutine(HideMoleAfterDelay(duration));
    }

    // モグラを叩いた時の処理
    public void TapMole()
    {
        if (!isActive) return;

        // 自動で戻るのをキャンセル
        if (activeCoroutine != null)
        {
            StopCoroutine(activeCoroutine);
        }

        // 叩いた効果音を再生（既にショック状態であれば再生しない）
        if (!isShocked)
        {
            SfxPlayer.Instance.PlayOneShot(SfxPlayer.Instance.hit);
        }

        // ショック状態にする
        isShocked = true;

        // 叩かれたアニメーション
        moleController.ShowShockState();

        // 短い時間で戻る
        activeCoroutine = StartCoroutine(HideMoleAfterDelay(0.3f));
    }

    // 一定時間後にモグラを隠す
    private IEnumerator HideMoleAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        HideMole();
    }

    // モグラを隠す
    private void HideMole()
    {
        moleController.gameObject.SetActive(false);

        // 汗エフェクトも非表示
        if (sweatEffect != null)
            sweatEffect.SetActive(false);

        isActive = false;
        activeCoroutine = null;
    }

    // アクティブ状態を取得
    public bool IsActive()
    {
        return isActive;
    }

    // ショック状態を取得
    public bool IsMoleShocked()
    {
        return isShocked;
    }
}
