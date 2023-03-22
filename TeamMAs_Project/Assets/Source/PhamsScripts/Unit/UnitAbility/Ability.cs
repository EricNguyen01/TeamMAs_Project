using PixelCrushers.DialogueSystem;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using UnityEngine;

namespace TeamMAsTD
{
    /*
     * This is the abstract template class for any ability to derive from (DONT ATTACH IT DIRECTLY TO GAMEOBJECT! ONLY ATTACH INHERITED).
     * The ability states and execution order are determined in this class.
     * Any deriving ability class from this class only needs to deal with and overriding these 3 functions:
     *    - ProcessAbilityStart()
     *    - ProcessAbilityUpdate()
     *    - ProcessAbilityEnd()
     * 
     * To start or stop ability from outside of this class, just call the public functions:
     *    - StartAbility()
     *    - ForceStopAbility()
     */
    [DisallowMultipleComponent]
    public abstract class Ability : MonoBehaviour
    {
        [field: Header("Ability Data")]

        [field: SerializeField]
        [field: DisallowNull]
        public AbilitySO abilityScriptableObject { get; protected set; }

        //INTERNALS...................................................................................

        public IUnit unitPossessingAbility { get; private set; }

        protected UnitSO unitPossessingAbilitySO { get; private set; }

        protected bool isCharging = false;

        private bool isInCooldown = false;

        private bool isUpdating = false;

        private bool isStopped = true;

        private bool abilityLocked = true;

        private bool isAbilityPendingUnlocked = false;

        private WaveSO currentWave;

        public static event System.Action<Ability> OnAbilityStarted;
        public static event System.Action<Ability> OnAbilityStopped;

        protected virtual void Awake()
        {
            unitPossessingAbility = GetComponentInParent<IUnit>();

            if(abilityScriptableObject == null || unitPossessingAbility == null )
            {
                Debug.LogError("Ability SO Data or a proper unit that can possess ability: " + name + " is missing. Disabling ability script!");

                enabled = false;

                return;
            }

            unitPossessingAbilitySO = unitPossessingAbility.GetUnitScriptableObjectData();

            if (unitPossessingAbilitySO == null)
            {
                Debug.LogError("Unit ScriptableObject data of ability: " + name + " is missing. Disabling ability script!");

                enabled = false;

                return;
            }

            abilityLocked = abilityScriptableObject.abilityLocked;

            gameObject.layer = unitPossessingAbility.GetUnitLayerMask();
        }

        protected virtual void OnEnable()
        {
            if (abilityLocked)
            {
                WaveSpawner.OnWaveFinished += PendingUnlockAbilityOnWaveEndedIfApplicable;

                Rain.OnRainEnded += (Rain r) => UnlockAbilityOnRainEndedIfApplicable();
            }
        }

        protected virtual void OnDisable()
        {
            WaveSpawner.OnWaveFinished -= PendingUnlockAbilityOnWaveEndedIfApplicable;

            Rain.OnRainEnded -= (Rain r) => UnlockAbilityOnRainEndedIfApplicable();
        }

        protected virtual void Start()
        {
            if(abilityScriptableObject != null)
            {
                if(abilityScriptableObject.useOnEquippedIfNotLocked && !abilityLocked)
                {
                    StartAbility();
                }
            }
        }

        #region AbilityStart

        private bool CanStartAbility()
        {
            if (abilityLocked) return false;

            if (abilityScriptableObject == null || unitPossessingAbility == null) return false;

            if (isCharging || isInCooldown || isUpdating) return false;

            if (abilityScriptableObject.abilityUseReservedFor == AbilitySO.AbilityUseReservedFor.PlantOnly)
            {
                if (unitPossessingAbility.GetType() == typeof(VisitorUnit)) return false;
            }

            if (abilityScriptableObject.abilityUseReservedFor == AbilitySO.AbilityUseReservedFor.VisitorOnly)
            {
                if (unitPossessingAbility.GetType() == typeof(PlantUnit)) return false;
            }

            return true;
        }

        public void StartAbility()
        {
            if (!CanStartAbility()) return;

            if(abilityScriptableObject.abilityChargeTime <= 0.0f)
            {
                BeginPerformAbility();

                return;
            }

            if(!isCharging) StartCoroutine(AbilityChargeCoroutine(abilityScriptableObject.abilityChargeTime));
        }

        private void BeginPerformAbility()
        {
            isStopped = false;

            //start cooldown right away after begin performing ability
            //only starts if not already in cooldown or ability cdr time > 0.0f
            if (!isInCooldown && abilityScriptableObject.abilityCooldownTime > 0.0f) 
            { 
                StartCoroutine(AbilityCooldownCoroutine(abilityScriptableObject.abilityCooldownTime)); 
            }

            ProcessAbilityStart();

            StartAbilityUpdate();
        }

        //To be edited in inherited classes
        protected abstract void ProcessAbilityStart();

        #endregion

        #region AbilityUpdate

        private void StartAbilityUpdate()
        {
            if (isStopped || isUpdating) return;

            if(abilityScriptableObject.abilityDuration < 0.0f)
            {
                //ability duration is set to < 0.0f meaning that
                //ability will be performed infinitely UNTIL there's an EXTERNAL INSTRUCTION to stop
                return;
            }

            if(abilityScriptableObject.abilityDuration == 0.0f)
            {
                //if ability duration is equal to 0.0f -> ability won't be updated and will be stopped immediately

                StopAbility();

                return;
            }

            StartCoroutine(AbilityUpdateDurationCoroutine(abilityScriptableObject.abilityDuration));
        }

        //To be edited in inherited classes
        protected abstract void ProcessAbilityUpdate();

        #endregion

        #region AbilityStop

