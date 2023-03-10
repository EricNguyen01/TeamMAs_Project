using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamMAsTD
{
    /*
     * Ability Received Inventory processes any ability/ability effects received by the unit of this inventory
     * If unit can receive an ability and its effects -> add, spawn, and activate all the AbilityEffects of the received Ability
     * Also handles the stacking of effects on unit if applicable
     * Further handles the removal of effects on unit when they have finished or being forced to remove (e.g through a cleanse)
     */
    [DisallowMultipleComponent]
    public class AbilityEffectReceivedInventory : MonoBehaviour
    {
        private List<AbilityEffect> effectsReceived = new List<AbilityEffect>();

        private bool disableAbilityEffectReceive = false;

        public void AddEffectFrom(Ability sourceAbility)
        {
            if (sourceAbility == null) return;

            if (sourceAbility.abilityScriptableObject == null) return;

            if (sourceAbility.abilityScriptableObject.abilityEffects == null ||
               sourceAbility.abilityScriptableObject.abilityEffects.Count == 0) return;

            for(int i = 0; i < sourceAbility.abilityScriptableObject.abilityEffects.Count; i++)
            {
                if (sourceAbility.abilityScriptableObject.abilityEffects[i] == null) continue;

                CreateInitAndAddEffect(sourceAbility.abilityScriptableObject.abilityEffects[i]);
            }
        }

        private void CreateInitAndAddEffect(AbilityEffectSO effectSO)
        {

        }

        public void RemoveEffect(AbilityEffect effectToRemove)
        {

        }

        public void EnableAbilityEffectReceive(bool canReceiveEffects)
        {
            disableAbilityEffectReceive = canReceiveEffects;
        }
    }
}
