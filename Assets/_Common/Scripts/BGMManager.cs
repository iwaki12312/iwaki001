using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// BGMを管理するシングルトンクラス
/// </summary>
public class BGMManager : MonoBehaviour
{
    // シングルトンインスタンス
    private static BGMManager instance;

    [Header("BGM設定")]
    [SerializeField] private AudioClip bgmSound;
    [SerializeField][Range(0f, 1f)] private float bgmVolume = 0.3f;

    // BGM用AudioSource
    private AudioSource bgmAudioSource;

    // シングルトンインスタンスを取得
    public static BGMManager Instance
    {
        get
        {
            if (instance == null)
            {
                // シーン内のBGMManagerを検索
                instance = FindObjectOfType<BGMManager>();

                // 見つからない場合は新しく作成
                if (instance == null)
                {
                    GameObject obj = new GameObject("BGMManager");
                    instance = obj.AddComponent<BGMManager>();
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
            Debug.Log("BGMManagerのシングルトンインスタンスを作成しました");
        }
        else if (instance != this)
        {
            Debug.Log("BGMManagerのインスタンスが既に存在するため、このインスタンスを破棄します");
            Destroy(gameObject);
            return;
        }

        // BGM用AudioSourceの追加
        bgmAudioSource = gameObject.AddComponent<AudioSource>();
        bgmAudioSource.loop = true;
        bgmAudioSource.playOnAwake = false;
        bgmAudioSource.volume = bgmVolume;

        // 初期化完了ログ
        Debug.Log("BGMManager初期化完了");
    }
    
    void OnEnable()
    {
        // シーン遷移イベントを購読
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        // シーン遷移イベントの購読を解除
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"シーンが読み込まれました: {scene.name}");
        
        // メニューシーンに戻った場合、BGMを再生
        if (scene.name == "Menu")
        {
            // 既に再生中でなければBGMを再生
            if (bgmAudioSource != null && !bgmAudioSource.isPlaying)
            {
                Debug.Log("メニューシーンでBGMが停止していたため、再生を開始します");
                PlayBGM();
            }
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
        }
    }

    /// <summary>
    /// 指定したBGMを再生
    /// </summary>
    public void PlayBGM(AudioClip bgm)
    {
        if (bgm != null)
        {
            // 既に同じBGMが再生中の場合は何もしない
            if (bgmAudioSource.isPlaying && bgmAudioSource.clip == bgm)
            {
                Debug.Log("指定されたBGMは既に再生中です");
                return;
            }

            bgmSound = bgm;
            bgmAudioSource.clip = bgmSound;
            bgmAudioSource.volume = bgmVolume;
            bgmAudioSource.loop = true;
            bgmAudioSource.Play();
            Debug.Log("指定されたBGMの再生を開始しました: " + bgmSound.name);
        }
        else
        {
            Debug.LogError("指定されたBGMがnullのため再生できません");
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

    /// <summary>
    /// BGMをフェードイン
    /// </summary>
    public void FadeInBGM(float duration = 1.0f)
    {
        if (bgmAudioSource != null && bgmSound != null)
        {
            StartCoroutine(FadeInCoroutine(duration));
        }
    }

    /// <summary>
    /// BGMをフェードアウト
    /// </summary>
    public void FadeOutBGM(float duration = 1.0f)
    {
        if (bgmAudioSource != null && bgmAudioSource.isPlaying)
        {
            StartCoroutine(FadeOutCoroutine(duration));
        }
    }

    // フェードイン用コルーチン
    private System.Collections.IEnumerator FadeInCoroutine(float duration)
    {
        float startVolume = 0;
        float targetVolume = bgmVolume;

        bgmAudioSource.volume = startVolume;
        bgmAudioSource.Play();

        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            bgmAudioSource.volume = Mathf.Lerp(startVolume, targetVolume, elapsedTime / duration);
            yield return null;
        }

        bgmAudioSource.volume = targetVolume;
    }

    // フェードアウト用コルーチン
    private System.Collections.IEnumerator FadeOutCoroutine(float duration)
    {
        float startVolume = bgmAudioSource.volume;
        float targetVolume = 0;

        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            bgmAudioSource.volume = Mathf.Lerp(startVolume, targetVolume, elapsedTime / duration);
            yield return null;
        }

        bgmAudioSource.Stop();
        bgmAudioSource.volume = bgmVolume;
    }
}
