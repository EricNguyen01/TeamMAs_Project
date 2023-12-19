// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Gameframe.SaveLoad;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TeamMAsTD
{
    [DisallowMultipleComponent]
    public class GameSettings : MonoBehaviour
    {
        [Header("Game Settings")]

        private const FullScreenMode DEFAULT_FULLSCREEN_MODE = FullScreenMode.FullScreenWindow;

        private const int DEFAULT_SCREEN_WIDTH = 1920;

        private const int DEFAULT_SCREEN_HEIGHT = 1080;

        private const int DEFAULT_SCREEN_WIDTH_WINDOWED = 1920;

        private const int DEFAULT_SCREEN_HEIGHT_WINDOWED = 1080;

        [Header("Game Settings Save Load")]

        [SerializeField] private SaveLoadManager gameSettingsSaveManager;

        private const string SETTINGS_SAVE_FILE_NAME = "GameSettingSave";

        private const string BASE_FOLDER = "GameData";

        private const string DEFAULT_FOLDER = "GameSettings";

        private const SerializationMethodType SERIALIZE_METHOD = SerializationMethodType.Default;

        [Header("Debug")]

        [SerializeField] private bool showDebugLog = false;

        public static GameSettings gameSettingsInstance;

        //Game Settings C# Events Declarations

        public static event System.Action OnGameSettingsBeginSaving;

        public static event System.Action OnGameSettingsFinishSaving;

        public static event System.Action OnGameSettingsStartLoading;

        public static event System.Action OnGameSettingsFinishLoading;


        public static event System.Action<Resolution> OnScreenResolutionChanged;

        public static event System.Action<FullScreenMode> OnFullScreenModeChanged;

        public static event System.Action<int> OnFullScreenModeIndexChanged;

        //INTERNALS...................................................................................

        //GAME SETTINGS SAVE DATA PRIVATE CLASS.....................................................................
        [Serializable]
        private class GameSettingsSaveData
        {
            public int fullScreenModeNum { get; private set; }

            public int screenWidth { get; private set; }

            public int screenHeight { get; private set; }

            public GameSettingsSaveData() { }//constructor

            public void SetAllSaveData(int fullScreenModeIndexToSave, Resolution resolutionToSave)
            {
                SetFullScreenSaveData(fullScreenModeIndexToSave);

                SetResolutionSaveData(resolutionToSave);
            }

            public void SetFullScreenSaveData(int fsModeIndexToSave)
            {
                if(fsModeIndexToSave < 0) fsModeIndexToSave = 0;

                if(fsModeIndexToSave > 4) fsModeIndexToSave = 4;

                fullScreenModeNum = fsModeIndexToSave;
            }

            public void SetResolutionSaveData(Resolution resolutionToSave)
            {
                screenWidth = resolutionToSave.width;

                screenHeight = resolutionToSave.height;
            }
        }
        //GAME SETTINGS SAVE DATA PRIVATE CLASS ENDS.............................................................................

        private GameSettingsSaveData gameSettingsSavedData;

        private bool isSaving = false;

        private bool isLoading = false;

        private bool isSettingScreenMode = false;

        private void Awake()
        {
            if (gameSettingsInstance && gameSettingsInstance != this)
            {
                Destroy(gameObject);

                return;
            }

            gameSettingsInstance = this;

            DontDestroyOnLoad(gameObject);

            Set_GameSettingsSaveLoadManager_Reference_IfMissing();

            LoadSettings();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += (Scene sc, LoadSceneMode loadMode) => LoadSettings();
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= (Scene sc, LoadSceneMode loadMode) => LoadSettings();
        }

        public void SetScreenMode(FullScreenMode fullScreenMode, bool sendEvent = true, bool saveSetting = true)
        {
            if (!enabled) return;

            if (isSettingScreenMode) StopCoroutine(SetScreenModeSequence(fullScreenMode, sendEvent, saveSetting));

            isSettingScreenMode = true;

            StartCoroutine(SetScreenModeSequence(fullScreenMode, sendEvent, saveSetting));
        }

        private IEnumerator SetScreenModeSequence(FullScreenMode fullScreenMode, bool sendEvent = true, bool saveSetting = true)
        {
            isSettingScreenMode = true;

            if (Screen.fullScreenMode == fullScreenMode)
            {
                isSettingScreenMode = false; yield break;
            }

            if (!Screen.fullScreen) Screen.fullScreen = true;

            Screen.fullScreenMode = fullScreenMode;

            yield return new WaitForSecondsRealtime(0.03f);

            if (fullScreenMode == FullScreenMode.FullScreenWindow)
            {
                SetScreenResolution(DEFAULT_SCREEN_WIDTH, DEFAULT_SCREEN_HEIGHT, sendEvent, saveSetting);
            }
            else if(fullScreenMode == FullScreenMode.Windowed)
            {
                SetScreenResolution(DEFAULT_SCREEN_WIDTH_WINDOWED, DEFAULT_SCREEN_HEIGHT_WINDOWED, sendEvent, saveSetting);
            }

            if (sendEvent) OnFullScreenModeChanged?.Invoke(fullScreenMode);

            if (showDebugLog) Debug.Log("Set New FullScreen Mode: " + Screen.fullScreenMode.ToString());

            if (saveSetting) SaveSettings();

            yield return new WaitForSecondsRealtime(0.02f);

            isSettingScreenMode = false;

            yield break;
        }

        public void SetScreenMode(int fsModeNum, bool sendEvent = true, bool saveSetting = true)
        {
            if (fsModeNum == (int)FullScreenMode.ExclusiveFullScreen) SetScreenMode(FullScreenMode.ExclusiveFullScreen, sendEvent, saveSetting);
            else if (fsModeNum == (int)FullScreenMode.MaximizedWindow) SetScreenMode(FullScreenMode.MaximizedWindow, sendEvent, saveSetting);
            else if (fsModeNum == (int)FullScreenMode.FullScreenWindow) SetScreenMode(FullScreenMode.FullScreenWindow, sendEvent, saveSetting);
            else if (fsModeNum == (int)FullScreenMode.Windowed) SetScreenMode(FullScreenMode.Windowed, sendEvent, saveSetting);
            else SetScreenMode(DEFAULT_FULLSCREEN_MODE, sendEvent, saveSetting);
        }

        public void SetScreenResolution(Resolution resolution, bool sendEvent = true, bool saveSetting = true)
        {
            if (!enabled) return;

            //set default res values if invalid resolution parameters
            //then call this function itself again but with default res values as params
            if (resolution.width == 0 || resolution.height == 0)
            {
                SetScreenResolution(DEFAULT_SCREEN_WIDTH, DEFAULT_SCREEN_HEIGHT);

                return;
            }

            //if new res param = current screen res -> return
            if (resolution.width == Screen.width && resolution.height == Screen.height) return;

            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreenMode, (int)Screen.mainWindowDisplayInfo.refreshRate.value);

            if (sendEvent) OnScreenResolutionChanged?.Invoke(resolution);
            
            if(showDebugLog) Debug.Log("Set New Screen Resolution: " + resolution.ToString());

            if (saveSetting) SaveSettings();
        }

        public void SetScreenResolution(int screenWidth, int screenHeight, bool sendEvent = true, bool saveSetting = true)
        {
            Resolution res = new Resolution();

            res.width = screenWidth;

            res.height = screenHeight;

            SetScreenResolution(res, sendEvent, saveSetting);
        }

        private void SetDefaultAllSettings(bool sendEvent = true, bool saveSettings = true)
        {
            if (!enabled) return;

            StartCoroutine(SetDefaultAllSettingsSequence(sendEvent, saveSettings));
        }

        private IEnumerator SetDefaultAllSettingsSequence(bool sendEvent = true, bool saveSettings = true)
        {
            yield return StartCoroutine(SetScreenModeSequence(DEFAULT_FULLSCREEN_MODE, sendEvent, saveSettings));

            SetScreenResolution(DEFAULT_SCREEN_WIDTH, DEFAULT_SCREEN_HEIGHT, sendEvent, saveSettings);

            yield return new WaitForSecondsRealtime(0.02f);

            yield break;
        }

        //GAME SETTINGS SAVE / LOAD LOGIC............................................................................................

        private void SaveSettings()
        {
            if (!gameSettingsSaveManager) return;

            if (isSaving) StopCoroutine(SaveSettingsSequence());

            //we need to wait until the next few frames to save because Unity updates the screen settings over multiple frames
            StartCoroutine(SaveSettingsSequence());
        }

        private IEnumerator SaveSettingsSequence()
        {
            isSaving = true;

            //waiting for Screen to update
            yield return new WaitForSecondsRealtime(0.03f);

            OnGameSettingsBeginSaving?.Invoke();

            //read and get existing gameSettingsSavedData from settings save file first
            gameSettingsSavedData = gameSettingsSaveManager.Load<GameSettingsSaveData>(SETTINGS_SAVE_FILE_NAME);

            //if no gameSettingsSavedData exists -> creates new
            if (gameSettingsSavedData == null) gameSettingsSavedData = new GameSettingsSaveData();

            Resolution resToSave = new Resolution();

            resToSave.width = Screen.width;

            resToSave.height = Screen.height;   

            //update the previous (or new) settings saved data
            gameSettingsSavedData.SetAllSaveData((int)Screen.fullScreenMode, resToSave);

            if (showDebugLog) 
            { 
                Debug.Log("Saving----------------------------------------------------------\n" +
                          "FullScreen Mode: " + Screen.fullScreenMode.ToString() + "\n" + 
                          "Screen Resolution: " + resToSave.ToString() + "\n" + 
                          "Saved!----------------------------------------------------------"); 
            }

            //write settings data back to file
            gameSettingsSaveManager.Save(gameSettingsSavedData, SETTINGS_SAVE_FILE_NAME);

            OnGameSettingsFinishSaving?.Invoke();

            isSaving = false;

            yield break;
        }

        private void LoadSettings()
        {
            if (!gameSettingsSaveManager) return;

            if (isLoading) StopCoroutine(LoadSettingsSequence());

            StartCoroutine(LoadSettingsSequence());
        }

        private IEnumerator LoadSettingsSequence()
        {
            isLoading = true;

            OnGameSettingsStartLoading?.Invoke();

            if (showDebugLog) Debug.Log("Game Settings Load Started!");

            if (!gameSettingsSaveManager ||
                !SaveLoadUtility.Exists(SETTINGS_SAVE_FILE_NAME, gameSettingsSaveManager.DefaultFolder, gameSettingsSaveManager.BaseFolder))
            {
                if (showDebugLog) Debug.Log("No Available Game Settings Save Exists. Load Default Settings!");

                yield return StartCoroutine(SetDefaultAllSettingsSequence(false));

                Resolution DEFAULT_RES = new Resolution();

                DEFAULT_RES.width = DEFAULT_SCREEN_WIDTH;

                DEFAULT_RES.height = DEFAULT_SCREEN_HEIGHT;

                //send event to UIs and any other subscribers
                OnFullScreenModeChanged?.Invoke(DEFAULT_FULLSCREEN_MODE);

                OnScreenResolutionChanged?.Invoke(DEFAULT_RES);

                OnGameSettingsFinishLoading?.Invoke();

                isLoading = false;

                yield break;
            }

            if (showDebugLog) Debug.Log("Available Game Settings Save Exists. Load Game Settings Save!");

            gameSettingsSavedData = gameSettingsSaveManager.Load<GameSettingsSaveData>(SETTINGS_SAVE_FILE_NAME);

            if (gameSettingsSavedData == null) yield break;

            SetScreenMode(gameSettingsSavedData.fullScreenModeNum, false, false);

            yield return new WaitWhile(() => isSettingScreenMode);

            Resolution savedRes = new Resolution();

            savedRes.width = gameSettingsSavedData.screenWidth;

            savedRes.height = gameSettingsSavedData.screenHeight;

            SetScreenResolution(savedRes.width, savedRes.height, false, false);

            //wait a few frames so the new screen window mode can be updated first before invoking setting finshed event

            yield return new WaitForSecondsRealtime(0.02f);

            if (showDebugLog)
            {
                Debug.Log("Loading----------------------------------------------------------\n" +
                          "FullScreen Mode: " + Screen.fullScreenMode.ToString() + "\n" +
                          "Screen Resolution: " + savedRes.ToString() + "\n" +
                          "Loaded!----------------------------------------------------------");
            }

            //send event to UIs and any other subscribers
            OnFullScreenModeIndexChanged?.Invoke(gameSettingsSavedData.fullScreenModeNum);

            OnScreenResolutionChanged?.Invoke(savedRes);

            OnGameSettingsFinishLoading?.Invoke();

            isLoading = false;

            MemoryUsageLogger.LogMemoryUsageAsText("GameSettingsSavedDataLoaded");

            yield break;
        }

        public void DeleteAllGameSettingsSavedData()
        {
            if (!gameSettingsSaveManager) return;

            gameSettingsSaveManager.DeleteSave(SETTINGS_SAVE_FILE_NAME);
        }

        public static void CreateGameSettingsInstance()
        {
            if (gameSettingsInstance) return;

            if (FindObjectOfType<GameSettings>() != null) return;

            GameObject go = new GameObject("GameSettings(1InstanceOnly)");

            GameSettings gSettings = go.AddComponent<GameSettings>();

            if (!gameSettingsInstance) gameSettingsInstance = gSettings;
        }

        private void Set_GameSettingsSaveLoadManager_Reference_IfMissing()
        {
            if (!gameSettingsSaveManager)
            {
                gameSettingsSaveManager = Resources.Load<SaveLoadManager>("GameSettingsSaveLoadManager");
            }

            if (!gameSettingsSaveManager)
            {
                gameSettingsSaveManager = SaveLoadManager.Create(BASE_FOLDER, DEFAULT_FOLDER, SERIALIZE_METHOD);
            }
        }

        //GAME SETTINGS EDITOR...................................................................................................

#if UNITY_EDITOR

        [CustomEditor(typeof(GameSettings))]
        private class GameSettingsEditor : Editor
        {
            private GameSettings gameSettings;

            private void OnEnable()
            {
                gameSettings = target as GameSettings;
            }

            public override void OnInspectorGUI()
            {
                DrawDefaultInspector();

                EditorGUILayout.Space();

                if (GUILayout.Button("Delete Settings Saved Data"))
                {
                    if (!gameSettings.gameSettingsSaveManager ||
                        !SaveLoadUtility.Exists(SETTINGS_SAVE_FILE_NAME, 
                                                gameSettings.gameSettingsSaveManager.DefaultFolder,
                                                gameSettings.gameSettingsSaveManager.BaseFolder))
                    {
                        if (gameSettings.showDebugLog)
                        {
                            Debug.Log("No Game Settings Save File To Delete!");
                        }

                        return;
                    }

                    if(gameSettings.showDebugLog)
                    {
                        Debug.Log("Deleting Game Settings Save File!");
                    }

                    gameSettings.DeleteAllGameSettingsSavedData();
                }
            }
        }

#endif
    }
}
