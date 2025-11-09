using UnityEngine;
using System.Collections;

/// <summary>
/// 釣り人と釣り竿を制御するクラス
/// 3段階のスプライト切り替え: idle → hooked → pull → idle
/// </summary>
public class FishermanController : MonoBehaviour
{
    [Header("スプライト設定")]
    [SerializeField] private Sprite idleSprite;       // 待機状態（釣り竿を垂らしている）
    [SerializeField] private Sprite hookedSprite;     // 魚がかかった瞬間
    [SerializeField] private Sprite pullSprite;       // 釣り竿を上げている（釣り上げ中）
    
    [Header("アニメーション時間設定")]
    [SerializeField] private float hookedDuration = 0.1f;   // 魚がかかった状態の表示時間（秒）
    [SerializeField] private float pullDuration = 1.5f;     // 引き上げ状態の表示時間（秒）
    
    private SpriteRenderer spriteRenderer;
    private Coroutine currentAnimation;
    
    public static FishermanController Instance { get; private set; }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
    }
    
    void Start()
    {
        // 初期状態は待機状態
        SetIdleState();
    }
    
    /// <summary>
    /// 魚が釣られた時の処理（FishControllerから呼ばれる）
    /// </summary>
    public void OnFishCaught()
    {
        // 既存のアニメーションをキャンセル
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
        }
        
        // 釣り上げアニメーション開始
        currentAnimation = StartCoroutine(CatchAnimation());
    }
    
    /// <summary>
    /// 待機状態に戻る（FishControllerから呼ばれる）
    /// </summary>
    public void ReturnToIdle()
    {
        SetIdleState();
    }
    
    /// <summary>
    /// 釣り上げアニメーションのコルーチン
    /// idle → hooked（一瞬） → pull → （自動的にidleに戻る）
    /// </summary>
    private IEnumerator CatchAnimation()
    {
        // 1. 魚がかかった瞬間（hookedSprite）
        if (hookedSprite != null)
        {
            spriteRenderer.sprite = hookedSprite;
            yield return new WaitForSeconds(hookedDuration);
        }
        
        // 2. 釣り上げ中（pullSprite）
        if (pullSprite != null)
        {
            spriteRenderer.sprite = pullSprite;
            yield return new WaitForSeconds(pullDuration);
        }
        
        // 3. 待機状態に戻る
        // （魚の釣り上げアニメーションが完了したら、FishControllerからReturnToIdleが呼ばれるので、
        //   ここでは自動的に戻さない。代わりに、タイムアウトとして一定時間後に戻す）
        yield return new WaitForSeconds(1.0f);
        SetIdleState();
    }
    
    /// <summary>
    /// 待機状態に設定
    /// </summary>
    private void SetIdleState()
    {
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
            currentAnimation = null;
        }
        
        if (idleSprite != null)
        {
            spriteRenderer.sprite = idleSprite;
        }
    }
}
