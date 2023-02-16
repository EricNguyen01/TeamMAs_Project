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

        [SerializeField] private StatPopupSpawner insufficientWateringFundStatPopupPrefab;

        private StatPopupSpawner thisTileInsufficientWateringFundPopup;

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

            if(wateringSoundPlayerPrefab == null)
            {
                Debug.LogWarning("Watering Sound Player Prefab is missing on WateringOnTile: " + name);
            }

            if (insufficientWateringFundStatPopupPrefab != null)
            {
                GameObject statPopUpSpawnerGO = Instantiate(insufficientWateringFundStatPopupPrefab.gameObject, transform);

                statPopUpSpawnerGO.transform.localPosition = Vector3.zero;

                thisTileInsufficientWateringFundPopup = statPopUpSpawnerGO.GetComponent<StatPopupSpawner>();
            }
        }

        //Watering button UI event function -> callback from button's OnClicked event 
        public void WaterTileIfHasPlantUnit()
        {
            if (tileToWater.plantUnitOnTile == null) return;

            PlantWaterUsageSystem tilePlantWaterUsageSystem = tileToWater.plantUnitOnTile.plantWaterUsageSystem;

            if (tilePlantWaterUsageSystem == null) return;

            //if water in plant water usage system is full -> exit function
            if (tilePlantWaterUsageSystem.IsWaterFull()) return;

            int waterBarsToRefill = tileToWater.plantUnitOnTile.plantUnitScriptableObject.waterBarsRefilledPerWatering;

            int wateringCoinsCost = tileToWater.plantUnitOnTile.plantUnitScriptableObject.wateringCoinsCost;

            //check for sufficient funds to water
            if (GameResource.gameResourceInstance != null && GameResource.gameResourceInstance.coinResourceSO != null)
            {
                if (GameResource.gameResourceInstance.coinResourceSO.resourceAmount < wateringCoinsCost)
                {
                    //if insufficient funds to water -> broadcast event and exit function
                    //Debug.LogError("Watering Failed: Insufficient Funds to Refill Water Bars!");

                    //if there's an insufficient fund to water stat popup script component attached -> show insufficient funds popup
                    if(thisTileInsufficientWateringFundPopup != null)
                    {
                        thisTileInsufficientWateringFundPopup.PopUp(null, null, false);
                    }

                    OnInsufficientFundsToWater?.Invoke();

                    return;
                }
            }

            //else if there is sufficient fund to water
            //play watering sound if plant on tile's water is not full (water full check is above)
            //watering sound is played by creating a sound player object with SoundPlayer.cs script attached that plays watering sound on awake
            //upon finished playing watering sounds, watering sound player object is destroyed based on watering sound clip's length
            SpawnAndDestroy_WateringSoundPlayer_IfNotNull();

            tilePlantWaterUsageSystem.RefillWaterBars(waterBarsToRefill, wateringCoinsCost);
        }

        public void SpawnAndDestroy_WateringSoundPlayer_IfNotNull()
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
