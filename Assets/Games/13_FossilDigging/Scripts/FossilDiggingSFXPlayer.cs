using UnityEngine;

/// <summary>
/// 化石掘りゲームの効果音を一元管理するクラス
/// </summary>
public class FossilDiggingSFXPlayer : MonoBehaviour
{
    public static FossilDiggingSFXPlayer Instance { get; private set; }

    [Header("効果音")]
    [SerializeField] private AudioClip attackSound;      // ツルハシで叩く音
    [SerializeField] private AudioClip brokenSound;      // 岩が壊れる音
    [SerializeField] private AudioClip normalFanfare;    // ノーマル登場時
    [SerializeField] private AudioClip rareFanfare;      // レア登場時
    [SerializeField] private AudioClip superRareFanfare; // スーパーレア登場時

    [Header("ボリューム設定")]
    [Range(0f, 1f)] [SerializeField] private float attackVolume = 1.0f;
    [Range(0f, 1f)] [SerializeField] private float brokenVolume = 1.0f;
    [Range(0f, 1f)] [SerializeField] private float normalFanfareVolume = 0.8f;
    [Range(0f, 1f)] [SerializeField] private float rareFanfareVolume = 0.9f;
    [Range(0f, 1f)] [SerializeField] private float superRareFanfareVolume = 1.0f;

    private AudioSource audioSource;

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
        audioSource.playOnAwake = false;
    }

    /// <summary>
    /// ツルハシで叩く音を再生
    /// </summary>
    public void PlayAttack()
    {
        if (attackSound != null)
        {
            audioSource.PlayOneShot(attackSound, attackVolume);
        }
    }

    /// <summary>
    /// 岩が壊れる音を再生
    /// </summary>
    public void PlayBroken()
    {
        if (brokenSound != null)
        {
            audioSource.PlayOneShot(brokenSound, brokenVolume);
        }
    }

    /// <summary>
    /// ノーマルレア登場時の効果音を再生
    /// </summary>
    public void PlayNormalFanfare()
    {
        if (normalFanfare != null)
        {
            audioSource.PlayOneShot(normalFanfare, normalFanfareVolume);
        }
    }

    /// <summary>
    /// レア登場時の効果音を再生
    /// </summary>
    public void PlayRareFanfare()
    {
        if (rareFanfare != null)
        {
            audioSource.PlayOneShot(rareFanfare, rareFanfareVolume);
        }
    }

    /// <summary>
    /// スーパーレア登場時の効果音を再生
    /// </summary>
    public void PlaySuperRareFanfare()
    {
        if (superRareFanfare != null)
        {
            audioSource.PlayOneShot(superRareFanfare, superRareFanfareVolume);
        }
    }

    /// <summary>
    /// レア度に応じたファンファーレを再生
    /// </summary>
    public void PlayFanfareByRarity(TreasureRarity rarity)
    {
        switch (rarity)
        {
            case TreasureRarity.Normal:
                PlayNormalFanfare();
                break;
            case TreasureRarity.Rare:
                PlayRareFanfare();
                break;
            case TreasureRarity.SuperRare:
                PlaySuperRareFanfare();
                break;
        }
    }
}

/// <summary>
/// 宝物のレア度
/// </summary>
public enum TreasureRarity
{
    Normal,
    Rare,
    SuperRare
}
