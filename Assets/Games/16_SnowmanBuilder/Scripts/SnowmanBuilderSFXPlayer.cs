using UnityEngine;

/// <summary>
/// SnowmanBuilderゲームの効果音を管理するクラス
/// </summary>
public class SnowmanBuilderSFXPlayer : MonoBehaviour
{
    public static SnowmanBuilderSFXPlayer Instance { get; private set; }

    [Header("オーディオソース")]
    [SerializeField] private AudioSource sfxAudioSource;

    [Header("効果音")]
    [SerializeField] private AudioClip snowballAppearSound;   // 雪玉出現音（ポフッ）
    [SerializeField] private AudioClip snowballStackSound;    // 雪玉積み上げ音（ポン）
    [SerializeField] private AudioClip completeSound;         // 完成音（キラキラ）
    [SerializeField] private AudioClip rareCompleteSound;     // レア完成音（ファンファーレ）
    [SerializeField] private AudioClip fadeOutSound;          // フェードアウト音

    [Header("ボリューム設定")]
    [SerializeField, Range(0f, 1f)] private float snowballAppearVolume = 0.8f;
    [SerializeField, Range(0f, 1f)] private float snowballStackVolume = 0.8f;
    [SerializeField, Range(0f, 1f)] private float completeVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float rareCompleteVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float fadeOutVolume = 0.5f;

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
    public void SetSoundClips(AudioClip appear, AudioClip stack, AudioClip complete, AudioClip rareComplete, AudioClip fadeOut)
    {
        snowballAppearSound = appear;
        snowballStackSound = stack;
        completeSound = complete;
        rareCompleteSound = rareComplete;
        fadeOutSound = fadeOut;
    }

    public void PlaySnowballAppear()
    {
        if (snowballAppearSound != null)
            sfxAudioSource.PlayOneShot(snowballAppearSound, snowballAppearVolume);
    }

    public void PlaySnowballStack()
    {
        if (snowballStackSound != null)
            sfxAudioSource.PlayOneShot(snowballStackSound, snowballStackVolume);
    }

    public void PlayComplete(bool isRare)
    {
        if (isRare && rareCompleteSound != null)
        {
            sfxAudioSource.PlayOneShot(rareCompleteSound, rareCompleteVolume);
        }
        else if (completeSound != null)
        {
            sfxAudioSource.PlayOneShot(completeSound, completeVolume);
        }
    }

    public void PlayFadeOut()
    {
        if (fadeOutSound != null)
            sfxAudioSource.PlayOneShot(fadeOutSound, fadeOutVolume);
    }
}
