using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamMAsTD
{
    public class PlantUnit : MonoBehaviour, IUnit
    {
        [field: SerializeField] public PlantUnitSO plantUnitScriptableObject { get; private set; }

        //INTERNAL....................................................................

        private SpriteRenderer unitSpriteRenderer;

        //PRIVATES....................................................................

        private void Awake()
        {
            if(plantUnitScriptableObject == null)
            {
                Debug.LogError("Unit Scriptable Object data is not assigned on Unit: " + name + ". Disabling Unit!");
                gameObject.SetActive(false);
                return;
            }

            InitializeUnitUsingDataFromUnitSO();
        }

        private void InitializeUnitUsingDataFromUnitSO()
        {
            if (plantUnitScriptableObject == null) return;

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

        public void SetUnitScriptableObject(PlantUnitSO plantUnitSO)
        {
            plantUnitScriptableObject = plantUnitSO;

            InitializeUnitUsingDataFromUnitSO();
        }

        //IUnit Interfact functions...............................................................
        public UnitSO GetUnitScriptableObjectData()
        {
            return plantUnitScriptableObject;
        }
    }
}
