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

        [SerializeField]
        [Min(0.0f)]
        [Tooltip("The wait time between each aura collider activation to check for any unit within it." +
        "If set to 0.0f meaning that units in aura are checked every fixed update.")]
        protected float timeToCheckForUnitsInAura = 0.1f;

        protected float currentTimeToCheckForUnitsInAura = 0.0f;

        protected float timeBeforeDisablingAuraCollider = 0.1f;

        protected float currentTimeBeforeDisablingAuraCollider = 0.1f;

        private float auraRange = 0.0f;

        protected bool checkingForUnitInAura = true;

        protected bool triggerExitEventCheck = true;

        private List<AbilityEffectReceivedInventory> DEBUG_effectReceivedInventoriesInAura = new List<AbilityEffectReceivedInventory>();   

        protected override void Awake()
        {
            base.Awake();

            if(unitPossessingAbility != null )
            {
                if(unitPossessingAbility.GetUnitObject().GetType() == typeof(PlantUnit))
                {
                    if (timeToCheckForUnitsInAura <= 0.0f) timeToCheckForUnitsInAura = 0.1f;
                }
            }

            if(auraCollider == null)
            {
                auraCollider = gameObject.AddComponent<CircleCollider2D>();
            }

            auraCollider.isTrigger = true;

            if(auraKinematicRb == null)
            {
                auraKinematicRb = gameObject.AddComponent<Rigidbody2D>();
            }

            auraKinematicRb.gravityScale = 0.0f;

            auraKinematicRb.isKinematic = true;
        }

        protected override void OnEnable()
        {
            auraRange = abilityScriptableObject.abilityRangeInTiles;

            if(auraCollider.radius != auraRange) auraCollider.radius = auraRange;
        }

        protected override void OnDisable()
        {
            //Call from Ability base class to stop this aura ability on obj disabled or destroyed
            ForceStopAbility();
        }

        /*private void LateUpdate()
        {
            if (timeToCheckForUnitsInAura <= 0.0f) return;

            if(currentTimeToCheckForUnitsInAura > 0.0f && currentTimeBeforeDisablingAuraCollider <= 0.0f)
            {
                currentTimeToCheckForUnitsInAura -= Time.fixedDeltaTime;

                if(currentTimeToCheckForUnitsInAura <= 0.0f)
                {
                    currentTimeToCheckForUnitsInAura = 0.0f;

                    if (!checkingForUnitInAura) checkingForUnitInAura = true;

                    currentTimeBeforeDisablingAuraCollider = timeBeforeDisablingAuraCollider;
                }

                return;
            }

            if(currentTimeBeforeDisablingAuraCollider > 0.0f && currentTimeToCheckForUnitsInAura <= 0.0f)
            {
                currentTimeBeforeDisablingAuraCollider -= Time.fixedDeltaTime;

                if(currentTimeBeforeDisablingAuraCollider <= 0.0f)
                {
                    currentTimeBeforeDisablingAuraCollider = 0.0f;

                    if (checkingForUnitInAura) checkingForUnitInAura = false;

                    currentTimeToCheckForUnitsInAura = timeToCheckForUnitsInAura;
                }

                return;
            }
        }*/

        protected override void ProcessAbilityStart()
        {
            auraCollider.enabled = true;

            checkingForUnitInAura = true;

            InvokeOnAbilityStartedEventOn(this);
        }

        protected override void ProcessAbilityUpdate()
        {
            //nothing to update. Aura logics are handled in 
            //OnCollision events below
        }

        protected override void ProcessAbilityEnd()
        {
            //auraCollider.enabled = false;

            checkingForUnitInAura = false;

            triggerExitEventCheck = false;

            InvokeOnAbilityStoppedEventOn(this);
        }

        //Aura Collision events............................................................................

        protected virtual void OnTriggerStay2D(Collider2D other)
        {
            if (!checkingForUnitInAura) return;

            if (other == null || !other.gameObject.activeInHierarchy) return;

            //if(other != null) Debug.Log("A collider is in aura of aura ability of : " + transform.parent.gameObject.name);

            IUnit unitInAura = CheckValidUnitAndAuraCollision(other);

            if (unitInAura == null) return;

            //Debug.Log("A unit is in aura of aura ability of : " + transform.parent.gameObject.name);

            AbilityEffectReceivedInventory abilityEffectReceivedInventory = unitInAura.GetAbilityEffectReceivedInventory();

            if (abilityEffectReceivedInventory == null) return;

            if(!DEBUG_effectReceivedInventoriesInAura.Contains(abilityEffectReceivedInventory)) DEBUG_effectReceivedInventoriesInAura.Add(abilityEffectReceivedInventory);

            abilityEffectReceivedInventory.ReceivedEffectsFromAbility(this);
        }

        protected virtual void OnTriggerExit2D(Collider2D other)
        {
            if (!triggerExitEventCheck) return;

            if (other == null || !other.gameObject.activeInHierarchy) return;

            IUnit unitLeavesAura = CheckValidUnitAndAuraCollision(other);

            if(unitLeavesAura == null) return;

            AbilityEffectReceivedInventory abilityEffectReceivedInventory = unitLeavesAura.GetAbilityEffectReceivedInventory();

            if (abilityEffectReceivedInventory == null) return;

            abilityEffectReceivedInventory.RemoveEffectsOfAbility(this);
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
    }
}
