using UnityEngine;

public class CookSFXPlayer : MonoBehaviour
{
    // シングルトンインスタンス
    private static CookSFXPlayer instance;
    
    [Header("効果音")]
    [SerializeField] private AudioClip potCookingSound;    // 鍋調理中の音
    [SerializeField] private AudioClip panCookingSound;    // フライパン調理中の音
    [SerializeField] private AudioClip cookCompletedSound; // 調理完了時の音
    
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
        
        // 効果音の読み込み
        LoadSoundEffects();
        
        Debug.Log("CookSFXPlayer初期化完了");
    }
    
    // 効果音の読み込み
    private void LoadSoundEffects()
    {
        if (potCookingSound == null)
        {
            potCookingSound = Resources.Load<AudioClip>("Games/04_Cook/Audios/pot_cooking");
            if (potCookingSound == null)
            {
                Debug.LogWarning("鍋調理中の効果音が見つかりません。インスペクタで直接設定してください。");
            }
        }
        
        if (panCookingSound == null)
        {
            panCookingSound = Resources.Load<AudioClip>("Games/04_Cook/Audios/pan_cooking");
            if (panCookingSound == null)
            {
                Debug.LogWarning("フライパン調理中の効果音が見つかりません。インスペクタで直接設定してください。");
            }
        }
        
        if (cookCompletedSound == null)
        {
            cookCompletedSound = Resources.Load<AudioClip>("Games/04_Cook/Audios/cook_completed");
            if (cookCompletedSound == null)
            {
                Debug.LogWarning("調理完了時の効果音が見つかりません。インスペクタで直接設定してください。");
            }
        }
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
