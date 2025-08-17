using UnityEngine;

/// <summary>
/// アニメーション状態を管理する静的クラス
/// </summary>
public static class AnimationStateManager
{
    // 現在makeBubbleアニメーションを再生しているキャラクター
    private static GameObject currentAnimatingCharacter = null;
    
    // 直前にアニメーションを実行したキャラクター
    private static GameObject lastAnimatedCharacter = null;
    
    // アニメーション開始時間
    private static float lastAnimationStartTime = 0f;
    
    // 最大アニメーション時間（秒）
    private static readonly float MAX_ANIMATION_DURATION = 5f;
    
    // 強制交互生成フラグ
    private static bool forceAlternation = true;
    
    /// <summary>
    /// アニメーション再生中かどうか
    /// </summary>
    public static bool IsAnyCharacterAnimating => currentAnimatingCharacter != null;
    
    /// <summary>
    /// 強制交互生成モードの設定
    /// </summary>
    public static bool ForceAlternation
    {
        get => forceAlternation;
        set
        {
            forceAlternation = value;
            Debug.Log($"強制交互生成モード: {(value ? "有効" : "無効")}");
        }
    }
    
    /// <summary>
    /// 現在アニメーション中のキャラクター名
    /// </summary>
    public static string CurrentAnimatingCharacterName => 
        currentAnimatingCharacter != null ? currentAnimatingCharacter.name : "なし";
    
    /// <summary>
    /// 直前にアニメーションを実行したキャラクター名
    /// </summary>
    public static string LastAnimatedCharacterName => 
        lastAnimatedCharacter != null ? lastAnimatedCharacter.name : "なし";
    
    /// <summary>
    /// アニメーション開始
    /// </summary>
    /// <param name="character">アニメーションを開始するキャラクター</param>
    /// <returns>アニメーション開始が許可されたかどうか</returns>
    public static bool StartAnimation(GameObject character)
    {
        // 長時間アニメーション中の場合は強制リセット
        CheckForceReset();
        
        // 他のキャラクターがアニメーション中の場合は拒否
        if (IsAnyCharacterAnimating && currentAnimatingCharacter != character)
        {
            Debug.Log($"{character.name}のアニメーション開始をスキップ: {CurrentAnimatingCharacterName}が既にアニメーション中");
            return false;
        }
        
        // 強制交互生成モードの場合、直前と同じキャラクターは必ず拒否
        if (forceAlternation && character == lastAnimatedCharacter && lastAnimatedCharacter != null)
        {
            Debug.Log($"{character.name}のアニメーション開始をスキップ: 強制交互生成モードのため（直前: {LastAnimatedCharacterName}）");
            return false;
        }
        
        // 直前のキャラクターと同じで、他のキャラクターが待機中なら拒否（交互生成の強制）
        if (!forceAlternation && character == lastAnimatedCharacter && AreOtherCharactersWaiting(character))
        {
            Debug.Log($"{character.name}のアニメーション開始をスキップ: 他のキャラクターが待機中のため交互生成を優先");
            return false;
        }
        
        // アニメーション開始
        currentAnimatingCharacter = character;
        lastAnimationStartTime = Time.time;
        Debug.Log($"{character.name}のmakeBubbleアニメーションを開始（直前: {LastAnimatedCharacterName}）");
        return true;
    }
    
    /// <summary>
    /// アニメーション終了
    /// </summary>
    /// <param name="character">アニメーションを終了するキャラクター</param>
    public static void EndAnimation(GameObject character)
    {
        // 自分がアニメーション中の場合のみリセット
        if (currentAnimatingCharacter == character)
        {
            lastAnimatedCharacter = character; // 直前のキャラクターを記録
            currentAnimatingCharacter = null;
            Debug.Log($"{character.name}のmakeBubbleアニメーションが終了");
        }
        else if (currentAnimatingCharacter != null)
        {
            Debug.LogWarning($"{character.name}がアニメーション終了を試行しましたが、現在アニメーション中なのは{CurrentAnimatingCharacterName}です");
        }
    }
    
    /// <summary>
    /// 他のキャラクターが待機中かどうか
    /// </summary>
    /// <param name="character">チェック対象のキャラクター</param>
    /// <returns>他のキャラクターが待機中かどうか</returns>
    private static bool AreOtherCharactersWaiting(GameObject character)
    {
        // AnimalBubbleMakerを持つ全オブジェクトを取得
        var allMakers = UnityEngine.Object.FindObjectsOfType<AnimalBubbleMaker>();
        
        // 自分以外のキャラクターで、タイマーが閾値を超えているものがあるか
        foreach (var maker in allMakers)
        {
            if (maker.gameObject != character && maker.IsReadyToMakeBubble())
            {
                Debug.Log($"{maker.gameObject.name}が待機中のため、{character.name}の連続実行を制限");
                return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// 長時間アニメーション中の場合の強制リセット
    /// </summary>
    private static void CheckForceReset()
    {
        if (IsAnyCharacterAnimating && Time.time - lastAnimationStartTime > MAX_ANIMATION_DURATION)
        {
            Debug.LogWarning($"アニメーションが長時間続いているため強制リセットします。キャラクター: {CurrentAnimatingCharacterName}");
            ForceReset();
        }
    }
    
    /// <summary>
    /// 強制的にアニメーション状態をリセット（デバッグ用）
    /// </summary>
    public static void ForceReset()
    {
        Debug.Log("アニメーション状態を強制リセットしました");
        currentAnimatingCharacter = null;
    }
    
    /// <summary>
    /// 現在の状態をログ出力（デバッグ用）
    /// </summary>
    public static void LogCurrentState()
    {
        Debug.Log($"アニメーション状態: アニメーション中={IsAnyCharacterAnimating}, キャラクター={CurrentAnimatingCharacterName}");
    }
}
