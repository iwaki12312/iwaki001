using System.Collections;
using UnityEngine;

/// <summary>
/// 海の生き物のデータ定義
/// </summary>
public enum SeaCreatureType
{
    Clownfish, Angelfish, Octopus, SeaTurtle,
    Pufferfish, Dolphin, Seahorse, Shark,
    WhaleShark, Manta
}

/// <summary>
/// 海の生き物1体を制御するコンポーネント
/// </summary>
public class SeaCreatureController : MonoBehaviour
{
    public SeaCreatureType creatureType;
    public bool isRare;

    private SpriteRenderer spriteRenderer;
    private CircleCollider2D circleCollider;
    private Vector2 swimDirection;
    private float swimSpeed;
    private float swimAmplitude;   // 上下ゆらぎ振幅
    private float swimFrequency;   // 上下ゆらぎ周波数
    private float baseY;
    private float elapsedTime;
    private bool isTapReacting = false;
    private bool isSwimmingAway = false;
    private bool isSpawning = true;
    private Vector3 targetScale;
    private Camera mainCamera;

    // 泳ぎ範囲
    private float minX = -6f;
    private float maxX = 6f;
    private float minY = -3.5f;
    private float maxY = 3f;

    public void Initialize(SeaCreatureType type, Sprite sprite, bool rare, float colliderRadius)
    {
        creatureType = type;
        isRare = rare;
        mainCamera = Camera.main;

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
        spriteRenderer.sortingOrder = rare ? 25 : 15;

        circleCollider = GetComponent<CircleCollider2D>();
        if (circleCollider == null)
            circleCollider = gameObject.AddComponent<CircleCollider2D>();
        circleCollider.radius = colliderRadius;

        // 泳ぎパラメータを生き物タイプ別に設定
        SetSwimParameters();

        baseY = transform.position.y;
        elapsedTime = Random.Range(0f, 10f); // 位相をずらす
    }

    private void SetSwimParameters()
    {
        switch (creatureType)
        {
            case SeaCreatureType.Clownfish:
                swimSpeed = Random.Range(1.2f, 1.8f);
                swimAmplitude = 0.3f;
                swimFrequency = 3f;
                swimDirection = Random.value < 0.5f ? Vector2.right : Vector2.left;
                break;
            case SeaCreatureType.Angelfish:
                swimSpeed = Random.Range(0.6f, 1.0f);
                swimAmplitude = 0.5f;
                swimFrequency = 1.5f;
                swimDirection = Random.value < 0.5f ? Vector2.right : Vector2.left;
                break;
            case SeaCreatureType.Octopus:
                swimSpeed = Random.Range(0.3f, 0.6f);
                swimAmplitude = 0.8f;
                swimFrequency = 0.8f;
                swimDirection = new Vector2(Random.Range(-1f, 1f), 0).normalized;
                break;
            case SeaCreatureType.SeaTurtle:
                swimSpeed = Random.Range(0.5f, 0.8f);
                swimAmplitude = 0.4f;
                swimFrequency = 1.0f;
                swimDirection = Random.value < 0.5f ? Vector2.right : Vector2.left;
                break;
            case SeaCreatureType.Pufferfish:
                swimSpeed = Random.Range(0.2f, 0.5f);
                swimAmplitude = 0.6f;
                swimFrequency = 0.6f;
                swimDirection = new Vector2(Random.Range(-0.5f, 0.5f), 0).normalized;
                break;
            case SeaCreatureType.Dolphin:
                swimSpeed = Random.Range(1.0f, 1.5f);
                swimAmplitude = 0.5f;
                swimFrequency = 1.5f;
                swimDirection = Random.value < 0.5f ? Vector2.right : Vector2.left;
                break;
            case SeaCreatureType.Seahorse:
                swimSpeed = Random.Range(0.1f, 0.2f);
                swimAmplitude = 0.4f;
                swimFrequency = 1.2f;
                swimDirection = Vector2.zero;
                break;
            case SeaCreatureType.Shark:
                swimSpeed = Random.Range(0.8f, 1.2f);
                swimAmplitude = 0.2f;
                swimFrequency = 0.8f;
                swimDirection = Random.value < 0.5f ? Vector2.right : Vector2.left;
                break;
            case SeaCreatureType.WhaleShark:
                swimSpeed = Random.Range(0.8f, 1.2f);
                swimAmplitude = 0.3f;
                swimFrequency = 0.5f;
                swimDirection = Random.value < 0.5f ? Vector2.right : Vector2.left;
                break;
            case SeaCreatureType.Manta:
                swimSpeed = Random.Range(0.7f, 1.0f);
                swimAmplitude = 0.4f;
                swimFrequency = 0.7f;
                swimDirection = Random.value < 0.5f ? Vector2.right : Vector2.left;
                break;
        }

        // スプライトの向き設定は StartSpawnAnimation で targetScale 反転時に行う
    }

