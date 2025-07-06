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
            arrowPrefab = Resources.Load<GameObject>("Games/BubbleGame/Prefabs/Arrow");
            
            if (arrowPrefab == null)
            {
                // 直接パスを指定して検索
                arrowPrefab = Resources.Load<GameObject>("Arrow");
                
                if (arrowPrefab == null)
                {
                    Debug.LogError("矢印プレハブが見つかりません");
                    return;
                }
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
