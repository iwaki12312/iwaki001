using UnityEngine;

/// <summary>
/// 効果音をゲーム単位で一元管理する
/// 命名規約: 【ゲーム名】 + SFXPlayer
/// </summary>
public class FireworksSFXPlayer : MonoBehaviour
{
    [SerializeField] private AudioClip rocketLaunchSfx;
    [SerializeField] private AudioClip explosionSfx;
    [SerializeField] private AudioClip giantExplosionExtraSfx;
    [SerializeField] private AudioClip shootingStarSfx;
    [SerializeField] private AudioClip shootingStarTapSfx;
    [SerializeField] private AudioClip starMineStartSfx;

    [Header("効果音ごとの音量(倍率)")]
    [SerializeField, Range(0f, 1f)] private float rocketLaunchVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float explosionVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float giantExplosionExtraVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float shootingStarVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float shootingStarTapVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float starMineStartVolume = 1f;

    private AudioSource oneShotSource;

    private void Awake()
    {
        if (oneShotSource == null)
        {
            oneShotSource = gameObject.AddComponent<AudioSource>();
            oneShotSource.playOnAwake = false;
        }
    }

    public AudioClip GetRocketLaunchClip()
    {
        return rocketLaunchSfx;
    }

    public float GetRocketLaunchVolume()
    {
        return Mathf.Clamp01(rocketLaunchVolume);
    }

    public void SetClips(AudioClip rocketLaunch, AudioClip explosion, AudioClip shootingStar, AudioClip starMineStart)
    {
        rocketLaunchSfx = rocketLaunch;
        explosionSfx = explosion;
        shootingStarSfx = shootingStar;
        starMineStartSfx = starMineStart;
    }

    public void PlayRocketLaunch(float volume = 1f)
    {
        PlayOneShot(rocketLaunchSfx, Mathf.Clamp01(volume) * Mathf.Clamp01(rocketLaunchVolume));
    }

    public void PlayExplosion(float volume = 1f)
    {
        PlayOneShot(explosionSfx, Mathf.Clamp01(volume) * Mathf.Clamp01(explosionVolume));
    }

    public void PlayGiantExplosion(float volume = 1f)
    {
        PlayExplosion(volume);
        PlayOneShot(giantExplosionExtraSfx, Mathf.Clamp01(volume) * Mathf.Clamp01(giantExplosionExtraVolume));
    }

    public void PlayShootingStar(float volume = 1f)
    {
        PlayOneShot(shootingStarSfx, Mathf.Clamp01(volume) * Mathf.Clamp01(shootingStarVolume));
    }

    public void PlayShootingStarTap(float volume = 1f)
    {
        PlayOneShot(shootingStarTapSfx, Mathf.Clamp01(volume) * Mathf.Clamp01(shootingStarTapVolume));
    }

    public void PlayStarMineStart(float volume = 1f)
    {
        PlayOneShot(starMineStartSfx, Mathf.Clamp01(volume) * Mathf.Clamp01(starMineStartVolume));
    }

    private void PlayOneShot(AudioClip clip, float volume)
    {
        if (clip == null || oneShotSource == null)
        {
            return;
        }

        oneShotSource.PlayOneShot(clip, Mathf.Clamp01(volume));
    }
}
