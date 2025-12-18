using UnityEngine;

/// <summary>
/// バブルゲーム用の効果音を管理するクラス
/// </summary>
public class BubbleSoundManager : MonoBehaviour
{
    // シングルトンインスタンス（ゲームシーン内でのみ有効）
    private static BubbleSoundManager instance;
    
    [Header("効果音")]
    [SerializeField] private AudioClip shotSound;    // シャボン玉発射音
    [SerializeField] private AudioClip splashSound;  // シャボン玉が割れる音
    [SerializeField] private AudioClip starShotSound;    // 星入りシャボン玉発射音
    [SerializeField] private AudioClip starSplashSound;  // 星入りシャボン玉が割れる音
    
    [Header("音量設定")]
    [SerializeField] [Range(0f, 1f)] private float shotVolume = 0.5f;
    [SerializeField] [Range(0f, 1f)] private float splashVolume = 0.5f;
    [SerializeField] [Range(0f, 1f)] private float starShotVolume = 0.5f;
    [SerializeField] [Range(0f, 1f)] private float starSplashVolume = 0.5f;
    
    // 効果音用AudioSource
    private AudioSource effectAudioSource;
    
    // シングルトンインスタンスを取得（ゲームシーン内でのみ有効）
    public static BubbleSoundManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<BubbleSoundManager>();
                
                if (instance == null)
                {
                    Debug.LogError("BubbleSoundManagerが見つかりません。シーン内にBubbleSoundManagerを配置してください。");
                }
            }
            return instance;
        }
    }
    
    void Awake()
    {
        // シングルトンの設定（シーン内でのみ有効）
        if (instance == null)
        {
            instance = this;
            Debug.Log("BubbleSoundManagerのインスタンスを設定しました");
        }
        else if (instance != this)
        {
            Debug.Log("BubbleSoundManagerのインスタンスが既に存在するため、このインスタンスを破棄します");
            Destroy(gameObject);
            return;
        }
        
        // 効果音用AudioSourceの取得または追加
        effectAudioSource = GetComponent<AudioSource>();
        if (effectAudioSource == null)
        {
            effectAudioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // 効果音の読み込み
        LoadSoundEffects();
        
        // 初期化完了ログ
        Debug.Log("BubbleSoundManager初期化完了");
    }
    
    // 効果音の読み込み
    private void LoadSoundEffects()
    {
        // シャボン玉発射音の読み込み
        if (shotSound == null)
        {
            // 直接アセットフォルダから読み込む
            shotSound = Resources.Load<AudioClip>("Games/MakeBubbles/Audio/Shot");
            
            if (shotSound == null)
            {
                Debug.LogWarning("シャボン玉発射音が見つかりません。インスペクタで直接設定してください。");
            }
            else
            {
                Debug.Log("シャボン玉発射音を読み込みました: " + shotSound.name);
            }
        }
        
        // シャボン玉が割れる音の読み込み
        if (splashSound == null)
        {
            // 直接アセットフォルダから読み込む
            splashSound = Resources.Load<AudioClip>("Games/MakeBubbles/Audio/Splash");
            
            if (splashSound == null)
            {
                Debug.LogWarning("シャボン玉が割れる音が見つかりません。インスペクタで直接設定してください。");
            }
            else
            {
                Debug.Log("シャボン玉が割れる音を読み込みました: " + splashSound.name);
            }
        }

        // 星入りシャボン玉発射音の読み込み
        if (starShotSound == null)
        {
            starShotSound = Resources.Load<AudioClip>("Games/MakeBubbles/Audio/StarShot");
            if (starShotSound == null)
            {
                Debug.LogWarning("星入りシャボン玉発射音が見つかりません。インスペクタで直接設定してください。");
            }
            else
            {
                Debug.Log("星入りシャボン玉発射音を読み込みました: " + starShotSound.name);
            }
        }

        // 星入りシャボン玉が割れる音の読み込み
        if (starSplashSound == null)
        {
            starSplashSound = Resources.Load<AudioClip>("Games/MakeBubbles/Audio/StarSplash");
            if (starSplashSound == null)
            {
                Debug.LogWarning("星入りシャボン玉が割れる音が見つかりません。インスペクタで直接設定してください。");
            }
            else
            {
                Debug.Log("星入りシャボン玉が割れる音を読み込みました: " + starSplashSound.name);
            }
        }
    }
    
    /// <summary>
    /// シャボン玉発射音を再生
    /// </summary>
    public void PlayShotSound()
    {
        if (shotSound != null)
        {
            effectAudioSource.PlayOneShot(shotSound, shotVolume);
        }
        else
        {
            Debug.LogWarning("シャボン玉発射音が設定されていません");
        }
    }

    public void PlayStarShotSound()
    {
        if (starShotSound != null)
        {
            effectAudioSource.PlayOneShot(starShotSound, starShotVolume);
        }
        else
        {
            Debug.LogWarning("星入りシャボン玉発射音が設定されていません");
        }
    }
    
    /// <summary>
    /// シャボン玉が割れる音を再生
    /// </summary>
    public void PlaySplashSound()
    {
        if (splashSound != null)
        {
            effectAudioSource.PlayOneShot(splashSound, splashVolume);
        }
        else
        {
            Debug.LogWarning("シャボン玉が割れる音が設定されていません");
        }
    }

    public void PlayStarSplashSound()
    {
        if (starSplashSound != null)
        {
            effectAudioSource.PlayOneShot(starSplashSound, starSplashVolume);
        }
        else
        {
            Debug.LogWarning("星入りシャボン玉が割れる音が設定されていません");
        }
    }
    
    /// <summary>
    /// 指定した効果音を再生
    /// </summary>
    public void PlaySound(AudioClip clip, float volume = 1.0f)
    {
        if (clip != null)
        {
            effectAudioSource.PlayOneShot(clip, volume);
        }
        else
        {
            Debug.LogWarning("指定された効果音がnullのため再生できません");
        }
    }
    
    /// <summary>
    /// 効果音の音量を設定
    /// </summary>
    public void SetEffectVolume(float volume)
    {
        shotVolume = Mathf.Clamp01(volume);
        splashVolume = Mathf.Clamp01(volume);
    }
    
    /// <summary>
    /// シャボン玉発射音の音量を設定
    /// </summary>
    public void SetShotVolume(float volume)
    {
        shotVolume = Mathf.Clamp01(volume);
    }
    
    /// <summary>
    /// シャボン玉が割れる音の音量を設定
    /// </summary>
    public void SetSplashVolume(float volume)
    {
        splashVolume = Mathf.Clamp01(volume);
    }
}
