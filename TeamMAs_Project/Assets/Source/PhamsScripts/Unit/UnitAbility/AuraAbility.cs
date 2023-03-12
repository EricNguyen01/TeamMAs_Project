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

        private float auraRange = 0.0f;

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

            if(auraKinematicRb == null)
            {
                auraKinematicRb = gameObject.AddComponent<Rigidbody2D>();
            }

            auraKinematicRb.gravityScale = 0.0f;

            auraKinematicRb.isKinematic = true;
        }

        protected override void OnEnable()
        {
            SetAuraRange();

            if(auraCollider.radius != auraRange) auraCollider.radius = auraRange;
        }

        protected override void OnDisable()
        {
            //Call from Ability base class to stop this aura ability on obj disabled or destroyed
            ForceStopAbility();
        }

        //this unity update is belong to this class only and has nothing to do with base class' update functions
        private void Update()
        {
            if (timeToCheckForUnitsInAura <= 0.0f) return;

            if(currentTimeToCheckForUnitsInAura <= 0.0f)
            {
                if (!auraCollider.enabled) auraCollider.enabled = true;

                currentTimeToCheckForUnitsInAura = timeToCheckForUnitsInAura;

                return;
            }

            if(currentTimeToCheckForUnitsInAura > 0.0f)
            {
                if (auraCollider.enabled) auraCollider.enabled = false;

                currentTimeToCheckForUnitsInAura -= Time.fixedDeltaTime;
            }
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
            auraCollider.enabled = false;

            InvokeOnAbilityStoppedEventOn(this);
        }

        //Aura Collision events............................................................................

        protected virtual void OnCollisionStay2D(Collision2D collision)
        {
            if (currentTimeToCheckForUnitsInAura > 0.0f) return;

            IUnit unitInAura = ChecValidUnitAndAuraCollision(collision);

            if (unitInAura == null) return;

            AbilityEffectReceivedInventory abilityEffectReceivedInventory = unitInAura.GetAbilityEffectReceivedInventory();

            if (abilityEffectReceivedInventory == null) return;

            abilityEffectReceivedInventory.ReceivedEffectsFromAbility(this);
        }

        protected virtual void OnCollisionExit2D(Collision2D collision)
        {
            IUnit unitLeavesAura = ChecValidUnitAndAuraCollision(collision);

            if(unitLeavesAura == null) return;

            AbilityEffectReceivedInventory abilityEffectReceivedInventory = unitLeavesAura.GetAbilityEffectReceivedInventory();

            if (abilityEffectReceivedInventory == null) return;

            abilityEffectReceivedInventory.RemoveEffectsFromAbility(this);
        }

        protected IUnit ChecValidUnitAndAuraCollision(Collision2D collision)
        {
            if (collision == null) return null;

            IUnit unitDetected = collision.gameObject.GetComponent<IUnit>();

            if (unitDetected == null) return null;

            if (!CanTargetUnitReceivesThisAbility(unitDetected)) return null;

            return unitDetected;
        }

        //Privates and other funcs belong to this class and its children......................................
        private void SetAuraRange()
        {
            if (unitPossessingAbility == null) return;

            if (abilityScriptableObject.abilityRangeInTiles <= 0.0f)
            {
                auraRange = 0.0f;

                return;
            }

            Tile tileUnitIsOn = unitPossessingAbility.GetTileUnitIsOn();

            if (tileUnitIsOn != null && tileUnitIsOn.gridParent != null)
            {
                auraRange = tileUnitIsOn.gridParent.GetDistanceFromTileNumber(abilityScriptableObject.abilityRangeInTiles);
            }
            else
            {
                auraRange = abilityScriptableObject.abilityRangeInTiles;
            }
        }
    }
}
