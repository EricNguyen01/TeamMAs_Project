// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TeamMAsTD
{
    [Serializable]
    public class PathGenerator
    {
        [SerializeField]
        [ReadOnlyInspector]
        private TDGrid gridPathOn;

        [Space]

        [SerializeField] private Tile startTileOnPath;//for base ref (should be static once set)

        private Tile startTileOnPathToUse;//the actual start tile to use in pathfinding (might change dynamically)

        [Space]

        [SerializeField] private bool autoGenerateMiddlePoints = false;

        [SerializeField]
        [DisableIf("autoGenerateMiddlePoints", false)]
        [Min(2)]
        private int maxTilesBetweenMidPoints = 2; 

        [SerializeField]
        [DisableIf("autoGenerateMiddlePoints", true)]
        private List<Tile> middleTilesOnPath = new List<Tile>();

        [Space]
         
        [SerializeField] private Tile endTileOnPath;//for base ref (should be static once set)

        private Tile endTileOnPathToUse;//the actual end tile to use in pathfinding (might change dynamically)

        private List<Tile> moreThan2TilesToTraverseList = new List<Tile>();//to use with middle tiles (more than 1 tiles that must be included in traversal)

        private Tile currentCheckingTile;
        
        private Queue<Tile> tileSearchFrontier = new Queue<Tile>();

        private List<Tile> exploredTiles = new List<Tile>();

        private Dictionary<Tile, Tile> tileAndConnectingTileDict = new Dictionary<Tile, Tile>(); 

        private Vector2Int[] neighborSearchDirections = { Vector2Int.right, Vector2Int.left, Vector2Int.up, Vector2Int.down };

        private List<Tile> finalGeneratedOrderedPathTiles = new List<Tile>();

        private bool canGeneratePath = false;

        private bool isFindingPath = false;

        private bool isFindingPathWithMultipleMidTiles = false;

        private bool couldCompletePath = false;

        [field: Header("Path Generator Debug")]

        [field: SerializeField] public bool showDebug { get; set; } = true;

        //PathGenerator base constructor
        public PathGenerator() { }

        //PathGenerator overloaded constructors
        public PathGenerator(TDGrid gridOfPath, Tile startTileOnPath, Tile endTileOnPath)
        {
            PathGeneratorInit(gridOfPath, startTileOnPath, endTileOnPath);
        }

        public PathGenerator(TDGrid gridOfPath, Tile startTileOnPath, List<Tile> middleTilesOnPath, Tile endTileOnPath)
        {
            PathGeneratorInit(gridOfPath, startTileOnPath, middleTilesOnPath, endTileOnPath);
        }

        public void PathGeneratorInit(TDGrid gridOfPath, Tile startTileOnPath, Tile endTileOnPath)
        {
            gridPathOn = gridOfPath;

            this.startTileOnPath = startTileOnPath;

            startTileOnPathToUse = startTileOnPath;

            this.endTileOnPath = endTileOnPath;

            endTileOnPathToUse = endTileOnPath;

            if (!CanGeneratePath_Internal())
            {
                if (showDebug) Debug.LogError("Path Generator Start And/Or End Tile Inputs Validation Check Failed. Path Generation Disabled!");
            }
        }

        public void PathGeneratorInit(TDGrid gridOfPath, Tile startTileOnPath, List<Tile> middleTilesOnPath, Tile endTileOnPath)
        {
            this.middleTilesOnPath = middleTilesOnPath;

            if(middleTilesOnPath == null || middleTilesOnPath.Count == 0)
            {
                PathGeneratorInit(gridOfPath, startTileOnPath, endTileOnPath);

                return;
            }

            if (moreThan2TilesToTraverseList.Count > 0) moreThan2TilesToTraverseList.Clear();

            if (middleTilesOnPath != null && middleTilesOnPath.Count > 0)
            {
                moreThan2TilesToTraverseList.Add(startTileOnPath);

                moreThan2TilesToTraverseList.AddRange(middleTilesOnPath);

                moreThan2TilesToTraverseList.Add(endTileOnPath);

                moreThan2TilesToTraverseList = moreThan2TilesToTraverseList.Distinct().ToList();

                moreThan2TilesToTraverseList.RemoveAll(x => x == null);

                if (moreThan2TilesToTraverseList.Count > 0)
                {
                    for (int i = 0; i < moreThan2TilesToTraverseList.Count; i++)
                    {
                        if (moreThan2TilesToTraverseList[i].gridParent != gridOfPath)
                        {
                            moreThan2TilesToTraverseList.RemoveAt(i);

                            i--;
                        }
                    }
                }
            }

            if (moreThan2TilesToTraverseList == null || 
                moreThan2TilesToTraverseList.Count == 0 ||
                moreThan2TilesToTraverseList.Count == 1)
            {
                if (showDebug) Debug.LogError("More than 1 key tiles between base start and end tile are provided but they are either null or invalid tiles." +
                                              "Path generation might not work as expected!");

                PathGeneratorInit(gridOfPath, startTileOnPath, endTileOnPath);

                return;
            }

            PathGeneratorInit(gridOfPath, startTileOnPath, endTileOnPath);

            startTileOnPathToUse = moreThan2TilesToTraverseList[0];

            endTileOnPathToUse = moreThan2TilesToTraverseList[1];
        }

        public void SetGridPathOn(TDGrid grid)
        {
            gridPathOn = grid;
        }

        public bool CanGeneratePath()
        {
            bool showDebugOriginalState = showDebug;

            if (showDebug) showDebug = false;

            bool canGeneratePath = CanGeneratePath_Internal();

            showDebug = showDebugOriginalState;

            return canGeneratePath;
        }

        private bool CanGeneratePath_Internal()
        {
            bool canGeneratePath = true;

            if (!gridPathOn || !startTileOnPathToUse || !endTileOnPathToUse)
            {
                if (showDebug) Debug.LogError("Path Generator missing required data inputs. Path generation will not work!");

                canGeneratePath = false;
            }

            if (startTileOnPathToUse)
            {
                if (startTileOnPath.gridParent && startTileOnPath.gridParent != gridPathOn)
                {
                    if (showDebug) Debug.LogError("Start Tile input of Path Generator does not belong to the provided grid. " +
                                                  "Path generation will not work!");

                    canGeneratePath = false;
                }
            }

            if (endTileOnPathToUse)
            {
                if (endTileOnPath.gridParent && endTileOnPath.gridParent != gridPathOn)
                {
                    if (showDebug) Debug.LogError("End Tile input of Path Generator does not belong to the provided grid. " +
                                                  "Path generation will not work!");

                    canGeneratePath = false;
                }
            }

            this.canGeneratePath = canGeneratePath;

            if (canGeneratePath) return true;

            return false;
        }

        private bool BreadthFirstSearchWithMoreThan2Tiles()
        {
            if (moreThan2TilesToTraverseList == null || moreThan2TilesToTraverseList.Count == 0)
            {
                return BreadthFirstSearchForPath();
            }

            isFindingPathWithMultipleMidTiles = true;

            for(int i = 0; i < moreThan2TilesToTraverseList.Count - 1; i++)
            {
                Tile startTile = moreThan2TilesToTraverseList[i];

                Tile endTile = moreThan2TilesToTraverseList[i + 1];

                startTileOnPathToUse = startTile;

                endTileOnPathToUse = endTile;

                if (!BreadthFirstSearchForPath())
                {
                    isFindingPathWithMultipleMidTiles = false;

                    couldCompletePath = false;

                    return false;
                }
            }

            isFindingPathWithMultipleMidTiles = false;

            return true;
        }

        private bool BreadthFirstSearchForPath()
        {
            if (!CanGeneratePath_Internal())
            {
                isFindingPath = false;

                couldCompletePath = false;

                if (showDebug) Debug.LogError("Invalid Start Or End Tile Encountered During Path Finding. Path Generation Stopped Abruptly!");

                return false;
            }

            ResetDuringSearch();

            isFindingPath = true;

            if (showDebug) Debug.Log("Started BFS for path operation with current start tile: " + 
                                     startTileOnPathToUse.name + " and end tile: " + endTileOnPathToUse.name);

            //add start tile to queue and to reached tiles list
            tileSearchFrontier.Enqueue(startTileOnPathToUse);

            int count = 0;

            while (tileSearchFrontier.Count > 0 && isFindingPath && count <= (gridPathOn.gridWidth * gridPathOn.gridHeight) * 2)
            {
                count++;

                currentCheckingTile = tileSearchFrontier.Dequeue();

                if (currentCheckingTile && !exploredTiles.Contains(currentCheckingTile)) 
                {
                    exploredTiles.Add(currentCheckingTile);
                }

                if (currentCheckingTile && currentCheckingTile == endTileOnPathToUse)
                {
                    isFindingPath = false;

                    couldCompletePath = true;

                    if (showDebug) Debug.Log("BFS operation has reached specified end tile.");

                    ConnectTilesToFormPath();

                    return true;
                }

                if (!ExploreAndEnqueueNeighborOf(currentCheckingTile))
                {
                    isFindingPath = false;

                    couldCompletePath = false;

                    if (showDebug) Debug.LogError("Exploring/Enqueue neighbors of a tile process failed. Path generation stopped abruptly!");

                    return false;
                }
            }

            if(showDebug) Debug.LogWarning("BFS pathfinding process couldn't reach or find end tile. Please recheck inputs...\n" +
                                           "TileSearchFrontier's count: " + tileSearchFrontier.Count + "\n" +
                                           "isFindingPath: " + isFindingPath.ToString() + "\n" +
                                           "IterationCount: " + count.ToString());

            isFindingPath = false; 

            couldCompletePath = false;

            ConnectTilesToFormPath();

            return false;
        }

        private bool ExploreAndEnqueueNeighborOf(Tile currentTile)
        {
            if (!currentTile) return false;

            if (currentTile.gridParent != gridPathOn) return false;

            bool shouldOverlapPathTile = true;

            Tile overlappingPathTile = null;

            Vector2Int currentTileCoord = new Vector2Int(currentTile.tileNumInRow, currentTile.tileNumInColumn);

            for(int i = 0; i < neighborSearchDirections.Length; i++)
            {
                Vector2Int neighborCoord = currentTileCoord + neighborSearchDirections[i];

                Tile checkingNeighborTile = gridPathOn.GetTileFromTileCoordinate(neighborCoord);

                if (!checkingNeighborTile) continue;

                if (tileSearchFrontier.Contains(checkingNeighborTile)) continue;

                if (exploredTiles.Contains(checkingNeighborTile)) continue;

                if (checkingNeighborTile.isOccupied) continue;

                if (checkingNeighborTile.is_AI_Path)
                {
                    overlappingPathTile = checkingNeighborTile;

                    continue;
                }

                tileAndConnectingTileDict.Add(checkingNeighborTile, currentTile);

                tileSearchFrontier.Enqueue(checkingNeighborTile);

                shouldOverlapPathTile = false;
            }

            if (shouldOverlapPathTile && overlappingPathTile)
            {
                tileAndConnectingTileDict.Add(overlappingPathTile, currentTile);

                tileSearchFrontier.Enqueue(overlappingPathTile);
            }

            return true;
        }

        private void ConnectTilesToFormPath()
        {
            if (!canGeneratePath || isFindingPath) return;

            if (tileAndConnectingTileDict.Count == 0) return;

            if (showDebug) Debug.Log("Tiles are now being connected to form path...");

            List<Tile> path = new List<Tile>();

            Tile currentTile = endTileOnPathToUse;

            if(endTileOnPathToUse.tileNumInRow == gridPathOn.gridWidth - 1)
            {
                Tile tileConnectedToEndTile = null;

                if(!tileAndConnectingTileDict.TryGetValue(currentTile, out tileConnectedToEndTile) || !tileConnectedToEndTile)
                {
                    for(int i = 0; i < exploredTiles.Count; i++)
                    {
                        if (!exploredTiles[i]) continue;

                        if (exploredTiles[i].tileNumInRow == endTileOnPathToUse.tileNumInRow)
                        {
                            currentTile = exploredTiles[i];

                            break;
                        }
                    }
                }
            }

            Tile tileConnectToCurrentTile;

            path.Add(currentTile);

            if(!endTileOnPathToUse.is_AI_Path) endTileOnPathToUse.is_AI_Path = true;

            while(currentTile != startTileOnPathToUse && tileAndConnectingTileDict.TryGetValue(currentTile, out tileConnectToCurrentTile))
            {
                if (!tileConnectToCurrentTile)
                {
                    if (showDebug) Debug.LogError("Encountered a NULL connecting tile while trying to construct path. Path construction INCOMPLETED!");

                    break;
                }

                currentTile = tileConnectToCurrentTile;

                if (!path.Contains(currentTile))
                {
                    path.Add(currentTile);

                    if(!currentTile.is_AI_Path) currentTile.is_AI_Path = true;
                }
            }

            path.Reverse();

            finalGeneratedOrderedPathTiles.AddRange(path);

            //remove duplicates only if they are next to each other to maintain path's correct flow
            for(int i = 0; i < finalGeneratedOrderedPathTiles.Count; i++)
            {
                if (i == finalGeneratedOrderedPathTiles.Count - 1) break;

                if (finalGeneratedOrderedPathTiles[i] == finalGeneratedOrderedPathTiles[i + 1])
                {
                    finalGeneratedOrderedPathTiles.RemoveAt(i + 1);
                }
            }

            if (showDebug) Debug.Log("Path Constructed!");
        }

        private void ResetDuringSearch()
        {
            if (tileSearchFrontier.Count > 0) tileSearchFrontier.Clear();

            if (exploredTiles.Count > 0) exploredTiles.Clear();

            if (tileAndConnectingTileDict.Count > 0) tileAndConnectingTileDict.Clear();

            currentCheckingTile = null;

            if (showDebug) Debug.Log("Partial Reset before new search: \n" +
                                     "TileSearchFrontier's count: " + tileSearchFrontier.Count + "\n" +
                                     "ExploredTiles' count: " + exploredTiles.Count + "\n" +
                                     "TileAndConnectingTileDict's count: " + tileAndConnectingTileDict.Count + "\n" +
                                     "CurrentCheckingTile: " + ((currentCheckingTile is null) ? "Is Null" : currentCheckingTile.name));
        }

        public void ResetAll()
        {
            isFindingPath = false;

            isFindingPathWithMultipleMidTiles = false;

            if (tileSearchFrontier.Count > 0) tileSearchFrontier.Clear();

            if (exploredTiles.Count > 0) exploredTiles.Clear();

            if(tileAndConnectingTileDict.Count > 0) tileAndConnectingTileDict.Clear();

            if (moreThan2TilesToTraverseList.Count > 0) moreThan2TilesToTraverseList.Clear();

            if (finalGeneratedOrderedPathTiles.Count > 0) finalGeneratedOrderedPathTiles.Clear();

            currentCheckingTile = null;

            couldCompletePath = false;

            if (showDebug) Debug.Log("Reset All Completed!");
        }

        public List<Tile> GeneratePath()
        {
            float timeStart = Time.realtimeSinceStartup;

            float timeEnd = 0.0f;

            float timeTookSecs = 0.0f;

            float timeTookMiliSecs = 0.0f;

            if (showDebug)
            {
                Debug.Log("Path Generation Started!\n");
            }

            ResetAll();//even if pathfinding is in process -> stops it and reset

            //re-init in case newer inputs were set and we didnt get it
            PathGeneratorInit(gridPathOn, startTileOnPath, middleTilesOnPath, endTileOnPath);

            if (!canGeneratePath)
            {
                timeEnd = Time.realtimeSinceStartup;

                timeTookSecs = timeEnd - timeStart;

                timeTookMiliSecs = timeTookSecs * 1000.0f;

                if (showDebug) Debug.LogWarning("Path Generator Can Not Generate Path Now. Please Check Errors Log And Input Params...\n" +
                                                "Time Took: " + timeTookSecs.ToString() + "s | " + timeTookMiliSecs.ToString() + "ms");

                return finalGeneratedOrderedPathTiles;
            }

            if(moreThan2TilesToTraverseList == null || moreThan2TilesToTraverseList.Count == 0)
            {
                BreadthFirstSearchForPath();
            }
            else
            {
                BreadthFirstSearchWithMoreThan2Tiles();
            }

            timeEnd = Time.realtimeSinceStartup;

            timeTookSecs = timeEnd - timeStart;

            timeTookMiliSecs = timeTookSecs * 1000.0f;

            if(showDebug) Debug.Log("Path Generation Completed!\n" +
                                    "Time Took: " + timeTookSecs.ToString() + "s | " + timeTookMiliSecs.ToString() + "ms");

            return finalGeneratedOrderedPathTiles;
        }
    }
}
