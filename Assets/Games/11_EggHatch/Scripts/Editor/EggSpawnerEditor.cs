using UnityEngine;
using UnityEditor;

/// <summary>
/// EggSpawnerのカスタムインスペクター
/// Scene View でたまご位置をドラッグ＆ドロップで編集可能にする
/// </summary>
[CustomEditor(typeof(EggSpawner))]
public class EggSpawnerEditor : Editor
{
    private void OnSceneGUI()
    {
        EggSpawner spawner = (EggSpawner)target;
        
        SerializedObject so = new SerializedObject(spawner);
        SerializedProperty spotsProp = so.FindProperty("eggSpots");
        
        if (spotsProp == null) return;
        
        for (int i = 0; i < spotsProp.arraySize; i++)
        {
            SerializedProperty spotProp = spotsProp.GetArrayElementAtIndex(i);
            SerializedProperty positionProp = spotProp.FindPropertyRelative("position");
            SerializedProperty scaleProp = spotProp.FindPropertyRelative("scale");
            
            Vector3 worldPos = positionProp.vector3Value;
            float scale = scaleProp.floatValue;
            
            // ハンドルを表示
            EditorGUI.BeginChangeCheck();
            Vector3 newPos = Handles.PositionHandle(worldPos, Quaternion.identity);
            
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(spawner, "Move Egg Spot");
                positionProp.vector3Value = new Vector3(newPos.x, newPos.y, 0);
                so.ApplyModifiedProperties();
            }
            
            // スポット位置を視覚化（たまご形のアイコン）
            Handles.color = new Color(1f, 0.9f, 0.7f, 0.8f);
            Handles.DrawSolidDisc(worldPos, Vector3.forward, 0.5f * scale);
            
            // 枠線
            Handles.color = new Color(0.6f, 0.4f, 0.2f, 1f);
            Handles.DrawWireDisc(worldPos, Vector3.forward, 0.5f * scale);
            
            // ラベル
            Handles.Label(worldPos + Vector3.up * 0.7f, $"Egg {i + 1}\nScale: {scale:F1}");
        }
    }
    
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("Scene View でたまごの位置をドラッグして調整できます。", MessageType.Info);
    }
}
