using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;


public class MenuManager : MonoBehaviour
{

    // 各ミニゲームへの遷移

    // 01_MakeBubbles
    public void TransitionToMakeBubble()
    {
        SceneManager.LoadScene("BubbleGame");
    }

    // 02_MoleGame
    public void TransitionToMoleGame()
    {
        SceneManager.LoadScene("MoleGame");
    }
}
