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

        [SerializeField] private HorizontalOrVerticalLayoutGroup UIContentToSpawnLookAheadSlotsUnder;

        [SerializeField] [Min(1)] private int lookAheadUISlotsToDisplay = 3;

        private Queue<WaveVisitorTypesLookAheadSlot> waveVisitorTypesLookAheadSlotsQueue = new Queue<WaveVisitorTypesLookAheadSlot>();

        private List<WaveVisitorTypesLookAheadSlot> waveVisitorTypesLookAheadSlotsList = new List<WaveVisitorTypesLookAheadSlot>();


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

        private void CreateVisitorTypesLookAheadUISlotsForWave(Wave wave)
        {
            if (wave == null || wave.waveSO == null || wave.waveSO.visitorTypesToSpawnThisWave == null) return;

            if (waveVisitorTypesLookAheadSlotsList.Count > 0) return;

            for(int i = 0; i < wave.waveSO.visitorTypesToSpawnThisWave.Length; i++)
            {
                GameObject visitorLookAheadSlotGO = Instantiate(visitorTypesLookAheadUISlotPrefab.gameObject, UIContentToSpawnLookAheadSlotsUnder.transform);

                WaveVisitorTypesLookAheadSlot visitorLookAheadSlotScript = visitorLookAheadSlotGO.GetComponent<WaveVisitorTypesLookAheadSlot>();

                if(visitorLookAheadSlotScript == null)
                {
                    Destroy(visitorLookAheadSlotGO);

                    continue;
                }

                visitorLookAheadSlotScript.InitializeVisitorTypeLookAheadSlot(wave.waveSO.visitorTypesToSpawnThisWave[i].visitorType);

                waveVisitorTypesLookAheadSlotsList.Add(visitorLookAheadSlotScript);
            }
        }

        private void UpdateVisitorTypesLookAheadUISlotsForWave(Wave wave)
        {
            if (wave == null || wave.waveSO == null || wave.waveSO.visitorTypesToSpawnThisWave == null) return;

            if (waveVisitorTypesLookAheadSlotsList.Count <= 0)
            {
                CreateVisitorTypesLookAheadUISlotsForWave(wave);

                return;
            }

            int currentCreatedSlots = waveVisitorTypesLookAheadSlotsList.Count;

            int waveTotalVisitors = wave.waveSO.GetTotalVisitorsSpawnNumber();

            if (currentCreatedSlots > waveTotalVisitors)
            {
                HasSetActiveOrInactiveLookAheadSlotFromTo(waveTotalVisitors, currentCreatedSlots, false);
            }
        }

        private void SetActiveOrInactiveAllLookAheadSlot(bool shouldActiveAll)
        {
            if (waveVisitorTypesLookAheadSlotsList == null || waveVisitorTypesLookAheadSlotsList.Count == 0) return;

            for (int i = 0; i < waveVisitorTypesLookAheadSlotsList.Count; i++)
            {
                if (waveVisitorTypesLookAheadSlotsList[i] == null) continue;

                waveVisitorTypesLookAheadSlotsList[i].DisplayLookAheadUISlot(shouldActiveAll);
            }
        }

        private bool HasSetActiveOrInactiveLookAheadSlotFromTo(int fromSlotNum, int toSlotNum, bool shouldActive)
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
                    waveVisitorTypesLookAheadSlotsList[fromSlotNum].DisplayLookAheadUISlot(shouldActive); 
                }

                fromSlotNum++;
            }

            return true;
        }

        private void StartWaveVisitorTypesLookAhead()
        {

        }

        private void EndWaveVisitorTypesLookAhead()
        {

        }
    }
}
