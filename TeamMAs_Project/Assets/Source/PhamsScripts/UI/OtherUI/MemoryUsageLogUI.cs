using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Runtime.CompilerServices;
using System.Text;

namespace TeamMAsTD
{
    [DisallowMultipleComponent]
    public class MemoryUsageLogUI : MonoBehaviour
    {
        [Header("Settings")]

        [SerializeField] private bool enableLogUIDisplay = true;

        [SerializeField] private bool closeUIOnSceneLoad = false;

        [SerializeField] private KeyCode toggleLogUIKey = KeyCode.F12;

        [Header("UI Components")]

        [SerializeField] private TextMeshProUGUI memoryLogSummaryText;

        //INTERNALS..........................................................................

        private Canvas mainCanvas;

        private CanvasGroup toggleCanvasGroup;

        private string memoryLogSummaryTitle = "MEMORY LOG SUMMARY:\n";

        private StringBuilder logStringBuilder = new StringBuilder();

        private bool isLogDisplayed = false;

        private void OnEnable()
        {
            if(!mainCanvas && !TryGetComponent<Canvas>(out mainCanvas))
            {
                enabled = false;

                return;
            }

            if(!toggleCanvasGroup && !TryGetComponent<CanvasGroup>(out toggleCanvasGroup)) 
            toggleCanvasGroup = gameObject.AddComponent<CanvasGroup>();

            if(mainCanvas)
            {
                if (!memoryLogSummaryText)
                {
                    memoryLogSummaryText = mainCanvas.GetComponentInChildren<TextMeshProUGUI>();
                }

                if (memoryLogSummaryText)
                {
                    memoryLogSummaryText.text = memoryLogSummaryTitle;

                    if (memoryLogSummaryText.raycastTarget) memoryLogSummaryText.raycastTarget = false;
                }
            }

            SceneManager.sceneLoaded += (Scene sc, LoadSceneMode loadScMode) => CloseMemoryLogUIOnSceneLoad();

            SceneManager.sceneLoaded += (Scene sc, LoadSceneMode loadScMode) => GetMainCamOnSceneLoaded();

            if(toggleCanvasGroup.blocksRaycasts) toggleCanvasGroup.blocksRaycasts = false;

            if(toggleCanvasGroup.interactable) toggleCanvasGroup.interactable = false;

            EnableMemoryLogUI(isLogDisplayed);
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= (Scene sc, LoadSceneMode loadScMode) => CloseMemoryLogUIOnSceneLoad();

            SceneManager.sceneLoaded -= (Scene sc, LoadSceneMode loadScMode) => GetMainCamOnSceneLoaded();
        }

        private void Start()
        {
            MemoryUsageLogger.SetupMemoryLogUI(this);
        }

        private void Update()
        {
            if (!enabled) return;

            ToggleMemoryLogUI();
        }

        public void SetMemoryLogSummaryUIText(string memoryLogSummaryText = "n/a")
        {
            if (this.memoryLogSummaryText)
            {
                if(logStringBuilder == null) logStringBuilder = new StringBuilder();

                if (logStringBuilder.Length > 0) logStringBuilder.Clear();

                this.memoryLogSummaryText.text = logStringBuilder.Append(memoryLogSummaryTitle).Append(memoryLogSummaryText).ToString();
            }
        }

        private void ToggleMemoryLogUI()
        {
            if (!enableLogUIDisplay) return;

            if (Input.GetKeyDown(toggleLogUIKey))
            {
                if(isLogDisplayed)
                {
                    EnableMemoryLogUI(false);

                    return;
                }

                EnableMemoryLogUI(true);
            }
        }

        private void EnableMemoryLogUI(bool enabled)
        {
            if (enabled)
            {
                isLogDisplayed = true;

                if (toggleCanvasGroup) toggleCanvasGroup.alpha = 1.0f;

                return;
            }

            isLogDisplayed = false;

            if(toggleCanvasGroup) toggleCanvasGroup.alpha = 0.0f;
        }

        private void CloseMemoryLogUIOnSceneLoad()
        {
            if (!closeUIOnSceneLoad) return;

            EnableMemoryLogUI(false);
        }

        private void GetMainCamOnSceneLoaded()
        {
            if (!mainCanvas) return;

            if (mainCanvas.renderMode == RenderMode.ScreenSpaceOverlay) return;

            mainCanvas.worldCamera = Camera.main;
        }
    }
}
