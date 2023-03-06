using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamMAsTD
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "Ability Data Asset/Buff Ability/Plant Buff Ability")]
    public class PlantBuffAbilitySO : GeneralBuffAbilitySO
    {
        protected override void Awake()
        {
            base.Awake();

            abilityUseReservedFor = AbilityUseReservedFor.PlantOnly;

            abilityOnlyAffect = AbilityOnlyAffect.PlantOnly;
        }

        protected override void OnValidate()
        {
            base.OnValidate();

            abilityUseReservedFor = AbilityUseReservedFor.PlantOnly;

            abilityOnlyAffect = AbilityOnlyAffect.PlantOnly;
        }
    }
}
