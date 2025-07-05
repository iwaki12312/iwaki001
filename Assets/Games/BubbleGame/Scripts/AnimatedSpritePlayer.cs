using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatedSpritePlayer : MonoBehaviour
{
    [SerializeField] private Sprite[] frames;
    [SerializeField] private float framesPerSecond = 10f;
    [SerializeField] private bool loop = false;
    [SerializeField] private bool destroyWhenFinished = true;
    
    private SpriteRenderer spriteRenderer;
    private int currentFrame = 0;
    private float timer = 0f;
    
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    void Start()
    {
        // BubbleSplash.gifのスプライトフレームを取得
        if (frames == null || frames.Length == 0)
        {
            frames = Resources.LoadAll<Sprite>("BubbleSplash/BubbleSplash");
            if (frames == null || frames.Length == 0)
            {
                Debug.LogError("スプライトフレームが見つかりません");
                return;
            }
            Debug.Log("読み込んだフレーム数: " + frames.Length);
        }
        
        // 最初のフレームを表示
        if (spriteRenderer != null && frames.Length > 0)
        {
            spriteRenderer.sprite = frames[0];
        }
    }
    
    void Update()
    {
        if (frames == null || frames.Length <= 1 || spriteRenderer == null)
            return;
            
        // フレームの更新
        timer += Time.deltaTime;
        if (timer >= 1f / framesPerSecond)
        {
            timer = 0f;
            currentFrame++;
            
            // ループ処理
            if (currentFrame >= frames.Length)
            {
                if (loop)
                {
                    currentFrame = 0;
                }
                else
                {
                    currentFrame = frames.Length - 1;
                    if (destroyWhenFinished)
                    {
                        Destroy(gameObject);
                        return;
                    }
                }
            }
            
            // スプライトの更新
            spriteRenderer.sprite = frames[currentFrame];
            Debug.Log("フレーム更新: " + currentFrame);
        }
    }
}
