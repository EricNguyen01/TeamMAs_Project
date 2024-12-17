// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

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

        protected AbilityEffectReceivedInventory plantAbilityEffectReceivedInventory;

        protected override bool OnEffectStarted()
        {
            if(unitBeingAffected.GetUnitObject().GetType() != typeof(PlantUnit))
            {
                Debug.LogError("The unit: " + name + " being affected by this wipe effect: " + name + " " +
                " IS NOT of type PlantUnit. Plant wipe effect won't work!\n" +
                "Destroying plant wipe effect!");

                DestroyEffectWithEffectEndedInvoked(false);

                return false;
            }

            plantUnitToWipe = (PlantUnit)unitBeingAffected.GetUnitObject();

            //The chunk below finds and disables all ability effect stat popup components in all effects that are currently affecting this plant
            //since this plant abt to be wiped out, we have no need to display any effect stat popups anymore

            plantAbilityEffectReceivedInventory = plantUnitToWipe.GetAbilityEffectReceivedInventory();

            DestroyEffectStatPopupSpawnersOfEffectsOnPlant();

            DisablePlantUnitAndItsAbilities(plantUnitToWipe);

            //get tile placed on
            tilePlantUnitToWipeOn = plantUnitToWipe.tilePlacedOn;

            //process FXs
            if(effectStartFx != null) effectStartFx.Play();

            if(effectUpdateFx != null) effectUpdateFx.Play();

            return true;
        }

        protected override bool EffectUpdate()
        {
            DestroyEffectStatPopupSpawnersOfEffectsOnPlant();

            return true;
        }

        protected override bool OnEffectEnded()
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
                Destroy(plantUnitToWipe, 0.1f);
            }

            return true;
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

                ability.TempDisable_SpawnedAbilityEffects_StatPopupSpawners_Except(this);

                ability.ForceStopAbilityImmediate();
            }
        }

        protected void DestroyEffectStatPopupSpawnersOfEffectsOnPlant()
        {
            if (plantAbilityEffectReceivedInventory.abilityEffectsReceived != null &&
               plantAbilityEffectReceivedInventory.abilityEffectsReceived.Count > 0)
            {
                for (int i = 0; i < plantAbilityEffectReceivedInventory.abilityEffectsReceived.Count; i++)
                {
                    if (i >= plantAbilityEffectReceivedInventory.abilityEffectsReceived.Count) break;

                    if (plantAbilityEffectReceivedInventory.abilityEffectsReceived[i].EffectStackSpawned() != null &&
                        plantAbilityEffectReceivedInventory.abilityEffectsReceived[i].EffectStackSpawned().Count > 0)
                    {
                        for (int j = 0; j < plantAbilityEffectReceivedInventory.abilityEffectsReceived[i].EffectStackSpawned().Count; j++)
                        {
                            if (j >= plantAbilityEffectReceivedInventory.abilityEffectsReceived[i].EffectStackSpawned().Count) break;

                            AbilityEffect aEffect = plantAbilityEffectReceivedInventory.abilityEffectsReceived[i].EffectStackSpawned()[j];

                            if (aEffect == null) continue;

                            if (aEffect == this) continue;

                            StatPopupSpawner eStatPopupSpawner = aEffect.GetAbilityEffectStatPopupSpawner();

                            if (eStatPopupSpawner != null)
                            {
                                eStatPopupSpawner.DetachAndDestroyAllStatPopupsIncludingSpawner(true);
                            }
                        }
                    }
                }
            }
        }
    }
}
