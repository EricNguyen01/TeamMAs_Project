using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamMAsTD
{
    public class VisitorPool : MonoBehaviour
    {
        public VisitorUnitSO visitorTypeInPool { get; private set; }

        private TD_GameObjectPool visitorGameObjectPool;

        public void InitializeVisitorPool(VisitorUnitSO visitorSO, int numberToPool)
        {
            if (visitorSO == null)
            {
                enabled = false;
                return;
            }
            if (visitorSO.unitPrefab == null)
            {
                Debug.LogError("Trying to pool: " + visitorSO.displayName + " but found no visitor prefab of this visitor ScriptableObject!");
                enabled = false;
                return;
            }

            visitorTypeInPool = visitorSO;

            visitorGameObjectPool = new TD_GameObjectPool(this, visitorSO.unitPrefab, numberToPool, transform, true);
        }

        public GameObject EnableVisitorFromPool()
        {
            if (visitorGameObjectPool == null) return null;

            GameObject visitorGO = visitorGameObjectPool.EnableGameObjectFromPool();
            
            VisitorUnit visitorUnit = visitorGO.GetComponent<VisitorUnit>();

            if (visitorUnit == null) return null;

            visitorUnit.SetPoolContainsThisVisitor(this);

            return visitorGO;
        }

        //This function will be called directly by the Visitor GameObject with Visitor script attached 
        //When a Visitor GameObject wants to return to pool (e.g on dead or reached destination) -> call this function inside it.
        public void ReturnVisitorToPool(VisitorUnit visitor)
        {
            if (visitor == null || visitorGameObjectPool == null) return;

            visitorGameObjectPool.ReturnGameObjectToPool(visitor.gameObject);
        }

        public void RemoveVisitorFromPool(VisitorUnit visitor)
        {
            if (visitor == null || visitorGameObjectPool == null) return;

            visitorGameObjectPool.RemoveGameObjectFromPool(visitor.gameObject);

            visitor.SetPoolContainsThisVisitor(null);
        }
    }
}
