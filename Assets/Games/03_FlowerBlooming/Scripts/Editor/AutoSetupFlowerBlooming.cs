// using UnityEngine;
// using UnityEditor;
// using UnityEditor.SceneManagement;
// using UnityEditor.Animations;
// using System.IO;
// using System.Collections.Generic;

// namespace Minigames.FlowerBlooming.Editor
// {
//     /// <summary>
//     /// FlowerBloomingゲームの自動セットアップを行うエディタスクリプト
//     /// </summary>
//     public class AutoSetupFlowerBlooming : EditorWindow
//     {
//         #region Constants
//         private const string ScenePath = "Assets/Games/03_FlowerBlooming/Scenes/FlowerBlooming.unity";
//         private const string PrefabsPath = "Assets/Games/03_FlowerBlooming/Prefabs";
//         private const string SpritesPath = "Assets/Games/03_FlowerBlooming/Sprites";
//         private const string AnimationsPath = "Assets/Games/03_FlowerBlooming/Animations";
//         private const string AudiosPath = "Assets/Games/03_FlowerBlooming/Audios";
        
//         private const int GridRows = 2;
//         private const int GridColumns = 3;
//         private const float GridSpacingX = 2.5f;
//         private const float GridSpacingY = 2.5f;
//         #endregion

//         #region MenuItem
//         [MenuItem("Tools/Auto Setup/Generate FlowerBlooming")]
//         public static void GenerateFlowerBlooming()
//         {
//             // 現在のシーンを保存
//             if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
//             {
//                 // プレハブを生成
//                 CreatePrefabs();
                
//                 // シーンを生成
//                 CreateScene();
                
//                 Debug.Log("FlowerBloomingゲームの自動セットアップが完了しました。");
//             }
//         }
//         #endregion

//         #region Private Methods
//         /// <summary>
//         /// プレハブを生成する
//         /// </summary>
//         private static void CreatePrefabs()
//         {
//             // プレハブ保存用のディレクトリを作成
//             if (!Directory.Exists(PrefabsPath))
//             {
//                 Directory.CreateDirectory(PrefabsPath);
//             }
            
//             // Planterプレハブを生成
//             CreatePlanterPrefab();
            
//             // 通常の花のプレハブを生成
//             for (int i = 1; i <= 6; i++)
//             {
//                 CreateFlowerPrefab($"Flower_Normal_{i:D2}", false);
//             }
            
//             // 特殊な花のプレハブを生成
//             for (int i = 1; i <= 3; i++)
//             {
//                 CreateFlowerPrefab($"Flower_Special_{i:D2}", true);
//             }
            
//             // SFXPlayerプレハブを生成
//             CreateSFXPlayerPrefab();
            
//             // アセットデータベースを更新
//             AssetDatabase.Refresh();
//         }
        
//         /// <summary>
//         /// Planterプレハブを生成する
//         /// </summary>
//         private static void CreatePlanterPrefab()
//         {
//             // Planterオブジェクトを作成
//             GameObject planterObj = new GameObject("Planter");
            
//             // SpriteRendererを追加
//             SpriteRenderer spriteRenderer = planterObj.AddComponent<SpriteRenderer>();
//             spriteRenderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesPath}/Planter_Empty.png");
            
//             // Planterコンポーネントを追加
//             planterObj.AddComponent<Planter>();
            
//             // プレハブとして保存
//             string prefabPath = $"{PrefabsPath}/Planter.prefab";
//             GameObject prefab = PrefabUtility.SaveAsPrefabAsset(planterObj, prefabPath);
            
//             // 一時オブジェクトを削除
//             Object.DestroyImmediate(planterObj);
            
//             Debug.Log($"Planterプレハブを作成しました: {prefabPath}");
//         }
        
//         /// <summary>
//         /// 花のプレハブを生成する
//         /// </summary>
//         /// <param name="flowerName">花の名前</param>
//         /// <param name="isSpecial">特殊な花かどうか</param>
//         private static void CreateFlowerPrefab(string flowerName, bool isSpecial)
//         {
//             // 花オブジェクトを作成
//             GameObject flowerObj = new GameObject(flowerName);
            
//             // SpriteRendererを追加
//             SpriteRenderer spriteRenderer = flowerObj.AddComponent<SpriteRenderer>();
//             spriteRenderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesPath}/{flowerName}.png");
            
//             // 花がプランターよりも前面に表示されるようにSorting Orderを設定
//             spriteRenderer.sortingOrder = 10;
            
//             // Animatorを追加
//             Animator animator = flowerObj.AddComponent<Animator>();
//             animator.runtimeAnimatorController = CreateAnimatorController(flowerName);
            
