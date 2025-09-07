using UnityEngine;

using UnityEngine;

/// <summary>
/// PianoAndViolinゲームのメインコントローラー。
/// タップ判定、音楽再生、アニメーション制御、楽曲の重複再生防止など全体管理。
/// </summary>
public class PianoAndViolinGameController : MonoBehaviour
{
    /// <summary>ピアノのGameObject</summary>
    public GameObject pianoObject;
    /// <summary>バイオリンのGameObject</summary>
    public GameObject violinObject;
    /// <summary>音符のGameObject</summary>
    public GameObject noteObject;
    /// <summary>音楽再生・管理用SFXPlayer</summary>
    public PianoAndViolinSFXPlayer sfxPlayer;
    /// <summary>ピアノのAnimator</summary>
    public Animator pianoAnimator;
    /// <summary>バイオリンのAnimator</summary>
    public Animator violinAnimator;
    /// <summary>音符のAnimator</summary>
    public Animator noteAnimator;

    /// <summary>ピアノ用音楽素材（3種）</summary>
    public AudioClip[] pianoClips;
    /// <summary>バイオリン用音楽素材（3種）</summary>
    public AudioClip[] violinClips;
    /// <summary>ピアノ＋バイオリン用音楽素材（3種）</summary>
    public AudioClip[] pianoViolinClips;

    /// <summary>前回再生したピアノ楽曲のインデックス</summary>
    private int lastPianoIndex = -1;
    /// <summary>前回再生したバイオリン楽曲のインデックス</summary>
    private int lastViolinIndex = -1;
    /// <summary>前回再生したピアノ＋バイオリン楽曲のインデックス</summary>
    private int lastPianoViolinIndex = -1;

    /// <summary>現在再生中の楽器種別</summary>
    private enum PlayType { None, Piano, Violin, PianoViolin }
    private PlayType currentPlayType = PlayType.None;

    /// <summary>ピアノのタップ判定用Collider</summary>
    private Collider2D pianoCollider;
    /// <summary>バイオリンのタップ判定用Collider</summary>
    private Collider2D violinCollider;
    /// <summary>音符のタップ判定用Collider</summary>
    private Collider2D noteCollider;

    /// <summary>
    /// 初期化。各オブジェクトのCollider取得。
    /// </summary>
    void Awake()
    {
        pianoCollider = pianoObject.GetComponent<Collider2D>();
        violinCollider = violinObject.GetComponent<Collider2D>();
        noteCollider = noteObject.GetComponent<Collider2D>();
    }

    /// <summary>
    /// 毎フレームの入力監視。タップ・クリックで各楽器の再生。
    /// 複数同時タップ対応。
    /// </summary>
    void Update()
    {
        // タッチ入力（スマホ）
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
        // マウスクリック（PC）
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (pianoCollider.OverlapPoint(worldPos)) PlayPiano();
            else if (violinCollider.OverlapPoint(worldPos)) PlayViolin();
            else if (noteCollider.OverlapPoint(worldPos)) PlayPianoViolin();
        }
    }

    /// <summary>
    /// ピアノをタップした時の処理。前回と異なる楽曲をランダム再生。
    /// アニメーション制御・再生終了時のリセットも行う。
    /// </summary>
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

    /// <summary>
    /// バイオリンをタップした時の処理。前回と異なる楽曲をランダム再生。
    /// アニメーション制御・再生終了時のリセットも行う。
    /// </summary>
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

    /// <summary>
    /// 音符をタップした時の処理。前回と異なる楽曲をランダム再生。
    /// ピアノ・バイオリン・音符全てのアニメーションを再生。
    /// 再生終了時にリセット。
    /// </summary>
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

    /// <summary>
    /// 楽曲のインデックスをランダム取得。前回再生分は除外。
    /// </summary>
    int GetRandomIndex(int length, int lastIdx)
    {
        if (length <= 1) return 0;
        int idx;
        do { idx = Random.Range(0, length); } while (idx == lastIdx);
        return idx;
    }

    /// <summary>
    /// アニメーションを初期状態に戻す。
    /// </summary>
    void ResetAnimation()
    {
        pianoAnimator.SetTrigger("Idle");
        violinAnimator.SetTrigger("Idle");
        noteAnimator.SetTrigger("Idle");
        currentPlayType = PlayType.None;
    }
}
