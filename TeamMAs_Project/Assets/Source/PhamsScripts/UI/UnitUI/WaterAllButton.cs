// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

namespace TeamMAsTD
{
    [DisallowMultipleComponent]
    public class WaterAllButton : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler, IPointerUpHandler
    {
        [Header("Water All Config")]
        [SerializeField] [Min(1)]
        [Tooltip("The number of time each plant will be watered. " +
        "Each time, the number of water bars refilled will be the number set in each plant's SO data.")]
        private int timesToWaterOnEachPlant = 1;

        [SerializeField]
        [Tooltip("Water to full for each plant.")]
        private bool waterAllBarsOnEachPlant = false;

        [SerializeField] private string waterFullMessage;

        [SerializeField] private Color canWaterAllHighlightColor;

        [SerializeField] private Color unableToWaterAllHighlightColor;

        [Header("Required Components")]

        [SerializeField] private StatPopupSpawner insufficientFundToWaterAllPopupPrefab;

        [SerializeField] private float insufficientFundToWaterAllPopupScaleMultiplier = 1.5f;

        [SerializeField] private TextMeshProUGUI waterAllCostText;

        private StatPopupSpawner insufficientFundToWaterAllPopup;

        private CanvasGroup waterAllCanvasGroup;

        private Button waterAllButtonComp;

        private Color defaultWaterAllButtonSelectHighlightColor;

        private List<PlantUnit> existingPlantUnits = new List<PlantUnit>();

        private int totalWaterAllCost = 0;

        private UIShakeFx UI_ShakeFx;

        private UIRotate UI_Rotate;

        private void Awake()
        {
            if (waterAllCanvasGroup == null)
            {
                if(!TryGetComponent<CanvasGroup>(out waterAllCanvasGroup))
                {
                    waterAllCanvasGroup = gameObject.AddComponent<CanvasGroup>();
                }
            }

            if (insufficientFundToWaterAllPopupPrefab != null)
            {
                GameObject go = Instantiate(insufficientFundToWaterAllPopupPrefab.gameObject, transform.position, Quaternion.identity);

                insufficientFundToWaterAllPopup = go.GetComponent<StatPopupSpawner>();

                insufficientFundToWaterAllPopup.SetStatPopupSpawnerConfig(0.0f,
                                                                          0.0f,
                                                                          0.0f,
                                                                          0.0f,
                                                                          insufficientFundToWaterAllPopupScaleMultiplier);
            }

            waterAllButtonComp = GetComponentInChildren<Button>();

            if (waterAllButtonComp != null)
            {
                var waterAllButtonColors = waterAllButtonComp.colors;

                if (canWaterAllHighlightColor != Color.clear) waterAllButtonColors.highlightedColor = canWaterAllHighlightColor;

                waterAllButtonComp.colors = waterAllButtonColors;

                defaultWaterAllButtonSelectHighlightColor = waterAllButtonComp.colors.highlightedColor;
            }

            if (!TryGetComponent<UIShakeFx>(out UI_ShakeFx))
            {
                UI_ShakeFx = gameObject.AddComponent<UIShakeFx>();
            }

            if (!TryGetComponent<UIRotate>(out UI_Rotate))
            {
                UI_Rotate = gameObject.AddComponent<UIRotate>();
            }
        }

        private void OnEnable()
        {
            //check for an existing EventSystem and disble script if null
            if (EventSystem.current == null)
            {
                Debug.LogError("Cannot find an EventSystem in the scene. " +
                "An EventSystem is required for WaterAll button to function. Disabling WaterAllButton!");

                enabled = false;

                return;
            }

            foreach (Tile tile in FindObjectsOfType<Tile>())
            {
                tile.OnPlantUnitPlantedOnTile.AddListener(RegisteringExistingPlantUnit);

                tile.OnPlantUnitUprootedOnTile.AddListener(RemovingExistingPlantUnit);

                tile.OnPlantUnitUprootedOnTile.AddListener((PlantUnit pUnit, Tile t) => UpdateCostOnPlantWaterBarsRefilled(pUnit.plantUnitScriptableObject));
            }

            Tile.OnTileLoaded += (Tile tile) => RegisteringExistingPlantUnit(tile.plantUnitOnTile, tile);

            WaveSpawner.OnAllWaveSpawned += (WaveSpawner ws, bool b) => TemporaryDisableWaterAll(true);

            Rain.OnRainStarted += (Rain rain) => TemporaryDisableWaterAll(true);
            Rain.OnRainEnded += (Rain rain) => ProcessWaterAllButtonBehaviorsOnRainEnded();

            PlantWaterUsageSystem.OnPlantWaterBarsRefilled += UpdateCostOnPlantWaterBarsRefilled;

            //calls on enable to display water all cost text UI right away
            CheckSufficientFundToWaterAll();
        }

