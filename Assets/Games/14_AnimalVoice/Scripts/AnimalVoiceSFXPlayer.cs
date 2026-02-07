using UnityEngine;

/// <summary>
/// AnimalVoiceゲームの効果音を管理するクラス
/// </summary>
public class AnimalVoiceSFXPlayer : MonoBehaviour
{
    public static AnimalVoiceSFXPlayer Instance { get; private set; }
    
    [Header("オーディオソース")]
    [SerializeField] private AudioSource voiceAudioSource;      // 鳴き声用
    [SerializeField] private AudioSource sfxAudioSource;        // その他SE用
    
    [Header("共通効果音")]
    [SerializeField] private AudioClip tapSound;                // タップ時のポップ音
    [SerializeField] private AudioClip timeChangeSound;         // 背景切り替え音
    [SerializeField] private AudioClip rareAppearSound;         // レア出現音
    
    [Header("ボリューム設定")]
    [SerializeField, Range(0f, 1f)] private float voiceVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float sfxVolume = 0.8f;
    [SerializeField, Range(0f, 1f)] private float rareVoiceVolume = 1f;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeAudioSources();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// AudioSourceを初期化
    /// </summary>
    private void InitializeAudioSources()
    {
        // 鳴き声用AudioSource
        if (voiceAudioSource == null)
        {
            voiceAudioSource = gameObject.AddComponent<AudioSource>();
        }
        voiceAudioSource.playOnAwake = false;
        
        // SE用AudioSource
        if (sfxAudioSource == null)
        {
            sfxAudioSource = gameObject.AddComponent<AudioSource>();
        }
        sfxAudioSource.playOnAwake = false;
    }
    
    /// <summary>
    /// 効果音を設定（Initializerから呼び出し）
    /// </summary>
    public void SetSoundClips(AudioClip tap, AudioClip timeChange, AudioClip rareAppear)
    {
        tapSound = tap;
        timeChangeSound = timeChange;
        rareAppearSound = rareAppear;
    }
    
    /// <summary>
    /// 動物の鳴き声を再生
    /// </summary>
    public void PlayVoice(AudioClip voiceClip, bool isRare = false)
    {
        if (voiceClip == null) return;
        
        float volume = isRare ? rareVoiceVolume : voiceVolume;
        
        // 複数の鳴き声が重なっても聞こえるようにPlayOneShotを使用
        voiceAudioSource.PlayOneShot(voiceClip, volume);
        
        // タップ音も同時に再生
        PlayTapSound();
        
        // レアの場合は特別な音も再生
        if (isRare && rareAppearSound != null)
        {
            sfxAudioSource.PlayOneShot(rareAppearSound, sfxVolume);
        }
    }
    
    /// <summary>
    /// タップ音を再生
    /// </summary>
    public void PlayTapSound()
    {
        if (tapSound != null)
        {
            sfxAudioSource.PlayOneShot(tapSound, sfxVolume * 0.5f);
        }
    }
    
    /// <summary>
    /// 時間切り替え音を再生
    /// </summary>
    public void PlayTimeChangeSound()
    {
        if (timeChangeSound != null)
        {
            sfxAudioSource.PlayOneShot(timeChangeSound, sfxVolume);
        }
    }
    
    /// <summary>
    /// ボリュームを設定
    /// </summary>
    public void SetVolume(float voice, float sfx)
    {
        voiceVolume = Mathf.Clamp01(voice);
        sfxVolume = Mathf.Clamp01(sfx);
    }
}
