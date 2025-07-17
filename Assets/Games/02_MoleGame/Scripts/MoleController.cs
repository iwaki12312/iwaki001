using UnityEngine;
using System.Collections;

public class MoleController : MonoBehaviour
{
    public SpriteRenderer moleRenderer;
    
    private MoleData currentMoleData;
    private bool isShockState = false;
    
    private void Awake()
    {
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
}
