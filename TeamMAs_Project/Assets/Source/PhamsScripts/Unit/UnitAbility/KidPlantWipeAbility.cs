using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamMAsTD
{
    public class KidPlantWipeAbility : AuraAbility
    {
        protected override void OnEnable()
        {
            base.OnEnable();

            auraRange = 1.0f;

            auraCollider.radius = auraRange;
        }

        protected override void ProcessAbilityStart()
        {
            //stop visitor carrying this ability from moving upon ability starts/updates
            //also makes visitor temporary invincible
            if(unitPossessingAbility != null)
            {
                if(unitPossessingAbility.GetUnitObject().GetType() == typeof(VisitorUnit))
                {
                    VisitorUnit visitorUnit = (VisitorUnit)unitPossessingAbility.GetUnitObject();

                    visitorUnit.SetVisitorInvincible(true);

                    visitorUnit.SetVisitorFollowingPath(false);
                }
            }

            base.ProcessAbilityStart();
        }

        protected override void ProcessAbilityUpdate()
        {
            //lerp expand the wipe aura collider ovetime here:

        }

        protected override void ProcessAbilityEnd()
        {
            //resume visitor movement on this ability ended and disable visitor invincibility
            if (unitPossessingAbility != null)
            {
                if (unitPossessingAbility.GetUnitObject().GetType() == typeof(VisitorUnit))
                {
                    VisitorUnit visitorUnit = (VisitorUnit)unitPossessingAbility.GetUnitObject();

                    visitorUnit.SetVisitorInvincible(false);

                    visitorUnit.SetVisitorFollowingPath(true);
                }
            }

            base.ProcessAbilityEnd();
        }

        protected override void OnTriggerEnter2D(Collider2D other)
        {
            ProcessAuraTriggerEnterStay(other);
        }

        protected override void OnTriggerStay2D(Collider2D other)
        {
            //do not call base function here
            //we want this function to do nothing for this specific ability
        }

        protected override void OnTriggerExit2D(Collider2D other)
        {
            //do not call base function here
            //we want this function to do nothing for this specific ability
        }
    }
}
