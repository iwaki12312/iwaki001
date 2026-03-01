using UnityEngine;

/// <summary>
/// 動物の種類を定義するEnum（AnimalVoice用）
/// </summary>
public enum AnimalVoiceAnimalType
{
    // 朝の動物
    Chicken,    // ニワトリ
    Cow,        // 牛
    Horse,      // 馬
    Pig,        // 豚
    Sheep,      // 羊
    Goat,       // ヤギ
    
    // 昼の動物
    Dog,        // 犬
    Cat,        // 猫
    Elephant,   // ゾウ
    Lion,       // ライオン
    Frog,       // カエル
    Chick,      // ひよこ
    
    // 夜の動物
    Owl,        // フクロウ
    Wolf,       // オオカミ
    Bat,        // コウモリ
    Mouse,      // ネズミ
    
    // レア動物
    Dinosaur,   // 恐竜
    Dragon,     // ドラゴン
    Unicorn,    // ユニコーン
    Monkey      // サル
}

/// <summary>
/// 時間帯を定義するEnum（AnimalVoice用）
/// </summary>
public enum AnimalVoiceTimeOfDay
{
    Morning,    // 朝
    Daytime,    // 昼
    Night       // 夜
}

/// <summary>
/// 動物データを保持するScriptableObject（AnimalVoice用）
/// </summary>
[CreateAssetMenu(fileName = "AnimalVoiceData", menuName = "AnimalVoice/AnimalVoiceData")]
public class AnimalVoiceData : ScriptableObject
{
    [Header("基本情報")]
    public AnimalVoiceAnimalType animalType;
    public string animalName;
    
    [Header("スプライト")]
    public Sprite normalSprite;      // 通常スプライト
    public Sprite reactionSprite;    // リアクション時スプライト
    
    [Header("効果音")]
    public AudioClip voiceClip;      // 鳴き声
    
    [Header("出現時間帯")]
    public AnimalVoiceTimeOfDay[] appearTimeOfDay;  // 出現する時間帯
    
    [Header("レア設定")]
    public bool isRare = false;      // レア動物かどうか
    
    [Header("アニメーション設定")]
    public float reactionDuration = 0.8f;  // リアクション時間
    public float scaleMultiplier = 1.2f;   // タップ時の拡大率
}
