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

        private PointerEventData pointerEventData;

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
                Debug.LogWarning("UnitGroupSelectionManager Instance: " + name + "Could Not Find An EventSystem in The Scene." +
                "Unit Group Selection won't work! Disabling script...");

                enabled = false;

                return;
            }

            pointerEventData = new PointerEventData(EventSystem.current);
        }

        private void Update()
        {
            if (!enabled) return;

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

            //Process unit single mouse click selection here

            ProcessUnitGroupClickSelection();

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

                    if(!plantUnit.GetTileUnitIsOn().tileGlowComp.isTileGlowing)
                        plantUnit.GetTileUnitIsOn().tileGlowComp.EnableTileGlowEffect(TileGlow.TileGlowMode.PositiveGlow);
                }

                DEBUG_unitGroupSelectedReadOnly.Add(selectedUnit.GetUnitTransform().gameObject);
            }
        }

        private void RemoveUnselectedUnit(IUnit unselectedUnit)
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
                RemoveUnselectedUnit(toggleSelectedUnit);
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

            DEBUG_unitGroupSelectedReadOnly.Clear();
        }

        public void SelectUnitsInDragSelectionBox(IUnit unitInBox)
        {
            if (isHoldingCtrl)
            {
                RemoveUnselectedUnit(unitInBox);

                return;
            }

            RegisterSelectedUnit(unitInBox);
        }

        public void UnselectUnitsOutsideDragSelectionBox(IUnit unitOutsideBox)
        {
            RemoveUnselectedUnit(unitOutsideBox);
        }

        public void ClearSelectedUnitsGroupOnDragBoxActive()
        {
            if(!isHoldingShift && !isHoldingCtrl)
            {
                RemoveAllSelectedUnits();
            }
        }

        public void ProcessUnitGroupClickSelection()
        {
            if(!enabled) return;

            if (DialogueManager.Instance)
            {
                if (DialogueManager.Instance.isConversationActive) return;
            }

            if (!EventSystem.current)
            {
                if(enabled) enabled = false;

                return;
            }

            if (Input.GetButtonDown("Fire1"))
            {
                raycastResults.Clear();

                pointerEventData.position = Input.mousePosition;

                EventSystem.current.RaycastAll(pointerEventData, raycastResults);

                //if click on nothing -> remove all selected plant units
                if (raycastResults.Count == 0)
                {
                    RemoveAllSelectedUnits();

                    return;
                }

                IUnit unit = null;

                for (int i = 0; i < raycastResults.Count; i++)
                {
                    if (!raycastResults[i].isValid) continue;

                    raycastResults[i].gameObject.TryGetComponent<IUnit>(out unit);

                    if(unit != null && unit is PlantUnit)
                    {
                        break;
                    }

                    //if click on an element that is not a unit or a plant unit -> remove all selected plant units

                    RemoveAllSelectedUnits();

                    return;
                }

                //if not holding shift or ctrl when selecting a plant unit AND the selected plant unit is NOT selected before ->
                //only select that new plant unit and deregister/remove any previously selected ones.
                //else, execute group functions and return.
                if (!isHoldingShift && !isHoldingCtrl)
                {
                    //if already selected -> return
                    if (unitGroupSelected.Contains(unit))
                    {
                        //DO selected plant group functions here (e.g open option box to bulk remove the selected plants)

                        return;
                    }

                    //if not already selected -> select only the clicked plant unit

                    RemoveAllSelectedUnits();

                    RegisterSelectedUnit(unit);
                }

                //if only holding shift when selecting a plant unit ->
                //add that plant unit to the set of selected plant units if not already
                if (isHoldingShift && !isHoldingCtrl)
                {
                    RegisterSelectedUnit(unit);
                }

                //if holding ctrl (even with or without shift being held down) when selecting a plant unit ->
                //toggle the selection of that plant unit (if alr selected -> remove OR if not alr selected -> add).
                if (isHoldingCtrl)
                {
                    ToggleUnitSelection(unit);
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

            RemoveUnselectedUnit(unselectableUnit);

            selectableUnits.Remove(unselectableUnit);

            DEBUG_selectableUnitsReadOnly.Remove(unselectableUnit.GetUnitTransform().gameObject);
        }
    }
}
