using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamMAsTD
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "Ability Data Asset/DeBuff Ability/General DeBuff Ability")]
    public class GeneralDebuffAbilitySO : AbilitySO
    {
        [field: Header("General DeBuff Ability Data")]

        [field: SerializeField]
        [field: Min(0.0f)]
        [field: Tooltip("If debuff duration is set to 0.0f, debuff lasts infinitely.")]
        public float deBuffDuration { get; private set; } = 0.0f;

        [field: SerializeField]
        [field: Min(0.0f)]
        public float damageDeBuffAmount { get; private set; } = 0.0f;

        [field: SerializeField]
        [field: Min(0.0f)]
        public float healthDeBuffAmount { get; private set; } = 0.0f;

        [field: SerializeField]
        [field: Min(0.0f)]
        public float attackSpeedDeBuffAmount { get; private set; } = 0.0f;

        [field: SerializeField]
        [field: Range(0.0f, 1.0f)]
        public float cooldownReductionPercentageDeBuff { get; private set; } = 0.0f;

        [field: SerializeField]
        [field: Range(0.0f, 1.0f)]
        public float chargeTimeReductionPercentageDeBuff { get; private set; } = 0.0f;

        protected override void Awake()
        {
            
        }

        protected override void OnValidate()
        {
            
        }
    }
}
