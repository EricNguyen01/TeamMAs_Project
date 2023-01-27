using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace TeamMAsTD
{
    [DisallowMultipleComponent]
    public class UprootConfirmationPopupUI : MonoBehaviour
    {
        [SerializeField] private CanvasGroup uprootPopupCanvasGroup;

        [SerializeField] private TextMeshProUGUI uprootPopupMessageText;

        [SerializeField][TextArea] private string popupConfirmationMessage;

        //INTERNALS.....................................................................................

        private Tile tileWithUnitToUproot;

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
            //EventSystem.current.SetSelectedGameObject(gameObject);
        }

        //Select Uproot button - Button UI UnityEvent function set manually in the inspector
        public void OnPlayerConfirmedUproot()
        {
            if(tileWithUnitToUproot != null)
            {
                //process uproot coins refund
                if (GameResource.gameResourceInstance != null && GameResource.gameResourceInstance.coinResourceSO != null)
                {
                    if(tileWithUnitToUproot.plantUnitOnTile != null)
                    {
                        float refundAmount = tileWithUnitToUproot.plantUnitOnTile.plantUnitScriptableObject.uprootRefundAmount;

                        GameResource.gameResourceInstance.coinResourceSO.AddResourceAmount(refundAmount);
                    }
                }

                //performs uproot
                tileWithUnitToUproot.UprootUnit(0.0f);
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
    }
}
