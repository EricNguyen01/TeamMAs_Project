// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace TeamMAsTD
{
    [DisallowMultipleComponent]
    public class GameResourceSliderUI : GameResourceUI
    {
        [Header("Game Resource Slider UI Components")]

        [SerializeField] private Slider gameResourceSlider;

        [SerializeField] private Image sliderFill;

        [SerializeField] private Image sliderHandle;

        [System.Serializable]
        private struct DynamicSliderVisualChangeBasedOnSliderPercentage
        {
            public Sprite handleSprite;
            public Color sliderHandleColor;
            public Color sliderFillColor;

            [Tooltip("The percentage number should be something like: 10, 20, 30, etc. Don't use fraction like 0.1, 0.2, etc...!")]
            public float changeOnSliderPercentReached;
        }

        [Header("Game Resource Slider Config")]

        [SerializeField] private Sprite startingHandleSprite;

        [SerializeField] private Color startingSliderHandleColor;

        [SerializeField] private Color startingSliderFillColor;

        [SerializeField]
        [Tooltip("The percentage number should be something like: 10, 20, 30, etc. Don't use fraction like 0.1, 0.2, etc...!")]
        private DynamicSliderVisualChangeBasedOnSliderPercentage[] sliderVisualChangeBasedOnSliderPercentages;

        protected override void Awake()
        {
            base.Awake();

            OrderSliderPercentVisualChangeArray();

            SetStartingSliderVisualOnAwake();
        }

        protected override void OnEnable()
        {
            GameResourceSO.OnResourceAmountUpdated += DisplayResourceSlider;

            base.OnEnable();
        }

        protected override void OnDisable()
        {
            GameResourceSO.OnResourceAmountUpdated -= DisplayResourceSlider;

            base.OnDisable();
        }

        protected override void Start()
        {
            base.Start();

            DisplayResourceSlider(gameResourceSO);
        }

        private void DisplayResourceSlider(GameResourceSO resourceSO)
        {
            if (gameResourceSO == null) return;

            if (resourceSO != gameResourceSO) return;

            if(gameResourceSlider == null)
            {
                if (showDebugAndErrorLog) Debug.LogError("GameResourceSliderUI: " + name + " is missing resource slider component reference!");

                return;
            }

            if (gameResourceSlider.minValue != gameResourceSO.resourceAmountMin) gameResourceSlider.minValue = gameResourceSO.resourceAmountMin;

            if (gameResourceSlider.maxValue != gameResourceSO.resourceAmountCap) gameResourceSlider.maxValue = gameResourceSO.resourceAmountCap;

            gameResourceSlider.value = gameResourceSO.resourceAmount;

            DynamicallyChangingSliderVisualBasedOnPercentage();
        }

        protected override void GameResourceUpdateStatPopupOnUI(GameResourceSO gameResourceSO)
        {
            if (gameResourceUIStatPopupSpawner == null) return;

            if (this.gameResourceSO == null || gameResourceSO == null) return;

            if (this.gameResourceSO != gameResourceSO) return;

            //update and override popup position to match slider current position below here...
            if (sliderHandle != null)
            {
                Vector3 popupPos = new Vector3(sliderHandle.transform.position.x, sliderHandle.transform.position.y, gameResourceUIStatPopupSpawner.transform.position.z);

                gameResourceUIStatPopupSpawner.transform.position = popupPos;
            }

            //process popup in base function in GameResourceUI.cs
            base.GameResourceUpdateStatPopupOnUI(gameResourceSO);
        }

        private void OrderSliderPercentVisualChangeArray()
        {
            if (sliderVisualChangeBasedOnSliderPercentages == null || sliderVisualChangeBasedOnSliderPercentages.Length == 0) return;

            sliderVisualChangeBasedOnSliderPercentages = sliderVisualChangeBasedOnSliderPercentages.OrderBy(x => x.changeOnSliderPercentReached).ToArray();
        }

        private void DynamicallyChangingSliderVisualBasedOnPercentage()
        {
            if (sliderVisualChangeBasedOnSliderPercentages == null || sliderVisualChangeBasedOnSliderPercentages.Length == 0) return;

            float sliderAtPercentage = (gameResourceSlider.value / gameResourceSlider.maxValue) * 100.0f;

            Sprite currentHandleSprite = null;

            Color sliderFillColor = Color.clear;

            Color sliderHandleColor = Color.clear;

            if(sliderFill != null) sliderFillColor = sliderFill.color;

            if (sliderHandle != null) 
            { 
                currentHandleSprite = sliderHandle.sprite;

                sliderHandleColor = sliderHandle.color; 
            }

            //slider visual change based on percent array needs to be sorted based on percentage which has and should be done in Awake().
            for(int i = 0; i < sliderVisualChangeBasedOnSliderPercentages.Length; i++)
            {
                if(sliderAtPercentage < sliderVisualChangeBasedOnSliderPercentages[i].changeOnSliderPercentReached)
                {
                    break;
                }

                if(sliderAtPercentage >= sliderVisualChangeBasedOnSliderPercentages[i].changeOnSliderPercentReached)
                {
                    if (sliderVisualChangeBasedOnSliderPercentages[i].handleSprite != null)
                    {
                        currentHandleSprite = sliderVisualChangeBasedOnSliderPercentages[i].handleSprite;
                    }
                    if (sliderVisualChangeBasedOnSliderPercentages[i].sliderHandleColor != Color.clear)
                    {
                        sliderHandleColor = sliderVisualChangeBasedOnSliderPercentages[i].sliderHandleColor;
                    }
                    if (sliderVisualChangeBasedOnSliderPercentages[i].sliderFillColor != Color.clear)
                    {
                        sliderFillColor = sliderVisualChangeBasedOnSliderPercentages[i].sliderFillColor;
                    }
                }
            }

            SetSliderVisual(currentHandleSprite, sliderHandleColor, sliderFillColor);
        }

        private void SetSliderVisual(Sprite handleSprite, Color handleColor, Color sliderFillColor)
        {
            if (sliderFill != null) sliderFill.color = sliderFillColor;

            if (sliderHandle != null)
            {
                sliderHandle.sprite = handleSprite;

                sliderHandle.color = handleColor;
            }
        }

        private void SetStartingSliderVisualOnAwake()
        {
            if(sliderHandle != null)
            {
                if (startingHandleSprite != null) sliderHandle.sprite = startingHandleSprite;
                else startingHandleSprite = sliderHandle.sprite;

                if (startingSliderHandleColor != Color.clear) sliderHandle.color = startingSliderHandleColor;
                else startingSliderHandleColor = sliderHandle.color;
            }

            if(sliderFill != null)
            {
                if(startingSliderFillColor != Color.clear) sliderFill.color = startingSliderFillColor;
                else startingSliderFillColor = sliderFill.color;
            }
        }
    }
}
