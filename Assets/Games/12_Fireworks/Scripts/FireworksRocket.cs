using UnityEngine;

public class FireworksRocket : FireworksTapTarget
{
    private FireworksManager manager;
    private SpriteRenderer spriteRenderer;
    private Collider2D collider2d;
    private AudioSource launchAudioSource;

    private float initialSpeed;
    private float currentSpeed;
    private float driftAmplitude;
    private float driftFrequency;

    private float baseX;
    private float driftPhase;

    private float slowStartY;
    private float stopY;
    private float fadeOutDuration;
    private float fadeOutStartSpeedThreshold;
    private float fadeOutElapsed;
    private bool isFadingOut;

    private bool isGiant;
    private bool hasExploded;

    private FireworksSFXPlayer sfxPlayer;
    private float launchVolume;

    public void Initialize(
        FireworksManager manager,
        Sprite sprite,
        float speed,
        bool isGiant,
        float slowStartY,
        float stopY,
        float fadeOutDuration,
        float fadeOutStartSpeedThreshold,
        FireworksSFXPlayer sfxPlayer,
        float launchVolume
    )
    {
        this.manager = manager;
        initialSpeed = speed;
        currentSpeed = speed;
        this.isGiant = isGiant;
        this.slowStartY = slowStartY;
        this.stopY = stopY;
        this.fadeOutDuration = Mathf.Max(0.05f, fadeOutDuration);
        this.fadeOutStartSpeedThreshold = Mathf.Max(0f, fadeOutStartSpeedThreshold);

        this.sfxPlayer = sfxPlayer;
        this.launchVolume = Mathf.Clamp01(launchVolume);

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }

        spriteRenderer.sprite = sprite;
        spriteRenderer.sortingOrder = 0;
        spriteRenderer.color = Random.ColorHSV(0f, 1f, 0.75f, 1f, 0.85f, 1f, 1f, 1f);

        collider2d = GetComponent<Collider2D>();
        if (collider2d == null)
        {
            var circle = gameObject.AddComponent<CircleCollider2D>();
            circle.isTrigger = true;
            collider2d = circle;
        }

        float scale = isGiant ? 1.8f : 1.0f;
        transform.localScale = new Vector3(scale, scale, 1f);

        // ゆらゆら
        driftAmplitude = Random.Range(0.05f, 0.18f);
        driftFrequency = Random.Range(1.0f, 2.2f);

        baseX = transform.position.x;
        driftPhase = Random.Range(0f, Mathf.PI * 2f);

        hasExploded = false;
        isFadingOut = false;
        fadeOutElapsed = 0f;

        SetAlpha(1f);

        SetupAndPlayLaunchSfx();
    }

    private void SetupAndPlayLaunchSfx()
    {
        if (sfxPlayer == null)
        {
            return;
        }

        AudioClip clip = sfxPlayer.GetRocketLaunchClip();
        if (clip == null)
        {
            return;
        }

        if (launchAudioSource == null)
        {
            launchAudioSource = gameObject.AddComponent<AudioSource>();
            launchAudioSource.playOnAwake = false;
            launchAudioSource.loop = true;
        }

        launchAudioSource.clip = clip;
        launchAudioSource.volume = Mathf.Clamp01(launchVolume * sfxPlayer.GetRocketLaunchVolume());
        launchAudioSource.Play();
    }

    private void Update()
    {
        if (hasExploded)
        {
            return;
        }

        if (isFadingOut)
        {
            fadeOutElapsed += Time.deltaTime;
            float t = Mathf.Clamp01(fadeOutElapsed / fadeOutDuration);
            SetAlpha(1f - t);

            if (fadeOutElapsed >= fadeOutDuration)
            {
                Destroy(gameObject);
            }
            return;
        }

        UpdateSpeedByHeight();

        float drift = Mathf.Sin((Time.time + driftPhase) * driftFrequency) * driftAmplitude;
        Vector3 pos = transform.position;
        pos.y += currentSpeed * Time.deltaTime;
        pos.x = baseX + drift;
        transform.position = pos;

        // タップされないまま速度が0近辺になったらフェードアウト
        if (currentSpeed <= fadeOutStartSpeedThreshold)
        {
            BeginFadeOut();
        }
    }

    public override void OnTapped()
    {
        Explode(true);
    }

    private void Explode(bool tapped)
    {
        if (hasExploded)
        {
            return;
        }

        hasExploded = true;

        // この花火（ロケット）分だけ打ち上げ音を止める
        if (launchAudioSource != null)
        {
            launchAudioSource.Stop();
        }

        if (manager != null)
        {
            manager.SpawnExplosion(transform.position, isGiant ? 2.2f : 1.0f, isGiant, tapped);
        }

        Destroy(gameObject);
    }

    private void BeginFadeOut()
    {
        if (hasExploded || isFadingOut)
        {
            return;
        }

        isFadingOut = true;
        fadeOutElapsed = 0f;
    }

    private void UpdateSpeedByHeight()
    {
        // 画面上部に近づくにつれて減速し、stopY付近で0になる
        float y = transform.position.y;
        if (stopY <= slowStartY)
        {
            currentSpeed = initialSpeed;
            return;
        }

        float t = Mathf.InverseLerp(slowStartY, stopY, y);
        // ease-out気味にして自然な減速にする
        t = Mathf.Clamp01(t);
        float eased = 1f - (1f - t) * (1f - t);
        currentSpeed = Mathf.Lerp(initialSpeed, 0f, eased);

        if (y >= stopY)
        {
            currentSpeed = 0f;
        }
    }

    private void SetAlpha(float a)
    {
        if (spriteRenderer == null)
        {
            return;
        }

        Color c = spriteRenderer.color;
        c.a = Mathf.Clamp01(a);
        spriteRenderer.color = c;
    }
}
