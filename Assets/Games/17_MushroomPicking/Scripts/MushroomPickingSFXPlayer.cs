using UnityEngine;

/// <summary>
/// MushroomPickingゲームの効果音を管理するクラス
/// </summary>
public class MushroomPickingSFXPlayer : MonoBehaviour
{
    public static MushroomPickingSFXPlayer Instance { get; private set; }

    [Header("オーディオソース")]
    [SerializeField] private AudioSource sfxAudioSource;

    [Header("共通効果音")]
    [SerializeField] private AudioClip growSound;           // キノコが生えてくる音
    [SerializeField] private AudioClip pickSound;           // 通常キノコ収穫音（タップ瞬間）
    [SerializeField] private AudioClip revealSound;         // シルエット解除音（イラスト出現時）
    [SerializeField] private AudioClip rarePickSound;       // レアキノコ収穫音（タップ瞬間）
    [SerializeField] private AudioClip rareRevealSound;     // レアシルエット解除音（イラスト出現時）
    [SerializeField] private AudioClip basketSound;         // カゴに入る音
    [SerializeField] private AudioClip hideSound;           // キノコが引っ込む音
    [SerializeField] private AudioClip rareAppearSound;     // レア出現予告音
    [SerializeField] private AudioClip superRareRevealSound; // スーパーレアシルエット解除音

    [Header("ボリューム設定")]
    [SerializeField, Range(0f, 1f)] private float growVolume = 0.6f;
    [SerializeField, Range(0f, 1f)] private float pickVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float revealVolume = 0.9f;
    [SerializeField, Range(0f, 1f)] private float rarePickVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float rareRevealVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float basketVolume = 0.7f;
    [SerializeField, Range(0f, 1f)] private float hideVolume = 0.4f;
    [SerializeField, Range(0f, 1f)] private float rareAppearVolume = 0.8f;
    [SerializeField, Range(0f, 1f)] private float superRareRevealVolume = 1f;

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
    public void SetSoundClips(AudioClip grow, AudioClip pick, AudioClip reveal,
                              AudioClip rarePick, AudioClip rareReveal,
                              AudioClip basket, AudioClip hide, AudioClip rareAppear,
                              AudioClip superRareReveal = null)
    {
        growSound = grow;
        pickSound = pick;
        revealSound = reveal;
        rarePickSound = rarePick;
        rareRevealSound = rareReveal;
        basketSound = basket;
        hideSound = hide;
        rareAppearSound = rareAppear;
        superRareRevealSound = superRareReveal;
    }

    /// <summary>
    /// キノコが生えてくる音を再生
    /// </summary>
    public void PlayGrowSound()
    {
        if (growSound != null)
        {
            sfxAudioSource.PlayOneShot(growSound, growVolume);
        }
    }

    /// <summary>
    /// キノコ収穫音を再生
    /// </summary>
    public void PlayPickSound(AudioClip customClip = null, bool isRare = false)
    {
        if (isRare && rarePickSound != null)
        {
            sfxAudioSource.PlayOneShot(rarePickSound, rarePickVolume);
        }
        else if (customClip != null)
        {
            sfxAudioSource.PlayOneShot(customClip, pickVolume);
        }
        else if (pickSound != null)
        {
            sfxAudioSource.PlayOneShot(pickSound, pickVolume);
        }
    }

    /// <summary>
    /// シルエット解除音を再生（イラストが見えた瞬間）
    /// </summary>
    public void PlayRevealSound(bool isRare = false, bool isSuperRare = false)
    {
        if (isSuperRare && superRareRevealSound != null)
        {
            sfxAudioSource.PlayOneShot(superRareRevealSound, superRareRevealVolume);
        }
        else if (isRare && rareRevealSound != null)
        {
            sfxAudioSource.PlayOneShot(rareRevealSound, rareRevealVolume);
        }
        else if (revealSound != null)
        {
            sfxAudioSource.PlayOneShot(revealSound, revealVolume);
        }
    }

    /// <summary>
    /// カゴに入る音を再生
    /// </summary>
    public void PlayBasketSound()
    {
        if (basketSound != null)
        {
            sfxAudioSource.PlayOneShot(basketSound, basketVolume);
        }
    }

    /// <summary>
    /// キノコが引っ込む音を再生
    /// </summary>
    public void PlayHideSound()
    {
        if (hideSound != null)
        {
            sfxAudioSource.PlayOneShot(hideSound, hideVolume);
        }
    }

    /// <summary>
    /// レア出現予告音を再生
    /// </summary>
    public void PlayRareAppearSound()
    {
        if (rareAppearSound != null)
        {
            sfxAudioSource.PlayOneShot(rareAppearSound, rareAppearVolume);
        }
    }
}
