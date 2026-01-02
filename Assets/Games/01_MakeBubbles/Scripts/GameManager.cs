using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;

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
        if (IsKeyPressedThisFrame(backToMenuKey))
        {
            BackToMenu();
        }
    }

    private static bool IsKeyPressedThisFrame(KeyCode keyCode)
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return false;

        switch (keyCode)
        {
            case KeyCode.Return:
                return keyboard.enterKey.wasPressedThisFrame;
            case KeyCode.KeypadEnter:
                return keyboard.numpadEnterKey.wasPressedThisFrame;
        }

        if (Enum.TryParse<Key>(keyCode.ToString(), out var key))
        {
            var keyControl = keyboard[key];
            return keyControl != null && keyControl.wasPressedThisFrame;
        }

        return false;
    }

    // メニューシーンに戻る
    public void BackToMenu()
    {
        Debug.Log("メニューに戻ります");
        SceneManager.LoadScene(menuSceneName);
    }
}
