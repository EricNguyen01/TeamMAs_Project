// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

namespace TeamMAsTD
{
    [DisallowMultipleComponent]
    public class UnitShopSlotUIDragDrop : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, ISaveable
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

        [SerializeField]
        [Tooltip("The object with the UI Image component attached that shows the red X to indicate that the currently dragged plant" +
        "is not plantable at mouse pos. This object must be the DragDropUIImageObject's child.")]
        private Image dropDenyImageObject;

        private GameObject plantOnTileMockUpVisualizationObj;

        [SerializeField] [Range(0.0f, 1.0f)] private float dragDropBlurAmount = 0.6f;

        [SerializeField] private Color dropAllowedColor = Color.green;

        [SerializeField] private Color dropDeniedColor = Color.red;

        private Color dropDefaultColor;

        [SerializeField]
        [Tooltip("Are we using the unit shop slot thumbnail sprite from the unit scriptable object " +
        "or placing a different one directly to the UI Image component attached to the same UI object as this script? " +
        "The default value is true")]
        private bool useUnitThumbnailFromScriptableObject = true;

        [SerializeField] 
        [Tooltip("Is the UI image when dragging and dropping the same as the UI Image of this shop slot's image? " +
        "If an image is set for the drag drop UI object, setting this option to true will override it. The default setting is true.")] 
        private bool dragDropVisualSameAsShopSlot = true;

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

        private CanvasGroup shopSlotCanvasGroup;

        private PlantRangeCircle plantRangeCircleIndicator;

        private Tile currentTileBeingDraggedOver;

        private Tile[] tilesUseForPlantableGlowEffectOnDragDrop;

        private WaveSO currentWaveSO;

        private bool isShopSlotUnlocked = false;

        private Vector2 dragDropUIImageObjectBaseRectSize;

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

            GetOrSetShopSlotCanvasGroup();
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

            WaveSpawner.OnWaveStarted += GetCurrentWaveToUnlockShopSlotOnWaveStarted;

            WaveSpawner.OnWaveFinished += GetCurrentWaveToUnlockShopSlotOnWaveFinished;

