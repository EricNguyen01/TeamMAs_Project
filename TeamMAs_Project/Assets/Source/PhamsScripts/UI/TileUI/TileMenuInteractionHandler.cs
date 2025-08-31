// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using PixelCrushers.DialogueSystem;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TeamMAsTD
{
    [DisallowMultipleComponent]
    public class TileMenuInteractionHandler : MonoBehaviour
    {
        [SerializeField] private Camera tileMenuInteractionRaycastCam;

        public TileMenuAndUprootOnTileUI tileMenuInUse { get; private set; }

        public Dictionary<GameObject, TileMenuAndUprootOnTileUI> tileObjectAndTileMenuDict { get; private set; } = new Dictionary<GameObject, TileMenuAndUprootOnTileUI>();

        private int DEBUG_tileMenuCount = 0;

        private List<RaycastResult> raycastResults = new List<RaycastResult>();

        public enum TileMenuInteractionOptions { Open = 0, Close = 1, Toggle = 2 }

        public bool isCheckingForTileMenuInteractions { get; private set; } = true;

        public static TileMenuInteractionHandler tileMenuInteractionHandlerInstance;

        private void Awake()
        {
            if (tileMenuInteractionHandlerInstance && tileMenuInteractionHandlerInstance != this)
            {
                enabled = false;

                Destroy(gameObject);

                return;
            }

            tileMenuInteractionHandlerInstance = this;

            DontDestroyOnLoad(gameObject);

            SceneManager.sceneLoaded += (Scene sc, LoadSceneMode loadMode) =>
            {
                if (sc.name.Contains("Menu")) enabled = false;
                else enabled = true;
            };

            Rain.OnRainStarted += (Rain r) => EnableCheckForTileMenuInteractions(false);

            Rain.OnRainEnded += (Rain r) => EnableCheckForTileMenuInteractions(true);
        }

        private void OnEnable()
        {
            if (!EventSystem.current)
            {
                Debug.LogWarning("Tile Menu Interaction Handler: " + name + "Could not find an EventSystem in the scene." +
                "Tile Menu Interaction Handler won't work! Disabling script...");

                enabled = false;

                return;
            }

            CheckAndGetTileRaycastCam();

            isCheckingForTileMenuInteractions = true;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= (Scene sc, LoadSceneMode loadMode) =>
            {
                if (sc.name.Contains("Menu")) enabled = false;
                else enabled = true;
            };

            Rain.OnRainStarted -= (Rain r) => EnableCheckForTileMenuInteractions(false);

            Rain.OnRainEnded -= (Rain r) => EnableCheckForTileMenuInteractions(true);
        }

        private void Update()
        {
            if (!enabled) return;

            CheckAndGetTileRaycastCam();

            if (!EventSystem.current)
            {
                if(enabled) enabled = false;

                return;
            }

            if (tileObjectAndTileMenuDict == null || tileObjectAndTileMenuDict.Count == 0)
            {
                return;
            }

            if (DialogueManager.Instance)
            {
                if (DialogueManager.Instance.isConversationActive) return;
            }

            if (!isCheckingForTileMenuInteractions) return;

            CheckAndProcessTileMenuInteractionOnMouseClick();
        }

        private void CheckAndProcessTileMenuInteractionOnMouseClick()
        {
            if (Input.GetButtonDown("Fire1"))
            {
                raycastResults.Clear();

                PointerEventData eventData = new PointerEventData(EventSystem.current);

                eventData.position = Input.mousePosition;

                EventSystem.current.RaycastAll(eventData, raycastResults);

                //if clicking on nothing (click away)

                if (raycastResults.Count == 0)
                {
                    //if a tile menu was previously clicked and opened -> close it

                    if (tileMenuInUse)
                    {
                        if (tileMenuInUse.isOpened) tileMenuInUse.OpenTileInteractionMenu(false);

                        tileMenuInUse = null;
                    }

                    return;
                }

                TileMenuAndUprootOnTileUI tileMenu = null;

                for (int i = 0; i < raycastResults.Count; i++)
                {
                    if (!raycastResults[i].isValid) continue;

                    if (raycastResults[i].gameObject.layer == LayerMask.GetMask("UI") ||
                        raycastResults[i].gameObject.layer == LayerMask.NameToLayer("UI"))
                    {
                        /*if (raycastResults[i].gameObject.GetComponent<Button>())*/ return;
                    }

                    if (tileMenuInUse)
                    {
                        //if click on one of the buttons of the currently opened tile menu
                        //(that was previously clicked on as well and hasnt been closed)/
                        //-> use the button instead of processing tile menu interactions (button logic is handled in TileMenu script)
                        //return and does not execute further if reaches here
                        if (tileMenuInUse.tileMenuButtons != null && tileMenuInUse.tileMenuButtons.Length > 0)
                        {
                            for (int j = 0; j < tileMenuInUse.tileMenuButtons.Length; j++)
                            {
                                if (raycastResults[i].gameObject == tileMenuInUse.tileMenuButtons[j].gameObject)
                                    return;
                            }
                        }
                    }

                    //else 

                    //if a tile menu clicked on is detected -> registers it and exit loop.
                    if (tileObjectAndTileMenuDict.ContainsKey(raycastResults[i].gameObject))
                    {
                        tileMenu = tileObjectAndTileMenuDict[raycastResults[i].gameObject];

                        break;
                    }
                }

                //if clicking away or clicked on nothing

                if (!tileMenu)
                {
                    //close previously opened tile menu (if exists) on clicking away
                    if (tileMenuInUse)
                    {
                        if (tileMenuInUse.isOpened) tileMenuInUse.OpenTileInteractionMenu(false);

                        tileMenuInUse = null;
                    }

                    return;
                }

                //if first time clicking on a new tile menu -> open the clicked tile menu

                if (!tileMenuInUse)
                {
                    tileMenuInUse = tileMenu;

                    if (!tileMenuInUse.isOpened) tileMenuInUse.OpenTileInteractionMenu(true);

                    return;
                }

                //if clicked on the same tile and tile menu again after having been clicking on it previously -> toggle it

                if (tileMenuInUse == tileMenu)
                {
                    if (tileMenuInUse.isOpened) tileMenuInUse.OpenTileInteractionMenu(false);

                    else tileMenuInUse.OpenTileInteractionMenu(true);

                    return;
                }

                //if clicked on a new tile/tile menu entirely from a previosuly selected tile menu ->
                //close the previously opened tile menu and then open the newly selected tile menu

                if (tileMenuInUse.isOpened) tileMenuInUse.OpenTileInteractionMenu(false);

                tileMenuInUse = tileMenu;

                if (!tileMenuInUse.isOpened) tileMenuInUse.OpenTileInteractionMenu(true);
            }
        }

        public void RegisterTileAndTileMenuOnTileEnabled(Tile tile)
        {
            if (!tile) return;

            if (tileObjectAndTileMenuDict.ContainsKey(tile.gameObject)) return;

            TileMenuAndUprootOnTileUI tileMenu = tile.tileMenuAndUprootOnTileUI;

            if (!tileMenu)
            {
                tile.TryGetComponent<TileMenuAndUprootOnTileUI>(out tileMenu);
            }

            if (tileMenu)
            {
                if(tileObjectAndTileMenuDict.TryAdd(tile.gameObject, tileMenu))
                {
                    DEBUG_tileMenuCount++;
                }
            }
        }

        public void RemoveTileAndTileMenuOnTileDisabled(Tile tile)
        {
            if (!tile) return;

            if (!tileObjectAndTileMenuDict.ContainsKey(tile.gameObject)) return;

            if (tileObjectAndTileMenuDict.Remove(tile.gameObject))
            {
                DEBUG_tileMenuCount--;
            }
        }

        /// <summary>
        /// Whether to enable or disable checking for mouse-click tile menu interactions in Update(). 
        /// Disable in case you want to manually interact with tile menus externally. REMEMBER TO RE-ENABLE AFTER!!!
        /// </summary>
        /// <param name="enabled"></param>
        public void EnableCheckForTileMenuInteractions(bool enabled)
        {
            isCheckingForTileMenuInteractions = enabled;

            if (tileMenuInUse && tileMenuInUse.isOpened)
            {
                tileMenuInUse.OpenTileInteractionMenu(false);

                tileMenuInUse = null;
            }
        }

        public void ForceSetTileMenuInteracted_External(TileMenuAndUprootOnTileUI tileMenuInUse, TileMenuInteractionOptions interactionOption)
        {
            if (this.tileMenuInUse && this.tileMenuInUse != tileMenuInUse)
            {
                this.tileMenuInUse.OpenTileInteractionMenu(false);
            }

            this.tileMenuInUse = tileMenuInUse;

            if (!tileMenuInUse) return;

            if(interactionOption == TileMenuInteractionOptions.Open)
            {
                if(!tileMenuInUse.isOpened) tileMenuInUse.OpenTileInteractionMenu(true);
            }
            else if(interactionOption == TileMenuInteractionOptions.Close)
            {
                if (tileMenuInUse.isOpened) tileMenuInUse.OpenTileInteractionMenu(false);
            }
            else if(interactionOption == TileMenuInteractionOptions.Toggle)
            {
                if(tileMenuInUse.isOpened) tileMenuInUse.OpenTileInteractionMenu(false);
                else tileMenuInUse.OpenTileInteractionMenu(true);
            }
        }

        public static void CreateTileMenuInteractionHandlerSingleton()
        {
            if(tileMenuInteractionHandlerInstance) return;

            GameObject tileMenuInteractionHandlerGO = new GameObject("TileMenuInteractionHandler(1InstanceOnly)", typeof(TileMenuInteractionHandler));

            TileMenuInteractionHandler tileMenuInteractionHandler;

            tileMenuInteractionHandlerGO.TryGetComponent<TileMenuInteractionHandler>(out  tileMenuInteractionHandler);

            tileMenuInteractionHandlerInstance = tileMenuInteractionHandler;

            //DontDestroyOnLoad(tileMenuInteractionHandlerGO);
        }

        private void CheckAndGetTileRaycastCam()
        {
            if (!tileMenuInteractionRaycastCam)
            {
                if (!Camera.main)
                {
                    if(enabled) enabled = false;

                    return;
                }

                tileMenuInteractionRaycastCam = Camera.main;
            }
        }
    }
}
