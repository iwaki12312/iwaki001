using UnityEngine;

/// <summary>
/// 化石掘りゲーム全体を管理するクラス
/// </summary>
public class FossilDiggingManager : MonoBehaviour
{
    public static FossilDiggingManager Instance { get; private set; }

    [Header("参照")]
    [SerializeField] private RockSpawner rockSpawner;
    [SerializeField] private FossilDiggingSFXPlayer sfxPlayer;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 参照を自動取得
        if (rockSpawner == null)
        {
            rockSpawner = FindObjectOfType<RockSpawner>();
        }
        if (sfxPlayer == null)
        {
            sfxPlayer = FindObjectOfType<FossilDiggingSFXPlayer>();
        }
    }

    void Start()
    {
        Debug.Log("[FossilDiggingManager] ゲーム開始");
    }

    /// <summary>
    /// ゲームをリセット
    /// </summary>
    public void ResetGame()
    {
        // 既存の岩をすべて削除
        RockController[] rocks = FindObjectsOfType<RockController>();
        foreach (var rock in rocks)
        {
            Destroy(rock.gameObject);
        }

        // 新しい岩を生成
        if (rockSpawner != null)
        {
            rockSpawner.SpawnAllRocks();
        }

        Debug.Log("[FossilDiggingManager] ゲームリセット");
    }
}
