using UnityEngine;

/// <summary>
/// VegetableDigゲームの効果音を一元管理するクラス
/// </summary>
public class VegetableDigSFXPlayer : MonoBehaviour
{
    [Header("引き抜き効果音")]
    [SerializeField] private AudioClip pullSound;               // 引き抜き音（ポン）
    
    [Header("ファンファーレ")]
    [SerializeField] private AudioClip normalFanfare;           // 通常野菜のファンファーレ
    [SerializeField] private AudioClip rareFanfare;             // レア野菜のスペシャルファンファーレ
    
    [Header("音量設定 (0.0 ~ 1.0)")]
    [SerializeField] [Range(0f, 1f)] private float pullVolume = 1.0f;
    [SerializeField] [Range(0f, 1f)] private float normalFanfareVolume = 1.0f;
    [SerializeField] [Range(0f, 1f)] private float rareFanfareVolume = 1.0f;
    
    private AudioSource audioSource;
    
    public static VegetableDigSFXPlayer Instance { get; private set; }
    
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
        
        // AudioSourceを取得または追加
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // AudioSourceの設定
        audioSource.playOnAwake = false;
        audioSource.loop = false;
    }
    
    /// <summary>
    /// 引き抜き音を再生（ポン）
    /// </summary>
    public void PlayPull()
    {
        if (pullSound != null)
        {
            audioSource.PlayOneShot(pullSound, pullVolume);
        }
    }
    
    /// <summary>
    /// 通常野菜のファンファーレを再生
    /// </summary>
    public void PlayNormalFanfare()
    {
        if (normalFanfare != null)
        {
            audioSource.PlayOneShot(normalFanfare, normalFanfareVolume);
        }
    }
    
    /// <summary>
    /// レア野菜のスペシャルファンファーレを再生
    /// </summary>
    public void PlayRareFanfare()
    {
        if (rareFanfare != null)
        {
            audioSource.PlayOneShot(rareFanfare, rareFanfareVolume);
        }
    }
}
