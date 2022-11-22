using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamMAsTD
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "Wave Data Asset/New Wave")]
    public class WaveSO : ScriptableObject
    {
        [System.Serializable]
        public struct VisitorTypeStruct
        {
            public VisitorSO visitorType;
            public int spawnNumbers;
            public int spawnChance;
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
    }
}
