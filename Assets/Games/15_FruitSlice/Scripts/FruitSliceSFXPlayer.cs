using UnityEngine;

/// <summary>
/// FruitSliceゲームの効果音を管理するクラス
/// </summary>
public class FruitSliceSFXPlayer : MonoBehaviour
{
    public static FruitSliceSFXPlayer Instance { get; private set; }

    [Header("オーディオソース")]
    [SerializeField] private AudioSource sfxAudioSource;

    [Header("効果音クリップ")]
    [SerializeField] private AudioClip cutClip;         // カット音
    [SerializeField] private AudioClip plateClip;       // 盛り付け音
    [SerializeField] private AudioClip completeClip;    // 完成音（お皿が満杯になった時）
    [SerializeField] private AudioClip rareClip;        // レア音
    [SerializeField] private AudioClip spawnClip;       // フルーツ出現音

    [Header("ボリューム設定")]
    [SerializeField, Range(0f, 1f)] private float cutVolume = 0.7f;
    [SerializeField, Range(0f, 1f)] private float plateVolume = 0.6f;
    [SerializeField, Range(0f, 1f)] private float completeVolume = 0.8f;
    [SerializeField, Range(0f, 1f)] private float rareVolume = 0.9f;
    [SerializeField, Range(0f, 1f)] private float spawnVolume = 0.3f;

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
    /// 効果音クリップを設定（Initializerから呼び出し）
    /// </summary>
    public void SetSoundClips(AudioClip cut, AudioClip plate, AudioClip complete, AudioClip rare, AudioClip spawn)
    {
        cutClip = cut;
        plateClip = plate;
        completeClip = complete;
        rareClip = rare;
        spawnClip = spawn;
    }

    /// <summary>
    /// カット音を再生
    /// </summary>
    public void PlayCut()
    {
        if (cutClip != null)
        {
            sfxAudioSource.PlayOneShot(cutClip, cutVolume);
        }
    }

    /// <summary>
    /// 盛り付け音を再生
    /// </summary>
    public void PlayPlate()
    {
        if (plateClip != null)
        {
            sfxAudioSource.PlayOneShot(plateClip, plateVolume);
        }
    }

    /// <summary>
    /// 完成音を再生
    /// </summary>
    public void PlayComplete()
    {
        if (completeClip != null)
        {
            sfxAudioSource.PlayOneShot(completeClip, completeVolume);
        }
    }

    /// <summary>
    /// レア音を再生
    /// </summary>
    public void PlayRare()
    {
        if (rareClip != null)
        {
            sfxAudioSource.PlayOneShot(rareClip, rareVolume);
        }
    }

    /// <summary>
    /// フルーツ出現音を再生
    /// </summary>
    public void PlaySpawn()
    {
        if (spawnClip != null)
        {
            sfxAudioSource.PlayOneShot(spawnClip, spawnVolume);
        }
    }
}
