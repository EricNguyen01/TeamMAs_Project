// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace TeamMAsTD
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "Ability Data Asset/New Ability")]
    public class AbilitySO : ScriptableObject, ISerializationCallbackReceiver
    {
        [field: Header("General Ability Data")]
        [field: SerializeField] public string abilityName { get; private set; }

        [field: SerializeField]
        [field: DisallowNull]
        public Ability abilityPrefab { get; private set; }

        [field: SerializeField]
        [field: Min(0)]
        [field: Tooltip("The range that this ability can be casted in tile number. " +
        "If 0 means it can only be casted on self or at self's tile.")]
        protected float initialAbilityRange;

        [field: NonSerialized]
        public float abilityRange { get; private set; }

        [field: SerializeField]
        [field: Min(-1.0f)]
        [field: Tooltip("The duration in which this ability will exist after it has started " +
        "(order like so: charge time (if any) -> ability starts (cdr begins) -> *AbilityDuration* -> ability stops)." +
        "If value is -1.0f meaning that this ability has infinite duration (e.g a toggle ability that only starts/stops on key pressed).")]
        protected float initialAbilityDuration = 0.0f;//in-editor static value

        [field: NonSerialized]
        public float abilityDuration { get; private set; } = 0.0f;//runtime value

        [field: SerializeField]
        [field: Min(0.0f)]
        [field: Tooltip("Cooldown time activates on ability begins performing after charge time.")]
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

        [field: SerializeField]
        [field: Tooltip("On ability cooldown finished and ability has finished updating, " +
        "should the ability restarts itself automatically?")]
        protected bool initialAutoRestartAbility = true;

        [field: NonSerialized]
        public bool autoRestartAbility { get; private set; } = true;

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
        [field: Min(0)]
        [field: Tooltip("The maximum number of units that this ability can affect. " +
        "If set to 0, ability can affect infinite number of units.")]
        protected int initialMaxNumberOfUnitsToAffect;

        [field: NonSerialized]
        public int maxNumberOfUnitsToAffect { get; private set; }

        [field: SerializeField]
        protected List<PlantUnitSO> initialAbilityAffectsSpecificPlantUnit = new List<PlantUnitSO>();

        [field: NonSerialized]
        public List<PlantUnitSO> abilityAffectsSpecificPlantUnit { get; private set; } = new List<PlantUnitSO>();

        [field: SerializeField]
        protected List<VisitorUnitSO> initialAbilityAffectsSpecificVisitorUnit = new List<VisitorUnitSO>();

        [field: NonSerialized]
        public List<VisitorUnitSO> abilityAffectsSpecificVisitorUnit { get; private set; } = new List<VisitorUnitSO>();

        [field: SerializeField]
        protected List<PlantUnitSO> initialSpecificPlantUnitImmuned = new List<PlantUnitSO>();

        [field: NonSerialized]
        public List<PlantUnitSO> specificPlantUnitImmuned { get; private set; } = new List<PlantUnitSO>();

        [field: SerializeField]
        protected List<VisitorUnitSO> initialSpecificVisitorUnitImmuned = new List<VisitorUnitSO>();

        [field: NonSerialized]
        public List<VisitorUnitSO> specificVisitorUnitImmuned { get; private set; } = new List<VisitorUnitSO>();

        [field: SerializeField]
        [field: Tooltip("The effects list that this ability can apply onto its targetted units.")]
        public List<AbilityEffectSO> abilityEffects { get; protected set; } = new List<AbilityEffectSO>();

        [field: SerializeField]
        [field: Tooltip("Wave that this ability will be unlocked and become usable. If null, unlockable will depend on " +
        "initialAbilityLockedOnStart status.")]
        public WaveSO waveToUnlockAbilityAfterFinished { get; private set; }

        [field: SerializeField]
        protected bool initialAbilityLocked = true;//default static in-editor value

        [field: NonSerialized]
        public bool abilityLocked { get; private set; } = true;//runtime non-static value

        [field: SerializeField]
        public bool useOnEquippedIfNotLocked { get; private set; } = false;

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

        public void SetAbilityRangeInTiles(float newRange)
        {
            if(newRange != abilityRange)
            {
                abilityRange = newRange;
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

        public void SetAbilityLockStatus(bool isLocked)
        {
            abilityLocked = isLocked;
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {

        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            abilityRange = initialAbilityRange;

            abilityDuration = initialAbilityDuration;

            abilityCooldownTime = initialAbilityCooldownTime;

            abilityChargeTime = initialAbilityChargeTime;

            abilityLocked = initialAbilityLocked;

            abilityUseReservedFor = initialAbilityUseReservedFor;

            abilityOnlyAffect = initialAbilityOnlyAffect;

            abilityAffectsSpecificVisitorType = initialAbilityAffectsSpecificVisitorType;

            maxNumberOfUnitsToAffect = initialMaxNumberOfUnitsToAffect;

            abilityAffectsSpecificPlantUnit = initialAbilityAffectsSpecificPlantUnit;

            abilityAffectsSpecificVisitorUnit = initialAbilityAffectsSpecificVisitorUnit;

            specificPlantUnitImmuned = initialSpecificPlantUnitImmuned;

            specificVisitorUnitImmuned = initialSpecificVisitorUnitImmuned;
        }
    }
}
