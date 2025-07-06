using UnityEngine;

/// <summary>
/// シーン開始時にAudioManagerを初期化するクラス
/// </summary>
public class AudioManagerInitializer : MonoBehaviour
{
    [SerializeField] private GameObject audioManagerPrefab;
    
    void Awake()
    {
        // AudioManagerが存在しない場合は作成
        if (FindObjectOfType<AudioManager>() == null)
        {
            if (audioManagerPrefab != null)
            {
                // プレハブからインスタンス化
                Instantiate(audioManagerPrefab);
                Debug.Log("AudioManagerをプレハブから初期化しました");
            }
            else
            {
                // AudioManagerのインスタンスを取得（自動的に作成される）
                var instance = AudioManager.Instance;
                Debug.Log("AudioManagerをシングルトンから初期化しました");
            }
        }
    }
}
