using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

/// <summary>
/// CakeDecorationゲームの自動初期化を行うクラス
/// </summary>
public class CakeDecorationInitializer : MonoBehaviour
{
    [Header("=== ケーキスプライト ===")]
    [SerializeField] private Sprite cakeSprite;

    [Header("=== デコレーションスプライト（通常）===")]
    [SerializeField] private Sprite decoStrawberry;
    [SerializeField] private Sprite decoBlueberry;
    [SerializeField] private Sprite decoChocolate;
    [SerializeField] private Sprite decoCream;
    [SerializeField] private Sprite decoCandy;
    [SerializeField] private Sprite decoCookie;
    [SerializeField] private Sprite decoMacaron;
    [SerializeField] private Sprite decoFruit;

    [Header("=== デコレーションスプライト（レア）===")]
    [SerializeField] private Sprite decoGoldenStar;
    [SerializeField] private Sprite decoRainbowCandy;

    [Header("=== 効果音 ===")]
    [SerializeField] private AudioClip decorateSound1;
    [SerializeField] private AudioClip decorateSound2;
    [SerializeField] private AudioClip decorateSound3;
    [SerializeField] private AudioClip rareSound;
    [SerializeField] private AudioClip celebrationSound;
    [SerializeField] private AudioClip bounceSound;
    [SerializeField] private AudioClip resetSound;
    [SerializeField] private AudioClip sparkleSound;

    [Header("=== ケーキ設定 ===")]
    [SerializeField] private Vector3 cakePosition = new Vector3(0f, -1.0f, 0f);
    [SerializeField, Range(0.5f, 5f)] private float cakeScale = 2f;

    [Header("=== ゲーム設定 ===")]
    [SerializeField, Range(5, 30)] private int maxDecorations = 15;
    [SerializeField, Range(0f, 1f)] private float rareChance = 0.1f;
    [SerializeField, Range(0.2f, 3f)] private float decorationScale = 0.5f;
    [SerializeField, Range(0.3f, 3f)] private float rareDecorationScale = 0.7f;

    [Header("=== デコレーション配置範囲 ===")]
    [SerializeField] private float cakeMinX = -2.5f;
    [SerializeField] private float cakeMaxX = 2.5f;
    [SerializeField] private float cakeMinY = -0.5f;
    [SerializeField] private float cakeMaxY = 2.2f;

    void Awake()
    {
        InitializeGame();
    }

    private void InitializeGame()
    {
        Debug.Log("[CakeDecorationInitializer] ゲームの初期化を開始...");

        CreateEventSystemIfNeeded();
        CreateSFXPlayer();
        GameObject cakeObj = CreateCake();
        CreateController(cakeObj);

        Debug.Log("[CakeDecorationInitializer] ゲームの初期化が完了しました！");
    }

    /// <summary>
    /// EventSystemを作成
    /// </summary>
    private void CreateEventSystemIfNeeded()
    {
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<InputSystemUIInputModule>();
            Debug.Log("[CakeDecorationInitializer] EventSystemを作成しました");
        }
    }

    /// <summary>
    /// SFXPlayerを作成
    /// </summary>
    private void CreateSFXPlayer()
    {
        if (CakeDecorationSFXPlayer.Instance != null) return;

        GameObject sfxObj = new GameObject("CakeDecorationSFXPlayer");
        CakeDecorationSFXPlayer sfxPlayer = sfxObj.AddComponent<CakeDecorationSFXPlayer>();
        sfxPlayer.SetSoundClips(
            decorateSound1, decorateSound2, decorateSound3,
            rareSound, celebrationSound, bounceSound,
            resetSound, sparkleSound
        );

        Debug.Log("[CakeDecorationInitializer] CakeDecorationSFXPlayerを作成しました");
    }

    /// <summary>
    /// ケーキオブジェクトを作成
    /// </summary>
    private GameObject CreateCake()
    {
        // 既存のケーキを検索
        GameObject existing = GameObject.Find("Cake");
        if (existing != null)
        {
            // スプライトを更新
            SpriteRenderer sr = existing.GetComponent<SpriteRenderer>();
            if (sr != null && cakeSprite != null)
            {
                sr.sprite = cakeSprite;
            }
            Debug.Log("[CakeDecorationInitializer] 既存のケーキオブジェクトを使用");
            return existing;
        }

        GameObject cakeObj = new GameObject("Cake");
        cakeObj.transform.position = cakePosition;
        cakeObj.transform.localScale = Vector3.one * cakeScale;

        SpriteRenderer cakeRenderer = cakeObj.AddComponent<SpriteRenderer>();
        cakeRenderer.sprite = cakeSprite;
        cakeRenderer.sortingOrder = 5;

        Debug.Log("[CakeDecorationInitializer] ケーキオブジェクトを作成しました");
        return cakeObj;
    }

    /// <summary>
    /// Controllerを作成
    /// </summary>
    private void CreateController(GameObject cakeObj)
    {
        if (FindFirstObjectByType<CakeDecorationController>() != null) return;

        GameObject controllerObj = new GameObject("CakeDecorationController");
        CakeDecorationController controller = controllerObj.AddComponent<CakeDecorationController>();

        // デコレーションスプライト配列を構築
        Sprite[] normalSprites = new Sprite[]
        {
            decoStrawberry, decoBlueberry, decoChocolate, decoCream,
            decoCandy, decoCookie, decoMacaron, decoFruit
        };

        Sprite[] rareSprites = new Sprite[]
        {
            decoGoldenStar, decoRainbowCandy
        };

        // SerializedFieldはプライベートなので、SetupメソッドでReflectionを使って注入
        SetControllerFields(controller, cakeObj, normalSprites, rareSprites);

        Debug.Log("[CakeDecorationInitializer] CakeDecorationControllerを作成しました");
    }

    /// <summary>
    /// Controllerのフィールドをリフレクションで設定
    /// </summary>
    private void SetControllerFields(CakeDecorationController controller, GameObject cakeObj,
                                      Sprite[] normalSprites, Sprite[] rareSprites)
    {
        var type = typeof(CakeDecorationController);
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;

        type.GetField("cakeTransform", flags)?.SetValue(controller, cakeObj.transform);
        type.GetField("cakeRenderer", flags)?.SetValue(controller, cakeObj.GetComponent<SpriteRenderer>());
        type.GetField("normalDecorationSprites", flags)?.SetValue(controller, normalSprites);
        type.GetField("rareDecorationSprites", flags)?.SetValue(controller, rareSprites);
        type.GetField("maxDecorations", flags)?.SetValue(controller, maxDecorations);
        type.GetField("rareChance", flags)?.SetValue(controller, rareChance);
        type.GetField("decorationScale", flags)?.SetValue(controller, decorationScale);
        type.GetField("rareDecorationScale", flags)?.SetValue(controller, rareDecorationScale);
        type.GetField("cakeMinX", flags)?.SetValue(controller, cakeMinX);
        type.GetField("cakeMaxX", flags)?.SetValue(controller, cakeMaxX);
        type.GetField("cakeMinY", flags)?.SetValue(controller, cakeMinY);
        type.GetField("cakeMaxY", flags)?.SetValue(controller, cakeMaxY);
    }
}
