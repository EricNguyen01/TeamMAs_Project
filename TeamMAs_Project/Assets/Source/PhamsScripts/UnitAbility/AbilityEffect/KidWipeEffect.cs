using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamMAsTD
{
    [DisallowMultipleComponent]
    public class KidWipeEffect : AbilityEffect
    {
        protected PlantUnit plantUnitToWipe;

        protected Tile tilePlantUnitToWipeOn;

        protected override void OnEffectStarted()
        {
            if(unitBeingAffected.GetUnitObject().GetType() != typeof(PlantUnit))
            {
                Debug.LogError("The unit: " + name + " being affected by this wipe effect: " + name + " " +
                " IS NOT of type PlantUnit. Plant wipe effect won't work!\n" +
                "Destroying plant wipe effect!");

                DestroyEffectWithEffectEndedInvoked(false);

                return;
            }

            plantUnitToWipe = (PlantUnit)unitBeingAffected.GetUnitObject();

            //The chunk below finds and disables all ability effect stat popup components in all effects that are currently affecting this plant
            //since this plant abt to be wiped out, we have no need to display any effect stat popups anymore

            AbilityEffectReceivedInventory plantAbilityEffectReceivedInventory = plantUnitToWipe.GetAbilityEffectReceivedInventory();

            if (plantAbilityEffectReceivedInventory.abilityEffectsReceived != null &&
               plantAbilityEffectReceivedInventory.abilityEffectsReceived.Count > 0)
            {
                for (int i = 0; i < plantAbilityEffectReceivedInventory.abilityEffectsReceived.Count; i++)
                {
                    if (plantAbilityEffectReceivedInventory.abilityEffectsReceived[i].effectStackSpawned != null &&
                        plantAbilityEffectReceivedInventory.abilityEffectsReceived[i].effectStackSpawned.Count > 0)
                    {
                        for (int j = 0; j < plantAbilityEffectReceivedInventory.abilityEffectsReceived[i].effectStackSpawned.Count; j++)
                        {

                            AbilityEffect aEffect = plantAbilityEffectReceivedInventory.abilityEffectsReceived[i].effectStackSpawned[j];

                            if (aEffect == null) continue;

                            if (aEffect == this) continue;

                            //if (plantUnitToWipe.tilePlacedOn.name.Contains("3.2")) Debug.Log("EffectFound!");

                            if (aEffect.GetAbilityEffectStatPopupSpawner() != null)
                            {
                                //if (plantUnitToWipe.tilePlacedOn.name.Contains("3.2")) Debug.Log("StatPopupSpawnerFound!");

                                aEffect.GetAbilityEffectStatPopupSpawner().disablePopup = true;

                                aEffect.GetAbilityEffectStatPopupSpawner().enabled = false;
                            }
                        }
                    }
                }
            }

            //actually disables plant aim shoot and plant abilities which would also trigger effect stat popups which we have disabled above.
            DisablePlantUnitAndItsAbilities(plantUnitToWipe);

            //get tile placed on
            tilePlantUnitToWipeOn = plantUnitToWipe.tilePlacedOn;

            //process FXs
            if(effectStartFx != null) effectStartFx.Play();

            if(effectUpdateFx != null) effectUpdateFx.Play();
        }

        protected override void EffectUpdate()
        {

        }

        protected override void OnEffectEnded()
        {
            if (effectStartFx != null && effectStartFx.isEmitting) effectStartFx.Stop();

            if (effectUpdateFx != null && effectUpdateFx.isEmitting) effectUpdateFx.Stop();

            //wipes plant off the map
            if (tilePlantUnitToWipeOn != null)
            {
                tilePlantUnitToWipeOn.UprootUnit(0.1f);
            }
            else
            {
                Destroy(plantUnitToWipe);
            }
        }

        protected void DisablePlantUnitAndItsAbilities(PlantUnit plantUnit)
        {
            if (plantUnit == null) return;

            if (plantUnit.plantAimShootSystem != null)
            {
                plantUnit.plantAimShootSystem.EnablePlantAimShoot(false);
            }

            foreach (Ability ability in plantUnit.GetComponentsInChildren<Ability>())
            {
                if (ability == null) continue;

                ability.ForceStopAbility();
            }
        }
    }
}
