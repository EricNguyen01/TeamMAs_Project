// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TeamMAsTD
{
    [DisallowMultipleComponent]
    public class LoadGameButtonBlurAndDisableIfNoSaveData : MonoBehaviour
    {
        [SerializeField] private MenuButton loadGameButton;

        private void OnEnable()
        {
            if(!loadGameButton) TryGetComponent<MenuButton>(out loadGameButton);

            if (!loadGameButton)
            {
                enabled = false;

                return;
            }
        }

        public void SetLoadButtonBlurDirectlyOnSaveDeleted(bool saveDeleted)
        {
            if (!saveDeleted) return;

            ToggleLoadButtonBlurAndDisableDependsOnSaveDataExistence();
        }

        public void ToggleLoadButtonBlurAndDisableDependsOnSaveDataExistence()
        {
            if (!enabled || !loadGameButton) return;

            if (!SaveLoadHandler.HasSavedData())
            {
                loadGameButton.DisableButton(true, 0.4f);

                return;
            }

            loadGameButton.DisableButton(false);
        }
    }
}
