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
    public class ResolutionDropdownMenuUI : DropdownSettingsMenuUI
    {
        [Serializable]
        private struct ResolutionOptionItem
        {
            private Resolution resolution;

            private string screenWidthString;

            private string screenHeightString;

            private string resolutionString;

            public void InitResolutionOptionItem(Resolution resolution)
            {
                if(resolution.width < 0) resolution.width = 0;

                if(resolution.height < 0) resolution.height = 0;

                this.resolution = resolution;

                screenWidthString = resolution.width.ToString();

                screenHeightString = resolution.height.ToString();

                resolutionString = screenWidthString + "x" + screenHeightString;
            }

            public Resolution GetResolution() { return resolution; }

            public string GetResolutionAsString() { return resolutionString; }
        }

        private List<ResolutionOptionItem> resolutionOptions = new List<ResolutionOptionItem>();

        protected override void OnEnable()
        {
            base.OnEnable();

            GameSettings.OnScreenResolutionChanged -= (Resolution res) => SetResolutionOptionDisplayToCurrentResolution(res);

            GameSettings.OnScreenResolutionChanged += (Resolution res) => SetResolutionOptionDisplayToCurrentResolution(res);
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            GameSettings.OnScreenResolutionChanged -= (Resolution res) => SetResolutionOptionDisplayToCurrentResolution(res);
        }

        protected override void SetupOptionsList()
        {
            if (!enabled) return;

            if (Screen.resolutions == null || Screen.resolutions.Length == 0) return;

            if (resolutionOptions.Count > 0) resolutionOptions.Clear();

            if(optionItems.Count > 0) optionItems.Clear();

            if(showDebugLog) Debug.Log("Start Setting Up Resolution Option Items List!");

            for(int i = 0; i < Screen.resolutions.Length; i++)
            {
                if (showDebugLog)
                {
                    Debug.Log("Current Checking Resolution's Refresh Rate: " + Screen.resolutions[i].refreshRate + "\n" +
                              "Current Screen's Refresh Rate: " + ((int)Screen.mainWindowDisplayInfo.refreshRate.value).ToString());
                }

                if (Screen.resolutions[i].refreshRate != (int)Screen.mainWindowDisplayInfo.refreshRate.value) continue;

                //if (fsMode == FullScreenMode.Windowed && 
                    //Screen.resolutions[i].width == 1920 && Screen.resolutions[i].height == 1080) continue;
                
                ResolutionOptionItem resItem = new ResolutionOptionItem();

                resItem.InitResolutionOptionItem(Screen.resolutions[i]);

                resolutionOptions.Add(resItem);

                if(showDebugLog) Debug.Log("Added Available Resolution: " + Screen.resolutions[i].ToString());

                TMP_Dropdown.OptionData optionItemData = new TMP_Dropdown.OptionData(resItem.GetResolutionAsString(), null);

                optionItems.Add(optionItemData);
            }

            base.SetupOptionsList();
        }

        protected override bool OnDropdownOptionValueChanged(int dropdownItemSlotIndexSelected)
        {
            if (!base.OnDropdownOptionValueChanged(dropdownItemSlotIndexSelected)) return false;

            Resolution resolutionToSet = resolutionOptions[dropdownItemSlotIndexSelected].GetResolution();

            //set screen res without sending event since event is only sent when settings are changed from other places
            //and we need the resolution dropdown UI (this) to update its visuals accordingly.
            GameSettings.gameSettingsInstance.SetScreenResolution(resolutionToSet);

            return true;
        }

        private void SetResolutionOptionDisplayToCurrentResolution(Resolution currentRes, bool raiseDropdownEvent = false)
        {
            if(this != null)
            {
                if (!enabled || !dropdown) return;

                if (dropdown.options == null || dropdown.options.Count == 0) return;

                if (resolutionOptions.Count == 0 || optionItems.Count == 0) return;

                if (showDebugLog) Debug.Log("Start Setting Resolution Option Display After Resolution Setting Loaded!");

                if (currentRes.width == 0 && currentRes.height == 0) currentRes = Screen.currentResolution;

                //if the fullscreen mode dropdown option item UI display is already set to the current fullscreen mode -> exit func
                if (dropdown.value >= 0 && dropdown.value < optionItems.Count)
                {
                    Resolution currentResOptionDisplayed = resolutionOptions[dropdown.value].GetResolution();

                    if (currentRes.width == currentResOptionDisplayed.width &&
                        currentRes.height == currentResOptionDisplayed.height)
                    {
                        if (showDebugLog) Debug.Log("Resolution Option Item UI Display SAME As Current Loaded Resolution Setting!");

                        return;
                    }
                }

                for (int i = 0; i < resolutionOptions.Count; i++)
                {
                    Resolution resIteration = resolutionOptions[i].GetResolution();

                    if (currentRes.width == resIteration.width && currentRes.height == resIteration.height)
                    {
                        if (i >= 0 && i < dropdown.options.Count)
                        {
                            if (raiseDropdownEvent) dropdown.value = i;
                            else dropdown.SetValueWithoutNotify(i);
                        }

                        if (showDebugLog) Debug.Log("Match Resolution Option Item UI Display To: " + dropdown.options[i].text);

                        break;
                    }
                }

                dropdown.RefreshShownValue();
            }
        }
    }
}
