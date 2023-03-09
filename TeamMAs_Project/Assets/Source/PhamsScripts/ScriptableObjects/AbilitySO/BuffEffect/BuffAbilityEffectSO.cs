using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.CanvasScaler;

namespace TeamMAsTD
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "Ability Effect Data Asset/New Buff Effect")]
    public class BuffAbilityEffectSO : AbilityEffectSO
    {
        [field: Header("General Buff Ability Data")]

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
        [field: Min(0.0f)]
        public float movementSpeedBuffAmount { get; private set; } = 0.0f;

        [field: SerializeField]
        [field: Range(0.0f, 1.0f)]
        public float cooldownReductionPercentageBuff { get; private set; } = 0.0f;

        [field: SerializeField]
        [field: Range(0.0f, 1.0f)]
        public float chargeTimeReductionPercentageBuff { get; private set; } = 0.0f;

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
