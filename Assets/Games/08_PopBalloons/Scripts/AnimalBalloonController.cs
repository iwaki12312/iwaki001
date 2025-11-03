using UnityEngine;

/// <summary>
/// 動物が出現する風船の制御
/// BalloonControllerに追加コンポーネントとして付与
/// </summary>
public class AnimalBalloonController : MonoBehaviour
{
    [SerializeField] private Sprite[] animalSprites;       // パラシュート付き動物スプライト(3種類)
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
    /// アニマルパラシュートの設定を受け取る
    /// </summary>
    public void SetAnimalParachuteConfig(GameObject prefab, Sprite[] sprites)
    {
        animalParachutePrefab = prefab;
        animalSprites = sprites;
    }
    
    /// <summary>
    /// 動物パラシュートを出現させる
    /// </summary>
    public void SpawnAnimalParachute(Vector3 position)
    {
        if (hasTriggered) return;
        hasTriggered = true;
        
        // ランダムな動物を選択(3種類)
        AnimalType animalType = (AnimalType)Random.Range(0, 3);
        
        // AnimalParachutePrefabを生成
        if (animalParachutePrefab != null)
        {
            GameObject parachuteObj = Instantiate(animalParachutePrefab, position, Quaternion.identity);
            AnimalParachuteController parachuteController = parachuteObj.GetComponent<AnimalParachuteController>();
            
            if (parachuteController != null)
            {
                // 動物パラシュートスプライトを取得
                Sprite animalParachuteSprite = animalSprites != null && animalSprites.Length > (int)animalType
                    ? animalSprites[(int)animalType]
                    : null;
                
                parachuteController.Initialize(animalType, animalParachuteSprite);
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
        // 風船がタップされて削除される時のみ動物パラシュートを出現
        // 画面外で削除された場合は出現させない
        if (!hasTriggered && balloonController != null && !balloonController.isDestroyedOffScreen)
        {
            SpawnAnimalParachute(transform.position);
        }
    }
}
