using UnityEngine;
using System.Collections;

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
    [SerializeField] private Vector2 spawnOffset = new Vector2(1.0f, 2.5f); // 生成位置のオフセット
    [SerializeField] private BubbleDirectionType directionType = BubbleDirectionType.LeftUp; // シャボン玉の飛ぶ方向
    [SerializeField] private float bubbleSpeed = 0.1f; // シャボン玉の初速
    
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
        else
        {
            Debug.Log($"{gameObject.name}: Animatorコンポーネントを取得しました。コントローラー: {animator.runtimeAnimatorController?.name}");
            
            // アニメーターのパラメータを確認
            if (animator.runtimeAnimatorController != null)
            {
                var parameters = animator.parameters;
                Debug.Log($"{gameObject.name}: アニメーターパラメータ数: {parameters.Length}");
                foreach (var param in parameters)
                {
                    Debug.Log($"  パラメータ: {param.name} (タイプ: {param.type})");
                }
            }
        }
        
        // 方向タイプに基づいて方向ベクトルを設定
        SetDirectionFromType();
        
        // 犬少年と猫少女で初期タイマー値を調整（交互生成を促進）
        if (gameObject.name.ToLower().Contains("dog"))
        {
            timer = Random.Range(0f, bubbleInterval * 0.4f); // 犬少年は前半
            Debug.Log($"{gameObject.name}: 犬少年として初期化（前半開始）");
        }
        else if (gameObject.name.ToLower().Contains("cat"))
        {
            timer = Random.Range(bubbleInterval * 0.6f, bubbleInterval); // 猫少女は後半
            Debug.Log($"{gameObject.name}: 猫少女として初期化（後半開始）");
        }
        else
        {
            timer = Random.Range(0f, bubbleInterval);
            Debug.Log($"{gameObject.name}: 不明なキャラクターとして初期化");
        }
        
        Debug.Log($"{gameObject.name}: AnimalBubbleMakerを初期化しました。方向={bubbleDirection}, 初期タイマー={timer:F2}秒, トリガー名={makeBubbleTrigger}");
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
        
        // 一定時間経過したらアニメーション開始（シャボン玉生成はアニメーションイベントから行う）
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
            
            // シャボン玉の上限チェック
            int currentBubbles = BubbleMakerManager.Instance.CountBubbles();
            bool canCreateBubbles = !BubbleMakerManager.Instance.IsCreatingBubble && 
                                   currentBubbles <= BubbleMakerManager.Instance.MaxBubbles - 3;
            
            // 他のキャラクターがアニメーション中でないことを確認
            bool canStartAnimation = AnimationStateManager.StartAnimation(gameObject);
            
            if (canCreateBubbles && canStartAnimation)
            {
                // アニメーション再生（シャボン玉生成はアニメーションイベントから行われる）
                if (animator != null)
                {
                    Debug.Log($"{gameObject.name}: アニメーション切り替えを開始します。現在の状態: {animator.GetCurrentAnimatorStateInfo(0).shortNameHash}");
                    
                    // トリガーパラメータが存在するか確認
                    bool hasParameter = false;
                    foreach (var param in animator.parameters)
                    {
                        if (param.name == makeBubbleTrigger && param.type == AnimatorControllerParameterType.Trigger)
                        {
                            hasParameter = true;
                            break;
                        }
                    }
                    
                    if (hasParameter)
                    {
                        animator.SetTrigger(makeBubbleTrigger);
                        Debug.Log($"{gameObject.name}: トリガー '{makeBubbleTrigger}' を実行しました（シャボン玉はアニメーションイベントから生成されます）");
                        
                        // 少し待ってから状態を確認
                        StartCoroutine(CheckAnimationState());
                    }
                    else
                    {
                        Debug.LogError($"{gameObject.name}: トリガーパラメータ '{makeBubbleTrigger}' が見つかりません！");
                        
                        // パラメータが見つからない場合はアニメーション状態をリセット
                        AnimationStateManager.EndAnimation(gameObject);
                        
                        // 代替案として、利用可能なトリガーを試す
                        foreach (var param in animator.parameters)
                        {
                            if (param.type == AnimatorControllerParameterType.Trigger)
                            {
                                Debug.Log($"{gameObject.name}: 利用可能なトリガー: {param.name}");
                                if (param.name.ToLower().Contains("bubble") || param.name.ToLower().Contains("make"))
                                {
                                    Debug.Log($"{gameObject.name}: 代替トリガー '{param.name}' を試行します");
                                    animator.SetTrigger(param.name);
                                    break;
                                }
                            }
                        }
                    }
                }
                else
                {
                    Debug.LogError($"{gameObject.name}: Animatorが見つかりません！");
                    // Animatorが見つからない場合はアニメーション状態をリセット
                    AnimationStateManager.EndAnimation(gameObject);
                }
            }
            else
            {
                // アニメーション開始条件を満たさない場合の詳細ログ
                if (!canCreateBubbles)
                {
                    Debug.Log($"{gameObject.name}: シャボン玉の数が上限に達しているため、アニメーションをスキップします（現在: {currentBubbles}/{BubbleMakerManager.Instance.MaxBubbles}）");
                }
                
                if (!canStartAnimation)
                {
                    Debug.Log($"{gameObject.name}: 他のキャラクター（{AnimationStateManager.CurrentAnimatingCharacterName}）がアニメーション中のため、アニメーションをスキップします");
                }
                
                // アニメーション状態をリセット（スキップした場合）
                if (canStartAnimation)
                {
                    AnimationStateManager.EndAnimation(gameObject);
                }
            }
            
            // タイマーリセット
            timer = 0;
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
    /// アニメーションイベントから呼び出されるシャボン玉生成メソッド
    /// </summary>
    public void CreateBubbleFromAnimationEvent()
    {
        Debug.Log($"{gameObject.name}: アニメーションイベントからシャボン玉生成を開始します");
        
        // BubbleMakerManagerが存在するか確認
        if (BubbleMakerManager.Instance == null)
        {
            Debug.LogWarning($"{gameObject.name}: シャボン玉生成時にBubbleMakerManagerが見つかりません");
            return;
        }
        
        // シャボン玉生成をリクエスト
        bool success = BubbleMakerManager.Instance.RequestBubbleCreation(
            transform, 
            spawnOffset, 
            bubbleDirection, 
            bubbleSpeed
        );
        
        if (success)
        {
            Debug.Log($"{gameObject.name}: アニメーションイベントからシャボン玉を生成しました");
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: シャボン玉生成リクエストが拒否されました（上限に達している可能性があります）");
        }
    }
    
    /// <summary>
    /// アニメーション終了時に呼び出されるメソッド（アニメーションイベントから呼び出される）
    /// </summary>
    public void OnAnimationEnd()
    {
        Debug.Log($"{gameObject.name}: アニメーション終了イベントが呼び出されました");
        AnimationStateManager.EndAnimation(gameObject);
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
    /// アニメーション状態を確認するコルーチン
    /// </summary>
    private IEnumerator CheckAnimationState()
    {
        yield return new WaitForSeconds(0.1f); // 少し待つ
        
        if (animator != null)
        {
            var currentState = animator.GetCurrentAnimatorStateInfo(0);
            Debug.Log($"{gameObject.name}: アニメーション状態確認 - ハッシュ: {currentState.shortNameHash}, 正規化時間: {currentState.normalizedTime}");
            
            // 状態名を取得（可能であれば）
            if (animator.runtimeAnimatorController != null)
            {
                foreach (var clip in animator.runtimeAnimatorController.animationClips)
                {
                    if (Animator.StringToHash(clip.name) == currentState.shortNameHash)
                    {
                        Debug.Log($"{gameObject.name}: 現在のアニメーション: {clip.name}");
                        break;
                    }
                }
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
    
    /// <summary>
    /// 手動でアニメーションをテストするメソッド（デバッグ用）
    /// </summary>
    [ContextMenu("アニメーションテスト")]
    public void TestAnimation()
    {
        if (animator != null)
        {
            Debug.Log($"{gameObject.name}: 手動でアニメーションをテストします");
            animator.SetTrigger(makeBubbleTrigger);
            StartCoroutine(CheckAnimationState());
        }
        else
        {
            Debug.LogError($"{gameObject.name}: Animatorが見つかりません");
        }
    }
    
    /// <summary>
    /// Inspectorから直接呼び出せるアニメーションテストメソッド
    /// </summary>
    public void TestAnimationFromInspector()
    {
        TestAnimation();
    }
}
