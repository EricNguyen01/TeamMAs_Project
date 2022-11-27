using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamMAsTD
{
    public class VisitorPool : MonoBehaviour
    {
        public VisitorUnitSO visitorTypeInPool { get; private set; }

        private List<VisitorUnit> visitorPool = new List<VisitorUnit>();

        private void CreateAndAddVisitorsToPool(VisitorUnitSO visitorSO, int numberToPool, bool setInactive)
        {
            if (visitorSO == null || visitorSO.unitPrefab == null) return;

            if (numberToPool <= 0) return;

            for (int i = 0; i < numberToPool; i++)
            {
                GameObject visitorGO = Instantiate(visitorSO.unitPrefab, transform);
                VisitorUnit visitorScriptComp = visitorGO.GetComponent<VisitorUnit>();

                if (visitorScriptComp == null)
                {
                    Debug.LogError("Visitor GameObject prefab of VisitorSO: " + visitorSO.name +
                    " has no Visitor script component attached! Pooling of this visitor obj failed!");
                    continue;
                }

                visitorPool.Add(visitorScriptComp);

                visitorScriptComp.SetPoolContainsThisVisitor(this);

                if(setInactive) visitorGO.SetActive(false);
            }
        }

        public void InitializeVisitorPool(VisitorUnitSO visitorSO, int numberToPool)
        {
            if(visitorSO == null)
            {
                enabled = false;
                return;
            }
            if(visitorSO.unitPrefab == null)
            {
                Debug.LogError("Trying to pool: " + visitorSO.displayName + " but found no visitor prefab of this visitor ScriptableObject!");
                enabled = false;
                return;
            }

            visitorTypeInPool = visitorSO;

            CreateAndAddVisitorsToPool(visitorSO, numberToPool, true);
        }

        private GameObject GetInactiveVisitorObjectFromPool()
        {
            if (visitorPool == null) return null;

            if (visitorTypeInPool == null) return null;

            //if there is no visitor in pool
            if(visitorPool.Count == 0)
            {
                //creates one visitor, adds it to the pool, sets it to inactive, and then returns it
                CreateAndAddVisitorsToPool(visitorTypeInPool, 1, true);
                return visitorPool[0].gameObject;
            }
            
            //if there are already visitors in pool
            for(int i = 0; i < visitorPool.Count; i++)
            {
                //if a visitor or its game object is null -> go next
                if (visitorPool[i] == null || visitorPool[i].gameObject == null) continue;
                //if a visitor is already active (in-use) -> go next
                if (visitorPool[i].gameObject.activeInHierarchy) continue;
                
                //returns the valid visitor game object
                return visitorPool[i].gameObject;
            }

            //if none of the visitors in the current pool is valid
            //creates a new visitor, adds to and sets inactive in pool
            CreateAndAddVisitorsToPool(visitorTypeInPool, 1, true);
            //returns the newly added visitor (the last element of the updated visitor list)
            return visitorPool[visitorPool.Count - 1].gameObject;
        }

        public GameObject EnableVisitorFromPool()
        {
            GameObject visitorObj = GetInactiveVisitorObjectFromPool();

            if (visitorObj == null) return null;

            visitorObj.transform.SetParent(null);

            if (!visitorObj.activeInHierarchy) visitorObj.SetActive(true);

            return visitorObj;
        }

        //This function will be called directly by the Visitor GameObject with Visitor script attached 
        //When a Visitor GameObject wants to return to pool (e.g on dead or reached destination) -> call this function inside it.
        public void ReturnVisitorToPool(VisitorUnit visitor)
        {
            if(visitor == null) return;

            if (visitor.gameObject.activeInHierarchy) visitor.gameObject.SetActive(false);

            visitor.transform.SetParent(transform);

            visitor.transform.position = Vector2.zero;
        }
    }
}
