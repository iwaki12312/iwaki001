using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 星の生成と管理を行うクラス
/// </summary>
public class StarManager : MonoBehaviour
{
    [Header("星の生成設定")]
    [SerializeField] private int maxStars = 5;
    [SerializeField] private float minSpawnInterval = 1.0f;
    [SerializeField] private float maxSpawnInterval = 2.0f;
    [SerializeField] private Sprite[] starSprites;
    
    [Header("巨大スター設定")]
    [SerializeField] private Sprite bigStarSprite;
    [SerializeField] private float bigStarSpawnChance = 0.1f; // 10%の確率
    
    [Header("生成範囲設定")]
    [SerializeField] private float spawnMargin = 1.0f; // 画面端からのマージン
    
    [Header("エフェクト設定")]
    [SerializeField] private GameObject starDisappearParticle;
    [SerializeField] private GameObject starOrbitParticle;
    [SerializeField] private GameObject bigStarDisappearParticle;
    [SerializeField] private GameObject bigStarOrbitParticle;
    
    private List<GameObject> activeStars = new List<GameObject>();
    private Camera mainCamera;
    private Coroutine spawnCoroutine;
    private bool isBigStarActive = false; // 巨大スターが出現中かどうか
    
    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("メインカメラが見つかりません！");
            return;
        }
        
        // 星のスプライトを自動的にロード
        LoadStarSprite();
        
        // 星の生成を開始
        StartStarSpawning();
    }
    
    void OnDestroy()
    {
        // コルーチンを停止
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }
    }
    
    /// <summary>
    /// 星のスプライトを自動的にロード
    /// </summary>
    private void LoadStarSprite()
    {
        if (starSprites == null || starSprites.Length == 0)
        {
            Debug.LogWarning("星のスプライト配列が設定されていません。TouchTheStarInitializerで設定してください。");
        }
        else
        {
            Debug.Log($"星のスプライト配列をロードしました。スプライト数: {starSprites.Length}");
        }
    }
    
    /// <summary>
    /// 星の生成を開始
    /// </summary>
    public void StartStarSpawning()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }
        
        spawnCoroutine = StartCoroutine(SpawnStarsCoroutine());
    }
    
    /// <summary>
    /// 星の生成を停止
    /// </summary>
    public void StopStarSpawning()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }
    
    /// <summary>
    /// 星を生成するコルーチン
    /// </summary>
    private IEnumerator SpawnStarsCoroutine()
    {
        while (true)
        {
            // 巨大スターが出現中でない場合のみ生成
            if (!isBigStarActive && activeStars.Count < maxStars)
            {
                SpawnStar();
            }
            
            // 次の生成まで待機
            float waitTime = Random.Range(minSpawnInterval, maxSpawnInterval);
            yield return new WaitForSeconds(waitTime);
        }
    }
    
    /// <summary>
    /// 星を生成
    /// </summary>
    private void SpawnStar()
    {
        if (mainCamera == null || starSprites == null || starSprites.Length == 0) return;
        
        // 巨大スターを生成するかどうかを判定
        bool shouldSpawnBigStar = Random.Range(0f, 1f) < bigStarSpawnChance && bigStarSprite != null;
        
        // 重なりを避けた位置を計算
        Vector3 spawnPosition = GetSafeSpawnPosition();
        
        // 星オブジェクトを作成
        GameObject starObject = new GameObject(shouldSpawnBigStar ? "BigStar" : "Star");
        starObject.transform.position = spawnPosition;
        
        // SpriteRendererを追加
        SpriteRenderer spriteRenderer = starObject.AddComponent<SpriteRenderer>();
        
        // スプライトを選択
        Sprite selectedSprite;
        if (shouldSpawnBigStar)
        {
            selectedSprite = bigStarSprite;
            // 巨大スターのサイズを0.5倍に設定（元のスプライトが通常の星より大きいため）
            starObject.transform.localScale = Vector3.one * 0.5f;
            // 巨大スターが出現中フラグを設定
            isBigStarActive = true;
        }
        else
        {
            selectedSprite = GetRandomStarSprite();
        }
        
        spriteRenderer.sprite = selectedSprite;
        spriteRenderer.sortingOrder = 2; // 星はUFOより前面に表示（UFOは1）
        
        // CircleCollider2Dを追加（Triggerに設定）
        CircleCollider2D collider = starObject.AddComponent<CircleCollider2D>();
        if (shouldSpawnBigStar)
        {
            collider.radius = 4.3f; // 巨大スター用のコライダーサイズ
        }
        else
        {
            collider.radius = 1.3f; // 通常の星のコライダーサイズ
        }
        collider.isTrigger = true; // 衝突検出用にTriggerに設定
        
        // Rigidbody2Dを追加（衝突検出のため）
        Rigidbody2D rb = starObject.AddComponent<Rigidbody2D>();
        rb.isKinematic = true; // 物理演算は使わず、衝突検出のみ
        rb.gravityScale = 0; // 重力を無効化
        
        // Starコンポーネントを追加
        Star starComponent = starObject.AddComponent<Star>();
        
        if (shouldSpawnBigStar)
        {
            // 巨大スターとして初期化
            starComponent.InitializeAsBigStar();
            starComponent.SetDisappearParticle(bigStarDisappearParticle); // 巨大スター用消滅パーティクル
            starComponent.SetOrbitParticle(bigStarOrbitParticle); // 巨大スター用軌道パーティクル
        }
        else
        {
            // 通常の星として初期化
            starComponent.SetAvailableSprites(starSprites); // 利用可能なスプライトを設定
            starComponent.SetDisappearParticle(starDisappearParticle); // 消滅パーティクルプレファブを設定
            starComponent.SetOrbitParticle(starOrbitParticle); // 軌道パーティクルプレファブを設定
        }
        
        // アクティブな星のリストに追加
        activeStars.Add(starObject);
        
        Debug.Log($"{(shouldSpawnBigStar ? "巨大スター" : "星")}を生成しました。スプライト: {selectedSprite.name}, 現在の星の数: {activeStars.Count}");
    }
    
    /// <summary>
    /// ランダムな星スプライトを取得
    /// </summary>
    private Sprite GetRandomStarSprite()
    {
        if (starSprites == null || starSprites.Length == 0) return null;
        
        // nullでないスプライトのみを選択
        List<Sprite> validSprites = new List<Sprite>();
        foreach (Sprite sprite in starSprites)
        {
            if (sprite != null)
            {
                validSprites.Add(sprite);
            }
        }
        
        if (validSprites.Count == 0) return null;
        
        int randomIndex = Random.Range(0, validSprites.Count);
        return validSprites[randomIndex];
    }
    
    /// <summary>
    /// ランダムな生成位置を取得
    /// </summary>
    private Vector3 GetRandomSpawnPosition()
    {
        if (mainCamera == null) return Vector3.zero;
        
        // 画面の境界を取得
        float cameraHeight = 2f * mainCamera.orthographicSize;
        float cameraWidth = cameraHeight * mainCamera.aspect;
        
        // マージンを考慮した生成範囲を計算
        float minX = mainCamera.transform.position.x - (cameraWidth / 2f) + spawnMargin;
        float maxX = mainCamera.transform.position.x + (cameraWidth / 2f) - spawnMargin;
        float minY = mainCamera.transform.position.y - (cameraHeight / 2f) + spawnMargin;
        float maxY = mainCamera.transform.position.y + (cameraHeight / 2f) - spawnMargin;
        
        // ランダムな位置を生成
        float randomX = Random.Range(minX, maxX);
        float randomY = Random.Range(minY, maxY);
        
        return new Vector3(randomX, randomY, 0);
    }
    
    /// <summary>
    /// 他の星と重ならない安全な生成位置を取得
    /// </summary>
    private Vector3 GetSafeSpawnPosition()
    {
        const int maxAttempts = 20; // 最大試行回数
        const float minDistanceFromOthers = 3.0f; // 他の星からの最小距離（コライダー半径1.3×2+余裕）
        
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            Vector3 candidatePosition = GetRandomSpawnPosition();
            bool isSafe = true;
            
            // 既存の星との距離をチェック
            foreach (GameObject existingStar in activeStars)
            {
                if (existingStar != null)
                {
                    float distance = Vector3.Distance(candidatePosition, existingStar.transform.position);
                    if (distance < minDistanceFromOthers)
                    {
                        isSafe = false;
                        break;
                    }
                }
            }
            
            if (isSafe)
            {
                Debug.Log($"安全な生成位置を見つけました（試行回数: {attempt + 1}）");
                return candidatePosition;
            }
        }
        
        // 安全な位置が見つからない場合は通常のランダム位置を返す
        Debug.LogWarning("安全な生成位置が見つかりませんでした。通常のランダム位置を使用します。");
        return GetRandomSpawnPosition();
    }
    
    /// <summary>
    /// 星が破棄されたときに呼び出される
    /// </summary>
    public void OnStarDestroyed()
    {
        // 破棄された星をリストから削除
        activeStars.RemoveAll(star => star == null);
        
        Debug.Log($"星が破棄されました。現在の星の数: {activeStars.Count}");
    }
    
    /// <summary>
    /// 巨大スターが破棄されたときに呼び出される
    /// </summary>
    public void OnBigStarDestroyed()
    {
        // 巨大スターが出現中フラグをリセット
        isBigStarActive = false;
        
        // 破棄された星をリストから削除
        activeStars.RemoveAll(star => star == null);
        
        Debug.Log($"巨大スターが破棄されました。通常の星の生成を再開します。現在の星の数: {activeStars.Count}");
    }
    
    /// <summary>
    /// 現在の星の数を取得
    /// </summary>
    public int GetActiveStarCount()
    {
        // nullの星を除去
        activeStars.RemoveAll(star => star == null);
        return activeStars.Count;
    }
    
    /// <summary>
    /// すべての星を削除
    /// </summary>
    public void ClearAllStars()
    {
        foreach (GameObject star in activeStars)
        {
            if (star != null)
            {
                Destroy(star);
            }
        }
        
        activeStars.Clear();
        Debug.Log("すべての星を削除しました。");
    }
    
    /// <summary>
    /// 星のスプライト配列を手動で設定（Inspector用）
    /// </summary>
    public void SetStarSprites(Sprite[] sprites)
    {
        starSprites = sprites;
    }
    
    /// <summary>
    /// 星の消滅パーティクルプレファブを手動で設定（Inspector用）
    /// </summary>
    public void SetStarDisappearParticle(GameObject particlePrefab)
    {
        starDisappearParticle = particlePrefab;
    }
    
    /// <summary>
    /// 星の軌道パーティクルプレファブを手動で設定（Inspector用）
    /// </summary>
    public void SetStarOrbitParticle(GameObject particlePrefab)
    {
        starOrbitParticle = particlePrefab;
    }
    
    /// <summary>
    /// 巨大スターのスプライトを手動で設定（Inspector用）
    /// </summary>
    public void SetBigStarSprite(Sprite sprite)
    {
        bigStarSprite = sprite;
    }
    
    /// <summary>
    /// 巨大スターの消滅パーティクルプレファブを手動で設定（Inspector用）
    /// </summary>
    public void SetBigStarDisappearParticle(GameObject particlePrefab)
    {
        bigStarDisappearParticle = particlePrefab;
    }
    
    /// <summary>
    /// 巨大スターの軌道パーティクルプレファブを手動で設定（Inspector用）
    /// </summary>
    public void SetBigStarOrbitParticle(GameObject particlePrefab)
    {
        bigStarOrbitParticle = particlePrefab;
    }
    
    /// <summary>
    /// 生成設定を変更
    /// </summary>
    public void SetSpawnSettings(int maxStarCount, float minInterval, float maxInterval)
    {
        maxStars = maxStarCount;
        minSpawnInterval = minInterval;
        maxSpawnInterval = maxInterval;
    }
}
