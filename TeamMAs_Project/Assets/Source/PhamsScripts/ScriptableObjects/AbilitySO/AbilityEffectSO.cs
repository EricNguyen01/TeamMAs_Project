using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamMAsTD
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "Ability Data Asset/New Ability Effect")]
    public class AbilityEffectSO : ScriptableObject
    {
        [field: Header("Ability Effect Data")]

        [field: SerializeField]
        [field: Min(0.0f)]
        public float effectDuration = 0.0f;

        [field: SerializeField]
        [field: Min(0.0f)]
        [field: Tooltip("This field only applies to DamageOverTime (DoT) ability effect type!" +
        "If set to 0.0f means the ability will only apply 1 tick of damage on first hit then stops. " +
        "Damage number is gotten from AbilitySO that contains this DoT ability effect SO.")]
        public float damageOverTimeSpeed = 0.0f;

        [field: SerializeField]
        [field: Min(0)]
        [field: Tooltip("The area of effect range in tile number of this ability effect. " +
        "If 0 means the AOE range is EXACTLY on the tile where this effect is casted on or landed.")]
        public int effectAreaInTiles = 0;

        [field: SerializeField]
        [field: Min(0)]
        [field: Tooltip("The number of units this ability effect can affect. " +
        "0 means infinite number of units that this effect can apply onto.")]
        public int maxUnitsToApplyEffect = 0;

        public enum EffectType { Stunt, Slowed, DoT, KnockedBack, KnockedUp }
    }
}
