// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using PixelCrushers.DialogueSystem;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.Universal;

namespace TeamMAsTD
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PhysicsRaycaster))]
    [ExecuteInEditMode]
    public class TileMenuInteractionHandler : MonoBehaviour
    {
        private TileMenuAndUprootOnTileUI tileMenuClicked;

        private TDGrid gridUsingComponent;

        private Dictionary<GameObject, TileMenuAndUprootOnTileUI> tileObjectAndTileMenuDict = new Dictionary<GameObject, TileMenuAndUprootOnTileUI>();

        private PointerEventData pointerEventData;

        private PhysicsRaycaster physRaycaster;

        private List<RaycastResult> raycastResults = new List<RaycastResult>();

        private void Awake()
        {
            if (!TryGetComponent<PhysicsRaycaster>(out physRaycaster))
            {
                physRaycaster = gameObject.AddComponent<PhysicsRaycaster>();
            }

            physRaycaster.eventMask = LayerMask.GetMask("Tile");
        }

        private void OnEnable()
        {
            if (!EventSystem.current)
            {
                Debug.LogWarning("TileClickToOpenTileMenu: " + name + "Could Not Find An EventSystem in The Scene." +
                "Tile Menu Interaction won't work! Disabling script...");

                enabled = false;

                return;
            }

            pointerEventData = new PointerEventData(EventSystem.current);

            physRaycaster.eventCamera.clearFlags = CameraClearFlags.Depth;

            if(Camera.main) physRaycaster.eventCamera.orthographic = Camera.main.orthographic;

            UniversalAdditionalCameraData raycastCamData = physRaycaster.eventCamera.GetUniversalAdditionalCameraData();

            if (raycastCamData) raycastCamData.renderType = CameraRenderType.Overlay;
        }

        private void Start()
        {
            if (!Application.isPlaying) return;

            if (gridUsingComponent && tileObjectAndTileMenuDict != null && tileObjectAndTileMenuDict.Count > 0)
            {
                foreach (GameObject tileGO in tileObjectAndTileMenuDict.Keys)
                {
                    if (!tileGO || !tileObjectAndTileMenuDict[tileGO]) continue;

                    tileObjectAndTileMenuDict[tileGO].OnTileMenuClosed.AddListener(() => tileMenuClicked = null);
                }
            }
        }

        private void OnDisable()
        {
            if (!Application.isPlaying) return;

            if (gridUsingComponent && tileObjectAndTileMenuDict != null && tileObjectAndTileMenuDict.Count > 0)
            {
                foreach (GameObject tileGO in tileObjectAndTileMenuDict.Keys)
                {
                    if (!tileGO || !tileObjectAndTileMenuDict[tileGO]) continue;

                    tileObjectAndTileMenuDict[tileGO].OnTileMenuClosed.RemoveListener(() => tileMenuClicked = null);
                }
            }
        }

        private void Update()
        {
            if (!Application.isPlaying) return;

            if (!enabled) return;

            if (DialogueManager.Instance)
            {
                if (DialogueManager.Instance.isConversationActive) return;
            }

            if (!EventSystem.current || !gridUsingComponent)
            {
                enabled = false;

                return;
            }

            if (tileObjectAndTileMenuDict == null || tileObjectAndTileMenuDict.Count == 0)
            {
                enabled = false;

                return;
            }

            CheckAndProcessTileMenuInteractionOnMouseClick();
        }

        private void CheckAndProcessTileMenuInteractionOnMouseClick()
        {
            if (Input.GetButtonDown("Fire1"))
            {
                raycastResults.Clear();

                pointerEventData.position = Input.mousePosition;

                EventSystem.current.RaycastAll(pointerEventData, raycastResults);

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

                    //if a clicked on element is not a child (part of) the grid that is using this component
                    //this means that it is definitely not a child Tile or any of a child Tile's children/components
                    //therefore, skip it and continue...
                    if (raycastResults[i].gameObject.transform.root.gameObject != gridUsingComponent.gameObject)
                    {
                        continue;
                    }

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

        public void SetGridAndGetTilesInGridOnStart(TDGrid grid)
        {
            if (!grid) return;

            gridUsingComponent = grid;

            Tile[] tilesInGrid = grid.GetGridFlattened2DArray();

            if(tilesInGrid == null || tilesInGrid.Length == 0)
            {
                if(enabled) enabled = false;

                return;
            }

            for(int i = 0; i < tilesInGrid.Length; i++)
            {
                if (!tilesInGrid[i] || !tilesInGrid[i].tileMenuAndUprootOnTileUI) continue;

                tileObjectAndTileMenuDict.TryAdd(tilesInGrid[i].gameObject, tilesInGrid[i].tileMenuAndUprootOnTileUI);
            }

            if (tileObjectAndTileMenuDict == null || tileObjectAndTileMenuDict.Count == 0)
            {
                if (enabled) enabled = false;

                return;
            }

            if (!enabled) enabled = true;
        }
    }
}
