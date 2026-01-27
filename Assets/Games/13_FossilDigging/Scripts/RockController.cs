using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using System.Collections.Generic;

/// <summary>
/// 岩を制御するクラス
/// タップ検出、ツルハシ演出、破片飛散、破壊演出、化石/宝石出現を管理
/// </summary>
public class RockController : MonoBehaviour
{
    [Header("基本設定")]
    [SerializeField] private int maxHits = 5;                    // 壊れるまでのタップ数
    [SerializeField] private float colliderRadius = 1.5f;        // タップ判定範囲

    [Header("スプライト参照")]
    [SerializeField] private Sprite[] rockSprites;               // 岩スプライト（6種類）
    [SerializeField] private Sprite pickaxeSprite;               // ツルハシスプライト
    [SerializeField] private Sprite[] brokenPieceSprites;        // 破片スプライト
    [SerializeField] private Sprite[] normalTreasureSprites;     // ノーマル化石/宝石
    [SerializeField] private Sprite[] rareTreasureSprites;       // レア化石/宝石
    [SerializeField] private Sprite[] superRareTreasureSprites;  // スーパーレア化石/宝石

    [Header("エフェクト参照")]
    [SerializeField] private GameObject hitEffectPrefab;         // ヒット時エフェクト
    [SerializeField] private GameObject breakEffectPrefab;       // 破壊時エフェクト

    [Header("演出設定")]
    [SerializeField] private float pickaxeSwingDuration = 0.15f; // ツルハシ振り下ろし時間
    [SerializeField] private float pickaxeOffset = 1.5f;         // ツルハシ表示オフセット
    [SerializeField] private float pieceScatterForce = 3f;       // 破片飛散力
    [SerializeField] private float treasureDisplayTime = 3f;     // 宝物表示時間
    [SerializeField] private float rockShakeStrength = 0.1f;     // 岩の揺れ強度

    [Header("ツルハシ演出設定")]
    [SerializeField] private Vector2 pickaxeStartOffset = new Vector2(1.5f, 1.5f);  // 開始位置オフセット（岩からの相対位置）
    [SerializeField] private Vector2 pickaxeEndOffset = new Vector2(0.3f, 0.3f);    // 終了位置オフセット（岩からの相対位置）
    [SerializeField] private float pickaxeStartRotation = 45f;   // 開始時の回転角度（度）
    [SerializeField] private float pickaxeEndRotation = -45f;    // 終了時の回転角度（度）
    [SerializeField] private float pickaxeScale = 1f;            // ツルハシのスケール
    [SerializeField] private Ease pickaxeEase = Ease.InQuad;     // アニメーションのイージング

    private SpriteRenderer spriteRenderer;
    private CircleCollider2D circleCollider;
    private int currentHitCount = 0;
    private bool isBreaking = false;
    private bool isTappable = true;
    private Camera mainCamera;
    private Vector3 originalPosition;
    private int spawnPointIndex = -1;

    // 現在のレア度（破壊時に決定）
    private TreasureRarity currentRarity;

