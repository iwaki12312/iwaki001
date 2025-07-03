using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("ゲームボタン")]
    [SerializeField] private Button bubbleGameButton;
    [SerializeField] private Button simpleGameButton;
    
    [Header("設定")]
    [SerializeField] private string bubbleGameSceneName = "BubbleGame";
    [SerializeField] private string simpleGameSceneName = "SimpleGame";

    private void Start()
    {
        // ボタンにリスナーを追加
        if (bubbleGameButton != null)
        {
            bubbleGameButton.onClick.AddListener(() => LoadGame(bubbleGameSceneName));
        }
        
        if (simpleGameButton != null)
        {
            simpleGameButton.onClick.AddListener(() => LoadGame(simpleGameSceneName));
        }
    }

    // ゲームシーンをロードする
    private void LoadGame(string sceneName)
    {
        Debug.Log($"ゲームをロード: {sceneName}");
        SceneManager.LoadScene(sceneName);
    }
}
