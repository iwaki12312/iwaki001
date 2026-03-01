using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

/// <summary>
/// FruitSliceゲームの自動初期化クラス
/// まな板の上でフルーツを切って盛り付けるゲーム
/// </summary>
public class FruitSliceInitializer : MonoBehaviour
{
    [Header("=== フルーツスプライト - 通常フルーツ（3状態） ===")]
    [SerializeField] private Sprite appleWhole;
    [SerializeField] private Sprite appleCut;
    [SerializeField] private Sprite applePlated;

    [SerializeField] private Sprite orangeWhole;
    [SerializeField] private Sprite orangeCut;
    [SerializeField] private Sprite orangePlated;

    [SerializeField] private Sprite peachWhole;
    [SerializeField] private Sprite peachCut;
    [SerializeField] private Sprite peachPlated;

    [SerializeField] private Sprite pineappleWhole;
    [SerializeField] private Sprite pineappleCut;
    [SerializeField] private Sprite pineapplePlated;

    [SerializeField] private Sprite watermelonWhole;
    [SerializeField] private Sprite watermelonCut;
    [SerializeField] private Sprite watermelonPlated;

    [SerializeField] private Sprite pearWhole;
    [SerializeField] private Sprite pearCut;
    [SerializeField] private Sprite pearPlated;

    [SerializeField] private Sprite kiwiWhole;
    [SerializeField] private Sprite kiwiCut;
    [SerializeField] private Sprite kiwiPlated;

    [SerializeField] private Sprite lemonWhole;
    [SerializeField] private Sprite lemonCut;
    [SerializeField] private Sprite lemonPlated;

    [Header("=== フルーツスプライト - レアフルーツ（3状態） ===")]
    [SerializeField] private Sprite goldenAppleWhole;
    [SerializeField] private Sprite goldenAppleCut;
    [SerializeField] private Sprite goldenApplePlated;

    [SerializeField] private Sprite rainbowMangoWhole;
    [SerializeField] private Sprite rainbowMangoCut;
    [SerializeField] private Sprite rainbowMangoPlated;

    [SerializeField] private Sprite diamondOrangeWhole;
    [SerializeField] private Sprite diamondOrangeCut;
    [SerializeField] private Sprite diamondOrangePlated;

    [Header("=== 背景 ===")]
    [SerializeField] private Sprite cuttingBoardSprite;

    [Header("=== 効果音 ===")]
    [SerializeField] private AudioClip cutSound;
    [SerializeField] private AudioClip plateSound;
    [SerializeField] private AudioClip completeSound;
    [SerializeField] private AudioClip rareSound;
    [SerializeField] private AudioClip spawnSound;

    [Header("=== スロット設定 ===")]
    [SerializeField, Range(0f, 1f)] private float rareSpawnChance = 0.1f;
    [SerializeField, Range(0.5f, 3f)] private float defaultColliderRadius = 1.0f;
    [SerializeField, Range(0.5f, 3f)] private float defaultFruitScale = 1.2f;

    [Header("=== スポーンポイント（シーン上に配置。Scene Viewでドラッグして位置調整可能） ===")]
    [SerializeField] private List<FruitSpawnPoint> spawnPoints = new List<FruitSpawnPoint>();

    void Awake()
    {
        InitializeGame();
    }

    private void InitializeGame()
    {
        Debug.Log("[FruitSliceInitializer] ゲームの初期化を開始...");

        CreateEventSystemIfNeeded();
        CreateSFXPlayer();
        SetupBackground();
        CreateSpawnManager();
        CreateFruitSlots();

        Debug.Log("[FruitSliceInitializer] ゲームの初期化が完了しました！");
    }

