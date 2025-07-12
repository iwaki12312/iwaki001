using UnityEngine;

/// <summary>
/// 効果音を管理するシングルトンクラス
/// </summary>
public class AudioManager : MonoBehaviour
{
    // シングルトンインスタンス
    private static AudioManager instance;
    
    // 効果音
    [Header("効果音")]
    [SerializeField] private AudioClip shotSound;
    [SerializeField] private AudioClip splashSound;
    [SerializeField] private AudioClip bgmSound;
    
    [Header("音量設定")]
    [SerializeField] [Range(0f, 1f)] private float shotVolume = 0.5f;
    [SerializeField] [Range(0f, 1f)] private float splashVolume = 0.5f;
    [SerializeField] [Range(0f, 1f)] private float bgmVolume = 0.3f;
    
    // AudioSourceコンポーネント
    private AudioSource effectAudioSource;  // 効果音用
    private AudioSource bgmAudioSource;     // BGM用
    
    // シングルトンインスタンスを取得
    public static AudioManager Instance
    {
        get
        {
            if (instance == null)
            {
                // シーン内のAudioManagerを検索
                instance = FindObjectOfType<AudioManager>();
                
                // 見つからない場合は新しく作成
                if (instance == null)
                {
                    GameObject obj = new GameObject("AudioManager");
                    instance = obj.AddComponent<AudioManager>();
                    DontDestroyOnLoad(obj);
                }
            }
            return instance;
        }
    }
    
    void Awake()
    {
        // シングルトンの設定
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("AudioManagerのシングルトンインスタンスを作成しました");
        }
        else if (instance != this)
        {
            Debug.Log("AudioManagerのインスタンスが既に存在するため、このインスタンスを破棄します");
            Destroy(gameObject);
            return;
        }
        
        // 効果音用AudioSourceの取得または追加
        effectAudioSource = GetComponent<AudioSource>();
        if (effectAudioSource == null)
        {
            effectAudioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // BGM用AudioSourceの追加
        bgmAudioSource = gameObject.AddComponent<AudioSource>();
        bgmAudioSource.loop = true;
        bgmAudioSource.playOnAwake = false;
        bgmAudioSource.volume = bgmVolume;
        
        // 効果音の読み込み
        if (shotSound == null)
        {
            shotSound = Resources.Load<AudioClip>("Games/BubbleGame/Audio/Shot");
            if (shotSound == null)
            {
                shotSound = Resources.Load<AudioClip>("BubbleGame/Audio/Shot");
            }
        }
        
        if (splashSound == null)
        {
            splashSound = Resources.Load<AudioClip>("Games/BubbleGame/Audio/Splash");
            if (splashSound == null)
            {
                splashSound = Resources.Load<AudioClip>("BubbleGame/Audio/Splash");
            }
        }
        
    // BGMの読み込み
    if (bgmSound == null)
    {
        // 複数のパスを試す
        string[] possiblePaths = new string[] {
            "_Common/Audio/BGM",
            "Audio/BGM",
            "BGM",
            "BubbleGame/Audio/BGM"
        };
        
        foreach (string path in possiblePaths)
        {
            Debug.Log("BGMの読み込みを試行: " + path);
            bgmSound = Resources.Load<AudioClip>(path);
            if (bgmSound != null)
            {
                Debug.Log("BGMを読み込みました: " + path);
                break;
            }
        }
        
        if (bgmSound == null)
        {
            Debug.LogError("BGMが見つかりません。以下のパスを試しました: Audio/BGM, BGM, BubbleGame/Audio/BGM");
            
            // 全てのリソースを検索
            AudioClip[] allClips = Resources.LoadAll<AudioClip>("");
            if (allClips.Length > 0)
            {
                Debug.Log("利用可能なAudioClipリソース:");
                foreach (AudioClip clip in allClips)
                {
                    Debug.Log("- " + clip.name);
                }
            }
            else
            {
                Debug.LogError("リソース内にAudioClipが見つかりません");
            }
        }
        else
        {
            // BGMを自動再生
            Debug.Log("Awakeでの初期化時にBGMを再生します: " + bgmSound.name);
            bgmAudioSource.clip = bgmSound;
            bgmAudioSource.volume = bgmVolume;
            bgmAudioSource.loop = true;
            bgmAudioSource.Play();
        }
    }
        
        // 初期化完了ログ
        Debug.Log("AudioManager初期化完了");
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
    
    /// <summary>
    /// BGMを再生
    /// </summary>
    public void PlayBGM()
    {
        if (bgmSound != null)
        {
            // 既に再生中の場合は何もしない
            if (bgmAudioSource.isPlaying && bgmAudioSource.clip == bgmSound)
            {
                Debug.Log("BGMは既に再生中です");
                return;
            }
            
            bgmAudioSource.clip = bgmSound;
            bgmAudioSource.volume = bgmVolume;
            bgmAudioSource.loop = true;
            bgmAudioSource.Play();
            Debug.Log("BGMの再生を開始しました: " + bgmSound.name);
        }
        else
        {
            Debug.LogError("BGMが設定されていないため再生できません");
            
            // BGMを再読み込み（複数のパスを試す）
            bgmSound = Resources.Load<AudioClip>("Audio/BGM");
            if (bgmSound == null)
            {
                bgmSound = Resources.Load<AudioClip>("BGM");
            }
            
            if (bgmSound != null)
            {
                Debug.Log("BGMを再読み込みしました: " + bgmSound.name);
                bgmAudioSource.clip = bgmSound;
                bgmAudioSource.volume = bgmVolume;
                bgmAudioSource.loop = true;
                bgmAudioSource.Play();
            }
            else
            {
                // 直接ファイルパスを指定して読み込み
                string[] possiblePaths = new string[] {
                    "Assets/Resources/_Common/Audio/BGM",
                    "Assets/Resources/Audio/BGM",
                    "Assets/_Common/Audio/BGM",
                    "Assets/Audio/BGM",
                    "_Common/Audio/BGM",
                    "Audio/BGM",
                    "BGM"
                };
                
                foreach (string path in possiblePaths)
                {
                    Debug.Log("BGMの読み込みを試行: " + path);
                    bgmSound = Resources.Load<AudioClip>(path);
                    if (bgmSound != null)
                    {
                        Debug.Log("BGMを読み込みました: " + path);
                        bgmAudioSource.clip = bgmSound;
                        bgmAudioSource.volume = bgmVolume;
                        bgmAudioSource.loop = true;
                        bgmAudioSource.Play();
                        break;
                    }
                }
                
                if (bgmSound == null)
                {
                    Debug.LogError("全てのパスでBGMの読み込みに失敗しました");
                }
            }
        }
    }
    
    /// <summary>
    /// BGMを停止
    /// </summary>
    public void StopBGM()
    {
        if (bgmAudioSource.isPlaying)
        {
            bgmAudioSource.Stop();
            Debug.Log("BGMを停止しました");
        }
    }
    
    /// <summary>
    /// BGMの音量を設定
    /// </summary>
    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        if (bgmAudioSource != null)
        {
            bgmAudioSource.volume = bgmVolume;
        }
    }
}
