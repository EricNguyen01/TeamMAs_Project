// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TeamMAsTD
{
    [DisallowMultipleComponent]
    [ExecuteInEditMode]
    public class Path : MonoBehaviour
    {
        [SerializeField] private TDGrid gridPathOn;

        private TDGrid currentGridPathOn;

        [field: Space]

        [field: Header("Path Visuals")]

        [field: SerializeField] public Sprite pathTileSprite { get; private set; }

        [SerializeField] [HideInInspector] private Sprite dirtTileSprite;

        [SerializeField][HideInInspector] private Sprite defaultTileSprite;

        [field: Space()]

        [field: Header("Set Path Manually")]

        [field: SerializeField] private bool setPathManually = true;

        [System.Serializable]
        private class OrderedPathTilesWrapper
        {
            [field: SerializeField]
            public List<Tile> orderedPathTiles { get; private set; } = new List<Tile>();

            public OrderedPathTilesWrapper() { }

            public OrderedPathTilesWrapper(List<Tile> orderedPathTiles)
            {
                this.orderedPathTiles = orderedPathTiles;
            }
        }

        [SerializeField]
        [DisableIf("setPathManually", false)]
        private OrderedPathTilesWrapper orderedPathTiles = new OrderedPathTilesWrapper();

        [Header("Auto Generate Path")]

        [SerializeField]
        [DisableIf("setPathManually", true)]
        [ReadOnlyInspectorPlayMode]
        private PathGenerator pathGenerator = new PathGenerator();

        [Header("Path Debug")]
        [SerializeField] private bool showDebugLog = true;

        //This list is used for when the user updating the path tiles list in the editor
        [SerializeField] [HideInInspector] private List<Tile> oldOrderedPathTiles = new List<Tile>();

        private bool isGeneratingPath = false;

        private bool isUpdatingPath = false;

        //PRIVATES................................................................................

        private void Awake()
        {
            if (!Application.isEditor)
            {
                showDebugLog = false;

                if (pathGenerator != null) pathGenerator.showDebug = false;
            }
        }

        private void OnEnable()
        {
            currentGridPathOn = gridPathOn;

            pathGenerator.SetGridPathOn(gridPathOn);

#if UNITY_EDITOR

            dirtTileSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprites/Dirt.png");

            defaultTileSprite = 
            AssetDatabase.LoadAssetAtPath<Sprite>("Packages/com.unity.2d.sprite/Editor/ObjectMenuCreation/DefaultAssets/Textures/SquareWithBorder.png");
#endif
        }

        private void OnDisable()
        {

#if UNITY_EDITOR

            if (pathGenerator != null && isGeneratingPath)
            {
                isGeneratingPath = false;

                pathGenerator.ResetAll();
            }
#endif

        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if(gridPathOn != null && gridPathOn != currentGridPathOn)
            {
                pathGenerator.SetGridPathOn(gridPathOn);

                currentGridPathOn = gridPathOn;
            }
        }
#endif

        private bool CanUpdatePath()
        {
            if (orderedPathTiles == null)
            {
                Debug.LogError("Path's tiles list is null! Make sure the list is intialized. Path update failed!");

                return false;
            }
            if (orderedPathTiles.orderedPathTiles.Count == 0)
            {
                Debug.LogWarning("Path's tiles list is empty! " +
                                 "Ideally, there should be at least a start and an end tile for a path to be generated." +
                                 "Path update failed!");

                return false;
            }

            return true;
        }

        //This function goes through the "orderedPathTiles" list that has been set in the inspector
        //and for each tile in the list that is not set with AI path status, set them as AI path
        //also, stores new AI path tiles into the "oldOrderedPathTiles" which is used for comparison with the modified path in later updates.
        private void SetOrderedPathTilesList()
        {
            for (int i = 0; i < orderedPathTiles.orderedPathTiles.Count; i++)
            {
                //if somewhere in the path has no tile->path is invalid
                if (orderedPathTiles.orderedPathTiles[i] == null)
                {
                    Debug.LogWarning("Path doesn't have a tile at element:" + i);
                    continue;
                }

                //else if path is still valid, do the below:

                //add new AI path tile to old ordered list
                //no need to check for duplicates when adding to oldOrderedPathTiles list as
                //either it is empty (1st update) or if not empty then we alr cleared the list on last update
                oldOrderedPathTiles.Add(orderedPathTiles.orderedPathTiles[i]);

                //if (orderedPathTiles.orderedPathTiles[i].isOccupied) orderedPathTiles.orderedPathTiles[i].isOccupied = false;

                //if the current tile element is not set as an AI path -> set it as AI path.
                if (!orderedPathTiles.orderedPathTiles[i].is_AI_Path) orderedPathTiles.orderedPathTiles[i].is_AI_Path = true;

                //set new path tile sprite if there's one provided
                SpriteRenderer tileSpriteRenderer = orderedPathTiles.orderedPathTiles[i].GetComponent<SpriteRenderer>();

                SetPathTileSprite(orderedPathTiles.orderedPathTiles[i], tileSpriteRenderer);
            }
        }

        private void SetPathTileSprite(Tile tile, SpriteRenderer tileSpriteRenderer)
        {
            if (tile == null || tileSpriteRenderer == null) return;

            if (pathTileSprite == null) return;

            tileSpriteRenderer.sprite = pathTileSprite;

            if(showDebugLog) Debug.Log("Path Tile: " + tile.name + " is set with new tile sprite!");

            tile.EnableDrawTileDebug(false);
        }

        private void RemovePathTileSprite(Tile tile, SpriteRenderer tileSpriteRenderer)
        {
            if (tile == null || tileSpriteRenderer == null) return;

            if (tileSpriteRenderer.sprite != null && tileSpriteRenderer.sprite.name.Contains("Path"))
            {
                if (gridPathOn)
                {
                    tileSpriteRenderer.sprite = gridPathOn.unOccupiedDirtTileSprite;
                }
            }

            if (!tileSpriteRenderer.sprite)
            {
                if (dirtTileSprite) tileSpriteRenderer.sprite = dirtTileSprite;
                else if (defaultTileSprite) tileSpriteRenderer.sprite = defaultTileSprite;
            }

            tile.EnableDrawTileDebug(true);
        }

        private void AutoGeneratePath()
        {
            if (pathGenerator == null) return;

            isGeneratingPath = true;

            ClearPath();

            if(orderedPathTiles == null) orderedPathTiles = new OrderedPathTilesWrapper();

            if (orderedPathTiles.orderedPathTiles.Count > 0) orderedPathTiles.orderedPathTiles.Clear();

            try
            {
                orderedPathTiles.orderedPathTiles.AddRange(pathGenerator.GeneratePath());
            }
            catch
            {
                throw;
            }
            finally 
            { 
                isGeneratingPath = false; 
            }
        }

        private void UpdatePath()
        {
            if (!CanUpdatePath()) return;

            if (showDebugLog) Debug.Log("Path update started!");

            isUpdatingPath = true;

            //if this is the first time the path is updated:
            if (oldOrderedPathTiles == null || oldOrderedPathTiles.Count == 0)
            {
                if (showDebugLog) Debug.Log("Path is being updated for the first time!");

                //the function below
                //goes through the orderedPathTiles list and sets each tile in it to AI path status.
                //each newly updated tile in path will also be stored in oldOrderedPath list for comparison in later updates.
                SetOrderedPathTilesList();

                isUpdatingPath = false;

                return;//stop executing function
            }

            //else if this is NOT the first time the path is updated:

            //first, compare bt/ the old and current path to see if any tile is no longer AI path:
            //Begins by go through the list of old ordered path tiles (snapshot of the last path update):
            for(int i = 0; i < oldOrderedPathTiles.Count; i++)
            {
                if (oldOrderedPathTiles[i] == null) continue;

                //if the current tile element in the oldOrderedPathTiles list still persists in the modified orderedPathTiles list-> continue.
                if (orderedPathTiles.orderedPathTiles.Contains(oldOrderedPathTiles[i])) continue;
                
                //if the current tile element in the old tile list is no longer in the current list -> removes its Visitor path status.
                oldOrderedPathTiles[i].is_AI_Path= false;
            }

            //after comparison, clear the old path to prepare for update
            oldOrderedPathTiles.Clear();

            //update the current path and then take a snapshot of it into oldOrderedPathTiles list.
            SetOrderedPathTilesList();

            isUpdatingPath = false;
        }

        private void ClearPath()
        {
            if (showDebugLog) Debug.Log("Clearing path started");

            if (gridPathOn)
            {
                Tile[] gridArr = gridPathOn.GetGridFlattened2DArray();

                if (gridArr != null && gridArr.Length > 0)
                {
                    for (int i = 0; i < gridArr.Length; i++)
                    {
                        if (gridArr[i] == null) continue;

                        if (gridArr[i].is_AI_Path)
                        {
                            gridArr[i].is_AI_Path = false;

                            SpriteRenderer tileSpriteRenderer;

                            gridArr[i].TryGetComponent<SpriteRenderer>(out tileSpriteRenderer);

                            if (tileSpriteRenderer.sprite != null) RemovePathTileSprite(gridArr[i], tileSpriteRenderer);
                        }
                    }
                }
            }
            else
            {
                if (orderedPathTiles != null && orderedPathTiles.orderedPathTiles != null && orderedPathTiles.orderedPathTiles.Count > 0)
                {
                    //set all the current tiles in the current path tiles list to NON AI path.
                    for (int i = 0; i < orderedPathTiles.orderedPathTiles.Count; i++)
                    {
                        if (orderedPathTiles.orderedPathTiles[i] == null) continue;

                        if (orderedPathTiles.orderedPathTiles[i].is_AI_Path) orderedPathTiles.orderedPathTiles[i].is_AI_Path = false;

                        //remove the AI Path sprite on tile if one exists
                        SpriteRenderer tileSpriteRenderer;

                        orderedPathTiles.orderedPathTiles[i].TryGetComponent<SpriteRenderer>(out tileSpriteRenderer);

                        if (tileSpriteRenderer.sprite != null) RemovePathTileSprite(orderedPathTiles.orderedPathTiles[i], tileSpriteRenderer);
                    }
                }
            }

            //clear the current path list
            if(orderedPathTiles != null && 
               orderedPathTiles.orderedPathTiles != null && 
               orderedPathTiles.orderedPathTiles.Count > 0) orderedPathTiles.orderedPathTiles.Clear();

            //also clear the old list
            if (oldOrderedPathTiles != null && oldOrderedPathTiles.Count > 0) oldOrderedPathTiles.Clear();
        }

        //PUBLICS..............................................................................

        public List<Tile> GetOrderedPathTiles()
        {
            if(orderedPathTiles.orderedPathTiles == null) return new List<Tile>();

            return orderedPathTiles.orderedPathTiles;
        }

        public TDGrid GetGridPathOn()
        {
            return gridPathOn;
        }

        public void SetGridPathOn(TDGrid grid)
        {
            gridPathOn = grid;

            if(pathGenerator != null) pathGenerator.SetGridPathOn(grid);
        }

        public void SetPathSprite(Sprite pathSprite)
        {
            pathTileSprite = pathSprite;
        }

        public void AutoGeneratePath(bool updatePathAfter = true)
        {
            AutoGeneratePath();

            if(updatePathAfter) UpdatePath();
        }

    //EDITOR.............................................................................

    //UNITY EDITOR only functions and class
    #if UNITY_EDITOR
        [CustomEditor(typeof(Path))]
        private class PathEditor : Editor
        {
            private Path path;

            private void OnEnable()
            {
                path = target as Path;
            }

            public override void OnInspectorGUI()
            {
                DrawDefaultInspector();

                serializedObject.UpdateIfRequiredOrScript();

                EditorGUILayout.Space();

                using (new EditorGUI.DisabledGroupScope(path.isUpdatingPath || path.isGeneratingPath || path.setPathManually || Application.isPlaying))
                {
                    if (GUILayout.Button("Auto-Generate Path"))
                    {
                        path.AutoGeneratePath();
                    }
                }

                EditorGUILayout.Space();

                EditorGUILayout.HelpBox(
                    "Please update path manually using the UpdatePath button after making changes or generating path! " +
                    "Updates may take a while to reflect in Scene view.",
                    MessageType.Warning);

                EditorGUILayout.Space();

                using (new EditorGUI.DisabledGroupScope(path.isUpdatingPath || path.isGeneratingPath || Application.isPlaying))
                {
                    //Draw update path button
                    if (GUILayout.Button("Update Path"))
                    {
                        path.UpdatePath();
                    }

                    //Draw clear path button
                    if (GUILayout.Button("Clear Path"))
                    {
                        path.ClearPath();
                    }
                }
            }
        }
    #endif
    }
}
