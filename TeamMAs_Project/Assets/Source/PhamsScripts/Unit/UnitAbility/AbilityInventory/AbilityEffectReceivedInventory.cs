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

        #region AbilityEffectReceived Private Class
        //AbilityEffectReceived Private Class..................................................................................
        //Represents 1 Effect type (and its stacks if exist) affecting this unit 
        [System.Serializable]
        private class AbilityEffectReceived
        {
            public Ability sourceAbilityReceived { get; private set; }
            public AbilityEffectSO effectSOReceived { get; private set; }
            public AbilityEffectReceivedInventory abilityEffectReceivedInventory { get; private set; }
            public List<AbilityEffect> effectStackSpawned { get; private set; } = new List<AbilityEffect>();

            //AbilityEffectReceived constructor
            public AbilityEffectReceived(Ability sourceAbility, AbilityEffectSO effectSO, AbilityEffectReceivedInventory effectReceivedInventory)
            {
                sourceAbilityReceived = sourceAbility;

                effectSOReceived = effectSO;

                abilityEffectReceivedInventory = effectReceivedInventory;
            }

            public void AddStack(int stackNumAdded)
            {
                if (stackNumAdded <= 0) return;

                if (sourceAbilityReceived == null || 
                    effectSOReceived == null || 
                    abilityEffectReceivedInventory == null) return;

                for(int i = 0; i < stackNumAdded; i++)
                {
                    AbilityEffect effectSpawned = null;

                    effectSpawned = abilityEffectReceivedInventory.CreateStartAndAddEffect(sourceAbilityReceived, effectSOReceived);

                    if (effectSpawned != null) effectStackSpawned.Add(effectSpawned);
                }
            }

            public void RemoveStack(int stackNumRemoved)
            {
                if (stackNumRemoved <= 0) return;

                if (effectStackSpawned == null || effectStackSpawned.Count == 0) return;

                bool clearAllStacks = false;

                if (stackNumRemoved >= effectStackSpawned.Count) 
                { 
                    stackNumRemoved = effectStackSpawned.Count; 

                    clearAllStacks = true;
                }

                for(int i = 0; i < stackNumRemoved; i++)
                {
                    effectStackSpawned[i].DestroyEffectWithEffectEndedInvoked(true);

                    effectStackSpawned.RemoveAt(i);
                }

                //if all stacks are pending to be removed -> also remove this effect slot (no stack = no effect on unit)
                //must ALWAYS be done at the end of this function to avoid infinite recursive loop
                if (clearAllStacks)
                {
                    RemoveThisEffectSlotCompletely();
                }
            }

            public void RemoveThisEffectSlotCompletely()//treat this as destructor
            {
                RemoveStack(effectStackSpawned.Count);

                effectStackSpawned.Clear();

                sourceAbilityReceived = null;

                effectSOReceived = null;

                if (abilityEffectReceivedInventory.abilityEffectsReceived.Contains(this))
                {
                    abilityEffectReceivedInventory.abilityEffectsReceived.Remove(this);
                }

                abilityEffectReceivedInventory = null;
            }
        }

        #endregion
        //End private class....................................................................................................

        //List of AbilityEffectReceived slots made of the AbilityEffectReceived private class above
        private List<AbilityEffectReceived> abilityEffectsReceived = new List<AbilityEffectReceived>();

        private bool disableAbilityEffectReceive = false;

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

        private void OnDisable()
        {
            ClearAllReceivedEffects();
        }

        public void ReceivedEffectFrom(Ability sourceAbility)
        {
            if (!enabled || disableAbilityEffectReceive) return;

            if (sourceAbility == null) return;

            if (sourceAbility.abilityScriptableObject == null) return;

            if (sourceAbility.abilityScriptableObject.abilityEffects == null ||
               sourceAbility.abilityScriptableObject.abilityEffects.Count == 0) return;

            for(int i = 0; i < sourceAbility.abilityScriptableObject.abilityEffects.Count; i++)
            {
                if (sourceAbility.abilityScriptableObject.abilityEffects[i] == null) continue;

                AbilityEffectSO currentEffectSOToApply = sourceAbility.abilityScriptableObject.abilityEffects[i];

                ProcessApplyEffect(sourceAbility, currentEffectSOToApply);
            }
        }

        private void ProcessApplyEffect(Ability sourceAbility, AbilityEffectSO effectSO)
        {
            //if no effect has been applied before (this effect is the first) -> proceed to apply effect immediately then exit func completely.
            if(abilityEffectsReceived.Count == 0)
            {
                ApplyEffectStackToSlot(null, sourceAbility, effectSO);

                return;
            }

            //else if there has been effect currently been applied, check for compatibility/applicability
            for (int i = 0; i < abilityEffectsReceived.Count; i++)
            {
                if (abilityEffectsReceived[i] == null || abilityEffectsReceived[i].effectSOReceived == null) continue;

                //if an existing effect of the same type of the one being received is found (process stackable):
                if (abilityEffectsReceived[i].effectSOReceived == effectSO)
                {
                    //if ability effect is stackable and a same effect type alr existed -> stack on it
                    if (effectSO.effectStackable)
                    {
                        ApplyEffectStackToSlot(abilityEffectsReceived[i], sourceAbility, effectSO);

                        return;//exit on finished stacking
                    }

                    //if ability effect IS NOT STACKABLE and a same effect type alr existed -> swap the old one with a new one

                    //first remove the old existing effect of same type
                    abilityEffectsReceived[i].RemoveThisEffectSlotCompletely();

                    //then apply new same type effect
                    //a new abilityEffectReceived slot is created for this effect
                    //since it is non-stackable, we are treating it as if it is a new effect being applied after removing the old one.
                    ApplyEffectStackToSlot(null, sourceAbility, effectSO);

                    return;
                }
            }

            //else if none of this effect type currently being applied to this unit -> apply it
            ApplyEffectStackToSlot(null, sourceAbility, effectSO);
        }

        private void ApplyEffectStackToSlot(AbilityEffectReceived abilityEffectReceived, Ability sourceAbility, AbilityEffectSO effectSO)
        {
            if (sourceAbility == null || effectSO == null) return;
            
            //if the effect received is a brand new effect that has not been applied to unit before
            //create new ability effect received slot and add 1 stack of it
            if(abilityEffectReceived == null)
            {
                abilityEffectReceived = new AbilityEffectReceived(sourceAbility, effectSO, this);

                abilityEffectReceived.AddStack(1);

                abilityEffectsReceived.Add(abilityEffectReceived);

                return;//exit on finished
            }

            //else if a slot of effect received existed -> add another stack to it.
            abilityEffectReceived.AddStack(1);
        }

        private AbilityEffect CreateStartAndAddEffect(Ability sourceAbility, AbilityEffectSO effectSO)
        {
            if (effectSO.abilityEffectPrefab == null) return null;

            GameObject go = Instantiate(effectSO.abilityEffectPrefab.gameObject, transform);

            go.transform.localPosition = Vector3.zero;

            AbilityEffect abilityEffect = go.GetComponent<AbilityEffect>();

            abilityEffect.InitializeAndStartAbilityEffect(sourceAbility, unitCarryingAbilityEffectReceivedInventory);

            return abilityEffect;
        }

        public void RemoveEffect(AbilityEffect effectToRemove, int stackToRemove = 1, bool removeAllStacks = false)
        {
            if (!enabled) return;

            if (effectToRemove == null) return;

            if(abilityEffectsReceived.Count == 0) return;

            RemoveEffectStackFromSlot(null, effectToRemove.abilityEffectSO, stackToRemove, removeAllStacks);
        }

        private void RemoveEffectStackFromSlot(AbilityEffectReceived abilityEffectReceivedSlot, AbilityEffectSO effectSO, int stackToRemove = 1, bool removeAllStacks = false)
        {
            if (stackToRemove <= 0) return;

            if (abilityEffectsReceived.Count == 0) return;

            if(abilityEffectReceivedSlot == null)
            {
                for (int i = 0; i < abilityEffectsReceived.Count; i++)
                {
                    if (abilityEffectsReceived[i] == null || abilityEffectsReceived[i].effectSOReceived == null) continue;

                    if (abilityEffectsReceived[i].effectSOReceived == effectSO)
                    {
                        if (removeAllStacks) abilityEffectsReceived[i].RemoveThisEffectSlotCompletely();
                        else abilityEffectsReceived[i].RemoveStack(stackToRemove);

                        return;
                    }
                }
            }

            if (abilityEffectReceivedSlot.effectSOReceived != effectSO) return;

            if (removeAllStacks) abilityEffectReceivedSlot.RemoveThisEffectSlotCompletely();
            else abilityEffectReceivedSlot.RemoveStack(stackToRemove);
        }

        public void ClearAllReceivedEffects()
        {
            if (abilityEffectsReceived.Count == 0) return;

            for(int i = 0; i < abilityEffectsReceived.Count; i++)
            {
                if(abilityEffectsReceived[i] != null) abilityEffectsReceived[i].RemoveThisEffectSlotCompletely();

                abilityEffectsReceived.RemoveAt(i);
            }

            abilityEffectsReceived.Clear();
        }

        public void EnableAbilityEffectReceive(bool canReceiveEffects)
        {
            disableAbilityEffectReceive = canReceiveEffects;
        }
    }
}
