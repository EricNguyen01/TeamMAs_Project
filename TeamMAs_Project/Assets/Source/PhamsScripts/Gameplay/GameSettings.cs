// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gameframe.SaveLoad;
using System;

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

        public static GameSettings gameSettingsInstance;

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

        public void SetScreenMode(FullScreenMode fullScreenMode)
        {
            if(Screen.fullScreenMode == fullScreenMode) return;

            Screen.fullScreenMode = fullScreenMode;

            SaveSettings();
        }

        public void SetScreenResolution(Resolution resolution)
        {
            if(resolution.width == 0 || resolution.height == 0)
            {
                Screen.SetResolution(DEFAULT_SCREEN_WIDTH, DEFAULT_SCREEN_HEIGHT, Screen.fullScreenMode);

                return;
            }

            if (resolution.width == Screen.currentResolution.width && resolution.height == Screen.currentResolution.height) return;

            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreenMode);

            SaveSettings();
        }

        public void SetScreenResolution(int screenWidth, int screenHeight)
        {
            Resolution res = new Resolution();

            res.width = screenWidth; 
            
            res.height = screenHeight;

            SetScreenResolution(res);
        }

        public void SaveSettings()
        {
            if (!gameSettingsSaveManager) return;

            GameSettingsSaveData gameSettingsData = new GameSettingsSaveData((int)Screen.fullScreenMode, Screen.currentResolution);

            gameSettingsSaveManager.Save(gameSettingsData, SETTINGS_SAVE_FILE_NAME);
        }

        private void LoadSettings()
        {
            if (!gameSettingsSaveManager ||
                !SaveLoadUtility.Exists(SETTINGS_SAVE_FILE_NAME, gameSettingsSaveManager.DefaultFolder, gameSettingsSaveManager.BaseFolder))
            {
                SetScreenMode(DEFAULT_FULLSCREEN_MODE);

                SetScreenResolution(DEFAULT_SCREEN_WIDTH, DEFAULT_SCREEN_HEIGHT);

                return;
            }

            GameSettingsSaveData gameSettingsSavedData = gameSettingsSaveManager.Load<GameSettingsSaveData>(SETTINGS_SAVE_FILE_NAME);

            if (gameSettingsSavedData == null) return;

            int fsModeNum = gameSettingsSavedData.fullScreenModeNum;

            if (fsModeNum == (int)FullScreenMode.ExclusiveFullScreen) SetScreenMode(FullScreenMode.ExclusiveFullScreen);
            else if (fsModeNum == (int)FullScreenMode.MaximizedWindow) SetScreenMode(FullScreenMode.MaximizedWindow);
            else if (fsModeNum == (int)FullScreenMode.FullScreenWindow) SetScreenMode(FullScreenMode.FullScreenWindow);
            else if (fsModeNum == (int)FullScreenMode.Windowed) SetScreenMode(FullScreenMode.Windowed);
            else SetScreenMode(DEFAULT_FULLSCREEN_MODE);

            SetScreenResolution(gameSettingsSavedData.screenWidth, gameSettingsSavedData.screenHeight);
        }
    }
}
