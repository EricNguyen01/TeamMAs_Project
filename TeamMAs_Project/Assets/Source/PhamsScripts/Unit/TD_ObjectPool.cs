using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamMAsTD
{
    public class TD_ObjectPool
    {
        private IPoolable poolableInPool;

        private System.Type objectTypeInPool = null;

        private List<object> objectsPool = new List<object>();

        private List<GameObject> gameObjectsPool = new List<GameObject>();

        public TD_ObjectPool(IPoolable poolableToPool, int numberToPool, Transform parentTransformOfPool, bool setInactive)
        {
            poolableInPool = poolableToPool;

            MonoBehaviour scriptSpawnedThisPool = poolableToPool.GetScriptCarriesThisIPoolable();

            bool poolCreatedSuccessfully = CreateAndAddToPool(poolableInPool, numberToPool, parentTransformOfPool, setInactive);

            if (!poolCreatedSuccessfully)
            {
                Debug.LogError("TD_ObjectPool spawned by script: " + scriptSpawnedThisPool.name + " on GameObject: " + parentTransformOfPool.name + " has failed to create its object pool!");
            }
        }

        private bool CreateAndAddToPool(IPoolable poolable, int numberToPool, Transform transformCarriesPool, bool setInactive)
        {
            if (poolable == null) return false;

            GameObject prefabToPool = poolable.GetPoolablePrefabGameObject();

            object poolableType = poolable.GetPoolableType();

            if (prefabToPool == null || poolableType == null || numberToPool <= 0) return false;

            for(int i = 0; i < numberToPool; i++)
            {
                GameObject instantiated = MonoBehaviour.Instantiate(prefabToPool, transformCarriesPool.position, Quaternion.Euler(Vector3.zero), transformCarriesPool);

                MonoBehaviour[] attachedComp = instantiated.GetComponents<MonoBehaviour>();

                for(int j = 0; j < attachedComp.Length; j++)
                {
                    if(attachedComp[j].GetType() == poolableType.GetType())
                    {
                        if (objectTypeInPool == null) objectTypeInPool = attachedComp[j].GetType();

                        objectsPool.Add(attachedComp[j]);

                        gameObjectsPool.Add(instantiated);

                        if (setInactive) instantiated.SetActive(false);
                    }
                }
            }

            if (gameObjectsPool.Count > 0) return true;

            return false;
        }

        public System.Type GetObjectTypeOfPool()
        {
            return objectTypeInPool;
        }
    }
}
