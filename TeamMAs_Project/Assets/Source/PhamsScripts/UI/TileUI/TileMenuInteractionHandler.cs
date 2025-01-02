// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using PixelCrushers.DialogueSystem;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TeamMAsTD
{
    [DisallowMultipleComponent]
    public class TileMenuInteractionHandler : MonoBehaviour
    {
        [SerializeField] private Camera tileMenuInteractionRaycastCam;

        private TileMenuAndUprootOnTileUI tileMenuClicked;

        private List<TDGrid> gridsInScene = new List<TDGrid>();

        private int DEBUG_tileMenuCount = 0;

        public Dictionary<GameObject, TileMenuAndUprootOnTileUI> tileObjectAndTileMenuDict { get; private set; } 
        = new Dictionary<GameObject, TileMenuAndUprootOnTileUI>();

        private List<RaycastResult> raycastResults = new List<RaycastResult>();

        public enum TileMenuInteractionOptions { Open = 0, Close = 1, Toggle = 2 }

        private bool isCheckingForTileMenuInteractions = true;

        public static TileMenuInteractionHandler tileMenuInteractionHandlerInstance;

        private void Awake()
        {
            if(tileMenuInteractionHandlerInstance && tileMenuInteractionHandlerInstance != this)
            {
                enabled = false;

                Destroy(gameObject);

                return;
            }

            tileMenuInteractionHandlerInstance = this;

            DontDestroyOnLoad(gameObject);
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

            if (!tileMenuInteractionRaycastCam)
            {
                if (!Camera.main)
                {
                    Debug.LogWarning("Tile Menu Interaction Handler: " + name + "Doesnt' Have A Camera To Perform Tile Raycast Selection." +
                    "Tile Menu Interaction Handler won't work! Disabling script...");

                    enabled = false;

                    return;
                }

                tileMenuInteractionRaycastCam = Camera.main;
            }

            isCheckingForTileMenuInteractions = true;
        }

        private void Update()
        {
            if (!enabled) return;

            if (!EventSystem.current || !tileMenuInteractionRaycastCam)
            {
                if(enabled) enabled = false;

                return;
            }

            if (tileObjectAndTileMenuDict == null || tileObjectAndTileMenuDict.Count == 0)
            {
                if (enabled) enabled = false;

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

                    if (tileMenuClicked)
                    {
                        if (tileMenuClicked.isOpened) tileMenuClicked.OpenTileInteractionMenu(false);

                        tileMenuClicked = null;
                    }

                    return;
                }

                TileMenuAndUprootOnTileUI tileMenu = null;

                for (int i = 0; i < raycastResults.Count; i++)
                {
                    if (!raycastResults[i].isValid) continue;

                    if (tileMenuClicked)
                    {
                        //if click on one of the buttons of the currently opened tile menu
                        //(that was previously clicked on as well and hasnt been closed)/
                        //-> use the button instead of processing tile menu interactions (button logic is handled in TileMenu script)
                        //return and does not execute further if reaches here
                        if (tileMenuClicked.tileMenuButtons != null && tileMenuClicked.tileMenuButtons.Length > 0)
                        {
                            for (int j = 0; j < tileMenuClicked.tileMenuButtons.Length; j++)
                            {
                                if (raycastResults[i].gameObject == tileMenuClicked.tileMenuButtons[j].gameObject)
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
                    if (tileMenuClicked)
                    {
                        if (tileMenuClicked.isOpened) tileMenuClicked.OpenTileInteractionMenu(false);

                        tileMenuClicked = null;
                    }

                    return;
                }

                //if first time clicking on a new tile menu -> open the clicked tile menu

                if (!tileMenuClicked)
                {
                    tileMenuClicked = tileMenu;

                    if (!tileMenuClicked.isOpened) tileMenuClicked.OpenTileInteractionMenu(true);

                    return;
                }

                //if clicked on the same tile and tile menu again after having been clicking on it previously -> toggle it

                if (tileMenuClicked == tileMenu)
                {
                    if (tileMenuClicked.isOpened) tileMenuClicked.OpenTileInteractionMenu(false);

                    else tileMenuClicked.OpenTileInteractionMenu(true);

                    return;
                }

                //if clicked on a new tile/tile menu entirely from a previosuly selected tile menu ->
                //close the previously opened tile menu and then open the newly selected tile menu

                if (tileMenuClicked.isOpened) tileMenuClicked.OpenTileInteractionMenu(false);

                tileMenuClicked = tileMenu;

                if (!tileMenuClicked.isOpened) tileMenuClicked.OpenTileInteractionMenu(true);
            }
        }

        public void AddGridAndItsTiles(TDGrid grid)
        {
            if (!grid) return;

            if (gridsInScene.Contains(grid)) return;

            gridsInScene.Add(grid);

            Tile[] tilesInGrid = grid.GetGridFlattened2DArray();

            if (tilesInGrid == null || tilesInGrid.Length == 0) return;

            for(int i = 0; i < tilesInGrid.Length; i++)
            {
                if (!tilesInGrid[i]) continue;

                TileMenuAndUprootOnTileUI tileMenu = tilesInGrid[i].tileMenuAndUprootOnTileUI;

                if (!tileMenu) 
                {
                    if (!tilesInGrid[i].TryGetComponent<TileMenuAndUprootOnTileUI>(out tileMenu)) continue;
                }

                if(tileObjectAndTileMenuDict.TryAdd(tilesInGrid[i].gameObject, tileMenu))
                {
                    DEBUG_tileMenuCount++;
                }
            }
        }

        public void RemoveGridAndItsTiles(TDGrid grid)
        {
            if (!grid) return;

            if (!gridsInScene.Contains(grid)) return;

            Tile[] tilesInGrid = grid.GetGridFlattened2DArray();

            if(tilesInGrid != null && tilesInGrid.Length > 0)
            {
                for (int i = 0; i < tilesInGrid.Length; i++)
                {
                    if (!tilesInGrid[i]) continue;

                    if (tileObjectAndTileMenuDict.Remove(tilesInGrid[i].gameObject))
                    {
                        DEBUG_tileMenuCount--;
                    }
                }
            }

            gridsInScene.Remove(grid);
        }

        public void EnableCheckForTileMenuInteractions(bool enabled)
        {
            isCheckingForTileMenuInteractions = enabled;

            tileMenuClicked = null;
        }

        public void SetTileMenuInteractedManually(TileMenuAndUprootOnTileUI tileMenuClicked, TileMenuInteractionOptions interactionOption)
        {
            this.tileMenuClicked = tileMenuClicked;

            if (!tileMenuClicked) return;

            if(interactionOption == TileMenuInteractionOptions.Open)
            {
                if(!tileMenuClicked.isOpened) tileMenuClicked.OpenTileInteractionMenu(true);
            }
            else if(interactionOption == TileMenuInteractionOptions.Close)
            {
                if (tileMenuClicked.isOpened) tileMenuClicked.OpenTileInteractionMenu(false);
            }
            else if(interactionOption == TileMenuInteractionOptions.Toggle)
            {
                if(tileMenuClicked.isOpened) tileMenuClicked.OpenTileInteractionMenu(false);
                else tileMenuClicked.OpenTileInteractionMenu(true);
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
    }
}
