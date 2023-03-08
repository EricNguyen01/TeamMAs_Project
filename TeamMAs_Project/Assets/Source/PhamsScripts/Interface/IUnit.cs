using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamMAsTD
{
    public interface IUnit
    {
        public UnitSO GetUnitScriptableObjectData();

        public Transform GetUnitTransform();

        public Tile GetTileUnitIsOn();
    }
}
