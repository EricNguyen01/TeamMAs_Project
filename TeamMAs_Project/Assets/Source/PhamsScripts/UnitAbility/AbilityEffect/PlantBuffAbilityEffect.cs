// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using UnityEngine;

namespace TeamMAsTD
{
    [DisallowMultipleComponent]
    public class PlantBuffAbilityEffect : BuffDebuffAbilityEffect
    {
        protected PlantUnitSO plantUnitSOReceivedBuff { get; private set; }

        protected PlantUnit plantUnitReceivedBuff { get; private set; }

        protected float finalDamageBuffedAmount = 0.0f;

        protected float finalAtkSpeedBuffedAmount = 0.0f;

        protected override void Awake()
        {
            base.Awake();

            if (!abilityEffectSO)
            {
                enabled = false;

                return;
            }

            if (!buffAbilityEffectSO)
            {
                enabled = false;

                DestroyEffectWithEffectEndedInvoked(false);

                return;
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (plantUnitReceivedBuff != null && plantUnitReceivedBuff.tilePlacedOn != null)
            {
                plantUnitReceivedBuff.tilePlacedOn.OnPlantUnitUprootedOnTile.RemoveListener(SubToPlantUnitBeingUprootedOnTileEvent);

                plantUnitReceivedBuff.tilePlacedOn.tileMenuAndUprootOnTileUI.OnTileMenuOpened.RemoveListener(ActivateBuffConnectingLine);

                plantUnitReceivedBuff.tilePlacedOn.tileMenuAndUprootOnTileUI.OnTileMenuClosed.RemoveListener(DeactivateBuffConnectingLine);
            }
        }

        protected override bool OnEffectStarted()
        {
            if (!base.OnEffectStarted()) return false;

            if (unitBeingAffectedUnitSO.GetType() != typeof(PlantUnitSO))
            {
                Debug.LogError("The unit: " + name + " being affected by this buff effect: " + name + " " +
                "has UnitSO data that IS NOT of type PlantUnitSO. Buff won't work!\n" +
                "Destroying buff effect!");

                DestroyEffectWithEffectEndedInvoked(false);

                return false;
            }

            if (plantUnitSOReceivedBuff == null) plantUnitSOReceivedBuff = (PlantUnitSO)unitBeingAffectedUnitSO;

            ProcessPlantBuffs();

            if (plantUnitReceivedBuff == null) plantUnitReceivedBuff = (PlantUnit)unitBeingAffected.GetUnitObject();

            if (plantUnitReceivedBuff != null && plantUnitReceivedBuff.tilePlacedOn != null)
            {
                plantUnitReceivedBuff.tilePlacedOn.OnPlantUnitUprootedOnTile.AddListener(SubToPlantUnitBeingUprootedOnTileEvent);

                plantUnitReceivedBuff.tilePlacedOn.tileMenuAndUprootOnTileUI.OnTileMenuOpened.AddListener(ActivateBuffConnectingLine);

                plantUnitReceivedBuff.tilePlacedOn.tileMenuAndUprootOnTileUI.OnTileMenuClosed.AddListener(DeactivateBuffConnectingLine);
            }

            //draw a line moving from plant that is providing buff to the target that this buff effect is on 
            //using ConnectingLineRenderer prefab if provided
            if (!disableLineOnBuffStarted && buffConnectingLineRenderer && plantUnitReceivedBuff)
            {
                Vector3 buffSourcePos = sourceUnitProducedEffect.GetUnitTransform().position;

                buffConnectingLineRenderer.ActivateLineWithSequence(buffSourcePos, plantUnitReceivedBuff.transform.position, true);
            }

            plantUnitReceivedBuff.SetPlantSODebugDataView();

            return true;
        }

        private void ProcessPlantBuffs()
        {
            finalDamageBuffedAmount = buffAbilityEffectSO.damageBuffAmount;

            finalAtkSpeedBuffedAmount = buffAbilityEffectSO.attackSpeedBuffAmount;

            if (buffAbilityEffectSO.damageBuffAmountPercentage > 0.0f)
            {
                float damage = plantUnitSOReceivedBuff.damage;

                finalDamageBuffedAmount = damage *= buffAbilityEffectSO.damageBuffAmountPercentage / 100.0f;
            }

            if (buffAbilityEffectSO.attackSpeedBuffAmountPercentage > 0.0f)
            {
                float atkSpd = plantUnitSOReceivedBuff.attackSpeed;

                finalAtkSpeedBuffedAmount = atkSpd *= buffAbilityEffectSO.attackSpeedBuffAmountPercentage / 100.0f;
            }

            plantUnitSOReceivedBuff.AddPlantUnitDamage(finalDamageBuffedAmount);

            ProcessEffectPopupForBuffEffects(null, "+" /*+ finalDamageBuffedAmount*/ + " APPEASEMENT STRENGTH", finalDamageBuffedAmount);

            plantUnitSOReceivedBuff.AddPlantAttackSpeed(finalAtkSpeedBuffedAmount);

            ProcessEffectPopupForBuffEffects(null, "+" /*+ finalAtkSpeedBuffedAmount*/ + " APPEASEMENT SPEED", finalAtkSpeedBuffedAmount);
        }

        protected override bool EffectUpdate()
        {
            //should do nothing here! Nothing to update.

            return true;
        }

        protected override bool OnEffectEnded()
        {
            if (buffAbilityEffectSO == null) return false;

            if (plantUnitSOReceivedBuff == null) return false;

            //if this effect is being destroyed because the plant unit being affected by it is being destroyed through being uprooted
            //no need to do anything here and return
            if (unitWithThisEffectIsBeingUprooted)
            {
                if (plantUnitReceivedBuff != null && plantUnitReceivedBuff.tilePlacedOn != null)
                {
                    plantUnitReceivedBuff.tilePlacedOn.OnPlantUnitUprootedOnTile.RemoveListener(SubToPlantUnitBeingUprootedOnTileEvent);
                }

                return false;
            }

            plantUnitSOReceivedBuff.RemovePlantUnitDamage(finalDamageBuffedAmount);

            ProcessEffectPopupForBuffEffects(null, "-" + finalDamageBuffedAmount + " AppeasementSTRENGTH", -finalDamageBuffedAmount);

            plantUnitSOReceivedBuff.RemovePlantAttackSpeed(finalAtkSpeedBuffedAmount);

            ProcessEffectPopupForBuffEffects(null, "-" + finalAtkSpeedBuffedAmount + " AppeasementSPEED", -finalAtkSpeedBuffedAmount);

            DetachAndDestroyAllEffectPopupsIncludingSpawner();

            plantUnitReceivedBuff.SetPlantSODebugDataView();

            return true;
        }

        protected void SubToPlantUnitBeingUprootedOnTileEvent(PlantUnit plantUnit, Tile tile)
        {
            if(plantUnitReceivedBuff != null && plantUnit == plantUnitReceivedBuff)
            {
                unitWithThisEffectIsBeingUprooted = true;
            }
        }

        public void ActivateBuffConnectingLine()
        {
            if (disableLineOnSelect) return;

            if (buffConnectingLineRenderer && plantUnitReceivedBuff)
            {
                Vector3 buffSourcePos = sourceUnitProducedEffect.GetUnitTransform().position;

                buffSourcePos += (plantUnitReceivedBuff.transform.position - buffSourcePos).normalized * 0.2f;

                buffConnectingLineRenderer.ActivateLine(buffSourcePos, plantUnitReceivedBuff.transform.position);
            }
        }

        public void DeactivateBuffConnectingLine()
        {
            if (buffConnectingLineRenderer) buffConnectingLineRenderer.DeactivateLine();
        }
    }
}
