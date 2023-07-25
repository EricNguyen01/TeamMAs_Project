// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamMAsTD
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "Ability Effect Data Asset/New DoT Effect")]
    public class DamageOverTimeEffectSO : AbilityEffectSO
    {
        [field: SerializeField]
        [field: Min(0.0f)]
        [field: Tooltip("This field only applies to DamageOverTime (DoT) ability effect type!" +
        "If set to 0.0f means the ability will only apply 1 tick of damage on first hit then stops. " +
        "Damage number is gotten from AbilitySO that contains this DoT ability effect SO.")]
        public float damageOverTimeTickSpeed = 0.0f;

        [field: SerializeField]
        [field: Min(0.0f)]
        public float damagePerTick = 0.0f;

        protected override void Awake()
        {
            effectType = EffectType.DoT;
        }

        protected override void OnValidate()
        {
            effectType = EffectType.DoT;
        }
    }
}
