using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Collider2D))]
public class Cookable : MonoBehaviour, IPointerDownHandler
{
    Animator anim;
    bool     isCooking;
    [SerializeField] ParticleSystem burst;
    public void PlayBurst() => burst.Play();

    void Awake() => anim = GetComponent<Animator>();

    public void OnPointerDown(PointerEventData e)
    {
        if (isCooking) return;       // 連打防止
        isCooking = true;
        anim.SetTrigger("Play");
    }

    // Animation の最後にイベントを置くと呼ばれる
    public void OnCookEnd() => isCooking = false;
}
