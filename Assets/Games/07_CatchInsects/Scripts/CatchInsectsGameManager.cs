using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// CatchInsectsゲーム全体を管理するクラス
/// 昆虫の中央表示を制御
/// </summary>
public class CatchInsectsGameManager : MonoBehaviour
{
    [Header("UI参照")]
    [SerializeField] private GameObject displayPanel;      // 中央表示パネル
    [SerializeField] private Image displayImage;           // 昆虫表示用Image
    [SerializeField] private float displayDuration = 2f;   // 表示時間(秒)
    [SerializeField] private float fadeDuration = 0.5f;    // フェード時間
    
    public static CatchInsectsGameManager Instance { get; private set; }
    
    private bool isDisplaying = false;
    
    /// <summary>
    /// 昆虫を中央表示中かどうか(外部から参照可能)
    /// </summary>
    public bool IsDisplayingInsect => isDisplaying;
    
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
        
        // 初期状態では非表示
        if (displayPanel != null)
        {
            displayPanel.SetActive(false);
        }
    }
    
    void Start()
    {
        Debug.Log("[CatchInsectsGameManager] ゲーム開始");
    }
    
    /// <summary>
    /// 捕まえた昆虫を画面中央に表示
    /// </summary>
    public void DisplayInsect(Sprite insectSprite, bool isRare)
    {
        if (isDisplaying)
        {
            Debug.LogWarning("[CatchInsectsGameManager] 既に表示中です");
            return;
        }
        
        if (displayImage == null || displayPanel == null)
        {
            Debug.LogError("[CatchInsectsGameManager] UI参照が設定されていません");
            return;
        }
        
        isDisplaying = true;
        
        // スプライトを設定
        displayImage.sprite = insectSprite;
        displayImage.color = new Color(1, 1, 1, 0); // 透明から開始
        
        // パネルを表示
        displayPanel.SetActive(true);
        
        // フェードイン → 表示 → フェードアウト
        DOTween.Sequence()
            .Append(displayImage.DOFade(1f, fadeDuration))
            .AppendInterval(displayDuration)
            .Append(displayImage.DOFade(0f, fadeDuration))
            .OnComplete(() =>
            {
                displayPanel.SetActive(false);
                isDisplaying = false; // フェードアウト完了でフラグOFF
                Debug.Log("[CatchInsectsGameManager] 昆虫表示完了、タップ再開可能");
            });
        
        Debug.Log($"[CatchInsectsGameManager] 昆虫表示開始: レア={isRare}、タップ無効化");
    }
    
    /// <summary>
    /// エディタ拡張からUI参照を設定
    /// </summary>
    public void SetUIReferences(GameObject panel, Image image)
    {
        displayPanel = panel;
        displayImage = image;
        Debug.Log("[CatchInsectsGameManager] UI参照を設定しました");
    }
}
