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

        private float auraRange = 0.0f;

        protected bool triggerExitEventCheck = true;

        private List<AbilityEffectReceivedInventory> DEBUG_effectReceivedInventoriesInAura = new List<AbilityEffectReceivedInventory>();   

        protected override void Awake()
        {
            base.Awake();

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

            auraKinematicRb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            auraKinematicRb.sleepMode = RigidbodySleepMode2D.NeverSleep;
        }

        protected override void OnEnable()
        {
            auraRange = (float)abilityScriptableObject.abilityRangeInTiles - 0.1f;

            if(auraCollider.radius != auraRange) auraCollider.radius = auraRange;
        }

        protected override void OnDisable()
        {
            //Call from Ability base class to stop this aura ability on obj disabled or destroyed
            ForceStopAbility();
        }

        protected override void ProcessAbilityStart()
        {
            auraCollider.enabled = true;

            InvokeOnAbilityStartedEventOn(this);
        }

        protected override void ProcessAbilityUpdate()
        {
            //nothing to update. Aura logics are handled in 
            //OnCollision events below
        }

        protected override void ProcessAbilityEnd()
        {
            triggerExitEventCheck = false;

            InvokeOnAbilityStoppedEventOn(this);
        }

        //Aura Collision events............................................................................

        protected virtual void OnTriggerStay2D(Collider2D other)
        {
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
