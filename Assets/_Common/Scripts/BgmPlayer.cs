using UnityEngine;

public class BgmPlayer : MonoBehaviour
{
    // シングルトンインスタンス
    public static BgmPlayer Instance { get; private set; }
    
    // BGM用AudioSource
    private AudioSource audioSource;
    
    private void Awake()
    {
        // シングルトンの設定
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
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
            audioSource.loop = true;
        }
    }
    
    // BGMを再生
    public void PlayBGM(AudioClip clip)
    {
        if (clip == null || audioSource == null) return;
        
        // 同じBGMが再生中なら何もしない
        if (audioSource.clip == clip && audioSource.isPlaying)
            return;
        
        audioSource.clip = clip;
        audioSource.Play();
    }
    
    // BGMを停止
    public void StopBGM()
    {
        if (audioSource == null) return;
        
        audioSource.Stop();
    }
}