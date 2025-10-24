using UnityEngine;

/// <summary>
/// 動物が出現する風船の制御
/// BalloonControllerに追加コンポーネントとして付与
/// </summary>
public class AnimalBalloonController : MonoBehaviour
{
    [SerializeField] private Sprite[] animalSprites;       // 動物スプライト(5種類)
    [SerializeField] private Sprite parachuteSprite;       // パラシュートスプライト
    [SerializeField] private GameObject animalParachutePrefab; // AnimalParachutePrefab
    private GameObject starParticlePrefab;
    
    private BalloonController balloonController;
    private bool hasTriggered = false;
    
    void Awake()
    {
        balloonController = GetComponent<BalloonController>();
    }
    
    /// <summary>
    /// パーティクルPrefabを設定
    /// </summary>
    public void SetParticlePrefab(GameObject particlePrefab)
    {
        starParticlePrefab = particlePrefab;
    }
    
    /// <summary>
    /// 動物パラシュートを出現させる
    /// </summary>
    public void SpawnAnimalParachute(Vector3 position)
    {
        if (hasTriggered) return;
        hasTriggered = true;
        
        // ランダムな動物を選択
        AnimalType animalType = (AnimalType)Random.Range(0, 5);
        
        // AnimalParachutePrefabを生成
        if (animalParachutePrefab != null)
        {
            GameObject parachuteObj = Instantiate(animalParachutePrefab, position, Quaternion.identity);
            AnimalParachuteController parachuteController = parachuteObj.GetComponent<AnimalParachuteController>();
            
            if (parachuteController != null)
            {
                // 動物スプライトとパラシュートスプライトを取得
                Sprite animalSprite = animalSprites != null && animalSprites.Length > (int)animalType
                    ? animalSprites[(int)animalType]
                    : null;
                
                parachuteController.Initialize(animalType, animalSprite, parachuteSprite);
            }
        }
        
        // 動物出現音を再生
        if (PopBalloonsSFXPlayer.Instance != null)
        {
            PopBalloonsSFXPlayer.Instance.PlayAnimalAppear();
        }
    }
    
    void OnDestroy()
    {
        // 風船が削除される時に動物パラシュートを出現
        if (!hasTriggered && balloonController != null)
        {
            SpawnAnimalParachute(transform.position);
        }
    }
}
