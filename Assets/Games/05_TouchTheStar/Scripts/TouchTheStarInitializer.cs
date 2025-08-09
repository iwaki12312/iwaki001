using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// TouchTheStarゲームの自動初期化を行うクラス
/// </summary>
public class TouchTheStarInitializer : MonoBehaviour
{
    [Header("効果音ファイル（必須）")]
    [SerializeField] private AudioClip starAppearSound;
    [SerializeField] private AudioClip starDisappearSound;
    
    [Header("星のスプライト（必須）- 9つの表情")]
    [SerializeField] private Sprite[] starSprites = new Sprite[9];
    
    void Awake()
    {
        InitializeGame();
    }
    
    /// <summary>
    /// ゲームの初期化
    /// </summary>
    private void InitializeGame()
    {
        Debug.Log("TouchTheStarゲームの初期化を開始します...");
        
        // 必須アセットの確認
        if (!ValidateRequiredAssets())
        {
            Debug.LogError("必須アセットが設定されていません。TouchTheStarInitializerのInspectorで効果音とスプライトを設定してください。");
            return;
        }
        
        // EventSystemの確認・作成
        CreateEventSystemIfNeeded();
        
        // TouchTheStarSFXPlayerの作成
        CreateSFXPlayer();
        
        // StarManagerの作成
        CreateStarManager();
        
        Debug.Log("TouchTheStarゲームの初期化が完了しました！");
    }
    
    /// <summary>
    /// 必須アセットが設定されているかを確認
    /// </summary>
    private bool ValidateRequiredAssets()
    {
        bool isValid = true;
        
        if (starAppearSound == null)
        {
            Debug.LogError("Star Appear Soundが設定されていません。");
            isValid = false;
        }
        
        if (starDisappearSound == null)
        {
            Debug.LogError("Star Disappear Soundが設定されていません。");
            isValid = false;
        }
        
        if (starSprites == null || starSprites.Length == 0)
        {
            Debug.LogError("Star Spritesが設定されていません。9つの星スプライトを設定してください。");
            isValid = false;
        }
        else
        {
            for (int i = 0; i < starSprites.Length; i++)
            {
                if (starSprites[i] == null)
                {
                    Debug.LogWarning($"Star Sprite[{i}]が設定されていません。");
                }
            }
        }
        
        return isValid;
    }
    
    /// <summary>
    /// EventSystemが存在しない場合に作成
    /// </summary>
    private void CreateEventSystemIfNeeded()
    {
        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<StandaloneInputModule>();
            Debug.Log("EventSystemを作成しました。");
        }
        else
        {
            Debug.Log("EventSystemは既に存在します。");
        }
    }
    
    /// <summary>
    /// TouchTheStarSFXPlayerオブジェクトを作成
    /// </summary>
    private void CreateSFXPlayer()
    {
        // 既に存在するかチェック
        if (FindObjectOfType<TouchTheStarSFXPlayer>() != null)
        {
            Debug.Log("TouchTheStarSFXPlayerは既に存在します。");
            return;
        }
        
        // SFXPlayerオブジェクトを作成
        GameObject sfxPlayerObj = new GameObject("TouchTheStarSFXPlayer");
        TouchTheStarSFXPlayer sfxPlayer = sfxPlayerObj.AddComponent<TouchTheStarSFXPlayer>();
        
        // 効果音ファイルを設定
        sfxPlayer.SetAudioClips(starAppearSound, starDisappearSound);
        Debug.Log("TouchTheStarSFXPlayerを作成し、効果音を設定しました。");
    }
    
    /// <summary>
    /// StarManagerオブジェクトを作成
    /// </summary>
    private void CreateStarManager()
    {
        // 既に存在するかチェック
        if (FindObjectOfType<StarManager>() != null)
        {
            Debug.Log("StarManagerは既に存在します。");
            return;
        }
        
        // StarManagerオブジェクトを作成
        GameObject starManagerObj = new GameObject("StarManager");
        StarManager starManager = starManagerObj.AddComponent<StarManager>();
        
        // 星のスプライト配列を設定
        starManager.SetStarSprites(starSprites);
        Debug.Log("StarManagerを作成し、星のスプライト配列を設定しました。");
    }
    
    /// <summary>
    /// 効果音ファイルを手動で設定（Inspector用）
    /// </summary>
    public void SetAudioClips(AudioClip appearSound, AudioClip disappearSound)
    {
        starAppearSound = appearSound;
        starDisappearSound = disappearSound;
    }
    
    /// <summary>
    /// 星のスプライト配列を手動で設定（Inspector用）
    /// </summary>
    public void SetStarSprites(Sprite[] sprites)
    {
        starSprites = sprites;
    }
}
