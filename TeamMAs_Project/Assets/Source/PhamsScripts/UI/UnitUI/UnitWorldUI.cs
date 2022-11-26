using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace TeamMAsTD
{
    public class UnitWorldUI : MonoBehaviour
    {
        [SerializeField] private Canvas unitWorldCanvas;
        [SerializeField] private TextMeshProUGUI nameTextMeshProComponent;

        private IUnit unitLinkedToUI;
        private UnitSO unitSO;

        private void Awake()
        {
            SetUpUnitWorldUI();
            TextUIDisplaysUnitName();
        }

        private void SetUpUnitWorldUI()
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

            unitWorldCanvas.worldCamera = Camera.main;
        }

        private void TextUIDisplaysUnitName()
        {
            if (unitSO == null) return;
            if (nameTextMeshProComponent == null) return;

            nameTextMeshProComponent.text = unitSO.displayName;

        }
    }
}
