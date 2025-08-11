using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 星オブジェクトの挙動を制御するクラス
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class Star : MonoBehaviour
{
    [Header("移動設定")]
    [SerializeField] private float minMoveSpeed = 0.5f;
    [SerializeField] private float maxMoveSpeed = 2.0f;
    
    [Header("回転設定")]
    [SerializeField] private float minRotationSpeed = 10f;
    [SerializeField] private float maxRotationSpeed = 50f;
    
    [Header("色設定")]
    [SerializeField] private float saturation = 0.8f;
    [SerializeField] private float brightness = 1.0f;
    
    private SpriteRenderer spriteRenderer;
    private Vector2 moveDirection;
    private float moveSpeed;
    private float rotationSpeed;
    private bool rotateClockwise;
    private Camera mainCamera;
    private Sprite[] availableSprites;
    private float collisionCooldown = 0f; // 衝突クールダウン時間
    private const float COLLISION_COOLDOWN_TIME = 0.5f; // 0.5秒間は再衝突を無視
    private GameObject disappearParticlePrefab; // 消滅時のパーティクルプレファブ
    private GameObject orbitParticlePrefab; // 軌道パーティクルプレファブ
    private GameObject orbitParticleInstance; // 軌道パーティクルのインスタンス
    private bool isBigStar = false; // 巨大スターかどうか
    
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        mainCamera = Camera.main;
        
        // コライダーがない場合は追加
        if (GetComponent<Collider2D>() == null)
        {
            CircleCollider2D collider = gameObject.AddComponent<CircleCollider2D>();
            collider.radius = 1.3f; // 星のスプライトに合わせたサイズ
            collider.isTrigger = true; // Triggerに設定
        }
        // 既存のコライダーがある場合は、StarManagerで設定された値をそのまま使用
        // （巨大スターの場合は4.3f、通常の星の場合は1.3fが設定済み）
    }
    
    void Start()
    {
        InitializeStar();
    }
    
    void Update()
    {
        MoveStar();
        RotateStar();
        CheckBounds();
        CheckMouseClick();
        CheckOverlap(); // 重なりチェックを追加
        UpdateOrbitParticle(); // 軌道パーティクルの位置を更新
        
        // 衝突クールダウンタイマーを更新
        if (collisionCooldown > 0f)
        {
            collisionCooldown -= Time.deltaTime;
        }
    }
    
    /// <summary>
    /// 他の星との重なりをチェックして修正
    /// </summary>
    private void CheckOverlap()
    {
        Star[] allStars = FindObjectsOfType<Star>();
        CircleCollider2D myCollider = GetComponent<CircleCollider2D>();
        
        foreach (Star otherStar in allStars)
        {
            if (otherStar == this) continue; // 自分自身は除外
            
            CircleCollider2D otherCollider = otherStar.GetComponent<CircleCollider2D>();
            float distance = Vector2.Distance(transform.position, otherStar.transform.position);
            float minDistance = myCollider.radius + otherCollider.radius;
            
            // 重なっている場合
            if (distance < minDistance)
            {
                Vector2 separationDirection = (transform.position - otherStar.transform.position).normalized;
                
                // 距離が0に近い場合はランダムな方向を使用
                if (separationDirection.magnitude < 0.1f)
                {
                    float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                    separationDirection = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle));
                }
                
                // 適切な距離まで離す
                float separationDistance = minDistance * 1.1f; // 少し余裕を持たせる
                transform.position = otherStar.transform.position + (Vector3)(separationDirection * separationDistance);
                
                // 移動方向も少し変更して再度重なることを防ぐ
                moveDirection = Vector2.Reflect(moveDirection, separationDirection).normalized;
                
                Debug.Log("重なりを検出して修正しました。");
                break; // 1フレームに1つの重なりのみ修正
            }
        }
    }
    
    /// <summary>
    /// マウスクリック検出（PC用）- TouchManagerに移管したため無効化
    /// </summary>
    private void CheckMouseClick()
    {
        // TouchManagerがタップ処理を行うため、この処理は無効化
    }
    
    /// <summary>
    /// 星の初期化
    /// </summary>
    public void InitializeStar()
    {
        SetRandomSprite();
        SetRandomColor();
        SetRandomMovement();
        SetRandomRotation();
        
        // 効果音を再生
        if (TouchTheStarSFXPlayer.Instance != null)
        {
            TouchTheStarSFXPlayer.Instance.PlayStarAppearSound();
        }
    }
    
    /// <summary>
    /// 巨大スターとして初期化
    /// </summary>
    public void InitializeAsBigStar()
    {
        isBigStar = true;
        
        // 巨大スターは色変更なし（専用スプライトの色をそのまま使用）
        SetRandomMovement();
        SetRandomRotation();
        
        // 巨大スター専用の効果音を再生
        if (TouchTheStarSFXPlayer.Instance != null)
        {
            TouchTheStarSFXPlayer.Instance.PlayBigStarAppearSound();
        }
        
        Debug.Log("巨大スターとして初期化されました。");
    }
    
    /// <summary>
    /// ランダムなスプライトを設定
    /// </summary>
    private void SetRandomSprite()
    {
        // 現在のスプライトが設定されている場合、そのテクスチャから分割されたスプライトを取得
        if (spriteRenderer.sprite != null)
        {
            Texture2D texture = spriteRenderer.sprite.texture;
            
            // テクスチャ名を使って分割されたスプライトを検索
            Sprite[] allSprites = Resources.FindObjectsOfTypeAll<Sprite>();
            List<Sprite> starSprites = new List<Sprite>();
            
            foreach (Sprite sprite in allSprites)
            {
                if (sprite.texture == texture)
                {
                    starSprites.Add(sprite);
                }
            }
            
            if (starSprites.Count > 0)
            {
                // ランダムにスプライトを選択
                int randomIndex = Random.Range(0, starSprites.Count);
                spriteRenderer.sprite = starSprites[randomIndex];
                Debug.Log($"スプライトを変更しました: {starSprites[randomIndex].name} (インデックス: {randomIndex})");
            }
            else
            {
                Debug.LogWarning("分割された星のスプライトが見つかりません。現在のスプライトを使用します。");
            }
        }
    }
    
    /// <summary>
    /// ランダムな色を設定
    /// </summary>
    private void SetRandomColor()
    {
        // HSV色空間でランダムな色相を生成
        float hue = Random.Range(0f, 1f);
        Color randomColor = Color.HSVToRGB(hue, saturation, brightness);
        spriteRenderer.color = randomColor;
    }
    
    /// <summary>
    /// ランダムな移動方向と速度を設定
    /// </summary>
    private void SetRandomMovement()
    {
        // ランダムな方向ベクトルを生成
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        moveDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)).normalized;
        
        // ランダムな移動速度を設定
        moveSpeed = Random.Range(minMoveSpeed, maxMoveSpeed);
    }
    
    /// <summary>
    /// ランダムな回転設定
    /// </summary>
    private void SetRandomRotation()
    {
        // ランダムな回転速度を設定
        rotationSpeed = Random.Range(minRotationSpeed, maxRotationSpeed);
        
        // ランダムな回転方向を設定
        rotateClockwise = Random.Range(0, 2) == 0;
    }
    
    /// <summary>
    /// 星の移動処理
    /// </summary>
    private void MoveStar()
    {
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime);
    }
    
    /// <summary>
    /// 星の回転処理
    /// </summary>
    private void RotateStar()
    {
        float rotationAmount = rotationSpeed * Time.deltaTime;
        if (!rotateClockwise)
        {
            rotationAmount = -rotationAmount;
        }
        
        transform.Rotate(0, 0, rotationAmount);
    }
    
    /// <summary>
    /// 画面外判定
    /// </summary>
    private void CheckBounds()
    {
        if (mainCamera == null) return;
        
        Vector3 viewportPosition = mainCamera.WorldToViewportPoint(transform.position);
        
        // 画面外に出た場合は星を破棄
        if (viewportPosition.x < -0.1f || viewportPosition.x > 1.1f ||
            viewportPosition.y < -0.1f || viewportPosition.y > 1.1f)
        {
            DestroyStar(false); // 画面外に出た場合は効果音なし
        }
    }
    
    /// <summary>
    /// タップ検出処理 - TouchManagerに移管したため無効化
    /// </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        // TouchManagerがタップ処理を行うため、この処理は無効化
    }
    
    /// <summary>
    /// TouchManagerからのタップ処理
    /// </summary>
    public void HandleTouchManagerTap()
    {
        Debug.Log("Star: TouchManagerからタップされました！");
        DestroyStar(true); // タップされた場合は効果音あり
    }
    
    /// <summary>
    /// 星を破棄する
    /// </summary>
    /// <param name="playSound">効果音を再生するかどうか</param>
    private void DestroyStar(bool playSound)
    {
        // 軌道パーティクルの処理
        if (orbitParticleInstance != null)
        {
            // 軌道パーティクルの親子関係を解除
            orbitParticleInstance.transform.SetParent(null);
            
            // パーティクルシステムの新規パーティクル生成を停止
            ParticleSystem orbitParticleSystem = orbitParticleInstance.GetComponent<ParticleSystem>();
            if (orbitParticleSystem != null)
            {
                orbitParticleSystem.Stop(false, ParticleSystemStopBehavior.StopEmitting);
                
                // パーティクルの再生時間後に自動削除
                float orbitParticleDuration = orbitParticleSystem.main.duration + orbitParticleSystem.main.startLifetime.constantMax;
                StartCoroutine(DestroyOrbitParticleAfterDelay(orbitParticleInstance, orbitParticleDuration));
                
                Debug.Log($"軌道パーティクルの生成を停止しました。{orbitParticleDuration}秒後に自然に消えます。");
            }
            else
            {
                // ParticleSystemがない場合は5秒後に削除
                Destroy(orbitParticleInstance, 5f);
            }
            
            orbitParticleInstance = null; // 参照をクリア
        }
        
        // パーティクルエフェクトを生成（タップされた場合のみ）
        if (playSound && disappearParticlePrefab != null)
        {
            GameObject particleInstance = Instantiate(disappearParticlePrefab, transform.position, Quaternion.identity);
            
            // パーティクルシステムを取得して再生
            ParticleSystem particleSystem = particleInstance.GetComponent<ParticleSystem>();
            if (particleSystem != null)
            {
                particleSystem.Play();
                
                // パーティクルの再生時間後に自動削除
                float particleDuration = particleSystem.main.duration + particleSystem.main.startLifetime.constantMax;
                Destroy(particleInstance, particleDuration);
                
                Debug.Log($"消滅パーティクルエフェクトを再生しました。{particleDuration}秒後に削除されます。");
            }
            else
            {
                // ParticleSystemがない場合は5秒後に削除
                Destroy(particleInstance, 5f);
                Debug.LogWarning("消滅パーティクルプレファブにParticleSystemコンポーネントが見つかりません。");
            }
        }
        
        if (playSound && TouchTheStarSFXPlayer.Instance != null)
        {
            if (isBigStar)
            {
                TouchTheStarSFXPlayer.Instance.PlayBigStarDisappearSound();
            }
            else
            {
                TouchTheStarSFXPlayer.Instance.PlayStarDisappearSound();
            }
        }
        
        // StarManagerに星が破棄されたことを通知
        StarManager starManager = FindObjectOfType<StarManager>();
        if (starManager != null)
        {
            if (isBigStar)
            {
                starManager.OnBigStarDestroyed();
            }
            else
            {
                starManager.OnStarDestroyed();
            }
        }
        
        Destroy(gameObject);
    }
    
    /// <summary>
    /// 軌道パーティクルを遅延削除するコルーチン
    /// </summary>
    private IEnumerator DestroyOrbitParticleAfterDelay(GameObject particleObject, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (particleObject != null)
        {
            Destroy(particleObject);
            Debug.Log("軌道パーティクルが自然に消えて削除されました。");
        }
    }
    
    /// <summary>
    /// 他の星との衝突処理
    /// </summary>
    void OnTriggerEnter2D(Collider2D other)
    {
        // クールダウン中は衝突を無視
        if (collisionCooldown > 0f) return;
        
        // 他の星と衝突した場合
        Star otherStar = other.GetComponent<Star>();
        if (otherStar != null && otherStar.collisionCooldown <= 0f)
        {
            // 衝突した星同士の方向を反射
            ReflectDirection(otherStar);
        }
    }
    
    /// <summary>
    /// 衝突時の方向反射処理
    /// </summary>
    private void ReflectDirection(Star otherStar)
    {
        // 衝突クールダウンを設定（両方の星に）
        collisionCooldown = COLLISION_COOLDOWN_TIME;
        otherStar.collisionCooldown = COLLISION_COOLDOWN_TIME;
        
        // 衝突点の計算
        Vector2 collisionDirection = (transform.position - otherStar.transform.position).normalized;
        
        // 距離が0に近い場合（完全に重なっている場合）はランダムな方向を使用
        if (collisionDirection.magnitude < 0.1f)
        {
            float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            collisionDirection = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle));
        }
        
        // より強い方向変化を適用（軽い衝突でも確実に方向が変わるように）
        // 反射方向にさらに大きなランダム要素を追加
        Vector2 reflectedDirection = Vector2.Reflect(moveDirection, collisionDirection);
        float randomAngleOffset = Random.Range(-60f, 60f) * Mathf.Deg2Rad; // ±60度のより大きなランダム要素
        float cos = Mathf.Cos(randomAngleOffset);
        float sin = Mathf.Sin(randomAngleOffset);
        moveDirection = new Vector2(
            reflectedDirection.x * cos - reflectedDirection.y * sin,
            reflectedDirection.x * sin + reflectedDirection.y * cos
        ).normalized;
        
        // 他の星の移動方向も反射（より大きなランダム要素を追加）
        Vector2 otherReflectedDirection = Vector2.Reflect(otherStar.moveDirection, -collisionDirection);
        float otherRandomAngleOffset = Random.Range(-60f, 60f) * Mathf.Deg2Rad;
        float otherCos = Mathf.Cos(otherRandomAngleOffset);
        float otherSin = Mathf.Sin(otherRandomAngleOffset);
        otherStar.moveDirection = new Vector2(
            otherReflectedDirection.x * otherCos - otherReflectedDirection.y * otherSin,
            otherReflectedDirection.x * otherSin + otherReflectedDirection.y * otherCos
        ).normalized;
        
        // 速度にも確実に変化を加える（より大きな変化範囲）
        float speedVariation = Random.Range(0.6f, 1.5f); // 60%〜150%の速度変化
        moveSpeed = Mathf.Clamp(moveSpeed * speedVariation, minMoveSpeed, maxMoveSpeed);
        
        float otherSpeedVariation = Random.Range(0.6f, 1.5f);
        otherStar.moveSpeed = Mathf.Clamp(otherStar.moveSpeed * otherSpeedVariation, otherStar.minMoveSpeed, otherStar.maxMoveSpeed);
        
        // 回転速度にも確実に変化を加える
        float rotationVariation = Random.Range(0.5f, 2.0f); // より大きな変化範囲
        rotationSpeed = Mathf.Clamp(rotationSpeed * rotationVariation, minRotationSpeed, maxRotationSpeed);
        
        float otherRotationVariation = Random.Range(0.5f, 2.0f);
        otherStar.rotationSpeed = Mathf.Clamp(otherStar.rotationSpeed * otherRotationVariation, otherStar.minRotationSpeed, otherStar.maxRotationSpeed);
        
        // 回転方向もより高い確率で変更
        if (Random.Range(0f, 1f) < 0.5f) // 50%の確率で回転方向変更
        {
            rotateClockwise = !rotateClockwise;
        }
        if (Random.Range(0f, 1f) < 0.5f)
        {
            otherStar.rotateClockwise = !otherStar.rotateClockwise;
        }
        
        // コライダーの半径を取得
        CircleCollider2D myCollider = GetComponent<CircleCollider2D>();
        CircleCollider2D otherCollider = otherStar.GetComponent<CircleCollider2D>();
        float separationDistance = (myCollider.radius + otherCollider.radius) * 1.3f; // より大きな分離距離
        
        // 十分な距離まで離す
        transform.position = otherStar.transform.position + (Vector3)(collisionDirection * separationDistance);
        
        Debug.Log($"星同士が衝突しました！方向・速度・回転を大きく変更し、{COLLISION_COOLDOWN_TIME}秒のクールダウンを設定しました。");
    }
    
    /// <summary>
    /// 移動方向を変更（外部から呼び出し用）
    /// </summary>
    public void SetMoveDirection(Vector2 newDirection)
    {
        moveDirection = newDirection.normalized;
    }
    
    /// <summary>
    /// 利用可能なスプライト配列を設定
    /// </summary>
    public void SetAvailableSprites(Sprite[] sprites)
    {
        availableSprites = sprites;
    }
    
    /// <summary>
    /// 消滅時のパーティクルプレファブを設定
    /// </summary>
    public void SetDisappearParticle(GameObject particlePrefab)
    {
        disappearParticlePrefab = particlePrefab;
    }
    
    /// <summary>
    /// 軌道パーティクルプレファブを設定
    /// </summary>
    public void SetOrbitParticle(GameObject particlePrefab)
    {
        orbitParticlePrefab = particlePrefab;
        CreateOrbitParticle(); // 軌道パーティクルを生成
    }
    
    /// <summary>
    /// 軌道パーティクルを生成
    /// </summary>
    private void CreateOrbitParticle()
    {
        if (orbitParticlePrefab != null)
        {
            // 軌道パーティクルを星の位置に生成
            orbitParticleInstance = Instantiate(orbitParticlePrefab, transform.position, Quaternion.identity);
            
            // パーティクルを星より背面に配置
            Vector3 particlePosition = transform.position;
            particlePosition.z = transform.position.z + 0.1f; // 星より後ろに配置
            orbitParticleInstance.transform.position = particlePosition;
            
            // パーティクルのSortingOrderを星より小さく設定
            ParticleSystem particleSystem = orbitParticleInstance.GetComponent<ParticleSystem>();
            if (particleSystem != null)
            {
                var renderer = particleSystem.GetComponent<ParticleSystemRenderer>();
                if (renderer != null)
                {
                    renderer.sortingOrder = spriteRenderer.sortingOrder - 1; // 星より背面
                }
                
                // パーティクルシステムを再生
                particleSystem.Play();
                Debug.Log("軌道パーティクルを生成し、再生を開始しました。");
            }
            else
            {
                Debug.LogWarning("軌道パーティクルプレファブにParticleSystemコンポーネントが見つかりません。");
            }
        }
    }
    
    /// <summary>
    /// 軌道パーティクルの位置を更新
    /// </summary>
    private void UpdateOrbitParticle()
    {
        if (orbitParticleInstance != null)
        {
            // 軌道パーティクルの位置を星の位置に合わせて更新
            Vector3 particlePosition = transform.position;
            particlePosition.z = transform.position.z + 0.1f; // 星より後ろに配置
            orbitParticleInstance.transform.position = particlePosition;
        }
    }
    
    /// <summary>
    /// 星のスプライトを手動で設定（Inspector用）
    /// </summary>
    public void SetSprite(Sprite sprite)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = sprite;
        }
    }
}
