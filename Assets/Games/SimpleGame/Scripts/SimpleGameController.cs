using UnityEngine;
using UnityEngine.UI;

public class SimpleGameController : MonoBehaviour
{
    [SerializeField] private Text scoreText;
    [SerializeField] private Button clickButton;
    
    private int score = 0;
    
    private void Start()
    {
        // スコアの初期表示
        UpdateScoreText();
        
        // ボタンにリスナーを追加
        if (clickButton != null)
        {
            clickButton.onClick.AddListener(IncrementScore);
        }
    }
    
    // スコアを増やす
    private void IncrementScore()
    {
        score++;
        UpdateScoreText();
    }
    
    // スコアテキストを更新
    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = $"スコア: {score}";
        }
    }
}
