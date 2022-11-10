using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif 

namespace TeamMAsTD
{
    [ExecuteInEditMode]
    public class Tile : MonoBehaviour
    {
        [field: Header("Tile Properties")]
        [field: SerializeField] public bool isOccupied { get; set; } = false;
        [field: SerializeField] public bool is_AI_Path { get; set; } = false;

        [Header("Tile Debug Config")]
        [SerializeField] private bool drawTileDebug = true;

        //Internal........................................................
        [field: SerializeField] [field: HideInInspector]
        public int tileNumInRow { get; private set; }//position X in the grid (not in world space)

        [field: SerializeField] [field: HideInInspector]
        public int tileNumInColumn { get; private set; }//position Y in the grid (not in world space)

        [field: SerializeField] [field: HideInInspector]
        public Grid gridParent { get; private set; }//the grid that is housing this tile

        //the method below gets and sets the tile's data from the Grid class upon grid generation
        public void InitializeTile(Grid parentGrid, int numInRow, int numInColumn)
        {
            tileNumInRow = numInRow;
            tileNumInColumn = numInColumn;
            gridParent = parentGrid;
            
            //set name for the tile game object to its coordinate in the grid for readability (we don't want smth like tile(1) as a name).
            gameObject.name = "Tile" + tileNumInRow.ToString() + "." + tileNumInColumn.ToString();
        }

    //Tile editor stuff.......................................................
    #if UNITY_EDITOR
        //Draw the debug gizmos for this tile in scene view only
        private void OnDrawGizmos()
        {
            if (!drawTileDebug) return;
            
            if (isOccupied)//if tile is occupied->tile is grey
            {
                Gizmos.color = Color.grey;
                Gizmos.DrawCube(transform.position, new Vector2(transform.localScale.x - 0.1f, transform.localScale.y - 0.1f));
            }
            else//if not occupied->tile is green
            {
                Gizmos.color = Color.green;
                Gizmos.DrawCube(transform.position, new Vector2(transform.localScale.x - 0.1f, transform.localScale.y - 0.1f));
            }

            if (is_AI_Path)//if tile is an AI path tile->tile is red
            {
                Gizmos.color = Color.red;
                Gizmos.DrawCube(transform.position, new Vector2(transform.localScale.x - 0.1f, transform.localScale.y - 0.1f));
            }
        }

        [CustomEditor(typeof(Tile))]
        private class TileEditor : Editor
        {
            Tile tile;

            private void OnEnable()
            {
                tile = target as Tile;
            }

            private void OnSceneGUI()
            {
                GUI.color = Color.black;
                Handles.Label((Vector2)tile.transform.position + Vector2.right * -0.2f, "" + tile.tileNumInRow + "," + tile.tileNumInColumn);
            }
        }
    #endif
    }
}
