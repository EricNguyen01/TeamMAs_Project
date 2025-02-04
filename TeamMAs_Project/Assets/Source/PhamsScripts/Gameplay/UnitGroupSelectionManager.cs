// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using PixelCrushers.DialogueSystem;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TeamMAsTD
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(DragSelectionBoxUI))]
    public class UnitGroupSelectionManager : MonoBehaviour
    {
        public HashSet<IUnit> selectableUnits { get; private set; } = new HashSet<IUnit>();

        public HashSet<IUnit> unitGroupSelected { get; private set; } = new HashSet<IUnit>();

        public DragSelectionBoxUI dragSelectionBoxUI { get; private set; }

        public static UnitGroupSelectionManager unitGroupSelectionManagerInstance;

        //INTERNALS..................................................................................

        private bool isHoldingShift = false;

        private bool isHoldingCtrl = false;

        private IUnit targettedUnit;

        private PointerEventData eventData;

        private List<RaycastResult> raycastResults = new List<RaycastResult>();

#if UNITY_EDITOR

        private List<GameObject> DEBUG_selectableUnitsReadOnly = new List<GameObject>();

        private List<GameObject> DEBUG_unitGroupSelectedReadOnly = new List<GameObject>();

#endif

        private bool unitGroupSelectionEnabled = true;

        private void Awake()
        {
            if(unitGroupSelectionManagerInstance && unitGroupSelectionManagerInstance != this)
            {
                enabled = false;

                Destroy(gameObject);

                return;
            }

            unitGroupSelectionManagerInstance = this;

            DontDestroyOnLoad(gameObject);

            dragSelectionBoxUI = GetComponent<DragSelectionBoxUI>();

            if(!dragSelectionBoxUI)
            {
                dragSelectionBoxUI = gameObject.AddComponent<DragSelectionBoxUI>();
            }
            
            dragSelectionBoxUI.InitDragSelectionBoxUI(this);

            SceneManager.sceneLoaded += (Scene sc, LoadSceneMode loadMode) =>
            {
                if (sc.name.Contains("Menu")) enabled = false;
                else enabled = true;
            };
        }

        private void OnEnable()
        {
            if (!EventSystem.current)
            {
                Debug.LogWarning("UnitGroupSelectionManager Instance: " + name + "Could not find an EventSystem in the scene." +
                "Unit Group Selection won't work! Disabling script...");

                enabled = false;

                return;
            }

            if(dragSelectionBoxUI && !dragSelectionBoxUI.enabled) dragSelectionBoxUI.enabled = true;

            if (TileMenuInteractionHandler.tileMenuInteractionHandlerInstance)
            {
                if (TileMenuInteractionHandler.tileMenuInteractionHandlerInstance.isCheckingForTileMenuInteractions)
                {
                    TileMenuInteractionHandler.tileMenuInteractionHandlerInstance.EnableCheckForTileMenuInteractions(false);
                }
            }

            Rain.OnRainStarted += (Rain r) =>
            {
                EnableUnitGroupSelection(false);

                RemoveAllSelectedUnits();
            };

            Rain.OnRainEnded += (Rain r) => EnableUnitGroupSelection(true);
        }

        private void OnDisable()
        {
            if (dragSelectionBoxUI && dragSelectionBoxUI.enabled) dragSelectionBoxUI.enabled = false;

            Rain.OnRainStarted -= (Rain r) => 
            {
                EnableUnitGroupSelection(false);

                RemoveAllSelectedUnits();
            };

            Rain.OnRainEnded -= (Rain r) => EnableUnitGroupSelection(true);

            RemoveAllSelectedUnits();

            if (TileMenuInteractionHandler.tileMenuInteractionHandlerInstance)
            {
                if (!TileMenuInteractionHandler.tileMenuInteractionHandlerInstance.isCheckingForTileMenuInteractions)
                {
                    TileMenuInteractionHandler.tileMenuInteractionHandlerInstance.EnableCheckForTileMenuInteractions(true);
                }
            }
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= (Scene sc, LoadSceneMode loadMode) =>
            {
                if (sc.name.Contains("Menu")) enabled = false;
                else enabled = true;
            };
        }

        private void Update()
        {
            if (!enabled || !unitGroupSelectionEnabled) return;

            if (!EventSystem.current)
            {
                if (enabled) enabled = false;

                return;
            }

            if (Input.GetKey(KeyCode.LeftShift))
            {
                if (!Input.GetKey(KeyCode.LeftControl)) isHoldingShift = true;
                else isHoldingShift = false;
            }
            else
            {
                isHoldingShift = false;
            }

            if (Input.GetKey(KeyCode.LeftControl))
            {
                isHoldingShift = false;

                isHoldingCtrl = true;
            }
            else
            {
                isHoldingCtrl = false;
            }

            if (DialogueManager.Instance)
            {
                if (DialogueManager.Instance.isConversationActive) return;
            }

            //Process unit single mouse click selection here

            ProcessUnitGroupMouseDownSelection();

            ProcessUnitGroupMouseUpSelectionFinish();

            //Unit group drag selection and drag selection box functionalities and logic are processed in the DragSelectionBoxUI.cs script
            //that connects and is spawned from this script instance
        }

        private void RegisterSelectedUnit(IUnit selectedUnit)
        {
            if (selectedUnit == null) return;
            
            if (unitGroupSelected.Add(selectedUnit))
            {
                if(selectedUnit is PlantUnit)
                {
                    PlantUnit plantUnit = selectedUnit as PlantUnit;

                    if (plantUnit.GetTileUnitIsOn())
                    {
                        if (plantUnit.GetTileUnitIsOn().tileGlowComp)
                        {
                            if (!plantUnit.GetTileUnitIsOn().tileGlowComp.isTileGlowing)
                                plantUnit.GetTileUnitIsOn().tileGlowComp.EnableTileGlowEffect(TileGlow.TileGlowMode.PositiveGlow);
                        }

                        //update total multi-selected plants watering cost on adding new selected plant
                        if (plantUnit.tilePlacedOn.wateringOnTileScriptComp)
                        {
                            plantUnit.tilePlacedOn.wateringOnTileScriptComp.AddNewSelectedPlantsWateringCost(plantUnit);
                        }

                        TileMenuAndUprootOnTileUI tileMenu = plantUnit.GetTileUnitIsOn().tileMenuAndUprootOnTileUI;

                        if (TileMenuInteractionHandler.tileMenuInteractionHandlerInstance)
                        {
                            if (TileMenuInteractionHandler.tileMenuInteractionHandlerInstance.isCheckingForTileMenuInteractions)
                            {
                                TileMenuInteractionHandler.tileMenuInteractionHandlerInstance.EnableCheckForTileMenuInteractions(false);
                            }

                            if (tileMenu && !tileMenu.isOpened)
                            {
                                TileMenuInteractionHandler.tileMenuInteractionHandlerInstance.SetTileMenuInteractedManually(tileMenu, TileMenuInteractionHandler.TileMenuInteractionOptions.Open);
                            }
                        }
                    }
                }

#if UNITY_EDITOR

                DEBUG_unitGroupSelectedReadOnly.Add(selectedUnit.GetUnitTransform().gameObject);
#endif
            }
        }

        private void RemoveUnselectedUnit(IUnit unselectedUnit, bool removeFromDragBox)
        {
            if(unselectedUnit == null) return;

            bool shouldOpenNextInLineTileMenu = false;

            if (unitGroupSelected.Contains(unselectedUnit))
            {
                if (unitGroupSelected.Remove(unselectedUnit))
                {
                    if (unselectedUnit is PlantUnit)
                    {
                        PlantUnit plantUnit = unselectedUnit as PlantUnit;

                        if (plantUnit.GetTileUnitIsOn())
                        {
                            if (plantUnit.GetTileUnitIsOn().tileGlowComp)
                            {
                                if (plantUnit.GetTileUnitIsOn().tileGlowComp.isTileGlowing)
                                    plantUnit.GetTileUnitIsOn().tileGlowComp.DisableTileGlowEffect();
                            }

                            //update total multi-selected plants watering cost on removing unselected plant
                            if (plantUnit.tilePlacedOn.wateringOnTileScriptComp)
                            {
                                plantUnit.tilePlacedOn.wateringOnTileScriptComp.SubtractUnselectedPlantWateringCost(plantUnit);
                            }

                            TileMenuAndUprootOnTileUI tileMenu = plantUnit.GetTileUnitIsOn().tileMenuAndUprootOnTileUI;

                            if (TileMenuInteractionHandler.tileMenuInteractionHandlerInstance)
                            {
                                if (tileMenu)
                                {
                                    if (tileMenu.isOpened)
                                    {
                                        TileMenuInteractionHandler.tileMenuInteractionHandlerInstance.SetTileMenuInteractedManually(tileMenu, TileMenuInteractionHandler.TileMenuInteractionOptions.Close);

                                        shouldOpenNextInLineTileMenu = true;
                                    }
                                    else
                                    {
                                        tileMenu = TileMenuInteractionHandler.tileMenuInteractionHandlerInstance.tileMenuInUse;

                                        if(tileMenu && 
                                           tileMenu.tileHoldingThisMenu && 
                                           tileMenu.tileHoldingThisMenu.wateringOnTileScriptComp)
                                        {
                                            tileMenu.tileHoldingThisMenu.wateringOnTileScriptComp.UpdateTotalWateringCostText();
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (dragSelectionBoxUI && removeFromDragBox) dragSelectionBoxUI.RemoveUnitFromDragSelectionBox(unselectedUnit);

#if UNITY_EDITOR

                    DEBUG_unitGroupSelectedReadOnly.Remove(unselectedUnit.GetUnitTransform().gameObject);
#endif
                }

                if (unitGroupSelected.Count == 0)
                {
                    return;
                }

                if (!shouldOpenNextInLineTileMenu) return;

                IUnit[] selectedUnitGroup = unitGroupSelected.ToArray();

                for (int i = selectedUnitGroup.Length - 1; i >= 0; i--)
                {
                    if (selectedUnitGroup[i] == null) continue;

                    if (selectedUnitGroup[i] is not PlantUnit) continue;

                    if (!selectedUnitGroup[i].GetTileUnitIsOn()) continue;

                    if (!selectedUnitGroup[i].GetTileUnitIsOn().tileMenuAndUprootOnTileUI) continue;

                    TileMenuAndUprootOnTileUI tileMenu = selectedUnitGroup[i].GetTileUnitIsOn().tileMenuAndUprootOnTileUI;

                    if (TileMenuInteractionHandler.tileMenuInteractionHandlerInstance)
                    {
                        if (!tileMenu.isOpened)
                        {
                            TileMenuInteractionHandler.tileMenuInteractionHandlerInstance.SetTileMenuInteractedManually(tileMenu, TileMenuInteractionHandler.TileMenuInteractionOptions.Open);
                        }
                    }

                    break;
                }
            }
        }

        private void ToggleUnitSelection(IUnit toggleSelectedUnit)
        {
            if(toggleSelectedUnit == null) return;

            if (!unitGroupSelected.Contains(toggleSelectedUnit))
            {
                RegisterSelectedUnit(toggleSelectedUnit);
            }
            else
            {
                RemoveUnselectedUnit(toggleSelectedUnit, false);
            }
        }

        private void RemoveAllSelectedUnits()
        {
            if(unitGroupSelected == null || unitGroupSelected.Count == 0) return;

            IUnit[] selectedUnitGroup = unitGroupSelected.ToArray();

            for(int i = 0; i < selectedUnitGroup.Length; i++)
            {
                if (selectedUnitGroup[i] == null) continue;

                RemoveUnselectedUnit(selectedUnitGroup[i], true);
            }

            if(unitGroupSelected.Count > 0) unitGroupSelected.Clear();

            if(dragSelectionBoxUI && dragSelectionBoxUI.GetUnitsInDragSelectionBoxCount() > 0) 
                dragSelectionBoxUI.ClearAllUnitsInDragSelectionBox();

#if UNITY_EDITOR

            if(DEBUG_unitGroupSelectedReadOnly.Count > 0) DEBUG_unitGroupSelectedReadOnly.Clear();
#endif
        }

        public void SelectUnitsInDragSelectionBox(IUnit unitInBox)
        {
            if (isHoldingCtrl)
            {
                ToggleUnitSelection(unitInBox);

                return;
            }

            RegisterSelectedUnit(unitInBox);
        }

        public void UnselectUnitsOutsideDragSelectionBox(IUnit unitOutsideBox)
        {
            RemoveUnselectedUnit(unitOutsideBox, true);
        }

        public void ClearSelectedUnitsGroupOnDragBoxActive()
        {
            if(!isHoldingShift && !isHoldingCtrl)
            {
                RemoveAllSelectedUnits();
            }
        }

        private void ProcessUnitGroupMouseDownSelection()
        {
            if(!enabled) return;

            if (Input.GetButtonDown("Fire1"))
            {
                raycastResults.Clear();

                targettedUnit = null;

                eventData = new PointerEventData(EventSystem.current);

                eventData.position = Input.mousePosition;

                EventSystem.current.RaycastAll(eventData, raycastResults);

                //later, OnMouseUp() after this func -> if click on nothing -> remove all selected plant units
                //for now, do the below...

                if (raycastResults.Count == 0) return;

                for(int i = 0; i < raycastResults.Count; i++)
                {
                    if (!raycastResults[i].gameObject) continue;
                    
                    if (raycastResults[i].gameObject.layer == LayerMask.GetMask("UI") ||
                        raycastResults[i].gameObject.layer == LayerMask.NameToLayer("UI"))
                    {
                        /*if (raycastResults[i].gameObject.GetComponent<Button>())*/ return;
                    }
                    
                    if (TileMenuInteractionHandler.tileMenuInteractionHandlerInstance)
                    {
                        if(TileMenuInteractionHandler.tileMenuInteractionHandlerInstance.tileObjectAndTileMenuDict != null &&
                           TileMenuInteractionHandler.tileMenuInteractionHandlerInstance.tileObjectAndTileMenuDict.Count > 0)
                        {
                            if (TileMenuInteractionHandler.tileMenuInteractionHandlerInstance.tileObjectAndTileMenuDict.ContainsKey(raycastResults[i].gameObject))
                            {
                                targettedUnit = TileMenuInteractionHandler.tileMenuInteractionHandlerInstance.tileObjectAndTileMenuDict[raycastResults[i].gameObject].tileHoldingThisMenu.plantUnitOnTile;
                            }
                        }
                    }

                    if (targettedUnit != null) break;

                    if (raycastResults[i].gameObject.TryGetComponent<IUnit>(out targettedUnit))
                    {
                        break;
                    }
                }

                if (targettedUnit == null || targettedUnit is not PlantUnit) return;

                PlantUnit targettedPlantUnit = targettedUnit as PlantUnit;

                //if not holding shift or ctrl when selecting a plant unit AND the selected plant unit is NOT selected before ->
                //only select that new plant unit and deregister/remove any previously selected ones.
                //else, execute group functions and return.
                if (!isHoldingShift && !isHoldingCtrl)
                {
                    if(unitGroupSelected.Count > 1)
                    {
                        RemoveAllSelectedUnits();

                        RegisterSelectedUnit(targettedUnit);

                        return;
                    }
                    
                    if(unitGroupSelected.Count == 1)
                    {
                        if(unitGroupSelected.ElementAt(0) != targettedUnit)
                        {
                            RemoveUnselectedUnit(unitGroupSelected.ElementAt(0), true);

                            RegisterSelectedUnit(targettedUnit);
                        }
                        else
                        {
                            if (TileMenuInteractionHandler.tileMenuInteractionHandlerInstance)
                            {
                                if (targettedPlantUnit.GetTileUnitIsOn())
                                {
                                    TileMenuAndUprootOnTileUI tileMenu = targettedPlantUnit.GetTileUnitIsOn().tileMenuAndUprootOnTileUI;

                                    if (!tileMenu.isOpened)
                                    {
                                        TileMenuInteractionHandler.tileMenuInteractionHandlerInstance.SetTileMenuInteractedManually(tileMenu, TileMenuInteractionHandler.TileMenuInteractionOptions.Open);
                                    }
                                    else
                                    {
                                        RemoveUnselectedUnit(targettedUnit, true);
                                    }
                                }
                            }
                        }

                        return;
                    }

                    RegisterSelectedUnit(targettedUnit);

                    return;
                }

                //if only holding shift when selecting a plant unit ->
                //add that plant unit to the set of selected plant units if not already
                if (isHoldingShift && !isHoldingCtrl)
                {
                    RegisterSelectedUnit(targettedUnit);

                    return;
                }

                //if holding ctrl (even with or without shift being held down) when selecting a plant unit ->
                //toggle the selection of that plant unit (if alr selected -> remove OR if not alr selected -> add).
                if (isHoldingCtrl)
                {
                    ToggleUnitSelection(targettedUnit);
                }
            }
        }

        private void ProcessUnitGroupMouseUpSelectionFinish()
        {
            if (!enabled) return;

            if (Input.GetButtonUp("Fire1"))
            {
                //if click on nothing -> remove all selected plant units
                if (targettedUnit == null)
                {
                    if (dragSelectionBoxUI && dragSelectionBoxUI.hasHeldToDrag)
                    {
                        if (dragSelectionBoxUI.GetUnitsInDragSelectionBoxCount() > 0) return;
                    }

                    RemoveAllSelectedUnits();

                    return;
                }
            }
        }

        public void RegisterNewSelectableUnitOnUnitEnabled(IUnit newSelectableUnit)
        {
            if (newSelectableUnit == null) return;

            if (selectableUnits.Contains(newSelectableUnit)) return;

            selectableUnits.Add(newSelectableUnit);

#if UNITY_EDITOR

            DEBUG_selectableUnitsReadOnly.Add(newSelectableUnit.GetUnitTransform().gameObject);
#endif
        }

        public void DeRegisterSelectableUnitOnUnitDisabled(IUnit unselectableUnit)
        {
            if (unselectableUnit == null) return;

            RemoveUnselectedUnit(unselectableUnit, true);

            selectableUnits.Remove(unselectableUnit);

#if UNITY_EDITOR

            DEBUG_selectableUnitsReadOnly.Remove(unselectableUnit.GetUnitTransform().gameObject);
#endif
        }

        public void EnableUnitGroupSelection(bool enabled)
        {
            unitGroupSelectionEnabled = enabled;

            if (dragSelectionBoxUI)
            {
                dragSelectionBoxUI.EnableDragSelectionBox(enabled);
            }
        }
    }
}
