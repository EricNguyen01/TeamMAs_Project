// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using PixelCrushers.DialogueSystem;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEngine.UI.CanvasScaler;

namespace TeamMAsTD
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Canvas))]
    public class DragSelectionBoxUI : MonoBehaviour
    {
        [SerializeField] private Image dragSelectionAllowedArea;

        [SerializeField] private Image dragSelectionBoxImage;

        [Header("Debug Data")]

        [SerializeField]
        [ReadOnlyInspector]
        private bool canDrag = true;

        [SerializeField]
        [ReadOnlyInspector]
        private bool hasStartedDragging = false;

        [SerializeField]
        [ReadOnlyInspector]
        public bool hasHeldToDrag { get; private set; } = false;

        //INTERNALS....................................................................

        private Canvas dragSelectionCanvas;

        private CanvasGroup dragSelectionCanvasGroup;

        private CanvasScaler dragSelectionCanvasScaler;

        //private GraphicRaycaster graphicRaycaster;

        private List<RaycastResult> raycastResults = new List<RaycastResult>();

        private PointerEventData pointerEventData;

        private UnitGroupSelectionManager unitGroupSelectionManager;

        private Vector3 startSelectionMousePos = Vector3.zero;

        private Vector3 mouseDragStartPosWorld = Vector3.zero;

        private float selectionBoxWidth = 0.0f;

        private float selectionBoxHeight = 0.0f;

        private bool dragSelectionBoxEnabled = true;

        private HashSet<IUnit> unitsInDragSelectionBox = new HashSet<IUnit>();

        private void Awake()
        {
            TryGetComponent<Canvas>(out dragSelectionCanvas);

            if (!dragSelectionCanvas)
            {
                Debug.LogWarning("Drag Selection UI Script Component Is Not Currently Being Attached To A UI Canvas Object. Disabling Script!");

                enabled = false;
            }

            if (dragSelectionCanvas.renderMode != RenderMode.ScreenSpaceCamera)
                dragSelectionCanvas.renderMode = RenderMode.ScreenSpaceCamera;

            if (!dragSelectionCanvas.worldCamera) dragSelectionCanvas.worldCamera = Camera.main;

            if (!dragSelectionCanvas.worldCamera)
            {
                Debug.LogWarning("Drag Selection UI Canvas doesn't have a camera component ref assigned. Disabling Script!");

                enabled = false;
            }

            if (!TryGetComponent<CanvasGroup>(out dragSelectionCanvasGroup))
            {
                dragSelectionCanvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            dragSelectionCanvasGroup.interactable = false;

            dragSelectionCanvasGroup.blocksRaycasts = true;

            if (!TryGetComponent<CanvasScaler>(out dragSelectionCanvasScaler))
            {
                dragSelectionCanvasScaler = dragSelectionCanvas.gameObject.AddComponent<CanvasScaler>();
            }

            dragSelectionCanvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;

            dragSelectionCanvasScaler.scaleFactor = 1f;

            dragSelectionCanvasScaler.referencePixelsPerUnit = 100.0f;

            /*if(!TryGetComponent<GraphicRaycaster>(out graphicRaycaster))
            {
                graphicRaycaster = dragSelectionCanvas.gameObject.AddComponent<GraphicRaycaster>();
            }*/

            if (!dragSelectionBoxImage)
            {
                GameObject boxImgObj = new GameObject("DragSelectionMaskImage");

                boxImgObj.transform.SetParent(transform);

                boxImgObj.transform.localPosition = Vector3.zero;

                boxImgObj.AddComponent<RectTransform>();

                Image img = boxImgObj.AddComponent<Image>();

                dragSelectionBoxImage = img;
            }

            if (dragSelectionBoxImage)
            {
                dragSelectionBoxImage.raycastTarget = false;
            }

            if (!dragSelectionAllowedArea)
            {
                Debug.LogWarning("DragSelectionBoxUI: " + name + " Doesn't have a specified drag selection allowed area.\n" +
                "The player can create a drag selection box anywhere on the screen which could cause unwanted behaviors!");
            }
            else
            {
                dragSelectionAllowedArea.sprite = null;

                dragSelectionAllowedArea.color = Color.clear;

                dragSelectionAllowedArea.raycastTarget = true;

                dragSelectionAllowedArea.gameObject.layer = LayerMask.GetMask("Default");
            }
        }

        private void OnEnable()
        {
            if (!unitGroupSelectionManager)
            {
                enabled = false;

                return;
            }

            if (dragSelectionBoxImage)
            {
                dragSelectionBoxImage.rectTransform.anchorMin = Vector3.zero;

                dragSelectionBoxImage.rectTransform.anchorMax = Vector3.zero;

                dragSelectionBoxImage.rectTransform.pivot = new Vector2(0.0f, 1.0f);

                dragSelectionBoxImage.rectTransform.localScale = Vector3.one;

                dragSelectionBoxImage.raycastTarget = false;
            }

            if (dragSelectionCanvasGroup) dragSelectionCanvasGroup.alpha = 0.0f;

            if (!EventSystem.current)
            {
                enabled = false;

                return;
            }

            if (dragSelectionAllowedArea && !dragSelectionAllowedArea.enabled)
            {
                dragSelectionAllowedArea.enabled = true;
            }

            pointerEventData = new PointerEventData(EventSystem.current);
        }

        private void OnDisable()
        {
            if (dragSelectionAllowedArea && dragSelectionAllowedArea.enabled)
            {
                dragSelectionAllowedArea.enabled = false;
            }
        }

        private void Update()
        {
            if (!enabled || !dragSelectionBoxEnabled) return;

            if (!EventSystem.current || !dragSelectionCanvas.worldCamera /*|| !graphicRaycaster*/ )
            {
                enabled = false;

                return;
            }

            if (DialogueManager.Instance)
            {
                if (DialogueManager.Instance.isConversationActive)
                {
                    if (hasStartedDragging)
                    {
                        EndDrag();
                    }

                    return;
                }
            }

            CheckIf_DragOccursInDragAllowedArea_ToCreateDragSelectionBox();

#if ENABLE_LEGACY_INPUT_MANAGER

            if (Input.GetButtonDown("Fire1"))
            {
                hasHeldToDrag = false;

                BeginDrag();
            }
            else if (Input.GetButton("Fire1"))
            {
                OnDrag();
            }
            else if (Input.GetButtonUp("Fire1"))
            {
                EndDrag();
            }

#endif
        }

        private void BeginDrag()
        {
            if (!enabled) return;

            if (!canDrag) return;

            if (hasStartedDragging) return;

            hasStartedDragging = true;

            selectionBoxWidth = 0.0f;

            selectionBoxHeight = 0.0f;

            dragSelectionBoxImage.rectTransform.localScale = Vector3.one;

            dragSelectionCanvasGroup.alpha = 1.0f;

            //selection box has size of 0 on begin drag
            dragSelectionBoxImage.rectTransform.sizeDelta = Vector3.zero;

            dragSelectionBoxImage.rectTransform.anchoredPosition = Input.mousePosition;

            startSelectionMousePos = Input.mousePosition;

            if (dragSelectionCanvas.worldCamera)
            {
                mouseDragStartPosWorld = dragSelectionCanvas.worldCamera.ScreenToWorldPoint(startSelectionMousePos);
            }

            ClearAllUnitsInDragSelectionBox();
        }

        private void OnDrag()
        {
            if (!enabled) return;

            if (!canDrag) return;

            if (!hasStartedDragging) return;

            Vector3 localScale = dragSelectionBoxImage.rectTransform.localScale;

            selectionBoxWidth = Input.mousePosition.x - startSelectionMousePos.x;

            //if selection box's width < 0 -> flip X scale to -1
            if (selectionBoxWidth < 0.0f && localScale.x > 0.0f) localScale.x *= -1.0f;

            //if width >= 0 -> flip X scale to 1
            else if (selectionBoxWidth >= 0.0f && localScale.x < 0.0f) localScale *= -1.0f;

            selectionBoxHeight = startSelectionMousePos.y - Input.mousePosition.y;

            //if selection box's height < 0 -> flip Y scale to -1
            if (selectionBoxHeight < 0.0f && localScale.y > 0.0f) localScale.y *= -1.0f;

            //if width >= 0 -> flip Y scale to 1
            else if (selectionBoxHeight >= 0.0f && localScale.y < 0.0f) localScale.y *= -1.0f;

            dragSelectionBoxImage.rectTransform.localScale = new Vector3(localScale.x, localScale.y, localScale.z);

            //adjusts selection box's size during drag
            dragSelectionBoxImage.rectTransform.sizeDelta = new Vector2(Mathf.Abs(selectionBoxWidth), Mathf.Abs(selectionBoxHeight));

            //units in/out box process.............

            Vector3 mouseDragCurrentPosWorld = Vector3.zero;

            //Only calculate "dragCurrentPosWorld" as that is the only mouse pos value of the box that is being changed during drag
            //"dragStartPosWorld" is already calculated in BeginDrag and cached.
            if (dragSelectionCanvas.worldCamera)
            {
                mouseDragCurrentPosWorld = dragSelectionCanvas.worldCamera.ScreenToWorldPoint(Input.mousePosition);
            }

            //Only considers drag box as active and processes drag box functionalities if either drag box width or height is >= 20.0f
            if (Mathf.Abs(selectionBoxWidth) >= 20.0f || Mathf.Abs(selectionBoxHeight) >= 20.0f)
            {
                if (!hasHeldToDrag)
                {
                    if (unitGroupSelectionManager) unitGroupSelectionManager.ClearSelectedUnitsGroupOnDragBoxActive();
                }

                hasHeldToDrag = true;

                //if drag box just span over the current checking unit or the unit just somehow happened to be inside the box -> add the unit
                //also add properly in unit group selection manager
                ProcessUnitsInsideDragSelectionBox(mouseDragCurrentPosWorld);

                //if a unit that USED to be Inside the drag box BUT is now Outside of box -> remove it.
                //ONLY PROCESS units that were PREVIOUSLY INSIDE box and are NOW OUTSIDE to avoid BUGS!
                ProcessUnits_UsedToBeInside_ButNowOutside_DragSelectionBox(mouseDragCurrentPosWorld);
            }
        }

        private void EndDrag()
        {
            if (!enabled) return;

            if (!hasStartedDragging) return;

            hasStartedDragging = false;

            dragSelectionCanvasGroup.alpha = 0.0f;

            dragSelectionBoxImage.rectTransform.localScale = Vector3.one;

            //selection box has size of 0 on end drag
            dragSelectionBoxImage.rectTransform.sizeDelta = Vector3.zero;

            startSelectionMousePos = Vector2.zero;

            mouseDragStartPosWorld = Vector3.zero;
        }

        //if drag box just span over the current checking unit or the unit just somehow happened to be inside the box -> add the unit
        //also add properly in unit group selection manager
        private void ProcessUnitsInsideDragSelectionBox(Vector3 mouseDragCurrentPosWorld)
        {
            if (!enabled) return;

            if(mouseDragCurrentPosWorld == Vector3.zero) return;

            if (!unitGroupSelectionManager) return;

            if (unitGroupSelectionManager.selectableUnits == null ||
                unitGroupSelectionManager.selectableUnits.Count == 0) return;

            foreach (IUnit unit in unitGroupSelectionManager.selectableUnits)
            {
                if (unit == null) continue;

                //if drag box just span over the current checking unit or the unit just somehow happened to be inside the box -> add the unit
                //also add properly in unit group selection manager
                if (IsUnitInsideBox(unit, mouseDragCurrentPosWorld))
                {
                    if (!unitsInDragSelectionBox.Contains(unit))
                    {
                        unitsInDragSelectionBox.Add(unit);

                        unitGroupSelectionManager.SelectUnitsInDragSelectionBox(unit);
                    }
                }
            }
        }

        //if a unit that USED to be Inside the drag box BUT is now Outside of box -> remove it.
        //ONLY PROCESS units that were PREVIOUSLY INSIDE box and are NOW OUTSIDE to avoid BUGS!
        private void ProcessUnits_UsedToBeInside_ButNowOutside_DragSelectionBox(Vector3 mouseDragCurrentPosWorld)
        {
            if (!enabled) return;

            if (mouseDragCurrentPosWorld == Vector3.zero) return;

            if (unitsInDragSelectionBox == null || unitsInDragSelectionBox.Count == 0) return;

            if (!unitGroupSelectionManager) return;

            List<IUnit> units = unitsInDragSelectionBox.ToList();

            for(int i = 0; i < units.Count; i++)
            {
                if (units[i] == null) continue;

                if (!IsUnitInsideBox(units[i], mouseDragCurrentPosWorld))
                {
                    RemoveUnitFromDragSelectionBox(units[i]);

                    unitGroupSelectionManager.UnselectUnitsOutsideDragSelectionBox(units[i]);
                }
            }
        }

        private bool IsUnitInsideBox(IUnit unit, Vector3 mouseDragCurrentPosWorld)
        {
            if (unit == null) return false;

            if (mouseDragCurrentPosWorld == Vector3.zero) return false;

            Vector3 unitPos = unit.GetUnitTransform().position;

            bool unitInBoxWidth = false;

            bool unitInBoxHeight = false;

            if (mouseDragCurrentPosWorld.x >= mouseDragStartPosWorld.x)
            {
                if (unitPos.x >= mouseDragStartPosWorld.x && unitPos.x <= mouseDragCurrentPosWorld.x)
                    unitInBoxWidth = true;
            }
            else if (mouseDragCurrentPosWorld.x < mouseDragStartPosWorld.x)
            {
                if (unitPos.x <= mouseDragStartPosWorld.x && unitPos.x >= mouseDragCurrentPosWorld.x)
                    unitInBoxWidth = true;
            }

            if (mouseDragCurrentPosWorld.y <= mouseDragStartPosWorld.y)
            {
                if (unitPos.y <= mouseDragStartPosWorld.y && unitPos.y >= mouseDragCurrentPosWorld.y)
                    unitInBoxHeight = true;
            }
            else if (mouseDragCurrentPosWorld.y > mouseDragStartPosWorld.y)
            {
                if (unitPos.y >= mouseDragStartPosWorld.y && unitPos.y <= mouseDragCurrentPosWorld.y)
                    unitInBoxHeight = true;
            }

            if (unitInBoxWidth && unitInBoxHeight) return true;

            return false;
        }

        public void InitDragSelectionBoxUI(UnitGroupSelectionManager unitGroupSelectionManager)
        {
            if (!unitGroupSelectionManager) return;

            this.unitGroupSelectionManager = unitGroupSelectionManager;

            if (unitGroupSelectionManager.dragSelectionBoxUI)
            {
                if (unitGroupSelectionManager.dragSelectionBoxUI != this)
                {
                    enabled = false;

                    this.unitGroupSelectionManager = null;

                    return;
                }
            }

            if (!enabled) enabled = true;
        }

        private void CheckIf_DragOccursInDragAllowedArea_ToCreateDragSelectionBox()
        {
            if (!enabled) return;

            if (!dragSelectionAllowedArea) return;

            raycastResults.Clear();

            pointerEventData.position = Input.mousePosition;

            EventSystem.current.RaycastAll(pointerEventData, raycastResults);

            if(raycastResults.Count == 0)
            {
                canDrag = false;
            }
            else
            {
                bool dragable = false;

                for (int i = 0; i < raycastResults.Count; i++)
                {
                    if (!raycastResults[i].isValid) continue;

                    if (raycastResults[i].gameObject.layer == LayerMask.GetMask("UI") ||
                        raycastResults[i].gameObject.layer == LayerMask.NameToLayer("UI"))
                    {
                        if(raycastResults[i].gameObject.transform.root.name.Contains("Grid") ||
                           raycastResults[i].gameObject.transform.root.name.Contains("Tile"))
                        {
                            continue;
                        }

                        break;
                    }

                    if (raycastResults[i].sortingLayer == dragSelectionCanvas.sortingLayerID &&
                        raycastResults[i].sortingOrder > dragSelectionCanvas.sortingOrder)
                        break;

                    if (raycastResults[i].gameObject == dragSelectionAllowedArea.gameObject)
                    {
                        dragable = true;

                        break;
                    }
                }

                canDrag = dragable;
            }

            if (!canDrag && hasStartedDragging) EndDrag();//On pointer enters a UI elements -> end drag selection if already dragging
        }

        public void ClearAllUnitsInDragSelectionBox()
        {
            if(unitsInDragSelectionBox == null || unitsInDragSelectionBox.Count == 0) return;

            unitsInDragSelectionBox.Clear();
        }

        public void RemoveUnitFromDragSelectionBox(IUnit unselectedUnit)
        {
            if (unitsInDragSelectionBox == null || unitsInDragSelectionBox.Count == 0) return;

            if (unitsInDragSelectionBox.Contains(unselectedUnit))
            {
                unitsInDragSelectionBox.Remove(unselectedUnit);
            }
        }

        public int GetUnitsInDragSelectionBoxCount()
        {
            if (unitsInDragSelectionBox == null || unitsInDragSelectionBox.Count == 0) return 0;

            return unitsInDragSelectionBox.Count;
        }

        public void EnableDragSelectionBox(bool enabled)
        {
            dragSelectionBoxEnabled = enabled;
        }
    }
}
