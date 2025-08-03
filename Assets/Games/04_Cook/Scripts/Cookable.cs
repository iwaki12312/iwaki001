using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Collider2D))]
public class Cookable : MonoBehaviour, IPointerDownHandler
{
    Animator anim;
    bool     isCooking;
    [SerializeField] ParticleSystem burst;
    
    // 調理器具の種類を指定するための列挙型
    public enum CookwareType
    {
        Pot,    // 鍋
        Pan     // フライパン
    }
    
    [SerializeField] CookwareType cookwareType = CookwareType.Pot;
    
    [Header("料理表示設定")]
    [SerializeField] private float dishOffsetY = 1.5f;      // 料理表示の高さオフセット
    [SerializeField] private float dishAppearDelay = 1.0f;  // 料理が出現するまでの待機時間（秒）
    [SerializeField] private float specialChance = 0.15f;   // 特別料理の確率
    [SerializeField] private float failChance = 0.15f;      // 失敗料理の確率
    
    [Header("料理スプライト - 通常")]
    [SerializeField] private Sprite[] normalDishes;         // 通常料理のスプライト配列
    
    [Header("料理スプライト - 特別/失敗")]
    [SerializeField] private Sprite specialDish;            // 特別料理のスプライト
    [SerializeField] private Sprite failDish;               // 失敗料理のスプライト

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void OnPointerDown(PointerEventData e)
    {
        if (isCooking) return;       // 連打防止
        isCooking = true;
        anim.SetTrigger("Play");
        
        // 調理中の効果音を再生
        PlayCookingSound();
    }
    
    // 調理中の効果音を再生
    private void PlayCookingSound()
    {
        if (CookSFXPlayer.Instance == null) return;
        
        // 調理器具の種類に応じた効果音を再生
        if (cookwareType == CookwareType.Pot)
        {
            CookSFXPlayer.Instance.PlayPotCookingSound();
        }
        else
        {
            CookSFXPlayer.Instance.PlayPanCookingSound();
        }
    }
    
    // 料理の種類を決定
    private CookedDish.DishType DetermineDishType()
    {
        float random = Random.value;
        
        if (random < failChance)
        {
            return CookedDish.DishType.Fail;
        }
        else if (random < failChance + specialChance)
        {
            return CookedDish.DishType.Special;
        }
        else
        {
            return CookedDish.DishType.Normal;
        }
    }
    
    // 料理のスプライトを取得
    private Sprite GetDishSprite(CookedDish.DishType type)
    {
        switch (type)
        {
            case CookedDish.DishType.Special:
                return specialDish;
            case CookedDish.DishType.Fail:
                return failDish;
            case CookedDish.DishType.Normal:
            default:
                // 通常料理からランダムに選択
                if (normalDishes != null && normalDishes.Length > 0)
                {
                    return normalDishes[Random.Range(0, normalDishes.Length)];
                }
                else
                {
                    Debug.LogError("通常料理のスプライトが設定されていません");
                    return null;
                }
        }
    }
    
    // 料理を表示
    private void ShowCookedDish()
    {
        // 料理の種類を決定
        CookedDish.DishType dishType = DetermineDishType();
        
        // 料理のスプライトを取得
        Sprite dishSprite = GetDishSprite(dishType);
        if (dishSprite == null) return;
        
        // 料理表示位置を計算（調理器具の上部）
        Vector3 dishPosition = transform.position + new Vector3(0, dishOffsetY, 0);
        
        // 料理表示用オブジェクトを生成
        GameObject dishObj = new GameObject($"CookedDish_{cookwareType}");
        dishObj.transform.position = dishPosition;
        
        // CookedDishコンポーネントを追加
        CookedDish cookedDish = dishObj.AddComponent<CookedDish>();
        
        // 料理を表示
        cookedDish.ShowDish(dishSprite, dishType);
        
        // 料理の種類に応じた効果音を再生
        PlayCompletionSound(dishType);
    }
    
    // 料理の種類に応じた効果音を再生
    private void PlayCompletionSound(CookedDish.DishType dishType)
    {
        if (CookSFXPlayer.Instance == null) return;
        
        switch (dishType)
        {
            case CookedDish.DishType.Special:
                CookSFXPlayer.Instance.PlaySpecialCompletedSound();
                break;
            case CookedDish.DishType.Fail:
                CookSFXPlayer.Instance.PlayFailCompletedSound();
                break;
            case CookedDish.DishType.Normal:
            default:
                CookSFXPlayer.Instance.PlayCookCompletedSound();
                break;
        }
    }
    
    // 待機時間後に料理を表示するコルーチン
    private IEnumerator ShowCookedDishWithDelay(CookedDish.DishType dishType, float delay)
    {
        // 指定した時間だけ待機
        yield return new WaitForSeconds(delay);
        
        // 料理のスプライトを取得
        Sprite dishSprite = GetDishSprite(dishType);
        if (dishSprite == null) yield break;
        
        // 料理表示位置を計算（調理器具の上部）
        Vector3 dishPosition = transform.position + new Vector3(0, dishOffsetY, 0);
        
        // 料理表示用オブジェクトを生成
        GameObject dishObj = new GameObject($"CookedDish_{cookwareType}");
        dishObj.transform.position = dishPosition;
        
        // CookedDishコンポーネントを追加
        CookedDish cookedDish = dishObj.AddComponent<CookedDish>();
        
        // 料理を表示
        cookedDish.ShowDish(dishSprite, dishType);
        
        // 料理の種類に応じたファンファーレを再生
        PlayFanfareSound(dishType);
    }
    
    // 料理の種類に応じたファンファーレを再生
    private void PlayFanfareSound(CookedDish.DishType dishType)
    {
        if (CookSFXPlayer.Instance == null) return;
        
        switch (dishType)
        {
            case CookedDish.DishType.Special:
                CookSFXPlayer.Instance.PlayFanfareSpecialSound();
                break;
            case CookedDish.DishType.Fail:
                CookSFXPlayer.Instance.PlayFanfareFailSound();
                break;
            case CookedDish.DishType.Normal:
            default:
                CookSFXPlayer.Instance.PlayFanfareSound();
                break;
        }
    }
    
    // パーティクルを再生（アニメーションイベントから呼び出し）
    public void PlayBurst()
    {
        burst.Play();
        
        // 調理中の効果音を停止
        if (CookSFXPlayer.Instance != null)
        {
            CookSFXPlayer.Instance.StopCookingSound();
        }
        
        // 料理の種類を決定（ここで決定しておく）
        CookedDish.DishType dishType = DetermineDishType();
        
        // 料理完了時の効果音を再生
        PlayCompletionSound(dishType);
        
        // 1秒後に料理を表示
        StartCoroutine(ShowCookedDishWithDelay(dishType, dishAppearDelay));
    }

    // Animation の最後にイベントを置くと呼ばれる
    public void OnCookEnd() => isCooking = false;
}
