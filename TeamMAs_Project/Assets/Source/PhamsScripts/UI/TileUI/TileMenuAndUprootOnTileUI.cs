// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace TeamMAsTD
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Tile))]
    public class TileMenuAndUprootOnTileUI : MonoBehaviour, IPointerDownHandler, IDeselectHandler
    {
        [SerializeField] private Canvas tileWorldCanvas;

        [SerializeField] private Camera worldUICam;

        //INTERNALS...........................................................................................

        private CanvasGroup tileWorldCanvasGroup;

        private Tile tileSelectedForUprootConfirmation;

        private PointerEventData pEventData;//Unity's EventSystem pointer event data

        private bool disableTileMenuOpen = false;

        //UnityEvent............................................................................................

        [SerializeField] public UnityEvent OnTileMenuOpened;

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

            tileWorldCanvasGroup = tileWorldCanvas.GetComponent<CanvasGroup>();

            if (tileWorldCanvasGroup == null) tileWorldCanvasGroup = tileWorldCanvas.gameObject.AddComponent<CanvasGroup>();

            tileSelectedForUprootConfirmation = GetComponent<Tile>();

            if (tileSelectedForUprootConfirmation == null)
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
        }

        private void OnEnable()
        {
            //check for an existing EventSystem and disble script if null
            if (FindObjectOfType<EventSystem>() == null)
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

        //PRIVATES..............................................................................

        private void OpenTileInteractionMenu(bool opened)
        {
            //if there is no tile script component reference->show error and stop executing
            if (tileSelectedForUprootConfirmation == null)
            {
                Debug.LogError("Trying to interact with tile: " + name + " but the Tile script component can't be found!");
                return;
            }

            //do nothing if tile doesnt have plant unit placed on
            if (tileSelectedForUprootConfirmation.plantUnitOnTile == null)
            {
                return;
            }

            PlantUnit plantSelected = tileSelectedForUprootConfirmation.plantUnitOnTile;

            //if a plant exists on this tile->process open/close tile menu
            if (opened)
            {
                if (!disableTileMenuOpen && !tileWorldCanvas.gameObject.activeInHierarchy)
                {
                    tileWorldCanvas.gameObject.SetActive(true);

                    OpenPlantRangeCircle(plantSelected, true);

                    OnTileMenuOpened?.Invoke();
                }
                else 
                {
                    OpenPlantRangeCircle(plantSelected, false);

                    tileWorldCanvas.gameObject.SetActive(false); 
                }

                return;
            }

            if (tileWorldCanvas.gameObject.activeInHierarchy) 
            { 
                tileWorldCanvas.gameObject.SetActive(false);

                OpenPlantRangeCircle(plantSelected, false);
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

            SetDisableTileMenuOpen(true);
        }

        private void StopDisableTileMenuInteractionOnRainEnded(Rain rain)
        {
            TemporaryDisableTileMenuContentInteraction(false);

            SetDisableTileMenuOpen(false);
        }

        //PUBLICS..............................................................................................

        //UnityEvent function for uproot UI button
        public void OnUprootOptionClicked()
        {
            //spawn uproot prompt
            UprootConfirmationPopupUI uprootConfirmationPopupUI = FindObjectOfType<UprootConfirmationPopupUI>();

            if(uprootConfirmationPopupUI == null)
            {
                Debug.LogWarning("Uproot option is selected but no UprootConfirmationPopupUI object is found in scene! Uproot confirmation failed!");
                return;
            }

            OpenTileInteractionMenu(false);

            uprootConfirmationPopupUI.ActivateUprootConfirmationPopupForTile(tileSelectedForUprootConfirmation, true);
        }

        public void SetDisableTileMenuOpen(bool disabled)
        {
            if(disabled) OpenTileInteractionMenu(false);

            disableTileMenuOpen = disabled;
        }

        public void TemporaryDisableTileMenuContentInteraction(bool disabled)
        {
            if (tileWorldCanvasGroup == null) return;

            if (disabled)
            {
                tileWorldCanvasGroup.interactable = false;

                return;
            }

            tileWorldCanvasGroup.interactable = true;
        }

        //Unity EventSystem OnPointerDownHandler interface function.............................................

        public void OnPointerDown(PointerEventData eventData)
        {
            //cache pointer event data to use later in OnDeselect()
            pEventData = eventData;

            OpenTileInteractionMenu(true);

            //set the selected game object in the current event system so that
            //when the event system detects a newly selected game obj whether null or not,
            //it will trigger OnDeselect() on this script which can be used to close the tile menu.
            EventSystem.current.SetSelectedGameObject(gameObject);
        }

        //This func is triggere by the EventSystem when user clicks on nothing or another obj, causing this class to be deselected
        public void OnDeselect(BaseEventData eventData)
        {
            //Only close if pointer is not over anything, not over an object, or over an object that is not the same as this obj.
            //This is done so that when clicking again on the same tile after opening the tile menu of that tile,
            //we don't want to close the menu in this OnDeselect function (which gets called even on selecting the same obj again)
            //just so we can open it again in OnPointerDown (OnPointerDown happens after OnDeselect).
            //We want to close the menu instead of keeping it open after re-clicking on the same tile.

            if (pEventData.pointerEnter == null || pEventData.pointerEnter.gameObject == null || pEventData.pointerEnter.gameObject != gameObject)
            {
                OpenTileInteractionMenu(false);
            }

            //after OnDeselect is called, EventSystem's selected object is set to null again so we don't have to reset it manually.
        }
    }
}
