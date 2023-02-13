using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace TeamMAsTD
{
    [DisallowMultipleComponent]
    public class WaterAllButton : MonoBehaviour
    {
        [SerializeField] private StatPopupSpawner insufficientFundToWaterAllPopupPrefab;

        private StatPopupSpawner insufficientFundToWaterAllPopup;

        private List<PlantUnit> existingPlantUnits = new List<PlantUnit>();

        private void Awake()
        {
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
        }

        private void OnDisable()
        {
            foreach (Tile tile in FindObjectsOfType<Tile>())
            {
                tile.OnPlantUnitPlantedOnTile.RemoveListener(RegisteringExisitngPlantUnit);

                tile.OnPlantUnitUprootedOnTile.RemoveListener(RemovingExistingPlantUnit);
            }
        }

        private bool CanWaterAll()
        {
            if(existingPlantUnits == null || existingPlantUnits.Count == 0) return false;

            int waterAllCoinCosts = 0;

            for(int i = 0; i < existingPlantUnits.Count; i++)
            {
                waterAllCoinCosts += existingPlantUnits[i].plantWaterUsageSystem.GetFullRefillTotalCoinCosts();
            }

            return CheckSufficientFundToWaterAll(waterAllCoinCosts);
        }

        private bool CheckSufficientFundToWaterAll(int waterAllCoinCosts)
        {
            if (GameResource.gameResourceInstance == null || GameResource.gameResourceInstance.coinResourceSO == null) return false;

            int currentCoinAmount = (int)GameResource.gameResourceInstance.coinResourceSO.resourceAmount;

            if(waterAllCoinCosts > currentCoinAmount)
            {
                if(insufficientFundToWaterAllPopup != null)
                {
                    insufficientFundToWaterAllPopup.PopUp(null, null, false);
                }

                return false;
            }

            return true;
        }
        
        public void WaterAll()
        {
            if(!CanWaterAll()) return;

            for(int i = 0; i < existingPlantUnits.Count; i++)
            {
                int barsPerRefill = existingPlantUnits[i].plantUnitScriptableObject.waterBarsRefilledPerWatering;

                int coinCostPerRefill = existingPlantUnits[i].plantUnitScriptableObject.wateringCoinsCost;

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
    }
}
