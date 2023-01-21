using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif 

namespace TeamMAsTD
{
#if UNITY_EDITOR
    [CanEditMultipleObjects]
    [ExecuteAlways]
#endif
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TileMenuAndUprootOnTileUI))]
    public class Tile : MonoBehaviour
    {
        [field: Header("Tile Properties")]
        [field: SerializeField] public bool isOccupied { get; set; } = false;
        [field: SerializeField] public bool is_AI_Path { get; set; } = false;
        [field: SerializeField] public PlantUnit plantUnitOnTile { get; private set; }
        [field: SerializeField] public bool disableUprootOnTile { get; private set; } = false;

#if UNITY_EDITOR
        [Header("Tile Debug Config")]
        [SerializeField] private bool drawTileDebug = true;
#endif

        [SerializeField]
        [Tooltip("Draw the tile color debug (e.g: green for plantable, grey for rocks, etc). " +
        "Dont forget to turn this to false if we have an actual tile with its own color on the tile sprite renderer.")]
        private bool drawDebugRuntime = true;

        //UnityEvents....................................................
        [SerializeField] public UnityEvent OnPlantUnitPlantedOnTile;
        [SerializeField] public UnityEvent OnPlantUnitUprootedOnTile;

        //Internal........................................................
        [field: SerializeField] [field: HideInInspector]
        public int tileNumInRow { get; private set; }//position X in the grid (not in world space)

        [field: SerializeField] [field: HideInInspector]
        public int tileNumInColumn { get; private set; }//position Y in the grid (not in world space)

        [field: SerializeField] [field: HideInInspector]
        public TDGrid gridParent { get; private set; }//the grid that is housing this tile

        private SpriteRenderer spriteRenderer;

        //PRIVATES......................................................................

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();

            if(spriteRenderer == null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }

            if (drawDebugRuntime)
            {
                if (isOccupied) spriteRenderer.color = Color.grey;
                else spriteRenderer.color = Color.green;

                if (is_AI_Path) spriteRenderer.color = Color.white;
            }

            Attach_TileMenu_And_UprootOnTileUI_ScriptComponentIfNull();
        }

#if UNITY_EDITOR
        private void Update()//run while not playing and is in editor only to check for tile's sprite change only!
        {
            if (Application.isPlaying) return;

            if (spriteRenderer == null) return;

            //if the user changes the tile's sprite in the editor -> process draw tile debug settings accordingly
            if (spriteRenderer != null && spriteRenderer.sprite != null && spriteRenderer.sprite.name != "Square")
            {
                EnableDrawTileDebug(false);
            }
        }
#endif

        private bool CanPlaceUnit(PlantUnitSO unitSO)
        {
            if (plantUnitOnTile != null || isOccupied)
            {
                return false;
            }
            if (is_AI_Path && !unitSO.isPlacableOnPath)
            {
                return false;
            }
            //check coin resource
            if(GameResource.gameResourceInstance != null)
            {
                //if coin resource amount < plant unit coin costs -> can't plant 
                if (GameResource.gameResourceInstance.coinResourceSO.resourceAmount < unitSO.plantingCoinCost)
                {
                    Debug.Log("Insufficient Fund! Not enough coins to plant unit.");
                    return false;
                }
            }

            return true;
        }

        //This func checks if there is an UprootOnTileUI script attached to this game object. If not, attach one
        //then, get the attached script component
        private void Attach_TileMenu_And_UprootOnTileUI_ScriptComponentIfNull()
        {
            if (GetComponent<TileMenuAndUprootOnTileUI>() == null)
            {
                gameObject.AddComponent<TileMenuAndUprootOnTileUI>();
            }
        }

        //PUBLICS........................................................................

        //the method below gets and sets the tile's data from the Grid class upon grid generation
        public void InitializeTile(TDGrid parentGrid, int numInRow, int numInColumn)
        {
            tileNumInRow = numInRow;
            tileNumInColumn = numInColumn;
            gridParent = parentGrid;
            
            //set name for the tile game object to its coordinate in the grid for readability (we don't want smth like tile(1) as a name).
            gameObject.name = "Tile" + tileNumInRow.ToString() + "." + tileNumInColumn.ToString();
        }

        //This function place a unit on this tile using the provided Unit scriptable object
        public bool PlaceUnit(PlantUnitSO plantUnitSO)//take in the unit scriptable object as an argument
        {
            //if can't place unit on this tile->do nothing
            if (!CanPlaceUnit(plantUnitSO)) return false;

            if(plantUnitSO.unitPrefab == null)
            {
                Debug.LogError("A plant unit: " + plantUnitSO.displayName + "is being planted on tile: " + name + 
                " without a unit prefab data assigned in the unit scriptable object! Unit placement failed!");
                return false;
            }

            //if can place unit on this tile:
            //make new unit on center of this tile and make this unit children of this tile
            GameObject unitObj = Instantiate(plantUnitSO.unitPrefab, transform.position, Quaternion.identity, transform);
            //get the Unit script component from the instantiated Unit obj
            PlantUnit unit = unitObj.GetComponent<PlantUnit>();

            if(unit == null)
            {
                Debug.LogError("A plant unit prefab is placed on tile: " + name + " but it has no PlantUnit script attached." +
                "This results in no plant unit being placed!");

                Destroy(unitObj);
                return false;
            }

            plantUnitOnTile = unit;

            OnPlantUnitPlantedOnTile?.Invoke();

            //coin cost on plant unit planted successful
            if(GameResource.gameResourceInstance == null || GameResource.gameResourceInstance.coinResourceSO == null)
            {
                Debug.LogError("GameResource Instance with Coin Resource is missing in scene! Planting coins cost won't function!");
            }
            else GameResource.gameResourceInstance.coinResourceSO.RemoveResourceAmount(plantUnitOnTile.plantUnitScriptableObject.plantingCoinCost);

            gridParent.CheckPlantUnitAsFirstPlantUnitOnGrid(plantUnitOnTile);

            return true;
        }

        public void UprootUnit(float uprootDelaySec)
        {
            if (plantUnitOnTile == null) return;

            if (disableUprootOnTile) return;

            Destroy(plantUnitOnTile.gameObject, uprootDelaySec);

            plantUnitOnTile = null;

            OnPlantUnitUprootedOnTile?.Invoke();
        }

        public void EnableDrawTileDebug(bool enabled)
        {
            if (enabled)
            {
                drawTileDebug = true;

                drawDebugRuntime = true;

                return;
            }

            drawTileDebug = false;

            drawDebugRuntime = false;
        }

    //EDITOR...........................................................................

    //Tile editor stuff................................................................
    #if UNITY_EDITOR
        //Draw the debug gizmos for this tile in scene view only
        private void OnDrawGizmos()
        {
            if (!drawTileDebug) return;

            if (spriteRenderer != null && spriteRenderer.sprite != null && spriteRenderer.sprite.name != "Square") return;

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
                Gizmos.color = Color.white;
                Gizmos.DrawCube(transform.position, new Vector2(transform.localScale.x - 0.1f, transform.localScale.y - 0.1f));
            }
        }

        [CustomEditor(typeof(Tile))]
        [CanEditMultipleObjects]
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
