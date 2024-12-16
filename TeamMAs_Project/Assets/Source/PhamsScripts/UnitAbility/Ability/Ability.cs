// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using PixelCrushers.DialogueSystem;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace TeamMAsTD
{
    /*
     * This is the abstract template class for any ability to derive from (DONT ATTACH IT DIRECTLY TO GAMEOBJECT! ONLY ATTACH INHERITED).
     * The ability states and execution order are determined in this class.
     * Any deriving ability class from this class only needs to deal with and overriding these functions:
     *    - Unity events functions (Awake, Start, Enable, etc.)
     *    - ProcessAbilityStart()
     *    - ProcessAbilityUpdate()
     *    - ProcessAbilityEnd()
     *    - InitializeAuraAbilityEffectsOnAwake()
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

        [Header("Ability Particle Effects")]

        [SerializeField] protected ParticleSystem abilityChargingParticleEffect;

        [SerializeField] protected ParticleSystem abilityParticleEffect;

        [Header("Ability Sounds")]

        [SerializeField] protected FMODUnity.StudioEventEmitter abilityEventEmitterFMOD;

        //INTERNALS...................................................................................

        public IUnit unitPossessingAbility { get; private set; }

        protected UnitSO unitPossessingAbilitySO { get; private set; }

        protected int numberOfUnitsAffected = 0;

        protected bool isCharging = false;

        private float chargeTime = 0.0f;//for debug

        protected bool isInCooldown = false;

        private float cooldownTime = 0.0f;//for debug  

        protected bool isUpdating = false;

        private float updateTime = 0.0f;//for debug

        protected bool isStopped = true;

        private bool abilityLocked = true;

        private bool isAbilityPendingUnlocked = false;

        protected SpriteRenderer visitorUsingAbilitySpriteRenderer;

        protected string visitorUsingAbilitySpriteRendererLayerName;

        protected int visitorUsingAbilitySpriteRendererLayerOrder;

        private WaveSO currentWave;

        //any ability effect created by this Ability and picked up by an AbilityEffectReceivedInventory 
        //will be registered by that AbilityEffectReceivedInventory to this list below
        protected List<AbilityEffect> abilityEffectsCreated = new List<AbilityEffect>();

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

            if (unitPossessingAbility != null)
            {
                if (unitPossessingAbility.GetUnitObject().GetType() == typeof(VisitorUnit))
                {
                    VisitorUnit visitorUnit = (VisitorUnit)unitPossessingAbility.GetUnitObject();

                    visitorUsingAbilitySpriteRenderer = visitorUnit.GetComponent<SpriteRenderer>();
                }
            }

            InitializeAuraAbilityEffectsOnAwake();
        }

        protected virtual void OnEnable()
        {
            if (abilityLocked)
            {
                WaveSpawner.OnWaveFinished += PendingUnlockAbilityOnWaveEndedIfApplicable;

                Rain.OnRainEnded += (Rain r) => UnlockAbilityOnRainEndedIfApplicable();
            }

            if (abilityScriptableObject != null)
            {
                if (abilityScriptableObject.useOnEquippedIfNotLocked && !abilityLocked)
                {
                    StartAbility();
                }
            }
        }

        protected virtual void OnDisable()
        {
            ForceStopAbility();

            numberOfUnitsAffected = 0;

            WaveSpawner.OnWaveFinished -= PendingUnlockAbilityOnWaveEndedIfApplicable;

            Rain.OnRainEnded -= (Rain r) => UnlockAbilityOnRainEndedIfApplicable();
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

            if (!isStopped) return;//if already started (not stopped) dont start again (in case multiple calls to func)

            if (abilityScriptableObject.abilityChargeTime > 0.0f)
            {
                if (!isCharging) StartCoroutine(AbilityChargeCoroutine(abilityScriptableObject.abilityChargeTime));

                return;
            }

            BeginPerformAbility();
        }

        private void BeginPerformAbility()
        {
            isStopped = false;

            //start cooldown right away after begin performing ability
            //only starts cdr if not already in cooldown or ability cdr time > 0.0f
            if (!isInCooldown && abilityScriptableObject.abilityCooldownTime > 0.0f) 
            { 
                StartCoroutine(AbilityCooldownCoroutine(abilityScriptableObject.abilityCooldownTime)); 
            }

            if (abilityParticleEffect != null) abilityParticleEffect.Play();

            ProcessAbilityStart();

            StartAbilityUpdate();
        }

        //To be edited in inherited classes
        protected virtual void ProcessAbilityStart()
        {
            numberOfUnitsAffected = 0;

            OnAbilityStarted?.Invoke(this);
        }

        #endregion

        #region AbilityUpdate

        private void StartAbilityUpdate()
        {
            if (isStopped || isUpdating) return;

            if (abilityScriptableObject.abilityDuration < 0.0f)
            {
                //ability duration is set to < 0.0f meaning that
                //ability will be performed infinitely UNTIL there's an EXTERNAL INSTRUCTION to stop
                return;
            }

            StartCoroutine(AbilityUpdateDurationCoroutine(abilityScriptableObject.abilityDuration));

            /*if (abilityScriptableObject.abilityDuration == 0.0f)
            {
                //if ability duration is equal to 0.0f -> ability won't be updated and will be stopped immediately

                StopAbility();

                return;
            }*/
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

            if (isCharging)
            {
                StopCoroutine(AbilityChargeCoroutine(abilityScriptableObject.abilityChargeTime));

                isCharging = false;

                if (abilityChargingParticleEffect != null) abilityChargingParticleEffect.Stop();
            }

            StopCoroutine(AbilityUpdateDurationCoroutine(abilityScriptableObject.abilityDuration));

            if (abilityParticleEffect != null) abilityParticleEffect.Stop();

            ProcessAbilityEnd();
        }

        //To be edited in inherited classes
        protected virtual void ProcessAbilityEnd()
        {
            OnAbilityStopped?.Invoke(this);
        }

        //To call from external scripts to stop/interupt this ability prematurely
        public void ForceStopAbility()
        {
            if (isCharging)
            {
                StopCoroutine(AbilityChargeCoroutine(abilityScriptableObject.abilityChargeTime));

                isCharging = false;

                if (abilityChargingParticleEffect != null) abilityChargingParticleEffect.Stop();
            }

            if(isStopped || abilityLocked) return;

            StopAbility();
        }

        #endregion

        #region AbilityCoroutines

        protected virtual IEnumerator AbilityChargeCoroutine(float chargeTime)
        {
            if (isCharging) yield break;

            if(abilityChargingParticleEffect != null) abilityChargingParticleEffect.Play();

            float chargeStartTime = Time.realtimeSinceStartup;

            isCharging = true;

            yield return new WaitForSeconds(chargeTime);

            isCharging = false;

            if (abilityChargingParticleEffect != null) abilityChargingParticleEffect.Stop();

            BeginPerformAbility();

            this.chargeTime = Time.realtimeSinceStartup - chargeStartTime;

            yield break;
        }

        private WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();

        private IEnumerator AbilityCooldownCoroutine(float cdrTime)
        {
            //if cooldown process alr started -> dont start again and exit coroutine
            if (isInCooldown) yield break;

            float cdrStartTime = Time.realtimeSinceStartup;

            isInCooldown = true;

            yield return new WaitForSeconds(cdrTime);

            isInCooldown = false;

            this.cooldownTime = Time.realtimeSinceStartup - cdrStartTime;

            if(isStopped && !isUpdating && abilityScriptableObject.autoRestartAbility)
            {
                yield return waitForFixedUpdate;

                StartAbility();
            }

            yield break;
        }

        private IEnumerator AbilityUpdateDurationCoroutine(float abilityDuration)
        {
            if(isStopped || isUpdating) yield break;

            float updateStartTime = Time.realtimeSinceStartup;

            isUpdating = true;

            if (abilityDuration == 0.0f)
            {
                ProcessAbilityUpdate();

                isUpdating = false;

                this.updateTime = Time.realtimeSinceStartup - updateStartTime;

                if (!isStopped) StopAbility();

                if (!isInCooldown)
                {
                    yield return waitForFixedUpdate;

                    StartAbility();
                }

                yield break;
            }

            float time = 0.0f;

            while (time < abilityDuration)
            {
                if (isStopped) break;

                time += Time.fixedDeltaTime;

                ProcessAbilityUpdate();

                yield return waitForFixedUpdate;
            }

            isUpdating = false;

            this.updateTime = time;

            if(!isStopped) StopAbility();

            if (!isInCooldown)
            {
                yield return waitForFixedUpdate;

                StartAbility();
            }

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
                if (abilityScriptableObject.abilityOnlyAffect == AbilitySO.AbilityOnlyAffect.PlantOnly) return false;

                if(visitorUnitSO != null)
                {
                    if (abilityScriptableObject.abilityAffectsSpecificVisitorType != VisitorUnitSO.VisitorType.None)
                    {
                        if (abilityScriptableObject.abilityAffectsSpecificVisitorType != visitorUnitSO.visitorType) return false;
                    }

                    if (abilityScriptableObject.specificVisitorUnitImmuned != null && abilityScriptableObject.specificVisitorUnitImmuned.Count > 0)
                    {
                        for (int i = 0; i < abilityScriptableObject.specificVisitorUnitImmuned.Count; i++)
                        {
                            if (abilityScriptableObject.specificVisitorUnitImmuned[i] == visitorUnitSO ||
                                abilityScriptableObject.specificVisitorUnitImmuned[i].Equals(visitorUnitSO) ||
                                ReferenceEquals(abilityScriptableObject.specificVisitorUnitImmuned[i], visitorUnitSO)) return false;
                        }
                    }

                    if (abilityScriptableObject.abilityAffectsSpecificVisitorUnit != null && abilityScriptableObject.abilityAffectsSpecificVisitorUnit.Count > 0)
                    {
                        for (int i = 0; i < abilityScriptableObject.abilityAffectsSpecificVisitorUnit.Count; i++)
                        {
                            if (abilityScriptableObject.abilityAffectsSpecificVisitorUnit[i] == visitorUnitSO ||
                                abilityScriptableObject.abilityAffectsSpecificVisitorUnit[i].Equals(visitorUnitSO) ||
                                ReferenceEquals(abilityScriptableObject.abilityAffectsSpecificVisitorUnit[i], visitorUnitSO)) return true;
                        }

                        return false;
                    }
                }
            }

            if(unitObj.GetType() == typeof(PlantUnit))
            {
                if (abilityScriptableObject.abilityOnlyAffect == AbilitySO.AbilityOnlyAffect.VisitorOnly) return false;

                if(plantUnitSO != null)
                {
                    if (abilityScriptableObject.specificPlantUnitImmuned != null && abilityScriptableObject.specificPlantUnitImmuned.Count > 0)
                    {
                        for (int i = 0; i < abilityScriptableObject.specificPlantUnitImmuned.Count; i++)
                        {
                            if (abilityScriptableObject.specificPlantUnitImmuned[i] == plantUnitSO ||
                                abilityScriptableObject.specificPlantUnitImmuned[i].Equals(plantUnitSO) ||
                                ReferenceEquals(abilityScriptableObject.specificPlantUnitImmuned[i], plantUnitSO) ||
                                abilityScriptableObject.specificPlantUnitImmuned[i].unitStaticID == plantUnitSO.unitStaticID)
                            {
                                //if (name.Contains("Kid")) Debug.Log("Plant: " + plantUnitSO.name + " is immuned to this ability.");

                                return false;
                            }
                        }
                    }

                    if (abilityScriptableObject.abilityAffectsSpecificPlantUnit != null && abilityScriptableObject.abilityAffectsSpecificPlantUnit.Count > 0)
                    {
                        for (int i = 0; i < abilityScriptableObject.abilityAffectsSpecificPlantUnit.Count; i++)
                        {
                            if (abilityScriptableObject.abilityAffectsSpecificPlantUnit[i] == plantUnitSO ||
                                abilityScriptableObject.abilityAffectsSpecificPlantUnit[i].Equals(plantUnitSO) ||
                                ReferenceEquals(abilityScriptableObject.abilityAffectsSpecificPlantUnit[i], plantUnitSO) ||
                                abilityScriptableObject.abilityAffectsSpecificPlantUnit[i].unitStaticID == plantUnitSO.unitStaticID) return true;
                        }

                        return false;
                    }
                }
            }
            
            return true;
        }

        protected void SetVisitorSortingOrderOnTopOfAbility(bool shouldBeOnTop)
        {
            if (shouldBeOnTop)
            {
                if (visitorUsingAbilitySpriteRenderer != null)
                {
                    visitorUsingAbilitySpriteRendererLayerName = visitorUsingAbilitySpriteRenderer.sortingLayerName;

                    visitorUsingAbilitySpriteRendererLayerOrder = visitorUsingAbilitySpriteRenderer.sortingOrder;

                    if (abilityParticleEffect != null)
                    {
                        ParticleSystemRenderer pRenderer = abilityParticleEffect.GetComponent<ParticleSystemRenderer>();

                        if (pRenderer != null)
                        {
                            visitorUsingAbilitySpriteRenderer.sortingLayerName = pRenderer.sortingLayerName;

                            visitorUsingAbilitySpriteRenderer.sortingOrder = pRenderer.sortingOrder + 5;
                        }
                    }
                }

                return;
            }

            if (visitorUsingAbilitySpriteRenderer != null)
            {
                visitorUsingAbilitySpriteRenderer.sortingLayerName = visitorUsingAbilitySpriteRendererLayerName;

                visitorUsingAbilitySpriteRenderer.sortingOrder = visitorUsingAbilitySpriteRendererLayerOrder;
            }
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

        protected virtual void InitializeAuraAbilityEffectsOnAwake()
        {
            if (abilityChargingParticleEffect != null)
            {
                var auraChargingFxMain = abilityChargingParticleEffect.main;

                auraChargingFxMain.playOnAwake = false;

                abilityChargingParticleEffect.Stop();

                if (!abilityChargingParticleEffect.gameObject.activeInHierarchy) abilityChargingParticleEffect.gameObject.SetActive(true);
            }

            if (abilityParticleEffect != null)
            {
                var auraFxMain = abilityParticleEffect.main;

                auraFxMain.playOnAwake = false;

                abilityParticleEffect.Stop();

                if (!abilityParticleEffect.gameObject.activeInHierarchy) abilityParticleEffect.gameObject.SetActive(true);
            }
        }

        public void TempDisable_SpawnedAbilityEffects_StatPopupSpawners_Except(bool disable,
                                                                               AbilityEffect abilityEffectToExcept = null, 
                                                                               List<AbilityEffect> abilityEffectsToExcept = null)
        {
            if (abilityEffectsCreated == null || abilityEffectsCreated.Count == 0) return;

            for(int i = 0; i < abilityEffectsCreated.Count; i++)
            {
                if (abilityEffectsCreated[i] == null) continue;

                if (abilityEffectToExcept != null && abilityEffectsCreated[i] == abilityEffectToExcept) continue;

                if(abilityEffectsToExcept != null && abilityEffectsToExcept.Count > 0)
                {
                    if (abilityEffectsToExcept.Contains(abilityEffectsCreated[i])) continue;
                }

                StatPopupSpawner eStatPopupSpawner = abilityEffectsCreated[i].GetAbilityEffectStatPopupSpawner();

                if (eStatPopupSpawner == null) continue;

                if (disable) eStatPopupSpawner.disablePopup = true;
                else eStatPopupSpawner.disablePopup = false;
            }
        }

        public void RegisterAbilityEffectCreatedByThisAbility(AbilityEffect abilityEffect)
        {
            if (abilityEffectsCreated == null) return;

            if (abilityScriptableObject.abilityEffects == null || abilityScriptableObject.abilityEffects.Count == 0) return;

            if (abilityEffect.abilityEffectSO == null) return;

            if (!abilityScriptableObject.abilityEffects.Contains(abilityEffect.abilityEffectSO)) return;

            abilityEffectsCreated.Add(abilityEffect);
        }

        public void DeRegisterAbilityEffectCreatedByThisAbility(AbilityEffect abilityEffect)
        {
            if (abilityEffectsCreated == null) return;

            if (abilityScriptableObject.abilityEffects == null || abilityScriptableObject.abilityEffects.Count == 0) return;

            if (abilityEffect.abilityEffectSO == null) return;

            if (!abilityScriptableObject.abilityEffects.Contains(abilityEffect.abilityEffectSO)) return;

            if (!abilityEffectsCreated.Contains(abilityEffect)) return;

            abilityEffectsCreated.Remove(abilityEffect);
        }
    }
}
