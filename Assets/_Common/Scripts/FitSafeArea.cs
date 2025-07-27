// ① SafeAreaRoot 空オブジェクトを作成
// ② このスクリプトを付ける
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class FitSafeArea : MonoBehaviour
{
    void Awake()
    {
        var rt = (RectTransform)transform;
        var area = Screen.safeArea;

        Vector2 min = area.position;
        Vector2 max = area.position + area.size;
        min.x /= Screen.width;  min.y /= Screen.height;
        max.x /= Screen.width;  max.y /= Screen.height;

        rt.anchorMin = min;
        rt.anchorMax = max;
    }
}
