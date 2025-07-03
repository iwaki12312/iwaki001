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
