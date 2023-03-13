using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using UnityEngine.WSA;

namespace TeamMAsTD
{
    [DisallowMultipleComponent]
    public class UnitShopSlotUIDragDrop : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] [Tooltip("The Unit Scriptable Object of this slot.")] 
        private PlantUnitSO slotUnitScriptableObject;

        [SerializeField] private Image unitThumbnailImage;
        [SerializeField] private TextMeshProUGUI unitNameDisplay;
        [SerializeField] private TextMeshProUGUI unitCostDisplay;

        [SerializeField]
        [Tooltip("The object with the UI Image component attached that will follow the mouse when dragging. " +
        "Dropping returns the object to its original position.")]
        private Image dragDropUIImageObject;

        private GameObject plantOnTileMockUpVisualizationObj;

        [SerializeField] [Range(0.0f, 1.0f)] private float dragDropBlurAmount = 0.6f;

        [SerializeField]
        [Tooltip("Are we using the unit shop slot thumbnail sprite from the unit scriptable object " +
        "or placing a different one directly to the UI Image component attached to the same UI object as this script? " +
        "The default value is true")]
        private bool useUnitThumbnailFromScriptableObject = true;

        [SerializeField] 
        [Tooltip("Is the UI image when dragging and dropping the same as the UI Image of this shop slot's image? " +
        "If an image is set for the drag drop UI object, setting this option to true will override it. The default setting is true.")] 
        private bool dragDropVisualSameAsShopSlots = true;

        //UnityEvents..................................................................................

        [SerializeField] public UnityEvent OnStartedDragging;
        [SerializeField] public UnityEvent OnDroppedSuccessful;
        [SerializeField] public UnityEvent OnDroppedFailed;

        //INTERNALS....................................................................................

        //The Image UI component with empty sprite and 0 alpha to use for EventSystem raycast detection
        //Must be attached to the same obj as this script.
        private Image unitShopSlotImageRaycastComponent;

        //the original position of the dragDropUIImageObject (obj with UI Image that moves with mouse when dragging)
        private Vector3 originalDragDropPos;

        //the top most UI Canvas component that houses the rest of the children UI elements
        private Canvas parentCanva;
        //the Rect Transform of the top most parent UI canva above
        private RectTransform parentCanvaRect;

        private PlantRangeCircle plantRangeCircleIndicator;

        private Tile currentTileBeingDraggedOver;

        private Tile[] tilesUseForPlantableGlowEffectOnDragDrop;

        //PRIVATES.........................................................................

        private void Awake()
        {
            //check for unit scriptable object data
            if (slotUnitScriptableObject == null)
            {
                Debug.LogError("Unit ScriptableObject data for this unit shop slot: " + name + " is missing! Disabling shop slot object!");
                gameObject.SetActive(false);
                return;
            }

            SetImageUIRaycastComponent();
            SetUnitShopSlotImageFromUnitSO();
            CheckAndGetDragDropRequirements();
            SetNameAndCostForUnitShopSlot();
        }

        private void OnEnable()
        {
            //check for an existing EventSystem and disble script if null
            if (FindObjectOfType<EventSystem>() == null)
            {
                Debug.LogError("Cannot find an EventSystem in the scene. " +
                "An EventSystem is required for shop unit slot UI drag/drop to function. Disabling shop slot object!");
                gameObject.SetActive(false);
                return;
            }
        }

        private void Start()
        {
            CreatePlantRangeCircleIndicatorOnStart();

            originalDragDropPos = dragDropUIImageObject.transform.localPosition;
        }

        private void SetImageUIRaycastComponent()
        {
            unitShopSlotImageRaycastComponent = GetComponent<Image>();

            if (unitShopSlotImageRaycastComponent == null)
            {
                unitShopSlotImageRaycastComponent = gameObject.AddComponent<Image>();
                var color = unitShopSlotImageRaycastComponent.color;
                color.a = 0.0f;
                unitShopSlotImageRaycastComponent.color = color;
            }
        }

        private void SetUnitShopSlotImageFromUnitSO()
        {
            if (unitThumbnailImage == null)
            {
                Debug.LogError("Unit thumbnail on shop slot UI obj: " + name + " is not assigned! Unit thumbnail for slot will not be displayed!");
                return;
            }

            if (!useUnitThumbnailFromScriptableObject || slotUnitScriptableObject == null) return;

            //if a unit thumbnail is assigned in the unit SO
            if (slotUnitScriptableObject.unitThumbnail != null)
            {
                unitThumbnailImage.sprite = slotUnitScriptableObject.unitThumbnail;
            }
            else
            {
                //else if theres no unit thumbnail because we are still in placeholder phase->only use the sprite and color from unit prefab
                //if a unit prefab is also not assigned in the unit SO->do nothing and exit
                if (slotUnitScriptableObject.unitPrefab == null) return;
                //else get the sprite and color from prefab 
                SpriteRenderer unitPrefabSpriteRenderer = slotUnitScriptableObject.unitPrefab.GetComponent<SpriteRenderer>();

                unitThumbnailImage.sprite = unitPrefabSpriteRenderer.sprite;
                unitThumbnailImage.color = unitPrefabSpriteRenderer.color;
            }
        }

        private void CheckAndGetDragDropRequirements()
        {
            //check for drag drop Image UI object and its CanvasGroup component
            if(dragDropUIImageObject == null)
            {
                Debug.LogError("Drag drop Image UI Object is not assigned on Unit Slot UI: " + name + ". Disabling drag/drop!");
                enabled = false;
                return;
            }

            CanvasGroup dragDropImageUICanvasGroup = dragDropUIImageObject.GetComponent<CanvasGroup>();

            if (dragDropImageUICanvasGroup == null)
            {
                dragDropUIImageObject.gameObject.AddComponent<CanvasGroup>();
            }

            SetDragDropSameVisualAsShopSlot();

            CreatePlantOnTileVisualizationMockUpFromDragDropObj(dragDropUIImageObject);

            dragDropImageUICanvasGroup.alpha = dragDropBlurAmount;

            //Get top most parent canvas component and canvas rect
            parentCanva = GetComponentInParent<Canvas>();

            parentCanvaRect = parentCanva.GetComponent<RectTransform>();
        }

        private void SetDragDropSameVisualAsShopSlot()
        {
            if (!dragDropVisualSameAsShopSlots) return;
            if(unitThumbnailImage == null)
            {
                Debug.LogError("The Image UI obj to drag/drop is set to use the same image as the unit thumbnail Image obj but unit thumbnail image obj is not assigned!");
                return;
            }

            dragDropUIImageObject.sprite = unitThumbnailImage.sprite;
            dragDropUIImageObject.color = unitThumbnailImage.color;
        }

        private void CreatePlantOnTileVisualizationMockUpFromDragDropObj(Image dragDropUIImageObj)
        {
            if (dragDropUIImageObj == null) return;

            plantOnTileMockUpVisualizationObj = Instantiate(dragDropUIImageObj.gameObject, transform);

            plantOnTileMockUpVisualizationObj.transform.localPosition = Vector3.zero;
        }

        private void SetNameAndCostForUnitShopSlot()
        {
            if(unitNameDisplay == null)
            {
                Debug.LogWarning("Unit Shop Slot name display text component is not assigned on slot: " + name);
            }
            else
            {
                unitNameDisplay.text = slotUnitScriptableObject.displayName;
            }

            if(unitCostDisplay == null)
            {
                Debug.LogWarning("Unit Shop Slot cost display text component is not assigned on slot: " + name);
            }
            else
            {
                unitCostDisplay.text = slotUnitScriptableObject.plantingCoinCost.ToString();
            }
        }

        private void ActiveChildrenObjectOfDragDropUIImageObj(bool active)
        {
            if (dragDropUIImageObject == null) return;

            if (dragDropUIImageObject.transform.childCount == 0) return;

            if (active)
            {
                for(int i = 0; i < dragDropUIImageObject.transform.childCount; i++)
                {
                    if (!dragDropUIImageObject.transform.GetChild(i).gameObject.activeInHierarchy)
                    {
                        dragDropUIImageObject.transform.GetChild(i).gameObject.SetActive(true);
                    }
                }

                return;
            }

            for (int i = 0; i < dragDropUIImageObject.transform.childCount; i++)
            {
                if (dragDropUIImageObject.transform.GetChild(i).gameObject.activeInHierarchy)
                {
                    dragDropUIImageObject.transform.GetChild(i).gameObject.SetActive(false);
                }
            }
        }

        private void CreatePlantRangeCircleIndicatorOnStart()
        {
            if (slotUnitScriptableObject == null) return;

            if (slotUnitScriptableObject.plantRangeCirclePrefab == null) return;

            plantRangeCircleIndicator = Instantiate(slotUnitScriptableObject.plantRangeCirclePrefab.gameObject).GetComponent<PlantRangeCircle>();

            plantRangeCircleIndicator.InitializePlantRangeCircle(slotUnitScriptableObject, false);

            plantRangeCircleIndicator.transform.position = Vector3.zero;

            plantRangeCircleIndicator.gameObject.SetActive(false);
        }

        private void EnableAndSet_PlantRangeCircle_To_WorldMousePos(Vector2 screenMousePos, bool enabled)
        {
            if (plantRangeCircleIndicator == null) return;

            if (enabled)
            {
                if (!plantRangeCircleIndicator.gameObject.activeInHierarchy) plantRangeCircleIndicator.gameObject.SetActive(true);

                plantRangeCircleIndicator.transform.position = screenMousePos;

                return;
            }

            if (plantRangeCircleIndicator.gameObject.activeInHierarchy) plantRangeCircleIndicator.gameObject.SetActive(false);
        }

        //This function is a little expensive and should not be used in a loop or in many frames consecutively
        private void FindAndEnableTilePlantableGlowEffect(bool enabled)
        {
            if (enabled)
            {
                tilesUseForPlantableGlowEffectOnDragDrop = FindObjectsOfType<Tile>();

                if (tilesUseForPlantableGlowEffectOnDragDrop == null || tilesUseForPlantableGlowEffectOnDragDrop.Length == 0) return;

                for(int i = 0; i < tilesUseForPlantableGlowEffectOnDragDrop.Length; i++)
                {
                    tilesUseForPlantableGlowEffectOnDragDrop[i].EnablePlantableTileGlowOnPlantDrag(slotUnitScriptableObject, true);
                }

                return;
            }

            if (tilesUseForPlantableGlowEffectOnDragDrop == null || tilesUseForPlantableGlowEffectOnDragDrop.Length == 0) return;

            for (int i = 0; i < tilesUseForPlantableGlowEffectOnDragDrop.Length; i++)
            {
                tilesUseForPlantableGlowEffectOnDragDrop[i].EnablePlantableTileGlowOnPlantDrag(slotUnitScriptableObject, false);
            }
        }

        public PlantUnitSO GetShopSlotPlantUnit()
        {
            return slotUnitScriptableObject;
        }

        //UnityEventSystem Drag/Drop Interface functions.........................................

        public void OnBeginDrag(PointerEventData eventData)
        {
            //On click and hold the mouse on the unit shop slot UI image:

            if(!dragDropUIImageObject.gameObject.activeInHierarchy) dragDropUIImageObject.gameObject.SetActive(true);

            dragDropUIImageObject.transform.SetParent(parentCanva.transform);

            ActiveChildrenObjectOfDragDropUIImageObj(true);

            if (plantOnTileMockUpVisualizationObj != null)
            {
                plantOnTileMockUpVisualizationObj.transform.SetParent(parentCanva.transform);

                if (plantOnTileMockUpVisualizationObj.activeInHierarchy) plantOnTileMockUpVisualizationObj.SetActive(false);
            }

            unitShopSlotImageRaycastComponent.raycastTarget = false;

            EventSystem.current.SetSelectedGameObject(null);

            Vector2 worldMousePos = parentCanva.worldCamera.ScreenToWorldPoint(Input.mousePosition);

            EnableAndSet_PlantRangeCircle_To_WorldMousePos(worldMousePos, true);

            FindAndEnableTilePlantableGlowEffect(true);

            OnStartedDragging?.Invoke();
        }

        public void OnDrag(PointerEventData eventData)
        {
            //On dragging while still holding the mouse:
            //fix the drag drop UI image object to the EventSystem mouse pointer (in dragDropUIImage UI space from screen space)
            Vector2 mousePosLocal;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(parentCanvaRect, eventData.position, parentCanva.worldCamera, out mousePosLocal);
            
            dragDropUIImageObject.transform.position = parentCanvaRect.transform.TransformPoint(mousePosLocal);

            Vector2 worldMousePos = parentCanva.worldCamera.ScreenToWorldPoint(Input.mousePosition);

            EnableAndSet_PlantRangeCircle_To_WorldMousePos(worldMousePos, true);

            if (plantOnTileMockUpVisualizationObj == null) return;

            if (eventData.pointerEnter == null)
            {
                plantOnTileMockUpVisualizationObj.SetActive(false);

                return;
            }

            //check if the mouse pointer is on an obj with Tile component attached
            Tile destinationTile = eventData.pointerEnter.GetComponent<Tile>();

            if (destinationTile == null || !destinationTile.CanPlaceUnit_EXTERNAL(slotUnitScriptableObject))
            {
                currentTileBeingDraggedOver = null;

                plantOnTileMockUpVisualizationObj.SetActive(false);

                return;
            }

            if(!plantOnTileMockUpVisualizationObj.activeInHierarchy) plantOnTileMockUpVisualizationObj.SetActive(true);

            bool shouldProcessMockupPos = false;

            Vector3 tilePosWorld;

            Vector3 tilePosScreenPnt;

            Vector2 tilePosUILocalPnt;

            if (currentTileBeingDraggedOver == null)
            {
                currentTileBeingDraggedOver = destinationTile;

                shouldProcessMockupPos = true;
            }
            else if (currentTileBeingDraggedOver != destinationTile)
            {
                currentTileBeingDraggedOver = destinationTile;

                shouldProcessMockupPos = true;
            }

            if (!shouldProcessMockupPos) return;

            tilePosWorld = currentTileBeingDraggedOver.transform.position;

            tilePosScreenPnt = parentCanva.worldCamera.WorldToScreenPoint(tilePosWorld);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(parentCanvaRect, tilePosScreenPnt, parentCanva.worldCamera, out tilePosUILocalPnt);

            plantOnTileMockUpVisualizationObj.transform.position = parentCanvaRect.transform.TransformPoint(tilePosUILocalPnt);

        }

        public void OnEndDrag(PointerEventData eventData)
        {
            //On releasing the mouse after dragging and holding:

            //Immediately return the drag/drop Image UI obj back to being this obj's children with original local pos
            //after that set it to inactive
            dragDropUIImageObject.transform.SetParent(transform);

            dragDropUIImageObject.transform.localPosition = originalDragDropPos;

            dragDropUIImageObject.gameObject.SetActive(false);

            if(currentTileBeingDraggedOver != null) currentTileBeingDraggedOver = null;

            if(plantOnTileMockUpVisualizationObj != null)
            {
                if (plantOnTileMockUpVisualizationObj.activeInHierarchy) plantOnTileMockUpVisualizationObj.SetActive(false);

                plantOnTileMockUpVisualizationObj.transform.SetParent(dragDropUIImageObject.transform);

                plantOnTileMockUpVisualizationObj.transform.localPosition = Vector3.zero;
            }

            ActiveChildrenObjectOfDragDropUIImageObj(false);

            //reset to prepare for next drag/drop
            unitShopSlotImageRaycastComponent.raycastTarget = true;

            EnableAndSet_PlantRangeCircle_To_WorldMousePos(Vector2.zero, false);

            //Check if the mouse is hovered upon anything that is recognized by the EventSystem
            if (eventData.pointerEnter == null)
            {
                FindAndEnableTilePlantableGlowEffect(false);

                OnDroppedFailed?.Invoke();
                return;
            }

            //check if the mouse pointer is on an obj with Tile component attached
            Tile destinationTile = eventData.pointerEnter.GetComponent<Tile>();

            if (destinationTile == null)
            {
                FindAndEnableTilePlantableGlowEffect(false);

                OnDroppedFailed?.Invoke();
                return;
            }

            FindAndEnableTilePlantableGlowEffect(false);

            //Place the unit on the destination tile (placeable conditions are checked within the PlaceUnit function below)
            destinationTile.PlaceUnit(slotUnitScriptableObject);

            OnDroppedSuccessful?.Invoke();
        }
    }
}
