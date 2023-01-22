using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamMAsTD
{
    /*
     * WaveSO stores data for a single wave
     */
    [System.Serializable]
    [CreateAssetMenu(menuName = "Wave Data Asset/New Wave")]
    public class WaveSO : ScriptableObject
    {
        [System.Serializable]
        public struct VisitorTypeStruct
        {
            public VisitorUnitSO visitorType;
            [Min(1)] public int spawnNumbers;
            [Range(1, 50)] public int spawnChance;
        }

        [field: Header("This Wave Data")]

        [field: SerializeField]
        [field: Tooltip("Add the type of visitor that you want to spawn in this wave. Press the + button below the array to add new visitors.")]
        public VisitorTypeStruct[] visitorTypesToSpawnThisWave { get; private set; }

        [field: Header("This Wave Spawn Wait Time Settings")]
        [field: SerializeField][field: Min(0.0f)] public float waitTimeToFirstSpawn { get; private set; } = 1.0f;
        [field: SerializeField][field: Min(0.0f)] public float waitTimeBetweenSpawn { get; private set; } = 1.0f;

        [field: SerializeField]
        [field: Tooltip("If there is a visitor type that is a boss in this wave, will it be spawned last or within the wave?")]
        public bool spawnBossLast { get; private set; } = true;

        [field: Header("This Wave Visitor Health Scaling Data")]
        [field: SerializeField] public bool applyVisitorHealthScalingThisWave { get; private set; } = true;
        [field: SerializeField][field: Min(1.0f)] public float visitorHealthScalingMultiplier { get; private set; } = 1.0f;

        public int GetSpawnNumberOfVisitorType(VisitorUnitSO visitorSO)
        {
            if (visitorSO == null) return 0;

            if(visitorTypesToSpawnThisWave == null || visitorTypesToSpawnThisWave.Length == 0) return 0;

            int visitorNum = 0;

            for(int i = 0; i < visitorTypesToSpawnThisWave.Length; i++)
            {
                if (visitorTypesToSpawnThisWave[i].visitorType == visitorSO)
                {
                    visitorNum += visitorTypesToSpawnThisWave[i].spawnNumbers;
                }
            }

            return visitorNum;
        }

        public int GetSpawnChanceOfVisitorType(VisitorUnitSO visitorSO)
        {
            if (visitorSO == null) return 0;

            if (visitorTypesToSpawnThisWave == null || visitorTypesToSpawnThisWave.Length == 0) return 0;

            int visitorSpawnChance = 0;

            for (int i = 0; i < visitorTypesToSpawnThisWave.Length; i++)
            {
                if (visitorTypesToSpawnThisWave[i].visitorType == visitorSO)
                {
                    visitorSpawnChance += visitorTypesToSpawnThisWave[i].spawnChance;
                }
            }

            return visitorSpawnChance;
        }
    }
}
