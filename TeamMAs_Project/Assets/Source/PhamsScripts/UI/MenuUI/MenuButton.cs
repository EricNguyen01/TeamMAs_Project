// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using System.Collections;
using System.Collections.Generic;
using TeamMAsTD;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TeamMAsTD
{
    [DisallowMultipleComponent]
    public class MenuButton : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Is this button a button for debug purposes only? " +
        "Debug button visibility and functionality will be hidden by default during runtime.")]
        private bool isDebugOnlyButton = false;

        [SerializeField]
        [Tooltip("If this button is a debug button, what is the key to toggle its visibility and functionality during runtime?" +
        "Default is F12.")]
        private KeyCode debugButtonToggleKey = KeyCode.F12;

        private bool debugButtonEnabled = false;

        private CanvasGroup buttonCanvasGroup;

        private void Awake()
        {
            TryGetComponent<CanvasGroup>(out buttonCanvasGroup);

            if (!buttonCanvasGroup)
            {
                buttonCanvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        private void OnEnable()
        {
            SaveLoadHandler.OnSavingStarted += () => DisableButton(true);

            SaveLoadHandler.OnLoadingStarted += () => DisableButton(true);

            SaveLoadHandler.OnSavingFinished += () => DisableButton(false);

            SaveLoadHandler.OnLoadingFinished += () => DisableButton(false);
        }

        private void OnDisable()
        {
            SaveLoadHandler.OnSavingStarted -= () => DisableButton(true);

            SaveLoadHandler.OnLoadingStarted -= () => DisableButton(true);

            SaveLoadHandler.OnSavingFinished -= () => DisableButton(false);

            SaveLoadHandler.OnLoadingFinished -= () => DisableButton(false);
        }

        private void Update()
        {
            if (!isDebugOnlyButton) return;

            if (buttonCanvasGroup && !debugButtonEnabled && buttonCanvasGroup.alpha > 0.0f) DisableButton(true, 0.0f);

            if(Input.GetKeyDown(debugButtonToggleKey))
            {
                if (!debugButtonEnabled)
                {
                    debugButtonEnabled = true;

                    DisableButton(false);

                    return;
                }

                debugButtonEnabled = false;

                DisableButton(true, 0.0f);
            }
        }

        public void NewGameButton()
        {
            if (!PersistentSceneLoadUI.persistentSceneLoadUIInstance)
            {
                Debug.LogWarning("Persistent Scene Load UI Object not found. Scene Load Functionality Won't Work!");

                return;
            }

            if (!SaveLoadHandler.saveLoadHandlerInstance) SaveLoadHandler.CreateSaveLoadManagerInstance();

            SaveLoadHandler.saveLoadHandlerInstance.DeleteAllSaveData();

            PersistentSceneLoadUI.persistentSceneLoadUIInstance.AllowLoadSaveAfterSceneLoaded(false);

            //load scene with build index 1 which is the 1st game scene after scene build index 0 which is the main menu scene.
            PersistentSceneLoadUI.persistentSceneLoadUIInstance.LoadFirstGameScene();
        }

        public void LoadGameButton()
        {
            if (!PersistentSceneLoadUI.persistentSceneLoadUIInstance)
            {
                Debug.LogWarning("Persistent Scene Load UI Object not found. Scene Load Functionality Won't Work!");

                return;
            }

            if (!SaveLoadHandler.HasSavedData())
            {
                NewGameButton();

                return;
            }

            SaveLoadHandler.EnableSaveLoad(true);

            PersistentSceneLoadUI.persistentSceneLoadUIInstance.AllowLoadSaveAfterSceneLoaded(true);

            //for now, we hard code the scene to load because we only have 1 main game scene.
            //if later, there are going to be multiple game scenes, create a new scene save/load system for this

            PersistentSceneLoadUI.persistentSceneLoadUIInstance.LoadScene(1);
        }

        public void SettingButton()
        {
            //settings button in menu to open settings sub-menu
        }

        public void BackToMainMenuButton()
        {
            if (PersistentSceneLoadUI.persistentSceneLoadUIInstance)
            {
                PersistentSceneLoadUI.persistentSceneLoadUIInstance.LoadScene(0);

                SaveLoadHandler.SaveAllSaveables();

                PersistentSceneLoadUI.persistentSceneLoadUIInstance.AllowLoadSaveAfterSceneLoaded(true);
            }
        }

        public void QuitGameButton()
        {
            Application.Quit();
        }

        public void DEBUG_DeleteAllSaveDataButton()
        {
            if (!isDebugOnlyButton) return;

            if (!debugButtonEnabled) return;

            if (!SaveLoadHandler.saveLoadHandlerInstance) SaveLoadHandler.CreateSaveLoadManagerInstance();

            SaveLoadHandler.saveLoadHandlerInstance.DeleteAllSaveData();

            if (GameSettings.gameSettingsInstance)
            {
                GameSettings.gameSettingsInstance.DeleteAllGameSettingsSavedData();
            }

            LoadGameButtonBlurAndDisableIfNoSaveData blurLoadGameButtonComp = transform.parent.GetComponentInChildren<LoadGameButtonBlurAndDisableIfNoSaveData>(true);

            if (blurLoadGameButtonComp) blurLoadGameButtonComp.SetLoadButtonBlurDirectlyOnSaveDeleted(true);
        }

        public void DisableButton(bool disabled, float disableAlpha = 0.5f, float enableAlpha = 1.0f)
        {
            if (!buttonCanvasGroup) return;

            if (disabled)
            {
                buttonCanvasGroup.alpha = disableAlpha;

                buttonCanvasGroup.blocksRaycasts = false;

                buttonCanvasGroup.interactable = false;

                return;
            }

            buttonCanvasGroup.alpha = enableAlpha;

            buttonCanvasGroup.blocksRaycasts = true;

            buttonCanvasGroup.interactable = true;
        }
    }
}
