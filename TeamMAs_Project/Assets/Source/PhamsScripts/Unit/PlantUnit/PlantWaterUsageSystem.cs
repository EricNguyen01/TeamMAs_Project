using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamMAsTD
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlantUnit))]
    public class PlantWaterUsageSystem : MonoBehaviour
    {
        private PlantUnit plantUnitLinked;

        private PlantUnitSO plantUnitSO;

        private Tile tilePlantedOn;

        private PlantUnitWorldUI plantUnitWorldUI;

        private int totalWaterBars = 0;

        private int waterBarsRemaining = 0;

        private int wavesCanSurviveWithoutWater = 1;//default, will be changed on initialize (check initialization func).

        private int currentWavesSurvivedWithoutWater = -1;//doesnt count on the wave that water gets to 0 (start counting from next no water wave)

        private void OnEnable()
        {
            WaveSpawner.OnWaveFinished += ConsumeWaterOnWaveFinished;

            Rain.OnRainEnded += RefillWaterOnRainEnded_And_CheckWavesSurvivedWithoutWater;
        }

        private void OnDisable()
        {
            WaveSpawner.OnWaveFinished -= ConsumeWaterOnWaveFinished;

            Rain.OnRainEnded -= RefillWaterOnRainEnded_And_CheckWavesSurvivedWithoutWater;
        }

        private void Start()
        {
            if(plantUnitLinked == null || plantUnitSO == null)
            {
                Debug.LogError("PlantUnitWaterUsageSystem script component on Plant Unit: " + name + " is not initialized in PlantUnit.cs. Disabling script!");
                enabled = false;

                return;
            }
        }

        //This function is called in the Awake method of the PlantUnit that this script attached to
        public void InitializePlantWaterUsageSystem(PlantUnit plantUnit)
        {
            if(plantUnit == null || plantUnit.plantUnitScriptableObject == null)
            {
                enabled = false;
                return;
            }

            plantUnitLinked = plantUnit;

            plantUnitSO = plantUnit.plantUnitScriptableObject;

            tilePlantedOn = plantUnitLinked.tilePlacedOn;

            plantUnitWorldUI = plantUnitLinked.plantUnitWorldUI;

            totalWaterBars = plantUnit.plantUnitScriptableObject.waterBars;

            waterBarsRemaining = totalWaterBars;

            wavesCanSurviveWithoutWater = plantUnit.plantUnitScriptableObject.wavesSurviveWithoutWater;
        }

        //Use this func for refilling water using both rain (0 coin cost) and/or the water bucket UI button (coin cost)
        public void RefillWaterBars(int barsRefilled, float coinsCost)
        {
            //if water is full -> don't refill!
            if (waterBarsRemaining >= totalWaterBars)
            {
                //invoke water full event here...any water full indicator objects/effects will pick it up

                return;
            }

            //else

            //refill amount cant be less than 0
            if (barsRefilled < 0) return;

            //use coins to fill water
            //check for GameResource and Coin Resource references
            if(GameResource.gameResourceInstance == null || GameResource.gameResourceInstance.coinResourceSO == null)
            {
                Debug.LogError("GameResource with Coin Resource is missing. Can't deduct coins from watering plant!");
            }
            //if references are not missing -> check for sufficient refilling funds
            else if(coinsCost > GameResource.gameResourceInstance.coinResourceSO.resourceAmount)
            {
                Debug.LogError("Insufficient Funds to Refill Water Bars!");
            }
            else GameResource.gameResourceInstance.coinResourceSO.RemoveResourceAmount(coinsCost);

            //filling water
            waterBarsRemaining += barsRefilled;

            //reset current rounds survived without water if water bars remaining > 0 after being refilled.
            if (waterBarsRemaining > 0) 
            { 
                currentWavesSurvivedWithoutWater = -1;

                //reset plant degradation value as well
                plantUnitWorldUI.SetHealthBarSliderValue(0, wavesCanSurviveWithoutWater, true);
            }
            
            if(waterBarsRemaining <= 0)
            {
                waterBarsRemaining = 0;
            }

            if (waterBarsRemaining >= totalWaterBars)
            {
                waterBarsRemaining = totalWaterBars;
            }

            //set water slider UI values
            plantUnitWorldUI.SetWaterSliderValue(waterBarsRemaining, totalWaterBars);
        }

        //Water is consumed first on wave finished
        private void ConsumeWaterOnWaveFinished(WaveSpawner waveSpawner, int waveNum, bool stillHasOngoingWaves)
        {
            if (stillHasOngoingWaves) return;

            ConsumingWaterBars();
        }
        
        //Then, after consuming from wave finished, rain occurs in which water is refilled on rain ended
        private void RefillWaterOnRainEnded_And_CheckWavesSurvivedWithoutWater(Rain rain)
        {
            RefillWaterBars(rain.plantWaterBarsRefilledAfterRain, 0);

            //since OnRainEnded indicates the VERY END of a wave/round (happens even after OnWaveFinished)
            //and if even after rain water is still below 0, do:
            //start incrementing waves survived without water or Uproot if this value is >= rounds can survive without water
            if (waterBarsRemaining <= 0)
            {
                //increment rounds survived without water if water is below or = 0
                //this value is reset in RefillingWaterBarsUsingCoins function above.
                currentWavesSurvivedWithoutWater++;

                //set plant HP (degradation value) on water depleted
                if (currentWavesSurvivedWithoutWater <= 0) plantUnitWorldUI.SetHealthBarSliderValue(0, wavesCanSurviveWithoutWater, true);
                else plantUnitWorldUI.SetHealthBarSliderValue(currentWavesSurvivedWithoutWater, wavesCanSurviveWithoutWater, true);

                //uproot if rounds survived without water = rounds can survive without water
                if (currentWavesSurvivedWithoutWater >= wavesCanSurviveWithoutWater)
                {
                    UprootOnWaterDepleted();
                }
            }
        }

        private void ConsumingWaterBars()
        {
            waterBarsRemaining -= plantUnitSO.waterUse;

            //set water slider UI values
            if(waterBarsRemaining > 0) plantUnitWorldUI.SetWaterSliderValue(waterBarsRemaining, totalWaterBars);
            else plantUnitWorldUI.SetWaterSliderValue(0, totalWaterBars);
        }

        private void UprootOnWaterDepleted()
        {
            //if the parent tile that this plant is planted on is not null:
            if (tilePlantedOn != null && tilePlantedOn.plantUnitOnTile == plantUnitLinked) 
            { 
                tilePlantedOn.UprootUnit(0.6f);
                return;
            }

            //if above is invalid -> find all tiles
            //if a tile is found to be the one carrying this plant -> process uproot
            foreach(Tile tile in FindObjectsOfType<Tile>())
            {
                if(tile.plantUnitOnTile != null && tile.plantUnitOnTile == plantUnitLinked)
                {
                    tile.UprootUnit(0.6f);
                    return;
                }
            }

            //else if all above failed -> just destroy the plant here
            Destroy(gameObject);
        }
    }
}
