// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamMAsTD
{
    public class WaveSpawnerManager : MonoBehaviour
    {
        public List<WaveSpawner> waveSpawnersList { get; private set; } = new List<WaveSpawner>();

        public static WaveSpawnerManager waveSpawnerManagerInstance;

        private void Awake()
        {
            if(waveSpawnerManagerInstance && waveSpawnerManagerInstance != this)
            {
                Destroy(gameObject);

                return;
            }

            waveSpawnerManagerInstance = this;

            DontDestroyOnLoad(gameObject);

            WaveSpawner[] waveSpawners = FindObjectsOfType<WaveSpawner>();

            for(int i = 0; i < waveSpawners.Length; i++)
            {
                waveSpawnersList.Add(waveSpawners[i]);
            }
        }

        public void AddWaveSpawnerToList(WaveSpawner waveSpawner)
        {
            if (waveSpawner == null) return;

            if (waveSpawnersList.Contains(waveSpawner)) return;

            waveSpawnersList.Add(waveSpawner);
        }

        public void RemoveWaveSpawnerFromList(WaveSpawner waveSpawner)
        {
            if (waveSpawner == null) return;

            if (!waveSpawnersList.Contains(waveSpawner)) return;

            waveSpawnersList.Remove(waveSpawner);
        }

        public bool HasActiveWaveSpawnersExcept(WaveSpawner checkingWaveSpawner)
        {
            for(int i = 0; i < waveSpawnersList.Count; i++)
            {
                if (checkingWaveSpawner != null && waveSpawnersList[i] == checkingWaveSpawner) continue;

                if (waveSpawnersList[i].waveAlreadyStarted) return true;
            }

            return false;
        }

        public static void CreateWaveSpawnerManagerInstance()
        {
            if (waveSpawnerManagerInstance) return;

            if(FindObjectOfType<WaveSpawner>()) return;

            GameObject go = new GameObject("WaveSpawnerManager(1InstanceOnly)");

            WaveSpawnerManager spawnerManager = go.AddComponent<WaveSpawnerManager>();

            if (!waveSpawnerManagerInstance) waveSpawnerManagerInstance = spawnerManager;
        }
    }
}