    /// <summary>
    /// EventSystemの作成
    /// </summary>
    private void CreateEventSystemIfNeeded()
    {
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<InputSystemUIInputModule>();
            Debug.Log("[FruitSliceInitializer] EventSystemを作成しました");
        }
    }

    /// <summary>
    /// SFXPlayerの作成
    /// </summary>
    private void CreateSFXPlayer()
    {
        if (FruitSliceSFXPlayer.Instance != null) return;

        GameObject sfxObj = new GameObject("FruitSliceSFXPlayer");
        FruitSliceSFXPlayer sfxPlayer = sfxObj.AddComponent<FruitSliceSFXPlayer>();
        sfxPlayer.SetSoundClips(cutSound, plateSound, completeSound, rareSound, spawnSound);

        Debug.Log("[FruitSliceInitializer] FruitSliceSFXPlayerを作成しました");
    }

    /// <summary>
    /// 背景（まな板）のセットアップ
    /// </summary>
    private void SetupBackground()
    {
        // まな板背景がScene内に配置されているか検索
        GameObject bgObj = GameObject.Find("Background_CuttingBoard");
        if (bgObj == null)
        {
            bgObj = new GameObject("Background_CuttingBoard");
            SpriteRenderer sr = bgObj.AddComponent<SpriteRenderer>();
            sr.sortingOrder = -100;
            sr.sprite = cuttingBoardSprite;
            bgObj.transform.localScale = new Vector3(10f, 10f, 1f);
        }
        else
        {
            SpriteRenderer sr = bgObj.GetComponent<SpriteRenderer>();
            if (sr != null && cuttingBoardSprite != null)
            {
                sr.sprite = cuttingBoardSprite;
            }
        }

        Debug.Log("[FruitSliceInitializer] 背景を設定しました");
    }

    /// <summary>
    /// FruitSpawnManagerを作成
    /// </summary>
    private void CreateSpawnManager()
    {
        if (FruitSpawnManager.Instance != null) return;

        GameObject managerObj = new GameObject("FruitSpawnManager");
        FruitSpawnManager manager = managerObj.AddComponent<FruitSpawnManager>();

        // フルーツデータを作成
        List<FruitSliceData> normals = CreateNormalFruitData();
        List<FruitSliceData> rares = CreateRareFruitData();

        manager.SetFruitData(normals, rares);
        manager.SetRareChance(rareSpawnChance);

        Debug.Log("[FruitSliceInitializer] FruitSpawnManagerを作成しました");
    }

    /// <summary>
    /// スポーンポイントからフルーツスロットを作成
    /// </summary>
    private void CreateFruitSlots()
    {
        FruitSpawnManager manager = FruitSpawnManager.Instance;
        if (manager == null)
        {
            Debug.LogError("[FruitSliceInitializer] FruitSpawnManagerが見つかりません");
            return;
        }

        // SpawnPointが未設定の場合、シーン内を自動検索
        if (spawnPoints == null || spawnPoints.Count == 0)
        {
            var found = new List<FruitSpawnPoint>(FindObjectsByType<FruitSpawnPoint>(FindObjectsSortMode.None));
            if (found.Count > 0)
            {
                spawnPoints = found;
            }
        }

        if (spawnPoints == null || spawnPoints.Count == 0)
        {
            Debug.LogError("[FruitSliceInitializer] スポーンポイントが見つかりません。Tools → Setup FruitSlice Gameを実行してください");
            return;
        }

        for (int i = 0; i < spawnPoints.Count; i++)
        {
            FruitSpawnPoint sp = spawnPoints[i];
            if (sp == null) continue;

            // Play mode中はプレビューを隠す
            sp.HidePreview();

            // スケールとコライダー半径の決定（オーバーライド優先）
            float scale = sp.OverrideScale > 0f ? sp.OverrideScale : defaultFruitScale;
            float colRadius = sp.OverrideColliderRadius > 0f ? sp.OverrideColliderRadius : defaultColliderRadius;

            // スロットGameObjectを生成（SpawnPointの子にはせず独立させる）
            GameObject slotObj = new GameObject($"FruitSlot_{i:D2}");
            slotObj.transform.position = sp.transform.position;
            slotObj.transform.localScale = Vector3.one * scale;

            SpriteRenderer sr = slotObj.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 10;

            CircleCollider2D col = slotObj.AddComponent<CircleCollider2D>();
            col.radius = colRadius;

            FruitSlotController slot = slotObj.AddComponent<FruitSlotController>();
            slot.Initialize(manager, colRadius);

            // 初期フルーツをスポーン
            FruitSliceData initialFruit = manager.GetRandomFruit();
            slot.SpawnFruit(initialFruit);
        }

        Debug.Log($"[FruitSliceInitializer] {spawnPoints.Count}個のフルーツスロットを作成しました");
    }

    // ── フルーツデータ生成 ──

    private List<FruitSliceData> CreateNormalFruitData()
    {
        List<FruitSliceData> list = new List<FruitSliceData>();

        list.Add(CreateFruitDataInstance(FruitType.Apple, "リンゴ", appleWhole, appleCut, applePlated, false));
        list.Add(CreateFruitDataInstance(FruitType.Orange, "オレンジ", orangeWhole, orangeCut, orangePlated, false));
        list.Add(CreateFruitDataInstance(FruitType.Peach, "桃", peachWhole, peachCut, peachPlated, false));
        list.Add(CreateFruitDataInstance(FruitType.Pineapple, "パイナップル", pineappleWhole, pineappleCut, pineapplePlated, false));
        list.Add(CreateFruitDataInstance(FruitType.Watermelon, "スイカ", watermelonWhole, watermelonCut, watermelonPlated, false));
        list.Add(CreateFruitDataInstance(FruitType.Pear, "洋梨", pearWhole, pearCut, pearPlated, false));
        list.Add(CreateFruitDataInstance(FruitType.Kiwi, "キウイ", kiwiWhole, kiwiCut, kiwiPlated, false));
        list.Add(CreateFruitDataInstance(FruitType.Lemon, "レモン", lemonWhole, lemonCut, lemonPlated, false));

        return list;
    }

    private List<FruitSliceData> CreateRareFruitData()
    {
        List<FruitSliceData> list = new List<FruitSliceData>();

        list.Add(CreateFruitDataInstance(FruitType.GoldenApple, "金のリンゴ", goldenAppleWhole, goldenAppleCut, goldenApplePlated, true));
        list.Add(CreateFruitDataInstance(FruitType.RainbowMango, "レインボーマンゴー", rainbowMangoWhole, rainbowMangoCut, rainbowMangoPlated, true));
        list.Add(CreateFruitDataInstance(FruitType.DiamondOrange, "ダイヤモンドオレンジ", diamondOrangeWhole, diamondOrangeCut, diamondOrangePlated, true));

        return list;
    }

    private FruitSliceData CreateFruitDataInstance(FruitType type, string name, Sprite whole, Sprite cut, Sprite plated, bool isRare)
    {
        FruitSliceData data = ScriptableObject.CreateInstance<FruitSliceData>();
        data.fruitType = type;
        data.fruitName = name;
        data.wholeSprite = whole;
        data.cutSprite = cut;
        data.platedSprite = plated;
        data.isRare = isRare;
        data.spawnWeight = isRare ? 0.5f : 1f;

        return data;
    }
}
