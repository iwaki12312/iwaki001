using UnityEngine;

[CreateAssetMenu(fileName = "MoleData", menuName = "MoleGame/Mole Data")]
public class MoleData : ScriptableObject
{
    public string moleName;           // モグラの名前
    public Sprite normalSprite;       // 通常の表情
    public Sprite shockSprite;        // 叩かれた時の表情
    public bool isSpecial;            // 特別なモグラかどうか
    public int spawnWeight = 10;      // 出現確率の重み
    public AudioClip popSound;        // 出現時の音
    public Color tintColor = Color.white; // 色調整用
}