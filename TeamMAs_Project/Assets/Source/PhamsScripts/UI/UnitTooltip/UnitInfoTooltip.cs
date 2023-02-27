using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TeamMAsTD
{
    [DisallowMultipleComponent]
    public class UnitInfoTooltip : MonoBehaviour
    {
        [Header("Required Components")]

        [SerializeField] private Image tooltipWorldUIImage;

        [SerializeField] private TextMeshProUGUI tooltipClickOnReminderText;

        [Header("Optional")]

        [SerializeField] private Camera worldUICam;

        private UnitInfoTooltipEnabler unitInfoTooltipEnablerSpawnedThisTooltip;

        private UnitSO unitScriptableObjectToDisplayTooltip;

        private Canvas unitInfoTooltipCanvas;

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

            unitInfoTooltipCanvas = GetComponent<Canvas>();

            if (worldUICam != null) unitInfoTooltipCanvas.worldCamera = worldUICam;
            else unitInfoTooltipCanvas.worldCamera = Camera.main;

            if(tooltipClickOnReminderText != null) tooltipClickOnReminderAnimator = tooltipClickOnReminderText.GetComponent<Animator>();
        }

        public void InitializeUnitInfoTooltip(UnitInfoTooltipEnabler tooltipEnablerSpawnedThis, UnitSO unitSO, Vector2 displayPos)
        {
            if(tooltipEnablerSpawnedThis == null)
            {
                enabled = false;

                return;
            }

            unitInfoTooltipEnablerSpawnedThisTooltip = tooltipEnablerSpawnedThis;

            UpdateTooltipDisplayDataFromTooltipEnabler();
        }

        private void UpdateTooltipDisplayDataFromTooltipEnabler()
        {
            if (unitInfoTooltipEnablerSpawnedThisTooltip == null) return;

            if (tooltipWorldUIImage == null) return;

            unitScriptableObjectToDisplayTooltip = unitInfoTooltipEnablerSpawnedThisTooltip.unitScriptableObjectToDisplayTooltip;

            tooltipWorldUIImage.sprite = unitScriptableObjectToDisplayTooltip.unitInfoTooltipImageSprite;

            SetTooltipClickOnReminderTextAnimator(unitInfoTooltipEnablerSpawnedThisTooltip.clickReminderAnimOverride);

            if(unitInfoTooltipEnablerSpawnedThisTooltip.unitInfoTooltipSpawnTransformRef != null)
            {
                transform.position = (Vector2)unitInfoTooltipEnablerSpawnedThisTooltip.unitInfoTooltipSpawnTransformRef.position;
            }
            else
            {
                transform.position = (Vector2)unitInfoTooltipEnablerSpawnedThisTooltip.transform.position + unitInfoTooltipEnablerSpawnedThisTooltip.unitInfoTooltipSpawnOffset;
            }
        }

        public void SetTooltipClickOnReminderTextAnimator(AnimatorOverrideController animOverrideController)
        {
            if (tooltipClickOnReminderText == null) return;

            if(tooltipClickOnReminderAnimator == null) return;

            if (animOverrideController == null) return;

            tooltipClickOnReminderAnimator.runtimeAnimatorController = animOverrideController;
        }

        public void EnableUnitInfoTooltipImage(bool enabled)
        {
            if (tooltipWorldUIImage == null) return;

            if (unitInfoTooltipEnablerSpawnedThisTooltip == null) return;

            if (enabled)
            {
                UpdateTooltipDisplayDataFromTooltipEnabler();

                isTooltipActive = true;

                if (!tooltipWorldUIImage.gameObject.activeInHierarchy) tooltipWorldUIImage.gameObject.SetActive(true);

                return;
            }

            isTooltipActive = false;

            if (tooltipWorldUIImage.gameObject.activeInHierarchy) tooltipWorldUIImage.gameObject.SetActive(false);
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
