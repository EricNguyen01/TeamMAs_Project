// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

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

        //AbilityEffectReceived abstract interface wrapper for external class to read
        //any instance of AbilityEffectReceived created internally within this class
        public interface IAbilityEffectReceived
        {
            public Ability SourceAbilityOfEffectReceived();
            public AbilityEffectSO EffectSOReceived();
            public AbilityEffectReceivedInventory AbilityEffectReceivedInventory();
            public List<AbilityEffect> EffectStackSpawned();
            public void RemoveAStackOfThisSpawnedEffect(AbilityEffect spawnedEffectToRemove);
            public void RemoveThisEffectSlotCompletely();
        }

        //AbilityEffectReceived private nested class..................................................................................
        //Represents and process the adding/removing of 1 Effect type slot (and its stacks within slot if exist) affecting this unit 
        //class and its constructor is private with an abstract interface wrapper above in case any external class would like to read
        //only AbilityEffectReceivedInventory class (this class) can create AbilityEffectReceived class

        #region AbilityEffectReceived Private Class

        [System.Serializable]
        private class AbilityEffectReceived : IAbilityEffectReceived
        {
            public Ability sourceAbilityOfEffect { get; private set; }

            public AbilityEffectSO effectSOReceived { get; private set; }

            public AbilityEffectReceivedInventory abilityEffectReceivedInventory { get; private set; }

            public List<AbilityEffect> effectStackSpawned { get; private set; } = new List<AbilityEffect>();

            //internal AbilityEffectReceived constructor
            public AbilityEffectReceived(Ability sourceAbility, AbilityEffectSO effectSO, AbilityEffectReceivedInventory effectReceivedInventory)
            {
                sourceAbilityOfEffect = sourceAbility;

                effectSOReceived = effectSO;

                abilityEffectReceivedInventory = effectReceivedInventory;
            }

            public void AddStack(Ability sourceAbilityReceived, int stackNumAdded)
            {
                if (stackNumAdded <= 0) return;

                if (sourceAbilityReceived == null || effectSOReceived == null || abilityEffectReceivedInventory == null) return;

                for(int i = 0; i < stackNumAdded; i++)
                {
                    AbilityEffect effectSpawned = null;

                    effectSpawned = abilityEffectReceivedInventory.CreateAndStartEffect(sourceAbilityReceived, effectSOReceived);

                    if (effectSpawned != null) 
                    { 
                        effectStackSpawned.Add(effectSpawned); 

                        sourceAbilityReceived.RegisterAbilityEffectCreatedByThisAbility(effectSpawned);
                    }
                }
            }

            public void RemoveAStackOfThisSpawnedEffect(AbilityEffect spawnedEffectToRemove)
            {
                if (spawnedEffectToRemove == null) return;

                //check if this effect slot contains the provided effect to remove, if contains, performs removal
                if (!effectStackSpawned.Contains(spawnedEffectToRemove)) return;

                effectStackSpawned.Remove(spawnedEffectToRemove);

                if(!spawnedEffectToRemove.effectIsBeingDestroyed) spawnedEffectToRemove.DestroyEffectWithEffectEndedInvoked(true);

                //clean up any null effect stack element just in case they were missed
                if(effectStackSpawned != null && effectStackSpawned.Count > 0) effectStackSpawned.RemoveAll(item => item == null);

                //if there are still stacks of effect type slot exist after removing this effect -> return
                if (effectStackSpawned.Count > 0) return;

                //else if there are no stacks left after removing this effect from its effect type slot -> remove effect type slot completely
                RemoveThisEffectSlotCompletely();
            }

            public void RemoveThisEffectSlotCompletely()//treat this as destructor
            {
                if(effectStackSpawned != null && effectStackSpawned.Count > 0)
                {
                    for (int i = 0; i < effectStackSpawned.Count; i++)
                    {
                        if (!effectStackSpawned[i].effectIsBeingDestroyed) effectStackSpawned[i].DestroyEffectWithEffectEndedInvoked(true);

                        if (i >= effectStackSpawned.Count || effectStackSpawned.Count == 0) break;
                    }
                }

                effectStackSpawned.Clear();

                effectSOReceived = null;

                if (abilityEffectReceivedInventory == null || abilityEffectReceivedInventory.abilityEffectsReceived == null) return;

                if (abilityEffectReceivedInventory.abilityEffectsReceived.Contains(this))
                {
                    abilityEffectReceivedInventory.abilityEffectsReceived.Remove(this);
                }

                abilityEffectReceivedInventory = null;
            }

            public Ability SourceAbilityOfEffectReceived()
            {
                return sourceAbilityOfEffect;
            }

            public AbilityEffectSO EffectSOReceived()
            {
                return effectSOReceived;
            }

            public AbilityEffectReceivedInventory AbilityEffectReceivedInventory()
            {
                return abilityEffectReceivedInventory;
            }

            public List<AbilityEffect> EffectStackSpawned()
            {
                return effectStackSpawned;
            }
        }

        #endregion

        //End nested class....................................................................................................

        //List of AbilityEffectReceived slots made of the AbilityEffectReceived private class above (readonly from outside)
        public List<IAbilityEffectReceived> abilityEffectsReceived { get; private set; } = new List<IAbilityEffectReceived>();

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

        public void ReceivedEffectsFromAbility(Ability sourceAbility)
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
                if (abilityEffectsReceived[i] == null || abilityEffectsReceived[i].EffectSOReceived() == null) continue;

                //if an existing effect of the same type of the one being received is found (process stackable):
                if (abilityEffectsReceived[i].EffectSOReceived() == effectSO)
                {
                    //if ability effect is stackable and a same effect type alr existed -> stack on it
                    if (effectSO.effectStackable)
                    {
                        ApplyEffectStackToSlot((AbilityEffectReceived)abilityEffectsReceived[i], sourceAbility, effectSO);

                        return;//exit on finished stacking
                    }
                    //else if ability effect is NOT stackable and a same effect type alr existed -> do nothing!
                    else return;
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
                //create new effect type received slot
                abilityEffectReceived = new AbilityEffectReceived(sourceAbility, effectSO, this);

                //spawn 1 effect obj (as 1 stack) of the created effect type slot 
                //store this spawned effect in "effectsSpawned" list of the newly created effect type slot
                abilityEffectReceived.AddStack(sourceAbility, 1);

                //add the newly created effect type slot into the abilityEffectsReceived list
                abilityEffectsReceived.Add(abilityEffectReceived);

                return;//exit on finished
            }

            //else if a slot of effect received existed -> add another stack to it.
            abilityEffectReceived.AddStack(sourceAbility, 1);
        }

        private AbilityEffect CreateAndStartEffect(Ability sourceAbility, AbilityEffectSO effectSO)
        {
            if (effectSO.abilityEffectPrefab == null) return null;

            GameObject go = Instantiate(effectSO.abilityEffectPrefab.gameObject, transform);

            go.transform.localPosition = Vector3.zero;

            AbilityEffect abilityEffect = go.GetComponent<AbilityEffect>();

            abilityEffect.InitializeAndStartAbilityEffect(sourceAbility, unitCarryingAbilityEffectReceivedInventory);

            return abilityEffect;
        }

        //This function removes all the currently spawned effects of every effect type that was spawned from "sourceAbility"
        public void RemoveAllEffectsOfAnAbility(Ability sourceAbility)
        {
            if (sourceAbility == null) return;

            if (sourceAbility.abilityScriptableObject == null) return;

            if (sourceAbility.abilityScriptableObject.abilityEffects == null ||
               sourceAbility.abilityScriptableObject.abilityEffects.Count == 0) return;

            if (abilityEffectsReceived == null || abilityEffectsReceived.Count == 0) return;

            //start going through every effect type received list
            for(int i = 0; i < abilityEffectsReceived.Count; i++)
            {
                if (abilityEffectsReceived[i] == null || abilityEffectsReceived[i].EffectSOReceived() == null) continue;

                if (abilityEffectsReceived[i].EffectStackSpawned() == null || abilityEffectsReceived[i].EffectStackSpawned().Count == 0) continue;
                
                //on valid effect type received element, start going through all the spawned effect objects of the current effect type element
                for(int j = 0; j < abilityEffectsReceived[i].EffectStackSpawned().Count; j++)
                {
                    //the line below are to avoid out of range exception since we are removing effects as we are looping through the lists
                    //which the effect lists are actively shrinking and at some point we would be out of range
                    //if (i >= abilityEffectsReceived.Count || abilityEffectsReceived.Count == 0) return;

                    if (abilityEffectsReceived[i].EffectStackSpawned()[j] == null) continue;

                    if (abilityEffectsReceived[i].EffectStackSpawned()[j].abilityCarriedEffect != sourceAbility) continue;

                    //if the effect was of "sourceAbility" -> remove it
                    //this func below also checks if the last spawned effect obj of this effect received slot has been removed
                    //and if so, it also removes this slot altogether.
                    abilityEffectsReceived[i].RemoveAStackOfThisSpawnedEffect(abilityEffectsReceived[i].EffectStackSpawned()[j]);

                    //the lines below are to avoid out of range exception since we are removing effects as we are looping through the lists
                    //which the effect lists are actively shrinking and at some point we would be out of range
                    //if (i >= abilityEffectsReceived.Count || abilityEffectsReceived.Count == 0) return;

                    if (abilityEffectsReceived[i] == null || abilityEffectsReceived[i].EffectStackSpawned() == null) break;

                    if (j >= abilityEffectsReceived[i].EffectStackSpawned().Count || 
                        abilityEffectsReceived[i].EffectStackSpawned().Count == 0) break;
                }

                if (i >= abilityEffectsReceived.Count || abilityEffectsReceived.Count == 0) return;
            }
        }

        /// <summary>
        /// Remove a specific effect that was received from a specific source ability.
        /// If stacks to remove is set to <=0 (default) then all stacks of the designated effect will be removed.
        /// </summary>
        /// <param name="sourceAbility"></param>
        /// <param name="stacksOfEffectToRemove"></param>
        public void RemoveASpecificEffectOfAnAbility(Ability sourceAbility, AbilityEffectSO effectToRemove, int stacksOfEffectToRemove = 0)
        {
            if (sourceAbility == null) return;

            if(effectToRemove == null) return;

            if (sourceAbility.abilityScriptableObject == null) return;

            if (sourceAbility.abilityScriptableObject.abilityEffects == null ||
               sourceAbility.abilityScriptableObject.abilityEffects.Count == 0) return;

            if(!sourceAbility.abilityScriptableObject.abilityEffects.Contains(effectToRemove)) return;

            if (abilityEffectsReceived == null || abilityEffectsReceived.Count == 0) return;

            //start going through every effect type received slot
            for (int i = 0; i < abilityEffectsReceived.Count; i++)
            {
                //null checks
                if (abilityEffectsReceived[i] == null || abilityEffectsReceived[i].EffectSOReceived() == null) continue;

                //check if source ability of effect to remove matches source ability of effect received slot
                if (!abilityEffectsReceived[i].SourceAbilityOfEffectReceived() ||
                    abilityEffectsReceived[i].SourceAbilityOfEffectReceived() != sourceAbility) continue;

                //check if effect to remove matches any effect received slot
                if (abilityEffectsReceived[i].EffectSOReceived() != effectToRemove) continue;

                if (abilityEffectsReceived[i].EffectStackSpawned() == null || abilityEffectsReceived[i].EffectStackSpawned().Count == 0) continue;

                if (stacksOfEffectToRemove > abilityEffectsReceived[i].EffectStackSpawned().Count)
                {
                    stacksOfEffectToRemove = abilityEffectsReceived[i].EffectStackSpawned().Count;
                }

                //if stacks to remove is <= 0 || == stacks count -> remove all stacks of provided effect type
                if (stacksOfEffectToRemove <= 0 || stacksOfEffectToRemove == abilityEffectsReceived[i].EffectStackSpawned().Count)
                {
                    abilityEffectsReceived[i].RemoveThisEffectSlotCompletely();

                    return;
                }

                int removedStacksCount = 0;

                //valid effect type slot found in effects received list -> begins going through its stacks (of the same effect type)
                for (int j = 0; j < abilityEffectsReceived[i].EffectStackSpawned().Count; j++)
                {
                    if (removedStacksCount == stacksOfEffectToRemove) return;

                    if (abilityEffectsReceived[i].EffectStackSpawned()[j] == null) continue;

                    if (abilityEffectsReceived[i].EffectStackSpawned()[j].abilityCarriedEffect != sourceAbility) continue;

                    abilityEffectsReceived[i].RemoveAStackOfThisSpawnedEffect(abilityEffectsReceived[i].EffectStackSpawned()[j]);

                    removedStacksCount++;

                    if (abilityEffectsReceived[i] == null || abilityEffectsReceived[i].EffectStackSpawned() == null) break;

                    if (j >= abilityEffectsReceived[i].EffectStackSpawned().Count ||
                        abilityEffectsReceived[i].EffectStackSpawned().Count == 0) break;
                }

                if (i >= abilityEffectsReceived.Count || abilityEffectsReceived.Count == 0) return;
            }
        }

        public void RemoveAStackOfASpecificEffect(AbilityEffect effectToRemove)
        {
            if (!enabled) return;

            if (effectToRemove == null) return;

            if(abilityEffectsReceived.Count == 0) return;

            RemoveAnEffectStackFromEffectSlot(effectToRemove);
        }

        private void RemoveAnEffectStackFromEffectSlot(AbilityEffect effect)
        {
            if (effect == null) return;

            if (abilityEffectsReceived.Count == 0) return;

            for (int i = 0; i < abilityEffectsReceived.Count; i++)
            {
                if (abilityEffectsReceived[i] == null || abilityEffectsReceived[i].EffectSOReceived() == null) continue;

                if (abilityEffectsReceived[i].EffectSOReceived() == effect.abilityEffectSO)
                {
                    abilityEffectsReceived[i].RemoveAStackOfThisSpawnedEffect(effect);

                    return;//exit func
                }
            }

            /*
            //if an ability effect type received slot IS provided

            //check if this effect slot contains the provided effect to remove, if contains, performs removal
            if (abilityEffectReceivedSlot.effectSOReceived != effect.abilityEffectSO) return;

            abilityEffectReceivedSlot.RemoveAStackOfThisSpawnedEffect(effect);*/
        }

        public void ClearAllReceivedEffects()
        {
            if (abilityEffectsReceived.Count == 0) return;

            for(int i = 0; i < abilityEffectsReceived.Count; i++)
            {
                if(abilityEffectsReceived[i] != null) abilityEffectsReceived[i].RemoveThisEffectSlotCompletely();
            }

            abilityEffectsReceived.Clear();
        }

        public void ForceDestroyImmediate_AllReceivedEffectsStatPopups_AndPopupSpawners()
        {
            if (abilityEffectsReceived.Count == 0) return;

            for (int i = 0; i < abilityEffectsReceived.Count; i++)
            {
                if (abilityEffectsReceived[i] == null) continue;

                if (abilityEffectsReceived[i].EffectStackSpawned() == null ||
                    abilityEffectsReceived[i].EffectStackSpawned().Count == 0) continue;

                for (int j = 0; j < abilityEffectsReceived[i].EffectStackSpawned().Count; j++)
                {
                    AbilityEffect aEffect = abilityEffectsReceived[i].EffectStackSpawned()[j];

                    if (aEffect == null) continue;

                    aEffect.DetachAndDestroyAllEffectPopupsIncludingSpawner(true);
                }
            }
        }

        public bool HasEffectsFromAbility(Ability sourceAbility)
        {
            if(sourceAbility == null) return false;

            if (abilityEffectsReceived == null || abilityEffectsReceived.Count == 0) return false;

            //start going through every effect type received slot
            for (int i = 0; i < abilityEffectsReceived.Count; i++)
            {
                if (abilityEffectsReceived[i].EffectStackSpawned() == null || 
                    abilityEffectsReceived[i].EffectStackSpawned().Count == 0) continue;

                //check if source ability of effect to remove matches source ability of effect received slot
                if (abilityEffectsReceived[i].SourceAbilityOfEffectReceived() &&
                    abilityEffectsReceived[i].SourceAbilityOfEffectReceived() == sourceAbility) return true;
            }
            
            return false;
        }

        public void EnableReceivingAbilityEffects(bool canReceiveEffects)
        {
            disableAbilityEffectReceive = canReceiveEffects;
        }
    }
}
