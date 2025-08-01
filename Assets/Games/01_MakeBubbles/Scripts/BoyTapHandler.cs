using UnityEngine;

public class BoyTapHandler : MonoBehaviour
{
    [SerializeField] private GameObject bubblePrefab;
    [SerializeField] private float spawnOffset = -1f;
    [SerializeField] private float bubbleSpeed = 5f;
    Camera mainCam;             // 毎フレーム Camera.main を探さないようにキャッシュ
    Collider2D myCollider;

    private void Start()
    {
        // Prefabの参照が設定されていない場合は、シーン内から検索
        if (bubblePrefab == null)
        {
            // シーン内のBubbleプレハブを検索
            GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.name == "Bubble" && obj.CompareTag("Bubble"))
                {
                    bubblePrefab = obj;
                    Debug.Log("シーン内でバブルプレハブを見つけました: " + obj.name);
                    break;
                }
            }

            // 見つからない場合は、GameInitializerに依頼
            if (bubblePrefab == null)
            {
                GameInitializer gameInitializer = FindObjectOfType<GameInitializer>();
                if (gameInitializer != null)
                {
                    // GameInitializerにバブルプレハブを要求
                    bubblePrefab = gameInitializer.GetBubblePrefab();
                    if (bubblePrefab != null)
                    {
                        Debug.Log("GameInitializerからバブルプレハブを取得しました");
                    }
                }
            }

            if (bubblePrefab == null)
            {
                Debug.LogError("バブルプレハブが見つかりません");
            }

        }

        mainCam = Camera.main;
        myCollider = GetComponent<Collider2D>();

    }

    void Update()
    {
        /* ❶ すべてのタッチをチェック（実機用） */
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch t = Input.GetTouch(i);
            if (t.phase == TouchPhase.Began && HitBoy(t.position))
            {
                SpawnBubble();
                HideArrow();
            }
        }

        /* ❷ エディタ / PC での確認用フォールバック */
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0) && HitBoy(Input.mousePosition))
        {
            SpawnBubble();
            HideArrow();
        }
#endif
    }

    /* 画面座標 pos がこの Boy のコライダー上か？ */
    bool HitBoy(Vector2 screenPos)
    {
        if (!mainCam)
        {
            mainCam = Camera.main;
            Debug.LogWarning("[BoyTapHandler] mainCam が null だったので再取得");
            if (!mainCam) return false;
        }
        if (!myCollider)
        {
            Debug.LogError("[BoyTapHandler] myCollider が null");
            return false;
        }

        Vector2 world = mainCam.ScreenToWorldPoint(screenPos);
        bool hit = myCollider.OverlapPoint(world);

        Debug.Log($"[BoyTapHandler] HitBoy(): worldPos={world}  result={hit}");
        return hit;
    }

    // 矢印を非表示にするメソッド
    private void HideArrow()
    {
        // ArrowManagerを検索
        ArrowManager arrowManager = FindObjectOfType<ArrowManager>();
        if (arrowManager != null)
        {
            arrowManager.HideArrow();
        }
        else
        {
            // ArrowControllerを直接検索
            ArrowController[] arrows = FindObjectsOfType<ArrowController>();
            foreach (ArrowController arrow in arrows)
            {
                arrow.HideArrow();
            }
        }
    }

    // シャボン玉を生成するメソッド
    private void SpawnBubble()
    {
        if (bubblePrefab != null)
        {
            // 効果音を再生
            if (BubbleSoundManager.Instance != null)
            {
                BubbleSoundManager.Instance.PlayShotSound();
            }
            else
            {
                Debug.LogWarning("BubbleSoundManagerが見つかりません。効果音が再生されません。");
            }

            // 少年の左上から少し離れた位置にシャボン玉を生成
            Vector3 spawnPosition = transform.position + new Vector3(spawnOffset, 1.5f, 0);
            GameObject bubble = Instantiate(bubblePrefab, spawnPosition, Quaternion.identity);

            // ランダムなサイズを設定
            float size = Random.Range(0.5f, 1.5f);
            bubble.transform.localScale = new Vector3(size, size, 1f);

            // ランダムな色を設定
            Color randomColor = new Color(
                Random.Range(0.7f, 1f),
                Random.Range(0.7f, 1f),
                Random.Range(0.7f, 1f),
                0.8f
            );
            SpriteRenderer renderer = bubble.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.color = randomColor;
            }

            // シャボン玉に左上方向への初速度を設定
            Rigidbody2D rb = bubble.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = new Vector2(-1.5f, 1f) * bubbleSpeed;
            }

            // BubbleControllerの処理も適用される
            Debug.Log("シャボン玉を生成しました。BubbleControllerの初期設定が適用されます。");
        }
        else
        {
            Debug.LogError("バブルプレハブが設定されていません。");
        }
    }
}
