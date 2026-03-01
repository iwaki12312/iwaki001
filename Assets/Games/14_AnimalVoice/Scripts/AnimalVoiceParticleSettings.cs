using UnityEngine;

/// <summary>
/// パーティクルのスケールを保持する設定コンポーネント。
/// AnimalVoiceInitializerから値を受け取り、AnimalVoiceParticleHelperが参照する。
/// </summary>
public class AnimalVoiceParticleSettings : MonoBehaviour
{
    public static AnimalVoiceParticleSettings Instance { get; private set; }

    private float normalParticleScale = 1.0f;
    private float rareParticleScale = 1.0f;

    public float NormalParticleScale => normalParticleScale;
    public float RareParticleScale => rareParticleScale;

    /// <summary>
    /// Initializerからスケール値を設定
    /// </summary>
    public void SetScales(float normal, float rare)
    {
        normalParticleScale = normal;
        rareParticleScale = rare;
    }

    void Awake()
    {
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
