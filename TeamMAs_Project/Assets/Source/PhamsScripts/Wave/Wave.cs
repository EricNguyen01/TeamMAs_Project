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
        private List<VisitorUnitSO> totalVisitorsToSpawnList = new List<VisitorUnitSO>();

        //This list stores the total visitorSO types in this wave
        //E.g: If this wave has VisitorSO of type X, Y, and Z (all have spawn nums > 0), there will be 3 elements of X,Y,Z in this list.
        private List<VisitorUnitSO> visitorTypes = new List<VisitorUnitSO>();

        //This list stores the number of VisitorSO based on their chance
        //For example visitorX has 50 spawn chance -> 50 visitorX goes into this list and visitorY has 25 spawn chance -> 25 visitorY goes into list
        //Then when spawn based on chance, get a random non-repeat number in the range of this list
        //The list element that was randomly landed on will be the Visitor for spawning.
        private List<VisitorUnitSO> visitorSpawnChanceList = new List<VisitorUnitSO>();
        
        //same with above list but for bosses (for use with spawnBossLast option in WaveSO.cs)
        private List<VisitorUnitSO> visitorBossSpawnChanceList = new List<VisitorUnitSO>();

        //this list keeps track of all the currently active visitor gameobjects spawned by this wave
        //wave will not end until this list is empty.
        private List<VisitorUnit> activeVisitorsInScene = new List<VisitorUnit>();

        private int waveNum;

        private int currentSpawnRandomNumber = 0;
        private int previousSpawnRandomNumber = -1;

        //for 1st time switching to boss spawn chance list
        private bool justSwitchedToBossSpawnChanceList = false;

        private bool waveHasAlreadyStarted = false;//to avoid overlapping wave start process

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        private void LoadTotalVisitorAndVisitorTypesListForThisWave(WaveSO waveSO)
        {
            if (waveSO == null) return;

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

        private void LoadVisitorSpawnChanceListForThisWave(WaveSO waveSO)
        {
            if (waveSO == null) return;

            if (waveSO.visitorTypesToSpawnThisWave == null || waveSO.visitorTypesToSpawnThisWave.Length == 0) return;

            //do not load spawn chance list if there's only 1 type of visitor in wave (always 100% chance to spawn this type)
            if (waveSO.visitorTypesToSpawnThisWave.Length == 1)
            {
                Debug.Log("Wave " + waveSO.name + " only has 1 type of visitor!");
                return;
            }

            for (int i = 0; i < waveSO.visitorTypesToSpawnThisWave.Length; i++)
            {
                if (waveSO.visitorTypesToSpawnThisWave[i].visitorType == null) continue;

                //if a type has spawn chance of 0 -> count it as a spawn chance of 1 -> continue loop
                if (waveSO.visitorTypesToSpawnThisWave[i].spawnChance == 0)
                {
                    AddVisitorTypesToSpawnChanceList(waveSO.visitorTypesToSpawnThisWave[i].visitorType);
                    
                    continue;
                }

                //if higher than 1 spawn chance -> add based on total spawn chance of current visitor type
                for(int j = 0; j < waveSO.visitorTypesToSpawnThisWave[i].spawnChance; j++)
                {
                    AddVisitorTypesToSpawnChanceList(waveSO.visitorTypesToSpawnThisWave[i].visitorType);
                }
            }
        }

        private void AddVisitorTypesToSpawnChanceList(VisitorUnitSO visitorSO)
        {
            //if in case of spawnBossLast = true
            if (waveSO.spawnBossLast)
            {
                //check if visitor type is boss -> if yes set to boss list
                if (visitorSO.isBoss)
                {
                    visitorBossSpawnChanceList.Add(visitorSO);
                    return;
                }
            }
            
            //if none of the above
            visitorSpawnChanceList.Add(visitorSO);
        }

        private VisitorUnitSO GetVisitorTypeToSpawnBasedOnChance()
        {
            //if normal visitors spawn chance list count is 0, it means that there's only 1 type of visitor in this wave (100% spawn chance)
            //OR it could mean that this wave contains all boss types
            if(visitorSpawnChanceList.Count == 0)
            {
                //if visitorBossSpawnChance list is also 0 in length -> we can be sure that there is only exactly 1 type of normal visitor in wave
                if (visitorBossSpawnChanceList.Count == 0)
                {
                    //check for null value
                    if (totalVisitorsToSpawnList == null || totalVisitorsToSpawnList.Count == 0) return null;

                    //in case of 1 type only with 100% chance, return that type everytime
                    for (int i = 0; i < totalVisitorsToSpawnList.Count; i++)
                    {
                        if (totalVisitorsToSpawnList[i] != null) return totalVisitorsToSpawnList[i];
                    }
                }
            }
            //if the above chunk is finished and the function is not returned yet:

            //check for whether a visitor type has all been spawned up is done in RemoveVisitorTypeIfDepleted() in SpawnAVisitor
            //any type that has been spawned up will also be removed from spawnChance lists (normal + boss) in RemoveVisitorTypeIfDepleted().
            //we don't have to do that again here.

            //The below remaining chunk processes getting random visitor types based on chance for more than 1 type
            //and for whether spawnBossLast is enabled or disabled.

            currentSpawnRandomNumber = 0;

            VisitorUnitSO visitorType;

            List<VisitorUnitSO> spawnChanceList = visitorSpawnChanceList;

            //if spawnBossLast option is set to true
            if (waveSO.spawnBossLast)
            {
                //if all normal visitor types have been spawned up -> switch to use bosses spawn chance list and start spawn bosses only
                if (visitorSpawnChanceList.Count == 0)
                {
                    if (!justSwitchedToBossSpawnChanceList)
                    {
                        previousSpawnRandomNumber = -1;
                        justSwitchedToBossSpawnChanceList = true;
                    }

                    spawnChanceList = visitorBossSpawnChanceList;
                }
            }

            //process looking for a valid random visitor type to spawn based on current selected spawn chance list
            while (true)
            {
                if (spawnChanceList.Count == 0) return null;

                visitorType = GetVisitorTypeFromSpawnChanceList(spawnChanceList);

                if (visitorType == null) continue;

                return visitorType;
            }
        }

        private VisitorUnitSO GetVisitorTypeFromSpawnChanceList(List<VisitorUnitSO> spawnChanceList)
        {
            currentSpawnRandomNumber = Random.Range(0, spawnChanceList.Count);

            //make sure rand is not a repeat 
            if (currentSpawnRandomNumber == previousSpawnRandomNumber) return null;

            previousSpawnRandomNumber = currentSpawnRandomNumber;

            if (spawnChanceList[currentSpawnRandomNumber] == null) return null;

            return spawnChanceList[currentSpawnRandomNumber];
        }

        private void SpawnAVisitor(VisitorUnitSO visitorSO)
        {
            if(visitorSO == null)
            {
                Debug.LogError("Trying to spawn a null visitor in wave: " + waveSO.name + ".");
                return;
            }

            if (!totalVisitorsToSpawnList.Contains(visitorSO))
            {
                Debug.LogWarning("Wave " + waveSO.name + ": Current visitor type: " + visitorSO.name + " spawn numbers exceeded its preset spawn numbers!");
                return;
            }

            if(waveSpawnerOfThisWave.visitorPools == null)
            {
                Debug.LogError("Trying to spawn a visitor in wave: " + waveSO.name + " but no visitor pools to spawn visitor from. " +
                "Check WaveSpawner.cs!");

                return;
            }

            VisitorPool pool = null;

            //get the pool for this type of visitor from WaveSpawner pools list
            for (int i = 0; i < waveSpawnerOfThisWave.visitorPools.Count; i++)
            {
                if (waveSpawnerOfThisWave.visitorPools[i].visitorTypeInPool == visitorSO)
                {
                    pool = waveSpawnerOfThisWave.visitorPools[i];
                    break;
                }
            }

            //if, for some reasons, there's no pool for the current visitorSO that we are trying to spawn -> make one:
            if (pool == null)
            {
                if (visitorSO.isBoss)
                {
                    pool = waveSpawnerOfThisWave.AddVisitorPoolForNewVisitorType(visitorSO, waveSO.GetSpawnNumberOfVisitorType(visitorSO));
                }
                else
                {
                    pool = waveSpawnerOfThisWave.AddVisitorPoolForNewVisitorType(visitorSO, waveSO.GetSpawnNumberOfVisitorType(visitorSO) * 2);
                }
            }

            //if the correct pool for this visitorSO is found -> spawn from that pool (active a current inactive and unused visitor)
            if (pool != null)
            {
                GameObject visitorGO = pool.EnableVisitorFromPool();

                VisitorUnit visitorUnitScriptComp = visitorGO.GetComponent<VisitorUnit>();

                if (visitorUnitScriptComp != null)
                {
                    visitorUnitScriptComp.SetWaveSpawnedThisVisitor(this);

                    activeVisitorsInScene.Add(visitorUnitScriptComp);
                }

                //if totalVisitorsToSpawn list contains this type of visitor, removes the first occurence of this type of visitor
                if(totalVisitorsToSpawnList.Contains(visitorSO)) totalVisitorsToSpawnList.Remove(visitorSO);

                //Removes any VisitorSO elements in the visitorTypes and spawn chance list
                //if there's no more visitors of that corresponding type to spawn.
                RemoveVisitorTypeIfDepleted(visitorSO);

                Debug.Log("Visitor: " + visitorSO.displayName + " successfully spawned in wave " + waveSO.name + 
                ". Remaining Visitors: " + totalVisitorsToSpawnList.Count);
            }
        }

        //This function removes any VisitorSO elements in the visitorTypes and spawn chance list
        //if there's no more visitors of that corresponding type to spawn.
        private void RemoveVisitorTypeIfDepleted(VisitorUnitSO visitorSO)
        {
            //if the current visitor types list has this visitorSO type
            if (visitorTypes.Contains(visitorSO))
            {
                //if this visitor type has been spawned up and no longer exists in total visitors to spawn list -> removes this visitor type
                if (!totalVisitorsToSpawnList.Contains(visitorSO)) visitorTypes.Remove(visitorSO);
            }

            //if the current normal visitor type spawn chance list contains this visitor type
            if (visitorSpawnChanceList.Contains(visitorSO))
            {
                //if this visitor type has spawned up -> also removes all of this visitor type instances in spawn chance list
                if (!totalVisitorsToSpawnList.Contains(visitorSO)) visitorSpawnChanceList.RemoveAll(visitorType => visitorType == visitorSO);
            }

            //same as above if but for boss type
            if (visitorBossSpawnChanceList.Contains(visitorSO))
            {
                if (!totalVisitorsToSpawnList.Contains(visitorSO)) visitorBossSpawnChanceList.RemoveAll(visitorType => visitorType == visitorSO);
            }
        }

        private IEnumerator VisitorSpawnCoroutine()
        {
            waveHasAlreadyStarted = true;

            //wait time until 1st visitor spawns
            yield return new WaitForSeconds(waveSO.waitTimeToFirstSpawn);

            //if was supposed to spawn but no visitor to spawn -> stop wave and break from spawn coroutine
            if (totalVisitorsToSpawnList == null || totalVisitorsToSpawnList.Count == 0)
            {
                WaveStoppedWithNoVisitorLeft();
                yield break;
            }
            //else
            //spawn visitors based on their chance with wait time in between until all visitor types and their numbers are spawned up
            Debug.Log("Wave " + waveSO.name + " Successfully Started! Total visitors: " + totalVisitorsToSpawnList.Count);

            while(totalVisitorsToSpawnList.Count > 0)
            {
                SpawnAVisitor(GetVisitorTypeToSpawnBasedOnChance());

                yield return new WaitForSeconds(waveSO.waitTimeBetweenSpawn);
            }

            //if all visitors have been spawned -> wait for them to all become inactive to end wave
            yield return new WaitUntil(() => activeVisitorsInScene.Count == 0);

            //process wave stopped
            WaveStoppedWithNoVisitorLeft();

            Debug.Log("Wave " + waveSO.name + " Successfully Stopped! Total visitors: " + totalVisitorsToSpawnList.Count);
        }

        private bool WaveStoppedWithNoVisitorLeft()
        {
            if (totalVisitorsToSpawnList == null || totalVisitorsToSpawnList.Count == 0)
            {
                waveSpawnerOfThisWave.ProcessWaveFinished(waveNum, true);

                waveHasAlreadyStarted = false;

                return true;
            }

            return false;
        }

        public void InitializeWave(WaveSpawner waveSpawner, WaveSO waveSO, int waveNum)
        {
            waveSpawnerOfThisWave = waveSpawner;
            this.waveSO = waveSO;
            this.waveNum = waveNum;

            float startInitTime = Time.realtimeSinceStartup;
            LoadTotalVisitorAndVisitorTypesListForThisWave(waveSO);
            LoadVisitorSpawnChanceListForThisWave(waveSO);
            float endInitTime = Time.realtimeSinceStartup - startInitTime;
            Debug.Log("Wave: " + waveSO.name + " finished initializing. Took: " + endInitTime * 1000.0f + "ms.");
        }

        public void ProcessWaveStarts()
        {
            //cant call visitor spawn coroutine multiple times if wave's been already started and running
            if (waveHasAlreadyStarted) return;

            //check if all visitors in wave has been spawned -> if yes, stop wave
            bool waveHasStopped = WaveStoppedWithNoVisitorLeft();

            if (waveHasStopped)
            {
                Debug.LogWarning("Wave: " + waveSO.name + " is started but there's no visitors left to spawn so it has been stopped!");
                return;
            }

            //if there are still visitors to spawn -> start wave coroutine
            StartCoroutine(VisitorSpawnCoroutine());
        }

        public void RemoveInactiveVisitorsFromActiveList(VisitorUnit inactiveVisitorGO)
        {
            if (activeVisitorsInScene.Contains(inactiveVisitorGO))
            {
                activeVisitorsInScene.Remove(inactiveVisitorGO);
            }
        }
    }
}
