// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamMAsTD
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "Ability Effect Data Asset/New Buff Effect")]
    public class BuffAbilityEffectSO : AbilityEffectSO
    {
        [field: Header("General Buff Ability Data")]
        [field: Header("Note: Amount Percentages Always Override Normal Amount!")]
        [field: Space]

        [field: SerializeField]
        [field: Range(0.0f, 100.0f)]
        public float damageBuffAmountPercentage { get; private set; } = 0.0f;

        [field: SerializeField]
        [field: Min(0.0f)]
        public float damageBuffAmount { get; private set; } = 0.0f;

        [field: SerializeField]
        [field: Range(0.0f, 100.0f)]
        public float healthBuffAmountPercentage { get; private set; } = 0.0f;

        [field: SerializeField]
        [field: Min(0.0f)]
        public float healthBuffAmount { get; private set; } = 0.0f;

        [field: SerializeField]
        [field: Range(0.0f, 100.0f)]
        public float attackSpeedBuffAmountPercentage { get; private set; } = 0.0f;

        [field: SerializeField]
        [field: Min(0.0f)]
        public float attackSpeedBuffAmount { get; private set; } = 0.0f;

        [field: SerializeField]
        [field: Range(0.0f, 100.0f)]
        public float movementSpeedBuffAmountPercentage { get; private set; } = 0.0f;

        [field: SerializeField]
        [field: Min(0.0f)]
        public float movementSpeedBuffAmount { get; private set; } = 0.0f;

        [field: SerializeField]
        [field: Range(0.0f, 100.0f)]
        public float cooldownReductionPercentageBuff { get; private set; } = 0.0f;

        [field: SerializeField]
        [field: Range(0.0f, 100.0f)]
        public float chargeTimeReductionPercentageBuff { get; private set; } = 0.0f;

        [field: Header("Buff Connection Line")]
        [field: SerializeField] public ConnectingLineRenderer buffConnectingLineRendererPrefab { get; private set; }

        protected override void Awake()
        {
            effectType = EffectType.Buff;
        }

        protected override void OnValidate()
        {
            effectType = EffectType.Buff;
        }
    }
}
