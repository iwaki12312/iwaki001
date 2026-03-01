using UnityEngine;

/// <summary>
/// キノコの種類を定義するEnum
/// </summary>
public enum MushroomType
{
    // 通常キノコ
    RedMushroom,        // 赤キノコ（ベニテングタケ風）
    YellowMushroom,     // 黄キノコ
    BlueMushroom,       // 青キノコ
    WhiteMushroom,      // 白キノコ
    GreenMushroom,      // 緑キノコ

    // レアキノコ
    GoldMushroom,       // 金キノコ
    RainbowMushroom,    // 虹キノコ
    StarMushroom        // 星キノコ
}

/// <summary>
/// キノコデータを保持するScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "MushroomData", menuName = "MushroomPicking/MushroomData")]
public class MushroomPickingData : ScriptableObject
{
    [Header("基本情報")]
    public MushroomType mushroomType;
    public string mushroomName;

    [Header("スプライト")]
    public Sprite mushroomSprite;       // カラフルなキノコ画像
    public Sprite silhouetteSprite;     // シルエット画像（null の場合はランタイムで黒く表示）

    [Header("効果音")]
    public AudioClip pickSound;         // 収穫時の効果音

    [Header("レア設定")]
    public bool isRare = false;

    [Header("アニメーション設定")]
    public float jumpHeight = 1.5f;         // 跳ね上がる高さ
    public float spinSpeed = 720f;          // 回転速度（度/秒）
    public float revealDuration = 0.6f;     // シルエット解除アニメーション時間
    public float flyToBasketDuration = 0.8f; // カゴに飛んでいく時間
}
