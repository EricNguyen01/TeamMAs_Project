// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gameframe.SaveLoad;

namespace TeamMAsTD
{
    public class SaveLoadHandler : MonoBehaviour
    {
        [SerializeField] private SaveLoadManager saveLoadManager;

        [SerializeField] private bool disableSaveLoad = false;

        [SerializeField] private bool showDebugLog = false;

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
        public static void SaveAllSaveables()
        {
            if (!saveLoadHandlerInstance || !saveLoadHandlerInstance.saveLoadManager) return;

            if (saveLoadHandlerInstance.disableSaveLoad) return;

            float saveStartTime = Time.realtimeSinceStartup;

            if (saveLoadHandlerInstance.showDebugLog) Debug.Log("Save All Started!");

            OnSavingStarted?.Invoke();

            foreach(ISaveable ISave in FindObjectsOfType(typeof(ISaveable)))
            {
                MonoBehaviour mono = ISave as MonoBehaviour;

                ISaveable.GenerateSaveableComponentIfNull(mono);
            }

            Dictionary<string, object> latestDataToSave = UpdateCurrentSavedData(LoadFromFile());

            WriteToFile(latestDataToSave);

            OnSavingFinished?.Invoke();

            float saveTime = Time.realtimeSinceStartup - saveStartTime;

            if (saveLoadHandlerInstance.showDebugLog) Debug.Log("Save All Finished! Time took: " + saveTime);
        }

        private static Dictionary<string, object> LoadFromFile()
        {
            object loadedData = saveLoadHandlerInstance.saveLoadManager.Load<object>(SAVE_FILE_NAME);

            //if no saved data to load or saved data is not of the right type -> return an empty save dict as type object
            if(loadedData == null || loadedData is not Dictionary<string, object>) return new Dictionary<string, object>();

            //else, return the saved data dict
            return (Dictionary<string, object>)loadedData;
        }

        private static Dictionary<string, object> UpdateCurrentSavedData(Dictionary<string, object> currentSavedData)
        {
            foreach (Saveable saveable in FindObjectsOfType<Saveable>())
            {
                UpdateCurrentSaveDataOfSaveable(currentSavedData, saveable);
            }

            return currentSavedData;
        }

        private static void WriteToFile(Dictionary<string, object> latestSavedData)
        {
            if (!saveLoadHandlerInstance || !saveLoadHandlerInstance.saveLoadManager) return;

            saveLoadHandlerInstance.saveLoadManager.Save(latestSavedData, SAVE_FILE_NAME);
        }

        //SAVE SINGLE SAVEABLE ONLY......................................................................................
        public static void SaveThisSaveableOnly(Saveable saveable)
        {
            if (!saveLoadHandlerInstance || !saveLoadHandlerInstance.saveLoadManager) return;

            if (saveLoadHandlerInstance.disableSaveLoad || !saveable) return;

            float saveStartTime = Time.realtimeSinceStartup;

            if (saveLoadHandlerInstance.showDebugLog) Debug.Log("Save " + saveable.name + " Started!");

            OnSavingStarted?.Invoke();

            Dictionary<string, object> latestDataToSave = UpdateCurrentSaveDataOfSaveable(LoadFromFile(), saveable);

            WriteToFile(latestDataToSave);

            OnSavingFinished?.Invoke();

            float saveTime = Time.realtimeSinceStartup - saveStartTime;

            if (saveLoadHandlerInstance.showDebugLog) Debug.Log("Save " + saveable.name + " Finished! Time took: " + saveTime);
        }

        private static Dictionary<string, object> UpdateCurrentSaveDataOfSaveable(Dictionary<string, object> currentSavedData, Saveable saveable)
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

        public static void LoadToAllSaveables()
        {
            if (!saveLoadHandlerInstance || !saveLoadHandlerInstance.saveLoadManager) return;

            if (saveLoadHandlerInstance.disableSaveLoad) return;

            float loadTimeStart = Time.realtimeSinceStartup;

            if (saveLoadHandlerInstance.showDebugLog) Debug.Log("Load Started!");

            OnLoadingStarted?.Invoke();

            RestoreSavedDataForAllSaveables(LoadFromFile());

            OnLoadingFinished?.Invoke();

            float loadTime = Time.realtimeSinceStartup - loadTimeStart;

            if (saveLoadHandlerInstance.showDebugLog) Debug.Log("Load Finished! Time took: " + loadTime);
        }

        private static void RestoreSavedDataForAllSaveables(Dictionary <string, object> savedData)
        {
            foreach (Saveable saveable in FindObjectsOfType<Saveable>())
            {
                RestoreSaveDataOfSaveable(savedData, saveable);
            }
        }

        //LOAD SINGLE SAVEABLE ONLY........................................................................
        public static void LoadThisSaveableOnly(Saveable saveable)
        {
            if (!saveLoadHandlerInstance || !saveLoadHandlerInstance.saveLoadManager) return;

            if (!saveable) return;

            float loadTimeStart = Time.realtimeSinceStartup;

            if (saveLoadHandlerInstance.showDebugLog) Debug.Log("Load " + saveable.name + " Started!");

            OnLoadingStarted?.Invoke();

            RestoreSaveDataOfSaveable(LoadFromFile(), saveable);

            OnLoadingFinished?.Invoke();

            float loadTime = Time.realtimeSinceStartup - loadTimeStart;

            if (saveLoadHandlerInstance.showDebugLog) Debug.Log("Load " + saveable.name + " Finished! Time took: " + loadTime);
        }

        private static void RestoreSaveDataOfSaveable(Dictionary <string, object> savedData, Saveable saveable)
        {
            if (savedData.ContainsKey(saveable.GetSaveableID())) saveable.RestoreSaveableState(savedData[saveable.GetSaveableID()]);
        }

        #endregion

        //OTHERS..........................................................................................................
        #region DeleteSave

        public static void DeleteAllSaveData()
        {
            if (!saveLoadHandlerInstance || !saveLoadHandlerInstance.saveLoadManager) return;

            saveLoadHandlerInstance.saveLoadManager.DeleteSave(SAVE_FILE_NAME);
        }

        public static void DeleteSaveDataOfSaveable(Saveable saveable)
        {
            if (!saveLoadHandlerInstance || !saveLoadHandlerInstance.saveLoadManager) return;

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

        //THIS FUNCTION IS CALLED IN WAVE SPAWNER'S WAVE STARTED/FINISHED UNITY EVENTS
        public static void EnableSaveLoad(bool enabled)
        {
            if (enabled) saveLoadHandlerInstance.disableSaveLoad = false;
            else saveLoadHandlerInstance.disableSaveLoad = true;
        }

        #endregion
    }
}
