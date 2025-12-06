using UnityEngine;
using DG.Tweening;

/// <summary>
/// 個別の魚を制御するクラス
/// マルチタッチ対応で泳ぎ→タップ→釣り上げの流れを管理
/// </summary>
public class FishController : MonoBehaviour
{
    [Header("魚の設定")]
    [SerializeField] private Sprite fishSprite;          // 魚のスプライト
    [SerializeField] private bool isRare = false;        // レアかどうか
    [SerializeField] [Range(0.5f, 2.0f)] private float sizeMultiplier = 1.0f; // 個別サイズ微調整（1.0=標準、0.5=半分、2.0=2倍）
    
    [Header("泳ぎ設定")]
    [SerializeField] private float swimSpeed = 2.0f;     // 泳ぐ速度
    
    [Header("釣り上げアニメーション設定")]
    [SerializeField] private float catchAngle = 45f;           // 釣り上げ角度（度）
    [SerializeField] private float catchDistance = 8.0f;       // 釣り上げ距離
    [SerializeField] private float catchDuration = 1.5f;       // 釣り上げ時間（秒）
    [SerializeField] private float fadeOutDuration = 0.5f;     // フェードアウト時間（秒）
    [SerializeField] private AnimationCurve catchCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // 軌道カーブ
    
    [Header("釣り糸位置設定")]
    [SerializeField] private float catchPointOffsetX = 1.5f;   // 釣り人からのX軸オフセット
    [SerializeField] private float catchPointOffsetY = -2.0f;  // 釣り人からのY軸オフセット
    
    private SpriteRenderer spriteRenderer;
    private CircleCollider2D circleCollider;
    private Camera mainCamera;
    private bool isCaught = false;
    private bool canBeStolen = true; // カモメに奪われる可能性があるか
    
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
        
        mainCamera = Camera.main;
    }
    
    /// <summary>
    /// 魚を初期化
    /// </summary>
    public void Initialize(Sprite sprite, Vector3 position, float speed, bool rare, float baseFishSize, float sizeMultiplier = 1.0f)
    {
        fishSprite = sprite;
        transform.position = position;
        swimSpeed = speed;
        isRare = rare;
        canBeStolen = !rare; // レアはカモメに奪われない
        
        // スプライトを設定
        spriteRenderer.sprite = fishSprite;
        spriteRenderer.sortingOrder = 1;
        
        // スプライトサイズを統一
        if (fishSprite != null)
        {
            float spriteWidth = fishSprite.bounds.size.x;
            float spriteHeight = fishSprite.bounds.size.y;
            float currentSize = Mathf.Max(spriteWidth, spriteHeight);
            
            if (currentSize > 0)
            {
                float scale = baseFishSize / currentSize * sizeMultiplier;
                transform.localScale = Vector3.one * scale;
            }
        }
        
        // コライダーサイズを設定
        circleCollider.radius = 1.2f;
    }
    
    void Update()
    {
        if (isCaught) return;
        
        // 右方向に泳ぐ
        transform.Translate(Vector3.right * swimSpeed * Time.deltaTime);
        
        // マルチタッチ対応
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);
            if (touch.phase == TouchPhase.Began && HitThisFish(touch.position))
            {
                // 同一フレームで複数の魚が釣られないように制御
                if (FishSpawner.Instance != null && !FishSpawner.Instance.TryClaimCatch())
                {
                    return;
                }
                OnFishCaught();
                return;
            }
        }
        
        // エディタ用マウス操作
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0) && HitThisFish(Input.mousePosition))
        {
            // 同一フレームで複数の魚が釣られないように制御
            if (FishSpawner.Instance != null && !FishSpawner.Instance.TryClaimCatch())
            {
                return;
            }
            OnFishCaught();
            return;
        }
