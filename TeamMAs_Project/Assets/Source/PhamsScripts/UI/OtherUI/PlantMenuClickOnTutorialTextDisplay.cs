using Language.Lua;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamMAsTD
{
    public class PlantMenuClickOnTutorialTextDisplay : MonoBehaviour
    {
        [SerializeField] private TDGrid gridToCheckForFirstPlantPlanted;

        [SerializeField] private InfoTooltip plantMenuClickOnInfoTooltip;

        [SerializeField] private WaveSO waveToDisplayAfterFinished;

        [SerializeField] private Vector2 tooltipOffset;

        [SerializeField] private float timeToDisplayTutorialTooltip = 5.0f;

        private Tile[] tilesInSelectedGrid;

        private WaveSO currentWave;

        private void Awake()
        {
            if (gridToCheckForFirstPlantPlanted == null || plantMenuClickOnInfoTooltip == null)
            {
                enabled = false;

                gameObject.SetActive(false);

                return;
            }

            plantMenuClickOnInfoTooltip.gameObject.SetActive(true);

            plantMenuClickOnInfoTooltip.EnableInfoTooltipImage(false, false);

            plantMenuClickOnInfoTooltip.EnableTooltipClickOnReminder(false);
        }

        private void OnEnable()
        {
            tilesInSelectedGrid = gridToCheckForFirstPlantPlanted.GetGridFlattened2DArray();

            if(tilesInSelectedGrid == null || tilesInSelectedGrid.Length == 0)
            {
                enabled = false;

                gameObject.SetActive(false);

                return;
            }

            if(waveToDisplayAfterFinished == null)
            {
                gridToCheckForFirstPlantPlanted.OnFirstPlantPlantedOnGrid.AddListener((PlantUnit pUnit) => EnablePlantMenuClickOnTutorialTooltip(pUnit, true));

                SubToTileMenuOpenEvent(true);
            }
            else
            {
                WaveSpawner.OnWaveFinished += DisplayTutorialTextOnWaveFinishedEvent;
            }
        }

        private void OnDisable()
        {
            gridToCheckForFirstPlantPlanted.OnFirstPlantPlantedOnGrid.RemoveListener((PlantUnit pUnit) => EnablePlantMenuClickOnTutorialTooltip(pUnit, true));

            SubToTileMenuOpenEvent(false);

            WaveSpawner.OnWaveFinished -= DisplayTutorialTextOnWaveFinishedEvent;
        }

        private void EnablePlantMenuClickOnTutorialTooltip(PlantUnit pUnit, bool enabled)
        {
            if (plantMenuClickOnInfoTooltip == null) return;

            if(pUnit != null)
            {
                Vector3 offset = new Vector3(tooltipOffset.x, tooltipOffset.y, pUnit.transform.position.z);

                plantMenuClickOnInfoTooltip.transform.position = pUnit.transform.position + offset;
            }

            if (enabled)
            {
                plantMenuClickOnInfoTooltip.EnableInfoTooltipImage(true, false);

                plantMenuClickOnInfoTooltip.EnableTooltipClickOnReminder(true);

                StartCoroutine(DisplayTooltipIn(timeToDisplayTutorialTooltip));

                return;
            }

            StopCoroutine(DisplayTooltipIn(timeToDisplayTutorialTooltip));

            plantMenuClickOnInfoTooltip.EnableInfoTooltipImage(false, false);

            plantMenuClickOnInfoTooltip.EnableTooltipClickOnReminder(false);
        }

        private IEnumerator DisplayTooltipIn(float displayTime)
        {
            yield return new WaitForSeconds(displayTime);

            plantMenuClickOnInfoTooltip.EnableInfoTooltipImage(false, false);

            plantMenuClickOnInfoTooltip.EnableTooltipClickOnReminder(false);

            yield break;
        }

        private void SubToTileMenuOpenEvent(bool sub)
        {
            if (tilesInSelectedGrid == null || tilesInSelectedGrid.Length == 0) return;

            if (sub)
            {
                for (int i = 0; i < tilesInSelectedGrid.Length; i++)
                {
                    if (tilesInSelectedGrid[i] == null) continue;

                    TileMenuAndUprootOnTileUI tileMenu = tilesInSelectedGrid[i].GetComponent<TileMenuAndUprootOnTileUI>();

                    if (tileMenu == null) continue;
                    
                    tileMenu.OnTileMenuOpened.AddListener(() => EnablePlantMenuClickOnTutorialTooltip(null, false));
                }

                return;
            }

            for (int i = 0; i < tilesInSelectedGrid.Length; i++)
            {
                if (tilesInSelectedGrid[i] == null) continue;

                TileMenuAndUprootOnTileUI tileMenu = tilesInSelectedGrid[i].GetComponent<TileMenuAndUprootOnTileUI>();

                if (tileMenu == null) continue;

                tileMenu.OnTileMenuOpened.RemoveListener(() => EnablePlantMenuClickOnTutorialTooltip(null, false));
            }
        }

        private void DisplayTutorialTextOnWaveFinishedEvent(WaveSpawner ws, int wNum, bool hasOngoingWave)
        {
            if (ws == null) return;

            if (hasOngoingWave) return;

            currentWave = ws.GetCurrentWave().waveSO;

            StartCoroutine(DisplayTutorialTextDelay(0.5f));
        }

        private IEnumerator DisplayTutorialTextDelay(float delaySecs)
        {
            yield return new WaitForSeconds(delaySecs);

            DisplayTutorialTextOnWaveFinished();

            yield break;
        }

        private void DisplayTutorialTextOnWaveFinished()
        {
            if (waveToDisplayAfterFinished == null) return;

            if (currentWave == null) return;

            if (currentWave != waveToDisplayAfterFinished) return;

            if (gridToCheckForFirstPlantPlanted == null) return;

            if (tilesInSelectedGrid == null || tilesInSelectedGrid.Length == 0) return;

            List<PlantUnit> plantsNeedWatering = new List<PlantUnit>();

            for(int i = 0; i < tilesInSelectedGrid.Length; i++)
            {
                if (tilesInSelectedGrid[i] == null) continue;   

                if (tilesInSelectedGrid[i].plantUnitOnTile == null) continue;

                if (tilesInSelectedGrid[i].plantUnitOnTile.plantWaterUsageSystem == null) continue;

                if (tilesInSelectedGrid[i].plantUnitOnTile.plantWaterUsageSystem.IsWaterFull()) continue;

                if (plantsNeedWatering.Contains(tilesInSelectedGrid[i].plantUnitOnTile)) continue;

                plantsNeedWatering.Add(tilesInSelectedGrid[i].plantUnitOnTile);
            }

            if (plantsNeedWatering.Count == 0) return;

            int rand = Random.Range(0, plantsNeedWatering.Count);

            EnablePlantMenuClickOnTutorialTooltip(plantsNeedWatering[rand], true);

            SubToTileMenuOpenEvent(true);
        }
    }
}
