using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamMAsTD
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "Ability Effect Data Asset/New DeBuff Effect")]
    public class DeBuffAbilityEffectSO : AbilityEffectSO
    {
        [field: Header("General DeBuff Ability Data")]
        [field: Header("Note: Amount Percentages Always Override Normal Amount!")]

        [field: SerializeField]
        [field: Range(0.0f, 100.0f)]
        public float damageDeBuffAmountPercentage { get; private set; } = 0.0f;

        [field: SerializeField]
        [field: Min(0.0f)]
        public float damageDeBuffAmount { get; private set; } = 0.0f;

        [field: SerializeField]
        [field: Range(0.0f, 100.0f)]
        public float healthDeBuffAmountPercentage { get; private set; } = 0.0f;

        [field: SerializeField]
        [field: Min(0.0f)]
        public float healthDeBuffAmount { get; private set; } = 0.0f;

        [field: SerializeField]
        [field: Range(0.0f, 100.0f)]
        public float attackSpeedDeBuffAmountPercentage { get; private set; } = 0.0f;

        [field: SerializeField]
        [field: Min(0.0f)]
        public float attackSpeedDeBuffAmount { get; private set; } = 0.0f;

        [field: SerializeField]
        [field: Range(0.0f, 100.0f)]
        public float movementSpeedDeBuffAmountPercentage { get; private set; } = 0.0f;

        [field: SerializeField]
        [field: Min(0.0f)]
        public float movementSpeedDeBuffAmount { get; private set; } = 0.0f;

        [field: SerializeField]
        [field: Range(0.0f, 100.0f)]
        public float cooldownReductionPercentageDeBuff { get; private set; } = 0.0f;

        [field: SerializeField]
        [field: Range(0.0f, 100.0f)]
        public float chargeTimeReductionPercentageDeBuff { get; private set; } = 0.0f;

        protected override void Awake()
        {
            effectType = EffectType.Debuff;
        }

        protected override void OnValidate()
        {
            effectType = EffectType.Debuff;
        }
    }
}
