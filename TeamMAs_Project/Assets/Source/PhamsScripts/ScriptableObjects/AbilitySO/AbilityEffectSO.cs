using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamMAsTD
{
    [System.Serializable]
    public abstract class AbilityEffectSO : ScriptableObject
    {
        [field: Header("Ability Effect Data")]

        [field: SerializeField]
        public string abilityEffectName { get; private set; }

        [field: SerializeField]
        [field: Min(-1.0f)]
        [field: Tooltip("The duration in which this effect will last. If set to -1 means that this effect will last infinitely.")]
        public float effectDuration = 0.0f;

        [field: SerializeField]
        [field: Tooltip("Set effect last duration to be the same as the duration of the ability that produces this effect?")]
        public bool effectDurationAsAbilityDuration { get; private set; } = true;

        [field: SerializeField]
        [field: Min(0)]
        [field: Tooltip("The area of effect range in tile number of this ability effect. " +
        "If 0 means the AOE range is EXACTLY on the tile where this effect is casted on or landed.")]
        public int effectAreaInTiles = 0;

        [field: SerializeField]
        [field: Tooltip("Set effect area in tile number the same as the ability range that produces this effect?")]
        public bool effectRangeAsAbilityRange { get; private set; } = true;

        [field: SerializeField]
        [field: Min(0)]
        [field: Tooltip("The number of units this ability effect can affect. " +
        "0 means infinite number of units that this effect can apply onto.")]
        public int maxUnitsToApplyEffect = 0;

        public enum EffectType { Default, Stunt, Slowed, DoT, KnockedBack, KnockedUp, Buff, Debuff  }

        [field: SerializeField]
        public EffectType effectType { get; protected set; } = EffectType.Default;

        protected abstract void OnValidate();

        protected abstract void Awake();
    }
}
