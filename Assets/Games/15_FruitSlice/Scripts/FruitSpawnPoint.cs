using UnityEngine;

/// <summary>
/// フルーツのスポーンポイント。
/// シーンにGameObjectとして配置し、Sceneビューでドラッグ&ドロップで位置を調整可能。
/// Inspectorでスケール・コライダー半径も個別にオーバーライドできる。
/// Edit mode中はプレビュースプライトが半透明で表示される。
/// Play mode開始時にInitializerが各スポーンポイントの位置にフルーツスロットを生成する。
/// </summary>
[ExecuteAlways]
public class FruitSpawnPoint : MonoBehaviour
{
    [Header("プレビュー設定")]
    [Tooltip("Sceneビューに表示するプレビュー用スプライト")]
    [SerializeField] private Sprite previewSprite;

    [Header("スポーン設定")]
    [Tooltip("このポイントのフルーツスケール（0=Initializerのデフォルト値を使用）")]
    [SerializeField, Range(0f, 5f)] private float overrideScale = 0f;

    [Tooltip("このポイントのコライダー半径（0=Initializerのデフォルト値を使用）")]
    [SerializeField, Range(0f, 5f)] private float overrideColliderRadius = 0f;

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

    /// <summary>コライダー半径オーバーライド（0ならデフォルト）</summary>
    public float OverrideColliderRadius => overrideColliderRadius;

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
            // Play mode中はスロットが管理するので非表示
            spriteRenderer.sprite = null;
            spriteRenderer.enabled = false;
        }
        else
        {
            // Edit mode中はプレビュースプライトを半透明で表示
            spriteRenderer.sprite = previewSprite;
            spriteRenderer.enabled = true;
            spriteRenderer.color = new Color(1f, 1f, 1f, 0.6f);
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
        // Sceneビューでスポーン位置がわかりやすいようにGizmo表示
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.4f); // オレンジ
        float radius = overrideColliderRadius > 0f ? overrideColliderRadius : 0.5f;
        Gizmos.DrawWireSphere(transform.position, radius);
    }

    void OnDrawGizmosSelected()
    {
        // 選択時はより見やすく
        Gizmos.color = new Color(1f, 0.8f, 0f, 0.8f); // 黄色
        float radius = overrideColliderRadius > 0f ? overrideColliderRadius : 0.6f;
        Gizmos.DrawWireSphere(transform.position, radius);
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.8f, gameObject.name);
    }
#endif
}
