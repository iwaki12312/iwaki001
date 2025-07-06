using UnityEngine;

/// <summary>
/// アニメーターによるスケール変更を無効化するコンポーネント
/// </summary>
[RequireComponent(typeof(Animator))]
public class DisableAnimatorScale : MonoBehaviour
{
    private Vector3 originalScale;
    private Animator animator;
    
    void Awake()
    {
        originalScale = transform.localScale;
        animator = GetComponent<Animator>();
        
        if (animator != null)
        {
            Debug.Log("DisableAnimatorScale: アニメーターのスケール変更を無効化します");
        }
    }
    
    void LateUpdate()
    {
        // アニメーションによるスケール変更を元に戻す
        if (transform.localScale != originalScale)
        {
            transform.localScale = originalScale;
        }
    }
    
    // スケールを更新するメソッド
    public void UpdateScale(Vector3 newScale)
    {
        originalScale = newScale;
        transform.localScale = newScale;
        Debug.Log("DisableAnimatorScale: スケールを更新しました: " + newScale);
    }
}
