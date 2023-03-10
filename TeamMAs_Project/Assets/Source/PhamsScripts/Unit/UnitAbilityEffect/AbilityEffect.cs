using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace TeamMAsTD
{
    [DisallowMultipleComponent]
    public abstract class AbilityEffect : MonoBehaviour
    {
        [field: Header("Required Components")]

        [field: SerializeField]
        [field: DisallowNull]
        public AbilityEffectSO abilityEffectSO { get; private set; }

        public Ability abilityCarriedEffect { get; private set; }

        public AbilitySO abilitySOCarriedEffect { get; private set; }

        public IUnit sourceUnitProducedEffect { get; private set; }

        public IUnit unitBeingAffected { get; private set; }

        public AbilityEffectReceivedInventory abilityEffectInventoryRegisteredTo { get; private set; }

        public float currentEffectDuration { get; set; } = 0.0f;

        protected bool canUpdateEffect { get; private set; } = false;

        protected bool effectIsBeingDestroyed { get; private set; } = false;

        protected virtual void OnDisable()
        {
            //if obj being affected by this effect is disabled (either being destroyed or just disabled),
            //this effect is destroyed regardless
            DestroyEffect();//check for effect alr destroyed is done inside this func
        }

        protected abstract void OnEffectStarted();

        //IMPORTANT:.........................................................................................................
        //If any child ability effect class that has recurring functionality that needs to call this UpdateEffect function
        //It should create its own UnityUpdate func and call this overrided UpdateEffect() func within Unity's Update().
        //REMEMBER to call DestroyEffect() func on effect finished.
        protected abstract void OnEffectUpdated();

        //...................................................................................................................

        protected abstract void OnEffectEnded();

        //EXTERNAL..........................................................................................................

        public void InitializeAbilityEffect(Ability sourceAbility, IUnit unitBeingAffected)
        {
            if (abilityEffectSO == null || sourceAbility == null || unitBeingAffected == null) 
            {
                Debug.LogError("Missing either AbilityEffectSO or SourceAbilityComponent(Ability.cs), or IUnit TargetUnitToAffect" +
                "for AbilityEffect: " + name + "to work. Destroying effect!");

                Destroy(gameObject);

                return;
            }

            abilityEffectInventoryRegisteredTo = unitBeingAffected.GetAbilityEffectReceivedInventory(
                                                    unitBeingAffected.GetUnitTransform().gameObject);

            abilityCarriedEffect = sourceAbility;

            abilitySOCarriedEffect = sourceAbility.abilityScriptableObject;

            sourceUnitProducedEffect = sourceAbility.unitPossessingAbility;

            this.unitBeingAffected = unitBeingAffected;

            currentEffectDuration = abilityEffectSO.effectDuration;

            OnEffectStarted();

            //if ability effect duration is between the range of -1.0f to 0.0f (and not exactly -1.0f)
            //then it is treated as if it has the duration of 0.0f anyway and will not be updated but rather destroyed immediately.
            if (abilityEffectSO != null)
            {
                if(abilityEffectSO.effectDuration > -1.0f && abilityEffectSO.effectDuration <= 0.0f)
                {
                    Destroy(gameObject);

                    return;
                }
            }

            //else can now update after start
            canUpdateEffect = true;
        }

        public void DestroyEffect()
        {
            if (effectIsBeingDestroyed) return;

            canUpdateEffect = false;

            OnEffectEnded();

            if (!effectIsBeingDestroyed) effectIsBeingDestroyed = true;
            
            Destroy(gameObject);
        }
    }
}
