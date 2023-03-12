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

        private IUnit parentUnit;

        public AbilityEffectReceivedInventory abilityEffectInventoryRegisteredTo { get; private set; }

        public float currentEffectDuration { get; set; } = 0.0f;

        protected bool canUpdateEffect { get; private set; } = false;

        protected bool effectIsBeingDestroyed { get; private set; } = false;

        protected virtual void OnEnable()
        {
            parentUnit = GetComponentInParent<IUnit>();

            if (parentUnit == null)
            {
                DestroyEffectWithEffectEndedInvoked(false);
            }

            Ability.OnAbilityStopped += DestroyEffectOnAbilityStoppedIfApplicable;
        }

        protected virtual void OnDisable()
        {
            //if parent unit obj being affected by this effect is disabled (either being destroyed or just disabled),
            //this effect is destroyed regardless
            if(!effectIsBeingDestroyed) DestroyEffectWithEffectEndedInvoked(true);

            Ability.OnAbilityStopped -= DestroyEffectOnAbilityStoppedIfApplicable;
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

        public void InitializeAndStartAbilityEffect(Ability sourceAbility, IUnit unitBeingAffected)
        {
            if (abilityEffectSO == null || sourceAbility == null || unitBeingAffected == null) 
            {
                Debug.LogError("Missing either AbilityEffectSO or SourceAbilityComponent(Ability.cs), or IUnit TargetUnitToAffect" +
                "for AbilityEffect: " + name + "to work. Destroying effect!");

                Destroy(gameObject);

                effectIsBeingDestroyed = true;

                return;
            }

            if(parentUnit != unitBeingAffected)
            {
                Debug.LogError("The unit this effect is on is not its original target. Destroying effect!");

                DestroyEffectWithEffectEndedInvoked(false);

                return;
            }

            abilityEffectInventoryRegisteredTo = unitBeingAffected.GetAbilityEffectReceivedInventory();

            if(abilityEffectInventoryRegisteredTo == null)
            {
                Debug.LogError("The unit this effect is on does not have an AbilityEffectReceivedInventory to process this effect. " +
                "Destroying effect!");

                DestroyEffectWithEffectEndedInvoked(false);

                return;
            }

            abilityCarriedEffect = sourceAbility;

            abilitySOCarriedEffect = sourceAbility.abilityScriptableObject;

            sourceUnitProducedEffect = sourceAbility.unitPossessingAbility;

            this.unitBeingAffected = unitBeingAffected;

            float effectDuration = abilityEffectSO.effectDuration;

            if (abilityEffectSO.effectDurationAsAbilityDuration) effectDuration = abilitySOCarriedEffect.abilityDuration;

            currentEffectDuration = effectDuration;

            OnEffectStarted();

            //if ability effect duration is between the range of -1.0f to 0.0f (and not exactly -1.0f)
            //then it is treated as if it has the duration of 0.0f anyway and will not be updated but rather destroyed immediately.
            if (abilityEffectSO.effectDuration > -1.0f && abilityEffectSO.effectDuration <= 0.0f)
            {
                DestroyEffectWithEffectEndedInvoked(true);

                return;
            }

            //else can now update after start
            canUpdateEffect = true;
        }

        public void DestroyEffectWithEffectEndedInvoked(bool processOnEffectEnded)
        {
            if (effectIsBeingDestroyed) return;

            canUpdateEffect = false;

            if(processOnEffectEnded) OnEffectEnded();

            if (abilityEffectInventoryRegisteredTo != null) abilityEffectInventoryRegisteredTo.RemoveEffect(this);

            if (!effectIsBeingDestroyed) effectIsBeingDestroyed = true;
            
            Destroy(gameObject);
        }

        private void DestroyEffectOnAbilityStoppedIfApplicable(Ability sourceAbility)
        {
            if (sourceAbility == null || sourceAbility != abilityCarriedEffect) return;

            if (sourceAbility.abilityScriptableObject == null || abilityEffectSO == null) return;

            if (abilityEffectSO.effectDurationAsAbilityDuration) DestroyEffectWithEffectEndedInvoked(true);
        }
    }
}
