using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 野菜のスポーン位置データ
/// </summary>
[System.Serializable]
public class VegetableSpot
{
    public Vector3 position = Vector3.zero;  // 野菜の位置（Inspector調整可能）
    public float scale = 1f;                  // スケール調整
    public float colliderRadius = 1.0f;       // タップ判定の範囲
}

/// <summary>
/// 野菜の種類データ
/// </summary>
[System.Serializable]
public class VegetableType
{
    public string name = "Vegetable";         // 野菜名（にんじん、大根など）
    public Sprite leafSprite;                 // 葉っぱスプライト
    public Sprite vegetableSprite;            // 野菜本体スプライト
    public bool isRare = false;               // レアフラグ
}

/// <summary>
/// 野菜のスポーン位置と種類を管理するクラス
/// </summary>
public class VegetableSpawner : MonoBehaviour
{
    [Header("スポーン位置設定")]
    [SerializeField] private VegetableSpot[] vegetableSpots = new VegetableSpot[6];
    
    [Header("野菜種類設定")]
    [SerializeField] private VegetableType[] normalVegetables;  // 通常野菜リスト
    [SerializeField] private VegetableType[] rareVegetables;    // レア野菜リスト
    
    [Header("スポーン設定")]
    [SerializeField] private GameObject vegetablePrefab;        // 野菜プレハブ
    [SerializeField] [Range(0f, 1f)] private float rareChance = 0.15f;  // レア出現確率
    
    private List<VegetableController> activeVegetables = new List<VegetableController>();
    
    public static VegetableSpawner Instance { get; private set; }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        Debug.Log("[VegetableSpawner] スポーナー開始");
        SpawnAllVegetables();
    }
    
    /// <summary>
    /// すべてのスポット位置に野菜を生成
    /// </summary>
    private void SpawnAllVegetables()
    {
        if (vegetablePrefab == null)
        {
            Debug.LogError("[VegetableSpawner] 野菜プレハブが設定されていません");
            return;
        }
        
        for (int i = 0; i < vegetableSpots.Length; i++)
        {
            SpawnVegetableAt(vegetableSpots[i]);
        }
        
        Debug.Log($"[VegetableSpawner] {vegetableSpots.Length}個の野菜をスポーンしました");
    }
    
    /// <summary>
    /// 指定位置に野菜を生成
    /// </summary>
    private void SpawnVegetableAt(VegetableSpot spot)
    {
        // ランダムに野菜を選択
        VegetableType selectedType = SelectRandomVegetable();
        if (selectedType == null)
        {
            Debug.LogWarning("[VegetableSpawner] 野菜タイプが設定されていません");
            return;
        }
        
        // プレハブから生成
        GameObject vegObj = Instantiate(vegetablePrefab, spot.position, Quaternion.identity);
        vegObj.transform.SetParent(transform);
        
        // プレハブのスケールにspot.scaleを掛け算（プレハブのスケールを反映）
        Vector3 prefabScale = vegetablePrefab.transform.localScale;
        vegObj.transform.localScale = new Vector3(
            prefabScale.x * spot.scale,
            prefabScale.y * spot.scale,
            prefabScale.z * spot.scale
        );
        vegObj.name = $"Vegetable_{selectedType.name}";
        
        // コントローラーを初期化
        VegetableController controller = vegObj.GetComponent<VegetableController>();
        if (controller != null)
        {
            controller.Initialize(
                selectedType.leafSprite,
                selectedType.vegetableSprite,
                selectedType.isRare,
                spot.position,
                OnVegetableReadyForRespawn
            );
            controller.SetColliderRadius(spot.colliderRadius);
            activeVegetables.Add(controller);
        }
    }
    
    /// <summary>
    /// ランダムに野菜を選択（レア判定込み）
    /// </summary>
    private VegetableType SelectRandomVegetable()
    {
        bool isRare = Random.value < rareChance;
        
        VegetableType selected = null;
        
        if (isRare && rareVegetables != null && rareVegetables.Length > 0)
        {
            // 有効なレア野菜を探す
            List<VegetableType> validRare = new List<VegetableType>();
            foreach (var v in rareVegetables)
            {
                if (v != null && v.vegetableSprite != null)
                {
                    validRare.Add(v);
                }
            }
            if (validRare.Count > 0)
            {
                selected = validRare[Random.Range(0, validRare.Count)];
            }
        }
        
        // レアが選べなかった場合、通常野菜から選択
        if (selected == null && normalVegetables != null && normalVegetables.Length > 0)
        {
            // 有効な通常野菜を探す
            List<VegetableType> validNormal = new List<VegetableType>();
            foreach (var v in normalVegetables)
            {
                if (v != null && v.vegetableSprite != null)
                {
                    validNormal.Add(v);
                }
            }
            if (validNormal.Count > 0)
            {
                selected = validNormal[Random.Range(0, validNormal.Count)];
            }
        }
        
        if (selected == null)
        {
            Debug.LogError("[VegetableSpawner] 有効な野菜が見つかりません！Inspectorで野菜スプライトが設定されているか確認してください");
        }
        else
        {
            Debug.Log($"[VegetableSpawner] 野菜選択: {selected.name}, レア={selected.isRare}, 葉={selected.leafSprite?.name}, 野菜={selected.vegetableSprite?.name}");
        }
        
        return selected;
    }
    
    /// <summary>
    /// 野菜が再スポーン準備完了時のコールバック
    /// </summary>
    private void OnVegetableReadyForRespawn(VegetableController controller)
    {
        // 新しい野菜を選択
        VegetableType newType = SelectRandomVegetable();
        if (newType == null) return;
        
        // 再初期化してフェードイン
        controller.Initialize(
            newType.leafSprite,
            newType.vegetableSprite,
            newType.isRare,
            controller.transform.position,
            OnVegetableReadyForRespawn
        );
        controller.FadeIn();
        
        Debug.Log($"[VegetableSpawner] 野菜を再スポーン: {newType.name}, レア={newType.isRare}");
    }
}
