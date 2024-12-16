// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace TeamMAsTD
{
    [DisallowMultipleComponent]
    public class AuraAbility : Ability
    {
        [Header("Required Components")]

        [SerializeField] protected CircleCollider2D auraCollider;

        [SerializeField] protected Rigidbody2D auraKinematicRb;

        protected float auraRange = 0.0f;

        protected bool canCheckForUnitsInAura = true;

        //protected bool triggerExitEventCheck = true;

        private bool enableTempDisableAuraTriggerCoroutine = true;

        private List<AbilityEffectReceivedInventory> DEBUG_effectReceivedInventoriesInAura = new List<AbilityEffectReceivedInventory>();   

        protected override void Awake()
        {
            base.Awake();

            if(auraCollider == null)
            {
                auraCollider = gameObject.AddComponent<CircleCollider2D>();
            }

            auraCollider.isTrigger = true;

            auraCollider.enabled = false;

            if(auraKinematicRb == null)
            {
                auraKinematicRb = gameObject.AddComponent<Rigidbody2D>();
            }

            auraKinematicRb.gravityScale = 0.0f;

            auraKinematicRb.isKinematic = true;

            auraKinematicRb.collisionDetectionMode = CollisionDetectionMode2D.Discrete;

            auraKinematicRb.sleepMode = RigidbodySleepMode2D.StartAsleep;
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            auraRange = abilityScriptableObject.abilityRange;

            auraCollider.radius = auraRange;

            if (unitPossessingAbility != null && unitPossessingAbility.GetType() == typeof(PlantUnit))
            {
                PlantUnit plant = (PlantUnit)unitPossessingAbility;

                plant.tilePlacedOn.tileMenuAndUprootOnTileUI.OnTileMenuOpened.AddListener(ShowConnectingLineToBuffedTargets);

                plant.tilePlacedOn.tileMenuAndUprootOnTileUI.OnTileMenuClosed.AddListener(HideConnectingLineToBuffedTargets);
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (unitPossessingAbility != null && unitPossessingAbility.GetType() == typeof(PlantUnit))
            {
                PlantUnit plant = (PlantUnit)unitPossessingAbility;

                plant.tilePlacedOn.tileMenuAndUprootOnTileUI.OnTileMenuOpened.RemoveListener(ShowConnectingLineToBuffedTargets);

                plant.tilePlacedOn.tileMenuAndUprootOnTileUI.OnTileMenuClosed.RemoveListener(HideConnectingLineToBuffedTargets);
            }
        }

        protected override void ProcessAbilityStart()
        {
            canCheckForUnitsInAura = true;

            //triggerExitEventCheck = true;

            auraCollider.enabled = true;

            auraKinematicRb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            auraKinematicRb.useFullKinematicContacts = true;

            auraKinematicRb.sleepMode = RigidbodySleepMode2D.NeverSleep;

            base.ProcessAbilityStart();
        }

        protected override void ProcessAbilityUpdate()
        {
            //nothing to update. Aura logics are handled in 
            //OnCollision events below
        }

        protected override void ProcessAbilityEnd()
        {
            StopCoroutine(TemporaryDisableAuraTriggerCollisionEvent(0.3f));

            canCheckForUnitsInAura = false;

            //triggerExitEventCheck = false;

            auraCollider.enabled = false;

            auraKinematicRb.collisionDetectionMode = CollisionDetectionMode2D.Discrete;

            auraKinematicRb.sleepMode = RigidbodySleepMode2D.StartAsleep;

            DEBUG_effectReceivedInventoriesInAura.Clear();

            base.ProcessAbilityEnd();
        }

        //Aura Collision events............................................................................

        protected virtual void OnTriggerEnter2D(Collider2D other)
        {

        }

        protected virtual void OnTriggerStay2D(Collider2D other)
        {
            ProcessAuraTriggerEnterStay(other);

            if (enableTempDisableAuraTriggerCoroutine) StartCoroutine(TemporaryDisableAuraTriggerCollisionEvent(0.1f));
        }

        protected virtual void OnTriggerExit2D(Collider2D other)
        {
            ProcessAuraTriggerExit(other);
        }

        protected void ProcessAuraTriggerEnterStay(Collider2D other)
        {
            if (!canCheckForUnitsInAura) return;

            if (other == null || !other.gameObject.activeInHierarchy) return;

            //if(other != null) Debug.Log("A collider is in aura of aura ability of : " + transform.parent.gameObject.name);

            IUnit unitInAura = CheckValidUnitAndAuraCollision(other);

            if (unitInAura == null) return;

            //Debug.Log("A unit is in aura of aura ability of : " + transform.parent.gameObject.name);

            AbilityEffectReceivedInventory abilityEffectReceivedInventory = unitInAura.GetAbilityEffectReceivedInventory();

            if (abilityEffectReceivedInventory == null || !abilityEffectReceivedInventory.enabled) return;

            //if the unit in aura being checked is a new one (its ability effect received inventory not exists in DEBUG list)
            if (!DEBUG_effectReceivedInventoriesInAura.Contains(abilityEffectReceivedInventory))
            {
                //check if can still affect this unit (maxNumberOfUnitsToAffect = infinite || within max number of units affected)
                if (abilityScriptableObject.maxNumberOfUnitsToAffect == 0 ||
                   numberOfUnitsAffected < abilityScriptableObject.maxNumberOfUnitsToAffect)
                {
                    //if can still affect, do the below:

                    DEBUG_effectReceivedInventoriesInAura.Add(abilityEffectReceivedInventory);

                    abilityEffectReceivedInventory.ReceivedEffectsFromAbility(this);

                    numberOfUnitsAffected++;
                }
            }
            //else if this unit in aura being checked has been checked before
            else
            {
                abilityEffectReceivedInventory.ReceivedEffectsFromAbility(this);
            }
        }

        protected void ProcessAuraTriggerExit(Collider2D other)
        {
            //if (!triggerExitEventCheck) return;

            if (other == null || !other.gameObject.activeInHierarchy) return;

            IUnit unitLeavesAura = CheckValidUnitAndAuraCollision(other);

            if (unitLeavesAura == null) return;

            AbilityEffectReceivedInventory abilityEffectReceivedInventory = unitLeavesAura.GetAbilityEffectReceivedInventory();

            if (abilityEffectReceivedInventory == null) return;

            if (DEBUG_effectReceivedInventoriesInAura.Contains(abilityEffectReceivedInventory))
            {
                DEBUG_effectReceivedInventoriesInAura.Remove(abilityEffectReceivedInventory);

                abilityEffectReceivedInventory.RemoveEffectsOfAbility(this);

                numberOfUnitsAffected--;
            }
        }

        protected IUnit CheckValidUnitAndAuraCollision(Collider2D otherCollider2D)
        {
            if (otherCollider2D == null) return null;

            IUnit unitDetected = otherCollider2D.GetComponent<IUnit>();

            if (unitDetected == null) return null;

            if (!CanTargetUnitReceivesThisAbility(unitDetected)) return null;

            return unitDetected;
        }

        protected void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;

            if(auraCollider != null) Gizmos.DrawWireSphere(transform.position, auraCollider.radius);
        }

        private WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();

        //For optimization purposes - we dont want the trigger collision check event to check on every objs and loop through everything 
        //every frame.
        private IEnumerator TemporaryDisableAuraTriggerCollisionEvent(float tempDisableTimeSecs)
        {
            if (enableTempDisableAuraTriggerCoroutine) enableTempDisableAuraTriggerCoroutine = false;

            //Debug.Log("Temp aura stays on started: " + Time.realtimeSinceStartup);

            yield return waitForFixedUpdate;

            //Debug.Log("Temp aura stays on stopped: " + Time.realtimeSinceStartup);

            //Debug.Log("Temp disable aura proccess started: " + Time.realtimeSinceStartup);

            if(canCheckForUnitsInAura) canCheckForUnitsInAura = false;

            yield return new WaitForSeconds(tempDisableTimeSecs);

            //Debug.Log("Temp disable aura proccess stopped: " + Time.realtimeSinceStartup);

            canCheckForUnitsInAura = true;

            enableTempDisableAuraTriggerCoroutine = true;

            yield break;
        }

        private void ShowConnectingLineToBuffedTargets()
        {
            if (abilityEffectsCreated == null || abilityEffectsCreated.Count == 0) return;

            for (int i = 0; i < abilityEffectsCreated.Count; i++)
            {
                if (abilityEffectsCreated[i] == null) continue;

                if (abilityEffectsCreated[i].GetType() != typeof(PlantBuffAbilityEffect)) continue;

                if (abilityEffectsCreated[i].unitBeingAffected == unitPossessingAbility) continue;

                PlantBuffAbilityEffect plantBuffEffect = (PlantBuffAbilityEffect)abilityEffectsCreated[i];

                plantBuffEffect.ActivateBuffConnectingLine();
            }
        }

        private void HideConnectingLineToBuffedTargets()
        {
            if (abilityEffectsCreated == null || abilityEffectsCreated.Count == 0) return;

            for (int i = 0; i < abilityEffectsCreated.Count; i++)
            {
                if (abilityEffectsCreated[i] == null) continue;

                if (abilityEffectsCreated[i].GetType() != typeof(PlantBuffAbilityEffect)) continue;

                if (abilityEffectsCreated[i].unitBeingAffected == unitPossessingAbility) continue;

                PlantBuffAbilityEffect plantBuffEffect = (PlantBuffAbilityEffect)abilityEffectsCreated[i];

                plantBuffEffect.DeactivateBuffConnectingLine();
            }
        }
    }
}
