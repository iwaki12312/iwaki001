using UnityEngine;

/// <summary>
/// たまご割りゲームの効果音を管理するシングルトン
/// </summary>
public class EggHatchSFXPlayer : MonoBehaviour
{
    [Header("効果音設定")]
    [SerializeField] private AudioClip crackSound;          // ヒビが入る音
    [SerializeField] private AudioClip normalFanfare;       // 通常動物ファンファーレ
    [SerializeField] private AudioClip rareFanfare;         // レア動物ファンファーレ
    
    private AudioSource audioSource;
    
    public static EggHatchSFXPlayer Instance { get; private set; }
    
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
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }
    
    /// <summary>
    /// ヒビが入る音を再生
    /// </summary>
    public void PlayCrack()
    {
        if (crackSound != null)
        {
            audioSource.PlayOneShot(crackSound);
        }
    }
    
    /// <summary>
    /// 通常動物ファンファーレを再生
    /// </summary>
    public void PlayNormalFanfare()
    {
        if (normalFanfare != null)
        {
            audioSource.PlayOneShot(normalFanfare);
        }
    }
    
    /// <summary>
    /// レア動物ファンファーレを再生
    /// </summary>
    public void PlayRareFanfare()
    {
        if (rareFanfare != null)
        {
            audioSource.PlayOneShot(rareFanfare);
        }
    }
}
