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

        public void NewGameButton()
        {
            if (!PersistentSceneLoadUI.persistentSceneLoadUIInstance)
            {
                Debug.LogWarning("Persistent Scene Load UI Object not found. Scene Load Functionality Won't Work!");

                return;
            }

            if (SaveLoadHandler.saveLoadHandlerInstance && SaveLoadHandler.HasSavedData())
            {
                SaveLoadHandler.saveLoadHandlerInstance.DeleteAllSaveData();
            }

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

        private void DisableButton(bool disabled)
        {
            if (!buttonCanvasGroup) return;

            if (disabled)
            {
                buttonCanvasGroup.alpha = 0.5f;

                buttonCanvasGroup.blocksRaycasts = false;

                buttonCanvasGroup.interactable = false;

                return;
            }

            buttonCanvasGroup.alpha = 1.0f;

            buttonCanvasGroup.blocksRaycasts = true;

            buttonCanvasGroup.interactable = true;
        }
    }
}
