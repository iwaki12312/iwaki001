using UnityEngine;

/// <summary>
/// PopBalloonsゲーム全体を管理するクラス
/// </summary>
public class PopBalloonsGameManager : MonoBehaviour
{
    public static PopBalloonsGameManager Instance { get; private set; }
    
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
        Debug.Log("[PopBalloonsGameManager] ゲーム開始");
    }
}
