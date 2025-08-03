using UnityEngine;

public class CookSFXPlayer : MonoBehaviour
{
    // シングルトンインスタンス
    private static CookSFXPlayer instance;
    
    [Header("効果音")]
    [SerializeField] private AudioClip potCookingSound;    // 鍋調理中の音
    [SerializeField] private AudioClip panCookingSound;    // フライパン調理中の音
    [SerializeField] private AudioClip cookCompletedSound; // 調理完了時の音
    
    [Header("追加効果音")]
    [SerializeField] private AudioClip cookCompletedSpecialSound; // 特別料理完了時の音
    [SerializeField] private AudioClip cookCompletedFailSound;    // 失敗料理完了時の音
    
    [Header("ファンファーレ効果音")]
    [SerializeField] private AudioClip fanfareSound;           // 通常料理のファンファーレ
    [SerializeField] private AudioClip fanfareSpecialSound;    // 特別料理のファンファーレ
    [SerializeField] private AudioClip fanfareFailSound;       // 失敗料理のファンファーレ
    
    [Header("音量設定")]
    [SerializeField] [Range(0f, 1f)] private float cookingVolume = 0.5f;
    [SerializeField] [Range(0f, 1f)] private float completedVolume = 0.5f;
    
    // 効果音用AudioSource
    private AudioSource effectAudioSource;
    
    // シングルトンインスタンスを取得
    public static CookSFXPlayer Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<CookSFXPlayer>();
                
                if (instance == null)
                {
                    Debug.LogError("CookSFXPlayerが見つかりません。シーン内にCookSFXPlayerを配置してください。");
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
            Debug.Log("CookSFXPlayerのインスタンスを設定しました");
        }
        else if (instance != this)
        {
            Debug.Log("CookSFXPlayerのインスタンスが既に存在するため、このインスタンスを破棄します");
            Destroy(gameObject);
            return;
        }
        
        // 効果音用AudioSourceの取得または追加
        effectAudioSource = GetComponent<AudioSource>();
        if (effectAudioSource == null)
        {
            effectAudioSource = gameObject.AddComponent<AudioSource>();
        }
        
        Debug.Log("CookSFXPlayer初期化完了");
    }
    
    // 鍋調理中の効果音を再生
    public void PlayPotCookingSound()
    {
        if (potCookingSound != null)
        {
            effectAudioSource.PlayOneShot(potCookingSound, cookingVolume);
        }
        else
        {
            Debug.LogWarning("鍋調理中の効果音が設定されていません");
        }
    }
    
    // フライパン調理中の効果音を再生
    public void PlayPanCookingSound()
    {
        if (panCookingSound != null)
        {
            effectAudioSource.PlayOneShot(panCookingSound, cookingVolume);
        }
        else
        {
            Debug.LogWarning("フライパン調理中の効果音が設定されていません");
        }
    }
    
    // 調理完了時の効果音を再生
    public void PlayCookCompletedSound()
    {
        if (cookCompletedSound != null)
        {
            effectAudioSource.PlayOneShot(cookCompletedSound, completedVolume);
        }
        else
        {
            Debug.LogWarning("調理完了時の効果音が設定されていません");
        }
    }
    
    // 特別料理完了時の効果音を再生
    public void PlaySpecialCompletedSound()
    {
        if (cookCompletedSpecialSound != null)
        {
            effectAudioSource.PlayOneShot(cookCompletedSpecialSound, completedVolume);
        }
        else
        {
            Debug.LogWarning("特別料理完了時の効果音が設定されていません");
            // フォールバックとして通常の完了音を再生
            PlayCookCompletedSound();
        }
    }
    
    // 失敗料理完了時の効果音を再生
    public void PlayFailCompletedSound()
    {
        if (cookCompletedFailSound != null)
        {
            effectAudioSource.PlayOneShot(cookCompletedFailSound, completedVolume);
        }
        else
        {
            Debug.LogWarning("失敗料理完了時の効果音が設定されていません");
            // フォールバックとして通常の完了音を再生
            PlayCookCompletedSound();
        }
    }
    
    // 通常料理のファンファーレを再生
    public void PlayFanfareSound()
    {
        if (fanfareSound != null)
        {
            effectAudioSource.PlayOneShot(fanfareSound, completedVolume);
        }
        else
        {
            Debug.LogWarning("通常料理のファンファーレ効果音が設定されていません");
        }
    }
    
    // 特別料理のファンファーレを再生
    public void PlayFanfareSpecialSound()
    {
        if (fanfareSpecialSound != null)
        {
            effectAudioSource.PlayOneShot(fanfareSpecialSound, completedVolume);
        }
        else
        {
            Debug.LogWarning("特別料理のファンファーレ効果音が設定されていません");
            // フォールバックとして通常のファンファーレを再生
            PlayFanfareSound();
        }
    }
    
    // 失敗料理のファンファーレを再生
    public void PlayFanfareFailSound()
    {
        if (fanfareFailSound != null)
        {
            effectAudioSource.PlayOneShot(fanfareFailSound, completedVolume);
        }
        else
        {
            Debug.LogWarning("失敗料理のファンファーレ効果音が設定されていません");
            // フォールバックとして通常のファンファーレを再生
            PlayFanfareSound();
        }
    }
    
    // 調理中の効果音を停止
    public void StopCookingSound()
    {
        if (effectAudioSource.isPlaying)
        {
            effectAudioSource.Stop();
        }
    }
    
    // 指定した効果音を再生
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
}
