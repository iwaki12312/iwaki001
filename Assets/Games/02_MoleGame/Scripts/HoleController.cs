using UnityEngine;
using System.Collections;

public class HoleController : MonoBehaviour
{
    public MoleController moleController;
    public SpriteRenderer holeEdge;
    public GameObject sweatEffect;
    public Vector2 sweatOffset = new Vector2(0.3f, 0.3f); // 汗エフェクトのオフセット位置
    public Sprite customMaskSprite; // カスタムマスク用のスプライト
    public Vector2 maskSize = new Vector2(0.8f, 0.8f); // マスクのサイズ
    public Vector2 maskOffset = new Vector2(0, 0); // マスクのオフセット位置

    private bool isActive = false;
    private bool isShocked = false;

    private Coroutine activeCoroutine;
    private SpriteMask holeMask; // 穴のマスク
    private GameObject maskObject; // マスク用のゲームオブジェクト

    // マウスクリック（タップ）イベント
    private void OnMouseDown()
    {
        TapMole();
    }

    private void Awake()
    {
        // 初期状態では汗エフェクトを非表示
        if (sweatEffect != null)
            sweatEffect.SetActive(false);
        
        // エディタモードで作成したマスクオブジェクトを削除
        Transform editorMask = transform.Find("HoleMask");
        if (editorMask != null && editorMask.CompareTag("EditorOnly"))
        {
            Destroy(editorMask.gameObject);
        }
        
        // 既存のマスクを削除（もし存在すれば）
        SpriteMask existingMask = GetComponentInChildren<SpriteMask>();
        if (existingMask != null && existingMask.gameObject != editorMask)
        {
            Destroy(existingMask.gameObject);
        }
        
        // カスタムマスク用のゲームオブジェクトを作成
        CreateCustomMask();
    }
    
#if UNITY_EDITOR
    // Inspectorで値が変更されたときや、スクリプトがロードされたときに呼ばれる
    private void OnValidate()
    {
        // エディタモードでマスクを作成・更新
        if (!Application.isPlaying)
        {
            // 既存のマスクオブジェクトを削除
            Transform existingMask = transform.Find("HoleMask");
            if (existingMask != null)
            {
                DestroyImmediate(existingMask.gameObject);
            }
            
            // マスクオブジェクトを作成
            GameObject editorMaskObj = new GameObject("HoleMask");
            editorMaskObj.transform.SetParent(transform);
            editorMaskObj.transform.localPosition = new Vector3(maskOffset.x, maskOffset.y, 0);
            editorMaskObj.transform.localScale = new Vector3(maskSize.x, maskSize.y, 1);
            
            // マスクの視覚化用にSpriteRendererを追加
            SpriteRenderer visualizer = editorMaskObj.AddComponent<SpriteRenderer>();
            
            // カスタムマスクスプライトが設定されている場合は使用
            if (customMaskSprite != null)
            {
                visualizer.sprite = customMaskSprite;
            }
            else
            {
                // デフォルトの円形スプライトを使用（実際のマスクと同じものは作れないので近似値）
                visualizer.sprite = UnityEditor.AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            }
            
            // 半透明の緑色で表示
            visualizer.color = new Color(0, 1, 0, 0.3f);
            
            // エディタ用のタグを付ける（実行時に削除するため）
            editorMaskObj.tag = "EditorOnly";
        }
    }
    
    // シーンビューにマスク範囲を描画
    private void OnDrawGizmos()
    {
        // マスクの中心位置
        Vector3 maskPos = transform.position + new Vector3(maskOffset.x, maskOffset.y, 0);
        
        // マスクの範囲を示す円を描画
        Gizmos.color = new Color(0, 1, 0, 0.5f);
        float radius = Mathf.Min(maskSize.x, maskSize.y) * 0.5f;
        Gizmos.DrawWireSphere(maskPos, radius);
        
        // マスクの中心を示す十字を描画
        Gizmos.color = Color.yellow;
        float crossSize = 0.1f;
        Gizmos.DrawLine(maskPos + new Vector3(-crossSize, 0, 0), maskPos + new Vector3(crossSize, 0, 0));
        Gizmos.DrawLine(maskPos + new Vector3(0, -crossSize, 0), maskPos + new Vector3(0, crossSize, 0));
        
        // マスクの範囲を示すテキストを描画
        UnityEditor.Handles.Label(maskPos + Vector3.up * (radius + 0.1f), "Mask Area");
    }
#endif
    
