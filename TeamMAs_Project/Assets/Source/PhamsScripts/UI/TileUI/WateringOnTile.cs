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

        [SerializeField] private float insufficientWateringFundPopupScaleMultiplier = 1.0f;

        //[SerializeField] private StatPopupSpawner insufficientWateringFundStatPopupPrefab;

        private StatPopupSpawner thisTileInsufficientWateringFundPopup;

        [SerializeField] private UnityEvent OnInsufficientFundsToWater;

        private Tile tileToWater;

        private Button wateringChildButton;

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

            foreach(Button button in GetComponentsInChildren<Button>(true))
            {
                bool validButtonFound = false;

                if (validButtonFound) break;

                for(int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
                {
                    if(button.onClick.GetPersistentMethodName(i) == "WaterTileIfHasPlantUnit")
                    {
                        validButtonFound = true;

                        wateringChildButton = button;

                        break;
                    }
                }
            }
        }

        private void Start()
        {
            //This if below needs to always be in Start() to avoid execution order conflicts
            //since it's getting the insufficient fund popup which is created in Tile.cs' Awake()
            if (tileToWater.thisTileInsufficientFundToPlantStatPopup != null)
            {
                thisTileInsufficientWateringFundPopup = tileToWater.thisTileInsufficientFundToPlantStatPopup;
            }
        }

        //Watering button UI event function -> callback from button's OnClicked event 
        //WARNING: IF THIS FUNCTION NAME IS TO BE CHANGED -> HAS TO CHANGE ITS STRING REFERENCE IN THE GET BUTTON IN AWAKE() AS WELL!!!!!
        public void WaterTileIfHasPlantUnit()
        {
            if (tileToWater.plantUnitOnTile == null) return;

            PlantWaterUsageSystem tilePlantWaterUsageSystem = tileToWater.plantUnitOnTile.plantWaterUsageSystem;

            if (tilePlantWaterUsageSystem == null) return;

            //if water in plant water usage system is full -> exit function
            if (tilePlantWaterUsageSystem.IsWaterFull()) return;

            int waterBarsToRefill = tileToWater.plantUnitOnTile.plantUnitScriptableObject.waterBarsRefilledPerWatering;

            int wateringCoinsCost = tileToWater.plantUnitOnTile.plantUnitScriptableObject.wateringCoinsCost;

            //check for insufficient watering fund and if insufficient -> process related popup and event
            if(HasInsufficientWateringFund(wateringCoinsCost)) return;

            //else if there is sufficient fund to water
            //play watering sound if plant on tile's water is not full (water full check is above)
            //watering sound is played by creating a sound player object with SoundPlayer.cs script attached that plays watering sound on awake
            //upon finished playing watering sounds, watering sound player object is destroyed based on watering sound clip's length
            SpawnAndDestroy_WateringSoundPlayer_IfNotNull();

            tilePlantWaterUsageSystem.RefillWaterBars(waterBarsToRefill, wateringCoinsCost);
        }

        private bool HasInsufficientWateringFund(int wateringCoinsCost)
        {
            //check for sufficient funds to water
            if (GameResource.gameResourceInstance != null && GameResource.gameResourceInstance.coinResourceSO != null)
            {
                if (GameResource.gameResourceInstance.coinResourceSO.resourceAmount < wateringCoinsCost)
                {
                    //if insufficient funds to water -> broadcast event and exit function
                    //Debug.LogError("Watering Failed: Insufficient Funds to Refill Water Bars!");

                    if (wateringChildButton != null)
                    {
                        if (thisTileInsufficientWateringFundPopup != null)
                        {
                            float popupOffsetX = thisTileInsufficientWateringFundPopup.GetPopupPositionAfterStartOffsetApplied().x;

                            float popupOffsetY = thisTileInsufficientWateringFundPopup.GetPopupPositionAfterStartOffsetApplied().y;

                            float offsetToButtonX = wateringChildButton.transform.position.x - popupOffsetX;

                            float offsetToButtonY = wateringChildButton.transform.position.y - popupOffsetY;

                            //move tile insufficient fund popup object (check Tile.cs) to watering button's center position
                            //reduce popup scale multiplier to 0.6f
                            //these config values are reset in Tile.cs
                            thisTileInsufficientWateringFundPopup.SetStatPopupSpawnerConfig(offsetToButtonY, 
                                                                                            offsetToButtonX,   
                                                                                            0.0f, 
                                                                                            0.0f, 
                                                                                            insufficientWateringFundPopupScaleMultiplier);
                        }
                    }

                    //if there's an insufficient fund to water stat popup script component attached -> show insufficient funds popup
                    if (thisTileInsufficientWateringFundPopup != null)
                    {
                        thisTileInsufficientWateringFundPopup.PopUp(null, null, false);
                    }

                    OnInsufficientFundsToWater?.Invoke();

                    return true;
                }
            }

            return false;
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
