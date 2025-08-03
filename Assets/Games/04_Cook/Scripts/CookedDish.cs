using System.Collections;
using UnityEngine;

public class CookedDish : MonoBehaviour
{
    // 料理の種類
    public enum DishType
    {
        Normal,  // 通常料理
        Special, // 特別料理
        Fail     // 失敗料理
    }
    
    private SpriteRenderer dishRenderer;
    [SerializeField] private float displayDuration = 2.0f; // 表示時間
    [SerializeField] private float fadeInSpeed = 5.0f;     // フェードイン速度
    [SerializeField] private Vector3 specialScale = new Vector3(0.6f, 0.6f, 0.6f); // 特別料理のスケール
    
    private DishType currentDishType;
    
    private void Awake()
    {
        // SpriteRendererがなければ追加
        dishRenderer = GetComponent<SpriteRenderer>();
        if (dishRenderer == null)
        {
            dishRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        
        // 初期状態は非表示
        Color c = dishRenderer.color;
        c.a = 0f;
        dishRenderer.color = c;
        
        // レイヤーとソート順を設定（調理器具の上に表示されるように）
        dishRenderer.sortingOrder = 10;
    }
    
    // 料理を表示
    public void ShowDish(Sprite dishSprite, DishType type)
    {
        currentDishType = type;
        dishRenderer.sprite = dishSprite;
        
        // 透明度をリセット
        Color c = dishRenderer.color;
        c.a = 0f;
        dishRenderer.color = c;
        
        // 特別料理の場合は大きく表示
        if (type == DishType.Special)
        {
            transform.localScale = specialScale;
        }
        else
        {
            transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
        }
        
        // フェードインと自動消去のコルーチンを開始
        StartCoroutine(FadeInAndHide());
    }
    
    // フェードインして一定時間後に消すコルーチン
    private IEnumerator FadeInAndHide()
    {
        // フェードイン
        float alpha = 0f;
        while (alpha < 1f)
        {
            alpha += fadeInSpeed * Time.deltaTime;
            Color c = dishRenderer.color;
            c.a = Mathf.Clamp01(alpha);
            dishRenderer.color = c;
            yield return null;
        }
        
        // 表示時間待機
        yield return new WaitForSeconds(displayDuration);
        
        // フェードアウト
        alpha = 1f;
        while (alpha > 0f)
        {
            alpha -= fadeInSpeed * Time.deltaTime;
            Color c = dishRenderer.color;
            c.a = Mathf.Clamp01(alpha);
            dishRenderer.color = c;
            yield return null;
        }
        
        // 使用後は破棄
        Destroy(gameObject);
    }
}
