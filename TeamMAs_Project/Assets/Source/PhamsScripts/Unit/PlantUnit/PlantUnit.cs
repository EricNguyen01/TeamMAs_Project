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

        private PlantAimShootSystem plantAimShootSystem;

        private SpriteRenderer unitSpriteRenderer;

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
        }

        //PUBLICS........................................................................

        public void OnWatered()
        {

        }

        public void OnReceivedFertilizerBuff()
        {

        }

        //IUnit Interfact functions...............................................................
        public UnitSO GetUnitScriptableObjectData()
        {
            return plantUnitScriptableObject;
        }
    }
}
