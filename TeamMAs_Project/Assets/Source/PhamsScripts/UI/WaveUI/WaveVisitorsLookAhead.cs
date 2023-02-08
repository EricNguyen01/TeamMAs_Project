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

        private Wave currentWaveToLookAhead;

        private Queue<WaveVisitorTypesLookAheadSlot> waveVisitorTypesLookAheadSlotsQueue = new Queue<WaveVisitorTypesLookAheadSlot>();

        private Vector2 foremostPositionInLookAheadQueue;

        private Vector2 furthestPositionInLookAheadQueue;

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

        private void CreateVisitorTypesLookAheadUISlots()
        {
            
        }

        private void UpdateVisitorTypesLookAheadUISlots()
        {

        }

        private void StartWaveVisitorTypesLookAhead()
        {

        }

        private void EndWaveVisitorTypesLookAhead()
        {

        }
    }
}