//             // Flowerコンポーネントを追加
//             Flower flower = flowerObj.AddComponent<Flower>();
            
//             // 特殊な花の場合、パーティクルシステムを追加
//             if (isSpecial)
//             {
//                 // パーティクルシステムの子オブジェクトを作成
//                 GameObject particleObj = new GameObject("Particles");
//                 particleObj.transform.SetParent(flowerObj.transform);
//                 particleObj.transform.localPosition = Vector3.zero;
                
//                 // パーティクルシステムを追加
//                 ParticleSystem particleSystem = particleObj.AddComponent<ParticleSystem>();
                
//                 // パーティクルシステムの設定
//                 var main = particleSystem.main;
//                 main.startLifetime = 1.0f;
//                 main.startSpeed = 2.0f;
//                 main.startSize = 0.1f;
//                 main.startColor = new Color(1f, 1f, 0.5f, 1f); // 黄色っぽい色
//                 main.simulationSpace = ParticleSystemSimulationSpace.World;
                
//                 var emission = particleSystem.emission;
//                 emission.rateOverTime = 10;
                
//                 var shape = particleSystem.shape;
//                 shape.shapeType = ParticleSystemShapeType.Circle;
//                 shape.radius = 0.5f;
                
//                 // パーティクルシステムのレンダラー設定
//                 var renderer = particleSystem.GetComponent<ParticleSystemRenderer>();
//                 renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
                
//                 // パーティクルが花よりも前面に表示されるようにSorting Orderを設定
//                 renderer.sortingOrder = 20;
                
//                 // Flowerコンポーネントにパーティクルシステムを設定
//                 flower.SetSpecial(true);
//                 flower.SetParticleEffect(particleSystem);
//             }
            
//             // プレハブとして保存
//             string prefabPath = $"{PrefabsPath}/{flowerName}.prefab";
//             GameObject prefab = PrefabUtility.SaveAsPrefabAsset(flowerObj, prefabPath);
            
//             // 一時オブジェクトを削除
//             Object.DestroyImmediate(flowerObj);
            
//             Debug.Log($"花のプレハブを作成しました: {prefabPath}");
//         }
        
//         /// <summary>
//         /// SFXPlayerプレハブを生成する
//         /// </summary>
//         private static void CreateSFXPlayerPrefab()
//         {
//             // SFXPlayerオブジェクトを作成
//             GameObject sfxPlayerObj = new GameObject("FlowerBloomingSFXPlayer");
            
//             // AudioSourceを追加
//             AudioSource audioSource = sfxPlayerObj.AddComponent<AudioSource>();
            
//             // FlowerBloomingSFXPlayerコンポーネントを追加
//             FlowerBloomingSFXPlayer sfxPlayer = sfxPlayerObj.AddComponent<FlowerBloomingSFXPlayer>();
            
//             // 音声ファイルを読み込む
//             AudioClip normalSFX = AssetDatabase.LoadAssetAtPath<AudioClip>($"{AudiosPath}/SFX_Flower_Normal_Appear.mp3");
//             AudioClip specialSFX = AssetDatabase.LoadAssetAtPath<AudioClip>($"{AudiosPath}/SFX_Flower_Special_Appear.mp3");
            
//             // SerializedObjectを使用して音声ファイルを設定
//             SerializedObject serializedObject = new SerializedObject(sfxPlayer);
            
//             SerializedProperty normalSFXProperty = serializedObject.FindProperty("normalFlowerSFX");
//             normalSFXProperty.objectReferenceValue = normalSFX;
            
//             SerializedProperty specialSFXProperty = serializedObject.FindProperty("specialFlowerSFX");
//             specialSFXProperty.objectReferenceValue = specialSFX;
            
//             serializedObject.ApplyModifiedProperties();
            
//             // プレハブとして保存
//             string prefabPath = $"{PrefabsPath}/FlowerBloomingSFXPlayer.prefab";
//             GameObject prefab = PrefabUtility.SaveAsPrefabAsset(sfxPlayerObj, prefabPath);
            
//             // 一時オブジェクトを削除
//             Object.DestroyImmediate(sfxPlayerObj);
            
//             Debug.Log($"SFXPlayerプレハブを作成しました: {prefabPath}");
//         }
        
//         /// <summary>
//         /// AnimatorControllerを作成する
//         /// </summary>
//         /// <param name="flowerName">花の名前</param>
//         /// <returns>作成したAnimatorController</returns>
//         private static RuntimeAnimatorController CreateAnimatorController(string flowerName)
//         {
//             // AnimatorControllerのパス
//             string controllerPath = $"{AnimationsPath}/{flowerName}Controller.controller";
            