            if (!slotUnitScriptableObject.canPurchasePlant)
            {
                EnableShopSlotCanvasGroup(false, false, 0.5f);
            }
        }

        private void OnDestroy()
        {
            WaveSpawner.OnWaveStarted -= GetCurrentWaveToUnlockShopSlotOnWaveStarted;

            WaveSpawner.OnWaveFinished -= GetCurrentWaveToUnlockShopSlotOnWaveFinished;
        }

        private void Start()
        {
            CreatePlantRangeCircleIndicatorOnStart();

            originalDragDropPos = dragDropUIImageObject.transform.localPosition;

            if (slotUnitScriptableObject.plantPurchaseLockOnStart && !isShopSlotUnlocked)
            {
                gameObject.SetActive(false);
            }
            else
            {
                if (!gameObject.activeInHierarchy) gameObject.SetActive(true);

                isShopSlotUnlocked = true;
            }
        }

        private void GetOrSetShopSlotCanvasGroup()
        {
            shopSlotCanvasGroup = GetComponent<CanvasGroup>();

            if(shopSlotCanvasGroup == null)
            {
                shopSlotCanvasGroup= gameObject.AddComponent<CanvasGroup>();
            }
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
            //check if the important DragDropUIImageObject is null and if null,
            //instantiate a child obj with an added Image component and attempt to assign the default Unity Square sprite to it as placeholder
            if (!dragDropUIImageObject)
            {
                GameObject go = Instantiate(new GameObject(), Vector3.zero, Quaternion.Euler(Vector3.zero), transform);

                dragDropUIImageObject = go.AddComponent<Image>();

                dragDropUIImageObject.sprite = slotUnitScriptableObject.plantThumbnailPlaceholderSpr;
            }

            //if dragDropUIImageObject is still null -> disable script
            if(!dragDropUIImageObject)
            {
                Debug.LogError("Drag drop Image UI Object is not assigned on Unit Slot UI: " + name + ". Disabling shop drag/drop!");

                enabled = false;

                return;
            }

            dropDefaultColor = dragDropUIImageObject.color;

            dragDropUIImageObjectBaseRectSize = dragDropUIImageObject.rectTransform.sizeDelta;

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

            if(dropDenyImageObject) dropDenyImageObject.gameObject.SetActive(false);
        }

        private void SetDragDropSameVisualAsShopSlot()
        {
            if (!dragDropVisualSameAsShopSlot) return;

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

            if (plantOnTileMockUpVisualizationObj.transform.childCount == 0) return;

            for(int i = 0; i < plantOnTileMockUpVisualizationObj.transform.childCount; i++)
            {
                GameObject childObj = plantOnTileMockUpVisualizationObj.transform.GetChild(i).gameObject;

                Image childImage = childObj.GetComponent<Image>();

                if (!childImage) continue;

                if(dropDenyImageObject && childImage.sprite == dropDenyImageObject.sprite)
                {
                    Destroy(childImage.gameObject);

                    break;
                }
            }
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
                    if (dragDropUIImageObject.transform.GetChild(i).gameObject == dropDenyImageObject.gameObject) continue;

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

        private void EnableShopSlotCanvasGroup(bool enabled, bool affectChildrenCanvasGroup, float canvasGroupAlpha = 0.0f)
        {
            foreach (CanvasGroup cg in GetComponentsInChildren<CanvasGroup>(true))
            {
                if (cg == null || cg == shopSlotCanvasGroup) continue;

                if(affectChildrenCanvasGroup) cg.ignoreParentGroups = false;
                else cg.ignoreParentGroups = true;
            }

            if (enabled)
            {
                shopSlotCanvasGroup.interactable = true;

                shopSlotCanvasGroup.blocksRaycasts = true;

                if(canvasGroupAlpha == 0.0f) shopSlotCanvasGroup.alpha = 1.0f;
                else shopSlotCanvasGroup.alpha = canvasGroupAlpha;

                return;
            }

            shopSlotCanvasGroup.interactable = false;

            shopSlotCanvasGroup.blocksRaycasts = false;

            shopSlotCanvasGroup.alpha = canvasGroupAlpha;
        }

        private void GetCurrentWaveToUnlockShopSlotOnWaveStarted(WaveSpawner waveSpawner, int waveNum)
        {
            if (isShopSlotUnlocked) return;

            if (waveSpawner == null) return;

            Wave wave = waveSpawner.GetWave(waveNum);

            if (wave == null) return;

            currentWaveSO = wave.waveSO;

            if (currentWaveSO == null) return;

            //if starts and reaches the wave number that is required to unlock this shop slot -> unlock it then exit func
            if(slotUnitScriptableObject.waveToUnlockPlantPurchaseOnWaveStarted != null)
            {
                if(currentWaveSO == slotUnitScriptableObject.waveToUnlockPlantPurchaseOnWaveStarted)
                {
                    if (!gameObject.activeInHierarchy) gameObject.SetActive(true);

                    isShopSlotUnlocked = true;

                    return;
                }
            }

            if (slotUnitScriptableObject.waveToUnlockPlantPurchaseOnWaveStarted == null &&
                slotUnitScriptableObject.waveToUnlockPlantPurchaseOnWaveFinished == null) return;

            //below is logic for
            //unlocking this plant shop slot if the game starts where the current wave number has gone past 
            //the wave number that is required to unlock this shop slot.

            List<Wave> wavesList = waveSpawner.GetWaveSpawnerWaveList();

            int waveNumToUnlock = -1;

            if(wavesList != null && wavesList.Count > 0)
            {
                for(int i = 0; i < wavesList.Count; i++)
                {
                    if (wavesList[i] == null) continue;

                    if(slotUnitScriptableObject.waveToUnlockPlantPurchaseOnWaveStarted != null)
                    {
                        if (wavesList[i].waveSO == slotUnitScriptableObject.waveToUnlockPlantPurchaseOnWaveStarted)
                        {
                            waveNumToUnlock = i;

                            break;
                        }
                    }

                    if(slotUnitScriptableObject.waveToUnlockPlantPurchaseOnWaveFinished != null)
                    {
                        if(wavesList[i].waveSO == slotUnitScriptableObject.waveToUnlockPlantPurchaseOnWaveFinished)
                        {
                            waveNumToUnlock = i;

                            break;
                        }
                    }
                }
            }

            if(waveNumToUnlock >= 0)
            {
                if(waveNum > waveNumToUnlock)
                {
                    if (!gameObject.activeInHierarchy) gameObject.SetActive(true);

                    isShopSlotUnlocked = true;
                }
            }
        }

        private void GetCurrentWaveToUnlockShopSlotOnWaveFinished(WaveSpawner waveSpawner, int waveNum, bool hasOnGoingWave)
        {
            if (hasOnGoingWave) return;

            if (isShopSlotUnlocked) return;

            if (waveSpawner == null) return;

            Wave wave = waveSpawner.GetWave(waveNum);

            if (wave == null) return;

            currentWaveSO = wave.waveSO;

            if (currentWaveSO == null) return;

            if (slotUnitScriptableObject.waveToUnlockPlantPurchaseOnWaveFinished != null)
            {
                if (currentWaveSO == slotUnitScriptableObject.waveToUnlockPlantPurchaseOnWaveFinished)
                {
                    if (!gameObject.activeInHierarchy) gameObject.SetActive(true);

                    isShopSlotUnlocked = true;
                }
            }
        }

        public PlantUnitSO GetShopSlotPlantUnit()
        {
            return slotUnitScriptableObject;
        }

        //UnityEventSystem Drag/Drop Interface functions.........................................

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!enabled) return;

            //On click and hold the mouse on the unit shop slot UI image:
            dragDropUIImageObject.rectTransform.sizeDelta = dragDropUIImageObjectBaseRectSize;

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
            if (!enabled) return;

            //On dragging while still holding the mouse:

            //first, process the dragDropUIImageObject (the blurred plant icon following the mouse to indicate a plant's being dragged)
            //fix the drag drop UI image object to the EventSystem mouse pointer (in dragDropUIImage UI space from screen space)
            Vector2 mousePosLocal;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(parentCanvaRect, eventData.position, parentCanva.worldCamera, out mousePosLocal);
            
            dragDropUIImageObject.transform.position = parentCanvaRect.transform.TransformPoint(mousePosLocal);

            Vector2 worldMousePos = parentCanva.worldCamera.ScreenToWorldPoint(Input.mousePosition);

            EnableAndSet_PlantRangeCircle_To_WorldMousePos(worldMousePos, true);

            //process the plant planted mock-up visual on tile and other visual cues below:

            //if not hovering on anything:
            if (!eventData.pointerEnter)
            {
                if (plantOnTileMockUpVisualizationObj) plantOnTileMockUpVisualizationObj.SetActive(false);

                dragDropUIImageObject.color = dropDeniedColor;

                if (dropDenyImageObject && !dropDenyImageObject.gameObject.activeInHierarchy)
                {
                    dropDenyImageObject.gameObject.SetActive(true);
                }

                return;
            }

            //if hovering on smth:
            //check if the mouse pointer is on an obj with Tile component attached
            Tile destinationTile;

            eventData.pointerEnter.TryGetComponent<Tile>(out destinationTile);

            //if not hovering on a tile or a tile does not accept plant placement:
            if (!destinationTile || !destinationTile.CanPlaceUnit_EXTERNAL(slotUnitScriptableObject))
            {
                currentTileBeingDraggedOver = null;

                plantOnTileMockUpVisualizationObj.SetActive(false);

                dragDropUIImageObject.color = dropDeniedColor;

                if (dropDenyImageObject && !dropDenyImageObject.gameObject.activeInHierarchy)
                {
                    dropDenyImageObject.gameObject.SetActive(true);
                }

                return;
            }

            //else if hovering on a valid plantable tile:

            if(dropDenyImageObject) dropDenyImageObject.gameObject.SetActive(false);

            dragDropUIImageObject.color = dropAllowedColor;

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
            if (!enabled) return;

            //On releasing the mouse after dragging and holding:

            //Immediately return the drag/drop Image UI obj back to being this obj's children with original local pos
            //after that set it to inactive
            dragDropUIImageObject.transform.SetParent(transform);

            dragDropUIImageObject.transform.localPosition = originalDragDropPos;

            dragDropUIImageObject.color = dropDefaultColor;

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
            Tile destinationTile;

            if (!eventData.pointerEnter.TryGetComponent<Tile>(out destinationTile))
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

        public SaveDataSerializeBase SaveData(string saveName = "")
        {
            SaveDataSerializeBase shopSlotSaveData;

            shopSlotSaveData = new SaveDataSerializeBase(isShopSlotUnlocked, 
                                                         transform.position, 
                                                         UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);

            return shopSlotSaveData;
        }

        public void LoadData(SaveDataSerializeBase savedDataToLoad)
        {
            if (savedDataToLoad == null) return;
            
            isShopSlotUnlocked = (bool)savedDataToLoad.LoadSavedObject();
            
            if(isShopSlotUnlocked)
            {
                if (!gameObject.activeInHierarchy) gameObject.SetActive(true);
            }
        }
    }
}
