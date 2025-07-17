using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class ReturnToMainMenuManager : MonoBehaviour
{
    
    // メインメニューに戻る
    public void ReturnToMainMenu()
    {
        // シーン名を直接使用
        SceneManager.LoadScene("Menu");
    }
}
