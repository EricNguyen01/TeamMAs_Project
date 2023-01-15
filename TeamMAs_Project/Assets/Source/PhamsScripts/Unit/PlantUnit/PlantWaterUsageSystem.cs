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
            
        }

        private void OnDisable()
        {
            
        }

        private void Start()
        {
            if(plantUnitLinked == null || plantUnitSO == null)
            {
                Debug.LogError("PlantUnitWaterUsageSystem script component on Plant Unit: " + name + " is missing crucial components. Disabling script!");
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

        public void RefillingWaterBarsUsingCoins(int barsRefilled, float coinsCost)
        {
            //if water is full -> don't refill!
            if (waterBarsRemaining >= totalWaterBars) return;

            //else

            //refill amount cant be less than 0
            if (barsRefilled < 0) return;

            //use coins to fill water
            GameResource.gameResourceInstance.coinResourceSO.RemoveResourceAmount(coinsCost);

            //filling water
            waterBarsRemaining += barsRefilled;

            currentRoundsSurvivedWithoutWater = -1;//reset current rounds survived without water since plant just received water

            if (waterBarsRemaining >= totalWaterBars)
            {
                waterBarsRemaining = totalWaterBars;
            }
        }

        private void ConsumeWaterOnRainFinished()
        {
            ConsumingWaterBars();
        }

        private void ConsumingWaterBars()
        {
            waterBarsRemaining -= plantUnitSO.waterUse;

            if(waterBarsRemaining <= 0)
            {
                waterBarsRemaining = 0;

                //increment rounds survived without water if water is below or = 0
                //this value is reset in RefillingWaterBarsUsingCoins function above.
                currentRoundsSurvivedWithoutWater++;

                //uproot if rounds survived without water = rounds can survive without water
                if(currentRoundsSurvivedWithoutWater >= roundsCanSurviveWithoutWater)
                {
                    UprootOnWaterDepleted();
                }
            }
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
