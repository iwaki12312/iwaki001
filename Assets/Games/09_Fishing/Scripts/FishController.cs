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
    [SerializeField] private float sizeMultiplier = 1.0f; // サイズ倍率（Inspector調整用）
    
    [Header("泳ぎ設定")]
    [SerializeField] private float swimSpeed = 2.0f;     // 泳ぐ速度
    
    [Header("釣り上げアニメーション設定")]
    [SerializeField] private float catchAngle = 45f;           // 釣り上げ角度（度）
    [SerializeField] private float catchDistance = 8.0f;       // 釣り上げ距離
    [SerializeField] private float catchDuration = 1.5f;       // 釣り上げ時間（秒）
    [SerializeField] private float fadeOutDuration = 0.5f;     // フェードアウト時間（秒）
    [SerializeField] private AnimationCurve catchCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // 軌道カーブ
    
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
    public void Initialize(Sprite sprite, Vector3 position, float speed, bool rare, float baseFishSize)
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
        circleCollider.radius = 0.5f;
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
                OnFishCaught();
                return;
            }
        }
        
        // エディタ用マウス操作
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0) && HitThisFish(Input.mousePosition))
        {
            OnFishCaught();
            return;
        }
#endif
        
        // 画面外判定（右端を超えたら削除）
        if (IsOutOfScreen())
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
            StartCatchAnimation();
            
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
            StartCatchAnimation();
            
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
        
        GameObject seagullObj = Instantiate(seagullPrefab);
        SeagullController seagull = seagullObj.GetComponent<SeagullController>();
        
        if (seagull != null)
        {
            Debug.Log("[FishController] カモメインスタンス化成功");
            seagull.Initialize(transform.position, this);
        }
        else
        {
            Debug.LogWarning("[FishController] カモメにSeagullControllerコンポーネントがありません");
            Destroy(seagullObj);
            StartCatchAnimation();
            
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
        }
    }
    
    /// <summary>
    /// 釣り上げアニメーション開始
    /// </summary>
    private void StartCatchAnimation()
    {
        // 角度から終点を計算
        float angleRad = catchAngle * Mathf.Deg2Rad;
        Vector3 endPos = transform.position + new Vector3(
            Mathf.Cos(angleRad) * catchDistance,
            Mathf.Sin(angleRad) * catchDistance,
            0
        );
        
        // 放物線の頂点を計算
        Vector3 midPos = (transform.position + endPos) / 2f + Vector3.up * catchDistance * 0.3f;
        
        // 軌道の配列
        Vector3[] path = new Vector3[] { transform.position, midPos, endPos };
        
        // DOTweenで釣り上げアニメーション
        Sequence catchSeq = DOTween.Sequence();
        
        // 移動アニメーション
        catchSeq.Append(transform.DOPath(path, catchDuration, PathType.CatmullRom)
            .SetEase(catchCurve));
        
        // フェードアウト（移動と同時に開始）
        catchSeq.Join(spriteRenderer.DOFade(0f, fadeOutDuration)
            .SetDelay(catchDuration - fadeOutDuration));
        
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
    
    /// <summary>
    /// カモメに奪われた時の処理（SeagullControllerから呼ばれる）
    /// </summary>
    public void OnStolenBySeagull()
    {
        // アニメーションをキャンセル
        DOTween.Kill(transform);
        DOTween.Kill(spriteRenderer);
        
        // 魚を削除
        Destroy(gameObject);
        
        // 釣り人を待機状態に戻す
        if (FishermanController.Instance != null)
        {
            FishermanController.Instance.ReturnToIdle();
        }
    }
    
    /// <summary>
    /// タップ判定
    /// </summary>
    private bool HitThisFish(Vector2 screenPosition)
    {
        if (mainCamera == null) return false;
        
        Vector2 worldPos = mainCamera.ScreenToWorldPoint(screenPosition);
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
        return hit && hit.collider != null && hit.collider.gameObject == gameObject;
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
