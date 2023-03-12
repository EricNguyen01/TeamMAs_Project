using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamMAsTD
{
    [DisallowMultipleComponent]
    public class PlantBuffAbilityEffect : AbilityEffect
    {
        protected PlantUnitSO plantUnitSOReceivedBuff { get; private set; }

        protected BuffAbilityEffectSO buffAbilityEffectSO { get; private set; }

        protected PlantUnit plantUnitReceivedBuff { get; private set; }

        protected float finalDamageBuffedAmount = 0.0f;

        protected float finalAtkSpeedBuffedAmount = 0.0f;

        protected override void Awake()
        {
            base.Awake();

            if (abilityEffectSO == null) return;

            if (abilityEffectSO.GetType() != typeof(BuffAbilityEffectSO) || abilityEffectSO.effectType != AbilityEffectSO.EffectType.Buff)
            {
                Debug.LogError("PlantBuffAbilityEffect script on : " + name + " has unmatched AbilityEffectSO ability type." +
                "Ability effect won't work and will be destroyed!");

                DestroyEffectWithEffectEndedInvoked(false);

                return;
            }

            buffAbilityEffectSO = (BuffAbilityEffectSO)abilityEffectSO;
        }
        protected override void OnEffectStarted()
        {
            if (unitBeingAffected == null) return;

            if (unitBeingAffectedUnitSO == null)
            {
                Debug.LogError("The unit: " + name + " being affected by this buff effect: " + name + " doesn't have a UnitSO data. " +
                "Destroying buff effect!");

                DestroyEffectWithEffectEndedInvoked(false);

                return;
            }

            if(unitBeingAffectedUnitSO.GetType() != typeof(PlantUnitSO))
            {
                Debug.LogError("The unit: " + name + " being affected by this buff effect: " + name + " " +
                "has UnitSO data that IS NOT of type PlantUnitSO. Buff won't work!\n" +
                "Destroying buff effect!");

                DestroyEffectWithEffectEndedInvoked(false);

                return;
            }

            if(plantUnitSOReceivedBuff == null) plantUnitSOReceivedBuff = (PlantUnitSO)unitBeingAffectedUnitSO;

            finalDamageBuffedAmount = buffAbilityEffectSO.damageBuffAmount;

            finalAtkSpeedBuffedAmount = buffAbilityEffectSO.attackSpeedBuffAmount;

            if(buffAbilityEffectSO.damageBuffAmountPercentage > 0.0f)
            {
                float damage = plantUnitSOReceivedBuff.damage;

                finalDamageBuffedAmount = damage *= buffAbilityEffectSO.damageBuffAmountPercentage / 100.0f;
            }

            if(buffAbilityEffectSO.attackSpeedBuffAmountPercentage > 0.0f)
            {
                float atkSpd = plantUnitSOReceivedBuff.attackSpeed;

                finalAtkSpeedBuffedAmount = atkSpd *= buffAbilityEffectSO.attackSpeedBuffAmountPercentage / 100.0f;
            }

            plantUnitSOReceivedBuff.AddPlantUnitDamage(finalDamageBuffedAmount);

            plantUnitSOReceivedBuff.AddPlantAttackSpeed(finalAtkSpeedBuffedAmount);

            if(plantUnitReceivedBuff == null) plantUnitReceivedBuff = (PlantUnit)unitBeingAffected.GetUnitObject();

            plantUnitReceivedBuff.SetPlantSODebugDataView();
        }

        protected override void OnEffectUpdated()
        {
            
        }

        protected override void OnEffectEnded()
        {
            if (plantUnitSOReceivedBuff == null) return;

            if (buffAbilityEffectSO == null) return;

            plantUnitSOReceivedBuff.RemovePlantUnitDamage(finalDamageBuffedAmount);

            plantUnitSOReceivedBuff.RemovePlantAttackSpeed(finalAtkSpeedBuffedAmount);

            plantUnitReceivedBuff.SetPlantSODebugDataView();
        }
    }
}
