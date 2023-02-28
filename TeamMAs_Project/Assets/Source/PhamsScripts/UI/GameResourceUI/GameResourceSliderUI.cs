using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TeamMAsTD
{
    public class GameResourceSliderUI : GameResourceUI
    {
        [Header("Game Resource Slider UI Components")]

        [SerializeField] private Slider gameResourceSlider;

        protected override void Awake()
        {
            base.Awake();

            DisplayResourceSlider(gameResourceSO);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
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

            if(gameResourceSlider.minValue != gameResourceSO.resourceAmountBase) gameResourceSlider.minValue = 0.0f;

            if(gameResourceSlider.maxValue != gameResourceSO.resourceAmountCap) gameResourceSlider.maxValue = gameResourceSO.resourceAmountCap;

            gameResourceSlider.value = gameResourceSO.resourceAmount;
        }

        protected override void GameResourceUpdateStatPopupOnUI(GameResourceSO gameResourceSO)
        {
            //update and override popup position to match slider current position below here...

            //process popup in base function in GameResourceUI.cs
            base.GameResourceUpdateStatPopupOnUI(gameResourceSO);
        }
    }
}
