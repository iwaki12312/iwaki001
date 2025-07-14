using UnityEngine;
using System.Collections;

public class MoleController : MonoBehaviour
{
    public SpriteRenderer moleRenderer;
    public GameObject sweatEffect;
    
    private MoleData currentMoleData;
    private bool isShockState = false;
    
    private void Awake()
    {
        // 初期状態では非表示
        gameObject.SetActive(false);
        sweatEffect.SetActive(false);
    }
    
    // モグラのデータを設定
    public void SetMoleData(MoleData moleData)
    {
        currentMoleData = moleData;
        isShockState = false;
        
        // スプライトと色を設定
        moleRenderer.sprite = moleData.normalSprite;
        moleRenderer.color = moleData.tintColor;
        
        // 汗エフェクトを非表示
        sweatEffect.SetActive(false);
    }
    
    // ショック状態を表示
    public void ShowShockState()
    {
        if (currentMoleData == null) return;
        
        isShockState = true;
        moleRenderer.sprite = currentMoleData.shockSprite;
        
        // 汗エフェクトを表示
        sweatEffect.SetActive(true);
        
        // 叩いた効果音を再生
        SfxPlayer.Instance.PlayOneShot(SfxPlayer.Instance.hit);
    }
}
