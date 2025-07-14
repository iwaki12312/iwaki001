using UnityEngine;

public class SfxPlayer : MonoBehaviour
{
    // シングルトンインスタンス
    public static SfxPlayer Instance { get; private set; }
    
    // 効果音
    [Header("効果音")]
    public AudioClip hit;
    public AudioClip molePop;
    

    // AudioSource
    private AudioSource audioSource;
    
    private void Awake()
    {
        // シングルトンの設定
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // AudioSourceの取得
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }
    
    // 効果音を再生
    public void PlayOneShot(AudioClip clip)
    {
        if (clip == null || audioSource == null) return;
        
        audioSource.PlayOneShot(clip);
    }
}