//             // AnimatorControllerが既に存在する場合は読み込む
//             if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), controllerPath)))
//             {
//                 return AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(controllerPath);
//             }
            
//             // AnimatorControllerを作成
//             AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            
//             // レイヤーを取得
//             AnimatorControllerLayer layer = controller.layers[0];
            
//             // ステートマシンを取得
//             AnimatorStateMachine stateMachine = layer.stateMachine;
            
//             // アニメーションクリップを読み込む
//             AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{AnimationsPath}/{flowerName}.anim");
            
//             if (clip != null)
//             {
//                 // ステートを追加
//                 AnimatorState state = stateMachine.AddState(flowerName);
//                 state.motion = clip;
//                 state.writeDefaultValues = true;
                
//                 // 開始ステートとして設定
//                 stateMachine.defaultState = state;
//             }
//             else
//             {
//                 Debug.LogError($"アニメーションクリップが見つかりません: {AnimationsPath}/{flowerName}.anim");
//             }
            
//             return controller;
//         }
        
//         /// <summary>
//         /// シーンを生成する
//         /// </summary>
//         private static void CreateScene()
//         {
//             // シーン保存用のディレクトリを作成
//             string scenesDirectory = Path.GetDirectoryName(ScenePath);
//             if (!Directory.Exists(scenesDirectory))
//             {
//                 Directory.CreateDirectory(scenesDirectory);
//             }
            
//             // 新しいシーンを作成
//             EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects);
            
//             // カメラの設定
//             Camera mainCamera = Camera.main;
//             if (mainCamera != null)
//             {
//                 mainCamera.orthographic = true;
//                 mainCamera.orthographicSize = 5;
//                 mainCamera.backgroundColor = new Color(0.9f, 0.9f, 1.0f); // 薄い青色
//             }
            
//             // 背景を配置
//             CreateBackground();
            
//             // プランターを配置
//             CreatePlanters();
            
//             // GameManagerを配置
//             CreateGameManager();
            
//             // SFXPlayerを配置
//             CreateSFXPlayer();
            
//             // シーンを保存
//             EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), ScenePath);
            
//             // シーンを開く
//             EditorSceneManager.OpenScene(ScenePath);
            
//             Debug.Log($"FlowerBloomingシーンを作成しました: {ScenePath}");
//         }
        
//         /// <summary>
//         /// 背景を配置する
//         /// </summary>
//         private static void CreateBackground()
//         {
//             // 背景オブジェクトを作成
//             GameObject backgroundObj = new GameObject("Background");
            
//             // SpriteRendererを追加
//             SpriteRenderer spriteRenderer = backgroundObj.AddComponent<SpriteRenderer>();
//             spriteRenderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesPath}/BackGround.png");
//             spriteRenderer.sortingOrder = -1;
            
//             // 背景を画面サイズに合わせる
//             if (spriteRenderer.sprite != null)
//             {
//                 float worldScreenHeight = Camera.main.orthographicSize * 2.0f;
//                 float worldScreenWidth = worldScreenHeight * Camera.main.aspect;
                
//                 float spriteWidth = spriteRenderer.sprite.bounds.size.x;
//                 float spriteHeight = spriteRenderer.sprite.bounds.size.y;
                
//                 backgroundObj.transform.localScale = new Vector3(
//                     worldScreenWidth / spriteWidth,
//                     worldScreenHeight / spriteHeight,
//                     1
//                 );
//             }
//         }
        
//         /// <summary>
//         /// プランターを配置する
//         /// </summary>
//         private static void CreatePlanters()
//         {
//             // Planterプレハブを読み込む
//             GameObject planterPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabsPath}/Planter.prefab");
            
//             if (planterPrefab == null)
//             {
//                 Debug.LogError($"Planterプレハブが見つかりません: {PrefabsPath}/Planter.prefab");
//                 return;
//             }
            
//             // プランターの親オブジェクトを作成
//             GameObject plantersObj = new GameObject("Planters");
            
//             // グリッド状にプランターを配置
//             float startX = -(GridColumns - 1) * GridSpacingX / 2;
//             float startY = -(GridRows - 1) * GridSpacingY / 2;
            
//             for (int row = 0; row < GridRows; row++)
//             {
//                 for (int col = 0; col < GridColumns; col++)
//                 {
//                     // プランターの位置を計算
//                     float x = startX + col * GridSpacingX;
//                     float y = startY + row * GridSpacingY;
//                     Vector3 position = new Vector3(x, y, 0);
                    
