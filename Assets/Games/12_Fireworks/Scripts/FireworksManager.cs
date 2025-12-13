using System.Collections;
using UnityEngine;

public class FireworksManager : MonoBehaviour
{
    [Header("参照 (Inspectorで設定可 / 未設定でも動作)")]
    [SerializeField] private Sprite rocketSprite;
    [SerializeField] private Sprite[] explosionSprites;
    [SerializeField] private Sprite shootingStarSprite;
    [SerializeField] private FireworksSFXPlayer sfxPlayer;

    [Header("通常スポーン")]
    [SerializeField] private int maxRockets = 3;
    [SerializeField] private float minSpawnInterval = 0.5f;
    [SerializeField] private float maxSpawnInterval = 1.2f;
    [SerializeField] private float minRocketSpeed = 2.5f;
    [SerializeField] private float maxRocketSpeed = 4.2f;

    [Header("ロケット挙動（上に行くほど減速→停止→フェードアウト）")]
    [SerializeField, Range(0.1f, 0.95f)] private float slowdownStartViewportY = 0.55f;
    [SerializeField, Range(0.2f, 1.0f)] private float stopViewportY = 0.90f;
    [SerializeField] private float rocketFadeOutDuration = 0.55f;
    [SerializeField, Min(0f)] private float fadeOutStartSpeedThreshold = 0.02f;

    [Header("爆発（スプライト1枚）")]
    [SerializeField] private float explosionDuration = 0.45f;
    [SerializeField] private float explosionScaleStart = 1.0f;
    [SerializeField] private float explosionScaleEnd = 1.7f;

    [Header("レア: 巨大花火")]
    [SerializeField] private float giantChance = 0.03f;

    [Header("レア: 流れ星")]
    [SerializeField] private float shootingStarChancePerCheck = 0.08f;
    [SerializeField] private float shootingStarCheckInterval = 6.0f;
    [SerializeField] private float shootingStarMinDuration = 0.9f;
    [SerializeField] private float shootingStarMaxDuration = 1.5f;

    [Header("レア: スターマイン（一定時間ごと）")]
    [SerializeField] private float starMineInterval = 30f;
    [SerializeField] private float starMineDuration = 2.5f;
    [SerializeField] private float starMineRocketInterval = 0.08f;

    private Camera cam;
    private int aliveRockets;
    private bool isStarMineActive;

    public void SetAssets(Sprite rocketSprite, Sprite[] explosionSprites, Sprite shootingStarSprite, FireworksSFXPlayer sfxPlayer)
    {
        this.rocketSprite = rocketSprite;
        this.explosionSprites = SanitizeSprites(explosionSprites);
        this.shootingStarSprite = shootingStarSprite;
        this.sfxPlayer = sfxPlayer;
    }

    private void Start()
    {
        cam = Camera.main;

        if (sfxPlayer == null)
        {
            sfxPlayer = FindObjectOfType<FireworksSFXPlayer>();
        }

        // 未設定でも遊べるように簡易スプライトを生成
        if (rocketSprite == null)
        {
            rocketSprite = FireworksRuntimeSpriteFactory.CreateSolidSprite(Color.white);
        }

        explosionSprites = SanitizeSprites(explosionSprites);
        if (explosionSprites == null || explosionSprites.Length == 0)
        {
            explosionSprites = new[] { FireworksRuntimeSpriteFactory.CreateSolidSprite(new Color(1f, 0.9f, 0.5f, 1f)) };
        }

        if (shootingStarSprite == null)
        {
            shootingStarSprite = FireworksRuntimeSpriteFactory.CreateSolidSprite(new Color(0.8f, 0.9f, 1f, 1f));
        }

        StartCoroutine(RocketLoop());
        StartCoroutine(ShootingStarLoop());
        StartCoroutine(StarMineLoop());
    }

