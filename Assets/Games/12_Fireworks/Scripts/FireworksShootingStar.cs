using UnityEngine;

public class FireworksShootingStar : FireworksTapTarget
{
    private FireworksManager manager;
    private SpriteRenderer spriteRenderer;
    private Collider2D collider2d;

    private Vector3 velocity;
    private bool isConsumed;
    private float rotationSpeedDegPerSec;

    public void Initialize(FireworksManager manager, Sprite sprite, Vector3 velocity)
    {
        this.manager = manager;
        this.velocity = velocity;

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        spriteRenderer.sprite = sprite;
        spriteRenderer.sortingOrder = 5;

        collider2d = GetComponent<Collider2D>();
        if (collider2d == null)
        {
            var circle = gameObject.AddComponent<CircleCollider2D>();
            circle.isTrigger = true;
            collider2d = circle;
        }

        transform.localScale = Vector3.one * 1.2f;
        rotationSpeedDegPerSec = Random.Range(-90f, 90f);
        isConsumed = false;
    }

    private void Update()
    {
        if (isConsumed)
        {
            return;
        }

        transform.position += velocity * Time.deltaTime;
        transform.Rotate(0f, 0f, rotationSpeedDegPerSec * Time.deltaTime);

        if (manager != null && manager.IsOutsideScreen(transform.position, 1.5f))
        {
            Destroy(gameObject);
        }
    }

    public override void OnTapped()
    {
        if (isConsumed)
        {
            return;
        }

        isConsumed = true;
        if (manager != null)
        {
            manager.OnShootingStarTapped(transform.position);
        }

        Destroy(gameObject);
    }
}
