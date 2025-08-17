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
            // 犬少年または猫少女のオブジェクトを検索
            GameObject targetObject = null;
            
            // AnimalBubbleMakerコンポーネントを持つオブジェクトを検索
            AnimalBubbleMaker[] animalMakers = FindObjectsOfType<AnimalBubbleMaker>();
            if (animalMakers.Length > 0)
            {
                targetObject = animalMakers[0].gameObject;
                Debug.Log("AnimalBubbleMakerを持つオブジェクトを見つけました: " + targetObject.name);
            }
            else
            {
                // 犬少年オブジェクトを名前で検索
                targetObject = GameObject.Find("DogBoy");
                if (targetObject == null)
                {
                    // 猫少女オブジェクトを名前で検索
                    targetObject = GameObject.Find("CatGirl");
                }
                
                if (targetObject != null)
                {
                    Debug.Log("動物オブジェクトを名前で見つけました: " + targetObject.name);
                }
            }
            
            if (targetObject != null)
            {
                // 対象オブジェクトの上に矢印を生成
                Vector3 arrowPosition = targetObject.transform.position + new Vector3(0f, 1.5f, 0);
                arrowInstance = Instantiate(arrowPrefab, arrowPosition, Quaternion.identity);
                
                // ArrowControllerコンポーネントがない場合は追加
                if (arrowInstance.GetComponent<ArrowController>() == null)
                {
                    arrowInstance.AddComponent<ArrowController>();
                }
                
                Debug.Log("矢印を生成しました: " + targetObject.name + "の上");
            }
            else
            {
                Debug.LogWarning("犬少年または猫少女のオブジェクトが見つかりません。矢印は生成されません。");
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
