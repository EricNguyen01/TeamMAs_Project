// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using System;
using System.Collections;
using System.IO;
using Unity.Profiling;
using UnityEngine;

namespace TeamMAsTD
{
    [DisallowMultipleComponent]
    public class MemoryUsageLogger : MonoBehaviour
    {
        [Header("Log Settings")]

        [SerializeField] private bool enableLogInEditor = true;

        [SerializeField] private bool enableLogInUpdate = true;

        [SerializeField] private bool deleteLogOnUnityClosed = true;

        [SerializeField] private float logToTextIntervalSec = 5.0f;

        //to record the last logToTextIntervalSec value above to use for checking if the player has changed this val
        private float previousLogIntervalValue;

        [Header("Log UI Display")]

        [SerializeField] private MemoryUsageLogUI memoryUsageLogUIPrefab;

        [SerializeField] private float memoryLogUIDisplayRefreshTime = 1.0f;

        private MemoryUsageLogUI memoryUsageLogUI;

        private float previousUIRefreshTime;

        private float currentUIRefreshTimer;

        //INTERNALS.............................................................................

        private const string LOG_FILE_NAME = "MemoryUsageLog.txt";

        private string pathToLogFile;

        private float currentLogTimer;

        private ProfilerRecorder totalReservedMemoryRecorder;

        private ProfilerRecorder gcUsedMemoryRecorder;

        private ProfilerRecorder systemUsedMemoryRecorder;

        private enum LogEnvironmentStatus { Editor = 0, PlayerBuild = 1 }

        private LogEnvironmentStatus logEnvironmentStatus = LogEnvironmentStatus.Editor;

        public struct ActiveMemoryLogStruct
        {
            public string logEnvironmentStatus { get; internal set; }

            public long totalReservedMemory { get; internal set; }

            public string totalReservedMemoryLogText { get; internal set; }

            public long gcUsedMemory { get; internal set; }

            public string gcUsedMemoryLogText { get; internal set; }

            public long systemUsedMemory { get; internal set; }

            public string systemUsedMemoryLogText { get; internal set; }

            public string memoryLogSummaryText { get; internal set; }

            internal void InitSetDefaultValues()
            {
                logEnvironmentStatus = "n/a";

                totalReservedMemory = 0L; 
                
                totalReservedMemoryLogText = "n/a";

                gcUsedMemory = 0L; 
                
                gcUsedMemoryLogText = "n/a";

                systemUsedMemory = 0L; 
                
                systemUsedMemoryLogText = "n/a";

                memoryLogSummaryText = "n/a";
            }
        }

        private ActiveMemoryLogStruct activeMemoryLogStruct;

        public static MemoryUsageLogger memoryUsageLoggerInstance;

        private bool pendingDeleteOnAppClosed = false;

        private void Awake()
        {
            if(memoryUsageLoggerInstance && memoryUsageLoggerInstance != this)
            {
                Destroy(gameObject);

                return;
            }

            memoryUsageLoggerInstance = this;

            DontDestroyOnLoad(gameObject);

            SetupMemoryLogUI();
        }

        private void OnEnable()
        {
            if (Application.isEditor && !enableLogInEditor)
            {
                enabled = false;

                return;
            }

            pathToLogFile = System.IO.Path.Combine(Application.persistentDataPath, LOG_FILE_NAME);

            string newMemoryLogSession;

            if (Application.isEditor)
            {
                newMemoryLogSession = "-----NEW IN-EDITOR MEMORY LOG SESSION STARTED-----\n";

                logEnvironmentStatus = LogEnvironmentStatus.Editor;

                activeMemoryLogStruct.logEnvironmentStatus = "Editor";
            }
            else 
            { 
                newMemoryLogSession = "-----NEW IN-BUILD MEMORY LOG SESSION STARTED-----\n";

                logEnvironmentStatus = LogEnvironmentStatus.PlayerBuild;

                activeMemoryLogStruct.logEnvironmentStatus = "PlayerBuild";
            }

            File.WriteAllText(pathToLogFile, newMemoryLogSession);

            previousLogIntervalValue = logToTextIntervalSec;

            currentLogTimer = logToTextIntervalSec;

            previousUIRefreshTime = memoryLogUIDisplayRefreshTime;

            currentUIRefreshTimer = memoryLogUIDisplayRefreshTime;

            totalReservedMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total Reserved Memory");

            gcUsedMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Used Memory");

            systemUsedMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "System Used Memory");

            activeMemoryLogStruct = new ActiveMemoryLogStruct();

            activeMemoryLogStruct.InitSetDefaultValues();
        }

        private void OnDisable()
        {
            if(totalReservedMemoryRecorder.Valid) totalReservedMemoryRecorder.Dispose();

            if(gcUsedMemoryRecorder.Valid) gcUsedMemoryRecorder.Dispose();

            if(systemUsedMemoryRecorder.Valid) systemUsedMemoryRecorder.Dispose();
        }

