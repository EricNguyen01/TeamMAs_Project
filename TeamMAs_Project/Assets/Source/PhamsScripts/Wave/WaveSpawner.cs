using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamMAsTD
{
    [DisallowMultipleComponent]
    /*
     * This class spawns and provides neccessary data for all wave game objects with Wave script attached (Wave objects spawned from prefabs from WaveSO)
     * This class also manages the enable/disable of a Wave object/Wave script based on the CurrentWave data
     * This class handles all the Visitor game object pools for all types of visitors 
     * and the active/inactive of all Visitors game objects from their pools
     */
    public class WaveSpawner : MonoBehaviour
    {
        [SerializeField] private List<WaveSO> waveSOList = new List<WaveSO>();

        [Header("Debug Only!")]

        [SerializeField] private bool enableDebug = false;

        [SerializeField]
        [Tooltip("For fast-forwarding to a specific wave. Set to 0 to disable this debug setting.")]
        private int waveNumberToStartAt = 0;

        //INTERNALS............................................................................

        //Each type of visitor will have their own visitor pool in which all the pools are stored in this list
        public List<VisitorPool> visitorPools { get; private set; } = new List<VisitorPool>();

        private List<VisitorUnitSO> visitorTypesWithPoolExisted = new List<VisitorUnitSO>();

        //WavesList is the children Wave gameobject list with Wave script component attached spawned from waveSOList
        //when a wave in this list is active -> it is in-use
        //Wave object is set active based on currentWave value and wave starts event
        //If a wave is null -> starting it will result in wave end state triggered immediately (basically nothing happens)
        private List<Wave> wavesList = new List<Wave>();

        public int currentWave { get; private set; } = 0;

        private bool waveAlreadyStarted = false;

        //Wave events declarations
        private static event System.Action<WaveSpawner, int> OnWaveStarted;
        private static event System.Action<WaveSpawner, int> OnWaveFinished;

        //PRIVATES...............................................................................

        private void Awake()
        {
            if(waveSOList.Count == 0)
            {
                Debug.LogWarning("Wave List in Wave Spawner: " + name + " has no waves! Wave Spawner won't work!");
                enabled = false;
                return;
            }

            float startInitTime = Time.realtimeSinceStartup;
            SetupVisitorPools();
            SpawnChildWaveObjects();
            float endInitTime = Time.realtimeSinceStartup - startInitTime;
            Debug.Log("WaveSpawner Finished Initializing VisitorPools and Child Wave Objects. Took: " + endInitTime * 1000.0f + "ms.");

            if (enableDebug)
            {
                if(waveNumberToStartAt < 0 || waveNumberToStartAt >= wavesList.Count)
                {
                    Debug.LogWarning("Invalid Wave Number To Start At! Value reset to 0!");

                    waveNumberToStartAt = 0;
                }

                currentWave = waveNumberToStartAt;
            }
        }

        private void SpawnChildWaveObjects()
        {
            for(int i = 0; i < waveSOList.Count; i++)
            {
                if (waveSOList[i].wavePrefab == null)
                {
                    Debug.LogError("Wave Prefab of WaveSO: " + waveSOList[i].name + " is null. This wave won't work!");
                    wavesList.Add(null);
                    continue;
                }

                GameObject waveObj = Instantiate(waveSOList[i].wavePrefab, transform);
                Wave wave = waveObj.GetComponent<Wave>();

                if(wave == null)
                {
                    Debug.LogError("Wave script component of WaveSO's WavePrefab: " + waveSOList[i].wavePrefab.name + " is null. This wave won't work!");
                    wavesList.Add(null);
                    continue;
                }

                wave.InitializeWave(this, waveSOList[i], i);
                wavesList.Add(wave);
            }
        }

        private void SetupVisitorPools()
        {
            List<VisitorUnitSO> visitorsList = new List<VisitorUnitSO>();

            for(int i = 0; i < waveSOList.Count; i++)
            {
                if (waveSOList[i].visitorTypesToSpawnThisWave == null || waveSOList[i].visitorTypesToSpawnThisWave.Length == 0) continue;

                for(int j = 0; j < waveSOList[i].visitorTypesToSpawnThisWave.Length; j++)
                {
                    if (waveSOList[i].visitorTypesToSpawnThisWave[j].visitorType == null) continue;

                    if (visitorsList.Contains(waveSOList[i].visitorTypesToSpawnThisWave[j].visitorType)) continue;

                    visitorsList.Add(waveSOList[i].visitorTypesToSpawnThisWave[j].visitorType);

                    if (waveSOList[i].visitorTypesToSpawnThisWave[j].visitorType.isBoss)
                    {
                        AddVisitorPoolForNewVisitorType(waveSOList[i].visitorTypesToSpawnThisWave[j].visitorType, 
                                                        waveSOList[i].visitorTypesToSpawnThisWave[j].spawnNumbers);
                    }
                    else
                    {
                        AddVisitorPoolForNewVisitorType(waveSOList[i].visitorTypesToSpawnThisWave[j].visitorType, 
                                                        waveSOList[i].visitorTypesToSpawnThisWave[j].spawnNumbers * 2);
                    }
                }
            }
        }

        private void StartWave(int waveNum)
        {
            if (waveNum < 0 || waveNum >= wavesList.Count)
            {
                Debug.LogWarning("Trying to start an invalid wave!");
                return;
            }

            if (wavesList == null || wavesList.Count == 0)
            {
                Debug.LogWarning("Trying to start a wave but there is no Wave Game Object spawned by WaveSpawner: " + name + "!");
                return;
            }

            if (wavesList[waveNum] == null)
            {
                Debug.LogError("Trying to start wave: " + waveNum + " but it is null! Check WaveSO prefab data of this wave! " +
                "Starting the next wave instead...");
                //TODO: set starting next wave here:
                return;
            }

            if (waveAlreadyStarted)
            {
                Debug.LogWarning("Trying to start a wave but a wave has already started and is in process!");
                return;
            }

            for (int i = 0; i < wavesList.Count; i++)
            {
                if (wavesList[i] == null) continue;

                if (wavesList[i].gameObject.activeInHierarchy) wavesList[i].gameObject.SetActive(false);
            }

            wavesList[waveNum].gameObject.SetActive(true);

            //Call the spawn function in Wave.cs
            wavesList[waveNum].ProcessWaveStarts();

            waveAlreadyStarted = true;

            //invoke wave started event
            OnWaveStarted?.Invoke(this, waveNum);
        }

        private void IncrementWaveNumber()
        {
            if (currentWave + 1 < 0 || currentWave + 1 >= wavesList.Count) return;

            currentWave++;
        }

        //PUBLICS..........................................................................

        //This function is to be called by WaveStart UI Button event to start a wave from current wave number.
        public void StartCurrentWave()
        {
            StartWave(currentWave);
        }

        public void ProcessWaveFinished(int waveNum, bool incrementWaveOnFinished)
        {
            if (wavesList[waveNum] == null) return;

            wavesList[waveNum].gameObject.SetActive(false);

            if(incrementWaveOnFinished) IncrementWaveNumber();

            waveAlreadyStarted = false;

            //invoke wave ended event
            OnWaveFinished?.Invoke(this, waveNum);
        }

        public void JumpToWave(int waveNum, bool startWaveAfterJump)
        {
            if (waveNum < 0 || waveNum >= wavesList.Count) return;

            currentWave = waveNum;

            if (startWaveAfterJump) StartWave(waveNum);
        }

        public VisitorPool AddVisitorPoolForNewVisitorType(VisitorUnitSO visitorSO, int visitorNumbersToPool)
        {
            if (visitorSO == null) return null;

            //if there is no exisiting visitor pools
            if (visitorPools.Count == 0)
            {
                //add a new pool for this visitorSO then exit function
                VisitorPool pool = gameObject.AddComponent<VisitorPool>();
                pool.InitializeVisitorPool(visitorSO, visitorNumbersToPool);
                visitorPools.Add(pool);
                visitorTypesWithPoolExisted.Add(visitorSO);
                return pool;
            }

            //if there are exisiting pools
            //check if a pool for this visitorSO is already existed
            //if existed -> exit function
            if (visitorTypesWithPoolExisted.Contains(visitorSO)) return null;

            //if none existed -> add
            VisitorPool visitorPool = gameObject.AddComponent<VisitorPool>();
            visitorPool.InitializeVisitorPool(visitorSO, visitorNumbersToPool);
            visitorPools.Add(visitorPool);
            visitorTypesWithPoolExisted.Add(visitorSO);
            return visitorPool;
        }
    }
}
