// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using System.Collections;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif 

namespace TeamMAsTD
{
#if UNITY_EDITOR
    [CanEditMultipleObjects]
    [ExecuteInEditMode]
#endif
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TileMenuAndUprootOnTileUI))]
    public class Tile : MonoBehaviour, ISaveable
    {
        [field: Header("Tile Properties")]
        [field: SerializeField] public bool isOccupied { get; set; } = false;
        [field: SerializeField] public bool is_AI_Path { get; set; } = false;
        [field: SerializeField] public PlantUnit plantUnitOnTile { get; private set; }
        [field: SerializeField] public bool disableUprootOnTile { get; private set; } = false;

        [Header("Tile Components")]
        [SerializeField] private StatPopupSpawner insufficientFundToPlantOnTilePopupPrefab;

        public StatPopupSpawner thisTileInsufficientFundToPlantStatPopup { get; private set; }

        public WateringOnTile wateringOnTileScriptComp { get; private set; }

        [Header("Tile Debug Config")]
        [SerializeField] private bool drawTileDebug = true;

        [SerializeField]
        [Tooltip("Draw the tile color debug (e.g: green for plantable, grey for rocks, etc). " +
        "Dont forget to turn this to false if we have an actual tile with its own color on the tile sprite renderer.")]
        private bool drawDebugRuntime = true;

        //UnityEvents....................................................
        [SerializeField] public UnityEvent<PlantUnit, Tile> OnPlantUnitPlantedOnTile;
        [SerializeField] private UnityEvent<PlantUnitSO, Tile> OnPlantingFailedOnTile;
        [SerializeField] public UnityEvent<PlantUnit, Tile> OnPlantUnitUprootedOnTile;
        [SerializeField] private UnityEvent OnInsufficientFundsToUproot;

        //Internal........................................................
        [field: ReadOnlyInspector]
        [field: SerializeField]
        public int tileNumInRow { get; private set; }//position X in the grid (not in world space)

        [field: ReadOnlyInspector]
        [field: SerializeField]
        public int tileNumInColumn { get; private set; }//position Y in the grid (not in world space)

        [field: ReadOnlyInspector]
        [field: SerializeField]
        public TDGrid gridParent { get; private set; }//the grid that is housing this tile

        private SpriteRenderer spriteRenderer;

        private TileMenuAndUprootOnTileUI tileMenuAndUprootOnTileUI;

        public AudioSource tileAudioSource { get; private set; }

        public TileGlow tileGlowComp { get; private set; }

        [field: SerializeField] [field: HideInInspector] 
        public FMODUnity.StudioEventEmitter uprootAudioEventEmitterFMOD { get; private set; }

        //PRIVATES......................................................................

        private void Awake()
        {
            if (Application.isPlaying)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();

                tileAudioSource = GetComponent<AudioSource>();

                wateringOnTileScriptComp = GetComponent<WateringOnTile>();

                if (spriteRenderer == null)
                {
                    spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
                }

                tileGlowComp = GetComponent<TileGlow>();

                if (tileGlowComp == null)
                {
                    tileGlowComp = gameObject.AddComponent<TileGlow>();
                }

                if (drawDebugRuntime)
                {
                    if (isOccupied) spriteRenderer.color = Color.grey;
                    else spriteRenderer.color = Color.green;

                    if (is_AI_Path) spriteRenderer.color = Color.white;
                }

                Attach_TileMenu_And_UprootOnTileUI_ScriptComponentIfNull();

                if (insufficientFundToPlantOnTilePopupPrefab != null)
                {
                    GameObject statPopupSpawnerGO = Instantiate(insufficientFundToPlantOnTilePopupPrefab.gameObject, transform);

                    statPopupSpawnerGO.transform.localPosition = Vector3.zero;

                    thisTileInsufficientFundToPlantStatPopup = statPopupSpawnerGO.GetComponent<StatPopupSpawner>();
                }
            }

#if UNITY_EDITOR
            foreach(FMODUnity.StudioEventEmitter fmodEventEmitter in GetComponents<FMODUnity.StudioEventEmitter>())
            {
                if (fmodEventEmitter.EventReference.Path.Contains("Uproot"))
                {
                    uprootAudioEventEmitterFMOD = fmodEventEmitter;

                    break;
                }
            }
#endif
        }

        private void OnEnable()
        {
            if(this is ISaveable)
            {
                ISaveable saveable = (ISaveable)this;

                saveable.GenerateSaveableComponentIfNull(this);
            }
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

        private bool CanPlaceUnit(PlantUnitSO plantUnitSO)
        {
            //check for plantable and also trigger related events/functions
            return CanPlaceUnit(plantUnitSO, false);
        }

        //use for external calls ONLY
        public bool CanPlaceUnit_EXTERNAL(PlantUnitSO plantUnitSO)
        {
            return CanPlaceUnit(plantUnitSO, true);
        }

        //The "doChecksOnly" bool is for whether this function should ONLY do the if checks to see if a plant can be planted on this tile
        //or if it should also trigger related events and functions if a plant can/can't be planted
        private bool CanPlaceUnit(PlantUnitSO unitSO, bool doChecksOnly)
        {
            if (plantUnitOnTile != null || isOccupied)
            {
                if(!doChecksOnly) OnPlantingFailedOnTile?.Invoke(unitSO, this);

                return false;
            }
            if (is_AI_Path && !unitSO.isPlacableOnPath)
            {
                if(!doChecksOnly) OnPlantingFailedOnTile?.Invoke(unitSO, this);

                return false;
            }
            //check coin resource
            if (GameResource.gameResourceInstance != null)
            {
                //if coin resource amount < plant unit coin costs -> can't plant 
                if (GameResource.gameResourceInstance.coinResourceSO.resourceAmount < unitSO.plantingCoinCost)
                {
                    //Debug.Log("Insufficient Funds! Not enough coins to plant unit.");

                    if (!doChecksOnly)
                    {
                        if (thisTileInsufficientFundToPlantStatPopup != null)
                        {
                            thisTileInsufficientFundToPlantStatPopup.ResetStatPopupSpawnerConfigToStartDefault();

                            thisTileInsufficientFundToPlantStatPopup.PopUp(null, null, false);
                        }

                        OnPlantingFailedOnTile?.Invoke(unitSO, this);
                    }

                    return false;
                }
            }

            return true;
        }

        //This func checks if there is an UprootOnTileUI script attached to this game object. If not, attach one
        //then, get the attached script component
        private void Attach_TileMenu_And_UprootOnTileUI_ScriptComponentIfNull()
        {
            if (tileMenuAndUprootOnTileUI == null)
            {
                tileMenuAndUprootOnTileUI = GetComponent<TileMenuAndUprootOnTileUI>();
            }

            if (tileMenuAndUprootOnTileUI == null)
            {
                tileMenuAndUprootOnTileUI = GetComponentInChildren<TileMenuAndUprootOnTileUI>(true);
            }

            if (tileMenuAndUprootOnTileUI == null)
            {
                tileMenuAndUprootOnTileUI = gameObject.AddComponent<TileMenuAndUprootOnTileUI>();
            }
        }

        private IEnumerator DisableTileUprootAudioIfAnotherIsPlaying()
        {
            if (tileAudioSource == null && uprootAudioEventEmitterFMOD == null) yield break;

            Tile[] tilesInGridParent = gridParent.GetGridFlattened2DArray();

            if (tilesInGridParent == null || tilesInGridParent.Length == 0) yield break;

            float baseVolume = 0.0f;

            if(tileAudioSource != null) baseVolume = tileAudioSource.volume;

            float t = 0.0f;

            while(tileAudioSource != null && t <= 0.3f)
            {
                if (tileAudioSource.volume > 0.0f)
                {
                    for (int i = 0; i < tilesInGridParent.Length; i++)
                    {
                        if (tilesInGridParent[i] == this) continue;

                        if (tilesInGridParent[i].tileAudioSource == null && 
                            tilesInGridParent[i].uprootAudioEventEmitterFMOD == null) continue;

                        if (tilesInGridParent[i].tileAudioSource != null && tilesInGridParent[i].tileAudioSource.isPlaying)
                        {
                            tileAudioSource.volume = 0.0f;

                            tileAudioSource.Stop();

                            break;
                        }
                    }
                }

                t += Time.fixedDeltaTime;

                yield return new WaitForFixedUpdate();
            }

            if(uprootAudioEventEmitterFMOD != null)
            {
                for (int i = 0; i < tilesInGridParent.Length; i++)
                {
                    if (tilesInGridParent[i].uprootAudioEventEmitterFMOD == null) continue;

                    tilesInGridParent[i].uprootAudioEventEmitterFMOD.TriggerOnce = true;
                }

                yield return new WaitForSeconds(0.3f);

                for (int i = 0; i < tilesInGridParent.Length; i++)
                {
                    if (tilesInGridParent[i].uprootAudioEventEmitterFMOD == null) continue;

                    tilesInGridParent[i].uprootAudioEventEmitterFMOD.TriggerOnce = false;

                    tilesInGridParent[i].uprootAudioEventEmitterFMOD.hasTriggered = false;
                }
            }

            if(tileAudioSource != null) tileAudioSource.volume = baseVolume;

            yield break;
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

            if (plantUnitSO.unitPrefab == null)
            {
                UnityEngine.Debug.LogError("A plant unit: " + plantUnitSO.displayName + "is being planted on tile: " + name +
                " without a unit prefab data assigned in the unit scriptable object! Unit placement failed!");
                return false;
            }

            //if can place unit on this tile:
            //make new unit on center of this tile and make this unit children of this tile
            GameObject unitObj = Instantiate(plantUnitSO.unitPrefab, transform.position, Quaternion.identity, transform);
            //get the Unit script component from the instantiated Unit obj
            PlantUnit unit = unitObj.GetComponent<PlantUnit>();

            if (unit == null)
            {
                Debug.LogError("A plant unit prefab is placed on tile: " + name + " but it has no PlantUnit script attached." +
                "This results in no plant unit being placed!");

                Destroy(unitObj);
                return false;
            }

            plantUnitOnTile = unit;
            
            //throw plant successful event
            OnPlantUnitPlantedOnTile?.Invoke(plantUnitOnTile, this);

            //re-enable tile UI open/close functionality on a plant planted on
            tileMenuAndUprootOnTileUI.SetDisableTileMenuOpen(false);

            //coin cost on plant unit planted successful
            if (GameResource.gameResourceInstance == null || GameResource.gameResourceInstance.coinResourceSO == null)
            {
                Debug.LogError("GameResource Instance with Coin Resource is missing in scene! Planting coins cost won't function!");
            }
            else GameResource.gameResourceInstance.coinResourceSO.RemoveResourceAmount(plantUnitOnTile.plantUnitScriptableObject.plantingCoinCost);

            gridParent.CheckPlantUnitAsFirstUnitOfTypeOnGrid(plantUnitOnTile);

            //disable tile plantable glow effect that was enabled during drag/drop in case it has not been disabled in dragdrop script
            EnablePlantableTileGlowOnPlantDrag(plantUnitSO, false);

            return true;
        }

        public void UprootUnit(float uprootDelaySec)
        {
            if (plantUnitOnTile == null) return;

            if (disableUprootOnTile) return;

            //close tile menu on plant on tile uprooted and
            //disable tile menu open/close functionality after plant uprooted
            tileMenuAndUprootOnTileUI.SetDisableTileMenuOpen(true);

            //this coroutine function is to avoid multiple instances of tile uproot audio being played 
            //when multiple plants are uprooted at the same time
            StartCoroutine(DisableTileUprootAudioIfAnotherIsPlaying());

            //throw uproot event
            OnPlantUnitUprootedOnTile?.Invoke(plantUnitOnTile, this);

            //process uproot health cost - DEPRACATED!!!
            /*if (GameResource.gameResourceInstance != null && GameResource.gameResourceInstance.emotionalHealthSO != null)
            {
                //Debug.Log("Plant Uprooted, Consuming Emotional Health!");

                GameResource.gameResourceInstance.emotionalHealthSO.RemoveResourceAmount(plantUnitOnTile.plantUnitScriptableObject.uprootHealthCost);
            }*/

            plantUnitOnTile.ProcessPlantDestroyEffectFrom(this);

            if (uprootDelaySec == 0.0f) Destroy(plantUnitOnTile.gameObject);
            else Destroy(plantUnitOnTile.gameObject, uprootDelaySec);

            plantUnitOnTile = null;
        }

        public void UprootingInsufficientFundsEventInvoke()
        {
            OnInsufficientFundsToUproot?.Invoke();
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

        public void EnablePlantableTileGlowOnPlantDrag(PlantUnitSO plantUnitSO, bool enabled)
        {
            if(plantUnitSO == null)
            {
                if (!tileGlowComp.isTileGlowing) return;

                if (tileGlowComp.isTileGlowing) 
                { 
                    tileGlowComp.DisableTileGlowEffect();

                    return;
                }
            }

            //if enabled:
            if (enabled)
            {
                //only check for plantable without triggering any other related events/functions ("doOnlyChecks" para = true)
                if(CanPlaceUnit(plantUnitSO, true))
                {
                    //enable positive glow (green glow)
                    tileGlowComp.EnableTileGlowEffect(true);
                }
                else
                {
                    //enable negative glow (red glow)
                    tileGlowComp.EnableTileGlowEffect(false);
                }

                return;//exit after enabled
            }

            //else if disabled:
            //if alr disabled -> do nothing and exit
            if (!tileGlowComp.isTileGlowing) return;

            //if not disabled -> disable
            tileGlowComp.DisableTileGlowEffect();
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

        //ISaveable interface implementation...................................................................

        public SaveDataSerializeBase SaveData(string saveName = "")
        {
            SaveDataSerializeBase tileSaveData = new SaveDataSerializeBase(this, transform.position);

            return tileSaveData;
        }

        public void LoadData(SaveDataSerializeBase saveDataToLoad)
        {
            if (saveDataToLoad == null || saveDataToLoad.LoadSavedObject() == null) return;

            object savedObject = saveDataToLoad.LoadSavedObject();

            if (savedObject.GetType() != this.GetType()) return;

            Tile savedTile = (Tile)savedObject;

            if (!savedTile.plantUnitOnTile) return;

            plantUnitOnTile = Instantiate(savedTile.plantUnitOnTile, transform);

            disableUprootOnTile = savedTile.disableUprootOnTile;
        }

        //.....................................................................................................

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
