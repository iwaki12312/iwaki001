using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using WakuWaku.IAP;

public class ParentalMenuLongPressButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [SerializeField, Min(0.1f)] private float holdSeconds = 2f;
    [SerializeField] private Image progressFillImage;
    [SerializeField] private Image progressBackgroundImage;
    [SerializeField] private Color progressColor = new Color(1f, 0.6f, 0.1f, 0.9f);
    [SerializeField] private Color progressBackgroundColor = new Color(0f, 0f, 0f, 0.25f);
    [SerializeField] private float progressHeight = 10f;
    [SerializeField] private bool disableChildRaycastTargets = true;
    [SerializeField] private bool useUnscaledTime = true;

    private bool isPressed;
    private float heldSeconds;
    private bool isTriggered;

    private void Awake()
    {
        EnsureProgressImage();
        if (disableChildRaycastTargets)
        {
            DisableChildRaycastTargets();
        }
        ResetProgress();
    }

    private void Update()
    {
        if (!isPressed || isTriggered) return;

        heldSeconds += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        UpdateProgressVisual();

        if (heldSeconds >= holdSeconds)
        {
            isTriggered = true;
            SetProgress(1f);
            LegalContentPanel.ShowMenu();
            CancelHold();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isActiveAndEnabled) return;

        isPressed = true;
        isTriggered = false;
        heldSeconds = 0f;
        UpdateProgressVisual();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        CancelHold();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        CancelHold();
    }

    private void CancelHold()
    {
        isPressed = false;
        heldSeconds = 0f;
        isTriggered = false;
        ResetProgress();
    }

    private void EnsureProgressImage()
    {
        if (progressFillImage != null)
        {
            var baseSprite = GetBaseSprite();
            progressFillImage.sprite ??= baseSprite;
            if (progressBackgroundImage != null)
            {
                progressBackgroundImage.sprite ??= baseSprite;
            }
            return;
        }

        var baseSpriteForExisting = GetBaseSprite();

        var bgTransform = transform.Find("HoldProgressBG");
        if (bgTransform != null)
        {
            progressBackgroundImage = bgTransform.GetComponent<Image>();
            if (progressBackgroundImage != null)
            {
                progressBackgroundImage.sprite ??= baseSpriteForExisting;
            }
        }

        var fillTransform = transform.Find("HoldProgressFill");
        if (fillTransform != null)
        {
            progressFillImage = fillTransform.GetComponent<Image>();
            if (progressFillImage != null)
            {
                progressFillImage.sprite ??= baseSpriteForExisting;
                return;
            }
        }

        var holdBg = new GameObject("HoldProgressBG", typeof(RectTransform), typeof(Image));
        holdBg.transform.SetParent(transform, false);

        var bgRect = (RectTransform)holdBg.transform;
        bgRect.anchorMin = new Vector2(0f, 0f);
        bgRect.anchorMax = new Vector2(1f, 0f);
        bgRect.pivot = new Vector2(0.5f, 0f);
        bgRect.anchoredPosition = Vector2.zero;
        bgRect.sizeDelta = new Vector2(0f, progressHeight);

        progressBackgroundImage = holdBg.GetComponent<Image>();
        progressBackgroundImage.raycastTarget = false;
        progressBackgroundImage.color = progressBackgroundColor;
        progressBackgroundImage.sprite ??= baseSpriteForExisting;
        progressBackgroundImage.type = Image.Type.Sliced;

        var holdFill = new GameObject("HoldProgressFill", typeof(RectTransform), typeof(Image));
        holdFill.transform.SetParent(holdBg.transform, false);

        var fillRect = (RectTransform)holdFill.transform;
        fillRect.anchorMin = new Vector2(0f, 0f);
        fillRect.anchorMax = new Vector2(1f, 1f);
        fillRect.pivot = new Vector2(0.5f, 0.5f);
        fillRect.anchoredPosition = Vector2.zero;
        fillRect.sizeDelta = Vector2.zero;

        progressFillImage = holdFill.GetComponent<Image>();
        progressFillImage.raycastTarget = false;
        progressFillImage.color = progressColor;
        progressFillImage.sprite ??= baseSpriteForExisting;
        progressFillImage.type = Image.Type.Filled;
        progressFillImage.fillMethod = Image.FillMethod.Horizontal;
        progressFillImage.fillOrigin = 0;
        progressFillImage.fillAmount = 0f;
    }

    private Sprite GetBaseSprite()
    {
        var image = GetComponent<Image>();
        if (image != null && image.sprite != null) return image.sprite;

        var button = GetComponent<Button>();
        if (button != null && button.targetGraphic is Image targetImage && targetImage.sprite != null)
        {
            return targetImage.sprite;
        }

        return Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
    }

    private void DisableChildRaycastTargets()
    {
        var allGraphics = GetComponentsInChildren<Graphic>(true);
        foreach (var graphic in allGraphics)
        {
            if (graphic == null) continue;
            if (graphic.gameObject == gameObject) continue;
            graphic.raycastTarget = false;
        }
    }

    private void ResetProgress()
    {
        SetProgress(0f);
    }

    private void UpdateProgressVisual()
    {
        var t = Mathf.Clamp01(holdSeconds <= 0f ? 1f : heldSeconds / holdSeconds);
        SetProgress(t);
    }

    private void SetProgress(float t)
    {
        if (progressFillImage != null)
        {
            progressFillImage.fillAmount = Mathf.Clamp01(t);
            progressFillImage.enabled = isPressed;
        }

        if (progressBackgroundImage != null)
        {
            progressBackgroundImage.enabled = isPressed;
        }
    }
}
