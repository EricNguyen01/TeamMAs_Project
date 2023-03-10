using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace TeamMAsTD
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlantAimShootSystem))]
    [RequireComponent(typeof(PlantWaterUsageSystem))]
    [RequireComponent(typeof(AbilityEffectReceivedInventory))]
    public class PlantUnit : MonoBehaviour, IUnit
    {
        [field: Header("Plant Unit SO Data")]

        [field: SerializeField] 
        public PlantUnitSO plantUnitScriptableObject { get; private set; }

        public PlantRangeCircle plantRangeCircle { get; private set; }

        //INTERNAL....................................................................

        public PlantAimShootSystem plantAimShootSystem { get; private set; }

        public PlantWaterUsageSystem plantWaterUsageSystem { get; private set; }

        public PlantUnitWorldUI plantUnitWorldUI { get; private set; }

        public Tile tilePlacedOn { get; private set; }

        private SpriteRenderer unitSpriteRenderer;

        private AbilityEffectReceivedInventory abilityEffectReceivedInventory;

        public float plantMaxAttackRange { get; private set; } = 1.0f;

        //PRIVATES....................................................................

        private void Awake()
        {
            if(plantUnitScriptableObject == null)
            {
                Debug.LogError("Plant Unit Scriptable Object data is not assigned on Plant Unit: " + name + ". Disabling Unit!");
                enabled = false;
                return;
            }

            if(plantUnitScriptableObject.plantProjectileSO == null)
            {
                Debug.LogError("Plant Unit Projectile Scriptable Object data is not assigned on Plant Unit: " + name + ". Disabling Unit!");
                enabled = false;
                return;
            }

            if(plantUnitScriptableObject.plantProjectileSO.plantProjectilePrefab == null)
            {
                Debug.LogError("Plant Unit Projectile Prefab data is not assigned on Plant Unit: " + name + ". Disabling Unit!");
                enabled = false;
                return;
            }

            tilePlacedOn = GetComponentInParent<Tile>();

            if(tilePlacedOn != null && tilePlacedOn.gridParent != null)
            {
                //convert tile number to float distance in the grid
                //max atk range = (tileSize * atk range in tiles) + 1/2 tile (to reach the edge of the last tile at max range)
                plantMaxAttackRange = tilePlacedOn.gridParent.GetDistanceFromTileNumber(plantUnitScriptableObject.attackRangeInTiles);
            }
            else
            {
                //if couldnt get atk range from tile size from the grid, assume that tile size is 1.0f and atk range = #of tiles
                //(e.g 3 tiles = 3.0f).
                plantMaxAttackRange = plantUnitScriptableObject.attackRangeInTiles;
            }

            abilityEffectReceivedInventory = GetComponent<AbilityEffectReceivedInventory>();

            if(abilityEffectReceivedInventory == null)
            {
                abilityEffectReceivedInventory = gameObject.AddComponent<AbilityEffectReceivedInventory>();
            }

            plantUnitWorldUI = GetComponent<PlantUnitWorldUI>();

            if(plantUnitWorldUI == null) plantUnitWorldUI = GetComponentInChildren<PlantUnitWorldUI>();

            //ALL of the plant unit's sub systems must be set below the get/set of plant unit's references such as tilePlacedOn or WorldUI...
            //to avoid missing references on initializing the systems.
            //no need to check for null data for the below components as this script has a require attribute for them.
            plantAimShootSystem = GetComponent<PlantAimShootSystem>();

            plantWaterUsageSystem = GetComponent<PlantWaterUsageSystem>();

            plantAimShootSystem.InitializePlantAimShootSystem(this, plantUnitScriptableObject.plantProjectileSO);

            plantWaterUsageSystem.InitializePlantWaterUsageSystem(this, true);

            if(plantUnitScriptableObject != null)
            {
                GameObject spawnedEffectGO = plantUnitScriptableObject.SpawnUnitEffectGameObject(plantUnitScriptableObject.unitSpawnEffectPrefab, transform, true, true);
                
                StartCoroutine(plantUnitScriptableObject.DestroyOnUnitEffectAnimFinishedCoroutine(spawnedEffectGO));
            }

            GetAndSetUnitSprite();

            SetPlantUnitWorldUIElementsValues();
        }

        private void Start()
        {
            CreateAndInitPlantRangeCircle();//must be in Start() here to avoid conflict with range circle class' Awake().
        }

        private void GetAndSetUnitSprite()
        {
            unitSpriteRenderer = GetComponent<SpriteRenderer>();

            if(unitSpriteRenderer == null)
            {
                unitSpriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }

            if (unitSpriteRenderer.sprite == null) unitSpriteRenderer.sprite = plantUnitScriptableObject.unitThumbnail;

            SetPlantSortingOrder();
        }

        private void CreateAndInitPlantRangeCircle()
        {
            if (plantUnitScriptableObject == null) return;

            if (plantUnitScriptableObject.plantRangeCirclePrefab == null) return;

            plantRangeCircle = Instantiate(plantUnitScriptableObject.plantRangeCirclePrefab.gameObject, transform).GetComponent<PlantRangeCircle>();

            plantRangeCircle.InitializePlantRangeCircle(this);

            plantRangeCircle.transform.localPosition = Vector3.zero;
        }

        //set this plant sorting order (in "Plant" sort layer) to other plants in the grid
        private void SetPlantSortingOrder()
        {
            if (tilePlacedOn == null) return;

            if (tilePlacedOn.gridParent == null) return;

            if (unitSpriteRenderer == null) return;

            unitSpriteRenderer.sortingOrder = (tilePlacedOn.gridParent.gridHeight - 1) - tilePlacedOn.tileNumInColumn;
        }

        private void SetPlantUnitWorldUIElementsValues()
        {
            if (plantUnitWorldUI != null)
            {
                plantUnitWorldUI.EnableUnitHealthBarSlider(true);

                plantUnitWorldUI.EnablePlantUnitWaterSlider(true);

                plantUnitWorldUI.SetHealthBarSliderValue(plantUnitScriptableObject.wavesSurviveWithoutWater, plantUnitScriptableObject.wavesSurviveWithoutWater);

                plantUnitWorldUI.SetWaterSliderValue(plantUnitScriptableObject.waterBars, plantUnitScriptableObject.waterBars);
            }
        }

        public void ProcessPlantDestroyEffectFrom(MonoBehaviour caller)
        {
            if (caller == null) return;

            if (plantUnitScriptableObject == null) return;

            GameObject spawnedEffectGO = plantUnitScriptableObject.SpawnUnitEffectGameObject(plantUnitScriptableObject.unitDestroyEffectPrefab, transform, false, true);
            
            //use other MonoBehavior (e.g Tile this plant on) to call this destroy effect coroutine
            //since if called by this plant obj which will be destroyed before 
            //this effect got destroyed, the destroy coroutine will stop as soon as this plant is destroyed
            //which this effect will then never be destroyed!
            caller.StartCoroutine(plantUnitScriptableObject.DestroyOnUnitEffectAnimFinishedCoroutine(spawnedEffectGO));
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, plantMaxAttackRange);
        }

        //PUBLICS........................................................................

        public void ReturnProjectileToPool(PlantProjectile projectile)
        {
            plantAimShootSystem.ReturnProjectileToPool(projectile);
        }

        //IUnit Interfact functions...............................................................
        public UnitSO GetUnitScriptableObjectData()
        {
            return plantUnitScriptableObject;
        }

        public object GetUnitObject()
        {
            return this;
        }

        public Tile GetTileUnitIsOn()
        {
            return tilePlacedOn;
        }

        public Transform GetUnitTransform()
        {
            return transform;
        }

        public AbilityEffectReceivedInventory GetAbilityEffectReceivedInventory()
        {
            return abilityEffectReceivedInventory;
        }

        //replace the current plant SO with a new one
        public void UpdateUnitSOData(UnitSO replacementUnitSO)
        {
            if (replacementUnitSO == null || replacementUnitSO.GetType() != typeof(PlantUnitSO)) return;

            PlantUnitSO replacementPlantSO = (PlantUnitSO)replacementUnitSO;

            plantUnitScriptableObject = replacementPlantSO;

            //must re-initialize plant's components upon replacing plant SO:

            plantAimShootSystem.InitializePlantAimShootSystem(this, plantUnitScriptableObject.plantProjectileSO);

            plantWaterUsageSystem.InitializePlantWaterUsageSystem(this, false);//false param is because this is not an awake init
        }
    }
}
