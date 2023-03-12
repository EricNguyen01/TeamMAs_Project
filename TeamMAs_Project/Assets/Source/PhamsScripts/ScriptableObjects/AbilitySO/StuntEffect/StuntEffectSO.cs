using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamMAsTD
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "Ability Effect Data Asset/New Stunt Effect")]
    public class StuntEffectSO : AbilityEffectSO
    {
        protected override void Awake()
        {
            effectType = EffectType.Stunt;
        }

        protected override void OnValidate()
        {
            effectType = EffectType.Stunt;
        }
    }
}
