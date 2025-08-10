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
    [SerializeField] private AudioClip ufoAppearSound;
    [SerializeField] private AudioClip ufoDisappearSound;
    [SerializeField] private AudioClip bigStarAppearSound;
    [SerializeField] private AudioClip bigStarDisappearSound;
    
    [Header("星のスプライト（必須）- 9つの表情")]
    [SerializeField] private Sprite[] starSprites = new Sprite[9];
    
    [Header("巨大スターのスプライト（必須）- 1つの表情")]
    [SerializeField] private Sprite bigStarSprite;
    
    [Header("UFOのスプライト（必須）- 3つの色")]
    [SerializeField] private Sprite[] ufoSprites = new Sprite[3];
    
    [Header("パーティクルエフェクト（必須）")]
    [SerializeField] private GameObject starDisappearParticle;
    [SerializeField] private GameObject starOrbitParticle;
    [SerializeField] private GameObject bigStarDisappearParticle;
    [SerializeField] private GameObject bigStarOrbitParticle;
    [SerializeField] private GameObject ufoDisappearParticle;
    
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
        
        // UFOManagerの作成
        CreateUFOManager();
        
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
        
        if (ufoAppearSound == null)
        {
            Debug.LogError("UFO Appear Soundが設定されていません。");
            isValid = false;
        }
        
        if (ufoDisappearSound == null)
        {
            Debug.LogError("UFO Disappear Soundが設定されていません。");
            isValid = false;
        }
        
        if (bigStarAppearSound == null)
        {
            Debug.LogError("Big Star Appear Soundが設定されていません。");
            isValid = false;
        }
        
        if (bigStarDisappearSound == null)
        {
            Debug.LogError("Big Star Disappear Soundが設定されていません。");
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
        
        if (ufoSprites == null || ufoSprites.Length == 0)
        {
            Debug.LogError("UFO Spritesが設定されていません。3つのUFOスプライトを設定してください。");
            isValid = false;
        }
        else
        {
            for (int i = 0; i < ufoSprites.Length; i++)
            {
                if (ufoSprites[i] == null)
                {
                    Debug.LogWarning($"UFO Sprite[{i}]が設定されていません。");
                }
            }
        }
        
        if (starDisappearParticle == null)
        {
            Debug.LogError("Star Disappear Particleが設定されていません。");
            isValid = false;
        }
        
        if (starOrbitParticle == null)
        {
            Debug.LogError("Star Orbit Particleが設定されていません。");
            isValid = false;
        }
        
        if (bigStarSprite == null)
        {
            Debug.LogError("Big Star Spriteが設定されていません。");
            isValid = false;
        }
        
        if (bigStarDisappearParticle == null)
        {
            Debug.LogError("Big Star Disappear Particleが設定されていません。");
            isValid = false;
        }
        
        if (bigStarOrbitParticle == null)
        {
            Debug.LogError("Big Star Orbit Particleが設定されていません。");
            isValid = false;
        }
        
        if (ufoDisappearParticle == null)
        {
            Debug.LogError("UFO Disappear Particleが設定されていません。");
            isValid = false;
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
        
        // 効果音ファイルを設定（巨大スター対応版）
        sfxPlayer.SetAudioClips(starAppearSound, starDisappearSound, ufoAppearSound, ufoDisappearSound, bigStarAppearSound, bigStarDisappearSound);
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
        
        // パーティクルプレファブを設定
        starManager.SetStarDisappearParticle(starDisappearParticle);
        starManager.SetStarOrbitParticle(starOrbitParticle);
        
        // 巨大スター用のアセットを設定
        starManager.SetBigStarSprite(bigStarSprite);
        starManager.SetBigStarDisappearParticle(bigStarDisappearParticle);
        starManager.SetBigStarOrbitParticle(bigStarOrbitParticle);
        Debug.Log("StarManagerを作成し、星と巨大スターのスプライト配列とパーティクルプレファブを設定しました。");
    }
    
    /// <summary>
    /// UFOManagerオブジェクトを作成
    /// </summary>
    private void CreateUFOManager()
    {
        // 既に存在するかチェック
        if (FindObjectOfType<UFOManager>() != null)
        {
            Debug.Log("UFOManagerは既に存在します。");
            return;
        }
        
        // UFOManagerオブジェクトを作成
        GameObject ufoManagerObj = new GameObject("UFOManager");
        UFOManager ufoManager = ufoManagerObj.AddComponent<UFOManager>();
        
        // UFOのスプライト配列を設定
        ufoManager.SetUFOSprites(ufoSprites);
        
        // UFO消滅パーティクルプレファブを設定
        ufoManager.SetUFODisappearParticle(ufoDisappearParticle);
        Debug.Log("UFOManagerを作成し、UFOのスプライト配列とパーティクルプレファブを設定しました。");
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
    
    /// <summary>
    /// UFOのスプライト配列を手動で設定（Inspector用）
    /// </summary>
    public void SetUFOSprites(Sprite[] sprites)
    {
        ufoSprites = sprites;
    }
}
