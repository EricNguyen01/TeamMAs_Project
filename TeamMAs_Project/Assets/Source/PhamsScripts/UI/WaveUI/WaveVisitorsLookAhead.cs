using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TeamMAsTD
{
    [DisallowMultipleComponent]
    public class WaveVisitorsLookAhead : MonoBehaviour
    {
        [Header("Required Components")]

        [SerializeField] private WaveSpawner waveSpawnerToLookAhead;

        [SerializeField] private WaveVisitorTypesLookAheadSlot visitorTypesLookAheadUISlotPrefab;

        [SerializeField] private GridLayoutGroup UIContentToSpawnLookAheadSlotsUnder;

        private List<WaveVisitorTypesLookAheadSlot> waveVisitorTypesLookAheadSlotsList = new List<WaveVisitorTypesLookAheadSlot>();

        private int currentWave = 0;

        private bool hasFinishedRaining = false;

        private void Awake()
        {
            if(waveSpawnerToLookAhead == null)
            {
                Debug.LogError("WaveSpawnerToLookAhead reference of WaveVisitorsLookAhead: " + name + " is missing! Disabling script...");

                enabled = false;

                return;
            }

            if(visitorTypesLookAheadUISlotPrefab == null)
            {
                Debug.LogError("VisitorTypes Look Ahead UI Slot Prefab reference of WaveVisitorsLookAhead: " + name + " is missing! Disabling script...");

                enabled = false;

                return;
            }

            if(UIContentToSpawnLookAheadSlotsUnder == null)
            {
                Debug.LogError("UI Content Object To Spawn Look Ahead Slots Under reference of WaveVisitorsLookAhead: " + name + " is missing! Disabling script...");

                enabled = false;

                return;
            }
        }

        private void OnEnable()
        {
            WaveSpawner.OnWaveFinished += UpdateVisitorTypesLookAheadOnWaveEvent;
            WaveSpawner.OnAllWaveSpawned += DisableLookAheadSlotsOnAllWaveSpawned;

            Rain.OnRainStarted += (Rain r) => hasFinishedRaining = false;
            Rain.OnRainEnded += (Rain r) => hasFinishedRaining = true;
        }

        private void OnDisable()
        {
            WaveSpawner.OnWaveFinished -= UpdateVisitorTypesLookAheadOnWaveEvent;
            WaveSpawner.OnAllWaveSpawned -= DisableLookAheadSlotsOnAllWaveSpawned;

            Rain.OnRainStarted -= (Rain r) => hasFinishedRaining = false;
            Rain.OnRainEnded -= (Rain r) => hasFinishedRaining = true;

            StopAllCoroutines();
        }

        private void Start()
        {
            if(waveSpawnerToLookAhead != null)
            {
                currentWave = waveSpawnerToLookAhead.currentWave;

                UpdateVisitorTypesLookAheadUISlotsForWave(waveSpawnerToLookAhead.GetCurrentWave());
            }
        }

        private void CreateVisitorTypesLookAheadUISlotsForWave(Wave wave, bool activeOnCreated)
        {
            if (wave == null || wave.waveSO == null || wave.waveSO.visitorTypesToSpawnThisWave == null) return;

            if (waveVisitorTypesLookAheadSlotsList.Count > 0) return;

            List<VisitorUnitSO> uniqueVisitorTypesInWave = wave.waveSO.GetWaveUniqueVisitorTypes();

            if (uniqueVisitorTypesInWave == null || uniqueVisitorTypesInWave.Count == 0) return;

            for (int i = 0; i < uniqueVisitorTypesInWave.Count; i++)
            {
                if (uniqueVisitorTypesInWave[i] == null) continue;

                CreateVisitorTypesSlotsFor(uniqueVisitorTypesInWave[i], activeOnCreated);
            }
        }

        private void CreateVisitorTypesSlotsFor(VisitorUnitSO visitorSO, bool activeOnCreated)
        {
            if (visitorSO == null) return;

            GameObject visitorLookAheadSlotGO = Instantiate(visitorTypesLookAheadUISlotPrefab.gameObject, UIContentToSpawnLookAheadSlotsUnder.transform);

            WaveVisitorTypesLookAheadSlot visitorLookAheadSlotScript = visitorLookAheadSlotGO.GetComponent<WaveVisitorTypesLookAheadSlot>();

            visitorLookAheadSlotScript.UpdateVisitorTypeLookAheadSlot(visitorSO);

            waveVisitorTypesLookAheadSlotsList.Add(visitorLookAheadSlotScript);

            if (activeOnCreated) visitorLookAheadSlotScript.EnableLookAheadUISlot(true);
            else visitorLookAheadSlotScript.EnableLookAheadUISlot(false);
        }

        private void UpdateVisitorTypesLookAheadUISlotsForWave(Wave wave)
        {
            if (wave == null || wave.waveSO == null || wave.waveSO.visitorTypesToSpawnThisWave == null) return;

            //if no visitor types look ahead slot exists -> create all the required look ahead slots for this wave
            if (waveVisitorTypesLookAheadSlotsList.Count <= 0)
            {
                CreateVisitorTypesLookAheadUISlotsForWave(wave, true);

                return;
            }

            //else if some look ahead slots exist and are in use -> update them accordingly for this wave

            int currentCreatedSlots = waveVisitorTypesLookAheadSlotsList.Count;

            List<VisitorUnitSO> waveUniqueVisitorTypes = wave.waveSO.GetWaveUniqueVisitorTypes();

            //if the currently created look ahead slot has more or equal number of slots than this wave's unique visitor types count
            if (currentCreatedSlots >= waveUniqueVisitorTypes.Count)
            {
                //loop through all the currently created look ahead slot
                for(int i = 0; i < waveVisitorTypesLookAheadSlotsList.Count; i++)
                {
                    //continue if null
                    if (waveVisitorTypesLookAheadSlotsList[i] == null) continue;

                    //if current checking slot is still in range of this wave's unique visitor types list range
                    if(i < waveUniqueVisitorTypes.Count)
                    {
                        //update slot with new visitor SO data of this wave
                        waveVisitorTypesLookAheadSlotsList[i].UpdateVisitorTypeLookAheadSlot(waveUniqueVisitorTypes[i]);

                        //enable slot display
                        waveVisitorTypesLookAheadSlotsList[i].EnableLookAheadUISlot(true);

                        continue;
                    }

                    //else if current checking slot has gone beyond the range of this wave's unique visitor types list range
                    //they are unused for this wave and thus are disabled.
                    waveVisitorTypesLookAheadSlotsList[i].UpdateVisitorTypeLookAheadSlot(null);

                    waveVisitorTypesLookAheadSlotsList[i].EnableLookAheadUISlot(false);
                }

                return;
            }
            //else if the currently created slots are less than this wave's unique visitor types count
            else
            {
                //loop through this wave's unique visitor types list
                for(int i = 0; i < waveUniqueVisitorTypes.Count; i++)
                {
                    //if current checking element is still within the currently created slots range
                    if(i < currentCreatedSlots)
                    {
                        if (waveVisitorTypesLookAheadSlotsList[i] == null) continue;

                        //update visitor SO data and set slot active
                        waveVisitorTypesLookAheadSlotsList[i].UpdateVisitorTypeLookAheadSlot(waveUniqueVisitorTypes[i]);

                        waveVisitorTypesLookAheadSlotsList[i].EnableLookAheadUISlot(true);

                        continue;
                    }

                    //if current checking element went beyond the currently created slots range
                    //-> add more slots to the current look ahead slots list
                    CreateVisitorTypesSlotsFor(waveUniqueVisitorTypes[i], true);
                }
            }
        }

        //Base update function for OnWaveFinished event in WaveSpawner.cs
        private void UpdateVisitorTypesLookAheadOnWaveEvent(WaveSpawner waveSpawner, int waveNum, bool stillHasOngoingWaves)
        {
            if (waveSpawner == null) return;

            if (waveSpawnerToLookAhead != waveSpawner) return;

            Wave waveToLookAhead = null;

            if (currentWave == waveSpawnerToLookAhead.currentWave) 
            {
                waveToLookAhead = waveSpawnerToLookAhead.GetNextWaveFromCurrentWave(1);

                currentWave += 1;
            }
            else
            {
                waveToLookAhead = waveSpawnerToLookAhead.GetCurrentWave();

                currentWave = waveSpawnerToLookAhead.currentWave;
            }

            StartCoroutine(VisitorTypesUpdateAfterRainCoroutine(waveToLookAhead));
        }

        private IEnumerator VisitorTypesUpdateAfterRainCoroutine(Wave waveToLookAhead)
        {
            SetActiveAllLookAheadSlot(false);

            yield return new WaitUntil(() => hasFinishedRaining);

            UpdateVisitorTypesLookAheadUISlotsForWave(waveToLookAhead);

            hasFinishedRaining = false;

            yield break;
        }

        private void DisableLookAheadSlotsOnAllWaveSpawned(WaveSpawner waveSpawner, bool allWaveSpawned)
        {
            if (waveSpawner == null) return;

            if (waveSpawnerToLookAhead != waveSpawner) return;

            if (!allWaveSpawned) return;

            SetActiveAllLookAheadSlot(false);
        }

        private void SetActiveAllLookAheadSlot(bool shouldActiveAll)
        {
            if (waveVisitorTypesLookAheadSlotsList == null || waveVisitorTypesLookAheadSlotsList.Count == 0) return;

            for (int i = 0; i < waveVisitorTypesLookAheadSlotsList.Count; i++)
            {
                if (waveVisitorTypesLookAheadSlotsList[i] == null) continue;

                waveVisitorTypesLookAheadSlotsList[i].EnableLookAheadUISlot(shouldActiveAll);
            }
        }

        private bool HasSetActiveLookAheadSlotFromTo(int fromSlotNum, int toSlotNum, bool shouldActive)
        {
            if (fromSlotNum >= waveVisitorTypesLookAheadSlotsList.Count) fromSlotNum = waveVisitorTypesLookAheadSlotsList.Count - 1;

            if (toSlotNum >= waveVisitorTypesLookAheadSlotsList.Count) toSlotNum = waveVisitorTypesLookAheadSlotsList.Count - 1;

            //if slot to active from is of higher order in list than slot to active to -> swap
            if(fromSlotNum > toSlotNum)
            {
                int tempFrom = fromSlotNum;

                fromSlotNum = toSlotNum;

                toSlotNum = tempFrom;
            }

            while(fromSlotNum <= toSlotNum)
            {
                if (waveVisitorTypesLookAheadSlotsList[fromSlotNum] != null) 
                { 
                    waveVisitorTypesLookAheadSlotsList[fromSlotNum].EnableLookAheadUISlot(shouldActive); 
                }

                fromSlotNum++;
            }

            return true;
        }
    }
}
