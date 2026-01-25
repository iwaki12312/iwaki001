using UnityEngine;
using UnityEditor;

/// <summary>
/// RockSpawnerのカスタムエディタ
/// シーンビューで岩の出現位置をドラッグ＆ドロップで調整可能
/// </summary>
[CustomEditor(typeof(RockSpawner))]
public class RockSpawnerEditor : Editor
{
    private RockSpawner spawner;
    private SerializedProperty spawnPointsProperty;
    private int selectedPointIndex = -1;

    private void OnEnable()
    {
        spawner = (RockSpawner)target;
        spawnPointsProperty = serializedObject.FindProperty("spawnPoints");
    }

    public override void OnInspectorGUI()
    {
        // デフォルトのインスペクター描画
        DrawDefaultInspector();

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("位置調整ツール", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "シーンビューで黄色い球をドラッグして岩の出現位置を調整できます。\n" +
            "球をクリックすると選択状態になります。",
            MessageType.Info);

        if (GUILayout.Button("位置をリセット（2行3列）"))
        {
            ResetPositions();
        }

        if (GUILayout.Button("現在位置にテスト岩を表示"))
        {
            SpawnTestRocks();
        }

        if (GUILayout.Button("テスト岩を削除"))
        {
            ClearTestRocks();
        }
    }

    private void OnSceneGUI()
    {
        if (spawner == null) return;

        serializedObject.Update();

        int pointCount = spawnPointsProperty.arraySize;

        for (int i = 0; i < pointCount; i++)
        {
            SerializedProperty pointProp = spawnPointsProperty.GetArrayElementAtIndex(i);
            SerializedProperty positionProp = pointProp.FindPropertyRelative("position");
            SerializedProperty scaleProp = pointProp.FindPropertyRelative("scale");

            Vector3 position = positionProp.vector3Value;
            float scale = scaleProp.floatValue;

            // ハンドルのサイズ
            float handleSize = HandleUtility.GetHandleSize(position) * 0.2f;

            // 選択状態に応じて色を変更
            if (selectedPointIndex == i)
            {
                Handles.color = Color.green;
            }
            else
            {
                Handles.color = Color.yellow;
            }

            // 位置ハンドル（ドラッグ可能な球）
            EditorGUI.BeginChangeCheck();
            
            // FreeMoveHandleでドラッグ移動
            Vector3 newPosition = Handles.FreeMoveHandle(
                position,
                handleSize,
                Vector3.one * 0.5f,
                Handles.SphereHandleCap
            );

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(spawner, "Move Rock Spawn Point");
                positionProp.vector3Value = new Vector3(newPosition.x, newPosition.y, 0);
                selectedPointIndex = i;
                serializedObject.ApplyModifiedProperties();
            }

            // ラベル表示
            Handles.Label(position + Vector3.up * (scale * 0.7f), 
                $"Rock {i}\n({position.x:F1}, {position.y:F1})", 
                new GUIStyle(GUI.skin.label) { 
                    normal = { textColor = Color.white },
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold
                });

            // 範囲を示す円
            Handles.color = new Color(1f, 1f, 0f, 0.3f);
            Handles.DrawSolidDisc(position, Vector3.forward, scale * 0.5f);
            
            Handles.color = Color.yellow;
            Handles.DrawWireDisc(position, Vector3.forward, scale * 0.5f);
        }

        serializedObject.ApplyModifiedProperties();
    }

    /// <summary>
    /// 位置をデフォルト（2行3列）にリセット
    /// </summary>
    private void ResetPositions()
    {
        Undo.RecordObject(spawner, "Reset Rock Positions");
        
        serializedObject.Update();
        
        spawnPointsProperty.arraySize = 6;
        
        float startX = -4f;
        float startY = -1f;
        float spacingX = 4f;
        float spacingY = 3f;

        for (int row = 0; row < 2; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                int index = row * 3 + col;
                SerializedProperty pointProp = spawnPointsProperty.GetArrayElementAtIndex(index);
                pointProp.FindPropertyRelative("pointName").stringValue = $"Point_{index}";
                pointProp.FindPropertyRelative("position").vector3Value = 
                    new Vector3(startX + col * spacingX, startY - row * spacingY, 0);
                pointProp.FindPropertyRelative("scale").floatValue = 1f;
            }
        }
        
        serializedObject.ApplyModifiedProperties();
        SceneView.RepaintAll();
    }

    /// <summary>
    /// テスト用の岩を現在位置に表示
    /// </summary>
    private void SpawnTestRocks()
    {
        ClearTestRocks();

        // スプライトをロード
        Sprite[] rockSprites = new Sprite[6];
        for (int i = 1; i <= 6; i++)
        {
            rockSprites[i - 1] = AssetDatabase.LoadAssetAtPath<Sprite>(
                $"Assets/Games/13_FossilDigging/Sprites/{i}.png");
        }

        serializedObject.Update();
        int pointCount = spawnPointsProperty.arraySize;

        for (int i = 0; i < pointCount; i++)
        {
            SerializedProperty pointProp = spawnPointsProperty.GetArrayElementAtIndex(i);
            Vector3 position = pointProp.FindPropertyRelative("position").vector3Value;
            float scale = pointProp.FindPropertyRelative("scale").floatValue;

            GameObject testRock = new GameObject($"TestRock_{i}");
            testRock.tag = "EditorOnly";
            testRock.transform.position = position;
            testRock.transform.localScale = Vector3.one * scale;

            SpriteRenderer sr = testRock.AddComponent<SpriteRenderer>();
            if (rockSprites[i % rockSprites.Length] != null)
            {
                sr.sprite = rockSprites[i % rockSprites.Length];
            }
            sr.sortingOrder = 10;
        }
    }

    /// <summary>
    /// テスト岩を削除
    /// </summary>
    private void ClearTestRocks()
    {
        for (int i = 0; i < 10; i++)
        {
            GameObject testRock = GameObject.Find($"TestRock_{i}");
            if (testRock != null)
            {
                DestroyImmediate(testRock);
            }
        }
    }
}