    // イベント
    public System.Action<int> OnRockBroken; // 岩が壊れた時（spawnPointIndexを渡す）

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }

        circleCollider = GetComponent<CircleCollider2D>();
        if (circleCollider == null)
        {
            circleCollider = gameObject.AddComponent<CircleCollider2D>();
        }
        circleCollider.radius = colliderRadius;

        mainCamera = Camera.main;
    }

    void Start()
    {
        originalPosition = transform.position;
    }

    /// <summary>
    /// 岩を初期化
    /// </summary>
    public void Initialize(int rockSpriteIndex, int pointIndex)
    {
        // SpriteRendererを確実に取得
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }
        }

        // CircleCollider2Dを確実に取得
        if (circleCollider == null)
        {
            circleCollider = GetComponent<CircleCollider2D>();
            if (circleCollider == null)
            {
                circleCollider = gameObject.AddComponent<CircleCollider2D>();
            }
            circleCollider.radius = colliderRadius;
        }

        // カメラを取得
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        spawnPointIndex = pointIndex;
        currentHitCount = 0;
        isBreaking = false;
        isTappable = true;

        // 岩のスプライトをランダムに設定
        if (rockSprites != null && rockSprites.Length > 0)
        {
            int index = rockSpriteIndex % rockSprites.Length;
            spriteRenderer.sprite = rockSprites[index];
            spriteRenderer.sortingOrder = 10; // 背景より前面に表示
            Debug.Log($"[RockController] スプライト設定: {rockSprites[index].name}");
        }
        else
        {
            Debug.LogWarning("[RockController] 岩スプライトが設定されていません");
        }

        // 初期状態に戻す
        transform.localScale = Vector3.one;
        spriteRenderer.color = Color.white;
        originalPosition = transform.position;

        Debug.Log($"[RockController] 岩を初期化: PointIndex={pointIndex}, Position={transform.position}");
    }

    void Update()
    {
        if (!isTappable || isBreaking) return;

        // カメラがない場合は取得
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null) return;
        }

        // 実行中のスケール変更を反映
        var spawner = RockSpawner.Instance;
        if (spawner != null)
        {
            transform.localScale = Vector3.one * spawner.RockScale;
        }

        // 新しいInput Systemを使用
        HandleTouch();
    }

    /// <summary>
    /// タッチ処理（マルチタッチ対応・Input System使用）
    /// </summary>
    private void HandleTouch()
    {
        // タッチスクリーン入力
        var touchscreen = Touchscreen.current;
        if (touchscreen != null)
        {
            foreach (var touch in touchscreen.touches)
            {
                if (!touch.press.wasPressedThisFrame) continue;

                Vector2 screenPos = touch.position.ReadValue();
                if (CheckTapAtPosition(screenPos))
                {
                    return;
                }
            }
        }

        // マウス入力（エディタ用）
        var mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
        {
            Vector2 screenPos = mouse.position.ReadValue();
            CheckTapAtPosition(screenPos);
        }
    }

    /// <summary>
    /// 指定位置でのタップ判定
    /// </summary>
    private bool CheckTapAtPosition(Vector2 screenPosition)
    {
        if (mainCamera == null) return false;
        
        Vector2 worldPos = mainCamera.ScreenToWorldPoint(screenPosition);
        
        // 距離で判定
        float distance = Vector2.Distance(worldPos, transform.position);
        if (distance <= colliderRadius)
        {
            OnTap();
            return true;
        }
        return false;
    }

    /// <summary>
    /// タップ時の処理
    /// </summary>
    private void OnTap()
    {
        if (!isTappable || isBreaking) return;

        currentHitCount++;
        Debug.Log($"[RockController] タップ: {currentHitCount}/{maxHits}");

        // ツルハシ演出
        PlayPickaxeAnimation();

        // 効果音
        FossilDiggingSFXPlayer.Instance?.PlayAttack();

        // 破片を少し飛散
        SpawnBrokenPieces(2, 0.5f);

        // ヒットエフェクト
        if (hitEffectPrefab != null)
        {
            GameObject effect = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 2f);
        }

        // 岩を揺らす
        transform.DOShakePosition(0.2f, rockShakeStrength, 10, 90, false, true);

        // 壊れるかチェック
        if (currentHitCount >= maxHits)
        {
            BreakRock();
        }
    }

    /// <summary>
    /// ツルハシの振り下ろしアニメーション
    /// </summary>
    private void PlayPickaxeAnimation()
    {
        if (pickaxeSprite == null) return;

        // RockSpawnerから最新の設定を取得（実行中の変更を反映）
        var spawner = RockSpawner.Instance;
        Vector2 startOffset = spawner != null ? spawner.PickaxeStartOffset : pickaxeStartOffset;
        Vector2 endOffset = spawner != null ? spawner.PickaxeEndOffset : pickaxeEndOffset;
        float startRot = spawner != null ? spawner.PickaxeStartRotation : pickaxeStartRotation;
        float endRot = spawner != null ? spawner.PickaxeEndRotation : pickaxeEndRotation;
        float scale = spawner != null ? spawner.PickaxeScale : pickaxeScale;
        float duration = spawner != null ? spawner.PickaxeSwingDuration : pickaxeSwingDuration;
        Ease ease = spawner != null ? spawner.PickaxeEase : pickaxeEase;

        // ツルハシオブジェクトを生成
        GameObject pickaxeObj = new GameObject("Pickaxe");
        SpriteRenderer pickaxeSr = pickaxeObj.AddComponent<SpriteRenderer>();
        pickaxeSr.sprite = pickaxeSprite;
        pickaxeSr.sortingOrder = 100;

        // スケール設定
        pickaxeObj.transform.localScale = Vector3.one * scale;

        // 岩からの相対位置に配置
        Vector3 startPos = transform.position + new Vector3(startOffset.x, startOffset.y, 0);
        Vector3 endPos = transform.position + new Vector3(endOffset.x, endOffset.y, 0);
        pickaxeObj.transform.position = startPos;
        pickaxeObj.transform.rotation = Quaternion.Euler(0, 0, startRot);

        // 振り下ろしアニメーション
        Sequence seq = DOTween.Sequence();
        seq.Append(pickaxeObj.transform.DORotate(new Vector3(0, 0, endRot), duration).SetEase(ease));
        seq.Join(pickaxeObj.transform.DOMove(endPos, duration).SetEase(ease));
        seq.AppendCallback(() =>
        {
            Destroy(pickaxeObj);
        });
    }

    /// <summary>
    /// 破片を飛散させる
    /// </summary>
    private void SpawnBrokenPieces(int count, float forceMultiplier = 1f)
    {
        if (brokenPieceSprites == null || brokenPieceSprites.Length == 0) return;

        // 岩のスケールを取得
        float rockScale = RockSpawner.Instance != null ? RockSpawner.Instance.RockScale : 1f;

        for (int i = 0; i < count; i++)
        {
            GameObject piece = new GameObject("BrokenPiece");
            SpriteRenderer pieceSr = piece.AddComponent<SpriteRenderer>();
            pieceSr.sprite = brokenPieceSprites[Random.Range(0, brokenPieceSprites.Length)];
            pieceSr.sortingOrder = 50;

            piece.transform.position = transform.position;
            piece.transform.localScale = Vector3.one * Random.Range(0.3f, 0.6f) * rockScale;

            // ランダムな方向に飛散（飛距離も岩のスケールに比例）
            Vector2 direction = Random.insideUnitCircle.normalized;
            float force = pieceScatterForce * forceMultiplier * Random.Range(0.8f, 1.2f) * rockScale;

            // DOTweenで飛散アニメーション
            Sequence seq = DOTween.Sequence();
            Vector3 targetPos = piece.transform.position + new Vector3(direction.x, direction.y, 0) * force;
            seq.Append(piece.transform.DOMove(targetPos, 0.5f).SetEase(Ease.OutQuad));
            seq.Join(piece.transform.DORotate(new Vector3(0, 0, Random.Range(-360f, 360f)), 0.5f, RotateMode.FastBeyond360));
            seq.Join(pieceSr.DOFade(0, 0.5f).SetDelay(0.3f));
            seq.AppendCallback(() => Destroy(piece));
        }
    }

    /// <summary>
    /// 岩を破壊
    /// </summary>
    private void BreakRock()
    {
        if (isBreaking) return;
        isBreaking = true;
        isTappable = false;

        Debug.Log("[RockController] 岩が壊れた！");

        // 効果音
        FossilDiggingSFXPlayer.Instance?.PlayBroken();

        // レア度を決定
        currentRarity = RockSpawner.Instance?.DetermineRarity() ?? TreasureRarity.Normal;

        // 破壊エフェクト
        if (breakEffectPrefab != null)
        {
            GameObject effect = Instantiate(breakEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 3f);
        }

        // 大量の破片を飛散
        SpawnBrokenPieces(8, 1.5f);

        // 岩が消える演出
        Sequence breakSeq = DOTween.Sequence();
        breakSeq.Append(transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack));
        breakSeq.Join(spriteRenderer.DOFade(0, 0.3f));
        breakSeq.AppendCallback(() =>
        {
            // 宝物を表示
            ShowTreasure();
        });
    }

    /// <summary>
    /// 宝物（化石/宝石）を表示
    /// </summary>
    private void ShowTreasure()
    {
        // 宝物スプライトを選択
        Sprite treasureSprite = GetTreasureSprite(currentRarity);
        if (treasureSprite == null)
        {
            Debug.LogWarning("[RockController] 宝物スプライトがありません");
            RespawnRock();
            return;
        }

        // ファンファーレ再生
        FossilDiggingSFXPlayer.Instance?.PlayFanfareByRarity(currentRarity);

        // 岩のスケールを取得
        float rockScale = RockSpawner.Instance != null ? RockSpawner.Instance.RockScale : 1f;

        // 宝物オブジェクトを生成
        GameObject treasure = new GameObject("Treasure");
        SpriteRenderer treasureSr = treasure.AddComponent<SpriteRenderer>();
        treasureSr.sprite = treasureSprite;
        treasureSr.sortingOrder = 60;
        treasure.transform.position = transform.position;
        treasure.transform.localScale = Vector3.zero;

        // 出現アニメーション（岩のスケールに合わせる）
        Vector3 targetScale = Vector3.one * rockScale;
        Sequence showSeq = DOTween.Sequence();
        showSeq.Append(treasure.transform.DOScale(targetScale, 0.5f).SetEase(Ease.OutBack));
        
        // スーパーレアの場合は特別な演出
        if (currentRarity == TreasureRarity.SuperRare)
        {
            showSeq.Append(treasure.transform.DOScale(targetScale * 1.2f, 0.3f).SetEase(Ease.InOutSine));
            showSeq.Append(treasure.transform.DOScale(targetScale, 0.3f).SetEase(Ease.InOutSine));
        }

        // 表示時間後に消える
        showSeq.AppendInterval(treasureDisplayTime);
        showSeq.Append(treasure.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack));
        showSeq.AppendCallback(() =>
        {
            Destroy(treasure);
            RespawnRock();
        });

        Debug.Log($"[RockController] 宝物出現: {currentRarity}");
    }

    /// <summary>
    /// レア度に応じた宝物スプライトを取得
    /// </summary>
    private Sprite GetTreasureSprite(TreasureRarity rarity)
    {
        Sprite[] sprites = null;
        switch (rarity)
        {
            case TreasureRarity.Normal:
                sprites = normalTreasureSprites;
                break;
            case TreasureRarity.Rare:
                sprites = rareTreasureSprites;
                break;
            case TreasureRarity.SuperRare:
                sprites = superRareTreasureSprites;
                break;
        }

        if (sprites != null && sprites.Length > 0)
        {
            return sprites[Random.Range(0, sprites.Length)];
        }
        return null;
    }

    /// <summary>
    /// 岩を再出現
    /// </summary>
    private void RespawnRock()
    {
        // イベント発火
        OnRockBroken?.Invoke(spawnPointIndex);

        // このオブジェクトは削除（Spawnerが新しい岩を生成する）
        Destroy(gameObject);
    }

    /// <summary>
    /// 各種スプライトを設定（セットアップ用）
    /// </summary>
    public void SetSprites(Sprite[] rocks, Sprite pickaxe, Sprite[] pieces, 
                          Sprite[] normal, Sprite[] rare, Sprite[] superRare)
    {
        rockSprites = rocks;
        pickaxeSprite = pickaxe;
        brokenPieceSprites = pieces;
        normalTreasureSprites = normal;
        rareTreasureSprites = rare;
        superRareTreasureSprites = superRare;
    }

    /// <summary>
    /// エフェクトプレハブを設定（セットアップ用）
    /// </summary>
    public void SetEffects(GameObject hitEffect, GameObject breakEffect)
    {
        hitEffectPrefab = hitEffect;
        breakEffectPrefab = breakEffect;
    }

    /// <summary>
    /// ツルハシ演出設定を適用（セットアップ用）
    /// </summary>
    public void SetPickaxeSettings(Vector2 startOffset, Vector2 endOffset, 
                                   float startRotation, float endRotation, 
                                   float scale, float swingDuration, Ease ease)
    {
        pickaxeStartOffset = startOffset;
        pickaxeEndOffset = endOffset;
        pickaxeStartRotation = startRotation;
        pickaxeEndRotation = endRotation;
        pickaxeScale = scale;
        pickaxeSwingDuration = swingDuration;
        pickaxeEase = ease;
    }
}
