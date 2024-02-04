// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TeamMAsTD
{
    [DisallowMultipleComponent]
    [ExecuteInEditMode]
    public class TDGrid : MonoBehaviour, ISaveable
    {
        [field: SerializeField] [field: Min(1)]
        public int gridWidth { get; private set; } = 10;//number of columns

        [field: SerializeField] [field: Min(0)] 
        public int gridHeight { get; private set; } = 5;//number of rows

        [field: SerializeField] [field: Min(1.0f)] 
        public float tileSize { get; private set; } = 1.0f;//size of a square tile in the grid (e.g 1x1)

        [SerializeField] private Tile tilePrefabToPopulate;//prefab with tile script attached

        [SerializeField] [HideInInspector] private Tile[] gridArray;//the 2D array representing the grid that has been flattened into a 1D array

        [Space(15.0f)]

        [SerializeField] private UnityEvent OnFirstDandelionPlantedOnGrid;

        [SerializeField] private UnityEvent OnFirstCloverPlantedOnGrid;

        [SerializeField] public UnityEvent<PlantUnit> OnFirstPlantPlantedOnGrid;

        [HideInInspector] private List<Tile> unplantedTileList = new List<Tile>();

        public List<Sprite> tileSpritesList { get; private set; } = new List<Sprite>();

        private bool alreadyHasDandelionOnGrid = false;//for debugging

        private bool alreadyHasCloverOnGrid = false;

        [System.Serializable]
        private class GridSave
        {
            public bool alreadyHasDandyOnGrid { get; private set; }
            public bool alreadyHasCloverOnGrid { get; private set; }

            public GridSave(bool hasDandyOnGrid, bool hasCloverOnGrid)
            {
                alreadyHasCloverOnGrid = hasCloverOnGrid;

                alreadyHasDandyOnGrid = hasDandyOnGrid;
            }
        }

        //UNITY

        private void Awake()
        {
            GetAllSpritesFromChildrenTiles();
        }

        private void OnEnable()
        {
            WaveSpawner.OnWaveStarted += SpawnPlantOnWaveStarted;
        }

        private void OnDisable()
        {
            WaveSpawner.OnWaveStarted -= SpawnPlantOnWaveStarted;
        }

        private void Start()
        {
            if (!SaveLoadHandler.HasSavedData())
            {
                //TO-DO: Add function to randomize blockers and path on the grid here if no save data existed (new game)...
            }
        }

        //PUBLICS...........................................................

        public Tile[] GetGridFlattened2DArray()
        {
            return gridArray;
        }

        public float GetDistanceFromTileNumber(int tileNumber)
        {
            return (tileSize * tileNumber) + (tileSize / 2.0f);
        }

        //the method below returns the grid array index correspond to the provided tile coordinate in the grid.
        public int GetGridArrayIndexFromTileCoordinate(Vector2Int tileCoordInt)
        {
            return GetGridArrayIndexFromTileCoordinate(tileCoordInt.x, tileCoordInt.y);
        }

        public int GetGridArrayIndexFromTileCoordinate(int tileGrid_X, int tileGrid_Y)
        {
            if (tileGrid_X < 0) tileGrid_X = 0;

            if (tileGrid_X > gridWidth) tileGrid_X = gridWidth - 1;

            if (tileGrid_Y < 0) tileGrid_Y = 0;

            if (tileGrid_Y > gridHeight) tileGrid_Y = gridHeight - 1;

            return tileGrid_X * gridHeight/*the array height*/ + tileGrid_Y;
        }

        public Tile GetTileFromTileCoordinate(Vector2Int tileCoordInt)
        {
            if (gridArray == null || gridArray.Length == 0) return null;

            if(tileCoordInt.x < 0 || tileCoordInt.x > gridWidth || tileCoordInt.y < 0 || tileCoordInt.y > gridHeight) return null;

            int tileGridArrayIndex = GetGridArrayIndexFromTileCoordinate(tileCoordInt);

            return gridArray[tileGridArrayIndex];
        }

        //iterate through the grid to check if any plant has been planted before or not
        public void CheckPlantUnitAsFirstUnitOfTypeOnGrid(PlantUnit plantUnit)
        {
            if (gridArray == null || gridArray.Length == 0) return;

            bool isFirstPlant = true;

            for (int i = 0; i < gridArray.Length; i++)
            {
                if(gridArray[i].plantUnitOnTile != null &&
                   gridArray[i].plantUnitOnTile != plantUnit)
                {
                    isFirstPlant = false;

                    if (gridArray[i].plantUnitOnTile.plantUnitScriptableObject.displayName == "Dandelion")
                    {
                        alreadyHasDandelionOnGrid = true;
                    }

                    if (gridArray[i].plantUnitOnTile.plantUnitScriptableObject.displayName == "Clover")
                    {
                        alreadyHasCloverOnGrid = true;
                    }
                }
            }

            if (isFirstPlant)
            {
                OnFirstPlantPlantedOnGrid?.Invoke(plantUnit);
            }

            if (plantUnit.plantUnitScriptableObject.displayName == "Dandelion")
            {
                if (!alreadyHasDandelionOnGrid)
                {
                    //Debug.Log("First Dandelion Has Been Planted On Grid!");

                    alreadyHasDandelionOnGrid = true;

                    OnFirstDandelionPlantedOnGrid?.Invoke();
                }
            }

            if(plantUnit.plantUnitScriptableObject.displayName == "Clover")
            {
                if (!alreadyHasCloverOnGrid)
                {
                    //Debug.Log("First Clover Planted On Grid!");

                    alreadyHasCloverOnGrid = true;

                    OnFirstCloverPlantedOnGrid?.Invoke();
                }
            }
        }

        public List<Tile> GetUnplantedTiles()
        {
            if (gridArray == null || gridArray.Length == 0) return null;

            for(int i = 0; i < gridArray.Length; i++)
            {
                if (gridArray[i].isOccupied) continue;

                if (gridArray[i].plantUnitOnTile == null)
                {
                    if(!unplantedTileList.Contains(gridArray[i])) unplantedTileList.Add(gridArray[i]);

                    continue;
                }

                if(gridArray[i].plantUnitOnTile != null)
                {
                    if (unplantedTileList.Contains(gridArray[i])) unplantedTileList.Remove(gridArray[i]);
                }
            }

            return unplantedTileList;
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

        private void SpawnPlantOnWaveStarted(WaveSpawner waveSpawner, int waveNum)
        {
            GetUnplantedTiles();

            if (unplantedTileList == null || unplantedTileList.Count == 0) return;

            if (waveSpawner == null) return;

            if (waveSpawner.GetCurrentWave() == null) return;

            Wave wave = waveSpawner.GetCurrentWave();

            if (wave.waveSO == null ||
                wave.waveSO.plantsToSpawnOnThisWaveStart == null ||
                wave.waveSO.plantsToSpawnOnThisWaveStart.Length == 0) return;

            for(int i = 0; i < wave.waveSO.plantsToSpawnOnThisWaveStart.Length; i++)
            {
                if (unplantedTileList.Count == 0) return;

                if (wave.waveSO.plantsToSpawnOnThisWaveStart[i].plantUnitSOToSpawn == null) continue;

                PlantUnitSO plantSO = wave.waveSO.plantsToSpawnOnThisWaveStart[i].plantUnitSOToSpawn;

                Vector2Int plantCoord = Vector2Int.zero;

                int previousTilesIndex = 0;

                for(int j = 0; j < wave.waveSO.plantsToSpawnOnThisWaveStart[i].spawnNumbers; j++)
                {
                    if (unplantedTileList.Count == 0) break;

                    int unplantedTilesIndex = UnityEngine.Random.Range(0, unplantedTileList.Count);

                    if(unplantedTileList.Count > 1 && unplantedTilesIndex == previousTilesIndex)
                    {
                        int count = 0;

                        while(count < 5 && unplantedTilesIndex == previousTilesIndex)
                        {
                            unplantedTilesIndex = UnityEngine.Random.Range(0, unplantedTileList.Count);

                            count++;
                        }
                    }

                    previousTilesIndex = unplantedTilesIndex;

                    plantCoord = new Vector2Int(unplantedTileList[unplantedTilesIndex].tileNumInRow, unplantedTileList[unplantedTilesIndex].tileNumInColumn);

                    if(SpawnPlantOnTileCoord(plantSO, plantCoord))
                    {
                        unplantedTileList.RemoveAt(unplantedTilesIndex);
                    }
                }
            }
        }

        private bool SpawnPlantOnTileCoord(PlantUnitSO plantSO, Vector2Int tileCoordInt)
        {
            int tileIndexInGrid = GetGridArrayIndexFromTileCoordinate(tileCoordInt);

            return gridArray[tileIndexInGrid].PlaceUnit(plantSO);
        }

        private void GetAllSpritesFromChildrenTiles()
        {
            if (gridArray == null || gridArray.Length == 0) return;

            if(tileSpritesList != null && tileSpritesList.Count > 0) tileSpritesList.Clear();

            for(int i = 0; i < gridArray.Length; i++)
            {
                if (!gridArray[i] || !gridArray[i].spriteRenderer || !gridArray[i].spriteRenderer.sprite) continue;

                if (tileSpritesList.Contains(gridArray[i].spriteRenderer.sprite)) continue;

                tileSpritesList.Add(gridArray[i].spriteRenderer.sprite);
            }
        }

        private void GenerateRandomBlockersAndPathOnGrid()
        {
            Path path = FindObjectOfType<Path>();

            if(path == null)
            {
                GameObject go = new GameObject("Path");
            }
        }

        //ISaveable interface implementations for saving/loading.............................................................................

        public SaveDataSerializeBase SaveData(string saveName = "")
        {
            SaveDataSerializeBase gridSaveData;

            GridSave gridSave = new GridSave(alreadyHasDandelionOnGrid, alreadyHasCloverOnGrid);

            gridSaveData = new SaveDataSerializeBase(gridSave,
                                                     transform.position,
                                                     UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);

            return gridSaveData;
        }

        public void LoadData(SaveDataSerializeBase savedDataToLoad)
        {
            if (savedDataToLoad == null) return;

            GridSave gridSavedData = (GridSave)savedDataToLoad.LoadSavedObject();

            alreadyHasDandelionOnGrid = gridSavedData.alreadyHasDandyOnGrid;

            alreadyHasCloverOnGrid = gridSavedData.alreadyHasCloverOnGrid;
        }

        //UNITY EDITOR only class and function for Grid....................................................................

#if UNITY_EDITOR

        [CustomEditor(typeof(TDGrid))]
        private class GridEditor : Editor
        {
            private TDGrid grid;

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

                //Create a custom inspector button to execute the generation of the grid in the editor.
                //Clicking the button regenerates the grid based on current grid settings.
                //Regenerate grid deletes old tiles and create new children tiles with default values.
                //Buttons are disabled during runtime.
                using (new EditorGUI.DisabledGroupScope(Application.isPlaying))
                {
                    if (GUILayout.Button("Generate Grid"))//On Generate Grid button pressed:...
                    {
                        if (!grid.CanGenerateGrid()) return;//if can not generate grid->do nothing

                        grid.ResetGrid();

                        grid.CreateGrid();
                    }
                }
            }
        }
    #endif
    }
}
