using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace TeamMAsTD
{
    [DisallowMultipleComponent]
    public class UprootConfirmationPopupUI : MonoBehaviour, IPointerEnterHandler, IDeselectHandler
    {
        [SerializeField] private CanvasGroup uprootPopupCanvasGroup;

        [SerializeField] private TextMeshProUGUI uprootPopupMessageText;

        [SerializeField][TextArea] private string popupConfirmationMessage;

        //INTERNALS.....................................................................................

        private Tile tileWithUnitToUproot;

        private PointerEventData pEventData;

        //PRIVATES......................................................................................

        private void Awake()
        {
            if (uprootPopupCanvasGroup == null)
            {
                uprootPopupCanvasGroup = GetComponent<CanvasGroup>();
                if (uprootPopupCanvasGroup == null) uprootPopupCanvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            //disable display and interaction of popup on awake
            ShowUprootConfirmationPopup(false);

            if (!string.IsNullOrEmpty(popupConfirmationMessage))
            {
                if(uprootPopupMessageText == null)
                {
                    Debug.LogWarning("Uproot popup confirmation message is set in: " + name + " but no TMPro text UI component is assigned to display this message.");
                }
                else
                {
                    uprootPopupMessageText.text = popupConfirmationMessage;
                }
            }
        }

        private void OnEnable()
        {
            //check for an existing EventSystem and disble script if null
            if (FindObjectOfType<EventSystem>() == null)
            {
                Debug.LogError("Cannot find an EventSystem in the scene. " +
                "An EventSystem is required for uproot confirmation popup to function. Disabling popup confirmation!");
                enabled = false;
                return;
            }
        }

        //Get the tile with a unit currently on that tile that the player has just clicked on and chosen the option of uprooting.
        private void SetTileWithUnitToUproot(Tile tileSelected)
        {
            tileWithUnitToUproot = tileSelected;
        }

        private void ShowUprootConfirmationPopup(bool show)
        {
            if (show)//if popup is enabled
            {
                uprootPopupCanvasGroup.interactable = true;//enable popup interaction
                uprootPopupCanvasGroup.blocksRaycasts = true;//block interaction with everything else beneath popup
                uprootPopupCanvasGroup.alpha = 1.0f;//display popup
            }
            else //if popup disabled->do the opposite of the above if
            {
                uprootPopupCanvasGroup.interactable = false;
                uprootPopupCanvasGroup.blocksRaycasts = false;
                uprootPopupCanvasGroup.alpha = 0.0f;
            }
        }

        //PUBLICS......................................................................................

        public void ActivateUprootConfirmationPopupForTile(Tile tileSelected, bool showPopupStatus)
        {
            SetTileWithUnitToUproot(tileSelected);
            ShowUprootConfirmationPopup(showPopupStatus);

            //set current EventSystem selected object to this obj to use with OnDeselect() later
            EventSystem.current.SetSelectedGameObject(gameObject);
        }

        //Select Uproot button - Button UI UnityEvent function set manually in the inspector
        public void OnPlayerConfirmedUproot()
        {
            if(tileWithUnitToUproot != null)
            {
                tileWithUnitToUproot.UprootUnit();
            }
            else
            {
                Debug.LogWarning("Unit uproot is confirmed but there is no selected tile with unit to uproot reference found for uprooting " +
                "(tileWithUnitToUproot var is null!).");
            }

            //stop showing popup after button pressed
            ShowUprootConfirmationPopup(false);
        }

        //Select Keep button - Button UI UnityEvent function set manually in the inspector
        public void OnPlayerDeclinedUproot()
        {
            //if the player chooses to keep the unit instead, do nothing and
            //stop showing popup after button pressed
            ShowUprootConfirmationPopup(false);
        }

        //EventSystem interface functions..................................................................

        public void OnPointerEnter(PointerEventData eventData)
        {
            pEventData = eventData;
        }

        public void OnDeselect(BaseEventData eventData)
        {
            //if pEventData is null or click on nothing -> close popup
            if(pEventData == null || pEventData.pointerEnter == null || pEventData.pointerEnter.gameObject == null)
            {
                ShowUprootConfirmationPopup(false);
                return;
            }

            bool canClose = true;

            //if click on a child obj that is a UI Button -> don't close popup
            //here, a recursive function is executed that goes through all children of children starting from this obj to check for the above
            if (IsChildButtonObjectSelected(gameObject))
            {
                //if a child of this obj is selected that is also a button, can't close popup
                canClose = false;
            }

            //else close popup
            if (canClose) ShowUprootConfirmationPopup(false);
        }

        //A recursive function that goes through all children of children starting from a provided obj
        //to check for if any of its children is selected and is also a UI button.
        private bool IsChildButtonObjectSelected(GameObject child)
        {
            if(child == null) return false;

            //check the current input obj first 
            if (pEventData.pointerEnter.gameObject == child)//if this is the obj the pointer is on
            {
                if (child.GetComponent<Button>()) return true;//if also a button->return true and no need to execute any further
            }

            //if above if not return->continue to check if this is the last child (no other children below this obj)
            if (child.transform.childCount == 0) return false;//if true->and since the above if alr failed-> return false

            //if there are still children below this obj->continue recursively checking
            for (int i = 0; i < child.transform.childCount; i++)
            {
                //if during the check of one of the children returned a true->stop looping and return true
                if (IsChildButtonObjectSelected(child.transform.GetChild(i).gameObject)) return true;
                else continue;
            }

            return false;//if all the above did not return true->obviously return a false
        }
    }
}
