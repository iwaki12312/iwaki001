using UnityEngine;

/// <summary>
/// 矢印オブジェクトを管理するクラス
/// </summary>
public class ArrowManager : MonoBehaviour
{
    [SerializeField] private GameObject arrowPrefab;
    
    private GameObject arrowInstance;
    
    void Start()
    {
        // 矢印プレハブが設定されていない場合は検索
        if (arrowPrefab == null)
        {
            Debug.Log("矢印プレハブが設定されていません。プロジェクト内から検索します。");
            
            // シーン内の矢印プレハブを検索
            GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.name.Contains("Arrow") && obj.GetComponent<ArrowController>() != null)
                {
                    arrowPrefab = obj;
                    Debug.Log("矢印プレハブを見つけました: " + obj.name);
                    break;
                }
            }
            
            // それでも見つからない場合は新しく作成
            if (arrowPrefab == null)
            {
                Debug.Log("矢印プレハブが見つからないため、新しく作成します。");
                
                arrowPrefab = new GameObject("Arrow");
                arrowPrefab.AddComponent<ArrowController>();
                
                // スプライトを設定
                SpriteRenderer renderer = arrowPrefab.AddComponent<SpriteRenderer>();
                
                // 矢印用のスプライトを検索
                Sprite arrowSprite = null;
                Sprite[] allSprites = Resources.FindObjectsOfTypeAll<Sprite>();
                foreach (Sprite sprite in allSprites)
                {
                    if (sprite.name.Contains("Arrow"))
                    {
                        arrowSprite = sprite;
                        Debug.Log("矢印用スプライトを見つけました: " + sprite.name);
                        break;
                    }
                }
                
                if (arrowSprite != null)
                {
                    renderer.sprite = arrowSprite;
                }
                else
                {
                    Debug.LogWarning("矢印用スプライトが見つかりません");
                }
                
                // 非表示にして、プレハブとして使用
                arrowPrefab.SetActive(false);
            }
        }
        
        // 矢印を生成
        CreateArrow();
    }
    
    // 矢印を生成するメソッド
    private void CreateArrow()
    {
        if (arrowPrefab != null)
        {
            // 少年オブジェクトを検索
            GameObject boy = GameObject.FindGameObjectWithTag("Boy");
            if (boy != null)
            {
                // 少年の左上に矢印を生成
                Vector3 arrowPosition = boy.transform.position + new Vector3(-1.0f, 1.5f, 0);
                arrowInstance = Instantiate(arrowPrefab, arrowPosition, Quaternion.identity);
                
                // ArrowControllerコンポーネントがない場合は追加
                if (arrowInstance.GetComponent<ArrowController>() == null)
                {
                    arrowInstance.AddComponent<ArrowController>();
                }
                
                Debug.Log("矢印を生成しました");
            }
            else
            {
                Debug.LogError("少年オブジェクトが見つかりません");
            }
        }
    }
    
    // 矢印を非表示にするメソッド
    public void HideArrow()
    {
        if (arrowInstance != null)
        {
            ArrowController controller = arrowInstance.GetComponent<ArrowController>();
            if (controller != null)
            {
                controller.HideArrow();
            }
            else
            {
                arrowInstance.SetActive(false);
            }
            
            Debug.Log("矢印を非表示にしました");
        }
    }
}
