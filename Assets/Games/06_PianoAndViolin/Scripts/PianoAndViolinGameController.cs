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

        // BGMが再生されている場合はフェードアウトして停止
        if (BGMManager.Instance != null && BGMManager.Instance.GetComponent<AudioSource>().isPlaying)
        {
            BGMManager.Instance.FadeOutBGM(1.0f); // 1秒かけてフェードアウト
        }
        
    }

    /// <summary>
    /// 毎フレームの入力監視。タップ・クリックで各楽器の再生。
    /// 複数同時タップ対応。
    /// </summary>
    /// <summary>
    /// 直前にタップしたオブジェクト種別（連続タップ判定用）
    /// </summary>
    private PlayType lastTapType = PlayType.None;

    /// <summary>
    /// 毎フレームの入力監視。
    /// 連続で同じオブジェクトをタップした場合は音楽を停止。
    /// 違うオブジェクトをタップした場合は次の音楽を再生。
    /// </summary>
    void Update()
    {
        // タッチ入力（スマホ）
        foreach (var touch in Input.touches)
        {
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(touch.position);
            if (touch.phase == TouchPhase.Began)
            {
                // ピアノオブジェクトの連続タップ判定
                if (pianoCollider.OverlapPoint(worldPos))
                {
                    // 直前もピアノかつ再生中なら停止
                    if (currentPlayType == PlayType.Piano && lastTapType == PlayType.Piano)
                    {
                        sfxPlayer.StopAll(); // 音楽停止
                        ResetAnimation();    // アニメーション初期化
                        currentPlayType = PlayType.None;
                    }
                    else
                    {
                        PlayPiano();         // 新しい音楽再生
                        lastTapType = PlayType.Piano;
                    }
                }
                // バイオリンオブジェクトの連続タップ判定
                else if (violinCollider.OverlapPoint(worldPos))
                {
                    if (currentPlayType == PlayType.Violin && lastTapType == PlayType.Violin)
                    {
                        sfxPlayer.StopAll();
                        ResetAnimation();
                        currentPlayType = PlayType.None;
                    }
                    else
                    {
                        PlayViolin();
                        lastTapType = PlayType.Violin;
                    }
                }
                // 音符オブジェクトの連続タップ判定
                else if (noteCollider.OverlapPoint(worldPos))
                {
                    if (currentPlayType == PlayType.PianoViolin && lastTapType == PlayType.PianoViolin)
                    {
                        sfxPlayer.StopAll();
                        ResetAnimation();
                        currentPlayType = PlayType.None;
                    }
                    else
                    {
                        PlayPianoViolin();
                        lastTapType = PlayType.PianoViolin;
                    }
                }
            }
        }
        // マウスクリック（PC）
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            // ピアノオブジェクトの連続クリック判定
            if (pianoCollider.OverlapPoint(worldPos))
            {
                if (currentPlayType == PlayType.Piano && lastTapType == PlayType.Piano)
                {
                    sfxPlayer.StopAll();
                    ResetAnimation();
                    currentPlayType = PlayType.None;
                }
                else
                {
                    PlayPiano();
                    lastTapType = PlayType.Piano;
                }
            }
            // バイオリンオブジェクトの連続クリック判定
            else if (violinCollider.OverlapPoint(worldPos))
            {
                if (currentPlayType == PlayType.Violin && lastTapType == PlayType.Violin)
                {
                    sfxPlayer.StopAll();
                    ResetAnimation();
                    currentPlayType = PlayType.None;
                }
                else
                {
                    PlayViolin();
                    lastTapType = PlayType.Violin;
                }
            }
            // 音符オブジェクトの連続クリック判定
            else if (noteCollider.OverlapPoint(worldPos))
            {
                if (currentPlayType == PlayType.PianoViolin && lastTapType == PlayType.PianoViolin)
                {
                    sfxPlayer.StopAll();
                    ResetAnimation();
                    currentPlayType = PlayType.None;
                }
                else
                {
                    PlayPianoViolin();
                    lastTapType = PlayType.PianoViolin;
                }
            }
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
        // トリガー競合防止
        pianoAnimator.ResetTrigger("Idle");
        pianoAnimator.ResetTrigger("Play");
        pianoAnimator.SetTrigger("Play");
        violinAnimator.ResetTrigger("Play");
        violinAnimator.ResetTrigger("Idle");
        violinAnimator.SetTrigger("Idle");
        noteAnimator.ResetTrigger("Play");
        noteAnimator.ResetTrigger("Idle");
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
        pianoAnimator.ResetTrigger("Play");
        pianoAnimator.ResetTrigger("Idle");
        pianoAnimator.SetTrigger("Idle");
        violinAnimator.ResetTrigger("Idle");
        violinAnimator.ResetTrigger("Play");
        violinAnimator.SetTrigger("Play");
        noteAnimator.ResetTrigger("Play");
        noteAnimator.ResetTrigger("Idle");
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
        pianoAnimator.ResetTrigger("Idle");
        pianoAnimator.ResetTrigger("Play");
        pianoAnimator.SetTrigger("Play");
        violinAnimator.ResetTrigger("Idle");
        violinAnimator.ResetTrigger("Play");
        violinAnimator.SetTrigger("Play");
        noteAnimator.ResetTrigger("Idle");
        noteAnimator.ResetTrigger("Play");
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
        // トリガー競合防止
        pianoAnimator.ResetTrigger("Play");
        pianoAnimator.ResetTrigger("Idle");
        pianoAnimator.SetTrigger("Idle");
        violinAnimator.ResetTrigger("Play");
        violinAnimator.ResetTrigger("Idle");
        violinAnimator.SetTrigger("Idle");
        noteAnimator.ResetTrigger("Play");
        noteAnimator.ResetTrigger("Idle");
        noteAnimator.SetTrigger("Idle");
        currentPlayType = PlayType.None;
    }
}
