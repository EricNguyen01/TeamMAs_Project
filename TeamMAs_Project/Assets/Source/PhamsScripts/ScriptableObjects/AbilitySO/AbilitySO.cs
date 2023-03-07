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
        protected int initialAbilityRangeInTiles;

        [field: NonSerialized]
        public int abilityRangeInTiles { get; private set; }

        [field: SerializeField]
        [field: Min(-1.0f)]
        [field: Tooltip("The duration after this ability is casted (order like so: charge time -> ability start -> abilityDuration -> cdr)." +
        "If value is -1.0f meaning that this ability has infinite duration (e.g a forever lasting aura ability).")]
        protected float initialAbilityDuration = 0.0f;//in-editor static value

        [field: NonSerialized]
        public float abilityDuration { get; private set; } = 0.0f;//runtime value

        [field: SerializeField]
        [field: Min(0.0f)]
        [field: Tooltip("Cooldown time activates right after abilityDuration and after charge time.")]
        protected float initialAbilityCooldownTime = 0.0f;//in-editor static value

        [field: NonSerialized]
        public float abilityCooldownTime { get; private set; } = 0.0f;//runtime value

        [field: SerializeField]
        [field: Min(0.0f)]
        [field: Tooltip("On ability used, if there's a charge time, the ability will wait out the charge duration before" +
        "the actual ability is performed.")]
        protected float initialAbilityChargeTime = 0.0f;//in-editor static value

        [field: NonSerialized]
        public float abilityChargeTime { get; private set; } = 0.0f;//runtime value

        public enum AbilityUseReservedFor { All, PlantOnly, VisitorOnly }

        [field: SerializeField] 
        protected AbilityUseReservedFor initialAbilityUseReservedFor = AbilityUseReservedFor.All;

        [field: NonSerialized]
        public AbilityUseReservedFor abilityUseReservedFor { get; protected set; } = AbilityUseReservedFor.All;

        public enum AbilityOnlyAffect { All, PlantOnly, VisitorOnly }

        [field: SerializeField]
        protected AbilityOnlyAffect initialAbilityOnlyAffect = AbilityOnlyAffect.All;

        [field: NonSerialized]
        public AbilityOnlyAffect abilityOnlyAffect { get; protected set; } = AbilityOnlyAffect.All;

        [field: SerializeField]
        protected VisitorUnitSO.VisitorType initialAbilityAffectsSpecificVisitorType = VisitorUnitSO.VisitorType.None;

        [field: NonSerialized]
        public VisitorUnitSO.VisitorType abilityAffectsSpecificVisitorType { get; protected set; } = VisitorUnitSO.VisitorType.None;

        [field: SerializeField]
        protected List<PlantUnitSO> initialAbilityAffectsSpecificPlantUnit = new List<PlantUnitSO>();

        [field: NonSerialized]
        public List<PlantUnitSO> abilityAffectsSpecificPlantUnit { get; private set; } = new List<PlantUnitSO>();

        [field: SerializeField]
        protected List<VisitorUnitSO> initialAbilityAffectsSpecificVisitorUnit = new List<VisitorUnitSO>();

        [field: NonSerialized]
        public List<VisitorUnitSO> abilityAffectsSpecificVisitorUnit { get; private set; } = new List<VisitorUnitSO>();

        [field: SerializeField]
        [field: Tooltip("The effects list that this ability can apply onto its targetted units.")]
        public List<AbilityEffectSO> abilityEffects { get; private set; } = new List<AbilityEffectSO>();

        [field: SerializeField]
        [field: Tooltip("Wave that this ability will be unlocked and become usable. If null, unlockable will depend on " +
        "initialAbilityLockedOnStart status.")]
        public WaveSO waveToUnlockAbility { get; private set; }

        [field: SerializeField]
        protected bool initialAbilityLocked = true;//default static in-editor value

        [field: NonSerialized]
        public bool abilityLocked { get; private set; } = true;//runtime non-static value

        protected abstract void Awake();

        protected abstract void OnValidate();

        public void SetNewAbilityTimeConfigs(float newAbilityDuration = 0.0f, float newAbilityCdrTime = 0.0f, float newAbilityChargeTime = 0.0f)
        {
            if(newAbilityDuration != 0.0f)
            {
                abilityDuration = newAbilityDuration;
            }

            if(newAbilityCdrTime > 0.0f)
            {
                abilityCooldownTime = newAbilityCdrTime;
            }

            if(newAbilityChargeTime > 0.0f)
            {
                abilityChargeTime = newAbilityChargeTime;
            }
        }

        public void SetAbilityLocked(bool isLocked)
        {
            if(isLocked != abilityLocked)
            {
                abilityLocked = isLocked;
            }
        }

        public void SetAbilityRangeInTiles(int newTileRange)
        {
            if(newTileRange != abilityRangeInTiles)
            {
                abilityRangeInTiles = newTileRange;
            }
        }

        public void SetAbilityUseReservedForStatus(AbilityUseReservedFor newAbilityReservedFor)
        {
            if(newAbilityReservedFor != abilityUseReservedFor)
            {
                abilityUseReservedFor = newAbilityReservedFor;
            }
        }

        public void SetAbilityOnlyAffects(AbilityOnlyAffect newAbilityOnlyAffects)
        {
            if(newAbilityOnlyAffects != abilityOnlyAffect)
            {
                abilityOnlyAffect = newAbilityOnlyAffects;
            }
        }

        public void SetAbilityAffectsSpecificVisitorUnitType(VisitorUnitSO.VisitorType visitorType)
        {
            if(visitorType != abilityAffectsSpecificVisitorType)
            {
                abilityAffectsSpecificVisitorType = visitorType;
            }
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {

        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            abilityRangeInTiles = initialAbilityRangeInTiles;

            abilityDuration = initialAbilityDuration;

            abilityCooldownTime = initialAbilityCooldownTime;

            abilityChargeTime = initialAbilityChargeTime;

            abilityLocked = initialAbilityLocked;

            abilityUseReservedFor = initialAbilityUseReservedFor;

            abilityOnlyAffect = initialAbilityOnlyAffect;

            abilityAffectsSpecificVisitorType = initialAbilityAffectsSpecificVisitorType;

            abilityAffectsSpecificPlantUnit = initialAbilityAffectsSpecificPlantUnit;

            abilityAffectsSpecificVisitorUnit = initialAbilityAffectsSpecificVisitorUnit;
        }
    }
}
