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

        [field: SerializeField]
        protected StatPopupSpawner effectStatPopupSpawner { get; private set; }

        public Ability abilityCarriedEffect { get; private set; }

        public AbilitySO abilitySOCarriedEffect { get; private set; }

        public IUnit sourceUnitProducedEffect { get; private set; }

        public IUnit unitBeingAffected { get; private set; }

        private IUnit parentUnit;

        public UnitSO unitBeingAffectedUnitSO { get; private set; }

        public AbilityEffectReceivedInventory abilityEffectInventoryRegisteredTo { get; private set; }

        public float currentEffectDuration { get; set; } = 0.0f;

        protected bool canUpdateEffect { get; private set; } = false;

        public bool effectIsBeingDestroyed { get; private set; } = false;

        protected virtual void Awake()
        {
            if (abilityEffectSO == null)
            {
                Debug.LogError("Missing AbilityEffectSO data" +
                "for AbilityEffect: " + name + "to work. Destroying effect!");

                DestroyEffectWithEffectEndedInvoked(false);

                return;
            }
        }

        protected virtual void OnEnable()
        {
            parentUnit = GetComponentInParent<IUnit>();

            if (parentUnit == null)
            {
                DestroyEffectWithEffectEndedInvoked(false);

                return;
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
            if (sourceAbility == null || unitBeingAffected == null) 
            {
                Debug.LogError("Missing either SourceAbilityComponent(Ability.cs) or IUnit TargetUnitToAffect" +
                "for AbilityEffect: " + name + "to work. Destroying effect!");

                DestroyEffectWithEffectEndedInvoked(false);

                return;
            }

            if(parentUnit != unitBeingAffected)
            {
                Debug.LogError("The unit this effect: " + name + " is on is not its original target. Destroying effect!");

                DestroyEffectWithEffectEndedInvoked(false);

                return;
            }

            unitBeingAffectedUnitSO = unitBeingAffected.GetUnitScriptableObjectData();

            if(unitBeingAffectedUnitSO == null)
            {
                Debug.LogError("The unit: " + name + " being affected by this effect: " + name + " doesn't have a UnitSO data.");
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

            gameObject.layer = unitBeingAffected.GetUnitLayerMask();

            float effectDuration = abilityEffectSO.effectDuration;

            if (abilityEffectSO.effectDurationAsAbilityDuration) effectDuration = abilitySOCarriedEffect.abilityDuration;

            currentEffectDuration = effectDuration;

            OnEffectStarted();

            //if ability effect duration is not using value from its carrying ability and,
            //if ability effect duration is between the range of -1.0f to 0.0f (and not exactly -1.0f)
            //then it is treated as if it has the duration of 0.0f anyway and will not be updated but rather destroyed immediately.
            if (!abilityEffectSO.effectDurationAsAbilityDuration)
            {
                if (abilityEffectSO.effectDuration > -1.0f && abilityEffectSO.effectDuration <= 0.0f)
                {
                    if(!effectIsBeingDestroyed) DestroyEffectWithEffectEndedInvoked(true);

                    return;
                }
            }

            //else if not being destroyed during any of the above checks - can now update after start (must be the last line of init)
            if(!effectIsBeingDestroyed) canUpdateEffect = true;
        }

        public void DestroyEffectWithEffectEndedInvoked(bool processOnEffectEndedFunc)
        {
            if (effectIsBeingDestroyed) return;

            //IMPORTANT:
            //This if check below ALWAYS need to be execute BEFORE RemoveEffect() func of abilityEffectReceivedInventory to avoid CONFLICTS!!!
            if (!effectIsBeingDestroyed) effectIsBeingDestroyed = true;

            //Debug.Log("Effect Is Being Destroyed On: " + transform.parent.gameObject.name);

            canUpdateEffect = false;

            if(processOnEffectEndedFunc) OnEffectEnded();

            if (abilityEffectInventoryRegisteredTo != null) abilityEffectInventoryRegisteredTo.RemoveEffect(this);

            abilityEffectInventoryRegisteredTo = null;
            
            Destroy(gameObject);
        }

        private void DestroyEffectOnAbilityStoppedIfApplicable(Ability sourceAbility)
        {
            if (sourceAbility == null || sourceAbility != abilityCarriedEffect) return;

            if (sourceAbility.abilityScriptableObject == null || abilityEffectSO == null) return;

            if (effectIsBeingDestroyed) return;

            //Debug.Log("AbilityStoppedDestroyedEventReceivedOn: " + transform.parent.name);

            if (abilityEffectSO.effectDurationAsAbilityDuration) DestroyEffectWithEffectEndedInvoked(true);
        }

        protected virtual void ProcessEffectPopupForBuffEffects(Sprite popupSprite, string popupText, float buffedNumber, float popupTime = 0.0f)
        {
            if (!gameObject.scene.isLoaded) return;

            if (effectStatPopupSpawner == null) return;

            if (buffedNumber == 0.0f) return;

            if (popupTime != 0.0f)
            {
                effectStatPopupSpawner.SetStatPopupSpawnerConfig(0.0f, 0.0f, 0.0f, 0.0f, 0.0f, popupTime);
            }

            if (buffedNumber > 0.0f)
            {
                effectStatPopupSpawner.PopUp(popupSprite, popupText, true);
            }
            else if(buffedNumber < 0.0f)
            {
                effectStatPopupSpawner.PopUp(popupSprite, popupText, false);
            }
        }

        protected void DetachAndDestroyAllEffectPopups()
        {
            if (effectStatPopupSpawner == null) return;

            effectStatPopupSpawner.DetachAndDestroyAllStatPopups();
        }
    }
}
