using UnityEngine;

/// <summary>
/// キノコのスポーンポイント。
/// シーンにGameObjectとして配置し、Sceneビューでドラッグ&ドロップで位置を調整可能。
/// Edit mode中はプレビュースプライトが表示される。
/// </summary>
[ExecuteAlways]
public class MushroomSpawnPoint : MonoBehaviour
{
    [Header("プレビュー設定")]
    [Tooltip("Sceneビューに表示するプレビュー用スプライト")]
    [SerializeField] private Sprite previewSprite;

    [Header("スポーン設定")]
    [Tooltip("このポイントのキノコスケール（0=Spawnerのデフォルト値を使用）")]
    [SerializeField, Range(0f, 5f)] private float overrideScale = 0f;

    private SpriteRenderer spriteRenderer;

    /// <summary>プレビュー用スプライト</summary>
    public Sprite PreviewSprite
    {
        get => previewSprite;
        set
        {
            previewSprite = value;
            RefreshPreview();
        }
    }

    /// <summary>スケールオーバーライド（0ならデフォルト）</summary>
    public float OverrideScale => overrideScale;

    void OnEnable()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        spriteRenderer.sortingOrder = 10;
        RefreshPreview();
    }

    void OnValidate()
    {
        RefreshPreview();
    }

    /// <summary>
    /// プレビュー表示を更新
    /// </summary>
    private void RefreshPreview()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        if (spriteRenderer == null) return;

        if (Application.isPlaying)
        {
            spriteRenderer.sprite = null;
            spriteRenderer.enabled = false;
        }
        else
        {
            spriteRenderer.sprite = previewSprite;
            spriteRenderer.enabled = true;
            spriteRenderer.color = new Color(0.2f, 0.2f, 0.2f, 0.6f); // シルエット風プレビュー
        }
    }

    /// <summary>
    /// Play mode開始時にプレビューを非表示にする
    /// </summary>
    public void HidePreview()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = null;
            spriteRenderer.enabled = false;
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.6f, 0.3f, 0.1f, 0.5f); // 茶色（土っぽい色）
        Gizmos.DrawWireSphere(transform.position, 0.4f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.8f, 0f, 0.8f); // 黄色
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.7f, gameObject.name);
    }
#endif
}
