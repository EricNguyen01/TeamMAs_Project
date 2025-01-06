// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

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

        [field: ReadOnlyInspectorPlayMode]
        [field: SerializeField]
        public PlantUnitSO plantUnitScriptableObject { get; private set; }

        public PlantRangeCircle plantRangeCircle { get; private set; }

        //INTERNAL....................................................................

        public PlantAimShootSystem plantAimShootSystem { get; private set; }

        public PlantWaterUsageSystem plantWaterUsageSystem { get; private set; }

        public PlantUnitWorldUI plantUnitWorldUI { get; private set; }

        public Tile tilePlacedOn { get; private set; }

        private SpriteRenderer unitSpriteRenderer;

        private Ability[] plantAbilities;

        private AbilityEffectReceivedInventory abilityEffectReceivedInventory;

        public float plantMaxAttackRange { get; private set; } = 1.0f;

        [System.Serializable]
        private struct PlantDebugData
        {
            public string plantDynamicID;
            public string plantStaticID;
            public float pDamage;
            public float pAtkSpeed;
        }

        private PlantDebugData plantDebugData = new PlantDebugData();

        //PRIVATES....................................................................

        private void Awake()
        {
            if (plantUnitScriptableObject == null)
            {
                Debug.LogError("Plant Unit Scriptable Object data is not assigned on Plant Unit: " + name + ". Disabling Unit!");
                enabled = false;
                return;
            }

            if (plantUnitScriptableObject.plantProjectileSO == null)
            {
                Debug.LogError("Plant Unit Projectile Scriptable Object data is not assigned on Plant Unit: " + name + ". Disabling Unit!");
                enabled = false;
                return;
            }

            if (plantUnitScriptableObject.plantProjectileSO.plantProjectilePrefab == null)
            {
                Debug.LogError("Plant Unit Projectile Prefab data is not assigned on Plant Unit: " + name + ". Disabling Unit!");
                enabled = false;
                return;
            }

            plantUnitScriptableObject = (PlantUnitSO)plantUnitScriptableObject.CloneThisUnitSO();

            SetPlantSODebugDataView();

            tilePlacedOn = GetComponentInParent<Tile>();

            if (tilePlacedOn != null && tilePlacedOn.gridParent != null)
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

            if (abilityEffectReceivedInventory == null)
            {
                abilityEffectReceivedInventory = gameObject.AddComponent<AbilityEffectReceivedInventory>();
            }

            plantUnitWorldUI = GetComponent<PlantUnitWorldUI>();

            if (plantUnitWorldUI == null) plantUnitWorldUI = GetComponentInChildren<PlantUnitWorldUI>();

            //ALL of the plant unit's sub systems must be set below the get/set of plant unit's references such as tilePlacedOn or WorldUI...
            //to avoid missing references on initializing the systems.
            //no need to check for null data for the below components as this script has a require attribute for them.
            plantAimShootSystem = GetComponent<PlantAimShootSystem>();

            plantWaterUsageSystem = GetComponent<PlantWaterUsageSystem>();

            plantAimShootSystem.InitializePlantAimShootSystem(this, plantUnitScriptableObject.plantProjectileSO);

            plantWaterUsageSystem.InitializePlantWaterUsageSystem(this, true);

            plantAbilities = GetComponentsInChildren<Ability>(true);

            if (plantUnitScriptableObject != null)
            {
                GameObject spawnedEffectGO = plantUnitScriptableObject.SpawnUnitEffectGameObject(plantUnitScriptableObject.unitSpawnEffectPrefab, transform, false, true);

                StartCoroutine(plantUnitScriptableObject.DestroyOnUnitEffectAnimFinishedCoroutine(spawnedEffectGO));
            }

            GetAndSetUnitSprite();

            SetPlantUnitWorldUIElementsValues();
        }

        private void OnEnable()
        {
            if (UnitGroupSelectionManager.unitGroupSelectionManagerInstance)
            {
                UnitGroupSelectionManager.unitGroupSelectionManagerInstance.RegisterNewSelectableUnitOnUnitEnabled(this);
            }
        }

        private void OnDisable()
        {
            if (UnitGroupSelectionManager.unitGroupSelectionManagerInstance)
            {
                UnitGroupSelectionManager.unitGroupSelectionManagerInstance.DeRegisterSelectableUnitOnUnitDisabled(this);
            }
        }

        private void Start()
        {
            CreateAndInitPlantRangeCircle();//must be in Start() here to avoid conflict with range circle class' Awake().
        }

        private void GetAndSetUnitSprite()
        {
            unitSpriteRenderer = GetComponent<SpriteRenderer>();

            if (unitSpriteRenderer == null)
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

            //use other MonoBehavior (e.g Tile this plant on) to call this destroy vfx play coroutine function
            //since if called by this plant obj which will be destroyed before 
            //this vfx got destroyed, the destroy vfx coroutine will stop as soon as this plant is destroyed
            //which the vfx supposed to be played will then never be!
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

        public LayerMask GetUnitLayerMask()
        {
            return gameObject.layer;
        }

        public AbilityEffectReceivedInventory GetAbilityEffectReceivedInventory()
        {
            return abilityEffectReceivedInventory;
        }

        public Ability[] GetPlantAbilities()
        {
            return plantAbilities;
        }

        public void DisablePlantUnitAndItsAbilities()
        {
            if (plantAimShootSystem != null)
            {
                plantAimShootSystem.EnablePlantAimShoot(false);
            }

            if (plantAbilities == null || plantAbilities.Length == 0) return;

            foreach (Ability ability in plantAbilities)
            {
                if (ability == null) continue;

                ability.TempDisable_SpawnedAbilityEffects_StatPopupSpawners_Except(this);

                ability.ForceStopAbilityImmediate();
            }
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

        public void SetPlantSODebugDataView()
        {
            plantDebugData.plantDynamicID = plantUnitScriptableObject.unitDynamicID;

            plantDebugData.plantStaticID = plantUnitScriptableObject.unitStaticID;

            plantDebugData.pDamage = plantUnitScriptableObject.damage;

            plantDebugData.pAtkSpeed = plantUnitScriptableObject.attackSpeed;
        }
    }
}
