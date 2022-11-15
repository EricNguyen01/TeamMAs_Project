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
    public class TDGrid : MonoBehaviour
    {
        [field: SerializeField] [field: Min(1)]
        public int gridWidth { get; private set; } = 10;//number of columns

        [field: SerializeField] [field: Min(0)] 
        public int gridHeight { get; private set; } = 5;//number of rows

        [field: SerializeField] [field: Min(0)] 
        public float tileSize { get; private set; } = 1.0f;//size of a square tile in the grid (e.g 1x1)

        [SerializeField] private Tile tilePrefabToPopulate;//prefab with tile script attached

        [SerializeField] [HideInInspector] private Tile[] gridArray;//the 2D array representing the grid that has been flattened into a 1D array

        //PUBLICS...........................................................

        public Tile[] GetGridFlattened2DArray()
        {
            return gridArray;
        }

        //the method below returns the grid array index correspond to the provided tile coordinate in the grid.
        public int GetGridArrayIndexFromTileCoordinate(int tileGrid_X, int tileGrid_Y)
        {
            return tileGrid_X * gridHeight/*the array height*/ + tileGrid_Y;
        }


        //PRIVATES...................................................................

        private bool CanGenerateGrid()//check for all neccessary requirements before generating the grid
        {
            if (tilePrefabToPopulate == null)
            {
                Debug.LogError("Tile Prefab is missing on Grid object: " + name + ". Disabling grid!");
                return false;
            }
            if(gridWidth < 1 && gridHeight <= 0)
            {
                Debug.LogError("Please provide a valid row and column number input!");
                return false;
            }
            if(tileSize <= 0)
            {
                Debug.LogError("Tile size cannot be smaller or equal 0!");
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

                    if (!gridArray[index].GetComponent<BoxCollider2D>())
                    {
                        gridArray[index].gameObject.AddComponent<BoxCollider2D>();
                    }

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
            //if a grid was generated and is housing children tiles -> destroy all the children tiles 
            if(transform.childCount > 0)
            {
                //have to bring all the children tile game object into a temp array because for some reasons,
                //Unity doesn't destroy all the children objects in edit mode unless it is in a new array
                var tempArray = new GameObject[transform.childCount];

                for (int i = 0; i < tempArray.Length; i++)
                {
                    tempArray[i] = transform.GetChild(i).gameObject;
                }

                foreach (var child in tempArray)
                {
                    DestroyImmediate(child);
                }
            }

            gridArray = null;//set grid to null so that it can be re-initialized and re-generated in CreateGrid() function.
        }
    
    //UNITY EDITOR only class and function for Grid
    #if UNITY_EDITOR
        [CustomEditor(typeof(TDGrid))]
        private class GridEditor : Editor
        {
            TDGrid grid;
            private void OnEnable()
            {
                grid = target as TDGrid;
            }

            public override void OnInspectorGUI()
            {
                DrawDefaultInspector();

                EditorGUILayout.HelpBox(
                    "Generating the grid removes all of its current children Tiles and makes new ones based on the current grid's settings. " +
                    "New children tiles have default Tile settings", MessageType.Info);

                //Create a button to execute the generation of the grid in the editor.
                //Clicking the button regenerates the grid based on current grid settings.
                //Regenerate grid deletes old tiles and create new children tiles with default values.
                if(GUILayout.Button("Generate Grid"))//On Generate Grid button pressed:...
                {
                    if (!grid.CanGenerateGrid()) return;//if can not generate grid->do nothing
                    //else
                    grid.ResetGrid();
                    grid.CreateGrid();
                }
            }
        }
    #endif
    }
}
