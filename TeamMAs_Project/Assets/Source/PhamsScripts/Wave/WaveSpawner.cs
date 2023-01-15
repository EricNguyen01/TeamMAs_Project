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

        public bool waveAlreadyStarted { get; private set; } = false;

        //Wave events declarations
        //StartWaveUI.cs receives these events to enable/disable start wave Button UI.
        //PlantAimShootSystem.cs receives these events to enable/disable plant targetting/shooting
        public static event System.Action<WaveSpawner, int> OnWaveStarted;
        public static event System.Action<WaveSpawner, int, bool> OnWaveFinished;
        public static event System.Action<WaveSpawner, bool> OnAllWaveSpawned;

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

            CreateWaveObjects();//Not actual Unity GameObject. Just Wave C# class object in memory. Check Wave.cs

            float endInitTime = Time.realtimeSinceStartup - startInitTime;

            Debug.Log("WaveSpawner Finished Initializing VisitorPools and Child Wave Objects. Took: " + endInitTime * 1000.0f + "ms.");
        }

        private void CreateWaveObjects()
        {
            for(int i = 0; i < waveSOList.Count; i++)
            {
                if (waveSOList[i] == null)
                {
                    Debug.LogError("Wave ScriptableObject list element: " + i + " in WaveSpawner: " + name + " is null. This wave won't work!");
                    wavesList.Add(null);
                    continue;
                }

                //Create wave object by calling its constructor and fills its parameters
                Wave wave = /*waveObj.GetComponent<Wave>()*/ new Wave(this, waveSOList[i], i);

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
                Debug.LogWarning("Trying to start a wave that is out of the WaveSpawner: " + name + "'s waveList range! WaveSpawner stopped!");
                return;
            }

            if (wavesList == null || wavesList.Count == 0)
            {
                Debug.LogWarning("Trying to start a wave but there is no Wave object spawned by WaveSpawner: " + name + "!");
                return;
            }

            if (wavesList[waveNum] == null)
            {
                Debug.LogError("Trying to start wave: " + waveNum + " in waveList but it is null! " +
                "Starting the next wave instead...");

                //JumpToWave if set startWaveAfterJump to true, will call this StartWave func with the newly provided waveNum para
                //if next wave is still null,
                //it will keeps calling JumpToWave with next wave value as para creating a nice recursive loop
                //until WaveSpawner is stopped from running out of waveList range.
                JumpToWave(waveNum++, true);

                return;
            }

            if (waveAlreadyStarted)
            {
                Debug.LogWarning("Trying to start a wave but a wave has already started and is in process!");
                return;
            }

            //if any wave is running while we abt to start a new wave -> stop the ongoing waves
            for (int i = 0; i < wavesList.Count; i++)
            {
                if (wavesList[i] == null) continue;

                //if (wavesList[i].gameObject.activeInHierarchy) wavesList[i].gameObject.SetActive(false);
                if (wavesList[i].waveHasAlreadyStarted) ProcessWaveFinished(i, false, false);
            }

            //Call the ProcessWaveStarted function in Wave.cs to start a new selected wave based on the provided waveNum parameter
            wavesList[waveNum].ProcessWaveStarted();

            waveAlreadyStarted = true;

            //invoke wave started event
            OnWaveStarted?.Invoke(this, waveNum);
        }

        //find if there's any other ongoing waves started by other wavespawner apart from this wavespawner
        //has looping -> should only call in 1 frame only -> dont use in Update()
        private bool FindOtherOnGoingWaves()
        {
            foreach (WaveSpawner waveSpawner in FindObjectsOfType<WaveSpawner>())
            {
                if (waveSpawner == this) continue;

                if (waveSpawner.waveAlreadyStarted)
                {
                    return true;
                }
            }

            return false;
        }

        //PUBLICS..........................................................................

        //This function is to be called by WaveStart UI Button event to start a wave from current wave number.
        public void StartCurrentWave()
        {
            StartWave(currentWave);
        }

        public void ProcessWaveFinished(int waveNum, bool incrementWaveOnFinished, bool broadcastWaveFinishedEvent)
        {
            if (wavesList[waveNum] == null) return;

            wavesList[waveNum].ProcessWaveStopped();

            waveAlreadyStarted = false;

            if (!broadcastWaveFinishedEvent) return;

            //invoke different wave ended events that depend on whether the last wave has spawned or not
            if (currentWave < wavesList.Count - 1)
            {
                Debug.Log("OnWaveFinished Invoked!");

                OnWaveFinished?.Invoke(this, waveNum, FindOtherOnGoingWaves());

                if (incrementWaveOnFinished) currentWave++;
            }
            else
            {
                Debug.Log("OnAllWaveSpawned Invoked!");
                OnAllWaveSpawned?.Invoke(this, FindOtherOnGoingWaves());
            }
        }

        public void JumpToWave(int waveNum, bool startWaveAfterJump)
        {
            if (waveNum < 0 || waveNum >= wavesList.Count)
            {
                Debug.LogWarning("Trying to start a wave that is out of the WaveSpawner: " + name + "'s waveList range! WaveSpawner stopped!");
                return;
            }

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
                VisitorPool pool = new VisitorPool(this, visitorSO, transform);

                pool.CreateAndAddInactiveVisitorsToPool(visitorNumbersToPool);

                visitorPools.Add(pool);

                visitorTypesWithPoolExisted.Add(visitorSO);

                return pool;
            }

            //if there are exisiting pools
            //check if a pool for this visitorSO is already existed
            //if existed -> exit function
            if (visitorTypesWithPoolExisted.Contains(visitorSO)) return null;

            //if no visitor pool of this visitor existed yet -> add
            VisitorPool visitorPool = new VisitorPool(this, visitorSO, transform);

            visitorPool.CreateAndAddInactiveVisitorsToPool(visitorNumbersToPool);

            visitorPools.Add(visitorPool);

            visitorTypesWithPoolExisted.Add(visitorSO);

            return visitorPool;
        }
    }
}
