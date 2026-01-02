using UnityEngine;
using System.Linq;
using UnityEngine.InputSystem;

/// <summary>
/// タップイベントを一元管理し、重なったオブジェクトの処理順序を制御するクラス
/// </summary>
public class TouchManager : MonoBehaviour
{
    private Camera mainCamera;
    
    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("メインカメラが見つかりません！");
        }
    }
    
    void Update()
    {
        // マウスクリック（PC用）
        var mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
        {
            Vector2 touchPosition = mainCamera.ScreenToWorldPoint(mouse.position.ReadValue());
            HandleTouch(touchPosition);
        }

        var touchscreen = Touchscreen.current;
        if (touchscreen != null)
        {
            foreach (var touch in touchscreen.touches)
            {
                if (!touch.press.wasPressedThisFrame) continue;
                Vector2 touchPosition = mainCamera.ScreenToWorldPoint(touch.position.ReadValue());
                HandleTouch(touchPosition);
            }
        }
    }
    
    /// <summary>
    /// タップ位置の処理
    /// </summary>
    /// <param name="position">タップされた位置（ワールド座標）</param>
    private void HandleTouch(Vector2 position)
    {
        // タップ位置にあるすべてのコライダーを取得
        Collider2D[] hitColliders = Physics2D.OverlapPointAll(position);
        
        if (hitColliders.Length == 0) return;
        
        // SpriteRendererを持つオブジェクトのみをフィルタリング
        var validObjects = hitColliders
            .Where(collider => collider.GetComponent<SpriteRenderer>() != null)
            .Select(collider => new {
                Collider = collider,
                SpriteRenderer = collider.GetComponent<SpriteRenderer>(),
                Star = collider.GetComponent<Star>(),
                UFO = collider.GetComponent<UFO>()
            })
            .Where(obj => obj.Star != null || obj.UFO != null)
            .OrderByDescending(obj => obj.SpriteRenderer.sortingOrder) // SortingOrderの降順でソート
            .ToArray();
        
        if (validObjects.Length == 0) return;
        
        // 最前面のオブジェクトを処理
        var frontObject = validObjects[0];
        
        // 星の場合
        if (frontObject.Star != null)
        {
            Debug.Log($"TouchManager: 星をタップしました（SortingOrder: {frontObject.SpriteRenderer.sortingOrder}）");
            frontObject.Star.HandleTouchManagerTap();
            return;
        }
        
        // UFOの場合
        if (frontObject.UFO != null)
        {
            Debug.Log($"TouchManager: UFOをタップしました（SortingOrder: {frontObject.SpriteRenderer.sortingOrder}）");
            frontObject.UFO.HandleTouchManagerTap();
            return;
        }
    }
}