//                     // プランターを生成
//                     GameObject planter = PrefabUtility.InstantiatePrefab(planterPrefab) as GameObject;
//                     planter.name = $"Planter_{row}_{col}";
//                     planter.transform.position = position;
//                     planter.transform.SetParent(plantersObj.transform);
//                 }
//             }
//         }
        
//         /// <summary>
//         /// GameManagerを配置する
//         /// </summary>
//         private static void CreateGameManager()
//         {
//             // GameManagerオブジェクトを作成
//             GameObject gameManagerObj = new GameObject("FlowerBloomingGameManager");
            
//             // FlowerBloomingGameManagerコンポーネントを追加
//             FlowerBloomingGameManager gameManager = gameManagerObj.AddComponent<FlowerBloomingGameManager>();
            
//             // 花のプレハブを設定
//             List<GameObject> normalFlowerPrefabs = new List<GameObject>();
//             List<GameObject> specialFlowerPrefabs = new List<GameObject>();
            
//             // 通常の花のプレハブを読み込む
//             for (int i = 1; i <= 6; i++)
//             {
//                 string prefabPath = $"{PrefabsPath}/Flower_Normal_{i:D2}.prefab";
//                 GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
//                 if (prefab != null)
//                 {
//                     normalFlowerPrefabs.Add(prefab);
//                 }
//                 else
//                 {
//                     Debug.LogError($"花のプレハブが見つかりません: {prefabPath}");
//                 }
//             }
            
//             // 特殊な花のプレハブを読み込む
//             for (int i = 1; i <= 3; i++)
//             {
//                 string prefabPath = $"{PrefabsPath}/Flower_Special_{i:D2}.prefab";
//                 GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
//                 if (prefab != null)
//                 {
//                     specialFlowerPrefabs.Add(prefab);
//                 }
//                 else
//                 {
//                     Debug.LogError($"花のプレハブが見つかりません: {prefabPath}");
//                 }
//             }
            
//             // SerializedObjectを使用してプレハブを設定
//             SerializedObject serializedObject = new SerializedObject(gameManager);
            
//             SerializedProperty normalFlowerPrefabsProperty = serializedObject.FindProperty("normalFlowerPrefabs");
//             normalFlowerPrefabsProperty.arraySize = normalFlowerPrefabs.Count;
//             for (int i = 0; i < normalFlowerPrefabs.Count; i++)
//             {
//                 normalFlowerPrefabsProperty.GetArrayElementAtIndex(i).objectReferenceValue = normalFlowerPrefabs[i];
//             }
            
//             SerializedProperty specialFlowerPrefabsProperty = serializedObject.FindProperty("specialFlowerPrefabs");
//             specialFlowerPrefabsProperty.arraySize = specialFlowerPrefabs.Count;
//             for (int i = 0; i < specialFlowerPrefabs.Count; i++)
//             {
//                 specialFlowerPrefabsProperty.GetArrayElementAtIndex(i).objectReferenceValue = specialFlowerPrefabs[i];
//             }
            
//             serializedObject.ApplyModifiedProperties();
//         }
        
//         /// <summary>
//         /// SFXPlayerを配置する
//         /// </summary>
//         private static void CreateSFXPlayer()
//         {
//             // SFXPlayerプレハブを読み込む
//             GameObject sfxPlayerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabsPath}/FlowerBloomingSFXPlayer.prefab");
            
//             if (sfxPlayerPrefab == null)
//             {
//                 Debug.LogError($"SFXPlayerプレハブが見つかりません: {PrefabsPath}/FlowerBloomingSFXPlayer.prefab");
//                 return;
//             }
            
//             // SFXPlayerを生成
//             GameObject sfxPlayer = PrefabUtility.InstantiatePrefab(sfxPlayerPrefab) as GameObject;
            
//             // GameManagerにSFXPlayerを設定
//             FlowerBloomingGameManager gameManager = Object.FindObjectOfType<FlowerBloomingGameManager>();
//             FlowerBloomingSFXPlayer sfxPlayerComponent = sfxPlayer.GetComponent<FlowerBloomingSFXPlayer>();
            
//             if (gameManager != null && sfxPlayerComponent != null)
//             {
//                 SerializedObject serializedObject = new SerializedObject(gameManager);
//                 SerializedProperty sfxPlayerProperty = serializedObject.FindProperty("sfxPlayer");
//                 sfxPlayerProperty.objectReferenceValue = sfxPlayerComponent;
//                 serializedObject.ApplyModifiedProperties();
//             }
//         }
//         #endregion
//     }
// }
