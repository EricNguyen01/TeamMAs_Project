// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gameframe.SaveLoad;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TeamMAsTD
{
    [DisallowMultipleComponent]
    public class SaveLoadHandler : MonoBehaviour
    {
        [ReadOnlyInspectorPlayMode]
        [SerializeField] private SaveLoadManager saveLoadManager;

        [SerializeField] private bool disableSaveLoadAlways = false;

        [ReadOnlyInspector]
        [SerializeField]
        private bool disableSaveLoadRuntime = false;

        [SerializeField] private bool showDebugLog = true;

        [SerializeField] private bool showEditorDebugLog = true;

        private const string SAVE_FILE_NAME = "GameSave";

        private const string BASE_FOLDER = "GameData";

        private const string DEFAULT_FOLDER = "SaveData";

        private const SerializationMethodType SERIALIZE_METHOD = SerializationMethodType.Default;

        private static SaveLoadHandler saveLoadHandlerInstance;

        [Serializable]
        private class StateDictionaryObject
        {
            [field: SerializeReference] public Dictionary<string, object> stateDict { get; private set; } = new Dictionary<string, object>();

            public StateDictionaryObject(Dictionary<string, object> stateDict = null)
            {
                if (stateDict == null) return;

                this.stateDict = stateDict;
            }
        }

        [Serializable]
        private struct SavedSaveable
        {
            public string saveableName; public string saveableID;

            public SavedSaveable(string saveableName, string saveableID)
            {
                this.saveableName = saveableName;

                this.saveableID = saveableID;
            }
        }

        private List<SavedSaveable> savedSaveablesList = new List<SavedSaveable>();

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

            disableSaveLoadRuntime = false;
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

            if (saveLoadHandlerInstance.disableSaveLoadAlways || saveLoadHandlerInstance.disableSaveLoadRuntime) return;

            float saveStartTime = Time.realtimeSinceStartup;

            if (saveLoadHandlerInstance.showDebugLog) Debug.Log("Save All Started!");

            OnSavingStarted?.Invoke();

            StateDictionaryObject latestDataToSave = UpdateCurrentSavedData(LoadFromFile());

            WriteToFile(latestDataToSave);

            OnSavingFinished?.Invoke();

            float saveTime = Time.realtimeSinceStartup - saveStartTime;

            if (saveLoadHandlerInstance.showDebugLog) Debug.Log("Save All Finished! Time took: " + saveTime);
        }

        private static StateDictionaryObject LoadFromFile()
        {
            StateDictionaryObject loadedData = saveLoadHandlerInstance.saveLoadManager.Load<StateDictionaryObject>(SAVE_FILE_NAME);

            //if no saved data to load or saved data is not of the right type -> return an empty save dict as type object
            if(loadedData == null) return new StateDictionaryObject();

            //else, return the saved data dict
            return loadedData;
        }

        private static StateDictionaryObject UpdateCurrentSavedData(StateDictionaryObject currentSavedData)
        {
            foreach (Saveable saveable in FindObjectsOfType<Saveable>(true))
            {
                UpdateCurrentSaveDataOfSaveable(currentSavedData, saveable);
            }

            return currentSavedData;
        }

        private static void WriteToFile(StateDictionaryObject latestSavedData)
        {
            if (!saveLoadHandlerInstance || !saveLoadHandlerInstance.saveLoadManager) return;

            saveLoadHandlerInstance.saveLoadManager.Save(latestSavedData, SAVE_FILE_NAME);
        }

        //SAVE SINGLE SAVEABLE ONLY......................................................................................
        public static void SaveThisSaveableOnly(Saveable saveable)
        {
            if (!saveLoadHandlerInstance || !saveLoadHandlerInstance.saveLoadManager) return;

            if (saveLoadHandlerInstance.disableSaveLoadAlways || saveLoadHandlerInstance.disableSaveLoadRuntime) return;

            if (!saveable) return;

            float saveStartTime = Time.realtimeSinceStartup;

            if (saveLoadHandlerInstance.showDebugLog) Debug.Log("Save " + saveable.name + " Started!");

            OnSavingStarted?.Invoke();

            StateDictionaryObject latestDataToSave = UpdateCurrentSaveDataOfSaveable(LoadFromFile(), saveable);

            WriteToFile(latestDataToSave);

            OnSavingFinished?.Invoke();

            float saveTime = Time.realtimeSinceStartup - saveStartTime;

            if (saveLoadHandlerInstance.showDebugLog) Debug.Log("Save " + saveable.name + " Finished! Time took: " + saveTime);
        }

        private static StateDictionaryObject UpdateCurrentSaveDataOfSaveable(StateDictionaryObject currentSavedData, Saveable saveable)
        {
            if(currentSavedData == null)
            {
                if(saveLoadHandlerInstance.showDebugLog) Debug.LogError("CurrentSaveDataNull!"); return currentSavedData;
            }

            if(currentSavedData.stateDict == null)
            {
                if (saveLoadHandlerInstance.showDebugLog) Debug.LogError("StateDictNull!"); return currentSavedData;
            }

            if (!saveable)
            {
                if (saveLoadHandlerInstance.showDebugLog) Debug.LogError("SaveableNull!"); return currentSavedData;
            }

            if (currentSavedData.stateDict.ContainsKey(saveable.GetSaveableID()))
            {
                currentSavedData.stateDict[saveable.GetSaveableID()] = saveable.CaptureSaveableState();

                return currentSavedData;
            }

            currentSavedData.stateDict.Add(saveable.GetSaveableID(), saveable.CaptureSaveableState());

            saveLoadHandlerInstance.savedSaveablesList.Add(new SavedSaveable(saveable.name, saveable.GetSaveableID()));

            return currentSavedData;
        }

        #endregion

        //LOAD............................................................................................................
        #region Load

        public static void LoadToAllSaveables()
        {
            if (!saveLoadHandlerInstance || !saveLoadHandlerInstance.saveLoadManager) return;

            if (saveLoadHandlerInstance.disableSaveLoadAlways || saveLoadHandlerInstance.disableSaveLoadRuntime) return;

            if (!HasSavedData())
            {
                if (saveLoadHandlerInstance.showDebugLog) Debug.LogWarning("No Saved Data To Load!");

                return;
            }

            float loadTimeStart = Time.realtimeSinceStartup;

            if (saveLoadHandlerInstance.showDebugLog) Debug.Log("Load Started!");

            OnLoadingStarted?.Invoke();

            RestoreSavedDataForAllSaveables(LoadFromFile());

            OnLoadingFinished?.Invoke();

            float loadTime = Time.realtimeSinceStartup - loadTimeStart;

            if (saveLoadHandlerInstance.showDebugLog) Debug.Log("Load Finished! Time took: " + loadTime);
        }

        private static void RestoreSavedDataForAllSaveables(StateDictionaryObject savedData)
        {
            foreach (Saveable saveable in FindObjectsOfType<Saveable>(true))
            {
                RestoreSaveDataOfSaveable(savedData, saveable);
            }
        }

        //LOAD SINGLE SAVEABLE ONLY........................................................................
        public static void LoadThisSaveableOnly(Saveable saveable)
        {
            if (!saveLoadHandlerInstance || !saveLoadHandlerInstance.saveLoadManager) return;

            if (saveLoadHandlerInstance.disableSaveLoadAlways || saveLoadHandlerInstance.disableSaveLoadRuntime) return;

            if (!saveable) return;

            float loadTimeStart = Time.realtimeSinceStartup;

            if (saveLoadHandlerInstance.showDebugLog) Debug.Log("Load " + saveable.name + " Started!");

            OnLoadingStarted?.Invoke();

            RestoreSaveDataOfSaveable(LoadFromFile(), saveable);

            OnLoadingFinished?.Invoke();

            float loadTime = Time.realtimeSinceStartup - loadTimeStart;

            if (saveLoadHandlerInstance.showDebugLog) Debug.Log("Load " + saveable.name + " Finished! Time took: " + loadTime);
        }

        private static void RestoreSaveDataOfSaveable(StateDictionaryObject savedData, Saveable saveable)
        {
            if (savedData.stateDict.ContainsKey(saveable.GetSaveableID()))
            {
                if (saveLoadHandlerInstance.showDebugLog)
                {
                    Debug.LogWarning("Saveable: " + saveable.name + " Has Matching Key! Load This Saveable's Saved Data!");
                }

                saveable.RestoreSaveableState(savedData.stateDict[saveable.GetSaveableID()]);
            }
        }

        #endregion

        //OTHERS..........................................................................................................
        #region DeleteSave

        public void DeleteAllSaveData()
        {
            if (!saveLoadManager) return;

            if (showDebugLog) Debug.Log("Deleting All Save Data!");

            saveLoadManager.DeleteSave(SAVE_FILE_NAME);
        }

        public void DeleteSaveDataOfSaveable(Saveable saveable)
        {
            if (!saveLoadManager) return;

            if (!saveable) return;

            StateDictionaryObject currentSavedData = LoadFromFile();

            if (!currentSavedData.stateDict.ContainsKey(saveable.GetSaveableID())) return;

            currentSavedData.stateDict.Remove(saveable.GetSaveableID());
        }

        #endregion

        #region Others

        public static void CreateSaveLoadManagerInstance()
        {
            if (saveLoadHandlerInstance) return;

            GameObject go = new GameObject("SaveLoadManager");

            go.AddComponent<SaveLoadHandler>();
        }

        public static bool HasSavedData()
        {
            return SaveLoadUtility.Exists(SAVE_FILE_NAME, DEFAULT_FOLDER, BASE_FOLDER);
        }

        //THIS FUNCTION IS CALLED IN WAVE SPAWNER'S WAVE STARTED/FINISHED UNITY EVENTS
        public static void EnableSaveLoad(bool enabled)
        {
            if (enabled) saveLoadHandlerInstance.disableSaveLoadRuntime = false;
            else saveLoadHandlerInstance.disableSaveLoadRuntime = true;
        }

        #endregion

        #region Editor

#if UNITY_EDITOR

        [CustomEditor(typeof(SaveLoadHandler))]
        private class SaveLoadHandlerEditor : Editor
        {
            private SaveLoadHandler saveLoadHandler;

            private void OnEnable()
            {
                saveLoadHandler = target as SaveLoadHandler;
            }

            public override void OnInspectorGUI()
            {
                DrawDefaultInspector();

                EditorGUILayout.Space();

                EditorGUILayout.HelpBox(
                    "Use this button to test the load functionality during RUNTIME ONLY." +
                    "If any save file exists on disk, it will be loaded.",
                    MessageType.Warning);

                EditorGUILayout.Space();

                if (GUILayout.Button("Test Load Save"))
                {
                    if (!HasSavedData())
                    {
                        if (saveLoadHandler.showEditorDebugLog) Debug.LogWarning("No Saved Data To Load!");

                        return;
                    }

                    if (!Application.isPlaying)
                    {
                        if (saveLoadHandler.showEditorDebugLog) Debug.LogWarning("Test Load Only Available On Runtime!");

                        return;
                    }

                    LoadToAllSaveables();
                }

                EditorGUILayout.Space();

                if(GUILayout.Button("Delete All Saved Data"))
                {
                    if (!HasSavedData())
                    {
                        if (saveLoadHandler.showEditorDebugLog) Debug.LogWarning("No Saved Data To Delete!");

                        return;
                    }

                    saveLoadHandler.DeleteAllSaveData();
                }
            }
        }
#endif

        #endregion
    }
}
