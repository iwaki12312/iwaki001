using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameInfo : MonoBehaviour
{

    // ページネーション設定
    public static int gamesPerPage = 6;  // 1ページあたりのゲーム数
    public static int currentPage = 0;   // 現在のページ（0から開始）
    
    // すべてのゲーム情報を格納するリスト
    // ゲームを追加する場合はここに追加して一元管理する
    public static List<GameData> allGames = new List<GameData>
    {
        new GameData("MakeBubbles", 1, "pack_free"),       // 01_MakeBubbles
        new GameData("WhackAMole", 2, "pack_free"),        // 02_WhackAMole
        new GameData("FlowerBlooming", 3, "pack_free"),    // 03_FlowerBlooming
        new GameData("Cook", 4, "pack_free"),              // 04_Cook
        new GameData("TouchTheStar", 5, "pack_free"),      // 05_TouchTheStar
        new GameData("PianoAndViolin", 6, "pack_free"),    // 06_PianoAndViolin
        new GameData("CatchInsects", 7, "pack_01"),        // 07_CatchInsects
        new GameData("PopBalloons", 8, "pack_01"),         // 08_PopBalloons
        new GameData("Fishing", 9, "pack_01"),             // 09_Fishing
        new GameData("VegetableDig", 10, "pack_01"),       // 10_VegetableDig
        new GameData("EggHatch", 11, "pack_01"),           // 11_EggHatch
        new GameData("Fireworks", 12, "pack_01")          // 12_Fireworks
    };
    
    // 総ページ数を取得
    public static int GetTotalPages()
    {
        return Mathf.CeilToInt((float)allGames.Count / gamesPerPage);
    }
    
    // 指定ページのゲームリストを取得
    public static List<GameData> GetGamesForPage(int page)
    {
        int startIndex = page * gamesPerPage;
        int count = Mathf.Min(gamesPerPage, allGames.Count - startIndex);
        
        if (startIndex >= allGames.Count || count <= 0)
            return new List<GameData>();
            
        return allGames.GetRange(startIndex, count);
    }

    // 各ゲームの情報を格納するクラス
    public class GameData
    {
        // シーン名（ゲームID的な意味でも使ってる）
        public string sceneName;
        // 表示順（ゲーム番号的な意味でも使ってる）
        public int displayOrder;
        // パックID
        public string packID;

        // コンストラクタ
        public GameData(string sceneName, int displayOrder, string packID)
        {
            this.sceneName = sceneName;
            this.displayOrder = displayOrder;
            this.packID = packID;
        }
    }

}
