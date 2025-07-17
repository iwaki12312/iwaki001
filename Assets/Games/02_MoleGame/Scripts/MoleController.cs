using UnityEngine;
using System.Collections;

public class MoleController : MonoBehaviour
{
    public SpriteRenderer moleRenderer;
    
    [Header("アニメーション設定")]
    public float appearDuration = 0.3f; // 出現アニメーションの時間
    public float disappearDuration = 0.3f; // 消失アニメーションの時間
    public float moveDistance = 1.2f; // 移動距離
    
    private MoleData currentMoleData;
    private bool isShockState = false;
    private Coroutine animationCoroutine; // アニメーションコルーチン
    private Vector3 originalPosition; // 元の位置
    private Vector3 hiddenPosition; // 隠れた位置
    
    private void Awake()
    {
        // 初期状態では非表示
        gameObject.SetActive(false);
        
        // SpriteRendererのMask Interactionを設定
        if (moleRenderer != null)
        {
            moleRenderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        }
    }
    
    private void OnEnable()
    {
        // 元の位置を保存
        originalPosition = transform.localPosition;
        
        // 隠れた位置を計算（下方向に移動）
        hiddenPosition = originalPosition - new Vector3(0, moveDistance, 0);
        
        // 初期位置を隠れた位置に設定
        transform.localPosition = hiddenPosition;
    }
    
    // モグラのデータを設定
    public void SetMoleData(MoleData moleData)
    {
        currentMoleData = moleData;
        isShockState = false;
        
        // スプライトと色を設定
        moleRenderer.sprite = moleData.normalSprite;
        moleRenderer.color = moleData.tintColor;
    }
    
    // ショック状態を表示
    public void ShowShockState()
    {
        if (currentMoleData == null) return;

        isShockState = true;
        moleRenderer.sprite = currentMoleData.shockSprite;

        // ショック状態になったことを通知
        if (OnShockStateChanged != null)
            OnShockStateChanged(true);
    }
    
    // 出現アニメーションを開始
    public void StartAppearAnimation(System.Action onComplete = null)
    {
        // ゲームオブジェクトがアクティブかどうかを確認
        if (!gameObject.activeInHierarchy)
        {
            Debug.LogError("Cannot start appear animation on inactive game object!");
            onComplete?.Invoke();
            return;
        }
        
        // 既存のアニメーションを停止
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
        
        // 初期位置を隠れた位置に設定
        transform.localPosition = hiddenPosition;
        
        // アニメーションを開始
        animationCoroutine = StartCoroutine(AppearAnimation(onComplete));
    }
    
    // 消失アニメーションを開始
    public void StartDisappearAnimation(System.Action onComplete = null)
    {
        // ゲームオブジェクトがアクティブかどうかを確認
        if (!gameObject.activeInHierarchy)
        {
            Debug.LogError("Cannot start disappear animation on inactive game object!");
            onComplete?.Invoke();
            return;
        }
        
        // 既存のアニメーションを停止
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
        
        // アニメーションを開始
        animationCoroutine = StartCoroutine(DisappearAnimation(onComplete));
    }
    
    // 出現アニメーション
    private IEnumerator AppearAnimation(System.Action onComplete)
    {
        if (moleRenderer == null)
        {
            Debug.LogError("MoleRenderer is not assigned!");
            onComplete?.Invoke();
            yield break;
        }
        
        float startTime = Time.time;
        float endTime = startTime + appearDuration;
        
        while (Time.time < endTime)
        {
            // 経過時間の割合を計算（0～1）
            float t = (Time.time - startTime) / appearDuration;
            
            // イージング関数を適用（スムーズに出現）
            t = Mathf.SmoothStep(0, 1, t);
            
            // 位置を更新（下から上に移動）
            transform.localPosition = Vector3.Lerp(hiddenPosition, originalPosition, t);
            
            yield return null;
        }
        
        // 最終位置に設定
        transform.localPosition = originalPosition;
        
        // 完了コールバックを呼び出し
        onComplete?.Invoke();
        
        animationCoroutine = null;
    }
    
    // 消失アニメーション
    private IEnumerator DisappearAnimation(System.Action onComplete)
    {
        if (moleRenderer == null)
        {
            Debug.LogError("MoleRenderer is not assigned!");
            onComplete?.Invoke();
            yield break;
        }
        
        float startTime = Time.time;
        float endTime = startTime + disappearDuration;
        
        while (Time.time < endTime)
        {
            // 経過時間の割合を計算（0～1）
            float t = (Time.time - startTime) / disappearDuration;
            
            // イージング関数を適用（スムーズに消失）
            t = Mathf.SmoothStep(0, 1, t);
            
            // 位置を更新（上から下に移動）
            transform.localPosition = Vector3.Lerp(originalPosition, hiddenPosition, t);
            
            yield return null;
        }
        
        // 最終位置に設定
        transform.localPosition = hiddenPosition;
        
        // 完了コールバックを呼び出し
        onComplete?.Invoke();
        
        animationCoroutine = null;
    }
    
    // ショック状態変更イベント
    public delegate void ShockStateChangedHandler(bool isShocked);
    public event ShockStateChangedHandler OnShockStateChanged;
}
