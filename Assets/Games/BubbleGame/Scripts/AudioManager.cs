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
    
    [Header("音量設定")]
    [SerializeField] [Range(0f, 1f)] private float shotVolume = 0.5f;
    [SerializeField] [Range(0f, 1f)] private float splashVolume = 0.5f;
    
    // AudioSourceコンポーネント
    private AudioSource audioSource;
    
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
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        // AudioSourceコンポーネントの取得または追加
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
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
    }
    
    /// <summary>
    /// シャボン玉発射音を再生
    /// </summary>
    public void PlayShotSound()
    {
        if (shotSound != null)
        {
            audioSource.PlayOneShot(shotSound, shotVolume);
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
            audioSource.PlayOneShot(splashSound, splashVolume);
        }
        else
        {
            Debug.LogWarning("シャボン玉が割れる音が設定されていません");
        }
    }
}
