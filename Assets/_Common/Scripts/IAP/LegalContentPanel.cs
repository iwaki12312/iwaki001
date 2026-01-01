using System;
using UnityEngine;
using UnityEngine.UI;

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

        [Header("URLs")]
        [SerializeField] private string termsOfServiceUrl;
        [SerializeField] private string privacyPolicyUrl;
        [SerializeField] private string legalNoticeUrl;
        [SerializeField] private string contactUrl;

        [Header("Behavior")]
        [SerializeField] private bool hideAfterOpenUrl = false;
        [SerializeField] private bool startHidden = true;

        private bool isWired;

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
    }
}
