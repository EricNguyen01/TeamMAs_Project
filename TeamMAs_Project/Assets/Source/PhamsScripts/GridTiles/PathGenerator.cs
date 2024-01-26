using System;
using System.Collections;
using System.Collections.Generic;
using TeamMAsTD;
using UnityEngine;

[Serializable]
public class PathGenerator
{
    private Grid gridPathOn;

    private Tile startTileOnPath;

    private List<Tile> middleTilesOnPath;

    private Tile endTileOnPath;

    private Tile currentCheckingTile;

    private Queue<Tile> tileSearchFrontier = new Queue<Tile>();

    private List<Tile> reachedTiles = new List<Tile>();

    private Vector2Int[] searchDirections = { Vector2Int.right, Vector2Int.left, Vector2Int.up, Vector2Int.down };

    private bool isGeneratingPath = false;

    private List<Tile> finalGeneratedOrderedPathTiles = new List<Tile>();

    //PathGenerator base constructor
    public PathGenerator(Grid gridOfPath, Tile startTileOnPath, Tile endTileOnPath)
    {
        PathGeneratorInit(gridOfPath, startTileOnPath, endTileOnPath);
    }

    //PathGenerator overloaded constructor
    public PathGenerator(Grid gridOfPath, Tile startTileOnPath, List<Tile> middleTilesOnPath, Tile endTileOnPath)
    {
        PathGeneratorInit(gridOfPath, startTileOnPath, endTileOnPath);
    }

    private void PathGeneratorInit(Grid gridOfPath, Tile startTileOnGrid, Tile endTileOnGrid)
    {

    }

    private bool BreadthFirstSearchForPath()
    {
        isGeneratingPath = true;

        isGeneratingPath = false;

        return true;
    }

    private bool ExploreNeighborTilesFrom(Tile currentTile)
    {
        if (!currentTile) return false;



        return true;
    }

    private void Reset()
    {

    }

    public List<Tile> GeneratePath()
    {
        return finalGeneratedOrderedPathTiles;
    }
}
