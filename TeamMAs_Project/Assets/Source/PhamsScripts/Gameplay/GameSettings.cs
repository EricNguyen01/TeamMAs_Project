// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gameframe.SaveLoad;
using System.Runtime.CompilerServices;

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

        private const int DEFAULT_SCREEN_WIDTH_WINDOWED = 1600;

        private const int DEFAULT_SCREEN_HEIGHT_WINDOWED = 900;

        [Header("Game Settings Save Load")]

        [SerializeField] private SaveLoadManager gameSettingsSaveManager;

        private const string SETTINGS_SAVE_FILE_NAME = "GameSettingSave";

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

        private WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();

        private bool isSaving = false;

        private void Awake()
        {
            if (gameSettingsInstance)
            {
                Destroy(gameObject);

                return;
            }

            gameSettingsInstance = this;

            DontDestroyOnLoad(gameObject);

            LoadSettings();
        }

        public void SetScreenMode(FullScreenMode fullScreenMode, bool sendEvent = true, bool saveSetting = true)
        {
            StartCoroutine(SetScreenModeSequence(fullScreenMode, sendEvent, saveSetting));
        }

        private IEnumerator SetScreenModeSequence(FullScreenMode fullScreenMode, bool sendEvent = true, bool saveSetting = true)
        {
            if (Screen.fullScreenMode == fullScreenMode) yield break;

            if(fullScreenMode == FullScreenMode.FullScreenWindow)
            {
                SetScreenResolution(DEFAULT_SCREEN_WIDTH, DEFAULT_SCREEN_HEIGHT, sendEvent, saveSetting);
            }
            else if(fullScreenMode == FullScreenMode.Windowed)
            {
                SetScreenResolution(DEFAULT_SCREEN_WIDTH_WINDOWED, DEFAULT_SCREEN_HEIGHT_WINDOWED, sendEvent, saveSetting);
            }

            yield return waitForFixedUpdate;

            yield return waitForFixedUpdate;

            Screen.fullScreenMode = fullScreenMode;

            if (sendEvent) OnFullScreenModeChanged?.Invoke(fullScreenMode);

            if (showDebugLog) Debug.Log("Set New FullScreen Mode: " + Screen.fullScreenMode.ToString());

            if (saveSetting) SaveSettings();

            yield return waitForFixedUpdate;

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
            //set default res values if invalid resolution parameters
            //then call this function itself again but with default res values as params
            if (resolution.width == 0 || resolution.height == 0)
            {
                SetScreenResolution(DEFAULT_SCREEN_WIDTH, DEFAULT_SCREEN_HEIGHT);

                return;
            }

            //if new res param = current screen res -> return
            if (resolution.width == Screen.currentResolution.width && resolution.height == Screen.currentResolution.height) return;

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
            StartCoroutine(SetDefaultAllSettingsSequence(sendEvent, saveSettings));
        }

        private IEnumerator SetDefaultAllSettingsSequence(bool sendEvent = true, bool saveSettings = true)
        {
            yield return StartCoroutine(SetScreenModeSequence(DEFAULT_FULLSCREEN_MODE, sendEvent, saveSettings));

            yield return waitForFixedUpdate;

            SetScreenResolution(DEFAULT_SCREEN_WIDTH, DEFAULT_SCREEN_HEIGHT, sendEvent, saveSettings);

            yield return waitForFixedUpdate;

            yield break;
        }

        //GAME SETTINGS SAVE / LOAD LOGIC............................................................................................

        private void SaveSettings()
        {
            if (!gameSettingsSaveManager) return;

            //we need to wait until the next frame to save because Unity only updates any screen related settings on the end of this frame
            if(!isSaving) StartCoroutine(SaveSettingsNextFrame());
        }

        private IEnumerator SaveSettingsNextFrame()
        {
            isSaving = true;

            //waiting for Screen to update
            //yield return waitForFixedUpdate;

            //waiting for Screen to update (another frame buffer to make sure Screen has finished updating before save)
            //yield return waitForFixedUpdate;

            OnGameSettingsBeginSaving?.Invoke();

            //read and get existing gameSettingsSavedData from settings save file first
            gameSettingsSavedData = gameSettingsSaveManager.Load<GameSettingsSaveData>(SETTINGS_SAVE_FILE_NAME);

            //if no gameSettingsSavedData exists -> creates new
            if (gameSettingsSavedData == null) gameSettingsSavedData = new GameSettingsSaveData();

            //update the previous (or new) settings saved data
            gameSettingsSavedData.SetAllSaveData((int)Screen.fullScreenMode, Screen.currentResolution);

            if (showDebugLog) 
            { 
                Debug.Log("FullScreen Mode: " + Screen.fullScreenMode.ToString() + "\n" + 
                          "Screen Resolution: " + Screen.currentResolution.ToString() + "\n" + 
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
            StartCoroutine(LoadSettingsSequence());
        }

        private IEnumerator LoadSettingsSequence()
        {
            OnGameSettingsStartLoading?.Invoke();

            if (showDebugLog) Debug.Log("Game Settings Load Started!");

            if (!gameSettingsSaveManager ||
                !SaveLoadUtility.Exists(SETTINGS_SAVE_FILE_NAME, gameSettingsSaveManager.DefaultFolder, gameSettingsSaveManager.BaseFolder))
            {
                if (showDebugLog) Debug.Log("No Available Game Settings Save Exists. Load Default Settings!");

                Resolution DEFAULT_RES = new Resolution();

                DEFAULT_RES.width = DEFAULT_SCREEN_WIDTH;

                DEFAULT_RES.height = DEFAULT_SCREEN_HEIGHT;

                yield return StartCoroutine(SetDefaultAllSettingsSequence(false));

                //wait for next frames so the new screen settings can be updated first before invoking setting finshed event
                yield return waitForFixedUpdate;

                //send event to UIs and any other subscribers
                OnFullScreenModeChanged?.Invoke(DEFAULT_FULLSCREEN_MODE);

                OnScreenResolutionChanged?.Invoke(DEFAULT_RES);

                OnGameSettingsFinishLoading?.Invoke();

                yield break;
            }

            if (showDebugLog) Debug.Log("Available Game Settings Save Exists. Load Game Settings Save!");

            gameSettingsSavedData = gameSettingsSaveManager.Load<GameSettingsSaveData>(SETTINGS_SAVE_FILE_NAME);

            if (gameSettingsSavedData == null) yield break;

            yield return StartCoroutine(SetDefaultAllSettingsSequence(false, false));

            yield return waitForFixedUpdate;

            Resolution savedRes = new Resolution();

            savedRes.width = gameSettingsSavedData.screenWidth;

            savedRes.height = gameSettingsSavedData.screenHeight;

            SetScreenResolution(savedRes.width, savedRes.height, false, false);

            //wait 2 frames for screen window mode to update

            yield return waitForFixedUpdate;

            yield return waitForFixedUpdate;

            SetScreenMode(gameSettingsSavedData.fullScreenModeNum, false, false);

            //wait 2 frames so the new screen window mode can be updated first before invoking setting finshed event

            yield return waitForFixedUpdate;

            yield return waitForFixedUpdate;

            //send event to UIs and any other subscribers
            OnFullScreenModeIndexChanged?.Invoke(gameSettingsSavedData.fullScreenModeNum);

            OnScreenResolutionChanged?.Invoke(savedRes);

            OnGameSettingsFinishLoading?.Invoke();
        }

        public void DeleteAllGameSettingsSavedData()
        {
            if (!gameSettingsSaveManager) return;

            gameSettingsSaveManager.DeleteSave(SETTINGS_SAVE_FILE_NAME);
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
                    if (!gameSettings.gameSettingsSaveManager) return;

                    gameSettings.DeleteAllGameSettingsSavedData();
                }
            }
        }

#endif
    }
}
