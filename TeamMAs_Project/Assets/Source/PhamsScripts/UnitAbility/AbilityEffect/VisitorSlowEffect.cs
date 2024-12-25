// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using UnityEngine;

namespace TeamMAsTD
{
    public class VisitorSlowEffect : BuffDebuffAbilityEffect
    {
        protected VisitorUnitSO visitorUnitSOReceivedBuff { get; private set; }

        protected VisitorUnit visitorUnitReceivedBuff { get; private set; }

        private float finalSlowedAmount = 0.0f;

        protected override void Awake()
        {
            base.Awake();

            if (!abilityEffectSO)
            {
                enabled = false;

                return;
            }

            if (!deBuffAbilityEffectSO)
            {
                enabled = false;

                DestroyEffectWithEffectEndedInvoked(false);

                return;
            }
        }

        protected override bool OnEffectStarted()
        {
            if (!base.OnEffectStarted()) return false;

            if (unitBeingAffectedUnitSO.GetType() != typeof(VisitorUnitSO))
            {
                Debug.LogError("The unit: " + name + " being affected by this debuff effect: " + name + " " +
                "has UnitSO data that IS NOT of Type VisitorUnitSO. DeBuff won't work!\n" +
                "Destroying debuff effect!");

                DestroyEffectWithEffectEndedInvoked(false);

                return false;
            }

            if (!visitorUnitSOReceivedBuff) visitorUnitSOReceivedBuff = (VisitorUnitSO)unitBeingAffectedUnitSO;

            if(!visitorUnitReceivedBuff) visitorUnitReceivedBuff = (VisitorUnit)unitBeingAffected.GetUnitObject();

            ProcessSlowingVisitor();

            return true;
        }

        private void ProcessSlowingVisitor()
        {
            finalSlowedAmount = deBuffAbilityEffectSO.movementSpeedDeBuffAmount;

            if(deBuffAbilityEffectSO.movementSpeedDeBuffAmountPercentage > 0)
            {
                float visitorMoveSpd = visitorUnitSOReceivedBuff.moveSpeed;

                finalSlowedAmount = visitorMoveSpd *= deBuffAbilityEffectSO.movementSpeedDeBuffAmountPercentage / 100.0f;
            }

            visitorUnitSOReceivedBuff.RemoveVisitorMoveSpeed(finalSlowedAmount);

            if(visitorUnitReceivedBuff) visitorUnitReceivedBuff.UpdateVisitorStatsDebugData();

            ProcessEffectPopupForBuffEffects(null, "Slowed", 0.0f, true);
        }

        protected override bool EffectUpdate()
        {
            return true;
        }

        protected override bool OnEffectEnded()
        {
            if (deBuffAbilityEffectSO == null) return false;

            if (visitorUnitSOReceivedBuff == null) return false;

            visitorUnitSOReceivedBuff.AddVisitorMoveSpeed(finalSlowedAmount);

            if (visitorUnitReceivedBuff) visitorUnitReceivedBuff.UpdateVisitorStatsDebugData();

            DetachAndDestroyAllEffectPopupsIncludingSpawner();

            return true;
        }
    }
}
