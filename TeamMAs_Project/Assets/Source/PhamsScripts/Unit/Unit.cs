using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamMAsTD
{
    public class Unit : MonoBehaviour
    {
        [field: SerializeField] public UnitSO unitScriptableObject { get; private set; }

        //INTERNAL....................................................................

        private SpriteRenderer unitSpriteRenderer;

        //PRIVATES....................................................................

        private void Awake()
        {
            if(unitScriptableObject == null)
            {
                Debug.LogError("Unit Scriptable Object data is not assigned on Unit: " + name + ". Disabling Unit!");
                gameObject.SetActive(false);
                return;
            }

            InitializeUnitUsingDataFromUnitSO();
        }

        private void InitializeUnitUsingDataFromUnitSO()
        {
            if (unitScriptableObject == null) return;

            GetAndSetUnitSprite();

        }

        private void GetAndSetUnitSprite()
        {
            unitSpriteRenderer = GetComponent<SpriteRenderer>();

            if(unitSpriteRenderer == null)
            {
                unitSpriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }

            if (unitSpriteRenderer.sprite == null) unitSpriteRenderer.sprite = unitScriptableObject.unitThumbnail;
        }

        //PUBLICS........................................................................

        public void OnWatered()
        {

        }

        public void OnReceivedFertilizerBuff()
        {

        }

        public void SetUnitScriptableObject(UnitSO unitSO)
        {
            unitScriptableObject = unitSO;
            InitializeUnitUsingDataFromUnitSO();
        }
    }
}