    /// <summary>
    /// スポーンアニメーション開始（DOTween不使用、自己管理コルーチン）
    /// </summary>
    public void StartSpawnAnimation(Vector3 scale)
    {
        targetScale = scale;
        isSpawning = true;
        // SetSwimParametersで向きが決まった後にtargetScaleを反映
        if (swimDirection.x < 0)
            targetScale = new Vector3(-Mathf.Abs(targetScale.x), targetScale.y, targetScale.z);
        transform.localScale = Vector3.zero;
        StartCoroutine(SpawnAnimationCoroutine());
    }

    private IEnumerator SpawnAnimationCoroutine()
    {
        float duration = 0.4f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            // OutBack easing: overshoots then settles
            float c1 = 1.70158f;
            float c3 = c1 + 1f;
            float easedT = 1f + c3 * Mathf.Pow(t - 1f, 3) + c1 * Mathf.Pow(t - 1f, 2);
            transform.localScale = Vector3.LerpUnclamped(Vector3.zero, targetScale, easedT);
            yield return null;
        }

        // 最終スケールを確実に設定
        transform.localScale = targetScale;
        isSpawning = false;
    }

    /// <summary>
    /// スポーンアニメーション完了時に呼ばれる（後方互換用）
    /// </summary>
    public void OnSpawnComplete()
    {
        isSpawning = false;
        transform.localScale = targetScale;
    }

    void Update()
    {
        if (isSpawning || isTapReacting || isSwimmingAway) return;

        elapsedTime += Time.deltaTime;

        // 横移動
        Vector3 pos = transform.position;
        pos.x += swimDirection.x * swimSpeed * Time.deltaTime;

        // 上下ゆらぎ
        pos.y = baseY + Mathf.Sin(elapsedTime * swimFrequency) * swimAmplitude;

        // 画面端で反転
        if (pos.x > maxX)
        {
            swimDirection.x = -Mathf.Abs(swimDirection.x);
            FlipSprite();
        }
        else if (pos.x < minX)
        {
            swimDirection.x = Mathf.Abs(swimDirection.x);
            FlipSprite();
        }

        // Y範囲制限
        pos.y = Mathf.Clamp(pos.y, minY, maxY);

        transform.position = pos;
    }

    private void FlipSprite()
    {
        Vector3 scale = transform.localScale;
        scale.x = -scale.x;
        transform.localScale = scale;
    }

    /// <summary>
    /// タップされたか判定（ワールド座標で）
    /// </summary>
    public bool ContainsPoint(Vector2 worldPoint)
    {
        if (circleCollider == null) return false;
        return circleCollider.OverlapPoint(worldPoint);
    }

    /// <summary>
    /// タップリアクション
    /// </summary>
    public void OnTapped()
    {
        if (isTapReacting) return;
        isTapReacting = true;

        if (AquariumSFXPlayer.Instance != null)
            AquariumSFXPlayer.Instance.PlayTapSound();

        // くるっと回転リアクション
        StartCoroutine(TapReactionCoroutine());
    }

    private System.Collections.IEnumerator TapReactionCoroutine()
    {
        float duration = 0.35f;
        float elapsed = 0f;
        Vector3 originalPos = transform.position;
        Vector3 originalScale = transform.localScale;
        float jumpHeight = 0.4f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // ピョンと跳ね上がり（放物線）
            float jumpT = 1f - (2f * t - 1f) * (2f * t - 1f); // 0→1→0
            transform.position = originalPos + Vector3.up * jumpHeight * jumpT;

            // 軽いスクイッシュ（着地で潰れる感じ）
            float squash;
            if (t < 0.5f)
                squash = 1f + 0.15f * (t / 0.5f); // 伸びる
            else
                squash = 1.15f - 0.25f * ((t - 0.5f) / 0.5f); // 潰れる→戻る
            
            transform.localScale = new Vector3(
                originalScale.x * (2f - squash),
                originalScale.y * squash,
                originalScale.z
            );

            yield return null;
        }

        transform.position = originalPos;
        transform.localScale = originalScale;
        isTapReacting = false;
    }

    /// <summary>
    /// フェードアウトしながら退場
    /// </summary>
    public void SwimAway()
    {
        isSwimmingAway = true;
        StartCoroutine(SwimAwayCoroutine());
    }

    private System.Collections.IEnumerator SwimAwayCoroutine()
    {
        float fadeDuration = 0.6f;
        float elapsed = 0f;
        Color originalColor = spriteRenderer.color;
        Vector3 originalScale = transform.localScale;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;

            // フェードアウト
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f - t);

            // 少し縮小
            float scale = Mathf.Lerp(1f, 0.3f, t);
            transform.localScale = new Vector3(
                originalScale.x * scale,
                originalScale.y * scale,
                originalScale.z
            );

            yield return null;
        }

        Destroy(gameObject);
    }

    void OnDestroy()
    {
        StopAllCoroutines();
    }
}
