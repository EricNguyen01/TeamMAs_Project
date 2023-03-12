using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TeamMAsTD
{
    [DisallowMultipleComponent]
    public class InfoTooltip : MonoBehaviour
    {
        [Header("Required Components")]

        [SerializeField] private Image tooltipWorldUIImage;

        [SerializeField] private TextMeshProUGUI tooltipClickOnReminderText;

        [Header("Optional")]

        [SerializeField] private Camera worldUICam;

        private InfoTooltipEnabler infoTooltipEnablerSpawnedThisTooltip;

        private UnitSO unitScriptableObjectToDisplayTooltip;

        private Canvas infoTooltipCanvas;

        private Animator tooltipClickOnReminderAnimator;

        public bool isTooltipActive { get; private set; } = false;

        public bool isTooltipReminderActive { get; private set; } = false;

        private void Awake()
        {
            if(tooltipWorldUIImage == null)
            {
                Debug.LogError("Unit Info Tooltip Image is not found on: " + name + ". Tooltip will not work and will be disabled!");

                enabled = false;

                return;
            }

            infoTooltipCanvas = GetComponent<Canvas>();

            if (worldUICam != null) infoTooltipCanvas.worldCamera = worldUICam;
            else infoTooltipCanvas.worldCamera = Camera.main;

            if(tooltipClickOnReminderText != null) tooltipClickOnReminderAnimator = tooltipClickOnReminderText.GetComponent<Animator>();
        }

        public void InitializeInfoTooltip(InfoTooltipEnabler tooltipEnablerSpawnedThis, UnitSO unitSO)
        {
            if(tooltipEnablerSpawnedThis == null)
            {
                enabled = false;

                return;
            }

            infoTooltipEnablerSpawnedThisTooltip = tooltipEnablerSpawnedThis;

            UpdateTooltipDisplayDataFromTooltipEnabler();
        }

        private void UpdateTooltipDisplayDataFromTooltipEnabler()
        {
            if (infoTooltipEnablerSpawnedThisTooltip == null) return;

            if (tooltipWorldUIImage == null) return;

            unitScriptableObjectToDisplayTooltip = infoTooltipEnablerSpawnedThisTooltip.unitScriptableObjectToDisplayTooltip;

            if (unitScriptableObjectToDisplayTooltip != null)
            {
                tooltipWorldUIImage.sprite = unitScriptableObjectToDisplayTooltip.unitInfoTooltipImageSprite;
            }

            SetTooltipClickOnReminderTextAnimator(infoTooltipEnablerSpawnedThisTooltip.clickReminderAnimOverride);

            if(infoTooltipEnablerSpawnedThisTooltip.infoTooltipSpawnTransformRef != null)
            {
                transform.position = (Vector2)infoTooltipEnablerSpawnedThisTooltip.infoTooltipSpawnTransformRef.position;
            }
            else
            {
                transform.position = (Vector2)infoTooltipEnablerSpawnedThisTooltip.transform.position + infoTooltipEnablerSpawnedThisTooltip.infoTooltipSpawnOffset;
            }
        }

        public void SetTooltipClickOnReminderTextAnimator(AnimatorOverrideController animOverrideController)
        {
            if (tooltipClickOnReminderText == null) return;

            if(tooltipClickOnReminderAnimator == null) return;

            if (animOverrideController == null) return;

            tooltipClickOnReminderAnimator.runtimeAnimatorController = animOverrideController;
        }

        public void EnableIntoTooltipImageFromButton(bool enabled)
        {
            EnableInfoTooltipImage(enabled);
        }

        public void EnableInfoTooltipImage(bool enabled, bool setTooltipClickReminderStatus = true)
        {
            if (tooltipWorldUIImage == null) return;

            if (infoTooltipEnablerSpawnedThisTooltip == null) return;

            if (enabled)
            {
                UpdateTooltipDisplayDataFromTooltipEnabler();

                isTooltipActive = true;

                if (!tooltipWorldUIImage.gameObject.activeInHierarchy) tooltipWorldUIImage.gameObject.SetActive(true);

                if (infoTooltipEnablerSpawnedThisTooltip.clickReminderDisplayTimer != null && setTooltipClickReminderStatus)
                {
                    infoTooltipEnablerSpawnedThisTooltip.clickReminderDisplayTimer.SetReminderInactiveAndStopTimerOnTooltipOpened(infoTooltipEnablerSpawnedThisTooltip);
                }

                return;
            }

            isTooltipActive = false;

            if (tooltipWorldUIImage.gameObject.activeInHierarchy) tooltipWorldUIImage.gameObject.SetActive(false);

            if (infoTooltipEnablerSpawnedThisTooltip.clickReminderDisplayTimer != null && setTooltipClickReminderStatus)
            {
                infoTooltipEnablerSpawnedThisTooltip.clickReminderDisplayTimer.StartClickOnReminderTimerOnTooltipClosed(infoTooltipEnablerSpawnedThisTooltip);
            }
        }

        public void EnableTooltipClickOnReminder(bool enabled)
        {
            if (tooltipClickOnReminderText == null) return;

            if (enabled)
            {
                UpdateTooltipDisplayDataFromTooltipEnabler();

                isTooltipReminderActive = true;

                if (!tooltipClickOnReminderText.gameObject.activeInHierarchy) tooltipClickOnReminderText.gameObject.SetActive(true);

                return;
            }

            isTooltipReminderActive = false;

            if (tooltipClickOnReminderText.gameObject.activeInHierarchy) tooltipClickOnReminderText.gameObject.SetActive(false);
        }
    }
}