        private void OnValidate()
        {
            if(!enabled) return;

            if(logToTextIntervalSec != previousLogIntervalValue || !Mathf.Approximately(previousLogIntervalValue, logToTextIntervalSec))
            {
                if(currentLogTimer >= logToTextIntervalSec) currentLogTimer = logToTextIntervalSec;

                previousLogIntervalValue = logToTextIntervalSec;
            }

            if(previousUIRefreshTime != memoryLogUIDisplayRefreshTime || !Mathf.Approximately(previousUIRefreshTime, memoryLogUIDisplayRefreshTime))
            {
                if(currentUIRefreshTimer >= memoryLogUIDisplayRefreshTime) currentUIRefreshTimer = memoryLogUIDisplayRefreshTime;

                previousUIRefreshTime = memoryLogUIDisplayRefreshTime;
            }
        }

        private void OnApplicationQuit()
        {
            if(Application.isEditor && deleteLogOnUnityClosed) DeleteMemoryLogFile();

            if (pendingDeleteOnAppClosed) DeleteMemoryLogFile();
        }

        private void Start()
        {
            LogMemoryUsageToTextFile("StartCalled");
        }

        private void Update()
        {
            if (!enabled || !enableLogInUpdate) return;

            if (currentUIRefreshTimer >= memoryLogUIDisplayRefreshTime)
            {
                GenerateMemoryLogText("UpdateCalled");

                currentUIRefreshTimer = 0.0f;
            }
            else currentUIRefreshTimer += Time.unscaledDeltaTime;

            if(currentLogTimer >= logToTextIntervalSec)
            {
                LogMemoryUsageToTextFile("UpdateCalled");

                currentLogTimer = 0.0f;
            }
            else currentLogTimer += Time.unscaledDeltaTime;
        }

        public static void LogMemoryUsageAsText(string logEventName = "n/a")
        {
            if (!memoryUsageLoggerInstance)
            {
                CreateMemoryLoggerInstance();
            }

            memoryUsageLoggerInstance.LogMemoryUsageToTextFile(logEventName);
        }

        public static void LogMemoryUsageDelay(float delaySec, string logEventName = "n/a")
        {
            if (!memoryUsageLoggerInstance)
            {
                CreateMemoryLoggerInstance();
            }

            if(delaySec <= 0.0f)
            {
                LogMemoryUsageAsText(logEventName);

                return;
            }

            memoryUsageLoggerInstance.StartCoroutine(memoryUsageLoggerInstance.LogMemoryUsageDelayCoroutine(delaySec, logEventName));
        }

        private IEnumerator LogMemoryUsageDelayCoroutine(float delaySec, string logEventName = "n/a")
        {
            yield return new WaitForSecondsRealtime(delaySec);

            LogMemoryUsageAsText(logEventName);

            yield break;
        }

        private void LogMemoryUsageToTextFile(string logEventName = "n/a")
        {
            if (!enabled) return;

            if (pathToLogFile == string.Empty ||
               string.IsNullOrEmpty(pathToLogFile) ||
               string.IsNullOrWhiteSpace(pathToLogFile) ||
               pathToLogFile == "" || pathToLogFile == null)
            {
                pathToLogFile = System.IO.Path.Combine(Application.persistentDataPath, LOG_FILE_NAME);
            }

            GenerateMemoryLogText(logEventName);

            File.AppendAllText(pathToLogFile, activeMemoryLogStruct.memoryLogSummaryText);
        }

        private void GenerateMemoryLogText(string logEventName = "n/a")
        {
            activeMemoryLogStruct.InitSetDefaultValues();

            if (totalReservedMemoryRecorder.Valid)
            {
                long memory = totalReservedMemoryRecorder.CurrentValue / 1048576;

                activeMemoryLogStruct.totalReservedMemory = memory;

                activeMemoryLogStruct.totalReservedMemoryLogText = memory.ToString() + "MB" + MemoryUsageLevelTag(memory, "Total Reserved Memory");
            }

            if(gcUsedMemoryRecorder.Valid)
            {
                long memory = gcUsedMemoryRecorder.CurrentValue / 1048576;

                activeMemoryLogStruct.gcUsedMemory = memory;

                activeMemoryLogStruct.gcUsedMemoryLogText = memory.ToString() + "MB" + MemoryUsageLevelTag(memory, "GC Used Memory");
            }

            if (systemUsedMemoryRecorder.Valid)
            {
                long memory = systemUsedMemoryRecorder.CurrentValue / 1048576;

                activeMemoryLogStruct.systemUsedMemory = memory;

                activeMemoryLogStruct.systemUsedMemoryLogText = memory.ToString() + "MB" + MemoryUsageLevelTag(memory, "System Used Memory");
            }

            activeMemoryLogStruct.memoryLogSummaryText = logEnvironmentStatus.ToString() + " Memory Log At:\n" +
                                                         "Frame: " + Time.frameCount.ToString() + "\n" +
                                                         "Log Event: " + logEventName + "\n" +
                                                         "System Used Memory: " + activeMemoryLogStruct.systemUsedMemoryLogText + "\n" +
                                                         "GC Used Memory: " + activeMemoryLogStruct.gcUsedMemoryLogText + "\n" +
                                                         "Total Reserved Memory: " + activeMemoryLogStruct.totalReservedMemoryLogText + "\n" +
                                                         "---------------------------------------\n";

            //update UI if reference to memory log UI component exists
            if (memoryUsageLogUI) memoryUsageLogUI.SetMemoryLogSummaryUIText(activeMemoryLogStruct.memoryLogSummaryText);
        }

