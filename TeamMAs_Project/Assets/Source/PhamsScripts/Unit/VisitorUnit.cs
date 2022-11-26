using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamMAsTD
{
    public class VisitorUnit : MonoBehaviour, IUnit
    {
        [field: SerializeField] public VisitorUnitSO visitorUnitSO { get; private set; }
        public VisitorPool poolContainsThisVisitor { get; private set; }

        public void SetPoolContainsThisVisitor(VisitorPool visitorPool)
        {
            poolContainsThisVisitor = visitorPool;
        }

        //IUnit Interface functions....................................................
        public UnitSO GetUnitScriptableObjectData()
        {
            return visitorUnitSO;
        }
    }
}
