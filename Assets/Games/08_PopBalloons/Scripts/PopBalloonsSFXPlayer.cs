using UnityEngine;

/// <summary>
/// PopBalloonsゲームの効果音を一元管理するクラス
/// </summary>
public class PopBalloonsSFXPlayer : MonoBehaviour
{
    [Header("効果音")]
    [SerializeField] private AudioClip popNormalSound;      // 通常風船の破裂音
    [SerializeField] private AudioClip popGiantSound1;      // ジャイアント風船の破裂音1
    [SerializeField] private AudioClip popGiantSound2;      // ジャイアント風船の破裂音2
    [SerializeField] private AudioClip stormStartSound;     // バルーンストーム開始音
    [SerializeField] private AudioClip animalAppearSound;   // 動物出現音
    [SerializeField] private AudioClip rabbitVoiceSound;    // ウサギの鳴き声
    [SerializeField] private AudioClip bearVoiceSound;      // クマの鳴き声
    [SerializeField] private AudioClip catVoiceSound;       // ネコの鳴き声
    
    [Header("音量設定 (0.0 ~ 1.0)")]
    [SerializeField] [Range(0f, 1f)] private float popNormalVolume = 1.0f;
    [SerializeField] [Range(0f, 1f)] private float popGiantSound1Volume = 1.0f;
    [SerializeField] [Range(0f, 1f)] private float popGiantSound2Volume = 1.0f;
    [SerializeField] [Range(0f, 1f)] private float stormStartVolume = 1.0f;
    [SerializeField] [Range(0f, 1f)] private float animalAppearVolume = 1.0f;
    [SerializeField] [Range(0f, 1f)] private float animalVoiceVolume = 1.0f;
    
    private AudioSource audioSource;
    
    public static PopBalloonsSFXPlayer Instance { get; private set; }
    
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
    /// 通常風船の破裂音を再生
    /// </summary>
    public void PlayPopNormal()
    {
        if (popNormalSound != null)
        {
            audioSource.PlayOneShot(popNormalSound, popNormalVolume);
        }
    }
    
    /// <summary>
    /// ジャイアント風船の破裂音を再生(2つ同時)
    /// </summary>
    public void PlayPopGiant()
    {
        if (popGiantSound1 != null)
        {
            audioSource.PlayOneShot(popGiantSound1, popGiantSound1Volume);
        }
        if (popGiantSound2 != null)
        {
            audioSource.PlayOneShot(popGiantSound2, popGiantSound2Volume);
        }
    }
    
    /// <summary>
    /// バルーンストーム開始音を再生
    /// </summary>
    public void PlayStormStart()
    {
        if (stormStartSound != null)
        {
            audioSource.PlayOneShot(stormStartSound, stormStartVolume);
        }
    }
    
    /// <summary>
    /// 動物出現音を再生
    /// </summary>
    public void PlayAnimalAppear()
    {
        if (animalAppearSound != null)
        {
            audioSource.PlayOneShot(animalAppearSound, animalAppearVolume);
        }
    }
    
    /// <summary>
    /// 動物の鳴き声を再生
    /// </summary>
    public void PlayAnimalVoice(AnimalType animalType)
    {
        AudioClip voiceClip = null;
        
        switch (animalType)
        {
            case AnimalType.Rabbit:
                voiceClip = rabbitVoiceSound;
                break;
            case AnimalType.Bear:
                voiceClip = bearVoiceSound;
                break;
            case AnimalType.Cat:
                voiceClip = catVoiceSound;
                break;
        }
        
        if (voiceClip != null)
        {
            audioSource.PlayOneShot(voiceClip, animalVoiceVolume);
        }
    }
}

/// <summary>
/// 動物の種類
/// </summary>
public enum AnimalType
{
    Rabbit,     // ウサギ
    Bear,       // クマ
    Cat         // ネコ
}
