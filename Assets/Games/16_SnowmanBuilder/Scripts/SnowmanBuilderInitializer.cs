using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

/// <summary>
/// SnowmanBuilderゲームの自動初期化を行うクラス
/// </summary>
public class SnowmanBuilderInitializer : MonoBehaviour
{
    [Header("=== 雪玉スプライト ===")]
    [SerializeField] private Sprite snowballSprite;

    [Header("=== 完成雪だるまスプライト（通常バリエーション）===")]
    [SerializeField] private Sprite snowmanComplete1;  // 赤マフラー＋黒帽子
    [SerializeField] private Sprite snowmanComplete2;  // 青マフラー＋シルクハット
    [SerializeField] private Sprite snowmanComplete3;  // 緑マフラー＋バケツ
    [SerializeField] private Sprite snowmanComplete4;  // 黄マフラー＋ニット帽
    [SerializeField] private Sprite snowmanComplete5;  // ピンクマフラー＋リボン

    [Header("=== レア雪だるまスプライト ===")]
    [SerializeField] private Sprite snowmanRare1;  // うさぎ耳
    [SerializeField] private Sprite snowmanRare2;  // 猫耳
    [SerializeField] private Sprite snowmanRare3;  // くま耳

    [Header("=== 背景スプライト ===")]
    [SerializeField] private Sprite backgroundSprite;

    [Header("=== 効果音 ===")]
    [SerializeField] private AudioClip snowballAppearSound;
    [SerializeField] private AudioClip snowballStackSound;
    [SerializeField] private AudioClip completeSound;
    [SerializeField] private AudioClip rareCompleteSound;
    [SerializeField] private AudioClip fadeOutSound;

    [Header("=== ゲーム設定 ===")]
    [SerializeField, Range(0f, 1f)] private float rareChance = 0.1f;
    [SerializeField] private int maxSnowmen = 5;
    [SerializeField] private float groundMinY = -4f;
    [SerializeField] private float groundMaxY = -1f;

    void Awake()
    {
        InitializeGame();
    }

    private void InitializeGame()
    {
        Debug.Log("[SnowmanBuilderInitializer] ゲームの初期化を開始...");

        CreateEventSystemIfNeeded();
        CreateSFXPlayer();
        FindAndSetupBackground();
        CreateController();

        Debug.Log("[SnowmanBuilderInitializer] ゲームの初期化が完了しました！");
    }

    private void CreateEventSystemIfNeeded()
    {
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<EventSystem>();
            esObj.AddComponent<InputSystemUIInputModule>();
            Debug.Log("[SnowmanBuilderInitializer] EventSystemを作成しました");
        }
    }

    private void CreateSFXPlayer()
    {
        GameObject sfxObj = new GameObject("SnowmanBuilderSFXPlayer");
        var sfxPlayer = sfxObj.AddComponent<SnowmanBuilderSFXPlayer>();
        sfxPlayer.SetSoundClips(
            snowballAppearSound,
            snowballStackSound,
            completeSound,
            rareCompleteSound,
            fadeOutSound
        );
        Debug.Log("[SnowmanBuilderInitializer] SFXPlayerを作成しました");
    }

    private void FindAndSetupBackground()
    {
        // 既にScene内に「Background」が配置されていればそれを使う
        GameObject bgObj = GameObject.Find("Background");
        if (bgObj != null)
        {
            SpriteRenderer sr = bgObj.GetComponent<SpriteRenderer>();
            if (sr != null && backgroundSprite != null)
            {
                sr.sprite = backgroundSprite;
            }
            Debug.Log("[SnowmanBuilderInitializer] 既存の背景を設定しました");
            return;
        }

        // なければ新規作成
        bgObj = new GameObject("Background");
        bgObj.transform.position = Vector3.zero;
        bgObj.transform.localScale = new Vector3(10, 10, 1);
        SpriteRenderer bgRenderer = bgObj.AddComponent<SpriteRenderer>();
        bgRenderer.sortingOrder = -100;
        if (backgroundSprite != null)
        {
            bgRenderer.sprite = backgroundSprite;
        }
        else
        {
            // 背景スプライトがない場合は色で代用
            bgRenderer.color = new Color(0.85f, 0.92f, 1f); // 雪空色
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            bgRenderer.sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);
        }
        Debug.Log("[SnowmanBuilderInitializer] 背景を作成しました");
    }

    private void CreateController()
    {
        GameObject ctrlObj = new GameObject("SnowmanBuilderController");
        SnowmanBuilderController controller = ctrlObj.AddComponent<SnowmanBuilderController>();

        // 通常完成スプライトの配列
        Sprite[] decoSets = BuildSpriteArray(
            snowmanComplete1, snowmanComplete2, snowmanComplete3,
            snowmanComplete4, snowmanComplete5
        );

        // レアスプライトの配列
        Sprite[] rareSets = BuildSpriteArray(
            snowmanRare1, snowmanRare2, snowmanRare3
        );

        controller.SetSprites(snowballSprite, decoSets, rareSets);

        // SerializedFieldを直接触れないため、リフレクションで設定
        SetFieldValue(controller, "rareChance", rareChance);
        SetFieldValue(controller, "maxSnowmen", maxSnowmen);
        SetFieldValue(controller, "groundMinY", groundMinY);
        SetFieldValue(controller, "groundMaxY", groundMaxY);

        Debug.Log("[SnowmanBuilderInitializer] Controllerを作成しました");
    }

    /// <summary>
    /// nullでないスプライトだけを配列にまとめる
    /// </summary>
    private Sprite[] BuildSpriteArray(params Sprite[] sprites)
    {
        var list = new System.Collections.Generic.List<Sprite>();
        foreach (var s in sprites)
        {
            if (s != null) list.Add(s);
        }
        // すべてnullの場合は空配列を返す（実行時にプレースホルダー表示で代用）
        return list.ToArray();
    }

    /// <summary>
    /// リフレクションでprivateフィールドを設定
    /// </summary>
    private void SetFieldValue(object obj, string fieldName, object value)
    {
        var field = obj.GetType().GetField(fieldName,
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Public);
        if (field != null)
        {
            field.SetValue(obj, value);
        }
    }
}
