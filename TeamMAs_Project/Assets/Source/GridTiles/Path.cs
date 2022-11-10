using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TeamMAsTD
{
    [DisallowMultipleComponent]
    public class Path : MonoBehaviour
    {
        [field: SerializeField] public List<Tile> orderedPathTiles { get; private set; } = new List<Tile>();

        [SerializeField] [HideInInspector] private List<Tile> oldOrderedPathTiles = new List<Tile>();

        /*private void Awake()
        {
            //On awake in editor only, gets a snapshot of the current ordered path tiles immediately and stores in oldOrderedPathTiles.
            if (Application.isEditor)
            {
                oldOrderedPathTiles = orderedPathTiles;
            }
        }*/

        private bool CanUpdatePath()
        {
            if (orderedPathTiles == null)
            {
                Debug.LogError("Path's tiles list is null! Make sure the list is intialized.");
                return false;
            }
            if (orderedPathTiles.Count == 0)
            {
                Debug.LogWarning("Path's tiles list is empty! Ideally, there should be at least a start and an end tile for a path to be generated!");
                return false;
            }

            return true;
        }

        //This function goes through the "orderedPathTiles" list that has been set in the inspector
        //and for each tile in the list that is not set with AI path status, set them as AI path
        //also, stores new AI path tiles into the "oldOrderedPathTiles" which is used for comparison with the modified path in later updates.
        private void SetOrderedPathTilesList()
        {
            for (int i = 0; i < orderedPathTiles.Count; i++)
            {
                //if somewhere in the path has no tile->path is invalid
                if (orderedPathTiles[i] == null)
                {
                    Debug.LogWarning("Path doesn't have a tile at element:" + i);
                    continue;
                }

                //else if path is still valid, do the below:

                //add new AI path tile to old ordered list
                //no need to check for duplicates when adding to oldOrderedPathTiles list as
                //either it is empty (1st update) or if not empty then we alr cleared the list on last update
                oldOrderedPathTiles.Add(orderedPathTiles[i]);

                //if the current tile element is alr set as an AI path -> ignore and continue.
                if (orderedPathTiles[i].is_AI_Path) continue;

                //if the current tile element is NOT an AI path -> set it as an AI path tile.
                orderedPathTiles[i].is_AI_Path = true;
            }
        }

        private void UpdatePath()
        {
            if (!CanUpdatePath()) return;

            //if this is the first time the path is updated:
            if (oldOrderedPathTiles == null || oldOrderedPathTiles.Count == 0)
            {
                //the function below
                //goes through the orderedPathTiles list and sets each tile in it to AI path status.
                //each newly updated tile in path will also be stored in oldOrderedPath list for comparison in later updates.
                SetOrderedPathTilesList();

                return;//stop executing function
            }

            //else if this is NOT the first time the path is updated:

            //FIRST, compare bt/ the old and current path to see if any tile is no longer AI path:
            //Begins by go through the list of old ordered path tiles (snapshot of the last path update):
            for(int i = 0; i < oldOrderedPathTiles.Count; i++)
            {
                if (oldOrderedPathTiles[i] == null) continue;

                //if the current tile element in the oldOrderedPathTiles list still persists in the modified orderedPathTiles list-> continue.
                if (orderedPathTiles.Contains(oldOrderedPathTiles[i])) continue;
                
                //if the current tile element in the old tile list is no longer in the current list -> removes its AI path status.
                oldOrderedPathTiles[i].is_AI_Path= false;
            }

            //after comparison, clear the old path to prepare for update
            oldOrderedPathTiles.Clear();

            //update the current path and then take a snapshot of it into oldOrderedPathTiles list.
            SetOrderedPathTilesList();
        }

        private void ClearPath()
        {
            if (orderedPathTiles == null || orderedPathTiles.Count == 0) return;
            
            //set all the current tiles in the current path tiles list to NON AI path.
            for (int i = 0; i < orderedPathTiles.Count; i++)
            {
                if (orderedPathTiles[i] == null) continue;

                orderedPathTiles[i].is_AI_Path = false;
            }
            //then:
            //clear the current path list
            orderedPathTiles.Clear();
            //also clear the old list
            oldOrderedPathTiles.Clear();
        }

        //UNITY EDITOR only functions and class
    #if UNITY_EDITOR
        [CustomEditor(typeof(Path))]
        private class PathEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                Path path = target as Path;

                DrawDefaultInspector();

                //Path script inspector text message box for what the path tiles list is and how it operates
                EditorGUILayout.HelpBox(
                    "This list represents the path. " +
                    "The first tile element is the start point and the last is the end point. " +
                    "The AIs will travel from one tile to the next based on their order in this list.", 
                    MessageType.Info);

                EditorGUILayout.Space();

                EditorGUILayout.HelpBox(
                    "Please update path manually after making changes! " +
                    "Changes may take a while to reflect in Scene view.", 
                    MessageType.Warning);

                //Draw update path button
                if(GUILayout.Button("Update Path"))
                {
                    path.UpdatePath();
                }
                
                //Draw clear path button
                if(GUILayout.Button("Clear Path"))
                {
                    path.ClearPath();
                }
            }
        }
    #endif
    }
}
