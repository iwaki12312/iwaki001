using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 昆虫の種類データ
/// 6種類の大まかな種類、それぞれに通常3色+レア1色
/// </summary>
[System.Serializable]
public class InsectType
{
    public string typeName = "Type A";           // 種類名(Inspector表示用)
    public Vector3 spawnPosition = Vector3.zero; // 固定出現位置(Inspector編集可能)
    public float spawnRotation = 0f;             // 固定回転角度(Inspector編集可能)
    public float spawnScale = 1f;                // 固定スケール(Inspector編集可能)
    public bool flipX = false;                   // スプライトを左右反転(Inspector編集可能)
    public Sprite normalSprite1;                 // 通常色1
    public Sprite normalSprite2;                 // 通常色2
    public Sprite normalSprite3;                 // 通常色3
    public Sprite rareSprite;                    // レア色
}

/// <summary>
/// 昆虫のスポーンを管理するクラス
/// 3秒ごとに6種類の固定位置から1つ選んでスポーン
/// </summary>
public class InsectSpawner : MonoBehaviour
{
    [Header("昆虫種類設定 (6種類)")]
    [SerializeField] private InsectType[] insectTypes = new InsectType[6];
    
    [Header("スポーン設定")]
    [SerializeField] private GameObject insectPrefab;           // 昆虫プレハブ
    [SerializeField] private float spawnInterval = 3f;          // スポーン間隔(秒)
    [SerializeField] private int maxSimultaneousInsects = 4;    // 同時最大数
    [SerializeField] private float rareSpawnChance = 0.2f;      // レア出現確率(20%)
    
    private List<GameObject> activeInsects = new List<GameObject>();
    private HashSet<int> occupiedPositions = new HashSet<int>(); // 使用中のポジションインデックス
    private float lastSpawnTime;
    
    void Start()
    {
        lastSpawnTime = Time.time;
        Debug.Log("[InsectSpawner] スポーナー開始");
    }
    
    void Update()
    {
        // 非アクティブな昆虫をリストから削除
        activeInsects.RemoveAll(insect => insect == null);
        
        // 中央表示中はスポーンを停止
        if (CatchInsectsGameManager.Instance != null && CatchInsectsGameManager.Instance.IsDisplayingInsect)
        {
            return;
        }
        
        // スポーン処理
        if (Time.time - lastSpawnTime >= spawnInterval)
        {
            if (activeInsects.Count < maxSimultaneousInsects)
            {
                SpawnInsect();
            }
            lastSpawnTime = Time.time;
        }
    }
    
    /// <summary>
    /// 昆虫をスポーン
    /// </summary>
    private void SpawnInsect()
    {
        // insectTypesが設定されていない、または空の場合
        if (insectTypes == null || insectTypes.Length == 0)
        {
            Debug.LogWarning("[InsectSpawner] insectTypesが設定されていません");
            return;
        }
        
        // 利用可能なポジション(使用中でないもの)を取得
        List<int> availablePositions = new List<int>();
        for (int i = 0; i < insectTypes.Length; i++)
        {
            if (!occupiedPositions.Contains(i))
            {
                availablePositions.Add(i);
            }
        }
        
        // 利用可能なポジションがない場合
        if (availablePositions.Count == 0)
        {
            Debug.LogWarning("[InsectSpawner] すべてのポジションが使用中です");
            return;
        }
        
        // 利用可能なポジションからランダムに1つ選択
        int selectedIndex = availablePositions[Random.Range(0, availablePositions.Count)];
        InsectType selectedType = insectTypes[selectedIndex];
        
        // ポジションを使用中としてマーク
        occupiedPositions.Add(selectedIndex);
        
        // レア判定
        bool isRare = Random.value < rareSpawnChance;
        
        // スプライト選択(その種類内の4つから)
        Sprite selectedSprite;
        if (isRare && selectedType.rareSprite != null)
        {
            selectedSprite = selectedType.rareSprite;
        }
        else
        {
            // 通常3色からランダム
            int colorIndex = Random.Range(0, 3);
            switch (colorIndex)
            {
                case 0:
                    selectedSprite = selectedType.normalSprite1;
                    break;
                case 1:
                    selectedSprite = selectedType.normalSprite2;
                    break;
                case 2:
                    selectedSprite = selectedType.normalSprite3;
                    break;
                default:
                    selectedSprite = selectedType.normalSprite1;
                    break;
            }
            
            if (selectedSprite == null)
            {
                Debug.LogWarning($"[InsectSpawner] {selectedType.typeName} のスプライトが設定されていません");
                return;
            }
        }
        
        // 固定位置にスポーン
        Vector3 spawnPos = selectedType.spawnPosition;
        
        // 固定回転角度でプレハブからインスタンス化
        Quaternion spawnRot = Quaternion.Euler(0, 0, selectedType.spawnRotation);
        GameObject insectObj = Instantiate(insectPrefab, spawnPos, spawnRot);
        insectObj.transform.SetParent(transform);
        insectObj.name = $"{selectedType.typeName}_{(isRare ? "Rare" : "Normal")}";
        
        // 固定スケールを適用
        insectObj.transform.localScale = Vector3.one * selectedType.spawnScale;
        
        // InsectControllerを初期化
        InsectController controller = insectObj.GetComponent<InsectController>();
        if (controller != null)
        {
            controller.Initialize(selectedSprite, isRare, spawnPos, selectedType.flipX);
            
            // ポジションインデックスを記録(削除時に解放するため)
            controller.SetPositionIndex(selectedIndex);
            
            // 昆虫が削除された時のコールバックを設定
            controller.OnDestroyed += () => OnInsectDestroyed(selectedIndex);
        }
        else
        {
            Debug.LogError("[InsectSpawner] InsectControllerが見つかりません");
        }
        
        activeInsects.Add(insectObj);
        
        Debug.Log($"[InsectSpawner] 昆虫スポーン: 種類={selectedType.typeName}, レア={isRare}, 位置={spawnPos}, ポジション={selectedIndex}, アクティブ数={activeInsects.Count}");
    }
    
    /// <summary>
    /// 昆虫が削除された時にポジションを解放
    /// </summary>
    private void OnInsectDestroyed(int positionIndex)
    {
        if (occupiedPositions.Contains(positionIndex))
        {
            occupiedPositions.Remove(positionIndex);
            Debug.Log($"[InsectSpawner] ポジション{positionIndex}を解放しました。使用中ポジション数: {occupiedPositions.Count}");
        }
    }
    
    /// <summary>
    /// エディタ拡張からプレハブを設定
    /// </summary>
    public void SetInsectPrefab(GameObject prefab)
    {
        insectPrefab = prefab;
    }
    
    /// <summary>
    /// エディタ拡張から昆虫種類を設定
    /// </summary>
    public void SetInsectTypes(InsectType[] types)
    {
        insectTypes = types;
        Debug.Log($"[InsectSpawner] 昆虫種類設定: {types.Length}種類");
    }
}
