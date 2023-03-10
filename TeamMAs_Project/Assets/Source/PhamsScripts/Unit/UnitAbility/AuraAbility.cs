using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace TeamMAsTD
{
    [DisallowMultipleComponent]
    public class AuraAbility : Ability
    {
        [Header("Required Components")]

        [SerializeField] protected CircleCollider2D auraCollider;

        [SerializeField] protected Rigidbody2D auraKinematicRb;

        private List<IUnit> unitsInAura = new List<IUnit>();

        private float auraRange = 0.0f;

        protected override void Awake()
        {
            base.Awake();

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

        protected override void ProcessAbilityStart()
        {
            auraCollider.enabled = true;
        }

        protected override void ProcessAbilityUpdate()
        {
            //nothing to update. Aura logics are handled in 
            //OnCollision events below
        }

        protected override void ProcessAbilityEnd()
        {
            auraCollider.enabled = false;

            //TODO: process un-buff for all unit elements in "unitsInAura" here

        }

        //Aura Collision events............................................................................

        protected virtual void OnCollisionEnter2D(Collision2D collision)
        {
            
        }

        protected virtual void OnCollisionExit2D(Collision2D collision)
        {

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

        protected void OrderUnitsInAuraByDist()
        {
            if(unitsInAura == null || unitsInAura.Count == 0) return;

            unitsInAura = unitsInAura.OrderBy(x => Vector2.Distance(transform.position, x.GetUnitTransform().position)).ToList();
        }
    }
}
