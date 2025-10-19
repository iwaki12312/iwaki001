using System;
using System.Collections.Generic;
using UnityEngine;

namespace WakuWaku.IAP
{
    /// <summary>
    /// ユーザーの購入権利を管理するクラス
    /// PlayerPrefsを使用してローカルに保存
    /// </summary>
    public class EntitlementStore : MonoBehaviour
    {
        private const string ENTITLEMENTS_KEY = "purchased_packs";
        private const string PACK_FREE = "pack_free";
        
        private HashSet<string> ownedPacks = new HashSet<string>();
        
        public static EntitlementStore Instance { get; private set; }
        
        public event Action<string> OnPackUnlocked;
        public event Action<string> OnPackRevoked;
        
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                LoadEntitlements();
                
                // 無料パックは常に所有
                GrantPack(PACK_FREE);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        /// <summary>
        /// パックを所有しているかチェック
        /// </summary>
        public bool HasPack(string packId)
        {
            if (string.IsNullOrEmpty(packId))
                return false;
                
            return ownedPacks.Contains(packId);
        }
        
        /// <summary>
        /// パックを付与
        /// </summary>
        public void GrantPack(string packId)
        {
            if (string.IsNullOrEmpty(packId))
                return;
                
            if (!ownedPacks.Contains(packId))
            {
                ownedPacks.Add(packId);
                SaveEntitlements();
                OnPackUnlocked?.Invoke(packId);
                Debug.Log($"[EntitlementStore] パック付与: {packId}");
            }
        }
        
        /// <summary>
        /// パックを剥奪（返金時など）
        /// </summary>
        public void RevokePack(string packId)
        {
            if (string.IsNullOrEmpty(packId) || packId == PACK_FREE)
                return;
                
            if (ownedPacks.Contains(packId))
            {
                ownedPacks.Remove(packId);
                SaveEntitlements();
                OnPackRevoked?.Invoke(packId);
                Debug.Log($"[EntitlementStore] パック剥奪: {packId}");
            }
        }
        
        /// <summary>
        /// 所有パック一覧を取得
        /// </summary>
        public HashSet<string> GetOwnedPacks()
        {
            return new HashSet<string>(ownedPacks);
        }
        
        /// <summary>
        /// 権利情報をローカルから読み込み
        /// </summary>
        private void LoadEntitlements()
        {
            string json = PlayerPrefs.GetString(ENTITLEMENTS_KEY, "");
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    var data = JsonUtility.FromJson<EntitlementData>(json);
                    ownedPacks = new HashSet<string>(data.ownedPacks);
                    Debug.Log($"[EntitlementStore] 権利情報読み込み完了: {ownedPacks.Count}個のパック");
                }
                catch (Exception e)
                {
                    Debug.LogError($"[EntitlementStore] 権利情報の読み込みに失敗: {e.Message}");
                    ownedPacks = new HashSet<string>();
                }
            }
            else
            {
                ownedPacks = new HashSet<string>();
                Debug.Log("[EntitlementStore] 初回起動 - 新規権利情報を作成");
            }
        }
        
        /// <summary>
        /// 権利情報をローカルに保存
        /// </summary>
        private void SaveEntitlements()
        {
            try
            {
                var data = new EntitlementData
                {
                    ownedPacks = new List<string>(ownedPacks)
                };
                string json = JsonUtility.ToJson(data);
                PlayerPrefs.SetString(ENTITLEMENTS_KEY, json);
                PlayerPrefs.Save();
                Debug.Log($"[EntitlementStore] 権利情報保存完了: {ownedPacks.Count}個のパック");
            }
            catch (Exception e)
            {
                Debug.LogError($"[EntitlementStore] 権利情報の保存に失敗: {e.Message}");
            }
        }
        
        /// <summary>
        /// デバッグ用：全権利をクリア
        /// </summary>
        [ContextMenu("Clear All Entitlements")]
        public void ClearAllEntitlements()
        {
            ownedPacks.Clear();
            GrantPack(PACK_FREE); // 無料パックは再付与
            SaveEntitlements();
            Debug.Log("[EntitlementStore] 全権利をクリアしました");
        }
        
        /// <summary>
        /// デバッグ用：権利情報を表示
        /// </summary>
        [ContextMenu("Debug Print Entitlements")]
        public void DebugPrintEntitlements()
        {
            Debug.Log($"[EntitlementStore] 所有パック一覧:");
            foreach (var pack in ownedPacks)
            {
                Debug.Log($"  - {pack}");
            }
        }
    }
    
    [Serializable]
    public class EntitlementData
    {
        public List<string> ownedPacks = new List<string>();
    }
}
