// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

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

        protected bool unitWithThisEffectIsBeingUprooted = false;

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

            ProcessEffectPopupForBuffEffects(null, "+" + finalDamageBuffedAmount + " AppeasementSTRENGTH", finalDamageBuffedAmount);

            plantUnitSOReceivedBuff.AddPlantAttackSpeed(finalAtkSpeedBuffedAmount);
            
            ProcessEffectPopupForBuffEffects(null, "+" + finalAtkSpeedBuffedAmount + " AppeasementSPEED", finalAtkSpeedBuffedAmount);

            if (plantUnitReceivedBuff == null) plantUnitReceivedBuff = (PlantUnit)unitBeingAffected.GetUnitObject();

            if (plantUnitReceivedBuff.tilePlacedOn != null)
            {
                plantUnitReceivedBuff.tilePlacedOn.OnPlantUnitUprootedOnTile.AddListener(SubToPlantUnitBeingUprootedOnTileEvent);
            }

            plantUnitReceivedBuff.SetPlantSODebugDataView();
        }

        protected override void EffectUpdate()
        {
            //should do nothing here! Nothing to update.
        }

        protected override void OnEffectEnded()
        {
            if (plantUnitSOReceivedBuff == null) return;

            if (buffAbilityEffectSO == null) return;

            //if this effect is being destroyed because the plant unit being affected by it is being destroyed through being uprooted
            //no need to do anything here and return
            if (unitWithThisEffectIsBeingUprooted)
            {
                if (plantUnitReceivedBuff.tilePlacedOn != null)
                {
                    plantUnitReceivedBuff.tilePlacedOn.OnPlantUnitUprootedOnTile.RemoveListener(SubToPlantUnitBeingUprootedOnTileEvent);
                }

                return;
            }

            plantUnitSOReceivedBuff.RemovePlantUnitDamage(finalDamageBuffedAmount);

            ProcessEffectPopupForBuffEffects(null, "-" + finalDamageBuffedAmount + " AppeasementSTRENGTH", -finalDamageBuffedAmount, 0.8f);

            plantUnitSOReceivedBuff.RemovePlantAttackSpeed(finalAtkSpeedBuffedAmount);

            ProcessEffectPopupForBuffEffects(null, "-" + finalAtkSpeedBuffedAmount + " AppeasementSPEED", -finalAtkSpeedBuffedAmount, 0.8f);

            DetachAndDestroyAllEffectPopupsIncludingSpawner();

            plantUnitReceivedBuff.SetPlantSODebugDataView();
        }

        protected void SubToPlantUnitBeingUprootedOnTileEvent(PlantUnit plantUnit, Tile tile)
        {
            if(plantUnitReceivedBuff != null && plantUnit == plantUnitReceivedBuff)
            {
                unitWithThisEffectIsBeingUprooted = true;
            }
        }
    }
}
