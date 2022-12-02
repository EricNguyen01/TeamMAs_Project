using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamMAsTD
{
    /*
     * This class ONLY handles the create of a GameObject pool based on the provided GameObject or GameObject prefab from another MonoBehaviour script.
     * This class also ONLY handles the adding, 
     *                             getting an inactive gameobject from pool, 
     *                             enabling/disabling an inactive gameobject from pool, 
     *                             and removing gameobject from pool.
     *                             
     * This class DOES NOT deal with any attached script or other components to the gameobject being pooled.
     * NOR DOES it deal with the object being pooled functionalities, initialization, behaviours, etc...
     */
    public class TD_GameObjectPool
    {
        private GameObject gameObjectInPool;

        private Transform parentTransformOfPool;

        public List<GameObject> gameObjectsPool { get; private set; }

        public TD_GameObjectPool(MonoBehaviour scriptSpawnedPool, GameObject gameObjectToPool, int numberToPool, Transform parentTransformOfPool, bool setInactive)
        {
            bool poolCreatedSuccessfully = false;

            if (gameObjectToPool != null)
            {
                gameObjectInPool = gameObjectToPool;

                this.parentTransformOfPool = parentTransformOfPool;

                poolCreatedSuccessfully = CreateAndAddToPool(gameObjectToPool, numberToPool, parentTransformOfPool, setInactive);
            }

            if (!poolCreatedSuccessfully)
            {
                Debug.LogError("TD_ObjectPool spawned by script: " + scriptSpawnedPool.name + " on GameObject: " + parentTransformOfPool.name + " has failed to create its object pool!");
            }
        }

        private bool CreateAndAddToPool(GameObject objectToPool, int numberToPool, Transform transformCarriesPool, bool setInactive)
        {
            if (objectToPool == null) return false;

            if (numberToPool <= 0) return false;

            if(gameObjectsPool == null) gameObjectsPool = new List<GameObject>();

            for(int i = 0; i < numberToPool; i++)
            {
                GameObject instantiated = MonoBehaviour.Instantiate(objectToPool, transformCarriesPool.position, Quaternion.Euler(Vector3.zero), transformCarriesPool);

                gameObjectsPool.Add(instantiated);

                if (setInactive) instantiated.SetActive(false);
            }

            return true;
        }

        public GameObject GetInactiveGameObjectFromPool()
        {
            //if 
            if (gameObjectsPool == null) return null;

            //if there is no game object in pool -> creates and adds 1 game object to pool then returns the newly added object
            if (gameObjectsPool.Count == 0)
            {
                CreateAndAddToPool(gameObjectInPool, 1, parentTransformOfPool, true);

                return gameObjectsPool[0];
            }

            //if there are game objects in pool -> finds the currently inactive 1 and returns it
            for(int i = 0; i < gameObjectsPool.Count; i++)
            {
                if (gameObjectsPool[i] == null) continue;

                if (gameObjectsPool[i].activeInHierarchy) continue;

                return gameObjectsPool[i];
            }

            //if no inactive found in pool-> creates and adds 1 new game object to pool and then returns it
            CreateAndAddToPool(gameObjectInPool, 1, parentTransformOfPool, true);

            return gameObjectsPool[gameObjectsPool.Count - 1];
        }

        public GameObject EnableGameObjectFromPool()
        {
            GameObject validObj = GetInactiveGameObjectFromPool();

            if (validObj == null) return null;

            validObj.transform.SetParent(null);

            if (!validObj.activeInHierarchy) validObj.SetActive(true);

            return validObj;
        }

        public bool ReturnGameObjectToPool(GameObject gameObject)
        {
            if(gameObject == null) return false;

            if (!gameObjectsPool.Contains(gameObject))
            {
                Debug.LogWarning("Game Object: " + gameObject.name + " " +
                "is trying to be returned to its object pool of script: " + parentTransformOfPool.name + 
                " but it's not belong to this pool!");

                return false;
            }

            if (gameObject.activeInHierarchy) gameObject.SetActive(false);

            gameObject.transform.SetParent(parentTransformOfPool);

            gameObject.transform.localPosition = Vector2.zero;

            gameObject.transform.localRotation = Quaternion.Euler(Vector3.zero);

            return true;
        }

        public void RemoveGameObjectFromPool(GameObject gameObject)
        {
            if (gameObjectsPool == null || gameObjectsPool.Count == 0) return;

            if (!gameObjectsPool.Contains(gameObject)) return;

            gameObjectsPool.Remove(gameObject);
        }
    }
}
