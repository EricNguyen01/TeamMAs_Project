using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamMAsTD
{
    public class Unit : MonoBehaviour
    {
        [field: SerializeField] public UnitSO unitScriptableObject { get; private set; }

        //INTERNAL....................................................................

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
