using UnityEngine;

/// <summary>
/// CatchInsectsゲームの効果音を一元管理するクラス
/// cline.ymlの方針に従い、ゲームごとに【ゲーム名】+SFXPlayerで管理
/// </summary>
public class CatchInsectsSFXPlayer : MonoBehaviour
{
    [Header("効果音")]
    [SerializeField] private AudioClip netSwingSound;    // 虫取り網を振る音
    [SerializeField] private AudioClip catchNormalSound; // 通常昆虫を捕まえた音
    [SerializeField] private AudioClip catchRareSound;   // レア昆虫を捕まえた音
    
    [Header("音量設定 (0.0 ~ 1.0)")]
    [SerializeField] [Range(0f, 1f)] private float netSwingVolume = 1.0f;    // 網を振る音の音量
    [SerializeField] [Range(0f, 1f)] private float catchNormalVolume = 1.0f; // 通常捕獲音の音量
    [SerializeField] [Range(0f, 1f)] private float catchRareVolume = 1.0f;   // レア捕獲音の音量
    
    private AudioSource audioSource;
    
    public static CatchInsectsSFXPlayer Instance { get; private set; }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // AudioSourceを取得または追加
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // AudioSourceの設定
        audioSource.playOnAwake = false;
        audioSource.loop = false;
    }
    
    /// <summary>
    /// 虫取り網を振る音を再生
    /// </summary>
    public void PlayNetSwing()
    {
        if (netSwingSound != null)
        {
            audioSource.PlayOneShot(netSwingSound, netSwingVolume);
            Debug.Log($"[CatchInsectsSFXPlayer] 網を振る音を再生 (音量: {netSwingVolume})");
        }
        else
        {
            Debug.LogWarning("[CatchInsectsSFXPlayer] netSwingSound が設定されていません");
        }
    }
    
    /// <summary>
    /// 通常昆虫を捕まえた音を再生
    /// </summary>
    public void PlayCatchNormal()
    {
        if (catchNormalSound != null)
        {
            audioSource.PlayOneShot(catchNormalSound, catchNormalVolume);
            Debug.Log($"[CatchInsectsSFXPlayer] 通常昆虫捕獲音を再生 (音量: {catchNormalVolume})");
        }
        else
        {
            Debug.LogWarning("[CatchInsectsSFXPlayer] catchNormalSound が設定されていません");
        }
    }
    
    /// <summary>
    /// レア昆虫を捕まえた音を再生
    /// </summary>
    public void PlayCatchRare()
    {
        if (catchRareSound != null)
        {
            audioSource.PlayOneShot(catchRareSound, catchRareVolume);
            Debug.Log($"[CatchInsectsSFXPlayer] レア昆虫捕獲音を再生 (音量: {catchRareVolume})");
        }
        else
        {
            Debug.LogWarning("[CatchInsectsSFXPlayer] catchRareSound が設定されていません");
        }
    }
    
    /// <summary>
    /// エディタ拡張から効果音を設定
    /// </summary>
    public void SetSounds(AudioClip netSwing, AudioClip catchNormal, AudioClip catchRare)
    {
        netSwingSound = netSwing;
        catchNormalSound = catchNormal;
        catchRareSound = catchRare;
        Debug.Log("[CatchInsectsSFXPlayer] 効果音を設定しました");
    }
}
