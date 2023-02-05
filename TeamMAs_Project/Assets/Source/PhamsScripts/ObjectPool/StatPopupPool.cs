using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TeamMAsTD
{
    public class StatPopupPool : TD_GameObjectPoolBase
    {
        private GameObject statPopupPrefab;

        private Transform transformCarriesPool;

        public StatPopupPool(MonoBehaviour scriptSpawnedPool, GameObject gameObjectToPool, Transform parentTransformOfPool) : base(scriptSpawnedPool, gameObjectToPool, parentTransformOfPool)
        {
            statPopupPrefab = gameObjectToPool;

            transformCarriesPool = parentTransformOfPool;
        }

        public bool CreateAndAddStatPopupsToPool(int numberToPool)
        {
            return base.CreateAndAddToPool(statPopupPrefab, numberToPool, transformCarriesPool, true);
        }

        public GameObject EnableStatPopupGameObjectFromPool(StatPopupSpawner thisStatPopupSpawner, Sprite popupSprite, string popupText, Vector3 enablePos, Vector3 endPos, float travelTime)
        {
            GameObject statPopupObj =  base.GetInactiveGameObjectFromPool();

            StatPopup statPopupOfStatPopupObj = statPopupObj.GetComponent<StatPopup>();

            if(statPopupOfStatPopupObj == null)
            {
                base.RemoveGameObjectFromPool(statPopupObj);

                MonoBehaviour.Destroy(statPopupObj);

                return null;
            }
            
            statPopupOfStatPopupObj.InitializeStatPopup(thisStatPopupSpawner, enablePos, endPos, travelTime);

            statPopupOfStatPopupObj.SetStatPopupImageAndText(popupSprite, popupText);

            if (!statPopupObj.activeInHierarchy) statPopupObj.SetActive(true);
            
            return statPopupObj;
        }

        public bool ReturnStatPopupGameObjectToPool(GameObject gameObject)
        {
            return base.ReturnGameObjectToPool(gameObject);
        }
    }
}
