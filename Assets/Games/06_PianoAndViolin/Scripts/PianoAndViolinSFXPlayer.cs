using UnityEngine;

public class PianoAndViolinSFXPlayer : MonoBehaviour
{
    [Range(0f, 1f)] public float pianoVolume = 1f;
    [Range(0f, 1f)] public float violinVolume = 1f;
    [Range(0f, 1f)] public float pianoViolinVolume = 1f;

    public AudioSource pianoSource;
    public AudioSource violinSource;
    public AudioSource pianoViolinSource;

    public void PlayPiano(AudioClip clip)
    {
        StopAll();
        pianoSource.clip = clip;
        pianoSource.volume = pianoVolume;
        pianoSource.Play();
    }
    public void PlayViolin(AudioClip clip)
    {
        StopAll();
        violinSource.clip = clip;
        violinSource.volume = violinVolume;
        violinSource.Play();
    }
    public void PlayPianoViolin(AudioClip clip)
    {
        StopAll();
        pianoViolinSource.clip = clip;
        pianoViolinSource.volume = pianoViolinVolume;
        pianoViolinSource.Play();
    }
    public void StopAll()
    {
        pianoSource.Stop();
        violinSource.Stop();
        pianoViolinSource.Stop();
    }
}
