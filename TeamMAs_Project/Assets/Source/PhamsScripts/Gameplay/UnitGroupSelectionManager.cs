// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using PixelCrushers.DialogueSystem;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

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

        private List<GameObject> DEBUG_selectableUnitsReadOnly = new List<GameObject>();

        private List<GameObject> DEBUG_unitGroupSelectedReadOnly = new List<GameObject>();    

        private void Awake()
        {
            if(unitGroupSelectionManagerInstance && unitGroupSelectionManagerInstance != this)
            {
                enabled = false;

                Destroy(gameObject);

                return;
            }

            unitGroupSelectionManagerInstance = this;   

            dragSelectionBoxUI = GetComponent<DragSelectionBoxUI>();

            if(!dragSelectionBoxUI)
            {
                dragSelectionBoxUI = gameObject.AddComponent<DragSelectionBoxUI>();
            }
            
            dragSelectionBoxUI.InitDragSelectionBoxUI(this);
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
        }

#if ENABLE_LEGACY_INPUT_MANAGER

        private void Update()
        {
            if (!enabled) return;

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
#endif

        private void RegisterSelectedUnit(IUnit selectedUnit)
        {
            if (selectedUnit == null) return;
            
            if (unitGroupSelected.Add(selectedUnit))
            {
                if(selectedUnit is PlantUnit)
                {
                    PlantUnit plantUnit = selectedUnit as PlantUnit;

                    if(!plantUnit.GetTileUnitIsOn().tileGlowComp.isTileGlowing)
                        plantUnit.GetTileUnitIsOn().tileGlowComp.EnableTileGlowEffect(TileGlow.TileGlowMode.PositiveGlow);
                }
                
                DEBUG_unitGroupSelectedReadOnly.Add(selectedUnit.GetUnitTransform().gameObject);
            }
        }

        private void RemoveUnselectedUnit(IUnit unselectedUnit, bool removeFromDragBox)
        {
            if(unselectedUnit == null) return;
            
            if (unitGroupSelected.Contains(unselectedUnit))
            {
                if (unitGroupSelected.Remove(unselectedUnit))
                {
                    if (unselectedUnit is PlantUnit)
                    {
                        PlantUnit plantUnit = unselectedUnit as PlantUnit;

                        if (plantUnit.GetTileUnitIsOn().tileGlowComp.isTileGlowing)
                            plantUnit.GetTileUnitIsOn().tileGlowComp.DisableTileGlowEffect();
                    }

                    if (dragSelectionBoxUI && removeFromDragBox) dragSelectionBoxUI.RemoveUnitFromDragSelectionBox(unselectedUnit);

                    DEBUG_unitGroupSelectedReadOnly.Remove(unselectedUnit.GetUnitTransform().gameObject);
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

            foreach(IUnit unit in unitGroupSelected)
            {
                if (unit == null) continue;

                if (unit is PlantUnit)
                {
                    PlantUnit plantUnit = unit as PlantUnit;

                    if (plantUnit.GetTileUnitIsOn().tileGlowComp.isTileGlowing)
                        plantUnit.GetTileUnitIsOn().tileGlowComp.DisableTileGlowEffect();
                }
            }
            
            unitGroupSelected.Clear();

            if(dragSelectionBoxUI) dragSelectionBoxUI.ClearAllUnitsInDragSelectionBox();

            DEBUG_unitGroupSelectedReadOnly.Clear();
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

                //if not holding shift or ctrl when selecting a plant unit AND the selected plant unit is NOT selected before ->
                //only select that new plant unit and deregister/remove any previously selected ones.
                //else, execute group functions and return.
                if (!isHoldingShift && !isHoldingCtrl)
                {
                    //if already selected -> return
                    if (unitGroupSelected.Contains(targettedUnit))
                    {
                        //DO selected plant group functions here (e.g open option box to bulk remove the selected plants)

                        return;
                    }

                    //if not already selected -> select only the clicked plant unit

                    RemoveAllSelectedUnits();

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
                    if (dragSelectionBoxUI)
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

            DEBUG_selectableUnitsReadOnly.Add(newSelectableUnit.GetUnitTransform().gameObject);
        }

        public void DeRegisterSelectableUnitOnUnitDisabled(IUnit unselectableUnit)
        {
            if (unselectableUnit == null) return;

            RemoveUnselectedUnit(unselectableUnit, true);

            selectableUnits.Remove(unselectableUnit);

            DEBUG_selectableUnitsReadOnly.Remove(unselectableUnit.GetUnitTransform().gameObject);
        }
    }
}
