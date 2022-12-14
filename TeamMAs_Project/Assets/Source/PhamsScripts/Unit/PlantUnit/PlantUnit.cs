using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamMAsTD
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlantAimShootSystem))]
    public class PlantUnit : MonoBehaviour, IUnit
    {
        [field: SerializeField] public PlantUnitSO plantUnitScriptableObject { get; private set; }

        //INTERNAL....................................................................

        public PlantAimShootSystem plantAimShootSystem { get; private set; }

        public Tile tilePlacedOn { get; private set; }

        private SpriteRenderer unitSpriteRenderer;

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
            if(tilePlacedOn != null)
            {
                //max atk range = (tileSize * atk range in tiles) + half of a tileSize (so that we can reach the end border of the final tile for max range)
                plantMaxAttackRange = (tilePlacedOn.gridParent.tileSize * plantUnitScriptableObject.attackRangeInTiles) + (tilePlacedOn.gridParent.tileSize / 2.0f);
            }

            //no need to check for null data for this component as this script has a require attribute for it.
            plantAimShootSystem = GetComponent<PlantAimShootSystem>();

            plantAimShootSystem.InitializePlantAimShootSystem(this, plantUnitScriptableObject.plantProjectileSO);

            GetAndSetUnitSprite();
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

        //set this plant sorting order (in "Plant" sort layer) to other plants in the grid
        private void SetPlantSortingOrder()
        {
            if (tilePlacedOn == null) return;

            if (tilePlacedOn.gridParent == null) return;

            if (unitSpriteRenderer == null) return;

            unitSpriteRenderer.sortingOrder = (tilePlacedOn.gridParent.gridHeight - 1) - tilePlacedOn.tileNumInColumn;
        }

        //PUBLICS........................................................................

        public void OnWatered()
        {

        }

        public void OnReceivedFertilizerBuff()
        {

        }

        public void ReturnProjectileToPool(PlantProjectile projectile)
        {
            plantAimShootSystem.ReturnProjectileToPool(projectile);
        }

        //IUnit Interfact functions...............................................................
        public UnitSO GetUnitScriptableObjectData()
        {
            return plantUnitScriptableObject;
        }
    }
}
