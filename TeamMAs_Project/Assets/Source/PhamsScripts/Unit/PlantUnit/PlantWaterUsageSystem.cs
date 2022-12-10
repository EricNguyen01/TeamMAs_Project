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

        private void Awake()
        {
            tilePlantedOn = GetComponentInParent<Tile>();
        }

        private void OnEnable()
        {
            WaveSpawner.OnWaveFinished += ConsumeWaterOnWaveFinished;
        }

        private void OnDisable()
        {
            WaveSpawner.OnWaveFinished -= ConsumeWaterOnWaveFinished;
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
        }

        private void ConsumeWaterOnWaveFinished(WaveSpawner waveSpawner, int waveNum)
        {
            ConsumingWaterBars();
        }

        private void ConsumingWaterBars()
        {
            waterBarsRemaining -= plantUnitSO.waterUse;

            if(waterBarsRemaining <= 0)
            {
                waterBarsRemaining = 0;

                UprootOnWaterDepleted();
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

        //sub this function to plant getting watered event with number of water bars refilled as para
        private void RefillingWaterBars(int barsRefilled)
        {
            //refill amount cant be less than 0
            if (barsRefilled < 0) return;

            waterBarsRemaining += barsRefilled;

            if(waterBarsRemaining >= totalWaterBars)
            {
                waterBarsRemaining = totalWaterBars;
            }
        }
    }
}
