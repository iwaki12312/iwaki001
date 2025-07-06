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
        Debug.Log("MenuManager.Awake()が呼ばれました");
        
        // AudioManagerが存在しない場合は作成
        if (FindObjectOfType<AudioManager>() == null)
        {
            Debug.Log("MenuManager: AudioManagerが見つからないため、新しく作成します");
            GameObject audioManagerObj = new GameObject("AudioManager");
            audioManagerObj.AddComponent<AudioManager>();
            DontDestroyOnLoad(audioManagerObj);
        }
        
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
        
        // BGMを再生
        AudioManager.Instance.PlayBGM();
        Debug.Log("MenuManager: BGMの再生を開始しました");
        
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
    
    private void OnEnable()
    {
        // シーンがアクティブになったときにBGMを再生
        Invoke("PlayBGMDelayed", 0.2f);
    }
    
    private void PlayBGMDelayed()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBGM();
            Debug.Log("MenuManager: BGMの再生を開始しました（PlayBGMDelayed）");
        }
    }

    // ゲームシーンをロードする
    private void LoadGame(string sceneName)
    {
        Debug.Log($"ゲームをロード: {sceneName}");
        SceneManager.LoadScene(sceneName);
    }
}
