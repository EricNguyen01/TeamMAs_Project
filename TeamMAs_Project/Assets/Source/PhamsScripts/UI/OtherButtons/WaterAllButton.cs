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
        [Header("Water All Config")]
        [SerializeField] [Min(1)]
        [Tooltip("The number of time each plant will be watered. " +
        "Each time, the number of water bars refilled will be the number set in each plant's SO data.")]
        private int timesToWaterOnEachPlant = 1;

        [SerializeField]
        [Tooltip("Water to full for each plant.")]
        private bool waterAllBarsOnEachPlant = false;

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
                    if (waterAllBarsOnEachPlant)
                    {
                        waterAllCoinCosts += existingPlantUnits[i].plantWaterUsageSystem.GetFullRefillTotalCoinCosts();
                    }
                    else
                    {
                        waterAllCoinCosts += existingPlantUnits[i].plantWaterUsageSystem.GetPartialRefillTotalCoinCost(timesToWaterOnEachPlant);
                    }
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

            if (waterAllBarsOnEachPlant)
            {
                totalWaterAllCost -= plantUnitReceivedWater.wateringCoinsCost;

                DisplayWaterAllCostText(totalWaterAllCost);
            }
            else
            {
                CheckSufficientFundToWaterAll();
            }
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

            bool hasPlayedWateringSound = false;

            for(int i = 0; i < existingPlantUnits.Count; i++)
            {
                if (existingPlantUnits[i] == null) continue;

                //Play individual plant watering sound from the WateringOnTile script attached to the tiles with plants planted on
                if (!hasPlayedWateringSound && existingPlantUnits[i].tilePlacedOn != null)
                {
                    if (existingPlantUnits[i].tilePlacedOn.wateringOnTileScriptComp != null)
                    {
                        //only play water sound on 1 plant instance instead of all (to avoid exploding player's ears)
                        hasPlayedWateringSound = true;

                        existingPlantUnits[i].tilePlacedOn.wateringOnTileScriptComp.SpawnAndDestroy_WateringSoundPlayer_IfNotNull();
                    }
                }

                //refill water bars for each plant water usage system on all plants in scene
                if (waterAllBarsOnEachPlant)
                {
                    existingPlantUnits[i].plantWaterUsageSystem.RefillAllWaterBars();
                }
                else
                {
                    existingPlantUnits[i].plantWaterUsageSystem.RefillPartialWaterBars(timesToWaterOnEachPlant);
                }
                
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
