using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TeamMAsTD
{
    [System.Serializable]
    public class StatPopupPool : TD_GameObjectPoolBase
    {
        public StatPopupSpawner statPopupSpawnerSpawnedThisPool { get; private set; } = null;

        private GameObject statPopupPrefab;

        private Transform parentTransformCarriesPool;

        public StatPopupPool(MonoBehaviour scriptSpawnedPool, GameObject gameObjectToPool, Transform parentTransformOfPool) : base(scriptSpawnedPool, gameObjectToPool, parentTransformOfPool)
        {
            if(scriptSpawnedPool != null && scriptSpawnedPool.GetType() == typeof(StatPopupSpawner))
            {
                statPopupSpawnerSpawnedThisPool = (StatPopupSpawner)scriptSpawnedPool;
            }

            statPopupPrefab = gameObjectToPool;

            parentTransformCarriesPool = parentTransformOfPool;
        }

        //Override CreateAndAddToPool() of TD_GameObjectPoolBase.cs
        protected override bool CreateAndAddToPool(GameObject objectToPool, int numberToPool, Transform transformCarriesPool, bool setInactive)
        {
            //add new stat popup gameobjects to pool
            bool createAndAddSuccessful = base.CreateAndAddToPool(objectToPool, numberToPool, transformCarriesPool, setInactive);

            if (!createAndAddSuccessful) return createAndAddSuccessful;

            if(gameObjectsPool == null || gameObjectsPool.Count == 0) return createAndAddSuccessful;

            //only initialize the newly added stat popups (only initialize the increased portion of the gameObjectsPool not the whole)
            for(int i = gameObjectsPool.Count - numberToPool; i < gameObjectsPool.Count; i++)
            {
                StatPopup statPopup = gameObjectsPool[i].GetComponent<StatPopup>();

                if (statPopup == null) continue;

                statPopup.InitializeStatPopup(statPopupSpawnerSpawnedThisPool, this);
            }

            return createAndAddSuccessful;
        }

        public void CreateAndAddStatPopupsToPool(int popupNumberToPool)
        {
            CreateAndAddToPool(statPopupPrefab, popupNumberToPool, parentTransformCarriesPool, true);
        }

        public GameObject Init_And_Enable_StatPopup_GameObject_FromPool(Sprite popupSprite, string popupText, bool isPositivePopup, Vector3 enablePos, Vector3 endPos, float travelTime)
        {
            GameObject statPopupObj = GetInactiveGameObjectFromPool();

            StatPopup statPopupOfStatPopupObj = statPopupObj.GetComponent<StatPopup>();

            if(statPopupOfStatPopupObj == null)
            {
                base.RemoveGameObjectFromPool(statPopupObj);

                MonoBehaviour.Destroy(statPopupObj);

                return null;
            }
            
            statPopupOfStatPopupObj.InitializeStatPopup(enablePos, endPos, travelTime);

            if(popupSprite != null) statPopupOfStatPopupObj.SetNewStatPopupSprite(popupSprite);

            if (!string.IsNullOrEmpty(popupText)) 
            { 
                statPopupOfStatPopupObj.SetStatPopupText(popupText); 
            }
            else
            {
                if (isPositivePopup) statPopupOfStatPopupObj.UseDefaultStatPopupText(true);
                else statPopupOfStatPopupObj.UseDefaultStatPopupText(false);
            }

            if (isPositivePopup)
            {
                statPopupOfStatPopupObj.SetPositiveStatPopupSprite();

                statPopupOfStatPopupObj.SetStatPopupPositiveTextColor();
            }
            else 
            {
                statPopupOfStatPopupObj.SetNegativeStatPopupSprite();

                statPopupOfStatPopupObj.SetStatPopupNegativeTextColor(); 
            }

            statPopupObj.transform.SetParent(null);//parent is reset to statPopupSpawner obj transform upon returning to pool

            if (!statPopupObj.activeInHierarchy) statPopupObj.SetActive(true);
            
            return statPopupObj;
        }

        public bool ReturnStatPopupGameObjectToPool(GameObject gameObject)
        {
            return ReturnGameObjectToPool(gameObject);
        }
    }
}