        private void StopAbility()
        {
            if(isStopped) return;  

            isStopped = true;

            isUpdating = false;

            StopCoroutine(AbilityChargeCoroutine(abilityScriptableObject.abilityChargeTime));

            StopCoroutine(AbilityUpdateDurationCoroutine(abilityScriptableObject.abilityDuration));

            ProcessAbilityEnd();
        }

        //To be edited in inherited classes
        protected abstract void ProcessAbilityEnd();

        //To call from external scripts to stop/interupt this ability prematurely
        public void ForceStopAbility()
        {
            if(isStopped || abilityLocked) return;

            StopAbility();
        }

        #endregion

        #region AbilityCoroutines

        private IEnumerator AbilityChargeCoroutine(float chargeTime)
        {
            if (isCharging) yield break;

            isCharging = true;

            yield return new WaitForSeconds(chargeTime);

            isCharging = false;

            BeginPerformAbility();

            yield break;
        }

        private IEnumerator AbilityCooldownCoroutine(float cdrTime)
        {
            //if cooldown process alr started -> dont start again and exit coroutine
            if (isInCooldown) yield break;

            isInCooldown = true;

            yield return new WaitForSeconds(cdrTime);

            isInCooldown = false;

            yield break;
        }

        private IEnumerator AbilityUpdateDurationCoroutine(float abilityDuration)
        {
            if(isStopped || isUpdating) yield break;

            isUpdating = true;

            float time = 0.0f;

            while(time < abilityDuration)
            {
                if (isStopped) break;

                time += Time.fixedDeltaTime;

                ProcessAbilityUpdate();

                yield return new WaitForFixedUpdate();
            }

            isUpdating = false;

            StopAbility();

            yield break;
        }

        #endregion

        //This func checks if a unit being targetted by this ability can actually receive it and takes its effects
        protected bool CanTargetUnitReceivesThisAbility(IUnit targetUnit)
        {
            object unitObj = targetUnit.GetUnitObject();

            UnitSO unitSO = targetUnit.GetUnitScriptableObjectData();

            PlantUnitSO plantUnitSO = null;

            VisitorUnitSO visitorUnitSO = null;

            if(unitSO.GetType() == typeof(PlantUnitSO)) plantUnitSO = (PlantUnitSO)unitSO;

            if(unitSO.GetType() == typeof(VisitorUnitSO)) visitorUnitSO = (VisitorUnitSO)unitSO;

            if (unitObj.GetType() == typeof(VisitorUnit))
            {
                VisitorUnit visitorUnit = (VisitorUnit)unitObj;

                if (abilityScriptableObject.abilityOnlyAffect == AbilitySO.AbilityOnlyAffect.PlantOnly) return false;

                if(visitorUnitSO != null)
                {
                    if (abilityScriptableObject.abilityAffectsSpecificVisitorType != VisitorUnitSO.VisitorType.None)
                    {
                        if (abilityScriptableObject.abilityAffectsSpecificVisitorType != visitorUnitSO.visitorType) return false;
                    }

                    if (abilityScriptableObject.abilityAffectsSpecificVisitorUnit != null && abilityScriptableObject.abilityAffectsSpecificVisitorUnit.Count > 0)
                    {
                        for (int i = 0; i < abilityScriptableObject.abilityAffectsSpecificVisitorUnit.Count; i++)
                        {
                            if (abilityScriptableObject.abilityAffectsSpecificVisitorUnit[i] == visitorUnitSO) return true;
                        }
                    }
                }
            }

            if(unitObj.GetType() == typeof(PlantUnit))
            {
                PlantUnit plantUnit = (PlantUnit)unitObj;

                if (abilityScriptableObject.abilityOnlyAffect == AbilitySO.AbilityOnlyAffect.VisitorOnly) return false;

                if(plantUnitSO != null)
                {
                    if (abilityScriptableObject.abilityAffectsSpecificPlantUnit != null && abilityScriptableObject.abilityAffectsSpecificPlantUnit.Count > 0)
                    {
                        for (int i = 0; i < abilityScriptableObject.abilityAffectsSpecificPlantUnit.Count; i++)
                        {
                            if (abilityScriptableObject.abilityAffectsSpecificPlantUnit[i] == plantUnitSO) return true;
                        }
                    }
                }
            }

            return true;
        }

        protected void InvokeOnAbilityStartedEventOn(Ability childAbilityClass)
        {
            OnAbilityStarted?.Invoke(childAbilityClass);
        }

        protected void InvokeOnAbilityStoppedEventOn(Ability childAbilityClass)
        {
            //Debug.Log("AbilityStoppedEventInvoked");

            OnAbilityStopped?.Invoke(childAbilityClass);
        }

        private void PendingUnlockAbilityOnWaveEndedIfApplicable(WaveSpawner waveSpawner, int waveNum, bool hasOngoingWave)
        {
            if (hasOngoingWave) return;

            if (abilityScriptableObject == null) return;

            if (abilityScriptableObject.waveToUnlockAbilityAfterFinished == null) return;

            if (waveSpawner == null) return;

            currentWave = waveSpawner.GetCurrentWave().waveSO;

            if (currentWave == abilityScriptableObject.waveToUnlockAbilityAfterFinished) isAbilityPendingUnlocked = true;
        }

        private void UnlockAbilityOnRainEndedIfApplicable()
        {
            if (abilityScriptableObject == null) return;

            if (abilityScriptableObject.waveToUnlockAbilityAfterFinished == null) return;

            if (!isAbilityPendingUnlocked) return;

            if (!abilityScriptableObject.useOnEquippedIfNotLocked) return;

            if (abilityLocked)
            {
                abilityScriptableObject.SetAbilityLocked(false);

                abilityLocked = false;

                isAbilityPendingUnlocked = false;

                StartAbility();
            }
        }
    }
}
