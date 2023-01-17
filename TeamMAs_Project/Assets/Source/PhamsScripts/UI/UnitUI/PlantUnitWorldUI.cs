using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TeamMAsTD
{
    public class PlantUnitWorldUI : UnitWorldUI
    {
        [SerializeField] protected Slider waterSlider;

        protected override void SetUpUnitWorldUI()
        {
            base.SetUpUnitWorldUI();

            if(waterSlider == null)
            {
                Debug.LogWarning("Plant water slider UI component is not assigned on Plant Unit World UI script on obj: " + name + ".");
            }
        }

        public virtual void EnablePlantUnitWaterSlider(bool enabled)
        {
            if (waterSlider == null) return;

            if (enabled)
            {
                if (!waterSlider.gameObject.activeInHierarchy) waterSlider.gameObject.SetActive(true);

                return;
            }

            waterSlider.gameObject.SetActive(false);
        }

        public virtual void SetWaterSliderValue(float currentVal, float maxVal)
        {
            if (waterSlider == null) return;

            waterSlider.value = currentVal / maxVal;

            if (waterSlider.value <= 0.0f) waterSlider.value = 0.0f;
        }

        public virtual void SetWaterSliderValue(float currentVal, float maxVal, bool reversedSlider)
        {
            float newCurrentVal = currentVal;

            if (reversedSlider)
            {
                newCurrentVal = maxVal - currentVal;
            }

            SetWaterSliderValue(newCurrentVal, maxVal);
        }
    }
}
