using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// UFOオブジェクトの挙動を制御するクラス
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class UFO : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Vector2 moveDirection;
    private float moveSpeed;
    private Camera mainCamera;
    private AudioSource audioSource;
    private bool isDestroyed = false;
    private GameObject disappearParticlePrefab; // 消滅時のパーティクルプレファブ
    
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        mainCamera = Camera.main;
        
        // AudioSourceを追加（出現効果音の継続再生用）
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true; // ループ再生
        audioSource.volume = 0.5f; // 音量を調整
    }
    
    void Start()
    {
        // 出現効果音を再生
        PlayUFOAppearSound();
    }
    
    void Update()
    {
        if (!isDestroyed)
        {
            MoveUFO();
            CheckBounds();
            CheckMouseClick();
        }
    }
    
    /// <summary>
    /// UFOの初期化
    /// </summary>
    /// <param name="direction">移動方向</param>
    /// <param name="speed">移動速度</param>
    public void Initialize(Vector2 direction, float speed)
    {
        moveDirection = direction.normalized;
        moveSpeed = speed;
        
        Debug.Log($"UFOを初期化しました。方向: {moveDirection}, 速度: {moveSpeed}");
    }
    
    /// <summary>
    /// UFOの移動処理
    /// </summary>
    private void MoveUFO()
    {
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime);
    }
    
    /// <summary>
    /// 画面外判定
    /// </summary>
    private void CheckBounds()
    {
        if (mainCamera == null) return;
        
        Vector3 viewportPosition = mainCamera.WorldToViewportPoint(transform.position);
        
        // 画面外に出た場合はUFOを破棄（効果音なし）
        if (viewportPosition.x < -0.2f || viewportPosition.x > 1.2f ||
            viewportPosition.y < -0.2f || viewportPosition.y > 1.2f)
        {
            DestroyUFO(false); // 画面外に出た場合は効果音なし
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
        Debug.Log("UFO: TouchManagerからタップされました！");
        DestroyUFO(true); // タップされた場合は効果音あり
    }
    
    /// <summary>
    /// UFO出現効果音を再生
    /// </summary>
    private void PlayUFOAppearSound()
    {
        if (TouchTheStarSFXPlayer.Instance != null && audioSource != null)
        {
            // TouchTheStarSFXPlayerから効果音クリップを取得してAudioSourceで継続再生
            AudioClip ufoAppearClip = GetUFOAppearClip();
            if (ufoAppearClip != null)
            {
                audioSource.clip = ufoAppearClip;
                audioSource.loop = true;
                audioSource.volume = 0.3f; // 音量を少し下げる
                audioSource.Play();
                Debug.Log("UFO出現効果音の継続再生を開始しました。");
            }
            else
            {
                // フォールバック：TouchTheStarSFXPlayerで一度だけ再生
                TouchTheStarSFXPlayer.Instance.PlayUFOAppearSound();
                Debug.LogWarning("UFO出現効果音クリップが取得できませんでした。一度だけ再生します。");
            }
        }
    }
    
    /// <summary>
    /// UFO出現効果音クリップを取得（リフレクションを使用）
    /// </summary>
    private AudioClip GetUFOAppearClip()
    {
        if (TouchTheStarSFXPlayer.Instance == null) return null;
        
        // リフレクションを使ってprivateフィールドにアクセス
        var field = typeof(TouchTheStarSFXPlayer).GetField("ufoAppearSound", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (field != null)
        {
            return field.GetValue(TouchTheStarSFXPlayer.Instance) as AudioClip;
        }
        
        return null;
    }
    
    /// <summary>
    /// 消滅時のパーティクルプレファブを設定
    /// </summary>
    public void SetDisappearParticle(GameObject particlePrefab)
    {
        disappearParticlePrefab = particlePrefab;
    }
    
    /// <summary>
    /// UFOを破棄する
    /// </summary>
    /// <param name="playSound">効果音を再生するかどうか</param>
    private void DestroyUFO(bool playSound)
    {
        if (isDestroyed) return; // 既に破棄処理中の場合は何もしない
        
        isDestroyed = true;
        
        // 出現効果音を停止
        if (audioSource != null)
        {
            audioSource.Stop();
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
                
                Debug.Log($"UFO消滅パーティクルエフェクトを再生しました。{particleDuration}秒後に削除されます。");
            }
            else
            {
                // ParticleSystemがない場合は5秒後に削除
                Destroy(particleInstance, 5f);
                Debug.LogWarning("UFO消滅パーティクルプレファブにParticleSystemコンポーネントが見つかりません。");
            }
        }
        
        // タップされた場合は消滅効果音を再生
        if (playSound && TouchTheStarSFXPlayer.Instance != null)
        {
            TouchTheStarSFXPlayer.Instance.PlayUFODisappearSound();
        }
        
        // UFOManagerにUFOが破棄されたことを通知
        UFOManager ufoManager = FindObjectOfType<UFOManager>();
        if (ufoManager != null)
        {
            ufoManager.OnUFODestroyed();
        }
        
        Debug.Log($"UFOが破棄されました。効果音再生: {playSound}");
        Destroy(gameObject);
    }
    
    void OnDestroy()
    {
        // AudioSourceを停止
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }
}
