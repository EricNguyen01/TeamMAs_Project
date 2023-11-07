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
    public class GameSettings : MonoBehaviour
    {
        [Header("Game Settings")]

        private const FullScreenMode DEFAULT_FULLSCREEN_MODE = FullScreenMode.FullScreenWindow;

        private const int DEFAULT_SCREEN_WIDTH = 1920;

        private const int DEFAULT_SCREEN_HEIGHT = 1080;

        [Header("Game Settings Save Load")]

        [SerializeField] private SaveLoadManager gameSettingsSaveManager;

        private const string SETTINGS_SAVE_FILE_NAME = "GameSettingSave";

        [Header("Debug")]

        [SerializeField] private bool showDebugLog = false;

        public static GameSettings gameSettingsInstance;

        public static event System.Action OnGameSettingsBeginSaving;

        public static event System.Action OnGameSettingsFinishSaving;

        public static event System.Action OnGameSettingsStartLoading;

        public static event System.Action OnGameSettingsFinishLoading;

        //INTERNALS...................................................................................

        [Serializable]
        private class GameSettingsSaveData
        {
            public int fullScreenModeNum { get; private set; }

            public int screenWidth { get; private set; }

            public int screenHeight { get; private set; }

            public GameSettingsSaveData(int fullScreenModeToSave, Resolution resolutionToSave)
            {
                fullScreenModeNum = fullScreenModeToSave;

                screenWidth = resolutionToSave.width;

                screenHeight = resolutionToSave.height;
            }
        }

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

        public void SetScreenMode(FullScreenMode fullScreenMode, bool saveSetting = true)
        {
            if (!Screen.fullScreen) Screen.fullScreen = true;

            if (Screen.fullScreenMode == fullScreenMode) return;
            
            Screen.fullScreenMode = fullScreenMode;

            if (showDebugLog) Debug.Log("Set New FullScreen Mode: " + Screen.fullScreenMode.ToString());

            if (saveSetting) SaveSettings();
        }

        public void SetScreenMode(int fsModeNum, bool saveSetting = true)
        {
            if (fsModeNum == (int)FullScreenMode.ExclusiveFullScreen) SetScreenMode(FullScreenMode.ExclusiveFullScreen, saveSetting);
            else if (fsModeNum == (int)FullScreenMode.MaximizedWindow) SetScreenMode(FullScreenMode.MaximizedWindow, saveSetting);
            else if (fsModeNum == (int)FullScreenMode.FullScreenWindow) SetScreenMode(FullScreenMode.FullScreenWindow, saveSetting);
            else if (fsModeNum == (int)FullScreenMode.Windowed) SetScreenMode(FullScreenMode.Windowed, saveSetting);
            else SetScreenMode(DEFAULT_FULLSCREEN_MODE, saveSetting);
        }

        public void SetScreenResolution(Resolution resolution, bool saveSetting = true)
        {
            if (resolution.width == 0 || resolution.height == 0)
            {
                Screen.SetResolution(DEFAULT_SCREEN_WIDTH, DEFAULT_SCREEN_HEIGHT, Screen.fullScreenMode);

                return;
            }

            if (resolution.width == Screen.currentResolution.width && resolution.height == Screen.currentResolution.height) return;

            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreenMode);

            if(showDebugLog) Debug.Log("Set New Screen Resolution: " + resolution.ToString());

            if (saveSetting) SaveSettings();
        }

        public void SetScreenResolution(int screenWidth, int screenHeight, bool saveSetting = true)
        {
            Resolution res = new Resolution();

            res.width = screenWidth;

            res.height = screenHeight;

            SetScreenResolution(res, saveSetting);
        }

        private void SaveSettings()
        {
            if (!gameSettingsSaveManager) return;

            //we need to wait until the next frame to save because Unity only updates any screen related settings on the end of this frame
            if(!isSaving) StartCoroutine(SaveSettingsNextFrame());
        }

        private WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();

        private IEnumerator SaveSettingsNextFrame()
        {
            isSaving = true;

            yield return waitForFixedUpdate;

            yield return waitForFixedUpdate;

            OnGameSettingsBeginSaving?.Invoke();

            GameSettingsSaveData gameSettingsData = new GameSettingsSaveData((int)Screen.fullScreenMode, Screen.currentResolution);

            if (showDebugLog) 
            { 
                Debug.Log("FullScreen Mode: " + Screen.fullScreenMode.ToString() + "\n" + 
                          "Screen Resolution: " + Screen.currentResolution.ToString() + "\n" + 
                          "Saved!----------------------------------------------------------"); 
            }

            gameSettingsSaveManager.Save(gameSettingsData, SETTINGS_SAVE_FILE_NAME);

            OnGameSettingsFinishSaving?.Invoke();

            isSaving = false;
        }

        private void LoadSettings(bool isSavedAfterLoad = false)
        {
            StartCoroutine(LoadSettingsSequence(isSavedAfterLoad));
        }

        private IEnumerator LoadSettingsSequence(bool isSavedAfterLoad)
        {
            OnGameSettingsStartLoading?.Invoke();

            if (showDebugLog) Debug.Log("Game Settings Load Started!");

            if (!gameSettingsSaveManager ||
                !SaveLoadUtility.Exists(SETTINGS_SAVE_FILE_NAME, gameSettingsSaveManager.DefaultFolder, gameSettingsSaveManager.BaseFolder))
            {
                if (showDebugLog) Debug.Log("No Available Game Settings Save Exists. Load Default Settings!");

                if (Screen.fullScreenMode != DEFAULT_FULLSCREEN_MODE) SetScreenMode(DEFAULT_FULLSCREEN_MODE);

                if (Screen.currentResolution.width != DEFAULT_SCREEN_WIDTH ||
                    Screen.currentResolution.height != DEFAULT_SCREEN_HEIGHT)
                {
                    SetScreenResolution(DEFAULT_SCREEN_WIDTH, DEFAULT_SCREEN_HEIGHT);
                }

                //wait for next frames so the new screen settings can be updated first before invoking setting finshed event
                yield return waitForFixedUpdate;

                yield return waitForFixedUpdate; 

                OnGameSettingsFinishLoading?.Invoke();

                yield break;
            }

            if (showDebugLog) Debug.Log("Available Game Settings Save Exists. Load Game Settings Save!");

            GameSettingsSaveData gameSettingsSavedData = gameSettingsSaveManager.Load<GameSettingsSaveData>(SETTINGS_SAVE_FILE_NAME);

            if (gameSettingsSavedData == null) yield break;

            if ((int)Screen.fullScreenMode != gameSettingsSavedData.fullScreenModeNum)
            {
                SetScreenMode(gameSettingsSavedData.fullScreenModeNum, isSavedAfterLoad);
            }

            if (Screen.currentResolution.width != gameSettingsSavedData.screenWidth ||
                Screen.currentResolution.height != gameSettingsSavedData.screenHeight)
            {
                SetScreenResolution(gameSettingsSavedData.screenWidth, gameSettingsSavedData.screenHeight, isSavedAfterLoad);
            }

            //wait for next frames so the new screen settings can be updated first before invoking setting finshed event

            yield return waitForFixedUpdate;

            yield return waitForFixedUpdate;

            OnGameSettingsFinishLoading?.Invoke();
        }

        public void DeleteAllGameSettingsSavedData()
        {
            if (!gameSettingsSaveManager) return;

            gameSettingsSaveManager.DeleteSave(SETTINGS_SAVE_FILE_NAME);
        }

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