    private static Sprite[] SanitizeSprites(Sprite[] sprites)
    {
        if (sprites == null || sprites.Length == 0)
        {
            return sprites;
        }

        int count = 0;
        for (int i = 0; i < sprites.Length; i++)
        {
            if (sprites[i] != null) count++;
        }

        if (count == sprites.Length)
        {
            return sprites;
        }

        var result = new Sprite[count];
        int idx = 0;
        for (int i = 0; i < sprites.Length; i++)
        {
            if (sprites[i] == null) continue;
            result[idx++] = sprites[i];
        }

        return result;
    }

    private void Update()
    {
        HandleTouches();
    }

    private void HandleTouches()
    {
        if (cam == null)
        {
            cam = Camera.main;
            if (cam == null) return;
        }

        // タッチ
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch t = Input.GetTouch(i);
            if (t.phase != TouchPhase.Began) continue;

            Vector2 world = cam.ScreenToWorldPoint(t.position);
            TryTapAt(world);
        }

#if UNITY_EDITOR || UNITY_STANDALONE
        // エディタ確認用（クリック）
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 world = cam.ScreenToWorldPoint(Input.mousePosition);
            TryTapAt(world);
        }
#endif
    }

    private void TryTapAt(Vector2 world)
    {
        Collider2D hit = Physics2D.OverlapPoint(world);
        if (hit == null)
        {
            return;
        }

        FireworksTapTarget target = hit.GetComponentInParent<FireworksTapTarget>();
        if (target == null)
        {
            return;
        }

        target.OnTapped();
    }

    private IEnumerator RocketLoop()
    {
        while (true)
        {
            if (!isStarMineActive && aliveRockets < maxRockets)
            {
                SpawnRocket(isGiant: Random.value < giantChance);
            }

            float wait = Random.Range(minSpawnInterval, maxSpawnInterval);
            yield return new WaitForSeconds(wait);
        }
    }

    private void SpawnRocket(bool isGiant)
    {
        if (cam == null)
        {
            cam = Camera.main;
            if (cam == null) return;
        }

        Vector2 bottomLeft = cam.ViewportToWorldPoint(new Vector3(0f, 0f, 0f));
        Vector2 topRight = cam.ViewportToWorldPoint(new Vector3(1f, 1f, 0f));

        float x = Random.Range(bottomLeft.x + 0.5f, topRight.x - 0.5f);
        float y = bottomLeft.y - 1.0f;

        var obj = new GameObject(isGiant ? "Rocket_Giant" : "Rocket");
        obj.transform.position = new Vector3(x, y, 0f);

        var rocket = obj.AddComponent<FireworksRocket>();
        float speed = Random.Range(minRocketSpeed, maxRocketSpeed) * (isGiant ? 0.85f : 1.0f);

        float slowStartY = cam.ViewportToWorldPoint(new Vector3(0f, slowdownStartViewportY, 0f)).y;
        float stopY = cam.ViewportToWorldPoint(new Vector3(0f, Mathf.Max(stopViewportY, slowdownStartViewportY + 0.05f), 0f)).y;
        rocket.Initialize(
            this,
            rocketSprite,
            speed,
            isGiant,
            slowStartY,
            stopY,
            rocketFadeOutDuration,
            fadeOutStartSpeedThreshold,
            sfxPlayer,
            0.6f
        );

        aliveRockets++;

        // 破棄時にカウントを戻す
        obj.AddComponent<FireworksOnDestroyCallback>().Init(() => aliveRockets--);
    }

    public void SpawnExplosion(Vector3 position, float scaleMultiplier, bool isGiant, bool tapped)
    {
        Sprite sprite = explosionSprites[Random.Range(0, explosionSprites.Length)];

        var obj = new GameObject(isGiant ? "Explosion_Giant" : "Explosion");
        obj.transform.position = new Vector3(position.x, position.y, 0f);

        var explosion = obj.AddComponent<FireworksExplosion>();
        float duration = isGiant ? explosionDuration * 1.2f : explosionDuration;

        float start = explosionScaleStart * scaleMultiplier;
        float end = explosionScaleEnd * scaleMultiplier;

        float rot = Random.Range(-90f, 90f);
        explosion.Initialize(sprite, duration, start, end, 1f, 0f, rot);

        sfxPlayer?.PlayExplosion(isGiant ? 1.0f : 0.8f);
    }

    private IEnumerator ShootingStarLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(shootingStarCheckInterval);

            if (isStarMineActive)
            {
                continue;
            }

            if (Random.value > shootingStarChancePerCheck)
            {
                continue;
            }

            SpawnShootingStar();
        }
    }

    private void SpawnShootingStar()
    {
        if (cam == null)
        {
            cam = Camera.main;
            if (cam == null) return;
        }

        Vector2 topLeft = cam.ViewportToWorldPoint(new Vector3(0f, 1f, 0f));
        Vector2 bottomRight = cam.ViewportToWorldPoint(new Vector3(1f, 0f, 0f));

        // 画面外から斜めに流す
        Vector3 start = new Vector3(topLeft.x - 1.0f, topLeft.y + 0.5f, 0f);
        Vector3 end = new Vector3(bottomRight.x + 1.0f, bottomRight.y - 0.5f, 0f);

        float duration = Random.Range(shootingStarMinDuration, shootingStarMaxDuration);
        Vector3 vel = (end - start) / Mathf.Max(0.01f, duration);

        var obj = new GameObject("ShootingStar");
        obj.transform.position = start;

        var star = obj.AddComponent<FireworksShootingStar>();
        star.Initialize(this, shootingStarSprite, vel);

        sfxPlayer?.PlayShootingStar(0.7f);
    }

    public void OnShootingStarTapped(Vector3 position)
    {
        // 「キラッ」: 小さめ爆発を短時間で
        SpawnExplosion(position, 0.9f, isGiant: false, tapped: true);
        sfxPlayer?.PlayShootingStar(1.0f);
    }

    private IEnumerator StarMineLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(starMineInterval);
            if (isStarMineActive)
            {
                continue;
            }

            yield return StartCoroutine(DoStarMine());
        }
    }

    private IEnumerator DoStarMine()
    {
        isStarMineActive = true;
        sfxPlayer?.PlayStarMineStart(1.0f);

        float elapsed = 0f;
        while (elapsed < starMineDuration)
        {
            SpawnRocket(isGiant: Random.value < 0.12f);
            yield return new WaitForSeconds(starMineRocketInterval);
            elapsed += starMineRocketInterval;
        }

        isStarMineActive = false;
    }

    public bool IsAboveTop(Vector3 position)
    {
        if (cam == null)
        {
            cam = Camera.main;
            if (cam == null) return false;
        }

        Vector2 top = cam.ViewportToWorldPoint(new Vector3(0f, 1f, 0f));
        return position.y > top.y + 1.0f;
    }

    public bool IsOutsideScreen(Vector3 position, float margin)
    {
        if (cam == null)
        {
            cam = Camera.main;
            if (cam == null) return false;
        }

        Vector2 bl = cam.ViewportToWorldPoint(new Vector3(0f, 0f, 0f));
        Vector2 tr = cam.ViewportToWorldPoint(new Vector3(1f, 1f, 0f));

        return position.x < bl.x - margin || position.x > tr.x + margin || position.y < bl.y - margin || position.y > tr.y + margin;
    }
}

public class FireworksOnDestroyCallback : MonoBehaviour
{
    private System.Action onDestroy;

    public void Init(System.Action onDestroy)
    {
        this.onDestroy = onDestroy;
    }

    private void OnDestroy()
    {
        onDestroy?.Invoke();
    }
}

public static class FireworksRuntimeSpriteFactory
{
    public static Sprite CreateSolidSprite(Color color)
    {
        var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        tex.SetPixels(new[] { color, color, color, color });
        tex.Apply();
        tex.filterMode = FilterMode.Point;

        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
    }
}
