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

        public LayerMask GetUnitLayerMask();

        public Tile GetTileUnitIsOn();

        public AbilityEffectReceivedInventory GetAbilityEffectReceivedInventory();

        public void UpdateUnitSOData(UnitSO replacementUnitSO);
    }
}
