// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

namespace TeamMAsTD
{
    [DisallowMultipleComponent]
    public abstract class DropdownMenuUI : MonoBehaviour
    {
        [SerializeField] protected TMP_Dropdown dropdown;

        [SerializeField] protected bool showDebugLog = false;

        protected List<TMP_Dropdown.OptionData> optionItems = new List<TMP_Dropdown.OptionData>();

        protected virtual void Awake()
        {
            if (!dropdown) TryGetComponent<TMP_Dropdown>(out dropdown);

            if (!dropdown)
            {
                enabled = false;

                return;
            }

            SetupOptionsList();
        }

        protected virtual void OnEnable()
        {
            if (dropdown)
            {
                dropdown.onValueChanged.AddListener((int i) => OnDropdownOptionValueChanged(i));
            }
        }

        protected virtual void Start() { }

        protected virtual void OnDisable()
        {
            if (dropdown)
            {
                dropdown.onValueChanged.RemoveListener((int i) => OnDropdownOptionValueChanged(i));
            }
        }

        protected virtual void OnDestroy() { }

        protected virtual void SetupOptionsList()
        {
            if (!enabled || !dropdown) return;

            dropdown.ClearOptions();

            dropdown.AddOptions(optionItems);

            dropdown.RefreshShownValue();
        }

        protected virtual void SetupOptionsList(FullScreenMode fsMode)
        {
            if (!enabled || !dropdown) return;

            dropdown.ClearOptions();

            dropdown.AddOptions(optionItems);

            dropdown.RefreshShownValue();
        }

        protected virtual bool OnDropdownOptionValueChanged(int dropdownItemSlotIndexSelected)
        {
            if (!enabled) return false;

            if (!GameSettings.gameSettingsInstance) return false;

            if (dropdownItemSlotIndexSelected < 0 || dropdownItemSlotIndexSelected >= optionItems.Count) return false;

            return true;
        }
    }
}
