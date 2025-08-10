using UnityEngine;

/// <summary>
/// TouchTheStarゲームの効果音を管理するクラス
/// </summary>
public class TouchTheStarSFXPlayer : MonoBehaviour
{
    public static TouchTheStarSFXPlayer Instance { get; private set; }
    
    [Header("効果音設定")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip starAppearSound;
    [SerializeField] private AudioClip starDisappearSound;
    [SerializeField] private AudioClip ufoAppearSound;
    [SerializeField] private AudioClip ufoDisappearSound;
    
    void Awake()
    {
        // シングルトンパターンの実装
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioComponents();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// オーディオコンポーネントの初期化
    /// </summary>
    private void InitializeAudioComponents()
    {
        // AudioSourceがない場合は追加
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        // 効果音ファイルを自動的にロード
        LoadAudioClips();
    }
    
    /// <summary>
    /// 効果音ファイルを自動的にロード
    /// </summary>
    private void LoadAudioClips()
    {
        if (starAppearSound == null)
        {
            starAppearSound = Resources.Load<AudioClip>("star_appear");
            if (starAppearSound != null)
            {
                Debug.Log("star_appear効果音をロードしました。");
            }
        }
        
        if (starDisappearSound == null)
        {
            starDisappearSound = Resources.Load<AudioClip>("star_disappear");
            if (starDisappearSound != null)
            {
                Debug.Log("star_disappear効果音をロードしました。");
            }
        }
        
        // ログ出力（デバッグ用）
        if (starAppearSound == null)
        {
            Debug.LogWarning("star_appear.mp3が見つかりません。手動でアサインしてください。");
        }
        if (starDisappearSound == null)
        {
            Debug.LogWarning("star_disappear.mp3が見つかりません。手動でアサインしてください。");
        }
    }
    
    /// <summary>
    /// 星出現時の効果音を再生
    /// </summary>
    public void PlayStarAppearSound()
    {
        if (audioSource != null && starAppearSound != null)
        {
            audioSource.PlayOneShot(starAppearSound);
        }
    }
    
    /// <summary>
    /// 星消滅時の効果音を再生
    /// </summary>
    public void PlayStarDisappearSound()
    {
        if (audioSource != null && starDisappearSound != null)
        {
            audioSource.PlayOneShot(starDisappearSound);
        }
    }
    
    /// <summary>
    /// UFO出現時の効果音を再生
    /// </summary>
    public void PlayUFOAppearSound()
    {
        if (audioSource != null && ufoAppearSound != null)
        {
            audioSource.PlayOneShot(ufoAppearSound);
        }
    }
    
    /// <summary>
    /// UFO消滅時の効果音を再生
    /// </summary>
    public void PlayUFODisappearSound()
    {
        if (audioSource != null && ufoDisappearSound != null)
        {
            audioSource.PlayOneShot(ufoDisappearSound);
        }
    }
    
    /// <summary>
    /// 効果音ファイルを手動で設定（Inspector用）
    /// </summary>
    public void SetAudioClips(AudioClip starAppearSound, AudioClip starDisappearSound, AudioClip ufoAppearSound, AudioClip ufoDisappearSound)
    {
        this.starAppearSound = starAppearSound;
        this.starDisappearSound = starDisappearSound;
        this.ufoAppearSound = ufoAppearSound;
        this.ufoDisappearSound = ufoDisappearSound;
    }
    
    /// <summary>
    /// 星の効果音ファイルを手動で設定（後方互換性のため）
    /// </summary>
    public void SetAudioClips(AudioClip appearSound, AudioClip disappearSound)
    {
        starAppearSound = appearSound;
        starDisappearSound = disappearSound;
    }
}
