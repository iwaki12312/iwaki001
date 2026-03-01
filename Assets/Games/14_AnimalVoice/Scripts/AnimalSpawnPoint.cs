using UnityEngine;

/// <summary>
/// 動物のスポーンポイント。
/// シーンにGameObjectとして配置し、Sceneビューでドラッグ&ドロップで位置・大きさを調整可能。
/// Edit mode中はプレビュースプライトが表示され、Play mode開始時にAnimalSpawnerが
/// 各スポーンポイントの位置に実際の動物を生成する。
/// </summary>
[ExecuteAlways]
public class AnimalSpawnPoint : MonoBehaviour
{
    [Header("プレビュー設定")]
    [Tooltip("Sceneビューに表示するプレビュー用スプライト")]
    [SerializeField] private Sprite previewSprite;

    [Header("スポーン設定")]
    [Tooltip("このポイントの動物スケール（0=Spawnerのデフォルト値を使用）")]
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
            // Play mode中はSpawnerが管理するので非表示
            spriteRenderer.sprite = null;
            spriteRenderer.enabled = false;
        }
        else
        {
            // Edit mode中はプレビュースプライトを表示
            spriteRenderer.sprite = previewSprite;
            spriteRenderer.enabled = true;
            // プレビューであることが分かるよう少し半透明に
            spriteRenderer.color = new Color(1f, 1f, 1f, 0.7f);
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
        // Sceneビューで位置がわかりやすいようにGizmo表示
        Gizmos.color = new Color(0f, 1f, 0.5f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }

    void OnDrawGizmosSelected()
    {
        // 選択時はより見やすく
        Gizmos.color = new Color(1f, 1f, 0f, 0.8f);
        Gizmos.DrawWireSphere(transform.position, 0.6f);
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.8f, gameObject.name);
    }
#endif
}
