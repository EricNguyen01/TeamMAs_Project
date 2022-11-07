using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TeamMAsTD
{
    public class Tile : MonoBehaviour
    {
        [field: Header("Tile Properties")]
        [field: SerializeField] private bool isOccupied { get; set; } = false;
        [field: SerializeField] private bool is_AI_Path { get; set; } = false;

        [Header("Tile Debug Config")]
        [SerializeField] private bool drawTileDebug = true;

        //Internal........................................................
        public int tileNumInRow { get; private set; } = 0;//position X in the grid (not in world space)
        public int tileNumInColumn { get; private set; } = 0;//position Y in the grid (not in world space)
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

        private void OnDrawGizmos()
        {
            if (!drawTileDebug) return;
            
            if (isOccupied)
            {
                Gizmos.color = Color.grey;
                Gizmos.DrawCube(transform.position, new Vector2(transform.localScale.x - 0.1f, transform.localScale.y - 0.1f));
            }
            else
            {
                Gizmos.color = Color.green;
                Gizmos.DrawCube(transform.position, new Vector2(transform.localScale.x - 0.1f, transform.localScale.y - 0.1f));
            }

            if (is_AI_Path)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawCube(transform.position, new Vector2(transform.localScale.x - 0.1f, transform.localScale.y - 0.1f));
            }
        }
    }
}
