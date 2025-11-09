using UnityEngine;
// using DG.Tweening;

/// <summary>
/// Fishingゲーム全体を管理するクラス
/// </summary>
public class FishingGameManager : MonoBehaviour
{
    public static FishingGameManager Instance { get; private set; }
    
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
        
        // // DOTweenを初期化
        // DOTween.Init();
        // DOTween.SetTweensCapacity(200, 50);
    }
    
    void Start()
    {
        Debug.Log("[FishingGameManager] ゲーム開始");
        
        // 他のコンポーネントが存在するか確認
        FishSpawner spawner = FindObjectOfType<FishSpawner>();
        FishingSFXPlayer sfx = FindObjectOfType<FishingSFXPlayer>();
        FishermanController fisherman = FindObjectOfType<FishermanController>();
        
        Debug.Log($"[FishingGameManager] コンポーネント確認 - Spawner={spawner != null}, SFX={sfx != null}, Fisherman={fisherman != null}");
    }
}
