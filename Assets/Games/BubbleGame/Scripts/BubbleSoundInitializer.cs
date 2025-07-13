using UnityEngine;

/// <summary>
/// バブルゲーム用の効果音マネージャーを初期化するクラス
/// </summary>
public class BubbleSoundInitializer : MonoBehaviour
{
    [SerializeField] private AudioClip shotSound;    // シャボン玉発射音
    [SerializeField] private AudioClip splashSound;  // シャボン玉が割れる音
    
    void Awake()
    {
        // BubbleSoundManagerが存在しない場合は作成
        if (FindObjectOfType<BubbleSoundManager>() == null)
        {
            GameObject soundManagerObj = new GameObject("BubbleSoundManager");
            BubbleSoundManager soundManager = soundManagerObj.AddComponent<BubbleSoundManager>();
            
            // 効果音を直接設定
            if (soundManager != null)
            {
                // リフレクションを使用して非公開フィールドにアクセス
                System.Type type = soundManager.GetType();
                
                if (shotSound != null)
                {
                    System.Reflection.FieldInfo shotSoundField = type.GetField("shotSound", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
                    if (shotSoundField != null)
                    {
                        shotSoundField.SetValue(soundManager, shotSound);
                        Debug.Log("BubbleSoundManagerにshotSoundを直接設定しました");
                    }
                }
                
                if (splashSound != null)
                {
                    System.Reflection.FieldInfo splashSoundField = type.GetField("splashSound", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
                    if (splashSoundField != null)
                    {
                        splashSoundField.SetValue(soundManager, splashSound);
                        Debug.Log("BubbleSoundManagerにsplashSoundを直接設定しました");
                    }
                }
            }
            
            Debug.Log("BubbleSoundManagerを作成しました");
        }
        else
        {
            Debug.Log("BubbleSoundManagerは既に存在します");
        }
    }
}
