using UnityEngine;

using UnityEngine;

public class PianoAndViolinGameController : MonoBehaviour
{
    public GameObject pianoObject;
    public GameObject violinObject;
    public GameObject noteObject;
    public PianoAndViolinSFXPlayer sfxPlayer;
    public Animator pianoAnimator;
    public Animator violinAnimator;
    public Animator noteAnimator;

    public AudioClip[] pianoClips;
    public AudioClip[] violinClips;
    public AudioClip[] pianoViolinClips;

    private int lastPianoIndex = -1;
    private int lastViolinIndex = -1;
    private int lastPianoViolinIndex = -1;

    private enum PlayType { None, Piano, Violin, PianoViolin }
    private PlayType currentPlayType = PlayType.None;

    private Collider2D pianoCollider;
    private Collider2D violinCollider;
    private Collider2D noteCollider;

    void Awake()
    {
        pianoCollider = pianoObject.GetComponent<Collider2D>();
        violinCollider = violinObject.GetComponent<Collider2D>();
        noteCollider = noteObject.GetComponent<Collider2D>();
    }

    void Update()
    {
        foreach (var touch in Input.touches)
        {
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(touch.position);
            if (touch.phase == TouchPhase.Began)
            {
                if (pianoCollider.OverlapPoint(worldPos)) PlayPiano();
                else if (violinCollider.OverlapPoint(worldPos)) PlayViolin();
                else if (noteCollider.OverlapPoint(worldPos)) PlayPianoViolin();
            }
        }
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (pianoCollider.OverlapPoint(worldPos)) PlayPiano();
            else if (violinCollider.OverlapPoint(worldPos)) PlayViolin();
            else if (noteCollider.OverlapPoint(worldPos)) PlayPianoViolin();
        }
    }

    void PlayPiano()
    {
        if (pianoClips.Length == 0) return;
        int idx = GetRandomIndex(pianoClips.Length, lastPianoIndex);
        lastPianoIndex = idx;
        sfxPlayer.PlayPiano(pianoClips[idx]);
        currentPlayType = PlayType.Piano;
        pianoAnimator.SetTrigger("Play");
        violinAnimator.SetTrigger("Idle");
        noteAnimator.SetTrigger("Idle");
        Invoke("ResetAnimation", sfxPlayer.pianoSource.clip.length);
    }
    void PlayViolin()
    {
        if (violinClips.Length == 0) return;
        int idx = GetRandomIndex(violinClips.Length, lastViolinIndex);
        lastViolinIndex = idx;
        sfxPlayer.PlayViolin(violinClips[idx]);
        currentPlayType = PlayType.Violin;
        pianoAnimator.SetTrigger("Idle");
        violinAnimator.SetTrigger("Play");
        noteAnimator.SetTrigger("Idle");
        Invoke("ResetAnimation", sfxPlayer.violinSource.clip.length);
    }
    void PlayPianoViolin()
    {
        if (pianoViolinClips.Length == 0) return;
        int idx = GetRandomIndex(pianoViolinClips.Length, lastPianoViolinIndex);
        lastPianoViolinIndex = idx;
        sfxPlayer.PlayPianoViolin(pianoViolinClips[idx]);
        currentPlayType = PlayType.PianoViolin;
        pianoAnimator.SetTrigger("Play");
        violinAnimator.SetTrigger("Play");
        noteAnimator.SetTrigger("Play");
        Invoke("ResetAnimation", sfxPlayer.pianoViolinSource.clip.length);
    }
    int GetRandomIndex(int length, int lastIdx)
    {
        if (length <= 1) return 0;
        int idx;
        do { idx = Random.Range(0, length); } while (idx == lastIdx);
        return idx;
    }
    void ResetAnimation()
    {
        pianoAnimator.SetTrigger("Idle");
        violinAnimator.SetTrigger("Idle");
        noteAnimator.SetTrigger("Idle");
        currentPlayType = PlayType.None;
    }
}
