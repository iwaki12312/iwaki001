using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

/// <summary>
/// Aquariumゲームのメインコントローラー
/// タップで泡→生き物出現、既存の生き物タップでリアクション
/// 12体揃ったらお祝い→リセット
/// </summary>
public class AquariumController : MonoBehaviour
{
    [Header("=== 生き物スプライト（通常）===")]
    [SerializeField] private Sprite clownfishSprite;
    [SerializeField] private Sprite angelfishSprite;
    [SerializeField] private Sprite octopusSprite;
    [SerializeField] private Sprite seaTurtleSprite;
    [SerializeField] private Sprite pufferfishSprite;
    [SerializeField] private Sprite jellyfishSprite;
    [SerializeField] private Sprite seahorseSprite;
    [SerializeField] private Sprite stingraySprite;

    [Header("=== 生き物スプライト（レア）===")]
    [SerializeField] private Sprite whaleSharkSprite;
    [SerializeField] private Sprite mantaSprite;

    [Header("=== ゲーム設定 ===")]
    [SerializeField, Range(5, 20)] private int maxCreatures = 12;
    [SerializeField, Range(0f, 1f)] private float rareChance = 0.1f;
    [SerializeField, Range(0.3f, 3f)] private float creatureScale = 0.8f;
    [SerializeField, Range(0.3f, 3f)] private float rareCreatureScale = 1.2f;
    [SerializeField, Range(0.3f, 3f)] private float colliderRadius = 0.8f;

    [Header("=== スポーン範囲 ===")]
    [SerializeField] private float spawnMinX = -5.5f;
    [SerializeField] private float spawnMaxX = 5.5f;
    [SerializeField] private float spawnMinY = -3.5f;
    [SerializeField] private float spawnMaxY = 3f;

    [Header("=== アニメーション設定 ===")]
    [SerializeField] private float bubbleDuration = 0.6f;
    [SerializeField] private float celebrationDelay = 3f;

    private List<SeaCreatureController> spawnedCreatures = new List<SeaCreatureController>();
    private int currentCreatureCount = 0;
    private bool isCelebrating = false;
    private Camera mainCamera;

    // マルチタッチ管理
    private HashSet<int> processedTouchIds = new HashSet<int>();

    // 通常生き物のスプライトと種類
    private struct CreatureInfo
    {
        public Sprite sprite;
        public SeaCreatureType type;
    }
    private List<CreatureInfo> normalCreaturePool;

    void Start()
    {
        mainCamera = Camera.main;
        BuildCreaturePool();

        // 環境音再生
        if (AquariumSFXPlayer.Instance != null)
        {
            AquariumSFXPlayer.Instance.StartAmbient();
        }
    }

    /// <summary>
    /// 通常生き物のプールを構築
    /// </summary>
    private void BuildCreaturePool()
    {
        normalCreaturePool = new List<CreatureInfo>();
        AddToPool(clownfishSprite, SeaCreatureType.Clownfish);
        AddToPool(angelfishSprite, SeaCreatureType.Angelfish);
        AddToPool(octopusSprite, SeaCreatureType.Octopus);
        AddToPool(seaTurtleSprite, SeaCreatureType.SeaTurtle);
        AddToPool(pufferfishSprite, SeaCreatureType.Pufferfish);
        AddToPool(jellyfishSprite, SeaCreatureType.Jellyfish);
        AddToPool(seahorseSprite, SeaCreatureType.Seahorse);
        AddToPool(stingraySprite, SeaCreatureType.Stingray);
    }

    private void AddToPool(Sprite sprite, SeaCreatureType type)
    {
        if (sprite != null)
        {
            normalCreaturePool.Add(new CreatureInfo { sprite = sprite, type = type });
        }
    }

    void Update()
    {
        if (isCelebrating) return;
        HandleMultiTouch();
    }

    /// <summary>
    /// マルチタッチ入力処理
    /// </summary>
    private void HandleMultiTouch()
    {
        bool touchProcessed = false;

        // タッチスクリーン入力
        if (Touchscreen.current != null)
        {
            foreach (var touch in Touchscreen.current.touches)
            {
                if (touch.press.wasPressedThisFrame)
                {
                    int touchId = touch.touchId.ReadValue();
                    if (!processedTouchIds.Contains(touchId))
                    {
                        processedTouchIds.Add(touchId);
                        Vector2 screenPos = touch.position.ReadValue();
                        ProcessTap(screenPos);
                        touchProcessed = true;
                    }
                }
                if (touch.press.wasReleasedThisFrame)
                {
                    int touchId = touch.touchId.ReadValue();
                    processedTouchIds.Remove(touchId);
                }
            }
        }

        // マウス入力（エディタ用）
        // タッチが処理されたフレームではスキップ（タッチからのマウスシミュレーションで二重発火を防止）
        if (!touchProcessed && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 screenPos = Mouse.current.position.ReadValue();
            ProcessTap(screenPos);
        }
    }

