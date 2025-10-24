using UnityEngine;
using UnityEditor;

/// <summary>
/// InsectSpawnerのカスタムエディタ
/// Scene Viewで各昆虫の位置と回転をドラッグで編集可能にする
/// </summary>
[CustomEditor(typeof(InsectSpawner))]
public class InsectSpawnerEditor : Editor
{
    private InsectSpawner spawner;
    private SerializedProperty insectTypesProperty;
    
    // ハンドルの色
    private Color[] handleColors = new Color[]
    {
        new Color(1f, 0.3f, 0.3f, 1f),  // 赤系
        new Color(0.3f, 1f, 0.3f, 1f),  // 緑系
        new Color(0.3f, 0.3f, 1f, 1f),  // 青系
        new Color(1f, 1f, 0.3f, 1f),    // 黄系
        new Color(1f, 0.3f, 1f, 1f),    // マゼンタ系
        new Color(0.3f, 1f, 1f, 1f)     // シアン系
    };
    
    void OnEnable()
    {
        spawner = (InsectSpawner)target;
        insectTypesProperty = serializedObject.FindProperty("insectTypes");
    }
    
    /// <summary>
    /// Scene Viewにハンドルを描画
    /// </summary>
    void OnSceneGUI()
    {
        if (spawner == null) return;
        
        serializedObject.Update();
        
        // insectTypes配列の各要素に対してハンドルを表示
        for (int i = 0; i < insectTypesProperty.arraySize; i++)
        {
            SerializedProperty typeProperty = insectTypesProperty.GetArrayElementAtIndex(i);
            SerializedProperty posProperty = typeProperty.FindPropertyRelative("spawnPosition");
            SerializedProperty rotProperty = typeProperty.FindPropertyRelative("spawnRotation");
            SerializedProperty scaleProperty = typeProperty.FindPropertyRelative("spawnScale");
            SerializedProperty flipXProperty = typeProperty.FindPropertyRelative("flipX");
            SerializedProperty nameProperty = typeProperty.FindPropertyRelative("typeName");
            
            Vector3 position = posProperty.vector3Value;
            float rotation = rotProperty.floatValue;
            float scale = scaleProperty.floatValue;
            bool flipX = flipXProperty.boolValue;
            string typeName = nameProperty.stringValue;
            
            // ハンドルの色を設定
            Color handleColor = handleColors[i % handleColors.Length];
            Handles.color = handleColor;
            
            // ラベル表示(種類名 + スケール + 反転)
            GUIStyle labelStyle = new GUIStyle(EditorStyles.boldLabel);
            labelStyle.normal.textColor = handleColor;
            labelStyle.fontSize = 14;
            string flipText = flipX ? " [FlipX]" : "";
            Handles.Label(position + Vector3.up * 0.8f, $"{typeName} (Scale: {scale:F2}){flipText}", labelStyle);
            
            // 位置ハンドル(ドラッグで移動可能)
            EditorGUI.BeginChangeCheck();
            Vector3 newPosition = Handles.PositionHandle(position, Quaternion.Euler(0, 0, rotation));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(spawner, "Move Insect Spawn Position");
                posProperty.vector3Value = newPosition;
                serializedObject.ApplyModifiedProperties();
            }
            
            // 回転ハンドル(円形のハンドルで回転可能)
            EditorGUI.BeginChangeCheck();
            Quaternion currentRotation = Quaternion.Euler(0, 0, rotation);
            Quaternion newRotation = Handles.Disc(currentRotation, position, Vector3.forward, 1.0f * scale, false, 0);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(spawner, "Rotate Insect Spawn");
                float newAngle = newRotation.eulerAngles.z;
                rotProperty.floatValue = newAngle;
                serializedObject.ApplyModifiedProperties();
            }
            
            // スケールハンドル(Ctrl+ドラッグでサイズ変更)
            EditorGUI.BeginChangeCheck();
            float handleSize = HandleUtility.GetHandleSize(position) * 0.15f;
            float newScale = Handles.ScaleSlider(scale, position, Vector3.up, Quaternion.identity, handleSize * 2f, 0.01f);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(spawner, "Scale Insect Spawn");
                scaleProperty.floatValue = Mathf.Max(0.1f, newScale); // 最小スケール0.1
                serializedObject.ApplyModifiedProperties();
            }
            
            // 小さな円を描画(視覚的に位置とサイズを示す)
            Handles.color = new Color(handleColor.r, handleColor.g, handleColor.b, 0.3f);
            Handles.DrawSolidDisc(position, Vector3.forward, 0.3f * scale);
            
            // ワイヤーフレーム円でサイズを視覚化
            Handles.color = handleColor;
            Handles.DrawWireDisc(position, Vector3.forward, 0.5f * scale);
            
            // 回転方向を示す矢印(スケールと反転に応じて大きさ・向き変更)
            Vector3 direction = Quaternion.Euler(0, 0, rotation) * Vector3.right;
            if (flipX) direction.x *= -1; // 反転時は矢印も反転
            Handles.DrawLine(position, position + direction * 0.6f * scale);
            Handles.ConeHandleCap(0, position + direction * 0.6f * scale, Quaternion.LookRotation(direction, Vector3.forward), 0.2f * scale, EventType.Repaint);
        }
        
        serializedObject.ApplyModifiedProperties();
    }
    
    /// <summary>
    /// Inspectorのカスタム表示(オプション)
    /// </summary>
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        EditorGUILayout.HelpBox(
            "Scene Viewで各昆虫の位置・回転・スケールを直接編集できます。\n" +
            "・カラフルなハンドルをドラッグして位置を移動\n" +
            "・円形のハンドルをドラッグして回転を変更\n" +
            "・縦のハンドル(↑)をドラッグしてスケールを変更\n" +
            "・Flip Xにチェックでスプライトを左右反転\n" +
            "・矢印が昆虫の向きを、円のサイズが大きさを示します",
            MessageType.Info
        );
        
        EditorGUILayout.Space();
        
        // デフォルトのInspector表示
        DrawDefaultInspector();
        
        serializedObject.ApplyModifiedProperties();
    }
}
