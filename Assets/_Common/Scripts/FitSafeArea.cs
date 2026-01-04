using UnityEngine;

[ExecuteAlways]                               // ★ 追加：Edit時も動く
[RequireComponent(typeof(RectTransform))]
public class FitSafeArea : MonoBehaviour
{
    public enum Mode { Full, TopOnly, SymmetricH }
    [SerializeField] Mode mode = Mode.Full;

    Rect _lastArea; Vector2Int _lastRes;
    RectTransform _rt;

    void OnEnable()
    {
        _rt = (RectTransform)transform;
        Apply();
    }

    void LateUpdate()                          // ★ 再生中/編集中どちらでも監視
    {
        if (_lastArea != Screen.safeArea ||
            _lastRes.x != Screen.width || _lastRes.y != Screen.height)
            Apply();
    }

#if UNITY_EDITOR
    void OnValidate() { Apply(); }             // ★ インスペクタ変更でも即反映
#endif

    void Apply()
    {
        if (_rt == null)
            _rt = (RectTransform)transform;

        if (Screen.width <= 0 || Screen.height <= 0)
            return;

        var sa = Screen.safeArea;
        _lastArea = sa; _lastRes = new Vector2Int(Screen.width, Screen.height);

        float left   = sa.xMin;
        float right  = Screen.width  - sa.xMax;
        float bottom = sa.yMin;
        float top    = Screen.height - sa.yMax;

        if (mode == Mode.TopOnly)
            { left = right = bottom = 0f; }
        else if (mode == Mode.SymmetricH)
            { float h = Mathf.Max(left, right); left = right = h; }

        var amin = new Vector2(left  / Screen.width,  bottom / Screen.height);
        var amax = new Vector2(1f - right / Screen.width, 1f - top / Screen.height);

        _rt.anchorMin = amin;
        _rt.anchorMax = amax;
        _rt.offsetMin = Vector2.zero;
        _rt.offsetMax = Vector2.zero;
    }
}
