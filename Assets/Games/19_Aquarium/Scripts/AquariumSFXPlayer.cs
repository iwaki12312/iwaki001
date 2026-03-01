using UnityEngine;

/// <summary>
/// Aquariumゲームの効果音を管理するクラス
/// </summary>
public class AquariumSFXPlayer : MonoBehaviour
{
    public static AquariumSFXPlayer Instance { get; private set; }

    [Header("オーディオソース")]
    [SerializeField] private AudioSource sfxAudioSource;

    [Header("効果音")]
    [SerializeField] private AudioClip bubbleSound;       // 泡出現
    [SerializeField] private AudioClip spawnSound;        // 生き物登場
    [SerializeField] private AudioClip tapSound1;         // 生き物タップ①（水しぶき）
    [SerializeField] private AudioClip tapSound2;         // 生き物タップ②（ちゃぷん）
    [SerializeField] private AudioClip rareSound;         // レア登場
    [SerializeField] private AudioClip completeSound;     // 水槽完成
    [SerializeField] private AudioClip resetSound;        // リセット（波の音）
    [SerializeField] private AudioClip ambientSound;      // 環境音（コポコポ）

    [Header("ボリューム設定")]
    [SerializeField, Range(0f, 1f)] private float bubbleVolume = 0.5f;
    [SerializeField, Range(0f, 1f)] private float spawnVolume = 0.7f;
    [SerializeField, Range(0f, 1f)] private float tapVolume = 0.6f;
    [SerializeField, Range(0f, 1f)] private float rareVolume = 0.9f;
    [SerializeField, Range(0f, 1f)] private float completeVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float resetVolume = 0.6f;
    [SerializeField, Range(0f, 1f)] private float ambientVolume = 0.3f;

    private AudioSource ambientAudioSource;

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

    private void InitializeAudioSources()
    {
        if (sfxAudioSource == null)
            sfxAudioSource = gameObject.AddComponent<AudioSource>();
        sfxAudioSource.playOnAwake = false;

        ambientAudioSource = gameObject.AddComponent<AudioSource>();
        ambientAudioSource.playOnAwake = false;
        ambientAudioSource.loop = true;
    }

    public void SetSoundClips(AudioClip bubble, AudioClip spawn, AudioClip tap1,
                               AudioClip tap2, AudioClip rare, AudioClip complete,
                               AudioClip reset, AudioClip ambient)
    {
        bubbleSound = bubble;
        spawnSound = spawn;
        tapSound1 = tap1;
        tapSound2 = tap2;
        rareSound = rare;
        completeSound = complete;
        resetSound = reset;
        ambientSound = ambient;
    }

    public void StartAmbient()
    {
        if (ambientSound != null && ambientAudioSource != null)
        {
            ambientAudioSource.clip = ambientSound;
            ambientAudioSource.volume = ambientVolume;
            ambientAudioSource.Play();
        }
    }

    public void PlayBubbleSound()
    {
        if (bubbleSound != null)
            sfxAudioSource.PlayOneShot(bubbleSound, bubbleVolume);
    }

    public void PlaySpawnSound()
    {
        if (spawnSound != null)
            sfxAudioSource.PlayOneShot(spawnSound, spawnVolume);
    }

    public void PlayTapSound()
    {
        AudioClip clip = Random.value < 0.5f ? tapSound1 : tapSound2;
        if (clip != null)
            sfxAudioSource.PlayOneShot(clip, tapVolume);
    }

    public void PlayRareSound()
    {
        if (rareSound != null)
            sfxAudioSource.PlayOneShot(rareSound, rareVolume);
    }

    public void PlayCompleteSound()
    {
        if (completeSound != null)
            sfxAudioSource.PlayOneShot(completeSound, completeVolume);
    }

    public void PlayResetSound()
    {
        if (resetSound != null)
            sfxAudioSource.PlayOneShot(resetSound, resetVolume);
    }
}
