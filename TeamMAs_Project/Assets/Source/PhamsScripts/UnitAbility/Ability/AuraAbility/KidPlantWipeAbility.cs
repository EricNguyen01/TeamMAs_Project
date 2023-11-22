// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamMAsTD
{
    [DisallowMultipleComponent]
    public class KidPlantWipeAbility : AuraAbility
    {
        private float currentWipeAuraExpandTime = 0.0f;

        private CartoonFX.CFXR_Effect camShakeScript;

        protected override void OnEnable()
        {
            base.OnEnable();

            auraRange = 0.3f;

            auraCollider.radius = auraRange;

            if(abilityParticleEffect != null)
            {
                camShakeScript = abilityParticleEffect.GetComponent<CartoonFX.CFXR_Effect>();

                if (camShakeScript != null) camShakeScript.enabled = false;
            }
        }

        protected override void ProcessAbilityStart()
        {
            currentWipeAuraExpandTime = 0.0f;

            auraRange = 0.5f;

            auraCollider.radius = auraRange;

            //stop visitor carrying this ability from moving upon ability starts/updates
            //also makes visitor temporary invincible
            if (unitPossessingAbility != null)
            {
                if(unitPossessingAbility.GetUnitObject().GetType() == typeof(VisitorUnit))
                {
                    VisitorUnit visitorUnit = (VisitorUnit)unitPossessingAbility.GetUnitObject();

                    visitorUnit.SetVisitorInvincible(true);

                    visitorUnit.SetVisitorFollowingPath(false);

                    SetVisitorSortingOrderOnTopOfAbility(true);
                }
            }

            if (camShakeScript != null) camShakeScript.enabled = true;

            if (abilityEventEmitterFMOD != null) abilityEventEmitterFMOD.Play();

            base.ProcessAbilityStart();
        }

        protected override void ProcessAbilityUpdate()
        {
            if (currentWipeAuraExpandTime < abilityScriptableObject.abilityDuration)
            {
                //lerp expand the wipe aura collider ovetime here:
                auraCollider.radius = Mathf.Lerp(auraRange,
                                                 abilityScriptableObject.abilityRangeInTiles,
                                                 currentWipeAuraExpandTime / abilityScriptableObject.abilityDuration);

                currentWipeAuraExpandTime += Time.fixedDeltaTime;

                if (currentWipeAuraExpandTime >= abilityScriptableObject.abilityDuration)
                {
                    currentWipeAuraExpandTime = abilityScriptableObject.abilityDuration;

                    auraRange = abilityScriptableObject.abilityRangeInTiles;

                    auraCollider.radius = auraRange;
                }
            }
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

                    SetVisitorSortingOrderOnTopOfAbility(false);
                }
            }

            if (abilityEventEmitterFMOD != null && abilityEventEmitterFMOD.IsPlaying()) abilityEventEmitterFMOD.Stop();

            base.ProcessAbilityEnd();

            LogMemoryUsageOnPlantWipeAbilityEnded();
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

        private void LogMemoryUsageOnPlantWipeAbilityEnded()
        {
            float abilityDuration = abilityScriptableObject.abilityDuration;

            float abilityEffectsDuration = 0.0f;

            float totalDuration;

            if (abilityScriptableObject.abilityEffects != null && abilityScriptableObject.abilityEffects.Count > 0)
            {
                for (int i = 0; i < abilityScriptableObject.abilityEffects.Count; i++)
                {
                    if (!abilityScriptableObject.abilityEffects[i]) continue;

                    abilityEffectsDuration += abilityScriptableObject.abilityEffects[i].effectDuration;
                }
            }

            totalDuration = abilityDuration + abilityEffectsDuration;

            BuildMemoryUsageLogger.LogMemoryUsageDelay(totalDuration, "KidPlantWipeAbilityFinished");
        }
    }
}
