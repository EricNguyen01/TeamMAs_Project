using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamMAsTD
{
    public interface IUnit
    {
        public UnitSO GetUnitScriptableObjectData();

        public object GetUnitObject();

        public Transform GetUnitTransform();

        public Tile GetTileUnitIsOn();

        public AbilityEffectReceivedInventory GetAbilityEffectReceivedInventory(GameObject go)
        {
            AbilityEffectReceivedInventory abilityEffectReceivedInventory = go.GetComponent<AbilityEffectReceivedInventory>();

            if (abilityEffectReceivedInventory == null)
            {
                abilityEffectReceivedInventory = go.AddComponent<AbilityEffectReceivedInventory>();
            }

            return abilityEffectReceivedInventory;
        }

        public void UpdateUnitSOData(UnitSO replacementUnitSO);
    }
}
