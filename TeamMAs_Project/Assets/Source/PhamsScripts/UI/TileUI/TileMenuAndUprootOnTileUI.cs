// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Reflection.Emit;

namespace TeamMAsTD
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Tile))]
    public class TileMenuAndUprootOnTileUI : MonoBehaviour
    {
        [SerializeField] private Canvas tileMenuWorldCanvas;

        [SerializeField] private Camera worldUICam;

        //INTERNALS...........................................................................................

        private CanvasGroup tileMenuWorldCanvasGroup;

        private UprootConfirmationPopupUI uprootConfirmationPopupUI;

        public Tile tileHoldingThisMenu { get; private set; }

        private PointerEventData pEventData;//Unity's EventSystem pointer event data

        private bool disableTileMenuOpen = false;

        public bool isOpened { get; private set; } = false;

        public Button[] tileMenuButtons { get; private set; }

        private Vector3 tileMenuCanvasLocalPos = Vector3.zero;

        private Transform tileMenuCanvasDefaultParent;

        //UnityEvents..........................................................................................

        [SerializeField] public UnityEvent OnTileMenuOpened;

        [SerializeField] public UnityEvent OnTileMenuClosed;

        //PRIVATES.............................................................................................

        private void Awake()
        {
            if (tileMenuWorldCanvas == null)
            {
                tileMenuWorldCanvas = GetComponentInChildren<Canvas>(true);

                if (tileMenuWorldCanvas == null)
                {
                    Debug.LogError("Tile World Canvas children component not found on tile: " + name + ". Plant uprooting won't work!");

                    enabled = false;

                    return;
                }
            }

            tileMenuButtons = tileMenuWorldCanvas.GetComponentsInChildren<Button>(true);

            tileMenuWorldCanvasGroup = tileMenuWorldCanvas.GetComponent<CanvasGroup>();

            if (tileMenuWorldCanvasGroup == null) tileMenuWorldCanvasGroup = tileMenuWorldCanvas.gameObject.AddComponent<CanvasGroup>();

            tileHoldingThisMenu = GetComponent<Tile>();

            if (tileHoldingThisMenu == null)
            {
                Debug.LogError("Tile script component not found. Plant uprooting won't work!");

                enabled = false;

                return;
            }

            if(worldUICam != null)
            {
                tileMenuWorldCanvas.worldCamera = worldUICam;
            }
            else
            {
                foreach(Camera cam in Camera.allCameras)
                {
                    if (cam.cullingMask == LayerMask.GetMask("WorldUI"))
                    {
                        worldUICam = cam;

                        if (tileMenuWorldCanvas.worldCamera == null) tileMenuWorldCanvas.worldCamera = worldUICam;

                        break;
                    }
                }
            }

            if (tileMenuWorldCanvas.worldCamera == null) tileMenuWorldCanvas.worldCamera = Camera.main;

            if (tileMenuWorldCanvas.gameObject.activeInHierarchy) tileMenuWorldCanvas.gameObject.SetActive(false);
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

            tileMenuCanvasDefaultParent = tileMenuWorldCanvas.transform.parent;

            tileMenuCanvasLocalPos = tileMenuWorldCanvas.transform.localPosition;

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

        public void OpenTileInteractionMenu(bool opened, bool shouldToggle = false)
        {
            //if there is no tile script component reference->show error and stop executing
            if (tileHoldingThisMenu == null)
            {
                Debug.LogError("Trying to interact with tile: " + name + " but the Tile script component can't be found!");

                if (tileMenuWorldCanvas.gameObject.activeInHierarchy) tileMenuWorldCanvas.gameObject.SetActive(false);

                return;
            }

            //do nothing if tile doesnt have plant unit placed on
            if (tileHoldingThisMenu.plantUnitOnTile == null)
            {
                if (tileMenuWorldCanvas.gameObject.activeInHierarchy) tileMenuWorldCanvas.gameObject.SetActive(false);

                return;
            }

            PlantUnit plantSelected = tileHoldingThisMenu.plantUnitOnTile;

            if (disableTileMenuOpen) goto CloseTileMenu;

            if (tileHoldingThisMenu && tileHoldingThisMenu.wateringOnTileScriptComp)
                tileHoldingThisMenu.wateringOnTileScriptComp.UpdateTotalWateringCostText();

            //if a plant exists on this tile->process open/close tile menu
            if (opened)
            {
                if (shouldToggle && tileMenuWorldCanvas.gameObject.activeInHierarchy) goto CloseTileMenu;

                SetTileMenuDefaultRuntimeParentAndLocalPos();

                tileMenuWorldCanvas.gameObject.SetActive(true);

                isOpened = true;

                OpenPlantRangeCircle(plantSelected, true);

                OnTileMenuOpened?.Invoke();

                return;
            }

        CloseTileMenu:

            if (tileMenuWorldCanvas.gameObject.activeInHierarchy)
            {
                tileMenuWorldCanvas.gameObject.SetActive(false);

                OpenPlantRangeCircle(plantSelected, false);

                isOpened = false;

                OnTileMenuClosed?.Invoke();
            }

            SetTileMenuDefaultRuntimeParentAndLocalPos();
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
            if (tileMenuWorldCanvasGroup == null) return;

            if (disabled)
            {
                SetDisableTileMenuOpen(true);

                tileMenuWorldCanvasGroup.interactable = false;

                tileMenuWorldCanvasGroup.blocksRaycasts = false;

                return;
            }

            tileMenuWorldCanvasGroup.interactable = true;

            tileMenuWorldCanvasGroup.blocksRaycasts = true;

            SetDisableTileMenuOpen(false);
        }

        public void SetTileMenuLocalPos(Vector3 pos, bool providedPosIsWorld, bool keep_Z_AsDefault = true)
        {
            Vector3 localPos = pos;
            
            if (providedPosIsWorld) localPos = tileMenuWorldCanvas.transform.parent.InverseTransformPoint(pos);

            if (keep_Z_AsDefault) localPos = new Vector3(localPos.x, localPos.y, tileMenuWorldCanvas.transform.localPosition.z);

            tileMenuWorldCanvas.transform.localPosition = localPos;
        }

        public void SetTileMenuDefaultRuntimeParentAndLocalPos()
        {
            if(tileMenuCanvasDefaultParent != null)
            {
                if(tileMenuWorldCanvas.transform.parent != tileMenuCanvasDefaultParent) 
                    tileMenuWorldCanvas.transform.SetParent(tileMenuCanvasDefaultParent);
            }
            else
            {
                if(tileHoldingThisMenu != null && tileMenuWorldCanvas.transform.parent == null) 
                    tileMenuWorldCanvas.transform.SetParent(tileHoldingThisMenu.transform);
            }

            if(tileMenuWorldCanvas.transform.localPosition != tileMenuCanvasLocalPos) 
                tileMenuWorldCanvas.transform.localPosition = tileMenuCanvasLocalPos;
        }
    }
}
