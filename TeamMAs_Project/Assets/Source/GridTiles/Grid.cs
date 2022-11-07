using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace TeamMAsTD
{
    public class Grid : MonoBehaviour
    {
        [field: SerializeField] public int gridWidth { get; private set; } = 10;//number of columns
        [field: SerializeField] public int gridHeight { get; private set; } = 5;//number of rows
        [field: SerializeField] public float tileSize { get; private set; } = 1.0f;//size of a square tile in the grid (e.g 1x1)

        [SerializeField] private Tile tilePrefabToPopulate;//prefab with tile script attached

        private Tile[] gridArray;//the 2D array representing the grid that has been flattened into a 1D array

        private void Awake()
        {

        }

        private bool CanGenerateGrid()//check for all neccessary requirements before generating the grid
        {
            if (tilePrefabToPopulate == null)
            {
                Debug.LogError("Tile Prefab is missing on Grid object: " + name + ". Disabling grid!");
                return false;
            }

            return true;
        }

        private Vector2 GetTileWorldPos(int tileGridPosX, int tileGridPosY)
        {
            float x = transform.position.x + (tileGridPosX * tileSize);
            float y = transform.position.y + (tileGridPosY * tileSize);
            return new Vector2(x, y);
        }

        //the method below returns the grid array index correspond to the provided tile coordinate in the grid.
        public int GetGridArrayIndexFromTileCoordinate(int tileGrid_X, int tileGrid_Y)
        {
            return tileGrid_X * gridHeight/*the array height*/ + tileGrid_Y;
        }

        private void CreateGrid()//grid-generation method
        {
            if (!CanGenerateGrid()) return;

            if (gridArray == null)
            {
                gridArray = new Tile[gridWidth * gridHeight];//initialize grid array with length as the total tiles in grid
            }

            int index = 0;//index of the 1D flattened grid from 2D array 
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    Vector2 currentTileWorldPos = GetTileWorldPos(x, y);
                    gridArray[index] = Instantiate(tilePrefabToPopulate, currentTileWorldPos, Quaternion.identity, transform);
                    gridArray[index].InitializeTile(this, x, y);
                    gridArray[index].transform.localScale = new Vector2(tileSize, tileSize);
                    index++;
                }
            }
        }

        //The function below destroys all current children tiles of this grid and reset the grid to null
        //so that the grid can be regenerated in CreateGrid() function.
        private void ResetGrid()
        {
            //cancel reset if no grid has been generated
            if (gridArray == null || gridArray.Length < 1) return;
            
            //if a grid was generated and is housing children tiles -> destroy all the children tiles 
            if(transform.childCount > 0)
            {
                for (int i = 0; i < gridArray.Length; i++)
                {
                    DestroyImmediate(gridArray[i].gameObject);
                }
            }

            gridArray = null;//set grid to null so that it can be re-initialized and re-generated in CreateGrid() function.
        }
    
    //UNITY EDITOR class and function for this grid
    #if UNITY_EDITOR
        [CustomEditor(typeof(Grid))]
        private class GridEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                DrawDefaultInspector();

                Grid grid = target as Grid;

                //Create a button to execute the generation of the grid in the editor.
                //Clicking the button regenerates the grid based on current grid settings.
                //Regenerate grid deletes old tiles and create new children tiles with default values.
                if(GUILayout.Button("Generate Grid"))
                {
                    grid.ResetGrid();
                    grid.CreateGrid();
                }
            }
        }
    #endif
    }
}
