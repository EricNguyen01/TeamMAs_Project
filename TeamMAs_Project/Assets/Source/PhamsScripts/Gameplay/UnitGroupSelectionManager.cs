// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using PixelCrushers.DialogueSystem;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

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

        [HideInInspector]
        private Vector3 selectedUnitTilesCenterWorldPos;

        private void Awake()
        {
            if (unitGroupSelectionManagerInstance && unitGroupSelectionManagerInstance != this)
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

            Rain.OnRainStarted += (Rain r) => EnableUnitGroupSelection(false, true);

            Rain.OnRainEnded += (Rain r) => EnableUnitGroupSelection(true);
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
        }

        private void OnDisable()
        {
            if (dragSelectionBoxUI && dragSelectionBoxUI.enabled) dragSelectionBoxUI.enabled = false;

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

            Rain.OnRainStarted -= (Rain r) => EnableUnitGroupSelection(false, true);

            Rain.OnRainEnded -= (Rain r) => EnableUnitGroupSelection(true);
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

                    if (plantUnit.plantRangeCircle) plantUnit.plantRangeCircle.DisplayPlantRangeCircle(true);

                    if (plantUnit.GetTileUnitIsOn())
                    {
                        if (plantUnit.GetTileUnitIsOn().tileGlowComp)
                        {
                            if (plantUnit.GetTileUnitIsOn().tileGlowComp.spriteGlowEffectComp)
                            {
                                plantUnit.GetTileUnitIsOn().tileGlowComp.spriteGlowEffectComp.OutlineWidth = 5;
                            }

                            plantUnit.GetTileUnitIsOn().tileGlowComp.OverrideTileGlowEffectColor(Color.cyan, Color.red);

                            plantUnit.GetTileUnitIsOn().tileGlowComp.OverrideTileGlowEffectBrightnessFromTo(0.55f, 2.2f);

                            if (!plantUnit.GetTileUnitIsOn().tileGlowComp.isTileGlowing)
                                plantUnit.GetTileUnitIsOn().tileGlowComp.EnableTileGlowEffect(TileGlow.TileGlowMode.PositiveGlow);
                        }

                        //update total multi-selected plants watering cost on adding new selected plant
                        if (plantUnit.tilePlacedOn.wateringOnTileScriptComp)
                        {
                            plantUnit.tilePlacedOn.wateringOnTileScriptComp.AddNewMultiSelectedPlantWateringCost(plantUnit);
                        }

                        TileMenuAndUprootOnTileUI tileMenu = plantUnit.GetTileUnitIsOn().tileMenuAndUprootOnTileUI;

                        if (TileMenuInteractionHandler.tileMenuInteractionHandlerInstance)
                        {
                            if (TileMenuInteractionHandler.tileMenuInteractionHandlerInstance.isCheckingForTileMenuInteractions)
                            {
                                TileMenuInteractionHandler.tileMenuInteractionHandlerInstance.EnableCheckForTileMenuInteractions(false);
                            }

                            if (!GetAndOpenClose_CenterTile_InSelectedTiles_IfPossible(true) && 
                                tileMenu && 
                                !tileMenu.isOpened)
                            {
                                TileMenuInteractionHandler.tileMenuInteractionHandlerInstance.ForceSetTileMenuInteracted_External(tileMenu, TileMenuInteractionHandler.TileMenuInteractionOptions.Open);
                            }

                            if (TileMenuInteractionHandler.tileMenuInteractionHandlerInstance.lastTileMenuInUse &&
                                unitGroupSelected.Count >= 2)
                            {
                                TileMenuInteractionHandler.tileMenuInteractionHandlerInstance.lastTileMenuInUse.tileHoldingThisMenu.plantUnitOnTile.plantRangeCircle.DisplayPlantRangeCircle(true);
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

                        if (plantUnit.plantRangeCircle) plantUnit.plantRangeCircle.DisplayPlantRangeCircle(false);

                        if (plantUnit.GetTileUnitIsOn())
                        {
                            if (plantUnit.GetTileUnitIsOn().tileGlowComp)
                            {
                                if (plantUnit.GetTileUnitIsOn().tileGlowComp.isTileGlowing)
                                    plantUnit.GetTileUnitIsOn().tileGlowComp.DisableTileGlowEffect();

                                if (plantUnit.GetTileUnitIsOn().tileGlowComp.spriteGlowEffectComp)
                                {
                                    plantUnit.GetTileUnitIsOn().tileGlowComp.spriteGlowEffectComp.SetDefaultRuntimeValues();
                                }

                                plantUnit.GetTileUnitIsOn().tileGlowComp.SetDefaultRuntimeValues();
                            }

                            //update total multi-selected plants watering cost on removing unselected plant
                            if (plantUnit.tilePlacedOn.wateringOnTileScriptComp)
                            {
                                plantUnit.tilePlacedOn.wateringOnTileScriptComp.SubtractUnselectedPlantFromMultiSelectWateringCost(plantUnit);
                            }

                            GetAndOpenClose_CenterTile_InSelectedTiles_IfPossible(true);

                            TileMenuAndUprootOnTileUI tileMenu = plantUnit.GetTileUnitIsOn().tileMenuAndUprootOnTileUI;

                            if (TileMenuInteractionHandler.tileMenuInteractionHandlerInstance)
                            {
                                if (tileMenu)
                                {
                                    if (tileMenu.isOpened)
                                    {
                                        TileMenuInteractionHandler.tileMenuInteractionHandlerInstance.ForceSetTileMenuInteracted_External(tileMenu, TileMenuInteractionHandler.TileMenuInteractionOptions.Close);

                                        shouldOpenNextInLineTileMenu = true;
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
                            TileMenuInteractionHandler.tileMenuInteractionHandlerInstance.ForceSetTileMenuInteracted_External(tileMenu, TileMenuInteractionHandler.TileMenuInteractionOptions.Open);
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
                        /*if (raycastResults[i].gameObject.GetComponent<Button>())*/
            return;
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
                                        TileMenuInteractionHandler.tileMenuInteractionHandlerInstance.ForceSetTileMenuInteracted_External(tileMenu, TileMenuInteractionHandler.TileMenuInteractionOptions.Open);
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

        private Tile GetOrUpdate_CenterUnitTile_InAllSelected(out Vector3 selectedUnitTilesCenterWorldPos)
        {
            selectedUnitTilesCenterWorldPos = Vector3.zero;

            if(unitGroupSelected == null || unitGroupSelected.Count < 1) return null;

            int minPosX = 0;

            int maxPosX = 0;

            int minPosY = 0;

            int maxPosY = 0;

            int totalPosX = 0;

            int totalPosY = 0;

            float minPosXWorld = 0.0f;

            float maxPosXWorld = 0.0f;

            float minPosYWorld = 0.0f;

            float maxPosYWorld = 0.0f;

            float totalPosXWorld = 0.0f;

            float totalPosYWorld = 0.0f;

            List<IUnit> unitGroupSelectedList = unitGroupSelected.ToList();

            for (int i = 0; i < unitGroupSelectedList.Count; i++)
            {
                if (unitGroupSelectedList[i] == null) continue;

                if (unitGroupSelectedList[i].GetTileUnitIsOn().tileNumInRow < minPosX)
                    minPosX = unitGroupSelectedList[i].GetTileUnitIsOn().tileNumInRow;

                if (unitGroupSelectedList[i].GetTileUnitIsOn().tileNumInRow > maxPosX)
                    maxPosX = unitGroupSelectedList[i].GetTileUnitIsOn().tileNumInRow;

                if (unitGroupSelectedList[i].GetTileUnitIsOn().tileNumInColumn < minPosY)
                    minPosY = unitGroupSelectedList[i].GetTileUnitIsOn().tileNumInColumn;

                if (unitGroupSelectedList[i].GetTileUnitIsOn().tileNumInColumn > maxPosY)
                    maxPosY = unitGroupSelectedList[i].GetTileUnitIsOn().tileNumInColumn;

                if(minPosXWorld == 0.0f)
                    minPosXWorld = unitGroupSelectedList[i].GetTileUnitIsOn().transform.position.x;

                else if (unitGroupSelectedList[i].GetTileUnitIsOn().transform.position.x < minPosXWorld)
                    minPosXWorld = unitGroupSelectedList[i].GetTileUnitIsOn().transform.position.x;

                if(maxPosXWorld == 0.0f)
                    maxPosXWorld = unitGroupSelectedList[i].GetTileUnitIsOn().transform.position.x;

                else if (unitGroupSelectedList[i].GetTileUnitIsOn().transform.position.x > maxPosXWorld)
                    maxPosXWorld = unitGroupSelectedList[i].GetTileUnitIsOn().transform.position.x;

                if(minPosYWorld == 0.0f)
                    minPosYWorld = unitGroupSelectedList[i].GetTileUnitIsOn().transform.position.y;

                else if (unitGroupSelectedList[i].GetTileUnitIsOn().transform.position.y < minPosYWorld)
                    minPosYWorld = unitGroupSelectedList[i].GetTileUnitIsOn().transform.position.y;

                if(maxPosYWorld == 0.0f)
                    maxPosYWorld = unitGroupSelectedList[i].GetTileUnitIsOn().transform.position.y;

                else if (unitGroupSelectedList[i].GetTileUnitIsOn().transform.position.y > maxPosYWorld)
                    maxPosYWorld = unitGroupSelectedList[i].GetTileUnitIsOn().transform.position.y;
            }

            totalPosX = minPosX + maxPosX;

            totalPosY = minPosY + maxPosY;

            totalPosXWorld = minPosXWorld + maxPosXWorld;

            totalPosYWorld = minPosYWorld + maxPosYWorld;

            Vector2Int selectedUnitTilesCenterPos = new Vector2Int(totalPosX / 2, totalPosY / 2);

            selectedUnitTilesCenterWorldPos = new Vector3(totalPosXWorld / 2.0f, totalPosYWorld / 2.0f, 0.0f);

            Tile centerTile = null;

            float closestDist = 0;   

            for(int i = 0; i < unitGroupSelectedList.Count; i++)
            {
                if (unitGroupSelectedList[i] == null) continue;

                int x = unitGroupSelectedList[i].GetTileUnitIsOn().tileNumInRow;

                int y = unitGroupSelectedList[i].GetTileUnitIsOn().tileNumInColumn;

                Vector2Int tileCoord = new Vector2Int(x, y);

                if(i == 0)
                {
                    closestDist = Vector2Int.Distance(selectedUnitTilesCenterPos, tileCoord);

                    centerTile = unitGroupSelectedList[i].GetTileUnitIsOn();

                    continue;
                }

                float currentTileDistToCenter = Vector2Int.Distance(selectedUnitTilesCenterPos, tileCoord);

                if (currentTileDistToCenter <= closestDist)
                {
                    closestDist = currentTileDistToCenter;

                    centerTile = unitGroupSelectedList[i].GetTileUnitIsOn();
                }
            }

            return centerTile;
        }

        private bool GetAndOpenClose_CenterTile_InSelectedTiles_IfPossible(bool openMenu)
        {
            if (!TileMenuInteractionHandler.tileMenuInteractionHandlerInstance) return false;

            Tile centerTile = GetOrUpdate_CenterUnitTile_InAllSelected(out selectedUnitTilesCenterWorldPos);

            if(!centerTile) return false;

            if(!centerTile.tileMenuAndUprootOnTileUI) return false;

            TileMenuAndUprootOnTileUI centerTileMenu = centerTile.tileMenuAndUprootOnTileUI;

            if (openMenu)
            {
                TileMenuInteractionHandler.tileMenuInteractionHandlerInstance.ForceSetTileMenuInteracted_External(centerTileMenu, TileMenuInteractionHandler.TileMenuInteractionOptions.Open);

                if (unitGroupSelected.Count > 1 &&
                    selectedUnitTilesCenterWorldPos != Vector3.zero)
                    TileMenuInteractionHandler.tileMenuInteractionHandlerInstance.tileMenuInUse.SetTileMenuLocalPos(selectedUnitTilesCenterWorldPos, true);

                return true;
            }

            TileMenuInteractionHandler.tileMenuInteractionHandlerInstance.ForceSetTileMenuInteracted_External(centerTileMenu, TileMenuInteractionHandler.TileMenuInteractionOptions.Close);

            return true;
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

        public void EnableUnitGroupSelection(bool enabled, bool removeAllSelectedUnitsIfDisable = false)
        {
            unitGroupSelectionEnabled = enabled;
            
            if (dragSelectionBoxUI)
            {
                dragSelectionBoxUI.EnableDragSelectionBox(enabled);
            }

            if (!enabled)
            {
                if (removeAllSelectedUnitsIfDisable) RemoveAllSelectedUnits();
            }
        }
    }
}