    // カスタムマスクを作成
    private void CreateCustomMask()
    {
        // マスク用のゲームオブジェクトを作成
        maskObject = new GameObject("HoleMask");
        maskObject.transform.SetParent(transform);
        maskObject.transform.localPosition = new Vector3(maskOffset.x, maskOffset.y, 0);
        
        // SpriteMaskコンポーネントを追加
        holeMask = maskObject.AddComponent<SpriteMask>();
        
        // カスタムマスクスプライトが設定されている場合は使用
        if (customMaskSprite != null)
        {
            holeMask.sprite = customMaskSprite;
        }
        // 設定されていない場合は円形のスプライトを作成
        else
        {
            // 円形のスプライトを作成
            Texture2D texture = new Texture2D(128, 128);
            Color[] colors = new Color[128 * 128];
            
            for (int y = 0; y < 128; y++)
            {
                for (int x = 0; x < 128; x++)
                {
                    float dx = x - 64;
                    float dy = y - 64;
                    float distance = Mathf.Sqrt(dx * dx + dy * dy);
                    
                    // 円の内側は白（不透明）、外側は透明
                    if (distance < 60)
                    {
                        colors[y * 128 + x] = Color.white;
                    }
                    else
                    {
                        colors[y * 128 + x] = Color.clear;
                    }
                }
            }
            
            texture.SetPixels(colors);
            texture.Apply();
            
            // スプライトを作成
            Sprite circleSprite = Sprite.Create(texture, new Rect(0, 0, 128, 128), new Vector2(0.5f, 0.5f));
            holeMask.sprite = circleSprite;
        }
        
        // マスクの設定
        holeMask.alphaCutoff = 0.5f;
        
        // マスクのサイズを設定
        maskObject.transform.localScale = new Vector3(maskSize.x, maskSize.y, 1);
    }

    private void OnEnable()
    {
        // モグラのショック状態変更イベントを購読
        if (moleController != null)
            moleController.OnShockStateChanged += OnMoleShockStateChanged;

    }

    private void OnDisable()
    {
        // イベント購読を解除
        if (moleController != null)
            moleController.OnShockStateChanged -= OnMoleShockStateChanged;
    }

    // モグラのショック状態が変更された時の処理
    private void OnMoleShockStateChanged(bool isShockedLocal)
    {
        if (sweatEffect != null)
        {
            Debug.Log($"OnMoleShockStateChanged: isShocked={isShockedLocal}, sweatEffect={sweatEffect.name}");

            // 汗エフェクトの表示/非表示を切り替え
            sweatEffect.SetActive(isShockedLocal);

            if (isShockedLocal)
            {
                // モグラの位置を基準に汗エフェクトの位置を設定
                Vector3 molePosition = moleController.transform.position;
                sweatEffect.transform.position = new Vector3(
                    molePosition.x + sweatOffset.x,
                    molePosition.y + sweatOffset.y,
                    molePosition.z
                );

                // SpriteRendererコンポーネントを取得
                SpriteRenderer sweatRenderer = sweatEffect.GetComponent<SpriteRenderer>();
                if (sweatRenderer != null)
                {
                    // ソーティングオーダーを設定（モグラよりも前面に表示）
                    sweatRenderer.sortingOrder = moleController.moleRenderer.sortingOrder + 1;

                    // スケールとColorのアルファ値を確認
                    Debug.Log($"SweatEffect: scale={sweatEffect.transform.localScale}, color={sweatRenderer.color}");
                }
                else
                {
                    Debug.LogError("SweatEffect has no SpriteRenderer component!");
                }
            }
        }
        else
        {
            Debug.LogError("SweatEffect is not assigned!");
        }
    }

    // モグラを出現させる
    public void ShowMole(MoleData moleData, float duration)
    {
        if (isActive) return;

        isActive = true;
        
        // 出現音を再生
        SfxPlayer.Instance.PlayOneShot(moleData.popSound);
        
        // ショック状態を初期化
        isShocked = false;
        
        // モグラを設定
        moleController.SetMoleData(moleData);
        
        // 汗エフェクトを非表示
        if (sweatEffect != null)
            sweatEffect.SetActive(false);
        
        // モグラを表示して出現アニメーションを開始
        moleController.gameObject.SetActive(true);
        moleController.StartAppearAnimation();

        // 一定時間後に自動で戻る
        activeCoroutine = StartCoroutine(HideMoleAfterDelay(duration));
    }

    // モグラを叩いた時の処理
    public void TapMole()
    {
        if (!isActive) return;

        // 自動で戻るのをキャンセル
        if (activeCoroutine != null)
        {
            StopCoroutine(activeCoroutine);
        }

        // 叩いた効果音を再生（既にショック状態であれば再生しない）
        if (!isShocked)
        {
            SfxPlayer.Instance.PlayOneShot(SfxPlayer.Instance.hit);
        }

        // ショック状態にする
        isShocked = true;

        // 叩かれたアニメーション
        moleController.ShowShockState();

        // 短い時間で戻る
        activeCoroutine = StartCoroutine(HideMoleAfterDelay(0.3f));
    }

    // 一定時間後にモグラを隠す
    private IEnumerator HideMoleAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        HideMole();
    }

    // モグラを隠す
    private void HideMole()
    {
        // 汗エフェクトを非表示
        if (sweatEffect != null)
            sweatEffect.SetActive(false);

        // モグラがアクティブかどうかを確認
        if (moleController.gameObject.activeInHierarchy)
        {
            // 消失アニメーションを開始し、完了時にモグラを非アクティブにする
            moleController.StartDisappearAnimation(() => {
                moleController.gameObject.SetActive(false);
                isActive = false;
                activeCoroutine = null;
            });
        }
        else
        {
            // モグラが既に非アクティブな場合は、アニメーションをスキップ
            isActive = false;
            activeCoroutine = null;
        }
    }

    // アクティブ状態を取得
    public bool IsActive()
    {
        return isActive;
    }

    // ショック状態を取得
    public bool IsMoleShocked()
    {
        return isShocked;
    }
}
