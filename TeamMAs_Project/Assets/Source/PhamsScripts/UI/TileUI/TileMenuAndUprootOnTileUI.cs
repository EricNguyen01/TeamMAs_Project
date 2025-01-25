// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace TeamMAsTD
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Tile))]
    public class TileMenuAndUprootOnTileUI : MonoBehaviour
    {
        [SerializeField] private Canvas tileWorldCanvas;

        [SerializeField] private Camera worldUICam;

        //INTERNALS...........................................................................................

        private CanvasGroup tileWorldCanvasGroup;

        private UprootConfirmationPopupUI uprootConfirmationPopupUI;

        public Tile tileHoldingThisMenu { get; private set; }

        private PointerEventData pEventData;//Unity's EventSystem pointer event data

        private bool disableTileMenuOpen = false;

        public bool isOpened { get; private set; } = false;

        public Button[] tileMenuButtons { get; private set; }

        //UnityEvents..........................................................................................

        [SerializeField] public UnityEvent OnTileMenuOpened;

        [SerializeField] public UnityEvent OnTileMenuClosed;

        //PRIVATES.............................................................................................

        private void Awake()
        {
            if (tileWorldCanvas == null)
            {
                tileWorldCanvas = GetComponentInChildren<Canvas>(true);

                if (tileWorldCanvas == null)
                {
                    Debug.LogError("Tile World Canvas children component not found on tile: " + name + ". Plant uprooting won't work!");

                    enabled = false;

                    return;
                }
            }

            tileMenuButtons = tileWorldCanvas.GetComponentsInChildren<Button>(true);

            tileWorldCanvasGroup = tileWorldCanvas.GetComponent<CanvasGroup>();

            if (tileWorldCanvasGroup == null) tileWorldCanvasGroup = tileWorldCanvas.gameObject.AddComponent<CanvasGroup>();

            tileHoldingThisMenu = GetComponent<Tile>();

            if (tileHoldingThisMenu == null)
            {
                Debug.LogError("Tile script component not found. Plant uprooting won't work!");

                enabled = false;

                return;
            }

            if(worldUICam != null)
            {
                tileWorldCanvas.worldCamera = worldUICam;
            }
            else
            {
                foreach(Camera cam in Camera.allCameras)
                {
                    if (cam.cullingMask == LayerMask.GetMask("WorldUI"))
                    {
                        worldUICam = cam;

                        if (tileWorldCanvas.worldCamera == null) tileWorldCanvas.worldCamera = worldUICam;

                        break;
                    }
                }
            }

            if (tileWorldCanvas.worldCamera == null) tileWorldCanvas.worldCamera = Camera.main;

            if (tileWorldCanvas.gameObject.activeInHierarchy) tileWorldCanvas.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            //check for an existing EventSystem and disble script if null
            if (EventSystem.current == null)
            {
                Debug.LogError("Cannot find an EventSystem in the scene. " +
                "An EventSystem is required for tile interaction menu to function. Disabling tile interaction menu!");

                enabled = false;

                return;
            }

            //Rain.cs C# Events sub
            Rain.OnRainStarted += TemporaryDisableTileMenuInteractionOnRainStarted;

            Rain.OnRainEnded += StopDisableTileMenuInteractionOnRainEnded;
        }

        private void OnDisable()
        {
            //Rain.cs C# events unsub
            Rain.OnRainStarted -= TemporaryDisableTileMenuInteractionOnRainStarted;

            Rain.OnRainEnded -= StopDisableTileMenuInteractionOnRainEnded;
        }

        public void OpenTileInteractionMenu(bool opened)
        {
            //if there is no tile script component reference->show error and stop executing
            if (tileHoldingThisMenu == null)
            {
                Debug.LogError("Trying to interact with tile: " + name + " but the Tile script component can't be found!");
                return;
            }

            //do nothing if tile doesnt have plant unit placed on
            if (tileHoldingThisMenu.plantUnitOnTile == null)
            {
                return;
            }

            PlantUnit plantSelected = tileHoldingThisMenu.plantUnitOnTile;

            //if a plant exists on this tile->process open/close tile menu
            if (opened)
            {
                if (!disableTileMenuOpen && !tileWorldCanvas.gameObject.activeInHierarchy)
                {
                    tileWorldCanvas.gameObject.SetActive(true);

                    isOpened = true;

                    OpenPlantRangeCircle(plantSelected, true);

                    OnTileMenuOpened?.Invoke();
                }
                else 
                {
                    OpenPlantRangeCircle(plantSelected, false);

                    tileWorldCanvas.gameObject.SetActive(false);

                    isOpened = false;

                    OnTileMenuClosed?.Invoke();
                }

                if(tileHoldingThisMenu && tileHoldingThisMenu.wateringOnTileScriptComp)
                {
                    tileHoldingThisMenu.wateringOnTileScriptComp.UpdateTotalWateringCostText();
                }

                return;
            }

            if (tileWorldCanvas.gameObject.activeInHierarchy) 
            { 
                tileWorldCanvas.gameObject.SetActive(false);

                OpenPlantRangeCircle(plantSelected, false);

                isOpened = false;

                OnTileMenuClosed?.Invoke();
            }
        }

        private void OpenPlantRangeCircle(PlantUnit plantUnit, bool shouldOpen)
        {
            if (plantUnit == null) return;

            if (plantUnit.plantRangeCircle == null) return;

            if (shouldOpen)
            {
                plantUnit.plantRangeCircle.DisplayPlantRangeCircle(true);

                return;
            }

            plantUnit.plantRangeCircle.DisplayPlantRangeCircle(false);
        }

        //Rain.cs C# Event functions............................................................................
        private void TemporaryDisableTileMenuInteractionOnRainStarted(Rain rain)
        {
            TemporaryDisableTileMenuContentInteraction(true);

            //SetDisableTileMenuOpen(true);
        }

        private void StopDisableTileMenuInteractionOnRainEnded(Rain rain)
        {
            TemporaryDisableTileMenuContentInteraction(false);

            //SetDisableTileMenuOpen(false);
        }

        //PUBLICS..............................................................................................

        //UnityEvent function for uproot UI button
        public void OnUprootOptionClicked()
        {
            OpenTileInteractionMenu(false);

            if (!uprootConfirmationPopupUI) uprootConfirmationPopupUI = FindObjectOfType<UprootConfirmationPopupUI>();

            if(uprootConfirmationPopupUI == null)
            {
                Debug.LogWarning("Uproot option is selected but no UprootConfirmationPopupUI object is found in scene! Uproot confirmation failed!");

                return;
            }

            //if there are multiple tiles (and their plants) being selected for uproot (processed through UnitGroupSelectionManager.cs)
            if (UnitGroupSelectionManager.unitGroupSelectionManagerInstance)
            {
                if(UnitGroupSelectionManager.unitGroupSelectionManagerInstance.unitGroupSelected != null &&
                   UnitGroupSelectionManager.unitGroupSelectionManagerInstance.unitGroupSelected.Count > 0)
                {
                    Tile[] selectedTiles = new Tile[UnitGroupSelectionManager.unitGroupSelectionManagerInstance.unitGroupSelected.Count];

                    int count = 0;

                    foreach(IUnit unit in UnitGroupSelectionManager.unitGroupSelectionManagerInstance.unitGroupSelected)
                    {
                        if(unit == null) continue;

                        if(unit is not PlantUnit) continue;

                        selectedTiles[count] = unit.GetTileUnitIsOn();

                        count++;
                    }

                    if(selectedTiles.Length > 0)
                    {
                        uprootConfirmationPopupUI.ActivateUprootConfirmationPopupForMultipleTiles(selectedTiles, true);

                        return;
                    }
                }
            }

            //else

            //if only this tile and its plant is being selected for uproot
            uprootConfirmationPopupUI.ActivateUprootConfirmationPopupForTile(tileHoldingThisMenu, true);
        }

        private void SetDisableTileMenuOpen(bool disabled)
        {
            if(disabled) OpenTileInteractionMenu(false);

            disableTileMenuOpen = disabled;
        }

        public void TemporaryDisableTileMenuContentInteraction(bool disabled)
        {
            if (tileWorldCanvasGroup == null) return;

            if (disabled)
            {
                SetDisableTileMenuOpen(true);

                tileWorldCanvasGroup.interactable = false;

                tileWorldCanvasGroup.blocksRaycasts = false;

                return;
            }

            tileWorldCanvasGroup.interactable = true;

            tileWorldCanvasGroup.blocksRaycasts = true;

            SetDisableTileMenuOpen(false);
        }
    }
}
