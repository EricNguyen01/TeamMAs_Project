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
        [SerializeField] private TDGrid gridPathOn;

        [Space]

        [SerializeField] private Tile startTileOnPath;//for base ref (should be static once set)

        private Tile currentStartTile;//the actual start tile to use in pathfinding (might change dynamically)

        [SerializeField] private List<Tile> middleTilesOnPath = new List<Tile>();
         
        [SerializeField] private Tile endTileOnPath;//for base ref (should be static once set)

        private Tile currentEndTile;//the actual end tile to use in pathfinding (might change dynamically)

        private List<Tile> moreThan2TilesToTraverseList = new List<Tile>();//to use with middle tiles (more than 1 tiles that must be included in traversal)

        private Tile currentCheckingTile;
        
        private Queue<Tile> tileSearchFrontier = new Queue<Tile>();

        private List<Tile> reachedTiles = new List<Tile>();

        private Vector2Int[] neighborSearchDirections = { Vector2Int.right, Vector2Int.left, Vector2Int.up, Vector2Int.down };

        private List<Tile> finalGeneratedOrderedPathTiles = new List<Tile>();

        private bool canGeneratePath = false;

        private bool isFindingPath = false;

        private bool isFindingPathWithMultipleMidTiles = false;

        private bool couldCompletePath = false;

        [Space]

        [Header("Path Generator Debug")]

        [SerializeField] private bool showDebug = true;

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

            currentStartTile = startTileOnPath;

            this.endTileOnPath = endTileOnPath;

            currentEndTile = endTileOnPath;

            if (!CanGeneratePath())
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

            currentStartTile = moreThan2TilesToTraverseList[0];

            currentEndTile = moreThan2TilesToTraverseList[1];
        }

        private bool CanGeneratePath()
        {
            bool canGeneratePath = true;

            if (!gridPathOn || !currentStartTile || !currentEndTile)
            {
                if (showDebug) Debug.LogError("Path Generator missing required data inputs. Path generation will not work!");

                canGeneratePath = false;
            }

            if (currentStartTile)
            {
                if (startTileOnPath.gridParent && startTileOnPath.gridParent != gridPathOn)
                {
                    if (showDebug) Debug.LogError("Start Tile input of Path Generator does not belong to the provided grid. " +
                                                  "Path generation will not work!");

                    canGeneratePath = false;
                }
            }

            if (currentEndTile)
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

            for(int i = 0; i < moreThan2TilesToTraverseList.Count - 2; i++)
            {
                Tile startTile = moreThan2TilesToTraverseList[i];

                Tile endTile = moreThan2TilesToTraverseList[i + 1];

                if(currentStartTile != startTile) currentStartTile = startTile;

                if(currentEndTile != endTile) currentEndTile = endTile;

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
            if (!CanGeneratePath())
            {
                isFindingPath = false;

                couldCompletePath = false;

                if (showDebug) Debug.LogError("Invalid Start Or End Tile Encountered During Path Finding. Path Generation Stopped Abruptly!");

                return false;
            }

            isFindingPath = true;

            //add start tile to queue and to reached tiles list
            tileSearchFrontier.Enqueue(currentStartTile);

            reachedTiles.Add(currentStartTile);

            while (tileSearchFrontier.Count > 0 && isFindingPath)
            {
                currentCheckingTile = tileSearchFrontier.Dequeue();

                if(currentCheckingTile) currentCheckingTile.SetTileExplored(true);

                if (currentCheckingTile && currentCheckingTile == currentEndTile)
                {
                    isFindingPath = false;

                    couldCompletePath = true;

                    ConnectTilesToFormPath();

                    return true;
                }

                if (!ExploreAndEnqueueNeighborOf(currentCheckingTile))
                {
                    isFindingPath = false;

                    couldCompletePath = false;

                    if (showDebug) Debug.LogError("Exploring/Enqueue Neighbors Of A Tile Process Failed. Path Generation Stopped Abruptly!");

                    return false;
                }
            }

            if(showDebug) Debug.LogWarning("Path Generator BFS pathfinding process couldn't reach or find end tile. Please recheck data inputs...");

            isFindingPath = false; 

            couldCompletePath = false;

            return false;
        }

        private bool ExploreAndEnqueueNeighborOf(Tile currentTile)
        {
            if (!currentTile) return false;

            if (currentTile.gridParent != gridPathOn) return false;

            Vector2Int currentTileCoord = new Vector2Int(currentTile.tileNumInRow, currentTile.tileNumInColumn);

            foreach (Vector2Int direction in neighborSearchDirections)
            {
                Vector2Int neighborCoord = currentTileCoord + direction;

                Tile checkingNeighborTile = gridPathOn.GetTileFromTileCoordinate(neighborCoord);

                if (!checkingNeighborTile) continue;

                if (checkingNeighborTile.isOccupied) continue;

                if (reachedTiles.Contains(checkingNeighborTile)) continue;

                if (checkingNeighborTile.IsExplored()) continue;

                checkingNeighborTile.SetConnectingTile(currentTile);

                reachedTiles.Add(checkingNeighborTile);

                tileSearchFrontier.Enqueue(checkingNeighborTile);
            }

            return true;
        }

        private void ConnectTilesToFormPath()
        {
            if (!canGeneratePath || isFindingPath || !couldCompletePath) return;

            List<Tile> path = new List<Tile>();

            Tile currentTile = currentEndTile;

            path.Add(endTileOnPath);

            while(currentTile != currentStartTile && currentTile.GetConnectingTile())
            {
                currentTile = currentTile.GetConnectingTile();

                if(!path.Contains(currentTile)) path.Add(currentTile);
            }

            path.Reverse();

            finalGeneratedOrderedPathTiles.AddRange(path);
        }

        private void Reset()
        {
            isFindingPath = false;

            isFindingPathWithMultipleMidTiles = false;

            if (tileSearchFrontier.Count > 0) tileSearchFrontier.Clear();

            if (reachedTiles.Count > 0) reachedTiles.Clear();

            if (finalGeneratedOrderedPathTiles.Count > 0) finalGeneratedOrderedPathTiles.Clear();

            currentCheckingTile = null;

            couldCompletePath = false;
        }

        public List<Tile> GeneratePath()
        {
            PathGeneratorInit(gridPathOn, startTileOnPath, middleTilesOnPath, endTileOnPath);

            if (!canGeneratePath)
            {
                if (showDebug) Debug.LogWarning("Path Generator Can Not Generate Path Now. Please check Errors Log And Input Params...");

                return finalGeneratedOrderedPathTiles;
            }

            Reset();//even if pathfinding is in process -> stops it and reset

            if(moreThan2TilesToTraverseList == null || moreThan2TilesToTraverseList.Count == 0)
            {
                BreadthFirstSearchForPath();

                return finalGeneratedOrderedPathTiles;
            }

            BreadthFirstSearchWithMoreThan2Tiles();

            return finalGeneratedOrderedPathTiles;
        }
    }
}
