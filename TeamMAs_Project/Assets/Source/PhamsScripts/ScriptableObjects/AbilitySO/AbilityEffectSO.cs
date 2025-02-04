// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace TeamMAsTD
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "Ability Effect Data Asset/New Default Effect")]
    public class AbilityEffectSO : ScriptableObject
    {
        [field: Header("Ability Effect Data")]

        [field: SerializeField]
        public string abilityEffectName { get; private set; }

        [field: SerializeField]
        [field: DisallowNull]
        public AbilityEffect abilityEffectPrefab { get; private set; }

        [field: SerializeField]
        [field: Min(-1.0f)]
        [field: Tooltip("The duration in which this effect will last. " +
        "If set to -1 means that this effect will last infinitely (e.g a bleed effect that lasts until target is ded).")]
        public float effectDuration { get; private set; } = 0.0f;

        [field: SerializeField]
        [field: Tooltip("Set effect last duration to be the same as the duration of the ability that produces this effect?")]
        public bool effectDurationAsAbilityDuration { get; private set; } = true;

        [field: SerializeField]
        [field: Tooltip("Should this effect be cancelled when leaving its ability's aoe?")]
        public bool effectEndsWhenLeavingAbilityArea { get; private set; } = true;

        [field: SerializeField]
        [field: Tooltip("Can multiple instances of this ability effect be applied on the same unit?")]
        public bool effectStackable { get; private set; } = false;

        public enum EffectType { Default, Stunt, DoT, Buff, Debuff  }

        [field: SerializeField]
        public EffectType effectType { get; protected set; } = EffectType.Default;

        //public static event System.Action<IUnit, UnitSO> OnAbilityEffectModifiedUnitSO;

        protected virtual void OnValidate()
        {

        }

        protected virtual void Awake()
        {

        }
    }
}
