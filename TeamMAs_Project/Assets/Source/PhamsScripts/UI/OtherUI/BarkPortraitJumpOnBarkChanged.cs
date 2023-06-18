using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TeamMAsTD
{
    public class BarkPortraitJumpOnBarkChanged : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI textMeshProUGUI;

        private float checkCycleDuration = 0.5f;

        private float currentCheckTime = 0.0f;

        private string textMeshProText;

        private UIJumpFx UI_JumpFX;

        private UIShakeFx parentUIShakeFx;

        private void Awake()
        {
            if (!textMeshProUGUI)
            {
                textMeshProUGUI = GetComponent<TextMeshProUGUI>();
            }

            if(!textMeshProUGUI)
            {
                enabled = false;

                return;
            }

            UI_JumpFX = GetComponent<UIJumpFx>();

            if(!UI_JumpFX) UI_JumpFX = gameObject.AddComponent<UIJumpFx>();

            parentUIShakeFx = GetComponentInParent<UIShakeFx>();
        }

        private void Start()
        {
            currentCheckTime = checkCycleDuration;

            if (!textMeshProUGUI) return;

            textMeshProText = textMeshProUGUI.text;
        }

        private void Update()
        {
            if (!textMeshProUGUI)
            {
                enabled = false;

                return;
            }

            if (currentCheckTime < checkCycleDuration)
            {
                currentCheckTime += Time.deltaTime;
            }
            else
            {
                if (textMeshProUGUI.text != textMeshProText)
                {
                    UI_JumpFX.RunTweenInternal();

                    if (parentUIShakeFx) parentUIShakeFx.RunTweenInternal();

                    textMeshProText = textMeshProUGUI.text;
                }

                currentCheckTime = 0.0f;
            }
        }
    }
}
