using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TeamMAsTD
{
    [DisallowMultipleComponent]
    public class MemoryUsageLogUI : MonoBehaviour
    {
        [Header("Settings")]

        [SerializeField] private bool enableLogUIDisplay = true;

        [SerializeField] private KeyCode toggleLogUIKey = KeyCode.F12;

        [Header("UI Components")]

        [SerializeField] private TextMeshProUGUI memoryLogSummaryText;

        //INTERNALS..........................................................................

        private Canvas mainCanvas;

        private CanvasGroup toggleCanvasGroup;

        private string memoryLogSummaryTitle = "MEMORY LOG SUMMARY:\n";

        private bool isLogDisplayed = false;

        private void Awake()
        {
            if (memoryLogSummaryText) 
            { 
                memoryLogSummaryText.text = memoryLogSummaryTitle;

                memoryLogSummaryText.raycastTarget = false;
            }
        }

        private void OnEnable()
        {
            if(!TryGetComponent<Canvas>(out mainCanvas))
            {
                enabled = false;

                return;
            }

            if(!TryGetComponent<CanvasGroup>(out toggleCanvasGroup)) toggleCanvasGroup = gameObject.AddComponent<CanvasGroup>();

            toggleCanvasGroup.blocksRaycasts = false;

            toggleCanvasGroup.interactable = false;

            EnableMemoryLogUI(false);
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
                this.memoryLogSummaryText.text = memoryLogSummaryTitle + memoryLogSummaryText;
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

                toggleCanvasGroup.alpha = 1.0f;

                return;
            }

            isLogDisplayed = false;

            toggleCanvasGroup.alpha = 0.0f;
        }
    }
}
