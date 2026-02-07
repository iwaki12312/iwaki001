using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

/// <summary>
/// 個別の動物を制御するクラス
/// マルチタッチ対応でタップ時のリアクションを管理
/// </summary>
public class AnimalController : MonoBehaviour
{
    [Header("動物データ")]
    [SerializeField] private AnimalVoiceData animalData;
    
    [Header("設定")]
    [SerializeField] private float colliderRadius = 1.0f;  // タップ判定の範囲
    
    [Header("パーティクル")]
    [SerializeField] private GameObject heartParticlePrefab;
    [SerializeField] private GameObject noteParticlePrefab;
    
    private SpriteRenderer spriteRenderer;
    private CircleCollider2D circleCollider;
    private Camera mainCamera;
    private bool isReacting = false;
    private Vector3 originalScale;
    
    // 削除時のイベント
    public System.Action OnDestroyed;
    
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
        originalScale = transform.localScale;
    }
    
    /// <summary>
    /// コライダー半径を外部から設定
    /// </summary>
    public void SetColliderRadius(float radius)
    {
        colliderRadius = radius;
    }
    
    /// <summary>
    /// 動物を初期化（スケール・コライダー指定あり）
    /// </summary>
    public void Initialize(AnimalVoiceData data, Vector3 position, GameObject heartPrefab, GameObject notePrefab, float baseScale, float colRadius)
    {
        colliderRadius = colRadius;
        Initialize(data, position, heartPrefab, notePrefab);
        
        // ベーススケールを適用
        transform.localScale = Vector3.one * baseScale;
        originalScale = transform.localScale;
        
        // コライダー半径を更新
        if (circleCollider != null)
        {
            circleCollider.radius = colliderRadius;
        }
    }
    
    /// <summary>
    /// 動物を初期化
    /// </summary>
    public void Initialize(AnimalVoiceData data, Vector3 position, GameObject heartPrefab, GameObject notePrefab)
    {
        // コンポーネントが未初期化の場合は初期化
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }
        }
        
        if (circleCollider == null)
        {
            circleCollider = GetComponent<CircleCollider2D>();
            if (circleCollider == null)
            {
                circleCollider = gameObject.AddComponent<CircleCollider2D>();
            }
            circleCollider.radius = colliderRadius;
        }
        
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        
        animalData = data;
        transform.position = position;
        heartParticlePrefab = heartPrefab;
        noteParticlePrefab = notePrefab;
        
        // スプライトを設定
        if (animalData != null && animalData.normalSprite != null)
        {
            spriteRenderer.sprite = animalData.normalSprite;
        }
        
        // レア動物は少し大きく表示
        if (animalData != null && animalData.isRare)
        {
            if (originalScale == Vector3.zero)
            {
                originalScale = transform.localScale;
            }
            transform.localScale = originalScale * 1.3f;
            originalScale = transform.localScale;
        }
        else if (originalScale == Vector3.zero)
        {
            originalScale = transform.localScale;
        }
        
        // フェードインアニメーション（一旦無効化）
        spriteRenderer.color = new Color(1, 1, 1, 1);  // 直接表示
        // spriteRenderer.DOFade(1f, 0.5f);
        
        Debug.Log($"[AnimalController] 動物スポーン: {animalData?.animalType}, レア={animalData?.isRare}");
    }
    
    /// <summary>
    /// 動物データを取得
    /// </summary>
    public AnimalVoiceData GetAnimalData()
    {
        return animalData;
    }
    
    /// <summary>
    /// レア動物かどうか
    /// </summary>
    public bool IsRare()
    {
        return animalData != null && animalData.isRare;
    }
    
    void Update()
    {
        if (!isReacting)
        {
            HandleTouch();
        }
    }
    
    /// <summary>
    /// タッチ処理（マルチタッチ対応）
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
                
                Vector3 worldPos = mainCamera.ScreenToWorldPoint(touch.position.ReadValue());
                worldPos.z = 0;
                
                if (circleCollider.OverlapPoint(worldPos))
                {
                    OnTapped();
                    return;
                }
            }
        }
        
        // エディタ用: マウスクリック
        var mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
        {
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(mouse.position.ReadValue());
            worldPos.z = 0;
            
            if (circleCollider.OverlapPoint(worldPos))
            {
                OnTapped();
            }
        }
    }
    
    /// <summary>
    /// タップされたときの処理
    /// </summary>
    private void OnTapped()
    {
        if (isReacting) return;
        if (animalData == null) return;
        
        isReacting = true;
        
        // 効果音再生
        if (AnimalVoiceSFXPlayer.Instance != null)
        {
            AnimalVoiceSFXPlayer.Instance.PlayVoice(animalData.voiceClip, animalData.isRare);
        }
        
        // スケールアップアニメーション
        float scale = animalData.scaleMultiplier;
        transform.DOScale(originalScale * scale, 0.15f).SetEase(Ease.OutBack);
        
        // リアクションスプライトに切り替え
        if (animalData.reactionSprite != null)
        {
            spriteRenderer.sprite = animalData.reactionSprite;
        }
        
        // パーティクル生成
        SpawnParticle();
        
        // レア動物は特別なエフェクト
        if (animalData.isRare)
        {
            PlayRareEffect();
        }
        
        // 元に戻す
        float duration = animalData.reactionDuration;
        DOVirtual.DelayedCall(duration, () =>
        {
            // 元のスプライトに戻す
            if (animalData.normalSprite != null)
            {
                spriteRenderer.sprite = animalData.normalSprite;
            }
            
            // スケールを戻す
            transform.DOScale(originalScale, 0.2f).SetEase(Ease.OutQuad);
            
            isReacting = false;
        });
    }
    
    /// <summary>
    /// パーティクルを生成
    /// </summary>
    private void SpawnParticle()
    {
        // ランダムでハートか音符を生成
        GameObject prefab = Random.value > 0.5f ? heartParticlePrefab : noteParticlePrefab;
        
        if (prefab != null)
        {
            Vector3 particlePos = transform.position + new Vector3(0, 0.5f, 0);
            GameObject particle = Instantiate(prefab, particlePos, Quaternion.identity);
            Destroy(particle, 2f);
        }
    }
    
    /// <summary>
    /// レア動物の特別エフェクト
    /// </summary>
    private void PlayRareEffect()
    {
        // 画面を少し揺らす
        if (mainCamera != null)
        {
            mainCamera.transform.DOShakePosition(0.3f, 0.1f, 10, 90, false, true);
        }
        
        // 色を虹色に変化させる
        Sequence colorSeq = DOTween.Sequence();
        colorSeq.Append(spriteRenderer.DOColor(new Color(1f, 0.8f, 0.8f), 0.1f));
        colorSeq.Append(spriteRenderer.DOColor(new Color(1f, 1f, 0.8f), 0.1f));
        colorSeq.Append(spriteRenderer.DOColor(new Color(0.8f, 1f, 0.8f), 0.1f));
        colorSeq.Append(spriteRenderer.DOColor(new Color(0.8f, 1f, 1f), 0.1f));
        colorSeq.Append(spriteRenderer.DOColor(Color.white, 0.1f));
    }
    
    /// <summary>
    /// フェードアウトして削除
    /// </summary>
    public void FadeOutAndDestroy()
    {
        spriteRenderer.DOFade(0f, 0.5f).OnComplete(() =>
        {
            OnDestroyed?.Invoke();
            Destroy(gameObject);
        });
    }
    
    void OnDestroy()
    {
        DOTween.Kill(transform);
        DOTween.Kill(spriteRenderer);
    }
}
