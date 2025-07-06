using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] private string menuSceneName = "Menu";
    [SerializeField] private KeyCode backToMenuKey = KeyCode.Escape;
    
    private Button backButton;
    
private void Start()
{
    // バックボタンを探して、クリックイベントを設定
    backButton = GameObject.Find("BackButton")?.GetComponent<Button>();
    if (backButton != null)
    {
        backButton.onClick.AddListener(BackToMenu);
    }
    
    // 矢印マネージャーを生成
    CreateArrowManager();
}

// 矢印マネージャーを生成するメソッド
private void CreateArrowManager()
{
    // 既存の矢印マネージャーを検索
    if (FindObjectOfType<ArrowManager>() == null)
    {
        // プレハブを検索
        GameObject arrowManagerPrefab = Resources.Load<GameObject>("Games/BubbleGame/Prefabs/ArrowManager");
        
        if (arrowManagerPrefab == null)
        {
            // 直接生成
            GameObject arrowManagerObj = new GameObject("ArrowManager");
            arrowManagerObj.AddComponent<ArrowManager>();
            Debug.Log("矢印マネージャーを生成しました");
        }
        else
        {
            // プレハブからインスタンス化
            Instantiate(arrowManagerPrefab);
            Debug.Log("矢印マネージャープレハブからインスタンスを生成しました");
        }
    }
}
    
    private void Update()
    {
        // ESCキーでメニューに戻る
        if (Input.GetKeyDown(backToMenuKey))
        {
            BackToMenu();
        }
    }

    // メニューシーンに戻る
    public void BackToMenu()
    {
        Debug.Log("メニューに戻ります");
        SceneManager.LoadScene(menuSceneName);
    }
}
