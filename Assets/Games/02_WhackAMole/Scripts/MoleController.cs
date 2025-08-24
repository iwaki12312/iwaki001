using UnityEngine;
using System.Collections;
using System;

public class MoleController : MonoBehaviour
{
    public SpriteRenderer moleRenderer;
    
    // アニメーション設定
    public float appearDuration = 0.3f;
    public float disappearDuration = 0.3f;
    public float moveDistance = 0.5f;
    
    private MoleData currentMoleData;
    private bool isShockState = false;
    private Coroutine animationCoroutine;
    private Vector3 initialPosition; // シーンビューで設定した初期位置
    
    private void Awake()
    {
        // シーンビューで設定した位置を保存
        initialPosition = transform.localPosition;
        
        // 初期状態では非表示
        gameObject.SetActive(false);
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
    
    // ショック状態変更イベント
    public delegate void ShockStateChangedHandler(bool isShocked);
    public event ShockStateChangedHandler OnShockStateChanged;
    
    // 出現アニメーションを開始
    public void StartAppearAnimation()
    {
        // 既存のアニメーションがあればキャンセル
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
        
        // 出現アニメーションを開始（位置はリセットしない）
        animationCoroutine = StartCoroutine(AppearAnimation());
    }
    
    // 消失アニメーションを開始
    public void StartDisappearAnimation(Action onComplete = null)
    {
        // 既存のアニメーションがあればキャンセル
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
        
        // 消失アニメーションを開始
        animationCoroutine = StartCoroutine(DisappearAnimation(onComplete));
    }
    
    // 出現アニメーション（下から上へ）
    private IEnumerator AppearAnimation()
    {
        // 開始位置（下）と終了位置（上）を設定
        Vector3 startPos = initialPosition + new Vector3(0, -moveDistance, 0);
        Vector3 endPos = initialPosition;
        
        // 初期位置を設定
        transform.localPosition = startPos;
        
        float startTime = Time.time;
        
        while (Time.time < startTime + appearDuration)
        {
            float t = (Time.time - startTime) / appearDuration;
            transform.localPosition = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }
        
        transform.localPosition = endPos;
        animationCoroutine = null;
    }
    
    // 消失アニメーション（上から下へ）
    private IEnumerator DisappearAnimation(Action onComplete)
    {
        // 開始位置（現在位置）と終了位置（下）を設定
        Vector3 startPos = transform.localPosition;
        Vector3 endPos = initialPosition + new Vector3(0, -moveDistance, 0);
        float startTime = Time.time;
        
        while (Time.time < startTime + disappearDuration)
        {
            float t = (Time.time - startTime) / disappearDuration;
            transform.localPosition = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }
        
        transform.localPosition = endPos;
        
        // ショック状態をリセット
        if (isShockState)
        {
            isShockState = false;
            if (currentMoleData != null)
            {
                moleRenderer.sprite = currentMoleData.normalSprite;
            }
            
            // ショック状態が変更されたことを通知
            if (OnShockStateChanged != null)
                OnShockStateChanged(false);
        }
        
        // 完了コールバックを呼び出し
        onComplete?.Invoke();
        
        animationCoroutine = null;
    }
}
