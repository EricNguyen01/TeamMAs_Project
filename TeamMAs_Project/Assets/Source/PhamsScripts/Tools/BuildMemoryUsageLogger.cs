// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using System;
using System.Collections;
using System.IO;
using System.Runtime.CompilerServices;
using Unity.Profiling;
using UnityEngine;

namespace TeamMAsTD
{
    public class BuildMemoryUsageLogger : MonoBehaviour
    {
        private const string LOG_FILE_NAME = "MemoryUsageLog.txt";

        private string pathToLogFile;

        private const float logIntervalInSec = 1.0f;

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

#if UNITY_STANDALONE_WIN

            pathToLogFile = System.IO.Path.Combine(Application.persistentDataPath, LOG_FILE_NAME);

            string newMemoryLogSession = "-----NEW MEMORY LOG SESSION STARTED-----\n";

            File.WriteAllText(pathToLogFile, newMemoryLogSession);

            currentLogTimer = logIntervalInSec;
#endif
        }

        private void OnEnable()
        {
#if UNITY_STANDALONE_WIN

            totalReservedMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total Reserved Memory");

            gcUsedMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Used Memory");

            systemUsedMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "System Used Memory");
#endif
        }

        private void OnDisable()
        {
#if UNITY_STANDALONE_WIN

            totalReservedMemoryRecorder.Dispose();

            gcUsedMemoryRecorder.Dispose();

            systemUsedMemoryRecorder.Dispose();
#endif
        }

        private void Start()
        {
#if UNITY_STANDALONE_WIN

            LogMemoryUsageToTextFile("StartCalled");
#endif
        }

        private void Update()
        {
#if UNITY_STANDALONE_WIN

            if(currentLogTimer >= logIntervalInSec)
            {
                LogMemoryUsageToTextFile("n/a");

                currentLogTimer = 0.0f;

                return;
            }

            currentLogTimer += Time.deltaTime;
#endif
        }

        public void LogMemoryUsageToTextFile(string logEventName)
        {
#if UNITY_STANDALONE_WIN

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
                totalReservedMemoryLogText += (totalReservedMemoryRecorder.LastValue / 1048576).ToString() + "MB";
            }
            else totalReservedMemoryLogText = "...";

            if(gcUsedMemoryRecorder.Valid)
            {
                gcUsedMemoryLogText += (gcUsedMemoryRecorder.LastValue / 1048576).ToString() + "MB";
            }
            else gcUsedMemoryLogText = "...";

            if (systemUsedMemoryRecorder.Valid)
            {
                systemUsedMemoryLogText += (systemUsedMemoryRecorder.LastValue / 1048576).ToString() + "MB";
            }
            else systemUsedMemoryLogText = "...";

            memoryLogText = "---Memory Log At:\n" +
                "Frame: " + Time.frameCount.ToString() + "\n" +
                "Log Event: " + logEventName + "\n" +
                "System Used Memory: " + systemUsedMemoryLogText + "\n" +
                "GC Used Memory: " + gcUsedMemoryLogText + "\n" +
                "Total Reserved Memory: " + totalReservedMemoryLogText + "\n" +
                "---------------------------------------\n";
        }
#endif
    }
}