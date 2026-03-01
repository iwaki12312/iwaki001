using UnityEngine;

/// <summary>
/// フルーツの種類を定義するEnum
/// </summary>
public enum FruitType
{
    Apple,
    Orange,
    Peach,
    Pineapple,
    Watermelon,
    Pear,
    Kiwi,
    Lemon,
    // レア
    GoldenApple,
    RainbowMango,
    DiamondOrange
}

/// <summary>
/// フルーツデータを保持するScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "FruitData", menuName = "FruitSlice/FruitData")]
public class FruitSliceData : ScriptableObject
{
    [Header("基本情報")]
    public FruitType fruitType;
    public string fruitName;

    [Header("スプライト（3状態）")]
    public Sprite wholeSprite;     // 丸ごと
    public Sprite cutSprite;       // カット後
    public Sprite platedSprite;    // お皿に盛り付け

    [Header("レア設定")]
    public bool isRare = false;

    [Header("出現確率の重み（大きいほど出やすい）")]
    public float spawnWeight = 1f;
}