    /// <summary>
    /// タップ処理 - 生き物に当たったらリアクション、水面ならスポーン
    /// </summary>
    private void ProcessTap(Vector2 screenPosition)
    {
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 0));
        worldPos.z = 0;

        // 既存の生き物に当たったかチェック
        for (int i = spawnedCreatures.Count - 1; i >= 0; i--)
        {
            if (spawnedCreatures[i] != null && spawnedCreatures[i].ContainsPoint(worldPos))
            {
                spawnedCreatures[i].OnTapped();
                return;
            }
        }

        // 空の水面をタップ → 新しい生き物をスポーン
        if (currentCreatureCount < maxCreatures)
        {
            StartCoroutine(SpawnCreatureWithBubble(worldPos));
        }
    }

    /// <summary>
    /// 泡エフェクト → 生き物出現
    /// </summary>
    private IEnumerator SpawnCreatureWithBubble(Vector3 tapPos)
    {
        // SE: 泡
        if (AquariumSFXPlayer.Instance != null)
        {
            AquariumSFXPlayer.Instance.PlayBubbleSound();
        }

        // 泡を生成（シンプルな円パーティクル）
        List<GameObject> bubbles = new List<GameObject>();
        int bubbleCount = Random.Range(3, 6);
        for (int i = 0; i < bubbleCount; i++)
        {
            GameObject bubble = CreateBubbleObject();
            Vector3 startPos = tapPos + new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(-0.3f, 0.3f), 0);
            bubble.transform.position = startPos;
            float bubbleScale = Random.Range(0.15f, 0.4f);
            bubble.transform.localScale = Vector3.zero;

            // 泡が膨らんで上へ漂う
            Sequence bubbleSeq = DOTween.Sequence();
            bubbleSeq.Append(bubble.transform.DOScale(Vector3.one * bubbleScale, 0.2f).SetEase(Ease.OutBack));
            bubbleSeq.Append(bubble.transform.DOMoveY(startPos.y + Random.Range(1f, 2f), bubbleDuration)
                .SetEase(Ease.OutSine));
            bubbleSeq.Join(bubble.transform.DOScale(0f, bubbleDuration * 0.5f).SetDelay(bubbleDuration * 0.5f));
            bubbleSeq.AppendCallback(() => Destroy(bubble));

            // 安全策: DOTweenが失敗しても確実に消す
            Destroy(bubble, 3f);

            bubbles.Add(bubble);
        }

        yield return new WaitForSeconds(bubbleDuration * 0.7f);

        // レア判定
        bool isRare = Random.value < rareChance;

        // 生き物を選択
        Sprite creatureSprite;
        SeaCreatureType creatureType;
        float scale;

        if (isRare)
        {
            // レア生き物
            if (Random.value < 0.5f && whaleSharkSprite != null)
            {
                creatureSprite = whaleSharkSprite;
                creatureType = SeaCreatureType.WhaleShark;
            }
            else if (mantaSprite != null)
            {
                creatureSprite = mantaSprite;
                creatureType = SeaCreatureType.Manta;
            }
            else
            {
                isRare = false;
                var info = normalCreaturePool[Random.Range(0, normalCreaturePool.Count)];
                creatureSprite = info.sprite;
                creatureType = info.type;
            }
            scale = rareCreatureScale;
        }
        else
        {
            if (normalCreaturePool.Count == 0)
            {
                Debug.LogWarning("[Aquarium] 生き物スプライトが設定されていません");
                yield break;
            }
            var info = normalCreaturePool[Random.Range(0, normalCreaturePool.Count)];
            creatureSprite = info.sprite;
            creatureType = info.type;
            scale = creatureScale;
        }

        // 生き物を生成
        SpawnCreature(tapPos, creatureSprite, creatureType, isRare, scale);
    }

    /// <summary>
    /// 泡オブジェクトを生成
    /// </summary>
    private GameObject CreateBubbleObject()
    {
        GameObject bubble = new GameObject("Bubble");
        SpriteRenderer sr = bubble.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 50;

        // 半透明の白い円を作成
        int texSize = 32;
        Texture2D tex = new Texture2D(texSize, texSize);
        Vector2 center = new Vector2(texSize / 2f, texSize / 2f);
        float radius = texSize / 2f;

        for (int x = 0; x < texSize; x++)
        {
            for (int y = 0; y < texSize; y++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                if (dist < radius)
                {
                    float alpha = 1f - (dist / radius);
                    tex.SetPixel(x, y, new Color(0.8f, 0.95f, 1f, alpha * 0.6f));
                }
                else
                {
                    tex.SetPixel(x, y, Color.clear);
                }
            }
        }
        tex.Apply();
        sr.sprite = Sprite.Create(tex, new Rect(0, 0, texSize, texSize), new Vector2(0.5f, 0.5f), texSize);

        return bubble;
    }

    /// <summary>
    /// 生き物を実際にスポーン
    /// </summary>
    private void SpawnCreature(Vector3 position, Sprite sprite, SeaCreatureType type, bool isRare, float scale)
    {
        // 最大数ガード（並行コルーチンからの超過防止）
        if (currentCreatureCount >= maxCreatures)
        {
            Debug.Log("[Aquarium] 最大数超過をガード");
            return;
        }

        // SE: 登場
        if (AquariumSFXPlayer.Instance != null)
        {
            AquariumSFXPlayer.Instance.PlaySpawnSound();
            if (isRare)
            {
                AquariumSFXPlayer.Instance.PlayRareSound();
            }
        }

        // スポーン位置をクランプ
        float x = Mathf.Clamp(position.x, spawnMinX, spawnMaxX);
        float y = Mathf.Clamp(position.y, spawnMinY, spawnMaxY);

        GameObject creatureObj = new GameObject($"Creature_{type}_{currentCreatureCount:D2}");
        creatureObj.transform.position = new Vector3(x, y, 0);

        SpriteRenderer sr = creatureObj.AddComponent<SpriteRenderer>();
        sr.sortingOrder = isRare ? 25 : 15;

        CircleCollider2D col = creatureObj.AddComponent<CircleCollider2D>();
        col.radius = colliderRadius;

        SeaCreatureController creature = creatureObj.AddComponent<SeaCreatureController>();
        creature.Initialize(type, sprite, isRare, colliderRadius);

        // ポップインアニメーション（生き物自身のコルーチンで管理、DOTween不使用）
        Vector3 targetScale = Vector3.one * scale;
        creature.StartSpawnAnimation(targetScale);

        // レア出現時の特別演出
        if (isRare)
        {
            PlayRareSpawnEffect(creatureObj);
        }

        spawnedCreatures.Add(creature);
        currentCreatureCount++;

        Debug.Log($"[Aquarium] 生き物スポーン: {type} (レア={isRare}), 合計: {currentCreatureCount}/{maxCreatures}");

        // 完成チェック（スポーンアニメ完了を待ってからお祝い開始）
        if (currentCreatureCount >= maxCreatures && !isCelebrating)
        {
            StartCoroutine(DelayedCelebration());
        }
    }

    /// <summary>
    /// スポーンアニメーション完了待ちでお祝い開始
    /// </summary>
    private IEnumerator DelayedCelebration()
    {
        isCelebrating = true;
        // スポーンアニメーション（0.4s）が完了するのを待つ
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(CelebrationSequence());
    }

    /// <summary>
    /// レア出現時の特別演出
    /// </summary>
    private void PlayRareSpawnEffect(GameObject creatureObj)
    {
        // カメラシェイク
        if (mainCamera != null)
        {
            mainCamera.transform.DOShakePosition(0.4f, 0.15f, 12, 90, false, true);
        }

        // レインボーフラッシュ
        SpriteRenderer sr = creatureObj.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Sequence colorSeq = DOTween.Sequence();
            colorSeq.Append(sr.DOColor(new Color(1f, 0.8f, 0.8f), 0.08f));
            colorSeq.Append(sr.DOColor(new Color(1f, 1f, 0.8f), 0.08f));
            colorSeq.Append(sr.DOColor(new Color(0.8f, 1f, 0.8f), 0.08f));
            colorSeq.Append(sr.DOColor(new Color(0.8f, 1f, 1f), 0.08f));
            colorSeq.Append(sr.DOColor(new Color(0.8f, 0.8f, 1f), 0.08f));
            colorSeq.Append(sr.DOColor(Color.white, 0.08f));
        }

        // キラキラパーティクル
        SpawnSparkles(creatureObj.transform.position, 8);
    }

    /// <summary>
    /// キラキラパーティクルを生成
    /// </summary>
    private void SpawnSparkles(Vector3 position, int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject sparkle = new GameObject("Sparkle");
            SpriteRenderer sr = sparkle.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 80;

            // 小さな光の粒
            Texture2D tex = new Texture2D(8, 8);
            Color sparkleColor = Color.HSVToRGB(Random.value, 0.3f, 1f);
            for (int px = 0; px < 8; px++)
                for (int py = 0; py < 8; py++)
                {
                    float dist = Vector2.Distance(new Vector2(px, py), new Vector2(4, 4));
                    float alpha = Mathf.Clamp01(1f - dist / 4f);
                    tex.SetPixel(px, py, new Color(sparkleColor.r, sparkleColor.g, sparkleColor.b, alpha));
                }
            tex.Apply();
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, 8, 8), new Vector2(0.5f, 0.5f), 8);

            sparkle.transform.position = position;
            sparkle.transform.localScale = Vector3.one * Random.Range(0.2f, 0.5f);

            Vector3 targetPos = position + new Vector3(
                Random.Range(-1.5f, 1.5f),
                Random.Range(-1.5f, 1.5f),
                0
            );

            Sequence seq = DOTween.Sequence();
            seq.Append(sparkle.transform.DOMove(targetPos, 0.6f).SetEase(Ease.OutQuad));
            seq.Join(sparkle.transform.DOScale(0f, 0.6f).SetEase(Ease.InQuad));
            seq.AppendCallback(() => Destroy(sparkle));

            // 安全策: DOTweenが失敗しても確実に消す
            Destroy(sparkle, 3f);
        }
    }

    /// <summary>
    /// お祝い演出シーケンス
    /// </summary>
    private IEnumerator CelebrationSequence()
    {
        // isCelebratingはDelayedCelebrationで設定済み
        Debug.Log("[Aquarium] 水槽完成！お祝い演出開始！");

        // ファンファーレ
        if (AquariumSFXPlayer.Instance != null)
        {
            AquariumSFXPlayer.Instance.PlayCompleteSound();
        }

        // 全生き物をジャンプアニメーション（スポーン完了済みのみ）
        foreach (var creature in spawnedCreatures)
        {
            if (creature != null)
            {
                creature.transform.DOPunchPosition(Vector3.up * 0.5f, 0.5f, 3);
                creature.transform.DOPunchScale(Vector3.one * 0.2f, 0.5f, 5);
            }
        }

        // キラキラを大量に撒く
        for (int i = 0; i < 3; i++)
        {
            SpawnSparkles(new Vector3(Random.Range(-3f, 3f), Random.Range(-2f, 2f), 0), 10);
            yield return new WaitForSeconds(0.3f);
        }

        // 泡エフェクト
        StartCoroutine(SpawnCelebrationBubbles());

        yield return new WaitForSeconds(celebrationDelay);

        // リセット: 全員泳いで退場
        ResetAquarium();
    }

    /// <summary>
    /// お祝い時の泡演出
    /// </summary>
    private IEnumerator SpawnCelebrationBubbles()
    {
        for (int wave = 0; wave < 3; wave++)
        {
            for (int i = 0; i < 8; i++)
            {
                GameObject bubble = CreateBubbleObject();
                float x = Random.Range(-5f, 5f);
                float startY = Random.Range(-5f, -3f);
                bubble.transform.position = new Vector3(x, startY, 0);
                float s = Random.Range(0.3f, 0.7f);
                bubble.transform.localScale = Vector3.one * s;

                float duration = Random.Range(2f, 3f);
                Sequence seq = DOTween.Sequence();
                seq.Append(bubble.transform.DOMoveY(6f, duration).SetEase(Ease.InOutSine));
                seq.Join(bubble.transform.DOMoveX(x + Random.Range(-1f, 1f), duration).SetEase(Ease.InOutSine));
                seq.AppendCallback(() => Destroy(bubble));

                // 安全策: DOTweenが失敗しても確実に消す
                Destroy(bubble, 5f);
            }
            yield return new WaitForSeconds(0.4f);
        }
    }

    /// <summary>
    /// 水槽をリセット
    /// </summary>
    private void ResetAquarium()
    {
        Debug.Log("[Aquarium] 水槽をリセット");

        if (AquariumSFXPlayer.Instance != null)
        {
            AquariumSFXPlayer.Instance.PlayResetSound();
        }

        // 全生き物を退場
        foreach (var creature in spawnedCreatures)
        {
            if (creature != null)
            {
                creature.SwimAway();
            }
        }

        spawnedCreatures.Clear();
        currentCreatureCount = 0;

        // 少し待ってからお祝い状態を解除
        DOVirtual.DelayedCall(1f, () =>
        {
            isCelebrating = false;
        });
    }

    void OnDestroy()
    {
        DOTween.Kill(transform);
        if (mainCamera != null)
        {
            DOTween.Kill(mainCamera.transform);
        }
        foreach (var creature in spawnedCreatures)
        {
            if (creature != null)
            {
                DOTween.Kill(creature.transform);
            }
        }
    }
}
