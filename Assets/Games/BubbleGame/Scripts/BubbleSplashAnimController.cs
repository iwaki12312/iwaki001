using UnityEngine;
using System.Collections;

public class BubbleSplashAnimController : MonoBehaviour
{
    [SerializeField] private float fadeOutDuration = 1.0f;
    [SerializeField] private float destroyDelay = 0.5f;
    
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private float startTime;
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
            startTime = Time.time;
        }
        
        // 一定時間後に自動的に破棄
        Destroy(gameObject, destroyDelay);
        
        // フェードアウト処理を開始
        StartCoroutine(FadeOut());
    }
    
    // フェードアウト処理
    private IEnumerator FadeOut()
    {
        float elapsedTime = 0f;
        
        // フェードアウト開始までの待機時間
        yield return new WaitForSeconds(destroyDelay - fadeOutDuration);
        
        // フェードアウト処理
        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(originalColor.a, 0f, elapsedTime / fadeOutDuration);
            
            if (spriteRenderer != null)
            {
                Color newColor = spriteRenderer.color;
                newColor.a = alpha;
                spriteRenderer.color = newColor;
            }
            
            yield return null;
        }
    }
    
    // シャボン玉の色とサイズを設定するメソッド
    public void SetBubbleProperties(Color bubbleColor, Vector3 bubbleScale)
    {
        if (spriteRenderer != null)
        {
            // 色を設定（アルファ値は維持）
            Color newColor = bubbleColor;
            newColor.a = spriteRenderer.color.a;
            spriteRenderer.color = newColor;
            originalColor = newColor;
            
            // スケールを設定
            transform.localScale = bubbleScale;
        }
    }
}
