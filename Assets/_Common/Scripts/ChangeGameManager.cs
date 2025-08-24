using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
public class ChangeGameManager : MonoBehaviour
{

    // ランダムにゲームを選択してシーンを切り替える
    // 直近3回までの履歴は保持して、同じゲームが続かないようにする

    // 直近のゲーム履歴を保持するリスト
    private static List<string> recentGameHistory = new List<string>();

    // ゲームを切り替える
    public void ChangeGame()
    {
        // 利用可能なゲームのリストを取得
        List<string> availableGames = new List<string>();
        foreach (var game in GameInfo.allGames)
        {
            availableGames.Add(game.sceneName);
        }

        // 直近の履歴を考慮して新しいゲームを選択
        string newGame = GetRandomGame(availableGames);
        while (recentGameHistory.Contains(newGame))
        {
            newGame = GetRandomGame(availableGames);
        }

        // 履歴を更新
        UpdateGameHistory(newGame);

        // シーンを切り替え
        SceneManager.LoadScene(newGame);
    }

    // ランダムにゲームを選択
    private string GetRandomGame(List<string> availableGames)
    {
        int randomIndex = Random.Range(0, availableGames.Count);
        return availableGames[randomIndex];
    }

    // ゲーム履歴を更新
public static void UpdateGameHistory(string newGame)
    {
        recentGameHistory.Add(newGame);
        if (recentGameHistory.Count > 3)
        {
            recentGameHistory.RemoveAt(0);
        }
    }
}
