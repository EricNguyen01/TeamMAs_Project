// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using System.Collections;
using UnityEngine;

namespace TeamMAsTD
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlantUnit))]
    public class PlantWaterUsageSystem : MonoBehaviour
    {
        [SerializeField] private StatPopupSpawner plantWateringPopup;

        private PlantUnit plantUnitLinked;

        private PlantUnitSO plantUnitSO;

        private Tile tilePlantedOn;

        private PlantUnitWorldUI plantUnitWorldUI;

        private int totalWaterBars = 0;

        private int waterBarsRemaining = 0;

        private int wavesCanSurviveWithoutWater = 1;//default, will be changed on initialize (check initialization func).

        private int currentWavesSurvivedWithoutWater = -1;//doesnt count on the wave that water gets to 0 (start counting from next no water wave)

        //WaterAllButton.cs sub to this event to check for water all costs 
        public static event System.Action<PlantUnitSO> OnPlantWaterBarsRefilled;

        private void OnEnable()
        {
            WaveSpawner.OnWaveFinished += ConsumeWaterOnWaveFinished;

            Rain.OnRainEnded += RefillWaterOnRainEnded_And_CheckWavesSurvivedWithoutWater;
        }

        private void OnDisable()
        {
            WaveSpawner.OnWaveFinished -= ConsumeWaterOnWaveFinished;

            Rain.OnRainEnded -= RefillWaterOnRainEnded_And_CheckWavesSurvivedWithoutWater;

            StopAllCoroutines();
        }

        private void Start()
        {
            if(plantUnitLinked == null || plantUnitSO == null)
            {
                Debug.LogError("PlantUnitWaterUsageSystem script component on Plant Unit: " + name + " is not initialized in PlantUnit.cs. Disabling script!");
                enabled = false;

                return;
            }

            if(plantWateringPopup == null)
            {
                Debug.LogWarning("PlantWateringPopupSpawner component is missing in :" + name + "'s PlantWaterUsageSystem!");
            }
        }

        //This function is called in the Awake method of the PlantUnit that this script attached to
        public void InitializePlantWaterUsageSystem(PlantUnit plantUnit, bool awakeInit)
        {
            if(plantUnit == null || plantUnit.plantUnitScriptableObject == null)
            {
                enabled = false;
                return;
            }

            plantUnitLinked = plantUnit;

            plantUnitSO = plantUnit.plantUnitScriptableObject;

            totalWaterBars = plantUnit.plantUnitScriptableObject.waterBars;

            if (awakeInit)
            {
                tilePlantedOn = plantUnitLinked.tilePlacedOn;

                plantUnitWorldUI = plantUnitLinked.plantUnitWorldUI;

                waterBarsRemaining = totalWaterBars;

                wavesCanSurviveWithoutWater = plantUnit.plantUnitScriptableObject.wavesSurviveWithoutWater;
            }
        }

        //Use this func for refilling water using both rain (0 coin cost) and/or the water bucket UI button (coin cost)
        public void RefillWaterBars(int barsPerRefill, float coinsCostPerRefill)
        {
            //if water is full -> don't refill!
            if (waterBarsRemaining >= totalWaterBars)
            {
                //invoke water full event here...any water full indicator objects/effects will pick it up

                return;
            }

            //else

            //refill amount cant be less than 0
            if (barsPerRefill < 0) return;

            //use coins to fill water
            //check for GameResource and Coin Resource references
            if(GameResource.gameResourceInstance == null || GameResource.gameResourceInstance.coinResourceSO == null)
            {
                Debug.LogError("GameResource with Coin Resource is missing. Can't deduct coins from watering plant!");
            }
            //if references are not missing -> check for sufficient refilling funds
            else if(coinsCostPerRefill > GameResource.gameResourceInstance.coinResourceSO.resourceAmount)
            {
                Debug.LogError("Insufficient Funds to Refill Water Bars!");

                return;//exit function (stop refill) on insufficient funds
            }
            else GameResource.gameResourceInstance.coinResourceSO.RemoveResourceAmount(coinsCostPerRefill);

            //filling water
            waterBarsRemaining += barsPerRefill;

            OnPlantWaterBarsRefilled?.Invoke(plantUnitSO);

            if (plantWateringPopup != null) plantWateringPopup.PopUp(null, "+" + barsPerRefill.ToString(), StatPopup.PopUpType.Positive);

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

            //SaveLoadHandler.SaveThisSaveableOnly(tilePlantedOn.GetTileSaveable());
        }

        //This function is for the WaterAll button functionality with the water all bars on each plant option checked.
        //This function refills all the water bars currently unfilled on this plant
        //The number of bars can be refilled for each refill time is based on the number of bars per refill in this plant's SO data.
        public void RefillAllWaterBars()
        {
            if (waterBarsRemaining >= totalWaterBars) return;

            int barsPerRefill = plantUnitSO.waterBarsRefilledPerWatering;

            int coinCostPerRefill = plantUnitSO.wateringCoinsCost;

            int barsRefilled = 0;

            int barsToRefill = totalWaterBars - waterBarsRemaining;

            while (barsRefilled < barsToRefill)
            {
                barsRefilled += barsPerRefill;

                RefillWaterBars(barsPerRefill, coinCostPerRefill);
            }
        }

        //This function is also for the WaterAll button with the water all bars on each plant option unchecked
        //This function only refill 1 time only based on the bars per refill per refill instance set in SO data
        public void RefillPartialWaterBars(int timesToRefill)
        {
            if (timesToRefill <= 0) return;

            if (waterBarsRemaining >= totalWaterBars) return;

            int barsPerRefill = plantUnitSO.waterBarsRefilledPerWatering;

            int barsToRefill = totalWaterBars - waterBarsRemaining;

            int coinCostPerRefill = plantUnitSO.wateringCoinsCost;

            int refillTimes = 0;

            int barsRefilled = 0;

            //if either the refill times have exceed the provided times to refill
            //or all water bars on this plant have been fully filled -> stop refilling water.
            while(refillTimes < timesToRefill && barsRefilled < barsToRefill)
            {
                RefillWaterBars(barsPerRefill, coinCostPerRefill);

                barsRefilled += barsPerRefill;

                refillTimes++;
            }
        }

        //Water is consumed first on wave finished
        private void ConsumeWaterOnWaveFinished(WaveSpawner waveSpawner, int waveNum, bool stillHasOngoingWaves)
        {
            if (stillHasOngoingWaves) return;

            ConsumingWaterBars(plantUnitSO.waterUse);
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
                    UprootOnWaterDepleted(0.1f);
                }
            }
        }

        public void ConsumingWaterBars(int waterBarsConsumed)
        {
            waterBarsRemaining -= waterBarsConsumed;

            if(plantWateringPopup != null) plantWateringPopup.PopUp(null, "-" + waterBarsConsumed.ToString(), StatPopup.PopUpType.Negative);

            //set water slider UI values
            if(waterBarsRemaining > 0) plantUnitWorldUI.SetWaterSliderValue(waterBarsRemaining, totalWaterBars);
            else plantUnitWorldUI.SetWaterSliderValue(0, totalWaterBars);

            //SaveLoadHandler.SaveThisSaveableOnly(tilePlantedOn.GetTileSaveable());
        }

        public void SetWaterBarsRemainingDirectly(int waterBarsRemainingToSet)
        {
            //Delay setting water bars by 1 phys frame
            //to avoid setting water bars data on the same frame when this plant water bars usage system script is initializing
            //usually happens during loading from saves
            //which could cause bugs.
            StartCoroutine(SetWaterBarsRemainingDirectlyNextPhysUpdate(waterBarsRemainingToSet));
        }

        private IEnumerator SetWaterBarsRemainingDirectlyNextPhysUpdate(int waterBarsRemainingToSet)
        {
            yield return new WaitForFixedUpdate();

            if (waterBarsRemainingToSet >= totalWaterBars) waterBarsRemainingToSet = totalWaterBars;

            if (waterBarsRemainingToSet < 0) waterBarsRemainingToSet = 0;

            waterBarsRemaining = waterBarsRemainingToSet;

            if (waterBarsRemaining > 0) 
            { 
                plantUnitWorldUI.SetWaterSliderValue(waterBarsRemaining, totalWaterBars);

                //SaveLoadHandler.SaveThisSaveableOnly(tilePlantedOn.GetTileSaveable());
            }
            else UprootOnWaterDepleted();
        }

        private void UprootOnWaterDepleted(float uprootDelaySec = 0.0f)
        {
            //if the parent tile that this plant is planted on is not null:
            if (tilePlantedOn != null && tilePlantedOn.plantUnitOnTile == plantUnitLinked) 
            { 
                tilePlantedOn.UprootUnit(uprootDelaySec);
                return;
            }

            //if above is invalid -> find all tiles
            //if a tile is found to be the one carrying this plant -> process uproot
            foreach(Tile tile in FindObjectsOfType<Tile>())
            {
                if(tile.plantUnitOnTile != null && tile.plantUnitOnTile == plantUnitLinked)
                {
                    tile.UprootUnit(0.1f);
                    return;
                }
            }

            //else if all above failed -> just destroy the plant here
            Destroy(gameObject);
        }

        public bool IsWaterFull()
        {
            if (waterBarsRemaining >= totalWaterBars) return true;

            return false;
        }

        public int GetPartialRefillTotalCoinCost(int refillTime)
        {
            //if water is full -> cost nothing
            if (waterBarsRemaining >= totalWaterBars) return 0;

            int totalBarsNeededRefill = totalWaterBars - waterBarsRemaining;

            int actualRefillTime = 0;

            int barsRefilled = 0;

            while(actualRefillTime < refillTime && barsRefilled < totalBarsNeededRefill)
            {
                actualRefillTime++;

                barsRefilled += plantUnitSO.waterBarsRefilledPerWatering;
            }

            return plantUnitSO.wateringCoinsCost * actualRefillTime;
        }

        public int GetFullRefillTotalCoinCosts()
        {
            //if water is full -> cost nothing
            if (waterBarsRemaining >= totalWaterBars) return 0;

            int barsToRefill = totalWaterBars - waterBarsRemaining;

            int barsPerRefill = plantUnitSO.waterBarsRefilledPerWatering;

            //if water bars need to be refilled is <= bars that can be refilled per watering -> cost is equal to 1 time water refill costs
            if (barsToRefill <= barsPerRefill) return plantUnitSO.wateringCoinsCost;

            int waterTimes = 0;

            int barsRefilled = 0;

            while (barsRefilled < barsToRefill)
            {
                barsRefilled += barsPerRefill;

                waterTimes++;
            }

            return plantUnitSO.wateringCoinsCost * waterTimes;
        }

        public int GetRemainingWaterBars()
        {
            return waterBarsRemaining;
        }
    }
}
