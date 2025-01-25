// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

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

        [SerializeField] [TextArea] private string popupConfirmationMessage;

        [SerializeField] private StatPopupSpawner insufficientFundToUprootPopupPrefab;

        [SerializeField] private Button uprootButton;

        //INTERNALS.....................................................................................

        private Tile tileWithUnitToUproot;

        private Tile[] multiTilesToUproot;

        private int uprootCosts = 0;

        private int refunds = 0;

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

            if (uprootPopupMessageText == null)
            {
                Debug.LogWarning("Uproot popup confirmation UI: " + name + " has no TMPro text UI component assigned to display uproot message.");
            }
            else
            {
                SetUprootCostToUprootMessage(0, 0);
            }

            if (insufficientFundToUprootPopupPrefab != null)
            {
                GameObject go = null;

                if(uprootButton != null) go = Instantiate(insufficientFundToUprootPopupPrefab.gameObject, uprootButton.transform.position, Quaternion.identity);
                else go = Instantiate(insufficientFundToUprootPopupPrefab.gameObject, transform.position, Quaternion.identity);

                insufficientFundToUprootPopupPrefab = go.GetComponent<StatPopupSpawner>();

                insufficientFundToUprootPopupPrefab.SetStatPopupSpawnerConfig(0.0f,
                                                                          0.0f,
                                                                          0.0f,
                                                                          0.0f,
                                                                          1.8f);
            }
        }

        private void OnEnable()
        {
            //check for an existing EventSystem and disble script if null
            if (EventSystem.current == null)
            {
                Debug.LogError("Cannot find an EventSystem in the scene. " +
                "An EventSystem is required for uproot confirmation popup to function. Disabling popup confirmation!");

                enabled = false;

                return;
            }
        }

        private void Start()
        {
            Rain.OnRainStarted += (Rain r) =>
            {
                ShowUprootConfirmationPopup(false);

                if (UnitGroupSelectionManager.unitGroupSelectionManagerInstance)
                {
                    UnitGroupSelectionManager.unitGroupSelectionManagerInstance.EnableUnitGroupSelection(false);
                }

                if (TileMenuInteractionHandler.tileMenuInteractionHandlerInstance)
                {
                    if (TileMenuInteractionHandler.tileMenuInteractionHandlerInstance.isCheckingForTileMenuInteractions)
                        TileMenuInteractionHandler.tileMenuInteractionHandlerInstance.EnableCheckForTileMenuInteractions(false);
                }
            };
        }

        private void OnDestroy()
        {
            Rain.OnRainStarted -= (Rain r) =>
            {
                ShowUprootConfirmationPopup(false);

                if (UnitGroupSelectionManager.unitGroupSelectionManagerInstance)
                {
                    UnitGroupSelectionManager.unitGroupSelectionManagerInstance.EnableUnitGroupSelection(false);
                }

                if (TileMenuInteractionHandler.tileMenuInteractionHandlerInstance)
                {
                    if (TileMenuInteractionHandler.tileMenuInteractionHandlerInstance.isCheckingForTileMenuInteractions)
                        TileMenuInteractionHandler.tileMenuInteractionHandlerInstance.EnableCheckForTileMenuInteractions(false);
                }
            };
        }

        //Get the tile with a unit currently on that tile that the player has just clicked on and chosen the option of uprooting.
        private void SetTileWithUnitToUproot(Tile tileSelected)
        {
            tileWithUnitToUproot = tileSelected;
        }

        private void SetMultiTilesToUproot(Tile[] tiles)
        {
            if(tiles == null || tiles.Length == 0)
            {
                multiTilesToUproot = null;

                return;
            }

            multiTilesToUproot = tiles;
        }

        private void SetUprootCostToUprootMessage(int uprootCost, int unitNum)
        {
            if(string.IsNullOrEmpty(popupConfirmationMessage) || string.IsNullOrWhiteSpace(popupConfirmationMessage))
            {
                popupConfirmationMessage = "Do you want to spend ${uprootCost} to uproot {unitNum} unit(s)?";
            }

            if (!popupConfirmationMessage.Contains("uprootCost") ||
                !popupConfirmationMessage.Contains("unitNum"))
            {
                popupConfirmationMessage = "Do you want to spend ${uprootCost} to uproot {unitNum} unit(s)?";
            }

            popupConfirmationMessage = popupConfirmationMessage.Replace("{uprootCost}", $"{uprootCost}");

            popupConfirmationMessage = popupConfirmationMessage.Replace("{unitNum}", $"{unitNum}");

            uprootPopupMessageText.text = popupConfirmationMessage;
        }

        private void ShowUprootConfirmationPopup(bool show)
        {
            if (show)//if popup is enabled
            {
                if (uprootPopupCanvasGroup)
                {
                    uprootPopupCanvasGroup.interactable = true;//enable popup interaction

                    uprootPopupCanvasGroup.blocksRaycasts = true;//block interaction with everything else beneath popup

                    uprootPopupCanvasGroup.alpha = 1.0f;//display popup
                }
                
                if (UnitGroupSelectionManager.unitGroupSelectionManagerInstance)
                {
                    UnitGroupSelectionManager.unitGroupSelectionManagerInstance.EnableUnitGroupSelection(false);
                }

                if (TileMenuInteractionHandler.tileMenuInteractionHandlerInstance)
                {
                    if (TileMenuInteractionHandler.tileMenuInteractionHandlerInstance.isCheckingForTileMenuInteractions)
                        TileMenuInteractionHandler.tileMenuInteractionHandlerInstance.EnableCheckForTileMenuInteractions(false);
                }
            }
            else //if popup disabled->do the opposite of the above if
            {
                if (uprootPopupCanvasGroup)
                {
                    uprootPopupCanvasGroup.interactable = false;

                    uprootPopupCanvasGroup.blocksRaycasts = false;

                    uprootPopupCanvasGroup.alpha = 0.0f;
                }

                if (UnitGroupSelectionManager.unitGroupSelectionManagerInstance)
                {
                    UnitGroupSelectionManager.unitGroupSelectionManagerInstance.EnableUnitGroupSelection(true);
                }

                if (TileMenuInteractionHandler.tileMenuInteractionHandlerInstance)
                {
                    if (TileMenuInteractionHandler.tileMenuInteractionHandlerInstance.isCheckingForTileMenuInteractions)
                        TileMenuInteractionHandler.tileMenuInteractionHandlerInstance.EnableCheckForTileMenuInteractions(true);
                }
            }
        }

        //PUBLICS......................................................................................

        public void ActivateUprootConfirmationPopupForTile(Tile tileSelected, bool showPopupStatus)
        {
            SetTileWithUnitToUproot(tileSelected);

            if (!tileWithUnitToUproot) return;

            uprootCosts = 0;

            refunds = 0;

            if(tileWithUnitToUproot && tileWithUnitToUproot.plantUnitOnTile && tileWithUnitToUproot.plantUnitOnTile.plantUnitScriptableObject)
            {
                uprootCosts = tileWithUnitToUproot.plantUnitOnTile.plantUnitScriptableObject.uprootCost;

                refunds = tileWithUnitToUproot.plantUnitOnTile.plantUnitScriptableObject.uprootRefundAmount;
            }

            SetUprootCostToUprootMessage(uprootCosts, 1);
            
            ShowUprootConfirmationPopup(showPopupStatus);
        }

        public void ActivateUprootConfirmationPopupForMultipleTiles(Tile[] selectedTiles, bool showPopupStatus)
        {
            SetMultiTilesToUproot(selectedTiles);

            if (multiTilesToUproot == null || multiTilesToUproot.Length == 0) return;

            uprootCosts = 0;

            refunds = 0;

            int unitNum = 0;

            for(int i = 0; i < selectedTiles.Length; i++)
            {
                if (!selectedTiles[i]) continue;

                if(!selectedTiles[i].plantUnitOnTile) continue;

                if (!selectedTiles[i].plantUnitOnTile.plantUnitScriptableObject) continue;

                uprootCosts += selectedTiles[i].plantUnitOnTile.plantUnitScriptableObject.uprootCost;

                refunds += selectedTiles[i].plantUnitOnTile.plantUnitScriptableObject.uprootRefundAmount;

                unitNum++;
            }

            SetUprootCostToUprootMessage(uprootCosts, unitNum);

            ShowUprootConfirmationPopup(showPopupStatus);
        }

        //Select Uproot button - Button UI UnityEvent function set manually in the inspector
        public void OnPlayerConfirmedUproot()
        {
            float costAmount = uprootCosts;

            float refundAmount = refunds;

            if (GameResource.gameResourceInstance != null && GameResource.gameResourceInstance.coinResourceSO != null)
            {
                if (GameResource.gameResourceInstance.coinResourceSO.resourceAmount < costAmount)
                {
                    if (multiTilesToUproot != null && multiTilesToUproot.Length > 0)
                    {
                        for(int i = 0; i < multiTilesToUproot.Length; i++)
                        {
                            if (!multiTilesToUproot[i]) continue;

                            multiTilesToUproot[i].UprootingInsufficientFundsEventInvoke();

                            break;
                        }
                    }
                    else
                    {
                        if(tileWithUnitToUproot) tileWithUnitToUproot.UprootingInsufficientFundsEventInvoke();
                    }

                    ProcessInsufficientFundToUprootPopup();

                    return;
                }

                GameResource.gameResourceInstance.coinResourceSO.RemoveResourceAmount(costAmount);

                GameResource.gameResourceInstance.coinResourceSO.AddResourceAmount(refundAmount);
            }

            if (multiTilesToUproot != null && multiTilesToUproot.Length > 0)
            {
                //handle multi uproots here:

                //first, force destroy immediate all the ability effects popups and popup spawners on all plants that will be uprooted
                for (int i = 0; i < multiTilesToUproot.Length; i++)
                {
                    if (!multiTilesToUproot[i]) continue;

                    if (!multiTilesToUproot[i].plantUnitOnTile) continue;

                    AbilityEffectReceivedInventory plantAbilityEffectsReceivedInventory = multiTilesToUproot[i].plantUnitOnTile.GetAbilityEffectReceivedInventory();

                    if (!plantAbilityEffectsReceivedInventory) continue;

                    plantAbilityEffectsReceivedInventory.ForceDestroyImmediate_AllReceivedEffectsStatPopups_AndPopupSpawners();
                }

                //then, proceed to uproot the plants
                for (int i = 0; i < multiTilesToUproot.Length; i++)
                {
                    if (!multiTilesToUproot[i]) continue;

                    multiTilesToUproot[i].UprootUnit();
                }

                multiTilesToUproot = null;

                goto ClosePopup;
            }

            if (tileWithUnitToUproot)
            {
                //performs single uproot
                tileWithUnitToUproot.UprootUnit(0.0f);

                tileWithUnitToUproot = null;
            }

        ClosePopup:

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

        private void ProcessInsufficientFundToUprootPopup()
        {
            if (uprootButton == null) return;

            if(insufficientFundToUprootPopupPrefab == null) return;

            float popupOffsetX = insufficientFundToUprootPopupPrefab.GetPopupPositionAfterStartOffsetApplied().x;

            float popupOffsetY = insufficientFundToUprootPopupPrefab.GetPopupPositionAfterStartOffsetApplied().y;

            float offsetToButtonX = uprootButton.transform.position.x - popupOffsetX;

            float offsetToButtonY = uprootButton.transform.position.y - popupOffsetY;

            insufficientFundToUprootPopupPrefab.SetStatPopupSpawnerConfig(offsetToButtonY,
                                                                          offsetToButtonX,
                                                                          0.0f,
                                                                          0.0f,
                                                                          1.8f);

            insufficientFundToUprootPopupPrefab.PopUp(null, null, StatPopup.PopUpType.Negative);
        }
    }
}
