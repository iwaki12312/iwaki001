using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// フルーツの出現を管理するクラス
/// 重み付きランダム選択とレア判定を行う
/// </summary>
public class FruitSpawnManager : MonoBehaviour
{
    public static FruitSpawnManager Instance { get; private set; }

    [Header("フルーツデータ")]
    [SerializeField] private List<FruitSliceData> normalFruits = new List<FruitSliceData>();
    [SerializeField] private List<FruitSliceData> rareFruits = new List<FruitSliceData>();

    [Header("レア出現確率")]
    [SerializeField, Range(0f, 1f)] private float rareChance = 0.1f;

    private float totalNormalWeight;

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

    /// <summary>
    /// フルーツデータを設定
    /// </summary>
    public void SetFruitData(List<FruitSliceData> normals, List<FruitSliceData> rares)
    {
        normalFruits = normals;
        rareFruits = rares;
        CalculateWeights();
    }

    /// <summary>
    /// レア出現確率を設定
    /// </summary>
    public void SetRareChance(float chance)
    {
        rareChance = Mathf.Clamp01(chance);
    }

    /// <summary>
    /// 重み合計を計算
    /// </summary>
    private void CalculateWeights()
    {
        totalNormalWeight = 0f;
        foreach (var fruit in normalFruits)
        {
            if (fruit != null)
            {
                totalNormalWeight += fruit.spawnWeight;
            }
        }
    }

    /// <summary>
    /// ランダムなフルーツを取得（レア判定込み）
    /// </summary>
    public FruitSliceData GetRandomFruit()
    {
        // レア判定
        if (rareFruits.Count > 0 && Random.value < rareChance)
        {
            return rareFruits[Random.Range(0, rareFruits.Count)];
        }

        // 通常フルーツ（重み付きランダム）
        if (normalFruits.Count == 0) return null;

        float roll = Random.Range(0f, totalNormalWeight);
        float runningWeight = 0f;

        foreach (var fruit in normalFruits)
        {
            if (fruit == null) continue;
            runningWeight += fruit.spawnWeight;
            if (roll <= runningWeight)
            {
                return fruit;
            }
        }

        // フォールバック
        return normalFruits[normalFruits.Count - 1];
    }
}
