// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using System;
using System.Collections.Generic;
using UnityEngine;
using Gameframe.SaveLoad;
using System.Linq;
using UnityEngine.SceneManagement;

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
        public bool disableSaveLoadRuntime { get; private set; } = false;

        [SerializeField] [Tooltip("Whether to only keep game progress save file existed during in-editor session or not. Does not affect build.")] 
        private bool deleteSaveOnEditorClosed = true;

        [SerializeField] private bool showDebugLog = false;

        [SerializeField] private bool showEditorDebugLog = false;

        private const string SAVE_FILE_NAME = "GameSave";

        private const string BASE_FOLDER = "GameData";

        private const string DEFAULT_FOLDER = "SaveData";

        private const SerializationMethodType SERIALIZE_METHOD = SerializationMethodType.Default;

        private HashSet<Saveable> saveablesInScene = new HashSet<Saveable>();   

        public static SaveLoadHandler saveLoadHandlerInstance;

        [Serializable]
        private class StateDictionaryObject
        {
            [field: SerializeReference] 
            public Dictionary<string, object> stateDict { get; private set; } = new Dictionary<string, object>();

            public StateDictionaryObject(Dictionary<string, object> stateDict = null)
            {
                if (stateDict == null)
                {
                    if(this.stateDict == null) this.stateDict = new Dictionary<string, object>();

                    return;
                }

                this.stateDict = stateDict;
            }

            public bool IsValid()
            {
                if(stateDict == null || stateDict.Count == 0) return false;

                return true;
            }

            public void Clear()
            {
                if (stateDict == null) return;

                stateDict.Clear();
            }
        }

        private StateDictionaryObject currentSaveData = new StateDictionaryObject();

        private SaveLoadProcessDelay saveLoadDelay;

        public static event Action OnSavingStarted;

        public static event Action OnSavingFinished;

        public static event Action OnLoadingStarted;

        public static event Action OnLoadingFinished;

        private void Awake()
        {
            if (saveLoadHandlerInstance && saveLoadHandlerInstance != this)
            {
                Destroy(gameObject);

                return;
            }

            saveLoadHandlerInstance = this;

            DontDestroyOnLoad(gameObject);

            saveLoadDelay = new SaveLoadProcessDelay(saveLoadHandlerInstance);

            if (!Application.isEditor)
            {
                showDebugLog = false;

                showEditorDebugLog = false;
            }

            SetSaveLoadManagerReferenceIfMissing();

            if(disableSaveLoadAlways) 
                disableSaveLoadRuntime = true;
            else 
                disableSaveLoadRuntime = false;

            SceneManager.sceneLoaded += (Scene sc, LoadSceneMode loadMode) => UpdateSaveablesListOnSceneLoaded();
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= (Scene sc, LoadSceneMode loadMode) => UpdateSaveablesListOnSceneLoaded();
        }

        private void OnApplicationQuit()
        {
            if (Application.isEditor && deleteSaveOnEditorClosed) DeleteAllSaveData();
        }

        public static void RegisterSaveable(Saveable saveable)
        {
            if (!saveLoadHandlerInstance) return;

            if (!saveable) return;

            if (!saveLoadHandlerInstance.saveablesInScene.Contains(saveable))
                    saveLoadHandlerInstance.saveablesInScene.Add(saveable);
        }

        public static void DeRegisterSaveable(Saveable saveable)
        {
            if (!saveLoadHandlerInstance) return;

            if (!saveable) return;

            if (saveLoadHandlerInstance.saveablesInScene.Contains(saveable))
                    saveLoadHandlerInstance.saveablesInScene.Remove(saveable);
        }

        private void UpdateSaveablesListOnSceneLoaded()
        {
            if(saveablesInScene.Count > 0)
            {
                saveablesInScene.Clear();
            }

            foreach (Saveable saveable in FindObjectsByType<Saveable>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                RegisterSaveable(saveable);
            }
        }

        //SAVE FUNCTIONALITIES....................................................................................................
        #region Save

        //SAVE ALL.......................................................................................

        public void SaveAllSaveables_UnityEventOnly()
        {
            SaveAllSaveables();
        }

        public static bool SaveAllSaveables()
        {
            return SaveMultiSaveables(saveLoadHandlerInstance.saveablesInScene.ToArray());
        }

        public static bool SaveMultiSaveables(Saveable[] saveablesToSave)
        {
            if (!Application.isPlaying) return false;

            if (!saveLoadHandlerInstance || !saveLoadHandlerInstance.saveLoadManager) return false;

            if (!saveLoadHandlerInstance.enabled) return false;

            if (saveLoadHandlerInstance.disableSaveLoadAlways || saveLoadHandlerInstance.disableSaveLoadRuntime) return false;

            if(saveablesToSave == null || saveablesToSave.Length == 0) return false;

            float saveStartTime = Time.realtimeSinceStartup;

            if (saveLoadHandlerInstance.showDebugLog) Debug.Log("Save Multi-Saveables Started!");

            if (saveLoadHandlerInstance.saveLoadDelay == null)
            {
                OnSavingStarted?.Invoke();
            }
            else
            {
                if (!saveLoadHandlerInstance.saveLoadDelay.isSavingSaveables) OnSavingStarted?.Invoke();

                if (saveLoadHandlerInstance.saveLoadDelay.IsDelayingSave())
                {
                    saveLoadHandlerInstance.saveLoadDelay.SaveDelayScheduled(saveablesToSave);

                    return true;
                }
            }

            if (saveLoadHandlerInstance.currentSaveData != null &&
               saveLoadHandlerInstance.currentSaveData.IsValid())
            {
                saveLoadHandlerInstance.currentSaveData = UpdateCurrentSavedData(saveLoadHandlerInstance.currentSaveData);
            }
            else saveLoadHandlerInstance.currentSaveData = UpdateCurrentSavedData(LoadFromFile());

            WriteToFile(saveLoadHandlerInstance.currentSaveData);

            OnSavingFinished?.Invoke();
            
            float saveTime = Time.realtimeSinceStartup - saveStartTime;

            if (saveLoadHandlerInstance.showDebugLog) Debug.Log("Save Multi-Saveables Finished! Time took: " + saveTime);

            return true;
        }

        private static StateDictionaryObject LoadFromFile()
        {
            StateDictionaryObject loadedData = saveLoadHandlerInstance.saveLoadManager.Load<StateDictionaryObject>(SAVE_FILE_NAME);

            //if no saved data to load or saved data is not of the right type -> return an empty save dict as type object
            if(loadedData == null) return new StateDictionaryObject();

            //else, return the saved data dict
            return loadedData;
        }

        private static StateDictionaryObject UpdateCurrentSavedData(StateDictionaryObject currentSavedData, Saveable[] saveablesToSave = null)
        {
            StateDictionaryObject latestDataToSave = null;

            if(saveablesToSave == null || saveablesToSave.Length == 0)
            {
                if(saveLoadHandlerInstance.saveablesInScene == null || saveLoadHandlerInstance.saveablesInScene.Count == 0)
                {
                    if (saveLoadHandlerInstance.saveablesInScene == null) saveLoadHandlerInstance.saveablesInScene = new HashSet<Saveable>();

                    foreach (Saveable saveable in FindObjectsByType<Saveable>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                    {
                        if (!saveable) continue;

                        saveLoadHandlerInstance.saveablesInScene.Add(saveable);

                        latestDataToSave = UpdateCurrentSaveDataOfSaveable(currentSavedData, saveable);
                    }

                    return latestDataToSave;
                }

                foreach (Saveable saveable in saveLoadHandlerInstance.saveablesInScene)
                {
                    if (!saveable) continue;

                    latestDataToSave = UpdateCurrentSaveDataOfSaveable(currentSavedData, saveable);
                }

                return latestDataToSave;
            }

            foreach (Saveable saveable in saveablesToSave)
            {
                if (!saveable) continue;

                latestDataToSave = UpdateCurrentSaveDataOfSaveable(currentSavedData, saveable);
            }

            return latestDataToSave;
        }

        private static void WriteToFile(StateDictionaryObject latestSavedData)
        {
            if (!saveLoadHandlerInstance && !saveLoadHandlerInstance.saveLoadManager) return;

            saveLoadHandlerInstance.saveLoadManager.Save(latestSavedData, SAVE_FILE_NAME);
        }

        //SAVE SINGLE SAVEABLE ONLY......................................................................................

        public static bool SaveThisSaveableOnly(Saveable saveable)
        {
            if (!Application.isPlaying) return false;

            if (!saveLoadHandlerInstance || !saveLoadHandlerInstance.saveLoadManager) return false;

            if (!saveLoadHandlerInstance.enabled) return false;
                 
            if (saveLoadHandlerInstance.disableSaveLoadAlways || saveLoadHandlerInstance.disableSaveLoadRuntime) return false;

            if (!saveable) return false;

            float saveStartTime = Time.realtimeSinceStartup;

            if (saveLoadHandlerInstance.showDebugLog) Debug.Log("Save " + saveable.name + " Started!");

            if (saveLoadHandlerInstance.saveLoadDelay == null)
            {
                OnSavingStarted?.Invoke();
            }
            else
            {
                if (!saveLoadHandlerInstance.saveLoadDelay.isSavingSaveables) OnSavingStarted?.Invoke();

                if (saveLoadHandlerInstance.saveLoadDelay.IsDelayingSave())
                {
                    saveLoadHandlerInstance.saveLoadDelay.SaveDelayScheduled(saveable);

                    return true;
                }
            }

            if (saveLoadHandlerInstance.currentSaveData != null &&
               saveLoadHandlerInstance.currentSaveData.IsValid())
            {
                saveLoadHandlerInstance.currentSaveData = UpdateCurrentSaveDataOfSaveable(saveLoadHandlerInstance.currentSaveData, saveable);
            }
            else saveLoadHandlerInstance.currentSaveData = UpdateCurrentSaveDataOfSaveable(LoadFromFile(), saveable);

            WriteToFile(saveLoadHandlerInstance.currentSaveData);

            OnSavingFinished?.Invoke();

            float saveTime = Time.realtimeSinceStartup - saveStartTime;

            if (saveLoadHandlerInstance.showDebugLog) Debug.Log("Save " + saveable.name + " Finished! Time took: " + saveTime);

            return true;
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

            //saveLoadHandlerInstance.DEBUG_SavedSaveablesList.Add(new SavedSaveable(saveable.name, saveable.GetSaveableID()));

            return currentSavedData;
        }

        #endregion

        //LOAD............................................................................................................

        #region Load

        public void LoadToAllSaveables_UnityEventOnly()
        {
            LoadToAllSaveables();
        }

        public static bool LoadToAllSaveables()
        {
            return LoadMultiSaveables(saveLoadHandlerInstance.saveablesInScene.ToArray());
        }

        public static bool LoadMultiSaveables(Saveable[] saveablesToLoad)
        {
            if (!Application.isPlaying) return false;

            if (!saveLoadHandlerInstance && !saveLoadHandlerInstance.saveLoadManager) return false;

            if (!saveLoadHandlerInstance.enabled) return false;

            if (saveLoadHandlerInstance.disableSaveLoadAlways || saveLoadHandlerInstance.disableSaveLoadRuntime) return false;

            if(saveablesToLoad == null || saveablesToLoad.Length == 0) return false;

            if (!HasSavedData())
            {
                if (saveLoadHandlerInstance.showDebugLog) Debug.LogWarning("No Saved Data To Load!");

                return false;
            }

            float loadTimeStart = Time.realtimeSinceStartup;

            if (saveLoadHandlerInstance.showDebugLog) Debug.Log("Load Multi-Saveables Started!");

            MemoryUsageLogger.LogMemoryUsageAsText("GameProgressStartedLoading");

            if (saveLoadHandlerInstance.saveLoadDelay == null)
            {
                OnLoadingStarted?.Invoke();
            }
            else
            {
                if(!saveLoadHandlerInstance.saveLoadDelay.isLoadingSaveables) OnLoadingStarted?.Invoke();

                if (saveLoadHandlerInstance.saveLoadDelay.IsDelayingLoad())
                {
                    saveLoadHandlerInstance.saveLoadDelay.LoadDelayScheduled(saveablesToLoad);

                    return true;
                }
            }

            if (saveLoadHandlerInstance.currentSaveData != null &&
               saveLoadHandlerInstance.currentSaveData.IsValid())
            {
                RestoreSavedDataForSaveables(saveLoadHandlerInstance.currentSaveData);
            }
            else RestoreSavedDataForSaveables(LoadFromFile());

            OnLoadingFinished?.Invoke();

            MemoryUsageLogger.LogMemoryUsageAsText("GameProgressSavedDataLoaded");

            float loadTime = Time.realtimeSinceStartup - loadTimeStart;

            if (saveLoadHandlerInstance.showDebugLog) Debug.Log("Load Finished! Time took: " + loadTime);

            return true;
        }

        private static void RestoreSavedDataForSaveables(StateDictionaryObject savedData, Saveable[] saveablesToLoad = null)
        {
            if(saveablesToLoad == null || saveablesToLoad.Length == 0)
            {
                if(saveLoadHandlerInstance.saveablesInScene == null || saveLoadHandlerInstance.saveablesInScene.Count == 0)
                {
                    if (saveLoadHandlerInstance.saveablesInScene == null) saveLoadHandlerInstance.saveablesInScene = new HashSet<Saveable>();

                    foreach (Saveable saveable in FindObjectsByType<Saveable>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                    {
                        if (!saveable) continue;

                        saveLoadHandlerInstance.saveablesInScene.Add(saveable);

                        RestoreSaveDataOfSaveable(savedData, saveable);
                    }

                    return;
                }

                foreach (Saveable saveable in saveLoadHandlerInstance.saveablesInScene)
                {
                    if (!saveable) continue;

                    RestoreSaveDataOfSaveable(savedData, saveable);
                }

                return;
            }

            foreach (Saveable saveable in saveablesToLoad)
            {
                if (!saveable) continue;

                RestoreSaveDataOfSaveable(savedData, saveable);
            }
        }

        //LOAD SINGLE SAVEABLE ONLY........................................................................

        public static bool LoadThisSaveableOnly(Saveable saveable)
        {
            if (!Application.isPlaying) return false;

            if (!saveLoadHandlerInstance && !saveLoadHandlerInstance.saveLoadManager) return false;

            if (!saveLoadHandlerInstance.enabled) return false;

            if (saveLoadHandlerInstance.disableSaveLoadAlways || saveLoadHandlerInstance.disableSaveLoadRuntime) return false;

            if (!saveable) return false;

            float loadTimeStart = Time.realtimeSinceStartup;

            if (saveLoadHandlerInstance.showDebugLog) Debug.Log("Load " + saveable.name + " Started!");

            if (saveLoadHandlerInstance.saveLoadDelay == null)
            {
                OnLoadingStarted?.Invoke();
            }
            else
            {
                if (!saveLoadHandlerInstance.saveLoadDelay.isLoadingSaveables) OnLoadingStarted?.Invoke();

                if (saveLoadHandlerInstance.saveLoadDelay.IsDelayingLoad())
                {
                    saveLoadHandlerInstance.saveLoadDelay.LoadDelayScheduled(saveable);

                    return true;
                }
            }

            if (saveLoadHandlerInstance.currentSaveData != null &&
               saveLoadHandlerInstance.currentSaveData.IsValid())
            {
                RestoreSaveDataOfSaveable(saveLoadHandlerInstance.currentSaveData, saveable);
            }
            else RestoreSaveDataOfSaveable(LoadFromFile(), saveable);

            OnLoadingFinished?.Invoke();

            float loadTime = Time.realtimeSinceStartup - loadTimeStart;

            if (saveLoadHandlerInstance.showDebugLog) Debug.Log("Load " + saveable.name + " Finished! Time took: " + loadTime);

            return true;
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
            if (!saveLoadHandlerInstance) return;

            saveLoadHandlerInstance.DeleteAllSaveDataEditorInternal();
        }

        private void DeleteAllSaveDataEditorInternal()
        {
            if (showDebugLog) Debug.Log("Start Deleting All Save Data!");

            if (!saveLoadManager)
            {
                saveLoadManager = GetSaveLoadManagerFromResources();
            }

            if (!saveLoadManager)
            {
                if (showDebugLog) Debug.Log("Delete All Save Data FAILED! " +
                    "No Save Load Manager SO found or No Save Data File Found!");

                return;
            }

            if (showDebugLog) Debug.Log("All Save Data Deleted SUCCESSFULLY!");

            saveLoadManager.DeleteSave(SAVE_FILE_NAME);

            if(currentSaveData != null) currentSaveData.Clear();
        }

        public void DeleteSaveDataOfSaveable(Saveable saveable)
        {
            if (!saveLoadManager)
            {
                saveLoadManager = GetSaveLoadManagerFromResources();
            }

            if (!saveLoadManager) return;

            if (!saveable) return;

            if(currentSaveData == null || !currentSaveData.IsValid()) currentSaveData = LoadFromFile();

            if (!currentSaveData.stateDict.ContainsKey(saveable.GetSaveableID())) return;

            currentSaveData.stateDict.Remove(saveable.GetSaveableID());

            WriteToFile(currentSaveData);
        }

        #endregion

        #region Other SaveLoad Utilities

        public static void CreateSaveLoadManagerInstance()
        {
            if (saveLoadHandlerInstance) return;

            if (FindObjectOfType<SaveLoadHandler>() != null) return;

            GameObject go = new GameObject("SaveLoadManager(1InstanceOnly)");

            SaveLoadHandler slHandler = go.AddComponent<SaveLoadHandler>();

            if (!saveLoadHandlerInstance) saveLoadHandlerInstance = slHandler;
        }

        public static bool HasSavedData()
        {
            return SaveLoadUtility.Exists(SAVE_FILE_NAME, DEFAULT_FOLDER, BASE_FOLDER);
        }

        //THIS FUNCTION IS CALLED IN WAVE SPAWNER'S WAVE STARTED/FINISHED UNITY EVENTS
        public static void EnableSaveLoad(bool enabled)
        {
            if (!saveLoadHandlerInstance) return;

            if (enabled) saveLoadHandlerInstance.disableSaveLoadRuntime = false;
            else saveLoadHandlerInstance.disableSaveLoadRuntime = true;
        }

        private SaveLoadManager GetSaveLoadManagerFromResources()
        {
            if (saveLoadManager) return saveLoadManager;

            SaveLoadManager tempSaveLoadManager = null;

            foreach (SaveLoadManager slManager in Resources.FindObjectsOfTypeAll<SaveLoadManager>())
            {
                if (slManager.DefaultFolder == DEFAULT_FOLDER &&
                    slManager.BaseFolder == BASE_FOLDER)
                {
                    tempSaveLoadManager = slManager;

                    break;
                }
            }

            return tempSaveLoadManager;
        }

        private void SetSaveLoadManagerReferenceIfMissing()
        {
            if (!saveLoadManager)
            {
                saveLoadManager = GetSaveLoadManagerFromResources();
            }

            if (!saveLoadManager)
            {
                saveLoadManager = SaveLoadManager.Create(BASE_FOLDER, DEFAULT_FOLDER, SERIALIZE_METHOD);
            }
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

                using (new EditorGUI.DisabledGroupScope(!Application.isPlaying))
                {
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
                }

                EditorGUILayout.Space();

                if(GUILayout.Button("Delete All Saved Data"))
                {
                    if (!HasSavedData())
                    {
                        if (saveLoadHandler.showEditorDebugLog) Debug.LogWarning("No Saved Data To Delete!");

                        return;
                    }

                    saveLoadHandler.DeleteAllSaveDataEditorInternal();
                }
            }
        }
#endif

        #endregion
    }
}
