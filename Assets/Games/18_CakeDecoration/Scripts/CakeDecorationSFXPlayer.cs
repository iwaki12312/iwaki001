using UnityEngine;

/// <summary>
/// CakeDecorationゲームの効果音を管理するクラス
/// </summary>
public class CakeDecorationSFXPlayer : MonoBehaviour
{
    public static CakeDecorationSFXPlayer Instance { get; private set; }

    [Header("オーディオソース")]
    [SerializeField] private AudioSource sfxAudioSource;

    [Header("効果音")]
    [SerializeField] private AudioClip decorateSound1;    // デコレーション配置音1
    [SerializeField] private AudioClip decorateSound2;    // デコレーション配置音2
    [SerializeField] private AudioClip decorateSound3;    // デコレーション配置音3
    [SerializeField] private AudioClip rareSound;         // レア出現音
    [SerializeField] private AudioClip celebrationSound;  // お祝いファンファーレ
    [SerializeField] private AudioClip bounceSound;       // バウンド音
    [SerializeField] private AudioClip resetSound;        // リセット音
    [SerializeField] private AudioClip sparkleSound;      // キラキラ音

    [Header("ボリューム設定")]
    [SerializeField, Range(0f, 1f)] private float decorateVolume = 0.7f;
    [SerializeField, Range(0f, 1f)] private float rareVolume = 0.9f;
    [SerializeField, Range(0f, 1f)] private float celebrationVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float bounceVolume = 0.5f;
    [SerializeField, Range(0f, 1f)] private float resetVolume = 0.6f;
    [SerializeField, Range(0f, 1f)] private float sparkleVolume = 0.6f;

    private AudioClip[] decorateSounds;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeAudioSource();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeAudioSource()
    {
        if (sfxAudioSource == null)
        {
            sfxAudioSource = gameObject.AddComponent<AudioSource>();
        }
        sfxAudioSource.playOnAwake = false;
    }

    /// <summary>
    /// 効果音を設定（Initializerから呼び出し）
    /// </summary>
    public void SetSoundClips(AudioClip deco1, AudioClip deco2, AudioClip deco3,
                               AudioClip rare, AudioClip celebration, AudioClip bounce,
                               AudioClip reset, AudioClip sparkle)
    {
        decorateSound1 = deco1;
        decorateSound2 = deco2;
        decorateSound3 = deco3;
        rareSound = rare;
        celebrationSound = celebration;
        bounceSound = bounce;
        resetSound = reset;
        sparkleSound = sparkle;
        decorateSounds = new AudioClip[] { decorateSound1, decorateSound2, decorateSound3 };
    }

    /// <summary>
    /// デコレーション配置音を再生（ランダムに3種から選択）
    /// </summary>
    public void PlayDecorateSound()
    {
        if (decorateSounds == null || decorateSounds.Length == 0)
        {
            decorateSounds = new AudioClip[] { decorateSound1, decorateSound2, decorateSound3 };
        }

        AudioClip clip = decorateSounds[Random.Range(0, decorateSounds.Length)];
        if (clip != null)
        {
            sfxAudioSource.PlayOneShot(clip, decorateVolume);
        }
    }

    /// <summary>
    /// レア出現音を再生
    /// </summary>
    public void PlayRareSound()
    {
        if (rareSound != null)
        {
            sfxAudioSource.PlayOneShot(rareSound, rareVolume);
        }
    }

    /// <summary>
    /// お祝いファンファーレを再生
    /// </summary>
    public void PlayCelebrationSound()
    {
        if (celebrationSound != null)
        {
            sfxAudioSource.PlayOneShot(celebrationSound, celebrationVolume);
        }
    }

    /// <summary>
    /// バウンド音を再生
    /// </summary>
    public void PlayBounceSound()
    {
        if (bounceSound != null)
        {
            sfxAudioSource.PlayOneShot(bounceSound, bounceVolume);
        }
    }

    /// <summary>
    /// リセット音を再生
    /// </summary>
    public void PlayResetSound()
    {
        if (resetSound != null)
        {
            sfxAudioSource.PlayOneShot(resetSound, resetVolume);
        }
    }

    /// <summary>
    /// キラキラ音を再生
    /// </summary>
    public void PlaySparkleSound()
    {
        if (sparkleSound != null)
        {
            sfxAudioSource.PlayOneShot(sparkleSound, sparkleVolume);
        }
    }
}
