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
    public class BuildMemoryUsageLogger : MonoBehaviour
    {
        [Header("Log Config")]

        [SerializeField] private bool enableLogInEditor = true;

        [SerializeField] private bool enableLogInUpdate = true;

        [SerializeField] private float logIntervalInSec = 10.0f;

        //INTERNALS.............................................................................

        private const string LOG_FILE_NAME = "MemoryUsageLog.txt";

        private string pathToLogFile;

        private float currentLogTimer;

        private ProfilerRecorder totalReservedMemoryRecorder;

        private ProfilerRecorder gcUsedMemoryRecorder;

        private ProfilerRecorder systemUsedMemoryRecorder;

        private string totalReservedMemoryLogText;

        private string gcUsedMemoryLogText;

        private string systemUsedMemoryLogText;

        private string memoryLogText;

        private static BuildMemoryUsageLogger memoryUsageLoggerInstance;

        private void Awake()
        {
            if(memoryUsageLoggerInstance && memoryUsageLoggerInstance != this)
            {
                Destroy(gameObject);

                return;
            }

            memoryUsageLoggerInstance = this;

            DontDestroyOnLoad(gameObject);

            if(Application.isEditor && !enableLogInEditor)
            {
                enabled = false;

                return;
            }
        }

#if UNITY_STANDALONE_WIN

        private void OnEnable()
        {
            pathToLogFile = System.IO.Path.Combine(Application.persistentDataPath, LOG_FILE_NAME);

            string newMemoryLogSession;

            if (Application.isEditor) newMemoryLogSession = "-----NEW IN-EDITOR MEMORY LOG SESSION STARTED-----\n";
            else newMemoryLogSession = "-----NEW IN-BUILD MEMORY LOG SESSION STARTED-----\n";

            File.WriteAllText(pathToLogFile, newMemoryLogSession);

            currentLogTimer = logIntervalInSec;

            totalReservedMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total Reserved Memory");

            gcUsedMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Used Memory");

            systemUsedMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "System Used Memory");
        }
#endif

#if UNITY_STANDALONE_WIN

        private void OnDisable()
        {
            totalReservedMemoryRecorder.Dispose();

            gcUsedMemoryRecorder.Dispose();

            systemUsedMemoryRecorder.Dispose();
        }
#endif

#if UNITY_STANDALONE_WIN

        private void Start()
        {
            LogMemoryUsageToTextFile("StartCalled");
        }
#endif

#if UNITY_STANDALONE_WIN

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
#endif

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
#if UNITY_STANDALONE_WIN

            if (!enabled) return;

            if (pathToLogFile == string.Empty ||
               string.IsNullOrEmpty(pathToLogFile) ||
               string.IsNullOrWhiteSpace(pathToLogFile) ||
               pathToLogFile == "" || pathToLogFile == null)
            {
                pathToLogFile = System.IO.Path.Combine(Application.persistentDataPath, LOG_FILE_NAME);
            }

            GenerateMemoryLogText(logEventName);

            File.AppendAllText(pathToLogFile, memoryLogText);
#endif
        }

        private void GenerateMemoryLogText(string logEventName = "n/a")
        {
#if UNITY_STANDALONE_WIN

            if (totalReservedMemoryRecorder.Valid)
            {
                totalReservedMemoryLogText = (totalReservedMemoryRecorder.LastValue / 1048576).ToString() + "MB";
            }
            else totalReservedMemoryLogText = "...";

            if(gcUsedMemoryRecorder.Valid)
            {
                gcUsedMemoryLogText = (gcUsedMemoryRecorder.LastValue / 1048576).ToString() + "MB";
            }
            else gcUsedMemoryLogText = "...";

            if (systemUsedMemoryRecorder.Valid)
            {
                systemUsedMemoryLogText = (systemUsedMemoryRecorder.LastValue / 1048576).ToString() + "MB";
            }
            else systemUsedMemoryLogText = "...";

            memoryLogText = "---Memory Log At:\n" +
                "Frame: " + Time.frameCount.ToString() + "\n" +
                "Log Event: " + logEventName + "\n" +
                "System Used Memory: " + systemUsedMemoryLogText + "\n" +
                "GC Used Memory: " + gcUsedMemoryLogText + "\n" +
                "Total Reserved Memory: " + totalReservedMemoryLogText + "\n" +
                "---------------------------------------\n";
#endif
        }

        public static void CreateMemoryLoggerInstance()
        {
            if (memoryUsageLoggerInstance) return;

            if (FindObjectOfType<BuildMemoryUsageLogger>()) return;

            GameObject obj = new GameObject("BuildMemoryUsageLogger(1InstanceOnly)");

            BuildMemoryUsageLogger memLogger = obj.AddComponent<BuildMemoryUsageLogger>();

            if (!memoryUsageLoggerInstance) memoryUsageLoggerInstance = memLogger;
        }
    }
}