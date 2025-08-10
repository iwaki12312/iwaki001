using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// UFOの生成と管理を行うクラス
/// </summary>
public class UFOManager : MonoBehaviour
{
    [Header("UFOの生成設定")]
    [SerializeField] private int maxUFOs = 3; // 同時に存在できるUFOの最大数
    [SerializeField] private float initialDelay = 6.0f; // ゲーム開始後の初期遅延時間
    [SerializeField] private float minSpawnInterval = 10.0f; // 最小生成間隔
    [SerializeField] private float maxSpawnInterval = 20.0f; // 最大生成間隔
    [SerializeField] private Sprite[] ufoSprites; // UFOのスプライト配列（3色）
    
    [Header("UFOの移動設定")]
    [SerializeField] private float ufoSpeed = 3.0f; // UFOの移動速度
    
    [Header("エフェクト設定")]
    [SerializeField] private GameObject ufoDisappearParticle; // UFO消滅パーティクルプレファブ
    
    private List<GameObject> activeUFOs = new List<GameObject>();
    private Camera mainCamera;
    private Coroutine spawnCoroutine;
    
    // UFOの出現パターン
    public enum UFOSpawnPattern
    {
        LeftTopToRightBottom,    // 左上から右下
        LeftBottomToRightTop,    // 左下から右上
        RightTopToLeftBottom,    // 右上から左下
        RightBottomToLeftTop     // 右下から左上
    }
    
    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("メインカメラが見つかりません！");
            return;
        }
        
        // UFOのスプライトを自動的にロード
        LoadUFOSprites();
        
        // UFOの生成を開始
        StartUFOSpawning();
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
    /// UFOのスプライトを自動的にロード
    /// </summary>
    private void LoadUFOSprites()
    {
        if (ufoSprites == null || ufoSprites.Length == 0)
        {
            Debug.LogWarning("UFOのスプライト配列が設定されていません。TouchTheStarInitializerで設定してください。");
        }
        else
        {
            Debug.Log($"UFOのスプライト配列をロードしました。スプライト数: {ufoSprites.Length}");
        }
    }
    
    /// <summary>
    /// UFOの生成を開始
    /// </summary>
    public void StartUFOSpawning()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }
        
        spawnCoroutine = StartCoroutine(SpawnUFOsCoroutine());
    }
    
    /// <summary>
    /// UFOの生成を停止
    /// </summary>
    public void StopUFOSpawning()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }
    
    /// <summary>
    /// UFOを生成するコルーチン
    /// </summary>
    private IEnumerator SpawnUFOsCoroutine()
    {
        // ゲーム開始直後は指定された時間だけ待機
        Debug.Log($"UFO生成開始まで{initialDelay}秒待機します。");
        yield return new WaitForSeconds(initialDelay);
        
        Debug.Log("UFO生成を開始します。");
        
        while (true)
        {
            // 最大数に達していない場合のみ生成
            if (activeUFOs.Count < maxUFOs)
            {
                SpawnUFO();
            }
            
            // 次の生成まで待機
            float waitTime = Random.Range(minSpawnInterval, maxSpawnInterval);
            yield return new WaitForSeconds(waitTime);
        }
    }
    
    /// <summary>
    /// UFOを生成
    /// </summary>
    private void SpawnUFO()
    {
        if (mainCamera == null || ufoSprites == null || ufoSprites.Length == 0) return;
        
        // ランダムな出現パターンを選択
        UFOSpawnPattern pattern = (UFOSpawnPattern)Random.Range(0, 4);
        
        // 出現位置と移動方向を計算
        Vector3 spawnPosition;
        Vector2 moveDirection;
        GetSpawnPositionAndDirection(pattern, out spawnPosition, out moveDirection);
        
        // UFOオブジェクトを作成
        GameObject ufoObject = new GameObject("UFO");
        ufoObject.transform.position = spawnPosition;
        
        // SpriteRendererを追加
        SpriteRenderer spriteRenderer = ufoObject.AddComponent<SpriteRenderer>();
        
        // ランダムなスプライトを選択
        Sprite randomSprite = GetRandomUFOSprite();
        spriteRenderer.sprite = randomSprite;
        spriteRenderer.sortingOrder = 0; // 星より背面に表示（星のsortingOrderは1）
        
        // UFOのサイズを小さくする（半分のサイズ）
        ufoObject.transform.localScale = Vector3.one * 0.5f;
        
        // CircleCollider2Dを追加（Triggerに設定）
        CircleCollider2D collider = ufoObject.AddComponent<CircleCollider2D>();
        collider.radius = 1.0f; // スケールが0.5なので実際の半径は0.5f
        collider.isTrigger = true; // 衝突検出用にTriggerに設定
        
        // Rigidbody2Dを追加（衝突検出のため）
        Rigidbody2D rb = ufoObject.AddComponent<Rigidbody2D>();
        rb.isKinematic = true; // 物理演算は使わず、衝突検出のみ
        rb.gravityScale = 0; // 重力を無効化
        
        // UFOコンポーネントを追加
        UFO ufoComponent = ufoObject.AddComponent<UFO>();
        ufoComponent.Initialize(moveDirection, ufoSpeed);
        ufoComponent.SetDisappearParticle(ufoDisappearParticle); // 消滅パーティクルプレファブを設定
        
        // アクティブなUFOのリストに追加
        activeUFOs.Add(ufoObject);
        
        Debug.Log($"UFOを生成しました。パターン: {pattern}, スプライト: {randomSprite.name}, 現在のUFO数: {activeUFOs.Count}");
    }
    
    /// <summary>
    /// 出現パターンに基づいて出現位置と移動方向を計算
    /// </summary>
    private void GetSpawnPositionAndDirection(UFOSpawnPattern pattern, out Vector3 spawnPosition, out Vector2 moveDirection)
    {
        // 画面の境界を取得
        float cameraHeight = 2f * mainCamera.orthographicSize;
        float cameraWidth = cameraHeight * mainCamera.aspect;
        
        float leftEdge = mainCamera.transform.position.x - (cameraWidth / 2f);
        float rightEdge = mainCamera.transform.position.x + (cameraWidth / 2f);
        float topEdge = mainCamera.transform.position.y + (cameraHeight / 2f);
        float bottomEdge = mainCamera.transform.position.y - (cameraHeight / 2f);
        
        // 画面外からの出現位置を設定
        float margin = 2.0f; // 画面外のマージン
        
        switch (pattern)
        {
            case UFOSpawnPattern.LeftTopToRightBottom:
                spawnPosition = new Vector3(leftEdge - margin, topEdge + margin, 0);
                moveDirection = new Vector2(1, -1).normalized;
                break;
                
            case UFOSpawnPattern.LeftBottomToRightTop:
                spawnPosition = new Vector3(leftEdge - margin, bottomEdge - margin, 0);
                moveDirection = new Vector2(1, 1).normalized;
                break;
                
            case UFOSpawnPattern.RightTopToLeftBottom:
                spawnPosition = new Vector3(rightEdge + margin, topEdge + margin, 0);
                moveDirection = new Vector2(-1, -1).normalized;
                break;
                
            case UFOSpawnPattern.RightBottomToLeftTop:
                spawnPosition = new Vector3(rightEdge + margin, bottomEdge - margin, 0);
                moveDirection = new Vector2(-1, 1).normalized;
                break;
                
            default:
                spawnPosition = Vector3.zero;
                moveDirection = Vector2.right;
                break;
        }
    }
    
    /// <summary>
    /// ランダムなUFOスプライトを取得
    /// </summary>
    private Sprite GetRandomUFOSprite()
    {
        if (ufoSprites == null || ufoSprites.Length == 0) return null;
        
        // nullでないスプライトのみを選択
        List<Sprite> validSprites = new List<Sprite>();
        foreach (Sprite sprite in ufoSprites)
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
    /// UFOが破棄されたときに呼び出される
    /// </summary>
    public void OnUFODestroyed()
    {
        // 破棄されたUFOをリストから削除
        activeUFOs.RemoveAll(ufo => ufo == null);
        
        Debug.Log($"UFOが破棄されました。現在のUFO数: {activeUFOs.Count}");
    }
    
    /// <summary>
    /// 現在のUFOの数を取得
    /// </summary>
    public int GetActiveUFOCount()
    {
        // nullのUFOを除去
        activeUFOs.RemoveAll(ufo => ufo == null);
        return activeUFOs.Count;
    }
    
    /// <summary>
    /// すべてのUFOを削除
    /// </summary>
    public void ClearAllUFOs()
    {
        foreach (GameObject ufo in activeUFOs)
        {
            if (ufo != null)
            {
                Destroy(ufo);
            }
        }
        
        activeUFOs.Clear();
        Debug.Log("すべてのUFOを削除しました。");
    }
    
    /// <summary>
    /// UFOのスプライト配列を手動で設定（Inspector用）
    /// </summary>
    public void SetUFOSprites(Sprite[] sprites)
    {
        ufoSprites = sprites;
    }
    
    /// <summary>
    /// UFO消滅パーティクルプレファブを手動で設定（Inspector用）
    /// </summary>
    public void SetUFODisappearParticle(GameObject particlePrefab)
    {
        ufoDisappearParticle = particlePrefab;
    }
    
    /// <summary>
    /// 生成設定を変更
    /// </summary>
    public void SetSpawnSettings(int maxUFOCount, float minInterval, float maxInterval, float speed)
    {
        maxUFOs = maxUFOCount;
        minSpawnInterval = minInterval;
        maxSpawnInterval = maxInterval;
        ufoSpeed = speed;
    }
}
