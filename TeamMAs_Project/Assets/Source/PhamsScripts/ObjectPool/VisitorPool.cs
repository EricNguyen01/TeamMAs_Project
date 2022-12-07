using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamMAsTD
{
    [System.Serializable]
    public class VisitorPool : TD_GameObjectPoolBase
    {
        public VisitorUnitSO visitorTypeInPool { get; private set; }

        private Transform waveSpawnerTransform;

        //VisitorPool's constructor
        public VisitorPool(WaveSpawner waveSpawner, VisitorUnitSO visitorSO, Transform waveSpawnerTransform) : base(waveSpawner, visitorSO.unitPrefab, waveSpawnerTransform)
        {
            if (visitorSO == null)
            {
                Debug.LogError("Visitor Pool of WaveSpawner: " + waveSpawner.name + " received no visitor ScriptableObject data!");
                return;
            }
            if (visitorSO.unitPrefab == null)
            {
                Debug.LogError("Trying to pool: " + visitorSO.displayName + " but found no visitor prefab of this visitor ScriptableObject!");

                return;
            }

            visitorTypeInPool = visitorSO;

            this.waveSpawnerTransform = waveSpawnerTransform;
        }

        public bool CreateAndAddInactiveVisitorsToPool(int numberToPool)
        {
            //Calls the base function in TD_GameObjectPoolBase
            return base.CreateAndAddToPool(visitorTypeInPool.unitPrefab, numberToPool, waveSpawnerTransform, true);
        }

        public GameObject EnableVisitorFromPool()
        {
            GameObject visitorGO = base.EnableGameObjectFromPool();
            
            VisitorUnit visitorUnit = visitorGO.GetComponent<VisitorUnit>();

            if (visitorUnit == null) return null;

            visitorUnit.SetPoolContainsThisVisitor(this);

            return visitorGO;
        }

        //This function will be called directly by the Visitor GameObject with Visitor script attached 
        //When a Visitor GameObject wants to return to pool (e.g on dead or reached destination) -> call this function inside it.
        public void ReturnVisitorToPool(VisitorUnit visitor)
        {
            if (visitor == null) return;

            base.ReturnGameObjectToPool(visitor.gameObject);
        }

        public void RemoveVisitorFromPool(VisitorUnit visitor)
        {
            if (visitor == null) return;

            base.RemoveGameObjectFromPool(visitor.gameObject);

            visitor.SetPoolContainsThisVisitor(null);
        }
    }
}
