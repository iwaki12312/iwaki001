using UnityEngine;
using DG.Tweening;

/// <summary>
/// 背景の時間変化を管理するクラス
/// 朝→昼→夜のループで背景と動物を切り替える
/// </summary>
public class BackgroundTimeManager : MonoBehaviour
{
    public static BackgroundTimeManager Instance { get; private set; }
    
    [Header("背景スプライト")]
    [SerializeField] private Sprite morningBackground;    // 朝の背景
    [SerializeField] private Sprite daytimeBackground;    // 昼の背景
    [SerializeField] private Sprite nightBackground;      // 夜の背景
    
    [Header("設定")]
    [SerializeField] private float changeInterval = 30f;  // 切り替え間隔（秒）
    [SerializeField] private float fadeDuration = 1f;     // フェード時間
    
    [Header("参照")]
    [SerializeField] private SpriteRenderer backgroundRenderer;
    [SerializeField] private SpriteRenderer fadeRenderer;  // フェード用の追加レンダラー
    
    private AnimalVoiceTimeOfDay currentTime = AnimalVoiceTimeOfDay.Morning;
    private float timer = 0f;
    private bool isTransitioning = false;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        // 初期背景を設定
        SetBackgroundImmediate(AnimalVoiceTimeOfDay.Morning);
    }
    
    /// <summary>
    /// 背景スプライトを設定（Initializerから呼び出し）
    /// </summary>
    public void SetBackgrounds(Sprite morning, Sprite daytime, Sprite night)
    {
        morningBackground = morning;
        daytimeBackground = daytime;
        nightBackground = night;
    }
    
    /// <summary>
    /// 時間帯切り替え間隔を設定（Initializerから呼び出し）
    /// </summary>
    public void SetChangeInterval(float interval)
    {
        changeInterval = interval;
    }
    
    /// <summary>
    /// レンダラーを設定（Initializerから呼び出し）
    /// </summary>
    public void SetRenderers(SpriteRenderer main, SpriteRenderer fade)
    {
        backgroundRenderer = main;
        fadeRenderer = fade;
    }
    
    void Update()
    {
        if (isTransitioning) return;
        
        timer += Time.deltaTime;
        
        if (timer >= changeInterval)
        {
            timer = 0f;
            TransitionToNextTime();
        }
    }
    
    /// <summary>
    /// 次の時間帯へ遷移
    /// </summary>
    private void TransitionToNextTime()
    {
        AnimalVoiceTimeOfDay nextTime = GetNextTimeOfDay(currentTime);
        TransitionTo(nextTime);
    }
    
    /// <summary>
    /// 次の時間帯を取得
    /// </summary>
    private AnimalVoiceTimeOfDay GetNextTimeOfDay(AnimalVoiceTimeOfDay current)
    {
        switch (current)
        {
            case AnimalVoiceTimeOfDay.Morning:
                return AnimalVoiceTimeOfDay.Daytime;
            case AnimalVoiceTimeOfDay.Daytime:
                return AnimalVoiceTimeOfDay.Night;
            case AnimalVoiceTimeOfDay.Night:
                return AnimalVoiceTimeOfDay.Morning;
            default:
                return AnimalVoiceTimeOfDay.Morning;
        }
    }
    
    /// <summary>
    /// 指定した時間帯へ遷移
    /// </summary>
    public void TransitionTo(AnimalVoiceTimeOfDay targetTime)
    {
        if (isTransitioning) return;
        
        isTransitioning = true;
        
        Debug.Log($"[BackgroundTimeManager] {currentTime} → {targetTime} へ遷移開始");
        
        // 切り替え効果音
        if (AnimalVoiceSFXPlayer.Instance != null)
        {
            AnimalVoiceSFXPlayer.Instance.PlayTimeChangeSound();
        }
        
        // フェードアウト → 背景切り替え → フェードイン
        Sequence seq = DOTween.Sequence();
        
        // フェードレンダラーをフェードイン（黒くなる）
        if (fadeRenderer != null)
        {
            fadeRenderer.color = new Color(0, 0, 0, 0);
            seq.Append(fadeRenderer.DOFade(1f, fadeDuration * 0.5f));
        }
        
        // 背景切り替え
        seq.AppendCallback(() =>
        {
            currentTime = targetTime;
            SetBackgroundSprite(targetTime);
            
            // 動物を入れ替え
            if (AnimalSpawner.Instance != null)
            {
                AnimalSpawner.Instance.SpawnAnimalsForTimeOfDay(targetTime);
            }
        });
        
        // フェードレンダラーをフェードアウト（明るくなる）
        if (fadeRenderer != null)
        {
            seq.Append(fadeRenderer.DOFade(0f, fadeDuration * 0.5f));
        }
        
        seq.OnComplete(() =>
        {
            isTransitioning = false;
            Debug.Log($"[BackgroundTimeManager] {targetTime} への遷移完了");
        });
    }
    
    /// <summary>
    /// 即座に背景を設定
    /// </summary>
    public void SetBackgroundImmediate(AnimalVoiceTimeOfDay time)
    {
        currentTime = time;
        SetBackgroundSprite(time);
        
        // 動物もスポーン
        if (AnimalSpawner.Instance != null)
        {
            AnimalSpawner.Instance.SpawnAnimalsForTimeOfDay(time);
        }
    }
    
    /// <summary>
    /// 背景スプライトを設定
    /// </summary>
    private void SetBackgroundSprite(AnimalVoiceTimeOfDay time)
    {
        if (backgroundRenderer == null) return;
        
        Sprite targetSprite = null;
        switch (time)
        {
            case AnimalVoiceTimeOfDay.Morning:
                targetSprite = morningBackground;
                break;
            case AnimalVoiceTimeOfDay.Daytime:
                targetSprite = daytimeBackground;
                break;
            case AnimalVoiceTimeOfDay.Night:
                targetSprite = nightBackground;
                break;
        }
        
        if (targetSprite != null)
        {
            backgroundRenderer.sprite = targetSprite;
        }
        else
        {
            Debug.LogWarning($"[BackgroundTimeManager] {time}の背景スプライトが設定されていません");
        }
    }
    
    /// <summary>
    /// 現在の時間帯を取得
    /// </summary>
    public AnimalVoiceTimeOfDay GetCurrentTime()
    {
        return currentTime;
    }
    
    void OnDestroy()
    {
        DOTween.Kill(fadeRenderer);
    }
}
