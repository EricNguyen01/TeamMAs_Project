using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif 

namespace TeamMAsTD
{
    [CanEditMultipleObjects]
    [DisallowMultipleComponent]
    public class Tile : MonoBehaviour
    {
        [field: Header("Tile Properties")]
        [field: SerializeField] public bool isOccupied { get; set; } = false;
        [field: SerializeField] public bool is_AI_Path { get; set; } = false;
        [field: SerializeField] public Unit unitOnTile { get; private set; }
        [field: SerializeField] public bool disableUprootOnTile { get; private set; } = false;

        [Header("Tile Debug Config")]
        [SerializeField] private bool drawTileDebug = true;

        [SerializeField]
        [Tooltip("Draw the tile color debug (e.g: green for plantable, grey for rocks, etc). " +
        "Dont forget to turn this to false if we have an actual tile with its own color on the tile sprite renderer.")]
        private bool drawDebugRuntime = true;

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

                if (is_AI_Path) spriteRenderer.color = Color.red;
            }

            AttachUprootOnTileUIScriptComponentIfNull();
        }

        private bool CanPlaceUnit(UnitSO unitSO)
        {
            if (unitOnTile != null || isOccupied)
            {
                return false;
            }
            if (is_AI_Path && !unitSO.isPlacableOnPath)
            {
                return false;
            }

            return true;
        }

        //This func checks if there is an UprootOnTileUI script attached to this game object. If not, attach one
        //then, get the attached script component
        private void AttachUprootOnTileUIScriptComponentIfNull()
        {
            if (GetComponent<UprootOnTileUI>() == null)
            {
                gameObject.AddComponent<UprootOnTileUI>();
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
        public bool PlaceUnit(UnitSO unitSO)//take in the unit scriptable object as an argument
        {
            //if can't place unit on this tile->do nothing
            if (!CanPlaceUnit(unitSO)) return false;

            if(unitSO.unitPrefab == null)
            {
                Debug.LogError("A unit: " + unitSO.displayName + "is being planted on tile: " + name + 
                " without a unit prefab data assigned in the unit scriptable object! Unit placement failed!");
                return false;
            }

            //if can place unit on this tile:
            //make new unit on center of this tile and make this unit children of this tile
            GameObject unitObj = Instantiate(unitSO.unitPrefab, transform.position, Quaternion.identity, transform);
            //get the Unit script component from the instantiated Unit obj
            Unit unit = unitObj.GetComponent<Unit>();

            //attach a new Unit script component on the Unit obj being placed if null
            if(unit == null)
            {
                unit = unitObj.AddComponent<Unit>();
                unit.SetUnitScriptableObject(unitSO);
            }

            unitOnTile = unit;

            //TODO: Send OnUnitPlacementSuccessful event

            return true;
        }

        public void UprootUnit()
        {
            if (unitOnTile == null) return;
            if (disableUprootOnTile) return;

            Destroy(unitOnTile.gameObject);
            unitOnTile = null;

            //TODO: Send OnUprootSuccessful event

            if(unitOnTile != null)
            {
                Debug.LogWarning("A unit is uprooted but tile: " + name + " is still referencing it!");
            }
        }

    //EDITOR...........................................................................

    //Tile editor stuff................................................................
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
