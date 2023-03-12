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
        [Header("Wave Spawner And Waves Config")]
        [SerializeField] private List<WaveSO> waveSOList = new List<WaveSO>();

        [SerializeField] private WaveSpawnerManager waveSpawnerManagerPrefab;//in case WaveSpawner is in use and WaveSpawnerManager is missing

        [field: Header("Debug Section")]
        [field: SerializeField] public bool showDebugLog { get; private set; } = false;
        [field: SerializeField] public bool showWavesEndHealthProcessDebugLog { get; private set; } = false;
        [field: SerializeField] public bool showWaveTimerLog { get; private set; } = true;

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

        private float waveTimerStartTime = 0.0f;

        private float waveTimerEndTime = 0.0f;

        //Wave events declarations
        //StartWaveUI.cs receives these events to enable/disable start wave Button UI.
        //PlantAimShootSystem.cs receives these events to enable/disable plant targetting/shooting.
        //Rain.cs receives these events to enable, well...rain.
        //BattleMusicPlayer.cs receives these events to enable/disable battle theme
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

            if(showDebugLog) Debug.Log("WaveSpawner Finished Initializing VisitorPools and Child Wave Objects. Took: " + endInitTime * 1000.0f + "ms.");
        }

        private void OnEnable()
        {
            if(WaveSpawnerManager.waveSpawnerManagerInstance != null)
            {
                WaveSpawnerManager.waveSpawnerManagerInstance.AddWaveSpawnerToList(this);
            }
            else
            {
                if(waveSpawnerManagerPrefab != null)
                {
                    Instantiate(waveSpawnerManagerPrefab, Vector3.zero, Quaternion.identity);
                }
            }
        }

        private void OnDisable()
        {
            if (WaveSpawnerManager.waveSpawnerManagerInstance != null)
            {
                WaveSpawnerManager.waveSpawnerManagerInstance.RemoveWaveSpawnerFromList(this);
            }

            StopAllCoroutines();
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
                Wave wave = new Wave(this, waveSOList[i], i);

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
                Debug.LogWarning("Trying to start a wave that is out of the WaveSpawner: " + name + "'s waveList range! " +
                    "\n Remember that WaveSpawner's WaveList range starts at the 0 index not 1!" +
                    "\n WaveSpawner stopped!");

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

            //if any wave is running while we abt to start a new wave -> stop the ongoing waves - CURRENTLY DEPRACATED!
            /*
            for (int i = 0; i < wavesList.Count; i++)
            {
                if (wavesList[i] == null) continue;

                //if (wavesList[i].gameObject.activeInHierarchy) wavesList[i].gameObject.SetActive(false);
                if (wavesList[i].waveHasAlreadyStarted) ProcessWaveFinished(i, false, false);
            }*/

            //Call the ProcessWaveStarted function in Wave.cs to start a new selected wave based on the provided waveNum parameter
            wavesList[waveNum].ProcessWaveStarted();

            waveAlreadyStarted = true;

            //log wave start time
            waveTimerStartTime = Time.realtimeSinceStartup;

            //invoke wave started event
            OnWaveStarted?.Invoke(this, waveNum);
        }

        //find if there's any other ongoing waves started by other wavespawner apart from this wavespawner
        //has looping -> should only call in 1 frame only -> dont use in Update()
        private bool FindOtherOnGoingWaves()
        {
            //if WaveSpawnerManager exists ->
            //check for active wave spawners in scene through it instead of running FindObjectsOfType which is expensive
            if(WaveSpawnerManager.waveSpawnerManagerInstance != null)
            {
                return WaveSpawnerManager.waveSpawnerManagerInstance.HasActiveWaveSpawnersExcept(this);
            }

            //else, run FindObjectsOfType instead lol!
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

        private void LogWaveRunTimeInSeconds(int waveNum)
        {
            if (!showWaveTimerLog) return;

            //calculate wave total runtime
            float totalWaveRuntime = waveTimerEndTime - waveTimerStartTime;

            //log time
            if(waveSOList != null && waveSOList.Count > 0)
            {
                Debug.Log("Wave: " + waveSOList[waveNum].name + " Took: " + totalWaveRuntime.ToString() + "s to finish!");
            }

            //reset timer
            waveTimerStartTime = 0.0f; waveTimerEndTime = 0.0f;
        }

        /*This function is DEPRACATED!!!
        private void HealEmotionalHealthAfterBossWave(WaveSO waveSO)//parameter is waveSO of the wave that just finished.
        {
            if (waveSO.visitorTypesToSpawnThisWave == null || waveSO.visitorTypesToSpawnThisWave.Length == 0) return;

            bool isBossWave = false;

            //if the waveSO of wave that just finished has a boss in its visitor types list -> wave is boss wave.
            for(int i = 0; i < waveSO.visitorTypesToSpawnThisWave.Length; i++)
            {
                if (waveSO.visitorTypesToSpawnThisWave[i].visitorType.isBoss)
                {
                    isBossWave = true;//boss wave is determined

                    break;//exit loop immediately
                }
            }

            if (!isBossWave) return;//if after checking through wave visitor types list and it was not a boss wave -> exit function

            //else if it was a boss wave

            //check for existing emotional health game resource
            if (GameResource.gameResourceInstance != null && GameResource.gameResourceInstance.emotionalHealthSO != null)
            {
                if(showWavesEndHealthProcessDebugLog) Debug.Log("Heal Health After Boss Wave!");

                //heal up amount according to EmotionalHealthSO's health healed after boss wave data
                GameResource.gameResourceInstance.emotionalHealthSO.AddResourceAmount(GameResource.gameResourceInstance.emotionalHealthSO.healthHealedAfterBossWave);
            }
        }*/

        /*This function is DEPRACATED!!!
        private void IncreaseTotalBaseEmotionalHealth()
        {
            //check for existing emotional health game resource
            if (GameResource.gameResourceInstance != null && GameResource.gameResourceInstance.emotionalHealthSO != null)
            {
                //if health amount is current = health amount cap (is at full hp)
                if(GameResource.gameResourceInstance.emotionalHealthSO.resourceAmount == GameResource.gameResourceInstance.emotionalHealthSO.resourceAmountCap)
                {
                    if (showWavesEndHealthProcessDebugLog) Debug.Log("Health is full. Increase Current Health and Total Health Cap!");

                    //increase both health cap and current health on wave end.
                    GameResource.gameResourceInstance.emotionalHealthSO.IncreaseResourceAmountCap(GameResource.gameResourceInstance.emotionalHealthSO.totalBaseHealthIncreaseOnWaveEnd, true);
                    //exit
                    return;
                }
                //else -> only icnrease health cap
                if (showWavesEndHealthProcessDebugLog) Debug.Log("Health is not full. Increase Total Health Cap Only!");

                GameResource.gameResourceInstance.emotionalHealthSO.IncreaseResourceAmountCap(GameResource.gameResourceInstance.emotionalHealthSO.totalBaseHealthIncreaseOnWaveEnd, false);
            }
        }*/

        private void DropCoinsOnWaveEnded(WaveSO waveJustEnded)
        {
            if (GameResource.gameResourceInstance == null || GameResource.gameResourceInstance.coinResourceSO == null) return;

            CoinResourceSO coinResourceSO = GameResource.gameResourceInstance.coinResourceSO;

            GameResource.gameResourceInstance.coinResourceSO.AddResourceAmount(coinResourceSO.coinsGainOnWaveEnded + waveJustEnded.extraCoinsDropFromWave);
        }

        //PUBLICS..........................................................................

        //This function is to be called by WaveStart UI Button event to start a wave from current wave number.
        public void StartCurrentWave()
        {
            StartWave(currentWave);
        }

        public void DEBUG_StartWaveAt(int waveNum)
        {
            //MUST use JumpToWave here instead of StartWave to avoid bugs!!!
            JumpToWave(waveNum, true);

            //decrease current wave by 1 so that when the currently running wave is finished, current wave ends up the same
            currentWave--;
        }

        public void ProcessWaveFinished(int waveNum, bool incrementWaveOnFinished, bool broadcastWaveFinishedEvent)
        {
            if (wavesList[waveNum] == null) return;

            wavesList[waveNum].ProcessWaveStopped();

            waveAlreadyStarted = false;

            waveTimerEndTime = Time.realtimeSinceStartup;

            LogWaveRunTimeInSeconds(waveNum);

            //if IS NOT LAST WAVE in wave list and broadcastWaveFinishedEvent is set to false -> dont broadcast and return
            //if LAST WAVE -> ALWAYS broadcast event because we need to disable startwave button UI (UI must picked up last wave event)
            if (currentWave < wavesList.Count - 1 && !broadcastWaveFinishedEvent) return;

            //else if broadcast event is true->
            //invoke different wave ended events that depend on whether the last wave has spawned or not
            //if the wave just ended is not the final wave:
            if (currentWave < wavesList.Count - 1)
            {
                //Debug.Log("OnWaveFinished Invoked!");
                bool hasOtherOngoingWaves = FindOtherOnGoingWaves();

                //coins drop on wave ended
                DropCoinsOnWaveEnded(waveSOList[waveNum]);

                OnWaveFinished?.Invoke(this, waveNum, hasOtherOngoingWaves);

                //process emotional health change if no other waves from other WaveSpawner are currently running
                //DEPRACATED!!!
                /*if (!hasOtherOngoingWaves)
                {
                    IncreaseTotalBaseEmotionalHealth();

                    HealEmotionalHealthAfterBossWave(waveSOList[waveNum]);
                }*/

                if (incrementWaveOnFinished) currentWave++;
            }
            //else if wave just ended is the final wave
            else
            {
                //Debug.Log("OnAllWaveSpawned Invoked!");

                //broadcast all waves of this WaveSpawner just finished running event
                OnAllWaveSpawned?.Invoke(this, FindOtherOnGoingWaves());
            }
        }

        public void JumpToWave(int waveNum, bool startWaveAfterJump)
        {
            if (waveNum < 0 || waveNum >= wavesList.Count)
            {
                Debug.LogWarning("Trying to start a wave that is out of the WaveSpawner: " + name + "'s waveList range! " +
                    "\n Remember that WaveSpawner's WaveList range starts at the 0 index not 1!" +
                    "\n WaveSpawner stopped!");

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

        public Wave GetCurrentWave()
        {
            return wavesList[currentWave];
        }

        public Wave GetNextWaveFromCurrentWave(int lookAheadNum)
        {
            int targetWaveNum = currentWave + lookAheadNum;

            if(targetWaveNum >= wavesList.Count) targetWaveNum = wavesList.Count - 1;

            return wavesList[targetWaveNum];
        }
    }
}
