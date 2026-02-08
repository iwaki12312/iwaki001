using UnityEngine;
using DG.Tweening;

/// <summary>
/// 背景の時間変化を管理するクラス
/// 朝→昼→夜のループで背景と動物を切り替える
/// Scene Viewで各背景の位置・スケールを調整可能
/// </summary>
[ExecuteAlways]
public class BackgroundTimeManager : MonoBehaviour
{
    public static BackgroundTimeManager Instance { get; private set; }
    
    [Header("背景SpriteRenderer（Scene内で配置）")]
    [SerializeField] private SpriteRenderer morningRenderer;    // 朝の背景
    [SerializeField] private SpriteRenderer daytimeRenderer;    // 昼の背景
    [SerializeField] private SpriteRenderer nightRenderer;      // 夜の背景
    
    [Header("フェードオーバーレイ")]
    [SerializeField] private SpriteRenderer fadeRenderer;  // フェード用レンダラー
    
    [Header("設定")]
    [SerializeField] private float changeInterval = 30f;  // 切り替え間隔（秒）
    [SerializeField] private float fadeDuration = 1f;     // フェード時間
    
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
    /// 時間帯切り替え間隔を設定（Initializerから呼び出し）
    /// </summary>
    public void SetChangeInterval(float interval)
    {
        changeInterval = interval;
    }
    
    /// <summary>
    /// SpriteRendererを設定（Initializerから呼び出し）
    /// </summary>
    public void SetRenderers(SpriteRenderer morning, SpriteRenderer daytime, SpriteRenderer night, SpriteRenderer fade)
    {
        morningRenderer = morning;
        daytimeRenderer = daytime;
        nightRenderer = night;
        fadeRenderer = fade;
    }
    
    void Update()
    {
        if (!Application.isPlaying) return; // Edit modeでは時間経過なし
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
    /// 背景の表示を切り替え（3つのSpriteRendererを切り替え）
    /// </summary>
    private void SetBackgroundSprite(AnimalVoiceTimeOfDay time)
    {
        if (morningRenderer == null || daytimeRenderer == null || nightRenderer == null)
        {
            Debug.LogWarning("[BackgroundTimeManager] 背景SpriteRendererが設定されていません");
            return;
        }
        
        // すべて非表示にしてから、対象のみ表示
        morningRenderer.enabled = false;
        daytimeRenderer.enabled = false;
        nightRenderer.enabled = false;
        
        switch (time)
        {
            case AnimalVoiceTimeOfDay.Morning:
                morningRenderer.enabled = true;
                break;
            case AnimalVoiceTimeOfDay.Daytime:
                daytimeRenderer.enabled = true;
                break;
            case AnimalVoiceTimeOfDay.Night:
                nightRenderer.enabled = true;
                break;
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
