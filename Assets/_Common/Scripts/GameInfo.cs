using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameInfo : MonoBehaviour
{

    // すべてのゲーム情報を格納するリスト
    public static List<GameData> allGames = new List<GameData>
    {
        new GameData("BubbleGame", 1, 1),       // 01_MakeBubbles
        new GameData("MoleGame", 2, 1),         // 02_MoleGame
        new GameData("FlowerBlooming", 3, 1),   // 03_FlowerBlooming
        new GameData("Cook", 4, 1),              // 04_Cook
        new GameData("TouchTheStar", 5, 1)      // 05_TouchTheStar
    };

    // 各ゲームの情報を格納するクラス
    public class GameData
    {
        // シーン名
        public string sceneName;
        // ゲーム番号
        public int gameID;
        // パック番号
        public int packID;

        // コンストラクタ
        public GameData(string sceneName, int gameID, int packID)
        {
            this.sceneName = sceneName;
            this.gameID = gameID;
            this.packID = packID;
        }
    }

}
