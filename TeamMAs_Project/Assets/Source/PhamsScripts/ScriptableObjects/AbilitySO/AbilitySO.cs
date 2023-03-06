using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamMAsTD
{
    public abstract class AbilitySO : ScriptableObject, ISerializationCallbackReceiver
    {
        [field: Header("General Ability Data")]
        [field: SerializeField] public string abilityName { get; private set; }

        [field: SerializeField] public Ability abilityPrefab { get; private set; }

        [field: SerializeField] 
        [field: Min(0)]
        [field: Tooltip("The range that this ability can be casted in tile number. " +
        "If 0 means it can only be casted on self or at self's tile.")]
        public int abilityRangeInTiles { get; private set; }

        [field: SerializeField]
        [field: Min(0.0f)]
        private float initialAbilityCooldownTime = 0.0f;//in-editor static value

        [field: NonSerialized]
        public float abilityCooldownTime { get; private set; } = 0.0f;//runtime value

        [field: SerializeField]
        [field: Min(0.0f)]
        private float initialAbilityChargeTime = 0.0f;//in-editor static value

        [field: NonSerialized]
        public float abilityChargeTime { get; private set; } = 0.0f;//runtime value

        public enum AbilityUseReservedFor { All, PlantOnly, VisitorOnly }

        [field: SerializeField] 
        public AbilityUseReservedFor abilityUseReservedFor { get; protected set; } = AbilityUseReservedFor.All;

        public enum AbilityOnlyAffect { All, PlantOnly, VisitorOnly }

        [field: SerializeField]
        public AbilityOnlyAffect abilityOnlyAffect { get; protected set; } = AbilityOnlyAffect.All;

        [field: SerializeField]
        public VisitorUnitSO.VisitorType abilityAffectsSpecificVisitorType { get; protected set; } = VisitorUnitSO.VisitorType.None;

        [field: SerializeField]
        public List<PlantUnitSO> abilityAffectsSpecificPlantUnit { get; private set; } = new List<PlantUnitSO>();

        [field: SerializeField]
        public List<VisitorUnitSO> abilityAffectsSpecificVisitorUnit { get; private set; } = new List<VisitorUnitSO>();

        [field: SerializeField]
        [field: Tooltip("The effects list that this ability can apply onto its targetted units.")]
        public List<AbilityEffectSO> abilityEffects { get; private set; } = new List<AbilityEffectSO>();

        [field: SerializeField]
        [field: Tooltip("Wave that this ability will be unlocked and become usable. If null, unlockable will depend on " +
        "initialAbilityLockedOnStart status.")]
        public WaveSO waveToUnlockAbility { get; private set; }

        [field: SerializeField]
        private bool initialAbilityLockedOnStart = true;//default static in-editor value

        [field: NonSerialized]
        public bool abilityLockedOnStart { get; private set; }//runtime non-static value

        protected abstract void Awake();

        protected abstract void OnValidate();

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {

        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            abilityCooldownTime = initialAbilityCooldownTime;

            abilityChargeTime = initialAbilityChargeTime;

            abilityLockedOnStart = initialAbilityLockedOnStart;
        }
    }
}
