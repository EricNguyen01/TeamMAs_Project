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
        private IUnit unitCarryingAbilityEffectReceivedInventory;

        private List<AbilityEffect> effectsReceived = new List<AbilityEffect>();

        private bool disableAbilityEffectReceive = false;

        public static event System.Action<AbilityEffectReceivedInventory, AbilityEffect> OnAbilityStacked;

        private void Awake()
        {
            unitCarryingAbilityEffectReceivedInventory = GetComponentInParent<IUnit>();

            if(unitCarryingAbilityEffectReceivedInventory == null)
            {
                Debug.LogError("An AbilityEffectReceivedInventory is attached to gameobject: " + name + " without an IUnit component. Disabling script!");

                enabled = false;

                return;
            }
        }

        public void ReceivedEffectFrom(Ability sourceAbility)
        {
            if (!enabled) return;

            if (sourceAbility == null) return;

            if (sourceAbility.abilityScriptableObject == null) return;

            if (sourceAbility.abilityScriptableObject.abilityEffects == null ||
               sourceAbility.abilityScriptableObject.abilityEffects.Count == 0) return;

            for(int i = 0; i < sourceAbility.abilityScriptableObject.abilityEffects.Count; i++)
            {
                if (sourceAbility.abilityScriptableObject.abilityEffects[i] == null) continue;

                //TODO: process stacking and other stuff here...
            }
        }

        private void CreateInitAndAddEffect(Ability sourceAbility, AbilityEffectSO effectSO)
        {
            if (effectSO.abilityEffectPrefab == null) return;

            GameObject go = Instantiate(effectSO.abilityEffectPrefab.gameObject, transform);

            go.transform.localPosition = Vector3.zero;

            AbilityEffect abilityEffect = go.GetComponent<AbilityEffect>();

            abilityEffect.InitializeAndStartAbilityEffect(sourceAbility, unitCarryingAbilityEffectReceivedInventory);

            effectsReceived.Add(abilityEffect);
        }

        public void RemoveEffect(AbilityEffect effectToRemove)
        {
            if (!enabled) return;

            if (effectToRemove == null) return;

            if(!effectsReceived.Contains(effectToRemove)) return;

            effectsReceived.Remove(effectToRemove);

            effectToRemove.DestroyEffectWithEffectEndedInvoked(true);
        }

        public void EnableAbilityEffectReceive(bool canReceiveEffects)
        {
            disableAbilityEffectReceive = canReceiveEffects;
        }

        public void ClearAllReceivedEffects()
        {
            if (effectsReceived.Count == 0) return;

            for(int i = 0; i <effectsReceived.Count; i++)
            {
                if (effectsReceived[i] == null) continue;

                RemoveEffect(effectsReceived[i]);
            }

            effectsReceived.Clear();
        }
    }
}
