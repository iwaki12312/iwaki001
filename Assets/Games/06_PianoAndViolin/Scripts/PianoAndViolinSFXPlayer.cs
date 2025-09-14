using UnityEngine;

/// <summary>
/// PianoAndViolin用の効果音・音楽再生管理クラス。
/// 各楽器のAudioSourceとボリュームをInspectorから調整可能。
/// </summary>
public class PianoAndViolinSFXPlayer : MonoBehaviour
{
    /// <summary>
    /// ピアノ音楽のボリューム（Inspectorで調整）
    /// </summary>
    [Range(0f, 1f)] public float pianoVolume = 1f;
    /// <summary>
    /// バイオリン音楽のボリューム（Inspectorで調整）
    /// </summary>
    [Range(0f, 1f)] public float violinVolume = 1f;
    /// <summary>
    /// ピアノ＋バイオリン音楽のボリューム（Inspectorで調整）
    /// </summary>
    [Range(0f, 1f)] public float pianoViolinVolume = 1f;

    /// <summary>
    /// ピアノ用AudioSource
    /// </summary>
    public AudioSource pianoSource;
    /// <summary>
    /// バイオリン用AudioSource
    /// </summary>
    public AudioSource violinSource;
    /// <summary>
    /// ピアノ＋バイオリン用AudioSource
    /// </summary>
    public AudioSource pianoViolinSource;

    /// <summary>
    /// ピアノ音楽を再生。再生前に他の音楽を停止。
    /// </summary>
    public void PlayPiano(AudioClip clip)
    {
        StopAll();
        pianoSource.clip = clip;
        pianoSource.volume = pianoVolume;
        pianoSource.Play();
    }

    /// <summary>
    /// バイオリン音楽を再生。再生前に他の音楽を停止。
    /// </summary>
    public void PlayViolin(AudioClip clip)
    {
        StopAll();
        violinSource.clip = clip;
        violinSource.volume = violinVolume;
        violinSource.Play();
    }

    /// <summary>
    /// ピアノ＋バイオリン音楽を再生。再生前に他の音楽を停止。
    /// </summary>
    public void PlayPianoViolin(AudioClip clip)
    {
        StopAll();
        pianoViolinSource.clip = clip;
        pianoViolinSource.volume = pianoViolinVolume;
        pianoViolinSource.Play();
    }

    /// <summary>
    /// 全ての音楽を停止。
    /// </summary>
    public void StopAll()
    {
        pianoSource.Stop();
        violinSource.Stop();
        pianoViolinSource.Stop();
    }
}
