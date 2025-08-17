using UnityEngine;

/// <summary>
/// シャボン玉の飛ぶ方向を指定する列挙型
/// </summary>
public enum BubbleDirectionType
{
    LeftUp,    // 左上方向（犬少年用）
    RightUp    // 右上方向（猫少女用）
}

/// <summary>
/// 犬少年と猫少女の両方で使用するシャボン玉生成コンポーネント
/// </summary>
public class AnimalBubbleMaker : MonoBehaviour
{
    [Header("シャボン玉生成設定")]
    [SerializeField] private float bubbleInterval = 3.0f; // シャボン玉を作る間隔（秒）
    [SerializeField] private Vector2 spawnOffset = new Vector2(1.0f, 0.5f); // 生成位置のオフセット
    [SerializeField] private BubbleDirectionType directionType = BubbleDirectionType.LeftUp; // シャボン玉の飛ぶ方向
    [SerializeField] private float bubbleSpeed = 5.0f; // シャボン玉の初速
    
    [Header("アニメーション設定")]
    [SerializeField] private string makeBubbleTrigger = "MakeBubble"; // アニメーショントリガー名
    
    private Animator animator;
    private float timer;
    private Vector2 bubbleDirection; // 実際の方向ベクトル
    
    void Start()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError($"{gameObject.name}: Animatorコンポーネントが見つかりません");
        }
        
        // 方向タイプに基づいて方向ベクトルを設定
        SetDirectionFromType();
        
        // ランダムな初期タイマー値を設定（同時に動作開始しないように）
        timer = Random.Range(0f, bubbleInterval);
        
        Debug.Log($"{gameObject.name}: AnimalBubbleMakerを初期化しました。方向={bubbleDirection}, 初期タイマー={timer:F2}秒");
    }
    
    /// <summary>
    /// 方向タイプに基づいて方向ベクトルを設定
    /// </summary>
    private void SetDirectionFromType()
    {
        switch (directionType)
        {
            case BubbleDirectionType.LeftUp:
                bubbleDirection = new Vector2(-1, 1).normalized;
                Debug.Log($"{gameObject.name}: 方向を左上に設定しました");
                break;
            case BubbleDirectionType.RightUp:
                bubbleDirection = new Vector2(1, 1).normalized;
                Debug.Log($"{gameObject.name}: 方向を右上に設定しました");
                break;
        }
    }
    
    void Update()
    {
        timer += Time.deltaTime;
        
        // 一定時間経過したらシャボン玉作成をリクエスト
        if (timer >= bubbleInterval)
        {
            // BubbleMakerManagerが存在しない場合は作成を試行
            if (BubbleMakerManager.Instance == null)
            {
                // GameInitializerを探してBubbleMakerManagerの作成を試行
                GameInitializer gameInitializer = FindObjectOfType<GameInitializer>();
                if (gameInitializer == null)
                {
                    Debug.LogWarning($"{gameObject.name}: GameInitializerが見つかりません。BubbleMakerManagerを直接作成します。");
                    CreateBubbleMakerManager();
                }
                else
                {
                    Debug.LogWarning($"{gameObject.name}: BubbleMakerManagerが見つかりません。少し待ってから再試行します。");
                }
                
                // 少し待ってから再試行
                timer = bubbleInterval - 1.0f;
                return;
            }
            
            // マネージャーにシャボン玉作成をリクエスト
            bool success = BubbleMakerManager.Instance.RequestBubbleCreation(
                transform, 
                spawnOffset, 
                bubbleDirection, 
                bubbleSpeed
            );
            
            if (success)
            {
                // シャボン玉作成アニメーション再生
                if (animator != null)
                {
                    animator.SetTrigger(makeBubbleTrigger);
                    Debug.Log($"{gameObject.name}: シャボン玉作成アニメーションを再生しました");
                }
                
                // タイマーリセット
                timer = 0;
            }
            else
            {
                // リクエストが拒否された場合は少し待ってから再試行
                timer = bubbleInterval - 0.5f;
            }
        }
    }
    
    /// <summary>
    /// BubbleMakerManagerを作成する
    /// </summary>
    private void CreateBubbleMakerManager()
    {
        GameObject managerObj = new GameObject("BubbleMakerManager");
        managerObj.AddComponent<BubbleMakerManager>();
        Debug.Log($"{gameObject.name}: BubbleMakerManagerを作成しました");
    }
    
    /// <summary>
    /// インスペクターでの設定確認用
    /// </summary>
    void OnValidate()
    {
        // 方向タイプに基づいて方向ベクトルを設定（エディタ時のみ）
        if (Application.isPlaying)
        {
            SetDirectionFromType();
        }
        else
        {
            // エディタ時は直接設定
            switch (directionType)
            {
                case BubbleDirectionType.LeftUp:
                    bubbleDirection = new Vector2(-1, 1).normalized;
                    break;
                case BubbleDirectionType.RightUp:
                    bubbleDirection = new Vector2(1, 1).normalized;
                    break;
            }
        }
    }
    
    /// <summary>
    /// 設定値をログ出力（デバッグ用）
    /// </summary>
    [ContextMenu("設定値をログ出力")]
    void LogSettings()
    {
        Debug.Log($"{gameObject.name}の設定:");
        Debug.Log($"  間隔: {bubbleInterval}秒");
        Debug.Log($"  生成オフセット: {spawnOffset}");
        Debug.Log($"  飛ぶ方向: {bubbleDirection}");
        Debug.Log($"  初速: {bubbleSpeed}");
        Debug.Log($"  アニメーショントリガー: {makeBubbleTrigger}");
    }
}
