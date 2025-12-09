using UnityEngine;
using UnityEditor;

/// <summary>
/// VegetableSpawnerのエディタ拡張
/// Scene Viewで各野菜スポットの位置をビジュアルに編集できる
/// </summary>
[CustomEditor(typeof(VegetableSpawner))]
public class VegetableSpawnerEditor : Editor
{
    private VegetableSpawner spawner;
    private int selectedSpotIndex = -1;
    private const float HANDLE_SIZE = 0.3f;
    private const float CIRCLE_RADIUS = 1.5f;
    
    void OnEnable()
    {
        spawner = target as VegetableSpawner;
        // Scene Viewでの描画イベントをリッスン
        SceneView.duringSceneGui += OnSceneGUI;
    }
    
    void OnDisable()
    {
        // イベントリスナーを削除
        SceneView.duringSceneGui -= OnSceneGUI;
    }
    
    /// <summary>
    /// Scene Viewでの描画・入力処理
    /// </summary>
    private void OnSceneGUI(SceneView sceneView)
    {
        if (spawner == null) return;
        
        // SerializedObjectの更新
        SerializedObject so = new SerializedObject(spawner);
        SerializedProperty spotsProp = so.FindProperty("vegetableSpots");
        
        if (spotsProp == null || spotsProp.arraySize == 0) return;
        
        // すべてのスポットを描画
        for (int i = 0; i < spotsProp.arraySize; i++)
        {
            SerializedProperty spotProp = spotsProp.GetArrayElementAtIndex(i);
            SerializedProperty posProp = spotProp.FindPropertyRelative("position");
            SerializedProperty scaleProp = spotProp.FindPropertyRelative("scale");
            SerializedProperty radiusProp = spotProp.FindPropertyRelative("colliderRadius");
            
            Vector3 position = posProp.vector3Value;
            float scale = scaleProp.floatValue;
            float radius = radiusProp.floatValue;
            
            // ハンドルを描画してドラッグで移動可能に
            EditorGUI.BeginChangeCheck();
            Vector3 newPosition = Handles.PositionHandle(position, Quaternion.identity);
            
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(spawner, "Move Vegetable Spot");
                posProp.vector3Value = newPosition;
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(spawner);
                selectedSpotIndex = i;
            }
            
            // 選択状態に応じて色を変更
            Color handleColor = (selectedSpotIndex == i) ? Color.green : Color.white;
            Handles.color = handleColor;
            
            // タップ判定範囲を円で描画
            Handles.DrawWireDisc(position, Vector3.forward, radius * scale);
            
            // スポット番号を表示
            Vector3 labelPosition = position + Vector3.up * (radius * scale + 0.5f);
            Handles.Label(labelPosition, $"Spot {i}");
            
            // ハンドルクリックで選択
            if (Handles.Button(position, Quaternion.identity, HANDLE_SIZE * scale, HANDLE_SIZE * scale, Handles.SphereHandleCap))
            {
                selectedSpotIndex = i;
            }
        }
        
        // 変更があれば保存
        if (GUI.changed)
        {
            so.ApplyModifiedProperties();
        }
    }
    
    /// <summary>
    /// Inspectorでの表示をカスタマイズ
    /// </summary>
    public override void OnInspectorGUI()
    {
        // デフォルトのインスペクター表示
        DrawDefaultInspector();
        
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "【Scene Viewでの操作】\n" +
            "・各スポットをドラッグして位置を移動できます\n" +
            "・スポット上のハンドルをクリックして選択\n" +
            "・白い円がタップ判定範囲です\n" +
            "・インスペクターで数値を直接編集することも可能",
            MessageType.Info);
        
        EditorGUILayout.Space();
        
        // クイック設定ボタン
        if (GUILayout.Button("Reset All Positions to Default", GUILayout.Height(30)))
        {
            ResetPositionsToDefault();
        }
        
        EditorGUILayout.Space();
        
        // 現在選択されているスポットの詳細情報
        SerializedObject so = new SerializedObject(spawner);
        SerializedProperty spotsProp = so.FindProperty("vegetableSpots");
        
        if (selectedSpotIndex >= 0 && selectedSpotIndex < spotsProp.arraySize)
        {
            EditorGUILayout.LabelField("Selected Spot Details", EditorStyles.boldLabel);
            SerializedProperty spotProp = spotsProp.GetArrayElementAtIndex(selectedSpotIndex);
            
            EditorGUILayout.PropertyField(spotProp.FindPropertyRelative("position"));
            EditorGUILayout.PropertyField(spotProp.FindPropertyRelative("scale"));
            EditorGUILayout.PropertyField(spotProp.FindPropertyRelative("colliderRadius"));
            
            if (GUI.changed)
            {
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(spawner);
            }
        }
    }
    
    /// <summary>
    /// ポジションをデフォルト値にリセット（2行3列配置）
    /// </summary>
    private void ResetPositionsToDefault()
    {
        Undo.RecordObject(spawner, "Reset Vegetable Positions");
        
        SerializedObject so = new SerializedObject(spawner);
        SerializedProperty spotsProp = so.FindProperty("vegetableSpots");
        
        float[] xPositions = { -4f, 0f, 4f };
        float[] yPositions = { 1.5f, -2f };
        
        int spotIndex = 0;
        for (int row = 0; row < 2; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                if (spotIndex < spotsProp.arraySize)
                {
                    SerializedProperty spotProp = spotsProp.GetArrayElementAtIndex(spotIndex);
                    spotProp.FindPropertyRelative("position").vector3Value = new Vector3(xPositions[col], yPositions[row], 0);
                    spotIndex++;
                }
            }
        }
        
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(spawner);
    }
}
