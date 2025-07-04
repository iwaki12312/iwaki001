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

    private void Awake()
    {
        // ボタンの参照を取得（Inspector上で設定されていない場合）
        if (bubbleGameButton == null)
        {
            bubbleGameButton = GameObject.Find("BubbleGameButton")?.GetComponent<Button>();
            Debug.Log("BubbleGameButtonを検索: " + (bubbleGameButton != null ? "見つかりました" : "見つかりませんでした"));
        }
        
        if (simpleGameButton == null)
        {
            simpleGameButton = GameObject.Find("SimpleGameButton")?.GetComponent<Button>();
            Debug.Log("SimpleGameButtonを検索: " + (simpleGameButton != null ? "見つかりました" : "見つかりませんでした"));
        }
    }

    private void Start()
    {
        Debug.Log("MenuManager.Start()が呼ばれました");
        
        // ボタンにリスナーを追加
        if (bubbleGameButton != null)
        {
            // 既存のリスナーをクリア
            bubbleGameButton.onClick.RemoveAllListeners();
            bubbleGameButton.onClick.AddListener(() => LoadGame(bubbleGameSceneName));
            Debug.Log("バブルゲームボタンにリスナーを追加しました");
        }
        else
        {
            Debug.LogError("バブルゲームボタンが見つかりません");
        }
        
        if (simpleGameButton != null)
        {
            // 既存のリスナーをクリア
            simpleGameButton.onClick.RemoveAllListeners();
            simpleGameButton.onClick.AddListener(() => LoadGame(simpleGameSceneName));
            Debug.Log("シンプルゲームボタンにリスナーを追加しました");
        }
        else
        {
            Debug.LogError("シンプルゲームボタンが見つかりません");
        }
    }

    // ゲームシーンをロードする
    private void LoadGame(string sceneName)
    {
        Debug.Log($"ゲームをロード: {sceneName}");
        SceneManager.LoadScene(sceneName);
    }
}
