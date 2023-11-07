// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TeamMAsTD
{
    public class FullscreenModeDropdownMenuUI : DropdownMenuUI
    {
        [Serializable]
        private struct FullscreenModeOptionItem
        {
            private FullScreenMode fullScreenMode;

            private string fullScreenModeString;

            public void InitFullscreenModeOptionItem(int fsModeNum)
            {
                if (fsModeNum == (int)FullScreenMode.FullScreenWindow) fullScreenMode = FullScreenMode.FullScreenWindow;
                //do not use ExclusiveFullScreen. It sucks! Switch to FullscreenWindow instead
                else if (fsModeNum == (int)FullScreenMode.ExclusiveFullScreen) fullScreenMode = FullScreenMode.FullScreenWindow;
                //do not use MaximizedWindow as it's not supported on Windows
                else if (fsModeNum == (int)FullScreenMode.MaximizedWindow) fullScreenMode = FullScreenMode.FullScreenWindow;
                else if (fsModeNum == (int)FullScreenMode.Windowed) fullScreenMode = FullScreenMode.Windowed;
                else fullScreenMode = FullScreenMode.FullScreenWindow;

                fullScreenModeString = fullScreenMode.ToString();
            }

            public FullScreenMode GetFullScreenMode() { return fullScreenMode; }

            public string GetFullScreenModeString() { return fullScreenModeString; }
        }

        private List<FullscreenModeOptionItem> fullScreenModeOptionItems = new List<FullscreenModeOptionItem>();

        protected override void OnEnable()
        {
            base.OnEnable();

            GameSettings.OnGameSettingsFinishLoading += SetFullScreenOptionDisplayToCurrentFullScreenMode;
        }

        protected override void SetupOptionsList()
        {
            if (!enabled) return;

            //count is only 4 because Unity only supports 4 window modes
            for(int i = 0; i < 4; i++)
            {
                //Do not include ExclusiveFullScreen as an option item in list. It sucks!
                if (i == (int)FullScreenMode.ExclusiveFullScreen) continue;

                //Do not include MaximizedWindow as it's not supported on Windows.
                if (i == (int)FullScreenMode.MaximizedWindow) continue;

                FullscreenModeOptionItem fsModeItem = new FullscreenModeOptionItem();

                fsModeItem.InitFullscreenModeOptionItem(i);

                fullScreenModeOptionItems.Add(fsModeItem);

                TMP_Dropdown.OptionData optionItemData = new TMP_Dropdown.OptionData(fsModeItem.GetFullScreenModeString(), null);

                optionItems.Add(optionItemData);
            }
            
            base.SetupOptionsList();
        }

        protected override bool OnDropdownOptionValueChanged(int dropdownItemSlotIndexSelected)
        {
            if (!base.OnDropdownOptionValueChanged(dropdownItemSlotIndexSelected)) return false;

            FullScreenMode fsModeToSet = fullScreenModeOptionItems[dropdownItemSlotIndexSelected].GetFullScreenMode();

            GameSettings.gameSettingsInstance.SetScreenMode(fsModeToSet);

            return true;
        }

        private void SetFullScreenOptionDisplayToCurrentFullScreenMode()
        {
            if (!enabled || !dropdown) return;

            if(dropdown.options == null || dropdown.options.Count == 0) return;

            if (fullScreenModeOptionItems.Count == 0 || optionItems.Count == 0) return;

            //if the fullscreen mode dropdown option item UI display is already set to the current fullscreen mode -> exit coroutine
            if(dropdown.value >= 0 && dropdown.value < optionItems.Count)
            {
                if (Screen.fullScreenMode == fullScreenModeOptionItems[dropdown.value].GetFullScreenMode()) return;
            }

            for (int i = 0; i < fullScreenModeOptionItems.Count; i++)
            {
                if (Screen.fullScreenMode == fullScreenModeOptionItems[i].GetFullScreenMode())
                {
                    if (i >= 0 && i < dropdown.options.Count) dropdown.SetValueWithoutNotify(i);

                    break;
                }
            }

            dropdown.RefreshShownValue();
        }
    }
}
