using UnityEngine;

public class FireworksExplosion : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    private float duration;
    private float elapsed;

    private float startScale;
    private float endScale;
    private float startAlpha;
    private float endAlpha;

    private float rotationSpeed;

    public void Initialize(Sprite sprite, float durationSeconds, float startScale, float endScale, float startAlpha, float endAlpha, float rotationSpeed)
    {
        duration = Mathf.Max(0.01f, durationSeconds);
        this.startScale = startScale;
        this.endScale = endScale;
        this.startAlpha = startAlpha;
        this.endAlpha = endAlpha;
        this.rotationSpeed = rotationSpeed;

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }

        spriteRenderer.sprite = sprite;
        spriteRenderer.sortingOrder = 10;

        elapsed = 0f;
        Apply(0f);
    }

    private void Update()
    {
        if (duration <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / duration);
        Apply(t);

        if (rotationSpeed != 0f)
        {
            transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
        }

        if (elapsed >= duration)
        {
            Destroy(gameObject);
        }
    }

    private void Apply(float t)
    {
        float scale = Mathf.Lerp(startScale, endScale, t);
        transform.localScale = new Vector3(scale, scale, 1f);

        if (spriteRenderer != null)
        {
            float a = Mathf.Lerp(startAlpha, endAlpha, t);
            Color c = spriteRenderer.color;
            c.a = a;
            spriteRenderer.color = c;
        }
    }
}
