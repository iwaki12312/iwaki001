using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("ゲームボタン")]
    [SerializeField] private Button bubbleGameButton;
    [SerializeField] private Button moleGameButton; // シンプルゲームからモグラたたきゲームに変更
    
    [Header("設定")]
    [SerializeField] private string bubbleGameSceneName = "BubbleGame";
    [SerializeField] private string moleGameSceneName = "MoleGame"; // ビルド設定に追加したシーン名に戻す

    private void Awake()
    {
        Debug.LogWarning("MenuManager.Awake()が呼ばれました - バージョン2");
        
        // BGMManagerが存在しない場合は作成
        if (FindObjectOfType<BGMManager>() == null)
        {
            Debug.Log("MenuManager: BGMManagerが見つからないため、新しく作成します");
            GameObject bgmManagerObj = new GameObject("BGMManager");
            bgmManagerObj.AddComponent<BGMManager>();
            DontDestroyOnLoad(bgmManagerObj);
        }
        
        // ボタンの参照を取得（Inspector上で設定されていない場合）
        if (bubbleGameButton == null)
        {
            bubbleGameButton = GameObject.Find("BubbleGameButton")?.GetComponent<Button>();
            Debug.Log("BubbleGameButtonを検索: " + (bubbleGameButton != null ? "見つかりました" : "見つかりませんでした"));
        }
        
        if (moleGameButton == null)
        {
            // SimpleGameButtonという名前のボタンを探す（既存のボタンを再利用）
            moleGameButton = GameObject.Find("SimpleGameButton")?.GetComponent<Button>();
            Debug.Log("MoleGameButton（SimpleGameButton）を検索: " + (moleGameButton != null ? "見つかりました" : "見つかりませんでした"));
            
            // ボタンのテキストを変更
            if (moleGameButton != null)
            {
                Text buttonText = moleGameButton.GetComponentInChildren<Text>();
                if (buttonText != null)
                {
                    buttonText.text = "モグラたたき";
                }
            }
        }
    }

    private void Start()
    {
        Debug.Log("MenuManager.Start()が呼ばれました");
        
        // BGMを再生
        BGMManager.Instance.PlayBGM();
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
        
        if (moleGameButton != null)
        {
            // 既存のリスナーをクリア
            moleGameButton.onClick.RemoveAllListeners();
            moleGameButton.onClick.AddListener(() => LoadGame(moleGameSceneName));
            Debug.Log("モグラたたきゲームボタンにリスナーを追加しました");
        }
        else
        {
            Debug.LogError("モグラたたきゲームボタンが見つかりません");
        }
    }
    
    private void OnEnable()
    {
        // シーンがアクティブになったときにBGMを再生
        Invoke("PlayBGMDelayed", 0.2f);
    }
    
    private void PlayBGMDelayed()
    {
        if (BGMManager.Instance != null)
        {
            BGMManager.Instance.PlayBGM();
            Debug.Log("MenuManager: BGMの再生を開始しました（PlayBGMDelayed）");
        }
    }

    // ゲームシーンをロードする
    private void LoadGame(string sceneName)
    {
        Debug.LogWarning($"ゲームをロード試行: {sceneName}");
        
        // EventSystemの存在を確認
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            Debug.LogError("EventSystemが見つかりません。UIイベントが機能しない可能性があります。");
            
            // EventSystemを作成
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            Debug.LogWarning("EventSystemを新規作成しました");
        }
        
        try
        {
            SceneManager.LoadScene(sceneName);
            Debug.LogWarning($"シーン {sceneName} のロードを実行しました");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"シーンのロードに失敗しました: {e.Message}");
        }
    }
}
