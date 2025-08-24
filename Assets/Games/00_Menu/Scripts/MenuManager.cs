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
        SceneManager.LoadScene("MakeBubbles");
    }

    // 02_WhackAMole
    public void TransitionToWhackAMole()
    {
        SceneManager.LoadScene("WhackAMole");
    }

    // 03_FlowerBlooming
    public void TransitionToFlowerBlooming()
    {
        SceneManager.LoadScene("FlowerBlooming");
    }

    // 04_Cook
    public void TransitionToCook()
    {
        SceneManager.LoadScene("Cook");
    }

    // 05_TouchTheStar
    public void TransitionToTouchTheStar()
    {
        SceneManager.LoadScene("TouchTheStar");
    }
}
