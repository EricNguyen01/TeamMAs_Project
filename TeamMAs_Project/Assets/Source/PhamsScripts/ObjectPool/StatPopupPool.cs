// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

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

        private StatPopup currentActiveStatPopup;

        private float totalPopupEnableDelayTime = 0.0f;

        public int statPopupDelayCoroutineCount { get; private set; } = 0;

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

                statPopup.InitializeStatPopup(this);
            }

            return createAndAddSuccessful;
        }

        public void CreateAndAddStatPopupsToPool(int popupNumberToPool)
        {
            CreateAndAddToPool(statPopupPrefab, popupNumberToPool, parentTransformCarriesPool, true);
        }

        public GameObject Init_And_Enable_StatPopup_GameObject_FromPool(Sprite popupSprite, string popupText, StatPopup.PopUpType popUpType, Vector3 enablePos, Vector3 endPos, float travelTime)
        {
            GameObject statPopupObj = GetInactiveGameObjectFromPool();

            StatPopup statPopupOfStatPopupObj = statPopupObj.GetComponent<StatPopup>();

            if(statPopupOfStatPopupObj == null)
            {
                base.RemoveGameObjectFromPool(statPopupObj);

                MonoBehaviour.Destroy(statPopupObj);

                return null;
            }

            if (statPopupSpawnerSpawnedThisPool != null && 
                statPopupSpawnerSpawnedThisPool.displayPopupsSeparately && 
                statPopupSpawnerSpawnedThisPool.enabled)
            {
                if (currentActiveStatPopup != null)
                {
                    totalPopupEnableDelayTime += travelTime / 1.5f;

                    statPopupSpawnerSpawnedThisPool.StartCoroutine(DisplayStatPopupDelay(statPopupOfStatPopupObj, totalPopupEnableDelayTime));

                    enablePos = new Vector3(enablePos.x, enablePos.y - 0.5f, enablePos.z);

                    travelTime += 0.6f;
                }
            }

            currentActiveStatPopup = statPopupOfStatPopupObj;

            statPopupOfStatPopupObj.InitializeStatPopup(enablePos, endPos, travelTime);

            if (statPopupSpawnerSpawnedThisPool != null)
            {
                statPopupOfStatPopupObj.SetStatPopupScaleMultipliers(statPopupSpawnerSpawnedThisPool.statPopupScaleMultiplier);
            }

            if(popupSprite != null) statPopupOfStatPopupObj.SetNewStatPopupSprite(popupSprite);

            if (!string.IsNullOrEmpty(popupText)) 
            {
                statPopupOfStatPopupObj.SetStatPopupText(popupText); 
            }
            else
            {
                if (popUpType == StatPopup.PopUpType.Positive) statPopupOfStatPopupObj.UseDefaultStatPopupText(StatPopup.PopUpType.Positive);
                else if (popUpType == StatPopup.PopUpType.Negative) statPopupOfStatPopupObj.UseDefaultStatPopupText(StatPopup.PopUpType.Negative);
                else if (popUpType == StatPopup.PopUpType.Neutral) statPopupOfStatPopupObj.UseDefaultStatPopupText(StatPopup.PopUpType.Neutral);
            }

            if (popUpType == StatPopup.PopUpType.Positive)
            {
                statPopupOfStatPopupObj.SetPositiveStatPopupSprite();

                statPopupOfStatPopupObj.SetStatPopupPositiveTextColor();
            }
            else if(popUpType == StatPopup.PopUpType.Negative) 
            {
                statPopupOfStatPopupObj.SetNegativeStatPopupSprite();

                statPopupOfStatPopupObj.SetStatPopupNegativeTextColor(); 
            }
            else if(popUpType == StatPopup.PopUpType.Neutral)
            {
                statPopupOfStatPopupObj.SetNeutralStatPopupSprite();

                statPopupOfStatPopupObj.SetStatPopupNeutralTextColor();
            }

            //if stat popup obj doesn't have its own canvas which mean it must be displayed within another canvas (that's the only way it can be displayed)
            //and if so, don't detach from parent
            if(statPopupOfStatPopupObj.statPopupCanvas) statPopupObj.transform.SetParent(null);//parent is reset to statPopupSpawner obj transform upon returning to pool

            if (!statPopupObj.activeInHierarchy && statPopupSpawnerSpawnedThisPool.enabled) statPopupObj.SetActive(true);
            
            return statPopupObj;
        }

        public bool ReturnStatPopupGameObjectToPool(GameObject gameObject)
        {
            bool returnSuccessful = ReturnGameObjectToPool(gameObject);

            if(HasActiveGameObjects()) return returnSuccessful;

            currentActiveStatPopup = null;

            totalPopupEnableDelayTime = 0.0f;

            return returnSuccessful;
        }

        private IEnumerator DisplayStatPopupDelay(StatPopup statPopup, float delaySec)
        {
            if (statPopup == null) yield break;

            statPopupDelayCoroutineCount++;

            if (statPopup.gameObject.activeInHierarchy) 
            { 
                statPopup.gameObject.SetActive(false);

                //statPopupCanvasGroup.alpha = 0.0f;
            }

            yield return new WaitForSeconds(delaySec);

            statPopup.gameObject.SetActive(true);

            statPopupDelayCoroutineCount--;

            //statPopupCanvasGroup.alpha = 1.0f;

            yield break;
        }
    }
}
