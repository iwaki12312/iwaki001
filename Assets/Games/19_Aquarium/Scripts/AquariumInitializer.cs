using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

/// <summary>
/// Aquariumゲームの自動初期化を行うクラス
/// </summary>
public class AquariumInitializer : MonoBehaviour
{
    [Header("=== 生き物スプライト（通常）===")]
    [SerializeField] private Sprite clownfishSprite;
    [SerializeField] private Sprite angelfishSprite;
    [SerializeField] private Sprite octopusSprite;
    [SerializeField] private Sprite seaTurtleSprite;
    [SerializeField] private Sprite pufferfishSprite;
    [SerializeField] private Sprite dolphinSprite;
    [SerializeField] private Sprite seahorseSprite;
    [SerializeField] private Sprite sharkSprite;

    [Header("=== 生き物スプライト（レア）===")]
    [SerializeField] private Sprite whaleSharkSprite;
    [SerializeField] private Sprite mantaSprite;

    [Header("=== 効果音 ===")]
    [SerializeField] private AudioClip bubbleSound;
    [SerializeField] private AudioClip spawnSound;
    [SerializeField] private AudioClip tapSound1;
    [SerializeField] private AudioClip tapSound2;
    [SerializeField] private AudioClip rareSound;
    [SerializeField] private AudioClip completeSound;
    [SerializeField] private AudioClip resetSound;
    [SerializeField] private AudioClip ambientSound;

    [Header("=== エフェクト ===")]
    [SerializeField] private GameObject rareEffectPrefab;

    [Header("=== ゲーム設定 ===")]
    [SerializeField, Range(5, 20)] private int maxCreatures = 12;
    [SerializeField, Range(0f, 1f)] private float rareChance = 0.1f;
    [SerializeField, Range(0.3f, 3f)] private float creatureScale = 0.8f;
    [SerializeField, Range(0.3f, 3f)] private float rareCreatureScale = 1.2f;
    [SerializeField, Range(0.3f, 3f)] private float colliderRadius = 0.8f;

    void Awake()
    {
        InitializeGame();
    }

    private void InitializeGame()
    {
        Debug.Log("[AquariumInitializer] ゲームの初期化を開始...");

        CreateEventSystemIfNeeded();
        CreateSFXPlayer();
        CreateController();

        Debug.Log("[AquariumInitializer] ゲームの初期化が完了しました！");
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
            Debug.Log("[AquariumInitializer] EventSystemを作成しました");
        }
    }

    /// <summary>
    /// SFXPlayerを作成
    /// </summary>
    private void CreateSFXPlayer()
    {
        if (AquariumSFXPlayer.Instance != null) return;

        GameObject sfxObj = new GameObject("AquariumSFXPlayer");
        AquariumSFXPlayer sfxPlayer = sfxObj.AddComponent<AquariumSFXPlayer>();
        sfxPlayer.SetSoundClips(
            bubbleSound, spawnSound, tapSound1, tapSound2,
            rareSound, completeSound, resetSound, ambientSound
        );

        Debug.Log("[AquariumInitializer] AquariumSFXPlayerを作成しました");
    }

    /// <summary>
    /// Controllerを作成
    /// </summary>
    private void CreateController()
    {
        if (FindFirstObjectByType<AquariumController>() != null) return;

        GameObject controllerObj = new GameObject("AquariumController");
        AquariumController controller = controllerObj.AddComponent<AquariumController>();

        // フィールドをリフレクションで設定
        SetControllerFields(controller);

        Debug.Log("[AquariumInitializer] AquariumControllerを作成しました");
    }

    /// <summary>
    /// Controllerのフィールドをリフレクションで設定
    /// </summary>
    private void SetControllerFields(AquariumController controller)
    {
        var type = typeof(AquariumController);
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;

        // 通常生き物スプライト
        type.GetField("clownfishSprite", flags)?.SetValue(controller, clownfishSprite);
        type.GetField("angelfishSprite", flags)?.SetValue(controller, angelfishSprite);
        type.GetField("octopusSprite", flags)?.SetValue(controller, octopusSprite);
        type.GetField("seaTurtleSprite", flags)?.SetValue(controller, seaTurtleSprite);
        type.GetField("pufferfishSprite", flags)?.SetValue(controller, pufferfishSprite);
        type.GetField("dolphinSprite", flags)?.SetValue(controller, dolphinSprite);
        type.GetField("seahorseSprite", flags)?.SetValue(controller, seahorseSprite);
        type.GetField("sharkSprite", flags)?.SetValue(controller, sharkSprite);

        // レア生き物スプライト
        type.GetField("whaleSharkSprite", flags)?.SetValue(controller, whaleSharkSprite);
        type.GetField("mantaSprite", flags)?.SetValue(controller, mantaSprite);

        // エフェクト
        type.GetField("rareEffectPrefab", flags)?.SetValue(controller, rareEffectPrefab);

        // ゲーム設定
        type.GetField("maxCreatures", flags)?.SetValue(controller, maxCreatures);
        type.GetField("rareChance", flags)?.SetValue(controller, rareChance);
        type.GetField("creatureScale", flags)?.SetValue(controller, creatureScale);
        type.GetField("rareCreatureScale", flags)?.SetValue(controller, rareCreatureScale);
        type.GetField("colliderRadius", flags)?.SetValue(controller, colliderRadius);
    }
}
