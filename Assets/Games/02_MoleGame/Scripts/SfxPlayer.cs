using UnityEngine;

public class SfxPlayer : MonoBehaviour
{
    // シングルトンインスタンス
    public static SfxPlayer Instance { get; private set; }

    // 効果音
    [Header("効果音")]
    public AudioClip hit;
    public AudioClip pop;
    public AudioClip starPop;


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
        if (clip == null || audioSource == null)
        {
            Debug.LogWarning("効果音クリップが設定されていないか、AudioSourceが見つかりません。");
            return;
        }
        else
        {
            // 効果音を再生
            audioSource.PlayOneShot(clip);
        }
    }
}