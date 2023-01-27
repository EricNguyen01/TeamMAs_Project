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
        [SerializeField] private SoundPlayer wateringSoundPlayerPrefab;

        [SerializeField] private UnityEvent OnInsufficientFundsToWater;

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

            ProcessWateringSufficientFundsEvent(wateringCoinsCost);

            //play watering sound if plant on tile's water is not full
            //watering sound is played by creating a sound player object with SoundPlayer.cs script attached that plays watering sound on awake
            //upon finished playing watering sounds, watering sound player object is destroyed based on watering sound clip's length
            if (!tilePlantWaterUsageSystem.IsWaterFull()) SpawnAndDestroyWateringSoundPlayerIfNotNull();

            tilePlantWaterUsageSystem.RefillWaterBars(waterBarsToRefill, wateringCoinsCost);
        }

        private void ProcessWateringSufficientFundsEvent(int waterCost)
        {
            if (GameResource.gameResourceInstance == null || GameResource.gameResourceInstance.coinResourceSO == null) return;

            if(waterCost > GameResource.gameResourceInstance.coinResourceSO.resourceAmount)
            {
                OnInsufficientFundsToWater?.Invoke();

                return;
            }
            
        }

        private void SpawnAndDestroyWateringSoundPlayerIfNotNull()
        {
            if (wateringSoundPlayerPrefab == null) return;

            GameObject wateringSoundObj = Instantiate(wateringSoundPlayerPrefab.gameObject, transform.position, Quaternion.identity, transform);

            SoundPlayer wateringSoundPlayer = wateringSoundObj.GetComponent<SoundPlayer>();

            if(wateringSoundPlayer != null)
            {
                float wateringSoundLength = wateringSoundPlayer.GetCurrentAudioClipLengthIfNotNull();

                if(wateringSoundLength > 0.0f)
                {
                    //if watering sound length is found -> destroy after this duration
                    Destroy(wateringSoundObj, wateringSoundLength);
                }
                else Destroy(wateringSoundObj, 1.0f);//if clip's length is undefined -> destroy after 1.0f

                return;
            }

            //if watering sound player script not found -> destroy after 1.0f;
            Destroy(wateringSoundObj, 1.0f);
        }
    }
}
