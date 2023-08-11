// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gameframe.SaveLoad;
using System;

namespace TeamMAsTD
{
    public class SaveLoadHandler : MonoBehaviour
    {
        [SerializeField] private SaveLoadManager saveLoadManager;

        [SerializeField] private bool disableSaveLoad = false;

        private const string SAVE_FILE_NAME = "GameSave";

        private const string BASE_FOLDER = "GameData";

        private const string DEFAULT_FOLDER = "SaveData";

        private const SerializationMethodType SERIALIZE_METHOD = SerializationMethodType.Default;

        private static SaveLoadHandler saveLoadHandlerInstance;

        public static event Action OnSavingStarted;

        public static event Action OnSavingFinished;

        public static event Action OnLoadingStarted;

        public static event Action OnLoadingFinished;

        private void Awake()
        {
            if (!saveLoadHandlerInstance)
            {
                saveLoadHandlerInstance = this;

                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);

                return;
            }
        }

        private void OnEnable()
        {
            if (!saveLoadManager) saveLoadManager = SaveLoadManager.Create(BASE_FOLDER, DEFAULT_FOLDER, SERIALIZE_METHOD);
        }

        //SAVE FUNCTIONALITIES....................................................................................................
        #region Save

        //SAVE ALL.......................................................................................
        public void SaveAllSaveables()
        {
            if (!saveLoadHandlerInstance || !saveLoadManager) return;

            if (disableSaveLoad) return;

            OnSavingStarted?.Invoke();

            Dictionary<string, object> latestDataToSave = UpdateCurrentSavedData(LoadFromFile());

            WriteToFile(latestDataToSave);

            OnSavingFinished?.Invoke();
        }

        private Dictionary<string, object> LoadFromFile()
        {
            object loadedData = saveLoadManager.Load<object>(SAVE_FILE_NAME);

            //if no saved data to load or saved data is not of the right type -> return an empty save dict as type object
            if(loadedData == null || loadedData is not Dictionary<string, object>) return new Dictionary<string, object>();

            //else, return the saved data dict
            return (Dictionary<string, object>)loadedData;
        }

        private Dictionary<string, object> UpdateCurrentSavedData(Dictionary<string, object> currentSavedData)
        {
            foreach (Saveable saveable in FindObjectsOfType<Saveable>())
            {
                UpdateCurrentSaveDataOfSaveable(currentSavedData, saveable);
            }

            return currentSavedData;
        }

        private void WriteToFile(Dictionary<string, object> latestSavedData)
        {
            if (!saveLoadHandlerInstance || !saveLoadManager) return;

            saveLoadManager.Save(latestSavedData, SAVE_FILE_NAME);
        }

        //SAVE SINGLE SAVEABLE ONLY......................................................................................
        public void SaveThisSaveableOnly(Saveable saveable)
        {
            if (!saveLoadHandlerInstance || !saveLoadManager) return;

            if (disableSaveLoad || !saveable) return;

            OnSavingStarted?.Invoke();

            Dictionary<string, object> latestDataToSave = UpdateCurrentSaveDataOfSaveable(LoadFromFile(), saveable);

            WriteToFile(latestDataToSave);

            OnSavingFinished?.Invoke();
        }

        private Dictionary<string, object> UpdateCurrentSaveDataOfSaveable(Dictionary<string, object> currentSavedData, Saveable saveable)
        {
            if (currentSavedData.ContainsKey(saveable.GetSaveableID()))
            {
                currentSavedData[saveable.GetSaveableID()] = saveable.CaptureSaveableState();

                return currentSavedData;
            }

            currentSavedData.Add(saveable.GetSaveableID(), saveable.CaptureSaveableState());

            return currentSavedData;
        }

        #endregion

        //LOAD............................................................................................................
        #region Load

        public void LoadToAllSaveables()
        {
            if (!saveLoadHandlerInstance || !saveLoadManager) return;

            if (disableSaveLoad) return;

            OnLoadingStarted?.Invoke();

            RestoreSavedDataForAllSaveables(LoadFromFile());

            OnLoadingFinished?.Invoke();
        }

        private void RestoreSavedDataForAllSaveables(Dictionary <string, object> savedData)
        {
            foreach (Saveable saveable in FindObjectsOfType<Saveable>())
            {
                RestoreSaveDataOfSaveable(savedData, saveable);
            }
        }

        //LOAD SINGLE SAVEABLE ONLY........................................................................
        public void LoadThisSaveableOnly(Saveable saveable)
        {
            if (!saveLoadHandlerInstance || !saveLoadManager) return;

            if (!saveable) return;

            OnLoadingStarted?.Invoke();

            RestoreSaveDataOfSaveable(LoadFromFile(), saveable);

            OnLoadingFinished?.Invoke();
        }

        private void RestoreSaveDataOfSaveable(Dictionary <string, object> savedData, Saveable saveable)
        {
            if (savedData.ContainsKey(saveable.GetSaveableID())) saveable.RestoreSaveableState(savedData[saveable.GetSaveableID()]);
        }

        #endregion

        //OTHERS..........................................................................................................
        #region DeleteSave

        public void DeleteAllSaveData()
        {
            if (!saveLoadHandlerInstance || !saveLoadManager) return;

            saveLoadManager.DeleteSave(SAVE_FILE_NAME);
        }

        public void DeleteSaveDataOfSaveable(Saveable saveable)
        {
            if (!saveLoadHandlerInstance || !saveLoadManager) return;

            if (!saveable) return;

            Dictionary<string, object> currentSavedData = LoadFromFile();

            if (!currentSavedData.ContainsKey(saveable.GetSaveableID())) return;

            currentSavedData.Remove(saveable.GetSaveableID());
        }

        #endregion

        #region Others

        public static void CreateSaveLoadManagerInstance()
        {
            if (saveLoadHandlerInstance) return;

            GameObject go = new GameObject("SaveLoadManager");

            go.AddComponent<SaveLoadHandler>();
        }

        #endregion
    }
}
