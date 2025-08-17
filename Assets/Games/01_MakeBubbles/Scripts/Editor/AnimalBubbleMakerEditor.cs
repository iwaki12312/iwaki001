#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/// <summary>
/// AnimalBubbleMakerのカスタムエディタ
/// </summary>
[CustomEditor(typeof(AnimalBubbleMaker))]
public class AnimalBubbleMakerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // デフォルトのInspectorを描画
        DrawDefaultInspector();
        
        // 空行を追加
        EditorGUILayout.Space();
        
        // デバッグ用のボタンを追加
        EditorGUILayout.LabelField("デバッグ機能", EditorStyles.boldLabel);
        
        AnimalBubbleMaker maker = (AnimalBubbleMaker)target;
        
        // アニメーションテストボタン
        if (GUILayout.Button("アニメーションテスト"))
        {
            if (Application.isPlaying)
            {
                maker.TestAnimation();
            }
            else
            {
                Debug.LogWarning("アニメーションテストはプレイモード中のみ実行できます");
            }
        }
        
        // 設定値ログ出力ボタン
        if (GUILayout.Button("設定値をログ出力"))
        {
            LogSettings(maker);
        }
        
        // アニメーターの状態確認ボタン
        if (GUILayout.Button("アニメーター状態確認"))
        {
            CheckAnimatorState(maker);
        }
        
        // 空行を追加
        EditorGUILayout.Space();
        
        // 情報表示
        if (Application.isPlaying)
        {
            EditorGUILayout.HelpBox("プレイモード中です。アニメーションテストが実行できます。", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox("アニメーションテストを実行するにはプレイモードに入ってください。", MessageType.Warning);
        }
    }
    
    /// <summary>
    /// 設定値をログ出力
    /// </summary>
    private void LogSettings(AnimalBubbleMaker maker)
    {
        Debug.Log($"{maker.gameObject.name}の設定:");
        
        // SerializedObjectを使用してプライベートフィールドにアクセス
        SerializedObject serializedObject = new SerializedObject(maker);
        
        var bubbleInterval = serializedObject.FindProperty("bubbleInterval");
        var spawnOffset = serializedObject.FindProperty("spawnOffset");
        var directionType = serializedObject.FindProperty("directionType");
        var bubbleSpeed = serializedObject.FindProperty("bubbleSpeed");
        var makeBubbleTrigger = serializedObject.FindProperty("makeBubbleTrigger");
        
        Debug.Log($"  間隔: {bubbleInterval.floatValue}秒");
        Debug.Log($"  生成オフセット: {spawnOffset.vector2Value}");
        Debug.Log($"  方向タイプ: {directionType.enumNames[directionType.enumValueIndex]}");
        Debug.Log($"  初速: {bubbleSpeed.floatValue}");
        Debug.Log($"  アニメーショントリガー: {makeBubbleTrigger.stringValue}");
    }
    
    /// <summary>
    /// アニメーターの状態を確認
    /// </summary>
    private void CheckAnimatorState(AnimalBubbleMaker maker)
    {
        Animator animator = maker.GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError($"{maker.gameObject.name}: Animatorコンポーネントが見つかりません");
            return;
        }
        
        Debug.Log($"{maker.gameObject.name}: アニメーター状態確認");
        Debug.Log($"  コントローラー: {animator.runtimeAnimatorController?.name ?? "なし"}");
        Debug.Log($"  有効: {animator.enabled}");
        
        if (animator.runtimeAnimatorController != null)
        {
            var parameters = animator.parameters;
            Debug.Log($"  パラメータ数: {parameters.Length}");
            
            foreach (var param in parameters)
            {
                Debug.Log($"    {param.name} (タイプ: {param.type})");
            }
            
            if (Application.isPlaying)
            {
                var currentState = animator.GetCurrentAnimatorStateInfo(0);
                Debug.Log($"  現在の状態ハッシュ: {currentState.shortNameHash}");
                Debug.Log($"  正規化時間: {currentState.normalizedTime}");
            }
        }
        else
        {
            Debug.LogWarning($"{maker.gameObject.name}: アニメーターコントローラーが設定されていません");
        }
    }
}
#endif
