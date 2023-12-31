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

        [SerializeField] private float logIntervalInSec = 5.0f;

        //INTERNALS.............................................................................

        private const string LOG_FILE_NAME = "MemoryUsageLog.txt";

        private string pathToLogFile;

        private float currentLogTimer;

        private ProfilerRecorder totalReservedMemoryRecorder;

        private ProfilerRecorder gcUsedMemoryRecorder;

        private ProfilerRecorder systemUsedMemoryRecorder;

        public struct ActiveMemoryLogStruct
        {
            public long totalReservedMemory { get; internal set; }

            public string totalReservedMemoryLogText { get; internal set; }

            public long gcUsedMemory { get; internal set; }

            public string gcUsedMemoryLogText { get; internal set; }

            public long systemUsedMemory { get; internal set; }

            public string systemUsedMemoryLogText { get; internal set; }

            public string memoryLogSummaryText { get; internal set; }

            internal void InitSetDefaultValues()
            {
                totalReservedMemory = 0L; 
                
                totalReservedMemoryLogText = "N/A";

                gcUsedMemory = 0L; 
                
                gcUsedMemoryLogText = "N/A";

                systemUsedMemory = 0L; 
                
                systemUsedMemoryLogText = "N/A";

                memoryLogSummaryText = "N/A";
            }
        }

        private ActiveMemoryLogStruct activeMemoryLogStruct;

        private static MemoryUsageLogger memoryUsageLoggerInstance;

        private void Awake()
        {
            if(memoryUsageLoggerInstance && memoryUsageLoggerInstance != this)
            {
                Destroy(gameObject);

                return;
            }

            memoryUsageLoggerInstance = this;

            DontDestroyOnLoad(gameObject);
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

            if (Application.isEditor) newMemoryLogSession = "-----NEW IN-EDITOR MEMORY LOG SESSION STARTED-----\n";
            else newMemoryLogSession = "-----NEW IN-BUILD MEMORY LOG SESSION STARTED-----\n";

            File.WriteAllText(pathToLogFile, newMemoryLogSession);

            currentLogTimer = logIntervalInSec;

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

        private void OnApplicationQuit()
        {
            if(Application.isEditor && deleteLogOnUnityClosed && File.Exists(pathToLogFile)) File.Delete(pathToLogFile);
        }

        private void Start()
        {
            LogMemoryUsageToTextFile("StartCalled");
        }

        private void Update()
        {
            if (!enabled || !enableLogInUpdate) return;

            if(currentLogTimer >= logIntervalInSec)
            {
                LogMemoryUsageToTextFile("UpdateCalled");

                currentLogTimer = 0.0f;

                return;
            }

            currentLogTimer += Time.unscaledDeltaTime;
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

            activeMemoryLogStruct.memoryLogSummaryText = "---Memory Log At:\n" +
                                                         "Frame: " + Time.frameCount.ToString() + "\n" +
                                                         "Log Event: " + logEventName + "\n" +
                                                         "System Used Memory: " + activeMemoryLogStruct.systemUsedMemoryLogText + "\n" +
                                                         "GC Used Memory: " + activeMemoryLogStruct.gcUsedMemoryLogText + "\n" +
                                                         "Total Reserved Memory: " + activeMemoryLogStruct.totalReservedMemoryLogText + "\n" +
                                                         "---------------------------------------\n";
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

        public static void CreateMemoryLoggerInstance()
        {
            if (memoryUsageLoggerInstance) return;

            if (FindObjectOfType<MemoryUsageLogger>()) return;

            GameObject obj = new GameObject("MemoryUsageLogger(1InstanceOnly)");

            MemoryUsageLogger memLogger = obj.AddComponent<MemoryUsageLogger>();

            if (!memoryUsageLoggerInstance) memoryUsageLoggerInstance = memLogger;
        }
    }
}