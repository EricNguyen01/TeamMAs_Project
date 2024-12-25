// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using System.Collections;
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

        [Header("Ability Effect Particle FXs")]

        [SerializeField] protected ParticleSystem effectStartFx;

        [SerializeField] protected ParticleSystem effectUpdateFx;


        //INTERNALS..............................................................................................
        public Ability abilityCarriedEffect { get; private set; }

        public AbilitySO abilitySOCarriedEffect { get; private set; }

        public IUnit sourceUnitProducedEffect { get; private set; }

        public IUnit unitBeingAffected { get; private set; }

        private IUnit parentUnit;

        public UnitSO unitBeingAffectedUnitSO { get; private set; }

        public AbilityEffectReceivedInventory abilityEffectInventoryRegisteredTo { get; private set; }

        protected float effectDuration { get; private set; } = 0.0f;

        //protected bool canUpdateEffect { get; private set; } = false;

        protected bool unitWithThisEffectIsBeingUprooted = false;

        public bool effectIsBeingDestroyed { get; private set; } = false;

        protected bool effectEndIsCalled = false;

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

        protected abstract bool OnEffectStarted();

        protected abstract bool EffectUpdate();

        protected abstract bool OnEffectEnded();

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

            gameObject.layer = unitBeingAffected.GetUnitLayerMask();

            float effectDuration = abilityEffectSO.effectDuration;

            if (abilityEffectSO.effectDurationAsAbilityDuration) effectDuration = abilitySOCarriedEffect.abilityDuration;

            this.effectDuration = effectDuration;

            InitializeAbilityEffectFXs(sourceAbility);
            
            //only start effect after all data have been initialized
            //any data not init must be init before effect starts/updates
            OnEffectStarted();

            if (!abilityEffectSO.effectDurationAsAbilityDuration)
            {
                //if ability effect duration is not using value from its carrying ability and,
                //if ability effect duration is between the range of -1.0f to 0.0f (and not exactly -1.0f)
                //then it is treated as if it has the duration of 0.0f anyway and will not be updated but rather destroyed immediately.
                if (abilityEffectSO.effectDuration > -1.0f && abilityEffectSO.effectDuration <= 0.0f)
                {
                    if(!effectIsBeingDestroyed) DestroyEffectWithEffectEndedInvoked(true);

                    return;
                }

                if (this.effectDuration > 0.0f)
                {
                    if (effectIsBeingDestroyed) return;

                    //else if not being destroyed during any of the above checks
                    //and if effect duration is set to a valid value (>0)
                    //can now update effect after start (must be the last function of init)
                    StartCoroutine(AbilityEffectUpdateCoroutine(abilityEffectSO.effectDuration));
                }
            }
        }

        private IEnumerator AbilityEffectUpdateCoroutine(float effectDuration)
        {
            if (effectIsBeingDestroyed) yield break;

            float time = 0.0f;

            while (time < effectDuration)
            {
                if (effectIsBeingDestroyed) yield break;

                time += Time.fixedDeltaTime;

                EffectUpdate();

                yield return new WaitForFixedUpdate();
            }

            if (!effectIsBeingDestroyed && !abilityEffectSO.effectDurationAsAbilityDuration)
            {
                DestroyEffectWithEffectEndedInvoked(true);
            }

            yield break;
        }

        public void DestroyEffectWithEffectEndedInvoked(bool callOnEffectEndedFunc)
        {
            if (effectIsBeingDestroyed) return;

            //IMPORTANT:
            //This if check below ALWAYS need to be execute BEFORE RemoveEffect() func of abilityEffectReceivedInventory to avoid CONFLICTS!!!
            if (!effectIsBeingDestroyed) effectIsBeingDestroyed = true;

            //Debug.Log("Effect Is Being Destroyed On: " + transform.parent.gameObject.name);

            StopCoroutine(AbilityEffectUpdateCoroutine(abilityEffectSO.effectDuration));

            if (callOnEffectEndedFunc && !effectEndIsCalled)
            {
                OnEffectEnded();

                effectEndIsCalled = true;
            }

            if (abilityEffectInventoryRegisteredTo != null) abilityEffectInventoryRegisteredTo.RemoveAStackOfASpecificEffect(this);

            abilityEffectInventoryRegisteredTo = null;

            abilityCarriedEffect.DeRegisterAbilityEffectCreatedByThisAbility(this);
            
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

        protected virtual void InitializeAbilityEffectFXs(Ability sourceAbility)
        {
            if (sourceAbility == null) return;

            if(effectStartFx != null)
            {
                var fxMain = effectStartFx.main;

                fxMain.playOnAwake = false;

                effectStartFx.Stop();

                if (!effectStartFx.gameObject.activeInHierarchy) effectStartFx.gameObject.SetActive(true);
                
            }

            if(effectUpdateFx != null)
            {
                var fxMain = effectUpdateFx.main;

                fxMain.playOnAwake = false;

                effectUpdateFx.Stop();

                if (!effectUpdateFx.gameObject.activeInHierarchy) effectUpdateFx.gameObject.SetActive(true);
            }
        }

        protected virtual void ProcessEffectPopupForBuffEffects(Sprite popupSprite, string popupText, float buffedNumber = 0.0f, bool allowNeutralPopup = false, float popupTime = 0.0f)
        {
            if (!gameObject.scene.isLoaded) return;

            if (effectStatPopupSpawner == null || 
                !effectStatPopupSpawner.enabled || 
                effectStatPopupSpawner.disablePopup) return;

            //if (buffedNumber == 0.0f) return;

            if (popupTime != 0.0f)
            {
                effectStatPopupSpawner.SetStatPopupSpawnerConfig(0.0f, 0.0f, 0.0f, 0.0f, 0.0f, popupTime);
            }
            
            if (buffedNumber > 0.0f)
            {
                effectStatPopupSpawner.PopUp(popupSprite, popupText, StatPopup.PopUpType.Positive);
            }
            else if(buffedNumber < 0.0f)
            {
                effectStatPopupSpawner.PopUp(popupSprite, popupText, StatPopup.PopUpType.Negative);
            }
            else if(buffedNumber - 0.0f <= Mathf.Epsilon)
            {
                if(allowNeutralPopup) effectStatPopupSpawner.PopUp(popupSprite, popupText, StatPopup.PopUpType.Neutral);
            }
        }

        protected void DetachAndDestroyAllEffectPopupsIncludingSpawner()
        {
            if (effectStatPopupSpawner == null) return;

            if (!gameObject.scene.isLoaded) return;

            effectStatPopupSpawner.DetachAndDestroyAllStatPopupsIncludingSpawner(false);
        }

        public StatPopupSpawner GetAbilityEffectStatPopupSpawner()
        {
            return effectStatPopupSpawner;
        }
    }
}