#endif
        
        // 画面外判定（右端を超えたら削除）- カモメに奪われている場合はスキップ
        if (transform.parent == null && IsOutOfScreen())
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// 魚が釣られた時の処理
    /// </summary>
    private void OnFishCaught()
    {
        if (isCaught) return;
        isCaught = true;
        
        // コライダーを無効化
        circleCollider.enabled = false;
        
        // 釣り人に通知
        if (FishermanController.Instance != null)
        {
            FishermanController.Instance.OnFishCaught();
        }
        
        // カモメイベント判定（レア魚は除外）
        bool seagullEvent = canBeStolen && ShouldSeagullSteal();
        Debug.Log($"[FishController] カモメ判定: canBeStolen={canBeStolen}, seagullChance={FishSpawner.Instance?.SeagullChance}, result={seagullEvent}");
        
        if (seagullEvent)
        {
            // カモメに奪われる
            SpawnSeagull();
        }
        else
        {
            // 通常の釣り上げ
            StartCatchAnimation(false);
            
            // 効果音を再生（カモメに奪われない場合のみ）
            if (FishingSFXPlayer.Instance != null)
            {
                if (isRare)
                {
                    FishingSFXPlayer.Instance.PlayFishRare();
                }
                else
                {
                    FishingSFXPlayer.Instance.PlayFishCatch();
                }
            }
        }
    }
    
    /// <summary>
    /// カモメに奪われるかどうかの判定
    /// </summary>
    private bool ShouldSeagullSteal()
    {
        if (FishSpawner.Instance == null) return false;
        return Random.value < FishSpawner.Instance.SeagullChance;
    }
    
    /// <summary>
    /// カモメを出現させる
    /// </summary>
    private void SpawnSeagull()
    {
        GameObject seagullPrefab = FishSpawner.Instance?.SeagullPrefab;
        Debug.Log($"[FishController] カモメ出現処理開始: Prefab={seagullPrefab != null}");
        
        if (seagullPrefab == null)
        {
            Debug.LogWarning("[FishController] カモメPrefabが設定されていません。通常の釣り上げに戻します。");
            // Prefabがない場合は通常の釣り上げ
            StartCatchAnimation(false);
            
            // 効果音を再生
            if (FishingSFXPlayer.Instance != null)
            {
                if (isRare)
                {
                    FishingSFXPlayer.Instance.PlayFishRare();
                }
                else
                {
                    FishingSFXPlayer.Instance.PlayFishCatch();
                }
            }
            return;
        }
        
        // 魚の釣り上げアニメーションを開始（カモメイベント用）
        StartCatchAnimation(true);
        
        // 0.5秒後にカモメを出現させる（魚が空中にいるタイミング）
        DOVirtual.DelayedCall(0.5f, () => {
            if (this != null && gameObject != null)
            {
                GameObject seagullObj = Instantiate(seagullPrefab);
                SeagullController seagull = seagullObj.GetComponent<SeagullController>();
                
                if (seagull != null)
                {
                    Debug.Log($"[FishController] カモメ出現: 魚の位置={transform.position}");
                    seagull.Initialize(transform.position, this);
                }
                else
                {
                    Debug.LogWarning("[FishController] カモメにSeagullControllerコンポーネントがありません");
                    Destroy(seagullObj);
                }
            }
        });
    }
    
    /// <summary>
    /// 釣り上げアニメーション開始
    /// </summary>
    /// <param name="isSeagullEvent">カモメイベントかどうか</param>
    private void StartCatchAnimation(bool isSeagullEvent)
    {
        // 釣り人の位置を取得（釣り糸が垂れている位置）
        Vector3 catchStartPos = GetFishermanCatchPoint();
        
        // 魚を釣り人の位置（釣り糸の位置）へ瞬時に移動
        transform.position = catchStartPos;
        
        // 釣り上げ軌道を計算（釣り人の位置から開始）
        float angleRad = catchAngle * Mathf.Deg2Rad;
        Vector3 endPos = catchStartPos + new Vector3(
            Mathf.Cos(angleRad) * catchDistance,
            Mathf.Sin(angleRad) * catchDistance,
            0
        );
        
        // 放物線の頂点を計算
        Vector3 midPos = (catchStartPos + endPos) / 2f + Vector3.up * catchDistance * 0.3f;
        
        // DOTweenで釣り上げアニメーション（2段階で放物線を描く）
        Sequence catchSeq = DOTween.Sequence();
        
        // 上昇（中間点へ）
        catchSeq.Append(transform.DOMove(midPos, catchDuration * 0.5f).SetEase(Ease.OutQuad));
        
        if (!isSeagullEvent)
        {
            // 通常の釣り上げ: 下降して終点まで
            catchSeq.Append(transform.DOMove(endPos, catchDuration * 0.5f).SetEase(Ease.InQuad));
            
            // フェードアウト（下降と同時に開始）
            catchSeq.Join(spriteRenderer.DOFade(0f, fadeOutDuration));
            
            // 完了後に削除
            catchSeq.OnComplete(() => {
                // 釣り人を待機状態に戻す
                if (FishermanController.Instance != null)
                {
                    FishermanController.Instance.ReturnToIdle();
                }
                Destroy(gameObject);
            });
        }
        else
        {
            // カモメイベント: 中間点で停止（カモメが奪いに来るまで）
            // カモメが奪った後は、カモメ側で魚を制御する
            Debug.Log("[FishController] カモメイベント用釣り上げアニメーション開始");
        }
    }
    
    /// <summary>
    /// 釣り人の釣り糸が垂れている位置を取得
    /// </summary>
    private Vector3 GetFishermanCatchPoint()
    {
        // 釣り人の位置を取得
        if (FishermanController.Instance != null)
        {
            Vector3 fishermanPos = FishermanController.Instance.transform.position;
            // 釣り人の位置からオフセットした位置を返す
            return new Vector3(fishermanPos.x + catchPointOffsetX, fishermanPos.y + catchPointOffsetY, 0);
        }
        
        // フォールバック: 画面中央やや左、水面付近
        return new Vector3(-2f, 0f, 0);
    }
    
    /// <summary>
    /// SpriteRendererを取得（SeagullControllerから使用）
    /// </summary>
    public SpriteRenderer GetSpriteRenderer()
    {
        return spriteRenderer;
    }
    
    /// <summary>
    /// タップ判定
    /// </summary>
    private bool HitThisFish(Vector2 screenPosition)
    {
        if (mainCamera == null) return false;
        
        Vector2 worldPos = mainCamera.ScreenToWorldPoint(screenPosition);
        
        // その位置に重なっている魚の中から、描画上手前にいるものを選ぶ
        FishController topMostFish = GetTopMostFishAt(worldPos);
        return topMostFish != null && topMostFish == this;
    }

    /// <summary>
    /// 指定位置に重なっている魚のうち、描画順で最前のものを取得
    /// </summary>
    private FishController GetTopMostFishAt(Vector2 worldPos)
    {
        Collider2D[] hits = Physics2D.OverlapPointAll(worldPos);
        FishController best = null;
        SortingKey bestKey = default;

        foreach (var hit in hits)
        {
            if (hit == null) continue;
            FishController fish = hit.GetComponent<FishController>();
            if (fish == null) continue;

            SortingKey key = SortingKey.From(fish);
            if (best == null || key.IsInFrontOf(bestKey))
            {
                best = fish;
                bestKey = key;
            }
        }

        return best;
    }

    /// <summary>
    /// SpriteRendererの描画順に基づく比較キー
    /// </summary>
    private struct SortingKey
    {
        public int layer;
        public int order;
        public float depth;

        public static SortingKey From(FishController fish)
        {
            var renderer = fish.GetComponent<SpriteRenderer>();
            int layerValue = renderer != null ? SortingLayer.GetLayerValueFromID(renderer.sortingLayerID) : 0;
            int sortingOrder = renderer != null ? renderer.sortingOrder : 0;
            // カメラに近い（zが小さい）ほど手前なので、逆符号で比較しやすくする
            float depthValue = -fish.transform.position.z;

            return new SortingKey
            {
                layer = layerValue,
                order = sortingOrder,
                depth = depthValue
            };
        }

        public bool IsInFrontOf(SortingKey other)
        {
            if (layer != other.layer) return layer > other.layer;
            if (order != other.order) return order > other.order;
            return depth > other.depth;
        }
    }
    
    /// <summary>
    /// 画面外判定
    /// </summary>
    private bool IsOutOfScreen()
    {
        if (mainCamera == null) return false;
        
        Vector3 viewportPosition = mainCamera.WorldToViewportPoint(transform.position);
        return viewportPosition.x > 1.2f; // 画面右端を超えたら削除
    }
    
    /// <summary>
    /// レアかどうかを取得
    /// </summary>
    public bool IsRare()
    {
        return isRare;
    }
}
