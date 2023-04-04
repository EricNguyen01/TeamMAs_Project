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

            //DisablePlantUnitAndItsAbilities(plantUnitToWipe);

            tilePlantUnitToWipeOn = plantUnitToWipe.tilePlacedOn;

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

            AbilityEffectReceivedInventory plantAbilityEffectReceivedInventory = plantUnitToWipe.GetAbilityEffectReceivedInventory();

            if(plantAbilityEffectReceivedInventory.abilityEffectsReceived != null && 
               plantAbilityEffectReceivedInventory.abilityEffectsReceived.Count > 0)
            {
                for (int i = 0; i < plantAbilityEffectReceivedInventory.abilityEffectsReceived.Count; i++)
                {
                    if (plantAbilityEffectReceivedInventory.abilityEffectsReceived[i].effectStackSpawned != null &&
                        plantAbilityEffectReceivedInventory.abilityEffectsReceived[i].effectStackSpawned.Count > 0)
                    {
                        for(int j = 0; j < plantAbilityEffectReceivedInventory.abilityEffectsReceived[i].effectStackSpawned.Count; j++)
                        {
                            if (plantAbilityEffectReceivedInventory.abilityEffectsReceived[i].effectStackSpawned[j] == null) continue;

                            if (plantAbilityEffectReceivedInventory.abilityEffectsReceived[i].effectStackSpawned[j].GetAbilityEffectStatPopupSpawner() == null) continue;

                            plantAbilityEffectReceivedInventory.abilityEffectsReceived[i].effectStackSpawned[j].GetAbilityEffectStatPopupSpawner().enabled = false;
                        }
                    }
                }
            }

            plantAbilityEffectReceivedInventory.enabled = false;

            if (tilePlantUnitToWipeOn != null)
            {
                //if is pending destroy plant = false meaning that OnEffectEnded is called prematurely due to 
                //source wipe ability being stopped early (e.g kid is defeated) => thus, wipe won't take effect and plants are safe.
                //else if is pending destroy plant = true meaning that OnEffectEnded is called after effect has finished 
                //plants will be wiped!
                tilePlantUnitToWipeOn.UprootUnit(0.1f);
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
