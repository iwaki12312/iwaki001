using UnityEngine;

public class StarPieceController : MonoBehaviour
{
    private Vector2 velocity;
    private float angularSpeed;
    private float lifetime;
    private float elapsed;
    private SpriteRenderer spriteRenderer;
    private Color baseColor;

    public void Initialize(Vector2 initialVelocity, float initialAngularSpeed, float pieceLifetime, Color color)
    {
        velocity = initialVelocity;
        angularSpeed = initialAngularSpeed;
        lifetime = Mathf.Max(0.01f, pieceLifetime);
        elapsed = 0f;
        baseColor = color;

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = baseColor;
        }
    }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        baseColor = spriteRenderer != null ? spriteRenderer.color : Color.white;
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        elapsed += dt;

        transform.position += (Vector3)(velocity * dt);
        transform.Rotate(0f, 0f, angularSpeed * dt, Space.Self);

        if (spriteRenderer != null)
        {
            float t = Mathf.Clamp01(elapsed / lifetime);
            Color c = baseColor;
            c.a = Mathf.Lerp(baseColor.a, 0f, t);
            spriteRenderer.color = c;
        }

        if (elapsed >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}

