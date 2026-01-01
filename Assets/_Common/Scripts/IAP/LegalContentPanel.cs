using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace WakuWaku.IAP
{
    public class LegalContentPanel : MonoBehaviour
    {
        public static LegalContentPanel Instance { get; private set; }
        public static bool IsShowingAny { get; private set; }

        [Header("Buttons (optional, auto-found by name if empty)")]
        [SerializeField] private Button closeButton;
        [SerializeField] private Button termsOfServiceButton;
        [SerializeField] private Button privacyPolicyButton;
        [SerializeField] private Button legalNoticeButton;
        [SerializeField] private Button contactButton;
        [SerializeField] private Button restoreButton;

        [Header("URLs")]
        [SerializeField] private string termsOfServiceUrl;
        [SerializeField] private string privacyPolicyUrl;
        [SerializeField] private string legalNoticeUrl;
        [SerializeField] private string contactUrl;

        [Header("Behavior")]
        [SerializeField] private bool hideAfterOpenUrl = false;
        [SerializeField] private bool startHidden = true;

        private bool isWired;
        private bool isRestoring;
        private string restoreButtonOriginalText;
        private bool restoreButtonOriginalInteractable;
        private bool restoreButtonOriginalStateCaptured;
        private Coroutine restoreFeedbackCoroutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            WireIfNeeded();
        }

        private void Start()
        {
            // シーン起動直後にアクティブで配置されてしまった場合のみ非表示にする。
            // （非アクティブ配置→後から表示、の初回表示を潰さないため）
            if (!startHidden) return;
            if (Time.frameCount > 2) return;

            Hide();
        }

        private void OnEnable()
        {
            Instance ??= this;
            if (ReferenceEquals(Instance, this))
            {
                IsShowingAny = true;
            }
        }

        private void OnDisable()
        {
            if (ReferenceEquals(Instance, this))
            {
                IsShowingAny = false;
            }

            ResetRestoreButtonVisuals();
        }

        public static void ShowMenu()
        {
            var panel = Instance ?? FindSceneInstanceIncludingInactive();
            if (panel == null)
            {
                Debug.LogError("[LegalContentPanel] LegalContentPanelが見つかりません。シーンに配置されているか確認してください。");
                return;
            }

            panel.Show();
        }

        public void Show()
        {
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }

            WireIfNeeded();
        }

        public void Hide()
        {
            if (gameObject.activeSelf)
            {
                gameObject.SetActive(false);
            }
        }

        private static LegalContentPanel FindSceneInstanceIncludingInactive()
        {
            var all = Resources.FindObjectsOfTypeAll<LegalContentPanel>();
            foreach (var panel in all)
            {
                if (panel == null) continue;
                if (!panel.gameObject.scene.IsValid()) continue; // ignore prefab assets
                Instance = panel;
                return panel;
            }

            return null;
        }

        private void WireIfNeeded()
        {
            if (isWired) return;
            isWired = true;

            TryAutoFindReferences();

            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(Hide);
                closeButton.onClick.AddListener(Hide);
            }

            WireOpenUrl(termsOfServiceButton, () => termsOfServiceUrl);
            WireOpenUrl(privacyPolicyButton, () => privacyPolicyUrl);
            WireOpenUrl(legalNoticeButton, () => legalNoticeUrl);
            WireOpenUrl(contactButton, () => contactUrl);

            WireRestorePurchases(restoreButton);
        }

        private void WireOpenUrl(Button button, Func<string> urlGetter)
        {
            if (button == null) return;

            void Handler()
            {
                OpenUrl(urlGetter());
            }

            button.onClick.RemoveListener(Handler);
            button.onClick.AddListener(Handler);
        }

        private void WireRestorePurchases(Button button)
        {
            if (button == null) return;

            if (!restoreButtonOriginalStateCaptured)
            {
                restoreButtonOriginalInteractable = button.interactable;
                restoreButtonOriginalText = GetButtonLabelText(button);
                restoreButtonOriginalStateCaptured = true;
            }

            void Handler()
            {
                OnRestoreClicked(button);
            }

            button.onClick.RemoveListener(Handler);
            button.onClick.AddListener(Handler);
        }

        private void OnRestoreClicked(Button button)
        {
            if (isRestoring) return;

            if (PurchaseService.Instance == null || !PurchaseService.Instance.IsInitialized)
            {
                StartRestoreFeedback(button, "購入サービス準備中…", 1.5f, keepDisabled: false);
                return;
            }

            isRestoring = true;
            StartRestoreFeedback(button, "復元中…", 0f, keepDisabled: true);

            PurchaseService.Instance.RestorePurchases(() =>
            {
                isRestoring = false;
                StartRestoreFeedback(button, "復元しました", 1.5f, keepDisabled: false);
            });
        }

        private void OpenUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                Debug.LogWarning("[LegalContentPanel] URLが未設定です（Inspectorで設定してください）。");
                return;
            }

            Application.OpenURL(url);

            if (hideAfterOpenUrl)
            {
                Hide();
            }
        }

        private void TryAutoFindReferences()
        {
            closeButton ??= FindButtonByName("CloseButton");
            termsOfServiceButton ??= FindButtonByName("Btn_TermsOfService");
            privacyPolicyButton ??= FindButtonByName("Btn_PrivacyPolicy");
            legalNoticeButton ??= FindButtonByName("Btn_LegalNotice");
            contactButton ??= FindButtonByName("Btn_Contact");
            restoreButton ??= FindButtonByName("Btn_Restore");
        }

        private Button FindButtonByName(string objectName)
        {
            foreach (var button in GetComponentsInChildren<Button>(true))
            {
                if (button != null && button.name == objectName)
                {
                    return button;
                }
            }

            return null;
        }

        private void StartRestoreFeedback(Button button, string temporaryText, float seconds, bool keepDisabled)
        {
            if (button == null) return;

            if (restoreFeedbackCoroutine != null)
            {
                StopCoroutine(restoreFeedbackCoroutine);
                restoreFeedbackCoroutine = null;
            }

            if (seconds <= 0f)
            {
                if (keepDisabled)
                {
                    button.interactable = false;
                }
                SetButtonLabelText(button, temporaryText);
                return;
            }

            restoreFeedbackCoroutine = StartCoroutine(RestoreFeedbackCoroutine(button, temporaryText, seconds, keepDisabled));
        }

        private System.Collections.IEnumerator RestoreFeedbackCoroutine(Button button, string temporaryText, float seconds, bool keepDisabled)
        {
            if (button == null) yield break;

            var previousInteractable = button.interactable;
            var previousText = GetButtonLabelText(button);

            button.interactable = keepDisabled ? false : previousInteractable;
            SetButtonLabelText(button, temporaryText);

            yield return new WaitForSecondsRealtime(seconds);

            button.interactable = previousInteractable;
            SetButtonLabelText(button, string.IsNullOrEmpty(restoreButtonOriginalText) ? previousText : restoreButtonOriginalText);

            restoreFeedbackCoroutine = null;
        }

        private void ResetRestoreButtonVisuals()
        {
            isRestoring = false;

            if (restoreFeedbackCoroutine != null)
            {
                StopCoroutine(restoreFeedbackCoroutine);
                restoreFeedbackCoroutine = null;
            }

            if (restoreButton == null) return;

            restoreButton.interactable = restoreButtonOriginalStateCaptured ? restoreButtonOriginalInteractable : true;
            if (!string.IsNullOrEmpty(restoreButtonOriginalText))
            {
                SetButtonLabelText(restoreButton, restoreButtonOriginalText);
            }
        }

        private string GetButtonLabelText(Button button)
        {
            if (button == null) return null;

            var tmp = button.GetComponentInChildren<TMP_Text>(true);
            if (tmp != null) return tmp.text;

            var legacy = button.GetComponentInChildren<Text>(true);
            if (legacy != null) return legacy.text;

            return null;
        }

        private void SetButtonLabelText(Button button, string text)
        {
            if (button == null) return;

            var tmp = button.GetComponentInChildren<TMP_Text>(true);
            if (tmp != null)
            {
                tmp.text = text;
                return;
            }

            var legacy = button.GetComponentInChildren<Text>(true);
            if (legacy != null)
            {
                legacy.text = text;
            }
        }
    }
}
