using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TeamMAsTD
{
    [DisallowMultipleComponent]
    public class WaterAllButton : MonoBehaviour
    {
        [Header("Required Components")]

        [SerializeField] private StatPopupSpawner insufficientFundToWaterAllPopupPrefab;

        [SerializeField] private TextMeshProUGUI waterAllCostText;

        private StatPopupSpawner insufficientFundToWaterAllPopup;

        private CanvasGroup waterAllCanvasGroup;

        private List<PlantUnit> existingPlantUnits = new List<PlantUnit>();

        private int totalWaterAllCost = 0;

        private void Awake()
        {
            if(waterAllCanvasGroup == null)
            {
                waterAllCanvasGroup = GetComponent<CanvasGroup>(); 
            }
            if(waterAllCanvasGroup == null)
            {
                waterAllCanvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            if(insufficientFundToWaterAllPopupPrefab != null)
            {
                GameObject go = Instantiate(insufficientFundToWaterAllPopupPrefab.gameObject, transform.position, Quaternion.identity);

                insufficientFundToWaterAllPopup = go.GetComponent<StatPopupSpawner>();
            }
        }

        private void OnEnable()
        {
            foreach(Tile tile in FindObjectsOfType<Tile>())
            {
                tile.OnPlantUnitPlantedOnTile.AddListener(RegisteringExisitngPlantUnit);

                tile.OnPlantUnitUprootedOnTile.AddListener(RemovingExistingPlantUnit);
            }

            WaveSpawner.OnAllWaveSpawned += (WaveSpawner ws, bool b) => TemporaryDisableWaterAll(true);

            Rain.OnRainStarted += (Rain rain) => TemporaryDisableWaterAll(true);
            Rain.OnRainEnded += (Rain rain) => ProcessWaterAllButtonBehaviorsOnRainEnded();

            PlantWaterUsageSystem.OnPlantWaterBarsRefilled += UpdateCostOnPlantWaterBarsRefilled;

            CheckSufficientFundToWaterAll();//this function also displays water all cost text
        }

        private void OnDisable()
        {
            foreach (Tile tile in FindObjectsOfType<Tile>())
            {
                tile.OnPlantUnitPlantedOnTile.RemoveListener(RegisteringExisitngPlantUnit);

                tile.OnPlantUnitUprootedOnTile.RemoveListener(RemovingExistingPlantUnit);
            }

            WaveSpawner.OnAllWaveSpawned -= (WaveSpawner ws, bool b) => TemporaryDisableWaterAll(true);

            Rain.OnRainStarted -= (Rain rain) => TemporaryDisableWaterAll(true);
            Rain.OnRainEnded -= (Rain rain) => ProcessWaterAllButtonBehaviorsOnRainEnded();

            PlantWaterUsageSystem.OnPlantWaterBarsRefilled -= UpdateCostOnPlantWaterBarsRefilled;

            StopAllCoroutines();
        }

        private bool CanWaterAll()
        {
            //if no plant to water -> do nothing
            if(existingPlantUnits == null || existingPlantUnits.Count == 0) return false;

            bool hasSufficientFund = CheckSufficientFundToWaterAll();

            //show insufficient popup if this component is assigned to this button and there's insufficient fund to water
            if (!hasSufficientFund)
            {
                if (insufficientFundToWaterAllPopup != null)
                {
                    insufficientFundToWaterAllPopup.PopUp(null, null, false);
                }
            }

            return hasSufficientFund;
        }

        private bool CheckSufficientFundToWaterAll()
        {
            int waterAllCoinCosts = 0;

            if(existingPlantUnits.Count > 0)
            {
                for (int i = 0; i < existingPlantUnits.Count; i++)
                {
                    waterAllCoinCosts += existingPlantUnits[i].plantWaterUsageSystem.GetFullRefillTotalCoinCosts();
                }
            }

            totalWaterAllCost = waterAllCoinCosts;
            
            DisplayWaterAllCostText(waterAllCoinCosts);

            //if game resource - coin resource is missing in scene, no need to process watering costs and just water all directly
            if (GameResource.gameResourceInstance == null || GameResource.gameResourceInstance.coinResourceSO == null) return true;

            int currentCoinAmount = (int)GameResource.gameResourceInstance.coinResourceSO.resourceAmount;

            if (waterAllCoinCosts > currentCoinAmount) return false;

            return true;
        }

        private void DisplayWaterAllCostText(int waterAllCosts)
        {
            waterAllCostText.text = "Cost: " + waterAllCosts.ToString();
        }

        private void UpdateCostOnPlantWaterBarsRefilled(PlantUnitSO plantUnitReceivedWater)
        {
            //do not execute this function if water all button is being temporary disabled
            if (waterAllCanvasGroup != null && !waterAllCanvasGroup.interactable) return;

            totalWaterAllCost -= plantUnitReceivedWater.wateringCoinsCost;

            DisplayWaterAllCostText(totalWaterAllCost);
        }

        private void ProcessWaterAllButtonBehaviorsOnRainEnded()
        {
            StartCoroutine(ProcessWaterAllButtonBehaviorsOnRainEndedCoroutine());
        }

        private IEnumerator ProcessWaterAllButtonBehaviorsOnRainEndedCoroutine()
        {
            yield return new WaitForSeconds(0.06f);

            CheckSufficientFundToWaterAll();

            TemporaryDisableWaterAll(false);

            yield break;
        }
        
        public void WaterAll()
        {
            if(!CanWaterAll()) return;

            for(int i = 0; i < existingPlantUnits.Count; i++)
            {
                int barsPerRefill = existingPlantUnits[i].plantUnitScriptableObject.waterBarsRefilledPerWatering;

                int coinCostPerRefill = existingPlantUnits[i].plantUnitScriptableObject.wateringCoinsCost;

                //Play individual plant watering sound from the WateringOnTile script attached to the tiles with plants planted on
                if (existingPlantUnits[i].tilePlacedOn != null)
                {
                    if (existingPlantUnits[i].tilePlacedOn.wateringOnTileScriptComp != null)
                    {
                        existingPlantUnits[i].tilePlacedOn.wateringOnTileScriptComp.SpawnAndDestroy_WateringSoundPlayer_IfNotNull();
                    }
                }

                //refill water bars for each plant water usage system on each plant in scene
                existingPlantUnits[i].plantWaterUsageSystem.RefillAllWaterBars(barsPerRefill, coinCostPerRefill);
            }
        }

        public void RegisteringExisitngPlantUnit(PlantUnit plantUnit, Tile tile)
        {
            if(!existingPlantUnits.Contains(plantUnit)) existingPlantUnits.Add(plantUnit);
        }

        public void RemovingExistingPlantUnit(PlantUnit plantUnit, Tile tile)
        {
            if(existingPlantUnits.Contains(plantUnit)) existingPlantUnits.Remove(plantUnit);
        }

        public void TemporaryDisableWaterAll(bool shouldDisable)
        {
            if (waterAllCanvasGroup == null) return;

            if (shouldDisable)
            {
                waterAllCanvasGroup.alpha = 0.5f;

                waterAllCanvasGroup.interactable = false;

                waterAllCanvasGroup.blocksRaycasts = false;

                return;
            }

            waterAllCanvasGroup.alpha = 1.0f;

            waterAllCanvasGroup.interactable = true;

            waterAllCanvasGroup.blocksRaycasts = true;
        }
    }
}
