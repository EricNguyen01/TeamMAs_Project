using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

namespace TeamMAsTD
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Tile), typeof(TileMenuAndUprootOnTileUI))]
    public class WateringOnTile : MonoBehaviour
    {
        private Tile tileToWater;

        private void Awake()
        {
            tileToWater= GetComponent<Tile>();
            if(tileToWater == null)
            {
                Debug.LogError("Tile script component not found. Watering on tile won't work, disabling script...");
                enabled = false;
                return;
            }
        }

        //Watering button UI event function -> callback from button's OnClicked event 
        public void WaterTileIfHasPlantUnit()
        {
            if (tileToWater.plantUnitOnTile == null) return;

            PlantWaterUsageSystem tilePlantWaterUsageSystem = tileToWater.plantUnitOnTile.plantWaterUsageSystem;

            if (tilePlantWaterUsageSystem == null) return;

            int waterBarsToRefill = tileToWater.plantUnitOnTile.plantUnitScriptableObject.waterBarsRefilledPerWatering;

            int wateringCoinsCost = tileToWater.plantUnitOnTile.plantUnitScriptableObject.wateringCoinsCost;

            tilePlantWaterUsageSystem.RefillWaterBars(waterBarsToRefill, wateringCoinsCost);
        }
    }
}
