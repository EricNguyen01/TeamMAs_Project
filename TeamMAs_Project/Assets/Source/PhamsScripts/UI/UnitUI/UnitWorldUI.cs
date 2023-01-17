using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace TeamMAsTD
{
    public class UnitWorldUI : MonoBehaviour
    {
        [SerializeField] protected Canvas unitWorldCanvas;
        [SerializeField] protected TextMeshProUGUI nameTextMeshProComponent;
        [SerializeField] protected Slider healthBarSlider;

        protected IUnit unitLinkedToUI;
        protected UnitSO unitSO;

        protected virtual void Awake()
        {
            SetUpUnitWorldUI();
            TextUIDisplaysUnitName();
        }

        protected virtual void SetUpUnitWorldUI()
        {
            if (unitWorldCanvas == null)
            {
                Debug.LogError("A UnitWorldUI script is attached to :" + name + " but its world canvas UI component is not assigned! Disabling script!");
                enabled = false;
                return;
            }

            unitLinkedToUI = GetComponent<IUnit>();

            if (unitLinkedToUI == null)
            {
                Debug.LogError("No Unit script component found in :" + name + ". Unit UI disabled!");
                enabled = false;
                return;
            }

            unitSO = unitLinkedToUI.GetUnitScriptableObjectData();

            if (unitSO == null)
            {
                Debug.LogError("Unit ScriptableObject data on unit script attached to obj: " + name + " is null! Disabling Unit UI!");
                enabled = false;
                return;
            }

            if (nameTextMeshProComponent == null)
            {
                Debug.LogWarning("Unit name text UI component is not assigned on Unit World UI script on obj: " + name + ".");
            }

            if(healthBarSlider == null)
            {
                Debug.LogWarning("Unit health bar slider UI component is not assigned on Unit World UI script on obj: " + name + ".");
            }

            unitWorldCanvas.worldCamera = Camera.main;
        }

        protected virtual void TextUIDisplaysUnitName()
        {
            if (unitSO == null) return;

            if (nameTextMeshProComponent == null) return;

            nameTextMeshProComponent.text = unitSO.displayName;
        }

        public virtual void EnableUnitNameTextUI(bool enabled)
        {
            if (nameTextMeshProComponent == null) return;

            if (enabled)
            {
                if(!nameTextMeshProComponent.enabled) nameTextMeshProComponent.enabled = true;
                return;
            }

            if (nameTextMeshProComponent.enabled) nameTextMeshProComponent.enabled = false;
        }

        public virtual void EnableUnitHealthBarSlider(bool enabled)
        {
            if (healthBarSlider == null) return;

            if (enabled)
            {
                if (!healthBarSlider.gameObject.activeInHierarchy) healthBarSlider.gameObject.SetActive(true);

                return;
            }

            healthBarSlider.gameObject.SetActive(false);
        }

        public virtual void SetHealthBarSliderValue(float currentVal, float maxVal)
        {
            if (healthBarSlider == null) return;

            healthBarSlider.value = currentVal / maxVal;

            if (healthBarSlider.value <= 0.0f) healthBarSlider.value = 0.0f;
        }

        public virtual void SetHealthBarSliderValue(float currentVal, float maxVal, bool reversedSlider)
        {
            float newCurrentVal = currentVal;

            if (reversedSlider)
            {
               newCurrentVal = maxVal - currentVal;
            }

            SetHealthBarSliderValue(newCurrentVal, maxVal);
        }
    }
}
