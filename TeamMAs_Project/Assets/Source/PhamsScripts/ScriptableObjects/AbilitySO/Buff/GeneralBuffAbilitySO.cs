using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamMAsTD
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "Ability Data Asset/Buff Ability/General Buff Ability")]
    public class GeneralBuffAbilitySO : AbilitySO
    {
        [field: Header("General Buff Ability Data")]

        [field: SerializeField]
        [field: Min(0.0f)]
        [field: Tooltip("If buff duration is set to 0.0f, buff lasts infinitely.")]
        public float buffDuration { get; private set; } = 0.0f;

        [field: SerializeField]
        [field: Min(0.0f)]
        public float damageBuffAmount { get; private set; } = 0.0f;

        [field: SerializeField]
        [field: Min(0.0f)]
        public float healthBuffAmount { get; private set; } = 0.0f;

        [field: SerializeField]
        [field: Min(0.0f)]
        public float attackSpeedBuffAmount { get; private set; } = 0.0f;

        [field: SerializeField]
        [field: Range(0.0f, 1.0f)]
        public float cooldownReductionPercentageBuff { get; private set; } = 0.0f;

        [field: SerializeField]
        [field: Range(0.0f, 1.0f)]
        public float chargeTimeReductionPercentageBuff { get; private set; } = 0.0f;

        protected override void Awake()
        {
            
        }

        protected override void OnValidate()
        {
            
        }
    }
}
