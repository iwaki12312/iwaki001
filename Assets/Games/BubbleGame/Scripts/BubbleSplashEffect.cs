using UnityEngine;

public class BubbleSplashEffect : MonoBehaviour
{
    [SerializeField] public float lifetime = 0.5f;
    [SerializeField] private float animationSpeed = 30.0f;
    
    private SpriteRenderer spriteRenderer;
    private float startTime;
    private Sprite[] frames;
    private int currentFrame = 0;
    private float frameTimer = 0;
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        startTime = Time.time;
        
        // 指定した時間後に自動的に破棄
        Destroy(gameObject, lifetime);
        
        // スプライトシートのフレームを取得
        frames = Resources.LoadAll<Sprite>("BubbleSplash/BubbleSplash");
        
        // フレームを並び替え（名前でソート）
        System.Array.Sort(frames, (a, b) => a.name.CompareTo(b.name));
        if (frames != null && frames.Length > 0)
        {
            Debug.Log("BubbleSplashEffect: フレーム数 " + frames.Length + " を読み込みました");
            spriteRenderer.sprite = frames[0];
        }
        else
        {
            Debug.LogError("BubbleSplashEffect: フレームが見つかりません");
        }
    }
    
    void Update()
    {
        if (frames == null || frames.Length <= 1)
            return;
            
        // フレームアニメーション
        frameTimer += Time.deltaTime * animationSpeed;
        if (frameTimer >= 1.0f)
        {
            frameTimer = 0;
            currentFrame = (currentFrame + 1) % frames.Length;
            spriteRenderer.sprite = frames[currentFrame];
            Debug.Log("BubbleSplashEffect: フレーム " + currentFrame + " に更新");
        }
        
        // フェードアウト効果
        float normalizedTime = (Time.time - startTime) / lifetime;
        if (normalizedTime > 0.7f) // 70%経過後にフェードアウト開始
        {
            float alpha = Mathf.Lerp(1.0f, 0.0f, (normalizedTime - 0.7f) / 0.3f);
            Color color = spriteRenderer.color;
            color.a = alpha;
            spriteRenderer.color = color;
        }
    }
}
