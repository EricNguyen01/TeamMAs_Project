using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace TeamMAsTD
{
    [DisallowMultipleComponent]
    public class UprootOnTileUI : MonoBehaviour, IPointerDownHandler, IDeselectHandler
    {
        [SerializeField] private Canvas tileWorldCanvas;

        //INTERNALS...........................................................................................

        private Tile tileSelectedForUprootConfirmation;
        private Camera mainCam;

        private PointerEventData pEventData;//Unity's EventSystem pointer event data

        //PRIVATES.............................................................................................
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

            if (tileWorldCanvas == null)
            {
                tileWorldCanvas = GetComponentInChildren<Canvas>();

                if (tileWorldCanvas == null)
                {
                    Debug.LogError("Tile World Canvas children component not found on tile: " + name + ". Plant uprooting won't work!");
                    enabled = false;
                    return;
                }
            }

            tileSelectedForUprootConfirmation = GetComponent<Tile>();
            if(tileSelectedForUprootConfirmation == null)
            {
                Debug.LogError("Tile script component not found. Plant uprooting won't work!");
                enabled = false;
                return;
            }

            if(mainCam == null) mainCam = Camera.main;

            if(tileWorldCanvas.worldCamera == null) tileWorldCanvas.worldCamera = mainCam;
        }

        //PRIVATES..............................................................................

        private void OpenTileInteractionMenu(bool opened)
        {
            //if there is no tile script component reference->show error and stop executing
            if(tileSelectedForUprootConfirmation == null)
            {
                Debug.LogError("Trying to interact with tile: " + name + " but the Tile script component can't be found!");
                return;
            }

            //do nothing if tile doesnt have plant unit placed on
            if(tileSelectedForUprootConfirmation.unitOnTile == null)
            {
                return;
            }

            //if a plant exists on this tile->process open/close tile menu
            if (opened)
            {
                if(!tileWorldCanvas.gameObject.activeInHierarchy) tileWorldCanvas.gameObject.SetActive(true);
                else tileWorldCanvas.gameObject.SetActive(false);
                return;
            }

            if(tileWorldCanvas.gameObject.activeInHierarchy) tileWorldCanvas.gameObject.SetActive(false);
        }

        //PUBLICS..............................................................................................
        public void OnUprootOptionClicked()
        {
            //spawn uproot prompt
            UprootConfirmationPopupUI uprootConfirmationPopupUI = FindObjectOfType<UprootConfirmationPopupUI>();

            if(uprootConfirmationPopupUI == null)
            {
                Debug.LogWarning("Uproot option is selected but no UprootConfirmationPopupUI object is found in scene! Uproot confirmation failed!");
                return;
            }

            uprootConfirmationPopupUI.ActivateUprootConfirmationPopupForTile(tileSelectedForUprootConfirmation, true);
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
