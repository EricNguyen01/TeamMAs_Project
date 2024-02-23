// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Diagnostics.CodeAnalysis;

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

        [SerializeField] 
        [DisallowNull]
        private Tile tilePrefabToPopulate;//prefab with tile script attached

        [SerializeField] 
        [HideInInspector] 
        private Tile[] gridArray;//the 2D array representing the grid that has been flattened into a 1D array

        [System.Serializable]
        private class ReadOnlyGridArray
        {
            [SerializeField]
            private Tile[] readOnlyGridArray;

            public ReadOnlyGridArray() { }

            public ReadOnlyGridArray(Tile[] readOnlyGridArray)
            {
                this.readOnlyGridArray = readOnlyGridArray;
            }
        }

        [SerializeField]
        [ReadOnlyInspector]
        private ReadOnlyGridArray readOnlyGridArray;

        [field: SerializeField]
        [field: HideInInspector]
        //this dict below is for path save/load (see Path.cs file)
        public Dictionary<string, Tile> tileNameToTileDict { get; private set; } = new Dictionary<string, Tile>();

        [Space(15.0f)]

        [SerializeField] private UnityEvent OnFirstDandelionPlantedOnGrid;

        [SerializeField] private UnityEvent OnFirstCloverPlantedOnGrid;

        [SerializeField] public UnityEvent<PlantUnit> OnFirstPlantPlantedOnGrid;

        [Space]

        [SerializeField] private bool showDebugLog = true;

        [HideInInspector] private List<Tile> unplantedTileList = new List<Tile>();

        private Path pathOfGrid;
        public List<Sprite> tileSpritesList { get; private set; } = new List<Sprite>();

        public Sprite occupiedTileSprite { get; private set; }

        public Sprite unOccupiedDirtTileSprite { get; private set; }

        private bool alreadyHasDandelionOnGrid = false;//for debugging

        private bool alreadyHasCloverOnGrid = false;

        private bool isRandomizingGrid = false;

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

        //UNITY CALLBACKS

        private void Awake()
        {
            if(readOnlyGridArray == null) readOnlyGridArray = new ReadOnlyGridArray(gridArray);

            pathOfGrid = CreatePathObjectIfNone();
        }

        private void OnEnable()
        {
            SetTileNameToTileDictionary();

            GetAllSpritesFromChildrenTiles();

            if (!Application.isEditor) showDebugLog = false;

            if (Application.isPlaying) WaveSpawner.OnWaveStarted += SpawnPlantOnWaveStarted;
        }

        private void OnDisable()
        {
            if (Application.isPlaying) WaveSpawner.OnWaveStarted -= SpawnPlantOnWaveStarted;
        }

        private void Start()
        {
            //only generate new random grid layout on new game started (no save files)
            //if there's a save exists meaning that a grid layout has already been generated and will be loaded and overriding the current grid 
            //on game scene entered
            if (Application.isPlaying && !SaveLoadHandler.HasSavedData())
            {
                StartCoroutine(RandomizedGridLayoutAndSaveLayoutOnStart());
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

            if (tileGrid_X >= gridWidth) tileGrid_X = gridWidth - 1;

            if (tileGrid_Y < 0) tileGrid_Y = 0;

            if (tileGrid_Y >= gridHeight) tileGrid_Y = gridHeight - 1;

            return tileGrid_X * gridHeight/*the array height*/ + tileGrid_Y;
        }

        public Tile GetTileFromTileCoordinate(Vector2Int tileCoordInt)
        {
            if (gridArray == null || gridArray.Length == 0) return null;

            if(tileCoordInt.x < 0 || tileCoordInt.x >= gridWidth || tileCoordInt.y < 0 || tileCoordInt.y >= gridHeight) return null;

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
                if(showDebugLog) Debug.LogError("Tile Prefab is missing on Grid object: " + name + ". Disabling grid!");
                return false;
            }
            if(gridWidth < 1 && gridHeight <= 0)
            {
                if (showDebugLog) Debug.LogError("Please provide a valid row and column number input!");
                return false;
            }
            if(tileSize <= 0)
            {
                if (showDebugLog) Debug.LogError("Tile size cannot be smaller or equal 0!");
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

        private bool CreateGrid(bool generateRandomGridLayout = false)//grid-generation method
        {
            if (!CanGenerateGrid()) return false;

            if (gridArray != null && gridArray.Length > 0) ResetGrid();

            gridArray = new Tile[gridWidth * gridHeight];//initialize grid array with length as the total tiles in grid

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

            if (generateRandomGridLayout)
            {
                try { ProcedurallyRandomizedGridLayout(); }
                catch { throw; }
            }

            SetTileNameToTileDictionary();

            return true;
        }

        private void SetTileNameToTileDictionary()
        {
            if (gridArray == null || gridArray.Length == 0) return;

            if (tileNameToTileDict == null) tileNameToTileDict = new Dictionary<string, Tile>();

            if(tileNameToTileDict.Count > 0) tileNameToTileDict.Clear();

            for (int i = 0; i < gridArray.Length; i++)
            {
                if (!gridArray[i]) continue;

                tileNameToTileDict.TryAdd(gridArray[i].name, gridArray[i]);
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
            if (gridArray == null || gridArray.Length == 0) return;

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

            if(tileSpritesList == null) tileSpritesList = new List<Sprite>();

            if(tileSpritesList.Count > 0) tileSpritesList.Clear();

            for(int i = 0; i < gridArray.Length; i++)
            {
                if (!gridArray[i] || !gridArray[i].spriteRenderer || !gridArray[i].spriteRenderer.sprite) continue;

                if (tileSpritesList.Contains(gridArray[i].spriteRenderer.sprite)) continue;

                tileSpritesList.Add(gridArray[i].spriteRenderer.sprite);

                if (gridArray[i].spriteRenderer.sprite.name.Contains("Rock")) occupiedTileSprite = gridArray[i].spriteRenderer.sprite;

                if(gridArray[i].spriteRenderer.sprite.name.Contains("Dirt")) unOccupiedDirtTileSprite = gridArray[i].spriteRenderer.sprite;
            }

#if UNITY_EDITOR

            if (!Application.isEditor) return;

            if (Application.isPlaying) return;

            if (!occupiedTileSprite) 
                occupiedTileSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprites/Rocks.png");

            if(!occupiedTileSprite) 
                occupiedTileSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Packages/com.unity.2d.sprite/Editor/ObjectMenuCreation/DefaultAssets/Textures/SquareWithBorder.png");

            if (!unOccupiedDirtTileSprite)
                unOccupiedDirtTileSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprites/Dirt.png");

            if(!unOccupiedDirtTileSprite)
                unOccupiedDirtTileSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Packages/com.unity.2d.sprite/Editor/ObjectMenuCreation/DefaultAssets/Textures/SquareWithBorder.png");
#endif
        }

        private Path CreatePathObjectIfNone()
        {
            Path path = null;

            Path[] paths = FindObjectsOfType<Path>();

            for(int i = 0; i < paths.Length; i++)
            {
                if (paths[i] == null) continue;

                if (paths[i].GetGridPathOn() == this)
                {
                    return paths[i];
                }
            }

            for (int i = 0; i < paths.Length; i++)
            {
                if (paths[i] == null) continue;

                if (paths[i].GetGridPathOn() == null)
                {
                    paths[i].SetGridPathOn(this);

                    return paths[i];
                }
            }

            if (!path)
            {
                GameObject go = new GameObject("Path");

                path = go.AddComponent<Path>();

                path.SetGridPathOn(this);
            }

            return path;
        }

        private int iterationCount = 0;//to use for path gen iteration counts control inside the below function
        private void ProcedurallyRandomizedGridLayout()//should only call after a grid has been generated and is existing in the scene
        {
            if (isRandomizingGrid) return;

            isRandomizingGrid = true;

            MemoryUsageLogger.LogMemoryUsageAsText("GridGenerationStarted");

            if (pathOfGrid == null) CreatePathObjectIfNone();

            if (gridArray == null || gridArray.Length == 0)
            {
                if (!CreateGrid()) return;
            }

            if (tileSpritesList == null || tileSpritesList.Count == 0) GetAllSpritesFromChildrenTiles();

            foreach (Sprite tileSpr in tileSpritesList)
            {
                if (tileSpr.name.Contains("Path"))
                {
                    pathOfGrid.SetPathSprite(tileSpr);

                    break;
                }
            }

            //starts randomizing blockers on grid with 25% blockers and the rest are non-blocker tiles

            //IMPORTANT: "0" = blocker | "1" = non-blocker tile

            int[] blockersSpawnChanceArr = new int[100];

            //load blockersSpawnChanceArr
            for (int i = 0; i < blockersSpawnChanceArr.Length; i++)
            {
                if (i < 20) blockersSpawnChanceArr[i] = 0;
                else blockersSpawnChanceArr[i] = 1;
            }

            int previousIndex = 0;

            int currentIndex = 0;

            //traverse grid and set random blockers
            for (int i = 0; i < gridArray.Length; i++)
            {
                int count = 0;

                while (currentIndex == previousIndex && count <= 10)
                {
                    currentIndex = Random.Range(0, blockersSpawnChanceArr.Length);

                    count++;
                }

                //above loop done -> valid current index should be found -> set previous index as current index to prep for next loop
                previousIndex = currentIndex;

                if (blockersSpawnChanceArr[currentIndex] == 0)
                {
                    if (!gridArray[i].isOccupied) gridArray[i].isOccupied = true;

                    if (gridArray[i].is_AI_Path) gridArray[i].is_AI_Path = false;

                    if (occupiedTileSprite) gridArray[i].spriteRenderer.sprite = occupiedTileSprite;
                }
                else if (blockersSpawnChanceArr[currentIndex] == 1)
                {
                    if (gridArray[i].isOccupied) gridArray[i].isOccupied = false;

                    if (gridArray[i].is_AI_Path) gridArray[i].is_AI_Path = false;

                    if (unOccupiedDirtTileSprite) gridArray[i].spriteRenderer.sprite = unOccupiedDirtTileSprite;
                }
            }

            int tilesBetweenMidpoints = 2;

            if (gridWidth > 10 && ((gridWidth - 10) % 5) == 0) tilesBetweenMidpoints = gridWidth / 5;

            Tile startTile = null;

            Tile endTile = null;

            List<Tile> middleTiles = new List<Tile>();

            startTile = GetTileFromTileCoordinate(new Vector2Int(0, Random.Range(0, gridHeight)));

            if (startTile.isOccupied) startTile.isOccupied = false;

            endTile = GetTileFromTileCoordinate(new Vector2Int(gridWidth - 1, Random.Range(0, gridHeight)));

            if (endTile.isOccupied) endTile.isOccupied = false;

            int tilesSkipped = 0;

            for (int i = 0; i < gridWidth; i++)
            {
                if (i == 0 || i == gridWidth - 1) continue;

                if (tilesSkipped == tilesBetweenMidpoints)
                {
                    Tile randTileOnRow = GetTileFromTileCoordinate(new Vector2Int(i, Random.Range(0, gridHeight)));

                    if (randTileOnRow.isOccupied) randTileOnRow.isOccupied = false;

                    if (!middleTiles.Contains(randTileOnRow)) middleTiles.Add(randTileOnRow);

                    tilesSkipped = 0;

                    continue;
                }

                tilesSkipped++;
            }

            pathOfGrid.AutoGeneratePath(startTile, endTile, middleTiles);

            PathGenerator pathGenerator = pathOfGrid.GetPathGenerator();

            //incursively re-randomizing grid layout in case the first iteration doesn't work 
            //to avoid overflow, number of recursions is controlled by "iterationCount"
            if (!pathGenerator.couldCompletePath && iterationCount < 10)
            {
                //set isRandomizingGrid status to false so we can call this func within itself again
                //on next recursive call below, this func gonna set isRandomzingGrid to true again
                isRandomizingGrid = false;

                ProcedurallyRandomizedGridLayout();

                iterationCount++;

                return;
            }

            if (showDebugLog) Debug.Log("Grid Randomization Finished! Iteration Counts: " + iterationCount);

            iterationCount = 0;

            //save the newly generated random grid layout
            //only save grid layout if is in playmode in case grid is randomly being generated outside of playmode
            if(Application.isPlaying) SaveGridLayoutAfterRandomlyGenerated();

            MemoryUsageLogger.LogMemoryUsageAsText("GridGenerationFinished");

            isRandomizingGrid = false;
        }

        private WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();
        private IEnumerator RandomizedGridLayoutAndSaveLayoutOnStart()
        {
            if (isRandomizingGrid) yield break;

            yield return waitForFixedUpdate;

            ProcedurallyRandomizedGridLayout();
        }

        private void SaveGridLayoutAfterRandomlyGenerated()
        {
            if (!Application.isPlaying) return;

            if(gridArray == null || gridArray.Length == 0) return;

            if(showDebugLog) Debug.Log("Saving Randomly Generated Grid Layout!");

            for(int i = 0; i < gridArray.Length; i++)
            {
                if (!gridArray[i]) continue;

                SaveLoadHandler.SaveThisSaveableOnly(gridArray[i].GetTileSaveable());
            }

            SaveLoadHandler.SaveThisSaveableOnly(pathOfGrid.GetPathSaveable());
        }

        //ISaveable interface implementations for grid saving/loading.............................................................................

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

                EditorGUILayout.Space(12);

                EditorGUILayout.HelpBox(
                    "Generating the grid removes all of its current children Tiles and makes new ones based on the current grid's settings. " +
                    "New children tiles have default Tile settings", MessageType.Info);

                //Create a custom inspector button to execute the generation of the grid in the editor.
                //Clicking the button regenerates the grid based on current grid settings.
                //Regenerate grid deletes old tiles and create new children tiles with default values.
                //Buttons are disabled during runtime.
                using (new EditorGUI.DisabledGroupScope(Application.isPlaying || grid.isRandomizingGrid))
                {
                    if (GUILayout.Button("Generate Grid"))//On Generate Grid button pressed:...
                    {
                        if (!grid.CanGenerateGrid()) return;//if can not generate grid->do nothing

                        float timeStart = Time.realtimeSinceStartup;

                        float timeEnd = 0.0f;

                        float timeTookSecs = 0.0f;

                        float timeTookMiliSecs = 0.0f;

                        grid.ResetGrid();

                        grid.CreateGrid(true);

                        timeEnd = Time.realtimeSinceStartup;

                        timeTookSecs = timeEnd - timeStart;

                        timeTookMiliSecs = timeTookSecs * 1000.0f;

                        if (grid.showDebugLog) Debug.Log("Grid Generation Completed! Time Took: " +
                                                    timeTookSecs.ToString() +
                                                    "s | " + timeTookMiliSecs.ToString() + "ms");
                    }
                }

                EditorGUILayout.Space();

                using (new EditorGUI.DisabledGroupScope(Application.isPlaying || 
                                                        grid.gridArray == null || 
                                                        grid.gridArray.Length == 0 || 
                                                        grid.isRandomizingGrid))
                {
                    if (GUILayout.Button("Randomize Grid Layout"))
                    {
                        float timeStart = Time.realtimeSinceStartup;

                        float timeEnd = 0.0f;

                        float timeTookSecs = 0.0f;

                        float timeTookMiliSecs = 0.0f;

                        grid.ProcedurallyRandomizedGridLayout();

                        timeEnd = Time.realtimeSinceStartup;

                        timeTookSecs = timeEnd - timeStart;

                        timeTookMiliSecs = timeTookSecs * 1000.0f;

                        if (grid.showDebugLog) Debug.Log("Grid Layout Randomization Time Took: " +
                                                    timeTookSecs.ToString() +
                                                    "s | " + timeTookMiliSecs.ToString() + "ms");
                    }
                }
            }
        }
    #endif
    }
}
