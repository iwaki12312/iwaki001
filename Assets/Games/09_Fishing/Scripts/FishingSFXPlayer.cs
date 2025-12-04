using UnityEngine;

/// <summary>
/// Fishingゲームの効果音を一元管理するクラス
/// </summary>
public class FishingSFXPlayer : MonoBehaviour
{
    [Header("効果音")]
    [SerializeField] private AudioClip fishCatchSound;      // 通常魚を釣る音
    [SerializeField] private AudioClip fishRareSound;       // レア魚を釣る音
    [SerializeField] private AudioClip seagullAppearSound;  // カモメ出現音
    [SerializeField] private AudioClip seagullCrySound;     // カモメの鳴き声
    
    [Header("音量設定 (0.0 ~ 1.0)")]
    [SerializeField] [Range(0f, 1f)] private float fishCatchVolume = 1.0f;
    [SerializeField] [Range(0f, 1f)] private float fishRareVolume = 1.0f;
    [SerializeField] [Range(0f, 1f)] private float seagullAppearVolume = 1.0f;
    [SerializeField] [Range(0f, 1f)] private float seagullCryVolume = 1.0f;
    
    private AudioSource audioSource;
    
    public static FishingSFXPlayer Instance { get; private set; }
    
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
    /// 通常魚を釣る音を再生
    /// </summary>
    public void PlayFishCatch()
    {
        if (fishCatchSound != null)
        {
            audioSource.PlayOneShot(fishCatchSound, fishCatchVolume);
        }
    }
    
    /// <summary>
    /// レア魚を釣る音を再生
    /// </summary>
    public void PlayFishRare()
    {
        if (fishRareSound != null)
        {
            audioSource.PlayOneShot(fishRareSound, fishRareVolume);
        }
    }
    
    /// <summary>
    /// カモメ出現音を再生
    /// </summary>
    public void PlaySeagullAppear()
    {
        if (seagullAppearSound != null)
        {
            audioSource.PlayOneShot(seagullAppearSound, seagullAppearVolume);
        }
    }
    
    /// <summary>
    /// カモメの鳴き声を再生
    /// </summary>
    public void PlaySeagullCry()
    {
        if (seagullCrySound != null)
        {
            audioSource.PlayOneShot(seagullCrySound, seagullCryVolume);
        }
    }
}
