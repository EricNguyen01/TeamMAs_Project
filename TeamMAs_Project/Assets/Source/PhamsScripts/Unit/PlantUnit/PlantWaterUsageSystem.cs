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

        private int totalWaterBars = 0;

        private int waterBarsRemaining = 0;

        //PlantWaterUsageSystem events
        //Sub by Tile.cs to uproot plant using this system whenever all water bars run out
        public event System.Action<PlantUnit> OnPlantWaterDepleted;

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
        }

        private void ConsumingWaterBars()
        {
            waterBarsRemaining -= plantUnitSO.waterUse;

            if(waterBarsRemaining <= 0)
            {
                waterBarsRemaining = 0;

                OnPlantWaterDepleted?.Invoke(plantUnitLinked);
            }
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