        private string MemoryUsageLevelTag(long memoryUsed, string profilerRecorderName = "")
        {
            if (!Application.isEditor)//if in player build and NOT editor
            {
                if(profilerRecorderName == "GC Used Memory")
                {
                    if (memoryUsed <= 5) return " - LOW";

                    if (memoryUsed > 5 && memoryUsed < 15) return " - MEDIUM";

                    if (memoryUsed >= 15) return " - HIGH";
                }

                if (memoryUsed <= 400) return " - LOW";

                if (memoryUsed > 400 && memoryUsed < 700) return " - MEDIUM";

                if (memoryUsed >= 700) return " - HIGH";
            }
            else//if IN editor
            {
                if (profilerRecorderName == "GC Used Memory")
                {
                    if (memoryUsed <= 50) return " - LOW";

                    if (memoryUsed > 50 && memoryUsed < 120) return " - MEDIUM";

                    if (memoryUsed >= 120) return " - HIGH";
                }

                if (memoryUsed < 1000) return " - LOW";

                if (memoryUsed >= 1000 && memoryUsed <= 1900) return " - MEDIUM";

                if (memoryUsed > 1900) return " - HIGH";
            }

            return "";
        }

        public ActiveMemoryLogStruct GetActiveMemoryLogStructData()
        {
            activeMemoryLogStruct.InitSetDefaultValues();

            if (!enabled)
            {
                activeMemoryLogStruct.memoryLogSummaryText = "Memory Usage Logger is currently disabled!";

                return activeMemoryLogStruct;
            }

            if(Application.isEditor && !enableLogInEditor)
            {
                activeMemoryLogStruct.memoryLogSummaryText = "Memory Usage Logger is not enabled in editor.";

                return activeMemoryLogStruct;
            }

            GenerateMemoryLogText();

            return activeMemoryLogStruct;
        }

        public void DeleteMemoryLogFile()
        {
            if (pendingDeleteOnAppClosed && File.Exists(pathToLogFile)) File.Delete(pathToLogFile);

            pendingDeleteOnAppClosed = true;
        }

        public static void CreateMemoryLoggerInstance()
        {
            if (memoryUsageLoggerInstance) return;

            if (FindObjectOfType<MemoryUsageLogger>()) return;

            GameObject obj = new GameObject("MemoryUsageLogger(1InstanceOnly)");

            MemoryUsageLogger memLogger = obj.AddComponent<MemoryUsageLogger>();

            if (!memoryUsageLoggerInstance) memoryUsageLoggerInstance = memLogger;
        }

        public static void SetupMemoryLogUI(MemoryUsageLogUI logUI = null)
        {
            if (!memoryUsageLoggerInstance) return;

            if (!memoryUsageLoggerInstance.enabled) return;

            if (!memoryUsageLoggerInstance.memoryUsageLogUI)
            {
                MemoryUsageLogUI childLogUI = memoryUsageLoggerInstance.GetComponentInChildren<MemoryUsageLogUI>();

                if (childLogUI)
                {
                    memoryUsageLoggerInstance.memoryUsageLogUI = childLogUI;
                }
                else if (!childLogUI && memoryUsageLoggerInstance.memoryUsageLogUIPrefab)
                {
                    GameObject logUIGO = Instantiate(memoryUsageLoggerInstance.memoryUsageLogUIPrefab.gameObject,
                                                     Vector3.zero,
                                                     Quaternion.identity,
                                                     memoryUsageLoggerInstance.transform);

                    MemoryUsageLogUI logUIComponent; logUIGO.TryGetComponent<MemoryUsageLogUI>(out logUIComponent);

                    memoryUsageLoggerInstance.memoryUsageLogUI = logUIComponent;
                }
                else if (!childLogUI && !memoryUsageLoggerInstance.memoryUsageLogUIPrefab && logUI)
                {
                    if (logUI.transform.parent == null || logUI.transform.parent != memoryUsageLoggerInstance.transform)
                    {
                        logUI.transform.SetParent(memoryUsageLoggerInstance.transform);
                    }

                    memoryUsageLoggerInstance.memoryUsageLogUI = logUI;
                }
            }

            if (memoryUsageLoggerInstance.memoryUsageLogUI)
            {
                if (logUI && logUI != memoryUsageLoggerInstance.memoryUsageLogUI) Destroy(logUI.gameObject);
            }
        }
    }
}