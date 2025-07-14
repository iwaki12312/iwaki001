using UnityEngine;
using System.Collections;

public class HoleController : MonoBehaviour
{
    public MoleController moleController;
    public SpriteRenderer holeEdge;
    
    private bool isActive = false;
    private Coroutine activeCoroutine;
    
    // マウスクリック（タップ）イベント
    private void OnMouseDown()
    {
        TapMole();
    }
    
    // モグラを出現させる
    public void ShowMole(MoleData moleData, float duration)
    {
        if (isActive) return;
        
        isActive = true;
        moleController.gameObject.SetActive(true);
        moleController.SetMoleData(moleData);
        
        // 出現音を再生
        SfxPlayer.Instance.PlayOneShot(moleData.popSound);
        
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
    private void
    HideMole()
    {
        moleController.gameObject.SetActive(false);
        isActive = false;
        activeCoroutine = null;
    }
    
    // アクティブ状態を取得
    public bool IsActive()
    {
        return isActive;
    }
}
