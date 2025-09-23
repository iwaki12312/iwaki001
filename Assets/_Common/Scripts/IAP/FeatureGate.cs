using UnityEngine;

namespace WakuWaku.IAP
{
    /// <summary>
    /// ゲームのアクセス制御を行うクラス
    /// パック所有状況に基づいてゲームの開始可否を判定
    /// </summary>
    public static class FeatureGate
    {
        /// <summary>
        /// 指定されたゲームが開始可能かチェック
        /// </summary>
        /// <param name="gameData">ゲーム情報</param>
        /// <returns>開始可能な場合true</returns>
        public static bool CanStartGame(GameInfo.GameData gameData)
        {
            if (gameData == null)
            {
                Debug.LogError("[FeatureGate] ゲーム情報がnullです");
                return false;
            }
            
            return CanStartGame(gameData.packID);
        }
        
        /// <summary>
        /// 指定されたパックIDのゲームが開始可能かチェック
        /// </summary>
        /// <param name="packId">パックID</param>
        /// <returns>開始可能な場合true</returns>
        public static bool CanStartGame(string packId)
        {
            if (string.IsNullOrEmpty(packId))
            {
                Debug.LogError("[FeatureGate] パックIDが空です");
                return false;
            }
            
            // EntitlementStoreが初期化されていない場合は無料パックのみ許可
            if (EntitlementStore.Instance == null)
            {
                Debug.LogWarning("[FeatureGate] EntitlementStoreが初期化されていません");
                return packId == "pack_free";
            }
            
            bool canStart = EntitlementStore.Instance.HasPack(packId);
            Debug.Log($"[FeatureGate] ゲームアクセスチェック: {packId} = {canStart}");
            
            return canStart;
        }
        
        /// <summary>
        /// シーン名からゲームが開始可能かチェック
        /// </summary>
        /// <param name="sceneName">シーン名</param>
        /// <returns>開始可能な場合true</returns>
        public static bool CanStartGameBySceneName(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError("[FeatureGate] シーン名が空です");
                return false;
            }
            
            // GameInfo.allGamesからシーン名に対応するゲームを検索
            var gameData = GameInfo.allGames.Find(g => g.sceneName == sceneName);
            if (gameData == null)
            {
                Debug.LogError($"[FeatureGate] 不明なシーン名: {sceneName}");
                return false;
            }
            
            return CanStartGame(gameData);
        }
        
        /// <summary>
        /// 表示順からゲームが開始可能かチェック
        /// </summary>
        /// <param name="displayOrder">表示順</param>
        /// <returns>開始可能な場合true</returns>
        public static bool CanStartGameByDisplayOrder(int displayOrder)
        {
            // GameInfo.allGamesから表示順に対応するゲームを検索
            var gameData = GameInfo.allGames.Find(g => g.displayOrder == displayOrder);
            if (gameData == null)
            {
                Debug.LogError($"[FeatureGate] 不明な表示順: {displayOrder}");
                return false;
            }
            
            return CanStartGame(gameData);
        }
        
        /// <summary>
        /// パックが購入済みかチェック
        /// </summary>
        /// <param name="packId">パックID</param>
        /// <returns>購入済みの場合true</returns>
        public static bool IsPackOwned(string packId)
        {
            if (string.IsNullOrEmpty(packId))
                return false;
                
            if (EntitlementStore.Instance == null)
                return packId == "pack_free";
                
            return EntitlementStore.Instance.HasPack(packId);
        }
        
        /// <summary>
        /// パックがロックされているかチェック
        /// </summary>
        /// <param name="packId">パックID</param>
        /// <returns>ロックされている場合true</returns>
        public static bool IsPackLocked(string packId)
        {
            return !IsPackOwned(packId);
        }
        
        /// <summary>
        /// ゲームがロックされているかチェック
        /// </summary>
        /// <param name="gameData">ゲーム情報</param>
        /// <returns>ロックされている場合true</returns>
        public static bool IsGameLocked(GameInfo.GameData gameData)
        {
            return !CanStartGame(gameData);
        }
        
        /// <summary>
        /// 指定パックに含まれるゲーム数を取得
        /// </summary>
        /// <param name="packId">パックID</param>
        /// <returns>ゲーム数</returns>
        public static int GetGameCountInPack(string packId)
        {
            if (string.IsNullOrEmpty(packId))
                return 0;
                
            int count = 0;
            foreach (var game in GameInfo.allGames)
            {
                if (game.packID == packId)
                    count++;
            }
            
            return count;
        }
        
        /// <summary>
        /// 指定パックに含まれるゲーム一覧を取得
        /// </summary>
        /// <param name="packId">パックID</param>
        /// <returns>ゲーム一覧</returns>
        public static System.Collections.Generic.List<GameInfo.GameData> GetGamesInPack(string packId)
        {
            var gamesInPack = new System.Collections.Generic.List<GameInfo.GameData>();
            
            if (string.IsNullOrEmpty(packId))
                return gamesInPack;
                
            foreach (var game in GameInfo.allGames)
            {
                if (game.packID == packId)
                    gamesInPack.Add(game);
            }
            
            return gamesInPack;
        }
        
        /// <summary>
        /// デバッグ用：全ゲームのアクセス状況を表示
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void DebugPrintAllGameAccess()
        {
            Debug.Log("[FeatureGate] 全ゲームのアクセス状況:");
            foreach (var game in GameInfo.allGames)
            {
                bool canStart = CanStartGame(game);
                string status = canStart ? "✓ 開始可能" : "✗ ロック中";
                Debug.Log($"  {game.displayOrder:D2}_{game.sceneName} ({game.packID}): {status}");
            }
        }
    }
}
