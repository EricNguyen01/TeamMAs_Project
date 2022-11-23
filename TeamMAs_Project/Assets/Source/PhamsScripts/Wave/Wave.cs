using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamMAsTD
{
    /*
     * This class represents a single wave with wave data provided by WaveSO
     */
    [DisallowMultipleComponent]
    public class Wave : MonoBehaviour
    {
        public WaveSO waveSO { get; private set; }
        public WaveSpawner waveSpawnerOfThisWave { get; private set; }

        //This list stores all the number of all visitor SO types that will be spawned in this wave
        //E.g: if there are 10 visitors of type X, there will be 10 type X VisitorSO elements in this list
        //this list does not containt visitor game objects or prefabs (only SO data)
        //this list is merely a representation of what visitors have been spawned, of what type, and how many are left
        private List<VisitorSO> totalVisitorsToSpawnList = new List<VisitorSO>();

        //This list stores the total visitorSO types in this wave
        //E.g: If this wave has VisitorSO of type X, Y, and Z (all have spawn nums > 0), there will be 3 elements of X,Y,Z in this list.
        private List<VisitorSO> visitorTypes = new List<VisitorSO>();

        private int waveNum;

        private static event System.Action<int> OnWaveFinished;

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        private void LoadTotalVisitorAndVisitorTypesListForThisWave(WaveSO waveSO)
        {
            if (waveSO.visitorTypesToSpawnThisWave == null || waveSO.visitorTypesToSpawnThisWave.Length == 0)
            {
                Debug.LogError("Wave: " + waveSO.name + " ScriptableObject doesnt have any visitor types to spawn! Spawning stopped!");
                return;
            }

            for (int i = 0; i < waveSO.visitorTypesToSpawnThisWave.Length; i++)
            {
                if (waveSO.visitorTypesToSpawnThisWave[i].visitorType == null) continue;

                if (waveSO.visitorTypesToSpawnThisWave[i].spawnNumbers == 0) continue;

                if (!visitorTypes.Contains(waveSO.visitorTypesToSpawnThisWave[i].visitorType))
                {
                    visitorTypes.Add(waveSO.visitorTypesToSpawnThisWave[i].visitorType);
                }

                for(int j = 0; j < waveSO.visitorTypesToSpawnThisWave[i].spawnNumbers; j++)
                {
                    totalVisitorsToSpawnList.Add(waveSO.visitorTypesToSpawnThisWave[i].visitorType);
                }
            }
        }

        private VisitorSO GetVisitorTypeToSpawnBasedOnChance()
        {
            RemoveVisitorTypeIfAllSpawned();

            //TODO: Calculate chance for any remaining types 

            return null;
        }

        //Removes any VisitorSO elements in the visitorTypes list if there's no more visitors of that corresponding type to spawn.
        private void RemoveVisitorTypeIfAllSpawned()
        {
            if (visitorTypes == null || visitorTypes.Count == 0) return;

            for(int i = 0; i < visitorTypes.Count; i++)
            {
                if (!totalVisitorsToSpawnList.Contains(visitorTypes[i]))
                {
                    visitorTypes.RemoveAt(i);
                }
            }
        }

        private void SpawnAVisitor(VisitorSO visitorSO)
        {
            if(waveSpawnerOfThisWave.visitorPools == null)
            {
                Debug.LogError("Trying to spawn a visitor in wave: " + name + " but no visitor pools to spawn visitor from. " +
                "Check WaveSpawner.cs!");

                return;
            }

            for(int i = 0; i < waveSpawnerOfThisWave.visitorPools.Count; i++)
            {
                if (waveSpawnerOfThisWave.visitorPools[i].visitorTypeInPool == visitorSO)
                {
                    waveSpawnerOfThisWave.visitorPools[i].EnableVisitorFromPool();
                    return;
                }
            }

            //if, for some reasons, there's no pool for the current visitorSO that we are trying to spawn -> make one:
            if (visitorSO.isBoss)
            {
                VisitorPool pool = waveSpawnerOfThisWave.AddVisitorPoolForNewVisitorType(visitorSO, waveSO.GetSpawnNumberOfVisitorType(visitorSO));
                
                if(pool != null)
                {
                    pool.EnableVisitorFromPool();
                }
            }
            else
            {
                VisitorPool pool = waveSpawnerOfThisWave.AddVisitorPoolForNewVisitorType(visitorSO, waveSO.GetSpawnNumberOfVisitorType(visitorSO) * 2);
                
                if (pool != null)
                {
                    pool.EnableVisitorFromPool();
                }
            }
        }

        private IEnumerator VisitorSpawnCoroutine()
        {
            yield return null;
        }

        public void InitializeWave(WaveSpawner waveSpawner, WaveSO waveSO, int waveNum)
        {
            waveSpawnerOfThisWave = waveSpawner;
            this.waveSO = waveSO;
            this.waveNum = waveNum;

            LoadTotalVisitorAndVisitorTypesListForThisWave(waveSO);
        }

        public void WaveStartsSpawningVisitors()
        {
            StartCoroutine(VisitorSpawnCoroutine());
        }
    }
}
