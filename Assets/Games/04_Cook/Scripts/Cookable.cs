using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Collider2D))]
public class Cookable : MonoBehaviour, IPointerDownHandler
{
    Animator anim;
    bool     isCooking;
    [SerializeField] ParticleSystem burst;
    
    // 調理器具の種類を指定するための列挙型
    public enum CookwareType
    {
        Pot,    // 鍋
        Pan     // フライパン
    }
    
    [SerializeField] CookwareType cookwareType = CookwareType.Pot;

    void Awake() => anim = GetComponent<Animator>();

    public void OnPointerDown(PointerEventData e)
    {
        if (isCooking) return;       // 連打防止
        isCooking = true;
        anim.SetTrigger("Play");
        
        // 調理中の効果音を再生
        PlayCookingSound();
    }
    
    // 調理中の効果音を再生
    private void PlayCookingSound()
    {
        if (CookSFXPlayer.Instance == null) return;
        
        // 調理器具の種類に応じた効果音を再生
        if (cookwareType == CookwareType.Pot)
        {
            CookSFXPlayer.Instance.PlayPotCookingSound();
        }
        else
        {
            CookSFXPlayer.Instance.PlayPanCookingSound();
        }
    }
    
    // パーティクルを再生（アニメーションイベントから呼び出し）
    public void PlayBurst()
    {
        burst.Play();
        
        // 調理完了時の効果音を再生
        if (CookSFXPlayer.Instance != null)
        {
            CookSFXPlayer.Instance.PlayCookCompletedSound();
        }
    }

    // Animation の最後にイベントを置くと呼ばれる
    public void OnCookEnd() => isCooking = false;
}