        private void OnDisable()
        {
            foreach (Tile tile in FindObjectsOfType<Tile>())
            {
                tile.OnPlantUnitPlantedOnTile.RemoveListener(RegisteringExistingPlantUnit);

                tile.OnPlantUnitUprootedOnTile.RemoveListener(RemovingExistingPlantUnit);

                tile.OnPlantUnitUprootedOnTile.RemoveListener((PlantUnit pUnit, Tile t) => UpdateCostOnPlantWaterBarsRefilled(pUnit.plantUnitScriptableObject));
            }

            Tile.OnTileLoaded -= (Tile tile) => RegisteringExistingPlantUnit(tile.plantUnitOnTile, tile);

            WaveSpawner.OnAllWaveSpawned -= (WaveSpawner ws, bool b) => TemporaryDisableWaterAll(true);

            Rain.OnRainStarted -= (Rain rain) => TemporaryDisableWaterAll(true);
            Rain.OnRainEnded -= (Rain rain) => ProcessWaterAllButtonBehaviorsOnRainEnded();

            PlantWaterUsageSystem.OnPlantWaterBarsRefilled -= UpdateCostOnPlantWaterBarsRefilled;

            StopAllCoroutines();
        }

        private void Start()
        {
            StartCoroutine(CheckSufficientFundToWaterAllDelayCoroutine());
        }

        private bool CanWaterAll(bool displayInsufficientFundPopup = false)
        {
            //if no plant to water -> do nothing
            if(existingPlantUnits == null || existingPlantUnits.Count == 0) return false;

            bool hasSufficientFund = CheckSufficientFundToWaterAll();

            //show insufficient popup if this component is assigned to this button and there's insufficient fund to water
            if (!hasSufficientFund)
            {
                if (displayInsufficientFundPopup && insufficientFundToWaterAllPopup != null)
                {
                    insufficientFundToWaterAllPopup.PopUp(null, null, StatPopup.PopUpType.Negative);
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

        /*
         * This function updates sufficient fund to water all plants on the next few phys update frames
         * To avoid updating while water bars data is being loaded to plants from saves which could cause 
         * mismatching data and bugs
         */
        private IEnumerator CheckSufficientFundToWaterAllDelayCoroutine()
        {
            yield return new WaitForSeconds(0.1f);

            CheckSufficientFundToWaterAll();

            yield break;
        }

        private void DisplayWaterAllCostText(int waterAllCosts)
        {
            if(waterAllCosts <= 0)
            {
                if(!string.IsNullOrEmpty(waterFullMessage)) waterAllCostText.text = waterFullMessage;
                else waterAllCostText.text = "Water Full!";

                return;
            }

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
            CheckSufficientFundToWaterAll();

            TemporaryDisableWaterAll(false);
        }
        
        public void WaterAll()
        {
            //if no sufficient funds OR
            //if total water all cost is less/equal 0 this means that all existing plant are having full water -> no need to water anymore
            if (!CanWaterAll(true) || totalWaterAllCost <= 0)
            {
                if(UI_ShakeFx) UI_ShakeFx.RunTweenInternal();

                return;
            }

            if (UI_Rotate && UI_Rotate.IsTweenRunning()) return;
            else UI_Rotate.RunTweenInternal();

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

            MemoryUsageLogger.LogMemoryUsageAsText("WaterAllFinished");
        }

        private void RegisteringExistingPlantUnit(PlantUnit plantUnit, Tile tile)
        {
            if (!plantUnit) return;

            if(!existingPlantUnits.Contains(plantUnit)) existingPlantUnits.Add(plantUnit);
        }

        private void RemovingExistingPlantUnit(PlantUnit plantUnit, Tile tile)
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

        private void ProcessWaterAllButtonSelectHighlightColors()
        {
            if (waterAllButtonComp == null) return;

            var waterAllButtonColors = waterAllButtonComp.colors;

            if (!CanWaterAll() || totalWaterAllCost <= 0)
            {
                if (unableToWaterAllHighlightColor != Color.clear)
                {
                    unableToWaterAllHighlightColor.a = 255f;

                    waterAllButtonColors.highlightedColor = unableToWaterAllHighlightColor;
                }
                else
                {
                    Color unableToWaterColor = Color.red;

                    unableToWaterColor.a = 255f;

                    waterAllButtonColors.highlightedColor = unableToWaterColor;
                }
            }
            else if (CanWaterAll() && totalWaterAllCost > 0)
            {
                defaultWaterAllButtonSelectHighlightColor.a = 255f;

                waterAllButtonColors.highlightedColor = defaultWaterAllButtonSelectHighlightColor;
            }

            waterAllButtonComp.colors = waterAllButtonColors;
        }

        //Unity UI Event Data Interface functions..............................................................................
        public void OnPointerEnter(PointerEventData pointerEventData)
        {
            ProcessWaterAllButtonSelectHighlightColors();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            ProcessWaterAllButtonSelectHighlightColors();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (waterAllButtonComp != null) waterAllButtonComp.OnDeselect(eventData);
        }
    }
}
