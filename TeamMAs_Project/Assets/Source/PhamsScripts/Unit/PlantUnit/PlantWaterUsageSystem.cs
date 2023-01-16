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

        private int totalWaterBars = 0;

        private int waterBarsRemaining = 0;

        private int roundsCanSurviveWithoutWater = 1;//default, will be changed on initialize (check initialization func).

        private int currentRoundsSurvivedWithoutWater = -1;//doesnt count on the wave that water gets to 0 (start counting from next no water wave)

        private void Awake()
        {
            tilePlantedOn = GetComponentInParent<Tile>();
        }

        private void OnEnable()
        {
            WaveSpawner.OnWaveFinished += ConsumeWaterOnWaveFinished;

            Rain.OnRainEnded += RefillWaterOnRainEnded;
        }

        private void OnDisable()
        {
            WaveSpawner.OnWaveFinished -= ConsumeWaterOnWaveFinished;

            Rain.OnRainEnded -= RefillWaterOnRainEnded;
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

        public void InitializePlantWaterUsageSystem(PlantUnit plantUnit)
        {
            if(plantUnit == null || plantUnit.plantUnitScriptableObject == null)
            {
                enabled = false;
                return;
            }

            plantUnitLinked = plantUnit;

            plantUnitSO = plantUnit.plantUnitScriptableObject;

            totalWaterBars = plantUnit.plantUnitScriptableObject.waterBars;

            waterBarsRemaining = totalWaterBars;

            roundsCanSurviveWithoutWater = plantUnit.plantUnitScriptableObject.roundsSurviveWithoutWater;
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
            if (waterBarsRemaining > 0) currentRoundsSurvivedWithoutWater = -1;

            if (waterBarsRemaining >= totalWaterBars)
            {
                waterBarsRemaining = totalWaterBars;
            }
        }

        //Water is consumed first on wave finished
        private void ConsumeWaterOnWaveFinished(WaveSpawner waveSpawner, int waveNum, bool stillHasOngoingWaves)
        {
            if (stillHasOngoingWaves) return;

            ConsumingWaterBars();
        }
        
        //Then, after consuming from wave finished, rain occurs in which water is refilled on rain ended
        private void RefillWaterOnRainEnded(Rain rain)
        {
            RefillWaterBars(rain.plantWaterBarsRefilledAfterRain, 0);

            //since OnRainEnded indicates the VERY END of a wave/round and...
            //if even after refilled and water still less than 0 -> set water bars to 0
            //then increment current rounds survived without water or Uproot if this value is >= rounds can survive without water
            if (waterBarsRemaining <= 0)
            {
                waterBarsRemaining = 0;

                //increment rounds survived without water if water is below or = 0
                //this value is reset in RefillingWaterBarsUsingCoins function above.
                currentRoundsSurvivedWithoutWater++;

                //uproot if rounds survived without water = rounds can survive without water
                if (currentRoundsSurvivedWithoutWater >= roundsCanSurviveWithoutWater)
                {
                    UprootOnWaterDepleted();
                }
            }
        }

        private void ConsumingWaterBars()
        {
            waterBarsRemaining -= plantUnitSO.waterUse;
        }

        private void UprootOnWaterDepleted()
        {
            //if the parent tile that this plant is planted on is not null:
            if (tilePlantedOn != null && tilePlantedOn.plantUnitOnTile == plantUnitLinked) 
            { 
                tilePlantedOn.UprootUnit();
                return;
            }

            //if above is invalid -> find all tiles
            //if a tile is found to be the one carrying this plant -> process uproot
            foreach(Tile tile in FindObjectsOfType<Tile>())
            {
                if(tile.plantUnitOnTile != null && tile.plantUnitOnTile == plantUnitLinked)
                {
                    tile.UprootUnit();
                    return;
                }
            }

            //else if all above failed -> just destroy the plant here
            Destroy(gameObject);
        }
    }
}
