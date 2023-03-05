using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamMAsTD
{
    public class AbilitySO : ScriptableObject, ISerializationCallbackReceiver
    {
        [field: Header("Ability Data")]
        [field: SerializeField] public string abilityName { get; private set; }

        [field: SerializeField] 
        [field: Min(0.0f)]
        [field: Tooltip("The range that this ability can be casted in tile number. " +
        "If 0.0f means it can only be casted on self or at self's tile.")]
        public float abilityRangeInTiles { get; private set; }

        [field: SerializeField]
        [field: Min(0.0f)]
        public float abilityCooldownTime { get; private set; }

        [field: SerializeField]
        [field: Min(0.0f)]
        public float abilityChargeTime { get; private set; }

        public enum AbilityUseReservedFor { All, PlantOnly, VisitorOnly }

        [field: SerializeField] 
        public AbilityUseReservedFor abilityUseReservedFor { get; private set; } = AbilityUseReservedFor.All;

        public enum AbilityOnlyAffect { All, PlantOnly, VisitorOnly }

        [field: SerializeField]
        public AbilityOnlyAffect abilityOnlyAffect { get; private set; } = AbilityOnlyAffect.All;

        [field: SerializeField]
        public VisitorUnitSO.VisitorType abilityAffectsSpecificVisitorType { get; private set; } = VisitorUnitSO.VisitorType.None;

        [field: SerializeField]
        public List<PlantUnitSO> abilityAffectsSpecificPlantUnit { get; private set; } = new List<PlantUnitSO>();

        [field: SerializeField]
        public List<VisitorUnitSO> abilityAffectsSpecificVisitorUnit { get; private set; } = new List<VisitorUnitSO>();

        [field: SerializeField]
        public bool initialAbilityLockedOnStart = true;//default static in-editor value

        [field: NonSerialized]
        public bool abilityLockedOnStart { get; private set; }//runtime non-static value



        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {

        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            abilityLockedOnStart = initialAbilityLockedOnStart;
        }
    }
}
